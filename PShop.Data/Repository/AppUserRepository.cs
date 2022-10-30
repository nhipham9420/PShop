using PShop.Data.Repository.IRepository;
using PShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PShop.Data.Repository
{
    public class AppUserRepository : Repository<AppUser>, IAppUserRepository
    {
        private AppDbContext _dbcontext;

        public AppUserRepository(AppDbContext dbcontext) : base(dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public void Update(AppUser obj)
        {
            _dbcontext.AppUsers.Update(obj);
        }
    }
}

