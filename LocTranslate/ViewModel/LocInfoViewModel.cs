using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

using GalaSoft.MvvmLight;

namespace LocTranslate
{
    public class LocInfoViewModel : ViewModelBase
    {
        LocInfo _locInfo;

        public LocInfoViewModel(LocInfo locInfo)
        {
            _locInfo = locInfo;
        }
        public LocInfoViewModel(string fileName)
            : this(new LocInfo())
        {
            this.FileFullName = fileName;

            this.LoadInfo();
        }

        void LoadInfo()
        {
            // Get the xml document
            XmlReader reader = XmlReader.Create(this.FileFullName);
            XmlDocument doc = new XmlDocument();
            doc.Load(reader);

            XmlNamespaceManager xNM = new XmlNamespaceManager(doc.NameTable);
            xNM.AddNamespace(App.NSP_LTL, App.NS_LTL);
            xNM.AddNamespace(App.NSP_X, App.NS_X);


            // Get the info
            XmlNode infoNode = doc.DocumentElement.SelectSingleNode(String.Format("./{0}:LocInfo", App.NSP_LTL), xNM);
            if (infoNode != null)
            {
                if (infoNode.Attributes["TargetProject"] != null)
                    this.TargetProject = infoNode.Attributes["TargetProject"].Value;
                if (infoNode.Attributes["TargetProjectVersion"] != null)
                    this.TargetProjectVersion = infoNode.Attributes["TargetProjectVersion"].Value;

                this.Info = infoNode.Attributes["Info"] != null ?
                    infoNode.Attributes["Info"].Value : infoNode.InnerXml;
            }
        }


        public string FileName
        {
            get
            {
                return this._fileName;
            }
            set
            {
                this._fileName = value;
                RaisePropertyChanged("FileName");
            }
        }
        string _fileName = null;


        public string FileFullName
        {
            get
            {
                return this._fileFullName;
            }
            set
            {
                this._fileFullName = value;
                RaisePropertyChanged("FileFullName");

                this.FileName = Path.GetFileName(this._fileFullName);
            }
        }
        string _fileFullName = null;


        public string Key
        {
            get
            {
                return _locInfo.Key;
            }
            set
            {
                _locInfo.Key = value;
                RaisePropertyChanged("Key");
            }
        }

        public string TargetProject
        {
            get
            {
                return _locInfo.TargetProject;
            }
            set
            {
                _locInfo.TargetProject = value;
                RaisePropertyChanged("TargetProject");
            }
        }

        public string TargetProjectVersion
        {
            get
            {
                return _locInfo.TargetProjectVersion;
            }
            set
            {
                _locInfo.TargetProjectVersion = value;
                RaisePropertyChanged("TargetProjectVersion");
            }
        }

        public string Info
        {
            get
            {
                return _locInfo.Info;
            }
            set
            {
                _locInfo.Info = value;
                RaisePropertyChanged("Info");
                RaisePropertyChanged("HasInfo");
            }
        }

        public bool HasInfo
        {
            get
            {
                return (this.Info != "");
            }
        }
    }
}
