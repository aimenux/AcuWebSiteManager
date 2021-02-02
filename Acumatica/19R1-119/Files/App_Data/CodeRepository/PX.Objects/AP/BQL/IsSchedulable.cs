using System;
using System.Collections.Generic;
using System.Text;

using PX.Data;
using PX.Objects.GL;
using System.Linq;
using PX.Data.SQLTree;
using PX.Objects.CS;
using PX.Objects.Common;
using PX.Objects.GL.BQL;

namespace PX.Objects.AP.BQL
{
	/// <summary>
	/// A predicate that returns <c>true</c> if and only if the
	/// <see cref="APRegister"/> descendant record can be added to
	/// a recurring transaction schedule.
	/// </summary>
	/// <remarks>
	/// Due to the absence of Exists<> BQL predicate, the BQL implementation
	/// of the current predicate does not check for the presence of <see cref="GLVoucher"/> 
	/// records referencing the document record. It should be done separately via a 
	/// left join and a null check. See <see cref="IsDocumentSchedulable"/> for example 
	/// and details.
	/// </remarks>
	public class IsSchedulable<TRegister> : IBqlUnary
		where TRegister : APRegister
	{
		private IBqlCreator where;

		public IsSchedulable()
		{
			Dictionary<string, Type> fields = typeof(TRegister)
				.GetNestedTypes()
				.ToDictionary(nestedTypes => nestedTypes.Name);

			Type releasedField = fields[nameof(APRegister.released)];
			Type prebookedField = fields[nameof(APRegister.prebooked)];
			Type holdField = fields[nameof(APRegister.hold)];
			Type voidedField = fields[nameof(APRegister.voided)];
			Type rejectedField = fields[nameof(APRegister.rejected)];
			Type origModuleField = fields[nameof(APRegister.origModule)];
			Type isMigratedRecordField = fields[nameof(APRegister.isMigratedRecord)];
			Type createdByScreenIdField = fields[nameof(APRegister.createdByScreenID)];
			Type docTypeField = fields[nameof(APRegister.docType)];
			Type refNbrField = fields[nameof(APRegister.refNbr)];
			Type noteIdField = fields[nameof(APRegister.noteID)];

			Type whereType = BqlCommand.Compose(
				typeof(Where<,,>),
					releasedField, typeof(Equal<>), typeof(False),
					typeof(And<,,>), prebookedField, typeof(Equal<>), typeof(False),
					typeof(And<,,>), holdField, typeof(Equal<>), typeof(False),
					typeof(And<,,>), voidedField, typeof(Equal<>), typeof(False),
					typeof(And<,,>), rejectedField, typeof(Equal<>), typeof(False),
					typeof(And<,,>), origModuleField, typeof(Equal<>), typeof(BatchModule.moduleAP),
					typeof(And<,,>), isMigratedRecordField, typeof(Equal<>), typeof(False),
					typeof(And2<,>), typeof(Not<>), typeof(IsPOLinked<,>), docTypeField, refNbrField,
					typeof(And<>), typeof(Not<>), typeof(ExistsJournalVoucher<>), noteIdField);

			where = Activator.CreateInstance(whereType) as IBqlUnary;
		}

		public bool AppendExpression(ref SQLExpression exp, PXGraph graph, BqlCommandInfo info, BqlCommand.Selection selection)
			=> where.AppendExpression(ref exp, graph, info, selection);

		public void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
			=> where.Verify(cache, item, pars, ref result, ref value);

		public static bool IsDocumentSchedulable(PXGraph graph, APRegister document)
		{
			bool? result = null;
			object value = null;

			new IsSchedulable<TRegister>().Verify(
				graph.Caches[typeof(TRegister)],
				document,
				new List<object>(),
				ref result,
				ref value);

			return result == true;
		}

		public static void Ensure(PXGraph graph, APRegister document)
		{
			if (!IsDocumentSchedulable(graph, document))
			{
				throw new PXException(Messages.DocumentCannotBeScheduled);
			}
		}
	}
}
