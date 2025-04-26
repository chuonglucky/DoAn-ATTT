using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Areas.Admin.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin/Admin
        QL_KhachSanEntities2 db =new QL_KhachSanEntities2();
        public ActionResult Index()
        {
            // Kiểm tra nếu người dùng đã đăng nhập
            if (Session["NhanVien"] != null && Session["UserChucVu"] != null)
            {
                string tenDangNhap = Session["NhanVien"].ToString();
                string chucVu = Session["UserChucVu"].ToString();

                // Lấy thông tin nhân viên từ cơ sở dữ liệu
                var nhanVien = db.NHANVIENs.SingleOrDefault(n => n.HoTen == tenDangNhap);

                if (nhanVien != null)
                {
                    // Truyền thông tin nhân viên vào ViewBag
                    ViewBag.ThongTinNhanVien = nhanVien;

                    // Nếu là quản lý, truyền thêm thông tin vai trò
                    if (chucVu.ToLower() == "quản lý" || chucVu.ToLower() == "manager")
                    {
                        ViewBag.VaiTro = "Quản lý";
                    }
                    else
                    {
                        ViewBag.VaiTro = "Nhân viên";
                    }
                }
            }
            else
            {
                // Nếu chưa đăng nhập, chuyển hướng đến trang đăng nhập
                return RedirectToAction("DangNhap");
            }

            return View();

        }
        [HttpGet]
        public ActionResult DangNhap()
        {
            return View();
        }
        [HttpPost]
        public ActionResult DangNhap(FormCollection f)
        {
            //Gán các giá trị người dùng nhập liệu cho các biến 
            var sTenDN = f["UserName"];
            var sMatKhau = f["Password"];
            //Gán giá trị cho đối tượng được tạo mới (ad)  
            NHANVIEN ad = db.NHANVIENs.SingleOrDefault(n => n.Username == sTenDN && n.Password == sMatKhau);
            if (ad != null)
            {
                Session["UserChucVu"] = ad.ChucVu;
                Session["NhanVien"] = ad.HoTen;
                return RedirectToAction("Index", "Admin");
            }
            else
            {
                ViewBag.ThongBao = "Tên đăng nhập hoặc mật khẩu không đúng";
            }
            return View();
        }
        
        public ActionResult Logout()
        {
            Session["Admin"] = null;
            return RedirectToAction("DangNhap");
        }
    }
}