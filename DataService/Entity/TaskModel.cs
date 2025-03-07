using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataService.Entity
{
    public class TaskModel
    {
        public string TaskName { get; set; }
        public bool IsCompleted { get; set; }
        public string Status { get; set; }
        public int Sequence { get; set; }  // 任务的执行顺序
    }
}
