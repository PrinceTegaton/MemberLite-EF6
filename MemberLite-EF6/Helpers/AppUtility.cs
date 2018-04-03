using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Linq;

public class AppUtility
{
    public static string ReturnMessage;

    private static HttpContext context = HttpContext.Current;

    public static readonly string AppDataPath = HttpContext.Current.Server.MapPath("~/App_Data/");
    
    public static string GetDeviceType()
    {
        return HttpContext.Current.Request.Browser.IsMobileDevice ? "mobile" : "desktop";
    }

    public static string GetUserIPAddress()
    {
        string sIPAddress = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
        if (string.IsNullOrEmpty(sIPAddress))
        {
            string remoteIP = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            return remoteIP == "::1" ? "127.0.0.1" : remoteIP;
        }
        else
        {
            string[] ipArray = sIPAddress.Split(new Char[] { ',' });
            return ipArray[0];
        }
    }

    public static string CleanUrlSlug(string Str, int MaxLength = 50)
    {
        Str = Str.Substring(0, Str.Length >= MaxLength ? MaxLength : Str.Length).ToLowerInvariant();
        //remove all accent
        dynamic bytes = Encoding.GetEncoding("Cyrillic").GetBytes(Str);
        Str = Encoding.ASCII.GetString(bytes);
        //replace whitespaces
        Str = Regex.Replace(Str, "\\s", "-", RegexOptions.Compiled);
        //remove invalid characters
        Str = Regex.Replace(Str, "[^\\w\\s\\p{Pd}]", "", RegexOptions.Compiled);
        //trim dashes from start and end
        Str = Str.Trim('-', '-');
        //replace double occurence of - or \_
        Str = Regex.Replace(Str, "([-_]){2,}", "$1", RegexOptions.Compiled);

        return Str;
    }

    public static bool IsNumeric(object Expression)
    {
        if (Expression == null || Expression is DateTime)
            return false;

        if (Expression is Int16 || Expression is Int32 || Expression is Int64 || Expression is Decimal || Expression is Single || Expression is Double || Expression is Boolean)
            return true;

        try
        {
            if (Expression is string)
                Double.Parse(Expression as string);
            else
                Double.Parse(Expression.ToString());
            return true;
        }
        catch { } // just dismiss errors but return false
        return false;
    }

    public static string CleanDomain(string Domain)
    {
        Domain = Domain.Replace("http://", "").Replace("https://", "").Replace("http://www.", "").Replace("https://www.", "").Replace("www.", "").Replace("/", "").Replace("\\", "");
        Domain = Domain.Split(':')[0];
        return Domain.ToLower();
    }
    
    public static string GenerateAlphaNumeric(int Length)
    {
        Random random = new Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        return new string(Enumerable.Repeat(chars, Length).Select(s => s[random.Next(s.Length)]).ToArray());
    }

    public static string GenerateNumeric(int Length)
    {
        Random random = new Random();
        const string chars = "0123456789";
        return new string(Enumerable.Repeat(chars, Length).Select(s => s[random.Next(s.Length)]).ToArray());
    }

    public static bool ValidateEmail(string Email)
    {
        Regex EmailRegex = new Regex("^(?<Username>[^@]+)@(?<host>.+)$");
        Match EmailMatch = EmailRegex.Match(Email.Trim());
        return EmailMatch.Success;
    }

    public static string GetUrlQueryString(string Key)
    {
        HttpRequest rq = HttpContext.Current.Request;
        if (rq.QueryString.HasKeys())
        {
            if (!string.IsNullOrEmpty(rq.QueryString[Key]))
            {
                return HttpContext.Current.Server.UrlDecode(rq.QueryString[Key].ToString());
            }
        }
        else
        {
            return "";
        }
        return "";
    }

    public static string DateTimeToWord(DateTime DateTimeString)
    {
        if ((DateTimeString == null)) return "...";
        long value; string str = "";

        if (DateDifference.DateDiff(DateDifference.DateInterval.Minute, DateTimeString, DateTime.Now) <= 2)
        {
            str = "now";
        }
        else if (DateDifference.DateDiff(DateDifference.DateInterval.Minute, DateTimeString, DateTime.Now) <= 60)
        {
            value = DateDifference.DateDiff(DateDifference.DateInterval.Minute, DateTimeString, DateTime.Now);
            str = value + " min" + (value > 1 ? "s " : " ") + "ago";
        }
        else if (DateDifference.DateDiff(DateDifference.DateInterval.Hour, DateTimeString, DateTime.Now) <= 24)
        {
            value = DateDifference.DateDiff(DateDifference.DateInterval.Hour, DateTimeString, DateTime.Now);
            str = value + " hr" + (value > 1 ? "s " : " ") + "ago";
        }
        else if (DateDifference.DateDiff(DateDifference.DateInterval.Day, DateTimeString, DateTime.Now) == 1)
        {
            value = DateDifference.DateDiff(DateDifference.DateInterval.Day, DateTimeString, DateTime.Now);
            str = " yst";
        }
        else if (DateDifference.DateDiff(DateDifference.DateInterval.Day, DateTimeString, DateTime.Now) <= 7)
        {
            value = DateDifference.DateDiff(DateDifference.DateInterval.Day, DateTimeString, DateTime.Now);
            str = value + " day" + (value > 1 ? "s " : " ") + "ago";
        }
        else if (DateDifference.DateDiff(DateDifference.DateInterval.Weekday, DateTimeString, DateTime.Now) <= 4)
        {
            value = DateDifference.DateDiff(DateDifference.DateInterval.Weekday, DateTimeString, DateTime.Now);
            str = value + " wk" + (value > 1 ? "s " : " ") + "ago";
        }
        else if (DateDifference.DateDiff(DateDifference.DateInterval.Month, DateTimeString, DateTime.Now) <= 12)
        {
            value = DateDifference.DateDiff(DateDifference.DateInterval.Month, DateTimeString, DateTime.Now);
            str = value + " mth" + (value > 1 ? "s " : " ") + "ago";
        }
        else if (DateDifference.DateDiff(DateDifference.DateInterval.Year, DateTimeString, DateTime.Now) >= 1)
        {
            value = DateDifference.DateDiff(DateDifference.DateInterval.Year, DateTimeString, DateTime.Now);
            str = value + " yr" + (value > 1 ? "s " : " ") + "ago";
        }

        return str;
    }

    public static string DigitStyle(dynamic Value)
    {
        double value = Convert.ToDouble(Value), before, after;
        string str = value.ToString();
        switch (value.ToString().Length)
        {
            case 4:
            case 5:
            case 6:
                before = Convert.ToDouble(str) / 1000;
                after = Math.Round(before, 1, MidpointRounding.ToEven);
                if (after == 100)
                {
                    str = 1 + "m";
                }
                str = after + "k";
                break;
            case 7:
            case 8:
            case 9:
                before = Convert.ToDouble(str) / 1000000;
                after = Math.Round(before, 1, MidpointRounding.ToEven);
                if (after == 1000)
                {
                    str = 1 + "b";
                }
                str = after + "m";
                break;
            case 10:
            case 11:
            case 12:
                before = Convert.ToDouble(str) / 1000000000;
                after = Math.Round(before, 1, MidpointRounding.ToEven);
                if (after == 1000)
                {
                    str = 1 + "t";
                }
                str = after + "b";
                break;
            case 13:
            case 14:
            case 15:
                before = Convert.ToDouble(str) / 1000000000000L;
                after = Math.Round(before, 1, MidpointRounding.ToEven);
                str = after + "t";
                break;
            default:
                return value.ToString();
        }

        return str;
    }

    public enum ConvertType
    {
        B = 0,
        KB = 1,
        MB = 2,
        GB = 3,
        TB = 4,
        PB = 5,
        EB = 6,
        ZI = 7,
        YI = 8
    }

    public static double ConvertSize(long Bytes, ConvertType ConvertTo)
    {
        return Math.Round(Bytes / (Math.Pow(1024, (int)ConvertTo)));
    }

    public static void SetCookie(string Cookie, string Value, int Expires = 7)
    {
        HttpResponse rsp = HttpContext.Current.Response;
        HttpCookie ck = new HttpCookie(Cookie, Value);
        ck.Expires = DateTime.Now.AddDays(Expires);
        rsp.Cookies.Add(ck);
    }

    public static void SetCookie(string Cookie, string Value, DateTime Expires, bool Secure = false, bool HTTPOnly = false)
    {
        HttpResponse rsp = HttpContext.Current.Response;
        HttpCookie ck = new HttpCookie(Cookie, Value);
        ck.Expires = Expires;
        ck.Secure = Secure;
        ck.HttpOnly = HTTPOnly;
        rsp.Cookies.Add(ck);
    }

    public static string GetCookie(string Cookie)
    {
        HttpRequest req = HttpContext.Current.Request;
        if ((req.Cookies[Cookie] != null))
        {
            return req.Cookies[Cookie].Value;
        }
        return "";
    }

    public static void DeleteCookie(string Cookie)
    {
        HttpResponse rsp = HttpContext.Current.Response;
        rsp.Cookies.Remove(Cookie);
    }

    public class Country
    {
        public string Name { get; set; }
        public string Code { get; set; }

        public static IEnumerable<Country> Load()
        {
            var list = new List<Country> { };
            var countryXml = new XmlDocument();
            countryXml.Load(AppDataPath + "Countries.xml");

            foreach (XmlNode node in countryXml.SelectSingleNode("/countries"))
            {
                list.Add(new Country { Name = node.InnerText, Code = node.Attributes["code"].InnerText });
            }
            return list;
        }

        public static string Get(string Code)
        {
            if (Code != "")
            {
                var countryXml = new XmlDocument();
                countryXml.Load(AppUtility.AppDataPath + "Countries.xml");
                XmlNode node = countryXml.SelectSingleNode("/countries/country[@code='" + Code + "']");
                return node.InnerText;
            }
            return "";
        }

        public static string Flag(string Code)
        {
            return "~/img/Flags/" + Code + ".png";
        }
    }

    public static string DelimitedStringListFromFile(string FilePath, string Delimiter)
    {
        string path = FilePath;
        if (File.Exists(path))
        {
            string str = File.ReadAllText(path);
            string[] str2 = str.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            string strList = "";
            for (int i = 0; i < str2.Count() + 1 - 1; i++)
            {
                //Create string array in js format to use in jquery autocomplete
                strList += "'" + str2[i].Trim() + "'" + (i != str2.Count() - 1 ? Delimiter : "");
            }

            return strList;
        }

        return string.Empty;
    }

    public static bool ExternalFileExist(string Url)
    {
        dynamic uri = new Uri(Url);
        if (uri.IsFile)
        {
            return File.Exists(uri.LocalPath);
        }
        else
        {
            try
            {
                HttpWebRequest r = WebRequest.Create(uri);
                r.Method = "HEAD";
                HttpWebResponse rsp = (HttpWebResponse)r.GetResponse();
                return rsp.StatusCode == HttpStatusCode.OK;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    public static string StripHtml(string HtmlContent)
    {
        return Regex.Replace(HtmlContent, "<(.|\\\\n)*?>", string.Empty);
    }

    public static string ReadXDocValue(XElement Document, XName Name, string defaultValue = "")
    {
        if ((Document.Element(Name) != null))
            return Document.Element(Name).Value;
        return defaultValue;
    }

    public static string ReadXDocAttribute(XElement Document, XName Name, string defaultValue = "")
    {
        if ((Document.Attribute(Name) != null))
            return Document.Attribute(Name).Value;
        return defaultValue;
    }

    public static string UploadToBase64(Stream file)
    {
        if (file != null)
        {
            Stream fs = file;
            var br = new BinaryReader(fs);
            byte[] bytes = br.ReadBytes((int)fs.Length);
            string base64Str = Convert.ToBase64String(bytes, 0, bytes.Length);

            return "data:image/png;base64," + base64Str;
        }
        return string.Empty;
    }

    public static bool Base64ImageToFile(string Base64String, string SaveAs, System.Drawing.Imaging.ImageFormat Format)
    {
        try
        {
            //Remove this part "data:image/jpeg;base64,"
            Base64String = Base64String.Split(',')[1];
            byte[] bytes = Convert.FromBase64String(Base64String);

            System.Drawing.Image image;
            using (var ms = new MemoryStream(bytes))
            {
                image = System.Drawing.Image.FromStream(ms);
            }
            image.Save(SaveAs, Format);
            return true;
        }
        catch (Exception ex)
        {
            ReturnMessage = ex.Message;
            return false;
        }
    }
}

public static partial class DateDifference
{
    public enum DateInterval
    {
        Day,
        DayOfYear,
        Hour,
        Minute,
        Month,
        Quarter,
        Second,
        Weekday,
        WeekOfYear,
        Year
    }

    public static long DateDiff(DateInterval intervalType, System.DateTime dateOne, System.DateTime dateTwo)
    {
        switch (intervalType)
        {
            case DateInterval.Day:
            case DateInterval.DayOfYear:
                System.TimeSpan spanForDays = dateTwo - dateOne;
                return (long)spanForDays.TotalDays;
            case DateInterval.Hour:
                System.TimeSpan spanForHours = dateTwo - dateOne;
                return (long)spanForHours.TotalHours;
            case DateInterval.Minute:
                System.TimeSpan spanForMinutes = dateTwo - dateOne;
                return (long)spanForMinutes.TotalMinutes;
            case DateInterval.Month:
                return ((dateTwo.Year - dateOne.Year) * 12) + (dateTwo.Month - dateOne.Month);
            case DateInterval.Quarter:
                long dateOneQuarter = (long)System.Math.Ceiling(dateOne.Month / 3.0);
                long dateTwoQuarter = (long)System.Math.Ceiling(dateTwo.Month / 3.0);
                return (4 * (dateTwo.Year - dateOne.Year)) + dateTwoQuarter - dateOneQuarter;
            case DateInterval.Second:
                System.TimeSpan spanForSeconds = dateTwo - dateOne;
                return (long)spanForSeconds.TotalSeconds;
            case DateInterval.Weekday:
                System.TimeSpan spanForWeekdays = dateTwo - dateOne;
                return (long)(spanForWeekdays.TotalDays / 7.0);
            case DateInterval.WeekOfYear:
                System.DateTime dateOneModified = dateOne;
                System.DateTime dateTwoModified = dateTwo;
                while (dateTwoModified.DayOfWeek != System.Globalization.DateTimeFormatInfo.CurrentInfo.FirstDayOfWeek)
                {
                    dateTwoModified = dateTwoModified.AddDays(-1);
                }
                while (dateOneModified.DayOfWeek != System.Globalization.DateTimeFormatInfo.CurrentInfo.FirstDayOfWeek)
                {
                    dateOneModified = dateOneModified.AddDays(-1);
                }
                System.TimeSpan spanForWeekOfYear = dateTwoModified - dateOneModified;
                return (long)(spanForWeekOfYear.TotalDays / 7.0);
            case DateInterval.Year:
                return dateTwo.Year - dateOne.Year;
            default:
                return 0;
        }
    }
}
