using System;
using System.Collections.Generic;
using ZIT.LBSExtend.Utility;
using ZIT.LBSExtend.Model;
using System.Text.RegularExpressions;
using System.Runtime.Remoting.Messaging;
using System.Runtime.CompilerServices;

namespace ZIT.LBSExtend.Controller.BusinessServer
{
    public class BSSServerMsgHandler
    {
        public delegate string LBSRequestHandler(LBSRequest lbr);

        private static string _TH="";

        public BSSServerMsgHandler()
        {
        }

        public void HandleMsg(string strMsg)
        {
            try
            {
                int m;
                string strMessageId = strMsg.Substring(1, 4);
                if (strMessageId != "3000" && int.TryParse(strMessageId,out m))
                {
                    LogUtility.DataLog.WriteLog(LogUtility.LogLevel.Info, "Recieve BSSServer message:" + strMsg, new LogUtility.RunningPlace("BSSServerMsgHandler", "HandleMsg"), "RecieveServer信息");
                }
                switch (strMessageId)
                { 
                    case "6201":
                        Handle6201Message(strMsg);
                        break;
                    default:
                    break;
                }
            }
            catch (Exception ex)
            {
                LogUtility.DataLog.WriteLog(LogUtility.LogLevel.Info, ex.Message, new LogUtility.RunningPlace("BSSServerMsgHandler", "HandleMsg"), "业务异常");
            }
        }

        public void Handle6201Message(string strMsg)
        {
            try
            {
                string strLSH = GetValueByKey(strMsg, "LSH");
                _TH = GetValueByKey(strMsg, "TH");
                string strSJ = GetValueByKey(strMsg, "SJ");
                if (string.IsNullOrEmpty(strLSH) || string.IsNullOrEmpty(strSJ))
                {
                    LogUtility.DataLog.WriteLog(LogUtility.LogLevel.Info, "定位请求信息的流水号或者电话号码为空，格式不正确。", new LogUtility.RunningPlace("BSSServerMsgHandler", "Handle6201Message"), "业务运行信息");
                }
                LogUtility.DataLog.WriteLog(LogUtility.LogLevel.Info, "定位请求开始，request：" + strMsg, new LogUtility.RunningPlace("BSSServerMsgHandler", "Handle6201Message"), "业务运行信息");
                LBSRequest lbr = new LBSRequest() { LSH = strLSH, Phone = strSJ, Name = "" };
                if (SysParameters.InvokeMothed == "Direct")
                {
                    LBSRequestHandler handler = new LBSRequestHandler(LBSRequestDir);
                    IAsyncResult result = handler.BeginInvoke(lbr, new AsyncCallback(NoticeLBSResponseDir), null);
                }
                else
                {
                    LBSRequestHandler handler = new LBSRequestHandler(LBSRequestInDir);
                    IAsyncResult result = handler.BeginInvoke(lbr, new AsyncCallback(NoticeLBSResponseInDir), null);
                }
            }
            catch (Exception ex)
            {
                LogUtility.DataLog.WriteLog(LogUtility.LogLevel.Info, ex.Message, new LogUtility.RunningPlace("BSSServerMsgHandler", "Handle6201Message"), "业务异常");
            }
        }



        

        public string LBSRequestDir(LBSRequest lbr)
        {
            string response = "";
            try
            {
                LOCATEREQUEST loc = new LOCATEREQUEST() {
                    SEQ = lbr.LSH,
                    ACCOUNT = SysParameters.ACCOUNT,
                    PASSWORD = SysParameters.PASSWORD,
                    MOBILE =lbr.Phone,
                    CITYNUMBER = SysParameters.CITYNUMBER,
                    AREANUMBER = SysParameters.AREANUMBER,
                };
                string request = XMLHelper.Serializer<LOCATEREQUEST>(loc);
                response = WebServiceHelper.InvokeWebService(SysParameters.LBSUrl, "location", new object[] { request }).ToString();
                LogUtility.DataLog.WriteLog(LogUtility.LogLevel.Info, "定位请求返回，response：" + response, new LogUtility.RunningPlace("BSSServerMsgHandler", "LBSRequest"), "业务运行信息");

            }
            catch (Exception ex)
            {
                LogUtility.DataLog.WriteLog(LogUtility.LogLevel.Info, ex.Message, new LogUtility.RunningPlace("BSSServerMsgHandler", "LBSRequest"), "业务异常");
            }
            return response;
        }
        private void NoticeLBSResponseDir(IAsyncResult result)
        {
            try
            {
                LBSRequestHandler handler = (LBSRequestHandler)((AsyncResult)result).AsyncDelegate;
                string reponse = handler.EndInvoke(result);
                RESPONSE lbr = new RESPONSE();
                string strTH = _TH;
                lbr = (RESPONSE)JSON.JsonToObject(reponse, lbr);
                if (lbr != null)
                {
                    string msg = "[6202LSH:"+ lbr.SEQ +"*#SJ:" + lbr.REQ_PHONE + "*#TLX:LBS*#TH:" + strTH + "*#WD:" + lbr.LAT + "*#JD:" + lbr.LNG + "*#ERRCODE:" + lbr.RESULTCODE + "*#ERRMSG:" + lbr.RESULTDESC + "*#]";
                    CoreService.GetInstance().bs.SendMessage(msg);
                }
            }
            catch (Exception ex)
            {

            }
        }

        public string LBSRequestInDir(LBSRequest lbr)
        {
            string response = "";
            try
            {
                string request = JSON.ObjectToJson(lbr);
                response = WebServiceHelper.InvokeWebService(SysParameters.LBSUrl, "RequestLocation", new object[] { request }).ToString();
                LogUtility.DataLog.WriteLog(LogUtility.LogLevel.Info, "定位请求返回，response：" + response, new LogUtility.RunningPlace("BSSServerMsgHandler", "LBSRequest"), "业务运行信息");

            }
            catch (Exception ex)
            {
                LogUtility.DataLog.WriteLog(LogUtility.LogLevel.Info, ex.Message, new LogUtility.RunningPlace("BSSServerMsgHandler", "LBSRequest"), "业务异常");
            }
            return response;
        }

        private void NoticeLBSResponseInDir(IAsyncResult result)
        {
            try
            {
                LBSRequestHandler handler = (LBSRequestHandler)((AsyncResult)result).AsyncDelegate;
                string  reponse = handler.EndInvoke(result);
                LBSResponse lbr = new LBSResponse();
                string strTH =_TH;
                lbr = (LBSResponse)JSON.JsonToObject(reponse, lbr);
                if (lbr != null)
                {
                    string msg = "[6202LSH:" + lbr.LSH + "*#SJ:" + lbr.Phone + "*#TLX:LBS*#TH:" + strTH + "*#WD:" + lbr.WD + "*#JD:" + lbr.JD + "*#ERRCODE:" + lbr.ErrorCode + "*#ERRMSG:" + lbr.ErrorMsg + "*#]";
                    CoreService.GetInstance().bs.SendMessage(msg);
                }
            }
            catch (Exception ex)
            {

            }
        }
        private string GetValueByKey(string message, string key)
        {
            string strReturn = "";

            Regex reg = new Regex(key + ":(.*?)\\*#");
            if (reg.IsMatch(message))
            {
                Match match = reg.Match(message);
                strReturn = match.Groups[1].Value.Trim();
            }
            return strReturn;
        }
    }

}
