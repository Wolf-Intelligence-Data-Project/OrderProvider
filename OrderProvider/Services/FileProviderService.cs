using OrderProvider.Interfaces.Services;

namespace OrderProvider.Services
{
    public class FileProviderService : IFileProviderService
    {
        private readonly HttpClient _httpClient;

        public FileProviderService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task SendUserIdToFileProvider(Guid userId)
        {
            await _httpClient.PostAsJsonAsync("https://fileprovider/api/files", new { UserId = userId });
        }
    }

}
