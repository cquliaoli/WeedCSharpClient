using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeedCSharpClient.Status
{
    public class ReplicaPlacementEntity
    {
        public int SameRackCount { get; set; }
        public int DiffRackCount { get; set; }
        public int DiffDataCenterCount { get; set; }
        public string replicationStrategy()
        {
            return string.Format("{0}{1}{2}", SameRackCount, DiffRackCount, DiffDataCenterCount);
        }
    }
}
