using System.Web;
using System.Web.Mvc;

namespace _25DH190025_NguyenHuynhHongQuan
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
