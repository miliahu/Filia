# Filia

**Filia** یک میکروسرویس مدیریت فایل با معماری Clean Architecture است.

| لایه / نگرانی | تکنولوژی |
|---|---|
| زبان / فریم‌ورک | .NET 10 / ASP.NET Core |
| پایگاه‌داده متادیتا | PostgreSQL + EF Core (فقط در Infrastructure) |
| الگوی دسترسی به داده | Repository Pattern (`IFileRepository` / `IUnitOfWork`) |
| ذخیره‌سازی فایل (Object Storage) | RustFS (سازگار با S3 API، از طریق AWSSDK.S3) |
| ارکستراسیون محلی | .NET Aspire (AppHost + ServiceDefaults)، پیکربندی از `appsettings.json` |
| لاگینگ | Serilog |
| پیام‌رسانی (اختیاری) | RabbitMQ (انتشار رویدادهای FileUploaded/FileDeleted) |
| تست | xUnit v3 — Unit / Integration (Testcontainers) / Architecture (NetArchTest) |
| کانتینر | Docker + docker-compose |

## ساختار سولوشن (Clean Architecture)

```
src/
  Filia.Domain          <- موجودیت‌ها، رویدادهای دامنه، بدون هیچ وابستگی بیرونی
  Filia.Application     <- Use Caseها (CQRS با MediatR)، IFileRepository/IUnitOfWork، Validation
  Filia.Infrastructure  <- EF Core/Postgres (پیاده‌سازی Repository)، RustFS، RabbitMQ، Serilog
  Filia.Api             <- کنترلرها، Program.cs، Dockerfile
  Filia.ServiceDefaults <- تنظیمات مشترک Aspire (OpenTelemetry، Health Checks)
  Filia.AppHost          <- ارکستراسیون Aspire (Postgres، RabbitMQ، RustFS، Api) - از appsettings.json می‌خواند
tests/
  Filia.UnitTests         <- تست دامنه و هندلرها با Mock (NSubstitute) - بدون EF/دیتابیس
  Filia.IntegrationTests  <- تست End-to-End روی API واقعی + Postgres در Testcontainers
  Filia.ArchitectureTests <- قوانین وابستگی بین لایه‌ها با NetArchTest
```

### قانون وابستگی

`Domain` هیچ ارجاعی به بیرون ندارد. `Application` **هیچ وابستگی‌ای به EF Core یا هر
تکنولوژی پایگاه‌داده‌ای ندارد** — فقط دو اینترفیس ساده تعریف می‌کند:

- `IFileRepository`: عملیات دسترسی به داده (`AddAsync`, `GetByIdAsync`, `GetPagedAsync`) با ورودی/خروجی موجودیت‌های دامنه، بدون هیچ نوع EF-specific (`IQueryable`, `DbSet`, ...).
- `IUnitOfWork`: فقط `SaveChangesAsync` برای commit اتمیک تغییرات.

پیاده‌سازی واقعی این دو (`FileRepository`, `ApplicationDbContext`) در `Filia.Infrastructure`
است و از EF Core/Npgsql استفاده می‌کند. این مرز به‌صورت خودکار در
`Filia.ArchitectureTests` (`Application_ShouldNotHaveDependencyOnEntityFrameworkCore`) تست می‌شود.

`Infrastructure` و `Api` این اینترفیس‌ها را در composition root (`Program.cs`) به هم وصل می‌کنند.

## پیش‌نیازها

- .NET 10 SDK
- Docker (برای Postgres / RustFS یا اجرای کامل با docker-compose)
- (اختیاری) [Aspire workload](https://learn.microsoft.com/dotnet/aspire): `dotnet workload install aspire`

> **نکته:** این آرشیو در محیطی بدون دسترسی به اینترنت و بدون dotnet SDK نصب‌شده تولید شده،
> بنابراین هیچ `dotnet restore` / `dotnet build` روی آن اجرا نشده و پوشه `Migrations` خالی است.
> پیش از build، شماره نسخهٔ پکیج‌ها در `Directory.Packages.props` را با NuGet مقایسه/به‌روزرسانی کنید.

## ⚠️ نکتهٔ لایسنس پکیج‌ها

سه‌تا از پکیج‌های استفاده‌شده، از یک نسخه به بعد لایسنس تجاری (پولی) گرفته‌اند:

- **MediatR** (v13+) و **AutoMapper** (v13+) — هر دو متعلق به LuckyPennySoftware — برای
  استفادهٔ تجاری نیاز به لایسنس دارند (رایگان برای متن‌باز/غیرتجاری). بدون لایسنس هم اجرا
  می‌شوند ولی یک هشدار در لاگ startup نشان می‌دهند.
- **FluentAssertions** (v8+) — مشابه، رایگان برای متن‌باز/غیرتجاری، برای تجاری پولی.

اگر این موضوع برایتان مسئله‌ساز است:
- `FluentAssertions` را می‌توانید در `Directory.Packages.props` روی آخرین نسخهٔ رایگان
  (`7.2.0`) پین کنید.
- برای `MediatR`/`AutoMapper` جایگزین‌های متن‌باز رایگان هم وجود دارند (مثلاً کتابخانهٔ
  source-generator محور [Mediator](https://github.com/martinothamar/Mediator) یا
  [Mapperly](https://github.com/riok/mapperly))، هرچند جایگزینی آن‌ها نیاز به تغییر کد دارد.

## اجرا (روش ۱ — .NET Aspire، توصیه‌شده برای توسعه)

```bash
cd src/Filia.AppHost
dotnet run
```

تنظیمات AppHost (نام دیتابیس، ایمیج/پورت‌های RustFS، نام ولوم‌ها) از
`src/Filia.AppHost/appsettings.json` خوانده می‌شود، نه هاردکد در `Program.cs` — برای
تغییر تنظیمات محیط dev/staging کافیست `appsettings.Development.json` را ویرایش کنید.
مقادیر حساس (رمز Postgres، Secret Key ی RustFS) به‌صورت Aspire Parameter مدل شده‌اند و
از user-secrets خوانده می‌شوند، نه از appsettings.

Aspire Dashboard به‌صورت خودکار باز می‌شود و Postgres، RustFS و خود API را
اجرا/مانیتور می‌کند.

## اجرا (روش ۲ — docker-compose)

```bash
docker compose up --build
```

Swagger روی `http://localhost:8080/swagger` در دسترس است.

## ساخت اولین Migration

```bash
dotnet tool install --global dotnet-ef
cd src/Filia.Infrastructure
dotnet ef migrations add InitialCreate \
  --startup-project ../Filia.Api \
  --output-dir Persistence/Migrations
dotnet ef database update --startup-project ../Filia.Api
```

## اجرای تست‌ها

```bash
dotnet test tests/Filia.UnitTests          # بدون Docker
dotnet test tests/Filia.ArchitectureTests  # بدون Docker
dotnet test tests/Filia.IntegrationTests   # نیازمند Docker (Testcontainers یک Postgres واقعی بالا می‌آورد)
```

یک GitHub Actions workflow آماده در `.github/workflows/ci.yml` هر سه را روی هر push/PR اجرا می‌کند.

## Endpoints اصلی

| Method | Route | توضیح |
|---|---|---|
| POST | `/api/v1/files` | آپلود فایل (multipart/form-data) |
| GET | `/api/v1/files/{id}` | دریافت متادیتای یک فایل |
| GET | `/api/v1/files` | لیست فایل‌ها (فیلتر با `folderPath`, `searchTerm`, صفحه‌بندی) |
| GET | `/api/v1/files/{id}/download-url` | لینک دانلود موقت (presigned URL) مستقیم از RustFS |
| PUT | `/api/v1/files/{id}` | ویرایش نام/پوشهٔ فایل |
| DELETE | `/api/v1/files/{id}` | حذف نرم (soft delete) + حذف آبجکت از RustFS |

## نکات معماری

- **Repository Pattern**: هر Use Case فقط با `IFileRepository`/`IUnitOfWork` کار می‌کند؛
  جزئیات EF Core (LINQ، `DbSet`، Migrations) کاملاً داخل `Filia.Infrastructure` پنهان است.
- **CQRS با MediatR**: هر Use Case یک Command/Query + Handler + Validator جدا دارد.
- **Domain Events → Integration Events**: با آپلود/حذف فایل، رویداد دامنه (`FileUploadedEvent`)
  توسط `DispatchDomainEventsInterceptor` بعد از `SaveChanges` پابلیش می‌شود؛ هندلر آن در
  Application رویداد را به یک Integration Event تبدیل و روی RabbitMQ منتشر می‌کند.
- **RustFS ↔ S3 SDK**: چون RustFS پروتکل S3 را پیاده‌سازی می‌کند، از `AWSSDK.S3` استاندارد با
  `ForcePathStyle = true` و Endpoint سفارشی استفاده شده.
- **Result/Exception handling**: خطاهای Validation و NotFound به‌صورت مرکزی در
  `ExceptionHandlingMiddleware` به `application/problem+json` تبدیل می‌شوند.

## انتشار در GitHub

این ریپو آماده‌ی publish است:
- `.gitignore` برای build artifact های دات‌نت
- `LICENSE` (MIT — در صورت نیاز تغییرش بدهید)
- `.github/workflows/ci.yml` برای build + تست خودکار

```bash
git init
git add .
git commit -m "Initial commit: Filia file management microservice"
git branch -M main
git remote add origin https://github.com/<your-username>/Filia.git
git push -u origin main
```
