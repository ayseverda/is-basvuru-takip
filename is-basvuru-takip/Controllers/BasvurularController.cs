using is_basvuru_takip.data;
using is_basvuru_takip.Dtos;
using is_basvuru_takip.Models;
using is_basvuru_takip.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
namespace is_basvuru_takip.Controllers;

[ApiController] // Bu sınıfın bir API controller’ı olduğunu söyler.
[Route("api/[controller]")]
[Authorize]

public class BasvurularController : ControllerBase
{
    private const long MaksimumDosyaBoyutu = 5 * 1024 * 1024; // 5 MB (bayt cinsinden)
    private readonly IBasvuruService _service;
    private readonly ILinkedInIceAktarService _iceAktarService;

    // Uygulama, Program.cs'te kaydettiğimiz service'i buraya kendisi verir
    public BasvurularController(IBasvuruService service, ILinkedInIceAktarService iceAktarService)
    {
        _service = service;
        _iceAktarService = iceAktarService;
    }
 
    [HttpGet]
    public async Task<IActionResult> GetBasvurular([FromQuery] BasvuruDurumu? durum, [FromQuery] Sehir? sehir)
    {
        // Kullanıcının ID'sini al
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();
        var basvurular = await _service.ListeleAsync(userId, durum, sehir);
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
        var yeniBasvuru = await _service.EkleAsync(dto, userId);
        return Ok(yeniBasvuru);

    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetBasvuru(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var basvuru = await _service.GetirAsync(id, userId);
        if (basvuru == null) return NotFound();

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
        var guncellendi = await _service.GuncelleAsync(id, dto, userId);
        if (guncellendi == false)
        {
            return NotFound();
        }
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
        var silinecek = await _service.SilAsync(id, userId);
        if (silinecek == false)
        {
            return NotFound();
        }

        return NoContent();

    }

    [HttpGet("ozet")]
    public async Task<IActionResult> GetOzet()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }
        var ozet = await _service.OzetGetirAsync(userId);
        return Ok(ozet);
    }

    [HttpPost("linkedin-ice-aktar")]
    [EnableRateLimiting("sabit")]
    public async Task<IActionResult> LinkedInIceAktar(IFormFile dosya)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }
       

        if (dosya == null || dosya.Length == 0)
            return BadRequest("Dosya boş ya da seçilmedi.");
        if (dosya.Length > MaksimumDosyaBoyutu)
        {
            return BadRequest("Dosya 5 MB’tan büyük olamaz.");
        }

        using var akis = dosya.OpenReadStream();
        var sonuc = await _iceAktarService.IceAktarAsync(userId, akis);
           
        return Ok(sonuc);
    }
}
