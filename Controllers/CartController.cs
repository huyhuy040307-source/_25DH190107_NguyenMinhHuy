using _25DH190107_NguyenMinhHuy.Models;
using _25DH190107_NguyenMinhHuy.Models.ViewModel;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace _25DH190107_NguyenMinhHuy.Controllers
{
    public class CartController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        private CartService GetCartService()
        {
            return new CartService(Session);
        }

        // Hiển thị giỏ hàng
        public ActionResult Index()
        {
            var cart = GetCartService().GetCart();
            return View(cart);
        }

        // Hiển thị giỏ hàng đã gom nhóm sản phẩm theo danh mục
        public ActionResult Index2(int? page)
        {
            var cart = GetCartService().GetCart();
            var products = db.Products.ToList();
            var similarProducts = new List<Product>();

            if (cart.Items != null && cart.Items.Any())
            {
                similarProducts = products.Where(p => cart.Items.Any(ci => ci.Category == p.Category.CategoryName)
                    && !cart.Items.Any(ci => ci.ProductID == p.ProductID)).ToList();
            }

            int pageNumber = page ?? 1;
            int pageSize = cart.PageSize;

            cart.SimilarProducts = similarProducts.OrderBy(p => p.ProductID).ToPagedList(pageNumber, pageSize);
            return View(cart);
        }

        // Thêm sản phẩm vào giỏ (có kiểm tra số lượng tồn kho)
        public ActionResult AddToCart(int id, int quantity = 1)
        {
            var product = db.Products.Find(id);
            if (product != null)
            {
                if (quantity < 1) quantity = 1;
                var cart = GetCartService().GetCart();
                var existing = cart.Items.FirstOrDefault(i => i.ProductID == id);
                int currentInCart = existing != null ? existing.Quantity : 0;

                if (product.Quantity <= 0)
                {
                    TempData["CartError"] = string.Format("Sản phẩm \"{0}\" hiện đã hết hàng!", product.ProductName);
                    return RedirectToAction("Index");
                }

                if (currentInCart + quantity > product.Quantity)
                {
                    quantity = Math.Max(0, product.Quantity - currentInCart);
                    TempData["CartError"] = string.Format("Số lượng tồn kho của \"{0}\" chỉ còn {1} sản phẩm!", product.ProductName, product.Quantity);
                }

                if (quantity > 0)
                {
                    cart.AddItem(product.ProductID, product.ProductImage,
                        product.ProductName, product.ProductPrice, quantity,
                        product.Category != null ? product.Category.CategoryName : "");
                }
            }
            return RedirectToAction("Index");
        }

        // Xóa sản phẩm khỏi giỏ
        public ActionResult RemoveFromCart(int id)
        {
            var cartService = GetCartService();
            cartService.GetCart().RemoveItem(id);
            return RedirectToAction("Index");
        }

        // Làm trống giỏ hàng
        public ActionResult ClearCart()
        {
            GetCartService().ClearCart();
            return RedirectToAction("Index");
        }

        // Cập nhật số lượng (không cho nhập < 1 và > số lượng tồn)
        [HttpPost]
        public ActionResult UpdateQuantity(int id, int quantity)
        {
            if (quantity < 1)
            {
                TempData["CartError"] = "Số lượng sản phẩm phải lớn hơn hoặc bằng 1.";
                return RedirectToAction("Index");
            }

            var product = db.Products.Find(id);
            if (product != null && quantity > product.Quantity)
            {
                quantity = product.Quantity;
                TempData["CartError"] = string.Format("Số lượng mua không được vượt quá số lượng tồn kho ({0} sản phẩm)!", product.Quantity);
            }

            var cartService = GetCartService();
            cartService.GetCart().UpdateQuantity(id, quantity);
            return RedirectToAction("Index");
        }
    }
}
