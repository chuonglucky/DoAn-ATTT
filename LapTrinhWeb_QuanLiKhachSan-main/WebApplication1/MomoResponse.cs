using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1
{
    public class MomoResponse
    {
        public string requestId { get; set; }
        public string orderId { get; set; }
        public int errorCode { get; set; }
        public string message { get; set; }
        public string payUrl { get; set; }
    }
}