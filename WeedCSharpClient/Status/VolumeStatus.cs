using System.Collections.Generic;

namespace WeedCSharpClient.Status
{
    public class VolumeStatus
    {
        public string Version { get; set; }
        public List<Volume> Volumes { get; set; }
    }
}
