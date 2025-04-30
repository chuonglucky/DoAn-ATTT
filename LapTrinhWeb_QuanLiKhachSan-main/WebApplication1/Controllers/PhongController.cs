using Newtonsoft.Json;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class PhongController : Controller
    {
        // GET: Phong
        QL_KhachSanEntities2 db = new QL_KhachSanEntities2();

        private List<PHONG> LayPhong(int count)
        {
            return db.PHONGs.OrderBy(a => a.SoPH).Take(count).ToList();
        }
        public ActionResult DanhSachPhong(int? page, string selectedLoaiPhong)
        {
            // Lấy danh sách loại phòng từ bảng LOAIPHONG để gửi vào ViewBag cho filter
            var loaiPhongList = db.LOAIPHONGs.ToList();
            ViewBag.LoaiPhongList = new SelectList(loaiPhongList, "MaLoai", "TenLoai"); // Hiển thị tên loại phòng

            // Lấy danh sách phòng và lọc theo loại phòng nếu có
            var listPhongQuery = db.PHONGs.AsQueryable();

            // Nếu có chọn loại phòng thì lọc theo loại phòng
            if (!string.IsNullOrEmpty(selectedLoaiPhong))
            {
                listPhongQuery = listPhongQuery.Where(p => p.MaLoai == selectedLoaiPhong);
            }

            // Lấy các phòng với phân trang
            int iSize = 9;
            int iPageNumber = (page ?? 1);
            var listPhong = listPhongQuery.ToList();

            // Truyền danh sách phòng đã lọc và phân trang vào View
            return View(listPhong.ToPagedList(iPageNumber, iSize));
        }

        public ActionResult ChiTietPhong(string id)
        {
            var phong = db.PHONGs.FirstOrDefault(s => s.MaPH == id);
            if (phong == null)
            {
                return HttpNotFound(); // Nếu không tìm thấy sách, trả về lỗi 404
            }

            // Lấy danh sách bình luận cùng tên khách hàng
            var binhLuans = (from bl in db.BINHLUANs
                             join kh in db.KHACHHANGs on bl.MaKH equals kh.MaKH
                             where bl.MaPH == id
                             select new CMMD
                             {
                                 NoiDung = bl.NDBL,
                                 DanhGia = (int)bl.DanhGia,
                                 ThoiGian = (DateTime)bl.ThoiGian,
                                 HoTenKhachHang = kh.HoTen
                             }).ToList();
            var avgRating = db.BINHLUANs
            .Where(r => r.MaPH == id)
            .Select(rate => rate.DanhGia)
            .DefaultIfEmpty(0)
            .Average();
            ViewBag.Avg = avgRating;
            ViewBag.BinhLuans = binhLuans;
            var dichVuList = db.DICHVUs.ToList();
            ViewBag.DichVuList = dichVuList;

            return View(phong);

        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult ThemBinhLuan(string MaPH, string NoiDung, int DanhGia)
        {
            // Kiểm tra xem người dùng đã đăng nhập chưa
            var khachHang = (KHACHHANG)Session["User"];

            if (khachHang == null)
            {
                // Nếu chưa đăng nhập, lưu URL hiện tại vào Session và điều hướng đến trang đăng nhập
                Session["ReturnUrl"] = Request.Url.ToString();
                return RedirectToAction("DangNhap", "User");
            }

            // Kiểm tra nội dung bình luận
            if (string.IsNullOrEmpty(NoiDung?.Trim()))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập nội dung bình luận.";
                return RedirectToAction("ChiTietPhong", new { id = MaPH });
            }

            // YÊU CẦU 1: Kiểm tra giới hạn 100 từ
            string[] words = NoiDung.Trim().Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length > 100)
            {
                TempData["ErrorMessage"] = $"Bình luận không được vượt quá 100 từ. Bạn đã nhập {words.Length} từ.";
                return RedirectToAction("ChiTietPhong", new { id = MaPH });
            }

            // YÊU CẦU 2: Kiểm tra bình luận trùng lặp
            var existingComment = db.BINHLUANs
                .Where(b => b.MaKH == khachHang.MaKH && b.MaPH == MaPH && b.NDBL == NoiDung)
                .FirstOrDefault();

            if (existingComment != null)
            {
                TempData["ErrorMessage"] = "Bạn đã gửi một bình luận với nội dung tương tự. Vui lòng nhập nội dung khác.";
                return RedirectToAction("ChiTietPhong", new { id = MaPH });
            }

            // YÊU CẦU 3: Kiểm tra nội dung độc hại
            if (ContainsHarmfulContent(NoiDung))
            {
                TempData["ErrorMessage"] = "Bình luận chứa nội dung không được phép.";
                return RedirectToAction("ChiTietPhong", new { id = MaPH });
            }

            // Thêm bình luận vào CSDL
            if (ModelState.IsValid)
            {
                BINHLUAN binhLuanMoi = new BINHLUAN
                {
                    MaPH = MaPH,
                    MaKH = khachHang.MaKH,
                    NDBL = NoiDung,
                    DanhGia = DanhGia,
                    ThoiGian = DateTime.Now
                };

                db.BINHLUANs.Add(binhLuanMoi);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Bình luận của bạn đã được gửi thành công!";
            }

            return RedirectToAction("ChiTietPhong", new { id = MaPH });
        }
        private bool ContainsHarmfulContent(string content)
        {
            if (string.IsNullOrEmpty(content))
                return false;

            // Danh sách các mẫu regex để phát hiện nội dung độc hại
            string[] harmfulPatterns = new string[]
            {
         @"<script[^>]*>",
         @"javascript\s*:",
         @"on\w+\s*=",  // onclick, onload, onerror, etc.
         @"eval\s*\(",
         @"document\.cookie",
         @"<iframe",
         @"alert\s*\(",
         @"document\.location",
         @"window\.location",
         @"\.submit\s*\(",
         // Thêm mẫu để phát hiện API key
         @"\b(sk_live_|api_|key_)[a-zA-Z0-9_-]{20,}\b"
            };

            foreach (var pattern in harmfulPatterns)
            {
                if (Regex.IsMatch(content, pattern, RegexOptions.IgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
        [HttpPost]
        public ActionResult CheckAvailability(string MaPH, string CheckIn, string CheckOut)
        {
            DateTime checkInDate, checkOutDate;

            // Chuyển đổi các ngày từ chuỗi thành DateTime
            if (!DateTime.TryParse(CheckIn, out checkInDate) || !DateTime.TryParse(CheckOut, out checkOutDate))
            {
                return Json(new { available = false, message = "Ngày không hợp lệ. Vui lòng chọn lại." });
            }

            if (checkInDate == null || checkOutDate == null)
            {
                return Json(new { available = false, message = "Vui lòng chọn ngày check-in và check-out." });
            }

            var phong = db.PHONGs.FirstOrDefault(s => s.MaPH == MaPH);
            if (phong == null)
            {
                return Json(new { available = false, message = "Phòng không tồn tại." });
            }

            // Kiểm tra các đơn đặt phòng có chồng lấp với thời gian check-in và check-out
            var overlappingBookings = db.DATPHONGs
                .Where(d => d.MaPH == MaPH &&
                            // Kiểm tra nếu một trong các điều kiện này thỏa mãn thì có sự trùng lặp
                            (
                                (d.NgayNhan < checkOutDate && d.NgayTra > checkInDate)  // Đặt phòng trùng với khoảng thời gian mới
                            ))
                .ToList();

            if (overlappingBookings.Any())
            {
                return Json(new { available = false, message = "Phòng đã được đặt trong khoảng thời gian này." });
            }

            // Nếu không có sự trùng lặp, phòng còn trống
            return Json(new { available = true, message = "Phòng có sẵn trong khoảng thời gian này." });
        }

    }
}