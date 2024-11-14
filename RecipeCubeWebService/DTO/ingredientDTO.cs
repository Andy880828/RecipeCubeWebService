namespace RecipeCubeWebService.DTO
{
    public class ingredientDTO
    {
        public int ingredientId { get; set; }

        public string ingredientName { get; set; }

        public string category { get; set; }

        public string synonym { get; set; }

        public int? expireDay { get; set; } = 7;

        public string unit { get; set; } = "克";

        public decimal? gram { get; set; }

        public IFormFile photo { get; set; }
    }
}
