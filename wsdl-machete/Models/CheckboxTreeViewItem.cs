using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace wsdl_machete.Models
{
    class CheckboxTreeViewItem : INotifyPropertyChanged
    {

        bool? _isChecked = false;

        public List<CheckboxTreeViewItem> Children { get; set; } = new List<CheckboxTreeViewItem>();
        public bool? IsChecked
        {
            get { return _isChecked; }
            set { this.SetIsChecked(value, true, true); }
        }
        public bool IsInitiallyChecked { get; set; }
        public string Name { get; set; }
        public CheckboxTreeViewItem Parent { get; set; }

        void OnPropertyChanged(string prop)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
  
        public event PropertyChangedEventHandler PropertyChanged;

        private void SetIsChecked(bool? value, bool updateChildren, bool updateParent)
        {
            if (value == _isChecked)
                return;

            _isChecked = value;

            if (updateChildren && _isChecked.HasValue)
                this.Children.ForEach(c => c.SetIsChecked(_isChecked, true, false));

            if (updateParent && Parent != null)
                Parent.VerifyCheckState();

            OnPropertyChanged("IsChecked");
        }

        private void VerifyCheckState()
        {
            bool? state = null;
            for (int i = 0; i < this.Children.Count; ++i)
            {
                bool? current = this.Children[i].IsChecked;
                if (i == 0)
                {
                    state = current;
                }
                else if (state != current)
                {
                    state = null;
                    break;
                }
            }
            this.SetIsChecked(state, false, true);
        }

    }
}
