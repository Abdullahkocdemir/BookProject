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

            var users = await _userManager.Users
                                          .ProjectTo<UserListDto>(_mapper.ConfigurationProvider) 
                                          .ToListAsync(); 

            if (users == null || !users.Any())
            {
                return NotFound(new { Message = "Sistemde kayıtlı kullanıcı bulunamadı." });
            }

            return Ok(users); 
        }



        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userExistsByEmail = await _userManager.FindByEmailAsync(model.Email);
            if (userExistsByEmail != null)
            {
                return BadRequest(new { Message = "Bu email adresi zaten kullanılıyor." });
            }
            var userExistsByUserName = await _userManager.FindByNameAsync(model.UserName);
            if (userExistsByUserName != null)
            {
                return BadRequest(new { Message = "Bu kullanıcı adı zaten kullanılıyor." });
            }
            var newUser = _mapper.Map<AppUser>(model);

            var result = await _userManager.CreateAsync(newUser, model.Password);
            if (result.Succeeded)
            {
                var roleResult = await _userManager.AddToRoleAsync(newUser, "User");

                if (roleResult.Succeeded)
                {
                    return Ok(new { Message = "Kullanıcı başarıyla kaydedildi ve 'User' rolü atandı." });
                }
                else
                {
                    foreach (var error in roleResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Kullanıcı kaydedildi ancak rol atanamadı.", Errors = ModelState });
                }
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return BadRequest(ModelState);
        }


    }
}
