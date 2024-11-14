using System;
using System.Collections.Generic;

namespace RecipeCubeWebService.Models;

public partial class inventory
{
    public int inventoryId { get; set; }

    public int? groupId { get; set; }

    public string? userId { get; set; }

    public int? ingredientId { get; set; }

    public decimal? quantity { get; set; }

    public DateTime? expiryDate { get; set; }

    public bool? visibility { get; set; }
}
