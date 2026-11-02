using System;
using System.Collections.Generic;
using System.Text;

namespace MRP.Domain
{
    internal class Rating
    {
        public int Id { get; set; }
        public int MediaId { get; set; }
        public int UserId { get; set; }
        public int Stars { get; set; }
        public string Comment { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public bool IsConfirmed { get; set; } = false;
    }
}
