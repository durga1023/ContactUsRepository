namespace ContactApplication.Repositories.DataObjects
{
    public class Contact
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string? LastName { get; set; }
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
        public string? Zip { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Comments { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
