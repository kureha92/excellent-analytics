using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace Analytics
{
    public static class DataProtectionHelper
    {
        private static byte[] randomBytes = { 4, 32, 62, 9, 145, 5 };
        public static string Protect(string str)
        {
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            return Convert.ToBase64String(ProtectedData.Protect(encoding.GetBytes(str), randomBytes, DataProtectionScope.CurrentUser));
        }
        public static string UnProtect(string bytes)
        {
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            try
            {
                return encoding.GetString(ProtectedData.Unprotect(Convert.FromBase64String(bytes), randomBytes, DataProtectionScope.CurrentUser));
            }
            catch (Exception e)
            {
                return bytes;
            }
        }
    }
}
