using DTOLayer.WebApiDTO.AppUserDTO;
using EntityLayer.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [AllowAnonymous] // Bu action'a herkes erişebilir
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(model.UserNameOrEmail);
            if (user == null)
            {
                user = await _userManager.FindByNameAsync(model.UserNameOrEmail);
            }

            // Kullanıcı bulunamadıysa veya aktif değilse
            if (user == null || !user.IsActive) // IsActive özelliğini EntityLayer.Entities.AppUser'da tanımladığınız varsayılmıştır.
            {
                ModelState.AddModelError(string.Empty, "Geçersiz kullanıcı adı/email veya şifre.");
                return BadRequest(ModelState);
            }

            // Şifreyi doğrula
            var result = await _signInManager.PasswordSignInAsync(user, model.Password, isPersistent: false, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                // JWT Token oluşturma ve döndürme mantığı buraya eklenecek.
                // Şimdilik sadece mesaj döndürüyoruz.
                return Ok(new { Message = "Giriş başarılı!" });
            }
            else if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Hesabınız kilitlenmiştir. Lütfen daha sonra tekrar deneyin.");
                return BadRequest(ModelState);
            }
            else if (result.IsNotAllowed)
            {
                ModelState.AddModelError(string.Empty, "Giriş izni verilmedi (örneğin, email onayı gerekebilir).");
                return BadRequest(ModelState);
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Geçersiz kullanıcı adı/email veya şifre.");
                return BadRequest(ModelState);
            }
        }

        [Authorize]
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok(new { Message = "Başarıyla çıkış yapıldı." });
        }


        [Authorize]
        [HttpGet("user/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)] // Yetkisiz erişim için
        public async Task<IActionResult> GetUserById(string id)
        {
            var currentUser = await _userManager.GetUserAsync(User); // Mevcut oturum açmış kullanıcıyı al
            if (currentUser == null)
            {
                return Unauthorized("Kullanıcı oturumu bulunamadı."); // Oturum açmış kullanıcı yoksa
            }

            // Eğer mevcut kullanıcı, kendi bilgilerini istiyorsa veya admin rolündeyse devam et
            if (currentUser.Id != id && !await _userManager.IsInRoleAsync(currentUser, "Admin"))
            {
                // Mevcut kullanıcı kendi ID'si dışında bir ID istiyor ve admin değilse yetkisiz
                return Forbid("Bu bilgilere erişim yetkiniz yok.");
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound($"ID'si '{id}' olan kullanıcı bulunamadı.");
            }

            var userDto = new AppUserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email!,
                UserName = user.UserName!,
            };

            return Ok(userDto);
        }

        [Authorize]
        [HttpPost("change-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.GetUserAsync(User); // Mevcut oturum açmış kullanıcıyı al
            if (user == null)
            {
                return Unauthorized("Kullanıcı oturumu bulunamadı.");
            }

            // Şifreyi değiştir
            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (result.Succeeded)
            {
                // Şifre başarıyla değiştirildiyse, kullanıcının oturumunu yeniden oluşturmak iyi bir güvenlik pratiğidir.
                // Bu, eski token'ların veya çerezlerin geçersiz kalmasını sağlar.
                await _signInManager.SignOutAsync(); // Önceki oturumu kapat
                // Kullanıcıya tekrar giriş yapması gerektiğini bildirin.
                return Ok(new { Message = "Şifreniz başarıyla değiştirildi. Lütfen yeni şifrenizle tekrar giriş yapın." });
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return BadRequest(ModelState);
            }
        }
    }
}