using is_basvuru_takip.Models;
using System.ComponentModel.DataAnnotations;
namespace is_basvuru_takip.Dtos;

// Kullanıcının başvuru eklerken veya güncellerken gönderebileceği bilgiler
// Id, UserId ve User bilerek yok: bunları kullanıcı belirleyemez
public class BasvuruIstekDto
{
    [Required(ErrorMessage = "Şirket adı zorunludur.")]
    [MaxLength(100, ErrorMessage = "Şirket adı en fazla 100 karakter olabilir.")]
    public string SirketAdi { get; set; } = "";

    [Required(ErrorMessage = "Pozisyon zorunludur.")]
    [MaxLength(100, ErrorMessage = "Pozisyon en fazla 100 karakter olabilir.")]
    public string Pozisyon { get; set; } = "";

    [Required(ErrorMessage = "Başvuru tarihi zorunludur.")]
    public DateOnly? BasvuruTarihi { get; set; }

    [Range(0,1000000, ErrorMessage = "Maaş doğru aralıkta değil")]
    public decimal? Maas { get; set; }

    [EnumDataType(typeof(BasvuruDurumu), ErrorMessage = "Başvuru durumu için seçeneklerden birini seçiniz")]
    public BasvuruDurumu Durum { get; set; }

    [EnumDataType(typeof(CalismaSekli), ErrorMessage = "Çalışma Şekli için seçeneklerden birini seçiniz")]
    public CalismaSekli Sekil { get; set; }

    [EnumDataType(typeof(BasvurdugumYer), ErrorMessage = "Basvurdugum Yer için seçeneklerden birini seçiniz")]
    public BasvurdugumYer Kaynak { get; set; }

    [Url(ErrorMessage = "Link formatına uygun değil")] 
    public string? IlanLinki { get; set; }

    [MaxLength(10000, ErrorMessage = "Not en fazla 10000 karakter olabilir.")]
    public string? Notlar { get; set; }

    [EnumDataType(typeof(Sehir), ErrorMessage = "Şehir için seçeneklerden birini seçiniz")]
    public Sehir? Sehir { get; set; }
}