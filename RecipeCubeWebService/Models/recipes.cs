using System;
using System.Collections.Generic;

namespace RecipeCubeWebService.Models;

public partial class recipes
{
    public int recipeId { get; set; }

    public string? recipeName { get; set; }

    public string? userId { get; set; }

    public sbyte? isCustom { get; set; }

    public sbyte? restriction { get; set; }

    public sbyte? westEast { get; set; }

    public string? category { get; set; }

    public string? detailedCategory { get; set; }

    public string? steps { get; set; }

    public string? seasoning { get; set; }

    public int? visibility { get; set; }

    public string? photo { get; set; }

    public sbyte? status { get; set; }

    public string? time { get; set; }

    public string? description { get; set; }
}
