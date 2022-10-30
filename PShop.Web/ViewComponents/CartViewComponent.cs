using Microsoft.AspNetCore.Mvc;
using PShop.Data.Repository.IRepository;
using PShop.Utility;
using System.Security.Claims;

namespace PShopWeb.ViewComponents
{
    public class CartViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;
        public CartViewComponent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null)
            {
                if (HttpContext.Session.GetInt32(ValueStore.SessionCart) != null)
                {
                    return View(HttpContext.Session.GetInt32(ValueStore.SessionCart));
                }
                else
                {
                    HttpContext.Session.SetInt32(ValueStore.SessionCart,
                        _unitOfWork.CartItem.GetAll(u => u.AppUserId == claim.Value).ToList().Count);
                    return View(HttpContext.Session.GetInt32(ValueStore.SessionCart));
                }
            }
            else
            {
                HttpContext.Session.Clear();
                return View(0);
            }
        }
    }

}
