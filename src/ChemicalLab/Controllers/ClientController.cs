using Microsoft.AspNetCore.Mvc;

namespace EKIFVK.ChemicalLab.Controllers {
    public class ClientController : Controller {
        public IActionResult Index() {
            return View("../Index");
        }
    }
}