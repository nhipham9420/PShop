using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PShop.Models.ViewModels
{
    public class OrderVM
    {
        public Order? Order { get; set; }
        public IEnumerable<OrderItem> ListOrderItem { get; set; }
    }
}
