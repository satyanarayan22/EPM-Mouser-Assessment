using EPM.Mouser.Interview.Data;
using EPM.Mouser.Interview.Models;
using Microsoft.AspNetCore.Mvc;

namespace EPM.Mouser.Interview.Web.Controllers
{
    public class WarehouseApi : Controller
    {
        private readonly IWarehouseRepository warehouseRepository;

        public WarehouseApi(IWarehouseRepository warehouseRepository)
        {
            this.warehouseRepository = warehouseRepository;
        }

        /*
         *  Action: GET
         *  Url: api/warehouse/id
         *  This action should return a single product for an Id
         */
        [HttpGet("api/warehouse/{id}")]
        public async Task<JsonResult> GetProduct(long id)
        {
            if (id == 0 || id < 0)
            {
                return Json(BadRequest());
            }

            var product = await warehouseRepository.Get(id);

            if (product == null)
            {
                return Json(NotFound());
            }

            return Json(product);
        }

        /*
         *  Action: GET
         *  Url: api/warehouse
         *  This action should return a collection of products in stock
         *  In stock means In Stock Quantity is greater than zero and In Stock Quantity is greater than the Reserved Quantity
         */
        [HttpGet("api/warehouse")]
        public async Task<JsonResult> GetPublicInStockProducts()
        {
            var productList = await warehouseRepository.Query(x => x.InStockQuantity > 0 && x.InStockQuantity > x.ReservedQuantity);

            if (productList == null || productList.Count < 1)
            {
                return Json(NotFound());
            }

            return Json(productList);
        }


        /*
         *  Action: GET
         *  Url: api/warehouse/order
         *  This action should return a EPM.Mouser.Interview.Models.UpdateResponse
         *  This action should have handle an input parameter of EPM.Mouser.Interview.Models.UpdateQuantityRequest in JSON format in the body of the request
         *       {
         *           "id": 1,
         *           "quantity": 1
         *       }
         *
         *  This action should increase the Reserved Quantity for the product requested by the amount requested
         *
         *  This action should return failure (success = false) when:
         *     - ErrorReason.NotEnoughQuantity when: The quantity being requested would increase the Reserved Quantity to be greater than the In Stock Quantity.
         *     - ErrorReason.QuantityInvalid when: A negative number was requested
         *     - ErrorReason.InvalidRequest when: A product for the id does not exist
        */
        [HttpGet("api/warehouse/order")]
        public async Task<JsonResult> OrderItem([FromBody]UpdateQuantityRequest request)
        {
            if (request == null || request.Id <= 0)
            {
                return Json(BadRequest());
            }

            if (request.Quantity < 0)
            {
                return GetFailureUpdateResponse(ErrorReason.QuantityInvalid);
            }

            var product = await warehouseRepository.Get(request.Id);

            if (product == null)
            {
                return GetFailureUpdateResponse(ErrorReason.InvalidRequest);
            }

            if (product.ReservedQuantity + request.Quantity > product.InStockQuantity)
            {
                return GetFailureUpdateResponse(ErrorReason.NotEnoughQuantity);
            }

            product.ReservedQuantity = product.ReservedQuantity + request.Quantity;
            await warehouseRepository.UpdateQuantities(product);

            return Json(new UpdateResponse()
            {
                Success = true
            });
        }

        /*
         *  Url: api/warehouse/ship
         *  This action should return a EPM.Mouser.Interview.Models.UpdateResponse
         *  This action should have handle an input parameter of EPM.Mouser.Interview.Models.UpdateQuantityRequest in JSON format in the body of the request
         *       {
         *           "id": 1,
         *           "quantity": 1
         *       }
         *
         *
         *  This action should:
         *     - decrease the Reserved Quantity for the product requested by the amount requested to a minimum of zero.
         *     - decrease the In Stock Quantity for the product requested by the amount requested
         *
         *  This action should return failure (success = false) when:
         *     - ErrorReason.NotEnoughQuantity when: The quantity being requested would cause the In Stock Quantity to go below zero.
         *     - ErrorReason.QuantityInvalid when: A negative number was requested
         *     - ErrorReason.InvalidRequest when: A product for the id does not exist
        */
        [HttpPut("api/warehouse/ship")]
        public async Task<JsonResult> ShipItem([FromBody]UpdateQuantityRequest request)
        {
            if (request == null || request.Id <= 0)
            {
                return Json(BadRequest());
            }

            if (request.Quantity < 0)
            {
                return GetFailureUpdateResponse(ErrorReason.QuantityInvalid);
            }

            var product = await warehouseRepository.Get(request.Id);

            if (product == null)
            {
                return GetFailureUpdateResponse(ErrorReason.InvalidRequest);
            }

            if (product.InStockQuantity - request.Quantity < 0)
            {
                return GetFailureUpdateResponse(ErrorReason.NotEnoughQuantity);
            }

            product.ReservedQuantity = Math.Max(product.ReservedQuantity - request.Quantity, 0);
            product.InStockQuantity = Math.Max(product.InStockQuantity - request.Quantity, 0);

            await warehouseRepository.UpdateQuantities(product);

            return Json(new UpdateResponse()
            {
                Success = true
            });
        }

        /*
        *  Url: api/warehouse/restock
        *  This action should return a EPM.Mouser.Interview.Models.UpdateResponse
        *  This action should have handle an input parameter of EPM.Mouser.Interview.Models.UpdateQuantityRequest in JSON format in the body of the request
        *       {
        *           "id": 1,
        *           "quantity": 1
        *       }
        *
        *
        *  This action should:
        *     - increase the In Stock Quantity for the product requested by the amount requested
        *
        *  This action should return failure (success = false) when:
        *     - ErrorReason.QuantityInvalid when: A negative number was requested
        *     - ErrorReason.InvalidRequest when: A product for the id does not exist
        */
        [HttpPut("api/warehouse/restock")]
        public async Task<JsonResult> RestockItem([FromBody]UpdateQuantityRequest request)
        {
            if (request == null || request.Id <= 0)
            {
                return Json(BadRequest());
            }

            if (request.Quantity < 0)
            {
                return GetFailureUpdateResponse(ErrorReason.QuantityInvalid);
            }

            var product = await warehouseRepository.Get(request.Id);

            if (product == null)
            {
                return GetFailureUpdateResponse(ErrorReason.InvalidRequest);
            }

            product.InStockQuantity = product.InStockQuantity + request.Quantity;

            await warehouseRepository.UpdateQuantities(product);

            return Json(new UpdateResponse()
            {
                Success = true
            });
        }

        /*
        *  Url: api/warehouse/add
        *  This action should return a EPM.Mouser.Interview.Models.CreateResponse<EPM.Mouser.Interview.Models.Product>
        *  This action should have handle an input parameter of EPM.Mouser.Interview.Models.Product in JSON format in the body of the request
        *       {
        *           "id": 1,
        *           "inStockQuantity": 1,
        *           "reservedQuantity": 1,
        *           "name": "product name"
        *       }
        *
        *
        *  This action should:
        *     - create a new product with:
        *          - The requested name - But forced to be unique - see below
        *          - The requested In Stock Quantity
        *          - The Reserved Quantity should be zero
        *
        *       UNIQUE Name requirements
        *          - No two products can have the same name
        *          - Names should have no leading or trailing whitespace before checking for uniqueness
        *          - If a new name is not unique then append "(1)" to the name [like windows file system does]
        *
        *
        *  This action should return failure (success = false) and an empty Model property when:
        *     - ErrorReason.QuantityInvalid when: A negative number was requested for the In Stock Quantity
        *     - ErrorReason.InvalidRequest when: A blank or empty name is requested
        */
        [HttpPost("api/warehouse/add")]
        public async Task<JsonResult> AddNewProduct([FromBody]Product request)
        {
            if (request == null || request.Id <= 0)
            {
                return Json(BadRequest());
            }

            if (string.IsNullOrEmpty(request.Name.Trim()))
            {
                return GetFailureCreateResponse(ErrorReason.InvalidRequest);
            }

            if (request.InStockQuantity < 0)
            {
                return GetFailureCreateResponse(ErrorReason.QuantityInvalid);
            }

            var productList = warehouseRepository.List().Result;

            var uniqueProductName = GetProductUniqueName(request.Name, productList);

            request.Name = uniqueProductName;
            request.ReservedQuantity = 0;

            var result = await warehouseRepository.Insert(request);

            return Json(new CreateResponse<Product>()
            {
                Success = true,
                Model = result
            });
        }

        
        private string GetProductUniqueName(string name, IList<Product> productList, int i = 0)
        {
            name = i == 0 ? name.Trim() : $"{name.Trim()}({i})";

            var product = productList.Where(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

            if (product != null)
            {
                i += 1;
                return GetProductUniqueName(name, productList, i);
            }
            {
                return name;
            }
        }

        private JsonResult GetFailureCreateResponse(ErrorReason errorReason, Product? product = null)
        {
            return Json(new CreateResponse<Product>()
            {
                Model = product,
                ErrorReason = errorReason,
                Success = false
            });
        }

        private JsonResult GetFailureUpdateResponse(ErrorReason errorReason)
        {
            return Json(new UpdateResponse()
            {
                ErrorReason = errorReason,
                Success = false
            });
        }
    }
}
