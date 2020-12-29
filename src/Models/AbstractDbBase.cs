using System;
using System.ComponentModel.DataAnnotations;

namespace ApiInABox.Models
{
    public class AbstractDbBase
    {
        public AbstractDbBase()
        {
            CreatedDate = DateTimeOffset.UtcNow;
            UpdatedDate = CreatedDate;
        }

        [Key]        
        public int Id { get; set; }

        public DateTimeOffset CreatedDate { get; set; }

        public DateTimeOffset UpdatedDate { get; set; }

        public uint xmin { get; set; }

    }
}
