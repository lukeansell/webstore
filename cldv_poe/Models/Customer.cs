using Azure.Data.Tables;
using Azure;
using System.ComponentModel.DataAnnotations;
using System.Runtime.ConstrainedExecution;
namespace cldv_poe.Models
{
    public class Customer : ITableEntity
    {
        public static readonly string PKey = "CustomerPartition";

        [Key]
        public int CustomerID { get; set; }

        [Required]
        [Display(Name = "Customer Name", Prompt = "Enter Customer Name")]
        [DisplayFormat(NullDisplayText = "[No Customer Name]")]
        public string? CustomerName { get; set; }

        [Required]
        [Display(Name = "Customer Email", Prompt = "Enter Customer Email")]
        [DisplayFormat(NullDisplayText = "[No Email Address]")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string? CustomerEmail { get; set; }

        [Display(Name = "Phone Number", Prompt = "Enter Phone Number")]
        [DataType(DataType.PhoneNumber)]
        [DisplayFormat(NullDisplayText = "[No Phone Number]")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Address", Prompt = "Enter Address")]
        [DisplayFormat(NullDisplayText = "[No Address]")]
        public string? Address { get; set; }

        public string? PartitionKey { get; set; } = PKey;

        public string? RowKey { get; set; }

        public DateTimeOffset? Timestamp { get; set; }

        public ETag ETag { get; set; }
    }
}
