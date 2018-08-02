using Cly.Common.Extensions;
using Cly.Common.Util;
using Clyconf.Net.Core.Model;
using Clyconf.Net.Core.Zookeeper;
using Org.Apache.Zookeeper.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZooKeeperNet;

namespace Clyconf.Net.Core
{
    /// <summary>
    /// 配置类
    /// </summary>
    public class ConfigManager
    {
        public static readonly ConfigManager Instance = new ConfigManager();

        private ZookeepConfigSection config = ZookeepConfigSection.Current;

        /// <summary>
        /// 更新异常时调用事件进行通知
        /// </summary>
        public event Action<Exception> Faulted;

        private ExceptionHandler _handler;

        private string RootPath;

        private string ClientPath;

        private NodeWatcher _itemWatcher;

        private ConfigManager()
        {
            this._handler = new ExceptionHandler();
            this._handler.Faulted += _handler_Faulted;

            RootPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "zookeeper");
            ClientPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "zconfig");
        }
        private void _handler_Faulted(string arg1, Exception arg2)
        {
            if (this.Faulted != null)
            {
                if (!string.IsNullOrWhiteSpace(arg1))
                {
                    arg2 = new Exception(arg1, arg2);
                }
                this.Faulted(arg2);
            }
        }

        /// <summary>
        /// 统一配置服务器站点的初始化
        /// </summary>
        public void InitServer()
        {
            var task = Task.Run(() =>
            {
                this._handler.Execute(() => {
                    //目前没考虑自定义配置 ，这里配置文件统一放置到zookeeper文件夹下
                    //同步zookeeper文件信息到zookeeper服务

                    if(!System.IO.Directory.Exists(RootPath))
                    {
                        System.IO.Directory.CreateDirectory(RootPath);
                    }

                    //第一级目录，我设想存放的是站点名称，我就存放站点对应的Key好了,类似1、2、3、4，你也可以自行扩展
                    var dirs = System.IO.Directory.GetDirectories(RootPath);

                    foreach(string dir in dirs)
                    {
                        string name = Path.GetFileName(dir);
                        using (MaintainWatcher mk = new MaintainWatcher(config.Host, 1000))
                        {
                            ZooKeeper zk = mk.ZooKeeper;
                            var stat = zk.Exists($"/{name}", false);
                            if(stat==null)
                            {
                                zk.Create($"/{name}", "".GetBytes(), Ids.OPEN_ACL_UNSAFE, CreateMode.Persistent);
                            }

                            var files = Directory.GetFiles(dir,"*.txt");

                            foreach (string file in files)
                            {
                                string key = Path.GetFileNameWithoutExtension(file);
                                stat = zk.Exists($"/{name}/{key}", false);
                                string text = FileHelper.ReadFile(file);

                                ZNode znode = Deserialize(text);

                                if (znode != null)
                                {
                                    if (stat == null)
                                    {
                                        zk.Create($"/{name}/{key}", znode.ToString().GetBytes(), Ids.OPEN_ACL_UNSAFE, CreateMode.Persistent);
                                    }
                                    else
                                    {
                                        //文件的版本号比znode大，这样才去更新zookeeper服务器
                                        //if (znode.Version > stat.Version)
                                        {
                                            //下面没有事物处理，可能会有问题
                                            var tmpdata = zk.GetData($"/{name}/{key}", false, null);
                                            string tmpString = System.Text.Encoding.UTF8.GetString(tmpdata);
                                            stat = zk.SetData($"/{name}/{key}", znode.ToString().GetBytes(), -1);
                                            znode.Version = stat.Version;
                                            FileHelper.WriteFile(file, znode.ToString());
                                        }
                                    }
                                }
                            }
                        }
                    }

                }, string.Empty);
            });
            task.Wait();
        }

        /// <summary>
        /// 同步zookeeper服务并序列化成配置文件到本地
        /// 初始化
        /// </summary>
        public void InitClient()
        {

            var task = Task.Run(() =>
            {
                this._handler.Execute(() => {

                    StringBuilder logBuilder = new StringBuilder();

                    #region 同步配置到本地
                    if (!Directory.Exists(ClientPath))
                    {
                        Directory.CreateDirectory(ClientPath);
                    }

                    logBuilder.AppendLine("同步zookeeper数据到本地\r\n");
                    using (MaintainWatcher mk = new MaintainWatcher(config.Host, 1000))
                    {
                        ZooKeeper zk = mk.ZooKeeper;
                        //本地站点的名称
                        string name = config.ClientInfo.AppName;
                        var stat = zk.Exists($"/{name}", false);
                        if (stat == null)
                        {
                            return;
                        }
                        if (!System.IO.Directory.Exists(Path.Combine(ClientPath, name)))
                        {
                            System.IO.Directory.CreateDirectory(Path.Combine(ClientPath, name));
                        }

                        var children = zk.GetChildren($"/{name}", false);//取不到节点会报错
                        foreach (var child in children)
                        {
                            stat = new Stat();
                            var tmpdata = zk.GetData($"/{name}/{child}", false, stat);
                            string tmpString = System.Text.Encoding.UTF8.GetString(tmpdata);
                            ZNode znode = Deserialize(tmpString);
                            if (znode == null)
                                continue;
                            znode.Version = stat.Version;

                            //看下本地的配置文件版本号，需要不需要更新
                            string file = Path.Combine(ClientPath, name, child + ".txt");
                            string text = FileHelper.ReadFile(file);
                            ZNode zLocal = Deserialize(text);

                            if (zLocal == null || zLocal.Version < znode.Version)
                            {
                                FileHelper.WriteFile(file, znode.ToString());
                                logBuilder.AppendLine($"下载节点:{$"/{name}/{child}"},值：{JsonHelper.ToNewtonJsonString(znode)}");
                            }
                        }
                    }
                    #endregion

                    #region 加载配置到内存
                    var files = Directory.GetFiles(Path.Combine(ClientPath, config.ClientInfo.AppName), "*.txt");
                    IZkTreeBuilder itemBuilder = new ZkTreeBuilder(config.ClientInfo.AppName);
                    foreach (string file in files)
                    {
                        string key = Path.GetFileNameWithoutExtension(file);
                        string text = FileHelper.ReadFile(file);
                        ZNode znode = Deserialize(text);
                        if(znode == null)
                        {
                            continue;
                        }
                        itemBuilder.GetOrAddZnodeName($"/{config.ClientInfo.AppName}/{key}", znode.Value);
                    }
                    this._itemWatcher = new NodeWatcher(config.Host, 30000, itemBuilder);
                    this._itemWatcher.NodeChanged += _itemWatcher_NodeChanged;
                    #endregion

                    LogHelper.WriteCustom(logBuilder.ToString(), "zookeeper\\",false);

                }, string.Empty);
            });
            task.Wait();

        }

        private void _itemWatcher_NodeChanged(string obj)
        {
            this._handler.Execute(() =>
            {
                using (MaintainWatcher mk = new MaintainWatcher(config.Host, 1000))
                {
                    ZooKeeper zk = mk.ZooKeeper;
                    Stat stat=new Stat();
                    var tmpdata = zk.GetData(obj, false, stat);
                    string tmpString = System.Text.Encoding.UTF8.GetString(tmpdata);

                    ZNode znode = Deserialize(tmpString);
                    if (znode == null)
                        return;
                    znode.Version = stat.Version;

                    //获取本地的版本号，如果比服务器的低，则更新
                    string file = Path.Combine(ClientPath, obj.TrimStart('/') + ".txt");
                    string text = FileHelper.ReadFile(file);
                    ZNode zLocal = Deserialize(text);

                    if (zLocal == null || zLocal.Version < znode.Version)
                    {
                        FileHelper.WriteFile(file, znode.ToString());
                        LogHelper.WriteCustom($"更新节点:{$"{obj}"},值：{JsonHelper.ToNewtonJsonString(znode)}", "zookeeper\\",false);
                    }
                    //刷新内存值
                    this._itemWatcher.RefreshConfigValue(obj, znode.Value);
                }

            }, string.Format("Some thing is wrong with item '{0}'", obj));
        }

        /// <summary>
        /// 获取配置节点的值
        /// </summary>
        /// <param name="nodePath"></param>
        /// <returns></returns>
        public string GetConfigValue(string nodePath)
        {
            if(_itemWatcher==null)
            {
                throw new Exception("请先执行InitClient方法后才能调用");
            }

            return _itemWatcher.GetConfigValue(nodePath);
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public  static ZNode Deserialize(string text)
        {
            ZNode znode = null;
            try
            {
                //第一行是版本号
                //第二行是描述
                //第三行以后才是真正的值

                string[] tmps = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                int cnt = tmps.Length;

                if(cnt<3)
                {
                    return null;
                }
                znode = new ZNode();
                znode.Version = tmps[0].ToInt32();
                znode.Description = tmps[1];

                StringBuilder sb = new StringBuilder();
                for(int i=2;i<cnt;i++)
                {
                    sb.AppendLine(tmps[i]);
                }
                znode.Value = sb.ToString().TrimEnd(new[] { '\r', '\n' });
            }
            catch
            {

            }
            return znode;
        }


    }


}
