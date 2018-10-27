using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NgTemplate.Areas.Identity.Data;
using NgTemplate.Data.Dto;
using NgTemplate.Models;
using NgTemplate.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NgTemplate.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ApplicationUserDbContext _context;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly IConfiguration _configuration;

        public UserController(ApplicationUserDbContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<object> Register([FromBody]RegisterDto user)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var newUser = new ApplicationUser
            {
                UserName = user.UserName,
                Email = user.Email
            };

            var result = await _userManager.CreateAsync(newUser, user.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(newUser, user.Role);
            }

            if (!result.Succeeded)
            {
                throw new ApplicationException("Account creation did not success");
            }

            return CreatedAtAction("Register", user);
        }

        [HttpPost]
        public async Task<object> Login([FromBody] LoginDto loginDto)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(loginDto.Email);

            var result = await _signInManager.PasswordSignInAsync(user.UserName, loginDto.Password, false, lockoutOnFailure: false);
            

            if (result.Succeeded)
            {
                ApplicationUser appUser = _userManager.Users.SingleOrDefault(r => r.Email == loginDto.Email);

                var token = await GenerateJwtToken(loginDto.Email, appUser);

                return Ok(new
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    token = token
                });
            }
            throw new ApplicationException("Unknown Error");
        }

        private async Task<object> GenerateJwtToken(string email, ApplicationUser user)
        {
            var userRole = await _userManager.GetRolesAsync(user);
            // var userRole = _context.UserRoles.Where(i => i.UserId == user.Id);

            var claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(JwtRegisteredClaimNames.NameId, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim("username", user.UserName.ToString())
            };

            foreach (var role in userRole)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
                claims.Add(new Claim("role", role.ToString()));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("FF1C4E7EA16663E005A8EE0E214BCFA5F82A7DE9454916C0D33158AEF20F7B66ED626D3383A841FF22A9740F8805A3F965FC72CA55B6786CF2DACDBEAE174393"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(Convert.ToDouble(_configuration["JwtExpireDay"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtIssuer"],
                claims: claims,
                expires: expires,
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
