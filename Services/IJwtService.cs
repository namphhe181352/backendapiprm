using BusinessObjects.Models;

namespace Services;

public interface IJwtService
{
    string GenerateToken(User user);
    DateTime GetExpiryUtc();
}
