using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;

namespace TwitterBot
{
    class Mail
    {
        private String emailAddress;
        private String password;
        private String emailToAdress;

        public Mail(String emailAddress, String password, String emailToAdress)
        {
            this.emailAddress = emailAddress;
            this.password = password;
            this.emailToAdress = emailToAdress;
        }

        public void SendMail(String message, String asunto)
        {
            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");

                mail.From = new MailAddress(emailAddress);
                mail.To.Add(emailToAdress);
                mail.Subject = asunto;
                mail.Body = message;

                SmtpServer.Port = 587;
                SmtpServer.Credentials = new System.Net.NetworkCredential(emailToAdress, password);
                SmtpServer.EnableSsl = true;

                SmtpServer.Send(mail);
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
            }
        }
    }
}
