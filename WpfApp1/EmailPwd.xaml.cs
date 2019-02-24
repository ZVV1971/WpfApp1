using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SendMail
{
    /// <summary>
    /// Логика взаимодействия для EmailPwd.xaml
    /// </summary>
    public partial class EmailPwd : Window
    {
        private int errorCount = 0;

        public string ToEmail
        {
            get { return (string)GetValue(ToEmailProperty); }
            set { if (value != null) SetValue(ToEmailProperty, value); }
        }

        public static readonly DependencyProperty ToEmailProperty =
            DependencyProperty.Register(nameof(ToEmail),
                typeof(string), typeof(EmailPwd),
                new PropertyMetadata(Properties.Settings.Default.DefaultEmailRecepient ?? "don.juan@microsoft.com"),
                new ValidateValueCallback(validateEmailValue));

        public bool wrongPwd
        {
            get { return (bool)GetValue(wrongPwdProperty); }
            set { SetValue(wrongPwdProperty, value); }
        }

        public static readonly DependencyProperty wrongPwdProperty =
            DependencyProperty.Register(nameof(wrongPwd),
                typeof(bool), typeof(EmailPwd),
                new PropertyMetadata(true));

        public EmailPwd()
        {
            InitializeComponent();
            DataContext = this;
        }

        static bool validateEmailValue(object value)
        {
            string validEmailPattern = @"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|"
                + @"([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)"
                + @"@[a-z0-9][\w\.-]*[a-z0-9]\.[a-z][a-z\.]*[a-z]$";

            if (value == null || (string)value == string.Empty) return false;

            Regex ValidEmailRegex = new Regex(validEmailPattern, RegexOptions.IgnoreCase);
            return ValidEmailRegex.IsMatch((string)value);
        }

        private void SaveCommand_Executed (object sender, ExecutedRoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void SaveCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = errorCount == 0 && tbToEmail.Text != null && tbToEmail.Text != string.Empty && !wrongPwd;
        }

        private void Window_Error(object sender, ValidationErrorEventArgs e)
        {
            var validationEventArgs = e as ValidationErrorEventArgs;
            if (validationEventArgs == null) throw new Exception("Unexpected event args");
            switch (validationEventArgs.Action)
            {
                case ValidationErrorEventAction.Added:
                    { errorCount++; break; }
                case ValidationErrorEventAction.Removed:
                    { errorCount--; break; }
                default:
                    throw new Exception("Unknown action");
            }
        }

        private void PwdBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            wrongPwd = pwdBox.Password.Length < 6;
        }
    }
}