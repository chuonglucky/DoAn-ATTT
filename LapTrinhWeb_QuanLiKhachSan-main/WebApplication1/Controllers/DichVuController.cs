using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class DichVuController : Controller
    {
        private QL_KhachSanEntities2 db = new QL_KhachSanEntities2();
        // GET: DichVu
        public ActionResult DSDichVu()
        {
            var dichVuList = db.DICHVUs.ToList(); // Lấy danh sách tất cả dịch vụ
            return View(dichVuList); // Truyền danh sách dịch vụ tới View
        }

        public ActionResult ChiTietDichVu(int id)
        {
            var dichVu = db.DICHVUs.Find(id); // Tìm dịch vụ theo id
            if (dichVu == null)
            {
                return HttpNotFound("Dịch vụ không tồn tại");
            }

            return View(dichVu); // Truyền thông tin dịch vụ tới View Chi tiết
        }
    }
}