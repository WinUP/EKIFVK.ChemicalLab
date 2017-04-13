using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EKIFVK.ChemicalLab.Attributes;
using EKIFVK.ChemicalLab.Configurations;
using EKIFVK.ChemicalLab.Models;
using EKIFVK.ChemicalLab.Services.Tracking;
using EKIFVK.ChemicalLab.Services.Verification;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EKIFVK.ChemicalLab.Controllers {
    /// <summary>
    /// API for Storage management
    /// </summary>
    [Route("api/1.1/storage")]
    public class StorageController : VerifiableController {
        private readonly IOptions<PlaceModule> Configuration;

        public StorageController(ChemicalLabContext database, IVerificationService verifier, ITrackerService tracker, IOptions<PlaceModule> configuration)
            : base(database, verifier, tracker) {
            Configuration = configuration;
        }

        [HttpPost("/room/{name}")]
        [Verify("SR:ADDROOM")]
        public JsonResult AddRoom(string name) {
            if (!IsNameValid(name))
                return Json(StatusCodes.Status400BadRequest, Configuration.Value.InvalidRoomName);
            var room = Database.Rooms.FirstOrDefault(e => e.Name == name);
            if (room != null)
                return Json(StatusCodes.Status409Conflict, Configuration.Value.AlreadyExisted);
            room = new Room {
                Name = name,
                LastUpdate = DateTime.Now
            };
            Database.Rooms.Add(room);
            Database.SaveChanges();
            Tracker.Get(Operation.AddNewRoom).By(CurrentUser).At(room.Id).From("").To("").Save();
            return Json(data: room.Id);
        }

        [HttpPost("/place/{name}")]
        [Verify("SR:ADDPLACE")]
        public JsonResult AddPlace(string name) {
            if (!IsNameValid(name))
                return Json(StatusCodes.Status400BadRequest, Configuration.Value.InvalidPlaceName);
            var place = Database.Places.FirstOrDefault(e => e.Name == name);
            if (place != null)
                return Json(StatusCodes.Status409Conflict, Configuration.Value.AlreadyExisted);
            place = new Place {
                Name = name,
                LastUpdate = DateTime.Now
            };
            Database.Places.Add(place);
            Database.SaveChanges();
            Tracker.Get(Operation.AddNewPlace).By(CurrentUser).At(place.Id).From("").To("").Save();
            return Json(data: place.Id);
        }

        [HttpPost("/location")]
        [Verify("SR:ADDPLACE")]
        public JsonResult AddLocation([FromBody] Hashtable parameter) {
            var roomName = parameter["room"].ToString();
            var room = Database.Rooms.FirstOrDefault(e => e.Name == roomName);
            if (room == null)
                return Json(StatusCodes.Status409Conflict, Configuration.Value.InvalidRoomName);
            var placeName = parameter["place"].ToString();
            var place = Database.Places.FirstOrDefault(e => e.Name == placeName);
            if (place == null)
                return Json(StatusCodes.Status409Conflict, Configuration.Value.InvalidPlaceName);
            var location = new Location {
                RoomNavigation = room,
                PlaceNavigation = place,
                LastUpdate = DateTime.Now
            };
            Database.Locations.Add(location);
            Database.SaveChanges();
            Tracker.Get(Operation.AddNewLocation).By(CurrentUser).At(location.Id).From("").To("").Save();
            return Json(data: room.Id);
        }



        [HttpGet]
        [Verify("US:MANAGE")]
        public JsonResult GetList() {
            return Json(data: new Hashtable {
                {"room", Database.Rooms.Select(e => new {e.Id, e.Name}).ToArray()},
                {"place", Database.Places.Select(e => new {e.Id, e.Name}).ToArray()},
                {"location", Database.Locations.Select(e => new {e.Id, e.Place, e.Room}).ToArray()}
            });
        }
    }
}