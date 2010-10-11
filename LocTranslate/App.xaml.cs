using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.IO;

namespace LocTranslate
{
    public partial class App : Application
    {
        public const string NS_X = "http://schemas.microsoft.com/winfx/2006/xaml"; //NS = Namespace
        public const string NSP_X = "x";                                           //NSP = Namespace prefix
        public const string NS_LTL = "clr-namespace:LocTranslate;assembly=LocTranslateLibrary";
        public const string NSP_LTL = "ltl";

        public static string TITLE = "LocTranslator";
        public static string VERSION = "1.0";

        private MainWindow _mainWindow;

        // Shows the main window.
        protected override void OnStartup(StartupEventArgs e)
        {
            // This must be called after the initialization
            SetLocalizedStrings();

            this._mainWindow = new MainWindow();
            this._mainWindow.Show();
        }

        //Gets the localized strings file and adds it to the the application resources
        private void SetLocalizedStrings()
        {
            ResourceDictionary dictionary = new ResourceDictionary();
            string cultureName = System.Threading.Thread.CurrentThread.CurrentCulture.Name;

            Uri languageUri = new Uri(Path.Combine(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Locales"),
                String.Format("LocTable-{0}.xaml", cultureName)));

            //If the localized StringTable doesn't exist, the default (compiled)
            //StringTable will be used.
            if (File.Exists(languageUri.LocalPath))
            {
                try
                {
                    dictionary.Source = languageUri;

                    this.Resources.MergedDictionaries.Add(dictionary);
                }
                catch { } //No action, the default LocTable.xaml is used
            }
        }
    }
}
