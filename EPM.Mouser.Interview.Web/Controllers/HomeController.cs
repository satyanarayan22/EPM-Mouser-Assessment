using Microsoft.AspNetCore.Mvc;
using EPM.Mouser.Interview.Data;
using EPM.Mouser.Interview.Models;
using EPM.Mouser.Interview.Web.Models;

namespace EPM.Mouser.Interview.Web.Controllers
{
    [Route("")]
    public class HomeController : Controller
    {
        private readonly IWarehouseRepository warehouseRepository;

        public HomeController(IWarehouseRepository warehouseRepository)
        {
            this.warehouseRepository = warehouseRepository;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var productList = new ProductListModel(warehouseRepository.List().Result);            
            return View(productList);
        }
    }
}