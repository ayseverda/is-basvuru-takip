using Microsoft.AspNetCore.Identity;

namespace is_basvuru_takip.Models
{
    public class Basvuru
    {
        public int Id { get; set; }                  // Her kaydın benzersiz numarası

        public string UserId { get; set; } = "";        // Veritabanındaki yabancı anahtar sütunu
        public IdentityUser? User { get; set; }         // Bu başvurunun sahibi olan kullanıcı nesnesi navigation property
        public string SirketAdi { get; set; } = "";         // Zorunlu alan, boş metinle başlar
        public string Pozisyon { get; set; } = "";
        public DateOnly BasvuruTarihi { get; set; }
        public decimal? Maas { get; set; }          // Boş olabilir
        public BasvuruDurumu Durum { get; set; }           // Enum tipinde alan

        public CalismaSekli Sekil { get; set; } // Enum tipinde alan
        public BasvurdugumYer Kaynak { get; set; }

        public string? IlanLinki { get; set; }          // Boş olabilir
        public string? Notlar { get; set; }          // Boş olabilir

        public Sehir? Sehir { get; set; }

    }

    public enum Sehir
    {
        Ankara,
        Istanbul,
        Sakarya,
        Bursa,
        Kocaeli,
        Diger
    }


    public enum CalismaSekli
    {
        Ofisten,
        Uzaktan,
        Hibrit,
        Belirtilmemis
    }

    public enum BasvuruDurumu
    {
        Basvuruldu,
        Reddedildi,
        Mulakat,
        TestAsamasinda,
        TeklifAlindi,
        TeklifKabulEdildi,
        CevapYok,
        TeklifReddedildi
        

    }

    public enum BasvurdugumYer
    {
        KariyerNet,
        LinkedIn,
        Indeed,
        KendiSitesi,
        Mail,
        Diger
    }
}
