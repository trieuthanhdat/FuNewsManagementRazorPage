using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUNewsManagement.Domain.DTOs
{
    public class SystemAccountDTO
    {
        public short? AccountID { get; set; }
        public string AccountName { get; set; }
        public string AccountEmail { get; set; }
        public int? AccountRole { get; set; }
        public string RoleName
        {
            get
            {
                if (AccountRole == null) return "Unknown";
                return AccountRole.Value switch
                {
                    0 => "Admin",
                    1 => "Staff",
                    2 => "Lecturer",
                    _ => "Unknown"
                };
            }
        }
    }
}
