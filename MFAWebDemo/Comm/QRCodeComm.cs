using QRCoder;
using System.Drawing;

namespace Operation.Comm
{
    public static class QRCodeComm
    {
        /// <summary>
        /// 生成二维码
        /// </summary>
        /// <param name="Content">内容</param>
        /// <param name="pixels">大小</param>
        /// <param name="isQRFrame">二维码边框</param>
        /// <returns></returns>
        public static Bitmap GenerateQRCode(string content, int pixels = 20, bool isQRFrame = true)
        {
            try
            {
                var qrGenerator = new QRCodeGenerator();
                var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.H);
                var qrCode = new QRCode(qrCodeData);
                var qrCodeImage = qrCode.GetGraphic(pixels, Color.Black, Color.White, isQRFrame);
                return qrCodeImage;
            }
            catch
            {
                return new Bitmap(100, 100);
            }
        }
    }
}
