using System.Security.Cryptography;
using System.Text;
using Events.Application.Interfaces;

namespace Events.Infrastructure.Auth;

public class PasswordHasher : IPasswordHasher
{
	public string Hash(string password)
	{
		var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
		return Convert.ToHexString(bytes);
	}

	public bool Verify(string inputPassword, string storedHashHex)
	{
		var inputHash = SHA256.HashData(Encoding.UTF8.GetBytes(inputPassword));
		var storedHash = Convert.FromHexString(storedHashHex);

		return CryptographicOperations.FixedTimeEquals(inputHash, storedHash);
	}
}