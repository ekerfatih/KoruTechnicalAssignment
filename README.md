# Koru Technical Assignment

Blazor WebAssembly + Minimal API uygulaması; kullanıcıların şubeler için randevu talep edip gönderebildiği, yöneticilerin de talepleri listeleyip onaylayabildiği/reddebildiği uçtan uca bir örnektir. Domain/Application/Infrastructure/Web katmanlarıyla ayrılmıştır ve tüm doğrulamalar FluentValidation üzerinden yapılır.

## Özellik Özeti
- **Kullanıcı paneli**: Filtreleme, arama, sayfalama ve durum rozetleriyle “Taleplerim” listesi. Talep detay/audit geçmişi Bootstrap modal içinde gösterilir.
- **Talep formu**: Şube seçimi, tarih-saat doğrulamaları ve taslak/gönderim akışı. Taslak olmayan talepler salt-okunur.
- **Yönetici paneli**: Pending talepler için gelişmiş liste, inline red açıklaması, audit modalı ve tek tıkla onay/red.
- **Global hata yönetimi**: API tarafında yakalanan hatalar `application/problem+json` formatında döner; istemci tarafında toast bildirimleri gösterilir.
- **Bildirimler**: Başarılı/başarısız tüm kritik işlemler `NotificationService` aracılığıyla toast olarak kullanıcıya iletilir.

## Kurulum
1. **Gereksinimler**
   - .NET 8 SDK
   - SQL Server Express veya LocalDB (varsayılan connection string LocalDB içindir)
2. **Bağımlılıkları yükleyin**
   ```bash
   dotnet restore src/KoruTechnicalAssignment.sln
   ```
3. **Veritabanını hazırlayın** (uygulama açılışında da migrasyon/seed yapar, manuel isterseniz):
   ```bash
   dotnet ef database update --project src/KoruTechnicalAssignment.Infrastructure --context ApplicationIdentityDbContext
   dotnet ef database update --project src/KoruTechnicalAssignment.Infrastructure --context ApplicationDbContext
   ```
4. **Uygulamayı çalıştırın**
   ```bash
   dotnet run --project src/KoruTechnicalAssignment.Web/KoruTechnicalAssignment.Web.csproj
   ```
   Varsayılan URL: `https://localhost:7122/` (aynı adres `AppBaseUrl` konfigürasyonunda da tanımlıdır).

## Giriş Bilgileri
Seed edilen kullanıcılar:

| Rol   | Kullanıcı        | Şifre    |
|-------|------------------|----------|
| Admin | admin1@koru.local | `Admin123$` |
| Admin | admin2@koru.local | `Admin123$` |
| User  | user1@koru.local  | `User123$`  |
| User  | user2@koru.local  | `User123$`  |

## Mimari ve Teknik Detaylar
- **Domain**: `Request`, `Branch`, `RequestStatusHistory` gibi entity’ler ve `RequestStatus` enumu.
- **Application**: Repository sözleşmeleri, servisler (`RequestService`, `BranchService`, vb.) ve FluentValidation validator’ları.
- **Infrastructure**: EF Core DbContext’leri (Identity + Application), SQL Server migrasyonları, seed operasyonları ve repository implementasyonları.
- **Web**: Minimal API uçları + Blazor WebAssembly istemcisi. API tarafında global Exception middleware’i `ProblemDetails` döner.
- **Doğrulama**: FluentValidation; istemci tarafında ek iş kuralları `ValidationMessageStore` ile gösterilir.
- **Audit trail**: Hem kullanıcı hem yönetici panelinde `RequestHistoryModal` component’i ile geçmiş durum değişimleri modalda listelenir.
- **Bildirimler**: `NotificationService` + `NotificationHost` ile tüm sayfalarda toast/snackbar geri bildirimi.

## Ekran Görüntüsü / Beklenen Akış
1. Kullanıcı giriş yapar → “Taleplerim” sayfasında filtreler & talep oluşturur.
2. Talep “Gönder” ile Pending’e taşınır → toast bildirimi.
3. Yönetici paneli pending talepleri görür, modal üzerinden geçmişi inceler, onaylar veya gerekçeli olarak reddeder.
4. Kullanıcı panelinde audit geçmişi modalda izlenir.

## Bonus ve Kalite Notları
- Global hata yönetimi API seviyesinde ProblemDetails döndürecek şekilde ele alındı.
- UI tarafında toast/snackbar, inline doğrulama mesajları ve modal audit trail ile kullanıcı deneyimi güçlendirildi.
- Kod okunabilirliği için tekrar eden durum/rozet metinleri `RequestStatusDisplay` yardımcı sınıfına taşındı, filtre alanları ortak helper’larla sadeleştirildi.

> Ek not: Farklı host/portlarda çalıştıracaksanız `appsettings*.json` içindeki `AppBaseUrl` değerini ve Blazor istemcisinin `HttpClient` base address’ini güncellemeniz yeterlidir.
