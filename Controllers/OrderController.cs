using _25DH190107_NguyenMinhHuy.Models;
using _25DH190107_NguyenMinhHuy.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace _25DH190107_NguyenMinhHuy.Controllers
{
    public class OrderController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        private string CurrentUsername
        {
            get
            {
                if (User != null && User.Identity != null && User.Identity.IsAuthenticated && !string.IsNullOrEmpty(User.Identity.Name))
                {
                    return User.Identity.Name;
                }
                return Session["Username"] as string;
            }
        }

        // GET: Order/Index -> dùng chung giao diện MyOrder (Đơn hàng của tôi)
        public ActionResult Index(string searchTerm = "", string status = "", string expandStatus = "")
        {
            return MyOrder(searchTerm, status, expandStatus);
        }

        // GET: Order/MyOrder (Trang Đơn hàng của tôi / Lịch sử mua hàng theo đúng Slide 6 & 7)
        public ActionResult MyOrder(string searchTerm = "", string status = "", string expandStatus = "")
        {
            var username = CurrentUsername;
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }

            var customer = db.Customers.SingleOrDefault(c => c.Username == username);
            if (customer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var orders = db.Orders
                .Include("OrderDetails.Product")
                .Where(o => o.CustomerID == customer.CustomerID)
                .OrderByDescending(o => o.OrderDate)
                .ThenByDescending(o => o.OrderID)
                .ToList();

            // Tìm kiếm theo Mã đơn hàng hoặc Tên sản phẩm
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string kw = searchTerm.Trim();
                orders = orders.Where(o =>
                    o.OrderID.ToString().Contains(kw) ||
                    (o.OrderDetails != null && o.OrderDetails.Any(od => od.Product != null && od.Product.ProductName.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0))
                ).ToList();
            }

            // Lọc theo tab trạng thái đơn hàng
            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status == "Chờ thanh toán")
                {
                    orders = orders.Where(o => o.PaymentOnlyStatus.IndexOf("Chưa thanh toán", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                               o.PaymentOnlyStatus.IndexOf("Chuyển khoản", StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                }
                else
                {
                    orders = orders.Where(o => string.Equals(o.DeliveryStatus, status, StringComparison.OrdinalIgnoreCase)).ToList();
                }
            }

            ViewBag.SearchTerm = searchTerm;
            ViewBag.SelectedStatus = status;
            ViewBag.ExpandStatus = expandStatus;

            return View("MyOrder", orders);
        }

        // GET: Order/Reorder/5 (Nút MUA LẠI: thêm sản phẩm vào Giỏ hàng với số lượng = 1)
        public ActionResult Reorder(int id)
        {
            var order = db.Orders
                .Include("OrderDetails.Product.Category")
                .SingleOrDefault(o => o.OrderID == id);

            if (order != null && order.OrderDetails != null)
            {
                var cartService = new CartService(Session);
                var cart = cartService.GetCart();

                foreach (var detail in order.OrderDetails)
                {
                    if (detail.Product != null && detail.Product.Quantity > 0)
                    {
                        cart.AddItem(
                            detail.Product.ProductID,
                            detail.Product.ProductImage,
                            detail.Product.ProductName,
                            detail.Product.ProductPrice,
                            1,
                            detail.Product.Category != null ? detail.Product.Category.CategoryName : ""
                        );
                    }
                }
            }

            return RedirectToAction("Index", "Cart");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CancelOrder(int id)
        {
            var username = CurrentUsername;
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }

            var customer = db.Customers.SingleOrDefault(c => c.Username == username);
            if (customer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = db.Orders.Include(o => o.OrderDetails).SingleOrDefault(o => o.OrderID == id && o.CustomerID == customer.CustomerID);
            if (order != null && order.DeliveryStatus == "Chưa giao hàng")
            {
                order.DeliveryStatus = "Đã hủy";
                // Hoàn lại số lượng tồn kho khi hủy đơn
                foreach (var item in order.OrderDetails)
                {
                    var prod = db.Products.Find(item.ProductID);
                    if (prod != null)
                    {
                        prod.Quantity += item.Quantity;
                        prod.SoldQuantity = Math.Max(0, prod.SoldQuantity - item.Quantity);
                    }
                }
                db.SaveChanges();
                TempData["OrderMessage"] = "Đã hủy đơn hàng #" + id + " thành công.";
            }
            else
            {
                TempData["OrderError"] = "Không thể hủy đơn hàng đang giao hoặc đã giao.";
            }

            return RedirectToAction("MyOrder");
        }

        // POST: Order/ConfirmDelivery/5 (Khách hàng xác nhận đã nhận hàng thành công khi Shipper đang giao)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmDelivery(int id, string returnUrl = "")
        {
            var username = CurrentUsername;
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }

            var customer = db.Customers.SingleOrDefault(c => c.Username == username);
            if (customer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = db.Orders.SingleOrDefault(o => o.OrderID == id && o.CustomerID == customer.CustomerID);
            if (order != null)
            {
                if (order.DeliveryStatus == "Đã hủy")
                {
                    TempData["OrderError"] = "Đơn hàng đã bị hủy, không thể xác nhận nhận hàng.";
                }
                else if (order.DeliveryStatus == "Đã giao hàng")
                {
                    TempData["OrderMessage"] = "Đơn hàng #" + id + " đã được xác nhận giao thành công trước đó.";
                }
                else if (order.DeliveryStatus == "Chưa giao hàng")
                {
                    TempData["OrderError"] = "Đơn hàng đang chuẩn bị tại kho của Shop và chưa được giao cho Shipper, quý khách chưa thể nhận hàng.";
                }
                else
                {
                    bool isPaid = order.PaymentOnlyStatus.IndexOf("Đã thanh toán", StringComparison.OrdinalIgnoreCase) >= 0;
                    bool isCOD = order.PaymentMethod.IndexOf("tiền mặt", StringComparison.OrdinalIgnoreCase) >= 0 || order.PaymentMethod.IndexOf("COD", StringComparison.OrdinalIgnoreCase) >= 0;

                    if (!isPaid && !isCOD)
                    {
                        TempData["OrderError"] = "Đơn hàng chuyển khoản cần được Quản trị viên (Admin) xác nhận thanh toán trước khi giao nhận.";
                    }
                    else
                    {
                        // Cập nhật tình trạng giao hàng thành "Đã giao hàng"
                        order.DeliveryStatus = "Đã giao hàng";

                        // Nếu là đơn COD tiền mặt khi nhận hàng, tự động cập nhật sang Đã thanh toán
                        if (!isPaid && isCOD)
                        {
                            order.PaymentOnlyStatus = "Đã thanh toán (Tiền mặt khi nhận hàng)";
                        }

                        db.Entry(order).State = EntityState.Modified;
                        db.SaveChanges();
                        TempData["OrderMessage"] = "Cảm ơn bạn! Đã xác nhận nhận hàng thành công cho đơn hàng #" + id + ".";
                    }
                }
            }
            else
            {
                TempData["OrderError"] = "Không tìm thấy đơn hàng cần xác nhận.";
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("MyOrder");
        }

        private List<CartItem> GetCartItems()
        {
            var cartService = Session["Cart"] as Cart;
            if (cartService != null)
            {
                return cartService.Items.ToList();
            }
            return Session["Cart"] as List<CartItem>;
        }

        // GET: Order/Checkout
        [Authorize]
        public ActionResult Checkout()
        {
            var cart = GetCartItems();
            if (cart == null || !cart.Any())
            {
                return RedirectToAction("Index", "Home");
            }

            var username = CurrentUsername;
            var user = db.Users.SingleOrDefault(u => u.Username == username);
            if (user == null) { return RedirectToAction("Login", "Account"); }

            var customer = db.Customers.SingleOrDefault(c => c.Username == user.Username);
            if (customer == null) { return RedirectToAction("Login", "Account"); }

            var model = new CheckoutVM
            {
                CartItems = cart,
                TotalAmount = cart.Sum(item => item.TotalPrice),
                OrderDate = DateTime.Now,
                ShippingAddress = customer.CustomerAddress,
                CustomerID = customer.CustomerID,
                Username = customer.Username,
                PaymentMethod = "Tiền mặt",
                ShippingMethod = "Giao hàng nhanh"
            };
            return View(model);
        }

        // POST: Order/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Checkout(CheckoutVM model)
        {
            var cart = GetCartItems();
            if (cart == null || !cart.Any()) { return RedirectToAction("Index", "Home"); }

            if (ModelState.IsValid)
            {
                var username = CurrentUsername;
                var user = db.Users.SingleOrDefault(u => u.Username == username);
                if (user == null) { return RedirectToAction("Login", "Account"); }

                var customer = db.Customers.SingleOrDefault(c => c.Username == user.Username);
                if (customer == null) { return RedirectToAction("Login", "Account"); }

                string paymentStatus = "Chưa thanh toán";
                switch (model.PaymentMethod)
                {
                    case "Tiền mặt":
                        paymentStatus = "Thanh toán tiền mặt";
                        break;
                    case "Chuyển khoản":
                        paymentStatus = "Chuyển khoản ngân hàng";
                        break;
                    default:
                        paymentStatus = "Chưa thanh toán";
                        break;
                }

                var order = new Order
                {
                    CustomerID = customer.CustomerID,
                    OrderDate = model.OrderDate == default(DateTime) ? DateTime.Now : model.OrderDate,
                    TotalAmount = cart.Sum(item => item.TotalPrice),
                    PaymentMethod = model.PaymentMethod,
                    ShippingAddress = model.ShippingAddress,
                    AddressDelivery = model.ShippingAddress,
                    OrderDetails = cart.Select(item => new OrderDetail
                    {
                        ProductID = item.ProductID,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        TotalPrice = item.TotalPrice
                    }).ToList()
                };

                order.PaymentOnlyStatus = paymentStatus;
                order.DeliveryStatus = "Chưa giao hàng";
                order.ShippingMethod = string.IsNullOrWhiteSpace(model.ShippingMethod) ? "Giao hàng nhanh" : model.ShippingMethod;

                db.Orders.Add(order);

                // Tính lại số lượng tồn (Quantity) và số lượng đã bán (SoldQuantity) cho từng sản phẩm
                foreach (var item in cart)
                {
                    var product = db.Products.Find(item.ProductID);
                    if (product != null)
                    {
                        product.Quantity = Math.Max(0, product.Quantity - item.Quantity);
                        product.SoldQuantity += item.Quantity;
                    }
                }

                db.SaveChanges();

                Session["Cart"] = null;
                return RedirectToAction("OrderSuccess", new { id = order.OrderID });
            }
            model.CartItems = cart;
            return View(model);
        }

        public ActionResult OrderSuccess(int id)
        {
            var order = db.Orders
                .Include(o => o.Customer)
                .Include("OrderDetails.Product")
                .SingleOrDefault(o => o.OrderID == id);

            if (order == null)
            {
                return HttpNotFound();
            }

            return View(order);
        }

        public ActionResult CreateOrder()
        {
            return RedirectToAction("Checkout");
        }
    }
}
