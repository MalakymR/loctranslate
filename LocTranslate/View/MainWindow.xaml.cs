using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GalaSoft.MvvmLight.Messaging;

namespace LocTranslate
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Messenger.Default.Register<TMsgFileNotFound>(this, MsgFileNotFound_Handler);
            Messenger.Default.Register<DialogMessage>(this, MsgDisplayMessage_Handler);
            Messenger.Default.Register<TMsgShowAbout>(this, MsgShowAbout_Handler);

            this.DataContext = new MainWindowViewModel();
        }

        protected override void OnClosed(EventArgs e)
        {
            Messenger.Default.Unregister(this);
            Messenger.Default.Unregister(this.DataContext);
            base.OnClosed(e);
        }

        private void MsgFileNotFound_Handler(TMsgFileNotFound msg)
        {
            switch (msg.FileType)
            {
                case TMsgFileNotFound.FileTypes.Destination:
                    MessageBox.Show("Destination file not found.",
                        "LocTableTranslate", MessageBoxButton.OK, MessageBoxImage.Stop);
                    break;

                case TMsgFileNotFound.FileTypes.Source:
                    MessageBox.Show("Source file not found.",
                        "LocTableTranslate", MessageBoxButton.OK, MessageBoxImage.Stop);
                    break;
            }
        }

        private void MsgDisplayMessage_Handler(DialogMessage msg)
        {
            MessageBoxResult r = MessageBox.Show(msg.Content, msg.Caption, msg.Button, msg.Icon);

            msg.ProcessCallback(r);
        }

        private void MsgShowAbout_Handler(TMsgShowAbout msg)
        {
            WndAbout wndAbout = new WndAbout();
            wndAbout.Owner = this;
            wndAbout.ShowDialog();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(
                new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri));

            e.Handled = true;
        }
    }
}
