namespace PX.Objects.CS
{
	public interface IAdvancedAuthenticationRestrictor
	{
		bool IsAllowedProviderName(string name);
	}
}
