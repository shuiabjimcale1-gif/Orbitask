using System.ComponentModel.DataAnnotations;

namespace Orbitask.Models.ModelDtos
{
    public class LoginDto
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
