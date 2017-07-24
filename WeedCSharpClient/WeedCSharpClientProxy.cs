using System;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using WeedCSharpClient.Net;
using WeedCSharpClient.Status;

namespace WeedCSharpClient
{
    public interface IWeedCSharpClientSubject 
    {
        Task<WriteResult> Upload(byte[] buffer, string fileName = null, string fid = null, 
            ReplicationStrategy replicationStrategy = ReplicationStrategy.None);
        Task<WriteResult> Upload(Stream stream, string fileName = null, string fid = null, 
            ReplicationStrategy replicationStrategy = ReplicationStrategy.None);
        Task<bool> Delete(string fid);
        Task<string> Lookup(long volumeId);
        /// <summary>
        /// 读取文件
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        Task<ReadResult> Read(string fileId);
        /// <summary>
        /// 通过文件url读取
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        Task<ReadResult> ReadByUrlAsync(string url);
        /// <summary>
        /// master状态
        /// </summary>
        /// <returns></returns>
        Task<MasterStatus> GetMasterStatus();
       /// <summary>
       /// volume状态
       /// </summary>
       /// <param name="location"></param>
       /// <returns></returns>

        Task<VolumeStatus> GetVolumeStatus(Location location);
    }
    //Singleton
    internal sealed class WeedCSharpClientSubject : IWeedCSharpClientSubject
    {
        private WeedCSharpClientSubject() { }
        public static readonly WeedCSharpClientSubject Instance = new WeedCSharpClientSubject();

        private readonly WeedCSharpClientImpl _weedCSharpClient = new WeedCSharpClientImpl(new Uri(WeedCSharpClient.Properties.Settings.Default.WeedMasterUrl));

        /// <summary>
        /// store or update the file content with byte array
        /// </summary>
        /// <param name="buffer">byte array</param>
        /// <param name="fileName">file name</param>
        /// <param name="fid">fid</param>
        /// <param name="replicationStrategy">replication strategy</param>
        /// <returns>Write Result</returns>
        public async Task<WriteResult> Upload(byte[] buffer, string fileName = null, string fid = null, 
            ReplicationStrategy replicationStrategy = ReplicationStrategy.None)
        {
            WriteResult writeResult;
            try
            {
                var assignResult = await _weedCSharpClient.Assign(new AssignParams(replicationStrategy));
                writeResult = await _weedCSharpClient.Write(assignResult.WeedFSFile, assignResult.Location, buffer, fileName);
            }
            catch (Exception)
            {

                throw;
            }
            return writeResult;
        }
        /// <summary>
        /// store or update the file content with stream
        /// </summary>
        /// <param name="stream">stream</param>
        /// <param name="fileName">file name</param>
        /// <param name="fid">fid</param>
        /// <param name="replicationStrategy">replication strategy</param>
        /// <returns>Write Result</returns>
        public async Task<WriteResult> Upload(Stream stream, string fileName = null, string fid = null, 
            ReplicationStrategy replicationStrategy = ReplicationStrategy.None)
        {
            WriteResult writeResult;
            try
            {
                var assignResult = await _weedCSharpClient.Assign(new AssignParams(replicationStrategy));
                writeResult = await _weedCSharpClient.Write(assignResult.WeedFSFile, assignResult.Location, stream, fileName);
            }
            catch (Exception)
            {

                throw;
            }
            return writeResult;
        }

        /// <summary>
        /// delete the file
        /// </summary>
        /// <param name="fid">fid</param>
        public async Task<bool> Delete(string fid)
        {
            bool result = false;
            try
            {
                var vid = long.Parse(fid.Split(',')[0]);
                var url = await this.Lookup(vid);
                result = await _weedCSharpClient.Delete($"{url}/{fid}");
            }
            catch (Exception)
            {

                throw;
            }
            return result;
        }

        /// <summary>
        /// lookup the file
        /// </summary>
        /// <param name="volumeId">volume id</param>
        /// <returns>url</returns>
        public async Task<string> Lookup(long volumeId)
        {
            string location;
            try
            {
                var locations = await _weedCSharpClient.Lookup(volumeId);
                if (locations.Count > 0)
                {
                    location = locations[0].publicUrl;
                }
                else
                {
                    throw new ArgumentException("There is no location", nameof(locations));
                }
            }
            catch (Exception)
            {

                throw;
            }
            return location;
        }
        /// <summary>
        /// 读取文件
        /// </summary>
        /// <param name="fid">4,10011dfd33d902</param>
        /// <returns></returns>
        public async Task<ReadResult> Read(string fid)
        {
            ReadResult readResult;
            try
            {
                var vid = long.Parse(fid.Split(',')[0]);
                var url = await this.Lookup(vid);
                readResult = await _weedCSharpClient.ReadFile($"{url}/{fid}");
                if (!string.IsNullOrEmpty(readResult.filename))
                {
                    readResult.filename = Path.Combine(fid, Path.GetExtension(readResult.filename));
                }
            }
            catch (Exception)
            {

                throw;
            }
            return readResult;
        }
        /// <summary>
        /// 通过url读取文件
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<ReadResult> ReadByUrlAsync(string url)
        {
            ReadResult readResult;
            try
            {
            	readResult = await _weedCSharpClient.ReadFile(url);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            return readResult;
        }

        public async Task<MasterStatus> GetMasterStatus()
        {
            MasterStatus masterStatus;
            try
            {
                masterStatus = await _weedCSharpClient.GetMasterStatus();
            }
            catch (Exception)
            {

                throw;
            }
            return masterStatus;
        }

        public async Task<VolumeStatus> GetVolumeStatus(Location location)
        {
            VolumeStatus volumeStatus;

            try
            {
                volumeStatus = await _weedCSharpClient.GetVolumeStatus(location);
            }
            catch (Exception)
            {

                throw;
            }
            return volumeStatus;
        }
    }
    //Proxy
    public class WeedCSharpClientProxy : IWeedCSharpClientSubject 
    {
        public async Task<WriteResult> Upload(byte[] buffer, string fileName = null, string fid = null, 
            ReplicationStrategy replicationStrategy = ReplicationStrategy.None)
        {
            WriteResult writeResult;

            try
            {
                writeResult = await WeedCSharpClientSubject.Instance.Upload(buffer, fileName, fid, replicationStrategy);
            }
            catch (Exception)
            {

                throw;
            }
            return writeResult;
        }

        public async Task<WriteResult> Upload(Stream stream, string fileName = null, string fid = null, 
            ReplicationStrategy replicationStrategy = ReplicationStrategy.None)
        {
            WriteResult writeResult;
            try
            {
                writeResult = await WeedCSharpClientSubject.Instance.Upload(stream, fileName, fid, replicationStrategy);
            }
            catch (Exception)
            {

                throw;
            }
            return writeResult;
        }
        /// <summary>
        /// 删除  通过fid删除
        /// </summary>
        /// <param name="fid"></param>
        /// <returns></returns>
        public async Task<bool> Delete(string fid)
        {
            bool result = false;
            try
            {
                result = await WeedCSharpClientSubject.Instance.Delete(fid);
            }
            catch (Exception)
            {

                throw;
            }
            return result;
        }
        /// <summary>
        /// 通过volumeId查询localtion
        /// </summary>
        /// <param name="volumeId"></param>
        /// <returns></returns>
        public async Task<string> Lookup(long volumeId)
        {
            string localtion;

            try
            {
                localtion = await WeedCSharpClientSubject.Instance.Lookup(volumeId);
            }
            catch (Exception)
            {

                throw;
            }
            return localtion;
        }
        /// <summary>
        /// 通过fid获取文件 1,10011dfd33d902
        /// </summary>
        /// <param name="fid"></param>
        /// <returns></returns>
        public async Task<ReadResult> Read(string fid)
        {
            ReadResult readResult;
            try
            {
                readResult = await WeedCSharpClientSubject.Instance.Read(fid);
            }
            catch (Exception)
            {
                throw;
            }
            return readResult;
        }

        public async Task<ReadResult> ReadByUrlAsync(string url)
        {
            ReadResult readResult;
            try
            {
                readResult = await WeedCSharpClientSubject.Instance.ReadByUrlAsync(url);
            }
            catch (Exception)
            {
                throw;
            }
            return readResult;
        }

        /// <summary>
        /// 获得master状态
        /// </summary>
        /// <returns></returns>
        public async Task<MasterStatus> GetMasterStatus()
        {
            MasterStatus masterStatus;
            try
            {
                masterStatus = await WeedCSharpClientSubject.Instance.GetMasterStatus();
            }
            catch (Exception)
            {
                throw;
            }
            return masterStatus;
        }
        /// <summary>
        /// 获得volume状态
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public async Task<VolumeStatus> GetVolumeStatus(Location location)
        {
            VolumeStatus volumeStatus;
            try
            {
                volumeStatus = await WeedCSharpClientSubject.Instance.GetVolumeStatus(location);
            }
            catch (Exception)
            {
                throw;
            }
            return volumeStatus;
        }
    }
}
