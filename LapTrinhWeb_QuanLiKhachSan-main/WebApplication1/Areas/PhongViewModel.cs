using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApplication1.Models;

namespace WebApplication1.Areas
{
    public class PhongViewModel
    {
        public string MaPH { get; set; }
        public string SoPH { get; set; }
        public int SoLuongDaDat { get; set; }
        public int SoLuongDangO { get; set; }
    }

    public class KhachHangDatPhongViewModel
    {
        public string MaKH { get; set; }
        public string MaDP { get; set; }
        public string MaPH { get; set; }

        public string TenKH { get; set; }
        public string DiaChi { get; set; }
        public string SDT { get; set; }
        public DateTime? NgayNhan { get; set; }
        public DateTime? NgayTra { get; set; }
    }
    public class HuyDatPhongViewModel
    {
        public string MaKH { get; set; } 
        public string TenKH { get; set; }
        public string MaPH { get; set; }
        public string SoPH { get; set; }
        public DateTime? NgayNhan { get; set; }
        public DateTime? NgayTra { get; set; }
    }


}