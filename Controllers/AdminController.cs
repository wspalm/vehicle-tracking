using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vehicle_Track.Data;
using Vehicle_Track.Models;

namespace Vehicle_Track.Controllers
{
    [Authorize(AuthenticationSchemes =
    JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[action]")]
    public class AdminController : Controller
    {
        //begin database inpendency injection
        private Vehicle_TrackDbContext _db;
        private UserManager<AppUser> _usermanager;
        private RoleManager<AppRole> _rolemanager;
        public AdminController(Vehicle_TrackDbContext db, UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
        {
            _db = db;
            _usermanager = userManager;
            _rolemanager = roleManager;
        }//end of contructor function 
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterData model)
        {
            var userExists = await _usermanager.FindByNameAsync(model.userName);
            if (userExists != null)
                return StatusCode(StatusCodes.Status500InternalServerError,
                new { Status = "Error", Message = "User already exists!" });

            AppUser user = new AppUser()
            {
                Email = model.email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.userName,
                first_name = model.first_name,
                last_name = model.last_name,
            };
            var result = await _usermanager.CreateAsync(user, model.password);
            //make new user become normal user
            await _usermanager.AddToRoleAsync(user, "admin");
            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError,
                new { status = "Error", message = "User creation failed! Please check user details and try again." });

            return Ok(new
            {
                message = "User created successfully"
            });
        }//end of function
        [HttpGet]
        public IActionResult hello()
        {
            return Ok(new
            {
                message = "Hello Admin !!"
            });
        }//end of function hello admin

        [HttpPost]
        public async Task<IActionResult> add_position_type([FromBody] PositionTypeInput input1)
        {
            //query to check if this name already exist
            PositionType check = await _db.PositionTypes.FirstOrDefaultAsync(
                c => c.positionTypeName == input1.positionTypeName
            );
            if (check != null)
            {
                return BadRequest(new
                {
                    message = "This position type already exist in database"
                });
            }//end of if

            PositionType _positionType = new PositionType()
            {
                positionTypeName = input1.positionTypeName
            };
            await _db.PositionTypes.AddAsync(_positionType);
            await _db.SaveChangesAsync();
            return Ok(new
            {
                message = "Position Type added successfully",
                data = _positionType
            });
        }//end of position type
        [HttpPost]
        public async Task<IActionResult> get_current_position([FromBody] PlateNumInput input1)
        {

            try
            {
                //use plate number to query for this current position of vehicle

                //search for the vehicle first
                Vehicle _vehicle = await _db.Vehicles.FirstOrDefaultAsync(
                    v => v.plate_number == input1.plateNumber
                );
                if (_vehicle == null)
                {
                    return NotFound(new
                    {
                        message = "vehicle not found"
                    });
                }//end of if

                //search for its postion
                Position current_pos = await _db.Positions
                .OrderBy(o => o.date_time)
                .LastOrDefaultAsync(
                    last => last.vehicleId == _vehicle.vehicleId
                );
                //search what kind of position is that
                PositionType _positionType = await _db.PositionTypes
                .FirstOrDefaultAsync(p => p.positionTypeId == current_pos.positionTypeId);

                return Ok(new
                {
                    message = "success, the current position detail is as follows",
                    PlateNumber = _vehicle.plate_number.ToString(),
                    Brand = _vehicle.brandName.ToString(),
                    Series = _vehicle.series.ToString(),
                    Latitude = current_pos.latitude.ToString(),
                    Longitude = current_pos.longitude.ToString(),
                    PositionType = _positionType.positionTypeName.ToString(),
                    DateTime = current_pos.date_time.ToString(),
                });
            }//end of try
            catch
            {
                return BadRequest(new
                {
                    message = "Invalid Request, please try again"
                });
            }//end of catch
        }//end of function get current position
        [HttpPost]
        public async Task<IActionResult> get_journey([FromBody] PlateNumInput input1){
            try{
                //search for the vehicle first
                Vehicle _vehicle = await _db.Vehicles.FirstOrDefaultAsync(
                    v => v.plate_number == input1.plateNumber
                );
                if (_vehicle == null)
                {
                    return NotFound(new
                    {
                        message = "vehicle not found"
                    });
                }//end of if
                //contruct anonymous list and output them
                //this is the list that will tell where this vehicle has been
                //its journey
                var position_list = await _db.Positions.Select(p => new{
                    PlateNumber = _vehicle.plate_number.ToString(),
                    BrandName = _vehicle.brandName.ToString(),
                    Series = _vehicle.series.ToString(),
                    Longitude = p.longitude.ToString(),
                    Latitude = p.latitude.ToString(),
                    DateTime = p.date_time.ToString(),
                    PositionType = p.positionType.positionTypeName.ToString(),
                    Route = p.route
                })
                .OrderBy( o => o.DateTime)
                .ToListAsync();
                return Ok(new{
                    message = "success, the vehicle's journey detail is as follows",
                    journey = position_list
                });
            }//end of try
            catch{
                return BadRequest(new{
                    message = "Invalid Request, please try again"
                });
            }//end of catch
        }//end of function get journey

        [HttpPost]
        public async Task<IActionResult> check_activities_by_date([FromBody] DateInput input1){
            try{
                Position dateCheck = _db.Positions.FirstOrDefault(
                    dc => dc.date_time == input1.dateTime
                );
                if(dateCheck == null){
                    return NotFound(new{
                        message = "This date has no activity"
                    });
                }//end of if
                //search activity on that date
                var activity = await _db.Positions
                .Where( c => c.date_time == input1.dateTime)
                .Select( p =>new{
                    vehicle_plate_number = p.vehicle.plate_number.ToString(),
                    brand_name = p.vehicle.brandName.ToString(),
                    series = p.vehicle.series.ToString(),
                    latitude = p.latitude.ToString(),
                    longitude = p.longitude.ToString(),
                    route = p.route.ToString(),
                })
                .ToListAsync();
                return Ok(activity);
            }//end of try
            catch{
                return BadRequest(new{
                    message = "invalid request"
                });
            }//end of catch
        }//end of function check activity

        [HttpPost]
        public async Task<IActionResult> check_latitude([FromBody] LatitudeInput input1){
            try{

                Position latCheck = _db.Positions.FirstOrDefault(
                    lc => lc.latitude == input1.latitude
                );
                if(latCheck == null){
                    return NotFound(new{
                        message = "This latitude has no activity"
                    });
                }//end of if
                var pos = await _db.Positions
                .Where( x => x.latitude == input1.latitude)
                .Select( s => new{
                    vehicle_plate_number = s.vehicle.plate_number.ToString(),
                    brand_name = s.vehicle.brandName.ToString(),
                    series = s.vehicle.series.ToString(),
                    longitude = s.longitude.ToString(),
                    route = s.route.ToString()
                }).ToListAsync();
                return Ok(pos);
            }//end of try
            catch{
                return BadRequest(new{
                    message = "invalid request"
                });
            }//end of catch
        }//end of function check latitude

        [HttpPost]
        public async Task<IActionResult> check_longitude([FromBody] LongitudeInput input1){
            try{
                Position longCheck = _db.Positions.FirstOrDefault(
                    lc => lc.longitude == input1.longitude
                );
                if(longCheck == null){
                    return NotFound(new{
                        message = "This longitude has no activity"
                    });
                }//end of if
                var pos = await _db.Positions
                .Where( x => x.longitude == input1.longitude)
                .Select( s => new{
                    vehicle_plate_number = s.vehicle.plate_number.ToString(),
                    brand_name = s.vehicle.brandName.ToString(),
                    series = s.vehicle.series.ToString(),
                    latitude = s.latitude.ToString(),
                    route = s.route.ToString()
                }).ToListAsync();
                return Ok(pos);
            }//end of try
            catch{
                return BadRequest(new{
                    message = "invalid request"
                });
            }//end of catch
        }//end of function check longitude

        [HttpPost]
        public async Task<IActionResult> check_lat_long([FromBody] LatLongInput input1){
            try{
                Position latLongCheck = _db.Positions.FirstOrDefault(
                    lc => lc.latitude == input1.latitude && lc.longitude == input1.longitude
                );
                if(latLongCheck == null){
                    return NotFound(new{
                        message = "At this coordinate there is no activity"
                    });
                }//end of if
                var activities = await _db.Positions
                .Where( x => x.longitude == input1.longitude && x.latitude == input1.latitude)
                .Select( s => new{
                    vehicle_plate_number = s.vehicle.plate_number.ToString(),
                    brand_name = s.vehicle.brandName.ToString(),
                    series = s.vehicle.series.ToString(),
                    date_time = s.date_time.ToString(),
                    route = s.route.ToString()
                })
                .ToListAsync();
                return Ok(activities);
            }//end of try
            catch{
                return BadRequest(new{
                    message = "invalid request"
                });
            }//end of catch
        }//end of function check lat long

        [HttpPost]
        public async Task<IActionResult> check_route([FromBody] RouteInput input1){
            try{
                Position routeCheck = _db.Positions.FirstOrDefault(
                    rc => rc.route == input1.route_identification
                );
                if(routeCheck == null){
                    return NotFound(new{
                        message = "This route doesn't exist"
                    });
                }//end of if
                var route = await _db.Positions
                .Where( x => x.route == input1.route_identification)
                .Select( s => new{
                    vehicle_plate_number = s.vehicle.plate_number.ToString(),
                    brand_name = s.vehicle.brandName.ToString(),
                    series = s.vehicle.series.ToString(),
                    date_time = s.date_time.ToString(),
                    position_type = s.positionType.positionTypeName.ToString(),
                    latitude = s.latitude.ToString(),
                    longitude = s.longitude.ToString(),
                })
                .ToListAsync();
                return Ok(route);
            }//end of try
            catch{
                return BadRequest();
            }//end of catch
        }//end of check route function

    }//end of class
}//end of namespace