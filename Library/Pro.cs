using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;


namespace XLibrary
{
    public class Pro
    {
        static string JmgPublicKey = "<RSAKeyValue><Modulus>ncSXeFS9r6oaFh+/kc5MVJCqw4t0624S+sfdojK9TEnTUL8ohgzOn/n5cpo/jItrK7GiQ0qFdhnLqF/UFYj8CmApxU3/4gJgS+sU9iCHCkvZXYnbP9R4JILqHteUfUvC94peLBGKD07RbeFWp2q5OOCposmPgNQqS9oE6wgpg78=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

        public static string SignedFile;
        public static bool Verified;

        public static string ID;
        public static string Name;
        public static string Company;
        public static string Date;


        public static bool LoadFromDirectory(string dirpath)
        {
            try
            {
                string filepath = Directory.EnumerateFiles(dirpath, "*.pro", SearchOption.TopDirectoryOnly).FirstOrDefault();

                if (filepath != null)
                    return LoadFromString(File.ReadAllText(filepath));
            }
            catch { }

            return false; 
        }

        public static bool LoadFromString(string proFile)
        {
            try
            {
                string proData = ReadTag(proFile, "Pro");
                proData = proData.Replace("\r\n", "");

                string signature = ReadTag(proFile, "Signature");

                var pubRsa = new RSACryptoServiceProvider();
                pubRsa.FromXmlString(JmgPublicKey);

                // check signature
                Verified = pubRsa.VerifyData(
                                UTF8Encoding.UTF8.GetBytes(proData),
                                new SHA1CryptoServiceProvider(),
                                Convert.FromBase64String(signature));

                if (!Verified)
                    return false;

                SignedFile = proFile;
                ID = ReadTag(proData, "ID");
                Name = ReadTag(proData, "Name");
                Company = ReadTag(proData, "Company");
                Date = ReadTag(proData, "Date");

                return true;
            }
            catch { return false; }
        }

        private static string ReadTag(string text, string tag)
        {
            int pos = text.IndexOf("<" + tag + ">");
            if (pos == -1)
                return null;

            pos += 1 + tag.Length + 1;

            int endPos = text.IndexOf("</" + tag + ">");
            if (endPos == -1)
                return null;

            return text.Substring(pos, endPos - pos);
        }
    }
}
