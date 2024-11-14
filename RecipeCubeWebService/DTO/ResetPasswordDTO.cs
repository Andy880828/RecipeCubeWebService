namespace RecipeCubeWebService.DTO
{
    public class ResetPasswordDTO
    {
        public required string password { get; set; }
        public required string token { get; set; }
    }
}
