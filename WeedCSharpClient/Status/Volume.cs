using System;

namespace WeedCSharpClient.Status
{
    public class Volume
    {
        public int Id { get; set; }
        public long Size { get; set; }
        public long ttl { get; set; }
        public ReplicaPlacementEntity ReplicaPlacement { get; set; }
        public string Collection { get; set; }
        public string Version { get; set; }
        public long FileCount { get; set; }
        public long DeleteCount { get; set; }
        public long DeletedByteCount { get; set; }
        public bool ReadOnly { get; set; }

        public ReplicationStrategy GetReplicationStrategy()
        {
            ReplicationStrategy replication;
            Enum.TryParse(ReplicaPlacement.replicationStrategy(), out replication);

            return replication;
        }
    }
}
