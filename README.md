# SecureBlog.API

BT Akademi **Güvenli Kod (.NET Core)** eğitiminin başlangıç (starter) projesi.

Bu proje çalışan ama **kasıtlı olarak güvenlik katmanı eklenmemiş** bir ASP.NET Core Web API'dir.
Katılımcılar eğitim boyunca güvenlik katmanlarını adım adım (M1...M17) bu projeye ekleyecek.

> ⚠️ Bu kod **eğitim amaçlıdır**. İçerdiği zafiyetler kasıtlıdır, production'da kullanılmamalıdır.

## Stack

- ASP.NET Core 10 Web API (Controller tabanlı)
- Entity Framework Core 10 — SQL Server (LocalDB)
- Serilog (Console sink)
- FluentValidation (iskelet)
- Swashbuckle (Swagger UI)

## Çalıştırma

```bash
dotnet restore
dotnet ef database update
dotnet run
```

Swagger UI: `https://localhost:{port}/swagger`

## Bilinen / Kasıtlı Zafiyetler

| Alan | Eksik | Eklenecek Modül |
|---|---|---|
| Auth | Parola plain text, `[Authorize]` yok | M3, M9, M11 |
| Posts search | Ham string birleştirmeyle SQL (SQL Injection) | M2 |
| Posts/Comments | Validation yok | M4 |
| Posts/Comments | Yetki kontrolü yok (IDOR) | M12 |
| Media upload | Dosya tipi kontrolü, path izolasyonu yok | M6, M17 |
| Media download | Path traversal koruması yok | M17 |
| Program.cs | HTTPS redirect, CORS, Rate Limiter, global exception handling yok | M3, M8, M14, M15 |
| appsettings.json | JWT secret plain text | M7 |

## Klasör Yapısı

```
Controllers/   API endpoint'leri
Models/        EF Core entity'leri
DTOs/          İstek/yanıt veri taşıyıcıları
Data/          AppDbContext
Services/      TokenService, CryptoService, AuditService (boş iskelet)
Middleware/    ExceptionHandling, AuditLogging (boş iskelet)
Validators/    FluentValidation kuralları (boş iskelet)
```

## Git Tag'leri

- `M01-start` — Başlangıç iskeleti, güvenlik katmanı yok
