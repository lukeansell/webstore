using Azure.Data.Tables;
using Azure;
using System.ComponentModel.DataAnnotations;
using System.Runtime.ConstrainedExecution;
using Microsoft.AspNetCore.Cors;
namespace FunctionApp.Models
{
    public class Purchase : ITableEntity
    {

        public static readonly string PKey = "PurchasePartition";

        [Key]
        public int PurchaseID { get; set; }

        [Required]
        [Display(Name = "Product ID", Prompt = "Enter Product ID")]
        [DisplayFormat(NullDisplayText = "[null]")]
        public int ProductID { get; set; }

        [Required]
        [Display(Name = "Customer ID", Prompt = "Enter Customer ID")]
        [DisplayFormat(NullDisplayText = "[null]")]
        public int CustomerID { get; set; }

        [Required]
        [Display(Name = "Quantity", Prompt = "Enter Quantity")]
        [DisplayFormat(NullDisplayText = "[null]")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be larger than 0")]
        public int Quantity { get; set; }

        [Required]
        [Display(Name = "Total Price")]
        [DataType(DataType.Currency)]
        public double TotalPrice { get; set; }

        [Required]
        [Display(Name = "Purchase Date", Prompt = "Enter Purchase Date")]
        [DisplayFormat(NullDisplayText = "[null]")]
        [DataType(DataType.Date)]
        public DateTime PurchaseDate { get; set; }

        public string? PartitionKey { get; set; } = PKey;

        public string? RowKey { get; set; }

        public DateTimeOffset? Timestamp { get; set; }

        public ETag ETag { get; set; }
    }
}
