using Microsoft.EntityFrameworkCore;
using PShop.Data.Repository.IRepository;
using PShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PShop.Data.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private AppDbContext _dbcontext;

        public ProductRepository(AppDbContext dbcontext) : base(dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public void Update(Product obj)
        {
            var objFromDb = _dbcontext.Products.FirstOrDefault(u => u.Id == obj.Id);
            if (objFromDb != null)
            {
                objFromDb.Name = obj.Name;
                objFromDb.Price = obj.Price;
                objFromDb.Description = obj.Description;
                objFromDb.CategoryId = obj.CategoryId;
                if (obj.ImageUrl != null)
                {
                    objFromDb.ImageUrl = obj.ImageUrl;
                }
            }
        }
    }
}
