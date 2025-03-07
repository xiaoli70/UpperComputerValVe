using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataService.Entity
{
    public class OperationLog
    {
        public string ValveName { get; set; }
        public DateTime Timestamp { get; set; }   // 操作时间

        public string OperationName { get; set; } // 操作名称


        public string Detail { get; set; }        // 操作详情
    }
}
