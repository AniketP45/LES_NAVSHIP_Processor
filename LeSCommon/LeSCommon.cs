using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using HTTPWrapper;
using System.Diagnostics;

namespace LeSCommon
{
    public class LeSCommon
    {
        public enum eActions
        {
            RFQ = 0,
            PO = 1,
            QUOTE = 2,
        }
        private string _sesionid = "SESSION";
        private string _currentresponsestring = "";
        private bool _isurlencoded = true;
        private string _requestMethod = "POST";

        public string Userid { get; set; }
        public string Password { get; set; }
        public string URL { get; set; }
        public int LoginRetry { get; set; }
        public string BuyerCode { get; set; }
        public string SupplierCode { get; set; }
        public string AuditPath { get; set; }
        public string DownloadPath { get; set; }
        public string PrintScreenPath { get; set; }        
        public string SessionIDCookieName { get {return _sesionid;} set{  _sesionid=value;} }
        public string HiddenAttributeKey { get; set; }
        public string _CurrentResponseString { get { return _currentresponsestring; } set { _currentresponsestring = value; _httpWrapper._CurrentResponseString = value; } }
        public bool IsUrlEncoded { get { return _isurlencoded; } set { _isurlencoded = value; _httpWrapper.IsUrlEncoded = value; } } //namrata
        public string RequestMethod { get { return _requestMethod; } set { _requestMethod = value; _httpWrapper.RequestMethod = value; } } //simmy 27.02.2018 

        public string Domain { get; set; }//namrata


        
        public Dictionary<string, string> dctAppSettings { get; set; }

        public Dictionary<string, string> dctPostDataValues { get; set; }      

        public HTTPWrapper.HTTPWrapper _httpWrapper = new HTTPWrapper.HTTPWrapper();

        public string LogPath { get; set; }

        public string LogText { set { WriteLog(value); } }

        private void WriteLog(string _logText, string _logFile = "")
        {

            if (LogPath == null || LogPath == "") LogPath = AppDomain.CurrentDomain.BaseDirectory + "Log";
            string _logfile = @"Log_" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
            if (_logFile.Length > 0) { _logfile = _logFile; }
            if (!Directory.Exists(LogPath)) Directory.CreateDirectory(LogPath);
            Console.WriteLine(_logText);
            File.AppendAllText(LogPath + @"\" + @_logfile, DateTime.Now.ToString() + " " + _logText + Environment.NewLine);

        }    

        public void MoveFiles(string Source_FilePath, string Destnation_FilePath)
        {
            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(Destnation_FilePath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(Destnation_FilePath));
                }
                if (File.Exists(Destnation_FilePath))
                {
                    File.Delete(Destnation_FilePath);
                }
                File.Move(Source_FilePath, Destnation_FilePath);
            }
            catch (Exception ex)
            {
                LogText = ("Error on MoveFiles : " + ex.Message);
            }
        }

        public void CreateAuditFile(string FileName, string Module, string RefNo, string LogType, string Audit,string _eSupplierBuyerAddressCode, string _eSupplierSupplierAddressCode, string _sAuditPath)
        {
            try
            {
                if (_eSupplierBuyerAddressCode == null || _eSupplierBuyerAddressCode == "") _eSupplierBuyerAddressCode = Convert.ToString(ConfigurationManager.AppSettings["BUYER"]);
                if (_eSupplierSupplierAddressCode == null || _eSupplierSupplierAddressCode == "") _eSupplierSupplierAddressCode = Convert.ToString(ConfigurationManager.AppSettings["SUPPLIER"]);

                string auditPath = _sAuditPath;
                if (!Directory.Exists(auditPath)) Directory.CreateDirectory(auditPath);

                string auditData = "";
                if (auditData.Trim().Length > 0) auditData += Environment.NewLine;
                auditData += _eSupplierBuyerAddressCode + "|";
                auditData += _eSupplierSupplierAddressCode + "|";
                auditData += Module + "|";
                auditData += Path.GetFileName(FileName) + "|";
                auditData += RefNo + "|";
                auditData += LogType + "|";
                auditData += DateTime.Now.ToString("yy-MM-dd HH:mm") + " : " + Audit;
                if (auditData.Trim().Length > 0)
                {
                    File.WriteAllText(auditPath + "\\Audit_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".txt", auditData);
                    System.Threading.Thread.Sleep(5);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool LoadURL(string validateNodeType, string validateAttribute, string attributeValue, bool _readResponse = true)
        {
            bool _result = false;
            _httpWrapper.CooKieName = SessionIDCookieName;
            _result = _httpWrapper.LoadURL(URL, validateNodeType, validateAttribute, attributeValue, SessionIDCookieName, HiddenAttributeKey,_readResponse);
            _CurrentResponseString = _httpWrapper._CurrentResponseString;
            return _result;
        }

        public bool PostURL(string validateNodeType, string validateAttribute, string attributeValue, bool _readResponse = true)
        {
            bool _result = false;
            _result = Navigate(validateNodeType, validateAttribute, attributeValue,false, _readResponse); //CHANGED ON 20.03.2018 added _readResponse since it was not being carried forward to navigate function           
            return _result;
        }

        //public bool LoadURL(string validateNodeType, string validateAttribute, string attributeValue,bool addHeaders,bool _readResponse=true)
        //{
        //    bool _result = false;
        //    _httpWrapper.CooKieName = SessionIDCookieName;
        //    _result = _httpWrapper.LoadURLWithHeaders(URL, validateNodeType, validateNodeType, attributeValue, SessionIDCookieName, HiddenAttributeKey,_readResponse);
        //    _CurrentResponseString = _httpWrapper._CurrentResponseString;
        //    return _result;
        //}
        
        private string GetPostData()
        {
            string _postData = "",_value="";
            foreach(KeyValuePair<string,string> _kv in dctPostDataValues)
            {
                _value = _kv.Value;

                if (_kv.Value == "" && _httpWrapper._dctStateData.ContainsKey(_kv.Key)) _value = _httpWrapper._dctStateData[_kv.Key];
                _postData += "&" + _kv.Key + "=" + _value;
            }
            return (_postData.Trim()!="") ?_postData.Substring(1):""; 
        }

        public string GetPostData(bool IsCommaSeperated)//namrata
        {
            string _postData = "", _value = "";

            foreach (KeyValuePair<string, string> _kv in dctPostDataValues)
            {
                _value = _kv.Value;

                if (_kv.Value == "" && _httpWrapper._dctStateData.ContainsKey(_kv.Key)) _value = _httpWrapper._dctStateData[_kv.Key];
                if (!IsCommaSeperated)
                    _postData += "&" + _kv.Key + "=" + _value;
                else
                {
                    _postData += ",\"" + _kv.Key + "\":\"" + _value + "\"";
                }
            }
            if (IsCommaSeperated && _postData.Trim()!="")
            {
                _postData = _postData.Substring(1) + "}";
                _postData = "{" + _postData;
            }
            return _postData;
        }

        #region virtual functions

        public virtual void Initialise()
        {
            dctPostDataValues = new Dictionary<string, string>();
            dctAppSettings = new Dictionary<string, string>();
            foreach (string Key in ConfigurationManager.AppSettings.AllKeys)
            {
                dctAppSettings.Add(Key, convert.ToString(ConfigurationManager.AppSettings[Key]).Trim());
            }
            AuditPath = AppDomain.CurrentDomain.BaseDirectory+"AuditLog";
            DownloadPath = AppDomain.CurrentDomain.BaseDirectory+"Download";
            LogPath = AppDomain.CurrentDomain.BaseDirectory + "Log";
            PrintScreenPath = AppDomain.CurrentDomain.BaseDirectory + "ScreenShots";

        }

        public virtual bool DoLogin(string validateNodeType, string validateAttribute, string attributeValue,bool bload=true)
        {            
            return Navigate(validateNodeType,validateAttribute,attributeValue,bload);
        }
       
        public virtual bool GetFilterScreen( string validateNodeType, string validateAttribute, string attributeValue,bool LoadScreen)
        {
            return Navigate(validateNodeType, validateAttribute, attributeValue,LoadScreen);
        }

        public virtual bool GetFilterScreenAddHeaders(string validateNodeType, string validateAttribute, string attributeValue)
        {
            return LoadURL(validateNodeType, validateAttribute, attributeValue, true);
        }

        public virtual bool OpenDetailView(string RequestURL, string validateNodeType, string validateAttribute, string attributeValue, string postData = "")
        {
            bool _result = false;
            URL = RequestURL;
            if (postData == "") _result = Navigate(validateNodeType, validateAttribute, attributeValue);
            else _result = Navigate(validateNodeType, validateAttribute, attributeValue);
            return _result;
        }

        public virtual bool DownloadRFQ(string RequestURL,string DownloadFileName,string ContentType="")
        {
            bool _result = false;
            _result = DownloadDocument(RequestURL, DownloadFileName, ContentType);
            return _result;
        }

        public virtual bool DownloadPO(string RequestURL,string DownloadFileName,string ContentType="")
        {
            bool _result = false;            
            _result = DownloadDocument(RequestURL, DownloadFileName, ContentType);
            return _result;
        }

        public virtual bool PrintScreen(string sFileName)
        {
            return _httpWrapper.PrintScreen(sFileName);
        }
        
        #endregion

        public bool DownloadDocument(string RequestURL, string DownloadFileName, string ContentType = "", bool PostData = true)  //[Sayli 17Feb18 : //Changed from 'private' to 'public'//Added 'bool PostData = true']
        {
            bool _result = false;
            if (PostData)//Sayli 17Feb18
            {
                string _postdata = GetPostData();
                _result = _httpWrapper.DownloadDocument(RequestURL, _postdata, DownloadFileName, ContentType);
            }
            else _result = _httpWrapper.DownloadDocument_Get(RequestURL, DownloadFileName, ContentType); //Sayli 17Feb18
            return _result;
            
        }

        private bool Navigate(string validateNodeType, string validateAttribute, string attributeValue, bool loadURL = false, bool _readResponse = true)// bool _readResponse=true//namrata
        {
            bool _result = false;
            if (loadURL) _httpWrapper.LoadURL(URL, "", "", "",SessionIDCookieName,HiddenAttributeKey);
            string _postdata = GetPostData();
            _result = _httpWrapper.PostURL(URL, _postdata, validateNodeType, validateAttribute, attributeValue, HiddenAttributeKey, _readResponse);// bool _readResponse=true//namrata
            _CurrentResponseString = _httpWrapper._CurrentResponseString;
            return _result;
        }

        public bool CheckInstance()//namrata
        {
            bool isResult = false;
            int nCnt = 0;
            try
            {
                Process currentProcess = Process.GetCurrentProcess();
                Process[] processesByName = Process.GetProcessesByName(currentProcess.ProcessName);
                for (int i = 0; i < (int)processesByName.Length; i++)
                {
                    if (processesByName[i].ProcessName == currentProcess.ProcessName)
                    {
                        nCnt++;
                    }
                }
                if (nCnt <= 1)
                {
                    isResult = true;
                }
                else
                {
                   LogText = "Application is already running";
                    processesByName.Select(x => x.Id != currentProcess.Id).ToList();
                    isResult = false;
                }
            }
            catch
            {

            }
            return isResult;
        }

        public void KillProcess()
        {
            Process current = Process.GetCurrentProcess();
            foreach (Process process in Process.GetProcessesByName(current.ProcessName))
            {
                TimeSpan timeSpan = DateTime.Now - process.StartTime;
                if (timeSpan.TotalMinutes >= 30)
                {
                    WriteLog( "Killing " + timeSpan.TotalMinutes + " minutes older instance");
                    process.Kill();
                }
            }
            current.Kill();
        }

       
       
        public class convert
        {
            public static long ToLong(object valueFromDb)
            {
                try
                {
                    return valueFromDb != DBNull.Value ? Convert.ToInt64(valueFromDb) : 0;
                }
                catch { return 0; }
            }

            public static double ToFloat(object valueFromDb)
            {
                try
                {
                    return valueFromDb != DBNull.Value ? Convert.ToDouble(valueFromDb) : 0.0F;
                }
                catch { return 0.0F; }
            }

            public static double ToDouble(object valueFromDb)
            {
                try
                {
                    return valueFromDb != DBNull.Value ? Convert.ToDouble(valueFromDb) : 0;
                }
                catch { return 0; }
            }

            public static Boolean ToBoolean(object valueFromDb)
            {
                try
                {
                    if (valueFromDb.ToString() == "1") return true;
                    else return false;
                }
                catch { return false; }
            }

            public static int ToInt(object valueFromDb)
            {
                try
                {
                    if (valueFromDb.ToString().ToUpper() == "TRUE") return 1;
                    else if (valueFromDb.ToString().ToUpper() == "YES") return 1;
                    return valueFromDb != DBNull.Value ? Convert.ToInt32(valueFromDb) : 0;
                }
                catch { return 0; }
            }

            public static string ToDBValue(int nValue)
            {
                try
                {
                    return nValue > 0 ? nValue.ToString() : "NULL";
                }
                catch { return "NULL"; }
            }

            public static object ToDBValue(string cValue)
            {
                try
                {
                    if (cValue.Trim() == "") return DBNull.Value;
                    else if (cValue.Trim() == "0") return DBNull.Value;
                    else return cValue;
                }
                catch { return DBNull.Value; }
            }

            public static string ToQuote(string Value)
            {
                if (Value == null) Value = "";
                Value = Value.Replace("'", "''");
                return ("'" + Value + "'");
            }

            public static DateTime ToDateTime(object valueFromDb)
            {
                try
                {
                    if ((valueFromDb.ToString() != null) && (valueFromDb.ToString().Length > 0))
                        return Convert.ToDateTime(valueFromDb);
                    else return DateTime.MinValue;
                }
                catch { return DateTime.MinValue; }
            }

            public static DateTime ToDateTime(object valueFromDb, string DateFormat)
            {
                try
                {
                    if (DateFormat != string.Empty)
                    {
                        if ((valueFromDb.ToString() != null) && (valueFromDb.ToString().Length > 0))
                            return DateTime.ParseExact(valueFromDb.ToString(), DateFormat, null);
                        else return DateTime.MinValue;
                    }
                    else
                    {
                        return ToDateTime(valueFromDb);
                    }
                }
                catch { return DateTime.MinValue; }
            }

            public static NameValueCollection ToNameValueCollection(DataSet ds)
            {
                DataTable dt = ds.Tables[0];
                NameValueCollection nvcReturn = new NameValueCollection();
                if (ds.Tables[0].Rows.Count > 0)
                {
                    for (int j = 0; j <= ds.Tables[0].Rows.Count - 1; j++)
                    {
                        DataRow dr = ds.Tables[0].Rows[j];

                        for (int i = 0; i <= dt.Columns.Count - 1; i++)
                        {
                            nvcReturn.Add(dt.Columns[i].ColumnName.ToString(), dr[dt.Columns[i].ColumnName.ToString()].ToString());
                        }
                    }
                }

                return nvcReturn;
            }

            public static Dictionary<string, string> ToDictionary(DataSet ds)
            {
                DataTable dt = ds.Tables[0];
                Dictionary<string, string> dicReturn = new Dictionary<string, string>();
                if (ds.Tables[0].Rows.Count > 0)
                {
                    for (int j = 0; j < ds.Tables[0].Rows.Count; j++)
                    {
                        DataRow dr = ds.Tables[0].Rows[j];

                        for (int i = 0; i < dt.Columns.Count; i++)
                        {
                            dicReturn.Add(dt.Columns[i].ColumnName.ToString(), dr[dt.Columns[i].ColumnName.ToString()].ToString());
                        }
                    }
                }
                return dicReturn;
            }

            public static string ToFileName(string cValue)
            {
                try
                {
                    cValue = cValue.Replace("-", "_");
                    cValue = cValue.Replace(" ", "_");
                    cValue = cValue.Replace("/", "_");
                    cValue = cValue.Replace("?", "_");
                    cValue = cValue.Replace("\\", "_");
                    cValue = cValue.Replace(":", "_");
                    cValue = cValue.Replace("*", "_");
                    cValue = cValue.Replace("<", "_");
                    cValue = cValue.Replace(">", "_");
                    cValue = cValue.Replace("|", "_");
                    cValue = cValue.Replace("\"", "_");
                    cValue = cValue.Replace("'", "");
                    cValue = cValue.Replace(",", "_");
                    return cValue;
                }
                catch { return cValue; }
            }

            public static string ToString(object valueFromDb)
            {
                try
                {
                    return valueFromDb != DBNull.Value ? Convert.ToString(valueFromDb) : "";
                }
                catch { return ""; }
            }

            public static string ToNumericOnly(string cInput)
            {
                string cNUM = "0123456789", cReturn = "";

                for (int i = 0; i < cInput.Length; i++)
                {
                    if (cNUM.IndexOf(cInput[i]) != -1)
                        cReturn += cInput[i];
                }
                return cReturn;
            }

            public static string ToXMLString(string cInput)
            {
                cInput = cInput.Replace("<", "&lt;");
                cInput = cInput.Replace(">", "&gt;");
                cInput = cInput.Replace(" & ", "&amp;");
                cInput = cInput.Replace("\"", "&quot;");
                cInput = cInput.Replace("\'", "&apos;");

                return cInput;
            }

            public static string ToPositive(object valueFromDb)
            {
                try
                {
                    if (valueFromDb.ToString().ToUpper() == "TRUE") return "1";
                    else if (valueFromDb == null) return "-";
                    else if (valueFromDb == DBNull.Value) return "-";
                    else if (convert.ToInt(valueFromDb) > 0) return convert.ToInt(valueFromDb).ToString();
                    else return "-";
                }
                catch { return "-"; }
            }

            public static Boolean CheckForNumericString(string cString)
            {
                Boolean blReturn = true;
                cString = cString.Replace("-", "");

                string cNUM = "0123456789";

                for (int i = 0; i < cString.Length; i++)
                {
                    if (cNUM.IndexOf(cString[i]) == -1)
                        return false;
                }

                return blReturn;
            }
        }


    }

   

}
