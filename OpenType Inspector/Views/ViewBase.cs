using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace OpenTypeInspector
{
    public abstract class ViewBase<T> : INotifyPropertyChanged where T : class
    {
        public abstract IReadOnlyList<T> Items { get; }

        protected void OnItemsChanged()
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, ItemsPropertyArgs);
        }

        private static readonly PropertyChangedEventArgs ItemsPropertyArgs = new PropertyChangedEventArgs("Items");
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
