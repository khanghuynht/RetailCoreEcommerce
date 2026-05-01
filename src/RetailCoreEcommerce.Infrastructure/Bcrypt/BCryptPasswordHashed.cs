using RetailCoreEcommerce.Application.Abstractions;

namespace RetailCoreEcommerce.Infrastructure.Bcrypt;

public class BCryptPasswordHashed : IPasswordHashed
{
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string providedPassword, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(providedPassword, hashedPassword);
    }
}