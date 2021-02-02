using System;
using PX.Common;
using PX.Data;
using PX.SM;

namespace PX.SM
{
	#region SMReportSubstituteAttribute

	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class SMReportSubstituteAttribute : PXEventSubscriberAttribute
	{
		private readonly Type _valueField;
		private readonly Type _textField;

		public SMReportSubstituteAttribute(Type valueField, Type textField)
			: base()
		{
			CheckBqlField(valueField, "valueField");
			CheckBqlField(textField, "textField");

			_valueField = valueField;
			_textField = textField;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			var table = sender.GetItemType();
			var command = BqlCommand.CreateInstance(
				typeof(Select<,>), table,
				typeof(Where<,>),
				_valueField, typeof(Equal<>), typeof(Required<>), _valueField);
			var view = new PXView(sender.Graph, true, command);
			sender.Graph.FieldSelecting.AddHandler(table, _FieldName, 
				(cache, args) =>
					{
						var row = view.SelectSingle(args.ReturnValue);
						var text = cache.GetValue(row, _textField.Name);
						args.ReturnValue = text;
					});
		}

		private static void CheckBqlField(Type field, string argumentName)
		{
			if (field == null) throw new ArgumentNullException(argumentName);
			if (!typeof(IBqlField).IsAssignableFrom(field))
				throw new ArgumentException(PXLocalizer.LocalizeFormat(ErrorMessages.InvalidIBqlField, field.FullName), argumentName);
		}
	}

	#endregion

	#region userType

	public sealed class userType : PX.Data.BQL.BqlString.Constant<userType>
	{
		public userType()
			: base(typeof(Users).FullName)
		{
		}
	}

	#endregion
}
