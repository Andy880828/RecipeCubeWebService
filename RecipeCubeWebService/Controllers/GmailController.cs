using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using MailKit.Net.Smtp;
using MimeKit;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RecipeCubeWebService.DTO;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RecipeCubeWebService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GmailController : ControllerBase
    {
        private const string CredentialPath = ".credentials";
        // 生成OAuth驗證
        /*
        [HttpGet("authorize")]
        public async Task<IActionResult> Authorize()
        {
            string[] scopes = { GmailService.Scope.GmailSend };
            UserCredential credential;

            using (var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(CredentialPath, true));
            }

            // 更新的過期判斷方式
            var expirationTime = credential.Token.IssuedUtc + credential.Token.ExpiresInSeconds.Value.Seconds();
            if (System.DateTime.UtcNow >= expirationTime)
            {
                if (!await credential.RefreshTokenAsync(CancellationToken.None))
                {
                    return BadRequest("Access Token 已經過期，而且執行 Refresh Token 作業失敗！");
                }
            }

            return Ok(new { Message = "Credential file saved.", CredentialPath });
        }
        */

        private async Task<UserCredential> GetCredential()
        {
            using var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read);
            return await GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.FromStream(stream).Secrets,
                new[] { GmailService.Scope.GmailSend },
                "user",
                CancellationToken.None,
                new FileDataStore(CredentialPath, true));
        }

        private static string Base64UrlEncode(string input)
        {
            var inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(inputBytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
        }


        [HttpPost("send")]
        public async Task<IActionResult> SendEmail(SendEmailDTO sendEmail)
        {
            if (sendEmail == null)
            {
                return BadRequest(new { Message = "內容為空" });
            }

            // 準備 Gmail API 憑證
            var credential = await GetCredential();

            // 準備郵件內容
            var toName = sendEmail.toName;
            var toEmail = sendEmail.toEmail;
            var subject = sendEmail.title;
            var body = sendEmail.body;

            var message = new MimeMessage();
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = subject;

            var builder = new BodyBuilder();
            builder.TextBody = body;
            message.Body = builder.ToMessageBody();

            // 轉為 RFC2822 格式並進行 Base64 編碼
            using var mem = new MemoryStream();
            message.WriteTo(mem);
            var rfc2822 = System.Text.Encoding.UTF8.GetString(mem.ToArray());
            var base64EncodedMessage = Base64UrlEncode(rfc2822);

            // 準備 Gmail API 訊息物件
            var msg = new Google.Apis.Gmail.v1.Data.Message
            {
                Raw = base64EncodedMessage
            };

            // 初始化 GmailService
            var service = new GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Gmail Sender",
            });

            // 使用 Gmail API 發送郵件
            try
            {
                service.Users.Messages.Send(msg, "me").Execute();
                return Ok("Email sent successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to send email: {ex.Message}");
            }
        }

    }

}
