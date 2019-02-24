using MailKit;
using MailKit.Security;
using MailKit.Net.Smtp;
using Microsoft.WindowsAPICodePack.Dialogs;
using MimeKit;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace SendMail
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string WatchPath;
        string toEmail;
        byte[] pwd;
        byte[] entropy = new byte[20];

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                Title = "Выберитe каталог для отслеживания",
                EnsurePathExists = true,
                ShowPlacesList = true,
                DefaultDirectory = Properties.Settings.Default.FolderToWatch ?? "C:"
            };
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                WatchPath = dialog.FileName;
                Properties.Settings.Default.FolderToWatch = dialog.FileName;
            }
            else
            {
                Close();
            }

            EmailPwd epwd = new EmailPwd();
            if (epwd.ShowDialog() == true)
            {
                toEmail = epwd.ToEmail;
                using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
                {
                    rng.GetBytes(entropy);
                }
                pwd = ProtectedData.Protect(Encoding.UTF8.GetBytes(epwd.pwdBox.Password), entropy,
                                            DataProtectionScope.CurrentUser);
            }
            else Close();

            using (FileSystemWatcher watcher = new FileSystemWatcher())
            {
                watcher.Path = WatchPath;
                watcher.NotifyFilter = NotifyFilters.FileName;
                watcher.Filter = Properties.Settings.Default.FilesToWatch ?? "*.pdf";
                watcher.Created += OnCreated;
                watcher.EnableRaisingEvents = true;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnCreated(object source, FileSystemEventArgs e)
        {
            string msg = @"Приказ на погрузку.";

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("KML", Properties.Settings.Default.DefaultEmailSender));
            message.To.Add(new MailboxAddress(toEmail));
            message.Subject = e.Name;

            var body = new TextPart("plain")
            {
                Text = "Документы от Частного предприятия \"КМЛ\"",
            };

            //MimePart attachment;
            byte[] arr = File.ReadAllBytes(e.FullPath);
            File.Delete(e.FullPath);
            MemoryStream mstr = new MemoryStream(arr);
            MimePart attachment = new MimePart("application", "pdf")
            {
                Content = new MimeContent(mstr//File.OpenRead(e.FullPath)
                , ContentEncoding.Default),
                ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                ContentTransferEncoding = ContentEncoding.Base64,
                FileName = e.Name
            };
            var multipart = new Multipart("mixed");
            multipart.Add(body);
            multipart.Add(attachment);

            // now set the multipart/mixed as the message body
            message.Body = multipart;

            SendMessage(message, Encoding.UTF8.GetString(ProtectedData.Unprotect(pwd, entropy, DataProtectionScope.CurrentUser)));
        }

        private void SendMessage(MimeMessage message, string pwd)
        {
            using (var client = new SmtpClient(new ProtocolLogger(Path.Combine(WatchPath, "smtp.log"))))
            {
                client.Connect("smtp.yandex.ru", 465, SecureSocketOptions.SslOnConnect);
                client.Authenticate(Properties.Settings.Default.DefaultEmailSender, pwd);
                client.Send(message);
                client.Disconnect(true);
            }
        }
    }
}