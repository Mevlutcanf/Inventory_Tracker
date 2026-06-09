# Inventory Checker

IT envanter ve zimmet yönetimi için SQLite-first, SQL Server-ready ASP.NET Core MVC kurumsal panel.

## Özellikler

- Dashboard özet ekranı
- Envanter, çalışan ve zimmet listeleri
- Varlık ve çalışan CRUD akışı
- Zimmet oluşturma ve iade alma akışı
- Liste ekranlarında arama ve filtreleme
- Kurumsal admin panel tasarımı
- EF Core ile SQLite varsayılan kurulum
- `Database:Provider` ayarı ile SQL Server'a geçiş

## Veritabanı Geçişi

Varsayılan provider SQLite'tır. SQL Server'a geçmek için `appsettings.json` içindeki `Database:Provider` değerini `SqlServer` yapın ve bağlantı dizesini güncelleyin.

## Not

Bu workspace içinde `dotnet` CLI görünmediği için proje dosyaları hazırlandı, ancak yerel derleme bu ortamda doğrulanamadı.

## Önerilen Sonraki Adım

SDK kurulu bir makinede `dotnet restore`, `dotnet ef migrations add InitialCreate` ve `dotnet run` ile veritabanını oluşturup paneli açabilirsiniz.
