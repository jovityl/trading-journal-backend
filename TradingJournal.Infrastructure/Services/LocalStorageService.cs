using TradingJournal.Application.Interfaces;

namespace TradingJournal.Infrastructure.Services
{
    public class LocalStorageService : IStorageService
    {
        private readonly string _uploadPath;

        public LocalStorageService()
        {
            _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            Directory.CreateDirectory(_uploadPath);
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default)
        {
            var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
            var filePath = Path.Combine(_uploadPath, uniqueFileName);

            using var fileOutput = new FileStream(filePath, FileMode.Create);
            await fileStream.CopyToAsync(fileOutput, cancellationToken);

            return $"/uploads/{uniqueFileName}";
        }
    }
}
