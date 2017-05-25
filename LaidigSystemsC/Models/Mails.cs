using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Mail;
using System.IO;

namespace LaidigSystemsC.Models
{
    public class Mails
    {
        public string To { get; set; }

        public string From { get; set; }

        public string Body { get; set; }

        public bool IsHtml { get; set; }

        public string Subject { get; set; }

        public string Attachment { get; set; }

        public int Port { get; set; }

        public string SMTP { get; set; }

        public bool SSL { get; set; }

        public string Password { get; set; }


        public void SendActivationLinkUsingEmail(UserAccount userregister, string message,string sessionuser)
        {
            MailMessage mail = new MailMessage();

            SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");

            mail.From = new MailAddress("coredemo16@gmail.com");

            //mail.Bcc.Add("jason.bakelaar@corelearningexchange.com");
            //mail.Bcc.Add("jeff.katzman@corelearningexchange.com");


            mail.To.Add("romit.khiru@gmail.com");

            mail.Subject = "Confirmation email for account activation";
           
            string body = "Hi " + sessionuser + "!\n" + message+
              "  \nThanks!";

            mail.Body = body.ToString();
            mail.IsBodyHtml = true;

            SmtpServer.Port = 587;

            SmtpServer.Credentials = new System.Net.NetworkCredential("kumarvikash842@gmail.com", "iloveyou@rosy");

            SmtpServer.EnableSsl = true;
            SmtpServer.Send(mail);
        }

        public void SendForgotPasswordUsingEmail(UserAccount propLogin, string ActivationUrl)
        {
            MailMessage mail = new MailMessage();

            SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");

            mail.From = new MailAddress("coredemo16@gmail.com");

            //mail.Bcc.Add("jason.bakelaar@corelearningexchange.com");
            //mail.Bcc.Add("jeff.katzman@corelearningexchange.com");


            mail.To.Add(propLogin.Email);

            mail.Subject = "Chage Password";

            string body = "Hi " + propLogin.Email + "!\n" +
              " Please <a href='" + ActivationUrl + "'>click here to chage password</a>  your account. \nThanks!";

            mail.Body = body.ToString();
            mail.IsBodyHtml = true;

            SmtpServer.Port = 587;

            SmtpServer.Credentials = new System.Net.NetworkCredential("coredemo16@gmail.com", "coredemo16@1");

            SmtpServer.EnableSsl = true;
            SmtpServer.Send(mail);
        }
        public void SendActivationLinkUsingEmailInstantFileMovedtoDB(string username, string message)
        {
            MailMessage mail = new MailMessage();

            SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");

            mail.From = new MailAddress("kumarvikash842@gmail.com");

            //mail.Bcc.Add("jason.bakelaar@corelearningexchange.com");
            //mail.Bcc.Add("jeff.katzman@corelearningexchange.com");


            mail.To.Add("romit.khiru@gmail.com");

            mail.Subject = "Confirmation email for  File moved to database";

            string body = "Hi " + username + " " + message + "!\n";
             

            mail.Body = body.ToString();
            mail.IsBodyHtml = true;

            SmtpServer.Port = 587;

            SmtpServer.Credentials = new System.Net.NetworkCredential("kumarvikash842@gmail.com", "iloveyou@rosy");

            SmtpServer.EnableSsl = true;
            SmtpServer.Send(mail);
        }

        public void SendActivationLinkUsingEmailDelayedFileMoveToDB(string message)
        {
            MailMessage mail = new MailMessage();

            SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");

            mail.From = new MailAddress("kumarvikash842@gmail.com");

            //mail.Bcc.Add("jason.bakelaar@corelearningexchange.com");
            //mail.Bcc.Add("jeff.katzman@corelearningexchange.com");


            mail.To.Add("romit.khiru@gmail.com");

            mail.Subject = "Confirmation email for Delayed upload File moved to database";

            string body = "Hi "  + message + "!\n";


            mail.Body = body.ToString();
            mail.IsBodyHtml = true;

            SmtpServer.Port = 587;

            SmtpServer.Credentials = new System.Net.NetworkCredential("kumarvikash842@gmail.com", "iloveyou@rosy");

            SmtpServer.EnableSsl = true;
            SmtpServer.Send(mail);
        }
        public void SendActivationLinkUsingEmailfileUpload(string user, string message)
        {
            MailMessage mail = new MailMessage();

            SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");

            mail.From = new MailAddress("kumarvikash842@gmail.com");

            //mail.Bcc.Add("jason.bakelaar@corelearningexchange.com");
            //mail.Bcc.Add("jeff.katzman@corelearningexchange.com");


            mail.To.Add("romit.khiru@gmail.com");

            mail.Subject = "Confirmation email for  File upload!!!";

            string body = "Hi " + user + "!\n" + message;



            mail.Body = body.ToString();
            mail.IsBodyHtml = true;

            SmtpServer.Port = 587;

            SmtpServer.Credentials = new System.Net.NetworkCredential("kumarvikash842@gmail.com", "iloveyou@rosy");

            SmtpServer.EnableSsl = true;
            SmtpServer.Send(mail);
        }

        public void SendActivationLinkUsingEmailUserUpdate(string user, string message)
        {
            MailMessage mail = new MailMessage();

            SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");

            mail.From = new MailAddress("kumarvikash842@gmail.com");

            //mail.Bcc.Add("jason.bakelaar@corelearningexchange.com");
            //mail.Bcc.Add("jeff.katzman@corelearningexchange.com");


            mail.To.Add("romit.khiru@gmail.com");

            mail.Subject = "Confirmation email for User profile Update!!!";

            string body = "Hi " + user + "!\n" + message;



            mail.Body = body.ToString();
            mail.IsBodyHtml = true;

            SmtpServer.Port = 587;

            SmtpServer.Credentials = new System.Net.NetworkCredential("kumarvikash842@gmail.com", "iloveyou@rosy");

            SmtpServer.EnableSsl = true;
            SmtpServer.Send(mail);
        }
        public void SendActivationLinkUsingEmailUserDelete(string user, string message)
        {
            MailMessage mail = new MailMessage();

            SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");

            mail.From = new MailAddress("kumarvikash842@gmail.com");

            //mail.Bcc.Add("jason.bakelaar@corelearningexchange.com");
            //mail.Bcc.Add("jeff.katzman@corelearningexchange.com");


            mail.To.Add("romit.khiru@gmail.com");

            mail.Subject = "Confirmation email for User profile Delete!!!";

            string body = "Hi " + user + "!\n" + message;



            mail.Body = body.ToString();
            mail.IsBodyHtml = true;

            SmtpServer.Port = 587;

            SmtpServer.Credentials = new System.Net.NetworkCredential("kumarvikash842@gmail.com", "iloveyou@rosy");

            SmtpServer.EnableSsl = true;
            SmtpServer.Send(mail);
        }
        //public void SendForgotPasswordUsingEmail(Login propLogin, string ActivationUrl)
        //{
        //    MailMessage mail = new MailMessage();

        //    SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");

        //    mail.From = new MailAddress("coredemo16@gmail.com");

        //    //mail.Bcc.Add("jason.bakelaar@corelearningexchange.com");
        //    //mail.Bcc.Add("jeff.katzman@corelearningexchange.com");


        //    mail.To.Add(propLogin.Email);

        //    mail.Subject = "Chage Password";

        //    string body = "Hi " + propLogin.Email + "!\n" +
        //      " Please <a href='" + ActivationUrl + "'>click here to chage password</a>  your account. \nThanks!";

        //    mail.Body = body.ToString();
        //    mail.IsBodyHtml = true;

        //    SmtpServer.Port = 587;

        //    SmtpServer.Credentials = new System.Net.NetworkCredential("coredemo16@gmail.com", "coredemo16@1");

        //    SmtpServer.EnableSsl = true;
        //    SmtpServer.Send(mail);
        //}
        //public void SendPdfUrl(string useremail, string PdfDownloadUrl, string reportName)
        //{
        //    MailMessage mail = new MailMessage();

        //    SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");

        //    mail.From = new MailAddress("coredemo16@gmail.com");

        //    //mail.Bcc.Add("jason.bakelaar@corelearningexchange.com");
        //    //mail.Bcc.Add("jeff.katzman@corelearningexchange.com");


        //    mail.To.Add(useremail);

        //    mail.Subject = "Report Generated Successfully";
        //    //mail.Attachments.Add(new Attachment(PdfDownloadUrl));
        //    //reportName has been generated.


        //    string body = "" + reportName + " has been generated." + "\n" +
        //        " Please <a href='" + "http://10.2.0.140/VSPAPPLICATION3/VSP/" + reportName + ".xls" + "'>click to download report</a> now";
        //    //string body = "please <a href='" + "http://10.2.0.140/VSPAPPLICATION/VSP/VSP7150.xls" + "'>click to open report</a> now";


        //    mail.Body = body.ToString();
        //    mail.IsBodyHtml = true;

        //    SmtpServer.Port = 587;

        //    SmtpServer.Credentials = new System.Net.NetworkCredential("coredemo16@gmail.com", "coredemo16@1");

        //    SmtpServer.EnableSsl = true;
        //    SmtpServer.Send(mail);
    }
}