using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Areas.Admin.Controllers
{
    public class PhongController : Controller
    {
        // GET: Admin/Phong
        QL_KhachSanEntities2 db = new QL_KhachSanEntities2();
        public ActionResult DanhSachPhong()
        {
            DateTime currentDate = DateTime.Now.Date;

            // Lấy danh sách đặt phòng và danh sách phòng
            var datphongList = db.DATPHONGs.ToList();
            var danhSachPhong = db.PHONGs.ToList();

            // Tạo danh sách ViewModel
            var phongViewModels = danhSachPhong.Select(phong =>
            {
                var datphongs = datphongList.Where(dp => dp.MaPH == phong.MaPH).ToList();

                return new PhongViewModel
                {
                    MaPH = phong.MaPH,
                    SoPH = phong.SoPH,
                    SoLuongDaDat = datphongs.Count(dp => dp.NgayNhan > currentDate && dp.NgayTra >= currentDate),
                    SoLuongDangO = datphongs.Count(dp => dp.NgayNhan <= currentDate && dp.NgayTra >= currentDate)
                };
            }).ToList();

            return View(phongViewModels);
        }
        public ActionResult Chitietphong(string MaPH)
        {
            var phong = db.PHONGs.FirstOrDefault(p => p.MaPH == MaPH);
            if (phong == null)
            {
                return HttpNotFound();
            }

            // Thông tin khách hàng đang ở phòng (nếu có)
            var datphong = db.DATPHONGs.FirstOrDefault(dp => dp.MaPH == MaPH && dp.NgayNhan <= DateTime.Now && dp.NgayTra >= DateTime.Now);

            if (datphong != null)
            {
                var khachHang = db.KHACHHANGs.FirstOrDefault(kh => kh.MaKH == datphong.MaKH);
                if (khachHang != null)
                {
                    ViewBag.MaKH = khachHang.MaKH;
                    ViewBag.TenKH = khachHang.HoTen;
                    ViewBag.NgayNhan = datphong.NgayNhan;
                    ViewBag.NgayTra = datphong.NgayTra;
                }
                else
                {
                    ViewBag.KhachHangStatus = "Không tìm thấy thông tin khách hàng.";
                }
            }
            else
            {
                ViewBag.KhachHangStatus = "Phòng hiện đang trống.";
            }

            // Danh sách khách hàng đã đặt phòng
            var danhSachDatPhong = db.DATPHONGs.Where(dp => dp.MaPH == MaPH).ToList();
            var danhSachKhachHang = new List<KhachHangDatPhongViewModel>();

            foreach (var dp in danhSachDatPhong)
            {
                var khachHang = db.KHACHHANGs.FirstOrDefault(kh => kh.MaKH == dp.MaKH);
                if (datphong != null)
                {

                    if (khachHang != null && dp.MaDP != datphong.MaDP)
                    {
                        danhSachKhachHang.Add(new KhachHangDatPhongViewModel
                        {
                            MaKH = khachHang.MaKH,
                            MaDP = dp.MaDP,
                            MaPH = MaPH,
                            TenKH = khachHang.HoTen,
                            DiaChi = khachHang.DiaChi,
                            SDT = khachHang.DienThoai,
                            NgayNhan = dp.NgayNhan.HasValue ? dp.NgayNhan.Value.Date : (DateTime?)null,
                            NgayTra = dp.NgayTra.HasValue ? dp.NgayTra.Value.Date : (DateTime?)null
                        });
                    }

                }
                else
                {
                    danhSachKhachHang.Add(new KhachHangDatPhongViewModel
                    {
                        MaKH = khachHang.MaKH,
                        MaDP = dp.MaDP,
                        MaPH = MaPH,
                        TenKH = khachHang.HoTen,
                        DiaChi = khachHang.DiaChi,
                        SDT = khachHang.DienThoai,
                        NgayNhan = dp.NgayNhan.HasValue ? dp.NgayNhan.Value.Date : (DateTime?)null,
                        NgayTra = dp.NgayTra.HasValue ? dp.NgayTra.Value.Date : (DateTime?)null
                    });
                }
                
            }

            ViewBag.DanhSachKhachHang = danhSachKhachHang;

            return View(phong);
        }

        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.MaLoai = new SelectList(db.LOAIPHONGs.OrderBy(n => n.TenLoai), "MaLoai", "TenLoai");
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(PHONG phong, FormCollection f, HttpPostedFileBase AnhUpload)
        {
            ViewBag.MaLoai = new SelectList(db.LOAIPHONGs.OrderBy(n => n.TenLoai), "MaLoai", "TenLoai");

            if (AnhUpload == null)
            {
                ViewBag.ThongBao = "Hãy chọn ảnh cho phòng.";
                return View(phong);
            }
            var existingPhong = db.PHONGs.FirstOrDefault(p => p.MaPH == phong.MaPH || p.SoPH == phong.SoPH);
            if (existingPhong != null)
            {
                ModelState.AddModelError("", "Mã phòng hoặc số phòng đã tồn tại. Vui lòng chọn mã khác.");
                return View(phong);
            }
            if (ModelState.IsValid)
            {
                try
                {
                    // Xử lý file upload
                    string fileName = Path.GetFileName(AnhUpload.FileName);
                    string path = Path.Combine(Server.MapPath("~/Images"), fileName);

                    // Kiểm tra file đã tồn tại hay chưa
                    if (!System.IO.File.Exists(path))
                    {
                        AnhUpload.SaveAs(path); // Lưu file lên server
                    }

                    phong.Anh = fileName; // Gán tên file vào thuộc tính Anh
                    phong.Mota= f["MoTa"];
                    // Thêm vào cơ sở dữ liệu
                    db.PHONGs.Add(phong);
                    db.SaveChanges();

                    return RedirectToAction("DanhSachPhong");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Đã xảy ra lỗi: {ex.Message}");
                }
            }

            return View(phong);
        }
        [HttpGet]
        public ActionResult Edit(string MaPH)
        {
            var phong = db.PHONGs.SingleOrDefault(n => n.MaPH == MaPH);
            if (phong == null)
            {
                Response.StatusCode = 404;
                return null;
            }

            ViewBag.MaLoai = new SelectList(db.LOAIPHONGs.OrderBy(n => n.TenLoai), "MaLoai", "TenLoai");
            ViewBag.MoTa = phong.Mota;

            return View(phong);
        }


        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(PHONG phong, FormCollection f, HttpPostedFileBase AnhUpload)
        {
            ViewBag.MaLoai = new SelectList(db.LOAIPHONGs.OrderBy(n => n.TenLoai), "MaLoai", "TenLoai");

            if (!ModelState.IsValid)
            {
                return View(phong);
            }

            try
            {
                // Lấy phòng hiện tại từ CSDL
                var phongDb = db.PHONGs.SingleOrDefault(n => n.MaPH == phong.MaPH);
                if (phongDb == null)
                {
                    return HttpNotFound(); // Nếu không tìm thấy phòng, trả về lỗi
                }

                // Kiểm tra ảnh upload nếu có
                if (AnhUpload != null && AnhUpload.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(AnhUpload.FileName);
                    var path = Path.Combine(Server.MapPath("~/Images"), fileName);

                    // Nếu ảnh chưa tồn tại, lưu ảnh mới
                    if (!System.IO.File.Exists(path))
                    {
                        AnhUpload.SaveAs(path); // Lưu ảnh lên server
                    }
                    phong.Anh = fileName; // Cập nhật tên ảnh vào thuộc tính phong.Anh
                }
                else if (string.IsNullOrEmpty(phong.Anh))
                {
                    // Nếu không có ảnh upload mới và ảnh hiện tại null, giữ ảnh cũ
                    phong.Anh = phongDb.Anh;
                }

                // Kiểm tra trùng số phòng
                var existingPhong = db.PHONGs.FirstOrDefault(p => p.SoPH == phong.SoPH && p.MaPH != phong.MaPH);
                if (existingPhong != null)
                {
                    ModelState.AddModelError("", "Số phòng đã tồn tại. Vui lòng chọn mã khác.");
                    return View(phong);
                }
                // Cập nhật thông tin phòng
                phongDb.SoPH = phong.SoPH;
                phongDb.Mota = f["MoTa"]; 
                phongDb.TrangThai = phong.TrangThai;
                phongDb.Gia = phong.Gia;
                phongDb.MaLoai = phong.MaLoai;
                phongDb.NoiThat = phong.NoiThat;
                phongDb.DienTich = phong.DienTich;
                phongDb.Anh = phong.Anh;

                // Cập nhật lại thông tin đặt phòng
                var datphongDB = db.DATPHONGs.SingleOrDefault(n => n.MaPH == phong.MaPH);
                if (datphongDB != null)
                {
                    datphongDB.NgayNhan = phong.CheckIn;
                    datphongDB.NgayTra = phong.CheckOut;
                }

                // Lưu thay đổi vào cơ sở dữ liệu
                db.SaveChanges();

                return RedirectToAction("DanhSachPhong");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Đã xảy ra lỗi: {ex.Message}");
            }

            return View(phong);
        }

        [HttpGet]
        public ActionResult TraPhong(string MaPH)
        {
            // Kiểm tra phòng tồn tại hay không
            var phong = db.PHONGs.SingleOrDefault(p => p.MaPH == MaPH);

            if (phong == null)
            {
                TempData["ErrorMessage"] = "Phòng không tồn tại hoặc không tìm thấy.";
                return RedirectToAction("DanhSachPhong");
            }

            // Kiểm tra xem phòng có khách đang ở hay không
            var datphong = db.DATPHONGs.SingleOrDefault(dp => dp.MaPH == MaPH && dp.NgayNhan <= DateTime.Now && dp.NgayTra >= DateTime.Now);
            if (datphong == null)
            {
                TempData["ErrorMessage"] = "Phòng hiện đang trống hoặc chưa có khách đặt.";
                return RedirectToAction("DanhSachPhong");
            }

            var kh = db.KHACHHANGs.SingleOrDefault(k => k.MaKH == datphong.MaKH);
            if (kh == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin khách hàng đang ở phòng này.";
                return RedirectToAction("DanhSachPhong");
            }

            try
            {
                // Lưu dữ liệu vào bảng LichSuTraPhong
                var lichSuTraPhong = new LichSuTraPhong
                {
                    MaPH = MaPH,
                    SoPH = phong.SoPH,
                    MaKH = kh.MaKH,
                    HoTen = kh.HoTen,
                    DienThoai = kh.DienThoai,
                    CCCD = kh.CCCD,
                    Email = kh.Email,
                    Gia = phong.Gia,
                    NgayNhan = datphong.NgayNhan,
                    NgayTra = DateTime.Now
                };

                db.LichSuTraPhongs.Add(lichSuTraPhong);
                var dichVuLienQuan = db.DATDICHVUs.Where(dv => dv.MaDP == datphong.MaDP).ToList();
                if (dichVuLienQuan.Any())
                {
                    db.DATDICHVUs.RemoveRange(dichVuLienQuan);
                }
                // Cập nhật trạng thái phòng thành "Available"
                phong.TrangThai = "Available";

                // Xóa thông tin đặt phòng (cho phòng trả lại)
                db.DATPHONGs.Remove(datphong);

                // Lưu thay đổi
                db.SaveChanges();

                // Thông báo trả phòng thành công
                TempData["SuccessMessage"] = "Trả phòng thành công!";
                return RedirectToAction("DanhSachPhong");
            }
            catch (Exception ex)
            {
                // Lỗi trong quá trình trả phòng
                TempData["ErrorMessage"] = $"Đã xảy ra lỗi khi trả phòng: {ex.Message}";
                return RedirectToAction("DanhSachPhong");
            }
        }
        public ActionResult HuyDatPhong(string maDP, string maPH)
        {
            if (string.IsNullOrEmpty(maDP))
            {
                return HttpNotFound();
            }

            // Tìm đặt phòng theo mã MaDP
            var room = db.DATPHONGs.FirstOrDefault(s => s.MaDP == maDP);
            if (room == null)
            {
                return HttpNotFound();
            }

            // Xóa các bản ghi trong bảng DATDICHVU liên quan đến MaDP
            var dichVuLienQuan = db.DATDICHVUs.Where(dv => dv.MaDP == maDP).ToList();
            if (dichVuLienQuan.Any())
            {
                db.DATDICHVUs.RemoveRange(dichVuLienQuan);
            }

            // Xóa bản ghi trong bảng DATPHONG
            db.DATPHONGs.Remove(room);

            // Lưu thay đổi vào cơ sở dữ liệu
            db.SaveChanges();

            // Điều hướng lại trang lịch sử
            return RedirectToAction("Chitietphong", new { MaPH = maPH });
        }

        public ActionResult LichSuTraPhong()
        {
            var lstraphong = db.LichSuTraPhongs.ToList();
            return View(lstraphong);
        }
    }

}