using OrderProvider.Entities;

namespace OrderProvider.Interfaces.Services
{
    public interface IFileProviderService
    {
        Task SendUserIdToFileProvider(Guid userId);
    }


}