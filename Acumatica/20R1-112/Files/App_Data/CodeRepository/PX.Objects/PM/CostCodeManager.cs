using PX.Data;
using PX.Objects.CS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PM
{
	public class CostCodeManager : ICostCodeManager
	{			
		private class DefaultCostCodeDefinition : IPrefetchable
		{
			public const string SLOT_KEY = "DefaultCostCodeDefinition";

			public static Type[] DependentTables
			{
				get
				{
					return new[] { typeof(PMCostCode) };
				}
			}

			
			public int? DefaultCostCodeID { get; private set; }

			public DefaultCostCodeDefinition()
			{				
			}

			public void Prefetch()
			{
				foreach (PXDataRecord record in PXDatabase.SelectMulti<PMCostCode>(
					new PXDataField<PMCostCode.costCodeID>(),
					new PXDataFieldValue<PMCostCode.isDefault>(true, PXComp.EQ)))
				{
					DefaultCostCodeID = record.GetInt32(0).GetValueOrDefault();
				}
			}
		}
				
		private DefaultCostCodeDefinition DefaultDefinition
		{
			get
			{
				return PXDatabase.GetSlot<DefaultCostCodeDefinition>(DefaultCostCodeDefinition.SLOT_KEY, DefaultCostCodeDefinition.DependentTables);
			}
		}

		public int? DefaultCostCodeID
		{
			get
			{
				return DefaultDefinition.DefaultCostCodeID;
			}
		}
	}

	public interface ICostCodeManager
	{
		int? DefaultCostCodeID { get;  }
	}
}
