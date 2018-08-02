C# DisConf

统一配置管理DEMO

准备环境：

基于zookeeper上的实现，首先需要一台服务器安装zookeeper组件，具体安装方法自行搜索

1、统一配置管理的web界面目前还没写，暂时用ConsoleApp1这个项目替代，呵呵，偷懒了。

项目文件增加配置节：  

<configSections>

    <section name="zookeepConfig" type="Clyconf.Net.Core.ZookeepConfigSection, Clyconf.Net.Core"/>

  </configSections>

  <zookeepConfig host="192.168.88.129:2181" >

  </zookeepConfig>

启动时调用：Clyconf.Net.Core.ConfigManager.Instance.InitServer()，同步本地磁盘文件配置信息，并填充至zookeeper服务。

所有站点的配置，都放在本地磁盘文件里，后面每次更新磁盘文件信息，同步到zookeeper。

2、业务平台站点 Website1，webconfig 配置如下

  <configSections>

    <section name="zookeepConfig" type="Clyconf.Net.Core.ZookeepConfigSection, Clyconf.Net.Core"/>

  </configSections>

  <zookeepConfig host="192.168.88.129:2181" >

    <clientInfo appName="1" clientName="Console_1" />

  </zookeepConfig>

在Global.asax.cs里，添加初始化脚本：Clyconf.Net.Core.ConfigManager.Instance.InitClient();

从zookeeper服务拉取已经配置的所有站点配置信息，并下载到本地磁盘文件。

调用的时候： Clyconf.Net.Core.ConfigManager.Instance.GetConfigValue("/1/dbcon");

表示，获取该节点的配置信息。

代码只实现主要部分，后续根据业务功能，需要整体完善。



PS:zookeeper实现部分参考网上其他资源，谢谢作者的分享，本资源也纯属个人学习。
