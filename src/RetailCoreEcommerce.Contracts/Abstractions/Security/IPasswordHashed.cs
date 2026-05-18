namespace RetailCoreEcommerce.Contracts.Abstractions.Security;

public interface IPasswordHashed
{
    string HashPassword(string password);
    bool VerifyPassword(string providedPassword, string hashedPassword);
}