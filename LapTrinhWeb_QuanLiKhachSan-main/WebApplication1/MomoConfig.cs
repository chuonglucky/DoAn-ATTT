using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1
{
    public class MomoConfig
    {
        public static string PartnerCode = "MOMO";
        public static string AccessKey = "F8BBA842ECF85";
        public static string SecretKey = "K951B6PE1waDMi640xX08PD3vg6EkVlz";
        public static string Endpoint = "https://test-payment.momo.vn/v2/gateway/api/create";
        public static string ReturnUrl = "https://localhost:44354/DatPhong/ReturnUrl";
        public static string NotifyUrl = "https://webhook.site/454e7b77-f177-4ece-8236-ddf1c26ba7f8"; // URL callback test
    }
}