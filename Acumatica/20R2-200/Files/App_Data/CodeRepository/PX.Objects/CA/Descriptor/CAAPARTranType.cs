using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.Common;
using PX.Objects.EP.Descriptor;
using PX.Objects.GL;

namespace PX.Objects.CA
{
	public class CAAPARTranType : ILabelProvider
	{
		public IEnumerable<ValueLabelPair> ValueLabelPairs => listAll;

		protected static readonly ValueLabelPair.KeyComparer Comparer = new ValueLabelPair.KeyComparer();

		protected static readonly ValueLabelList listGL = new ValueLabelList
		{
			new GLTranType().ValueLabelPairs
		};

		protected static readonly ValueLabelList listAR = new ValueLabelList
		{
			new ARDocType().ValueLabelPairs,
			listGL
		};

		protected static readonly ValueLabelList restrictedListAR = new ValueLabelList
		{
			{ARDocType.Invoice,     AR.Messages.Invoice     },
			{ARDocType.CashSale,    AR.Messages.CashSale    },
			{ARDocType.CreditMemo,  AR.Messages.CreditMemo  },
			{ARDocType.DebitMemo,   AR.Messages.DebitMemo   },
			{ARDocType.Payment,     AR.Messages.Payment     },
			{ARDocType.Prepayment,  AR.Messages.Prepayment  }
		};

		protected static readonly ValueLabelList listAP = new ValueLabelList
		{
			new APDocType().ValueLabelPairs,
			listGL,
			{CATranType.CABatch,    AP.Messages.APBatch }
		};

		protected static readonly ValueLabelList restrictedListAP = new ValueLabelList
		{
			{APDocType.Invoice,     AP.Messages.Invoice     },
			{APDocType.QuickCheck,  AP.Messages.QuickCheck  },
			{APDocType.Check,       AP.Messages.Check       },
			{APDocType.CreditAdj,   AP.Messages.CreditAdj   },
			{APDocType.DebitAdj,    AP.Messages.DebitAdj    },
			{APDocType.Prepayment,  AP.Messages.Prepayment  }
		};

		protected static readonly ValueLabelList restrictedListARAPIntersect = new ValueLabelList
		{
			{ARDocType.Invoice,     CA.Messages.ARAPInvoice },
			{ARDocType.Prepayment,  CA.Messages.ARAPPrepayment}
		};

		protected static readonly ValueLabelList listARAPIntersect = new ValueLabelList
		{
			restrictedListARAPIntersect,
			{ARDocType.Refund,      CA.Messages.ARAPRefund  },
            {ARDocType.VoidRefund,  CA.Messages.ARAPVoidRefund}
        };

		protected static readonly ValueLabelList listCA = new ValueLabelList
		{
			new CATranType().FullValueLabelPairs,
			listGL
		};

		protected static readonly ValueLabelList restrictedListCA = new ValueLabelList
		{
			{CATranType.CAAdjustment,CA.Messages.CAAdjustment}
		};

		protected static readonly ValueLabelList listAll = new ValueLabelList
		{
			new CATranType().FullValueLabelPairs,
			new GLTranType().ValueLabelPairs,
			{ CATranType.CABatch,	AP.Messages.APBatch } ,
			new APDocType().ValueLabelPairs.Except(listARAPIntersect, Comparer),
			new ARDocType().ValueLabelPairs.Except(listARAPIntersect, Comparer),
			listARAPIntersect,
			new EPDocumentType().ValueLabelList
		};

		protected static readonly ValueLabelList restrictedListAll = new ValueLabelList
		{
			listGL,
			restrictedListCA,
			restrictedListAP.Except(restrictedListARAPIntersect, Comparer),
			restrictedListAR.Except(restrictedListARAPIntersect, Comparer),
			restrictedListARAPIntersect
		};

		public class ListByModuleRestrictedAttribute : ListByModuleAttribute
		{
			public ListByModuleRestrictedAttribute(Type moduleField)
				: base(moduleField, restrictedListAR, restrictedListAP, restrictedListCA, listGL, restrictedListAll)
			{ }
		}
		public class ListByModuleAttribute : LabelListAttribute
		{
			protected ValueLabelList _listAR;
			protected ValueLabelList _listAP;
			protected ValueLabelList _listCA;
			protected ValueLabelList _listGL;
			protected ValueLabelList _listAll;

			protected Type _ModuleField;
			protected string lastModule;
			protected ListByModuleAttribute(Type moduleField, ValueLabelList listAR, ValueLabelList listAP, ValueLabelList listCA,
				ValueLabelList listGL, ValueLabelList listAll) : base(listAll)
			{
				_ModuleField = moduleField;
				_listAR = listAR;
				_listAP = listAP;
				_listCA = listCA;
				_listGL = listGL;
				_listAll = listAll;
			}
			public ListByModuleAttribute(Type moduleField)
				: this(moduleField, listAR, listAP, listCA,	listGL, listAll)
			{ }

			public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
			{
				string module = (string)sender.GetValue(e.Row, _ModuleField.Name);
				if (module != lastModule)
				{
					if (sender.Graph.GetType() == typeof(PXGraph)) //report mode
					{
						SetNewList(sender, _listAll);
					}
					else
					switch (module)
					{
						case GL.BatchModule.AP:
							SetNewList(sender, _listAP);
							break;
						case GL.BatchModule.AR:
							SetNewList(sender, _listAR);
							break;
						case GL.BatchModule.CA:
							SetNewList(sender, _listCA);
							break;
						case GL.BatchModule.GL:
							SetNewList(sender, _listGL);
							break;
						default:
							SetNewList(sender, _listAll);
							break;
					}
				lastModule = module;
					}
					base.FieldSelecting(sender, e);
				}


			protected void SetNewList(PXCache sender, ValueLabelList valueLabelList)
			{
				List<ListByModuleAttribute> list = new List<ListByModuleAttribute>();
				list.Add(this);
				SetListInternal(list, valueLabelList.Select(pair => pair.Value).ToArray(), valueLabelList.Select(pair => pair.Label).ToArray(), sender);
			}

			protected override void TryLocalize(PXCache sender)
			{
				base.TryLocalize(sender);
				RipDynamicLabels(_listAP.Select(pair => pair.Label).ToArray(), sender);
				RipDynamicLabels(_listAR.Select(pair => pair.Label).ToArray(), sender);
				RipDynamicLabels(_listCA.Select(pair => pair.Label).ToArray(), sender);
				RipDynamicLabels(_listGL.Select(pair => pair.Label).ToArray(), sender);
			}

		}
	}
}