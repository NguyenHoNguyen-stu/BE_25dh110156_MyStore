using _25DH110156_MyStore.Models;
using _25DH110651_MyStore.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace _25DH110651_MyStore.Controllers
{
    public class OrderController : Controller
    {
        [Authorize]
        public class OrderController : Controller
        {
            private MyStoreEntities db = new MyStoreEntities();

            public ActionResult Checkout()
            {
                var cart = Session["Cart"] as Cart;
                if (cart == null || !cart.Items.Any()) return RedirectToAction("Index", "Cart");

                var user = db.Users.SingleOrDefault(u => u.Username == User.Identity.Name);
                var customer = db.Customers.SingleOrDefault(c => c.Username == user.Username);

                var model = new CheckoutVM
                {
                    Cart = cart,
                    CustomerID = customer.CustomerID,
                    ShippingAddress = customer.CustomerAddress,
                    TotalAmount = cart.TotalValue()
                };
                return View(model);
            }

            [HttpPost]
            [ValidateAntiForgeryToken]
            public ActionResult Checkout(CheckoutVM model)
            {
                var cart = Session["Cart"] as Cart;
                if (cart == null || !cart.Items.Any()) return RedirectToAction("Index", "Home");

                if (ModelState.IsValid)
                {
                    var order = new Order
                    {
                        CustomerID = model.CustomerID,
                        OrderDate = DateTime.Now,
                        TotalAmount = cart.TotalValue(),
                        PaymentMethod = model.PaymentMethod,
                        DeliveryMethod = model.ShippingMethod,
                        ShippingAddress = model.ShippingAddress,
                        PaymentStatus = "Chưa thanh toán"
                    };

                    // Chuyển đổi trạng thái tự động theo hình thức thanh toán được chọn
                    if (model.PaymentMethod == "Tiền mặt") order.PaymentStatus = "Thanh toán tiền mặt";

                    foreach (var item in cart.Items)
                    {
                        order.OrderDetails.Add(new OrderDetail
                        {
                            ProductID = item.ProductID,
                            Quantity = item.Quantity,
                            UnitPrice = item.UnitPrice
                        });
                    }

                    db.Orders.Add(order);
                    db.SaveChanges();
                    Session["Cart"] = null; // Làm trống giỏ hàng

                    return RedirectToAction("OrderSuccess", new { id = order.OrderID });
                }
                model.Cart = cart;
                return View(model);
            }

            public ActionResult OrderSuccess(int id)
            {
                var order = db.Orders.Include("OrderDetails").SingleOrDefault(o => o.OrderID == id);
                if (order == null) return HttpNotFound();
                return View(order);
            }
        }
    }
}