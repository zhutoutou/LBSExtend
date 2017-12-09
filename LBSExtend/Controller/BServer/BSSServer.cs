using System;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using ZIT.LBSExtend.Utility;

namespace ZIT.LBSExtend.Controller.BusinessServer
{
    public class BSSServer
    {
        /// <summary>
        /// 与120业务服务器连接状态改变事件
        /// </summary>
        public event EventHandler<StatusEventArgs> ConnectionStatusChanged; 
        
        private bool blConnected;

        /// <summary>
        /// 接收消息缓冲区
        /// </summary>
        private string strRecvMsg;

        /// <summary>
        /// 与120业务服务器通信
        /// </summary>
        public UdpClient udpClient;

        /// <summary>
        /// 消息队列互斥量
        /// </summary>
        private Mutex MsgMutex = new Mutex();
        /// <summary>
        /// 处理消息线程
        /// </summary>
        public Thread UdpThread = null;

        /// <summary>
        /// 120业务服务器IP地址
        /// </summary>
        public string strRemoteIP;
        /// <summary>
        /// 120业务服务器IP端口
        /// </summary>
        public short nRemotePort;
        /// <summary>
        /// 本地端口
        /// </summary>
        public short nLocalPort;


        private DateTime dtLastRecieveMsgTime;

        /// <summary>
        /// 消息处理类
        /// </summary>
        public BSSServerMsgHandler MsgHandler;

        /// <summary>
        /// 构造函数
        /// </summary>
        public BSSServer()
        {
            MsgHandler = new BSSServerMsgHandler();

            dtLastRecieveMsgTime = DateTime.MinValue;
            strRecvMsg = "";
            blConnected = false;
            
        }


        /// <summary>
        /// 启动
        /// </summary>
        public void Start()
        {
            try
            {
                StartLocalListener();

                //shake handle with bisniess server
                ThreadPool.QueueUserWorkItem(new WaitCallback(SharkHands_Thread), SysParameters.SharkHandsInterval);

                // handle recieved message
                ThreadPool.QueueUserWorkItem(new WaitCallback(HandleRecvMsg_Thread));

                //check business server connect status
                ThreadPool.QueueUserWorkItem(new WaitCallback(CheckConnectedStatus_Thread), SysParameters.InsertInterval);
               
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 停止
        /// </summary>
        public void Stop()
        {
            udpClient.Close();
        }

        /// <summary>
        /// 检测与业务服务器连接状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckConnectedStatus_Thread(object e)
        {
            int SharkHandsTime = int.Parse(e.ToString());
            int CheckConnectedInterval = 3;
            while (true)
            {
                Thread.Sleep(CheckConnectedInterval * 1000);
                try
                {
                    if (DateTime.Now.Subtract(dtLastRecieveMsgTime) > new TimeSpan(0, 0, 2 * SharkHandsTime + 1))
                    {
                        if (blConnected)
                        {
                            blConnected = false;
                            //raise disconnect event
                            OnConnectionStatusChanged(NetStatus.DisConnected);
                        }
                    }
                    else
                    {
                        if (!blConnected)
                        {
                            blConnected = true;
                            //raise connect event
                            OnConnectionStatusChanged(NetStatus.Connected);
                        }
                    }
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// 开启本地UDP监听
        /// </summary>
        public void StartLocalListener()
        {
            try
            {
                if (udpClient != null)
                {
                    UdpThread.Abort();
                    udpClient.Close();
                }
                udpClient = new UdpClient(nLocalPort);
                UdpThread = new Thread(new ThreadStart(UdpReciveThread));
                UdpThread.Start();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 接收数据线程
        /// </summary>
        private void UdpReciveThread()
        {
            IPEndPoint remoteHost = null;
            while (udpClient != null && Thread.CurrentThread.ThreadState.Equals(System.Threading.ThreadState.Running))
            {
                try
                {
                    byte[] buf = udpClient.Receive(ref remoteHost);
                    string bufs = Encoding.GetEncoding(936).GetString(buf);
                    
                    if (bufs.Trim() !=""  && !blConnected) 
                    {
                        blConnected=true;
                        OnConnectionStatusChanged(NetStatus.Connected);
                    }


                    dtLastRecieveMsgTime = DateTime.Now;
                    MsgMutex.WaitOne();
                    try
                    {
                        strRecvMsg += bufs.Trim();
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        MsgMutex.ReleaseMutex();
                    }
                }
                catch (SocketException ex)
                {
                    if (ex.ErrorCode != 10054)
                    {
                        LogUtility.DataLog.WriteLog(LogUtility.LogLevel.Info, "SocketException：" + ex.Message, new LogUtility.RunningPlace("BSSServer", "UdpReciveThread"), "业务异常");
                    }
                }
                catch (Exception ex)
                {
                    LogUtility.DataLog.WriteLog(LogUtility.LogLevel.Info, ex.Message, new LogUtility.RunningPlace("BSSServer", "UdpReciveThread"), "业务异常");
                }
            }
        }

        /// <summary>
        /// 处理数据线程
        /// </summary>
        /// <summary>
        /// 处理数据线程
        /// </summary>
        private void HandleRecvMsg_Thread(object e)
        {
            while (true)
            {
                Thread.Sleep(100);
                if (strRecvMsg == null || strRecvMsg == "") continue;

                string strOneMsg = "";

                try
                {
                    MsgMutex.WaitOne();
                    try
                    {
                        int StartIndex = strRecvMsg.IndexOf("[");
                        int EndIndex = strRecvMsg.IndexOf("]");

                        if (StartIndex >= 0 && EndIndex >= 1)
                        {
                            if (EndIndex > StartIndex)
                            {
                                strOneMsg = strRecvMsg.Substring(StartIndex, EndIndex + 1 - StartIndex);
                                strRecvMsg = strRecvMsg.Substring(EndIndex + 1);
                            }
                            else
                            {
                                //去掉错误的消息内容
                                strRecvMsg = strRecvMsg.Substring(EndIndex + 1);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogUtility.DataLog.WriteLog(LogUtility.LogLevel.Info, ex.Message, new LogUtility.RunningPlace("BSSServer", "HandleRecvMsg_Thread_In"), "业务异常");
                    }
                    finally
                    {
                        MsgMutex.ReleaseMutex();
                    }

                    if (strOneMsg != "")
                    {
                        MsgHandler.HandleMsg(strOneMsg);
                    }
                }
                catch (Exception ex)
                {
                    LogUtility.DataLog.WriteLog(LogUtility.LogLevel.Info, ex.Message, new LogUtility.RunningPlace("BSSServer", "HandleRecvMsg_Thread_Out"), "业务异常");

                }
            }
        }

        /// <summary>
        /// 与业务服务器握手线程
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SharkHands_Thread(object e)
        {
            int intSharkHandsInterval = int.Parse(e.ToString());
            while (true)
            {
                try
                {
                    ///发送握手消息
                    if (udpClient != null)
                    {
                        string str = "[3000DWBH:" + SysParameters.LocalUnitCode + "*#DWMC:*#ZJM:" + GetZJM() + "*#TLX:LBS*#TH:1*#ZBY:*#ZT:1*#LSH:*#ZBBC:*#]";
                        SendMessage(str,false);
                    }
                    else
                    {
                        udpClient = null;
                        udpClient.Close();
                        udpClient = new UdpClient(nLocalPort);
                        UdpThread = new Thread(new ThreadStart(UdpReciveThread));
                        UdpThread.Start();
                    }
                }
                catch (Exception ex)
                {
                    LogUtility.DataLog.WriteLog(LogUtility.LogLevel.Info, ex.Message, new LogUtility.RunningPlace("BSSServer", "SharkHands_Thread"), "业务异常");
                }
                finally
                {
                    Thread.Sleep(intSharkHandsInterval * 1000);
                }
            }

        }

        /// <summary>
        /// 退出消息
        /// </summary>
        public void SendExitMessage()
        {
            ///发送握手消息
            if (udpClient != null)
            {
                string str = "[3000DWBH:" + SysParameters.LocalUnitCode + "*#DWMC:*#ZJM:" + GetZJM() + "*#TLX:LBS*#TH:1*#ZBY:*#ZT:0*#LSH:*#ZBBC:*#]";
                Byte[] sendBytes = Encoding.ASCII.GetBytes(str);
                udpClient.Send(sendBytes, sendBytes.Length, strRemoteIP, nRemotePort);
            }
        }

        /// <summary>
        /// 发给消息给120业务服务器
        /// </summary>
        /// <param name="strMsg"></param>
        public void SendMessage(string strMsg,bool isLog = true)
        {
            try
            {
                if (udpClient != null)
                {
                    if (strMsg != "")
                    {
                        Byte[] sendBytes = Encoding.GetEncoding(936).GetBytes(strMsg);
                        udpClient.Send(sendBytes, sendBytes.Length, strRemoteIP, nRemotePort);
                        if (isLog)
                        {
                            LogUtility.DataLog.WriteLog(LogUtility.LogLevel.Info, "Send BSSServer message:" + strMsg, new LogUtility.RunningPlace("BSSServer", "SendMessage"), "SendServer信息");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogUtility.DataLog.WriteLog(LogUtility.LogLevel.Info, ex.Message, new LogUtility.RunningPlace("BSSServer", "SendMessage"), "业务异常");
            }
        }



        private void OnConnectionStatusChanged(NetStatus status)
        {
            var handler = ConnectionStatusChanged;
            if (handler != null)
            {
                handler(this, new StatusEventArgs(status));
            }
        }
        private string GetZJM()
        {
            string hostName = "";
            try
            {
                hostName = Dns.GetHostName();
            }
            catch (Exception ex)
            {
                LogUtility.DataLog.WriteLog(LogUtility.LogLevel.Info, ex.Message, new LogUtility.RunningPlace("BSSServer", "GetZJM"), "业务异常");
            }
            return hostName;
        }

    }
}
