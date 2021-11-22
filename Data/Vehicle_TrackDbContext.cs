using Vehicle_Track.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace Vehicle_Track.Data{
    public class Vehicle_TrackDbContext : IdentityDbContext<AppUser,AppRole,int>{
        //build contructor function
        public Vehicle_TrackDbContext(DbContextOptions<Vehicle_TrackDbContext> options):base(options){

        }//end of contructor function
        //create onmodel creating function
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
        public DbSet<AppUser> AppUsers {get;set;}
        public DbSet<Vehicle> Vehicles {get;set;}
        public DbSet<Position> Positions {get;set;}
        public DbSet<PositionType> PositionTypes {get;set;}

    }//end of class
}//end of namespace