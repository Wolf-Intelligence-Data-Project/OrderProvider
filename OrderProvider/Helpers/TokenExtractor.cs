using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OrderProvider.Interfaces.Helpers;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace OrderProvider.Helpers;

public class TokenExtractor : ITokenExtractor
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<TokenExtractor> _logger;
    private readonly string _jwtSecretKey;

    public TokenExtractor(IHttpContextAccessor httpContextAccessor, ILogger<TokenExtractor> logger, string jwtSecretKey)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _jwtSecretKey = jwtSecretKey; 
    }

    public Guid GetUserIdFromToken()
    {
        // Extracting the AccessToken from the HttpOnly cookie
        var accessToken = _httpContextAccessor.HttpContext?.Request.Cookies["AccessToken"];

        if (string.IsNullOrEmpty(accessToken))
        {
            _logger.LogWarning("AccessToken cookie is missing.");
            throw new UnauthorizedAccessException("AccessToken cookie is missing.");
        }

        try
        {
            // Decoding and validating the JWT token
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadToken(accessToken) as JwtSecurityToken;

            if (token == null)
            {
                _logger.LogWarning("Invalid JWT token.");
                throw new UnauthorizedAccessException("Invalid JWT token.");
            }

            // Get the userId from the token's payload
            var userIdClaim = token?.Claims.FirstOrDefault(c => c.Type == "userId");

            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                _logger.LogWarning("User ID claim is missing or invalid.");
                throw new UnauthorizedAccessException("User ID claim is missing or invalid.");
            }

            _logger.LogInformation("Extracted userId: {UserId}", userId);
            return userId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decoding the JWT token.");
            throw new UnauthorizedAccessException("Error decoding the JWT token.", ex);
        }
    }
}
