using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class CMMD
    {
        public string NoiDung { get; set; }
        public int DanhGia { get; set; }
        public DateTime ThoiGian { get; set; }
        public string HoTenKhachHang
        {
            get; set;
        }
    }
}