using PShop.Data.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PShop.Data.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private AppDbContext _dbcontext;

        public UnitOfWork(AppDbContext dbcontext)
        {
            _dbcontext = dbcontext;
            Category = new CategoryRepository(_dbcontext);
            Product = new ProductRepository(_dbcontext);
            AppUser = new AppUserRepository(_dbcontext);
            CartItem = new CartItemRepository(_dbcontext);
            Order = new OrderRepository(_dbcontext);
            OrderItem = new OrderItemRepository(_dbcontext);

        }
        public ICategoryRepository Category { get; private set; }
        public IProductRepository Product { get; private set; }
        public ICartItemRepository CartItem { get; private set; }
        public IAppUserRepository AppUser { get; private set; }
        public IOrderRepository Order { get; private set; }
        public IOrderItemRepository OrderItem { get; private set; }

        public void Save()
        {
            _dbcontext.SaveChanges();
        }
    }
}
