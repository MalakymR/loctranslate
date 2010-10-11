using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Xml;
using System.IO;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

namespace LocTranslate
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            this.LocItems = new ObservableCollection<LocItemViewModel>();

            Messenger.Default.Register<TMsgLocItemChanged>(this, MsgLocItemChanged_Handler);
        }


        /// <summary>
        /// Get or set a LocInfoViewModel object to store informations about the original document
        /// </summary>
        public LocInfoViewModel OriginalFile
        {
            get
            {
                return this._originalFile;
            }
            set
            {
                this._originalFile = value;
                RaisePropertyChanged("OriginalFile");
            }
        }
        LocInfoViewModel _originalFile = null;

        
        /// <summary>
        /// Get or set a LocInfoViewModel object to store informations about the localized document
        /// </summary>
        public LocInfoViewModel LocalizedFile
        {
            get
            {
                return this._localizedFile;
            }
            set
            {
                this._localizedFile = value;
                RaisePropertyChanged("LocalizedFile");
            }
        }
        LocInfoViewModel _localizedFile = null;

        
        /// <summary>
        /// Indicates whether the warning message has to be shown
        /// </summary>
        public bool ShowWarning
        {
            get
            {
                return this._showWarning;
            }
            set
            {
                this._showWarning = value;
                RaisePropertyChanged("ShowWarning");
            }
        }
        bool _showWarning = false;

        /// <summary>
        /// Indicates whether either original or localized file is changed
        /// </summary>
        public bool IsChanged { get; set; }

        /// <summary>
        /// List of items that needs to be localized
        /// </summary>
        public ObservableCollection<LocItemViewModel> LocItems { get; private set; }


        #region CmdOpenOriginalFile

        RelayCommand _cmdOpenOriginalFile;
        public ICommand CmdOpenOriginalFile
        {
            get
            {
                if (this._cmdOpenOriginalFile == null)
                {
                    this._cmdOpenOriginalFile = new RelayCommand(() => this.DoCmdOpenOriginalFile());
                }
                return this._cmdOpenOriginalFile;
            }
        }
        private void DoCmdOpenOriginalFile()
        {
            string fileName = this.OpenXamlFile();

            if ((fileName != null) && (fileName != ""))
            {
                this.OriginalFile = new LocInfoViewModel(fileName);

                if (CmdRefreshItems.CanExecute(null))
                    CmdRefreshItems.Execute(null);

                if ((this.LocalizedFile != null) &&
                    (this.OriginalFile.TargetProject != this.LocalizedFile.TargetProject))
                {
                    this.ShowWarning = true;
                }
                else
                {
                    this.ShowWarning = false;
                }
            }
        }

        #endregion


        #region CmdOpenLocalizedFile

        RelayCommand _cmdOpenLocalizedFile;
        public ICommand CmdOpenLocalizedFile
        {
            get
            {
                if (this._cmdOpenLocalizedFile == null)
                {
                    this._cmdOpenLocalizedFile = new RelayCommand(() => this.DoCmdOpenLocalizedFile(),
                        () => this.CanDoCmdOpenLocalizedFile);
                }
                return this._cmdOpenLocalizedFile;
            }
        }
        private void DoCmdOpenLocalizedFile()
        {
            string fileName = this.OpenXamlFile();

            if ((fileName != null) && (fileName != ""))
            {
                this.LocalizedFile = new LocInfoViewModel(fileName);

                if (CmdRefreshItems.CanExecute(null))
                    CmdRefreshItems.Execute(null);

                if ((this.OriginalFile != null) &&
                    (this.OriginalFile.TargetProject != this.LocalizedFile.TargetProject))
                {
                    this.ShowWarning = true;
                }
                else
                {
                    this.ShowWarning = false;
                }
            }
        }
        private bool CanDoCmdOpenLocalizedFile
        {
            get
            {
                return this.OriginalFile != null;
            }
        }

        #endregion


        #region CmdRefreshItems

        RelayCommand _cmdRefreshItems;
        public ICommand CmdRefreshItems
        {
            get
            {
                if (this._cmdRefreshItems == null)
                {
                    this._cmdRefreshItems = new RelayCommand(() => this.DoCmdRefreshItems(),
                        () => CanDoCmdRefreshItems);
                }
                return this._cmdRefreshItems;
            }
        }
        private void DoCmdRefreshItems()
        {
            if ((this.OriginalFile == null) ||
                (!File.Exists(this.OriginalFile.FileFullName)))
            {
                Messenger.Default.Send<TMsgFileNotFound>(new TMsgFileNotFound(TMsgFileNotFound.FileTypes.Destination));
                return;
            }

            // Get the original xml document
            XmlReader reader = XmlReader.Create(this.OriginalFile.FileFullName);
            XmlDocument oDoc = new XmlDocument();
            oDoc.Load(reader);

            XmlNamespaceManager oxNM = new XmlNamespaceManager(oDoc.NameTable);
            oxNM.AddNamespace(App.NSP_LTL, App.NS_LTL);
            oxNM.AddNamespace(App.NSP_X, App.NS_X);

            
            // If both LocalizedFile and OriginalFile are provided I load both of them.
            // If only OriginalFile is provided I load it with empty LocalizedValue
            if ((this.LocalizedFile != null) && 
                (File.Exists(this.LocalizedFile.FileFullName)))
            {

                // Get the localized xml document
                reader = XmlReader.Create(this.LocalizedFile.FileFullName);
                XmlDocument lDoc = new XmlDocument();
                lDoc.Load(reader);

                XmlNamespaceManager lxNM = new XmlNamespaceManager(lDoc.NameTable);
                lxNM.AddNamespace(App.NSP_LTL, App.NS_LTL);
                lxNM.AddNamespace(App.NSP_X, App.NS_X);

                
                // Get the nodes
                this.LocItems.Clear();
                foreach (XmlNode node in oDoc.DocumentElement.SelectNodes(String.Format("./{0}:LocItem", App.NSP_LTL), oxNM))
                {
                    Console.WriteLine(node.InnerXml);

                    LocItem item = new LocItem();
                    item.Key = node.Attributes[String.Format("{0}:Key", App.NSP_X)].Value; // Surely I have a Key
                    item.OriginalValue = node.InnerXml;
                    XmlNode lNode = lDoc.DocumentElement.SelectSingleNode(
                        String.Format("./{0}:LocItem[@{1}:Key='{2}']", App.NSP_LTL, App.NSP_X, item.Key), lxNM);
                    item.LocalizedValue = lNode == null ? "" : lNode.InnerXml;
                    item.Comment = node.Attributes["Comment"] == null ? "" : node.Attributes["Comment"].Value;
                    item.Deprecated = node.Attributes["Deprecated"] == null ? false : bool.Parse(node.Attributes["Deprecated"].Value);

                    this.LocItems.Add(new LocItemViewModel(item));
                }
            }
            else if ((this.LocalizedFile == null) || ((this.LocalizedFile.FileFullName == "")))
            {
                // Get the nodes
                this.LocItems.Clear();
                foreach (XmlNode node in oDoc.DocumentElement.SelectNodes(String.Format("./{0}:LocItem", App.NSP_LTL), oxNM))
                {
                    Console.WriteLine(node.InnerXml);

                    LocItem item = new LocItem();
                    item.Key = node.Attributes[String.Format("{0}:Key", App.NSP_X)].Value; // Surely I have a Key
                    item.OriginalValue = node.Attributes["OriginalValue"] == null ? node.InnerXml : node.Attributes["OriginalValue"].Value;
                    item.LocalizedValue = ""; // node.InnerXml;
                    item.Comment = node.Attributes["Comment"] == null ? "" : node.Attributes["Comment"].Value;
                    item.Deprecated = node.Attributes["Deprecated"] == null ? false : bool.Parse(node.Attributes["Deprecated"].Value);

                    this.LocItems.Add(new LocItemViewModel(item));
                }
            }
            else
            {
                Messenger.Default.Send<TMsgFileNotFound>(new TMsgFileNotFound(TMsgFileNotFound.FileTypes.Destination));
            }

        }
        private bool CanDoCmdRefreshItems
        {
            get{
                return (this.OriginalFile != null);
            }
        }

        #endregion


        #region CmdSave

        RelayCommand _cmdSave;
        public ICommand CmdSave
        {
            get
            {
                if (this._cmdSave == null)
                {
                    this._cmdSave = new RelayCommand(() => this.DoCmdSave(),
                        () => CanDoCmdSave);
                }
                return this._cmdSave;
            }
        }
        private void DoCmdSave()
        {
            // Get the original xml document
            XmlReader reader = XmlReader.Create(this.OriginalFile.FileFullName);
            XmlDocument oDoc = new XmlDocument();
            oDoc.Load(reader);

            XmlNamespaceManager oxNM = new XmlNamespaceManager(oDoc.NameTable);
            oxNM.AddNamespace(App.NSP_LTL, App.NS_LTL);
            oxNM.AddNamespace(App.NSP_X, App.NS_X);


            foreach (LocItemViewModel item in this.LocItems)
            {
                XmlNode node = oDoc.DocumentElement.SelectSingleNode(
                    String.Format("./{0}:LocItem[@{1}:Key='{2}']", App.NSP_LTL, App.NSP_X, item.Key), oxNM);

                if (node != null)
                {
                    // Update "Comment" attribute
                    if (node.Attributes["Comment"] != null)
                    {
                        node.Attributes["Comment"].Value = item.Comment;
                    }
                    else if(item.Comment != "")
                    {
                        XmlAttribute a = oDoc.CreateAttribute("Comment");
                        a.Value = item.Comment;
                        node.Attributes.Append(a);
                    }

                    // Update node value
                    node.InnerXml = item.LocalizedValue;
                }
            }


            // If Localized file is not provided create an empty one
            if (this.LocalizedFile == null)
            {
                this.LocalizedFile = new LocInfoViewModel(new LocInfo());
            }

            // If localized file doesn't exists or is the same as the original file
            // open the SaveFileDialog window
            if ((!File.Exists(this.LocalizedFile.FileFullName)) ||
                (this.LocalizedFile.FileFullName == this.OriginalFile.FileFullName))
            {
                this.LocalizedFile.FileFullName = SaveXamlFile();
            }
            
            // If a valid file name has been provided, save the file
            if (this.LocalizedFile.FileFullName != "")
            {
                try
                {
                    // If something goes wrong, catch the error and display a message
                    oDoc.Save(this.LocalizedFile.FileFullName);

                    this.IsChanged = false;
                }
                catch
                {
                    DialogMessage message = new DialogMessage(App.Current.FindResource("loc_genericSaveError").ToString(),
                        x => x = MessageBoxResult.None)
                    {
                        Caption = App.TITLE,
                        Button = MessageBoxButton.OK,
                        Icon = MessageBoxImage.Error
                    };
                    Messenger.Default.Send(message);
                }
            }

        }
        private bool CanDoCmdSave
        {
            get
            {
                return this.IsChanged;
            }
        }

        #endregion


        #region CmdClosing

        RelayCommand<System.ComponentModel.CancelEventArgs> _cmdClosing;
        public ICommand CmdClosing
        {
            get
            {
                if (this._cmdClosing == null)
                {
                    this._cmdClosing = new RelayCommand<System.ComponentModel.CancelEventArgs>(
                        x => this.DoCmdClosing(x));
                }
                return this._cmdClosing;
            }
        }
        private void DoCmdClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (this.IsChanged)
            {
                DialogMessage message = new DialogMessage(App.Current.FindResource("loc_askForSave").ToString(),
                    x => this.CmdClosingCallback(x, e))
                    {
                        Caption = App.TITLE,
                        Button = MessageBoxButton.YesNoCancel,
                        Icon = MessageBoxImage.Question
                    };
                Messenger.Default.Send(message);
            }

        }

        private void CmdClosingCallback(MessageBoxResult result, System.ComponentModel.CancelEventArgs e)
        {
            if (result == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
            }
            else if (result == MessageBoxResult.Yes)
            {
                CmdSave.Execute(null);
            }
            else
            {
                e.Cancel = false;
            }
        }

        #endregion


        #region CmdAbout

        RelayCommand _cmdAbout;
        public ICommand CmdAbout
        {
            get
            {
                if (this._cmdAbout == null)
                {
                    this._cmdAbout = new RelayCommand(() => this.DoCmdAbout());
                }
                return this._cmdAbout;
            }
        }
        private void DoCmdAbout()
        {
            Messenger.Default.Send<TMsgShowAbout>(new TMsgShowAbout());
        }

        #endregion


        private string OpenXamlFile()
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Multiselect = false;
            ofd.Filter = "XAML file|*.xaml";
            ofd.ShowDialog();

            return ofd.FileName;
        }

        private string SaveXamlFile()
        {
            Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog();
            sfd.Filter = "XAML file|*.xaml";
            sfd.ShowDialog();

            return sfd.FileName;
        }

        private void MsgLocItemChanged_Handler(TMsgLocItemChanged msg)
        {
            
            this.IsChanged = true;
        }
    }
}

