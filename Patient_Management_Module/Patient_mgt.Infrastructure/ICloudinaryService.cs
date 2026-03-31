namespace Patient_mgt.Infrastructure
{
    public interface ICloudinaryService
    {
        Task<string> UploadImageAsync(byte[] imageData, string fileName, string folder = "patient-photos");
        Task<string> UploadDocumentAsync(byte[] documentData, string fileName, string folder = "medical-reports");
        Task<bool> DeleteFileAsync(string publicId);
        Task<byte[]> DownloadFileAsync(string url);
        Task<string> GetSignedUrlAsync(string publicId);
    }
}