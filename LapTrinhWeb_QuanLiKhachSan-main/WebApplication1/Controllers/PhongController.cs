using Newtonsoft.Json;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            }

            return RedirectToAction("ChiTietPhong", new { id = MaPH});
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