using _25DH190025_NguyenHuynhHongQuan.Models;
using _25DH190025_NguyenHuynhHongQuan.Models.ViewModel;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace _25DH190025_NguyenHuynhHongQuan.Controllers
{
    public class HomeController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Home/Index
        public ActionResult Index(string searchTerm, int? page)
        {
            var model = new HomeProductVM();
            // Chỉ hiển thị sản phẩm có số lượng tồn > 0 theo đúng Checklist
            var products = db.Products.Include(p => p.Category).Where(p => p.Quantity > 0).AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                model.SearchTerm = searchTerm;
                products = products.Where(p =>
                    p.ProductName.Contains(searchTerm) ||
                    p.ProductDescription.Contains(searchTerm) ||
                    p.Category.CategoryName.Contains(searchTerm));
            }

            int pageNumber = page ?? 1;
            int pageSize = 6;

            model.FeaturedProducts = products
                .OrderByDescending(p => p.OrderDetails.Count())
                .ThenByDescending(p => p.SoldQuantity)
                .Take(10)
                .ToList();

            model.NewProducts = products
                .OrderBy(p => p.OrderDetails.Count())
                .ThenByDescending(p => p.ProductID)
                .Take(20)
                .ToPagedList(pageNumber, pageSize);

            ViewBag.Categories = db.Categories.ToList();
            return View(model);
        }

        // GET: Home/ProductCategory (Trang danh mục & Lọc / Tìm kiếm / Phân trang sản phẩm cho Customer)
        public ActionResult ProductCategory(int? categoryId, string searchTerm, decimal? minPrice, decimal? maxPrice, string sortOrder, int? page)
        {
            var model = new ProductSearchVM();
            var products = db.Products.Include(p => p.Category).Where(p => p.Quantity > 0).AsQueryable();

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                products = products.Where(p => p.CategoryID == categoryId.Value);
                var cat = db.Categories.Find(categoryId.Value);
                ViewBag.SelectedCategoryName = cat != null ? cat.CategoryName : "";
                ViewBag.SelectedCategoryID = categoryId.Value;
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                model.SearchTerm = searchTerm.Trim();
                products = products.Where(p => p.ProductName.Contains(model.SearchTerm) ||
                                               p.ProductDescription.Contains(model.SearchTerm) ||
                                               p.Category.CategoryName.Contains(model.SearchTerm));
            }

            if (minPrice.HasValue)
            {
                model.MinPrice = minPrice.Value;
                products = products.Where(p => p.ProductPrice >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                model.MaxPrice = maxPrice.Value;
                products = products.Where(p => p.ProductPrice <= maxPrice.Value);
            }

            switch (sortOrder)
            {
                case "name_desc":
                    products = products.OrderByDescending(p => p.ProductName);
                    break;
                case "price_asc":
                    products = products.OrderBy(p => p.ProductPrice);
                    break;
                case "price_desc":
                    products = products.OrderByDescending(p => p.ProductPrice);
                    break;
                default:
                    products = products.OrderBy(p => p.ProductName);
                    break;
            }

            model.SortOrder = sortOrder;
            int pageNumber = page ?? 1;
            int pageSize = 8;
            model.Products = products.ToPagedList(pageNumber, pageSize);

            ViewBag.Categories = db.Categories.ToList();
            return View(model);
        }

        // GET: Home/ProductDetails/5
        public ActionResult ProductDetails(int? id, int? quantity, int? page)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product pro = db.Products.Include(p => p.Category).Include(p => p.OrderDetails).SingleOrDefault(p => p.ProductID == id);
            if (pro == null)
            {
                return HttpNotFound();
            }

            var products = db.Products
                .Where(p => p.CategoryID == pro.CategoryID && p.ProductID != pro.ProductID && p.Quantity > 0)
                .AsQueryable();

            ProductDetailsVM model = new ProductDetailsVM();

            int pageNumber = page ?? 1;
            int pageSize = model.PageSize;
            model.product = pro;
            model.RelatedProducts = products.OrderBy(p => p.ProductID).Take(8).ToList();
            model.TopProducts = products.OrderByDescending(p => p.OrderDetails.Count()).Take(8).ToPagedList(pageNumber, pageSize);

            if (quantity.HasValue)
            {
                int validQty = Math.Max(1, quantity.Value);
                if (pro.Quantity > 0 && validQty > pro.Quantity)
                {
                    validQty = pro.Quantity;
                }
                model.quantity = validQty;
            }
            model.estimatedValue = pro.ProductPrice * model.quantity;

            return View(model);
        }
    }
}
