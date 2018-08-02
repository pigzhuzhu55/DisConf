using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace Clyconf.Net.Core
{
    public class ZookeepConfigSection: ConfigurationSection
    {
        /// <summary>
        /// zookeeper服务器ip以及端口
        /// </summary>
        [ConfigurationProperty("host", IsRequired = true)]
        public string Host
        {
            get { return this["host"].ToString(); }
            set { this["host"] = value; }
        }

        /// <summary>
        /// 业务系统信息设置
        /// </summary>
        [ConfigurationProperty("clientInfo", IsRequired = true)]
        public ClientInfoSection ClientInfo
        {
            get { return (ClientInfoSection)this["clientInfo"]; }
        }

        public class ClientInfoSection : ConfigurationElement
        {
            /// <summary>
            /// 客户端程序名称，注意大小写要与服务端一致
            /// </summary>
            [ConfigurationProperty("appName", IsRequired = true)]
            public string AppName
            {
                get { return this["appName"].ToString(); }
                set { this["appName"] = value; }
            }
            /// <summary>
            /// 客户端标识，用于服务端查看已更新客户端，如果不设置则默认获取客户端电脑名称
            /// </summary>
            [ConfigurationProperty("clientName", DefaultValue = null)]
            public string ClientName
            {
                get { return this["clientName"].ToString(); }
                set { this["clientName"] = value; }
            }
        }


        public static ZookeepConfigSection Current
        {
            get { return (ZookeepConfigSection)ConfigurationManager.GetSection("zookeepConfig"); }
        }
    }
}
