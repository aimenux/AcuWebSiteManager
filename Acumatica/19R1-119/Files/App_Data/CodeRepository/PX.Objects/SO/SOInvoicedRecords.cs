using PX.Data;
using PX.Objects.AR;
using PX.Objects.IN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.SO
{
	public class SOInvoicedRecords
	{
		protected IEqualityComparer<ARTran> comparer;
		private Dictionary<ARTran, Record> records;

		public SOInvoicedRecords(IEqualityComparer<ARTran> comparer)
		{
			if (comparer == null)
				throw new ArgumentNullException();

			this.comparer = comparer;
			this.records = new Dictionary<ARTran, Record>(comparer);
		}
		
		public void Add(PXResult<ARTran, InventoryItem, SOLine, INTran, INTranSplit, INLotSerialStatus, SOSalesPerTran> res)
		{
			InventoryItem invItem = res;
			INTranSplit split = res;
			INTran tran = res;
			SOLine line = res;
			ARTran artran = res;
			INLotSerialStatus lotStatus = res;
			SOSalesPerTran sptran = res;

			Record item = null;
			if (!records.TryGetValue(artran, out item))
			{
				item = new Record(artran, line, invItem, sptran);
				if (tran.LineNbr != null)
				{
					INTransaction intran = new INTransaction(tran);
					intran.Splits.Add(new Tuple<INTranSplit, bool>(split, string.IsNullOrEmpty(lotStatus.LotSerialNbr)));
					item.Transactions.Add(tran.LineNbr.Value, intran);
				}
								
				records.Add(artran, item);
			}
			else
			{
				if (tran.LineNbr != null)
				{
					INTransaction intran = null;
					if ( !item.Transactions.TryGetValue(tran.LineNbr.Value, out intran) )
					{
						intran = new INTransaction(tran);
						item.Transactions.Add(tran.LineNbr.Value, intran);
					}
										
					intran.Splits.Add(new Tuple<INTranSplit, bool>(split, string.IsNullOrEmpty(lotStatus.LotSerialNbr)));					
				}
			}
		}

		public Dictionary<ARTran, Record>.ValueCollection Records
		{
			get
			{
				return records.Values;
			}
		}

		public class INTransaction
		{
			public INTran Transaction { get; set; }

			/// <summary>
			/// Split, IsAvailable
			/// </summary>
			public List<Tuple<INTranSplit, bool>> Splits { get; private set; }

			public INTransaction(INTran tran)
			{
				this.Transaction = tran;
				this.Splits = new List<Tuple<INTranSplit, bool>>();
			}
		}

		public class Record
		{
			public ARTran ARTran { get; private set; }
			public SOLine SOLine { get; private set; }
			public InventoryItem Item { get; private set; }
			public SOSalesPerTran SalesPerTran { get; private set; }
			public Dictionary<int, INTransaction> Transactions { get; private set; }

			public Record(ARTran artran, SOLine soline, InventoryItem item, SOSalesPerTran salesPerTran)
			{
				this.ARTran = artran;
				this.SOLine = soline;
				this.Item = item;
				this.SalesPerTran = salesPerTran;
				this.Transactions = new Dictionary<int, INTransaction>();  
			}
		}
	}

}
