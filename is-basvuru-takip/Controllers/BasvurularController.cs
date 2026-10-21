using System.Security.Claims;
using is_basvuru_takip.data;
using is_basvuru_takip.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using is_basvuru_takip.Dtos;
namespace is_basvuru_takip.Controllers;

[ApiController] // Bu sınıfın bir API controller’ı olduğunu söyler.
[Route("api/[controller]")]
[Authorize]
public class BasvurularController : ControllerBase
{
    private readonly AppDbContext _context;

    //Uygulama, Program.cs'te kaydettiğimiz AppDbContext'i buraya kendisi verir
    public BasvurularController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetBasvurular()
    {
        // Kullanıcının ID'sini al
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Kullanıcının başvurularını veritabanından çek
        var basvurular = await _context.Basvurular
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.BasvuruTarihi)
             .Select(b => new BasvuruDto
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
             })
            .ToListAsync();

        return Ok(basvurular);
    }
    [HttpPost]
    public async Task<IActionResult> CreateBasvuru([FromBody] BasvuruIstekDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }
        var basvuru = new Basvuru
        {
            UserId = userId,
            SirketAdi = dto.SirketAdi,
            Pozisyon = dto.Pozisyon,
            BasvuruTarihi = dto.BasvuruTarihi!.Value,
            Maas = dto.Maas,
            Durum = dto.Durum,
            Sekil = dto.Sekil,
            Kaynak = dto.Kaynak,
            IlanLinki = dto.IlanLinki,
            Notlar = dto.Notlar,
            Sehir = dto.Sehir

        };
        // Başvuruyu veritabanına ekle
        _context.Basvurular.Add(basvuru);
        await _context.SaveChangesAsync();

        var cevap = new BasvuruDto
        {
            Id = basvuru.Id,
            SirketAdi = basvuru.SirketAdi,
            Pozisyon = basvuru.Pozisyon,
            BasvuruTarihi = basvuru.BasvuruTarihi,
            Maas = basvuru.Maas,
            Durum = basvuru.Durum,
            Sekil = basvuru.Sekil,
            Kaynak = basvuru.Kaynak,
            IlanLinki = basvuru.IlanLinki,
            Notlar = basvuru.Notlar,
            Sehir = basvuru.Sehir

        };
        return Ok(cevap);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetBasvuru(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }
        var basvuru = await _context.Basvurular
            .Where(b => b.Id == id && b.UserId == userId)
            .Select(b => new BasvuruDto
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
            })
            .FirstOrDefaultAsync();

        if (basvuru == null)
        {
            return NotFound();
        }

        return Ok(basvuru);
    }
    [HttpPut("{id}")]
    public async Task<IActionResult> PutBasvuru(int id, [FromBody] BasvuruIstekDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }
        var basvuru = await _context.Basvurular
        .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
        if (basvuru == null)
        {
            return NotFound();
        }
        basvuru.SirketAdi = dto.SirketAdi;
        basvuru.Durum = dto.Durum;
        basvuru.Sehir = dto.Sehir;
        basvuru.Pozisyon = dto.Pozisyon;
        basvuru.IlanLinki = dto.IlanLinki;
        basvuru.Kaynak = dto.Kaynak;
        basvuru.Maas = dto.Maas;
        basvuru.Notlar = dto.Notlar;
        basvuru.Sekil = dto.Sekil;
        basvuru.BasvuruTarihi = dto.BasvuruTarihi!.Value;


        await _context.SaveChangesAsync();
        return NoContent();
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBasvuru(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }
        var basvuru = await _context.Basvurular
       .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
        if (basvuru == null)
        {
            return NotFound();
        }
        _context.Remove(basvuru);
        await _context.SaveChangesAsync();
        return NoContent();

    }
}
