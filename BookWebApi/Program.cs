using DataAccessLayer.Context; // DbContext i�in
using EntityLayer.Entities; // AppUser, AppRole i�in
using Microsoft.AspNetCore.Authorization; // Yetkilendirme policy i�in
using Microsoft.AspNetCore.Identity; // Identity servisleri i�in
using Microsoft.AspNetCore.Mvc.Authorization; // AuthorizeFilter i�in
using Microsoft.EntityFrameworkCore; // UseNpgsql i�in
using WebApi.Mapping; // AutoMapper GeneralMapping profiliniz i�in

var builder = WebApplication.CreateBuilder(args);

// Connection string appsettings.json i�inde olmal�
var connectionString = builder.Configuration.GetConnectionString("PostgreSqlConnection");

// DbContext ve Identity servislerini ekle
builder.Services.AddDbContext<ETicaretDb>(options =>
    options.UseNpgsql(connectionString)); // PostgreSQL kulland���n�z� varsay�yorum

// --- BURADAN �T�BAREN IDENTITY AYARLARI EKLEND� VE G�NCELLEND� ---
// Identity servislerini ve �ifre/kullan�c� se�eneklerini yap�land�rma
builder.Services.AddIdentity<AppUser, AppRole>(options => // AppRole kulland���n�z i�in AppRole olarak b�rak�ld�
{
    // �ifre Politikalar� (g�venli�i art�rmak i�in)
    options.Password.RequireDigit = true; // �ifrede en az bir rakam olmal�
    options.Password.RequireLowercase = true; // �ifrede en az bir k���k harf olmal�
    options.Password.RequireUppercase = true; // �ifrede en az bir b�y�k harf olmal�
    options.Password.RequireNonAlphanumeric = true; // �ifrede en az bir �zel karakter olmal�
    options.Password.RequiredLength = 8; // �ifre en az 8 karakter uzunlu�unda olmal�
    // options.Password.RequiredUniqueChars = 1; // Opsiyonel: �ifrede tekrarlayan karakterlerin minimum say�s�

    // Kullan�c� Ayarlar�
    options.User.RequireUniqueEmail = true; // Her kullan�c�n�n benzersiz bir e-posta adresi olmas� zorunludur

    // Giri� Ayarlar� (API projelerinde genellikle e-posta onay� gerekmez, ancak kontrol edilebilir)
    options.SignIn.RequireConfirmedEmail = false; // E-posta onay�n�n giri� i�in zorunlu olup olmad���n� belirler
    options.SignIn.RequireConfirmedAccount = false; // Hesap onay�n�n giri� i�in zorunlu olup olmad���n� belirler
})
.AddEntityFrameworkStores<ETicaretDb>() // Identity'nin veritaban� deposunu belirtir
.AddDefaultTokenProviders(); // �ifre s�f�rlama, e-posta onay� vb. i�in token sa�lay�c�lar�n� ekler

// Identity'nin Cookie ayarlar�n� API'lerde genel olarak kurcalaman�za gerek yok,
// ��nk� API'ler genellikle stateless �al���r ve client (Web UI) kendi �erezlerini y�netir.
// Ancak e�er API'niz de do�rudan cookie tabanl� kimlik do�rulama kullanacaksa,
// varsay�lan ayarlar genellikle yeterlidir.
// E�er spesifik cookie ayarlar� gerekiyorsa buradan devam edebiliriz:
// builder.Services.ConfigureApplicationCookie(options =>
// {
//     options.LoginPath = "/Account/Login"; // API i�in bu genellikle bir anlam ifade etmez
//     options.AccessDeniedPath = "/Account/AccessDenied";
//     options.ReturnUrlParameter = "ReturnUrl";
//     options.Cookie.Name = "ETicaretApiCookie"; // API'ye �zel bir isim verebilirsiniz
//     options.Cookie.HttpOnly = true;
//     options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
//     options.Cookie.SameSite = SameSiteMode.Lax; // API i�in Strict yerine Lax daha esnek olabilir
//     options.ExpireTimeSpan = TimeSpan.FromHours(15);
// });
// --- IDENTITY AYARLARI SONA ERD� ---


// AutoMapper servislerini ekle
builder.Services.AddAutoMapper(typeof(GeneralMapping)); // GeneralMapping s�n�f�n�z�n bulundu�u assembly'yi belirtir

// Global yetkilendirme filtresi ekle
// Bu blok, t�m Controller'lar i�in varsay�lan olarak kimlik do�rulamas� gereklili�ini ayarlar.
builder.Services.AddControllers(options =>
{
    // T�m Controller'lar i�in default yetkilendirme uygula
    // Yani, hi�bir [AllowAnonymous] niteli�i yoksa, kullan�c� giri�i gerekecek.
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser() // Kimlik do�rulamas� yap�lm�� kullan�c� gerektirir
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy)); // Olu�turulan politikay� bir filtre olarak ekler
});

// Dikkat! builder.Services.AddControllers(); sat�r� burada tekrarlanmamal�d�r!
// E�er varsa, bu sat�r� silin: builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer(); // API ke�fi i�in gerekli (Swagger i�in)
builder.Services.AddSwaggerGen(); // Swagger/OpenAPI dok�mantasyonu i�in gerekli

var app = builder.Build();

// Geli�tirme ortam�nda Swagger UI'� etkinle�tir
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // Swagger JSON endpoint'ini etkinle�tirir
    app.UseSwaggerUI(); // Swagger UI aray�z�n� etkinle�tirir
}

app.UseHttpsRedirection(); // HTTP isteklerini HTTPS'e y�nlendirir

app.UseAuthentication();  // Kimlik do�rulama middleware'i: Kullan�c�n�n kimli�ini do�rular.
app.UseAuthorization();   // Yetkilendirme middleware'i: Do�rulanm�� kullan�c�n�n eri�im yetkilerini kontrol eder.

app.MapControllers(); // Controller'lardaki rotalar� e�ler

// Admin rol ve kullan�c� olu�turma i�lemini async method i�ine al
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

    string adminRole = "Admin";
    string adminEmail = "kcdmirapo96@gmail.com";
    string adminPassword = "123456aA*"; // �ifre politikalar�n�za uygun olmal�!

    // Admin rol�n�n varl���n� kontrol et ve yoksa olu�tur
    if (!await roleManager.RoleExistsAsync(adminRole))
    {
        await roleManager.CreateAsync(new AppRole
        {
            Name = adminRole,
            Description = "Sistem Y�neticisi"
        });
    }

    // Admin kullan�c�s�n�n varl���n� kontrol et ve yoksa olu�tur
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new AppUser
        {
            UserName = "Apo2550", // Admin kullan�c�s� i�in kullan�c� ad�
            Email = adminEmail,
            EmailConfirmed = true,
            FirstName = "Abdullah",
            LastName = "KO�DEM�R",
            CreatedAt = DateTime.UtcNow // DateTimeKind hatas�n� �nlemek i�in UTC
        };

        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, adminRole); // Admin rol�n� kullan�c�ya ata
        }
        else
        {
            // Admin kullan�c� olu�turulamazsa hata mesajlar�n� loglamak iyi bir pratiktir
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"Admin User Creation Error: {error.Description}");
            }
        }
    }
}

app.Run(); // Uygulamay� ba�lat