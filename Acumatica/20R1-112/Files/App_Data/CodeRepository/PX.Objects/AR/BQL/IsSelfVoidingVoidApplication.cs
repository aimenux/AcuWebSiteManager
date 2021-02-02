using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using PX.Data;
using PX.Data.SQLTree;

namespace PX.Objects.AR.BQL
{
	/// <summary>
	/// A predicate that returns <c>true</c> if and only if the application record
	/// is produced by the voiding process for one of the Accounts Receivable 
	/// self-voiding documents. Due to the fact that there is no separate voiding
	/// document record for self-voiding documents, such applications should be
	/// separately reported in <see cref="ARStatementProcess"/>.
	/// </summary>
	public class IsSelfVoidingVoidApplication<TAdjust> : IBqlUnary
		where TAdjust : ARAdjust
	{
		private static Type _whereType;
		
		private static IBqlCreator Where
		{
			get
			{
				if (_whereType == null)
				{
					Dictionary<string, Type> fields = typeof(TAdjust)
						.GetNestedTypes()
						.ToDictionary(nestedTypes => nestedTypes.Name);

					Type voidedApplicationNumberField = fields[nameof(ARAdjust.voidAdjNbr)];
					Type voidedField = fields[nameof(ARAdjust.voided)];
					Type adjustingDocumentTypeField = fields[nameof(ARAdjust.adjgDocType)];
					Type adjustedDocumentTypeField = fields[nameof(ARAdjust.adjdDocType)];

					_whereType = BqlCommand.Compose(
						typeof(Where<,,>),
							voidedApplicationNumberField, typeof(IsNotNull),
							typeof(And<,,>), voidedField, typeof(Equal<>), typeof(True),
							typeof(And<>), typeof(Where2<,>),
								typeof(IsSelfVoiding<>), adjustingDocumentTypeField,
								typeof(Or<>), typeof(IsSelfVoiding<>), adjustedDocumentTypeField);
				}

				return Activator.CreateInstance(_whereType) as IBqlUnary;
			}
		}

		public bool AppendExpression(ref SQLExpression exp, PXGraph graph, BqlCommandInfo info, BqlCommand.Selection selection)
			=> Where.AppendExpression(ref exp, graph, info, selection);

		public void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
			=> Where.Verify(cache, item, pars, ref result, ref value);

		public static bool Verify(TAdjust application)
			=> application.Voided == true
			&& application.VoidAdjNbr != null
			&& (
				ARDocType.IsSelfVoiding(application.AdjgDocType)
				|| ARDocType.IsSelfVoiding(application.AdjdDocType));
	}
}
