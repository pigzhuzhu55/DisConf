using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clyconf.Net.Core.Zookeeper
{
    public interface IZkTreeBuilder
    {
        /// <summary>
        /// 根据配置名称获取对应的Zookeeper下的节点名称，如果无法找到对应节点则添加并返回对应节点名称
        /// </summary>
        /// <param name="nodePath">节点路径</param>
        /// <param name="configValue">节点值</param>
        /// <returns></returns>
        string GetOrAddZnodeName(string nodePath,string configValue);

        /// <summary>
        /// 获取配置节点的值
        /// </summary>
        /// <param name="nodePath"></param>
        /// <returns></returns>
        string GetConfigValue(string nodePath);
        /// <summary>
        /// 设置配置节点的值
        /// </summary>
        /// <param name="nodePath"></param>
        /// <param name="configValue"></param>
        /// <returns></returns>
        void SetConfigValue(string nodePath, string configValue);
        /// <summary>
        /// 获取所有已配置的znodeName
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetAllZnodes();
    }
}
