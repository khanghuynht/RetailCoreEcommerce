using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RetailCoreEcommerce.Contracts.Models.Inventory;

public class UpdateInventoryQuantityRequest
{
    [JsonIgnore] public Guid ProductId { get; set; } // set from route in controller

    [Range(0, int.MaxValue, ErrorMessage = "StockQuantity must be >= 0.")]
    public int StockQuantity { get; set; }
}