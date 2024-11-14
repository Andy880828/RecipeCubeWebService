using System;
using System.Collections.Generic;

namespace RecipeCubeWebService.Models;

public partial class product_evaluate
{
    public int evaluateId { get; set; }

    public string? userId { get; set; }

    public int? productId { get; set; }

    public string? commentMessage { get; set; }

    public int commentStars { get; set; }

    public DateTime date { get; set; }
}
