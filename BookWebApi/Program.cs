using DataAccessLayer.Context; // DbContext için
using EntityLayer.Entities; // AppUser, AppRole için
using Microsoft.AspNetCore.Authorization; // Yetkilendirme policy için
using Microsoft.AspNetCore.Identity; // Identity servisleri için
using Microsoft.AspNetCore.Mvc.Authorization; // AuthorizeFilter için
using Microsoft.EntityFrameworkCore; // UseNpgsql için
using WebApi.Mapping; // AutoMapper GeneralMapping profiliniz için

var builder = WebApplication.CreateBuilder(args);

// Connection string appsettings.json içinde olmalý
var connectionString = builder.Configuration.GetConnectionString("PostgreSqlConnection");

// DbContext ve Identity servislerini ekle
builder.Services.AddDbContext<ETicaretDb>(options =>
    options.UseNpgsql(connectionString)); // PostgreSQL kullandýðýnýzý varsayýyorum

// --- BURADAN ÝTÝBAREN IDENTITY AYARLARI EKLENDÝ VE GÜNCELLENDÝ ---
// Identity servislerini ve þifre/kullanýcý seçeneklerini yapýlandýrma
builder.Services.AddIdentity<AppUser, AppRole>(options => // AppRole kullandýðýnýz için AppRole olarak býrakýldý
{
    // Þifre Politikalarý (güvenliði artýrmak için)
    options.Password.RequireDigit = true; // Þifrede en az bir rakam olmalý
    options.Password.RequireLowercase = true; // Þifrede en az bir küçük harf olmalý
    options.Password.RequireUppercase = true; // Þifrede en az bir büyük harf olmalý
    options.Password.RequireNonAlphanumeric = true; // Þifrede en az bir özel karakter olmalý
    options.Password.RequiredLength = 8; // Þifre en az 8 karakter uzunluðunda olmalý
    // options.Password.RequiredUniqueChars = 1; // Opsiyonel: Þifrede tekrarlayan karakterlerin minimum sayýsý

    // Kullanýcý Ayarlarý
    options.User.RequireUniqueEmail = true; // Her kullanýcýnýn benzersiz bir e-posta adresi olmasý zorunludur

    // Giriþ Ayarlarý (API projelerinde genellikle e-posta onayý gerekmez, ancak kontrol edilebilir)
    options.SignIn.RequireConfirmedEmail = false; // E-posta onayýnýn giriþ için zorunlu olup olmadýðýný belirler
    options.SignIn.RequireConfirmedAccount = false; // Hesap onayýnýn giriþ için zorunlu olup olmadýðýný belirler
})
.AddEntityFrameworkStores<ETicaretDb>() // Identity'nin veritabaný deposunu belirtir
.AddDefaultTokenProviders(); // Þifre sýfýrlama, e-posta onayý vb. için token saðlayýcýlarýný ekler

// Identity'nin Cookie ayarlarýný API'lerde genel olarak kurcalamanýza gerek yok,
// çünkü API'ler genellikle stateless çalýþýr ve client (Web UI) kendi çerezlerini yönetir.
// Ancak eðer API'niz de doðrudan cookie tabanlý kimlik doðrulama kullanacaksa,
// varsayýlan ayarlar genellikle yeterlidir.
// Eðer spesifik cookie ayarlarý gerekiyorsa buradan devam edebiliriz:
// builder.Services.ConfigureApplicationCookie(options =>
// {
//     options.LoginPath = "/Account/Login"; // API için bu genellikle bir anlam ifade etmez
//     options.AccessDeniedPath = "/Account/AccessDenied";
//     options.ReturnUrlParameter = "ReturnUrl";
//     options.Cookie.Name = "ETicaretApiCookie"; // API'ye özel bir isim verebilirsiniz
//     options.Cookie.HttpOnly = true;
//     options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
//     options.Cookie.SameSite = SameSiteMode.Lax; // API için Strict yerine Lax daha esnek olabilir
//     options.ExpireTimeSpan = TimeSpan.FromHours(15);
// });
// --- IDENTITY AYARLARI SONA ERDÝ ---


// AutoMapper servislerini ekle
builder.Services.AddAutoMapper(typeof(GeneralMapping)); // GeneralMapping sýnýfýnýzýn bulunduðu assembly'yi belirtir

// Global yetkilendirme filtresi ekle
// Bu blok, tüm Controller'lar için varsayýlan olarak kimlik doðrulamasý gerekliliðini ayarlar.
builder.Services.AddControllers(options =>
{
    // Tüm Controller'lar için default yetkilendirme uygula
    // Yani, hiçbir [AllowAnonymous] niteliði yoksa, kullanýcý giriþi gerekecek.
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser() // Kimlik doðrulamasý yapýlmýþ kullanýcý gerektirir
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy)); // Oluþturulan politikayý bir filtre olarak ekler
});

// Dikkat! builder.Services.AddControllers(); satýrý burada tekrarlanmamalýdýr!
// Eðer varsa, bu satýrý silin: builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer(); // API keþfi için gerekli (Swagger için)
builder.Services.AddSwaggerGen(); // Swagger/OpenAPI dokümantasyonu için gerekli

var app = builder.Build();

// Geliþtirme ortamýnda Swagger UI'ý etkinleþtir
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // Swagger JSON endpoint'ini etkinleþtirir
    app.UseSwaggerUI(); // Swagger UI arayüzünü etkinleþtirir
}

app.UseHttpsRedirection(); // HTTP isteklerini HTTPS'e yönlendirir

app.UseAuthentication();  // Kimlik doðrulama middleware'i: Kullanýcýnýn kimliðini doðrular.
app.UseAuthorization();   // Yetkilendirme middleware'i: Doðrulanmýþ kullanýcýnýn eriþim yetkilerini kontrol eder.

app.MapControllers(); // Controller'lardaki rotalarý eþler

// Admin rol ve kullanýcý oluþturma iþlemini async method içine al
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

    string adminRole = "Admin";
    string adminEmail = "kcdmirapo96@gmail.com";
    string adminPassword = "123456aA*"; // Þifre politikalarýnýza uygun olmalý!

    // Admin rolünün varlýðýný kontrol et ve yoksa oluþtur
    if (!await roleManager.RoleExistsAsync(adminRole))
    {
        await roleManager.CreateAsync(new AppRole
        {
            Name = adminRole,
            Description = "Sistem Yöneticisi"
        });
    }

    // Admin kullanýcýsýnýn varlýðýný kontrol et ve yoksa oluþtur
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new AppUser
        {
            UserName = "Apo2550", // Admin kullanýcýsý için kullanýcý adý
            Email = adminEmail,
            EmailConfirmed = true,
            FirstName = "Abdullah",
            LastName = "KOÇDEMÝR",
            CreatedAt = DateTime.UtcNow // DateTimeKind hatasýný önlemek için UTC
        };

        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, adminRole); // Admin rolünü kullanýcýya ata
        }
        else
        {
            // Admin kullanýcý oluþturulamazsa hata mesajlarýný loglamak iyi bir pratiktir
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"Admin User Creation Error: {error.Description}");
            }
        }
    }
}

app.Run(); // Uygulamayý baþlat