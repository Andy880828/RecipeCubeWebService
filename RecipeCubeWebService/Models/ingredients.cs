using System;
using System.Collections.Generic;

namespace RecipeCubeWebService.Models;

public partial class ingredients
{
    public int ingredientId { get; set; }

    public string? ingredientName { get; set; }

    public string? category { get; set; }

    public string? synonym { get; set; }

    public int? expireDay { get; set; }

    public string? unit { get; set; }

    public decimal? gram { get; set; }

    public string? photo { get; set; }
}
