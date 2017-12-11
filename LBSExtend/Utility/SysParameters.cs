using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace ZIT.LBSExtend.Utility
{
    public class SysParameters
    {
        static SysParameters()
        {
            SharkHandsInterval = 10;

            SoftName = ConfigurationManager.AppSettings["SoftName"];

            BSSServerIP = ConfigurationManager.AppSettings["BSSServerIP"];
            BSSServerPort = short.Parse(ConfigurationManager.AppSettings["BSSServerPort"]);
            BSSLocalPort = short.Parse(ConfigurationManager.AppSettings["BSSLocalPort"]);

            InsertInterval = short.Parse(ConfigurationManager.AppSettings["InsertInterval"]);
            LocalUnitCode = ConfigurationManager.AppSettings["LocalUnitCode"];

            LBSUrl = ConfigurationManager.AppSettings["LBSUrl"];

        }
        /// <summary>
        /// 软件的名称
        /// </summary>
        public static string SoftName;
        /// <summary>
        /// 与各服务器握手时间间隔，单位：秒
        /// </summary>
        public static int InsertInterval;// = 10
        /// 120业务服务器IP地址
        /// </summary>
        public static string BSSServerIP;// = "192.168.0.254";
        /// <summary>
        /// 与120业务服务器连接的本地端口
        /// </summary>
        public static short BSSLocalPort;// = 2000;

        /// <summary>
        /// 120业务服务器监听端口
        /// </summary>
        public static short BSSServerPort;// = 1003;
        /// <summary>
        /// 本地单位编号
        /// </summary>
        public static string LocalUnitCode;// = "000000";
        /// <summary>
        /// 与各服务器握手时间间隔，单位：秒
        /// </summary>
        public static int SharkHandsInterval;// = 5

        public static string LBSUrl { get; set; }
    }
}
