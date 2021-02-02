using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.PM;
using System;
using System.Collections.Generic;

namespace PX.Objects.PR
{
	public class AnnualizationExceptionAttribute : PXEventSubscriberAttribute, IPXFieldSelectingSubscriber
	{
		private bool _UseWageAbovePrevailingAnnualizationException;
		private Type _DefinitionType = null;
		private Type _DefinitionAnnualizationExceptionField = null;
		private Type[] _DefinitionFields = null;
		private Type[] _MatchFields = null;
		private Type _ProjectIDField = null;
		private PXView _DefinitionSelectView = null;

		public AnnualizationExceptionAttribute(
			Type definitionType,
			Type definitionAnnualizationExceptionField,
			Type[] definitionFields,
			Type[] matchFields)
		{
			_DefinitionType = definitionType;
			_DefinitionAnnualizationExceptionField = definitionAnnualizationExceptionField;
			_DefinitionFields = definitionFields;
			_MatchFields = matchFields;
			_UseWageAbovePrevailingAnnualizationException = false;
		}

		public AnnualizationExceptionAttribute(Type projectIDField)
		{
			_ProjectIDField = projectIDField;
			_UseWageAbovePrevailingAnnualizationException = true;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			if (_UseWageAbovePrevailingAnnualizationException)
			{
				return;
			}

			BqlCommand command = BqlCommand.CreateInstance(BqlCommand.Compose(typeof(Select<>), _DefinitionType));
			for (int i = 0; i < _DefinitionFields.Length && i < _MatchFields.Length; i++)
			{
				command = command.WhereAnd(BqlCommand.Compose(
					typeof(Where<,>),
					_DefinitionFields[i],
					typeof(Equal<>),
					typeof(Current<>),
					_MatchFields[i]));
			}

			_DefinitionSelectView = new PXView(sender.Graph, true, command);
		}

		public void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (e.Row == null)
			{
				return;
			}

			if (_UseWageAbovePrevailingAnnualizationException)
			{
				PMProject project = new SelectFrom<PMProject>.Where<PMProject.contractID.IsEqual<P.AsInt>>.View(sender.Graph)
					.SelectSingle(sender.GetValue(e.Row, _ProjectIDField.Name));
				if (project != null)
				{
					e.ReturnValue = sender.Graph.Caches[typeof(PMProject)].GetExtension<PMProjectExtension>(project).WageAbovePrevailingAnnualizationException;
				}
			}
			else
			{
				object definitionRecord = _DefinitionSelectView.SelectSingleBound(new object[] { e.Row });
				e.ReturnValue = sender.Graph.Caches[_DefinitionType].GetValue(definitionRecord, _DefinitionAnnualizationExceptionField.Name);
			}
		}
	}
}
