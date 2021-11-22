using Microsoft.AspNetCore.Identity;

namespace Vehicle_Track.Data{

    public class AppRole:IdentityRole<int>{
 
        public AppRole(string Name):base(Name){}
        
    }//end of class
}//end of namespace