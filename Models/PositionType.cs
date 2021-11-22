
using System.ComponentModel.DataAnnotations;

namespace Vehicle_Track.Models{
    public class PositionType {
        [Key]
        public int positionTypeId {get;set;}
        public string positionTypeName {get;set;}
    }//end of position type class
}//end of namespace