﻿using System;
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
    /// <summary>
    /// Note: Essentially the Random seed is the actual Private key; and the Private key is [seed + public key]
    /// </summary>
    public class GeneratedKeyPairData
    {
        public byte[] randomSeed = new byte[32];
        public byte[] publicKey = new byte[32];
        public byte[] privateKey = new byte[32];

        public GeneratedKeyPairData(byte[] randomSeed, byte[] publicKey, byte[] privateKey)
        {
            this.randomSeed = randomSeed;
            this.publicKey = publicKey;
            this.privateKey = privateKey;
        }

        public GeneratedKeyPairData()
        {
            (new RNGCryptoServiceProvider()).GetBytes(randomSeed);
            Ed25519.KeyPairFromSeed(out publicKey, out privateKey, randomSeed);
        }
    }
    
    public class UserAccessController
    {
        byte[] publicKey;
        private byte[] privateKey;
        byte[] randomSeed = new byte[32];
        byte[] randomSalt = new byte[8];
        byte[] encryptedRandomSeed;
        byte[] encryptedRandomSalt;
        public string LoggedUser = "";

        static Encoding enc = Encoding.GetEncoding(28591);

        public byte[] PrivateKey
        {
            get
            {
                return privateKey;
            }

        }

        public byte[] PublicKey
        {
            get
            {
                return publicKey;
            }

        }

        public void logOut()
        {
            try
            {
                for (int i = 0; i < publicKey.Length; i++)
                    publicKey[i] = 0;

                for (int i = 0; i < privateKey.Length; i++)
                    privateKey[i] = 0;

                // App.IsAnyBodyHome = false;
            }
            catch
            {

            }
            finally
            {
                LoggedUser = "";
                App.IsAnyBodyHome = false;
            }
        }

        public bool UserExistsLocal(string Username)
        {
            SQLiteConnection sqlite_conn;
            SQLiteCommand sqlite_cmd;
            SQLiteDataReader sqlite_datareader;
            sqlite_conn = new SQLiteConnection(Constants.ConnectionString);
            sqlite_conn.Open();
            sqlite_cmd = sqlite_conn.CreateCommand();

            sqlite_cmd.CommandText = "SELECT * FROM AppUserTable where Username = @u1";
            sqlite_cmd.Parameters.Add(new SQLiteParameter("@u1", Username));

            sqlite_datareader = sqlite_cmd.ExecuteReader();

            //if there is already user return true
            while (sqlite_datareader.Read())
                return true;

            return false;
        }


        /// <summary>
        /// If there is already a user it will return zero
        /// if it is successful then this function wul retun 1
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        /// 
        public int newUserRegistration(GeneratedKeyPairData keypairData, String username, String password, out string message)
        {
            if (username.Length == 0)
            {
                message = "Username field can not be empty";
                return 0;
            }

            if (password.Length == 0)
            {
                message = "Password field can not be empty";
                return 0;
            }


            SQLiteConnection sqlite_conn;
            SQLiteCommand sqlite_cmd;
            SQLiteDataReader sqlite_datareader;
            sqlite_conn = new SQLiteConnection(Constants.ConnectionString);
            sqlite_conn.Open();
            sqlite_cmd = sqlite_conn.CreateCommand();

            sqlite_cmd.CommandText = "SELECT * FROM AppUserTable where Username = @u1";
            sqlite_cmd.Parameters.Add(new SQLiteParameter("@u1", username));

            sqlite_datareader = sqlite_cmd.ExecuteReader();

            //if there is already user return 0
            //also check with the server
            //TODO
            while (sqlite_datareader.Read())
            {
                message = "User " + username + " already exists";
                sqlite_conn.Close();
                return 0;
            }

            sqlite_datareader.Close();
            
            // (new RNGCryptoServiceProvider()).GetBytes(randomSeed);
            // Ed25519.KeyPairFromSeed(out publicKey, out privateKey, randomSeed);

            randomSeed = keypairData.randomSeed;
            privateKey = keypairData.privateKey;
            publicKey = keypairData.publicKey;

            (new RNGCryptoServiceProvider()).GetBytes(randomSalt);

            byte[] hashedPassword = (new SHA512Managed()).ComputeHash(enc.GetBytes(password).Concat(randomSalt).ToArray());

            this.encryptedRandomSeed = EncryptAES(randomSeed, hashedPassword.Take(32).ToArray());
            this.encryptedRandomSalt = EncryptAES(randomSalt, hashedPassword.Take(32).ToArray());

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
                    "INSERT INTO AppUserTable (UserName, RandomSalt, EncryptedRandomSeed, EncryptedRandomSalt) VALUES (@u1, @u2, @u3, @u4);";
                sqlite_cmd.Parameters.Add(new SQLiteParameter("@u1", username));
                sqlite_cmd.Parameters.Add(new SQLiteParameter("@u2", enc.GetString(randomSalt)));
                sqlite_cmd.Parameters.Add(new SQLiteParameter("@u3", enc.GetString(encryptedRandomSeed)));
                sqlite_cmd.Parameters.Add(new SQLiteParameter("@u4", enc.GetString(encryptedRandomSalt)));
                sqlite_cmd.ExecuteNonQuery();
            }

            catch (Exception)
            {
                message = "Database error";
                sqlite_conn.Close();
                return 0;
            }

            sqlite_conn.Close();
            message = "User registration successful";
            return 1;
        }


        /// <summary>
        /// Retun 1 in case success otherwise zero
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public int userLogin(String username, String password, out string message)
        {
            if (username == "@trestor.com")
            {
                message = "Username field can not be empty";
                return 0;
            }

            if (password.Length == 0)
            {
                message = "Password field can not be empty";
                return 0;
            }

            SQLiteConnection sqlite_conn;
            SQLiteCommand sqlite_cmd;
            SQLiteDataReader sqlite_datareader;
            sqlite_conn = new SQLiteConnection(Constants.ConnectionString);
            sqlite_conn.Open();
            sqlite_cmd = sqlite_conn.CreateCommand();

            sqlite_cmd.CommandText = "SELECT * FROM AppUserTable where Username = @u1";
            sqlite_cmd.Parameters.Add(new SQLiteParameter("@u1", username));

            sqlite_datareader = sqlite_cmd.ExecuteReader();

            //if there is no user retun 0
            bool userExist = false;



            while (sqlite_datareader.Read())
            {
                userExist = true;
                this.randomSalt = enc.GetBytes(sqlite_datareader["RandomSalt"].ToString());
                this.encryptedRandomSeed = enc.GetBytes(sqlite_datareader["EncryptedRandomSeed"].ToString());
                this.encryptedRandomSalt = enc.GetBytes(sqlite_datareader["EncryptedRandomSalt"].ToString());
            }

            sqlite_datareader.Close();


            if (!userExist)
            {
                message = "User " + username + " not exists in the app database";
                sqlite_conn.Close();
                return 0;
            }

            byte[] hashedPassword = (new SHA512Managed()).ComputeHash(enc.GetBytes(password).Concat(randomSalt).ToArray());

            byte[] test_salt;
            try
            {
                test_salt = DecryptAES(this.encryptedRandomSalt, hashedPassword.Take(32).ToArray());
            }
            catch
            {
                message = "Password error";
                sqlite_conn.Close();
                return 0;
            }
            //check if the key is correct or not
            if (test_salt.Length == randomSalt.Length)
            {
                for (int i = 0; i < test_salt.Length; i++)
                {
                    if (test_salt[i] != randomSalt[i])
                    {
                        message = "Password error";
                        sqlite_conn.Close();
                        return 0;
                    }
                }
            }
            else
            {
                message = "Password error";
                sqlite_conn.Close();
                return 0;
            }

            //get random seed
            try
            {
                this.randomSeed = DecryptAES(this.encryptedRandomSeed, hashedPassword.Take(32).ToArray());
            }
            catch
            {
                message = "Password error";
                sqlite_conn.Close();
                return 0;
            }
            //make keypair
            Ed25519.KeyPairFromSeed(out publicKey, out privateKey, randomSeed);



            sqlite_cmd.CommandText =
                    "UPDATE AppUserTable SET LastLoginTime=@u2 WHERE Username=@u1;";
            sqlite_cmd.Parameters.Add(new SQLiteParameter("@u1", username));
            sqlite_cmd.Parameters.Add(new SQLiteParameter("@u2", DateTime.Now.ToFileTimeUtc()));

            sqlite_cmd.ExecuteNonQuery();


            sqlite_conn.Close();
            message = "Success";
            LoggedUser = username;
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
                rijAlg.Mode = CipherMode.CBC;
                rijAlg.Padding = PaddingMode.PKCS7;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform encryptor = rijAlg.CreateEncryptor(rijAlg.Key, new byte[16]);

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
                rijAlg.Mode = CipherMode.CBC;
                rijAlg.Padding = PaddingMode.PKCS7;
                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = rijAlg.CreateDecryptor(rijAlg.Key, new byte[16]);

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
