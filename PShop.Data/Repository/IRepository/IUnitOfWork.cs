using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PShop.Data.Repository.IRepository
{
    public interface IUnitOfWork
    {
        ICategoryRepository Category { get; }
        IProductRepository Product { get; }
        ICartItemRepository CartItem { get; }
        IAppUserRepository AppUser { get; }
        IOrderItemRepository OrderItem { get; }
        IOrderRepository Order { get; }

        void Save();
    }
}
