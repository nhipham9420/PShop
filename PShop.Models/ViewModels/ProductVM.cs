using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using X.PagedList;

namespace PShop.Models.ViewModels
{
    public class ProductVM
    {
        public Product? Product { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> ListCategory { get; set; }


        public IPagedList<Product>? ListProducts { get; set; }
        public int? CategoryId { get; set; }
        public string? SearchString { get; set; }
        public int? SortBy { get; set; }

    }
}
