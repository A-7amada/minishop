using System.Security.Cryptography;
using MiniShop.Domain.Entities;
using MiniShop.Domain.Security;

namespace MiniShop.Infrastructure.Persistence;

public static class SeedData
{
    public static void Initialize(AppDbContext db, string? adminPassword = null)
    {
        if (!db.Products.Any())
        {
            db.Products.AddRange(
                new Product { Name = "Wireless Mouse", NameAr = "ماوس لاسلكي", Description = "Ergonomic 2.4 GHz wireless mouse with silent clicks.", DescriptionAr = "ماوس لاسلكي مريح بتقنية 2.4 جيجاهرتز ونقرات صامتة.", Price = 349m, Stock = 25, Emoji = "🖱️" },
                new Product { Name = "Mechanical Keyboard", NameAr = "لوحة مفاتيح ميكانيكية", Description = "Compact TKL keyboard with hot-swappable switches.", DescriptionAr = "لوحة مفاتيح TKL مدمجة بمفاتيح قابلة للتبديل.", Price = 1299m, Stock = 12, Emoji = "⌨️" },
                new Product { Name = "USB-C Hub", NameAr = "موزع USB-C", Description = "7-in-1 hub: HDMI, 3x USB-A, SD card, USB-C PD.", DescriptionAr = "موزع 7 في 1: HDMI و3 منافذ USB-A وقارئ SD وشحن PD.", Price = 749m, Stock = 30, Emoji = "🔌" },
                new Product { Name = "Noise-Cancelling Headphones", NameAr = "سماعات عزل ضوضاء", Description = "Over-ear Bluetooth headphones with 30h battery.", DescriptionAr = "سماعات بلوتوث فوق الأذن ببطارية تدوم 30 ساعة.", Price = 2499m, Stock = 8, Emoji = "🎧" },
                new Product { Name = "27\" Monitor", NameAr = "شاشة 27 بوصة", Description = "QHD IPS display, 75 Hz, height-adjustable stand.", DescriptionAr = "شاشة QHD IPS بتردد 75 هرتز وحامل قابل لتعديل الارتفاع.", Price = 6499m, Stock = 5, Emoji = "🖥️" },
                new Product { Name = "Laptop Stand", NameAr = "حامل لابتوب", Description = "Aluminium stand, raises screen to eye level.", DescriptionAr = "حامل ألومنيوم يرفع الشاشة لمستوى النظر.", Price = 499m, Stock = 40, Emoji = "💻" }
            );
        }

        if (!db.Users.Any())
        {
            // DEV-ONLY seeded accounts. Outside Development the admin password must come
            // from the ADMIN_SEED_PASSWORD configuration value; if it is missing a random
            // password is generated and the account stays locked. Never hard-code one here.
            var effectiveAdminPassword = adminPassword;
            if (string.IsNullOrEmpty(effectiveAdminPassword))
                effectiveAdminPassword = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development"
                    ? "Admin123!"
                    : Convert.ToHexString(RandomNumberGenerator.GetBytes(24));
            var (adminHash, adminSalt) = PasswordHasher.Hash(effectiveAdminPassword);
            db.Users.Add(new AppUser { Email = "admin@minishop.local", FullName = "Store Admin", PasswordHash = adminHash, PasswordSalt = adminSalt, IsAdmin = true });

            var (demoHash, demoSalt) = PasswordHasher.Hash("Demo123!");
            db.Users.Add(new AppUser { Email = "demo@minishop.local", FullName = "Demo User", PasswordHash = demoHash, PasswordSalt = demoSalt });
        }

        if (!db.Coupons.Any()) db.Coupons.Add(new Coupon { Code="WELCOME10", Percent=10, ExpiresAt=DateTime.UtcNow.AddYears(1) });
        if (!db.ShippingZones.Any())
        {
            // Demo values, modeled on zone-based fee + free-delivery-threshold schemes used by Egyptian marketplaces. Editable in Admin > Shipping.
            var zones = new[]
            {
                (new ShippingZone { Name="Greater Cairo", NameAr="القاهرة الكبرى", Fee=30, FreeThreshold=500, MinDays=1, MaxDays=2 }, new[]{("Cairo","القاهرة"),("Giza","الجيزة"),("Qalyoubia","القليوبية")}),
                (new ShippingZone { Name="Alexandria, Delta & Canal", NameAr="الإسكندرية والدلتا والقناة", Fee=45, FreeThreshold=750, MinDays=2, MaxDays=3 }, new[]{("Alexandria","الإسكندرية"),("Beheira","البحيرة"),("Gharbia","الغربية"),("Monufia","المنوفية"),("Dakahlia","الدقهلية"),("Sharqia","الشرقية"),("Kafr El Sheikh","كفر الشيخ"),("Damietta","دمياط"),("Port Said","بورسعيد"),("Ismailia","الإسماعيلية"),("Suez","السويس")}),
                (new ShippingZone { Name="Upper Egypt", NameAr="الصعيد", Fee=60, FreeThreshold=1000, MinDays=3, MaxDays=5 }, new[]{("Fayoum","الفيوم"),("Beni Suef","بني سويف"),("Minya","المنيا"),("Assiut","أسيوط"),("Sohag","سوهاج"),("Qena","قنا"),("Luxor","الأقصر"),("Aswan","أسوان")}),
                (new ShippingZone { Name="Remote areas", NameAr="المناطق النائية", Fee=90, FreeThreshold=1500, MinDays=4, MaxDays=7 }, new[]{("Red Sea","البحر الأحمر"),("New Valley","الوادي الجديد"),("Matrouh","مطروح"),("North Sinai","شمال سيناء"),("South Sinai","جنوب سيناء")})
            };
            foreach (var (zone, regions) in zones)
            {
                db.ShippingZones.Add(zone); db.SaveChanges();
                foreach (var (en, ar) in regions) db.ShippingRegions.Add(new ShippingRegion { Name=en, NameAr=ar, ZoneId=zone.Id });
            }
        }
        if (!db.ShippingSettings.Any()) db.ShippingSettings.Add(new ShippingSettings { Id=1, CodEnabled=true, CodFee=15, CodMaxOrder=20000 });
        db.SaveChanges();
    }
}
