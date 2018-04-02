using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Net.Mail;
using System.Text;

public class WebMailer
{
    public static string Host { get { return ConfigurationManager.AppSettings["mail:Host"]; } }
    public static int Port { get { return Convert.ToInt32(ConfigurationManager.AppSettings["mail:Port"]); } }
    public static bool UsePort { get { return Convert.ToBoolean(ConfigurationManager.AppSettings["mail:UsePort"]); } }
    public static bool EnableSSL { get { return Convert.ToBoolean(ConfigurationManager.AppSettings["mail:EnableSSL"]); } }
    public static int Timeout { get { return Convert.ToInt32(ConfigurationManager.AppSettings["mail:Timeout"]); } }

    public static string Support { get { return ConfigurationManager.AppSettings["mail:Support"]; } }
    public static string SupportPwd { get { return ConfigurationManager.AppSettings["mail:SupportPassword"]; } }
    public static string SupportSender { get { return ConfigurationManager.AppSettings["mail:SupportSender"]; } }
    public static string Alert { get { return ConfigurationManager.AppSettings["mail:Alert"]; } }
    public static string AlertPwd { get { return ConfigurationManager.AppSettings["mail:AlertPassword"]; } }
    public static string AlertSender { get { return ConfigurationManager.AppSettings["mail:AlertSender"]; } }

    public static bool UseAsync { get; set; }
    public static object AsyncCallback { get; set; }
    public static string ReplyTo { get; set; }

    public static string ReturnMessage;

    public static bool Send(string SenderAddress, string RecipientAddress, string Subject, string Body,
                                bool IsHtmlMessage, bool MultipleRecipient = false)
    {

        string str = "", pwd = "";
        if (SenderAddress == Support)
        {
            str = SupportSender;
            pwd = SupportPwd;
        }
        else if (SenderAddress == Alert)
        {
            str = AlertSender;
            pwd = AlertPwd;
        }

        MailAddress from = new MailAddress(SenderAddress, string.Join(" - ", AppConfig.Name, str), Encoding.UTF8);
        MailMessage message = new MailMessage();
        SmtpClient SMTP = new SmtpClient();
        try
        {
            if (!MultipleRecipient)
            {
                MailAddress mailto = new MailAddress(RecipientAddress);
                message = new MailMessage(from, mailto);
            }
            else
            {
                message.From = from;
                string[] r = RecipientAddress.Split(',');
                for (int i = 0; i <= r.Length - 1; i++)
                {
                    message.To.Add(r[i].Trim());
                }
            }

            SMTP.UseDefaultCredentials = false;
            SMTP.Host = Host;
            if (UsePort) SMTP.Port = Port;
            SMTP.EnableSsl = EnableSSL;
            SMTP.Timeout = Timeout;
            SMTP.Credentials = new System.Net.NetworkCredential(SenderAddress, pwd);

            message.Body = Body;
            message.IsBodyHtml = IsHtmlMessage;
            message.BodyEncoding = Encoding.UTF8;
            message.Subject = Subject;
            if (!string.IsNullOrEmpty(ReplyTo))
                message.ReplyToList.Add(ReplyTo);
            if (UseAsync)
            {
                SMTP.SendAsync(message, AsyncCallback);
            }
            else
            {
                SMTP.Send(message);
            }

            return true;

        }
        catch (Exception ex)
        {
            ReturnMessage = ex.Message;
        }
        finally
        {
            message.Dispose();
        }

        return false;
    }
}