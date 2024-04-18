using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Net;
using System.IO;
using System.Xml;
using LeSDataMain;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using MTML.GENERATOR;


namespace LES_NAVSHIP_Routine
{
    public class NAVSHIP_Routine : LeSCommon.LeSCommon
    {



        string PORTAL_URL = "", USERNAME = "", NaveShipUrl = "", PASSWORD = "", FILTER_DASHBOARD = "", REF_URL = "", appendurl = "";
        static string Quote_Inbox = "", processorname = "", buyercode = "", suppcode = "", suppid = "", buyerlinkcode = "", buyername = "", suppname = "", buyerlinkId = "", Actions = "", currDocType = "", branch = "", _processor_name = "";
        static string mtmlInbox = "", textfile = "", auditpath = "", Downloaded_RFQ_path = "", Downloaded_po_path = "", linkfromfile = "", attachments_path = "", _currentfilename = "", module_Name = "", outbox_Path = "", _tempLink = "";
        static string Oauth_Consumer_Key = "c3RhbmRhcmRVc2VyTmF2c2hpcA", BASEURL = "", Oauth_Token = "4b630be7-1da2-48b3-8136-6acdbb5154cf", OauthVersion = "1.0";

        private Dictionary<string, string> dictAppSettings = null;
        private List<Dictionary<string, string>> appSettings = null;
        //static Root _root = new Root();
        //static AttachmentInfo _attachmentInfo = new AttachmentInfo();
        private string[] docTypes = null;
        private MTMLInterchange interchange = null;
        private DocHeader docHeader = null;
        private bool IsSaveQuote = false, IsSubmitQuote = false;
        private string quoteReferenceNumber = "";
        private double totalPrice;


        public NAVSHIP_Routine()
        {
            try
            {
                Initialise();

            }
            catch (System.Exception ex)
            {
                LogText = "Error while Initialise : " + ex.Message;
            }
        }



        public void loadAppsettings()
        {
            //string srt=Get_Link_Mail();
            //Console.WriteLine(srt);
            ReadAppSettingsXml();

            foreach (var item in appSettings)
            {

                dictAppSettings = item;

                processorname = dictAppSettings["PROCESSOR_NAME"];
                buyercode = dictAppSettings["BUYERCODE"];
                NaveShipUrl = dictAppSettings["NAVSHIPURL"];
                BASEURL = dictAppSettings["BASEURL"];
                buyername = dictAppSettings["BUYERNAME"];
                suppname = dictAppSettings["SUPPLIERNAME"];
                suppcode = dictAppSettings["SUPPLIERCODE"];
                suppid = dictAppSettings["SUPPLIERID"];
                buyerlinkcode = dictAppSettings["BUYERSUPPLIERLINKCODE"];
                buyerlinkId = dictAppSettings["BUYERSUPPLIERLINKID"];
                Actions = dictAppSettings["ACTIONS"];
                //branch = dictAppSettings["branch"].Trim();
                textfile = dictAppSettings["LINK_INBOX_PATH"];
                //module_Name = dictAppSettings["module_name"];

                IsSaveQuote = Convert.ToBoolean(dictAppSettings["SAVE_QUOTE"]);
                IsSubmitQuote = Convert.ToBoolean(dictAppSettings["SUBMIT_QUOTE"]);


                mtmlInbox = convert.ToString(ConfigurationManager.AppSettings["LeS_MTML_PATH"]).Trim();
                auditpath = convert.ToString(ConfigurationManager.AppSettings["ESUPPLIER_AUDIT"]).Trim();
                processorname = Convert.ToString(ConfigurationManager.AppSettings["PROCESSOR_NAME"]);
                attachments_path = Convert.ToString(ConfigurationManager.AppSettings["ESUPPLIER_ATTACHMENTS"]);
                PORTAL_URL = Convert.ToString(ConfigurationManager.AppSettings["PORTAL_URL"]);
                //outbox_Path = Convert.ToString(ConfigurationManager.AppSettings["OUTBOX_PATH"]);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            }
            Start_Process();


        }


        private void ReadAppSettingsXml()
        {
            try
            {
                if (dictAppSettings == null)
                    dictAppSettings = new Dictionary<string, string>();
                if (appSettings == null)
                    appSettings = new List<Dictionary<string, string>>();

                string appSettingFile = Environment.CurrentDirectory + "\\AppSettings.xml";
                if (File.Exists(appSettingFile))
                {
                    XmlDocument document = new XmlDocument();
                    document.Load(appSettingFile);

                    XmlNodeList xmlAppSettings = document.SelectNodes("//APPSETTINGS");
                    if (xmlAppSettings != null)
                    {
                        foreach (XmlNode appSetting in xmlAppSettings)
                        {
                            dictAppSettings = new Dictionary<string, string>();
                            XmlNodeList childNodes = appSetting.ChildNodes;
                            foreach (XmlNode setting in childNodes)
                            {
                                XmlElement userSetting = (XmlElement)setting;
                                dictAppSettings.Add(userSetting.Name, userSetting.InnerText);
                            }
                            appSettings.Add(dictAppSettings);
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                LeSDM.AddConsoleLog("Error occurred while reading AppSettings xml - " + ex.Message);
                throw ex;
            }
        }

        private void Start_Process()
        {
            try
            {
                docTypes = Actions.Split(',');

                foreach (string docType in docTypes)
                {
                    currDocType = docType;
                    switch (docType)
                    {
                        case "RFQ":
                            ProcessRFQ();
                            break;
                        case "QUOTE":
                            //ProcessQUOTE();
                            break;
                    }
                }

            }
            catch (Exception ex)
            {
                LeSDM.AddConsoleLog("Error occurred starting process " + ex.GetBaseException());
                //  Add Audit log
                LeSDM.SetAuditLogFile(module_Name, "Error", "", "Error occurred starting process - " + ex.Message, "", "", "", ""); //module_name
            }
        }







        #region RFQ

        private void ProcessRFQ()
        {

            try
            {
                LogText = "RFQ Processing started.";
                DirectoryInfo _dir = new DirectoryInfo(textfile);
                if (_dir.GetFiles().Length > 0)
                {
                    FileInfo[] _Files = _dir.GetFiles();
                    foreach (FileInfo f in _Files)
                    {
                        string cMSgFile = File.ReadAllText(f.FullName);
                        string link = GetLink_TextFile(f.FullName);

                        if (!string.IsNullOrWhiteSpace(link))
                        {


                            LogText = "link found in this file:" + f.Name;
                            _currentfilename = f.FullName;
                            string _currfile = Path.GetFileNameWithoutExtension(f.FullName);
                            loadPortal(link, _currfile, _currentfilename);

                        }
                        else
                        {
                            LogText = "link not found in this file:" + f.FullName;
                            CreateAuditFile(f.FullName, processorname, "", "Error", "link not found :" + f.FullName, buyercode, suppcode, auditpath);
                            MoveFiles(f.FullName, textfile + "\\Error\\" + Path.GetFileName(f.FullName));
                        }
                    }
                }
                else
                {
                    LogText = "No files found.";
                }

                LogText = "RFQ Processing ended.";

            }
            catch (System.Exception ex)
            {
                LogText = "Error while processing rfq :" + ex.Message;

            }
        }


        public string GetLink_TextFile(string filepath)
        {
            string url = "";
            try
            {
                string fileContent = File.ReadAllText(filepath);
                string pattern = @"informados: <(.*?)>"; ;
                MatchCollection matches = Regex.Matches(fileContent, pattern);
                foreach (Match match in matches)
                {
                    url = match.Groups[1].Value;
                    linkfromfile = url;
                    break;

                }

            }
            catch (Exception ex)
            {
                LogText = "Not getting url from file...";
                throw new Exception("Not getting url from file :" + ex.Message);
            }
            return url;
        }



        public void loadPortal(string link, string file, string fullfilename)
        {
            try
            {
                _httpWrapper.LoadURL(link, "", "", "", "");
                string strdata = _httpWrapper._CurrentResponseString;

                string cTempLink = convert.ToString(_httpWrapper._CurrentResponse.ResponseUri);
                //_httpWrapper.LoadURL(cTempLink, "", "", "", "");

                GetPostData(cTempLink);


            }
            catch (Exception ex)
            {

                LogText = "Portal Load issue";
                CreateAuditFile(fullfilename, processorname, "", "Error", "Portal Load issue:" + ex.Message, buyercode, suppcode, auditpath);
                MoveFiles(fullfilename, textfile + "\\Error\\" + Path.GetFileName(fullfilename));
            }

        }

        public void GetPostData(string Referrer)
        {
            try
            {
                //_httpWrapper._AddRequestHeaders.Clear();
                //_httpWrapper._SetRequestHeaders.Clear();
                string url = "https://fluig.navship.com.br/api/public/ecm/dataset/datasets";
                string body = @"{""name"":""Ds_Portal_Cotacao"",""fields"":[],""constraints"":[{""_field"":""FORNECEDOR"",""_finalValue"":""9mrEG9r2WI6JN76hNViGJnmfTRSECGH16itP1VdayXsnfNmNm0DHouRPJDTKl8HgTGwWe85km6a3psId18ttyEWkLZK4cFbIaFEU14531920102023"",""_initialValue"":""9mrEG9r2WI6JN76hNViGJnmfTRSECGH16itP1VdayXsnfNmNm0DHouRPJDTKl8HgTGwWe85km6a3psId18ttyEWkLZK4cFbIaFEU14531920102023"",""_type"":1},{""_field"":""CODCOLIGADA"",""_finalValue"":""1"",""_initialValue"":""1"",""_type"":1},{""_field"":""CODCOTACAO"",""_finalValue"":""2023.012986"",""_initialValue"":""2023.012986"",""_type"":1}],""order"":[]}";

                _httpWrapper.Referrer = Referrer;
                _httpWrapper.RequestMethod = "POST";
                _httpWrapper._AddRequestHeaders.Add("Origin", @"https://fluig.navship.com.br");
                _httpWrapper.ContentType = "application/json";
                _httpWrapper.AcceptMimeType = "*/*";

                //to set the cookie

                bool aaa = _httpWrapper.PostURL(url, body, "", "", "");
                string strdata = _httpWrapper._CurrentResponseString;
            }
            catch (Exception ex)
            {

            }
        }
        #endregion RFQ


        private string GetBaseString()
        {
            string OuthNonce = GetNonce();
            long oauth_timestamp = GetTimestamp();
            string baseString = "POST&https%3A%2F%2F" + URL + "%2Fapi%2Fpublic%2Fecm%2Fdataset%2Fdatasets&oauth_consumer_key%3D" + Oauth_Consumer_Key + "%253D%253D%26oauth_nonce%3D" + OuthNonce + "%26oauth_signature_method%3DHMAC-SHA1%26oauth_timestamp%3D" + oauth_timestamp + "%26oauth_token%3D" + Oauth_Token + "%26oauth_version%3D" + OauthVersion;

            return baseString;
        }

        public string GetNonce()
        {
            var wordCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var result = new char[6];

            Random random = new Random();

            for (var i = 0; i < 6; i++)
            {
                result[i] = wordCharacters[random.Next(wordCharacters.Length)];
            }

            return new string(result);
        }

        public long GetTimestamp()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }


        public string getSignature()
        {
            string Key = "UGFzc3dvcmRGb3JOYXZzaGlwT2F1dGhBcHBsaWNhdGlvbg%3D%3D&af7d65b9-2c46-47de-a471-11fcdcfef369824e4a79-76a3-42fd-ac1b-2b9ad365b7d5";
            var BaseString = GetBaseString();
            //string BaseString = "POST&https%3A%2F%2Ffluig.navship.com.br%2Fapi%2Fpublic%2Fecm%2Fdataset%2Fdatasets&oauth_consumer_key%3Dc3RhbmRhcmRVc2VyTmF2c2hpcA%253D%253D%26oauth_nonce%3DGUDo5r%26oauth_signature_method%3DHMAC-SHA1%26oauth_timestamp%3D1705042558%26oauth_token%3D4b630be7-1da2-48b3-8136-6acdbb5154cf%26oauth_version%3D1.0";

            //string Key = "UGFzc3dvcmRGb3JOYXZzaGlwT2F1dGhBcHBsaWNhdGlvbg%3D%3D&af7d65b9-2c46-47de-a471-11fcdcfef369824e4a79-76a3-42fd-ac1b-2b9ad365b7d5";

            return HmacSHA1(BaseString, Key);
        }



        static string HmacSHA1(string baseString, string key)
        {
            using (var hmacsha1 = new HMACSHA1(Encoding.UTF8.GetBytes(key)))
            {
                byte[] hashBytes = hmacsha1.ComputeHash(Encoding.UTF8.GetBytes(baseString));
                return Convert.ToBase64String(hashBytes);
            }

        }


    }
}
