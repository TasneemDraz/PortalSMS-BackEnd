using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalSMS.DAL.Data.DataSeeding
{
    public static class SeedData
    {
        public static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            var roleNames = new[] { "Admin", "Sender", "Viewer" };

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }
    }
}
