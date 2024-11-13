using System;
using System.Collections.Generic;

namespace RecipeCubeWebService.Models;

public partial class pantry_management
{
    public int pantryId { get; set; }

    public int? groupId { get; set; }

    public string? ownerId { get; set; }

    public string? userId { get; set; }

    public int? ingredientId { get; set; }

    public decimal? quantity { get; set; }

    public string? action { get; set; }

    public DateTime time { get; set; }
}
