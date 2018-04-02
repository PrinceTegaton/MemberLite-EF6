using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

public class CustomErrorLogger
{

    public DateTime DateStamp { get; set; }
    public string Code { get; set; }
    public string Message { get; set; }
    public string Page { get; set; }
    public string IP { get; set; }

    public static void Log(string Message, string Code = "", string Page = "")
    {
        try
        {
            string file = HttpContext.Current.Server.MapPath("~/App_Data/ErrorLog.xml");
            XDocument errorLog = XDocument.Load(file);
            if (string.IsNullOrEmpty(Code))
                Code = HttpContext.Current.Response.StatusCode.ToString();
            if (string.IsNullOrEmpty(Page))
                Page = HttpContext.Current.Request.Url.AbsolutePath.ToString();

            errorLog.Root.Add(new XElement("log", new XAttribute("code", Code),
                new XAttribute("message", Message),
                new XAttribute("datetime", DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString()),
                new XAttribute("page", Page),
                new XAttribute("ip", AppUtility.GetUserIPAddress())));
            errorLog.Save(file);
        }
        catch (Exception ex)
        {
            string msg = ex.Message;
        }
    }

    public static List<CustomErrorLogger> LoadLog()
    {
        string file = HttpContext.Current.Server.MapPath("~/App_Data/ErrorLog.xml");
        XDocument errorLog = XDocument.Load(file);
        List<CustomErrorLogger> list = new List<CustomErrorLogger>();

        CustomErrorLogger log;
        foreach (var e in errorLog.Root.Elements())
        {
            log = new CustomErrorLogger
            {
                DateStamp = DateTime.Parse(e.Attribute("datetime").Value),
                Code = e.Attribute("code").Value,
                Message = e.Attribute("message").Value,
                Page = e.Attribute("page").Value,
                IP = e.Attribute("ip").Value
            };
            list.Add(log);
        }
        return list;
        //.OrderBy(Function(p) p.Key)
    }

    public static bool Clear()
    {
        string file = HttpContext.Current.Server.MapPath("~/App_Data/ErrorLog.xml");
        XDocument errorLog = XDocument.Load(file);
        errorLog.Root.Elements().Remove();
        errorLog.Save(file);

        return true;
    }
}