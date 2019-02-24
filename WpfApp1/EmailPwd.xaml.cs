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
using System.Windows.Shapes;

namespace SendMail
{
    /// <summary>
    /// Логика взаимодействия для EmailPwd.xaml
    /// </summary>
    public partial class EmailPwd : Window
    {
        public string ToEmail
        {
            get { return (string)GetValue(ToEmailProperty); }
            set { if (value != null) SetValue(ToEmailProperty, value); }
        }

        public static readonly DependencyProperty ToEmailProperty =
            DependencyProperty.Register(nameof(ToEmail),
                typeof(string), typeof(EmailPwd),
                new PropertyMetadata("")
                //,new ValidateValueCallback(validateUserNameValue)
                );


        public EmailPwd()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void SaveCommand_Executed (object sender, ExecutedRoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void SaveCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {

        }
    }
}
