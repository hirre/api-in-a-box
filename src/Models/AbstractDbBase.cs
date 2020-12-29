using NodaTime;
using System;
using System.ComponentModel.DataAnnotations;

namespace ApiInABox.Models
{
    public class AbstractDbBase
    {
        public AbstractDbBase()
        {
            CreatedDate = Instant.FromDateTimeUtc(DateTime.UtcNow);
            UpdatedDate = CreatedDate;
        }

        [Key]        
        public int Id { get; set; }

        public Instant CreatedDate { get; set; }

        public Instant UpdatedDate { get; set; }

        public uint xmin { get; set; }

    }
}
