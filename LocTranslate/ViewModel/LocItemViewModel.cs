using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;

namespace LocTranslate
{
    public class LocItemViewModel : ViewModelBase
    {
        LocItem _locItem;

        public LocItemViewModel(LocItem locItem)
        {
            this._locItem = locItem;
        }

        public string Key 
        {
            get
            {
                return _locItem.Key;
            }
            set
            {
                _locItem.Key = value;
                RaisePropertyChanged("Key");
            }
        }

        public string OriginalValue
        {
            get
            {
                return _locItem.OriginalValue;
            }
            set
            {
                _locItem.OriginalValue = value;
                RaisePropertyChanged("OriginalValue");
            }
        }

        public string LocalizedValue
        {
            get
            {
                return _locItem.LocalizedValue;
            }
            set
            {
                _locItem.LocalizedValue = value;
                RaisePropertyChanged("LocalizedValue");
                Messenger.Default.Send<TMsgLocItemChanged>(new TMsgLocItemChanged());
            }
        }

        public bool Deprecated
        {
            get
            {
                return _locItem.Deprecated;
            }
            set
            {
                _locItem.Deprecated = value;
                RaisePropertyChanged("Deprecated");
            }
        }

        public string Comment
        {
            get
            {
                return _locItem.Comment;
            }
            set
            {
                _locItem.Comment = value;
                RaisePropertyChanged("Comment");
                Messenger.Default.Send<TMsgLocItemChanged>(new TMsgLocItemChanged());
            }
        }
    }
}
