using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clyconf.Net.Core.Zookeeper
{
    public class ZkTreeBuilder: IZkTreeBuilder
    {
        /// <summary>
        /// 用于存储znodeName,Key为znodeName,Value为znodeValue
        /// </summary>
        protected ConcurrentDictionary<string, string> _dic = new ConcurrentDictionary<string, string>();
        /// <summary>
        /// 应用名
        /// </summary>
        public string AppName { get; private set; }

        public ZkTreeBuilder(string appName)
        {
            this.AppName = appName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodePath"></param>
        /// <returns></returns>
        public virtual string GetOrAddZnodeName(string nodePath, string configValue)
        {
            this._dic[nodePath] = configValue;
            return nodePath;
        }

        /// <summary>
        /// 获取配置节点的值
        /// </summary>
        /// <param name="nodePath"></param>
        /// <returns></returns>
        public string GetConfigValue(string nodePath)
        {
            string configValue;
            this._dic.TryGetValue(nodePath, out configValue);
            return configValue;
        }

        public void SetConfigValue(string nodePath, string configValue)
        {
            this._dic[nodePath] = configValue;
        }

        public IEnumerable<string> GetAllZnodes()
        {
            return this._dic.Keys;
        }
    }
}
