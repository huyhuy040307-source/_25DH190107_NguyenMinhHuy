using System.Web;
using System.Web.Mvc;

namespace _25DH190107_NguyenMinhHuy
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
