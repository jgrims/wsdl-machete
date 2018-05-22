using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wsdl_machete.Models
{

    public enum WsdlElementType
    {
        None = 0, 
        Service = 1,
        Binding = 2,
        Operation = 3
    }


    class WsdlTreeViewItem : CheckboxTreeViewItem, IComparable
    {
        public WsdlElementType Type { get; set; }

        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }

            WsdlTreeViewItem other = obj as WsdlTreeViewItem;
            if (other != null)
            {
                return Name.CompareTo(other.Name);
            } 
            else
            {
                throw new ArgumentException("Object is not a WsdlTreeViewItem");
            }

        }
    }
}
