using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using CapsuleAPI.Data;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using CapsuleAPI.DTO;
using CapsuleAPI.Helper;
using Microsoft.AspNetCore.Authorization;

namespace CapsuleAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IConfiguration _config;


        public AuthController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IConfiguration config)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _config = config;
        }

        [HttpPost("register")]
        
        public async Task<IActionResult> Register(RegisterModel model)
        {
            var user = new AppUser
            {
                UserName = model.Username,
                Email = model.Email,
                Password = model.Password
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok(new { message = "User Registered Successfully. You can Login Now." });

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            if ((user == null) || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                return Unauthorized("Invalid Login Credentials! Please Check Username or Password.");
            }

            var myToken = JwtTokenHelper.GenerateToken(user, _config);


            return Ok(new
            {
                message = "Logged in successfully!",
                token = myToken
            });
        }

    }
}
