using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PShop.Data.Repository.IRepository;
using PShop.Models;
using PShop.Models.ViewModels;
using PShop.Utility;
using System.Diagnostics;
using System.Security.Claims;
using X.PagedList;

namespace PShopWeb.Controllers
{
    [Area("Customer")]

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Detail(int productId)
        {
            CartItem cartItemObj = new()
            {
                Quantity = 1,
                ProductId = productId,
                Product = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == productId, includeProperties: "Category"),
            };

            return View(cartItemObj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Detail(CartItem cartItem)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            cartItem.AppUserId = claim.Value;

            CartItem cartItemFromDb = _unitOfWork.CartItem.GetFirstOrDefault(
                u => u.AppUserId == claim.Value && u.ProductId == cartItem.ProductId);

            if (cartItemFromDb == null)
            {
                _unitOfWork.CartItem.Add(cartItem);
                _unitOfWork.Save();

                //use session
                HttpContext.Session.SetInt32(ValueStore.SessionCart,
                _unitOfWork.CartItem.GetAll(u => u.AppUserId == claim.Value).ToList().Count);
            }
            else
            {
                _unitOfWork.CartItem.IncrementQuantity(cartItemFromDb, cartItem.Quantity);
                _unitOfWork.Save();
            }

            return RedirectToAction(nameof(Index));
            //return View();
        }


        public IActionResult Index(int? page, string? searchString, int? categoryId, int? sortBy)
        {


            int pageSize = 6;
            int pageNumber = (page ?? 1);

            ViewBag.ListCategory = _unitOfWork.Category.GetAll();

            //ViewBag.ListSortBy = Enum.GetValues(typeof(SortEnums)).Cast<SortEnums>().Select(
            //                                            enu => new SelectListItem()
            //                                            {
            //                                                Text = enu.ToString(),
            //                                                Value = ((int)enu).ToString()
            //                                            });

            var model = new ProductVM()
            {
                ListProducts = _unitOfWork.Product.GetAll(includeProperties: "Category").ToPagedList(pageNumber, pageSize)
            };


            if (!String.IsNullOrEmpty(searchString))
            {
                model.ListProducts = (from p in model.ListProducts
                                      where p.Name.Contains(searchString)
                                      select p).ToPagedList(pageNumber, pageSize);
            }

            if (categoryId.HasValue)
            {
                model.ListProducts = (from p in model.ListProducts
                                      where p.CategoryId.Equals(categoryId)
                                      select p).ToPagedList(pageNumber, pageSize);
            }

            if (sortBy.HasValue)
            {
                switch (sortBy.Value)
                {
                    case 1:
                        model.ListProducts = model.ListProducts.OrderBy(p => p.Name).ToPagedList(pageNumber, pageSize);
                        break;
                    case 2:
                        model.ListProducts = model.ListProducts.OrderByDescending(p => p.Name).ToPagedList(pageNumber, pageSize);
                        break;
                    case 3:
                        model.ListProducts = model.ListProducts.OrderBy(p => p.Price).ToPagedList(pageNumber, pageSize);
                        break;
                    case 4:
                        model.ListProducts = model.ListProducts.OrderByDescending(p => p.Price).ToPagedList(pageNumber, pageSize);
                        break;
                    default:
                        model.ListProducts = model.ListProducts.OrderBy(p => p.Id).ToPagedList(pageNumber, pageSize);
                        break;
                }
            }

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorVM { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
