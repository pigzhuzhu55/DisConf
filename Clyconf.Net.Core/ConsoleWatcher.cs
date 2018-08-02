using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZooKeeperNet;

namespace Clyconf.Net.Core
{
    public class ConsoleWatcher: IWatcher
    {
        public void Process(WatchedEvent @event)
        {
            switch(@event.Type)
            {
                case EventType.NodeCreated: {
                        Console.WriteLine($"增加节点：{@event.Path}");
                    } break;
                case EventType.NodeDataChanged:
                    {
                        Console.WriteLine($"更新节点：{@event.Path}");
                    }
                    break;
                case EventType.NodeChildrenChanged:
                    {
                        Console.WriteLine($"子节点变更：{@event.Path}");
                    }
                    break;
                case EventType.NodeDeleted:
                    {
                        Console.WriteLine($"节点删除：{@event.Path}");
                    }
                    break;
                case EventType.None:
                    {
                        
                    }
                    break;
            }
        }
    }
}
