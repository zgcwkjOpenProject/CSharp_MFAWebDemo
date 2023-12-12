using Microsoft.AspNetCore.Mvc;
using Operation.Comm;
using OtpNet;
using System.Drawing.Imaging;

namespace Operation.Controllers
{
    /// <summary>
    /// 二步验证
    /// </summary>
    public class MfaController : Controller
    {
        private IWebHostEnvironment webHost { get; }

        public MfaController(IWebHostEnvironment webHost)
        {
            this.webHost = webHost;
        }

        public IActionResult Index(string userId)
        {
            ViewBag.UserId = userId ?? "test";
            //
            return View();
        }

        /// <summary>
        /// 生成 MFA
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Generate(string userId)
        {
            if (userId == default) return BadRequest("Error");
            // 生成共享密钥
            var secretStr = GetUserMfa(userId);
            // 获取用户的MFA密钥
            var mfaKey = string.Format("otpauth://totp/{0}?secret={1}&issuer={2}", "MyAppMFA", secretStr, "zgcwkj");
            //生成二维码
            var img = QRCodeComm.GenerateQRCode(mfaKey);
            using var ms = new MemoryStream();
            img.Save(ms, ImageFormat.Jpeg);
            var bytes = ms.GetBuffer();
            return File(bytes, "image/jpg");
        }

        /// <summary>
        /// 验证 MFA
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="code">实时码</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Verify(string userId, string code)
        {
            if (userId == default) return BadRequest("Error");
            // 生成共享密钥
            var secretStr = GetUserMfa(userId);
            // 创建 TOTP 对象
            var base32Secret = Base32Encoding.ToBytes(secretStr);
            var totp = new Totp(base32Secret);
            if (totp.VerifyTotp(code, out _))
            {
                return Ok();
            }
            else
            {
                return BadRequest("Invalid verification code.");
            }
        }

        /// <summary>
        /// 获取 MFA 验证码
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="code">实时码</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult GetCode(string userId)
        {
            if (userId == default) return BadRequest("Error");
            // 生成共享密钥
            var secretStr = GetUserMfa(userId);
            // 创建 TOTP 对象
            var base32Secret = Base32Encoding.ToBytes(secretStr);
            var totp = new Totp(base32Secret);
            // 生成当前时间的验证码
            var currentCode = totp.ComputeTotp();
            //
            //Console.WriteLine("验证码: " + currentCode);
            return BadRequest("验证码: " + currentCode);
        }

        /// <summary>
        /// 获取用户 MFA 密钥
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns></returns>
        private string GetUserMfa(string userId)
        {
            var mfaPath = this.webHost.ContentRootPath;
            //创建文件夹
            var mfaUserPath = $"{mfaPath}/MFA";
            if (!Directory.Exists(mfaUserPath))
            {
                Directory.CreateDirectory(mfaUserPath);
            }
            //生成共享密钥
            var mfaUserFilePath = $"{mfaUserPath}/{userId}.txt";
            if (!System.IO.File.Exists(mfaUserFilePath))
            {
                var base32SecretTemp = KeyGeneration.GenerateRandomKey(20);
                var secretStrTemp = Base32Encoding.ToString(base32SecretTemp);
                System.IO.File.WriteAllText(mfaUserFilePath, secretStrTemp);
            }
            var secretStr = System.IO.File.ReadAllText(mfaUserFilePath);
            return secretStr;
        }
    }
}
