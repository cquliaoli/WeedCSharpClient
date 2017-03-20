using System.Collections.Generic;

namespace WeedCSharpClient.Status
{
    public class Rack : AbstractNode
    {
        public List<DataNode> DataNodes { get; set; }
        public string Id { get; set; }
    }
}
