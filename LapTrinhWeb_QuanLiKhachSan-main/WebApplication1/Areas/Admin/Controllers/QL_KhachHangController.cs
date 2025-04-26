using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Areas.Admin.Controllers
{
    public class QL_KhachHangController : Controller
    {
        // GET: Admin/QL_KhachHang
        QL_KhachSanEntities2 db = new QL_KhachSanEntities2();
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult DanhSachKhachHang()
        {
            var kh = db.KHACHHANGs.ToList();
            var user = db.NHANVIENs.SingleOrDefault(u => u.Username == User.Identity.Name);
            ViewBag.nhanvien = user?.ChucVu; // Truyền chức vụ của người dùng hiện tại
            return View(kh);
        }
        public ActionResult Chitietkhachhang(string MaKH)
        {
            var kh = db.KHACHHANGs.FirstOrDefault(k => k.MaKH == MaKH);

            if (kh == null)
            {
                return HttpNotFound();
            }

            return View(kh);
        }
        [HttpGet]
        public ActionResult Edit(string MaKH)
        {
            var kh = db.KHACHHANGs.SingleOrDefault(k => k.MaKH == MaKH);
            if (kh == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            ViewBag.maKH = new SelectList(db.KHACHHANGs.OrderBy(n => n.HoTen), "MaKH", "HoTen");
            return View(kh);
        }


        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(KHACHHANG kh)
        {
            ViewBag.maKH = new SelectList(db.KHACHHANGs.OrderBy(n => n.HoTen), "MaKH", "HoTen");
            if (!ModelState.IsValid)
            {
                return View(kh);
            }

            try
            {
                var khDB = db.KHACHHANGs.SingleOrDefault(n => n.MaKH == kh.MaKH);
                if (khDB == null)
                {
                    return HttpNotFound();
                }

                LuuLichSuThayDoi(khDB, kh, "HoTen", khDB.HoTen, kh.HoTen);
                LuuLichSuThayDoi(khDB, kh, "DiaChi", khDB.DiaChi, kh.DiaChi);
                LuuLichSuThayDoi(khDB, kh, "DienThoai", khDB.DienThoai, kh.DienThoai);
                LuuLichSuThayDoi(khDB, kh, "GioiTinh", khDB.GioiTinh, kh.GioiTinh);
                LuuLichSuThayDoi(khDB, kh, "CCCD", khDB.CCCD, kh.CCCD);
                LuuLichSuThayDoi(khDB, kh, "NgaySinh", khDB.NgaySinh?.ToString(), kh.NgaySinh?.ToString());
                LuuLichSuThayDoi(khDB, kh, "QuocTich", khDB.QuocTich, kh.QuocTich);
                LuuLichSuThayDoi(khDB, kh, "Email", khDB.Email, kh.Email);
                LuuLichSuThayDoi(khDB, kh, "Username", khDB.Username, kh.Username);

                khDB.HoTen = kh.HoTen;
                khDB.DiaChi = kh.DiaChi;
                khDB.DienThoai = kh.DienThoai;
                khDB.GioiTinh = kh.GioiTinh;
                khDB.CCCD=kh.CCCD;
                khDB.NgaySinh = kh.NgaySinh;
                khDB.QuocTich = kh.QuocTich;
                khDB.Email = kh.Email;
                khDB.Username = kh.Username;
                khDB.Password = kh.Password;

                // Lưu thay đổi vào cơ sở dữ liệu
                db.SaveChanges();

                return RedirectToAction("DanhSachKhachHang");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Đã xảy ra lỗi: {ex.Message}");
                return View(kh);
            }
        }
        private void LuuLichSuThayDoi(KHACHHANG khDB, KHACHHANG kh, string truong, string giaTriCu, string giaTriMoi)
        {
            if (giaTriCu != giaTriMoi) // Chỉ lưu khi có sự thay đổi
            {
                var lichSu = new LichSuThayDoi
                {
                    MaKH = kh.MaKH,
                    TruongThayDoi = truong,
                    GiaTriCu = giaTriCu,
                    GiaTriMoi = giaTriMoi,
                    NgayThayDoi = DateTime.Now,
                    NguoiThayDoi = Session["NhanVien"]?.ToString() // Lấy tên người dùng hiện tại
                };
                db.LichSuThayDois.Add(lichSu);
                db.SaveChanges();
            }
        }

        public ActionResult LichSuThayDoi(string MaKH)
        {
            // Kiểm tra quyền
            if (Session["UserChucVu"] == null || Session["UserChucVu"].ToString() != "Quan ly")
            {
                return new HttpStatusCodeResult(403, "Bạn không có quyền truy cập vào lịch sử thay đổi.");
            }

            var lichSu = db.LichSuThayDois
                           .Where(ls => ls.MaKH == MaKH)
                           .OrderByDescending(ls => ls.NgayThayDoi)
                           .ToList();

            return View(lichSu);
        }


        [HttpGet]
        public ActionResult Delete(string MaKH)
        {
            var kh = db.KHACHHANGs.Find(MaKH);
            if (kh == null)
            {
                return HttpNotFound();
            }
            return View(kh);  // Hiển thị thông báo xác nhận xóa
        }

        // Xóa Loại Phòng
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(string MaKH)
        {
            var kh = db.KHACHHANGs.Find(MaKH);
            if (kh != null)
            {
                db.KHACHHANGs.Remove(kh);
                db.SaveChanges();
            }
            return RedirectToAction("DanhSachKhachHang"); // Redirect về danh sách sau khi xóa
        }
    }
}