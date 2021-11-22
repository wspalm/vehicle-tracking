
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vehicle_Track.Data;
using Vehicle_Track.Models;

namespace Vehicle_Track
{
    //define routing
    [Route("api/[action]")]
    public class ApiController : Controller
    {
        private Random random = new Random();
        //initiate field object that will be used as DbContext Referencing
        private Vehicle_TrackDbContext _db;
        //create contructor function and do dependency(ies) injection
        public ApiController(Vehicle_TrackDbContext db)
        {
            _db = db;
        }//end of contructor function

        [HttpGet]
        public IActionResult helloworld()
        {
            return Json(new
            {
                status_code = 200,
                message = "hello world"
            });
        }//end of helloworld function

        [HttpGet]
        public IActionResult greeting(){
            return Json(new{
                name = "wongsathorn",
                surname = "sereephap"
            });
        }//end of function

        [HttpPost]
        public IActionResult hi([FromBody] Name input1 ){
            return Json(new{
                message = "hello " + input1.first_name.ToString(),
                message2 = "family name " + input1.last_name.ToString(),
            });
            
        }//end of function

        [HttpPost]
        public async Task<IActionResult> register_vehicle([FromBody] VehicleInput _vehicle)
        {
            try
            {
                //search for plate number input to prevent duplicate car(plate number)
                Vehicle v1 = _db.Vehicles.FirstOrDefault(
                   v => v.plate_number == _vehicle.plate_number
                );
                if (v1 != null)
                {
                    return Unauthorized(new
                    {
                        message = "This vehicle already exist"
                    });
                }
                //map dto input object into Database model
                Vehicle vehicle1 = new Vehicle()
                {
                    brandName = _vehicle.brandName,
                    series = _vehicle.series,
                    plate_number = _vehicle.plate_number,
                    is_active = false,
                };
                await _db.Vehicles.AddAsync(vehicle1);
                await _db.SaveChangesAsync();
                return Ok(vehicle1);
            }//end of try
            catch
            {
                return BadRequest();
            }//end of catch     
        }//end of register vehicle

        [HttpPost]
        public async Task<IActionResult> record_position([FromBody] PositionInput input1)
        {
            //query for vehicle
            Vehicle _vehicle = await _db.Vehicles.FirstOrDefaultAsync(
                v => v.plate_number == input1.plate_number
            );
            //check if this vehicle exist or not
            if (_vehicle == null)
            {
                return NotFound(new
                {
                    message = "Vehicle not found, please input correct registered plate number"
                });
            }//end of if            

            //query for position type
            PositionType posType = await _db.PositionTypes.FirstOrDefaultAsync(
                p => p.positionTypeId == input1.positionTypeId
            );
            //check if the position exist or not
            if (posType == null)
            {
                return NotFound(new
                {
                    message = "Position not found, please input correct positionId as documented"
                });
            }//end of if

            //handle the situation that vehicle will be taken twice
            if (_vehicle.is_active && posType.positionTypeId == 1)
            {
                return Unauthorized(new
                {
                    message = "This vehicle already been taken"
                });
            }//end of if

            //vehicle available and being taken out
            if (!_vehicle.is_active && posType.positionTypeId == 1)
            {
                string _route = generate_route(32);
 
                Position _position = new Position()
                {
                    latitude = input1.latitude,
                    longitude = input1.longitude,
                    date_time = input1.date_time,
                    vehicle = _vehicle,
                    positionType = posType,
                    route = _route
                };
                //make this vehicle active
                _vehicle.is_active = true;
                await _db.Positions.AddAsync(_position);
                await _db.SaveChangesAsync();
                return Ok(new
                {
                    message = "Journey started, the vehicle is now on action"
                });
            }//end of if

            //vehicle got updated on the way
            if (_vehicle.is_active && posType.positionTypeId == 2)
            {
                //get route
                Position pos1 = await _db.Positions
                .OrderBy( o => o.date_time)
                .LastOrDefaultAsync(
                    p => p.vehicleId == _vehicle.vehicleId
                );
                //record position, map input DTO into database model
                Position _position = new Position()
                {
                    latitude = input1.latitude,
                    longitude = input1.longitude,
                    date_time = input1.date_time,
                    vehicle = _vehicle,
                    positionType = posType,
                    route = pos1.route
                };
                //make this vehicle active
                _vehicle.is_active = true;
                await _db.Positions.AddAsync(_position);
                await _db.SaveChangesAsync();
                return Ok(new
                {
                    message = "Position recored successfully"
                });
            }//end of if

            //situation that vehicle arrive at the destination
            if (_vehicle.is_active && posType.positionTypeId == 3)
            {
                //get route
                Position pos1 = await _db.Positions
                .OrderBy( o => o.date_time)
                .LastOrDefaultAsync(
                    p => p.vehicleId == _vehicle.vehicleId
                );
                //record position, map input DTO into database model
                Position _position = new Position()
                {
                    latitude = input1.latitude,
                    longitude = input1.longitude,
                    date_time = input1.date_time,
                    vehicle = _vehicle,
                    positionType = posType,
                    route = pos1.route
                };
                //make this vehicle active
                _vehicle.is_active = false;
                await _db.Positions.AddAsync(_position);
                await _db.SaveChangesAsync();
                return Ok(new
                {
                    message = "Position recored successfully, vehicle arrived at destination"
                });
            }//end of if

            //situation that vehicle is not yet activate 
            //but got assigned with wrong position type

            if (!_vehicle.is_active && posType.positionTypeId == 3)
            {
                return Unauthorized(new
                {
                    message = "Vehicle must be taken out first before arriving at destination"
                });
            }//end of if

            if (!_vehicle.is_active && posType.positionTypeId == 2)
            {
                Position _position = new Position()
                {
                    latitude = input1.latitude,
                    longitude = input1.longitude,
                    date_time = input1.date_time,
                    vehicle = _vehicle,
                    positionType = posType
                };
                //make this vehicle active
                _vehicle.is_active = true;
                await _db.Positions.AddAsync(_position);
                await _db.SaveChangesAsync();
                return Ok(new
                {
                    warning = "Vehicle has been taken out without start position"
                });

            }//end of if

            return BadRequest(new
            {
                message = "Bad Request, please try again"
            });

        }//end of record position function


        //#-+=_!?&*
        string _random(int input1)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789#-+=_!?&*";
            return new string(Enumerable.Repeat(chars, input1)
                        .Select(s => s[random.Next(s.Length)]).ToArray());
        }//end of _random function

        //make recursive function to generate
        //and to make sure that there won't be duplicate routing
        //if it get repeated for more than 5 times, it means that
        //there might not be possible way to generate more
        //so route_count is there to stand and increment every time function got reapeated
        //use that one to plus and regenerate again in different length
        int route_count = 0;
        string generate_route(int input1)
        {
            string routing = _random(input1);
            Position p1 = _db.Positions.FirstOrDefault(
                p => p.route == routing
            );
            if (p1 != null)
            {
                route_count++;
                if (route_count > 5)
                {
                    generate_route(32 + route_count);
                }//end of if
                generate_route(32);
            }//end of if
            return routing;
        }//end of recursive function

    }//end of class
}//end of namespace