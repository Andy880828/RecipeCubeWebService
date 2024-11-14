namespace RecipeCubeWebService.DTO
{
    public class SendEmailDTO
    {
        public string?  toName  { get; set; }
        public required string toEmail { get; set; }
        public string? title { get; set; }
        public string? body { get; set; }

    }
}
