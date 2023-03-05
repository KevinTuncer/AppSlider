using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace Kamera_Linearführung
{
    static class DateiVerschlüsselung
    {
        //  Call this function to remove the key from memory after use for security
        [DllImport("KERNEL32.DLL", EntryPoint = "RtlZeroMemory")]
        public static extern bool ZeroMemory(IntPtr Destination, int Length);


        [DllImport("tkl.dll")]
        public static extern char getKey(int nr);


        //// Function to Generate a 64 bits Key.
        //static string GenerateKey() 
        //{
        //    // Create an instance of Symetric Algorithm. Key and IV is generated automatically.
        //    DESCryptoServiceProvider desCrypto =(DESCryptoServiceProvider)DESCryptoServiceProvider.Create();

        //    // Use the Automatically generated key for Encryption. 
        //    return ASCIIEncoding.ASCII.GetString(desCrypto.Key);
        //}

        static void EncryptFile(string sInput, string sOutputFilename, string sKey)
        {
            FileStream fsEncrypted = new FileStream(sOutputFilename, FileMode.Create, FileAccess.Write);
            DESCryptoServiceProvider DES = new DESCryptoServiceProvider();
            DES.Key = ASCIIEncoding.ASCII.GetBytes(sKey);
            DES.IV = ASCIIEncoding.ASCII.GetBytes(sKey);
            ICryptoTransform desencrypt = DES.CreateEncryptor();
            CryptoStream cryptostream = new CryptoStream(fsEncrypted,
            desencrypt,
            CryptoStreamMode.Write);

            byte[] bytearrayinput = ASCIIEncoding.ASCII.GetBytes(sInput);

            cryptostream.Write(bytearrayinput, 0, bytearrayinput.Length);
            cryptostream.Close();
            fsEncrypted.Close();
        }

        static string DecryptFile(string sInputFilename, string sKey)
        {
            if (!File.Exists(sInputFilename))
                return "";

            DESCryptoServiceProvider DES = new DESCryptoServiceProvider();
            //A 64 bit key and IV is required for this provider.
            //Set secret key For DES algorithm.
            DES.Key = ASCIIEncoding.ASCII.GetBytes(sKey);
            //Set initialization vector.
            DES.IV = ASCIIEncoding.ASCII.GetBytes(sKey);

            //Create a file stream to read the encrypted file back.
            FileStream fsread = new FileStream(sInputFilename,
            FileMode.Open,
            FileAccess.Read);
            //Create a DES decryptor from the DES instance.
            ICryptoTransform desdecrypt = DES.CreateDecryptor();
            //Create crypto stream set to read and do a 
            //DES decryption transform on incoming bytes.
            CryptoStream cryptostreamDecr = new CryptoStream(fsread, desdecrypt, CryptoStreamMode.Read);
            //Print the contents of the decrypted file.
            //StreamWriter fsDecrypted = new StreamWriter(sOutputFilename);

            string decrypt = "";
            try
            {
                decrypt = new StreamReader(cryptostreamDecr).ReadToEnd();
            }
            catch { }
            fsread.Flush();
            fsread.Close();
            return decrypt;
        }

        static private string erhalteKey()
        {
            char[] secretKey = new char[8];

            // Merkt sich den Chararray
            GCHandle gch1 = GCHandle.Alloc(secretKey, GCHandleType.Pinned);

            for (int i = 0; i < secretKey.Length; i++)
            {
                secretKey[i] = getKey(i);
            }

            string sSecretkey = new string(secretKey);

            //Löschet den Chararray wieder
            ZeroMemory(gch1.AddrOfPinnedObject(), secretKey.Length * 2);
            gch1.Free();

            return sSecretkey;
        }

        static public bool schreiben(string DateiPfad, string Inhalt)
        {
            string sSecretKey = erhalteKey();

            // Merkt sich den String
            GCHandle gch2 = GCHandle.Alloc(sSecretKey, GCHandleType.Pinned);

            // Encrypt the file.        
            EncryptFile(Inhalt, DateiPfad, sSecretKey);

            ZeroMemory(gch2.AddrOfPinnedObject(), sSecretKey.Length * 2);
            gch2.Free();
            //Löschet den String wieder

            return true;
        }

        public static string lesen(string DateiPfad)
        {
            string sSecretKey = erhalteKey();

            // Merkt sich den String
            GCHandle gch2 = GCHandle.Alloc(sSecretKey, GCHandleType.Pinned);

            // Decrypt the file.
            string s = DecryptFile(DateiPfad, sSecretKey);

            ZeroMemory(gch2.AddrOfPinnedObject(), sSecretKey.Length * 2);
            gch2.Free();
            //Löschet den String wieder

            return s;
        }

    }
}
