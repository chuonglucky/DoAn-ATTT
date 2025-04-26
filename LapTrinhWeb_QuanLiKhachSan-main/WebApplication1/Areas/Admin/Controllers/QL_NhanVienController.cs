using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Areas.Admin.Controllers
{
    public class QL_NhanVienController : Controller
    {
        QL_KhachSanEntities2 db = new QL_KhachSanEntities2();
        public ActionResult DanhSachNhanVien()
        {
            var danhsachnhanvien = db.NHANVIENs.ToList();
            return View(danhsachnhanvien);
        }
        public ActionResult Chitietnhanvien(string maNV)
        {
            var nhanvien = db.NHANVIENs.FirstOrDefault(s => s.MaNV == maNV);

            if (nhanvien == null)
            {
                return HttpNotFound();
            }

            return View(nhanvien);
        }
        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.maNV = new SelectList(db.NHANVIENs.OrderBy(n => n.HoTen), "MaNV", "HoTen");
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(NHANVIEN nv)
        {
            ViewBag.maNV = new SelectList(db.NHANVIENs.OrderBy(n => n.HoTen), "MaNV", "HoTen");

            
            var existingPhong = db.NHANVIENs.FirstOrDefault(p => p.MaNV == nv.MaNV);
            if (existingPhong != null)
            {
                ModelState.AddModelError("", "Mã đã tồn tại. Vui lòng chọn mã khác.");
                return View(nv);
            }
            if (ModelState.IsValid)
            {
                try
                {
                    db.NHANVIENs.Add(nv);
                    db.SaveChanges();

                    return RedirectToAction("DanhSachNhanVien");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Đã xảy ra lỗi: {ex.Message}");
                }
            }

            return View(nv);
        }
        [HttpGet]
        public ActionResult Edit(string MaNV)
        {
            var nv = db.NHANVIENs.SingleOrDefault(n => n.MaNV == MaNV);
            if (nv == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            ViewBag.maNV = new SelectList(db.NHANVIENs.OrderBy(n => n.HoTen), "MaNV", "HoTen");
            return View(nv);
        }


        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(NHANVIEN nv)
        {
            ViewBag.maNV = new SelectList(db.NHANVIENs.OrderBy(n => n.HoTen), "MaNV", "HoTen");
            if (!ModelState.IsValid)
            {
                return View(nv);
            }

            try
            {
                var nvDB = db.NHANVIENs.SingleOrDefault(n => n.MaNV == nv.MaNV);
                if (nvDB == null)
                {
                    return HttpNotFound();
                }
                nvDB.HoTen = nv.HoTen;
                nvDB.DiaChi = nv.DiaChi;
                nvDB.Email = nv.Email;
                nvDB.NgaySinh = nv.NgaySinh;
                nvDB.GioiTinh = nv.GioiTinh;
                nvDB.Username = nv.Username;
                nvDB.Password = nv.Password;
                nvDB.ChucVu=nv.ChucVu;

                // Lưu thay đổi vào cơ sở dữ liệu
                db.SaveChanges();

                return RedirectToAction("DanhSachNhanVien");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Đã xảy ra lỗi: {ex.Message}");
                return View(nv);
            }
        }
        // Hiển thị modal xác nhận xóa
        [HttpGet]
        public ActionResult Delete(string maNV)
        {
            var nv = db.NHANVIENs.Find(maNV);
            if (nv == null)
            {
                return HttpNotFound();
            }
            return View(nv);  // Hiển thị thông báo xác nhận xóa
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(string maNV)
        {
            var nv = db.NHANVIENs.Find(maNV);
            if (nv != null)
            {
                db.NHANVIENs.Remove(nv);
                db.SaveChanges();
            }
            return RedirectToAction("DanhSachNhanVien"); // Redirect về danh sách sau khi xóa
        }
    }
}