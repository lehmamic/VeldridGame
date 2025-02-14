using System.Security.Cryptography;
using System.Text;

namespace VeldridGame.Utils;

public static class GuidExtensions
{
    public static Guid ComputeGuid(this string relativePath)
    {
        using MD5 md5 = MD5.Create();
        byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(relativePath));
        return new Guid(hash);
    }
}