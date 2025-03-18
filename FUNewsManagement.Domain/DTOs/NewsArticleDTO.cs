using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsManagement.Domain.DTOs
{
    public class NewsArticleDTO
    {
        public string NewsArticleID { get; set; }
        public string NewsTitle { get; set; }
        public string Headline { get; set; }
        public DateTime CreatedDate { get; set; }
        public string NewsContent { get; set; }
        public string? NewsSource { get; set; }
        public int CategoryID { get; set; }
        public bool NewsStatus { get; set; }
        public int CreatedByID { get; set; }
        public int? UpdatedByID { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public string CategoryName { get; set; }
    }
}
