using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LocTranslate
{
    public class TMsgFileNotFound
    {
        public enum FileTypes
        {
            Source,
            Destination
        }

        public TMsgFileNotFound(FileTypes fileType)
        {
            this.FileType = fileType;
        }

        public FileTypes FileType { get; set; }
    }

    public class TMsgLocItemChanged
    {
        public TMsgLocItemChanged()
        { }
    }

    public class TMsgClosing
    {
        bool _cancel = false;
        public TMsgClosing(bool cancel)
        {
            this._cancel = cancel;
        }
    }

    public class TMsgShowAbout
    {
        public TMsgShowAbout()
        { }
    }
}
