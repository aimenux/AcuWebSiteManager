using PX.Data;

namespace PX.Objects.PR
{
	/// <summary>
	/// Provide a nice formatting for Watch window to display Key values of records when debugging.
	/// </summary>
	/// <example>
	/// [DebuggerDisplay(@"{DebuggerHelper.Info(this)}")]
	/// public class PRPayment : IBqlTable
	/// </example>
	/// <remarks>
	/// This attribute should not be left in code as it can affect performance. Remove after debugging.
	/// </remarks>
	public class DebuggerHelper
	{
		public static string Info(object obj)
		{
			var debugString = obj.GetType().Name + " :";
			foreach (var prop in obj.GetType().GetProperties())
			{
				foreach (var att in prop.GetCustomAttributes(true))
				{
					if (att is PXDBFieldAttribute dbAtt && dbAtt.IsKey == true)
					{
						debugString += " " + prop.Name + " = {" + prop.GetValue(obj) + "},";
					}
				}
			}

			return debugString;
		}
	}
}
