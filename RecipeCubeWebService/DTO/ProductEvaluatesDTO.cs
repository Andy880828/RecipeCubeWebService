namespace RecipeCubeWebService.DTO;

    public class ProductEvaluatesDTO
    {
        public int evaluateId { get; set; }

        public string userId { get; set; }

        public string userName {  get; set; }

        public int? productId { get; set; }

        public string commentMessage { get; set; }

        public int? commentStars { get; set; }

        public DateTime? date { get; set; }
    }

