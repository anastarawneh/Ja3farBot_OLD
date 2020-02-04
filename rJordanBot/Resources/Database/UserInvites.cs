using System.ComponentModel.DataAnnotations;

namespace rJordanBot.Resources.Database
{
    public class UserInvite
    {
        [Key]
        public ulong UserID { get; set; }
        public string Code { get; set; }
    }
}
