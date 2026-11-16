namespace _25DH190107_NguyenMinhHuy.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Web;

    public partial class Product
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Product()
        {
            this.OrderDetails = new HashSet<OrderDetail>();
            this.Quantity = 50;
            this.SoldQuantity = 0;
        }

        public int ProductID { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục sản phẩm.")]
        [Display(Name = "Danh mục")]
        public int CategoryID { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm không được để trống.")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Tên sản phẩm phải từ 2 đến 200 ký tự.")]
        [Display(Name = "Tên sản phẩm")]
        public string ProductName { get; set; }

        [Required(ErrorMessage = "Mô tả sản phẩm không được để trống.")]
        [StringLength(2000, MinimumLength = 5, ErrorMessage = "Mô tả sản phẩm phải từ 5 đến 2000 ký tự.")]
        [Display(Name = "Mô tả")]
        public string ProductDescription { get; set; }

        [Required(ErrorMessage = "Đơn giá không được để trống.")]
        [Range(0.01, 1000000000, ErrorMessage = "Đơn giá phải lớn hơn 0.")]
        [DataType(DataType.Currency)]
        [Display(Name = "Đơn giá")]
        public decimal ProductPrice { get; set; }

        [Display(Name = "Hình ảnh")]
        public string ProductImage { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số lượng tồn kho.")]
        [Range(0, 1000000, ErrorMessage = "Số lượng tồn phải từ 0 trở lên.")]
        [Display(Name = "Số lượng tồn")]
        public int Quantity { get; set; }

        [Display(Name = "Số lượng đã bán")]
        public int SoldQuantity { get; set; }

        [NotMapped]
        [Display(Name = "Tải ảnh lên")]
        public HttpPostedFileBase UploadImage { get; set; }

        public virtual Category Category { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
