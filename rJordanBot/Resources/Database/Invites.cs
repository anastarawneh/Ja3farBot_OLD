using System.ComponentModel.DataAnnotations;

namespace rJordanBot.Resources.Database
{
    public class Invite
    {
        [Key]
        public string Text { get; set; }
        public ulong UserId { get; set; }
        public int Uses { get; set; }
    }
}
