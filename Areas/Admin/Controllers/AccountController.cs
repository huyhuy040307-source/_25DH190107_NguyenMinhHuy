using System.Web.Mvc;

namespace _25DH190025_NguyenHuynhHongQuan.Areas.Admin.Controllers
{
    public class AccountController : Controller
    {
        // Điều hướng toàn bộ về trang đăng nhập chung duy nhất của hệ thống (giống Reciper)
        public ActionResult Login()
        {
            return RedirectToAction("Login", "Account", new { area = "" });
        }

        public ActionResult Logout()
        {
            return RedirectToAction("Logout", "Account", new { area = "" });
        }

        public ActionResult Register()
        {
            return RedirectToAction("Register", "Account", new { area = "" });
        }
    }
}
