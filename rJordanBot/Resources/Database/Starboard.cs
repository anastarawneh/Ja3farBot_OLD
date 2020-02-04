using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace rJordanBot.Resources.Database
{
    public class Starboard
    {
        [Key]
        public ulong MsgID { get; set; }
        public ulong ChannelID { get; set; }
        public ulong UserID { get; set; }
        public ulong SBMessageID { get; set; }
    }
}
