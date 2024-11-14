namespace RecipeCubeWebService.DTO;

    public class OrderOrderItemDTO
    {
        public long orderId { get; set; }

        public string? userId { get; set; }

        public DateTime? orderTime { get; set; }

        public int? totalAmount { get; set; }

        public int? status { get; set; }

        public string? orderAddress { get; set; }

        public string? orderPhone { get; set; }

        public string? orderEmail { get; set; }

        public string? orderRemark { get; set; }

        public string? orderName { get; set; }

        public List<OrderItemDTO>? OrderItemsDTO { get; set; }
}

