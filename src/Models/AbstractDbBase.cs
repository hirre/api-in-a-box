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
            CreatedBy = UpdatedBy = "sys";
        }

        [Key]        
        public int Id { get; set; }

        public Instant CreatedDate { get; set; }

        public string CreatedBy { get; set; }

        public Instant UpdatedDate { get; set; }

        public string UpdatedBy { get; set; }

        public uint xmin { get; set; }
    }
}
