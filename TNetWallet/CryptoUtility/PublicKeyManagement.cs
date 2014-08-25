using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Chaos.NaCl;
using System.IO;
using System.Data.SQLite;


namespace TNetWallet.CryptoUtility
{
    class PublicKeyManagement
    {
        byte[] publicKey;
        byte[] privateKey;
        byte[] randomSeed = new byte[32];
        byte[] randomSalt = new byte[8];
        byte[] encryptedRandomSeed;
        byte[] encryptedRandomSalt;

        static Encoding enc = Encoding.GetEncoding("ISO8859-1");

        /// <summary>
        /// If there is already a user it will return zero
        /// if it is successful then this function wul retun 1
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public int newUserRegistration(String username, String password)
        {
            SQLiteConnection sqlite_conn;
            SQLiteCommand sqlite_cmd;
            SQLiteDataReader sqlite_datareader;
            sqlite_conn = new SQLiteConnection(@"data source=..\..\db\db.dat; Version=3; New=True; Compress=True;");
            sqlite_conn.Open();
            sqlite_cmd = sqlite_conn.CreateCommand();

            sqlite_cmd.CommandText = "SELECT * FROM AppUsertable where Username = @u1";
            sqlite_cmd.Parameters.Add(new SQLiteParameter("@u1", username));

            sqlite_datareader = sqlite_cmd.ExecuteReader();

            //if there is already user return 0
            //also check with the server
            //TODO
            while (sqlite_datareader.Read())
            {
                return 0;
            }
          

            (new RNGCryptoServiceProvider()).GetBytes(randomSeed);
            Ed25519.KeyPairFromSeed(out publicKey, out privateKey, randomSeed);

            (new RNGCryptoServiceProvider()).GetBytes(randomSalt);

            byte[] hashedPassword = (new SHA512Managed()).ComputeHash(enc.GetBytes(password).Concat(randomSalt).ToArray());

            this.encryptedRandomSeed = EncryptAES(randomSeed, hashedPassword);
            this.encryptedRandomSalt = EncryptAES(randomSalt, hashedPassword);

            /*
            FileStream fst = new FileStream(username+".key", FileMode.OpenOrCreate);

            StreamWriter sw = new StreamWriter(fst);

            sw.WriteLine(enc.GetString(randomSalt));
            sw.WriteLine("\n");
            sw.WriteLine(enc.GetString(randomSeed));

            sw.Close();
            */
            try
            {
                sqlite_cmd.CommandText =
                    "INSERT INTO AppUsertable (UserName, RandomSalt, EncryptedRandomSeed, EncryptedRandomSalt) VALUES (@u1, @u2, @u3, @u4);";
                sqlite_cmd.Parameters.Add(new SQLiteParameter("@u1", username));
                sqlite_cmd.Parameters.Add(new SQLiteParameter("@u2", enc.GetString(randomSalt)));
                sqlite_cmd.Parameters.Add(new SQLiteParameter("@u3", enc.GetString(encryptedRandomSeed)));
                sqlite_cmd.Parameters.Add(new SQLiteParameter("@u4", enc.GetString(encryptedRandomSalt)));
                sqlite_cmd.ExecuteNonQuery();
            }

            catch
            {
                return 0;
            }

            return 1;
        }


        /// <summary>
        /// Retun 1 in case success otherwise zero
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public int userLogin(String username, String password)
        {
            SQLiteConnection sqlite_conn;
            SQLiteCommand sqlite_cmd;
            SQLiteDataReader sqlite_datareader;
            sqlite_conn = new SQLiteConnection(@"data source=..\..\db\db.dat; Version=3; New=True; Compress=True;");
            sqlite_conn.Open();
            sqlite_cmd = sqlite_conn.CreateCommand();

            sqlite_cmd.CommandText = "SELECT * FROM AppUsertable where Username = @u1";
            sqlite_cmd.Parameters.Add(new SQLiteParameter("@u1", username));

            sqlite_datareader = sqlite_cmd.ExecuteReader();

            //if there is no user retun 0
            bool userExist = false;



            while (sqlite_datareader.Read())
            {
                userExist = true;
                this.randomSalt = enc.GetBytes(sqlite_datareader[0].ToString());
                this.encryptedRandomSeed = enc.GetBytes(sqlite_datareader[1].ToString());
                this.encryptedRandomSalt = enc.GetBytes(sqlite_datareader[2].ToString());
            }
            if(!userExist)
                return 0;

            byte[] hashedPassword = (new SHA512Managed()).ComputeHash(enc.GetBytes(password).Concat(randomSalt).ToArray());

            byte[] test_salt = DecryptAES(this.encryptedRandomSalt, hashedPassword);

            //check if the key is correct or not
            if (test_salt.Length == randomSalt.Length)
            {
                for(int i = 0; i<test_salt.Length; i++)
                {
                    if(test_salt[i] != randomSalt[i])
                        return 0;
                }
            }
            else
            {
                return 0;
            }

            //get random seed
            this.randomSeed = DecryptAES(this.encryptedRandomSeed, hashedPassword);

            //make keypair
            Ed25519.KeyPairFromSeed(out publicKey, out privateKey, randomSeed);

            return 1;
        }



        static byte[] EncryptAES(byte[] _plainText, byte[] Key)
        {

            string plainText = enc.GetString(_plainText);

            // Check arguments. 
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
         
            byte[] encrypted;
            // Create an RijndaelManaged object 
            // with the specified key and IV. 
            using (RijndaelManaged rijAlg = new RijndaelManaged())
            {
                rijAlg.Key = Key;             

                // Create a decrytor to perform the stream transform.
                ICryptoTransform encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);

                // Create the streams used for encryption. 
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }


            // Return the encrypted bytes from the memory stream. 
            return encrypted;

        }

        static byte[] DecryptAES(byte[] cipherText, byte[] Key)
        {
            // Check arguments. 
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
       

            // Declare the string used to hold 
            // the decrypted text. 
            string plaintext = null;

            // Create an RijndaelManaged object 
            // with the specified key and IV. 
            using (RijndaelManaged rijAlg = new RijndaelManaged())
            {
                rijAlg.Key = Key;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

                // Create the streams used for decryption. 
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream 
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }

            }


            return enc.GetBytes(plaintext);

        }

    }
}
