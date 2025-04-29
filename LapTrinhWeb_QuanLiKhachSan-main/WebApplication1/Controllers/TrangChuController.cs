using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class TrangChuController : Controller
    {
        // GET: TrangChu
        QL_KhachSanEntities2 db =new QL_KhachSanEntities2();
        public ActionResult Index()
        {
            var lstp = (from bl in db.BINHLUANs
                        join p in db.PHONGs
                        on bl.MaPH equals p.MaPH
                        group bl by bl.MaPH into phonghot
                        select new DanhGiaPhong
                        {
                            Name = phonghot.FirstOrDefault().PHONG.SoPH,
                            rating = phonghot.Average(p => (double)p.DanhGia),
                            gia = phonghot.FirstOrDefault().PHONG.Gia.HasValue ? (double)phonghot.FirstOrDefault().PHONG.Gia : 0.0,
                            anh = phonghot.FirstOrDefault().PHONG.Anh,
                            cnt = phonghot.Count(),
                            maphong = phonghot.FirstOrDefault().PHONG.MaPH
                        }
                        );
            var lstRoom = lstp.Take(4).ToList();

            return View(lstRoom);
        }
        public ActionResult _NavPartial()
        {
            var trangTinList = db.TRANGTINs.ToList();
            return PartialView(trangTinList);
        }
        

        public ActionResult FooterPartial()
        {
            return PartialView("_FooterPartial");
        }

        public ActionResult SliderPartial()
        {
            return PartialView("_SliderPartial");
        }
        public ActionResult TrangTin(string metatitle)
        {
            var trangTin = db.TRANGTINs.FirstOrDefault(x => x.MetaTitle == metatitle);
            if (trangTin == null)
            {
                return HttpNotFound();
            }
            return View(trangTin);
        }
    }
}