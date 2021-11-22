using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Vehicle_Track.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Vehicle_Track.Models;

namespace Vehicle_Track.Controllers{
    [Route("api/[action]")]
    public class AccountController : Controller{
        public IConfiguration _configuration;
        private UserManager<AppUser> _userManager;
        private RoleManager<AppRole> _roleManager;
        private readonly Vehicle_TrackDbContext _db;
        public AccountController(
            IConfiguration configuration,
            Vehicle_TrackDbContext db,
            UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager
        ){
            _configuration = configuration;
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
        }//end of contructor class

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] UserData model1){
            //if every is not null , do authenticate()
            if(model1 != null && model1.username != null && model1.password != null){
                return await authenticate(model1);
            }//end of if
            else {
                return BadRequest(new{
                    message = "Missing some username or password"
                });
            }//end of else
        }//end of login function

        //create authenticate function
        private async Task<IActionResult> authenticate(UserData model){
              var user = await _userManager.FindByNameAsync(model.username);  
              
            if (user != null && await _userManager.CheckPasswordAsync(user, model.password))  
            {  
                var userRoles = await _userManager.GetRolesAsync(user);  
   
                var authClaims = new List<Claim>  
                {  
                    new Claim(ClaimTypes.Name, user.UserName),  
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),  
                };  
  
                foreach (var userRole in userRoles)  
                {  
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));  
                }//end of foreach  
  
                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));  
  
                var token = new JwtSecurityToken(  
                    issuer: _configuration["JWT:Issuer"],  
                    audience: _configuration["JWT:Audience"],  
                    expires: DateTime.Now.AddHours(1),  
                    claims: authClaims,  
                    signingCredentials: 
                    new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)  
                    );  
  
                return Ok(new  
                {  
                    roles = userRoles,
                    token = new JwtSecurityTokenHandler().WriteToken(token),  
                    expiration = token.ValidTo  
                });  
            }
            else{
                return Unauthorized(new{
                    message = "Incorrect username or password"
                });
            }//end of else 
        }//end of function

        

    }//end of controller
}//end of namespace