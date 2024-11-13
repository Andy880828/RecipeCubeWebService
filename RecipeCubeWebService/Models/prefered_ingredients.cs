using System;
using System.Collections.Generic;

namespace RecipeCubeWebService.Models;

public partial class prefered_ingredients
{
    public int preferIngredientId { get; set; }

    public string? userId { get; set; }

    public int? ingredientId { get; set; }
}
