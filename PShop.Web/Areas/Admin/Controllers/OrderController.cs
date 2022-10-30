using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PShop.Data.Repository.IRepository;
using PShop.Models;
using PShop.Models.ViewModels;
using PShop.Utility;
using Stripe;
using System.Security.Claims;

namespace PShopWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public OrderVM OrderVM { get; set; }
        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [Route("/{controller}")]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Detail(int orderId)
        {
            OrderVM = new OrderVM()
            {
                Order = _unitOfWork.Order.GetFirstOrDefault(u => u.Id == orderId, includeProperties: "AppUser"),
                ListOrderItem = _unitOfWork.OrderItem.GetAll(u => u.OrderId == orderId, includeProperties: "Product"),
            };
            return View(OrderVM);
        }

        [HttpPost]
        [Authorize(Roles = ValueStore.RoleAdmin + "," + ValueStore.RoleManager)] //or
        [ValidateAntiForgeryToken]
        public IActionResult UpdateOrderDetail()
        {
            var orderFromDb = _unitOfWork.Order.GetFirstOrDefault(u => u.Id == OrderVM.Order.Id, tracked: false);
            orderFromDb.Name = OrderVM.Order.Name;
            orderFromDb.PhoneNumber = OrderVM.Order.PhoneNumber;
            orderFromDb.Address = OrderVM.Order.Address;

            _unitOfWork.Order.Update(orderFromDb);
            _unitOfWork.Save();
            TempData["Success"] = "Order Details Updated Successfully.";
            return RedirectToAction("Detail", "Order", new { orderId = orderFromDb.Id });
        }

        [HttpPost]
        [Authorize(Roles = ValueStore.RoleAdmin + "," + ValueStore.RoleManager)]
        [ValidateAntiForgeryToken]
        public IActionResult StartProcessing()
        {
            _unitOfWork.Order.UpdateStatus(OrderVM.Order.Id, ValueStore.StatusInProcess);
            _unitOfWork.Save();
            TempData["Success"] = "Order Status Updated Successfully.";
            return RedirectToAction("Detail", "Order", new { orderId = OrderVM.Order.Id });
        }

        [HttpPost]
        [Authorize(Roles = ValueStore.RoleAdmin + "," + ValueStore.RoleManager)]
        [ValidateAntiForgeryToken]
        public IActionResult ShipOrder()
        {
            var order = _unitOfWork.Order.GetFirstOrDefault(u => u.Id == OrderVM.Order.Id, tracked: false);
            order.OrderStatus = ValueStore.StatusShipped;
            order.ShippingDate = DateTime.Now;
            _unitOfWork.Order.Update(order);
            _unitOfWork.Save();
            TempData["Success"] = "Order Shipped Successfully.";
            return RedirectToAction("Detail", "Order", new { orderId = OrderVM.Order.Id });
        }

        [HttpPost]
        [Authorize(Roles = ValueStore.RoleAdmin + "," + ValueStore.RoleManager)]
        [ValidateAntiForgeryToken]
        public IActionResult CancelOrder()
        {
            var order = _unitOfWork.Order.GetFirstOrDefault(u => u.Id == OrderVM.Order.Id, tracked: false);

            if (order.PaymentIntentId != null)
            {
                if (order.PaymentStatus == ValueStore.PaymentStatusApproved)
                {
                    var options = new RefundCreateOptions
                    {
                        Reason = RefundReasons.RequestedByCustomer,
                        PaymentIntent = order.PaymentIntentId
                    };

                    var service = new RefundService();
                    Refund refund = service.Create(options);

                    _unitOfWork.Order.UpdateStatus(order.Id, ValueStore.StatusCancelled, ValueStore.StatusRefunded);
                }

            }
            else
            {
                _unitOfWork.Order.UpdateStatus(order.Id, ValueStore.StatusCancelled, ValueStore.StatusCancelled);
            }

            _unitOfWork.Save();

            TempData["Success"] = "Order Cancelled Successfully.";
            return RedirectToAction("Detail", "Order", new { orderId = OrderVM.Order.Id });
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<Order> listOrders;

            if (User.IsInRole(ValueStore.RoleAdmin) || User.IsInRole(ValueStore.RoleManager))
            {
                listOrders = _unitOfWork.Order.GetAll(includeProperties: "AppUser");
            }
            else
            {
                ////bug: order success --> create new row with new id in table aspnetuser
                //var claimsIdentity = (ClaimsIdentity)User.Identity;
                //var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                //listOrders = _unitOfWork.Order.GetAll(u=>u.AppUserId==claim.Value,includeProperties: "AppUser");

                var currentUserEmail = User.FindFirstValue(ClaimTypes.Email);
                listOrders = _unitOfWork.Order.GetAll(u => u.AppUser.Email == currentUserEmail, includeProperties: "AppUser");
            }

            switch (status)
            {
                case "pending":
                    listOrders = listOrders.Where(u => u.PaymentStatus == ValueStore.PaymentStatusPending);
                    break;
                case "inprocess":
                    listOrders = listOrders.Where(u => u.OrderStatus == ValueStore.StatusInProcess);
                    break;
                case "completed":
                    listOrders = listOrders.Where(u => u.OrderStatus == ValueStore.StatusShipped);
                    break;
                case "approved":
                    listOrders = listOrders.Where(u => u.OrderStatus == ValueStore.StatusApproved);
                    break;
                default:
                    break;
            }


            return Json(new { data = listOrders });
        }
        #endregion
    }
}
