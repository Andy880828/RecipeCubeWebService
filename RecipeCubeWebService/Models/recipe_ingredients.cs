using System;
using System.Collections.Generic;

namespace RecipeCubeWebService.Models;

public partial class recipe_ingredients
{
    public int recipeIngredientId { get; set; }

    public int? recipeId { get; set; }

    public int? ingredientId { get; set; }

    public decimal? quantity { get; set; }
}
