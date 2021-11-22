using Microsoft.AspNetCore.Identity;

namespace Vehicle_Track.Data{

    public class AppUser:IdentityUser<int>{

        public string first_name {get;set;}
        public string last_name {get;set;}       
        
    }//end of class
}//end of namespace