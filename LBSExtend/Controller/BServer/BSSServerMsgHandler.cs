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
        /// <summary>
        /// WebService代理类
        /// </summary>

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

        private void Handle6201Message(string strMsg)
        {
            string strLSH = GetValueByKey(strMsg, "LSH");
            _TH = GetValueByKey(strMsg, "TH");
            string strSJ = GetValueByKey(strMsg, "SJ");
            LBSRequest lbr = new LBSRequest() { LSH = strLSH, Phone = strSJ, Name = "" };
            LBSRequestHandler handler = new LBSRequestHandler(LBSRequest);
            IAsyncResult result = handler.BeginInvoke(lbr, new AsyncCallback(NoticeLBSResponse), null);
        }

        public delegate string LBSRequestHandler(LBSRequest lbr);

        public string LBSRequest(LBSRequest lbr)
        {
            string reponse ="";
            try
            {

            }
            catch (Exception ex)
            {
                LogUtility.DataLog.WriteLog(LogUtility.LogLevel.Info, ex.Message, new LogUtility.RunningPlace("BSSServerMsgHandler", "LBSRequest"), "业务异常");
            }
            return reponse;
        }

        private void NoticeLBSResponse(IAsyncResult result)
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
                    string msg = "[6202SJ:" + lbr.Phone + "*#TLX:LBS*#TH:" + strTH + "*#WD:" + lbr.WD + "*#JD:" + lbr.JD + "*#ERRCODE:" + lbr.ErrorCode + "*#ERRMSG:" + lbr.ErrorMsg + "*#]";
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
