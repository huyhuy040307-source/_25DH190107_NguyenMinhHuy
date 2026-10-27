using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using PagedList.Mvc;

namespace _25DH190107_NguyenMinhHuy.Models.ViewModel
{
    public class ProductSearchVM
    {
        public string SearchTerm { get; set; }

        //các tiêu chí để search theo giá
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        //thứ tự sắp xếp
        public string SortOrder { get; set; }

        //các thuộc tính hỗ trợ phân trang
        public int PageNumber { get; set; }
        public int PageSize { get; set; } = 10;

        //danh sách sản phẩm đã phân trang
        public PagedList.IPagedList<Product> Products { get; set; }
        //danh sách sản phẩm thỏa điều kiện tiềm kiếm
        //public List<Product> Products { get; set; }
    }
}