using PShop.Data.Repository.IRepository;
using PShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PShop.Data.Repository
{
    public class CartItemRepository : Repository<CartItem>, ICartItemRepository
    {
        private AppDbContext _dbcontext;

        public CartItemRepository(AppDbContext dbcontext) : base(dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public int DecrementQuantity(CartItem shoppingCart, int quantity)
        {
            shoppingCart.Quantity -= quantity;
            return shoppingCart.Quantity;
        }

        public int IncrementQuantity(CartItem shoppingCart, int quantity)
        {
            shoppingCart.Quantity += quantity;
            return shoppingCart.Quantity;
        }
    }
}

