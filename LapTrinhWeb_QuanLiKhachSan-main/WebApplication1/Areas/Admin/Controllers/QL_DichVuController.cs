using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;
namespace WebApplication1.Areas.Admin.Controllers
{
    public class QL_DichVuController : Controller
    {
        private QL_KhachSanEntities2 db = new QL_KhachSanEntities2();  
        // GET: Admin/QL_DichVu
        public ActionResult DSDichVu()
        {
            var dv = db.DICHVUs.ToList();
            return View(dv);
        }


        private string GenerateMaDV()
        {
            var lastMaDV = db.DICHVUs
                     .OrderByDescending(d => d.MaDV)
                     .Select(d => d.MaDV)
                     .FirstOrDefault();

            if (lastMaDV == null)
            {
                return "DV001"; // Mã đầu tiên nếu bảng rỗng
            }

            // Lấy phần số từ mã dịch vụ cuối cùng
            var numberPart = int.Parse(lastMaDV.Substring(2));
            return "DV" + (numberPart + 1).ToString("D3"); // Tăng thêm 1 và định dạng 3 chữ số
        }

        [HttpGet]
        public ActionResult ThemDichVu()
        {      
            return View();
        }

        [HttpPost]
        public ActionResult ThemDichVu(DICHVU dichVu)
        {
            if (ModelState.IsValid)
            {
                // Gọi hàm tự động sinh mã
                dichVu.MaDV = GenerateMaDV();

                db.DICHVUs.Add(dichVu);
                db.SaveChanges();
                return RedirectToAction("DSDichVu");
            }
            return View(dichVu);
        }

        [HttpGet]
        public ActionResult SuaDichVu(string id)
        {
            var dichVu = db.DICHVUs.Find(id);
            if (dichVu == null)
            {
                return HttpNotFound();
            }
            return View(dichVu);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SuaDichVu(DICHVU dichVu)
        {
            if (ModelState.IsValid)
            {
                db.Entry(dichVu).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("DSDichVu");
            }
            return View(dichVu);
        }

        [HttpGet]
        public ActionResult XoaDichVu(string id)
        {
            var dichVu = db.DICHVUs.Find(id);
            if (dichVu == null)
            {
                return HttpNotFound();
            }
            db.DICHVUs.Remove(dichVu);
            db.SaveChanges();
            return RedirectToAction("DSDichVu");
        }

    }
}