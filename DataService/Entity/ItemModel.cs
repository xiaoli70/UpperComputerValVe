using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataService.Entity
{
    public class ItemModel
    {
        public int Index { get; set; } // 索引
        public string Name { get; set; } // 名称
        public string Model { get; set; } // 型号
        public string Unit { get; set; } // 单位
        public string Type { get; set; } // 类型
    }
}
