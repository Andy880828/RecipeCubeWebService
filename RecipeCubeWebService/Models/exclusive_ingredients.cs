using System;
using System.Collections.Generic;

namespace RecipeCubeWebService.Models;

public partial class exclusive_ingredients
{
    public int exclusiveIngredientId { get; set; }

    public string? userId { get; set; }

    public int? ingredientId { get; set; }
}
