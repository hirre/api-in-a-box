using System;

namespace ApiInABox.Models
{
    public class AbstractDbBase
    {
        public AbstractDbBase()
        {
            CreatedDate = DateTimeOffset.UtcNow;
            UpdatedDate = CreatedDate;
        }

        public int Id { get; set; }

        public DateTimeOffset CreatedDate { get; set; }

        public DateTimeOffset UpdatedDate { get; set; }

        public uint xmin { get; set; }

    }
}
