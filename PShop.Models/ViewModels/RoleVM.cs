using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PShop.Models.ViewModels
{
    public class RoleVM
    {
        public IEnumerable<IdentityRole> ListRoles { get; set; }
        public IdentityRole Role { get; set; }
    }
}
