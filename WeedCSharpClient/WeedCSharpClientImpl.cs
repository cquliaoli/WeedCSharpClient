using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;

using WeedCSharpClient.Caching;
using WeedCSharpClient.Helper;
using WeedCSharpClient.Net;
using WeedCSharpClient.Status;

using ServiceStack.Text;
using System.Threading.Tasks;
using System.Web;

namespace WeedCSharpClient
{
    public class WeedCSharpClientImpl : IWeedCSharpClient
    {
        private readonly Uri _masterUri;
        private readonly HttpClient _httpClient = new HttpClient();
        private static readonly ILookupCache LookupCache = new MapLookupCache();

        public WeedCSharpClientImpl(Uri masterUri)
        {
            _masterUri = masterUri;
        }

        #region Implement IWeedCSharpClient
        /// <summary>
        /// 异步返回分配id
        /// </summary>
        /// <param name="assignParams"></param>
        /// <returns></returns>

        public async Task<Assignation> Assign(AssignParams assignParams)
        {
            var url = new StringBuilder(new Uri(_masterUri, "/dir/assign").AbsoluteUri);
            url.Append("?count=").Append(assignParams.VersionCount);

            if (assignParams.ReplicationStrategy != null)
            {
                url.Append("&replication=");
                url.AppendFormat("{0:D3}", (int)assignParams.ReplicationStrategy);
            }

            if (!string.IsNullOrWhiteSpace(assignParams.Collection))
            {
                url.Append("&collection=").Append(assignParams.Collection);
            }
            Assignation assignation;
            try
            {
                var response = await _httpClient.GetAsync(url.ToString());
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.DeserializeFromString<AssignResult>(content);
                if (!string.IsNullOrWhiteSpace(result.error))
                {
                    throw new WeedFSException(result.error);
                }
                assignation = new Assignation(result);
            }
            catch (Exception)
            {

                throw;
            }
            return assignation;
        }

        public async Task<WriteResult> Write(WeedFSFile file, Location location, FileInfo fileToUpload)
        {
            WriteResult writeReuslt;
            if (fileToUpload.Length == 0)
            {
                throw new WeedFSException("Cannot write a 0-length file");
            }
            writeReuslt = await Write(file, location, fileToUpload.Name, fileToUpload);
            return writeReuslt;
        }

        public async Task<WriteResult> Write(WeedFSFile file, Location location, byte[] dataToUpload, string fileName)
        {
            WriteResult writeReuslt;
            if (dataToUpload.Length == 0)
            {
                throw new WeedFSException("Cannot write a 0-length data");
            }
            writeReuslt= await Write(file, location, fileName, null, dataToUpload);
            return writeReuslt;
        }

        public async Task<WriteResult> Write(WeedFSFile file, Location location, Stream inputToUpload, string fileName)
        {
            WriteResult writeReuslt= await Write(file, location, fileName, null, null, inputToUpload);
            return writeReuslt;
        }
        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<bool> Delete(string url)
        {
            try
            {
                var absoluteUrl = new StringBuilder();
                if (!url.Contains("http"))
                {
                    absoluteUrl.Append("http://");
                }
                absoluteUrl.Append(url);
                var response = await _httpClient.DeleteAsync(absoluteUrl.ToString());
                if (response.StatusCode < HttpStatusCode.OK || response.StatusCode > HttpStatusCode.PartialContent)
                {
                    var index = url.LastIndexOf('/') + 1;
                    var fid = url.Substring(index);
                    throw new WeedFSException($"Error deleting file {fid} on {url}: {response.StatusCode} {response.ReasonPhrase}");
                }
                return true;
            }
            catch (Exception)
            {

                throw;
            }
        }
        /// <summary>
        /// 查找location
        /// </summary>
        /// <param name="volumeId"></param>
        /// <returns></returns>
        public async Task<List<Location>> Lookup(long volumeId)
        {
            List<Location> locations;
            locations = LookupCache?.Lookup(volumeId);
            if (locations != null)
            {
                return locations;
            }

            var url = new StringBuilder(new Uri(_masterUri, "/dir/lookup").AbsoluteUri);
            url.Append("?volumeId=").Append(volumeId);
            try
            {
                var response = await _httpClient.GetAsync(url.ToString());
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.DeserializeFromString<LookupResult>(content);
                if (!string.IsNullOrWhiteSpace(result.error))
                {
                    throw new WeedFSException(result.error);
                }

                LookupCache?.SetLocation(volumeId, result.locations);
                locations = result.locations;
            }
            catch (Exception)
            {

                throw;
            }
            return locations;
        }
        /// <summary>
        /// 下载
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<ReadResult> ReadFile(string url)
        {
            var absoluteUrl = new StringBuilder();
            if (!url.Contains("http"))
            {
                absoluteUrl.Append("http://");
            }
            absoluteUrl.Append(url);
            ReadResult readResult=new ReadResult();
            var cts = new CancellationTokenSource();
            try
            {
                var response = await _httpClient.GetAsync(absoluteUrl.ToString(), cts.Token);
                var charset = response.Content.Headers.ContentType.CharSet;
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    cts.Cancel();

                    throw new WeedFSFileNotFoundException(url);
                }
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    
                    throw new WeedFSException($"Error reading file  on {url}: {response.StatusCode} {response.ReasonPhrase}");
                }
                IEnumerable<string> contentDisposition;
                if (response.Content.Headers.TryGetValues("Content-Disposition", out contentDisposition))
                {
                    var contentDispostions = contentDisposition as string[] ?? contentDisposition.ToArray();
                    foreach (var s in contentDispostions)
                    {
                        string temp = s.Replace("\"", "");
                        int equalIndex = temp.IndexOf("=", StringComparison.Ordinal);
                        if (temp.Contains("filename")&& equalIndex!=-1)
                        {
                            temp = temp.Substring(equalIndex+1);
                            readResult.filename = temp;
                            break;
                        }
                    }
                    
                }
                /*foreach (var header in response.Content.Headers)
                {
                    
                    if (header.Key.Equals("Content-Disposition"))
                    {
                        ContentDispositionHeaderValue headerValue = null;
                        ContentDispositionHeaderValue.TryParse(header.Value.FirstOrDefault(), out headerValue);
                        readResult.filename = header.Value.FirstOrDefault();
                    }
                    Console.WriteLine(header.Key + ":" + FormatValue(header.Value));
                }*/
                ContentDispositionHeaderValue contentDispositionHeaderValue = response.Content.Headers.ContentDisposition;
                if (contentDispositionHeaderValue != null&&string.IsNullOrEmpty(readResult.filename))
                {
                    readResult.filename = contentDispositionHeaderValue.FileName;
                }
                if (string.IsNullOrEmpty(readResult.filename))
                {
                    readResult.filename = response.Content.Headers.ContentType.MediaType;
                }
                var mediaType = response.Content.Headers.ContentType;
                /*if (!string.IsNullOrEmpty(readResult.filename))
                {
                    readResult.filename = Uri.UnescapeDataString(readResult.filename);
                }*/
                readResult.stream = response.Content.ReadAsStreamAsync().Result;
            }
            catch (Exception)
            {

                throw;
            }
            return readResult;
        }
        string FormatValue(object obj)
        {
            if (obj is Array)
            {
                return String.Join(", ", (object[])obj);
            }
            return obj == null ? "null" : obj.ToString();
        }
        [Obsolete]
        public Stream Read(WeedFSFile file, Location location)
        {
            var url = new StringBuilder();
            if (!location.publicUrl.Contains("http"))
            {
                url.Append("http://");
            }
            url.AppendFormat("{0}/{1}", location.publicUrl, file.Fid);

            if (file.Version > 0)
            {
                url.Append('_').Append(file.Version);
            }

            var cts = new CancellationTokenSource();
            using (var response = _httpClient.GetAsync(url.ToString(), cts.Token))
            {
                var result = response.Result;
                var statusCode = result.StatusCode;

                if (statusCode == HttpStatusCode.NotFound)
                {
                    cts.Cancel();

                    throw new WeedFSFileNotFoundException(file, location);
                }

                if (statusCode != HttpStatusCode.OK)
                {
                    if (!response.IsCanceled)
                        cts.Cancel();

                    throw new WeedFSException($"Error reading file {file.Fid} on {location.publicUrl}: {statusCode} {result.ReasonPhrase}");
                }

                return response.Result.Content.ReadAsStreamAsync().Result;
            }
        }

        public async Task<MasterStatus> GetMasterStatus()
        {
            var url = new Uri(_masterUri, "/dir/status").AbsoluteUri;
            MasterStatus masterStatus;
            try
            {
                var response = await _httpClient.GetAsync(url);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new IOException("Not 200 status recieved for master status url: " + url);
                }
                var result = await response.Content.ReadAsStringAsync();
                
                masterStatus=JsonSerializer.DeserializeFromString<MasterStatus>(result);
            }
            catch (Exception)
            {

                throw;
            }
            return masterStatus;
        }

        public async Task<VolumeStatus> GetVolumeStatus(Location location)
        {
            var url = new StringBuilder();
            if (!location.publicUrl.Contains("http"))
            {
                url.Append("http://");
            }
            url.Append(location.publicUrl).Append("/status");

            var urlStr = url.ToString();
            VolumeStatus volumeStatus;
            try
            {
                var response = await _httpClient.GetAsync(urlStr);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new IOException("Not 200 status recieved for master status url: " + urlStr);
                }
                var content = response.Content.ReadAsStringAsync().Result;
                volumeStatus=JsonSerializer.DeserializeFromString<VolumeStatus>(content);
            }
            catch (Exception)
            {

                throw;
            }
            return volumeStatus;
        }

        #endregion

        private string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return "file";
            }

            return fileName.Length > 255
                ? fileName.Substring(0, 255)
                : fileName;
        }
        /// <summary>
        /// 异步上传
        /// </summary>
        /// <param name="file"></param>
        /// <param name="location"></param>
        /// <param name="fileName"></param>
        /// <param name="fileToUpload"></param>
        /// <param name="dataToUpload"></param>
        /// <param name="inputToUpload"></param>
        /// <returns></returns>
        private async Task<WriteResult> Write(WeedFSFile file, Location location, string fileName = null, FileInfo fileToUpload = null,
            byte[] dataToUpload = null, Stream inputToUpload = null)
        {
            WriteResult writeResult;
            var url = new StringBuilder();
            if (!location.publicUrl.Contains("http"))
            {
                url.Append("http://");
            }
            url.AppendFormat("{0}/{1}", location.publicUrl, file.Fid);

            if (file.Version > 0)
            {
                url.Append('_').Append(file.Version);
            }

            byte[] buffer;
            if (fileToUpload != null)
            {
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    fileName = fileToUpload.Name;
                }

                var stream = fileToUpload.OpenRead();
                buffer = StreamHelper.StreamToBytes(stream);
            }
            else if (dataToUpload != null)
            {
                buffer = dataToUpload;
            }
            else
            {
               buffer = StreamHelper.StreamToBytes(inputToUpload); 
            }

            var multipart = new MultipartFormDataContent();
            var ContentDisposition = new ContentDispositionHeaderValue("form-data");
            ContentDisposition.FileName = fileName;
            multipart.Add(new ByteArrayContent(buffer)
            {
                Headers =
                    {
                        ContentType = new MediaTypeHeaderValue("application/octet-stream"),
                        ContentDisposition =ContentDisposition
                    }
                }, "file", SanitizeFileName(fileName));

            var fileUrl = url.ToString();
            try
            {
                var response = await _httpClient.PostAsync(fileUrl, multipart);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                writeResult = JsonSerializer.DeserializeFromString<WriteResult>(content);
                writeResult.url = fileUrl;

                if (!string.IsNullOrWhiteSpace(writeResult.error))
                {
                    throw new WeedFSException(writeResult.error);
                }
            }
            catch (Exception)
            {

                throw;
            }
            return writeResult;
        }
    }
}
