using is_basvuru_takip.data;
using is_basvuru_takip.Hatalar;
using is_basvuru_takip.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Veritabaný baðlantýsýný kaydet: AppDbContext, PostgreSQL'e appsettings.json'daki baðlantý bilgisiyle baðlansýn
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
// "Biri IBasvuruService isterse ona BasvuruService ver"
builder.Services.AddScoped<IBasvuruService, BasvuruService>();
builder.Services.AddScoped<ILinkedInIceAktarService, LinkedInIceAktarService>();
// Kullanýcý sistemini ekle: kullanýcýlar AppDbContext üzerinden veritabanýna kaydedilsin
builder.Services.AddAuthorization();
builder.Services.AddIdentityApiEndpoints<IdentityUser>()
    .AddEntityFrameworkStores<AppDbContext>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Yakalanmamýþ hatalarý GlobalHataYoneticisi'ne yönlendir
builder.Services.AddExceptionHandler<GlobalHataYoneticisi>();
builder.Services.AddProblemDetails();
builder.Services.AddRateLimiter(ayarlar =>
{
    // Her IP adresi için ayrý sayaç: herkesin kendi 5 hakký olsun
    ayarlar.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    ayarlar.AddPolicy("sabit", istek =>
        RateLimitPartition.GetFixedWindowLimiter(
            // Sayacý neye göre ayýralým? Ýsteði gönderenin IP adresine göre
            partitionKey: istek.Connection.RemoteIpAddress?.ToString() ?? "bilinmiyor",
            // Her IP'nin sayacý: 1 dakikada 5 istek
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1)
            }));
});

var app = builder.Build();


// Hata yöneticisi en baþta olmalý ki sonraki her adýmdaki hatayý yakalayabilsin
app.UseExceptionHandler();

// Kimlik doðrulama, hata yöneticisinin içinde çalýþsýn
app.UseAuthentication();
app.UseRateLimiter();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
// Kayýt, giriþ gibi hazýr kullanýcý uç noktalarýný aç
app.MapIdentityApi<IdentityUser>().RequireRateLimiting("sabit");
app.Run();
