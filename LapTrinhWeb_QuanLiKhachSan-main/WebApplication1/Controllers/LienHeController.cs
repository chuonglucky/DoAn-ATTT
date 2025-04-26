using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web.Mvc;
using WebApplication1.Models;


namespace WebApplication1.Controllers
{
    public class LienHeController : Controller
    {
        private QL_KhachSanEntities2 db = new QL_KhachSanEntities2();
        // GET: LienHe/GuiLienHe
        [HttpGet]
        public ActionResult GuiLienHe()
        {
            return View(); // Hiển thị form liên hệ
        }

        // POST: LienHe/GuiLienHe
        [HttpPost]
        public ActionResult GuiLienHe(string tieuDe, string noiDung)
        {
            try
            {
                var khachHang = (KHACHHANG)Session["User"];
                if (khachHang == null)
                {
                    return RedirectToAction("DangNhap", "User");
                }

                // Gửi email
                GuiEmailLienHe(khachHang.Email, tieuDe, noiDung);

                // Thông báo thành công
                ViewBag.Message = "Liên hệ của bạn đã được gửi thành công. Chúng tôi sẽ phản hồi sớm nhất.";
                return View(); // Hiển thị lại form với thông báo
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Có lỗi xảy ra khi gửi liên hệ: " + ex.Message;
                return View(); // Hiển thị lại form với thông báo lỗi
            }
        }


        private void GuiEmailLienHe(string emailNguoiDung, string tieuDe, string noiDung)
        {
            try
            {
                // Địa chỉ email gửi
                var fromAddress = new MailAddress("2224802010314@student.tdmu.edu.vn", "Lucky Hotel");

                // Địa chỉ email nhận (bạn có thể đặt email của đội hỗ trợ)
                var toAddress = new MailAddress("support@luckyhotel.com");

                const string fromPassword = "hbpk bhtz zvic aysp"; // Nên lưu mật khẩu bảo mật hơn
                string subject = $"[Liên Hệ] {tieuDe}";

                string body = $"<h3>Khách hàng: {emailNguoiDung}</h3>" +
                              $"<p>Nội dung liên hệ:</p>" +
                              $"<p>{noiDung}</p>" +
                              $"<p>Trân trọng,</p>" +
                              $"<p><em>Lucky Hotel</em></p>";

                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                };

                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                })
                {
                    smtp.Send(message);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Có lỗi xảy ra khi gửi email liên hệ: " + ex.Message, ex);
            }
        }

    }
}