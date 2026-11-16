using System;
using System.Web.Mvc;

namespace _25DH190107_NguyenMinhHuy.Areas.Admin.Controllers
{
    // Controller nền tảng phân quyền: Chỉ cho phép tài khoản vai trò Admin truy cập khu vực quản trị
    public class BaseAdminController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var role = Session["UserRole"] as string;
            bool isAdmin = !string.IsNullOrWhiteSpace(role) &&
                           (role.Trim().Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                            role.Trim().Equals("A", StringComparison.OrdinalIgnoreCase));

            if (!isAdmin)
            {
                TempData["Notice"] = "Bạn cần đăng nhập bằng tài khoản Quản trị viên (Admin) để truy cập khu vực này!";
                filterContext.Result = new RedirectResult("~/Account/Login");
                return;
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
