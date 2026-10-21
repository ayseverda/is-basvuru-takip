using is_basvuru_takip.Models;

namespace is_basvuru_takip.Dtos;

// tabloda göstermek istediğimiz alanlar
public class BasvuruDto
{
    public int Id { get; set; }                 
    public string SirketAdi { get; set; } = "";         
    public string Pozisyon { get; set; } = "";
    public DateOnly BasvuruTarihi { get; set; }
    public decimal? Maas { get; set; }     
    public BasvuruDurumu Durum { get; set; }         
    public CalismaSekli Sekil { get; set; } 
    public BasvurdugumYer Kaynak { get; set; }
    public string? IlanLinki { get; set; }      
    public string? Notlar { get; set; }        
    public Sehir? Sehir { get; set; }
}