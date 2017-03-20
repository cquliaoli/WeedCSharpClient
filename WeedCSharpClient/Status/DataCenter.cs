using System.Collections.Generic;

namespace WeedCSharpClient.Status
{
    public class DataCenter : AbstractNode
    {
        public List<Rack> Racks { get; set; }
        public string Id { get; set; }
    }
}
