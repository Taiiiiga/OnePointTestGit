using System.ComponentModel.DataAnnotations.Schema;

namespace TestOnePoint.Dtos
{
    public class UserResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        // Plain password received from clients (create / update). Not mapped to DB.
        [NotMapped]
        public string? Password { get; set; }

        // Persisted password hash. Mapped to the existing DB column "Password" to avoid schema changes.
        [Column("Password")]
        public string PasswordHash { get; set; } = default!;
    }
}
