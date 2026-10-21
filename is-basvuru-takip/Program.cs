using Microsoft.EntityFrameworkCore;
using is_basvuru_takip.data;
using Microsoft.AspNetCore.Identity;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Veritabaný baðlantýsýný kaydet: AppDbContext, PostgreSQL'e appsettings.json'daki baðlantý bilgisiyle baðlansýn
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Kullanýcý sistemini ekle: kullanýcýlar AppDbContext üzerinden veritabanýna kaydedilsin
builder.Services.AddAuthorization();
builder.Services.AddIdentityApiEndpoints<IdentityUser>()
    .AddEntityFrameworkStores<AppDbContext>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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
app.MapIdentityApi<IdentityUser>();
app.Run();
