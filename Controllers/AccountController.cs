using _25DH190107_NguyenMinhHuy.Models;
using _25DH190107_NguyenMinhHuy.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace _25DH190107_NguyenMinhHuy.Controllers
{
    public class AccountController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        private static bool IsAdmin(string role)
        {
            return !string.IsNullOrWhiteSpace(role) &&
                   (role.Trim().Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                    role.Trim().Equals("A", StringComparison.OrdinalIgnoreCase));
        }

        private ActionResult RedirectByRole(string role)
        {
            if (IsAdmin(role))
            {
                return RedirectToAction("Index", "Products", new { area = "Admin" });
            }
            return RedirectToAction("Index", "Home", new { area = "" });
        }

        // GET: Account/Login
        public ActionResult Login()
        {
            if (Session["Username"] != null)
            {
                return RedirectByRole(Session["UserRole"] as string);
            }
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginVM model)
        {
            if (string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.Password))
            {
                ViewBag.Error = "Vui lòng nhập tên đăng nhập và mật khẩu!";
                return View(model);
            }

            var user = db.Users.SingleOrDefault(u => u.Username == model.Username && u.Password == model.Password);
            if (user == null)
            {
                ViewBag.Error = "Sai tên đăng nhập hoặc mật khẩu!";
                return View(model);
            }

            string role = (user.UserRole ?? "Customer").Trim();

            Session["Username"] = user.Username;
            Session["UserRole"] = role;

            var customer = db.Customers.SingleOrDefault(c => c.Username == user.Username);
            if (customer != null && !string.IsNullOrWhiteSpace(customer.CustomerName))
            {
                Session["FullName"] = customer.CustomerName;
            }
            else
            {
                Session["FullName"] = user.Username;
            }

            FormsAuthentication.SetAuthCookie(user.Username, false);

            return RedirectByRole(role);
        }

        // GET: Account/Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterVM model)
        {
            if (ModelState.IsValid)
            {
                var existingUser = db.Users.SingleOrDefault(u => u.Username == model.Username);
                if (existingUser != null)
                {
                    ViewBag.Error = "Tên đăng nhập này đã tồn tại!";
                    return View(model);
                }

                var user = new User
                {
                    Username = model.Username,
                    Password = model.Password,
                    UserRole = "Customer"
                };
                db.Users.Add(user);

                var customer = new Customer
                {
                    CustomerName = model.CustomerName,
                    CustomerEmail = model.CustomerEmail,
                    CustomerPhone = model.CustomerPhone,
                    CustomerAddress = model.CustomerAddress,
                    Username = model.Username,
                };
                db.Customers.Add(customer);

                db.SaveChanges();
                TempData["Notice"] = "Đăng ký tài khoản thành công! Vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }

            return View(model);
        }

        // GET: Account/ProfileInfo (Hồ sơ khách hàng)
        public ActionResult ProfileInfo()
        {
            string username = Session["Username"] as string ?? (User != null && User.Identity != null ? User.Identity.Name : null);
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login");
            }

            var customer = db.Customers.SingleOrDefault(c => c.Username == username);
            if (customer == null)
            {
                customer = new Customer { Username = username, CustomerName = username, CustomerPhone = "" };
            }
            return View(customer);
        }

        // POST: Account/ProfileInfo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProfileInfo(Customer model, string newUsername = "", string NewPassword = "")
        {
            string username = Session["Username"] as string ?? (User != null && User.Identity != null ? User.Identity.Name : null);
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login");
            }

            string oldUsername = username.Trim();
            string targetUsername = !string.IsNullOrWhiteSpace(newUsername) ? newUsername.Trim() : (!string.IsNullOrWhiteSpace(model.Username) ? model.Username.Trim() : oldUsername);

            // Nếu người dùng thay đổi tên đăng nhập
            if (!string.Equals(targetUsername, oldUsername, StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(targetUsername))
                {
                    TempData["ErrorMessage"] = "Tên đăng nhập không được để trống!";
                    return RedirectToAction("ProfileInfo");
                }

                // Kiểm tra xem tên đăng nhập mới đã tồn tại chưa
                if (db.Users.Any(u => u.Username.ToLower() == targetUsername.ToLower()))
                {
                    TempData["ErrorMessage"] = "Tên đăng nhập '" + targetUsername + "' đã tồn tại! Vui lòng chọn tên khác.";
                    return RedirectToAction("ProfileInfo");
                }

                // Cập nhật bảng User (SQL Server CASCADE sẽ tự động cập nhật bảng Customer)
                db.Database.ExecuteSqlCommand("UPDATE [User] SET Username = @p0 WHERE Username = @p1", targetUsername, oldUsername);

                // Cập nhật lại định danh cho Session và FormsAuthentication
                Session["Username"] = targetUsername;
                FormsAuthentication.SetAuthCookie(targetUsername, false);
                oldUsername = targetUsername;
            }

            var customer = db.Customers.SingleOrDefault(c => c.Username == oldUsername);
            if (customer != null)
            {
                customer.CustomerName = string.IsNullOrWhiteSpace(model.CustomerName) ? customer.CustomerName : model.CustomerName;
                customer.CustomerPhone = string.IsNullOrWhiteSpace(model.CustomerPhone) ? customer.CustomerPhone : model.CustomerPhone;
                customer.CustomerEmail = model.CustomerEmail;
                customer.CustomerAddress = model.CustomerAddress;
                Session["FullName"] = customer.CustomerName;
            }

            if (!string.IsNullOrWhiteSpace(NewPassword))
            {
                var user = db.Users.SingleOrDefault(u => u.Username == oldUsername);
                if (user != null)
                {
                    user.Password = NewPassword.Trim();
                }
            }

            db.SaveChanges();
            TempData["SuccessMessage"] = "Cập nhật hồ sơ tài khoản thành công!";
            return RedirectToAction("ProfileInfo");
        }

        // GET: Account/Logout
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Account", new { area = "" });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
