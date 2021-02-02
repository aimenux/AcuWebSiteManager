using System;
using System.Linq;
using PX.Data;
using PX.Objects.CA;
using PX.Objects.Common;
using PX.Objects.CS;

namespace PX.Objects.PR
{
	public class PRxCATran : PXCacheExtension<CATran>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.payrollModule>();
		}

		#region OrigTranType
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDBString(3, IsFixed = true)]
		[PXRemoveBaseAttribute(typeof(CAAPARTranType.ListByModuleAttribute))]
		[PRCATranType.ListByModule(typeof(CATran.origModule))]
		public virtual string OrigTranType { get; set; }
		#endregion
	}

	public class PRCATranType
	{
		public class ListByModuleAttribute : CAAPARTranType.ListByModuleAttribute
		{
			private readonly ValueLabelList _listPRRestricted;
			private readonly ValueLabelList _listPRFull;

			public ListByModuleAttribute(Type moduleField) : base(moduleField)
			{
				_listPRRestricted = new ValueLabelList
				{
					{PayrollType.Regular, Messages.Regular},
					{PayrollType.Special, Messages.Special},
					{PayrollType.Adjustment, Messages.Adjustment}
				};

				_listPRFull = new ValueLabelList(_listPRRestricted) {{PayrollType.VoidCheck, Messages.VoidCheck}};
				_listAll = new ValueLabelList(_listAll) {_listPRRestricted};
			}

			public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
			{
				string module = (string)sender.GetValue(e.Row, _ModuleField.Name);
				if (module == GL.BatchModule.PR && module != lastModule && sender.Graph.GetType() != typeof(PXGraph))
				{
					SetNewList(sender, _listPRFull);
					lastModule = module;
				}

				base.FieldSelecting(sender, e);
			}

			protected override void TryLocalize(PXCache sender)
			{
				base.TryLocalize(sender);
				RipDynamicLabels(_listPRFull.Select(pair => pair.Label).ToArray(), sender);
			}
		}
	}
}
