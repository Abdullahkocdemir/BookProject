using BusinessLayer.ValidationRules.ProductValidator;
using DataAccessLayer.Context;
using EntityLayer.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using WebApi.Mapping;
using BusinessLayer.Conteiner;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("PostgreSqlConnection");

// DbContext ve Identity servislerini ekle
builder.Services.AddDbContext<ETicaretDb>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

// Dependency Injection
builder.Services.ConteinerDependencies();


// --- BURADAN ÝTÝBAREN IDENTITY AYARLARI EKLENDÝ VE GÜNCELLENDÝ ---
// Identity servislerini ve þifre/kullanýcý seçeneklerini yapýlandýrma
builder.Services.AddIdentity<AppUser, AppRole>(options => // AppRole kullandýðýnýz için AppRole olarak býrakýldý
{
    // Þifre Politikalarý (güvenliði artýrmak için)
    options.Password.RequireDigit = true; // Þifrede en az bir rakam olmalý
    options.Password.RequireLowercase = true; // Þifrede en az bir küçük harf olmalý
    options.Password.RequireUppercase = true; // Þifrede en az bir büyük harf olmalý
    options.Password.RequireNonAlphanumeric = true; // Þifrede en az bir özel karakter olmalý
    options.Password.RequiredLength = 6; // Þifre en az 8 karakter uzunluðunda olmalý
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


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

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