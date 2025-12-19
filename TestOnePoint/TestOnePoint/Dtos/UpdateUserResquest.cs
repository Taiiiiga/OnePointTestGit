using System.ComponentModel.DataAnnotations;

namespace TestOnePoint.Dtos
{
    // Classe de requête pour les mises à jour partielles.
    // Les propriétés sont facultatives (nullable) afin d'autoriser des mises à jour partielles.
    public class UpdateUserRequest
    {
        public string? Name { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [MinLength(6)]
        public string? Password { get; set; }
    }
}