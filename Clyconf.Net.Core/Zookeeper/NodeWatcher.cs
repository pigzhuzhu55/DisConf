using Clyconf.Net.Core.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZooKeeperNet;

namespace Clyconf.Net.Core.Zookeeper
{
    /// <summary>
    /// 这个类只负责监控znode的变化
    /// </summary>
    public class NodeWatcher : ConnectWatcher
    {
        #region fileds
        IZkTreeBuilder _builder;
        /// <summary>
        /// znode发生变化时的回调事件，arg1对应nodepath
        /// </summary>
        public event Action<string> NodeChanged;
        #endregion

        public NodeWatcher(string connectionString, int timeOut, IZkTreeBuilder builder)
            : base(connectionString, timeOut)
        {
            this._builder = builder;
            this.RegisterWatcher();
        }
        protected override void ReConnectCallBack()
        {
            var configs = this.RegisterWatcher();
            if (this.NodeChanged != null)
            {
                //Expired之后变更的节点需要补调通知
                foreach (var config in configs)
                {
                    if (!string.IsNullOrWhiteSpace(config))
                    {
                        this.NodeChanged(config);
                    }
                }
            }
        }
        /// <summary>
        /// 注册监控关系
        /// </summary>
        public IEnumerable<string> RegisterWatcher()
        {
            var configs = new HashSet<string>();
            if (this._builder != null)
            {
                foreach (var node in this._builder.GetAllZnodes())
                {
                    try
                    {
                        var stat = this.ZooKeeper.Exists(node, true);
                        if (stat != null)
                        {
                            configs.Add(node);
                        }
                    }
                    catch (Exception ex)
                    {
                        //TODO:可能需要判断Expired
                    }
                }
            }
            return configs;
        }
        public override void Process(WatchedEvent @event)
        {
            base.Process(@event);
            switch (@event.Type)
            {
                case EventType.NodeDataChanged:
                    var path = @event.Path;
                    if (this.NodeChanged != null && !string.IsNullOrWhiteSpace(path))
                    {
                        this.NodeChanged(path);
                        try
                        {
                            //重新注册监控
                            var stat = this.ZooKeeper.Exists(path, true);
                        }
                        catch (Exception ex)
                        {
                            //TODO:可能需要判断Expired
                        }
                    }
                    break;
            }
        }

        public string GetConfigValue(string nodePath)
        {
            return _builder.GetConfigValue(nodePath);
        }
        public void RefreshConfigValue(string nodePath,string configValue)
        {
            _builder.SetConfigValue(nodePath, configValue);
        }
    }
}
