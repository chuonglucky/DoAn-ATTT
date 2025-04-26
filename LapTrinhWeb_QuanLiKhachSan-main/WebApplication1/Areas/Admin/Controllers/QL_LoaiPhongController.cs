using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Areas.Admin.Controllers
{
    public class QL_LoaiPhongController : Controller
    {
        QL_KhachSanEntities2 db = new QL_KhachSanEntities2();

        // GET: Admin/QL_LoaiPhong
        public ActionResult Index()
        {
            return View();
        }

        // Danh sách Loại Phòng
        public ActionResult DanhSachLoaiPhong()
        {
            var danhSachLP = db.LOAIPHONGs.ToList();
            return View(danhSachLP);
        }

        // Tạo mới Loại Phòng
        [HttpPost]
        public ActionResult Create(LOAIPHONG loaiPhong)
        {
            if (ModelState.IsValid)
            {
                db.LOAIPHONGs.Add(loaiPhong);
                db.SaveChanges();
                return RedirectToAction("DanhSachLoaiPhong");
            }

            return View(loaiPhong); // Nếu có lỗi, trả về cùng modal hiện tại
        }

        // Hiển thị modal xác nhận xóa
        [HttpGet]
        public ActionResult Delete(string maLoai)
        {
            var loaiPhong = db.LOAIPHONGs.Find(maLoai);
            if (loaiPhong == null)
            {
                return HttpNotFound();
            }
            return View(loaiPhong);  // Hiển thị thông báo xác nhận xóa
        }

        // Xóa Loại Phòng
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(string maLoai)
        {
            var loaiPhong = db.LOAIPHONGs.Find(maLoai);
            if (loaiPhong != null)
            {
                db.LOAIPHONGs.Remove(loaiPhong);
                db.SaveChanges();
            }
            return RedirectToAction("DanhSachLoaiPhong"); // Redirect về danh sách sau khi xóa
        }

        // Hiển thị modal chỉnh sửa
        [HttpGet]
        public ActionResult Edit(string maLoai)
        {
            var loaiPhong = db.LOAIPHONGs.Find(maLoai);
            if (loaiPhong == null)
            {
                return HttpNotFound();
            }
            return View(loaiPhong);
        }

        // Chỉnh sửa Loại Phòng
        [HttpPost]
        public ActionResult Edit(LOAIPHONG loaiPhong)
        {
            if (ModelState.IsValid)
            {
                db.Entry(loaiPhong).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("DanhSachLoaiPhong");
            }
            return View(loaiPhong);
        }
    }
}
