🛒 E-Ticaret Web Uygulaması

Modern, hızlı ve ölçeklenebilir bir E-Ticaret web uygulaması. ASP.NET Core MVC, Entity Framework Core ve SQL Server teknolojileriyle geliştirilmiştir. Hem kullanıcı tarafı hem de Admin Paneli ile tam bir yönetim deneyimi sunar.

📌 Özellikler
🛍 Ürün Yönetimi (CRUD – Ekle, Listele, Güncelle, Sil)

📂 Kategori Yönetimi

🎯 Sepet & Ödeme Süreci

📰 Blog Sistemi

❓ SSS (FAQ) Modülü

🖼 Slider Yönetimi

📬 İletişim Formu & Mesaj Yönetimi

🔐 Admin Paneli (Areas yapısıyla ayrılmış)

📱 Responsive (Mobil uyumlu tasarım)

🛠 Teknoloji Yığını (Tech Stack)
Backend: ASP.NET Core MVC (.NET 8.0)

Veritabanı: Microsoft SQL Server + Entity Framework Core (Code First)

Frontend: HTML5, CSS3, Bootstrap

Mimari: Katmanlı Mimari (Core, Data, Service, WebUI)

Versiyon Kontrol: Git & GitHub

📂 Proje Klasör Yapısı
bash
Kopyala
Düzenle
E-Ticaret/
├── ECommerce.Core/        # Entity ve Domain modelleri
├── ECommerce.Data/        # DbContext, Migration, Veritabanı işlemleri
├── ECommerce.Service/     # Servisler ve iş mantığı
├── ECommerce.WebUI/       # MVC Controller, View, wwwroot, Admin panel
│   ├── Areas/Admin/       # Yönetim paneli
│   ├── Controllers/       # Kullanıcı tarafı controller'ları
│   ├── Views/             # Razor view dosyaları
│   └── wwwroot/           # CSS, JS, görseller
└── ECommerceSolution.sln  # Visual Studio çözüm dosyası
⚙️ Kurulum
1️⃣ Projeyi klonla

bash
Kopyala
Düzenle
git clone https://github.com/kullaniciadi/e-ticaret.git
cd e-ticaret
2️⃣ Bağımlılıkları yükle

bash
Kopyala
Düzenle
dotnet restore
3️⃣ Veritabanı bağlantısını ayarla
ECommerce.WebUI/appsettings.json dosyasındaki:

json
Kopyala
Düzenle
"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=ETicaretDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
Bilgilerini kendi SQL Server ayarlarına göre güncelle.

4️⃣ Migration işlemlerini çalıştır

bash
Kopyala
Düzenle
dotnet ef database update --project ECommerce.Data --startup-project ECommerce.WebUI
5️⃣ Projeyi çalıştır

bash
Kopyala
Düzenle
dotnet run --project ECommerce.WebUI
Tarayıcıda https://localhost:5001 adresine gidin.

🚀 Yayına Alma
IIS Üzerinde:
dotnet publish -c Release ile yayın dosyalarını oluştur ve IIS’e ekle.

Linux (Nginx + Kestrel):
Self-contained publish yap, Nginx reverse proxy olarak ayarla.

Azure App Service:
Visual Studio üzerinden veya CLI ile deploy et.

🛡 Güvenlik Notları
Tüm formlarda input validation uygulanmalıdır.

Admin Paneli kimlik doğrulama ve yetkilendirme ile korunmalıdır.

Parola saklama işlemleri hash algoritmalarıyla yapılmalıdır.

HTTPS zorunlu hale getirilmelidir.

📜 Lisans
Bu proje MIT Lisansı ile lisanslanmıştır.
Detaylar için LICENSE dosyasına göz atın.
