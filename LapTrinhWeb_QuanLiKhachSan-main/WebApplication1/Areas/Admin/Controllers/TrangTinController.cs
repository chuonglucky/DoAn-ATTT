using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Areas.Admin.Controllers
{
    public class TrangTinController : Controller
    {
        // GET: Admin/TrangTin
        QL_KhachSanEntities2 db = new QL_KhachSanEntities2();
        public ActionResult Index()
        {
            var trangTinList = db.TRANGTINs.ToList();
            ViewBag.TrangTinList = trangTinList;
            return View(db.TRANGTINs.ToList());
        }
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(TRANGTIN tt, FormCollection f)
        {
            if (ModelState.IsValid)
            {
                tt.MetaTitle = tt.TenTrang.RemoveDiacritics().Replace(" ", "-");
                tt.NgayTao = DateTime.Now;
                tt.NoiDung = f["NoiDung"];
                db.TRANGTINs.Add(tt);
                db.SaveChanges();
                // Cập nhật lại danh sách trang tin trong NavPartial
                var trangTinList = db.TRANGTINs.ToList();
                ViewBag.TrangTinList = trangTinList;
                return RedirectToAction("Index");
            }
            return View();
        }
        [HttpGet]
        public ActionResult Edit(int id)
        {
            var tt = db.TRANGTINs.Where(t => t.MaTT == id).SingleOrDefault();
            ViewBag.NoiDung=tt.NoiDung;
            return View(tt);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(FormCollection f)
        {
            if (ModelState.IsValid)
            {
                // Sử dụng TryParse để tránh lỗi nếu MaTT không hợp lệ
                if (int.TryParse(f["MaTT"], out int maTT))
                {
                    var tt = db.TRANGTINs.SingleOrDefault(t => t.MaTT == maTT);
                    if (tt != null)
                    {
                        tt.TenTrang = f["TenTrang"];
                        tt.NoiDung = f["NoiDung"];

                        // Chuyển đổi ngày tạo an toàn
                        if (DateTime.TryParse(f["NgayTao"], out DateTime ngayTao))
                        {
                            tt.NgayTao = ngayTao;
                        }

                        // Xử lý MetaTitle bằng cách loại bỏ dấu và thay thế khoảng trắng
                        tt.MetaTitle = f["TenTrang"].RemoveDiacritics().Replace(" ", "-");
                        var trangTinList = db.TRANGTINs.ToList();
                        ViewBag.TrangTinList = trangTinList;
                        db.SaveChanges();
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        return HttpNotFound(); // Không tìm thấy bản ghi
                    }
                }
                else
                {
                    ModelState.AddModelError("MaTT", "Mã TT không hợp lệ.");
                }
            }
            return View();
        }
        [HttpGet]
        public ActionResult Delete(int id)
        {
            var tt = (from t in db.TRANGTINs where t.MaTT == id select t).SingleOrDefault();
            return View(tt);
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirm(int id)
        {
            var tt = (from t in db.TRANGTINs where t.MaTT == id select t).SingleOrDefault();
            db.TRANGTINs.Remove(tt);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

    }
}