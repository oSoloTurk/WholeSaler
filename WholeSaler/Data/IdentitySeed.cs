using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WholeSaler.Models;

namespace WholeSaler.Data
{
    public class IdentitySeed
    {

        public static async Task InitializeUsersAsync(UserManager<User> userManager, RoleManager<AppRole> roleManager)
        {
            var defaultUsers = new User[] { new User
            {
                UserName = "Hakki",
                Email = "g211210350@sakarya.edu.tr",
                Name = "Hakki",
                SurName = "Ceylan",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,                
            }, new User
            {
                UserName = "Mete",
                Email = "g191210053@sakarya.edu.tr",
                Name = "Mete",
                SurName = "Dokgöz",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
            } };
            foreach(var defaultUser in defaultUsers)
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "123");
                    await userManager.AddToRoleAsync(defaultUser, Enums.Roles.Customer.ToString());
                    await userManager.AddToRoleAsync(defaultUser, Enums.Roles.Employee.ToString());
                    await userManager.AddToRoleAsync(defaultUser, Enums.Roles.Admin.ToString());
                    await userManager.AddToRoleAsync(defaultUser, Enums.Roles.SuperAdmin.ToString());
                    }
            }
        }

        public static async Task InitializeRolesAsync(UserManager<User> userManager, RoleManager<AppRole> roleManager)
            {
                if(roleManager.Roles.Count() == 0)
                {
                await roleManager.CreateAsync(new AppRole(Enums.Roles.SuperAdmin.ToString()));
                await roleManager.CreateAsync(new AppRole(Enums.Roles.Admin.ToString()));
                await roleManager.CreateAsync(new AppRole(Enums.Roles.Employee.ToString()));
                await roleManager.CreateAsync(new AppRole(Enums.Roles.Customer.ToString()));
                }
            }

        internal async static void Initialize(IServiceProvider applicationServices)
        {
            using (var scope = applicationServices.CreateScope())
            {
                var provider = scope.ServiceProvider;
                var context = provider.GetRequiredService<ApplicationDbContext>();
                var userManager = provider.GetRequiredService<UserManager<User>>();
                var roleManager = provider.GetRequiredService<RoleManager<AppRole>>();

                // automigration 
                await InitializeRolesAsync(userManager, roleManager);
                await InitializeUsersAsync(userManager, roleManager);
            }
        }
    }
}
