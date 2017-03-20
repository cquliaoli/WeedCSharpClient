using System;
using System.Configuration;
using System.IO;
using static System.Console;

using WeedCSharpClient;
using WeedCSharpClient.Helper;
using WeedCSharpClient.Net;
using System.Threading.Tasks;
using WeedCSharpClient.Status;
using ServiceStack.Text;

namespace WeedCSharpClientTest
{
    class Program
    {
        private static readonly WeedCSharpClientProxy WeedProxy = new WeedCSharpClientProxy();
        private static Random random = new Random();

        static void Main(string[] args)
        {
            WriteLine("start...");
            var json = "{\"Topology\":{\"DataCenters\":[{\"Free\":0,\"Id\":\"DefaultDataCenter\",\"Max\":14,\"Racks\":[{\"DataNodes\":[{\"Free\":0,\"Max\":7,\"PublicUrl\":\"127.0.0.1:8081\",\"Url\":\"127.0.0.1:8081\",\"Volumes\":7},{\"Free\":0,\"Max\":7,\"PublicUrl\":\"127.0.0.1:8082\",\"Url\":\"127.0.0.1:8082\",\"Volumes\":7}],\"Free\":0,\"Id\":\"DefaultRack\",\"Max\":14}]}],\"Free\":0,\"Max\":14,\"layouts\":[{\"collection\":\"\",\"replication\":\"000\",\"ttl\":\"\",\"writables\":[4,7,5,3,6,1,2]},{\"collection\":\"benchmark\",\"replication\":\"000\",\"ttl\":\"\",\"writables\":[9,8,12,11,13,14,10]}]},\"Version\":\"0.74\"}";
            var masterStatus = JsonSerializer.DeserializeFromString<MasterStatus>(json);
            //UploadTmpFiles();
            //UploadPdfs();
            testRead();
            //UploadLocalFiles();
            WriteLine("finish...");
            Console.ReadKey();
        }

        //private static void UploadTmpFiles()
        //{
        //    var upper = random.Next(100) + 1;
        //    for (var i = 0; i < upper; i++)
        //    {
        //        var buffer = new byte[random.Next(100) + 50];
        //        random.NextBytes(buffer);

        //        #region Upload
        //        var result = WeedProxy.Upload(buffer, "file" + i);
        //        WriteLine(result.url);
        //        #endregion

        //        #region Lookup
        //        var index = result.url.LastIndexOf('/') + 1;
        //        var fid = result.url.Substring(index);
        //        var vid = long.Parse(fid.Split(',')[0]);
        //        var location = WeedProxy.Lookup(vid);
        //        WriteLine($"http://{location}/{fid}");
        //        #endregion
        //    }
        //    Console.ReadKey();
        //    #region Upload using HttpHelper
        //    var bytes = new byte[random.Next(100) + 50];
        //    random.NextBytes(bytes);
        //    var uri = new Uri(ConfigurationManager.AppSettings["WeedMasterUrl"]);
        //    var assignation = new WeedCSharpClientImpl(uri).Assign(new AssignParams());
        //    var uploadUrl = $"http://{assignation.Location.publicUrl}/{assignation.WeedFSFile.Fid}";
        //    var postResult = HttpHelper.MultipartPost(uploadUrl, bytes, "file", random.Next().ToString(), "application/octet-stream");
        //    WriteLine($"Upload using HttpHelper: {Environment.NewLine}{uploadUrl}{Environment.NewLine}{postResult}");
        //    #endregion

        //    #region Delete
        //    WeedProxy.Delete(uploadUrl);
        //    WriteLine(uploadUrl + " has been deleted.");
        //    #endregion
        //    Console.ReadKey();
        //}
        private  async static void UploadPdfs()
        {
            var resulttxt = @"D:\项目\Go\pdf\result.txt";
            try
            {
                using (StreamWriter sw = new StreamWriter(resulttxt, true))
                {
                    var fileInfo = new FileInfo(@"D:\项目\Go\test\基于Web的网盘系统的设计与实现.pdf");
                    var fileName = fileInfo.Name;
                    var stream = fileInfo.OpenRead();
                    //var buffer = StreamHelper.StreamToBytes(stream);
                    var result = await WeedProxy.Upload(stream, fileName);
                    sw.WriteLine(result.url + "  size:" + result.size);
                    var index = result.url.LastIndexOf('/') + 1;
                    var fid = result.url.Substring(index);
                    var vid = long.Parse(fid.Split(',')[0]);
                    var location = WeedProxy.Lookup(vid);
                    ReadResult rr = await WeedProxy.Read(result.url);
                    var pdfpath = @"D:\项目\Go\pdf\" + rr.filename;
                    using (var fileStream = File.Create(pdfpath))
                    {
                        rr.stream.CopyTo(fileStream);
                    }
                    var deleteResult = await WeedProxy.Delete(result.url);
                    ReadResult rr1 = await WeedProxy.Read(result.url);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        private async static void testRead()
        {
            try
            {
                var fileInfo = new FileInfo(@"D:\项目\Go\test\基于Web的网盘系统的设计与实现.pdf");
                var fileName = fileInfo.Name;
                var stream = fileInfo.OpenRead();
                //var buffer = StreamHelper.StreamToBytes(stream);
                var result = await WeedProxy.Upload(stream, fileName);
                var index = result.url.LastIndexOf('/') + 1;
                var fid = result.url.Substring(index);
                var vid = long.Parse(fid.Split(',')[0]);
                var deleteResult = await WeedProxy.Delete(fid);
                //ReadResult rr1 = await WeedProxy.Read(fid);
                var masterstatu = await WeedProxy.GetMasterStatus();
                foreach( var item in masterstatu.Topology.GetVolumesLocations())
                {
                    var volumestatu = await WeedProxy.GetVolumeStatus(item);
                    Console.WriteLine(volumestatu);
                }
               
            }
            catch (Exception)
            {

                throw;
            }
        }
        //private static void UploadLocalFiles()
        //{
        //    var files = Directory.GetFiles(ConfigurationManager.AppSettings["FilePath"]);
        //    foreach (var file in files)
        //    {
        //        #region Upload
        //        var fileInfo = new FileInfo(file);
        //        var fileName = fileInfo.Name;
        //        var stream = fileInfo.OpenRead();
        //        //var buffer = StreamHelper.StreamToBytes(stream);
        //        var result = WeedProxy.Upload(stream, fileName);
        //        WriteLine(result.url);
        //        #endregion

        //        #region Lookup
        //        var index = result.url.LastIndexOf('/') + 1;
        //        var fid = result.url.Substring(index);
        //        var vid = long.Parse(fid.Split(',')[0]);
        //        var location = WeedProxy.Lookup(vid);
        //        WriteLine($"http://{location}/{fid}");
        //        #endregion
        //    }
        //}
    }
}
