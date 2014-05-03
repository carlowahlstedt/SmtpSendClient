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
using System.Net.Mail;
using System.Net;
using System.Timers;
using System.ComponentModel;

namespace SmtpSendClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Timer _timer = new Timer(10000);
        private BackgroundWorker _backgroundWorker = new BackgroundWorker();
        private string _to = string.Empty;
        private string _from = string.Empty;
        private string _subject = string.Empty;
        private string _message = string.Empty;
        private string _host = string.Empty;
        private string _login = string.Empty;
        private string _password = string.Empty;
        private string _domain = string.Empty;

        public MainWindow()
        {
            InitializeComponent();

            _timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
            _timer.AutoReset = false;

            _backgroundWorker.DoWork += new DoWorkEventHandler(backgroundWorker_DoWork);
            _backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker_RunWorkerCompleted);

            this.txtTo.Focus();
        }

        /// <summary>
        /// Event that fires when the send button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();

            this.lblSendComplete.Content = "Sending...";
            this.lblSendComplete.Foreground = new SolidColorBrush(Colors.Black);
            this.lblSendComplete.Visibility = System.Windows.Visibility.Visible;

            _to = this.txtTo.Text;
            _from = this.txtFrom.Text;
            _subject = this.txtSubject.Text;
            _message = this.txtMessage.Text;
            _host = this.txtHost.Text;
            _login = this.txtLogin.Text;
            _password = this.pwbPassword.Password;
            _domain = this.txtDomain.Text;

            SetReadOnly(true);

            this._backgroundWorker.RunWorkerAsync();
        }

        /// <summary>
        /// Does the work of the background worker
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                MailMessage mailMessage = new MailMessage();
                mailMessage.To.Add(_to);
                mailMessage.From = new MailAddress(_from);
                mailMessage.Subject = _subject;
                mailMessage.Body = _message;

                SmtpClient smtpClient = new SmtpClient(_host);
                if (!string.IsNullOrEmpty(_login) && !string.IsNullOrEmpty(_password))
                {
                    NetworkCredential networkCredential = null;
                    if (string.IsNullOrEmpty(_domain))
                    {
                        networkCredential = new NetworkCredential(_login, _password);
                    }
                    else
                    {
                        networkCredential = new NetworkCredential(_login, _password, _domain);
                    }

                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = networkCredential;
                }

                smtpClient.Send(mailMessage);

                this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
                    new System.Windows.Threading.DispatcherOperationCallback(delegate
                    {
                        this.lblSendComplete.Content = "Send Complete";
                        this.lblSendComplete.Foreground = new SolidColorBrush(Colors.Black);
                        return null;
                    }), null);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Sending Email\r\n Exception Message:\r\n" + ex.Message + "\r\n StackTrace:\r\n" + ex.StackTrace, "Error");
                this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
                    new System.Windows.Threading.DispatcherOperationCallback(delegate
                    {
                        this.lblSendComplete.Content = "Error Sending";
                        this.lblSendComplete.Foreground = new SolidColorBrush(Colors.Red);
                        return null;
                    }), null);
            }
        }

        /// <summary>
        /// Event that fires when the background worker is complete
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            SetReadOnly(false);
            _timer.Start();
            this.lblSendComplete.Visibility = System.Windows.Visibility.Visible;
        }

        /// <summary>
        /// Fires when the timer elapses
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
                new System.Windows.Threading.DispatcherOperationCallback(delegate
                {
                    this.lblSendComplete.Visibility = System.Windows.Visibility.Hidden;
                    return null;
                }), null);
        }

        /// <summary>
        /// Sets the fields to be read-only or not
        /// </summary>
        /// <param name="readOnly"></param>
        private void SetReadOnly(bool readOnly)
        {
            this.txtTo.IsReadOnly = readOnly;
            this.txtFrom.IsReadOnly = readOnly;
            this.txtSubject.IsReadOnly = readOnly;
            this.txtMessage.IsReadOnly = readOnly;
            this.txtHost.IsReadOnly = readOnly;
            this.txtLogin.IsReadOnly = readOnly;
            this.pwbPassword.IsEnabled = !readOnly;
            this.txtDomain.IsReadOnly = readOnly;
            
            this.btnSend.IsEnabled = !readOnly;
        }
    }
}
