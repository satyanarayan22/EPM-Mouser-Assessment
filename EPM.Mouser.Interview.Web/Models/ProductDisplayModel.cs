using EPM.Mouser.Interview.Models;
using System.ComponentModel.DataAnnotations;

namespace EPM.Mouser.Interview.Web.Models
{
    public class ProductListModel
    {
        public IList<ProductDisplayModel> ProductDisplayModels { get; set; }

        public ProductListModel(IList<Product> products)
        {
            ProductDisplayModels = new List<ProductDisplayModel>();

            foreach(var product in products)
            {
                ProductDisplayModels.Add(new ProductDisplayModel(product));
            }
        }
    }

    public class ProductDisplayModel
    {
        [Display(Name = "Id")]
        public long Id { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Total Amount in stock")]
        public int InStockQuantity { get; set; }

        [Display(Name = "Reserved Quantity")]
        public int ReservedQuantity { get; set; }

        [Display(Name = "Available Stock")]
        public int AvailableStock 
        { 
            get
            {
                return InStockQuantity - ReservedQuantity;
            }
        }

        public ProductDisplayModel(Product product)
        {
            Id = product.Id;
            Name = product.Name;
            ReservedQuantity = product.ReservedQuantity;
            InStockQuantity = product.InStockQuantity;            
        }
    }
}