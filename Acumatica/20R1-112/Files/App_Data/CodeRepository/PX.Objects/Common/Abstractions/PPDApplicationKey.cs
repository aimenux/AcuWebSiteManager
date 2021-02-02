using PX.Common;
using System.Linq;
using System.Reflection;

namespace PX.Objects.Common.Abstractions
{
	public class PPDApplicationKey
	{
		private readonly FieldInfo[] _fields;

		public int? BranchID;
		public int? BAccountID;
		public int? LocationID;
		public string CuryID;
		public decimal? CuryRate;
		public int? AccountID;
		public int? SubID;
		public string TaxZoneID;

		public PPDApplicationKey()
		{
			_fields = GetType().GetFields();
		}

		public override bool Equals(object obj)
		{
			FieldInfo info = _fields.FirstOrDefault(field => !Equals(field.GetValue(this), field.GetValue(obj)));
			return info == null;
		}
		public override int GetHashCode()
		{
			int hashCode = 17;
			_fields.ForEach(field => hashCode = hashCode * 23 + field.GetValue(this).GetHashCode());
			return hashCode;
		}
	}
}
