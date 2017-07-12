using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoneBox
{
    public class CheckBoxItem<T>
    {
        public CheckBoxItem(T item, bool isChecked = false, int number =0)
        {
            this.Item = item;
            this.IsChecked = isChecked;
            this.Number = number;
        }

        public T Item { get; set; }

        public bool IsChecked { get; set; }

        public int Number { get; set; }
    }
}
