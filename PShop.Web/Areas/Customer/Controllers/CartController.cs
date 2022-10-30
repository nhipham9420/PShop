using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using PShop.Data.Repository.IRepository;
using PShop.Models;
using PShop.Models.ViewModels;
using PShop.Utility;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;

namespace PShopWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IEmailSender _emailSender;


        [BindProperty]
        public CartVM CartVM { get; set; }


        public CartController(IUnitOfWork unitOfWork, IEmailSender emailSender)
        {
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
        }
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            CartVM = new CartVM()
            {
                ListCartItem = _unitOfWork.CartItem.GetAll(u => u.AppUserId == claim.Value,
                includeProperties: "Product"),
                Order = new()
            };
            foreach (var item in CartVM.ListCartItem)
            {
                item.Price = item.Product.Price;
                CartVM.Order.OrderTotal += (item.Price * item.Quantity);
            }
            return View(CartVM);
        }

        public IActionResult Plus(int cartId)
        {
            var cartItem = _unitOfWork.CartItem.GetFirstOrDefault(u => u.Id == cartId);
            _unitOfWork.CartItem.IncrementQuantity(cartItem, 1);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int cartId)
        {
            var cartItem = _unitOfWork.CartItem.GetFirstOrDefault(u => u.Id == cartId);
            if (cartItem.Quantity <= 1)
            {
                _unitOfWork.CartItem.Remove(cartItem);

                //use session
                var quantity = _unitOfWork.CartItem.GetAll(u => u.AppUserId == cartItem.AppUserId).ToList().Count - 1;
                HttpContext.Session.SetInt32(ValueStore.SessionCart, quantity);
            }
            else
            {
                _unitOfWork.CartItem.DecrementQuantity(cartItem, 1);
            }
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int cartId)
        {
            var cart = _unitOfWork.CartItem.GetFirstOrDefault(u => u.Id == cartId);
            _unitOfWork.CartItem.Remove(cart);
            _unitOfWork.Save();

            //use session
            var quantity = _unitOfWork.CartItem.GetAll(u => u.AppUserId == cart.AppUserId).ToList().Count;
            HttpContext.Session.SetInt32(ValueStore.SessionCart, quantity);

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Checkout()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            CartVM = new CartVM()
            {
                ListCartItem = _unitOfWork.CartItem.GetAll(u => u.AppUserId == claim.Value,
                includeProperties: "Product"),
                Order = new()
            };

            CartVM.Order.AppUser = _unitOfWork.AppUser.GetFirstOrDefault(
                u => u.Id == claim.Value);

            CartVM.Order.Name = CartVM.Order.AppUser.Name;
            CartVM.Order.PhoneNumber = CartVM.Order.AppUser.PhoneNumber;
            CartVM.Order.Address = CartVM.Order.AppUser.Address;

            foreach (var item in CartVM.ListCartItem)
            {
                item.Price = item.Product.Price;
                CartVM.Order.OrderTotal += (item.Price * item.Quantity);
            }
            return View(CartVM);
        }

        [HttpPost]
        [ActionName("Checkout")]
        //[ValidateAntiForgeryToken]
        public IActionResult CheckoutPOST(IFormCollection flagPayment)
        {
            string radiovalue = Convert.ToString(flagPayment["paymentMethod"]);

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            CartVM.ListCartItem = _unitOfWork.CartItem.GetAll(u => u.AppUserId == claim.Value,
                includeProperties: "Product");

            CartVM.Order.OrderStatus = ValueStore.StatusPending;
            CartVM.Order.PaymentStatus = ValueStore.PaymentStatusPending;

            CartVM.Order.OrderDate = System.DateTime.Now;
            CartVM.Order.AppUserId = claim.Value;


            foreach (var item in CartVM.ListCartItem)
            {
                item.Price = item.Product.Price;
                CartVM.Order.OrderTotal += (item.Price * item.Quantity);
            }

            _unitOfWork.Order.Add(CartVM.Order);
            _unitOfWork.Save();

            foreach (var cart in CartVM.ListCartItem)
            {
                OrderItem orderItem = new()
                {
                    ProductId = cart.ProductId,
                    OrderId = CartVM.Order.Id,
                    Price = cart.Price,
                    Quantity = cart.Quantity
                };
                _unitOfWork.OrderItem.Add(orderItem);
                _unitOfWork.Save();
            }

            if (String.Equals(radiovalue, "stripe"))
            {
                //stripe settings 
                var domain = "https://localhost:7153/";

                //using stripe.checkout
                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string>
                        {
                          "card",
                        },
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                    SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={CartVM.Order.Id}&flagPayment=stripe",
                    CancelUrl = domain + $"customer/cart/index",
                };

                foreach (var item in CartVM.ListCartItem)
                {

                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)item.Price,
                            Currency = "vnd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Product.Name
                            },
                        },
                        Quantity = item.Quantity,
                    };

                    options.LineItems.Add(sessionLineItem);

                }

                var sessionService = new SessionService();
                Session session = sessionService.Create(options);

                _unitOfWork.Order.UpdateStripePaymentID(CartVM.Order.Id, session.Id, session.PaymentIntentId);
                _unitOfWork.CartItem.RemoveRange(CartVM.ListCartItem);
                _unitOfWork.Save();

                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);
            }

            if (String.Equals(radiovalue, "cod"))
            {
                _unitOfWork.CartItem.RemoveRange(CartVM.ListCartItem);
                _unitOfWork.Save();

                return RedirectToAction("OrderConfirmation", "Cart", new { id = CartVM.Order.Id, flagPayment = "cod" });
            }

            return View();
        }


        public IActionResult OrderConfirmation(int id, string flagPayment)
        {
            Order order = _unitOfWork.Order.GetFirstOrDefault(u => u.Id == id, includeProperties: "AppUser");

            if (String.Equals(flagPayment, "cod"))
            {
                _unitOfWork.Order.UpdateStatus(order.Id, ValueStore.StatusApproved, ValueStore.PaymentStatusApproved);
            }

            if (String.Equals(flagPayment, "stripe"))
            {
                var service = new SessionService();
                Session session = service.Get(order.SessionId);

                //check stripe status
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.Order.UpdateStripePaymentID(order.Id, order.SessionId, session.PaymentIntentId);
                    _unitOfWork.Order.UpdateStatus(id, ValueStore.StatusApproved, ValueStore.PaymentStatusApproved);
                }
            }

            //clear session
            HttpContext.Session.Clear();

            _unitOfWork.Save();

            return View(id);
        }
    }
}
