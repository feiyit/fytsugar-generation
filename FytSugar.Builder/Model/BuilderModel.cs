using System;
using System.Collections.Generic;

namespace FytSugar.Builder
{
    /// <summary>
    /// 连接对象
    /// </summary>
    public class BuilderConnection
    {
        public string Ip { get; set; }

        public string Port { get; set; }

        public string Name { get; set; }

        public string PassWord { get; set; }

        public string DbName { get; set; }
    }

    /// <summary>
    /// 生成的对象
    /// </summary>
    public class BuilderModel
    {
        /// <summary>
        /// 数据库表名字  例如：sys_admin
        /// </summary>
        public string[] TableNames { get; set; }

        /// <summary>
        /// 命名空间，根据不同的业务，分文件夹=命名空间
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// 控制器的名字
        /// </summary>
        public string ControllerName { get; set; }

        /// <summary>
        /// 生成类型 1=全部表   2=部分表
        /// </summary>
        public int Types { get; set; } = 1;

        public BuilderConnection connection { get; set; }
    }
}
