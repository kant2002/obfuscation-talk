using System.Text;

static class Decoder
{
    public static string DecodeString(string str)
    {
        return Encoding.UTF8.GetString(Convert.FromBase64String(str));
    }
    public static string EncodeString(string str)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(str));
    }
}
