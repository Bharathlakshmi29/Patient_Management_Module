using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Patient_mgt.Data;
using Patient_mgt.Domain;
using Patient_mgt.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Patient_Management_Module.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly PatientContext _context;
        private readonly SymmetricSecurityKey _key;

        public TokenController(PatientContext context, IConfiguration config)
        {
            _context = context;
            _key = new SymmetricSecurityKey(UTF8Encoding.UTF8.GetBytes(config["Key"]!));

        }
        [HttpPost]
        public IActionResult GenerateToken([FromBody] loginDTO logindto)
        {
            var user = ValidateUser(logindto.Email, logindto.Password);

            if (user == null)
            {
                return Unauthorized("Invalid email or password");
            }

            var claims = new List<Claim>
               {
                   new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                   new Claim(JwtRegisteredClaimNames.NameId, user.Name!),
                   new Claim(JwtRegisteredClaimNames.Email, user.EmailId),
               };

            if (user.Role != null)
                claims.Add(new Claim(ClaimTypes.Role, user.Role.ToString()));

            var cred = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);
            var tokenDescription = new SecurityTokenDescriptor
            {
                SigningCredentials = cred,
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddMinutes(30)
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var createToken = tokenHandler.CreateToken(tokenDescription);
            var token = tokenHandler.WriteToken(createToken);

            return Ok(new { token, role = user.Role.ToString(), username = user.Name, userId = user.UserId.ToString() });
        }
        private User ValidateUser(string email, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.EmailId == email);
            if (user == null) return null;
            
            bool isValidPassword = false;
            try
            {
                isValidPassword = BCrypt.Net.BCrypt.Verify(password, user.Password);
            }
            catch (BCrypt.Net.SaltParseException)
            {
                // Fallback for plain text passwords in development / seeded data
                isValidPassword = (password == user.Password);
            }

            return isValidPassword ? user : null;
        }
    }
}
