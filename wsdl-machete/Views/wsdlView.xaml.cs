using ICSharpCode.AvalonEdit.Folding;
using Microsoft.Win32;
using System.ComponentModel;
using System.IO;
using System.Web.Services.Description;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Schema;
using wsdl_machete.Models;
using wsdl_machete.Utilities;
using System;
using System.Text;

namespace wsdl_machete
{
    public partial class wsdlView : Window
    {

        private WsdlDocument wsdlIn = new WsdlDocument();
        private WsdlDocument wsdlOut = new WsdlDocument();
        private XmlFoldingStrategy foldingStrategy = new XmlFoldingStrategy();
        private FoldingManager foldingManager = null;

        public wsdlView()
        {
            InitializeComponent();

            foldingManager = FoldingManager.Install(wsdlEditor.TextArea);

            RestoreWindow();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                Properties.Settings.Default.Top = RestoreBounds.Top;
                Properties.Settings.Default.Left = RestoreBounds.Left;
                Properties.Settings.Default.Height = RestoreBounds.Height;
                Properties.Settings.Default.Width = RestoreBounds.Width;
                Properties.Settings.Default.Maximized = true;
            }
            else
            {
                Properties.Settings.Default.Top = this.Top;
                Properties.Settings.Default.Left = this.Left;
                Properties.Settings.Default.Height = this.Height;
                Properties.Settings.Default.Width = this.Width;
                Properties.Settings.Default.Maximized = false;
            }

            Properties.Settings.Default.Save();
        }

        private void RestoreWindow()
        {
            if (WindowPositionIsRational())
            {
                this.Top = Properties.Settings.Default.Top;
                this.Left = Properties.Settings.Default.Left;
                this.Height = Properties.Settings.Default.Height;
                this.Width = Properties.Settings.Default.Width;
                if (Properties.Settings.Default.Maximized) { WindowState = WindowState.Maximized; }
            }
            else
            {
                this.Top = 0;
                this.Left = 0;
                this.Height = 400;
                this.Width = 600;
            }
        }

        private bool WindowPositionIsRational()
        {
            return (Properties.Settings.Default.Top >= SystemParameters.VirtualScreenTop &&
                Properties.Settings.Default.Left >= SystemParameters.VirtualScreenLeft &&
                Properties.Settings.Default.Height <= SystemParameters.VirtualScreenHeight &&
                Properties.Settings.Default.Width <= SystemParameters.VirtualScreenWidth);
        }

        private void File_Open_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "Web Service | *.wsdl;*.xml";
            if (openDialog.ShowDialog().Value)
            {
                string fileName = openDialog.FileName;
                statusFileName.Text = fileName;

                wsdlIn = new WsdlDocument(fileName, "Input File");
                wsdlOut = new WsdlDocument("mangled_" + fileName, "Output File");

                if (wsdlIn.LoadServiceDescription(wsdlIn.FileName))
                {
                    LoadWsdlTreeView(wsdlIn);
                }

            }
        }

        private void File_Close_Click(object sender, RoutedEventArgs e)
        {
            wsdlEditor.Text = "";
            wsdlTree.Items.Clear();
            wsdlIn = new WsdlDocument();
            wsdlOut = new WsdlDocument();
        }

        private void File_Exit_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }

        private void File_Save_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "Web Service | *.wsdl";
            saveDialog.OverwritePrompt = true;
            saveDialog.Title = "Save WSDL";
            if (saveDialog.ShowDialog().Value)
            {
                if (saveDialog.FileName != "")
                {
                    FileStream outputFile = (FileStream)saveDialog.OpenFile();
                    UnicodeEncoding encoding = new UnicodeEncoding();
                    outputFile.Write(encoding.GetBytes(wsdlEditor.Text), 0, encoding.GetByteCount(wsdlEditor.Text));
                    outputFile.Close();
                }
            }
        }

        private void Edit_Undo_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Sorry, no undo yet");
        }

        private void Edit_Redo_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Sorry, no redo yet");
        }

        private void LoadWsdlTreeView(WsdlDocument wsdl)
        {
            wsdlTree.Items.Clear();

            foreach(Service service in wsdl.serviceDescription.Services)
            {
                WsdlTreeViewItem serviceTreeItem = new WsdlTreeViewItem();
                serviceTreeItem.PropertyChanged += new PropertyChangedEventHandler(TreeItem_Change);

                serviceTreeItem.Name = service.Name;
                serviceTreeItem.Type = WsdlElementType.Service;
                foreach(Port port in service.Ports)
                {
                    string portBinding = port.Binding.Name;
                    foreach (Binding binding in wsdl.serviceDescription.Bindings)
                    {
                        if (binding.Name == portBinding)
                        {
                            WsdlTreeViewItem bindingTreeItem = new WsdlTreeViewItem();
                            bindingTreeItem.PropertyChanged += new PropertyChangedEventHandler(TreeItem_Change);
                            bindingTreeItem.Name = binding.Name;
                            bindingTreeItem.Parent = serviceTreeItem;
                            bindingTreeItem.Type = WsdlElementType.Binding;
                            foreach (OperationBinding operation in binding.Operations)
                            {
                                WsdlTreeViewItem operationTreeItem = new WsdlTreeViewItem();
                                operationTreeItem.PropertyChanged += new PropertyChangedEventHandler(TreeItem_Change);
                                operationTreeItem.Name = operation.Name;
                                operationTreeItem.Parent = bindingTreeItem;
                                operationTreeItem.Type = WsdlElementType.Operation;
                                bindingTreeItem.Children.Add(operationTreeItem);
                            }
                            bindingTreeItem.Children.Sort();
                            serviceTreeItem.Children.Add(bindingTreeItem);
                            SetUpOutputWsdl(wsdl);
                        }
                    }
                }
                wsdlTree.Items.Add(serviceTreeItem);
                wsdlTree.Items.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            }
        }



        private void SetUpOutputWsdl(WsdlDocument inputDocument)
        {
            wsdlOut.LoadServiceDescription(wsdlIn.FileName);

            foreach (Binding binding in wsdlOut.serviceDescription.Bindings)
            {
                binding.Operations.Clear();
            }

            foreach (PortType portType in wsdlOut.serviceDescription.PortTypes)
            {
                portType.Operations.Clear();
            }

            wsdlOut.serviceDescription.ValidationWarnings.Clear();

            UpdateDocumentView(wsdlOut);
        }

        private void TreeItem_Change(object sender, PropertyChangedEventArgs e)
        {
            WsdlTreeViewItem item = (WsdlTreeViewItem)sender;

            if (item.Type == WsdlElementType.Operation)
            {
                if (item.IsChecked.HasValue && item.IsChecked.Value)
                {
                    wsdlOut.AddPortTypeOperation(item.Parent.Parent.Name, GetPortTypeOperation(item));
                    wsdlOut.AddOperationBinding(item.Parent.Name, GetOperationBinding(item));
                } else
                if (item.IsChecked.HasValue && !item.IsChecked.Value)
                {
                    wsdlOut.RemovePortTypeOperation(item.Parent.Parent.Name, GetPortTypeOperation(item));
                    wsdlOut.RemoveOperationBinding(item.Parent.Name, GetOperationBinding(item));
                }
            }

            UpdateDocumentView(wsdlOut);

        }

        private OperationBinding GetOperationBinding(WsdlTreeViewItem item)
        {
            Binding bind = wsdlIn.serviceDescription.Bindings[item.Parent.Name];
            OperationBinding bindOperation = null;
            for (int i = 0; i < bind.Operations.Count; i++)
            {
                if (bind.Operations[i].Name == item.Name)
                {
                    bindOperation = bind.Operations[i];
                    break;
                }
            }

            return bindOperation;
        }

        private Operation GetPortTypeOperation(WsdlTreeViewItem item)
        {
            PortType pt = wsdlIn.serviceDescription.PortTypes[item.Parent.Parent.Name];
            Operation ptOperation = null;
            for (int i = 0; i < pt.Operations.Count; i++)
            {
                if (pt.Operations[i].Name == item.Name)
                {
                    ptOperation = pt.Operations[i];
                    break;
                }
            }

            return ptOperation;
        }

        private void UpdateDocumentView(WsdlDocument document)
        {
            StringWriter writer = new StringWriter();
            document.serviceDescription.Write(writer);
            wsdlEditor.Text = writer.GetStringBuilder().ToString();

            foldingStrategy.UpdateFoldings(foldingManager, wsdlEditor.Document);

        }
    }
}
