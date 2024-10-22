using Azure.Data.Tables;
using Azure;
using System.ComponentModel.DataAnnotations;
using System.Runtime.ConstrainedExecution;
using Microsoft.AspNetCore.Cors;
namespace cldv_poe.Models
{
    public class Product : ITableEntity
    {
        public static readonly string PKey = "ProductPartition";

        [Key]
        public int ProductID { get; set; }

        [Required]
        [Display(Name = "Product Name", Prompt = "Enter Product Name")]
        [DisplayFormat(NullDisplayText = "[null]")]
        public string? ProductName { get; set; }

        [Required]
        [Display(Name = "Product Description", Prompt = "Enter Product Description")]
        [DisplayFormat(NullDisplayText = "[null]")]
        public string? ProductDescription { get; set; }

        [Display(Name = "Category", Prompt = "Enter Product Category")]
        [DisplayFormat(NullDisplayText = "[No Category]")]
        public string? Category { get; set; }

        [Display(Name = "ImageURL", Prompt = "Enter Image URL")]
        [DisplayFormat(NullDisplayText = "[No Image]")]
        [DataType(DataType.ImageUrl)]
        public string? ImageUrl { get; set; }

        [Required]
        [Display(Name = "Price", Prompt = "Enter Product Price")]
        [DataType(DataType.Currency)]
        //[DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = true)]
        public double Price { get; set; } = 0;

        [Required]
        [Display(Name = "Number in Stock", Prompt = "Enter Stock Level")]
        public int Stock { get; set; }

        public Boolean HasStock { get { return Stock != 0; } }

        public string? PartitionKey { get; set; } = PKey;

        public string? RowKey { get; set; }

        public DateTimeOffset? Timestamp { get; set; }

        public ETag ETag { get; set; }


    }
}
