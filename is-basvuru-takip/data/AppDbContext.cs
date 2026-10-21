using is_basvuru_takip.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

// namespace satırı: Visual Studio'nun dosyada oluşturduğu satır neyse onu koru
namespace is_basvuru_takip.data;
// Uygulama ile veritabanı arasındaki köprü
public class AppDbContext : IdentityDbContext
{
    // Veritabanı ayarlarını (hangi veritabanı, bağlantı bilgisi) dışarıdan alır
    // ve türediği IdentityDbContext sınıfına iletir
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    // Veritabanındaki Basvurular tablosunu temsil eder
    public DbSet<Basvuru> Basvurular { get; set; }

  
}

