using System.ComponentModel.DataAnnotations;
namespace RecipeCubeWebService.DTO
{
    public class inventoryDTO
    {
        public int inventoryId { get; set; }
        public int groupId { get; set; }
        public string userId { get; set; }
        public string userName { get; set; }
        public int? ingredientId { get; set; }
        public string ingredientName { get; set; }
        public string category { get; set; }
        public string synonym { get; set; }
        public string unit { get; set; }
        public decimal? gram { get; set; }
        public string photo { get; set; }
        public decimal? quantity { get; set; }
        public DateTime? expiryDate { get; set; }
        public bool? isExpiring { get; set; }
        public bool? isExpired { get; set; }
        public bool? visibility { get; set; }
    }
}