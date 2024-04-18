using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CalendarApi.Models;
using MongoDB.Driver.Linq;
using System.Net;
using CalendarApi.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Cryptography;

namespace CalendarApi.Controllers
{
    [ApiController]
    [Route("api/v1/autheticate")]
    public class AutenticationController : ControllerBase
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public AutenticationController(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpPost]
        [Route("roles/add")]
        public async Task<IActionResult> CreateRole([FromBody] CreatedRoleRequest request)
        {
            var appRole = new ApplicationRole { Name = request.Role };
            var createRole = await _roleManager.CreateAsync(appRole);

            return Ok(new { message = "role created succesfully" });
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await RegisterAsync(request);
            return result.Success ? Ok(result) : BadRequest(result.Message);
        }

        private async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            try
            {
                var userExists = await _userManager.FindByEmailAsync(request.Email);
                if (userExists != null) return new RegisterResponse { Message = "User already exists", Success = false };

                //if we get here, no user with this email

                userExists = new ApplicationUser
                {
                    FullName = request.FullName,
                    Email = request.Email,
                    ConcurrencyStamp = Guid.NewGuid().ToString(),
                    UserName = request.Email,
                };
                var createUserResult = await _userManager.CreateAsync(userExists, request.Password);
                if (!createUserResult.Succeeded) return new RegisterResponse { Message = "Create user failed " +
                    $"{createUserResult?.Errors?.First()?.Description}", Success = false };
                //user is created
                //then add user to a role
                var addUserToRoleResult = await _userManager.AddToRoleAsync(userExists, "ADMIN");
                if(!addUserToRoleResult.Succeeded) return new RegisterResponse { Message = "Create user succeeded but could not add user to role " +
                    $"{addUserToRoleResult?.Errors?.First()?.Description}", Success = false };
                //all is still well...
                return new RegisterResponse
                {
                    Success = true,
                    Message = "User registered successfully"
                };
            }
            catch (Exception ex)
            {
                return new RegisterResponse { Message = ex.Message, Success = false };
            }
        }
        private async Task<string> GenerateAccessTokenAsync(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            var key = GenerateRandomKey(256);
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddMinutes(30);

            var token = new JwtSecurityToken(
                issuer: "https://localhost:5001",
                audience: "https://localhost:5001",
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
            await SaveAccessTokenAsync(user.FullName, accessToken); 
            return accessToken;
        }

        [HttpPost]
        [Route("login")]
        [ProducesResponseType((int) HttpStatusCode.OK, Type = typeof(LoginResponse))]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await LoginAsync(request);
            return result.Success ? Ok(result) : BadRequest(result.Message);
        }

        private async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                
                if (user == null)
                {
                    return new LoginResponse { Message = "Invalid email/password", Success = false };
                }

                bool passwordIsValid = await _userManager.CheckPasswordAsync(user, request.Password);
                if (!passwordIsValid)
                {
                    return new LoginResponse { Message = "Invalid email/password", Success = false };
                }

                // Obtener roles del usuario
                var roles = await _userManager.GetRolesAsync(user);

                // Comprobar si el usuario tiene el rol de administrador
                bool isAdmin = roles.Contains("ADMIN");

                // Generar token de acceso si el login es exitoso
                if (passwordIsValid && isAdmin)
                {
                    var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                };
                    var roleClaims = roles.Select(x => new Claim(ClaimTypes.Role, x));
                    claims.AddRange(roleClaims);

                    var accessToken = await GenerateAccessTokenAsync(user);
                    var logintoken = "Success";



                    var response = new LoginResponse
                    {
                        Access = accessToken,
                        Message = "Login Successful",
                        Success = true,
                    };

                    return response;
                }
                else
                {
                    return new LoginResponse { Message = "Unauthorized", Success = false };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new LoginResponse { Success = false, Message = ex.Message };
            }
        }

        private Dictionary<string, string> _accessTokenMap = new Dictionary<string, string>();

        private async Task SaveAccessTokenAsync(string userId, string accessToken)
        {
            _accessTokenMap[userId] = accessToken;
        }

        private SymmetricSecurityKey GenerateRandomKey(int keySize)
        {
            var randomNumberGenerator = new RNGCryptoServiceProvider();
            var randomNumber = new byte[keySize / 8];
            randomNumberGenerator.GetBytes(randomNumber);
            return new SymmetricSecurityKey(randomNumber);
        }
    }
}

