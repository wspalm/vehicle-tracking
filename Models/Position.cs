
using System;
using System.ComponentModel.DataAnnotations;

namespace Vehicle_Track.Models{
    public class Position{
        [Key]
        public int positionId {get;set;}
        public string latitude {get;set;}
        public string longitude {get;set;}
        public DateTime date_time {get;set;}
        public int vehicleId {get;set;}
        public Vehicle vehicle {get;set;}
        public int positionTypeId {get;set;}
        public PositionType positionType {get;set;}
        public string route {get;set;}
    }//end of class
}//end of namespace