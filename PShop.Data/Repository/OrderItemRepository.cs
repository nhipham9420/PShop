using PShop.Data.Repository.IRepository;
using PShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PShop.Data.Repository
{
    public class OrderItemRepository : Repository<OrderItem>, IOrderItemRepository
    {
        private AppDbContext _dbcontext;

        public OrderItemRepository(AppDbContext dbcontext) : base(dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public void Update(OrderItem obj)
        {
            _dbcontext.OrderItems.Update(obj);
        }
    }
}

