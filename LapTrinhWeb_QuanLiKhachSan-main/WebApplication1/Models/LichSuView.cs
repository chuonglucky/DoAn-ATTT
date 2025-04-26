using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class LichSuView
    {
        public string MaDatPhong { get; set; }
        public string TenPhong { get; set; }
        public string LoaiPhong { get; set; }
        public string NgayDat { get; set; }
        public string NgayNhan { get; set; }
        public string NgayTra { get; set; }
        public string DichVu { get; set; }
        public double? ThanhTien { get; set; }
        public bool CoTheHuy { get; set; }
    }
}