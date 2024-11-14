using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RecipeCubeWebService.DTO;
using RecipeCubeWebService.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RecipeCubeWebService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OAuthController : ControllerBase
    {
        private readonly RecipeCubeContext _context;

        public OAuthController(RecipeCubeContext context)
        {
            _context = context;
        }

        //  /api/OAuth/VerifyAudience
        // 判斷 aud是否和OAuth 2.0 用戶端 ID相同且exp未到期
        [HttpPost("VerifyAudience")]
        public async Task<IActionResult> VerifyAudience(VerifyAudDTO verifyAud)
        {
            if (string.IsNullOrEmpty(verifyAud.oAuthGoogleJwt))
            {
                return BadRequest(new { Message = "輸入為空" });
            }
            // 創建 JwtSecurityTokenHandler 來解析 JWT
            var handler = new JwtSecurityTokenHandler();
            // 驗證 Token 是否為有效的 JWT 格式
            if (!handler.CanReadToken(verifyAud.oAuthGoogleJwt))
            {
                return BadRequest(new { Message = "無效的 JWT 格式" });
            }
            // 解析 JWT
            var jwtToken = handler.ReadJwtToken(verifyAud.oAuthGoogleJwt);
            // OAuth 2.0 用戶端 ID
            // Google OAuth 2.0 用戶端 ID
            var RecipeCubeUserInUp = "925872879369-bn925g9150fm8dlkkip5n3cfd61cb3tb.apps.googleusercontent.com";
            var Channel_ID = "2006541501";
            // 取得 `aud` (Audience) 資訊
            var audience = jwtToken.Audiences.FirstOrDefault();
            Console.WriteLine("++++++++++++++++++++++++");
            Console.WriteLine(audience);
            Console.WriteLine("++++++++++++++++++++++++");
            // 如果沒有 `aud`，返回錯誤 或與 與OAuth 2.0 用戶端 ID 不同
            if (audience == null || audience != RecipeCubeUserInUp)
                if (audience == null)
                {
                    return BadRequest(new { Message = "無效的 JWT 格式或 Audience 不匹配" });
                    if (audience != RecipeCubeUserInUp)
                    {
                        return BadRequest(new { Message = "無效的 JWT 格式或 Google aud不匹配" });
                    }
                    else if (audience != Channel_ID)
                    {
                        return BadRequest(new { Message = "無效的 JWT 格式或 Line aud 不匹配" });
                    }
                }
            // 取得所有 Claims 並轉為字典，方便提取所需值
            var claims = jwtToken.Claims.ToDictionary(c => c.Type, c => c.Value);
            // 提取需要的值
            claims.TryGetValue(JwtRegisteredClaimNames.Exp, out var expClaim);
            claims.TryGetValue(JwtRegisteredClaimNames.Email, out var email);
            claims.TryGetValue(JwtRegisteredClaimNames.Name, out var username);
            // 解析 `exp` 值並檢查過期
            if (expClaim != null && long.TryParse(expClaim, out var exp))
            {
                var expirationDate = DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime;
                if (DateTime.UtcNow > expirationDate)
                {
                    return BadRequest(new { Message = "JWT 已過期" });
                }
            }
            else
            {
                return BadRequest(new { Message = "無法取得或解析 JWT 的過期時間 (exp)" });
            }

            var oAuthUser = await _context.user.SingleOrDefaultAsync(u => u.Email == email);
            // 如果第三方登入使用者不存在，呼叫 oAuthsignUpDto api
            if (oAuthUser == null)
            {
                var oAuthsignUp = new oAuthSignUpDTO { UserName = username, Email = email };
                // 呼叫 oAuthSignUp 方法
                var result = await oAuthSignUp(oAuthsignUp);
                // 根據呼叫的結果返回相應的訊息
                if (result is OkObjectResult okResult)
                {
                    //return Redirect("http://localhost:5173/oAuthFirstSignIn");
                    return Ok(new { Message = "請填寫必填資訊已完成註冊" });
                }
            }
            // 如果第三方登入使用者存在，未填寫必要資訊則導向前端填寫基本資料頁面
            if (oAuthUser.status)
            {
                var oAuthSignIn_Id = oAuthUser.Id;
                var result = await oAuthSignIn(oAuthSignIn_Id);
                if (result is OkObjectResult okResult)
                {
                    return Ok(okResult.Value);
                }
            }
            else
            {   // 防止使用者網路不好造成錯誤
                //return Redirect("http://localhost:5173/oAuthFirstSignIn");
                return Ok(new { Message = "請填寫必填資訊已完成註冊" });
            }

            var user = _context.user.SingleOrDefault(u => u.Email == email);
            return Ok(new
            {
                Message = "JWT 解析成功",
                Claims = claims
            });
        }
        // /api/OAuth/oAuthSignUp
        // 第三方註冊功能，僅給api呼叫使用
        [HttpPost("oAuthSignUp")]
        public async Task<IActionResult> oAuthSignUp(oAuthSignUpDTO oAuthsignUp)
        {
            if (oAuthsignUp == null)
            {
                return BadRequest("輸入為空.");
            }

            var newUser = new user
            {
                Id = Guid.NewGuid().ToString(), // 隨機生成UUID
                UserName = oAuthsignUp.UserName, // 使用者名稱預設為 email
                NormalizedUserName = oAuthsignUp.UserName.ToUpper(), // 正常化名稱
                Email = oAuthsignUp.Email, // 註冊的Email
                NormalizedEmail = oAuthsignUp.Email.ToUpper(), // 正常化Email
                EmailConfirmed = false, // 設為未確認電子郵件
                SecurityStamp = Guid.NewGuid().ToString(), // 隨機生成
                ConcurrencyStamp = Guid.NewGuid().ToString(), // 隨機生成
                PhoneNumber = "", // 註冊時的手機號碼
                PhoneNumberConfirmed = false, // 手機號碼未確認
                TwoFactorEnabled = false, // 預設關閉雙重驗證
                LockoutEnabled = true, // 開啟鎖定功能
                AccessFailedCount = 0, // 登入失敗次數設為0
                dietary_restrictions = false, //預設false
                exclusiveChecked = false, // 預設不可食用食物關閉，驗證後可在使用者設定修改填入
                groupId = 0, // 預設沒有群組
                preferredChecked = false, // 預設偏好食物關閉，驗證後可在使用者設定修改填入
                status = false // 使用者狀態設定為關閉，不可登入
            };

            // 將新使用者保存至資料庫
            _context.user.Add(newUser);
            await _context.SaveChangesAsync();

            // return Redirect("http://localhost:5173/oAuthFirstSignIn");
            return Ok("第三方註冊登入成功");
        }

        // /api/OAuth/oAuthFirstSignIn
        // 第三方註冊完成後初次登入
        [HttpPut("oAuthFirstSignIn")]
        public async Task<IActionResult> oAuthFirstSignIn(oAuthFirstSignInDTO oAuthFirstSignIn)
        {
            // 檢查傳入的資料是否為 null
            if (oAuthFirstSignIn == null)
            {
                return BadRequest("輸入為空"); // 如果資料為 null，返回400錯誤
            }

            // 根據傳入的 User_email 查詢資料庫中的使用者
            var user = await _context.user.SingleOrDefaultAsync(u => u.Email == oAuthFirstSignIn.oAuthEmail);
            if (user == null)
            {
                return NotFound(new { Message = "User not found." }); // 如果找不到使用者，返回404錯誤
            }

            // 更新使用者的屬性
            user.dietary_restrictions = oAuthFirstSignIn.dietaryRestrictions; // 更新使用者葷素狀態
            user.status = true; //將鎖定狀態解除

            _context.Entry(user).State = EntityState.Modified; // 將使用者的狀態設置為已修改

            try
            {
                await _context.SaveChangesAsync(); // 保存變更到資料庫
                var oAuthSignIn_Id = user.Id;
                var result = await oAuthSignIn(oAuthSignIn_Id);
                if (result is OkObjectResult okResult)
                {
                    return Ok(okResult.Value);
                }

            }
            catch (DbUpdateConcurrencyException) // 處理並發更新異常
            {
                if (!UserExists(oAuthFirstSignIn.oAuthEmail)) // 檢查使用者是否仍存在
                {
                    return NotFound(); // 如果使用者不存在，返回404錯誤
                }
                else
                {
                    throw; // 重新拋出異常
                }
            }

            return Ok(new { Message = "填寫成功", });
        }
        // /api/OAuth/oAuthSignIn
        // 第三方登入功能 
        [HttpPost("oAuthSignIn")]
        public async Task<IActionResult> oAuthSignIn(string oAuthSignIn)
        {
            var user = _context.user.SingleOrDefault(u => u.Id == oAuthSignIn);
            // 創建 JWT Token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("thisisaverylongsecretkeyforjwtwhichis256bits!!");

            bool exclusiveChecked = user.exclusiveChecked;
            bool preferredChecked = user.preferredChecked;

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.SerialNumber, user.Id),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.GroupSid, user.groupId.ToString()),
                    new Claim(ClaimTypes.MobilePhone, user.PhoneNumber),
                    new Claim("ExclusiveChecked", exclusiveChecked.ToString()),
                    new Claim("PreferredChecked", preferredChecked.ToString())
                    }),
                Expires = DateTime.UtcNow.AddDays(30),
                SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            return Ok(new
            {
                Token = tokenString,
                Message = "登入成功",
                usernamejwt = user.UserName
            });
            return Ok(new { Message = "登入失敗" });
        }


        private bool UserExists(string id)
        {
            return _context.user.Any(e => e.Id == id);
        }
    }

}
