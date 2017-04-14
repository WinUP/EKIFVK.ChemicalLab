using System;
using System.Collections;
using System.Linq;
using EKIFVK.ChemicalLab.Attributes;
using EKIFVK.ChemicalLab.Configurations;
using EKIFVK.ChemicalLab.Models;
using EKIFVK.ChemicalLab.Services.Tracking;
using EKIFVK.ChemicalLab.Services.Verification;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EKIFVK.ChemicalLab.Controllers {
    [Route("api/1.1/storage")]
    public class StorageController : VerifiableController {
        private readonly IOptions<PlaceModule> Configuration;

        public StorageController(ChemicalLabContext database, IVerificationService verifier, ITrackerService tracker, IOptions<PlaceModule> configuration)
            : base(database, verifier, tracker) {
            Configuration = configuration;
        }

        [HttpPost("/room")]
        [Verify("SR:ADD")]
        public JsonResult AddRoom([FromBody] Hashtable parameter) {
            var name = parameter["name"].ToString();
            if (!IsNameValid(name))
                return Json(StatusCodes.Status400BadRequest, Configuration.Value.InvalidRoom);
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

        [HttpPost("/place")]
        [Verify("SR:ADD")]
        public JsonResult AddPlace([FromBody] Hashtable parameter) {
            var name = parameter["name"].ToString();
            if (!IsNameValid(name))
                return Json(StatusCodes.Status400BadRequest, Configuration.Value.InvalidPlace);
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
        [Verify("SR:ADD")]
        public JsonResult AddLocation([FromBody] Hashtable parameter) {
            var roomId = (int) parameter["room"];
            var room = Database.Rooms.FirstOrDefault(e => e.Id == roomId);
            if (room == null)
                return Json(StatusCodes.Status404NotFound, Configuration.Value.InvalidRoom);
            var placeId = (int) parameter["place"];
            var place = Database.Places.FirstOrDefault(e => e.Id == placeId);
            if (place == null)
                return Json(StatusCodes.Status404NotFound, Configuration.Value.InvalidPlace);
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

        [HttpDelete("/room/{id}")]
        [Verify("SR:DELETE")]
        public JsonResult DeleteRoom(int id) {
            var target = Database.Rooms.FirstOrDefault(e => e.Id == id);
            if (target == null)
                return Json(StatusCodes.Status404NotFound, Configuration.Value.InvalidRoom);
            if (Database.Locations.Count(e => e.Room == id) > 0)
                return Json(StatusCodes.Status403Forbidden, Configuration.Value.OperationDenied);
            try {
                Tracker.Get(Operation.DeleteRoom).By(CurrentUser).At(target.Id).From("").Do(() => {
                    Database.Rooms.Remove(target);
                }).To("").Save();
            } catch (Exception ex) {
                return Json(ex);
            }
            return Json();
        }

        [HttpDelete("/place/{id}")]
        [Verify("SR:DELETE")]
        public JsonResult DeletePlace(int id) {
            var target = Database.Places.FirstOrDefault(e => e.Id == id);
            if (target == null)
                return Json(StatusCodes.Status404NotFound, Configuration.Value.InvalidPlace);
            if (Database.Locations.Count(e => e.Room == id) > 0)
                return Json(StatusCodes.Status403Forbidden, Configuration.Value.OperationDenied);
            try {
                Tracker.Get(Operation.DeletePlace).By(CurrentUser).At(target.Id).From("").Do(() => {
                    Database.Places.Remove(target);
                }).To("").Save();
            } catch (Exception ex) {
                return Json(ex);
            }
            return Json();
        }

        [HttpDelete("/location/{id}")]
        [Verify("SR:DELETE")]
        public JsonResult DeleteLocation(int id)  {
            var target = Database.Locations.FirstOrDefault(e => e.Id == id);
            if (target == null)
                return Json(StatusCodes.Status404NotFound, Configuration.Value.InvalidLocation);
            if (Database.Items.Count(e => e.Location == id) > 0)
                return Json(StatusCodes.Status403Forbidden, Configuration.Value.OperationDenied);
            try {
                Tracker.Get(Operation.DeleteLocation).By(CurrentUser).At(target.Id).From("").Do(() => {
                    Database.Locations.Remove(target);
                }).To("").Save();
            } catch (Exception ex) {
                return Json(ex);
            }
            return Json();
        }

        [HttpPatch("/room/{id}")]
        [Verify("SR:MANAGE")]
        public JsonResult ChangeRoomInformation(int id, [FromBody] Hashtable param) {
            var target = Database.Rooms.FirstOrDefault(e => e.Id == id);
            if (target == null)
                return Json(StatusCodes.Status404NotFound, Configuration.Value.InvalidRoom);
            var data = new Hashtable();
            if (param.ContainsKey("name")) {
                data["name"] = true;
                Tracker.Get(Operation.ChangeRoomName).By(CurrentUser).At(target.Id).From(target.Name).Do(() => {
                    target.Name = param["name"].ToString();
                }).To(target.Name).Save(false);
            }
            target.LastUpdate = DateTime.Now;
            Database.SaveChanges();
            return Json(data: data);
        }

        [HttpPatch("/place/{id}")]
        [Verify("SR:MANAGE")]
        public JsonResult ChangePlaceInformation(int id, [FromBody] Hashtable param) {
            var target = Database.Places.FirstOrDefault(e => e.Id == id);
            if (target == null)
                return Json(StatusCodes.Status404NotFound, Configuration.Value.InvalidPlace);
            var data = new Hashtable();
            if (param.ContainsKey("name")) {
                data["name"] = true;
                Tracker.Get(Operation.ChangePlaceName).By(CurrentUser).At(target.Id).From(target.Name).Do(() => {
                    target.Name = param["name"].ToString();
                }).To(target.Name).Save(false);
            }
            target.LastUpdate = DateTime.Now;
            Database.SaveChanges();
            return Json(data: data);
        }

        [HttpPatch("/location/{id}")]
        [Verify("SR:MANAGE")]
        public JsonResult ChangeLocationInformation(int id, [FromBody] Hashtable param) {
            var target = Database.Locations.FirstOrDefault(e => e.Id == id);
            if (target == null)
                return Json(StatusCodes.Status404NotFound, Configuration.Value.InvalidLocation);
            var data = new Hashtable();
            if (param.ContainsKey("room")) {
                data["room"] = true;
                var roomId = (int) param["room"];
                var room = Database.Rooms.FirstOrDefault(e => e.Id == roomId);
                if (room == null)
                    data["room"] = Json(StatusCodes.Status404NotFound, Configuration.Value.InvalidRoom);
                else
                    Tracker.Get(Operation.ChangeLocationRoom).By(CurrentUser).At(target.Id).From(target.Room.ToString()).Do(() => {
                        target.RoomNavigation = room;
                    }).To(room.Id.ToString()).Save(false);
            }
            if (param.ContainsKey("place")) {
                data["place"] = true;
                var placeId = (int) param["place"];
                var place = Database.Places.FirstOrDefault(e => e.Id == placeId);
                if (place == null)
                    data["place"] = Json(StatusCodes.Status404NotFound, Configuration.Value.InvalidPlace);
                else
                    Tracker.Get(Operation.ChangeLocationPlace).By(CurrentUser).At(target.Id).From(target.Place.ToString()).Do(() => {
                        target.PlaceNavigation = place;
                    }).To(place.Id.ToString()).Save(false);
            }
            target.LastUpdate = DateTime.Now;
            Database.SaveChanges();
            return Json(data: data);
        }

        [HttpGet]
        [Verify("")]
        public JsonResult GetList() {
            return Json(data: new Hashtable {
                {"room", Database.Rooms.Select(e => new {
                    e.Id,
                    e.Name,
                    Update = e.LastUpdate.ToString("yyyy-MM-dd HH:mm:ss")
                }).ToArray()},
                {"place", Database.Places.Select(e => new {
                    e.Id,
                    e.Name,
                    Update = e.LastUpdate.ToString("yyyy-MM-dd HH:mm:ss")
                }).ToArray()},
                {"location", Database.Locations.Include(e => e.Items).Select(e => new {
                    e.Id,
                    e.Place,
                    e.Room,
                    Update = e.LastUpdate.ToString("yyyy-MM-dd HH:mm:ss"),
                    Items = e.Items.Count
                }).ToArray()}
            });
        }
    }
}