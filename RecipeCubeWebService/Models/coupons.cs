using System;
using System.Collections.Generic;

namespace RecipeCubeWebService.Models;

public partial class coupons
{
    public int couponId { get; set; }

    public string? couponName { get; set; }

    public sbyte? status { get; set; }

    public decimal? discountValue { get; set; }

    public string? discountType { get; set; }

    public int? minSpend { get; set; }
}
