namespace Patient_mgt.Infrastructure
{
    public interface IOcrService
    {
        Task<string> ExtractTextFromImageAsync(byte[] imageData);
    }
}