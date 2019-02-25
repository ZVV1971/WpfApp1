﻿using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.WindowsAPICodePack.Dialogs;
using MimeKit;
using System;
using System.ComponentModel;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
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
        BackgroundWorker bgWorker;
        StatusChecker sc;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            bgWorker = (BackgroundWorker)FindResource("bgWorker");
            sc = new StatusChecker();
            sc.Status = false;
            miStop.Header = "Остановить";

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

            bgWorker.RunWorkerAsync();
        }

        private void OnCreated(object source, FileSystemEventArgs e)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("KML", Properties.Settings.Default.DefaultEmailSender));
            message.To.Add(new MailboxAddress(toEmail));
            message.Subject = e.Name;

            var body = new TextPart("plain")
            {
                Text = "Документы от Частного предприятия \"КМЛ\"",
            };

            //MimePart attachment
            byte[] arr;
            while (true)
            {
                int i = 0;
                try
                {
                    arr = File.ReadAllBytes(e.FullPath);
                    break;
                }
                catch (IOException)
                {
                    Thread.Sleep(1000 * (++i));
                }
            }
            //can be done afterwards
            new Thread((x) =>
            {
                while (true)
                {
                    try
                    {
                        File.Delete((string)x);
                        break;
                    }
                    catch (IOException)
                    {
                        Thread.Sleep(5000);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Не удалось удалить файл: " + (string)x, "Ошибка при удалении файла",
                            MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        break;
                    }
                }
            }).Start(e.FullPath);

            MemoryStream mstr = new MemoryStream(arr);
            MimePart attachment = new MimePart("application", "pdf")
            {
                Content = new MimeContent(mstr, ContentEncoding.Default),
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            sc.Status = true;
            Close();
        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            using (FileSystemWatcher watcher = new FileSystemWatcher())
            {
                watcher.Path = WatchPath;
                watcher.NotifyFilter = NotifyFilters.FileName;
                watcher.Filter = Properties.Settings.Default.FilesToWatch ?? "*.pdf";
                watcher.Created += OnCreated;
                watcher.EnableRaisingEvents = true;

                var autoEvent = new AutoResetEvent(false);

                var workTimer = new Timer(sc.CheckStatus,
                                        autoEvent,
                                        0,     // initial wait period
                                        1000);  // subsequent wait period
                autoEvent.WaitOne();
            }
        }

        private void MiStop_Click(object sender, RoutedEventArgs e)
        {
            if (sc.Status) bgWorker.RunWorkerAsync();
            sc.Status = !sc.Status;
            miStop.Header = sc.Status ? "Запустить" : "Остановить";
        }
    }
}