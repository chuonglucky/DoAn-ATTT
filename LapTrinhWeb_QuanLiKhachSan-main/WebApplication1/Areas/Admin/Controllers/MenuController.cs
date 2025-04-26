using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Areas.Admin.Controllers
{
    public class MenuController : Controller
    {
        QL_KhachSanEntities2 db = new QL_KhachSanEntities2();
        public ActionResult Index()
        {
            // Lấy danh sách các menu cấp cha và sắp xếp theo thứ tự
            var listMenu = db.MENUs.Where(m => m.ParentId == null)
                                     .OrderBy(m => m.OrderNumber)
                                     .ToList();

            // Tạo một mảng để lưu trữ số lượng con của mỗi menu cha
            int[] a = new int[listMenu.Count()];

            // Duyệt qua từng menu cha
            for (int i = 0; i < listMenu.Count; i++)
            {
                // Lấy ID của menu cha hiện tại
                int id = listMenu[i].Id;

                // Lấy danh sách các menu con của menu cha hiện tại
                var l = db.MENUs.Where(m => m.ParentId == id);

                // Đếm số lượng menu con
                int k = l.Count();

                // Lưu số lượng menu con vào mảng
                a[i] = k;
            }

            // Gán mảng chứa số lượng con vào ViewBag
            ViewBag.lst = a;
            List<TRANGTIN> tt = db.TRANGTINs.ToList();


            ViewBag.TrangTin = tt;

            // Trả về view với danh sách các menu cấp cha
            return View(listMenu);
        }
        [ChildActionOnly]
        public ActionResult ChildMenu(int parentId)
        {
            List<MENU> lst = new List<MENU>();
            lst = db.MENUs.Where(n => n.ParentId == parentId).OrderBy(m => m.OrderNumber).ToList();
            ViewBag.Count = lst.Count();
            int[] a = new int[lst.Count()];
            for (int i = 0; i < lst.Count; i++)
            {
                int id = lst[i].Id;
                var l = db.MENUs.Where(n => n.ParentId == id);
                int k = l.Count();
                a[i] = k;
            }
            ViewBag.lst = a;
            return PartialView("ChildMenu", lst);
        }

        [ChildActionOnly]
        public ActionResult ChildMenu1(int parentId)
        {
            List<MENU> lst = new List<MENU>();
            lst = db.MENUs.Where(n => n.ParentId == parentId).OrderBy(m => m.OrderNumber).ToList();
            ViewBag.Count = lst.Count();
            int[] a = new int[lst.Count()];
            for (int i = 0; i < lst.Count; i++)
            {
                int id = lst[i].Id;
                var l = db.MENUs.Where(n => n.ParentId == id);
                int k = l.Count();
                a[i] = k;
            }
            ViewBag.lst = a;
            return PartialView("ChildMenu1", lst);
        }

        [HttpPost]
        public ActionResult AddMenu(FormCollection f)
        {
            if (!string.IsNullOrEmpty(f["ThemChuDe"]))
            {
                MENU m = new MENU();
                int maCD = int.Parse(f["MaCD"].ToString());
                if (!string.IsNullOrEmpty(f["ParentID"]))
                {
                    m.ParentId = int.Parse(f["ParentID"]);
                }
                else
                {
                    m.ParentId = null;
                }
                m.OrderNumber = int.Parse(f["Number"]);
                List<MENU> l = null;
                if (m.ParentId == null)
                    l = db.MENUs.Where(k => k.ParentId == null && k.OrderNumber >= m.OrderNumber).ToList();
                else
                    l = db.MENUs.Where(k => k.ParentId == m.ParentId && k.OrderNumber >= m.OrderNumber).ToList();
                for (int i = 0; i < l.Count; i++)
                    l[i].OrderNumber++;
                db.MENUs.Add(m);
                db.SaveChanges();
            }
            else if (!string.IsNullOrEmpty(f["ThemTrang"]))
            {
                MENU m = new MENU();
                int maTT = int.Parse(f["MaTT"].ToString());
                var trang = db.TRANGTINs.Where(t => t.MaTT == maTT).SingleOrDefault();
                m.MenuName = trang.TenTrang;
                m.MenuLink = trang.MetaTitle;
                if (!string.IsNullOrEmpty(f["ParentID"]))
                {
                    m.ParentId = int.Parse(f["ParentID"]);
                }
                else
                {
                    m.ParentId = null;
                }
                m.OrderNumber = int.Parse(f["Number1"]);
                List<MENU> l = null;
                if (m.ParentId == null)
                    l = db.MENUs.Where(k => k.ParentId == null && k.OrderNumber >= m.OrderNumber).ToList();
                else
                    l = db.MENUs.Where(k => k.ParentId == m.ParentId && k.OrderNumber >= m.OrderNumber).ToList();
                for (int i = 0; i < l.Count; i++)
                    l[i].OrderNumber++;
                db.MENUs.Add(m);
                db.SaveChanges();
            }
            else if (!string.IsNullOrEmpty(f["ThemLink"]))
            {
                MENU m = new MENU();
                m.MenuName = f["TenMenu"];
                m.MenuLink = f["Link"];
                if (!string.IsNullOrEmpty(f["ParentID"]))
                {
                    m.ParentId = int.Parse(f["ParentID"]);
                }
                else
                {
                    m.ParentId = null;
                }
                m.OrderNumber = int.Parse(f["Number2"]);
                List<MENU> l = null;
                if (m.ParentId == null)
                    l = db.MENUs.Where(k => k.ParentId == null && k.OrderNumber >= m.OrderNumber).ToList();
                else
                    l = db.MENUs.Where(k => k.ParentId == m.ParentId && k.OrderNumber >= m.OrderNumber).ToList();
                for (int i = 0; i < l.Count; i++)
                    l[i].OrderNumber++;
                db.MENUs.Add(m);
                db.SaveChanges();
            }
            return Redirect("~/Admin/Menu/Index");
        }
        [HttpPost]
        public JsonResult Delete(int id)
        {
            List<MENU> submn = db.MENUs.Where(m => m.ParentId == id).ToList();
            if (submn.Count() > 0)
            {
                return Json(new { code = 500, msg = "Còn menu con, không xóa được." }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var mn = db.MENUs.SingleOrDefault(m => m.Id == id);
                List<MENU> l = null;
                if (mn.ParentId == null)
                    l = db.MENUs.Where(k => k.ParentId == null && k.OrderNumber > mn.OrderNumber).ToList();
                else
                    l = db.MENUs.Where(k => k.ParentId == mn.ParentId && k.OrderNumber > mn.OrderNumber).ToList();
                for (int i = 0; i < l.Count; i++)
                    l[i].OrderNumber--;
                db.MENUs.Remove(mn);
                db.SaveChanges();
                return Json(new { code = 200, msg = "Xóa thành công." }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public JsonResult Update(int id)
        {
            try
            {
                var mn = (from m in db.MENUs
                          where (m.Id == id)
                          select new
                          {
                              Id = m.Id,
                              MenuName = m.MenuName,
                              MenuLink = m.MenuLink,
                              OrderNumber = m.OrderNumber
                          }).SingleOrDefault();

                return Json(new { code = 200, mn = mn, msg = "Lấy thông tin thành công." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { code = 500, msg = "Lấy thông tin thất bại." + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public JsonResult Update(int id, string strTenMenu, string strLink, int STT)
        {
            try
            {
                var menu = db.MENUs.SingleOrDefault(m => m.Id == id);
                List<MENU> l = null;

                if (STT < menu.OrderNumber)
                {
                    if (menu.ParentId == null)
                        l = db.MENUs.Where(m => m.ParentId == null && m.OrderNumber >= STT && m.OrderNumber <= menu.OrderNumber).ToList();
                    else
                        l = db.MENUs.Where(m => m.ParentId == menu.ParentId && m.OrderNumber >= STT && m.OrderNumber <= menu.OrderNumber).ToList();
                    for (int i = 0; i < l.Count; i++)
                        l[i].OrderNumber++;
                }
                else if (STT > menu.OrderNumber)
                {
                    if (menu.ParentId == null)
                        l = db.MENUs.Where(m => m.ParentId == null && m.OrderNumber >= menu.OrderNumber && m.OrderNumber <= STT).ToList();
                    else
                        l = db.MENUs.Where(m => m.ParentId == menu.ParentId && m.OrderNumber >= menu.OrderNumber && m.OrderNumber <= STT).ToList();
                    for (int i = 0; i < l.Count; i++)
                        l[i].OrderNumber--;
                }


                menu.MenuName = strTenMenu;
                menu.MenuLink = strLink;
                menu.OrderNumber = STT;

                db.SaveChanges();

                return Json(new { code = 200, msg = "Sửa menu thành công.", JsonRequestBehavior.AllowGet });
            }
            catch (Exception ex)
            {
                return Json(new { code = 500, msg = "Sửa menu thất bại. Lỗi: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}