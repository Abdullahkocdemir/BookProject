using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc; 
using Microsoft.AspNetCore.Identity; 
using System.Threading.Tasks;
using System.Collections.Generic; 
using System.Linq; 
using Microsoft.AspNetCore.Authorization; 
using EntityLayer.Entities; 
using DTOLayer.WebApiDTO.AppRoleDTO; 
using DTOLayer.WebApiDTO.AppUserDTO; 

namespace WebApi.Controllers 
{
    [Route("api/[controller]")] 
    [ApiController] 
    [Authorize(Roles = "Admin")] 
    public class AdminController : ControllerBase 
    {
        private readonly RoleManager<AppRole> _roleManager; 
        private readonly UserManager<AppUser> _userManager; 

        public AdminController(RoleManager<AppRole> roleManager, UserManager<AppUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager; 
        }

        [HttpPost("roles")] 
        public async Task<IActionResult> CreateRole([FromBody] RoleCreateDto model) // 
        {
            if (!ModelState.IsValid) // Model doğrulaması başarısız olursa (DTO'daki Required gibi öznitelikler).
            {
                return BadRequest(ModelState); // Geçersiz model durumuyla 400 Bad Request döner.
            }

            // Rolün zaten var olup olmadığını kontrol et.
            if (await _roleManager.RoleExistsAsync(model.RoleName)) // Belirtilen rol adının zaten var olup olmadığını asenkron olarak kontrol eder.
            {
                return BadRequest($"Role '{model.RoleName}' already exists."); // Rol zaten varsa 400 Bad Request döner.
            }

            var role = new AppRole // Yeni bir AppRole nesnesi oluşturur.
            {
                Name = model.RoleName, // Rol adını DTO'dan alır.
                Description = model.Description // Rol açıklamasını DTO'dan alır.
            };

            var result = await _roleManager.CreateAsync(role); // Rolü asenkron olarak oluşturmayı dener.

            if (result.Succeeded) // Rol oluşturma başarılı olursa.
            {
                return Ok($"Role '{model.RoleName}' created successfully."); // Başarılı mesajıyla 200 OK döner.
            }

            return BadRequest(result.Errors.Select(e => e.Description)); // Hata oluşursa, hataların açıklamalarıyla birlikte 400 Bad Request döner.
        }

        [HttpGet("roles")] // Bu eylemin HTTP GET metoduyla ve "api/Admin/roles" yoluyla erişilebilir olduğunu belirtir.
        public IActionResult GetAllRoles() // Tüm rolleri getirmek için eylem metodu.
        {
            var roles = _roleManager.Roles // RoleManager'daki tüm rolleri alır.
                                        .Select(r => new RoleDto // Her rolü bir RoleDto nesnesine dönüştürür.
                                        {
                                            Id = r.Id, // Rol ID'si
                                            Name = r.Name ?? string.Empty, // Rol adı (null olabilecek durumlar için boş string atanır).
                                            Description = r.Description // Rol açıklaması.
                                        })
                                        .ToList(); // Sonuçları bir liste olarak alır.
            return Ok(roles); // Rol listesiyle 200 OK döner.
        }

        [HttpGet("users")] // Bu eylemin HTTP GET metoduyla ve "api/Admin/users" yoluyla erişilebilir olduğunu belirtir.
        public async Task<IActionResult> GetAllUsers() // Tüm kullanıcıları ve rollerini getirmek için asenkron eylem metodu.
        {
            var users = _userManager.Users.ToList(); // Tüm kullanıcıları bir liste olarak alır.
            var userDtos = new List<AppUserRoleDTO>(); // AppUserRoleDTO nesnelerini tutacak bir liste oluşturur.

            foreach (var user in users) // Her bir kullanıcı için döngü başlatır.
            {
                var roles = await _userManager.GetRolesAsync(user); // Kullanıcının sahip olduğu rolleri asenkron olarak alır.
                userDtos.Add(new AppUserRoleDTO // Yeni bir AppUserRoleDTO nesnesi oluşturur ve listeye ekler.
                {
                    Id = user.Id, // Kullanıcı ID'si.
                    UserName = user.UserName ?? string.Empty, // Kullanıcı adı (null olabilecek durumlar için boş string atanır).
                    Email = user.Email ?? string.Empty, // E-posta (null olabilecek durumlar için boş string atanır).
                    FullName = user.FullName, // Tam adı.
                    IsActive = user.IsActive, // Hesabın aktif olup olmadığı.
                    Roles = roles.ToList() // Kullanıcının rollerini listeye ekler.
                });
            }
            return Ok(userDtos); // Kullanıcı ve rol listesiyle 200 OK döner.
        }

        [HttpPost("users/assign-role")] // Bu eylemin HTTP POST metoduyla ve "api/Admin/users/assign-role" yoluyla erişilebilir olduğunu belirtir.
        public async Task<IActionResult> AssignRoleToUser([FromBody] UserRoleUpdateDto model) // Bir kullanıcıya rol atamak için asenkron eylem metodu. İstek gövdesinden UserRoleUpdateDto alır.
        {
            if (!ModelState.IsValid) // Model doğrulaması başarısız olursa.
            {
                return BadRequest(ModelState); // Geçersiz model durumuyla 400 Bad Request döner.
            }

            var user = await _userManager.FindByIdAsync(model.UserId); // Belirtilen ID'ye sahip kullanıcıyı asenkron olarak bulur.
            if (user == null) // Kullanıcı bulunamazsa.
            {
                return NotFound($"User with ID '{model.UserId}' not found."); // 404 Not Found döner.
            }

            if (!await _roleManager.RoleExistsAsync(model.RoleName)) // Atanacak rolün var olup olmadığını kontrol eder.
            {
                return NotFound($"Role '{model.RoleName}' not found."); // Rol bulunamazsa 404 Not Found döner.
            }

            // Kullanıcının zaten bu role sahip olup olmadığını kontrol et.
            if (await _userManager.IsInRoleAsync(user, model.RoleName)) // Kullanıcının zaten belirtilen rolde olup olmadığını kontrol eder.
            {
                return BadRequest($"User '{user.UserName}' already has role '{model.RoleName}'."); // Kullanıcı zaten rolde ise 400 Bad Request döner.
            }

            var result = await _userManager.AddToRoleAsync(user, model.RoleName); // Kullanıcıya rolü asenkron olarak atar.

            if (result.Succeeded) // Rol atama başarılı olursa.
            {
                return Ok($"Role '{model.RoleName}' assigned to user '{user.UserName}' successfully."); // Başarılı mesajıyla 200 OK döner.
            }

            return BadRequest(result.Errors.Select(e => e.Description)); // Hata oluşursa, hataların açıklamalarıyla birlikte 400 Bad Request döner.
        }

        [HttpPost("users/remove-role")] // Bu eylemin HTTP POST metoduyla ve "api/Admin/users/remove-role" yoluyla erişilebilir olduğunu belirtir.
        public async Task<IActionResult> RemoveRoleFromUser([FromBody] UserRoleUpdateDto model) // Bir kullanıcıdan rolü kaldırmak için asenkron eylem metodu. İstek gövdesinden UserRoleUpdateDto alır.
        {
            if (!ModelState.IsValid) // Model doğrulaması başarısız olursa.
            {
                return BadRequest(ModelState); // Geçersiz model durumuyla 400 Bad Request döner.
            }

            var user = await _userManager.FindByIdAsync(model.UserId); // Belirtilen ID'ye sahip kullanıcıyı asenkron olarak bulur.
            if (user == null) // Kullanıcı bulunamazsa.
            {
                return NotFound($"User with ID '{model.UserId}' not found."); // 404 Not Found döner.
            }

            if (!await _roleManager.RoleExistsAsync(model.RoleName)) // Kaldırılacak rolün var olup olmadığını kontrol eder.
            {
                return NotFound($"Role '{model.RoleName}' not found."); // Rol bulunamazsa 404 Not Found döner.
            }

            // Kullanıcının bu role sahip olup olmadığını kontrol et.
            if (!await _userManager.IsInRoleAsync(user, model.RoleName)) // Kullanıcının belirtilen rolde olup olmadığını kontrol eder.
            {
                return BadRequest($"User '{user.UserName}' does not have role '{model.RoleName}'."); // Kullanıcıda bu rol yoksa 400 Bad Request döner.
            }

            var result = await _userManager.RemoveFromRoleAsync(user, model.RoleName); // Kullanıcıdan rolü asenkron olarak kaldırır.

            if (result.Succeeded) // Rol kaldırma başarılı olursa.
            {
                return Ok($"Role '{model.RoleName}' removed from user '{user.UserName}' successfully."); // Başarılı mesajıyla 200 OK döner.
            }

            return BadRequest(result.Errors.Select(e => e.Description)); // Hata oluşursa, hataların açıklamalarıyla birlikte 400 Bad Request döner.
        }
    }
}