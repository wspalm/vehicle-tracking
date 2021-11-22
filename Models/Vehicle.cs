using System.ComponentModel.DataAnnotations;

namespace Vehicle_Track.Models{
    public class Vehicle{
        [Key]
        public int vehicleId {get;set;}
        public string brandName {get;set;}
        public string series {get;set;}
        public string plate_number {get;set;}
        public bool is_active {get;set;}
        
    }//end of class
}//end of namespace