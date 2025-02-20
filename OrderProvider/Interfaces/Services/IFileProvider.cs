using OrderProvider.Entities;

namespace OrderProvider.Interfaces.Services
{
    public interface IFileProvider
    {
        Task GenerateProductFileAsync(List<ProductEntity> products);
    }

}