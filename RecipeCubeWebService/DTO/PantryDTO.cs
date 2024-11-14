namespace RecipeCubeWebService.DTO
{
    public class PantryDTO
    {
        public int pantryId { get; set; }

        public int? groupId { get; set; }

        public string? userId { get; set; }

        public string? userName { get; set; }

        public string? ownerId { get; set; }

        public string? ownerName { get; set; }

        public int? ingredientId { get; set; }

        public string? ingredientName { get; set; }

        public decimal? quantity { get; set; }

        public string? unit { get; set; }

        public string action { get; set; }

        public DateTime? time { get; set; }
    }
}
