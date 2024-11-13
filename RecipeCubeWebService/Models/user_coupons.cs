using System;
using System.Collections.Generic;

namespace RecipeCubeWebService.Models;

public partial class user_coupons
{
    public int userCouponId { get; set; }

    public string? userId { get; set; }

    public int? couponId { get; set; }

    public sbyte status { get; set; }

    public DateTime acquireDate { get; set; }
}
