using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using _25DH190107_NguyenMinhHuy.Models;

namespace _25DH190107_NguyenMinhHuy.Areas.Admin.Controllers
{
    public class CategoryStatisticVM
    {
        public string CategoryName { get; set; }
        public int ProductCount { get; set; }
        public int TotalStock { get; set; }
        public decimal MaxPrice { get; set; }
        public decimal MinPrice { get; set; }
        public decimal AvgPrice { get; set; }
    }

    public class HomeController : BaseAdminController
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Admin/Home (Trang chủ Admin & Thống kê)
        public ActionResult Index()
        {
            ViewBag.TotalCategories = db.Categories.Count();
            ViewBag.TotalProducts = db.Products.Count();
            ViewBag.TotalCustomers = db.Customers.Count();
            ViewBag.TotalOrders = db.Orders.Count();
            ViewBag.TotalRevenue = db.Orders.Any() ? db.Orders.Sum(o => (decimal?)o.TotalAmount) ?? 0m : 0m;
            ViewBag.OutOfStockCount = db.Products.Count(p => p.Quantity <= 0);

            var stats = db.Categories
                .Include(c => c.Products)
                .ToList()
                .Select(c => new CategoryStatisticVM
                {
                    CategoryName = c.CategoryName,
                    ProductCount = c.Products != null ? c.Products.Count : 0,
                    TotalStock = c.Products != null && c.Products.Any() ? c.Products.Sum(p => p.Quantity) : 0,
                    MaxPrice = c.Products != null && c.Products.Any() ? c.Products.Max(p => p.ProductPrice) : 0m,
                    MinPrice = c.Products != null && c.Products.Any() ? c.Products.Min(p => p.ProductPrice) : 0m,
                    AvgPrice = c.Products != null && c.Products.Any() ? c.Products.Average(p => p.ProductPrice) : 0m
                })
                .ToList();

            return View(stats);
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
