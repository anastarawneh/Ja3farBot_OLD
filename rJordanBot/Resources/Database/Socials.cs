using System.ComponentModel.DataAnnotations;

namespace rJordanBot.Resources.Database
{
    public class Social
    {
        [Key]
        public ulong UserId { get; set; }
        public string Twitter { get; set; }
        public string Instagram { get; set; }
        public string Snapchat { get; set; }
        public ulong MsgId { get; set; }
    }
}
