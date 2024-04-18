using System;
using System.Text;
using System.Net;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using HtmlAgilityPack;
using System.Web;

namespace HTTPRoutines
{
    public class HTTP:WebRequest
    {
        private string _hiddenAttr = "id";
        private bool isReload = false;
        private bool _isurlencoded = true; //namrata
        private string _requestMethod = "POST";
        #region Properties

     
        
        public CookieContainer _CookieContainer { get; set; }
        public HttpWebResponse _CurrentResponse { get; set; }
        
        public Dictionary<HttpRequestHeader, string> _SetRequestHeaders { get; set; }
        public Dictionary<string, string> _AddRequestHeaders { get; set; }        
        public string SessionID { get; set; }
        public string FileName { get; set; }                
        public bool RequireAuthentication { get; set; }
        public string PROXYURI { get; set; }
        public string ProxyUser { get; set; }
        public string ProxyPassword { get; set; }
        public string UserAgent { get; set; }
        public string AcceptMimeType { get; set; }
        public bool Expect100Continue { get; set; }
        public string Referrer { get; set; }
        public bool KeepAlive { get; set; }
        public string HiddenAttributeKey { get { return _hiddenAttr; } set { if (!String.IsNullOrEmpty(value)) _hiddenAttr = value; } }
        new public string ContentType { get; set; }
        public string DownloadFileContentType { get; set; }
        public bool IsUrlEncoded { get { return _isurlencoded; } set { _isurlencoded = value; } } //namrata
        public string RequestMethod { get { return _requestMethod; } set { _requestMethod = value; } } //simmy 27.02.2018 
        

        #endregion

        public HTTP(){
            _SetRequestHeaders = new Dictionary<HttpRequestHeader, string>();
            _AddRequestHeaders = new Dictionary<string, string>();            
        }

     
        public void SendGetRequest(string RequestURL)
        {
            WebResponse response = null;

            try
            {

                //  var _request = (HttpWebRequest)WebRequest.Create(RequestURL);
                //   _request.CookieContainer = this._CookieContainer;
                //   if (RequireAuthentication)
                //   {
                //       IWebProxy proxy = _request.Proxy;
                //       WebProxy myProxy = new WebProxy();
                //       Uri newUri = new Uri(PROXYURI);
                //       myProxy.Address = newUri;
                //       myProxy.Credentials = new NetworkCredential(ProxyUser, ProxyPassword);
                //       _request.Proxy = myProxy;
                //   }
                //   if (_CurrentResponse != null) _CurrentResponse.Close();
                //   _CurrentResponse = (HttpWebResponse)_request.GetResponse();
                ////   _CurrentResponseString = ReadResponse(_CurrentResponse);     

                //Create request to URL.              
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(RequestURL);
                request.CookieContainer = this._CookieContainer;
                //Set request headers.
                request.KeepAlive = KeepAlive;
                request.UserAgent = UserAgent;
                request.Referer = Referrer;
                request.Accept = AcceptMimeType;
                request.ContentType = ContentType;
                request.UseDefaultCredentials = true;
                request.PreAuthenticate = true;
                request.Credentials = CredentialCache.DefaultCredentials;
                SetRequestHeaders(ref request);
                //Get response to request.
                response = (HttpWebResponse)request.GetResponse();
                if (_CurrentResponse != null) _CurrentResponse.Close();
                _CurrentResponse = (HttpWebResponse)response;

            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    response = (HttpWebResponse)e.Response;
                    if (_CurrentResponse != null) _CurrentResponse.Close();
                    _CurrentResponse = (HttpWebResponse)response;
                }
                if (e.Status == WebExceptionStatus.Timeout)
                {
                    if (!isReload)
                    {
                        isReload = true;
                        SendGetRequest(RequestURL);
                    }
                    else
                    {
                        isReload = false;
                        if (response != null) response.Close();
                        throw e;
                    }
                }
                else
                {
                    isReload = false;
                    if (response != null) response.Close();
                    throw e;
                }

            }
            catch (Exception e)
            {
                if (response != null) response.Close();

                throw e;
            }
            finally {
                isReload = false;
            }
        }

        //public void SendGetRequestWithHeaders(string RequestURL)
        //{
        //    try
        //    {

        //        //Create request to URL.
        //        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(RequestURL);
        //        request.CookieContainer = this._CookieContainer;
        //        //Set request headers.
        //        request.KeepAlive = KeepAlive;
        //        request.UserAgent = UserAgent;
        //        request.Accept = AcceptMimeType;
        //        request.ContentType = ContentType;
        //        SetRequestHeaders(ref request);                                
        //        //Get response to request.
        //        WebResponse response = (HttpWebResponse)request.GetResponse();
        //        if (_CurrentResponse != null) _CurrentResponse.Close();
        //        _CurrentResponse = (HttpWebResponse)response;
        //      //  _CurrentResponseString = ReadResponse(_CurrentResponse);

        //    }
        //    catch (Exception e) { throw e; }
        //}


        public void SendPostRequest(string Request, string postData)
        {
            WebResponse response = null;

            try
            {

                var request = (HttpWebRequest)WebRequest.Create(Request);
                request.CookieContainer = _CookieContainer;
                request.KeepAlive = KeepAlive;               
                SetRequestHeaders(ref request);
                request.ContentType = ContentType;
                request.UserAgent = UserAgent;
                request.Accept = AcceptMimeType;
                request.Referer = Referrer;
                request.Method = _requestMethod;
                request.ServicePoint.Expect100Continue = Expect100Continue;
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);                
                request.ContentLength = byteArray.Length;
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
                 response = request.GetResponse();
                if (_CurrentResponse != null) _CurrentResponse.Close();
                _CurrentResponse = (HttpWebResponse)response;
                //_CurrentResponseString = ReadResponse(_CurrentResponse);

            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError) {
                    response = (HttpWebResponse)e.Response;
                    if (_CurrentResponse != null) _CurrentResponse.Close();
                    _CurrentResponse = (HttpWebResponse)response;
                
                }

                if (e.Status == WebExceptionStatus.Timeout)
                {
                    if (!isReload)
                    {
                        isReload = true;
                        SendPostRequest(Request, postData);
                    }
                    else
                    {
                        isReload = false;
                        if (response != null) response.Close();
                        throw e;
                    }
                }
                else
                {
                    isReload = false;
                    if (response != null) response.Close();
                    throw e;
                }
                
            }        
            catch (Exception e)
            {
                if (response != null) response.Close();
                throw e;
            }
        }


        private string ReadResponse(HttpWebResponse response)
        {
            try
            {
                using (Stream responseStream = response.GetResponseStream())
                {
                    Stream streamToRead = responseStream;
                    if (response.ContentEncoding.ToLower().Contains("gzip"))
                    {
                        streamToRead = new GZipStream(streamToRead, CompressionMode.Decompress);
                    }
                    else if (response.ContentEncoding.ToLower().Contains("deflate"))
                    {
                        streamToRead = new DeflateStream(streamToRead, CompressionMode.Decompress);
                    }

                    using (StreamReader streamReader = new StreamReader(streamToRead, Encoding.UTF8))
                    {
                        return streamReader.ReadToEnd();
                    }
                }
            }
            catch (Exception e)
            { throw e; }
        }

        private void SetRequestHeaders(ref HttpWebRequest _request)
        {
            foreach (KeyValuePair<HttpRequestHeader, string> _kv in _SetRequestHeaders)
            {
                _request.Headers.Set(_kv.Key, _kv.Value);
            }

            foreach (KeyValuePair<string, string> _kv in _AddRequestHeaders)
            {
                _request.Headers.Add(_kv.Key, _kv.Value);
            }
        }

        public Dictionary<string, string> GetStateInfo(string Request) 
        {
            try{
            Dictionary<string, string> dctState = new Dictionary<string, string>();
            SendGetRequest(Request);
            string _responseString = ReadResponse(_CurrentResponse);
            return getStateInfo(_responseString);   
            }
            catch{throw;}

        }

        public Dictionary<string, string> GetStateInfo(string Request, string postData) 
        {
            try
            {                
                SendPostRequest(Request, postData);
                string _CurrentResponseString = ReadResponse(_CurrentResponse);
                return getStateInfo(_CurrentResponseString); 
            } 
            catch { throw; }

        }

        public Dictionary<string, string> GetStateInfo(HtmlDocument _doc,  string _CurrentResponseString = "")
        {
            try
            {
                return getStateInfo(_CurrentResponseString,  _doc); 
            }
            catch { throw; }

        }

        private Dictionary<string, string> getStateInfo(string _CurrentResponseString,  HtmlDocument _doc = null) 
        {
            Dictionary<string, string> dctState = new Dictionary<string, string>();
            if (_doc == null)
            {
                _doc = new HtmlAgilityPack.HtmlDocument();
                if (_CurrentResponseString != "") _doc.LoadHtml(_CurrentResponseString);
            }
            
            HtmlNodeCollection _nodeCollection = _doc.DocumentNode.SelectNodes("//input[@type='hidden']");
            string sKey="",sValue="";
            if (_nodeCollection != null)
            {
                foreach (HtmlNode _hiddenNode in _nodeCollection)
                {
                    if ((_hiddenNode.Attributes["value"] != null) && (_hiddenNode.Attributes[HiddenAttributeKey] != null))
                    {
                        if (IsUrlEncoded)//namrata
                        {
                            sValue = HttpUtility.UrlEncode(_hiddenNode.Attributes["value"].Value);
                            sKey = HttpUtility.UrlEncode(_hiddenNode.Attributes[HiddenAttributeKey].Value);
                        }
                        else//namrata
                        {
                            sValue = Uri.EscapeDataString(_hiddenNode.Attributes["value"].Value);
                            sKey = Uri.EscapeDataString(_hiddenNode.Attributes[HiddenAttributeKey].Value);
                        }
                        if (sKey != "" && !dctState.ContainsKey(sKey)) dctState.Add(sKey, sValue);
                    }
                }
            }
            return dctState;

        }

      


        #region Print PDF
        public void PrintScreen(string sFileName, string _CurrentResponseString)
        {
            try
            {

               string sHTML = _CurrentResponseString;
               if (sHTML != null)//namrata
               {
                   string HTMLFile = Path.ChangeExtension(sFileName, ".html");
                   File.WriteAllText(HTMLFile, sHTML.Trim());
                   SautinSoft.PdfVision _vision = new SautinSoft.PdfVision();
                   SautinSoft.PdfVision.TrySetBrowserModeEdgeInRegistry();
                   _vision.PageStyle.PageOrientation.Auto();
                   _vision.ConvertHtmlFileToImageFile(HTMLFile, sFileName, SautinSoft.PdfVision.eImageFormat.Png);
               }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void DownloadDocument(string Request, string DownloadFile) //[Sayli 17Feb18 : Added function]
        {
            try
            {
                FileName = DownloadFile;
                var request = (HttpWebRequest)WebRequest.Create(Request);
                request.Timeout = 420000;
                //request.CookieContainer = this._CookieContainer;
                request.KeepAlive = KeepAlive;
                //request.UserAgent = UserAgent;
                request.Accept = AcceptMimeType;
                //request.ContentType = ContentType;
                SetRequestHeaders(ref request);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (DownloadFileContentType != "" && response.ContentType != DownloadFileContentType)
                {
                  //  string _CurrentResponseString = ReadResponse(response);
                    return;
                }
                byte[] b = null;
                FileStream fileStream = File.OpenWrite(FileName);
                byte[] buffer = new byte[1024];
                using (Stream input = response.GetResponseStream())
                using (MemoryStream ms = new MemoryStream())
                {
                    int count = 0;
                    do
                    {
                        byte[] buf = new byte[1024];
                        count = input.Read(buf, 0, 1024);
                        ms.Write(buf, 0, count);
                    } while (input.CanRead && count > 0);
                    b = ms.ToArray();
                }
                fileStream.Write(b, 0, b.Length);
                fileStream.Flush();
                fileStream.Close();                
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public void DownloadDocument(string Request,string postData, string DownloadFile)
        {
            try
            {
                FileName = DownloadFile;                
                var request = (HttpWebRequest)WebRequest.Create(Request);
                request.KeepAlive = KeepAlive;
                SetRequestHeaders(ref request);               
                request.Referer = Referrer;                
                request.Method = "POST";
                request.ProtocolVersion = HttpVersion.Version10;
                request.ServicePoint.Expect100Continue = Expect100Continue;
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                request.ContentType = ContentType;
                request.ContentLength = byteArray.Length;
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
                request.BeginGetResponse(new AsyncCallback(PlayResponeAsync), request);
            }
            catch (Exception e)
            {
                throw e;
            }
        }        
        
        private void PlayResponeAsync(IAsyncResult asyncResult)
        {
            HttpWebRequest webRequest = (HttpWebRequest)asyncResult.AsyncState;
            byte[] b = null;
            
            using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.EndGetResponse(asyncResult))
            {
                
                if (DownloadFileContentType != "" && webResponse.ContentType != DownloadFileContentType)
                {
                    string _CurrentResponseString = ReadResponse(webResponse);            
                    return; 
                }
                else
                {
                    FileStream fileStream = File.OpenWrite(FileName);
                    byte[] buffer = new byte[1024];
                    using (Stream input = webResponse.GetResponseStream())
                    using (MemoryStream ms = new MemoryStream())
                    {
                        int count = 0;
                        do
                        {
                            byte[] buf = new byte[1024];
                            count = input.Read(buf, 0, 1024);
                            ms.Write(buf, 0, count);
                        } while (input.CanRead && count > 0);
                        b = ms.ToArray();
                    }
                    fileStream.Write(b, 0, b.Length);
                    fileStream.Flush();
                    fileStream.Close();
                }
                
            }
          
        }

        #endregion
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
