using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsManagement.Domain.DTOs
{
    public class CategoryDTO
    {
        public short CategoryID { get; set; }
        public string CategoryName { get; set; }
        public string CategoryDescription { get; set; }
        public short? ParentCategoryID { get; set; }
        public bool IsActive { get; set; }
        public string ParentCategoryName { get; set; }
    }
}
