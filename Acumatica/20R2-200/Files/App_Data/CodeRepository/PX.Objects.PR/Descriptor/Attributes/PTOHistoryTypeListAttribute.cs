using PX.Data;

namespace PX.Objects.PR
{
	public class PTOHistoryType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Add, Remove },
				new string[] { Messages.Add, Messages.Remove, })
			{ }
		}

		public const string Add = "A";
		public class add : PX.Data.BQL.BqlString.Constant<add>
		{
			public add()
			  : base(Add)
			{
			}
		}

		public const string Remove = "R";
		public class remove : PX.Data.BQL.BqlString.Constant<remove>
		{
			public remove()
			  : base(Remove)
			{
			}
		}
	}
}
