using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;


namespace EquipmentSignalData.Models
{
    public class SubItem
    {
        public string Name { get; set; }
        public Func<UserControl> CreateControl { get; set; }

        public List<SubItem> SubItems { get; set; } // 子菜单列表

        private WeakReference<UserControl> _weakControl; // 弱引用 内存紧张时调用GC回收

        public SubItem(string name, Func<UserControl> createControl = null, List<SubItem> subItems = null)
        {
            Name = name;
            CreateControl = createControl;
            SubItems = subItems ?? new List<SubItem>(); // 初始化子菜单
        }

        public UserControl GetControl() 
        {
            if (_weakControl != null && _weakControl.TryGetTarget(out var control))
            {
                return control;
            }
            else
            {
                var newControl = CreateControl?.Invoke();
                if (newControl != null)
                {
                    _weakControl = new WeakReference<UserControl>(newControl);
                }
                return newControl;
            }
        }
    }
}
