using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Areas.Admin.Controllers
{
    public class BaseController : Controller
    {
        protected QL_KhachSanEntities2 db = new QL_KhachSanEntities2();

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Lấy danh sách TrangTin và thiết lập ViewBag
            ViewBag.TrangTinList = db.TRANGTINs.ToList();
            base.OnActionExecuting(filterContext);
        }
    }
}