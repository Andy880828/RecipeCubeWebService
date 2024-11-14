namespace RecipeCubeWebService.DTO
{
    public class SignUpDTO
    {
        public required string email { get; set; }
        public required string password { get; set; }
        public required bool dietaryRestrictions { get; set; }
    }
}
