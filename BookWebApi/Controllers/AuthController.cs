using EntityLayer.Entities; 
using Microsoft.AspNetCore.Http; 
using Microsoft.AspNetCore.Identity; 
using Microsoft.AspNetCore.Mvc; 
using Microsoft.EntityFrameworkCore; 
using AutoMapper; 
using AutoMapper.QueryableExtensions;
using DTOLayer.WebApiDTO.AppUserDTO;
using Microsoft.AspNetCore.Authorization;

namespace WebApi.Controllers 
{
    [Route("api/[controller]")] 
    [ApiController] 
    public class AuthController : ControllerBase 
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper; 

        public AuthController(UserManager<AppUser> userManager, IMapper mapper) 
        {
            _userManager = userManager; 
            _mapper = mapper; 
        }

        [HttpGet("list")] 
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)] 
        [ProducesResponseType(StatusCodes.Status403Forbidden)] 
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetListAllUser()
        {
            // UserManager'ın Users özelliği IQueryable<AppUser> döndürür.
            // Bu sayede LINQ sorguları ile veritabanı seviyesinde filtreleme, sıralama yapabiliriz.
            // AutoMapper'ın ProjectTo uzantı metodu, IQueryable üzerinde doğrudan DTO'ya dönüşüm yapar
            // ve sadece gerekli sütunları veritabanından çeker, bu da performansı artırır.
            var users = await _userManager.Users
                                          .ProjectTo<UserListDto>(_mapper.ConfigurationProvider) // AutoMapper ile AppUser'dan UserListDto'ya dönüşüm
                                          .ToListAsync(); // Veritabanından veriyi asenkron olarak çeker.

            // Kullanıcıların boş olup olmadığını kontrol etme, genelde bu bir hata durumu değildir
            // return Ok(users); döndürmek yeterlidir. Ancak özel bir durum için kontrol edilebilir.
            if (users == null || !users.Any())
            {
                return NotFound(new { Message = "Sistemde kayıtlı kullanıcı bulunamadı." });
            }

            return Ok(users); // Kullanıcı listesini başarıyla döner.
        }

        /// <summary>
        /// Yeni bir kullanıcı kaydeder.
        /// </summary>
        /// <param name="model">Kayıt için gerekli kullanıcı bilgileri.</param>
        /// <returns>Kayıt işleminin başarılı olup olmadığını gösteren bir sonuç.</returns>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterDto model) // Register metodunu tanımlar, RegisterDto tipinde bir model alır ve asenkron çalışır.
        {
            // Model doğrulamasını kontrol et (DataAnnotations tarafından yapılır)
            if (!ModelState.IsValid) // Gelen modelin Data Annotations kurallarına göre geçerli olup olmadığını kontrol eder.
            {
                return BadRequest(ModelState); // Eğer model geçerli değilse, 400 Bad Request döner ve model hatalarını içerir.
            }

            // Email ile aynı kullanıcı adı kullanma varsayımı
            var userExistsByEmail = await _userManager.FindByEmailAsync(model.Email); // Veritabanında bu email adresine sahip bir kullanıcı olup olmadığını kontrol eder.
            if (userExistsByEmail != null) // Eğer bu email adresine sahip bir kullanıcı zaten varsa...
            {
                return BadRequest(new { Message = "Bu email adresi zaten kullanılıyor." }); // 400 Bad Request döner ve hata mesajını içerir.
            }

            // Kullanıcı adı ile aynı kullanıcı adı kullanma varsayımı (Yeni eklendi)
            var userExistsByUserName = await _userManager.FindByNameAsync(model.UserName); // Veritabanında bu kullanıcı adına sahip bir kullanıcı olup olmadığını kontrol eder.
            if (userExistsByUserName != null) // Eğer bu kullanıcı adına sahip bir kullanıcı zaten varsa...
            {
                return BadRequest(new { Message = "Bu kullanıcı adı zaten kullanılıyor." }); // 400 Bad Request döner ve hata mesajını içerir.
            }


            // Yeni AppUser nesnesini oluştur
            AppUser newUser = new AppUser() // Yeni bir AppUser nesnesi oluşturur.
            {
                Email = model.Email, // Modelden gelen email'i AppUser'ın Email özelliğine atar.
                UserName = model.UserName, // Modelden gelen UserName'i kullanır.
                FirstName = model.FirstName, // Modelden gelen adı AppUser'ın FirstName özelliğine atar.
                LastName = model.LastName, // Modelden gelen soyadı AppUser'ın LastName özelliğine atar.
                IsActive = true, // Yeni kullanıcıyı varsayılan olarak aktif yapar.
                CreatedAt = DateTime.UtcNow // Kullanıcının oluşturulma tarihini UTC olarak ayarlar.
            };

            // Kullanıcıyı Identity sistemine kaydet
            var result = await _userManager.CreateAsync(newUser, model.Password); // Yeni kullanıcıyı ve şifresini Identity sistemine kaydetmeye çalışır.

            if (result.Succeeded) // Eğer kullanıcı kayıt işlemi başarılıysa...
            {
                // Başarılı kayıt sonrası isterseniz varsayılan bir rol atayabilirsiniz,
                // örneğin "Customer" veya "Member" rolü.
                // await _userManager.AddToRoleAsync(newUser, "Customer"); // Örnek: Yeni kullanıcıya "Customer" rolünü atar (yorum satırı yapılmış).

                return Ok(new { Message = "Kullanıcı başarıyla kaydedildi." }); // 200 OK döner ve başarılı kayıt mesajını içerir.
            }

            // Hatalar varsa, hata mesajlarını dön
            foreach (var error in result.Errors) // Kayıt işlemi sırasında oluşan hataları (IdentityResult.Errors) döngüye alır.
            {
                ModelState.AddModelError(string.Empty, error.Description); // Her bir hatayı ModelState'e ekler.
            }
            return BadRequest(ModelState); // Hataları içeren 400 Bad Request döner.
        }
    }
}
