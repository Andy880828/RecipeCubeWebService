using System;
using System.Collections.Generic;

namespace RecipeCubeWebService.Models;

public partial class order_items
{
    public int orderItemId { get; set; }

    public long? orderId { get; set; }

    public int? productId { get; set; }

    public int? quantity { get; set; }

    public int? price { get; set; }
}
