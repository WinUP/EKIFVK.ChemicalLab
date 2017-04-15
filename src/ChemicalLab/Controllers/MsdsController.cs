using System;
using System.IO;
using System.Linq;
using EKIFVK.ChemicalLab.Attributes;
using EKIFVK.ChemicalLab.Models;
using EKIFVK.ChemicalLab.Services.Tracking;
using EKIFVK.ChemicalLab.Services.Verification;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace EKIFVK.ChemicalLab.Controllers {
    [Route("api/1.1/msds")]
    public class MsdsController : VerifiableController {
        private readonly string _path;

        public MsdsController(IHostingEnvironment env, ChemicalLabContext database, IVerificationService verifier, ITrackerService tracker)
            : base(database, verifier, tracker) {
            _path = $"{env.WebRootPath}/msds";
        }

        [HttpPost]
        [Verify("ID:ADD")]
        public JsonResult Upload() {
            var file = Request.Form.Files[0];
            var fileExt = file.FileName.IndexOf('.') > -1 ? Path.GetExtension(file.FileName) : "";
            var fileId = Guid.NewGuid().ToString().ToUpper();
            while (System.IO.File.Exists($"{_path}/{fileId}{fileExt}"))
                fileId = Guid.NewGuid().ToString().ToUpper();
            using (var fs = System.IO.File.Create($"{_path}/{fileId}{fileExt}")) {
                file.CopyTo(fs);
                fs.Flush();
            }
            Tracker.Get(Operation.UploadMsds).By(CurrentUser).At(-1).From("").To(fileId).Save();
            return Json(data: fileId);
        }

        [HttpGet]
        [Verify("MS:MANAGE")]
        public JsonResult GetList() {
            var data = Database.ItemDetails.GroupBy(f => f.Msds)
                .Select(f => new {
                    Msds = f.Key,
                    Count = f.Count()
                }).ToList();
            return Json(data: Directory.GetFiles(_path).Select(e => {
                var fileName = Path.GetFileNameWithoutExtension(e);
                return new {
                    Name = Path.GetFileName(e),
                    Referenced = data.Where(f => f.Msds == fileName).Select(f => f.Count)
                };
            }).ToArray());
        }

        [HttpDelete]
        [Verify("MS:DELETE")]
        public JsonResult RemoveRange(string ids)  {
            var idList = ids.Split(';').Select(int.Parse).ToArray();
            foreach (var e in idList) {
                var filePath = $@"{_path}/{e}";
                if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
            }
            Tracker.Get(Operation.DeleteMsds).By(CurrentUser).At(-1).From(ids).To("").Save();
            return Json();
        }
    }
}