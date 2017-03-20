using System;
using System.Collections.Generic;

namespace WeedCSharpClient.Status
{
    public class Layout
    {
        public string collection { get; set; }
        public string replication { get; set; }
        public int ttl { get; set; }
        public List<int> Writables { get; set; }

        public ReplicationStrategy GetReplicationStrategy()
        {
            ReplicationStrategy replication;
            Enum.TryParse(this.replication, out replication);

            return replication;
        }
    }
}
