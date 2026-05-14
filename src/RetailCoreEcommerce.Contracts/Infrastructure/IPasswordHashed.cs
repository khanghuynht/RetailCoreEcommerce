namespace RetailCoreEcommerce.Contracts.Infrastructure;

public interface IPasswordHashed
{
    string HashPassword(string password);
    bool VerifyPassword(string providedPassword, string hashedPassword);
}