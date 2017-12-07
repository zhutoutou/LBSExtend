using System;
using ZIT.LBSExtend.Utility;
using ZIT.LBSExtend.Controller.BusinessServer;

namespace ZIT.LBSExtend.Controller
{
    /// <summary>
    /// 核心服务类
    /// </summary>
    public class CoreService
    {
        /// <summary>
        /// 确保CoreService只有一个实例。
        /// </summary>
        private static CoreService instance = null;

        /// <summary>
        /// 与120业务服务器连接状态改变事件
        /// </summary>
        public event EventHandler<StatusEventArgs> BServerConnectionStatusChanged;
        /// <summary>
        /// 数据库连接状态变化事件
        /// </summary>
        public event EventHandler<StatusEventArgs> DBLConnectStatusChanged;


        public event EventHandler<StatusEventArgs> DBRConnectStatusChanged;

        public BSSServer bs;


        /// <summary>
        /// 获取当前类实例
        /// </summary>
        /// <returns></returns>
        public static CoreService GetInstance()
        {
            if (null == instance)
            {
                instance = new CoreService();
            }
            return instance;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        private CoreService()
        {
            bs = new BSSServer();
            bs.strRemoteIP = SysParameters.BSSServerIP;
            bs.nRemotePort = SysParameters.BSSServerPort;
            bs.nLocalPort = SysParameters.BSSLocalPort;
        }

        /// <summary>
        /// 开始数据交换服务
        /// </summary>
        public void StartService()
        {
            try
            {
                //UDP连接120业务服务器
                bs.ConnectionStatusChanged += BusnessServer_StatusChanged;
                bs.Start();
            }
            catch (Exception ex) { LogUtility.DataLog.WriteLog(LogUtility.LogLevel.Info,ex.Message,new LogUtility.RunningPlace("CoreService","StartService"),"业务异常"); }        
        }

        /// <summary>
        /// 停止数据交换服务
        /// </summary>
        public void StopService()
        {
            bs.Stop();
        }

        private void BusnessServer_StatusChanged(object sender, StatusEventArgs e)
        {
            OnBServerConnectionStatusChanged(e.Status);
        }
        private void DBLConnect_StatusChanged(object sender, StatusEventArgs e)
        {
            OnDBLConnectStatusChanged(e.Status);
        }

        private void DBRConnect_StatusChanged(object sender, StatusEventArgs e)
        {
            OnDBRConnectStatusChanged(e.Status);
        }
        /// <summary>
        /// Raises BServerConnectionStatusChanged event.
        /// </summary>
        /// <param name="message">Received message</param>
        protected virtual void OnBServerConnectionStatusChanged(NetStatus status)
        {
            var handler = BServerConnectionStatusChanged;
            if (handler != null)
            {
                handler(this, new StatusEventArgs(status));
            }
        }

        /// <summary>
        /// Raises DBConnectStatusChanged event.
        /// </summary>
        /// <param name="message">Received message</param>
        protected virtual void OnDBLConnectStatusChanged(NetStatus status)
        {
            var handler = DBLConnectStatusChanged;
            if (handler != null)
            {
                handler(this, new StatusEventArgs(status));
            }
        }

        protected virtual void OnDBRConnectStatusChanged(NetStatus status)
        {
            var handler = DBRConnectStatusChanged;
            if (handler != null)
            {
                handler(this, new StatusEventArgs(status));
            }
        }
 
    }
}
