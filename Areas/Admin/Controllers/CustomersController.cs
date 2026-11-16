using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using _25DH190107_NguyenMinhHuy.Models;

namespace _25DH190107_NguyenMinhHuy.Areas.Admin.Controllers
{
    public class CustomersController : BaseAdminController
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Admin/Customers (Chỉ xem danh sách khách hàng, không có Thêm/Sửa/Xóa)
        public ActionResult Index(string searchTerm = "")
        {
            var customers = db.Customers.Include(c => c.Orders).AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();
                customers = customers.Where(c => c.CustomerName.Contains(searchTerm) ||
                                                 c.CustomerPhone.Contains(searchTerm) ||
                                                 c.CustomerEmail.Contains(searchTerm) ||
                                                 c.Username.Contains(searchTerm));
                ViewBag.SearchTerm = searchTerm;
            }

            return View(customers.OrderBy(c => c.CustomerName).ToList());
        }

        // GET: Admin/Customers/Details/5 (Hiển thị thông tin khách hàng + danh sách đơn hàng của khách hàng)
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var customer = db.Customers
                .Include(c => c.Orders.Select(o => o.OrderDetails))
                .SingleOrDefault(c => c.CustomerID == id);

            if (customer == null)
            {
                return HttpNotFound();
            }

            return View(customer);
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
