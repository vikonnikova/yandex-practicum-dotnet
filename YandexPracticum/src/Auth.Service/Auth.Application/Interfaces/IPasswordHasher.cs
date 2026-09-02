namespace Auth.Application.Interfaces;

public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string inputPassword, string storedHash);
}