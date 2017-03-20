using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using WeedCSharpClient.Net;
using WeedCSharpClient.Status;

namespace WeedCSharpClient
{
    /// <summary>
    /// Note: fileName that exceeds 256 characters will be truncated.
    /// </summary>
    public interface IWeedCSharpClient
    {
        Task<Assignation> Assign(AssignParams assignParams);

        Task<WriteResult> Write(WeedFSFile weedFSFile, Location location, FileInfo file);

        Task<WriteResult> Write(WeedFSFile file, Location location, byte[] dataToUpload, string fileName);

        Task<WriteResult> Write(WeedFSFile file, Location location, Stream inputToUpload, string fileName);

        Task<bool> Delete(string url);

        Task<List<Location>> Lookup(long volumeId);

        ///    Stream Read(WeedFSFile file, Location location);
        Task<ReadResult> ReadFile(string url);

        Task<MasterStatus> GetMasterStatus();

        Task<VolumeStatus> GetVolumeStatus(Location location);
    }
}
