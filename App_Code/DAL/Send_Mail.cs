using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;

/// <summary>
/// Summary description for Send_Mail
/// </summary>
public class Send_Mail
{
    public Send_Mail()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    public static void Send(string toAddress, string cc, string subject, string body)
    {
        try
        {
            // Set the sender's address
            string fromAddress = "noreply@nortwest.edu.au";


            // Set up the SMTP client
            SmtpClient smtpClient = new SmtpClient();
            smtpClient.Host = "smtp.gmail.com"; // replace with your SMTP server
            smtpClient.Port = 587; // replace with your SMTP port (usually 587 for TLS or 465 for SSL)
            smtpClient.EnableSsl = true;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential("noreply@nortwest.edu.au", "sxzj uvxc uayn cjlm"); // replace with your SMTP username and password

            // Create the MailMessage object
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress(fromAddress);
            mailMessage.To.Add(toAddress);
            if (cc != "")
            {
                mailMessage.CC.Add(cc);
            }
            mailMessage.Subject = subject;
            mailMessage.Body = body;
            mailMessage.IsBodyHtml = true;

            // Send the email
            smtpClient.Send(mailMessage);

            Console.WriteLine("Email sent successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred while sending the email: " + ex.Message);
        }
    }

}