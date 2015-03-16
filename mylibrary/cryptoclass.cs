using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using System.Security.Cryptography;

namespace mylibrary
{
    class cryptoclass
    {
        private static char splitchar = '`';
        public static string paddingstring(string str)
        {
            string paddingstring = str;

            int lngth = str.Length;
            int i = 0;

            if (lngth % 16 > 0)
            {
                paddingstring += splitchar;
                while ((paddingstring.Length % 16) > 0)
                {
                    paddingstring += paddingstring[i];
                    i++;
                }
            }

            byte[] bt = Encoding.UTF8.GetBytes(paddingstring);
            if (bt.Length % 16 != 0)
            {
                while (bt.Length % 16 > 0)
                {
                    Array.Resize(ref bt, bt.Length + 1);
                    bt[bt.Length - 1] = 96;
                }
                paddingstring = Encoding.UTF8.GetString(bt);
            }

            return paddingstring;
        }

        public static string deletepaddingstring(string str)
        {
            string clearstring = str;

            string[] res = str.Split(new char[] { splitchar });

            return res[0];
        }

        public static byte[] EncryptStringToBytes(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("Key");
            byte[] encrypted;

            byte[] bpt = Encoding.UTF8.GetBytes(plainText);
            // Create an Rijndael object
            // with the specified key and IV.
            using (Rijndael rijAlg = Rijndael.Create())
            {
                rijAlg.Key = Key;
                rijAlg.IV = IV;
                rijAlg.Padding = PaddingMode.None;

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

        public static string DecryptStringFromBytes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an Rijndael object
            // with the specified key and IV.
            using (Rijndael rijAlg = Rijndael.Create())
            {
                rijAlg.Key = Key;
                rijAlg.IV = IV;
                rijAlg.Padding = PaddingMode.None;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

                // Create the streams used for decryption.
                MemoryStream msDecrypt = new MemoryStream(cipherText);
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

            return plaintext;

        }

        //dest - зашифрованный датасет
        //source - незашифрованныей датасет
        public static void EncryptDataSet(ref DataSet dest, DataSet source, byte[] key, byte[] iv)
        {
            foreach (DataTable dt in source.Tables)
            {
                if(dest.Tables.Contains(dt.TableName))
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        if (!dest.Tables[dt.TableName].Rows.Contains(dr["id"]))
                        {
                            DataRow newrow = dest.Tables[dt.TableName].NewRow();
                            newrow["id"] = (int)dr["id"];
                            /*foreach (DataColumn dc in dt.Columns)
                            {
                                if ((dc.ColumnName == "serviecename") || (dc.ColumnName == "username") || (dc.ColumnName == "pswrd") || (dc.ColumnName == "ipadr") || (dc.ColumnName == "uid") || (dc.ColumnName == "comment"))
                                {
                                    newrow[dc.ColumnName] = Encoding.UTF8.GetBytes((string)dr[dc.ColumnName]);
                                }
                                else
                                    newrow[dc.ColumnName] = dr[dc.ColumnName];
                                //dest.Tables[dt.TableName].Rows.Add(dr);
                            }*/
                            dest.Tables[dt.TableName].Rows.Add(newrow);
                        }
                    }

                    foreach (DataRow dr in dt.Rows)
                    {
                        DataRow destrow = dest.Tables[dt.TableName].Rows.Find((int)dr["id"]);
                        foreach (DataColumn dc in dt.Columns)
                        {
                            if ((dc.ColumnName == "serviecename") || (dc.ColumnName == "username") || (dc.ColumnName == "pswrd") || (dc.ColumnName == "ipadr") || (dc.ColumnName == "uid") || (dc.ColumnName == "comment"))
                            {
                                if (key.Length == 0)
                                {
                                    if (!dr[dc.ColumnName].GetType().ToString().Contains("DBNull"))
                                    {
                                        destrow[dc.ColumnName] = Encoding.UTF8.GetBytes((string)dr[dc.ColumnName]);
                                    }
                                    else
                                        destrow[dc.ColumnName] = null;

                                }
                                else
                                {
                                    string value = "";
                                    if (!dr[dc.ColumnName].GetType().ToString().Contains("DBNull"))
                                    {
                                        value = (string)dr[dc.ColumnName];

                                        value = paddingstring(value);
                                        try
                                        {
                                            byte[] encryptedvalue = EncryptStringToBytes(value, key, iv);
                                            destrow[dc.ColumnName] = encryptedvalue;
                                        }
                                        catch (CryptographicException exp)
                                        {
                                            System.Windows.Forms.MessageBox.Show(exp.Message);
                                            destrow[dc.ColumnName] = Encoding.UTF8.GetBytes((string)dr[dc.ColumnName]);
                                        }
                                    }
                                    else
                                        destrow[dc.ColumnName] = null;

                                }
                            }
                            else
                            {
                                destrow[dc.ColumnName] = dr[dc.ColumnName];
                            }
                        }
                    }
                }
            }
        }
    }
}
