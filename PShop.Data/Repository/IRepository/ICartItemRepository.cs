using PShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PShop.Data.Repository.IRepository
{
    public interface ICartItemRepository : IRepository<CartItem>
    {
        int IncrementQuantity(CartItem shoppingCart, int count);
        int DecrementQuantity(CartItem shoppingCart, int count);
    }
}
