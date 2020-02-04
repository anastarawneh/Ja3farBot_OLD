using System.ComponentModel.DataAnnotations;

namespace rJordanBot.Resources.Database
{
    public class Channel
    {
        [Key]
        public ulong ID { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
    }
}
