namespace OrderProvider.Interfaces.Helpers
{
    public interface ITokenExtractor
    {
        Guid GetUserIdFromToken();
    }
}
