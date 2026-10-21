using is_basvuru_takip.data;
using is_basvuru_takip.Dtos;
using is_basvuru_takip.Models;
using is_basvuru_takip.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
namespace is_basvuru_takip.Controllers;

[ApiController] // Bu sınıfın bir API controller’ı olduğunu söyler.
[Route("api/[controller]")]
[Authorize]
public class BasvurularController : ControllerBase
{
    private readonly IBasvuruService _service;

    // Uygulama, Program.cs'te kaydettiğimiz service'i buraya kendisi verir
    public BasvurularController(IBasvuruService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetBasvurular()
    {
        // Kullanıcının ID'sini al
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();
        var basvurular = await _service.ListeleAsync(userId);
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
}
