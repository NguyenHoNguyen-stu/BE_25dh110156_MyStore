using _25DH110156_MyStore.Models;
using _25DH110651_MyStore.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace _25DH110156_MyStore.Controllers
{
    public class HomeController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        public ActionResult Index(string searchTerm, int? page)
        {
            var model = new HomeProductVM();
            var products = db.Products.AsQueryable();

            // Tìm kiếm sản phẩm
            if (!string.IsNullOrEmpty(searchTerm))
            {
                model.SearchTerm = searchTerm;
                products = products.Where(p => p.ProductName.Contains(searchTerm) ||
                                               p.ProductDescription.Contains(searchTerm) ||
                                               p.Category.CategoryName.Contains(searchTerm));
            }

            int pageNumber = page ?? 1;
            int pageSize = 6;

            // 1. Lấy top 10 sản phẩm bán chạy nhất (Featured)
            model.FeaturedProducts = products.OrderByDescending(p => p.OrderDetails.Count())
                                             .Take(10).ToList();

            // 2. Lấy 20 sản phẩm mới nhất (hoặc ít người mua nhất tùy theo yêu cầu cụ thể) và phân trang
            model.NewProducts = products.OrderBy(p => p.OrderDetails.Count()) // Hoặc OrderByDescending theo ngày
                                        .Take(20)
                                        .ToPagedList(pageNumber, pageSize);

            return View(model);
        }
    }
}