using System;
using System.Security.Cryptography;
using System.Text;

namespace Better_xNet
{
    public static class MD5Hashes
    {
        public static string Get(byte[] data)
        {
            if (data == null) throw new ArgumentNullException("data");
            if (data.Length == 0) return string.Empty;

            using (HashAlgorithm ha = new MD5CryptoServiceProvider())
            {
                StringBuilder sb = new StringBuilder(32);
                byte[] array = ha.ComputeHash(data);

                for (int i = 0; i < array.Length; i++)
                {
                    sb.Append(array[i].ToString("x2"));
                }

                return sb.ToString();
            }

            return null;
        }

        public static string Get(string data, Encoding e = null)
        {
            if (string.IsNullOrEmpty(data)) return string.Empty;
            if (e == null) e = Encoding.Default;
            
            return Get(e.GetBytes(data));
        }
    }
}
