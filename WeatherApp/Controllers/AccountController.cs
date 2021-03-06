using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WeatherApp.Models;
using WeatherApp.Utilities;

namespace WeatherApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly AppSettings _opt;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IOptions<AppSettings> options)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _opt = options.Value;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User u)
        {
            var newUser = new IdentityUser
            {
                UserName = u.Email, 
                Email = u.Email
            };

            var userCreationResult = await _userManager.CreateAsync(newUser, u.Password);

            if (userCreationResult.Succeeded)
            {
                return Ok(newUser);
            }

            foreach (var error in userCreationResult.Errors) 
                ModelState.AddModelError(string.Empty, error.Description);

            return BadRequest(ModelState);

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] User u)
        {
            var user = await _userManager.FindByEmailAsync(u.Email);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login"); 
                return BadRequest(ModelState);
            } 
            
            var passwordSignInResult = await _signInManager.CheckPasswordSignInAsync(user, u.Password, false); 
            
            if (passwordSignInResult.Succeeded) 
                return new ObjectResult(GenerateToken(u.Email)); 
            
            return BadRequest("Invalid login");
        }

        private string GenerateToken(string username)
        {
            var claims = new Claim[]
            {
                new Claim(ClaimTypes.Name, username), 
                new Claim(JwtRegisteredClaimNames.Nbf, 
                    new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds().ToString()), 
                new Claim(JwtRegisteredClaimNames.Exp, 
                    new DateTimeOffset(DateTime.Now.AddDays(1)).ToUnixTimeSeconds().ToString()),
            };

            var key = Encoding.UTF8.GetBytes(_opt.SecretKey);
            var token = new JwtSecurityToken(
                new JwtHeader(
                    new SigningCredentials(
                        new SymmetricSecurityKey(key), 
                        SecurityAlgorithms.HmacSha256)), 
                new JwtPayload(claims));

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}