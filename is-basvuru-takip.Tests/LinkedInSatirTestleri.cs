using is_basvuru_takip.Dtos;
using is_basvuru_takip.Models;
using is_basvuru_takip.Services;

namespace is_basvuru_takip.Tests;

public class LinkedInSatirTestleri
{
    [Fact]
    public void TarihindeVirgulOlanSatir_DogruCevrilir()
    {
        // Hazýrla: Tarihin içindeki virgül yüzünden ikiye bölünmüþ bir satýr (sahte veri)
        string[] satir =
        {
            "10/21/26", " 3:15 PM",                 // tarih iki parçaya bölünmüþ
            "", "",                                 // e-posta ve telefon (boþ)
            "Örnek Þirket", "Backend Developer",
            "https://www.linkedin.com/jobs/view/123",
            "cv.pdf"
        };

        // Çalýþtýr
        var sonuc = LinkedInIceAktarService.SatiriCevir(satir);
        // Kontrol et
        Assert.Equal("Örnek Þirket", sonuc.SirketAdi);
        Assert.Equal("Backend Developer", sonuc.Pozisyon);
        Assert.Equal(new DateOnly(2026, 10, 21), sonuc.BasvuruTarihi);
    }
    [Fact]
    public void SirketAdiUzunlukKontrolu()
    {
        string uzunAd = new string('A', 150);   // 150 tane 'A'

        string[] satir =
          {
             "10/21/26", " 3:15 PM",
             "", "",
             uzunAd, "Backend Developer",     // þirket adý yerine uzun ad
            "https://www.linkedin.com/jobs/view/123",
            "cv.pdf"
          };

        var sonuc = LinkedInIceAktarService.SatiriCevir(satir);
        // Kontrol et
        Assert.Equal(100, sonuc.SirketAdi.Length);
    }

    [Theory]
    [InlineData("10/21/26", " 3:15 PM", 2026, 10, 21)]
    [InlineData("1/2/26", " 3:15 PM", 2026, 1, 2)]
    [InlineData("10/2/26", " 3:15 PM", 2026, 10, 2)]
    public void FarkliTarihler_DogruCevrilir(string tarih, string saat, int yil, int ay, int gun)
    {
        // Hazýrla: Tarih ve saat dýþarýdan geliyor
        string[] satir =
        {
        tarih, saat,
        "", "",
        "Örnek Þirket", "Backend Developer",
        "https://www.linkedin.com/jobs/view/123",
        "cv.pdf"
    };

        // Çalýþtýr
        var sonuc = LinkedInIceAktarService.SatiriCevir(satir);

        // Kontrol et
        Assert.Equal(new DateOnly(yil, ay, gun), sonuc.BasvuruTarihi);
    }
}