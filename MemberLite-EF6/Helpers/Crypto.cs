using System.IO;
using System.Security.Cryptography;
using System.Text;

public class Crypto
{
    public static string MD5Hash(string PlainText, string Salt = "")
    {
        dynamic plainStr = PlainText + Salt;
        byte tmpSource = 0;
        byte[] tmpHash = null;

        tmpSource = ASCIIEncoding.ASCII.GetBytes(plainStr);
        tmpHash = new MD5CryptoServiceProvider().ComputeHash(new MemoryStream(tmpSource));
        return ByteArrayToString(tmpHash);
    }

    public static string SHA256Hash(string PlainText, string Salt = "")
    {
        string plainStr = PlainText + Salt;
        byte[] tmpSource = { 0 };
        byte[] tmpHash;

        tmpSource = ASCIIEncoding.ASCII.GetBytes(plainStr);
        tmpHash = new SHA256CryptoServiceProvider().ComputeHash(new MemoryStream(tmpSource));
        return ByteArrayToString(tmpHash);
    }

    public static string ByteArrayToString(byte[] ArrayData)
    {
        StringBuilder strBuilder = new StringBuilder(ArrayData.Length);
        for (int i = 0; i <= ArrayData.Length - 1; i++)
        {
            strBuilder.Append(ArrayData[i].ToString("X2"));
        }
        return strBuilder.ToString();
    }

    public static bool CompareMD5Hash(string Hash1, string Hash2, string Hash1Salt = "")
    {
        if (string.IsNullOrEmpty(MD5Hash(Hash1, Hash1Salt)))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool CompareSHA256Hash(string Hash1, string Hash2, string Hash1Salt = "")
    {
        if (string.IsNullOrEmpty(SHA256Hash(Hash1, Hash1Salt)))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}