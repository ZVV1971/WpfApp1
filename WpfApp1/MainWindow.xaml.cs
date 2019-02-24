using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace SendMail
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string WatchPath;
        string toEmail;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                Title = "Выберитe каталог для отслеживания",
                EnsurePathExists = true,
                ShowPlacesList = true
            };
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                WatchPath = dialog.FileName;
            }
            else
            {
                Close();
            }

            EmailPwd epwd = new EmailPwd();
            if (epwd.ShowDialog() == true)
            {
                toEmail = epwd.ToEmail;
            }
            else Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}