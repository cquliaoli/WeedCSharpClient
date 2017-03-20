using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeedCSharpClient.Net
{
    public class ReadResult
    {
        public Stream stream { get; set; }
        public string filename { get; set; }
    }
}
