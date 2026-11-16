using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using _25DH190025_NguyenHuynhHongQuan.Models;

namespace _25DH190025_NguyenHuynhHongQuan.Areas.Admin.Controllers
{
    public class UsersController : BaseAdminController
    {
        private MyStoreEntities db = new MyStoreEntities();

        private void PopulateRoles(string selectedRole = "Customer")
        {
            selectedRole = !string.IsNullOrWhiteSpace(selectedRole) ? selectedRole.Trim() : "Customer";
            var roles = new List<SelectListItem>
            {
                new SelectListItem { Value = "Admin", Text = "👑 Quản trị viên (Admin) - Toàn quyền quản trị website" },
                new SelectListItem { Value = "Customer", Text = "🛒 Khách hàng (Customer) - Mua sắm & xem đơn hàng" }
            };
            ViewBag.RoleList = new SelectList(roles, "Value", "Text", selectedRole);
        }

        // GET: Admin/Users
        public ActionResult Index(string searchTerm = "", string roleFilter = "")
        {
            var users = db.Users.Include(u => u.Customers).AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.Trim();
                users = users.Where(u => u.Username.Contains(searchTerm) ||
                                         u.UserRole.Contains(searchTerm) ||
                                         u.Customers.Any(c => c.CustomerName.Contains(searchTerm) ||
                                                              c.CustomerEmail.Contains(searchTerm) ||
                                                              c.CustomerPhone.Contains(searchTerm)));
                ViewBag.SearchTerm = searchTerm;
            }

            if (!string.IsNullOrWhiteSpace(roleFilter))
            {
                string rf = roleFilter.Trim().ToLower();
                if (rf == "admin")
                {
                    users = users.Where(u => u.UserRole.ToLower() == "admin" || u.UserRole.ToLower() == "a");
                }
                else if (rf == "customer")
                {
                    users = users.Where(u => u.UserRole.ToLower() == "customer" || u.UserRole.ToLower() == "c" || u.UserRole.ToLower() == "user");
                }
                ViewBag.RoleFilter = roleFilter;
            }

            ViewBag.TotalCount = db.Users.Count();
            ViewBag.AdminCount = db.Users.Count(u => u.UserRole.ToLower() == "admin" || u.UserRole.ToLower() == "a");
            ViewBag.CustomerCount = db.Users.Count(u => u.UserRole.ToLower() == "customer" || u.UserRole.ToLower() == "c" || u.UserRole.ToLower() == "user");

            return View(users.OrderByDescending(u => u.UserRole).ThenBy(u => u.Username).ToList());
        }

        // GET: Admin/Users/Details/admin
        public ActionResult Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            User user = db.Users.Include(u => u.Customers).SingleOrDefault(u => u.Username == id);
            if (user == null)
            {
                return HttpNotFound();
            }

            return View(user);
        }

        // GET: Admin/Users/Create
        public ActionResult Create()
        {
            PopulateRoles("Customer");
            return View(new User { UserRole = "Customer" });
        }

        // POST: Admin/Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Username,Password,UserRole")] User user,
                                   string CustomerName, string CustomerPhone, string CustomerEmail, string CustomerAddress)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrWhiteSpace(user.Username))
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập không được để trống!");
                    PopulateRoles(user.UserRole);
                    return View(user);
                }

                if (db.Users.Any(u => u.Username.ToLower() == user.Username.ToLower()))
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại!");
                    PopulateRoles(user.UserRole);
                    return View(user);
                }

                db.Users.Add(user);

                var customer = new Customer
                {
                    Username = user.Username,
                    CustomerName = string.IsNullOrWhiteSpace(CustomerName) ? user.Username : CustomerName,
                    CustomerPhone = string.IsNullOrWhiteSpace(CustomerPhone) ? "Chưa cập nhật" : CustomerPhone,
                    CustomerEmail = CustomerEmail,
                    CustomerAddress = CustomerAddress
                };
                db.Customers.Add(customer);

                db.SaveChanges();
                TempData["SuccessMessage"] = "Đã thêm tài khoản " + user.Username + " thành công!";
                return RedirectToAction("Index");
            }

            PopulateRoles(user.UserRole);
            return View(user);
        }

        // GET: Admin/Users/Edit/admin
        public ActionResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            User user = db.Users.Include(u => u.Customers).SingleOrDefault(u => u.Username == id);
            if (user == null)
            {
                return HttpNotFound();
            }

            PopulateRoles(user.UserRole);
            return View(user);
        }

        // POST: Admin/Users/Edit/admin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Username,Password,UserRole")] User user,
                                 string originalUsername,
                                 string CustomerName, string CustomerPhone, string CustomerEmail, string CustomerAddress)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrWhiteSpace(originalUsername))
                {
                    originalUsername = user.Username;
                }

                string newUsername = !string.IsNullOrWhiteSpace(user.Username) ? user.Username.Trim() : "";
                string oldUsername = originalUsername.Trim();

                if (string.IsNullOrWhiteSpace(newUsername))
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập không được để trống!");
                    PopulateRoles(user.UserRole);
                    return View(user);
                }

                // Nếu người dùng thay đổi tên đăng nhập
                if (!string.Equals(newUsername, oldUsername, StringComparison.OrdinalIgnoreCase))
                {
                    // Kiểm tra xem tên đăng nhập mới đã có người dùng chưa
                    if (db.Users.Any(u => u.Username.ToLower() == newUsername.ToLower()))
                    {
                        ModelState.AddModelError("Username", "Tên đăng nhập mới này đã tồn tại!");
                        PopulateRoles(user.UserRole);
                        return View(user);
                    }

                    // Cập nhật tên đăng nhập trong bảng User (SQL Server tự cascade sang Customer.Username)
                    db.Database.ExecuteSqlCommand("UPDATE [User] SET Username = @p0 WHERE Username = @p1", newUsername, oldUsername);

                    // Cập nhật Session nếu đang đổi tên của chính tài khoản đang đăng nhập
                    if (string.Equals(oldUsername, Session["Username"] as string, StringComparison.OrdinalIgnoreCase))
                    {
                        Session["Username"] = newUsername;
                    }
                }

                // Tải lại đối tượng user với tên mới (hoặc tên hiện tại)
                var existingUser = db.Users.Include(u => u.Customers).SingleOrDefault(u => u.Username == newUsername);
                if (existingUser == null)
                {
                    return HttpNotFound();
                }

                existingUser.Password = user.Password;
                existingUser.UserRole = !string.IsNullOrWhiteSpace(user.UserRole) ? user.UserRole.Trim() : "Customer";

                if (string.Equals(newUsername, Session["Username"] as string, StringComparison.OrdinalIgnoreCase))
                {
                    Session["UserRole"] = existingUser.UserRole;
                }

                var customer = existingUser.Customers.FirstOrDefault();
                if (customer != null)
                {
                    customer.CustomerName = string.IsNullOrWhiteSpace(CustomerName) ? customer.CustomerName : CustomerName;
                    customer.CustomerPhone = string.IsNullOrWhiteSpace(CustomerPhone) ? customer.CustomerPhone : CustomerPhone;
                    customer.CustomerEmail = CustomerEmail;
                    customer.CustomerAddress = CustomerAddress;
                    if (string.Equals(newUsername, Session["Username"] as string, StringComparison.OrdinalIgnoreCase))
                    {
                        Session["FullName"] = customer.CustomerName;
                    }
                }
                else
                {
                    db.Customers.Add(new Customer
                    {
                        Username = existingUser.Username,
                        CustomerName = string.IsNullOrWhiteSpace(CustomerName) ? existingUser.Username : CustomerName,
                        CustomerPhone = string.IsNullOrWhiteSpace(CustomerPhone) ? "Chưa cập nhật" : CustomerPhone,
                        CustomerEmail = CustomerEmail,
                        CustomerAddress = CustomerAddress
                    });
                }

                db.SaveChanges();
                TempData["SuccessMessage"] = "Cập nhật tài khoản " + newUsername + " thành công!";
                return RedirectToAction("Index");
            }

            PopulateRoles(user.UserRole);
            return View(user);
        }

        // GET: Admin/Users/Delete/admin
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            User user = db.Users.Include(u => u.Customers).SingleOrDefault(u => u.Username == id);
            if (user == null)
            {
                return HttpNotFound();
            }

            return View(user);
        }

        // POST: Admin/Users/Delete/admin
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            string currentUsername = Session["Username"] as string ?? (User != null && User.Identity != null ? User.Identity.Name : "");
            if (string.Equals(id, currentUsername, StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Không thể xóa tài khoản hiện đang đăng nhập (" + id + ")!";
                return RedirectToAction("Index");
            }

            User user = db.Users.Include(u => u.Customers).SingleOrDefault(u => u.Username == id);
            if (user != null)
            {
                // Xóa các bản ghi Customer liên quan nếu có
                var customers = db.Customers.Where(c => c.Username == id).ToList();
                foreach (var c in customers)
                {
                    // Nếu customer có đơn hàng, không xóa trực tiếp mà thông báo
                    if (db.Orders.Any(o => o.CustomerID == c.CustomerID))
                    {
                        TempData["Error"] = "Tài khoản " + id + " đã có đơn hàng trong hệ thống, không thể xóa!";
                        return RedirectToAction("Index");
                    }
                    db.Customers.Remove(c);
                }

                db.Users.Remove(user);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Đã xóa tài khoản " + id + " thành công!";
            }

            return RedirectToAction("Index");
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
