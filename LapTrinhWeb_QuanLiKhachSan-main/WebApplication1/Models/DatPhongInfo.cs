using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class DatPhongInfo
    {
        public string MaDP { get; set; }
        public string MaKH { get; set; }
        public string MaPH { get; set; }
        public DateTime NgayDat { get; set; }
        public DateTime NgayNhan { get; set; }
        public DateTime NgayTra { get; set; }
        public string TinhTrang { get; set; }
        public decimal DonGia { get; set; }
        public decimal DatCoc { get; set; }
        public List<string> SelectedServices { get; set; }
 
    }
}