﻿using System.Collections.Generic;

namespace WeedCSharpClient.Status
{
    public class Topology : AbstractNode
    {
        public List<DataCenter> DataCenters { get; set; }
        public List<Layout> layouts { get; set; }
        private Stats _stats;

        public int GetDataCenterCount()
        {
            if (_stats == null)
            {
                ComputeStats();
                return 0;
            }

            return _stats.DcCount;
        }
        /// <summary>
        /// 得到所有卷信息
        /// </summary>
        /// <returns></returns>
        public List<Location>GetVolumesLocations()
        {
            List<Location> locations = new List<Location>();
            if (DataCenters == null)
            {
                return locations;
            }
            foreach (var dc in DataCenters)
            {
                if (dc.Racks != null)
                {
                    foreach (var rack in dc.Racks)
                    {
                        if(rack.DataNodes!=null)
                        {
                            foreach (var datanode in rack.DataNodes)
                            {
                                locations.Add(datanode.AsLocation());

                            }
                        }
                        
                    }
                }
            }
            return locations;
        }
        public int GetRackCount()
        {
            if (_stats == null)
            {
                ComputeStats();
                return 0;
            }

            return _stats.RackCount;
        }

        public int GetDataNodeCount()
        {
            if (_stats == null)
            {
                ComputeStats();
                return 0;
            }

            return _stats.NodeCount;
        }

        public List<DataNode> GetDataNodes()
        {
            if (_stats == null)
            {
                ComputeStats();
                return new List<DataNode>();
            }

            return _stats.NodeList;
        }

        private void ComputeStats()
        {
            if (DataCenters == null)
            {
                return;
            }

            _stats = new Stats();
            foreach (var dc in DataCenters)
            {
                _stats.DcCount += 1;

                if (dc.Racks != null)
                {
                    foreach (var rack in dc.Racks)
                    {
                        _stats.RackCount += 1;

                        if (rack.DataNodes != null)
                        {
                            _stats.NodeCount += rack.DataNodes.Count;
                            _stats.NodeList.AddRange(rack.DataNodes);
                        }  
                    }
                }                
            }
        }

        private class Stats
        {
            public int DcCount;
            public int RackCount;
            public int NodeCount;
            public List<DataNode> NodeList = new List<DataNode>();
        }
    }
}
