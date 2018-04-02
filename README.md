# MemberLite-EF6
Simple and flexible asp.net membership system with C#, Entity Framework 6 and FormsAuthentication

By default Microsoft ASP.NET FormsAuthentication provides you with Owin Authentication classes using Entity Framework, but sometimes it gets so clumsy and hard to customize for beginners and even pros.
MemberLite-EF6 is a lightweight system that is easy to use and highly customizable with codes for all setups. It takes you behind the scene for effective control of the process. MemberLite provides the basics of FormsAuthentication and can easily be incorporated into existing apps and database. Some core features includes;
* Signup
* Signin
* Send verification email with HTML template 
* Verify email
* Change password
* Retrieve password with HTML template
* Update profile
* Change status
* Upload avatar
* User login history
* Web mailer

It contains several methods that are needed for a User Object and makes your job faster. With highly editable HTML templates you can set how you want your user emails to appear.

## Database
The database contains 2 tables - Users and LoginHistory.
Execute the database script MemberLite.sql from App_Data folder.

## AppUtility Helper Class
I addition, a very rich AppUtility class is available. It contains several methods that developers use regularly in every application. Some methods include;
* GetDeviceType()
* GetUserIPAddress()
* CleanUrlSlug(string Str, int MaxLength = 50)
* IsNumeric(object Expression)
* CleanDomain(string Domain)
* GenerateAlphaNumeric(int Length)
* GenerateNumeric(int Length)
* ValidateEmail(string Email)
* GetUrlQueryString(string Key)
* DateTimeToWord(DateTime DateTimeString)
* DigitStyle(dynamic Value)
* ConvertSize(long Bytes, ConvertType ConvertTo)
* SetCookie(string Cookie, string Value, int Expires = 7)
* SetCookie(string Cookie, string Value, DateTime Expires, bool Secure = false, bool HTTPOnly = false)
* GetCookie(string Cookie)
* Country class
* DelimitedStringListFromFile(string FilePath, string Delimiter)
* ExternalFileExist(string Url)
* StripHtml(string HtmlContent)
* ReadXDocAttribute(XElement Document, XName Name, string defaultValue = "")
* UploadToBase64(Stream file)
* Base64ImageToFile(string Base64String, string SaveAs, System.Drawing.Imaging.ImageFormat Format)
* DateDifference class

## WebMailer Helper Class
WebMailer is a class that handles sending of emails from your app with HTML templates packed properly.
All configurations are saved in the Web.config appSettings and called through the AppConfig.cs. This class is fully dynamic as allows switching of sending email account as seen in the sample.

## Crypto
This is a simple class that provides cryptographic functions for hashing in MD5 and SHA256, also privides comparism.
* MD5Hash(string PlainText, string Salt = "")
* SHA256Hash(string PlainText, string Salt = "")
* ByteArrayToString(byte[] ArrayData)
* CompareMD5Hash(string Hash1, string Hash2, string Hash1Salt = "")
* CompareSHA256Hash(string Hash1, string Hash2, string Hash1Salt = "")

## CustomErrorLogger Helper Class
This is a simple error logging utility that logs errors to App_Data/ErrorLog.xml and provides methods to read and clear errors
* Log(string Message, string Code = "", string Page = "")
* List<CustomErrorLogger> LoadLog()
* Clear()

## Email Templates
Email verification and password reset uses HTML email templates predefined in App_Data/MailTemplates and can be customized with your app logo and details. Parameters are replaced in C# calls before passing them to WebMailer to send.

## StaticContents
These are text files used as dynamic source. NameRestriction.txt contains a list of restricted names that users cannot signup with such as admin, superuser etc so not to confuse or impersonate other users in the site.

## Countries
App_Data/Countries.xml contains all countries with their Name, 2 letter code and ISO. It corresponds to AppUtility.Country to access them.

