using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;

namespace Vehicle_Track.Data{
    public class SeedUserAccount{
        public static async Task go(UserManager<AppUser> _userManager , 
        RoleManager<AppRole> _roleManager){

            //craete role of user
            if(!await _roleManager.RoleExistsAsync("admin")){
                await _roleManager.CreateAsync(new AppRole("admin"));
            }//end of if
            

            AppUser superUser = new AppUser{
                UserName = "root@localhost.com",
                Email    = "root@localhost.com",
                first_name = "super",
                last_name = "iamadmin"
            };

            //make query of existing user
            if(_userManager.Users.All(u => u.UserName != superUser.UserName)){
                await _userManager.CreateAsync(superUser,"admin1234");
                Console.WriteLine("Admin User account got Created ++++ ");
            }//end of if

            superUser = await _userManager.FindByEmailAsync("root@localhost.com");
            

            //insert role for super user
            if(!await _userManager.IsInRoleAsync(superUser,"admin")){
                await _userManager.AddToRoleAsync(superUser,"admin");
                Console.WriteLine("Apply admin role to root -- ");
            }//end of if
            else{
                Console.WriteLine("admin user exist ===");
            }//end of else

 

        }//end of go function
    }//end of class
}//end of namespace