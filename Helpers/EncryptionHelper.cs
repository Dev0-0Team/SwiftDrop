using System.Security.Cryptography;
using System.Text;

namespace SwiftDrop.Helpers
{
    //for decrypting the encrypted text in appsettings.json file
    public static class EncryptionHelper
    {

        public static string DecryptString(string cipherText, string keyString)
        {
            if (string.IsNullOrEmpty(cipherText)) return cipherText;

            try
            {

                byte[] fullCipher = Convert.FromBase64String(cipherText);

                byte[] iv = new byte[16];
                Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);

                byte[] cipher = new byte[fullCipher.Length - iv.Length];
                Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, cipher.Length);

                byte[] key = Encoding.UTF8.GetBytes(keyString.PadRight(32).Substring(0, 32));

                using (Aes aesAlg = Aes.Create())
                {
                    using (ICryptoTransform decryptor = aesAlg.CreateDecryptor(key, iv))
                    {
                        string result;
                        using (MemoryStream msDecrypt = new MemoryStream(cipher))
                        {
                            using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                            {
                                using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                                {
                                    result = srDecrypt.ReadToEnd();
                                }
                            }
                        }
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {

                throw new Exception("فشل في فك التشفير! تأكد أن مفتاح التشفير (Key) صحيح ولم يتم العبث بالنص المشفر.", ex);
            }
        }
    }
}
