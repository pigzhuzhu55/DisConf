using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clyconf.Net.Core.Model
{
    public class ZNode
    {
        /// <summary>
        /// 值
        /// </summary>
        [JsonProperty(PropertyName = "val")]
        public string Value { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        [JsonProperty(PropertyName = "desc")]
        public string Description { get; set; }

        /// <summary>
        /// 版本号
        /// </summary>
        [JsonProperty(PropertyName = "ver")]
        public int Version { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(this.Version.ToString());
            sb.AppendLine(this.Description.ToString());
            sb.AppendLine(this.Value.ToString());

            return sb.ToString();
        }
    }
}
