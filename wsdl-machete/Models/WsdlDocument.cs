using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Services.Description;
using System.Xml;

namespace wsdl_machete.Models
{
    class WsdlDocument
    {
        public string FileName { get; set; }
        public string Description { get; set; }

        public ServiceDescription serviceDescription { get; set; } = new ServiceDescription();

        public WsdlDocument()
        {

        }

        public WsdlDocument(string fileName)
        {
            FileName = fileName;
        }

        public WsdlDocument(string fileName, string description)
        {
            Description = description;
            FileName = fileName;
        }

        public bool LoadServiceDescription(string fileName)
        {
            if (File.Exists(fileName))
            {
                using (XmlReader reader = XmlReader.Create(fileName))
                {
                    if (ServiceDescription.CanRead(reader))
                    {
                        serviceDescription = ServiceDescription.Read(reader);
                        return true;
                    }
                }
            }
            return false;
        }

        internal void UpdateBinding(Binding binding, bool? isChecked)
        {
            if (!isChecked.HasValue || isChecked.Value == true)
            {
                if (!serviceDescription.Bindings.Contains(binding))
                {
                    serviceDescription.Bindings.Add(binding);
                }
            }
            else
            {
                if (serviceDescription.Bindings.Contains(binding))
                {
                    serviceDescription.Bindings.Remove(binding);
                }
            }
        }

        internal void UpdateOperation(string operation, bool? isChecked)
        {

        }

        internal void UpdateService(string name, bool? isChecked)
        {
            Service s = findServiceByName(name);
            if (isChecked.HasValue && isChecked.Value == false)
            {
                if ( s != null)
                {
                    serviceDescription.Services.Remove(s);
                }
            }
            else
            {
                if (s != null)
                {
                    serviceDescription.Services.Add(s);
                }
            }
        }

        internal Service findServiceByName(string name)
        {
            foreach (Service s in serviceDescription.Services)
            {
                if (s.Name == name)
                {
                    return s;
                }
            }
            return null;
        }

        internal void AddPortTypeOperation(string portTypeName, Operation operation)
        {
            if (operation != null && !serviceDescription.PortTypes[portTypeName].Operations.Contains(operation))
            {
                serviceDescription.PortTypes[portTypeName].Operations.Add(operation);
            }
        }

        internal void AddOperationBinding(string bindingName, OperationBinding operation)
        {
            if (operation != null && !serviceDescription.Bindings[bindingName].Operations.Contains(operation))
            {
                serviceDescription.Bindings[bindingName].Operations.Add(operation);
            }
        }

        internal void RemovePortTypeOperation(string portTypeName, Operation operation)
        {
            if (operation != null && serviceDescription.PortTypes[portTypeName].Operations.Contains(operation))
            {
                serviceDescription.PortTypes[portTypeName].Operations.Remove(operation);
            }
        }

        internal void RemoveOperationBinding(string bindingName, OperationBinding operation)
        {
            if (operation != null && serviceDescription.Bindings[bindingName].Operations.Contains(operation))
            {
                serviceDescription.Bindings[bindingName].Operations.Remove(operation);
            }
        }
    }

}
