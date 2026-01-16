using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Orbitask.Models;
using Orbitask.Models.ModelDtos;
using Orbitask.Services.Interfaces;

namespace Orbitask.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> userManager;
        private readonly ITokenService tokenService;
        private readonly SignInManager<User> signInManager;

        public AuthController(UserManager<User> userManager, ITokenService tokenService, SignInManager<User> signInManager)
        {
            this.userManager = userManager;
            this.tokenService = tokenService;
            this.signInManager = signInManager;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                var User = new User
                {
                    DisplayName = registerDto.DisplayName,
                    UserName = registerDto.UserName,
                    Email = registerDto.Email
                };
                var createdUser = await userManager.CreateAsync(User, registerDto.Password);
                if (createdUser.Succeeded)
                {
                    return Ok(
                        new NewUserDto
                        {
                            DisplayName = User.DisplayName,
                            UserName = User.UserName,
                            Email = User.Email,
                            Token = tokenService.CreateToken(User)
                        }
                    );
                }
                else
                {
                    return BadRequest(createdUser.Errors);
                }

            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                BadRequest(ModelState);
            }
            var user = await userManager.Users.FirstOrDefaultAsync(u => u.UserName == loginDto.UserName.ToLower());
            if (user == null)
                return Unauthorized("Invalid username or password!");
            var result = await signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
            if (!result.Succeeded)
                return Unauthorized("Invalid username or password!");
            return Ok(
               new NewUserDto
               {
                   DisplayName = user.DisplayName,
                   UserName = user.UserName,
                   Email = user.Email,
                   Token = tokenService.CreateToken(user)
               }
            );
        }
    }
}
