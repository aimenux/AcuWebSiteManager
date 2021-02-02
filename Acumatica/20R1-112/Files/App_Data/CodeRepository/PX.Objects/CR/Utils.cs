using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;

using PX.SM;

namespace PX.Objects.CR
{
	[Obsolete("Will be removed in 7.0 version")]
	public static class Utils
	{
		public static BqlCommand CreateSelectCommand(Type entityType, Type fieldType)
		{
			Type required = BqlCommand.Compose(typeof(Required<>), fieldType);
			Type equal = BqlCommand.Compose(typeof(Equal<>), required);
			Type where = BqlCommand.Compose(typeof(Where<,>), fieldType, equal);
			return BqlCommand.CreateInstance(typeof(Select<,>), entityType, where);
		}

		public static BqlCommand CreateSelectCommand(Type fieldType)
		{
			return CreateSelectCommand(fieldType.DeclaringType, fieldType);
		}
	}
}
