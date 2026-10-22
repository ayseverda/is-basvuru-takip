using is_basvuru_takip.Dtos;

namespace is_basvuru_takip.Services
{
    public interface ILinkedInIceAktarService
    {
        Task<IceAktarSonucDto> IceAktarAsync(string userId, Stream dosya);
    }
}
