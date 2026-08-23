namespace Events.Domain;

public class User
{
	public Guid Id { get; init; }
	public string Login { get; init; }
	public string PasswordHash { get; private set; }
	public UserRole Role { get; private set; }

	private User(Guid id, string login, string passwordHash, UserRole role)
	{
		Id = id;
		Login = login ?? throw new ArgumentNullException(nameof(login));
		PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
		Role = role;
	}

	public static User Create(Guid id, string login, string passwordHash, UserRole role)
	{
		return new User(id, login, passwordHash, role);
	}
	
	public void UpdatePassword(string passwordHash)
	{
		PasswordHash = passwordHash;
	}
	
	public void UpdateRole(UserRole role)
	{
		Role = role;
	}
}