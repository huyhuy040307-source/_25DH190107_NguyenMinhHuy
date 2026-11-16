using _25DH190107_NguyenMinhHuy.Models;
using _25DH190107_NguyenMinhHuy.Models.ViewModel;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PagedList.Mvc;

namespace _25DH190107_NguyenMinhHuy.Areas.Admin.Controllers
{
    public class ProductsController : BaseAdminController
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Admin/Products
        public ActionResult Index(string searchTerm, decimal? minPrice, decimal? maxPrice, string sortOrder, int? page)
        {
            var model = new ProductSearchVM();
            var products = db.Products.Include(p => p.Category).AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                model.SearchTerm = searchTerm;
                products = products.Where(p => p.ProductName.Contains(searchTerm) ||
                                               p.ProductDescription.Contains(searchTerm) ||
                                               p.Category.CategoryName.Contains(searchTerm));
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
                case "name_asc":
                    products = products.OrderBy(p => p.ProductName);
                    break;
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
            int pageSize = 7;
            model.Products = products.ToPagedList(pageNumber, pageSize);

            return View(model);
        }

        // GET: Admin/Products/Inventory (Quản lý kho hàng: Hàng tồn > 0 và Hàng cần nhập = 0)
        public ActionResult Inventory()
        {
            var allProducts = db.Products.Include(p => p.Category).OrderBy(p => p.Quantity).ToList();
            ViewBag.InStockProducts = allProducts.Where(p => p.Quantity > 0).ToList();
            ViewBag.OutOfStockProducts = allProducts.Where(p => p.Quantity <= 0).ToList();
            return View(allProducts);
        }

        // POST: Admin/Products/UpdateStock
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStock(int productId, int quantity)
        {
            var prod = db.Products.Find(productId);
            if (prod != null && quantity >= 0)
            {
                prod.Quantity = quantity;
                db.SaveChanges();
                TempData["SuccessMessage"] = string.Format("Đã cập nhật số lượng tồn kho của \"{0}\" thành {1}.", prod.ProductName, quantity);
            }
            return RedirectToAction("Inventory");
        }

        // GET: Admin/Products/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Include(p => p.Category).Include(p => p.OrderDetails).SingleOrDefault(p => p.ProductID == id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // GET: Admin/Products/Create
        public ActionResult Create()
        {
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName");
            return View(new Product { Quantity = 50, SoldQuantity = 0 });
        }

        // POST: Admin/Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ProductID,CategoryID,ProductName,ProductDescription,ProductPrice,ProductImage,Quantity,SoldQuantity,UploadImage")] Product product)
        {
            if (product.UploadImage != null && product.UploadImage.ContentLength > 0)
            {
                string fileName = Path.GetFileName(product.UploadImage.FileName);
                string uploadDir = Server.MapPath("~/Images");
                if (!Directory.Exists(uploadDir))
                {
                    Directory.CreateDirectory(uploadDir);
                }
                string filePath = Path.Combine(uploadDir, fileName);
                product.UploadImage.SaveAs(filePath);
                product.ProductImage = "/Images/" + fileName;
            }

            if (ModelState.IsValid)
            {
                db.Products.Add(product);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Thêm sản phẩm mới thành công!";
                return RedirectToAction("Index");
            }

            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        // GET: Admin/Products/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        // POST: Admin/Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ProductID,CategoryID,ProductName,ProductDescription,ProductPrice,ProductImage,Quantity,SoldQuantity,UploadImage")] Product product)
        {
            if (product.UploadImage != null && product.UploadImage.ContentLength > 0)
            {
                string fileName = Path.GetFileName(product.UploadImage.FileName);
                string uploadDir = Server.MapPath("~/Images");
                if (!Directory.Exists(uploadDir))
                {
                    Directory.CreateDirectory(uploadDir);
                }
                string filePath = Path.Combine(uploadDir, fileName);
                product.UploadImage.SaveAs(filePath);
                product.ProductImage = "/Images/" + fileName;
            }

            if (ModelState.IsValid)
            {
                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
                TempData["SuccessMessage"] = "Cập nhật sản phẩm thành công!";
                return RedirectToAction("Index");
            }
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        // GET: Admin/Products/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Include(p => p.Category).Include(p => p.OrderDetails).SingleOrDefault(p => p.ProductID == id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // POST: Admin/Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Products.Include(p => p.OrderDetails).SingleOrDefault(p => p.ProductID == id);
            if (product == null)
            {
                return HttpNotFound();
            }

            // Kiểm tra ràng buộc khóa ngoại với bảng OrderDetail
            if (product.OrderDetails != null && product.OrderDetails.Any())
            {
                TempData["Error"] = string.Format("Không thể xóa sản phẩm \"{0}\" vì sản phẩm này đã phát sinh trong {1} chi tiết đơn hàng!", product.ProductName, product.OrderDetails.Count);
                return RedirectToAction("Index");
            }

            db.Products.Remove(product);
            db.SaveChanges();
            TempData["SuccessMessage"] = "Đã xóa sản phẩm thành công!";
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
