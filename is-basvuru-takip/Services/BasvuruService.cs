using is_basvuru_takip.data;
using is_basvuru_takip.Dtos;
using is_basvuru_takip.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


namespace is_basvuru_takip.Services;

// IBasvuruService menüsündeki işlerin gerçek tarifleri
public class BasvuruService : IBasvuruService
{
    private readonly AppDbContext _context;

    public BasvuruService(AppDbContext context)
    {
        _context = context;
    }

    // ---------- Yardımcılar: eşleştirme artık tek yerde ----------

    // Veritabanı nesnesini kullanıcıya gösterilecek DTO'ya çevirir
    private static BasvuruDto DtoyaCevir(Basvuru b)
    {
        return new BasvuruDto
        {
            Id = b.Id,
            SirketAdi = b.SirketAdi,
            Pozisyon = b.Pozisyon,
            BasvuruTarihi = b.BasvuruTarihi,
            Maas = b.Maas,
            Durum = b.Durum,
            Sekil = b.Sekil,
            Kaynak = b.Kaynak,
            IlanLinki = b.IlanLinki,
            Notlar = b.Notlar,
            Sehir = b.Sehir
        };
    }

    // Kullanıcının gönderdiği bilgileri bir veritabanı nesnesinin üzerine yazar
    // (hem eklemede hem güncellemede kullanılır)
    private static void DtodanAktar(BasvuruIstekDto dto, Basvuru b)
    {
        b.SirketAdi = dto.SirketAdi;
        b.Pozisyon = dto.Pozisyon;
        b.BasvuruTarihi = dto.BasvuruTarihi!.Value;
        b.Maas = dto.Maas;
        b.Durum = dto.Durum;
        b.Sekil = dto.Sekil;
        b.Kaynak = dto.Kaynak;
        b.IlanLinki = dto.IlanLinki;
        b.Notlar = dto.Notlar;
        b.Sehir = dto.Sehir;
    }

    // ---------- Menüdeki işler ----------

    public async Task<List<BasvuruDto>> ListeleAsync(string userId)
    {
        return await _context.Basvurular
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.BasvuruTarihi)
            .Select(b => DtoyaCevir(b))
            .ToListAsync();
    }

    public async Task<BasvuruDto?> GetirAsync(int id, string userId)
    {
        var basvuru = await _context.Basvurular
        .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
        if (basvuru == null)
        {
            return null;
        }

        return DtoyaCevir(basvuru);
    }
    // GetirAsync, EkleAsync, GuncelleAsync, SilAsync: sende

    public async Task<BasvuruDto> EkleAsync(BasvuruIstekDto dto, string userId)
    {
        // Yeni, boş bir başvuru oluştur ve sahibini yaz
        var basvuru = new Basvuru { UserId = userId };

        // Kullanıcının gönderdiği bilgileri üzerine aktar
        DtodanAktar(dto, basvuru);

        // Tabloya ekle ve veritabanına yaz
        _context.Basvurular.Add(basvuru);
        await _context.SaveChangesAsync();

        // Kaydedilen hali (artık Id'si de dolu) DTO olarak döndür
        return DtoyaCevir(basvuru);
    }

    public async Task<bool> GuncelleAsync(int id, BasvuruIstekDto dto, string userId)
    {
        var basvuru = await _context.Basvurular
         .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
        if (basvuru == null)
        {
            return false;
        }
        DtodanAktar(dto, basvuru);


        await _context.SaveChangesAsync();
        return true;

    }

    public async Task<bool> SilAsync(int id, string userId)
    {
        var basvuru = await _context.Basvurular
        .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
        if (basvuru == null)
        {
            return false;
        }

        _context.Basvurular.Remove(basvuru);
        await _context.SaveChangesAsync();
        return true;

    }

}