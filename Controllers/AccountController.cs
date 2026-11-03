using _25DH110156_MyStore.Models;
using _25DH110651_MyStore.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace _25DH110651_MyStore.Controllers
{
    public class AccountController : Controller
    {
        // Khởi tạo đối tượng kết nối cơ sở dữ liệu Entity Framework
        private MyStoreEntities db = new MyStoreEntities();

        // ==========================================
        // 1. CHỨC NĂNG ĐĂNG KÝ TÀI KHOẢN
        // ==========================================

        // GET: Account/Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterVM model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra xem tên đăng nhập đã tồn tại trong bảng User chưa
                var existingUser = db.Users.SingleOrDefault(u => u.Username == model.Username);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập này đã tồn tại!");
                    return View(model);
                }

                // Nếu chưa tồn tại thì tạo bản ghi thông tin tài khoản trong bảng User
                var user = new User
                {
                    Username = model.Username,
                    Password = model.Password, // Lưu ý: Nên mã hóa mật khẩu trước khi lưu nếu cần bảo mật nâng cao
                    UserRole = "Customer"
                };
                db.Users.Add(user);

                // Đồng thời tạo bản ghi thông tin khách hàng trong bảng Customer liên kết qua Username
                var customer = new Customer
                {
                    CustomerName = model.CustomerName,
                    CustomerEmail = model.CustomerEmail,
                    CustomerPhone = model.CustomerPhone,
                    CustomerAddress = model.CustomerAddress,
                    Username = model.Username
                };
                db.Customers.Add(customer);

                // Lưu thông tin cả 2 bảng vào cơ sở dữ liệu
                db.SaveChanges();

                // Đăng ký thành công, chuyển hướng về trang Đăng nhập
                return RedirectToAction("Login", "Account");
            }

            return View(model);
        }

        // ==========================================
        // 2. CHỨC NĂNG ĐĂNG NHẬP TÀI KHOẢN
        // ==========================================

        // GET: Account/Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginVM model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra tài khoản đăng nhập có khớp thông tin trong bảng User không
                var user = db.Users.SingleOrDefault(u => u.Username == model.Username
                                                    && u.Password == model.Password
                                                    && u.UserRole == "Customer");
                if (user != null)
                {
                    // Lưu trạng thái đăng nhập vào session hệ thống
                    Session["Username"] = user.Username;
                    Session["UserRole"] = user.UserRole;

                    // Lưu thông tin xác thực người dùng vào cookie thiết bị
                    FormsAuthentication.SetAuthCookie(user.Username, false);

                    // Chuyển hướng về trang chủ sau khi đăng nhập thành công
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    // Nếu sai thông tin thì báo lỗi ra giao diện chính
                    ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
                }
            }

            return View(model);
        }

        // ==========================================
        // 3. CHỨC NĂNG ĐĂNG XUẤT TÀI KHOẢN
        // ==========================================

        // GET: Account/Logout
        public ActionResult Logout()
        {
            // Xóa toàn bộ Session và đăng xuất Cookie Authentication
            Session.Clear();
            FormsAuthentication.SignOut();

            // Quay về trang chủ ngoài root sau khi đăng xuất
            return RedirectToAction("Index", "Home");
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