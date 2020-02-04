using System.ComponentModel.DataAnnotations;

namespace rJordanBot.Resources.Database
{
    public class Strike
    {
        [Key]
        public ulong UserId { get; set; }
        public int Amount { get; set; }
        public string Username { get; set; }
    }
}
