using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using is_basvuru_takip.Dtos;
using is_basvuru_takip.Models;

namespace is_basvuru_takip.Services;

public class LinkedInIceAktarService : ILinkedInIceAktarService
{
    private readonly IBasvuruService _basvuruService;

    public LinkedInIceAktarService(IBasvuruService basvuruService)
    {
        _basvuruService = basvuruService;
    }

    public async Task<IceAktarSonucDto> IceAktarAsync(string userId, Stream dosya)
    {
        // 1. Boş bir sonuç kutusu hazırla (sayılar 0'dan başlasın)
        var sonuc = new IceAktarSonucDto();

        // 2. Kullanıcının zaten kayıtlı olan ilan linklerini bul
        var mevcutBasvurular = await _basvuruService.ListeleAsync(userId,null,null);
        var mevcutLinkler = mevcutBasvurular
            .Where(b => b.IlanLinki != null)
            .Select(b => b.IlanLinki!)
            .ToHashSet();

        // 3. Dosyayı satır satır oku
        using var okuyucu = new StreamReader(dosya, Encoding.UTF8);
        var ayarlar = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            BadDataFound = null,      // Bozuk tırnaklar yüzünden bütün işlemi durdurma
            MissingFieldFound = null
        };
        using var csv = new CsvReader(okuyucu, ayarlar);

        // Başlık satırını oku ve geç
        await csv.ReadAsync();
        csv.ReadHeader();

        int satirNo = 1;
        while (await csv.ReadAsync())
        {
            satirNo++;
            try
            {
                // a. Satırı başvuruya çevir
                var dto = SatiriCevir(csv.Parser.Record!);

                // b. Bu link zaten varsa ekleme, Atlanan'ı 1 artır
                if (dto.IlanLinki != null && mevcutLinkler.Contains(dto.IlanLinki))
                {
                    sonuc.Atlanan++;
                    continue;
                }

                // c. Yoksa ekle, Eklenen'i 1 artır (linki de listeye yaz ki tekrar gelirse atlansın)
                await _basvuruService.EkleAsync(dto, userId);
                if (dto.IlanLinki != null)
                {
                    mevcutLinkler.Add(dto.IlanLinki);
                }
                sonuc.Eklenen++;
            }
            catch (Exception hata)
            {
                // d. Bir sorun çıkarsa Hatali'yı 1 artır ve nedenini Hatalar'a yaz
                sonuc.Hatali++;
                sonuc.Hatalar.Add($"Satır {satirNo}: {hata.Message}");
            }
        }

        // 4. Sonuç kutusunu döndür
        return sonuc;
    }


    // Bir CSV satırını başvuruya çevirir; sorun varsa hata fırlatır
    private static BasvuruIstekDto SatiriCevir(string[] p)
    {
        // Tarihteki virgül yüzünden "8/14/26" ve " 3:02 AM" iki parçaya bölünüyor.
        // İkinci parça saatse birleştir; diğer bütün sütunlar bir sağa kaymış demektir.
        int kayma = p.Length > 1 && (p[1].Trim().EndsWith("AM") || p[1].Trim().EndsWith("PM")) ? 1 : 0;

        if (p.Length < 6 + kayma)
        {
            throw new Exception("Satırda eksik sütun var");
        }

        string tarihMetni = kayma == 1 ? $"{p[0].Trim()}, {p[1].Trim()}" : p[0].Trim();
        string sirket = KarakterDuzelt(p[3 + kayma].Trim());
        string pozisyon = KarakterDuzelt(p[4 + kayma].Trim());
        string link = p[5 + kayma].Trim();
        string cv = p.Length > 6 + kayma ? KarakterDuzelt(p[6 + kayma].Trim()) : "";

        // Amerikan biçimi: ay/gün/yıl
        if (!DateTime.TryParseExact(tarihMetni, "M/d/yy, h:mm tt", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var tarih))
        {
            throw new Exception($"Tarih okunamadı ({tarihMetni})");
        }
        if (string.IsNullOrWhiteSpace(sirket)) throw new Exception("Şirket adı boş");
        if (string.IsNullOrWhiteSpace(pozisyon)) throw new Exception("Pozisyon boş");

        return new BasvuruIstekDto
        {
            SirketAdi = sirket.Length > 100 ? sirket.Substring(0, 100) : sirket,
            Pozisyon = pozisyon.Length > 100 ? pozisyon.Substring(0, 100) : pozisyon,
            BasvuruTarihi = DateOnly.FromDateTime(tarih),
            Durum = BasvuruDurumu.Basvuruldu,
            Sekil = CalismaSekli.Belirtilmemis,
            Kaynak = BasvurdugumYer.LinkedIn,
            IlanLinki = string.IsNullOrWhiteSpace(link) ? null : link,
            Notlar = string.IsNullOrWhiteSpace(cv) ? null : $"Gönderilen CV: {cv}"
        };
    }

    // LinkedIn'in bozduğu Türkçe karakterleri düzeltir ("YazÄ±lÄ±m" → "Yazılım")
    private static string KarakterDuzelt(string metin)
    {
        // Bozulma belirtisi yoksa dokunma
        if (!metin.Any(c => c == 'Ã' || c == 'Ä' || c == 'Å' || c == 'Ì'))
        {
            return metin;
        }

        // Yanlış kodlamayla okunmuş baytları geri alıp doğru kodlamayla tekrar oku
        var duzeltilmis = Encoding.UTF8.GetString(Encoding.Latin1.GetBytes(metin));
        return duzeltilmis.Contains('\uFFFD') ? metin : duzeltilmis.Normalize(NormalizationForm.FormC);
    }
}