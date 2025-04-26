using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class DanhGiaPhong
    {
        public string Name { get; set; }
        public double rating { get; set; }
        public double gia { get; set; }
        public string anh { get; set; }
        public int cnt { get; set; }

        public string maphong
        {
            get; set;
        }
    }
}