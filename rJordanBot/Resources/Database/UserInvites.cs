using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace rJordanBot.Resources.Database
{
    public class UserInvite
    {
        [Key]
        public ulong UserID { get; set; }
        public string Code { get; set; }
    }
}
