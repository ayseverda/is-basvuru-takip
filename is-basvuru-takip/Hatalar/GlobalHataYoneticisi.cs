using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace is_basvuru_takip.Hatalar;

// Uygulamada yakalanmamış bir hata olursa buraya düşer:
// hatayı kayda geçirir ve kullanıcıya düzgün bir cevap döndürür
public class GlobalHataYoneticisi : IExceptionHandler //IExceptionHandler: .NET’in hazır menüsü. “Hata yöneticisi olmak isteyen sınıf
{
    private readonly ILogger<GlobalHataYoneticisi> _logger;

    public GlobalHataYoneticisi(ILogger<GlobalHataYoneticisi> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        // 1. Hatanın bütün ayrıntılarını log'a yaz (sadece geliştirici görür)
        _logger.LogError(exception, "Beklenmeyen hata: {Metot} {Yol}",
            httpContext.Request.Method, httpContext.Request.Path);

        // 2. Kullanıcıya sade ve her seferinde aynı biçimde bir cevap hazırla
        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Beklenmeyen bir hata oluştu",
            Detail = "İşlem tamamlanamadı. Lütfen biraz sonra tekrar deneyin.",
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);

        // "Bu hatayı ben hallettim" demek
        return true;
    }
}