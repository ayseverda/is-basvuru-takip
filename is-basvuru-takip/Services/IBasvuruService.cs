using is_basvuru_takip.Dtos;

namespace is_basvuru_takip.Services;

// Başvurularla ilgili yapılabilecek işlerin listesi.
// Ne yapılacağını söyler, nasıl yapılacağını değil.
public interface IBasvuruService
{
    Task<List<BasvuruDto>> ListeleAsync(string userId);
    Task<BasvuruDto?> GetirAsync(int id, string userId);
    Task<BasvuruDto> EkleAsync(BasvuruIstekDto dto, string userId);
    Task<bool> GuncelleAsync(int id, BasvuruIstekDto dto, string userId);
    Task<bool> SilAsync(int id, string userId);

    Task<OzetDto> OzetGetirAsync(string userId);
}