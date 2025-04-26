using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.EnterpriseServices;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class DatPhongController : Controller
    {
        private QL_KhachSanEntities2 db = new QL_KhachSanEntities2();

        public ActionResult DatPhong(string MaPH)
        {
            // Lấy thông tin phòng
            var phong = db.PHONGs.FirstOrDefault(p => p.MaPH == MaPH);
            var dichVuList = db.DICHVUs.ToList();
            if (phong == null)
            {
                return HttpNotFound("Không tìm thấy phòng.");
            }

            // Lấy danh sách dịch vụ
            if (dichVuList == null || !dichVuList.Any())
            {
                ViewBag.ErrorMessage = "Không có dịch vụ nào trong hệ thống.";
            }

            ViewBag.Phong = phong;
            ViewBag.DichVuList = dichVuList;

            return View();
        }


        [HttpPost]
        public ActionResult DatPhong(string MaPH, DateTime? NgayDat, DateTime? NgayNhan, DateTime? NgayTra, string MaDV = null, string[] selectedServices = null, string thanhtoan = null)
        {

            var khachHang = (KHACHHANG)Session["User"];
            if (khachHang == null)
            {
                return RedirectToAction("DangNhap", "User");
            }

            if (!NgayNhan.HasValue || !NgayTra.HasValue)
            {
                ModelState.AddModelError("", "Vui lòng nhập ngày nhận và ngày trả.");
                return View();
            }

            if (NgayTra <= NgayNhan)
            {
                ModelState.AddModelError("", "Ngày trả phải sau ngày nhận.");
                return View();
            }

            var isOverlapping = db.DATPHONGs.Any(dp =>
                dp.MaPH == MaPH &&
                ((NgayNhan < dp.NgayTra && NgayTra > dp.NgayNhan)));

            if (isOverlapping)
            {
                ModelState.AddModelError("", "Phòng đã được đặt trong khoảng thời gian này.");
                return View();
            }

            var phong = db.PHONGs.FirstOrDefault(p => p.MaPH == MaPH);
            var dichVuList = db.DICHVUs.ToList();
            if (phong == null)
            {
                return HttpNotFound("Không tìm thấy phòng.");
            }
            // Tính toán giá trị tổng
            decimal giaDichVu = selectedServices != null
                ? (decimal)(db.DICHVUs.Where(dv => selectedServices.Contains(dv.MaDV)).Sum(dv => dv.Gia) ?? 0)
                : 0;

            int soNgayO = (NgayTra.Value - NgayNhan.Value).Days;
       
            decimal giaDichVuDecimal = (decimal)giaDichVu; // Convert giaDichVu to decimal
            decimal soNgayODecimal = (decimal)soNgayO; // Convert soNgayO to decimal

            decimal donGiaPhong = (((decimal)(phong.Gia ?? 0) * soNgayODecimal + giaDichVuDecimal)) - (((decimal)(phong.Gia ?? 0) * soNgayODecimal + giaDichVuDecimal)) * 0.2M;

            string MaKH = khachHang.MaKH;
            string MaDP = "DP" + new Random().Next(1000, 9999);

            TempData["DatPhongInfo"] = new DatPhongInfo
            {
                MaDP = "DP" + new Random().Next(1000, 9999), // Tạo mã đặt phòng
                MaKH = khachHang.MaKH,                      // Mã khách hàng từ Session
                MaPH = MaPH,                                // Mã phòng từ tham số
                NgayDat = DateTime.Now,                     // Ngày đặt hiện tại
                NgayNhan = NgayNhan.Value,                  // Ngày nhận từ người dùng
                NgayTra = NgayTra.Value,                    // Ngày trả từ người dùng
                DonGia = donGiaPhong,
                DatCoc = (((decimal)(phong.Gia ?? 0) * soNgayODecimal + giaDichVuDecimal)) * 0.2M,
                SelectedServices = selectedServices?.ToList() // Chuyển mảng dịch vụ thành danh sách
                
            };

            // Điều hướng đến phương thức thanh toán
            switch (thanhtoan)
            {
                case "vivnpay":
                    return RedirectToAction("PaymentVNPay", "DatPhong", new { MaDP });
                case "vimomo":
                    return RedirectToAction("CreatePayment", "DatPhong", new { MaDP });
                default:
                    break;
            }

            return View();
        }

        public ActionResult DatPhongConfirmation(string MaDP)
        {
            var datPhong = db.DATPHONGs.FirstOrDefault(dp => dp.MaDP == MaDP);
            if (datPhong == null)
            {
                return HttpNotFound("Không tìm thấy thông tin đặt phòng.");
            }

            return View(datPhong);
        }

        public ActionResult PaymentVNPay(string MaDP, string NgayDat)
        {
            string url = ConfigurationManager.AppSettings["Url"];
            string returnUrl = ConfigurationManager.AppSettings["ReturnUrl"];
            string tmnCode = ConfigurationManager.AppSettings["TmnCode"];
            string hashSecret = ConfigurationManager.AppSettings["HashSecret"];
            PayLib pay = new PayLib();

            pay.AddRequestData("vnp_Version", "2.1.0");
            pay.AddRequestData("vnp_Command", "pay");
            pay.AddRequestData("vnp_TmnCode", tmnCode);
            pay.AddRequestData("vnp_Amount", ((Int32)(TempData["DatPhongInfo"] as DatPhongInfo).DatCoc * 100).ToString());
            //pay.AddRequestData("vnp_Amount", (200000 * 100).ToString());
            pay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_CurrCode", "VND");
            pay.AddRequestData("vnp_IpAddr", PayUtil.GetIpAddress());
            pay.AddRequestData("vnp_Locale", "vn");
            pay.AddRequestData("vnp_OrderInfo", "Thanh toán đặt phòng");
            pay.AddRequestData("vnp_OrderType", "other");
            pay.AddRequestData("vnp_ReturnUrl", returnUrl);
            pay.AddRequestData("vnp_TxnRef", MaDP);

            string paymentUrl = pay.CreateRequestUrl(url, hashSecret);
            return Redirect(paymentUrl);
        }

        public ActionResult PaymentConfirm()
        {
            if (Request.QueryString.Count > 0)
            {
                string hashSecret = ConfigurationManager.AppSettings["HashSecret"];
                var vnpayData = Request.QueryString;
                PayLib pay = new PayLib();

                foreach (string s in vnpayData)
                {
                    if (!string.IsNullOrEmpty(s) && s.StartsWith("vnp_"))
                    {
                        pay.AddResponseData(s, vnpayData[s]);
                    }
                }

                string vnp_ResponseCode = pay.GetResponseData("vnp_ResponseCode");
                string vnp_SecureHash = Request.QueryString["vnp_SecureHash"];
                bool checkSignature = pay.ValidateSignature(vnp_SecureHash, hashSecret);

                if (checkSignature && vnp_ResponseCode == "00")
                {
                    // Thanh toán thành công -> Lưu thông tin đặt phòng
                    var datPhongInfo = TempData["DatPhongInfo"] as DatPhongInfo;
                    if (datPhongInfo != null)
                    {
                        var datPhong = new DATPHONG
                        {
                            MaDP = datPhongInfo.MaDP,
                            MaKH = datPhongInfo.MaKH,
                            MaPH = datPhongInfo.MaPH,
                            NgayDat = datPhongInfo.NgayDat,
                            NgayNhan = datPhongInfo.NgayNhan,
                            NgayTra = datPhongInfo.NgayTra,
                            TinhTrang = "Đã đặt cọc",
                            DonGia = (float)datPhongInfo.DonGia,
                            DatCoc = (float)datPhongInfo.DatCoc
                        };

                        db.DATPHONGs.Add(datPhong);
                        db.SaveChanges();

                        // Lưu thông tin dịch vụ nếu có
                        if (datPhongInfo.SelectedServices != null)
                        {
                            foreach (var maDV in datPhongInfo.SelectedServices)
                            {
                                var service = db.DICHVUs.FirstOrDefault(dv => dv.MaDV == maDV);
                                if (service != null)
                                {
                                    var datDichVu = new DATDICHVU
                                    {
                                        MaDV = maDV,
                                        MaDP = datPhongInfo.MaDP,
                                        NgayDat = DateTime.Now,
                                        NgayNhan = datPhongInfo.NgayNhan,
                                        NgayTra = datPhongInfo.NgayTra
                                    };
                                    db.DATDICHVUs.Add(datDichVu);
                                }
                                else
                                {
                                    var datDichVu = new DATDICHVU
                                    {
                                        MaDV = null,
                                        MaDP = datPhongInfo.MaDP,
                                        NgayDat = DateTime.Now,
                                        NgayNhan = datPhongInfo.NgayNhan,
                                        NgayTra = datPhongInfo.NgayTra
                                    };
                                    db.DATDICHVUs.Add(datDichVu);
                                }
                            }
                            db.SaveChanges();
                        }
                    }

                    ViewBag.Message = "Thanh toán thành công và thông tin đặt phòng đã được lưu.";
                    return RedirectToAction("DatPhongConfirmation", new { MaDP = datPhongInfo.MaDP });
                }
                else
                {
                    ViewBag.Message = "Thanh toán không thành công. Vui lòng thử lại.";
                }
            }
            return View();
        }
        public async Task<ActionResult> CreatePayment()
        {
            // Lấy thông tin số tiền từ session hoặc sử dụng giá trị cố định
            decimal Amount = (TempData["DatPhongInfo"] as DatPhongInfo).DatCoc;// Tiền thanh toán là 200000 VND
            double? amountInCents = (double?)Amount; // where Amount is a decimal

            string requestId = MomoConfig.PartnerCode + DateTime.Now.Ticks;
            string orderId = requestId;
            string amount = amountInCents.ToString(); // Số tiền thanh toán tính theo đơn vị cents
            string orderInfo = "Thanh toán thử nghiệm MoMo";
            string extraData = ""; // Không có dữ liệu thêm
            string requestType = "payWithMethod"; // Loại thanh toán

            // Tạo chuỗi raw signature
            string rawSignature = $"accessKey={MomoConfig.AccessKey}&amount={amount}&extraData={extraData}&ipnUrl={MomoConfig.NotifyUrl}&orderId={orderId}&orderInfo={orderInfo}&partnerCode={MomoConfig.PartnerCode}&redirectUrl={MomoConfig.ReturnUrl}&requestId={requestId}&requestType={requestType}";
            string signature = GenerateSignature(rawSignature, MomoConfig.SecretKey);

            // Tạo dữ liệu gửi đi
            var jsonData = new
            {
                partnerCode = MomoConfig.PartnerCode,
                accessKey = MomoConfig.AccessKey,
                requestId = requestId,
                amount = amount,
                orderId = orderId,
                orderInfo = orderInfo,
                redirectUrl = MomoConfig.ReturnUrl,
                ipnUrl = MomoConfig.NotifyUrl,
                extraData = extraData,
                requestType = requestType,
                signature = signature,
                lang = "en"
            };

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Gửi request POST đến MoMo
                    var content = new StringContent(JsonConvert.SerializeObject(jsonData), Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(MomoConfig.Endpoint, content);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    // Log phản hồi để kiểm tra
                    Console.WriteLine("Phản hồi từ MoMo: " + responseContent);

                    // Phân tích phản hồi JSON thành object
                    var momoResponse = JsonConvert.DeserializeObject<MomoResponse>(responseContent);

                    if (momoResponse != null)
                    {
                        if (momoResponse.errorCode == 0)
                        {
                            // Thành công, chuyển hướng đến URL thanh toán
                            if (!string.IsNullOrEmpty(momoResponse.payUrl))
                            {
                                return Redirect(momoResponse.payUrl);
                            }
                            else
                            {
                                ViewBag.ErrorMessage = "Không tìm thấy payUrl trong phản hồi từ MoMo.";
                                return View("Error");
                            }
                        }
                        else
                        {
                            // Hiển thị lỗi từ MoMo
                            ViewBag.ErrorMessage = $"Lỗi từ MoMo: Mã lỗi {momoResponse.errorCode}, Thông báo: {momoResponse.message}";
                            return View("Error");
                        }
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Không nhận được phản hồi từ API MoMo.";
                        return View("Error");
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Lỗi: {ex.Message}";
                return View("Error");
            }
        }



        // Phương thức tạo chữ ký HMAC SHA256
        private string GenerateSignature(string data, string key)
        {
            using (HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
            {
                byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        public ActionResult ReturnUrl()
        {
            string resultCode = Request.QueryString["resultCode"];
            string orderId = Request.QueryString["orderId"];
            string message = Request.QueryString["message"];

            if (resultCode == "0")
            {
                // Thanh toán thành công, lưu thông tin đặt phòng vào CSDL
                var datPhongInfo = TempData["DatPhongInfo"] as DatPhongInfo;
                if (datPhongInfo != null)
                {
                    var datPhong = new DATPHONG
                    {
                        MaDP = datPhongInfo.MaDP,
                        MaKH = datPhongInfo.MaKH,
                        MaPH = datPhongInfo.MaPH,
                        NgayDat = datPhongInfo.NgayDat,
                        NgayNhan = datPhongInfo.NgayNhan,
                        NgayTra = datPhongInfo.NgayTra,
                        TinhTrang = "Đã đặt cọc",
                        DonGia = (float)datPhongInfo.DonGia,
                        DatCoc = (float)datPhongInfo.DatCoc
                    };

                    db.DATPHONGs.Add(datPhong);
                    db.SaveChanges();

                    // Lưu thông tin dịch vụ nếu có
                    if (datPhongInfo.SelectedServices != null)
                    {
                        foreach (var maDV in datPhongInfo.SelectedServices)
                        {
                            var datDichVu = new DATDICHVU
                            {
                                MaDV = maDV,
                                MaDP = datPhongInfo.MaDP,
                                NgayDat = DateTime.Now,
                                NgayNhan = datPhongInfo.NgayNhan,
                                NgayTra = datPhongInfo.NgayTra
                            };
                            db.DATDICHVUs.Add(datDichVu);
                        }
                        db.SaveChanges();
                    }
                }

                ViewBag.Message = "Thanh toán thành công! Thông tin đặt phòng đã được lưu.";
                return RedirectToAction("DatPhongConfirmation", new { MaDP = datPhongInfo.MaDP });
            }
            else
            {
                ViewBag.Message = $"Thanh toán thất bại: {message}";
            }

            return View();
        }
        public ActionResult NotifyUrl()
        {
            try
            {
                var requestParams = Request.Form;
                string signature = requestParams["signature"];

                // Tạo chuỗi raw signature để kiểm tra
                string rawSignature = $"accessKey={MomoConfig.AccessKey}&amount={requestParams["amount"]}&extraData={requestParams["extraData"]}&message={requestParams["message"]}&orderId={requestParams["orderId"]}&orderInfo={requestParams["orderInfo"]}&partnerCode={requestParams["partnerCode"]}&requestId={requestParams["requestId"]}&responseTime={requestParams["responseTime"]}&resultCode={requestParams["resultCode"]}";

                string generatedSignature = GenerateSignature(rawSignature, MomoConfig.SecretKey);

                if (signature == generatedSignature)
                {
                    if (requestParams["resultCode"] == "0")
                    {
                        // Xử lý thanh toán thành công
                        Console.WriteLine("Giao dịch thành công: " + requestParams["orderId"]);
                        return new HttpStatusCodeResult(200);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi NotifyUrl: " + ex.Message);
            }

            return new HttpStatusCodeResult(400); // Báo lỗi nếu không xử lý được
        }


    }
}