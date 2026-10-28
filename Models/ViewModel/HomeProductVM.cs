using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PagedList.Mvc;

namespace _25DH190107_NguyenMinhHuy.Models.ViewModel
{
    public class HomeProductVM
    {
        //Tiêu chí để search
        public string SearchTerm { get; set; }

        // Các thuộc tính hỗ trợ phân trang
        public int PageNumber { get; set; } // Trang hiện tại
        public int PageSize { get; set; } = 10; // Số sản phẩm mỗi trang

        //danh sách sản phẩm nổi bật
        public List<Product> FeaturedProducts { get; set; }

        // Danh sách sản phẩm mới đã phân trang
        public PagedList.IPagedList<Product> NewProducts { get; set; }
    }
}