using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using _25DH190107_NguyenMinhHuy.Models;
using _25DH190107_NguyenMinhHuy.Models.ViewModel;
using PagedList;

namespace _25DH190107_NguyenMinhHuy.Controllers
{
    public class HomeController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();
        public ActionResult Index(string searchTerm,int? page)
        {
            var model = new HomeProductVM();
            var products = db.Products.AsQueryable();

            // Lọc danh sách product theo từ khóa tìm kiếm 
            if (!string.IsNullOrEmpty(searchTerm))
            {
                model.SearchTerm = searchTerm;
                products = products.Where(p =>
                    p.ProductName.Contains(searchTerm) ||
                    p.ProductDescription.Contains(searchTerm) ||
                    p.Category.CategoryName.Contains(searchTerm));
            }

            // Đoạn code liên quan tới phân trang
            // Lấy số trang hiện tại (mặc định là trang 1 nếu không có giá trị)
            int pageNumber = page ?? 1;
            int pageSize = 6; // Số sản phẩm mỗi trang

            // Lấy top 10 sản phẩm bán chạy nhất
            model.FeaturedProducts = db.Products
                .OrderByDescending(p => p.OrderDetails.Count())
                .Take(10)
                .ToList();

            // Lấy 20 sản phẩm bán lẻ nhất và phân trang
            model.NewProducts = products
                .OrderBy(p => p.OrderDetails.Count())
                .Take(20)
                .ToPagedList(pageNumber, pageSize);

            return View(model);
        }
    }
}