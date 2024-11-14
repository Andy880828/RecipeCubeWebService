using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RecipeCubeWebService.Models;
using RecipeCubeWebService.DTO;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RecipeCubeWebService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailVerificationController : ControllerBase
    {
        private readonly RecipeCubeContext _context; // 注入資料庫上下文
        private readonly string _jwtSecret = "thisisaverylongsecretkeyforjwtwhichis256bits!!"; // 替換為您的 JWT 秘鑰
        private readonly HttpClient _httpClient;
        private readonly IPasswordHasher<user> _passwordHasher;
        public EmailVerificationController(RecipeCubeContext context, HttpClient httpClient, IPasswordHasher<user> passwordHasher) // 構造函數
        {
            _context = context;
            _httpClient = httpClient;
            _passwordHasher = passwordHasher;
        }

        // POST: api/EmailVerification/GenerateToken
        // 生成 驗證連結api 方法
        [HttpPost("GenerateToken")]
        public IActionResult GenerateEmailVerificationToken([FromBody] UserVerificationDTO user)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.email))
            {
                return BadRequest("Invalid user information.");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[] {
            new Claim("email", user.email) // 使用字符串 "email"
        }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            var verificationLink = Url.Action("VerifyEmail", "EmailVerification", new { token = tokenString }, Request.Scheme);
            Console.WriteLine($"生成的 Token: {tokenString}");
            return Ok(new { VerificationLink = verificationLink });
        }

        // POST: api/EmailVerification/ResetEmailConfirmed
        //  重發驗證信 = GenerateToken 呼叫email api
        [HttpPost("ResetEmailConfirmed")]
        public async Task<IActionResult> ResetEmailConfirmed([FromBody] UserVerificationDTO user)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.email))
            {
                return BadRequest("Invalid user information.");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[] {
            new Claim("email", user.email) // 使用字符串 "email"
        }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };



            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            var verificationLink = Url.Action("VerifyEmail", "EmailVerification", new { token = tokenString }, Request.Scheme);


            // 呼叫 Email API 發送郵件
            var emailSendEndpoint = "https://localhost:7188/api/Gmail/send";
            var emailSendRequest = new
            {
                toName = "",
                toEmail = user.email,
                title = "重發驗證信件",
                body = $"請點擊以下連結以驗證您的帳號：{verificationLink}"
            };

            var emailResponse = await _httpClient.PostAsJsonAsync(emailSendEndpoint, emailSendRequest);
            if (!emailResponse.IsSuccessStatusCode)
            {
                return StatusCode((int)emailResponse.StatusCode, new { Message = "Failed to send verification email." });
            }
            return Ok(new
            {
                Email = user.email,
                VerificationLink = verificationLink,
                Message = "Verification email has been sent."
            });
        }



        // /api/EmailVerification/VerifyEmail
        // 接收驗證連結，驗證後將修改sql資料表內的EmailConfirmed為true
        [HttpGet("VerifyEmail")]
        public IActionResult VerifyEmail(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return BadRequest("Invalid token.");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret);

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.FromMinutes(5)
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var email = jwtToken.Claims.First(x => x.Type == "email").Value; // 更新為使用字符串 "email"

                // 查找用戶並更新 EmailConfirmed 狀態
                var user = _context.user.SingleOrDefault(u => u.Email == email);
                if (user == null)
                {
                    return NotFound(new { Message = "User not found." });
                }

                user.EmailConfirmed = true; // 更新 EmailConfirmed 狀態
                _context.user.Update(user); // 更新用戶
                _context.SaveChanges(); // 保存更改
                var redirectUrl = "https://localhost:5173/signin";
                return Redirect(redirectUrl);
            }
            catch (SecurityTokenExpiredException)
            {
                return Unauthorized(new { Message = "Token expired" });
            }
            catch (SecurityTokenInvalidSignatureException)
            {
                return Unauthorized(new { Message = "Invalid token signature" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error validating token: {ex.GetType().Name} - {ex.Message}");
                return Unauthorized(new { Message = "Invalid token" });
            }
        }



        // POST: api/EmailVerification/ForgetPassword
        //  發送清空密碼連結
        [HttpPost("ForgetPassword")]
        public async Task<IActionResult> ForgetPassword([FromBody] UserVerificationDTO user)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.email))
            {
                return BadRequest("Invalid user information.");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[] {
            new Claim("email", user.email) // 使用字符串 "email"
        }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            var resetPasswordLink = Url.Action("VerifyPassword", "EmailVerification", new { token = tokenString }, Request.Scheme);


            // 呼叫 Email API 發送郵件
            var emailSendEndpoint = "https://localhost:7188/api/Gmail/send";
            var emailSendRequest = new
            {
                toName = "",
                toEmail = user.email,
                title = "忘記密碼重設信件",
                body = $"請點擊以下連結以重設你的密碼：{resetPasswordLink}"
            };

            var emailResponse = await _httpClient.PostAsJsonAsync(emailSendEndpoint, emailSendRequest);
            if (!emailResponse.IsSuccessStatusCode)
            {
                return StatusCode((int)emailResponse.StatusCode, new { Message = "Failed to send verification email." });
            }
            return Ok(new
            {
                ResetPasswordLink = resetPasswordLink,
                Message = "Password reset link sent to your email."
            });
        }
        // GET: api/EmailVerification/VerifyPassword
        [HttpGet("VerifyPassword")]
        // 接收清空密碼連結 驗證後修改sql內的passwordhash為""，再導向前端重設密碼頁面
        public IActionResult VerifyPassword(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return BadRequest("Invalid token.");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret);

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.FromMinutes(5)
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var email = jwtToken.Claims.First(x => x.Type == "email").Value; // 更新為使用字符串 "email"

                // 查找用戶並更新 EmailConfirmed 狀態
                var user = _context.user.SingleOrDefault(u => u.Email == email);
                if (user == null)
                {
                    return NotFound(new { Message = "User not found." });
                }

                user.PasswordHash = ""; //  清空密碼
                _context.user.Update(user); // 更新用戶
                _context.SaveChanges(); // 保存更改


                // 生成新的短期 token
                var newTokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[] {
                new Claim("email", email)
            }),
                    Expires = DateTime.UtcNow.AddMinutes(1), // 1分鐘過期
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
                var newToken = tokenHandler.CreateToken(newTokenDescriptor);
                var newTokenString = tokenHandler.WriteToken(newToken);

                // 將新 token 包含在跳轉的 URL 中
                var redirectUrl = $"https://localhost:5173/resetpasswordset?token={newTokenString}";
                return Redirect(redirectUrl);
                //return Ok(redirectUrl);

            }
            catch (SecurityTokenExpiredException)
            {
                return Unauthorized(new { Message = "Token expired" });
            }
            catch (SecurityTokenInvalidSignatureException)
            {
                return Unauthorized(new { Message = "Invalid token signature" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error validating token: {ex.GetType().Name} - {ex.Message}");
                return Unauthorized(new { Message = "Invalid token" });
            }
        }

        // POST: api/EmailVerification/ResetPassword
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO resetPasswordRequest)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret);
            try
            {
                tokenHandler.ValidateToken(resetPasswordRequest.token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.FromMinutes(1)
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var email = jwtToken.Claims.First(x => x.Type == "email").Value;
                var user = await _context.user.FirstOrDefaultAsync(u => u.Email == email);

                if (user == null)
                {
                    return NotFound(new { Message = "User not found." });
                }
                user.PasswordHash = _passwordHasher.HashPassword(user, resetPasswordRequest.password);//  修改密碼
                _context.user.Update(user); // 更新用戶
                await _context.SaveChangesAsync(); // 保存更改
                return Ok(new
                {
                    Message = "修改成功"
                });
            }
            catch (SecurityTokenExpiredException)
            {
                // Token 過期時回傳 401 錯誤
                return Unauthorized(new { Message = "Token 過期了" });
            }
            catch (SecurityTokenInvalidSignatureException)
            {
                // 無效的 token 簽名
                return Unauthorized(new { Message = "連結錯誤" });
            }
            catch (Exception)
            {
                // 捕捉其他例外
                return Unauthorized(new { Message = "Invalid token" });
            }
        }
    }

}
