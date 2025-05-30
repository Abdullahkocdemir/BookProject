using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOLayer.WebApiDTO.AppUserDTO
{
    public class LoginDto
    {

        [Required(ErrorMessage = "Kullanıcı adı veya Email alanı zorunludur.")]
        public string UserNameOrEmail { get; set; } = string.Empty; // Hem kullanıcı adı hem de email için kullanılacak

        [Required(ErrorMessage = "Şifre alanı zorunludur.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
