using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using webbAPI.Models;
using webbAPI.Repositories;

namespace webbAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AccountController(IConfiguration config, AccountRepository accountRepository) : ControllerBase
    {
        private IConfiguration _config = config;
        private readonly AccountRepository _accountRepository = accountRepository;

        [HttpPost("login")]
        public IActionResult Post([FromBody] User user)
        {
             var fetchedUser = _accountRepository.GetUserCredentials(user, out string error);
            if (fetchedUser == null || !string.IsNullOrEmpty(error))
            {
                return BadRequest(error != "" ? error : "Fel användarnamn eller lösenord");
            }

            if (fetchedUser?.Username != null)
            {
                var hashedPassword = HashPassword(user.Password, fetchedUser.Salt);
                bool userIsValid = hashedPassword.SequenceEqual(fetchedUser.Password);
                if (!userIsValid && error == "")
                {
                    return BadRequest(error != "" ? error : "Fel användarnamn eller lösenord");
                }

                if (userIsValid)
                {
                    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
                    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
                    var tokenClaims = new List<Claim>
                    {
                        new("id", fetchedUser.Id.ToString()),
                        new("name", fetchedUser.Username)
                    };

                    var expirationTime = 2;

                    var Sectoken = new JwtSecurityToken(
                        issuer: _config["Jwt:Issuer"],
                        audience: _config["Jwt:Audience"],
                        claims: tokenClaims,
                        expires: DateTime.Now.AddHours(expirationTime),
                        signingCredentials: credentials
                        );

                    var token =  new JwtSecurityTokenHandler().WriteToken(Sectoken);

                    // var cookieOptions = new CookieOptions
                    // {
                    //     Expires = DateTime.Now.AddHours(expirationTime),
                    //     Secure = true,
                    //     HttpOnly = true,
                    // };
                    
                    // Response.Cookies.Append("Jwt-cookie", token, cookieOptions);

                    return Ok(token);
                }
            }
            return BadRequest("Något gick fel");
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] User user)
        {
            string base64Salt = GenerateSalt();
            user.Salt = base64Salt;
            
            var hashedPassword = HashPassword(user.Password, base64Salt);
            user.Password = hashedPassword;

            var insertedId = _accountRepository.Insert(user, out string error);

            if (insertedId == 0)
            {
                return BadRequest("Ingen användare skapades");
            }
            if (error != "")
            {
                return BadRequest(error);
            }
            return Ok(insertedId);
        }

        // Implementerad based on code example 
        // from https://www.thatsoftwaredude.com/content/6218/how-to-encrypt-passwords-using-sha-256-in-c-and-net
        private static string HashPassword(string password, string salt)
        {
            var passwordSalt = password + salt;
            byte[] hashPassword;
            UTF8Encoding objUtf8 = new();
            hashPassword = SHA256.HashData(objUtf8.GetBytes(passwordSalt));
            string hashPasswordString = Convert.ToBase64String(hashPassword);

            return hashPasswordString;
        }

        // Based on code example from https://medium.com/@imAkash25/hashing-and-salting-passwords-in-c-0ee223f07e20
        // and https://juldhais.net/secure-way-to-store-passwords-in-database-using-sha256-asp-net-core-898128d1c4ef
        private static string GenerateSalt()
        {
            using var rng = RandomNumberGenerator.Create();
            byte[] byteSalt = new byte[16];
            rng.GetBytes(byteSalt);
            string salt = Convert.ToBase64String(byteSalt);
            return salt;
        }
    }
}