using System;
using System.Configuration;

public class AppConfig
{
    public static string Name { get { return ConfigurationManager.AppSettings["app:Name"]; } }
    public static string Description { get { return ConfigurationManager.AppSettings["app:Description"]; } }
    public static string Version { get { return ConfigurationManager.AppSettings["app:Version"]; } }
    public static string Url { get { return new Uri(ConfigurationManager.AppSettings["app:Url"]).AbsoluteUri; } }
    public static string Port { get { return ConfigurationManager.AppSettings["app:Port"]; } }
    
    public static string AvatarDirectory { get { return ConfigurationManager.AppSettings["app:AvatarDirectory"]; } }
    public static int AvatarMaxFileSize { get { return Convert.ToInt32(ConfigurationManager.AppSettings["app:AvatarMaxFileSize"]); } }
}