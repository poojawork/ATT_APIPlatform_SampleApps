﻿// <copyright file="Default.aspx.cs" company="AT&amp;T">
// Licensed by AT&amp;T under 'Software Development Kit Tools Agreement.' 2013
// TERMS AND CONDITIONS FOR USE, REPRODUCTION, AND DISTRIBUTION: http://developer.att.com/sdk_agreement/
// Copyright 2013 AT&amp;T Intellectual Property. All rights reserved. http://developer.att.com
// For more information contact developer.support@att.com
// </copyright>

#region References

using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;


#endregion
public partial class IMMN_App1 : System.Web.UI.Page
{
    #region Instance variables

    private string endPoint;

    private string apiKey, authCode, authorizeRedirectUri, secretKey, accessToken,
        scope, refreshToken, refreshTokenExpiryTime, accessTokenExpiryTime;

    private int maxAddresses;

    private List<string> addressList = new List<string>();

    private string phoneNumbersParameter = null;

    private int refreshTokenExpiresIn;

    private string AttachmentFilesDir = string.Empty;

    #endregion
    public List<string> attachments = null;
    public string sendMessageSuccessResponse = string.Empty;
    public string sendMessageErrorResponse = string.Empty;
    public string getHeadersErrorResponse = string.Empty;
    public string getHeadersSuccessResponse = string.Empty;
    public string getMessageSuccessResponse = string.Empty;
    public string getMessageContentSuccessResponse = string.Empty;
    public string getMessageContentErrorResponse = string.Empty;
    public string getMessageErrorResponse = string.Empty;
    public string filters = string.Empty;

    public string createMessageIndexSuccessResponse = string.Empty;
    public string createMessageIndexErrorResponse = string.Empty;
    public string csGetMessageListDetailsErrorResponse = string.Empty;
    public string csGetMessageListDetailsSuccessResponse = string.Empty;
    public string getNotificationConnectionDetailsSuccessResponse = string.Empty;
    public string getNotificationConnectionDetailsErrorResponse = string.Empty;
    public csNotificationConnectionDetails getNotificationConnectionDetailsResponse = new csNotificationConnectionDetails();
    public MessageList csGetMessageListDetailsResponse = new MessageList();
    public DeltaResponse csDeltaResponse = new DeltaResponse();
    public csMessageContentDetails getMessageContentResponse = new csMessageContentDetails();
    public Message getMessageDetailsResponse = new Message();
    public MessageIndexInfo getMessageIndexInfoResponse = new MessageIndexInfo();
    public string deleteMessageSuccessResponse = string.Empty;
    public string deleteMessageErrorResponse = string.Empty;
    public string updateMessageSuccessResponse = string.Empty;
    public string updateMessageErrorResponse = string.Empty;
    public string messageIndexSuccessResponse = string.Empty;
    public string messageIndexErrorResponse = string.Empty;
    public string deltaSuccessResponse = string.Empty;
    public string deltaErrorResponse = string.Empty;
    public string getMessageListSuccessResponse = string.Empty;
    public string getMessageListErrorResponse = string.Empty;

    public string content_result = string.Empty;
    public byte[] receivedBytes = null;
    public WebResponse getContentResponseObject = null;
    public string[] imageData = null;
    public string showSendMsg = string.Empty;
    public string showCreateMessageIndex = string.Empty;
    public string showGetNotificationConnectionDetails = string.Empty;
    public string showDeleteMessage = string.Empty;
    public string showUpdateMessage = string.Empty;
    public string showGetMessage = string.Empty;

    protected void Page_Load(object sender, EventArgs e)
    {
        this.BypassCertificateError();
        this.ReadConfigFile();

        if ((Session["cs_rest_appState"] == "GetToken") && (Request["Code"] != null))
        {
            this.authCode = Request["code"].ToString();
            if (this.GetAccessToken(AccessTokenType.Authorization_Code) == true)
            {
                RestoreRequestSessionVariables();
                ResetRequestSessionVariables();
                if (string.Compare(Session["cs_rest_ServiceRequest"].ToString(), "sendmessasge") == 0)
                    this.SendMessageRequest();
                else if (string.Compare(Session["cs_rest_ServiceRequest"].ToString(), "getmessagecontent") == 0)
                    this.GetMessageContentByIDnPartNumber();
                else if (string.Compare(Session["cs_rest_ServiceRequest"].ToString(), "createmessageindex") == 0)
                    this.createMessageIndex();
                else if (string.Compare(Session["cs_rest_ServiceRequest"].ToString(), "getnotificationconnectiondetails") == 0)
                    this.getNotificationConnectionDetails();
                else if (string.Compare(Session["cs_rest_ServiceRequest"].ToString(), "deletemessage") == 0)
                    this.deleteMessage();
                else if (string.Compare(Session["cs_rest_ServiceRequest"].ToString(), "deltamessage") == 0)
                    this.getDelta();
                else if (string.Compare(Session["cs_rest_ServiceRequest"].ToString(), "getmessagelist") == 0)
                    this.getMessageList();
                else if (string.Compare(Session["cs_rest_ServiceRequest"].ToString(), "messageindex") == 0)
                    this.getMessageIndex();
                else if (string.Compare(Session["cs_rest_ServiceRequest"].ToString(), "updatemessage") == 0)
                    this.updateMessage();
            }
            else
            {
                sendMessageErrorResponse = "Failed to get Access token";
                this.ResetTokenSessionVariables();
                this.ResetTokenVariables();
                return;
            }
        }

    }

    #region Access Token functions

    private void ResetTokenSessionVariables()
    {
        Session["cs_rest_AccessToken"] = null;
        Session["cs_rest_AccessTokenExpirtyTime"] = null;
        Session["cs_rest_RefreshToken"] = null;
        Session["cs_rest_RefreshTokenExpiryTime"] = null;
    }
    private void ResetTokenVariables()
    {
        this.accessToken = null;
        this.refreshToken = null;
        this.refreshTokenExpiryTime = null;
        this.accessTokenExpiryTime = null;
    }
    private void GetAuthCode()
    {
        try
        {
            Response.Redirect(string.Empty + this.endPoint + "/oauth/authorize?scope=" + this.scope + "&client_id=" + this.apiKey + "&redirect_url=" + this.authorizeRedirectUri);
        }
        catch (Exception ex)
        {
            if (Session["cs_rest_ServiceRequest"] != null && (string.Compare(Session["cs_rest_ServiceRequest"].ToString(),
                                                                        "sendmessasge") == 0))
            {
                sendMessageErrorResponse = ex.Message;
            }
            else if (Session["cs_rest_ServiceRequest"] != null && (string.Compare(Session["cs_rest_ServiceRequest"].ToString(),
                                                                        "createmessageindex") == 0))
            {
                createMessageIndexErrorResponse = ex.Message;
            }
            else if (Session["cs_rest_ServiceRequest"] != null && (string.Compare(Session["cs_rest_ServiceRequest"].ToString(),
                                                        "getnotificationconnectiondetails") == 0))
            {
                getNotificationConnectionDetailsErrorResponse = ex.Message;
            }
            else if (Session["cs_rest_ServiceRequest"] != null && (string.Compare(Session["cs_rest_ServiceRequest"].ToString(),
                                                        "deletemessage") == 0))
            {
                deleteMessageErrorResponse = ex.Message;
            }
            else if (Session["cs_rest_ServiceRequest"] != null && (string.Compare(Session["cs_rest_ServiceRequest"].ToString(),
                                                    "deltamessage") == 0))
            {
                deltaErrorResponse = ex.Message;
            }
            else if (Session["cs_rest_ServiceRequest"] != null && (string.Compare(Session["cs_rest_ServiceRequest"].ToString(),
                                                    "getmessagelist") == 0))
            {
                getMessageListErrorResponse = ex.Message;
            }
            else if (Session["cs_rest_ServiceRequest"] != null && (string.Compare(Session["cs_rest_ServiceRequest"].ToString(),
                                                "messageindex") == 0))
            {
                messageIndexErrorResponse = ex.Message;
            }
            else if (Session["cs_rest_ServiceRequest"] != null && (string.Compare(Session["cs_rest_ServiceRequest"].ToString(),
                                                "updatemessage") == 0))
            {
                updateMessageErrorResponse = ex.Message;
            }
            else
            {
                getMessageErrorResponse = ex.Message;
            }
        }
    }

    private bool ReadTokenSessionVariables()
    {
        if (Session["cs_rest_AccessToken"] != null)
        {
            this.accessToken = Session["cs_rest_AccessToken"].ToString();
        }
        else
        {
            this.accessToken = null;
        }

        if (Session["cs_rest_AccessTokenExpirtyTime"] != null)
        {
            this.accessTokenExpiryTime = Session["cs_rest_AccessTokenExpirtyTime"].ToString();
        }
        else
        {
            this.accessTokenExpiryTime = null;
        }
        if (Session["cs_rest_RefreshToken"] != null)
        {
            this.refreshToken = Session["cs_rest_RefreshToken"].ToString();
        }
        else
        {
            this.refreshToken = null;
        }
        if (Session["cs_rest_RefreshTokenExpiryTime"] != null)
        {
            this.refreshTokenExpiryTime = Session["cs_rest_RefreshTokenExpiryTime"].ToString();
        }
        else
        {
            this.refreshTokenExpiryTime = null;
        }

        if ((this.accessToken == null) || (this.accessTokenExpiryTime == null) || (this.refreshToken == null) || (this.refreshTokenExpiryTime == null))
        {
            return false;
        }

        return true;
    }

    private string IsTokenValid()
    {
        if (Session["cs_rest_AccessToken"] == null)
        {
            return "INVALID_ACCESS_TOKEN";
        }

        try
        {
            DateTime currentServerTime = DateTime.UtcNow.ToLocalTime();
            if (currentServerTime >= DateTime.Parse(this.accessTokenExpiryTime))
            {
                if (currentServerTime >= DateTime.Parse(this.refreshTokenExpiryTime))
                {
                    return "INVALID_ACCESS_TOKEN";
                }
                else
                {
                    return "REFRESH_TOKEN";
                }
            }
            else
            {
                return "VALID_ACCESS_TOKEN";
            }
        }
        catch
        {
            return "INVALID_ACCESS_TOKEN";
        }
    }
    private bool GetAccessToken(AccessTokenType type)
    {
        Stream postStream = null;
        try
        {
            DateTime currentServerTime = DateTime.UtcNow.ToLocalTime();
            WebRequest accessTokenRequest = System.Net.HttpWebRequest.Create(string.Empty + this.endPoint + "/oauth/token");
            accessTokenRequest.Method = "POST";
            string oauthParameters = string.Empty;

            if (type == AccessTokenType.Authorization_Code)
            {
                oauthParameters = "client_id=" + this.apiKey + "&client_secret=" + this.secretKey + "&code=" + this.authCode + "&grant_type=authorization_code&scope=" + this.scope;
            }
            else
            {
                oauthParameters = "grant_type=refresh_token&client_id=" + this.apiKey + "&client_secret=" + this.secretKey + "&refresh_token=" + this.refreshToken;
            }

            accessTokenRequest.ContentType = "application/x-www-form-urlencoded";
            UTF8Encoding encoding = new UTF8Encoding();
            byte[] postBytes = encoding.GetBytes(oauthParameters);
            accessTokenRequest.ContentLength = postBytes.Length;
            postStream = accessTokenRequest.GetRequestStream();
            postStream.Write(postBytes, 0, postBytes.Length);
            postStream.Close();

            WebResponse accessTokenResponse = accessTokenRequest.GetResponse();
            using (StreamReader accessTokenResponseStream = new StreamReader(accessTokenResponse.GetResponseStream()))
            {
                string access_token_json = accessTokenResponseStream.ReadToEnd();
                JavaScriptSerializer deserializeJsonObject = new JavaScriptSerializer();
                AccessTokenResponse deserializedJsonObj = (AccessTokenResponse)deserializeJsonObject.Deserialize(access_token_json, typeof(AccessTokenResponse));
                if (deserializedJsonObj.access_token != null)
                {
                    this.accessToken = deserializedJsonObj.access_token;
                    this.refreshToken = deserializedJsonObj.refresh_token;

                    DateTime refreshExpiry = currentServerTime.AddHours(this.refreshTokenExpiresIn);

                    Session["cs_rest_AccessTokenExpirtyTime"] = currentServerTime.AddSeconds(Convert.ToDouble(deserializedJsonObj.expires_in));

                    if (deserializedJsonObj.expires_in.Equals("0"))
                    {
                        int defaultAccessTokenExpiresIn = 100; // In Years
                        Session["cs_rest_AccessTokenExpirtyTime"] = currentServerTime.AddYears(defaultAccessTokenExpiresIn);
                    }

                    this.refreshTokenExpiryTime = refreshExpiry.ToLongDateString() + " " + refreshExpiry.ToLongTimeString();

                    Session["cs_rest_AccessToken"] = this.accessToken;

                    this.accessTokenExpiryTime = Session["cs_rest_AccessTokenExpirtyTime"].ToString();
                    Session["cs_rest_RefreshToken"] = this.refreshToken;
                    Session["cs_rest_RefreshTokenExpiryTime"] = this.refreshTokenExpiryTime.ToString();
                    Session["cs_rest_appState"] = "TokenReceived";
                    accessTokenResponseStream.Close();
                    return true;
                }
                else
                {
                    string errorMessage = "Auth server returned null access token";
                    if (Session["cs_rest_ServiceRequest"] != null && (string.Compare(Session["cs_rest_ServiceRequest"].ToString(),
                                                            "sendmessasge") == 0))
                    {
                        sendMessageErrorResponse = errorMessage;
                    }
                    else if (Session["cs_rest_ServiceRequest"] != null && (string.Compare(Session["cs_rest_ServiceRequest"].ToString(),
                                        "createmessageindex") == 0))
                    {
                        createMessageIndexErrorResponse = errorMessage;
                    }
                    else if (Session["cs_rest_ServiceRequest"] != null && (string.Compare(Session["cs_rest_ServiceRequest"].ToString(),
                                        "getnotificationconnectiondetails") == 0))
                    {
                        getNotificationConnectionDetailsErrorResponse = errorMessage;
                    }
                    else if (Session["cs_rest_ServiceRequest"] != null && (string.Compare(Session["cs_rest_ServiceRequest"].ToString(),
                                        "deletemessage") == 0))
                    {
                        deleteMessageErrorResponse = errorMessage;
                    }
                    else
                    {
                        getMessageErrorResponse = errorMessage;
                    }
                    return false;
                }
            }
        }
        catch (Exception ex)
        {
            string errorMessage = ex.Message;
            if (Session["cs_rest_ServiceRequest"] != null && (string.Compare(Session["cs_rest_ServiceRequest"].ToString(),
                                                    "sendmessasge") == 0))
            {
                sendMessageErrorResponse = errorMessage;
            }
            else if (Session["cs_rest_ServiceRequest"] != null && (string.Compare(Session["cs_rest_ServiceRequest"].ToString(),
                                                    "createmessageindex") == 0))
            {
                createMessageIndexErrorResponse = errorMessage;
            }
            else if (Session["cs_rest_ServiceRequest"] != null && (string.Compare(Session["cs_rest_ServiceRequest"].ToString(),
                                    "getnotificationconnectiondetails") == 0))
            {
                getNotificationConnectionDetailsErrorResponse = errorMessage;
            }
            else if (Session["cs_rest_ServiceRequest"] != null && (string.Compare(Session["cs_rest_ServiceRequest"].ToString(),
                                        "deletemessage") == 0))
            {
                deleteMessageErrorResponse = errorMessage;
            }
        }
        finally
        {
            if (null != postStream)
            {
                postStream.Close();
            }
        }

        return false;
    }

    #endregion
    private bool ReadConfigFile()
    {
        this.endPoint = ConfigurationManager.AppSettings["endPoint"];
        if (string.IsNullOrEmpty(this.endPoint))
        {
            sendMessageErrorResponse = "endPoint is not defined in configuration file";
            return false;
        }

        this.apiKey = ConfigurationManager.AppSettings["api_key"];
        if (string.IsNullOrEmpty(this.apiKey))
        {
            sendMessageErrorResponse = "api_key is not defined in configuration file";
            return false;
        }

        this.secretKey = ConfigurationManager.AppSettings["secret_key"];
        if (string.IsNullOrEmpty(this.secretKey))
        {
            sendMessageErrorResponse = "secret_key is not defined in configuration file";
            return false;
        }

        this.authorizeRedirectUri = ConfigurationManager.AppSettings["authorize_redirect_uri"];
        if (string.IsNullOrEmpty(this.authorizeRedirectUri))
        {
            sendMessageErrorResponse = "authorize_redirect_uri is not defined in configuration file";
            return false;
        }

        this.scope = ConfigurationManager.AppSettings["scope"];
        if (string.IsNullOrEmpty(this.scope))
        {
            this.scope = "IMMN,MIM";
        }

        if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["max_addresses"]))
        {
            this.maxAddresses = 10;
        }
        else
        {
            this.maxAddresses = Convert.ToInt32(ConfigurationManager.AppSettings["max_addresses"]);
        }

        string refreshTokenExpires = ConfigurationManager.AppSettings["refreshTokenExpiresIn"];
        if (!string.IsNullOrEmpty(refreshTokenExpires))
        {
            this.refreshTokenExpiresIn = Convert.ToInt32(refreshTokenExpires);
        }
        else
        {
            this.refreshTokenExpiresIn = 24;
        }

        if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["AttachmentFilesDir"]))
        {
            this.AttachmentFilesDir = Request.MapPath(ConfigurationManager.AppSettings["AttachmentFilesDir"]);
        }

        if (!IsPostBack)
        {

            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["AttachmentFilesDir"]))
            {
                string[] filePaths = Directory.GetFiles(this.AttachmentFilesDir);
                foreach (string filePath in filePaths)
                {
                    attachment.Items.Add(Path.GetFileName(filePath));
                }
            }
        }
        return true;
    }

    private string GetContentTypeFromExtension(string extension)
    {
        Dictionary<string, string> extensionToContentType = new Dictionary<string, string>()
            {
                { ".jpg", "image/jpeg" }, { ".bmp", "image/bmp" }, { ".mp3", "audio/mp3" },
                { ".m4a", "audio/m4a" }, { ".gif", "image/gif" }, { ".3gp", "video/3gpp" },
                { ".3g2", "video/3gpp2" }, { ".wmv", "video/x-ms-wmv" }, { ".m4v", "video/x-m4v" },
                { ".amr", "audio/amr" }, { ".mp4", "video/mp4" }, { ".avi", "video/x-msvideo" },
                { ".mov", "video/quicktime" }, { ".mpeg", "video/mpeg" }, { ".wav", "audio/x-wav" },
                { ".aiff", "audio/x-aiff" }, { ".aifc", "audio/x-aifc" }, { ".midi", ".midi" },
                { ".au", "audio/basic" }, { ".xwd", "image/x-xwindowdump" }, { ".png", "image/png" },
                { ".tiff", "image/tiff" }, { ".ief", "image/ief" }, { ".txt", "text/plain" },
                { ".html", "text/html" }, { ".vcf", "text/x-vcard" }, { ".vcs", "text/x-vcalendar" },
                { ".mid", "application/x-midi" }, { ".imy", "audio/iMelody" }
            };
        if (extensionToContentType.ContainsKey(extension))
        {
            return extensionToContentType[extension];
        }
        else
        {
            return "Not Found";
        }
    }

    private void SendMessageRequest(string accToken, string edPoint, string subject, string message, string groupflag, ArrayList attachments)
    {
        Stream postStream = null;
        try
        {
            string boundaryToSend = "----------------------------" + DateTime.Now.Ticks.ToString("x");

            HttpWebRequest msgRequestObject = (HttpWebRequest)WebRequest.Create(string.Empty + edPoint + "/myMessages/v2/messages");
            msgRequestObject.Headers.Add("Authorization", "Bearer " + accToken);
            msgRequestObject.Method = "POST";
            string contentType = "multipart/related; type=\"application/x-www-form-urlencoded\"; start=\"startpart\"; boundary=\"" + boundaryToSend + "\"\r\n";
            msgRequestObject.ContentType = contentType;
            //msgRequestObject.Accept = "application/xml";
            string mmsParameters = this.phoneNumbersParameter + "subject=" + Server.UrlEncode(subject) + "&text=" + Server.UrlEncode(message) + "&isGroup=" + groupflag;
            string dataToSend = string.Empty;
            dataToSend += "--" + boundaryToSend + "\r\n";
            dataToSend += "Content-Disposition: form-data; name=\"root-fields\"\r\n" + "Content-Type: application/x-www-form-urlencoded; charset=UTF-8\r\nContent-Transfer-Encoding: 8bit\r\nContent-ID: startpart\r\n\r\n" + mmsParameters + "\r\n";

            UTF8Encoding encoding = new UTF8Encoding();
            if ((attachments == null) || (attachments.Count == 0))
            {
                if (!groupCheckBox.Checked)
                {
                    msgRequestObject.ContentType = "application/x-www-form-urlencoded";
                    byte[] postBytes = encoding.GetBytes(mmsParameters);
                    msgRequestObject.ContentLength = postBytes.Length;

                    postStream = msgRequestObject.GetRequestStream();
                    postStream.Write(postBytes, 0, postBytes.Length);
                    postStream.Close();

                    WebResponse mmsResponseObject1 = msgRequestObject.GetResponse();
                    using (StreamReader sr = new StreamReader(mmsResponseObject1.GetResponseStream()))
                    {
                        string mmsResponseData = sr.ReadToEnd();
                        JavaScriptSerializer deserializeJsonObject = new JavaScriptSerializer();
                        MsgResponseId deserializedJsonObj = (MsgResponseId)deserializeJsonObject.Deserialize(mmsResponseData, typeof(MsgResponseId));
                        sendMessageSuccessResponse = deserializedJsonObj.Id.ToString();
                        sr.Close();
                    }
                }
                else
                {
                    dataToSend += "--" + boundaryToSend + "--\r\n";
                    byte[] bytesToSend = encoding.GetBytes(dataToSend);

                    int sizeToSend = bytesToSend.Length;

                    var memBufToSend = new MemoryStream(new byte[sizeToSend], 0, sizeToSend, true, true);
                    memBufToSend.Write(bytesToSend, 0, bytesToSend.Length);

                    byte[] finalData = memBufToSend.GetBuffer();
                    msgRequestObject.ContentLength = finalData.Length;

                    postStream = msgRequestObject.GetRequestStream();
                    postStream.Write(finalData, 0, finalData.Length);

                    WebResponse mmsResponseObject1 = msgRequestObject.GetResponse();
                    using (StreamReader sr = new StreamReader(mmsResponseObject1.GetResponseStream()))
                    {
                        string mmsResponseData = sr.ReadToEnd();
                        JavaScriptSerializer deserializeJsonObject = new JavaScriptSerializer();
                        MsgResponseId deserializedJsonObj = (MsgResponseId)deserializeJsonObject.Deserialize(mmsResponseData, typeof(MsgResponseId));
                        sendMessageSuccessResponse = deserializedJsonObj.Id.ToString();
                        sr.Close();
                    }
                }
            }
            else
            {
                byte[] dataBytes = encoding.GetBytes(string.Empty);
                byte[] totalDataBytes = encoding.GetBytes(string.Empty);
                int count = 0;
               
                foreach (string attachment in attachments)
                {
                    string mmsFileName = Path.GetFileName(attachment.ToString());
                    string mmsFileExtension = Path.GetExtension(attachment.ToString());
                    string attachmentContentType = this.GetContentTypeFromExtension(mmsFileExtension);
                    FileStream imageFileStream = new FileStream(attachment.ToString(), FileMode.Open, FileAccess.Read);
                    BinaryReader imageBinaryReader = new BinaryReader(imageFileStream);
                    byte[] image = imageBinaryReader.ReadBytes((int)imageFileStream.Length);
                    imageBinaryReader.Close();
                    imageFileStream.Close();
                    if (count == 0)
                    {
                        dataToSend += "\r\n--" + boundaryToSend + "\r\n";
                    }
                    else
                    {
                        dataToSend = "\r\n--" + boundaryToSend + "\r\n";
                    }

                    dataToSend += "Content-Disposition: form-data; name=\""+ mmsFileName +"\"; filename=" + mmsFileName + "\r\n";
                    dataToSend += "Content-Type: " + attachmentContentType + "; charset=UTF-8" + "\r\n";
                    dataToSend += "Content-ID:" + mmsFileName + "\r\n";                    
                    dataToSend += "Content-Transfer-Encoding: binary \r\n";
                    dataToSend += "Content-Location: " + mmsFileName + "\r\n\r\n";
                    
                    byte[] dataToSendByte = encoding.GetBytes(dataToSend);
                    int dataToSendSize = dataToSendByte.Length + image.Length;
                    var tempMemoryStream = new MemoryStream(new byte[dataToSendSize], 0, dataToSendSize, true, true);
                    tempMemoryStream.Write(dataToSendByte, 0, dataToSendByte.Length);
                    tempMemoryStream.Write(image, 0, image.Length);
                    dataBytes = tempMemoryStream.GetBuffer();
                    if (count == 0)
                    {
                        totalDataBytes = dataBytes;
                    }
                    else
                    {
                        byte[] tempForTotalBytes = totalDataBytes;
                        var tempMemoryStreamAttach = this.JoinTwoByteArrays(tempForTotalBytes, dataBytes);
                        totalDataBytes = tempMemoryStreamAttach.GetBuffer();
                    }

                    count++;
                }

                byte[] byteLastBoundary = encoding.GetBytes("\r\n--" + boundaryToSend + "--\r\n");
                int totalDataSize = totalDataBytes.Length + byteLastBoundary.Length;
                var totalMemoryStream = new MemoryStream(new byte[totalDataSize], 0, totalDataSize, true, true);
                totalMemoryStream.Write(totalDataBytes, 0, totalDataBytes.Length);
                totalMemoryStream.Write(byteLastBoundary, 0, byteLastBoundary.Length);
                byte[] finalpostBytes = totalMemoryStream.GetBuffer();

                msgRequestObject.ContentLength = finalpostBytes.Length;
                
                postStream = msgRequestObject.GetRequestStream();
                postStream.Write(finalpostBytes, 0, finalpostBytes.Length);

                WebResponse mmsResponseObject1 = msgRequestObject.GetResponse();
                using (StreamReader sr = new StreamReader(mmsResponseObject1.GetResponseStream()))
                {
                    string mmsResponseData = sr.ReadToEnd();
                    JavaScriptSerializer deserializeJsonObject = new JavaScriptSerializer();
                    MsgResponseId deserializedJsonObj = (MsgResponseId)deserializeJsonObject.Deserialize(mmsResponseData, typeof(MsgResponseId));
                    sendMessageSuccessResponse = deserializedJsonObj.Id.ToString();
                    sr.Close();
                }
            }
        }
        catch (WebException we)
        {
            if (null != we.Response)
            {
                using (Stream stream = we.Response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(stream);
                    sendMessageErrorResponse = reader.ReadToEnd();
                }
            }
        }
        catch (Exception ex)
        {
            sendMessageErrorResponse = ex.ToString();
        }
        finally
        {
            if (null != postStream)
            {
                postStream.Close();
            }
        }
    }

    private MemoryStream JoinTwoByteArrays(byte[] firstByteArray, byte[] secondByteArray)
    {
        int newSize = firstByteArray.Length + secondByteArray.Length;
        var totalMemoryStream = new MemoryStream(new byte[newSize], 0, newSize, true, true);
        totalMemoryStream.Write(firstByteArray, 0, firstByteArray.Length);
        totalMemoryStream.Write(secondByteArray, 0, secondByteArray.Length);
        return totalMemoryStream;
    }

    private void GetMessageContentByIDnPartNumber(string accTok, string endP, string messId, string partNum)
    {
        try
        {
            HttpWebRequest mimRequestObject1 = (HttpWebRequest)WebRequest.Create(string.Empty + endP + "/myMessages/v2/messages/" + messId + "/parts/" + partNum);
            mimRequestObject1.Headers.Add("Authorization", "Bearer " + accTok);
            mimRequestObject1.Method = "GET";
            mimRequestObject1.KeepAlive = true;
            int offset = 0;

            getContentResponseObject = mimRequestObject1.GetResponse();
            int remaining = Convert.ToInt32(getContentResponseObject.ContentLength);
            using (var stream = getContentResponseObject.GetResponseStream())
            {
                receivedBytes = new byte[getContentResponseObject.ContentLength];
                while (remaining > 0)
                {
                    int read = stream.Read(receivedBytes, offset, remaining);
                    if (read <= 0)
                    {
                        getMessageContentErrorResponse = String.Format("End of stream reached with {0} bytes left to read", remaining);
                        return;
                    }

                    remaining -= read;
                    offset += read;
                }

                getMessageContentSuccessResponse = "Success";
            }
        }
        catch (WebException we)
        {
            string errorResponse = string.Empty;
            try
            {
                using (StreamReader sr2 = new StreamReader(we.Response.GetResponseStream()))
                {
                    errorResponse = sr2.ReadToEnd();
                    sr2.Close();
                }
                getMessageContentErrorResponse = errorResponse;
            }
            catch
            {
                errorResponse = "Unable to get response";
                getMessageContentErrorResponse = errorResponse;
            }
        }
        catch (Exception ex)
        {
            getMessageContentErrorResponse = ex.Message;
            return;
        }

    }

    private void BypassCertificateError()
    {
        string bypassSSL = ConfigurationManager.AppSettings["IgnoreSSL"];

        if ((!string.IsNullOrEmpty(bypassSSL))
            && (string.Equals(bypassSSL, "true", StringComparison.OrdinalIgnoreCase)))
        {
            ServicePointManager.ServerCertificateValidationCallback +=
                delegate(object sender1, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
                {
                    return true;
                };
        }
    }

    protected void SetRequestSessionVariables()
    {
        Session["cs_rest_Address"] = Address.Text;
        Session["cs_rest_Message"] = message.Text;
        Session["cs_rest_Subject"] = subject.Text;
        Session["cs_rest_Group"] = groupCheckBox.Checked.ToString();
        Session["cs_rest_Attachments"] = attachment.Value;
        Session["cs_rest_GetHeadercount"] = "abc";
        Session["cs_rest_GetHeaderIndex"] = "abc";
        Session["cs_rest_GetMessageId"] = "";
        Session["cs_rest_GetMessagePart"] = "";
        if (notificationMms.Checked)
            Session["cs_rest_GetNotificationConnectionDetailsQueue"] = notificationMms.Value;
        else if (notificationText.Checked)
            Session["cs_rest_GetNotificationConnectionDetailsQueue"] = notificationText.Value;
        Session["cs_rest_deleteMessageId"] = deleteMessageId.Text;
        Session["cs_rest_updateMessageId"] = updateMessageId.Text;
    }

    protected void ResetRequestSessionVariables()
    {
        Session["cs_rest_Address"] = null;
        Session["cs_rest_Message"] = null;
        Session["cs_rest_Subject"] = null;
        Session["cs_rest_Group"] = null;
        Session["cs_rest_Attachments"] = null;
        Session["cs_rest_GetHeadercount"] = null;
        Session["cs_rest_GetHeaderIndex"] = null;
        Session["cs_rest_GetMessageId"] = null;
        Session["cs_rest_GetMessagePart"] = null;
        Session["cs_rest_GetNotificationConnectionDetailsQueue"] = null;
        Session["cs_rest_deleteMessageId"] = null;
        Session["cs_rest_updateMessageId"] = null;
    }

    protected void RestoreRequestSessionVariables()
    {
        Address.Text = Session["cs_rest_Address"].ToString();
        message.Text = Session["cs_rest_Message"].ToString(); ;
        subject.Text = Session["cs_rest_Subject"].ToString();
        groupCheckBox.Checked = Convert.ToBoolean(Session["cs_rest_Group"].ToString());
        attachment.Value = Session["cs_rest_Attachments"].ToString();
        //headerCountTextBox.Value = Session["cs_rest_GetHeadercount"].ToString();
        //indexCursorTextBox.Value = Session["cs_rest_GetHeaderIndex"].ToString();
        MessageId.Text = Session["cs_rest_GetMessageId"].ToString();
        //PartNumber.Value = Session["cs_rest_GetMessagePart"].ToString();
        if (string.Compare(Session["cs_rest_GetNotificationConnectionDetailsQueue"].ToString(), notificationMms.Value) == 0)
        {
            notificationMms.Checked = true;
        }
        else if (string.Compare(Session["cs_rest_GetNotificationConnectionDetailsQueue"].ToString(), notificationText.Value) == 0)
        {
            notificationText.Checked = true;
        }
        deleteMessageId.Text = Session["cs_rest_deleteMessageId"].ToString();
        updateMessageId.Text = Session["cs_rest_updateMessageId"].ToString();
    }

    #region updateMessageRoutines

    protected void updateMessage_Click(object sender, EventArgs e)
    {
        showUpdateMessage = "true";
        this.ReadTokenSessionVariables();

        string tokentResult = this.IsTokenValid();

        if (tokentResult.CompareTo("INVALID_ACCESS_TOKEN") == 0)
        {
            SetRequestSessionVariables();
            Session["cs_rest_ServiceRequest"] = "updatemessage";
            Session["cs_rest_appState"] = "GetToken";
            this.GetAuthCode();
        }
        else if (tokentResult.CompareTo("REFRESH_TOKEN") == 0)
        {
            if (this.GetAccessToken(AccessTokenType.Refresh_Token) == false)
            {
                updateMessageErrorResponse = "Failed to get Access token";
                this.ResetTokenSessionVariables();
                this.ResetTokenVariables();
                return;
            }
        }

        if (this.accessToken == null || this.accessToken.Length <= 0)
        {
            return;
        }

        this.updateMessage(this.accessToken, this.endPoint, updateMessageId.Text, read.Checked);
    }

    private void updateMessage()
    {
        showUpdateMessage = "show";
        this.updateMessage(this.accessToken, this.endPoint, updateMessageId.Text, read.Checked);
    }

    private void updateMessage(string accTok, string endP, string updateMessages, bool read)
    {
        try
        {
            string contextURL = string.Empty;
            string messagesJSON = string.Empty;
            Stream dataStream;
            Message message;
            MessagesList messageList;
            List<Message> messages = new List<Message>();
            JavaScriptSerializer serializeJsonObject;
            contextURL = string.Empty + endP + "/myMessages/v2/messages";
            string[] messageIds = updateMessages.Split(',');
            foreach (String messageId in messageIds)
            {
                message = new Message();
                message.isUnread = Convert.ToBoolean(read);
                message.messageId = messageId;
                messages.Add(message);
            }
            messageList = new MessagesList();
            messageList.messages = messages;
            serializeJsonObject = new JavaScriptSerializer();
            messagesJSON = serializeJsonObject.Serialize(messageList);
            UTF8Encoding encoding = new UTF8Encoding();
            byte[] msgBytes = encoding.GetBytes(messagesJSON);
            HttpWebRequest updateMessageWebRequest = (HttpWebRequest)WebRequest.Create(contextURL);
            updateMessageWebRequest.Headers.Add("Authorization", "Bearer " + accTok);
            updateMessageWebRequest.Method = "PUT";
            updateMessageWebRequest.KeepAlive = true;
            updateMessageWebRequest.ContentType = "application/json";
            dataStream = updateMessageWebRequest.GetRequestStream();
            dataStream.Write(msgBytes, 0, msgBytes.Length);
            dataStream.Close();

            WebResponse deleteMessageWebResponse = updateMessageWebRequest.GetResponse();
            using (var stream = deleteMessageWebResponse.GetResponseStream())
            {
                updateMessageSuccessResponse = "Success";
            }
        }
        catch (WebException we)
        {
            string errorResponse = string.Empty;
            try
            {
                using (StreamReader sr2 = new StreamReader(we.Response.GetResponseStream()))
                {
                    errorResponse = sr2.ReadToEnd();
                    sr2.Close();
                }
                updateMessageErrorResponse = updateMessages + "@" + read + "@" + errorResponse;
            }
            catch
            {
                errorResponse = "Unable to get response";
                updateMessageErrorResponse = errorResponse;
            }
        }
        catch (Exception ex)
        {
            updateMessageErrorResponse = read + "@" + ex.Message;
            return;
        }

    }
    #endregion


    #region deleteMessageRoutines

    protected void deleteMessage_Click(object sender, EventArgs e)
    {
        showDeleteMessage = "true";
        this.ReadTokenSessionVariables();

        string tokentResult = this.IsTokenValid();

        if (tokentResult.CompareTo("INVALID_ACCESS_TOKEN") == 0)
        {
            SetRequestSessionVariables();
            Session["cs_rest_ServiceRequest"] = "deletemessage";
            Session["cs_rest_appState"] = "GetToken";
            this.GetAuthCode();
        }
        else if (tokentResult.CompareTo("REFRESH_TOKEN") == 0)
        {
            if (this.GetAccessToken(AccessTokenType.Refresh_Token) == false)
            {
                deleteMessageErrorResponse = "Failed to get Access token";
                this.ResetTokenSessionVariables();
                this.ResetTokenVariables();
                return;
            }
        }

        if (this.accessToken == null || this.accessToken.Length <= 0)
        {
            return;
        }
        this.deleteMessage(this.accessToken, this.endPoint, deleteMessageId.Text);
    }

    private void deleteMessage()
    {
        showDeleteMessage = "show";
        this.deleteMessage(this.accessToken, this.endPoint, deleteMessageId.Text);
    }

    private void deleteMessage(string accTok, string endP, string deleteMessages)
    {
        try
        {
            string contextURL = string.Empty;
            if (!deleteMessages.Contains(","))
                contextURL = string.Empty + endP + "/myMessages/v2/messages/" + deleteMessages;
            else
                contextURL = string.Empty + endP + "/myMessages/v2/messages?messageIds=" + System.Web.HttpUtility.UrlEncode(deleteMessages);
            HttpWebRequest deleteMessageWebRequest = (HttpWebRequest)WebRequest.Create(contextURL);
            deleteMessageWebRequest.Headers.Add("Authorization", "Bearer " + accTok);
            deleteMessageWebRequest.Method = "DELETE";
            deleteMessageWebRequest.KeepAlive = true;
            WebResponse deleteMessageWebResponse = deleteMessageWebRequest.GetResponse();
            using (var stream = deleteMessageWebResponse.GetResponseStream())
            {
                deleteMessageSuccessResponse = "Success";
            }
        }
        catch (WebException we)
        {
            string errorResponse = string.Empty;
            try
            {
                using (StreamReader sr2 = new StreamReader(we.Response.GetResponseStream()))
                {
                    errorResponse = sr2.ReadToEnd();
                    sr2.Close();
                }
                deleteMessageErrorResponse = errorResponse;
            }
            catch
            {
                errorResponse = "Unable to get response";
                deleteMessageErrorResponse = errorResponse;
            }
        }
        catch (Exception ex)
        {
            deleteMessageErrorResponse = ex.Message;
            return;
        }

    }
    #endregion

    #region getMessageListRoutines

    protected void getMessageList_Click(object sender, EventArgs e)
    {
        showGetMessage = "true";
        this.ReadTokenSessionVariables();

        string tokentResult = this.IsTokenValid();

        if (tokentResult.CompareTo("INVALID_ACCESS_TOKEN") == 0)
        {
            SetRequestSessionVariables();
            Session["cs_rest_ServiceRequest"] = "getmessagelist";
            Session["cs_rest_appState"] = "GetToken";
            this.GetAuthCode();
        }
        else if (tokentResult.CompareTo("REFRESH_TOKEN") == 0)
        {
            if (this.GetAccessToken(AccessTokenType.Refresh_Token) == false)
            {
                deleteMessageErrorResponse = "Failed to get Access token";
                this.ResetTokenSessionVariables();
                this.ResetTokenVariables();
                return;
            }
        }

        if (this.accessToken == null || this.accessToken.Length <= 0)
        {
            return;
        }
        this.filters = "";
        if (CheckBox1.Checked == true)
        {
            if (this.filters.CompareTo(string.Empty) == 0)
                this.filters = "isFavorite=true&";
            else
                this.filters = this.filters + "&isFavorite=true" +"&";
        }
        if (CheckBox2.Checked == true)
        {
            if (this.filters.CompareTo(string.Empty) == 0)
                this.filters = "isUnread=true&";
            else
                this.filters = this.filters + "&isUnread=true" + "&";
        }
        if (CheckBox3.Checked == true)
        {
            if (this.filters.CompareTo(string.Empty) == 0)
                this.filters = "isIncoming=true" + "&";
            else
                this.filters = this.filters + "&isIncoming=true" + "&";
        }
        if (!string.IsNullOrEmpty(FilterKeyword.Text))
        {
            if (this.filters.CompareTo(string.Empty) == 0)
                this.filters = "keyword=" + FilterKeyword.Text.ToString() + "&";
            else
                this.filters = this.filters + "&keyword=" + FilterKeyword.Text.ToString() + "&";
        }
        this.getMessageList(this.accessToken, this.endPoint, this.filters);
    }

    private void getMessageList()
    {
        showDeleteMessage = "show";
        this.getMessageList(this.accessToken, this.endPoint, this.filters);
    }

    private void getMessageList(string accTok, string endP, string filters)
    {
        try
        {
            string contextURL = string.Empty;
            contextURL = string.Empty + endP + "/myMessages/v2/messages?" + filters + "limit=5&offset=1";

            HttpWebRequest getMessageListWebRequest = (HttpWebRequest)WebRequest.Create(contextURL);
            getMessageListWebRequest.Headers.Add("Authorization", "Bearer " + accTok);
            getMessageListWebRequest.Method = "GET";
            getMessageListWebRequest.KeepAlive = true;
            WebResponse getMessageListWebResponse = getMessageListWebRequest.GetResponse();
            using (var stream = getMessageListWebResponse.GetResponseStream())
            {
                StreamReader sr = new StreamReader(stream);
                string csGetMessageListDetailsData = sr.ReadToEnd();

                JavaScriptSerializer deserializeJsonObject = new JavaScriptSerializer();
                csGetMessageListDetails deserializedJsonObj = (csGetMessageListDetails)deserializeJsonObject.Deserialize(csGetMessageListDetailsData, typeof(csGetMessageListDetails));

                if (null != deserializedJsonObj)
                {
                    getMessageListSuccessResponse = "Success";
                    csGetMessageListDetailsResponse = deserializedJsonObj.messageList;
                }
                else
                {
                    getMessageListErrorResponse = "No response from server";
                }

                sr.Close();

            }
        }
        catch (WebException we)
        {
            string errorResponse = string.Empty;
            try
            {
                using (StreamReader sr2 = new StreamReader(we.Response.GetResponseStream()))
                {
                    errorResponse = sr2.ReadToEnd();
                    sr2.Close();
                }
                getMessageListErrorResponse = errorResponse;
            }
            catch
            {
                errorResponse = "Unable to get response";
                getMessageListErrorResponse = errorResponse;
            }
        }
        catch (Exception ex)
        {
            getMessageListErrorResponse = ex.Message;
            return;
        }

    }
    #endregion

    #region getMessageRoutines

    protected void getMessage_Click(object sender, EventArgs e)
    {
        showGetMessage = "true";
        this.ReadTokenSessionVariables();

        string tokentResult = this.IsTokenValid();

        if (tokentResult.CompareTo("INVALID_ACCESS_TOKEN") == 0)
        {
            SetRequestSessionVariables();
            Session["cs_rest_ServiceRequest"] = "getmessage";
            Session["cs_rest_appState"] = "GetToken";
            this.GetAuthCode();
        }
        else if (tokentResult.CompareTo("REFRESH_TOKEN") == 0)
        {
            if (this.GetAccessToken(AccessTokenType.Refresh_Token) == false)
            {
                getMessageErrorResponse = "Failed to get Access token";
                this.ResetTokenSessionVariables();
                this.ResetTokenVariables();
                return;
            }
        }

        if (this.accessToken == null || this.accessToken.Length <= 0)
        {
            return;
        }
        this.getMessage(this.accessToken, this.endPoint, MessageId.Text);
    }

    private void getMessage()
    {
        showGetMessage = "show";
        this.getMessage(this.accessToken, this.endPoint, MessageId.Text);
    }

    private void getMessage(string accTok, string endP, string getMessage1)
    {
        try
        {
            string contextURL = string.Empty;
            contextURL = string.Empty + endP + "/myMessages/v2/messages/" + getMessage1;

            HttpWebRequest getMessageWebRequest = (HttpWebRequest)WebRequest.Create(contextURL);
            getMessageWebRequest.Headers.Add("Authorization", "Bearer " + accTok);
            getMessageWebRequest.Method = "GET";
            getMessageWebRequest.KeepAlive = true;
            getMessageWebRequest.Accept = "application/json";
            WebResponse getMessageWebResponse = getMessageWebRequest.GetResponse();
            using (StreamReader stream = new StreamReader(getMessageWebResponse.GetResponseStream()))
            {
                string getMessageData = stream.ReadToEnd();
                JavaScriptSerializer deserializeJsonObject = new JavaScriptSerializer();
                csGetMessageDetails deserializedJsonObj = (csGetMessageDetails)deserializeJsonObject.Deserialize(getMessageData, typeof(csGetMessageDetails));

                if (null != deserializedJsonObj)
                {
                    getMessageSuccessResponse = getMessageData + ":Success";
                    getMessageDetailsResponse = deserializedJsonObj.message;
                }
                else
                {
                    getMessageErrorResponse = "No response from server";
                }

                stream.Close();
            }
        }
        catch (WebException we)
        {
            string errorResponse = string.Empty;
            try
            {
                using (StreamReader sr2 = new StreamReader(we.Response.GetResponseStream()))
                {
                    errorResponse = sr2.ReadToEnd();
                    sr2.Close();
                }
                getMessageErrorResponse = errorResponse;
            }
            catch
            {
                errorResponse = "Unable to get response";
                getMessageErrorResponse = errorResponse;
            }
        }
        catch (Exception ex)
        {
            getMessageErrorResponse = ex.Message;
            return;
        }

    }
    #endregion

    #region getDeltaRoutines

    protected void getDelta_Click(object sender, EventArgs e)
    {
        showGetMessage = "true";
        this.ReadTokenSessionVariables();

        string tokentResult = this.IsTokenValid();

        if (tokentResult.CompareTo("INVALID_ACCESS_TOKEN") == 0)
        {
            SetRequestSessionVariables();
            Session["cs_rest_ServiceRequest"] = "deltamessage";
            Session["cs_rest_appState"] = "GetToken";
            this.GetAuthCode();
        }
        else if (tokentResult.CompareTo("REFRESH_TOKEN") == 0)
        {
            if (this.GetAccessToken(AccessTokenType.Refresh_Token) == false)
            {
                deltaErrorResponse = "Failed to get Access token";
                this.ResetTokenSessionVariables();
                this.ResetTokenVariables();
                return;
            }
        }

        if (this.accessToken == null || this.accessToken.Length <= 0)
        {
            return;
        }
        this.getDelta(this.accessToken, this.endPoint, MessageIdForDelta.Text);
    }

    private void getDelta()
    {
        //showDeltaMessage = "show";
        this.getDelta(this.accessToken, this.endPoint, MessageIdForDelta.Text);
    }

    private void getDelta(string accTok, string endP, string delta)
    {
        try
        {
            string contextURL = string.Empty;
            contextURL = string.Empty + endP + "/myMessages/v2/delta?state=" + delta;
            HttpWebRequest deltaWebRequest = (HttpWebRequest)WebRequest.Create(contextURL);
            deltaWebRequest.Headers.Add("Authorization", "Bearer " + accTok);
            deltaWebRequest.Method = "GET";
            deltaWebRequest.KeepAlive = true;
            WebResponse deltaWebResponse = deltaWebRequest.GetResponse();
            using (var stream = deltaWebResponse.GetResponseStream())
            {
                StreamReader sr = new StreamReader(stream);
                string deltaMessageData = sr.ReadToEnd();
                JavaScriptSerializer deserializeJsonObject = new JavaScriptSerializer();
                csGetDeltaDetails deserializedJsonObj = (csGetDeltaDetails)deserializeJsonObject.Deserialize(deltaMessageData, typeof(csGetDeltaDetails));
                if (null != deserializedJsonObj)
                {
                    deltaSuccessResponse = deltaMessageData + ":Success";
                    csDeltaResponse = deserializedJsonObj.deltaResponse;
                }
                else
                {
                    deltaErrorResponse = "No response from server";
                }

                stream.Close();
            }
        }
        catch (WebException we)
        {
            string errorResponse = string.Empty;
            try
            {
                using (StreamReader sr2 = new StreamReader(we.Response.GetResponseStream()))
                {
                    errorResponse = sr2.ReadToEnd();
                    sr2.Close();
                }
                deltaErrorResponse = errorResponse;
            }
            catch
            {
                errorResponse = "Unable to get response";
                deltaErrorResponse = errorResponse;
            }
        }
        catch (Exception ex)
        {
            deltaErrorResponse = ex.Message;
            return;
        }

    }
    #endregion

    #region getMessageIndexRoutines

    protected void getMessageIndex_Click(object sender, EventArgs e)
    {
        showGetMessage = "true";
        this.ReadTokenSessionVariables();

        string tokentResult = this.IsTokenValid();

        if (tokentResult.CompareTo("INVALID_ACCESS_TOKEN") == 0)
        {
            SetRequestSessionVariables();
            Session["cs_rest_ServiceRequest"] = "messageindex";
            Session["cs_rest_appState"] = "GetToken";
            this.GetAuthCode();
        }
        else if (tokentResult.CompareTo("REFRESH_TOKEN") == 0)
        {
            if (this.GetAccessToken(AccessTokenType.Refresh_Token) == false)
            {
                messageIndexErrorResponse = "Failed to get Access token";
                this.ResetTokenSessionVariables();
                this.ResetTokenVariables();
                return;
            }
        }

        if (this.accessToken == null || this.accessToken.Length <= 0)
        {
            return;
        }
        this.getMessageIndex(this.accessToken, this.endPoint);
    }

    private void getMessageIndex()
    {
        //showDeltaMessage = "show";
        this.getMessageIndex(this.accessToken, this.endPoint);
    }

    private void getMessageIndex(string accTok, string endP)
    {
        try
        {
            string contextURL = string.Empty;
            contextURL = string.Empty + endP + "/myMessages/v2/messages/index/info";
            HttpWebRequest msgIndxWebRequest = (HttpWebRequest)WebRequest.Create(contextURL);
            msgIndxWebRequest.Headers.Add("Authorization", "Bearer " + accTok);
            msgIndxWebRequest.Method = "GET";
            msgIndxWebRequest.KeepAlive = true;
            WebResponse msgIndxWebResponse = msgIndxWebRequest.GetResponse();
            using (StreamReader stream = new StreamReader(msgIndxWebResponse.GetResponseStream()))
            {
                string getMessageIndexData = stream.ReadToEnd();
                JavaScriptSerializer deserializeJsonObject = new JavaScriptSerializer();
                csMessageIndexInfo deserializedJsonObj = (csMessageIndexInfo)deserializeJsonObject.Deserialize(getMessageIndexData, typeof(csMessageIndexInfo));
                if (null != deserializedJsonObj)
                {

                    messageIndexSuccessResponse = getMessageIndexData + ":Success";
                    getMessageIndexInfoResponse = deserializedJsonObj.messageIndexInfo;
                }
                else
                {
                    messageIndexErrorResponse = "No response from server";
                }

                stream.Close();

            }
            messageIndexSuccessResponse = ":Success";
        }
        catch (WebException we)
        {
            string errorResponse = string.Empty;
            try
            {
                using (StreamReader sr2 = new StreamReader(we.Response.GetResponseStream()))
                {
                    errorResponse = sr2.ReadToEnd();
                    sr2.Close();
                }
                messageIndexErrorResponse = errorResponse;
            }
            catch
            {
                errorResponse = "Unable to get response";
                messageIndexErrorResponse = errorResponse;
            }
        }
        catch (Exception ex)
        {
            messageIndexErrorResponse = ex.Message;
            return;
        }

    }
    #endregion

    #region getNotificationConnectionDetailsRoutines

    protected void getNotificationConnectionDetails_Click(object sender, EventArgs e)
    {
        showGetNotificationConnectionDetails = "true";
        this.ReadTokenSessionVariables();

        string tokentResult = this.IsTokenValid();

        if (tokentResult.CompareTo("INVALID_ACCESS_TOKEN") == 0)
        {
            SetRequestSessionVariables();
            Session["cs_rest_ServiceRequest"] = "getnotificationconnectiondetails";
            Session["cs_rest_appState"] = "GetToken";
            this.GetAuthCode();
        }
        else if (tokentResult.CompareTo("REFRESH_TOKEN") == 0)
        {
            if (this.GetAccessToken(AccessTokenType.Refresh_Token) == false)
            {
                getNotificationConnectionDetailsErrorResponse = "Failed to get Access token";
                this.ResetTokenSessionVariables();
                this.ResetTokenVariables();
                return;
            }
        }

        if (this.accessToken == null || this.accessToken.Length <= 0)
        {
            return;
        }
        string queueType = string.Empty;
        if (notificationMms.Checked)
            queueType = notificationMms.Value;
        else if (notificationText.Checked)
            queueType = notificationText.Value;
        this.getNotificationConnectionDetails(this.accessToken, this.endPoint, queueType);
    }

    protected void getNotificationConnectionDetails()
    {
        showGetNotificationConnectionDetails = "show";
        string queueType = string.Empty;
        if (notificationMms.Checked)
            queueType = notificationMms.Value;
        else if (notificationText.Checked)
            queueType = notificationText.Value;
        this.getNotificationConnectionDetails(this.accessToken, this.endPoint, queueType);
    }

    private void getNotificationConnectionDetails(string accTok, string endP, string queues)
    {
        try
        {
            HttpWebRequest getNotificationConnectionDetailsWebRequest = (HttpWebRequest)WebRequest.Create(string.Empty + endP + "/myMessages/v2/notificationConnectionDetails?queues=" + queues);
            getNotificationConnectionDetailsWebRequest.Headers.Add("Authorization", "Bearer " + accTok);
            getNotificationConnectionDetailsWebRequest.Method = "GET";
            getNotificationConnectionDetailsWebRequest.KeepAlive = true;
            getNotificationConnectionDetailsWebRequest.Accept = "application/json";
            WebResponse getNotificationConnectionDetailsWebResponse = getNotificationConnectionDetailsWebRequest.GetResponse();
            using (StreamReader sr = new StreamReader(getNotificationConnectionDetailsWebResponse.GetResponseStream()))
            {
                string getNotificationConnectionDetailsData = sr.ReadToEnd();

                JavaScriptSerializer deserializeJsonObject = new JavaScriptSerializer();
                csGetNotificationConnectionDetails deserializedJsonObj = (csGetNotificationConnectionDetails)deserializeJsonObject.Deserialize(getNotificationConnectionDetailsData, typeof(csGetNotificationConnectionDetails));

                if (null != deserializedJsonObj)
                {
                    getNotificationConnectionDetailsSuccessResponse = "SUCCESS";
                    getNotificationConnectionDetailsResponse = deserializedJsonObj.notificationConnectionDetails;
                }
                else
                {
                    getNotificationConnectionDetailsErrorResponse = "No response from server";
                }

                sr.Close();
            }
        }
        catch (WebException we)
        {
            string errorResponse = string.Empty;
            try
            {
                using (StreamReader sr2 = new StreamReader(we.Response.GetResponseStream()))
                {
                    errorResponse = sr2.ReadToEnd();
                    sr2.Close();
                }
                getNotificationConnectionDetailsErrorResponse = errorResponse;
            }
            catch
            {
                errorResponse = "Unable to get response";
                getNotificationConnectionDetailsErrorResponse = errorResponse;
            }
        }
        catch (Exception ex)
        {
            getNotificationConnectionDetailsErrorResponse = ex.Message;
            return;
        }

    }
    #endregion

    #region createMessageIndexRoutines

    protected void createMessageIndex_Click(object sender, EventArgs e)
    {
        showCreateMessageIndex = "true";
        this.ReadTokenSessionVariables();

        string tokentResult = this.IsTokenValid();

        if (tokentResult.CompareTo("INVALID_ACCESS_TOKEN") == 0)
        {
            SetRequestSessionVariables();
            Session["cs_rest_ServiceRequest"] = "createmessageindex";
            Session["cs_rest_appState"] = "GetToken";
            this.GetAuthCode();
        }
        else if (tokentResult.CompareTo("REFRESH_TOKEN") == 0)
        {
            if (this.GetAccessToken(AccessTokenType.Refresh_Token) == false)
            {
                createMessageIndexErrorResponse = "Failed to get Access token";
                this.ResetTokenSessionVariables();
                this.ResetTokenVariables();
                return;
            }
        }

        if (this.accessToken == null || this.accessToken.Length <= 0)
        {
            return;
        }
        this.createMessageIndex(this.accessToken, this.endPoint);
    }

    private void createMessageIndex()
    {
        showCreateMessageIndex = "show";
        this.createMessageIndex(this.accessToken, this.endPoint);
    }

    private void createMessageIndex(string accTok, string endP)
    {
        try
        {
            HttpWebRequest createMessageIndexWebRequest = (HttpWebRequest)WebRequest.Create(string.Empty + endP + "/myMessages/v2/messages/index");
            createMessageIndexWebRequest.Headers.Add("Authorization", "Bearer " + accTok);
            createMessageIndexWebRequest.Method = "POST";
            createMessageIndexWebRequest.KeepAlive = true;
            UTF8Encoding encoding = new UTF8Encoding();
            byte[] postBytes = encoding.GetBytes("");
            //createMessageIndexWebRequest.ContentLength = postBytes.Length;
            Stream postStream = createMessageIndexWebRequest.GetRequestStream();
            postStream.Write(postBytes, 0, postBytes.Length);
            postStream.Close();
            WebResponse createMessageIndexWebResponse = createMessageIndexWebRequest.GetResponse();
            using (var stream = createMessageIndexWebResponse.GetResponseStream())
            {
                createMessageIndexSuccessResponse = "Success";
            }
        }
        catch (WebException we)
        {
            string errorResponse = string.Empty;
            try
            {
                using (StreamReader sr2 = new StreamReader(we.Response.GetResponseStream()))
                {
                    errorResponse = sr2.ReadToEnd();
                    sr2.Close();
                }
                createMessageIndexErrorResponse = errorResponse;
            }
            catch
            {
                errorResponse = "Unable to get response";
                createMessageIndexErrorResponse = errorResponse;
            }
        }
        catch (Exception ex)
        {
            createMessageIndexErrorResponse = ex.Message;
            return;
        }

    }
    #endregion

    protected void Button1_Click(object sender, EventArgs e)
    {
        showSendMsg = "true";
        this.ReadTokenSessionVariables();

        string tokentResult = this.IsTokenValid();

        if (tokentResult.CompareTo("INVALID_ACCESS_TOKEN") == 0)
        {
            SetRequestSessionVariables();
            Session["cs_rest_ServiceRequest"] = "sendmessasge";
            Session["cs_rest_appState"] = "GetToken";
            this.GetAuthCode();
        }
        else if (tokentResult.CompareTo("REFRESH_TOKEN") == 0)
        {
            if (this.GetAccessToken(AccessTokenType.Refresh_Token) == false)
            {
                sendMessageErrorResponse = "Failed to get Access token";
                this.ResetTokenSessionVariables();
                this.ResetTokenVariables();
                return;
            }
        }

        if (this.accessToken == null || this.accessToken.Length <= 0)
        {
            return;
        }
        this.SendMessageRequest();
    }

    protected void SendMessageRequest()
    {
        this.IsValidAddress();
        string attachFile = this.AttachmentFilesDir + attachment.Value.ToString();
        ArrayList attachmentList = new ArrayList();
        if (string.Compare(attachment.Value.ToString().ToLower(), "none") != 0)
            attachmentList.Add(attachFile);
        string accessToken = this.accessToken;
        string endpoint = this.endPoint;
        this.SendMessageRequest(accessToken, endpoint, subject.Text, message.Text, groupCheckBox.Checked.ToString().ToLower(),
                                      attachmentList);
    }

    protected void GetMessageContentByIDnPartNumber(object sender, EventArgs e)
    {
        showGetMessage = "true";
        this.ReadTokenSessionVariables();

        string tokentResult = this.IsTokenValid();

        if (tokentResult.CompareTo("INVALID_ACCESS_TOKEN") == 0)
        {
            SetRequestSessionVariables();
            Session["cs_rest_ServiceRequest"] = "getmessagecontent";
            Session["cs_rest_appState"] = "GetToken";
            this.GetAuthCode();
        }
        else if (tokentResult.CompareTo("REFRESH_TOKEN") == 0)
        {
            if (this.GetAccessToken(AccessTokenType.Refresh_Token) == false)
            {
                sendMessageErrorResponse = "Failed to get Access token";
                this.ResetTokenSessionVariables();
                this.ResetTokenVariables();
                return;
            }
        }

        if (this.accessToken == null || this.accessToken.Length <= 0)
        {
            return;
        }

        GetMessageContentByIDnPartNumber(this.accessToken, this.endPoint, MessageIdForContent.Text, PartNumberForContent.Text);
    }

    protected void GetMessageContentByIDnPartNumber()
    {
        this.GetMessageContentByIDnPartNumber(this.accessToken, this.endPoint, MessageIdForContent.Text, PartNumberForContent.Text);
    }
    /// <summary>
    /// Validates the given addresses based on following conditions
    /// 1. Group messages should not allow short codes
    /// 2. Short codes should be 3-8 digits in length
    /// 3. Valid Email Address
    /// 4. Group message must contain more than one address
    /// 5. Valid Phone number
    /// </summary>
    /// <returns>true/false; true - if address specified met the validation criteria, else false</returns>
    private bool IsValidAddress()
    {
        string phonenumbers = string.Empty;

        bool isValid = true;
        if (string.IsNullOrEmpty(Address.Text))
        {
            sendMessageErrorResponse = "Address field cannot be blank.";
            return false;
        }

        string[] addresses = Address.Text.Trim().Split(',');

        if (addresses.Length > this.maxAddresses)
        {
            sendMessageErrorResponse = "Message cannot be delivered to more than 10 receipients.";
            return false;
        }

        if (groupCheckBox.Checked && addresses.Length < 1)
        {
            sendMessageErrorResponse = "Specify more than one address for Group message.";
            return false;
        }

        foreach (string addressraw in addresses)
        {
            string address = addressraw.Trim();
            if (string.IsNullOrEmpty(address))
            {
                break;
            }

            if (address.Length < 3)
            {
                sendMessageErrorResponse = "Invalid address specified.";
                return false;
            }

            // Verify if short codes are present in address
            if (!address.StartsWith("short") && (address.Length > 2 && address.Length < 9))
            {
                if (groupCheckBox.Checked)
                {
                    sendMessageErrorResponse = "Group Message with short codes is not allowed.";
                    return false;
                }

                this.addressList.Add(address);
                this.phoneNumbersParameter = this.phoneNumbersParameter + "addresses=short:" + Server.UrlEncode(address.ToString()) + "&";
            }

            if (address.StartsWith("short"))
            {
                if (groupCheckBox.Checked)
                {
                    sendMessageErrorResponse = "Group Message with short codes is not allowed.";
                    return false;
                }

                System.Text.RegularExpressions.Regex regex = new Regex("^[0-9]*$");
                if (!regex.IsMatch(address.Substring(6)))
                {
                    sendMessageErrorResponse = "Invalid short code specified.";
                    return false;
                }

                this.addressList.Add(address);
                this.phoneNumbersParameter = this.phoneNumbersParameter + "addresses=" + Server.UrlEncode(address.ToString()) + "&";
            }
            else if (address.Contains("@"))
            {
                isValid = this.IsValidEmail(address);
                if (isValid == false)
                {
                    sendMessageErrorResponse = "Specified Email Address is invalid.";
                    return false;
                }
                else
                {
                    this.addressList.Add(address);
                    this.phoneNumbersParameter = this.phoneNumbersParameter + "addresses=" + Server.UrlEncode(address.ToString()) + "&";
                }
            }
            else
            {
                if (this.IsValidMISDN(address) == true)
                {
                    if (address.StartsWith("tel:"))
                    {
                        phonenumbers = address.Replace("-", string.Empty);
                        this.phoneNumbersParameter = this.phoneNumbersParameter + "addresses=" + Server.UrlEncode(phonenumbers.ToString()) + "&";
                    }
                    else
                    {
                        phonenumbers = address.Replace("-", string.Empty);
                        this.phoneNumbersParameter = this.phoneNumbersParameter + "addresses=" + Server.UrlEncode("tel:" + phonenumbers.ToString()) + "&";
                    }

                    this.addressList.Add(address);
                }
            }
        }

        return true;
    }

    private bool IsValidMISDN(string number)
    {
        string smsAddressInput = number;
        long tryParseResult = 0;
        string smsAddressFormatted;
        string phoneStringPattern = "^\\d{3}-\\d{3}-\\d{4}$";
        if (Regex.IsMatch(smsAddressInput, phoneStringPattern))
        {
            smsAddressFormatted = smsAddressInput.Replace("-", string.Empty);
        }
        else
        {
            smsAddressFormatted = smsAddressInput;
        }

        if (smsAddressFormatted.Length == 16 && smsAddressFormatted.StartsWith("tel:+1"))
        {
            smsAddressFormatted = smsAddressFormatted.Substring(6, 10);
        }
        else if (smsAddressFormatted.Length == 15 && smsAddressFormatted.StartsWith("tel:1"))
        {
            smsAddressFormatted = smsAddressFormatted.Substring(5, 10);
        }
        else if (smsAddressFormatted.Length == 14 && smsAddressFormatted.StartsWith("tel:"))
        {
            smsAddressFormatted = smsAddressFormatted.Substring(4, 10);
        }
        else if (smsAddressFormatted.Length == 12 && smsAddressFormatted.StartsWith("+1"))
        {
            smsAddressFormatted = smsAddressFormatted.Substring(2, 10);
        }
        else if (smsAddressFormatted.Length == 11 && smsAddressFormatted.StartsWith("1"))
        {
            smsAddressFormatted = smsAddressFormatted.Substring(1, 10);
        }

        if ((smsAddressFormatted.Length != 10) || (!long.TryParse(smsAddressFormatted, out tryParseResult)))
        {
            return false;
        }

        return true;
    }

    private bool IsValidEmail(string emailID)
    {
        string strRegex = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
              @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
              @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
        Regex re = new Regex(strRegex);
        if (re.IsMatch(emailID))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool IsNumber(string address)
    {
        bool isValid = false;
        Regex regex = new Regex("^[0-9]*$");
        if (regex.IsMatch(address))
        {
            isValid = true;
        }

        return isValid;
    }
    #region Data Structures

    public class AccessTokenResponse
    {
        public string access_token
        {
            get;
            set;
        }
        public string refresh_token
        {
            get;
            set;
        }
        public string expires_in
        {
            get;
            set;
        }
    }

    public class MessageObject
    {
        public Message message { get; set; }
    }
    public class MessagesList
    {
        public List<Message> messages
        {
            get;
            set;
        }
    }

    public class MsgResponseId
    {
        public string Id { get; set; }
    }
    public enum AccessTokenType
    {
        Authorization_Code,
        Refresh_Token
    }
    #endregion

    #region GetNotificationConnectionDetailsDataTypes

    public class csGetNotificationConnectionDetails
    {
        public csNotificationConnectionDetails notificationConnectionDetails { get; set; }
    }

    public class csNotificationConnectionDetails
    {
        public string username { get; set; }
        public string password { get; set; }
        public string httpsUrl { get; set; }
        public string wssUrl { get; set; }
        public csqueues queues { get; set; }
    }

    public class csqueues
    {
        public string text { get; set; }
        public string mms { get; set; }
    }
    #endregion

    #region GetMessageContentDetails

    public class csGetMessageContentDetails
    {
        public csMessageContentDetails MessageContentDetails { get; set; }
    }

    public class csMessageContentDetails
    {
        public string contenttype { get; set; }
        public string textplain { get; set; }
        public string imgjpg { get; set; }
    }
    #endregion

    #region GetMessageDetails

    public class csGetMessageDetails
    {
        public Message message { get; set; }
    }

    public class csGetMessageListDetails
    {
        public MessageList messageList { get; set; }
    }

    public class From
    {
        public string value { get; set; }
    }
    public class Recipient
    {
        public string value { get; set; }
    }
    public class SegmentationDetails
    {

        public int segmentationMsgRefNumber { get; set; }

        public int totalNumberOfParts { get; set; }

        public int thisPartNumber { get; set; }

    }
    public class TypeMetaData
    {
        public bool isSegmented { get; set; }
        public SegmentationDetails segmentationDetails { get; set; }
        public string subject { get; set; }
    }
    public class MmsContent
    {
        public string contentType { get; set; }
        public string contentName { get; set; }
        public string contentUrl { get; set; }
        public string type { get; set; }
    }
    public class Message
    {
        public string messageId { get; set; }
        public From from { get; set; }
        public List<Recipient> recipients { get; set; }
        public string timeStamp { get; set; }
        public Boolean isFavorite { get; set; }
        public Boolean isUnread { get; set; }
        public string type { get; set; }
        public TypeMetaData typeMetaData { get; set; }
        public string isIncoming { get; set; }
        public List<MmsContent> mmsContent { get; set; }
        public string text { get; set; }
        public string subject { get; set; }
    }

    public class MessageList
    {
        public List<Message> messages { get; set; }
        public int offset { get; set; }
        public int limit { get; set; }
        public int total { get; set; }
        public string state { get; set; }
        public string cacheStatus { get; set; }
        public List<string> failedMessages { get; set; }
    }
    #endregion

    #region GetDeltaDetails

    public class csGetDeltaDetails
    {
        public DeltaResponse deltaResponse { get; set; }
    }

    public class Delta
    {
        public List<Message> adds { get; set; }
        public List<Message> deletes { get; set; }
        public string type { get; set; }
        public List<Message> updates { get; set; }
    }

    public class DeltaResponse
    {
        public string state { get; set; }
        public List<Delta> delta { get; set; }
    }
    #endregion
    #region GetMessageIndexInfo

    public class MessageIndexInfo
    {
        public string status { get; set; }
        public string state { get; set; }
        public int messageCount { get; set; }
    }
    public class csMessageIndexInfo
    {
        public MessageIndexInfo messageIndexInfo { get; set; }
    }
    #endregion

}
