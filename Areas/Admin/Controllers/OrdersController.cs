using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PagedList;
using _25DH190107_NguyenMinhHuy.Models;

namespace _25DH190107_NguyenMinhHuy.Areas.Admin.Controllers
{
    public class OrdersController : BaseAdminController
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Admin/Orders
        public ActionResult Index(string searchName = "", string paymentStatus = "", DateTime? orderDate = null, string status = "", int? page = 1)
        {
            var ordersQuery = db.Orders
                .Include(o => o.Customer)
                .Include("OrderDetails.Product")
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchName))
            {
                string keyword = searchName.Trim();
                ordersQuery = ordersQuery.Where(o => o.Customer.CustomerName.Contains(keyword));
            }

            if (orderDate.HasValue)
            {
                DateTime targetDate = orderDate.Value.Date;
                ordersQuery = ordersQuery.Where(o => DbFunctions.TruncateTime(o.OrderDate) == targetDate);
            }

            var ordersList = ordersQuery
                .OrderByDescending(o => o.OrderDate)
                .ThenByDescending(o => o.OrderID)
                .ToList();

            if (!string.IsNullOrWhiteSpace(paymentStatus))
            {
                ordersList = ordersList.Where(o => o.PaymentOnlyStatus.IndexOf(paymentStatus, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                ordersList = ordersList.Where(o => string.Equals(o.DeliveryStatus, status, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            ViewBag.SearchName = searchName;
            ViewBag.PaymentStatus = paymentStatus;
            ViewBag.OrderDate = orderDate.HasValue ? orderDate.Value.ToString("yyyy-MM-dd") : "";
            ViewBag.SelectedStatus = status;

            int pageNumber = page ?? 1;
            int pageSize = 15; // Phân trang 15 đơn hàng/trang theo đúng yêu cầu Slide 5

            return View(ordersList.ToPagedList(pageNumber, pageSize));
        }

        // GET: Admin/Orders/DetailedOrder/5
        public ActionResult DetailedOrder(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Order order = db.Orders
                .Include(o => o.Customer)
                .Include("OrderDetails.Product")
                .SingleOrDefault(o => o.OrderID == id);

            if (order == null)
            {
                return HttpNotFound();
            }

            return View("DetailedOrder", order);
        }

        // GET: Admin/Orders/Details/5
        public ActionResult Details(int? id)
        {
            return DetailedOrder(id);
        }

        // POST: Admin/Orders/ConfirmPayment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmPayment(int orderId, string returnUrl = "")
        {
            var order = db.Orders.SingleOrDefault(o => o.OrderID == orderId);
            if (order == null)
            {
                return HttpNotFound();
            }

            order.PaymentOnlyStatus = "Đã thanh toán";
            db.Entry(order).State = EntityState.Modified;
            db.SaveChanges();

            TempData["SuccessMessage"] = string.Format("Đã xác nhận thanh toán thành công cho đơn hàng #{0}!", orderId);

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("DetailedOrder", new { id = orderId });
        }

        // POST: Admin/Orders/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStatus(int orderId, string deliveryStatus, string paymentStatus, string returnUrl = "")
        {
            var order = db.Orders.SingleOrDefault(o => o.OrderID == orderId);
            if (order == null)
            {
                return HttpNotFound();
            }

            if (!string.IsNullOrWhiteSpace(paymentStatus))
            {
                order.PaymentOnlyStatus = paymentStatus;
            }
            if (!string.IsNullOrWhiteSpace(deliveryStatus))
            {
                order.DeliveryStatus = deliveryStatus;
                if (deliveryStatus == "Đã giao hàng" && string.IsNullOrWhiteSpace(paymentStatus))
                {
                    order.PaymentOnlyStatus = "Đã thanh toán";
                }
            }

            db.Entry(order).State = EntityState.Modified;
            db.SaveChanges();

            TempData["SuccessMessage"] = string.Format("Đã cập nhật trạng thái đơn hàng #{0} thành công!", orderId);

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("DetailedOrder", new { id = orderId });
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
