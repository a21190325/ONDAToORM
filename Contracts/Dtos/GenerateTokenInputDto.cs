using System.ComponentModel.DataAnnotations;

namespace Contracts.Dtos
{
    public class GenerateTokenInputDto
    {
        [Required]
        [DataType(DataType.Text)]
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
