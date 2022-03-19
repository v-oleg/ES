using System.ComponentModel.DataAnnotations;

namespace ES.EventStoreDb.Example.Sql
{
    public class Checkpoints
    {
        [Key]
        public int Id { get; set; }
        public string Projector { get; set; } = null!;
        public ulong LastEventNumber { get; set; }
    }
}
