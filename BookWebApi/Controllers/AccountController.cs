using DTOLayer.WebApiDTO.AppUserDTO;
using EntityLayer.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost("login")]
        [AllowAnonymous] // Bu metot herkese açık olmalı, giriş yapmadan erişilebilir.
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            // Model doğrulamasını kontrol et
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Kullanıcıyı hem Email hem de UserName ile bulmaya çalış
            var user = await _userManager.FindByEmailAsync(model.UserNameOrEmail);
            if (user == null)
            {
                user = await _userManager.FindByNameAsync(model.UserNameOrEmail);
            }
            if (user == null || !user.IsActive)
            {
                ModelState.AddModelError(string.Empty, "Geçersiz kullanıcı adı/email veya şifre.");
                return BadRequest(ModelState);
            }

            // Şifreyi doğrula
            var result = await _signInManager.PasswordSignInAsync(user, model.Password, isPersistent: false, lockoutOnFailure: false);
            // lockoutOnFailure: false -> Yanlış şifre denemelerinde hesabı kilitleme (Identity ayarlarında yönetilir).

            if (result.Succeeded)
            {
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

        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok(new { Message = "Başarıyla çıkış yapıldı." });
        }
    }
}
