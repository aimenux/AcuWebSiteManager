using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using PX.Common;
using PX.Data;
using PX.Data.SQLTree;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.GL;
using System.Linq;
using PX.Objects.PM;
using PX.SM;
using PX.CS;
using PX.CS.Contracts.Interfaces;
using PX.Reports.ARm;
using PX.Objects.GL.DAC;
using PX.Objects.GDPR;

namespace PX.Objects.CS
{
	public class PXDataUtils
	{
		public static string FieldName<Field>()
			where Field : IBqlField
		{
			string ret = typeof(Field).Name;

			if (string.IsNullOrEmpty(ret) == false)
			{
				return char.ToUpper(ret[0]) + ret.Substring(1);
			}
			return ret;
		}
	}

    public class Left<V1, V2> : IBqlFunction, IBqlOperand
        where V1 : IBqlOperand
        where V2 : IBqlOperand
    {
        IBqlCreator _formula = new Substring<V1, int1, V2>();

		public bool AppendExpression(ref SQLExpression exp, PXGraph graph, BqlCommandInfo info, BqlCommand.Selection selection)
			=> _formula.AppendExpression(ref exp, graph, info, selection);

        public void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
        {
            _formula.Verify(cache, item, pars, ref result, ref value);
        }

		#region IBqlFunction Members

		public void GetAggregates(List<IBqlFunction> fields)
		{
			throw new NotImplementedException();
		}

		public Type GetField()
		{
			if (!typeof(IBqlCreator).IsAssignableFrom(typeof(V1)))
			{
				return typeof(V1);
			}
			return null;
		}

	    public bool IsGroupBy()
	    {
			return false;
	    }

	    public string GetFunction()
		{
			throw new NotImplementedException();
		}

		#endregion
	}

    public class Left4<V1> : IBqlCreator
        where V1 : IBqlOperand
    {
        IBqlCreator _formula = new Substring<V1, int1, int4>();

		public bool AppendExpression(ref SQLExpression exp, PXGraph graph, BqlCommandInfo info, BqlCommand.Selection selection)
			=> _formula.AppendExpression(ref exp, graph, info, selection);

        public void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
        {
            _formula.Verify(cache, item, pars, ref result, ref value);
        }
    }

	public class DefaultValue<Field> : IBqlCreator, IBqlOperand
		where Field : IBqlField
	{
		protected object _DefaultValue = PXCache.NotSetValue;

		#region IBqlCreator Members

		public bool AppendExpression(ref SQLExpression exp, PXGraph graph, BqlCommandInfo info, BqlCommand.Selection selection)
		{
			if (_DefaultValue == PXCache.NotSetValue && graph != null) {
				PXCache cache = graph.Caches[BqlCommand.GetItemType(typeof(Field))];
				cache.RaiseFieldDefaulting<Field>(null, out _DefaultValue);
				PXDefaultAttribute.SetDefault<Field>(cache, null);
			}

			return true;
		}

		public void Verify(PXCache cache, object item, System.Collections.Generic.List<object> pars, ref bool? result, ref object value)
		{
			if (_DefaultValue == PXCache.NotSetValue)
			{
				PXCache c = cache.Graph.Caches[BqlCommand.GetItemType(typeof (Field))];
				c.RaiseFieldDefaulting<Field>(null, out _DefaultValue);
				PXDefaultAttribute.SetDefault<Field>(c, null);
			}
			if(_DefaultValue != PXCache.NotSetValue)
				value = _DefaultValue;
		}

		#endregion
	}

	public class PXRestrictionAttribute : PXEventSubscriberAttribute, IPXCommandPreparingSubscriber
	{
		public PXRestrictionAttribute()
		{
		}

		public virtual void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			e.IsRestriction = true;
		}
	}

	public class MultDiv
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(new string[] { Multiply, Divide }, new string[] { Messages.Multiply, Messages.Divide }) { ;}
		}

		public const string Multiply = "M";
		public const string Divide = "D";

		public class multiply : PX.Data.BQL.BqlString.Constant<multiply>
		{
			public multiply()
				: base(Multiply) { ; }
		}

		public class divide : PX.Data.BQL.BqlString.Constant<divide>
		{
			public divide()
				: base(Divide) { ; }
		}
	}

	public class DocumentList<T0, T1> : DocumentListBase<PXResult<T0, T1>>
		where T0 : class, IBqlTable, new()
		where T1 : class, IBqlTable, new()
	{
		public DocumentList(PXGraph Graph)
			: base(Graph)
		{
		}

		public virtual void Add(T0 item0, T1 item1)
		{
			Add(new PXResult<T0, T1>(item0, item1));
		}

		protected override object GetValue(object data, Type field)
		{
			Type SourceType = BqlCommand.GetItemType(field);

			if (SourceType.IsAssignableFrom(typeof(T0)))
			{
				SourceType = typeof(T0);
			}
			if (SourceType.IsAssignableFrom(typeof(T1)))
			{
				SourceType = typeof(T1);
			}

			return _Graph.Caches[SourceType].GetValue(((PXResult)data)[SourceType], field.Name);
		}

		public override PXResult<T0, T1> Find(object item)
		{
			if (item is T0)
			{
				PXCache cache = _Graph.Caches[typeof(T0)];

				return this.Find(delegate(PXResult<T0, T1> data)
				{
					return cache.ObjectsEqual((T0)data, item);
				});
			}

			if (item is T1)
			{
				PXCache cache = _Graph.Caches[typeof(T1)];

				return this.Find(delegate(PXResult<T0, T1> data)
				{
					return cache.ObjectsEqual((T1)data, item);
				});
			}

			throw new PXArgumentException();
		}
	}

	public class DocumentList<T> : DocumentListBase<T>
		where T : class, IBqlTable
	{
        public bool Consolidate = true;

		public DocumentList(PXGraph Graph)
			: base(Graph)
		{
		}

		protected override object GetValue(object data, Type field)
		{
			return Consolidate ? _Graph.Caches[typeof(T)].GetValue(data, field.Name) : null;
		}

		public override T Find(object item)
		{
			PXCache cache = _Graph.Caches[typeof(T)];

			return this.Find(delegate(T data)
			{
				return cache.ObjectsEqual(data, item);
			});
		}
	}

	public class FieldLookup
	{
		protected Type _type;
		protected object _value;

		public Type Field
		{
			get { return _type; }
		}
		public object Value
		{
			get { return _value; }
		}

		protected FieldLookup(Type type, object value)
		{
			_type = type;
			_value = value;
		}
	}

	public class FieldLookup<TField> : FieldLookup
		where TField : IBqlField
	{
		public FieldLookup(object value)
			: base(typeof(TField), value)
		{
		}
	}

	public abstract class DocumentListBase<T> : List<T>
		where T : class
	{
		#region State
		protected PXGraph _Graph;
		#endregion
		#region Ctor
		public DocumentListBase(PXGraph Graph)
		{
			_Graph = Graph;
		}
		#endregion
		#region Implementation

		protected abstract object GetValue(object data, Type field);

		protected virtual object GetValue<Field>(object data)
			where Field : IBqlField
		{
			return GetValue(data, typeof(Field));
		}

		public abstract T Find(object item);

		public new int IndexOf(T item)
		{
			T existing = this.Find(item);
			return base.IndexOf(existing);
		}

		public T Find<Field1>(params object[] values)
			where Field1 : IBqlField
		{
			return this.Find(delegate(T data)
			{
				return object.Equals(GetValue<Field1>(data), values[0]);
			});
		}

		public T Find<Field1, Field2>(params object[] values)
			where Field1 : IBqlField
			where Field2 : IBqlField
		{
			return this.Find(delegate(T data)
			{
				return object.Equals(GetValue<Field1>(data), values[0]) && object.Equals(GetValue<Field2>(data), values[1]);
			});
		}

		public T Find<Field1, Field2, Field3>(params object[] values)
			where Field1 : IBqlField
			where Field2 : IBqlField
			where Field3 : IBqlField
		{
			return this.Find(delegate(T data)
			{
				return object.Equals(GetValue<Field1>(data), values[0]) && object.Equals(GetValue<Field2>(data), values[1]) && object.Equals(GetValue<Field3>(data), values[2]);
			});
		}

		public T Find<Field1, Field2, Field3, Field4>(params object[] values)
			where Field1 : IBqlField
			where Field2 : IBqlField
			where Field3 : IBqlField
			where Field4 : IBqlField
		{
			return this.Find(delegate(T data)
			{
				return object.Equals(GetValue<Field1>(data), values[0]) && object.Equals(GetValue<Field2>(data), values[1]) && object.Equals(GetValue<Field3>(data), values[2]) && object.Equals(GetValue<Field4>(data), values[3]);
			});
		}

		public T Find<Field1, Field2, Field3, Field4, Field5>(params object[] values)
			where Field1 : IBqlField
			where Field2 : IBqlField
			where Field3 : IBqlField
			where Field4 : IBqlField
			where Field5 : IBqlField
		{
			return this.Find(delegate(T data)
			{
				return object.Equals(GetValue<Field1>(data), values[0]) && object.Equals(GetValue<Field2>(data), values[1]) && object.Equals(GetValue<Field3>(data), values[2]) && object.Equals(GetValue<Field4>(data), values[3]) && object.Equals(GetValue<Field5>(data), values[4]);
			});
		}

		public T Find<Field1, Field2, Field3, Field4, Field5, Field6>(params object[] values)
			where Field1 : IBqlField
			where Field2 : IBqlField
			where Field3 : IBqlField
			where Field4 : IBqlField
			where Field5 : IBqlField
			where Field6 : IBqlField
		{
			return this.Find(delegate(T data)
			{
				return object.Equals(GetValue<Field1>(data), values[0]) && object.Equals(GetValue<Field2>(data), values[1]) && object.Equals(GetValue<Field3>(data), values[2]) && object.Equals(GetValue<Field4>(data), values[3]) && object.Equals(GetValue<Field5>(data), values[4]) && object.Equals(GetValue<Field6>(data), values[5]);
			});
		}

		public T Find<Field1, Field2, Field3, Field4, Field5, Field6, Field7>(params object[] values)
			where Field1 : IBqlField
			where Field2 : IBqlField
			where Field3 : IBqlField
			where Field4 : IBqlField
			where Field5 : IBqlField
			where Field6 : IBqlField
			where Field7 : IBqlField
		{
			return this.Find(delegate(T data)
			{
				return object.Equals(GetValue<Field1>(data), values[0]) && object.Equals(GetValue<Field2>(data), values[1]) && object.Equals(GetValue<Field3>(data), values[2]) && object.Equals(GetValue<Field4>(data), values[3]) && object.Equals(GetValue<Field5>(data), values[4]) && object.Equals(GetValue<Field6>(data), values[5]) && object.Equals(GetValue<Field7>(data), values[6]);
			});
		}

		public T Find<Field1, Field2, Field3, Field4, Field5, Field6, Field7, Field8>(params object[] values)
			where Field1 : IBqlField
			where Field2 : IBqlField
			where Field3 : IBqlField
			where Field4 : IBqlField
			where Field5 : IBqlField
			where Field6 : IBqlField
			where Field7 : IBqlField
			where Field8 : IBqlField
		{
			return this.Find(delegate(T data)
			{
				return object.Equals(GetValue<Field1>(data), values[0]) && object.Equals(GetValue<Field2>(data), values[1]) && object.Equals(GetValue<Field3>(data), values[2]) && object.Equals(GetValue<Field4>(data), values[3]) && object.Equals(GetValue<Field5>(data), values[4]) && object.Equals(GetValue<Field6>(data), values[5]) && object.Equals(GetValue<Field7>(data), values[6]) && object.Equals(GetValue<Field8>(data), values[7]);
			});
		}

		public T Find<Field1, Field2, Field3, Field4, Field5, Field6, Field7, Field8, Field9>(params object[] values)
			where Field1 : IBqlField
			where Field2 : IBqlField
			where Field3 : IBqlField
			where Field4 : IBqlField
			where Field5 : IBqlField
			where Field6 : IBqlField
			where Field7 : IBqlField
			where Field8 : IBqlField
			where Field9 : IBqlField
		{
			return this.Find(delegate(T data)
			{
				return object.Equals(GetValue<Field1>(data), values[0]) && object.Equals(GetValue<Field2>(data), values[1]) && object.Equals(GetValue<Field3>(data), values[2]) && object.Equals(GetValue<Field4>(data), values[3]) && object.Equals(GetValue<Field5>(data), values[4]) && object.Equals(GetValue<Field6>(data), values[5]) && object.Equals(GetValue<Field7>(data), values[6]) && object.Equals(GetValue<Field8>(data), values[7]) && object.Equals(GetValue<Field9>(data), values[8]);
			});
		}

		public T Find<Field1, Field2, Field3, Field4, Field5, Field6, Field7, Field8, Field9, Field10>(params object[] values)
			where Field1 : IBqlField
			where Field2 : IBqlField
			where Field3 : IBqlField
			where Field4 : IBqlField
			where Field5 : IBqlField
			where Field6 : IBqlField
			where Field7 : IBqlField
			where Field8 : IBqlField
			where Field9 : IBqlField
			where Field10 : IBqlField
		{
			return this.Find(delegate(T data)
			{
				return object.Equals(GetValue<Field1>(data), values[0]) && object.Equals(GetValue<Field2>(data), values[1]) && object.Equals(GetValue<Field3>(data), values[2]) && object.Equals(GetValue<Field4>(data), values[3]) && object.Equals(GetValue<Field5>(data), values[4]) && object.Equals(GetValue<Field6>(data), values[5]) && object.Equals(GetValue<Field7>(data), values[6]) && object.Equals(GetValue<Field8>(data), values[7]) && object.Equals(GetValue<Field9>(data), values[8]) && object.Equals(GetValue<Field10>(data), values[9]);
			});
		}
		public T Find<Field1, Field2, Field3, Field4, Field5, Field6, Field7, Field8, Field9, Field10, Field11>(params object[] values)
			where Field1 : IBqlField
			where Field2 : IBqlField
			where Field3 : IBqlField
			where Field4 : IBqlField
			where Field5 : IBqlField
			where Field6 : IBqlField
			where Field7 : IBqlField
			where Field8 : IBqlField
			where Field9 : IBqlField
			where Field10 : IBqlField
			where Field11 : IBqlField
		{
			return this.Find(delegate(T data)
			{
				return object.Equals(GetValue<Field1>(data), values[0]) &&
					object.Equals(GetValue<Field2>(data), values[1]) &&
					object.Equals(GetValue<Field3>(data), values[2]) &&
					object.Equals(GetValue<Field4>(data), values[3]) &&
					object.Equals(GetValue<Field5>(data), values[4]) &&
					object.Equals(GetValue<Field6>(data), values[5]) &&
					object.Equals(GetValue<Field7>(data), values[6]) &&
					object.Equals(GetValue<Field8>(data), values[7]) &&
					object.Equals(GetValue<Field9>(data), values[8]) &&
					object.Equals(GetValue<Field10>(data), values[9]) &&
					object.Equals(GetValue<Field11>(data), values[10]);
			});
		}

		public T Find<Field1, Field2, Field3, Field4, Field5, Field6, Field7, Field8, Field9, Field10, Field11, Field12>(params object[] values)
			where Field1 : IBqlField
			where Field2 : IBqlField
			where Field3 : IBqlField
			where Field4 : IBqlField
			where Field5 : IBqlField
			where Field6 : IBqlField
			where Field7 : IBqlField
			where Field8 : IBqlField
			where Field9 : IBqlField
			where Field10 : IBqlField
			where Field11 : IBqlField
			where Field12 : IBqlField
		{
			return this.Find(delegate(T data)
			{
				return object.Equals(GetValue<Field1>(data), values[0]) &&
					object.Equals(GetValue<Field2>(data), values[1]) &&
					object.Equals(GetValue<Field3>(data), values[2]) &&
					object.Equals(GetValue<Field4>(data), values[3]) &&
					object.Equals(GetValue<Field5>(data), values[4]) &&
					object.Equals(GetValue<Field6>(data), values[5]) &&
					object.Equals(GetValue<Field7>(data), values[6]) &&
					object.Equals(GetValue<Field8>(data), values[7]) &&
					object.Equals(GetValue<Field9>(data), values[8]) &&
					object.Equals(GetValue<Field10>(data), values[9]) &&
					object.Equals(GetValue<Field11>(data), values[10]) &&
					object.Equals(GetValue<Field12>(data), values[11]);
			});
		}
		public T Find<Field1, Field2, Field3, Field4, Field5, Field6, Field7, Field8, Field9, Field10, Field11, Field12, Field13>(params object[] values)
			where Field1 : IBqlField
			where Field2 : IBqlField
			where Field3 : IBqlField
			where Field4 : IBqlField
			where Field5 : IBqlField
			where Field6 : IBqlField
			where Field7 : IBqlField
			where Field8 : IBqlField
			where Field9 : IBqlField
			where Field10 : IBqlField
			where Field11 : IBqlField
			where Field12 : IBqlField
			where Field13 : IBqlField
		{
			return this.Find(delegate(T data)
			{
				return object.Equals(GetValue<Field1>(data), values[0]) &&
					object.Equals(GetValue<Field2>(data), values[1]) &&
					object.Equals(GetValue<Field3>(data), values[2]) &&
					object.Equals(GetValue<Field4>(data), values[3]) &&
					object.Equals(GetValue<Field5>(data), values[4]) &&
					object.Equals(GetValue<Field6>(data), values[5]) &&
					object.Equals(GetValue<Field7>(data), values[6]) &&
					object.Equals(GetValue<Field8>(data), values[7]) &&
					object.Equals(GetValue<Field9>(data), values[8]) &&
					object.Equals(GetValue<Field10>(data), values[9]) &&
					object.Equals(GetValue<Field11>(data), values[10]) &&
					object.Equals(GetValue<Field12>(data), values[11]) &&
					object.Equals(GetValue<Field13>(data), values[12]);
			});
		}
		public T Find<Field1, Field2, Field3, Field4, Field5, Field6, Field7, Field8, Field9, Field10, Field11, Field12, Field13, Field14>(params object[] values)
			where Field1 : IBqlField
			where Field2 : IBqlField
			where Field3 : IBqlField
			where Field4 : IBqlField
			where Field5 : IBqlField
			where Field6 : IBqlField
			where Field7 : IBqlField
			where Field8 : IBqlField
			where Field9 : IBqlField
			where Field10 : IBqlField
			where Field11 : IBqlField
			where Field12 : IBqlField
			where Field13 : IBqlField
			where Field14 : IBqlField
		{
			return this.Find(delegate(T data)
			{
				return object.Equals(GetValue<Field1>(data), values[0]) &&
					object.Equals(GetValue<Field2>(data), values[1]) &&
					object.Equals(GetValue<Field3>(data), values[2]) &&
					object.Equals(GetValue<Field4>(data), values[3]) &&
					object.Equals(GetValue<Field5>(data), values[4]) &&
					object.Equals(GetValue<Field6>(data), values[5]) &&
					object.Equals(GetValue<Field7>(data), values[6]) &&
					object.Equals(GetValue<Field8>(data), values[7]) &&
					object.Equals(GetValue<Field9>(data), values[8]) &&
					object.Equals(GetValue<Field10>(data), values[9]) &&
					object.Equals(GetValue<Field11>(data), values[10]) &&
					object.Equals(GetValue<Field12>(data), values[11]) &&
					object.Equals(GetValue<Field13>(data), values[12]) &&
					object.Equals(GetValue<Field14>(data), values[13]);
			});
		}
		public T Find<Field1, Field2, Field3, Field4, Field5, Field6, Field7, Field8, Field9, Field10, Field11, Field12, Field13, Field14, Field15>(params object[] values)
			where Field1 : IBqlField
			where Field2 : IBqlField
			where Field3 : IBqlField
			where Field4 : IBqlField
			where Field5 : IBqlField
			where Field6 : IBqlField
			where Field7 : IBqlField
			where Field8 : IBqlField
			where Field9 : IBqlField
			where Field10 : IBqlField
			where Field11 : IBqlField
			where Field12 : IBqlField
			where Field13 : IBqlField
			where Field14 : IBqlField
			where Field15 : IBqlField
		{
			return this.Find(delegate(T data)
			{
				return object.Equals(GetValue<Field1>(data), values[0]) &&
					object.Equals(GetValue<Field2>(data), values[1]) &&
					object.Equals(GetValue<Field3>(data), values[2]) &&
					object.Equals(GetValue<Field4>(data), values[3]) &&
					object.Equals(GetValue<Field5>(data), values[4]) &&
					object.Equals(GetValue<Field6>(data), values[5]) &&
					object.Equals(GetValue<Field7>(data), values[6]) &&
					object.Equals(GetValue<Field8>(data), values[7]) &&
					object.Equals(GetValue<Field9>(data), values[8]) &&
					object.Equals(GetValue<Field10>(data), values[9]) &&
					object.Equals(GetValue<Field11>(data), values[10]) &&
					object.Equals(GetValue<Field12>(data), values[11]) &&
					object.Equals(GetValue<Field13>(data), values[12]) &&
					object.Equals(GetValue<Field14>(data), values[13]) &&
					object.Equals(GetValue<Field15>(data), values[14]);
			});
		}

		public T Find(params FieldLookup[] values)
		{
			return this.Find(data
				=> values.All(fld => object.Equals(GetValue(data, fld.Field), fld.Value)));
		}

		#endregion
	}


	#region AddressAttribute
	public interface IValidatedAddress
	{
		Boolean? IsValidated { get; set; }
	}

	public interface IAddress: IAddressBase, IValidatedAddress, INotable
	{
		int? AddressID { get; set;}
		int? BAccountID { get; set; }
		int? BAccountAddressID { get; set; }
		int? RevisionID { get; set; }
		bool? IsDefaultAddress { get; set; }
#if false
		string AddressLine1 { get; set;}
		string AddressLine2 { get; set;}
		string AddressLine3 { get; set;}
		string City { get; set;}
		string CountryID { get; set;}
		string State { get; set;}
		string PostalCode { get; set;}
		bool? IsValidated { get; set; } 
#endif
	}

	public abstract class AddressAttribute : SharedRecordAttribute
	{
		#region State
		protected Dictionary<object, bool> _canceled;
		#endregion
		#region Ctor
		public AddressAttribute(Type AddressIDType, Type IsDefaultAddressType, Type SelectType)
			:base(AddressIDType, IsDefaultAddressType, SelectType)
		{
		}
		#endregion
		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.RowUpdating.AddHandler(_RecordType, Record_RowUpdating);
			sender.Graph.RowInserting.AddHandler(_RecordType, Record_RowInserting);
			_canceled = new Dictionary<object, bool>();
		}
		#endregion
		#region Implementation
		protected virtual void Record_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			if (e.ExternalCall && !PXContext.GetSlot<bool>("ForceInternalCall"))
			{
				e.Cancel = true;
			}
		}

		public virtual void Record_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			if (_canceled.ContainsKey(e.NewRow))
			{
				//avoid updating of shared record, this will cause Another Process Updated in case both addresses in SO were pointing to one record and both were overriden simultaneously.
				e.Cancel = true;
				if (sender.GetStatus(e.NewRow) == PXEntryStatus.Updated)
				{
					sender.SetStatus(e.NewRow, PXEntryStatus.Notchanged);
				}
			}

			object key = sender.GetValue(e.NewRow, _RecordID);
			object isdefault = sender.GetValue(e.NewRow, _IsDefault);
			if (Convert.ToInt32(key) > 0 && Convert.ToBoolean(isdefault) == true)
			{
				if (sender.GetStatus(e.NewRow) == PXEntryStatus.Updated)
				{
					sender.SetStatus(e.NewRow, PXEntryStatus.Notchanged);
				}
			}
		}

		protected override void Record_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			base.Record_RowUpdated(sender, e);

			if ((bool?)sender.GetValue(e.Row, _IsDefault) == true)
			{
				//clear errorvalues committed from gui for "default" record
				PXUIFieldAttribute.SetError(sender, e.Row, "City", null, null);
				PXUIFieldAttribute.SetError(sender, e.Row, "State", null, null);
				PXUIFieldAttribute.SetError(sender, e.Row, "PostalCode", null, null);
				PXUIFieldAttribute.SetError(sender, e.Row, "CountryID", null, null);
			}
		}

		public void Address_IsDefaultAddress_FieldVerifying<TAddress>(PXCache sender, PXFieldVerifyingEventArgs e)
			where TAddress : class, IBqlTable, IAddress, new()
		{
			PXCache cache = sender.Graph.Caches[_ItemType];
			int? key = (int?)cache.GetValue(cache.Current, _FieldOrdinal);

			if (((TAddress)e.Row).AddressID == key)
			{
				if ((bool?)e.NewValue == false && ((TAddress)e.Row).AddressID > 0)
				{
					e.Cancel = true;
					e.NewValue = true;
					_canceled[e.Row] = true;
					TAddress newaddr = (TAddress)sender.CreateCopy((TAddress)e.Row);
					newaddr.AddressID = null;
					newaddr.NoteID = null;
					newaddr.IsDefaultAddress = false;
					PXContext.SetSlot<bool>("ForceInternalCall", true);
					try
					{
						sender.Insert(sender.ToDictionary(newaddr));
					}
					finally
					{
						PXContext.SetSlot<bool>("ForceInternalCall", false);
					}
					newaddr = (TAddress)sender.Current;

					cache.SetValue(cache.Current, _FieldOrdinal, newaddr.AddressID);
					cache.MarkUpdated(cache.Current);
				}
				else if (e.NewValue != null && (bool)e.NewValue)
				{
					if ((bool?)e.NewValue == true)
					{
						e.Cancel = true;
						e.NewValue = false;
						//_canceled.Add(e.Row, false);
						DefaultRecord(cache, cache.Current, e.Row);

						if (e.ExternalCall)
						{
							sender.SetValuePending<addressLine1>(e.Row, PXCache.NotSetValue);
							sender.SetValuePending<addressLine2>(e.Row, PXCache.NotSetValue);
							sender.SetValuePending<addressLine3>(e.Row, PXCache.NotSetValue);
							sender.SetValuePending<city>(e.Row, PXCache.NotSetValue);
							sender.SetValuePending<state>(e.Row, PXCache.NotSetValue);
							sender.SetValuePending<postalCode>(e.Row, PXCache.NotSetValue);
							sender.SetValuePending<countryID>(e.Row, PXCache.NotSetValue);
						}

						cache.MarkUpdated(cache.Current);
					}
				}
			}
		}

        public virtual void DefaultAddress<TAddress, TAddressID>(PXCache sender, object DocumentRow, object AddressRow)
            where TAddress : class, IBqlTable, IAddress, new()
            where TAddressID : IBqlField
        {
            PXView view = sender.Graph.TypedViews.GetView(_Select, false);
            int startRow = -1;
            int totalRows = 0;
            bool addressFind = false;
            foreach (PXResult res in view.Select(new object[] { DocumentRow }, null, null, null, null, null, ref startRow, 1, ref totalRows))
            {
                addressFind = DefaultAddress<TAddress, TAddressID>(sender, FieldName, DocumentRow, AddressRow, res);
                break;
            }

            if (!addressFind && !_Required)
                this.ClearRecord(sender, DocumentRow);
        }

		public static bool DefaultAddress<TAddress, TAddressID>(PXCache sender, string FieldName, object DocumentRow, object AddressRow, object SourceRow)
			where TAddress : class, IBqlTable, IAddress, new()
			where TAddressID : IBqlField
		{
			bool addressFind = false;			
			if (SourceRow != null)
			{
				TAddress address = AddressRow as TAddress;
				if (address == null)
				{
					address = (TAddress)PXSelect<TAddress, Where<TAddressID, Equal<Required<TAddressID>>>>.Select(sender.Graph, sender.GetValue(DocumentRow, FieldName));
				}

                if (PXResult.Unwrap<TAddress>(SourceRow).AddressID == null || sender.GetValue(DocumentRow, FieldName) == null)
				{
					if (address == null || address.AddressID > 0)
					{
						address = new TAddress();
					}
                    
					address.BAccountAddressID = PXResult.Unwrap<Address>(SourceRow).AddressID;
                    address.BAccountID = PXResult.Unwrap<Address>(SourceRow).BAccountID;
                    address.RevisionID = PXResult.Unwrap<Address>(SourceRow).RevisionID;
					address.IsDefaultAddress = true;
                    address.AddressLine1 = PXResult.Unwrap<Address>(SourceRow).AddressLine1;
                    address.AddressLine2 = PXResult.Unwrap<Address>(SourceRow).AddressLine2;
                    address.AddressLine3 = PXResult.Unwrap<Address>(SourceRow).AddressLine3;
                    address.City = PXResult.Unwrap<Address>(SourceRow).City;
                    address.State = PXResult.Unwrap<Address>(SourceRow).State;
                    address.PostalCode = PXResult.Unwrap<Address>(SourceRow).PostalCode;
                    address.CountryID = PXResult.Unwrap<Address>(SourceRow).CountryID;
                    address.IsValidated = PXResult.Unwrap<Address>(SourceRow).IsValidated;
					addressFind = address.BAccountAddressID != null && address.BAccountID != null && address.RevisionID != null;
					if (address.AddressID == null)
					{
						address = (TAddress)sender.Graph.Caches[typeof(TAddress)].Insert(address);
						sender.SetValue(DocumentRow, FieldName, address.AddressID);
					}
					else if (AddressRow == null)
					{
						sender.Graph.Caches[typeof(TAddress)].Update(address);
					}
				}
				else
				{
					if (address != null && address.AddressID < 0)
					{
						sender.Graph.Caches[typeof(TAddress)].Delete(address);
					}
                    sender.SetValue(DocumentRow, FieldName, PXResult.Unwrap<TAddress>(SourceRow).AddressID);
                    addressFind = PXResult.Unwrap<TAddress>(SourceRow).AddressID != null;
				}
			}

            return addressFind;
		}

		public static void Copy(IAddress dest, IAddress source)
		{
			dest.BAccountID = source.BAccountID;
			dest.BAccountAddressID = source.BAccountAddressID;
			dest.RevisionID = source.RevisionID;
			dest.IsDefaultAddress = source.IsDefaultAddress;
			dest.AddressLine1 = source.AddressLine1;
			dest.AddressLine2 = source.AddressLine2;
			dest.AddressLine3 = source.AddressLine3;
			dest.City = source.City;
			dest.CountryID = source.CountryID;
			dest.State = source.State;
			dest.PostalCode = source.PostalCode;
		}

		protected void CopyAddress<TAddress, TAddressID>(PXCache sender, object DocumentRow, object SourceRow, bool clone)
			where TAddress : class, IBqlTable, IAddress, new()
			where TAddressID : IBqlField
		{
			IAddress source =
				SourceRow as IAddress ??
				(TAddress)PXSelect<TAddress, Where<TAddressID, Equal<Required<TAddressID>>>>.Select(sender.Graph, sender.GetValue(SourceRow, _FieldOrdinal));
			
			if (source != null && (clone || source.IsDefaultAddress != true))
			{
				if ((int?)sender.GetValue(DocumentRow, _FieldOrdinal) < 0)
				{
					TAddress result = new TAddress();
					result.AddressID = (int?)sender.GetValue(DocumentRow, _FieldOrdinal);
					result = (TAddress)sender.Graph.Caches[typeof(TAddress)].Locate(result);
					Copy(result, source);
				}
				else
				{
					TAddress result = new TAddress();
					Copy(result, source);
					result = (TAddress)sender.Graph.Caches[typeof(TAddress)].Insert(result);
					if (result != null)
						sender.SetValue(DocumentRow, FieldOrdinal, result.AddressID);
				}
			}
			else
				DefaultAddress<TAddress, TAddressID>(sender, DocumentRow, null);

		}
		#endregion
		#region Address Members
		public abstract class addressLine1 : PX.Data.BQL.BqlString.Field<addressLine1> { }
		public abstract class addressLine2 : PX.Data.BQL.BqlString.Field<addressLine2> { }
		public abstract class addressLine3 : PX.Data.BQL.BqlString.Field<addressLine3> { }
		public abstract class city : PX.Data.BQL.BqlString.Field<city> { }
		public abstract class countryID : PX.Data.BQL.BqlString.Field<countryID> { }
		public abstract class state : PX.Data.BQL.BqlString.Field<state> { }
		public abstract class postalCode : PX.Data.BQL.BqlString.Field<postalCode> { }
		#endregion
	}

    #endregion

    #region ContactAttribute
    public interface IPersonalContact : IContact
    {
        string FirstName { get; set; }
        string LastName { get; set; }
        string WebSite { get; set; }
    }

    public interface IContact : INotable
	{
		int? ContactID { get; set;}
		int? BAccountID { get; set; }
		int? BAccountContactID { get; set; }
		int? RevisionID { get; set; }
		bool? IsDefaultContact { get; set; }
		string FullName { get; set; }
		string Salutation { get; set; }
		string Attention { get; set; }
		string Title { get; set; }
		string Phone1 { get; set; }
		string Phone1Type { get; set; }
		string Phone2 { get; set; }
		string Phone2Type { get; set; }
		string Phone3 { get; set; }
		string Phone3Type { get; set; }
		string Fax { get; set; }
		string FaxType { get; set; }
		string Email { get; set; }
	}

    public class SharedRecordMissingException : PXOverridableException
    {
        public SharedRecordMissingException() : base(Messages.AddressOrContactIsMissing) { }

        public SharedRecordMissingException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            PXReflectionSerializer.RestoreObjectProps(this, info);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            PXReflectionSerializer.GetObjectData(this, info);
            base.GetObjectData(info, context);
        }
    }

	public abstract class ContactAttribute : SharedRecordAttribute
	{
		#region State
		protected Dictionary<object, bool> _canceled;
		#endregion
		#region State
		#endregion
		#region Ctor
		public ContactAttribute(Type AddressIDType, Type IsDefaultAddressType, Type SelectType)
			: base(AddressIDType, IsDefaultAddressType, SelectType)
		{
		}
		#endregion
		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.RowUpdating.AddHandler(_RecordType, Record_RowUpdating);
			sender.Graph.RowInserting.AddHandler(_RecordType, Record_RowInserting);
			_canceled = new Dictionary<object, bool>();
		}
		#endregion
		#region Implementation
		protected virtual void Record_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			if (e.ExternalCall)
			{
				e.Cancel = true;
			}
		}

		public virtual void Record_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			if (_canceled.ContainsKey(e.NewRow))
			{
				//avoid updating of shared record, this will cause Another Process Updated in case both addresses in SO were pointing to one record and both were overriden simultaneously.
				e.Cancel = true;
				if (sender.GetStatus(e.NewRow) == PXEntryStatus.Updated)
				{
					sender.SetStatus(e.NewRow, PXEntryStatus.Notchanged);
				}
			}

			object key = sender.GetValue(e.NewRow, _RecordID);
			object isdefault = sender.GetValue(e.NewRow, _IsDefault);
			if (Convert.ToInt32(key) > 0 && Convert.ToBoolean(isdefault) == true)
			{
				if (sender.GetStatus(e.NewRow) == PXEntryStatus.Updated)
				{
					sender.SetStatus(e.NewRow, PXEntryStatus.Notchanged);
				}
			}
		}

		public void Contact_IsDefaultContact_FieldVerifying<TContact>(PXCache sender, PXFieldVerifyingEventArgs e)
			where TContact : class, IBqlTable, IContact, new()
		{
			PXCache cache = sender.Graph.Caches[_ItemType];
			int? key = (int?)cache.GetValue(cache.Current, _FieldOrdinal);

			if (((TContact)e.Row).ContactID == key)
			{
				if ((bool?)e.NewValue == false && ((TContact)e.Row).ContactID > 0)
				{
					e.Cancel = true;
					e.NewValue = true;
					_canceled[e.Row] = true;
					TContact newaddr = (TContact)sender.CreateCopy((TContact)e.Row);
					newaddr.ContactID = null;
					newaddr.IsDefaultContact = false;
					newaddr = (TContact)sender.Insert(newaddr);

					cache.SetValue(cache.Current, _FieldOrdinal, newaddr.ContactID);
					cache.MarkUpdated(cache.Current);
				}
				else if (e.NewValue != null && (bool)e.NewValue)
				{
					if ((bool?)e.NewValue == true)
					{
						e.Cancel = true;
						e.NewValue = false;
						//_canceled.Add(e.Row, false);
						DefaultRecord(cache, cache.Current, e.Row);

						if (e.ExternalCall)
						{
							sender.SetValuePending<title>(e.Row, PXCache.NotSetValue);
							sender.SetValuePending<salutation>(e.Row, PXCache.NotSetValue);
							sender.SetValuePending<attention>(e.Row, PXCache.NotSetValue);
							sender.SetValuePending<fullName>(e.Row, PXCache.NotSetValue);
							sender.SetValuePending<email>(e.Row, PXCache.NotSetValue);
							sender.SetValuePending<phone1>(e.Row, PXCache.NotSetValue);
							sender.SetValuePending<phone1Type>(e.Row, PXCache.NotSetValue);
							sender.SetValuePending<phone2>(e.Row, PXCache.NotSetValue);
							sender.SetValuePending<phone2Type>(e.Row, PXCache.NotSetValue);
							sender.SetValuePending<phone3>(e.Row, PXCache.NotSetValue);
							sender.SetValuePending<phone3Type>(e.Row, PXCache.NotSetValue);
							sender.SetValuePending<fax>(e.Row, PXCache.NotSetValue);
							sender.SetValuePending<faxType>(e.Row, PXCache.NotSetValue);
						}

						cache.MarkUpdated(cache.Current);
					}
				}
			}
		}


        public virtual void DefaultContact<TContact, TContactID>(PXCache sender, object DocumentRow, object ContactRow)
            where TContact : class, IBqlTable, IContact, new()
            where TContactID : IBqlField
        {
            PXView view = sender.Graph.TypedViews.GetView(_Select, false);
            int startRow = -1;
            int totalRows = 0;

            bool contactFound = false;
            foreach (PXResult res in view.Select(new object[] { DocumentRow }, null, null, null, null, null, ref startRow, 1, ref totalRows))
            {
                contactFound = DefaultContact<TContact, TContactID>(sender, FieldName, DocumentRow, ContactRow, res);
                break;
            }

            if (!contactFound && !_Required)
                ClearRecord(sender, DocumentRow);
            
        }            

        public static bool DefaultContact<TContact, TContactID>(PXCache sender, string FieldName, object DocumentRow, object ContactRow, object SourceRow)
			where TContact : class, IBqlTable, IContact, new()
            where TContactID : IBqlField
		{
			bool contactFound = false;            
            if (SourceRow != null)
            {
                TContact contact = ContactRow as TContact;
				if (contact == null)
				{
					contact = (TContact)PXSelect<TContact, Where<TContactID, Equal<Required<TContactID>>>>.Select(sender.Graph, sender.GetValue(DocumentRow, FieldName));
				}

                if (PXResult.Unwrap<TContact>(SourceRow).ContactID == null || sender.GetValue(DocumentRow, FieldName) == null)
				{
					if (contact == null || contact.ContactID > 0)
					{
						contact = new TContact();
					}

					contact.BAccountContactID = PXResult.Unwrap<Contact>(SourceRow).ContactID;
					contact.BAccountID = PXResult.Unwrap<Contact>(SourceRow).BAccountID;
					contact.RevisionID = PXResult.Unwrap<Contact>(SourceRow).RevisionID;
					contact.IsDefaultContact = true;
					contact.FullName = PXResult.Unwrap<Contact>(SourceRow).FullName;
					contact.Salutation = PXResult.Unwrap<Contact>(SourceRow).Salutation;
					contact.Attention = PXResult.Unwrap<Contact>(SourceRow).Attention;
					contact.Title = PXResult.Unwrap<Contact>(SourceRow).Title;
					contact.Phone1 = PXResult.Unwrap<Contact>(SourceRow).Phone1;
					contact.Phone1Type = PXResult.Unwrap<Contact>(SourceRow).Phone1Type;
					contact.Phone2 = PXResult.Unwrap<Contact>(SourceRow).Phone2;
					contact.Phone2Type = PXResult.Unwrap<Contact>(SourceRow).Phone2Type;
					contact.Phone3 = PXResult.Unwrap<Contact>(SourceRow).Phone3;
					contact.Phone3Type = PXResult.Unwrap<Contact>(SourceRow).Phone3Type;
					contact.Fax = PXResult.Unwrap<Contact>(SourceRow).Fax;
					contact.FaxType = PXResult.Unwrap<Contact>(SourceRow).FaxType;
					contact.Email = PXResult.Unwrap<Contact>(SourceRow).EMail;                    

                    IPersonalContact person = contact as IPersonalContact;
                    if (person != null)
                    {
                        person.FirstName = PXResult.Unwrap<Contact>(SourceRow).FirstName;
                        person.LastName = PXResult.Unwrap<Contact>(SourceRow).LastName;
                        person.WebSite = PXResult.Unwrap<Contact>(SourceRow).WebSite;
                    }

					IConsentable consentContact = contact as IConsentable;
					if (consentContact != null)
					{
						consentContact.ConsentAgreement = PXResult.Unwrap<Contact>(SourceRow).ConsentAgreement;
						consentContact.ConsentDate = PXResult.Unwrap<Contact>(SourceRow).ConsentDate;
						consentContact.ConsentExpirationDate = PXResult.Unwrap<Contact>(SourceRow).ConsentExpirationDate;
					}

					contactFound = contact.BAccountContactID != null && contact.BAccountID != null && contact.RevisionID != null;

                    if (contact.ContactID == null)
					{
						contact = (TContact)sender.Graph.Caches[typeof(TContact)].Insert(contact);
						sender.SetValue(DocumentRow, FieldName, contact.ContactID);
					}
					else if (ContactRow == null)
					{
						sender.Graph.Caches[typeof(TContact)].Update(contact);
					}
				}
				else
				{
					if (contact != null && contact.ContactID < 0)
					{
						sender.Graph.Caches[typeof(TContact)].Delete(contact);
					}
                    sender.SetValue(DocumentRow, FieldName, PXResult.Unwrap<TContact>(SourceRow).ContactID);
                    contactFound = PXResult.Unwrap<TContact>(SourceRow).ContactID != null;
				}
			}

            return contactFound;
		}        

        protected void CopyContact<TContact, TContactID>(PXCache sender, object DocumentRow, object SourceRow, bool clone)
			where TContact : class, IBqlTable, IContact, new()
			where TContactID : IBqlField
		{
			IContact source = 
				SourceRow as IContact ??
				(TContact)PXSelect<TContact, Where<TContactID, Equal<Required<TContactID>>>>.Select(sender.Graph, sender.GetValue(SourceRow, _FieldOrdinal));
			
			
			if (source != null && (clone == true || source.IsDefaultContact != true))
			{
				if ((int?)sender.GetValue(DocumentRow, _FieldOrdinal) < 0)
				{
					TContact result = new TContact();
					result.ContactID = (int?)sender.GetValue(DocumentRow, _FieldOrdinal);
					result = (TContact)sender.Graph.Caches[typeof(TContact)].Locate(result);
					CopyContact(result, source);
				}
				else
				{
					TContact result = new TContact();
					CopyContact(result, source);
					result = (TContact) sender.Graph.Caches[typeof (TContact)].Insert(result);
					if (result != null)
						sender.SetValue(DocumentRow, FieldOrdinal, result.ContactID);
				}
			}
			else
				DefaultContact<TContact, TContactID>(sender, DocumentRow, null);
		}

		public static void CopyContact(IContact dest, IContact source)
		{
			dest.BAccountID = source.BAccountID;
			dest.BAccountContactID = source.BAccountContactID;
			dest.RevisionID = source.RevisionID;
			dest.IsDefaultContact = source.IsDefaultContact;
			dest.FullName = source.FullName;
			dest.Salutation = source.Salutation;
            dest.Attention = source.Attention;
			dest.Title = source.Title;
			dest.Phone1 = source.Phone1;
			dest.Phone1Type = source.Phone1Type;
			dest.Phone2 = source.Phone2;
			dest.Phone2Type = source.Phone2Type;
			dest.Phone3 = source.Phone3;
			dest.Phone3Type = source.Phone3Type;
			dest.Fax = source.Fax;
			dest.FaxType = source.FaxType;
			dest.Email = source.Email;
			dest.NoteID = null;
		}
		#endregion
		#region Contact Members
		public abstract class title : PX.Data.BQL.BqlString.Field<title> { }
		public abstract class salutation : PX.Data.BQL.BqlString.Field<salutation> { }
		public abstract class attention : PX.Data.IBqlField
		{
		}
		public abstract class fullName : PX.Data.BQL.BqlString.Field<fullName> { }
		public abstract class email : PX.Data.BQL.BqlString.Field<email> { }
		public abstract class fax : PX.Data.BQL.BqlString.Field<fax> { }
		public abstract class faxType : PX.Data.BQL.BqlString.Field<faxType> { }
		public abstract class phone1 : PX.Data.BQL.BqlString.Field<phone1> { }
		public abstract class phone1Type : PX.Data.BQL.BqlString.Field<phone1Type> { }
		public abstract class phone2 : PX.Data.BQL.BqlString.Field<phone2> { }
		public abstract class phone2Type : PX.Data.BQL.BqlString.Field<phone2Type> { }
		public abstract class phone3 : PX.Data.BQL.BqlString.Field<phone3> { }
		public abstract class phone3Type : PX.Data.BQL.BqlString.Field<phone3Type> { }
		#endregion
	}

	#endregion
    
	#region SharedRecordAttribute

	public abstract class SharedRecordAttribute : PXEventSubscriberAttribute, IPXRowInsertedSubscriber, IPXRowPersistingSubscriber, IPXRowPersistedSubscriber, IPXRowSelectedSubscriber, IPXRowDeletedSubscriber
	{
		#region State
		protected BqlCommand _Select;
		protected Type _ItemType;
		protected Type _RecordType;
		protected object _KeyToAbort;
		protected object _SelfKeyToAbort;
		protected string _RecordID;
		protected string _IsDefault;
		protected PXView _ClearView;
		public bool Required
		{
			get
			{
				return _Required;
			}
			set
			{
				_Required = value;
			}
		}
		protected bool _Required;
		#endregion
		#region Ctor
		public SharedRecordAttribute(Type RecordIDType, Type IsDefaultType, Type SelectType)
		{
			_RecordType = BqlCommand.GetItemType(RecordIDType);
			_RecordID = RecordIDType.Name;
			_IsDefault = IsDefaultType.Name;
			_Required = true;

			if (typeof(IBqlSelect).IsAssignableFrom(SelectType))
			{
				_Select = BqlCommand.CreateInstance(SelectType);
			}
			else
			{
				throw new PXArgumentException();
			}
		}
		#endregion
		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.RowSelected.AddHandler(_RecordType, Record_RowSelected);
			sender.Graph.RowUpdated.AddHandler(_RecordType, Record_RowUpdated);
			sender.Graph.FieldVerifying.AddHandler(_RecordType, _IsDefault, Record_IsDefault_FieldVerifying);
			sender.Graph.RowPersisting.AddHandler(_RecordType, Record_RowPersisting);
			sender.Graph.RowPersisted.AddHandler(_RecordType, Record_RowPersisted);

			_ItemType = sender.GetItemType();
			_persistedItems = new Dictionary<object, object>();
		}
		#endregion
		#region Implementation
		public virtual void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{			
			if (_Required && sender.GetValue(e.Row, _FieldOrdinal) == null)
			{
				using (ReadOnlyScope rs = new ReadOnlyScope(sender.Graph.Caches[_RecordType]))
				{
					object record = sender.Graph.Caches[_RecordType].Insert();
					object recordid = sender.Graph.Caches[_RecordType].GetValue(record, _RecordID);

					sender.SetValue(e.Row, _FieldOrdinal, recordid);
				}
			}				
		}

		public virtual void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			object key = sender.GetValue(e.Row, _FieldOrdinal);
			if (key != null)
			{
				PXCache cache = sender.Graph.Caches[_RecordType];
				if (Convert.ToInt32(key) < 0)
				{
					foreach (object data in cache.Inserted)
					{
						object datakey = cache.GetValue(data, _RecordID);
						if (Equals(key, datakey))
						{
							if (!cache.PersistInserted(data))
							{
								throw new PXException(ErrorMessages.RecordAddedByAnotherProcess, cache.DisplayName, ErrorMessages.ChangesWillBeLost);
							}
							_KeyToAbort = sender.GetValue(e.Row, _FieldOrdinal);
							int id = Convert.ToInt32(PXDatabase.SelectIdentity());
							sender.SetValue(e.Row, _FieldOrdinal, id);
							cache.SetValue(data, _RecordID, id);

							break;
						}
					}
				}
				else
				{
					foreach (object data in cache.Updated)
					{
						object datakey = cache.GetValue(data, _RecordID);
						if (Equals(key, datakey))
						{
							cache.PersistUpdated(data);
							break;
						}
					}
				}
			}
		}

		public virtual void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (e.TranStatus != PXTranStatus.Open)
			{
				PXCache cache = sender.Graph.Caches[_RecordType];
				if (e.TranStatus == PXTranStatus.Aborted)
				{
					if (_KeyToAbort != null)
					{
						object key = sender.GetValue(e.Row, _FieldOrdinal);
						sender.SetValue(e.Row, _FieldOrdinal, _KeyToAbort);
						foreach (object data in cache.Inserted)
						{
							object datakey = cache.GetValue(data, _RecordID);
							if (Equals(key, datakey))
							{
								cache.SetValue(data, _RecordID, Convert.ToInt32(_KeyToAbort));
								cache.ResetPersisted(data);
							}
						}
						_KeyToAbort = null;
					}
					else
					{
						object key = sender.GetValue(e.Row, _FieldOrdinal);
						foreach (object data in cache.Updated)
						{
							object datakey = cache.GetValue(data, _RecordID);
							if (Equals(key, datakey))
							{
								cache.ResetPersisted(data);
							}
						}
					}
				}
				else
				{
					object key = sender.GetValue(e.Row, _FieldOrdinal);
					foreach (object data in cache.Inserted)
					{
						object datakey = cache.GetValue(data, _RecordID);
						if (Equals(key, datakey))
						{
							cache.SetStatus(data, PXEntryStatus.Notchanged);
							PXTimeStampScope.PutPersisted(cache, data, sender.Graph.TimeStamp);
							cache.ResetPersisted(data);
						}
					}
					foreach (object data in cache.Updated)
					{
						object datakey = cache.GetValue(data, _RecordID);
						if (Equals(key, datakey))
						{
							cache.SetStatus(data, PXEntryStatus.Notchanged);
							PXTimeStampScope.PutPersisted(cache, data, sender.Graph.TimeStamp);
							cache.ResetPersisted(data);
						}
					}
					cache.IsDirty = false;
				}
				cache.Normalize();
			}
		}

		public virtual void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (sender.Graph.Caches.ContainsKey(_RecordType))
			{
				PXCache cache = sender.Graph.Caches[_RecordType];
				if (sender.GetValue(e.Row, _FieldOrdinal) == null)
				{
					PXUIFieldAttribute.SetEnabled(cache, null, false);
					PXUIFieldAttribute.SetEnabled(cache, null, false);
				}
			}
		}

		public virtual void RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			MarkDeletedIfRelatedRecordDeleted(sender, e.Row);
		}

		protected virtual void MarkDeletedIfRelatedRecordDeleted(PXCache relatedRowCache, object relatedRow)
		{
			if (relatedRow == null || !relatedRowCache.Graph.Caches.ContainsKey(_RecordType)) return;

			if (relatedRowCache.GetStatus(relatedRow).IsNotIn(PXEntryStatus.Deleted, PXEntryStatus.InsertedDeleted)) return;

			var value = relatedRowCache.GetValue(relatedRow, FieldOrdinal);

			PXCache recordCache = relatedRowCache.Graph.Caches[_RecordType];

			foreach (var record in recordCache.Inserted)
			{
				var recordKeyValue = recordCache.GetValue(record, _RecordID);

				if (object.Equals(recordKeyValue, value))
				{
					recordCache.SetStatus(record, PXEntryStatus.InsertedDeleted);

					break;
				}
			}
		}

		protected virtual bool IsDefault(PXCache cache, object row)
			=> (bool?)cache.GetValue(row, _IsDefault) == true;

		protected virtual void Record_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row != null)
			{
				bool? isDefault = (bool?)sender.GetValue(e.Row, _IsDefault);

				PXUIFieldAttribute.SetVisible(sender, null, _RecordID, false);
				PXUIFieldAttribute.SetEnabled(sender, e.Row, isDefault == false && sender.AllowUpdate);
				PXUIFieldAttribute.SetEnabled(sender, e.Row, _IsDefault, sender.AllowUpdate);
			}			
		}

		protected virtual void Record_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			PXCache cache = sender.Graph.Caches[_ItemType];
			cache.MarkUpdated(cache.Current);
		}

		public virtual void Record_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (e.Operation == PXDBOperation.Insert)
			{
				_SelfKeyToAbort = sender.GetValue(e.Row, _RecordID);
			}
		}

		Dictionary<object, object> _persistedItems = null;

		protected virtual void Record_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (e.Operation == PXDBOperation.Insert && e.TranStatus == PXTranStatus.Open)
			{
				PXCache cache = sender.Graph.Caches[_ItemType];
				object newkey = sender.GetValue(e.Row, _RecordID);

				foreach (object data in cache.Updated)
				{
					object datakey = cache.GetValue(data, _FieldOrdinal);
					if (Equals(_SelfKeyToAbort, datakey))
					{
						cache.SetValue(data, _FieldOrdinal, newkey);
					}
				}
				_persistedItems.Add(newkey, _SelfKeyToAbort);
			}
			if (e.Operation == PXDBOperation.Insert && e.TranStatus == PXTranStatus.Aborted)
			{
				PXCache cache = sender.Graph.Caches[_ItemType];
				foreach (object data in cache.Updated)
				{
					object datakey = cache.GetValue(data, _FieldOrdinal);
					if (_persistedItems.TryGetValue(datakey, out _SelfKeyToAbort))
					{
						cache.SetValue(data, _FieldOrdinal, _SelfKeyToAbort);
					}
				}
			}
			if (e.TranStatus == PXTranStatus.Completed || e.TranStatus == PXTranStatus.Aborted)
			{
				_SelfKeyToAbort = null;
			}
		}

		public abstract void Record_IsDefault_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e);

		public abstract void DefaultRecord(PXCache cache, object DocumentRow, object Row);

		public abstract void CopyRecord(PXCache cache, object DocumentRow, object SourceRow, bool clone);
	
		protected virtual void ClearRecord(PXCache cache, object DocumentRow)
		{
            bool isDirty = cache.Graph.Caches[_RecordType].IsDirty;
            object recordid = cache.GetValue(DocumentRow, _FieldOrdinal);
			if (recordid != null)
			{
				string key = "_" + _RecordType.Name + ":Clear";
				if (_ClearView == null)
				{
					lock(cache.Graph.Views)
						if (!cache.Graph.Views.TryGetValue(key, out _ClearView))
						{
							BqlCommand command =
								BqlCommand.CreateInstance(
								BqlCommand.Compose(
								typeof(Select<,>),
								_RecordType,
								typeof(Where<,>),
								_RecordType.GetNestedType(_RecordID),
								typeof(Equal<>),
								typeof(Required<>), 
								_RecordType.GetNestedType(_RecordID)));
							_ClearView = new PXView(cache.Graph, false, command);
							cache.Graph.Views.Add(key, _ClearView);
						}					
				}

				foreach (object item in _ClearView.SelectMulti(recordid))
				{
					object id = cache.Graph.Caches[_RecordType].GetValue(item, _RecordID);
					if (object.Equals(recordid, id))
					{
						cache.Graph.Caches[_RecordType].Delete(item);
						cache.SetValue(DocumentRow, _FieldOrdinal, null);
						break;
					}
				}
			}
            cache.Graph.Caches[_RecordType].IsDirty = isDirty;
        }

        public static void DefaultRecord<Field>(PXCache cache, object data)
			where Field : IBqlField
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly(typeof(Field).Name))
			{
				if (attr is SharedRecordAttribute)
				{
					try
					{
                        ((SharedRecordAttribute)attr).DefaultRecord(cache, data, (object)null);
                    }
					catch (PXSetPropertyException)
					{
						cache.Graph.RowUpdating.AddHandler(cache.GetItemType(), (sender, e) =>
						{
							if (sender.ObjectsEqual(e.Row, data))
							{
								e.Cancel = true;
							}
						});

						throw;
					}
				}
			}
		}

		public static void DefaultRecord(PXCache cache, object data, string field)
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly(field))
			{
				if (attr is SharedRecordAttribute)
				{
					((SharedRecordAttribute)attr).DefaultRecord(cache, data, (object)null);
				}
			}
		}
		public static void CopyRecord<Field>(PXCache cache, object data, object source, bool clone)
			where Field : IBqlField
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly(typeof(Field).Name))
			{
				if (attr is SharedRecordAttribute)
				{
					((SharedRecordAttribute)attr).CopyRecord(cache, data, source, clone);
				}
			}
		}

		public static void CopyRecord(PXCache cache, object data, string field, object source, bool clone)
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly(field))
			{
				if (attr is SharedRecordAttribute)
				{
					((SharedRecordAttribute)attr).CopyRecord(cache, data, source, clone);
				}
			}
		}


        public static void ClearRecord<Field>(PXCache cache, object data)
            where Field : IBqlField
        {
            foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly(typeof(Field).Name))
            {
                if (attr is SharedRecordAttribute)
                {
                    try
                    {
                        ((SharedRecordAttribute)attr).ClearRecord(cache, data);
                    }
                    catch (PXSetPropertyException)
                    {
                        cache.Graph.RowUpdating.AddHandler(cache.GetItemType(), (sender, e) =>
                        {
                            if (sender.ObjectsEqual(e.Row, data))
                            {
                                e.Cancel = true;
                            }
                        });

                        throw;
                    }
                }
            }
        }
        #endregion

    }

	#endregion

	#region PXMaskArgumentException

	public class PXMaskArgumentException : PXArgumentException
	{
		public int SourceIdx = -1;
		protected object[] _args = new object[0];
		public PXMaskArgumentException()
			:base()
		{ 
		}

		public PXMaskArgumentException(int SourceIdx)
			:base()
		{
			this.SourceIdx = SourceIdx;
		}

		public PXMaskArgumentException(params object[] args)
			: base(args.Length < 3 ? Messages.MaskSourceMissing : Messages.MaskSourceMissing2, args)
		{ 
			_args = args;
		}

		public static object[] ConcatArrays(object[] a, object[] b)
		{
			List<object> c = new List<object>(a); 
			c.AddRange(b);
			return c.ToArray();
		}

		public PXMaskArgumentException(PXMaskArgumentException source, params object[] args)
			: this(ConcatArrays(source._args, args)) 
		{
			this.SourceIdx = source.SourceIdx;
		}
		public PXMaskArgumentException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			PXReflectionSerializer.RestoreObjectProps(this, info);
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			PXReflectionSerializer.GetObjectData(this, info);
			base.GetObjectData(info, context);
		}


	}

	public class PXMaskValueException : PXArgumentException
	{
		public int SourceIdx = -1;
		public PXMaskValueException()
			: base()
		{
		}

		public PXMaskValueException(int SourceIdx)
			:base()
		{
			this.SourceIdx = SourceIdx;
		}

		public PXMaskValueException(params object[] args)
			: base(Messages.InvalidMask, args)
		{
		}

		public PXMaskValueException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			PXReflectionSerializer.RestoreObjectProps(this, info);
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			PXReflectionSerializer.GetObjectData(this, info);
			base.GetObjectData(info, context);
		}

	}

	#endregion

	#region PXDimensionMaskAttribute

	public class PXDimensionMaskAttribute : PXDimensionAttribute
	{
		#region State
		protected string _Mask;
		protected string _defaultValue;
		protected string[] _allowedValues;
		protected string[] _allowedLabels;

		#endregion

		#region Ctor
		public PXDimensionMaskAttribute(string dimension, string mask, string defaultValue, string[] allowedValues, string[] allowedLabels)
			: base(dimension)
		{
			_Mask = mask;
			if (allowedLabels.Length != allowedValues.Length)
			{
				throw new ArgumentException();
			}
			_defaultValue = defaultValue;
			_allowedValues = allowedValues;
            _allowedLabels = allowedLabels;
		}
		#endregion
		#region Runtime
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			var pairs =
					_allowedValues.Zip(_allowedLabels, (v, l) => new Tuple<string, string>(v, l))
					.Where(t => !PXAccess.IsStringListValueDisabled(sender.GetItemType().FullName, _FieldName, t.Item1));
			_allowedValues = pairs.Select(p => p.Item1).ToArray();
			_allowedLabels = pairs.Select(p => p.Item2).ToArray();

			sender.Graph.Views["_" + _Mask + "_Segments_"] =
				new PXView(sender.Graph, true, new Select<SegmentValue>(), (PXSelectDelegate<short?>)_MaskGetArgs);

		}

		internal IEnumerable _MaskGetArgs(
			[PXShort]
				short? segment
			)
		{
			if (!_Definition.Dimensions.ContainsKey(_Dimension) || segment == null || segment < 0 || segment >= _Definition.Values[_Dimension].Length)
			{
				yield break;
			}

			PXSegment seg = _Definition.Dimensions[_Dimension][(int)segment - 1];
			for (int i = 0; i < _allowedValues.Length; i++)
			{
				yield return new SegmentValue(new string(char.Parse(_allowedValues[i]), seg.Length), PXMessages.LocalizeNoPrefix(_allowedLabels[i]), false);
			}
		}
		#endregion
		#region Implementation
		public override void SelfRowSelecting(PXCache sender, PXRowSelectingEventArgs e, PXSegment[] segs, int total)
		{
			char defchar = !string.IsNullOrEmpty(_defaultValue) ? _defaultValue[0] : ' ';
			object val = sender.GetValue(e.Row, _FieldOrdinal);
			if (val is string)
			{
				string sval = (string)val;
				
				if (defchar != ' ')
				{
					sval = sval.TrimEnd();
				}
				if (sval.Length < total)
				{
					sval = sval + new String(defchar, total - sval.Length);
				}
				else if (sval.Length > total)
				{
					sval = sval.Substring(0, total);
				}
				else
				{	
					return;
				}

				int index = 0;
				char[] arr = sval.ToCharArray();
				
				foreach (PXSegment seg in segs)
				{
					char fillchar = arr[index];
					int lenght = index + seg.Length;
					
					for (int i = index; i < lenght; i++)
					{
						arr[i] = fillchar;
					}

					index += seg.Length;
				}

				sender.SetValue(e.Row, _FieldOrdinal, new String(arr));	
			}
		}

		public virtual void CopySegments(PXSegment[] segments, out PXSegment[] copy)
		{
			copy = new PXSegment[segments.Length];

			for (int i = 0; i < segments.Length; i++)
			{
				copy[i] = new PXSegment('A', ' ', segments[i].Length, true, 1, 0, segments[i].Separator, false, segments[i].Descr);
			}
		}
		public PXSegment[] CopySegments(PXSegment[] segments)
		{
			PXSegment[] copy;
			CopySegments(segments, out copy);

			return copy;
		}

		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				e.ReturnState = PXSegmentedState.CreateInstance(e.ReturnState, _FieldName, _Definition.Dimensions.ContainsKey(_Dimension) ? CopySegments(_Definition.Dimensions[_Dimension]) : new PXSegment[0], "_" + _Mask + "_Segments_", PX.Data.DimensionLookupMode.BySegmentedKeys, false, _Wildcard);
			}
		}

		public override void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (!_Definition.Dimensions.ContainsKey(_Dimension))
			{
				throw new PXSetPropertyException(PXMessages.LocalizeFormat(ErrorMessages.DimensionDontExist, _Dimension));
			}

			int totallength = 0;
			for (int i = 0; i < _Definition.Dimensions[_Dimension].Length; i++)
			{
				totallength += ((PXSegment)_Definition.Dimensions[_Dimension][i]).Length;
			}

			e.NewValue = new string(char.Parse(_defaultValue), totallength);
			e.Cancel = true;
		}

		public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (!_Definition.Dimensions.ContainsKey(_Dimension))
			{
				throw new PXSetPropertyException(PXMessages.LocalizeFormat(ErrorMessages.DimensionDontExist, _Dimension));
			}
			if(e.NewValue == null) return;

			string NewValue = (string)e.NewValue;
			List<string> errs = new List<string>();

			for (int i = 0; i < _Definition.Dimensions[_Dimension].Length; i++)
			{
				string input = NewValue.Substring(0, ((PXSegment)_Definition.Dimensions[_Dimension][i]).Length);

				bool MatchFound = false;
				foreach (string val in _allowedValues)
				{
					if ((MatchFound = new string(char.Parse(val), input.Length).CompareTo(input) == 0))
					{
						break;
					}
				}

				if (!MatchFound)
				{
					errs.Add(input);
				}

				NewValue = NewValue.Substring(((PXSegment)_Definition.Dimensions[_Dimension][i]).Length);
			}

			if (errs.Count > 0)
			{
				if (errs.Count == 1)
				{
					throw new PXSetPropertyException(PXMessages.LocalizeFormat(ErrorMessages.ElementOfFieldDoesntExist, errs[0], _FieldName));
				}
				errs.Add(_FieldName);
				StringBuilder bld = new StringBuilder();
				StringBuilder bld2 = new StringBuilder();
				int i;
				for (i = 0; i < errs.Count - 1; i++)
				{
					bld.Append('{');
					bld.Append(i);
					bld.Append('}');
					if (i < errs.Count - 2)
					{
						bld.Append(", ");
					}
				}
				bld2.Append('{');
				bld2.Append(i);
				bld2.Append('}');

				string localstring = PXMessages.LocalizeFormat(ErrorMessages.ElementsOfFieldsDontExist, bld.ToString(), bld2.ToString());
				throw new PXSetPropertyException(String.Format(localstring, errs.ToArray()));
			}
		}

        public void SynchronizeLabels(string[] allowedvalues, string[] allowedlabels)
        {
            for (int i = 0; i < allowedvalues.Length; i++)
            {
                int ind = Array.IndexOf(_allowedValues, allowedvalues[i]);
                if (ind < 0)
                    continue;
                _allowedLabels[ind] = allowedlabels[i];
            }
        }

		public static string MakeSub(string mask, string[] _allowedValues, int DefaultValueIdx, params string[] sources)
		{
			string _Dimension = SubAccountAttribute.DimensionName;
			Definition _Definition = PXDimensionAttribute.GetDefinition(_Dimension);

            var valuelength = _Definition.Dimensions[_Dimension].Cast<PXSegment>().Sum(x => x.Length);

            mask = PXDimensionAttribute.AdjustValueLength(mask, valuelength);

            for (int i = 0; i < sources.Length; i++)
            {
                sources[i] = PXDimensionAttribute.AdjustValueLength(sources[i], valuelength);
            }

            if (string.IsNullOrEmpty(mask) && DefaultValueIdx >= 0)
            {
                return sources[DefaultValueIdx];
            }

			int segstart = 0;
			StringBuilder bld = new StringBuilder();

			for (int i = 0; i < _Definition.Dimensions[_Dimension].Length; i++)
			{
				int seglen = ((PXSegment)_Definition.Dimensions[_Dimension][i]).Length;
				string input = mask.Substring(segstart, seglen);

				bool MatchFound = false;
				for (int j = 0; j < _allowedValues.Length; j++)
				{
					if ((MatchFound = new string(char.Parse(_allowedValues[j]), input.Length).CompareTo(input) == 0))
					{
						if (string.IsNullOrEmpty(sources[j]) || sources[j].Length < segstart + seglen)
						{
							if (DefaultValueIdx < 0)
							{
								throw new PXMaskArgumentException(j);
							}
							else if (string.IsNullOrEmpty(sources[DefaultValueIdx]) || sources[DefaultValueIdx].Length < segstart + seglen)
							{
								throw new PXMaskArgumentException(DefaultValueIdx);
							}
							else
							{
								bld.Append(sources[DefaultValueIdx].Substring(segstart, seglen));
								break;
							}
						}
						else
						{
							bld.Append(sources[j].Substring(segstart, seglen));
							break;
						}
					}
				}

				if (!MatchFound)
				{
					if (new string(' ', input.Length).CompareTo(input) == 0)
					{
						bld.Append(input);
					}
					else
					{
						throw new PXMaskValueException(i);
					}
				}

				segstart += seglen;
			}

			return bld.ToString();
		}

		public static string MakeSub<Field>(PXGraph graph, string mask, string[] allowedValues, params object[] sourceIDs)
			where Field : IBqlField
		{
			return MakeSub<Field>(graph, mask, allowedValues, -1, sourceIDs);
		}

		public static string MakeSub<Field>(PXGraph graph, string mask, bool? stkItem, string[] allowedValues, params object[] sourceIDs)
			where Field : IBqlField
		{
			if (stkItem==true)
				return MakeSub<Field>(graph, mask, allowedValues, -1, sourceIDs);
			else
				return MakeSub<Field>(graph, mask, allowedValues, 0, sourceIDs);
		}

		public static string MakeSub<Field>(PXGraph graph, string mask, string[] allowedValues, int DefaultValueIdx, params object[] sourceIDs)
			where Field : IBqlField
		{
			string[] sourceCDs = new string[sourceIDs.Length];
			Dictionary<object, string> dict = PXDatabase.GetSlot<Dictionary<object, string>>("SubCDs", typeof(Sub));
			lock (((ICollection)dict).SyncRoot)
			{
				for (int i = 0; i < sourceIDs.Length; i++)
				{
					if (sourceIDs[i] != null)
					{
						string subcd;
						if (!dict.TryGetValue(sourceIDs[i], out subcd))
						{
							Sub sub = PXSelect<Sub, Where<Sub.subID, Equal<Required<Sub.subID>>>>.Select(graph, sourceIDs[i]);
							if (sub != null)
							{
								dict[sourceIDs[i]] = subcd = sub.SubCD;
							}
							else
							{
								dict[sourceIDs[i]] = subcd = null;
							}
						}
						sourceCDs[i] = subcd;
					}
				}
			}

			try
			{
				return MakeSub(mask, allowedValues, DefaultValueIdx, sourceCDs);
			}
			catch (PXMaskValueException ex)
			{
				PXCache cache = graph.Caches[BqlCommand.GetItemType(typeof(Field))];
				string fieldName = typeof(Field).Name.ToLower();
				throw new PXMaskValueException(++ex.SourceIdx, PXUIFieldAttribute.GetDisplayName(cache, fieldName));
			}
		}
		#endregion
	}

	#endregion

	#region PXForeignSelectorAttribute

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true)]
	public class PXForeignSelectorAttribute : PXEventSubscriberAttribute
	{
		#region State
		protected Type _SearchType = null;
		#endregion
		#region Ctor
		public PXForeignSelectorAttribute(Type SearchType)
		{
			if (SearchType != null && typeof(IBqlField).IsAssignableFrom(SearchType))
			{
				this._SearchType = SearchType;
			}
			else
			{
				throw new PXArgumentException();
			}
		}
		#endregion
		#region Implementation
		protected virtual object GetValueExt(PXCache sender, object item)
		{
			object val = sender.GetValue(item, _FieldOrdinal);
			object copyval = val;

			PXCache cache = sender.Graph.Caches[BqlCommand.GetItemType(_SearchType)];

			if (cache != null && val != null)
			{
				cache.RaiseFieldSelecting(_SearchType.Name, null, ref val, true);

				if (val is PXFieldState && Equals(((PXFieldState)val).Value, copyval))
				{
					return null;
				}
                if (val != null)
                {
                    return val.ToString().TrimEnd();
                }
			}
			return null;
		}

		public static object GetValueExt(PXCache cache, object data, string name)
		{
			object val = null;

			foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly(name))
			{
				if (attr is PXForeignSelectorAttribute)
				{
					if ((val = ((PXForeignSelectorAttribute)attr).GetValueExt(cache, data)) != null)
					{
						return val;
					}
				}
			}
			val = cache.GetValueExt(data, name);
            if (val != null)
            {
                return val.ToString().TrimEnd();
            }
            return null;
		}

		public static object GetValueExt<Field>(PXCache cache, object data)
			where Field : IBqlField 
		{
			object val = null;

			foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly<Field>())
			{
				if (attr is PXForeignSelectorAttribute)
				{
					if ((val = ((PXForeignSelectorAttribute)attr).GetValueExt(cache, data)) != null)
					{
						return val;
					}
				}
			}
			val = cache.GetValueExt<Field>(data);
            if (val != null)
            {
                return val.ToString().TrimEnd();
            }
            return null;
		}
		#endregion
	}

	#endregion

	#region PXLiteSelectorAttribute

	public class PXLiteSelectorAttribute : PXEventSubscriberAttribute
	{
		#region State
		protected Type _SearchType = null;
		protected Type _SubstituteKey = null;
		public Type SubstituteKey
		{
			get
			{
				return this._SubstituteKey;
			}
			set
			{
				if (value != null && typeof(IBqlField).IsAssignableFrom(value) && BqlCommand.GetItemType(value) == BqlCommand.GetItemType(_SearchType))
				{
					this._SubstituteKey = value;
				}
				else
				{
					throw new PXArgumentException();
				}
			}
		}
		#endregion
		#region Ctor
		public PXLiteSelectorAttribute(Type SearchType)
		{
			if (SearchType != null && typeof(IBqlField).IsAssignableFrom(SearchType))
			{
				this._SearchType = SearchType;
			}
			else
			{
				throw new PXArgumentException();
			}
		}
		#endregion
		#region Implementation
		public virtual object GetValueExt(PXCache sender, object item)
		{
			List<PXDataField> fields = new List<PXDataField>();

			fields.Add(new PXDataField(_SubstituteKey.Name));
			fields.Add(new PXDataFieldValue(_SearchType.Name, sender.GetValue(item, _FieldOrdinal)));

			PXCache cache = sender.Graph.Caches[BqlCommand.GetItemType(_SearchType)];

			PXFieldState state = (PXFieldState) cache.GetStateExt(null, _SubstituteKey.Name);
			TypeCode tc = Type.GetTypeCode(state.DataType);

			using (PXDataRecord record = PXDatabase.SelectSingle(BqlCommand.GetItemType(_SearchType), fields.ToArray()))
			{
				if (record != null)
				{
					switch (tc)
					{
						case TypeCode.String:
							return record.GetString(0);
						default:
							throw new PXException();
					}
				}
				return null;
			}
		}

		public static object GetValueExt(PXCache cache, object data, string name)
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly(name))
			{
				if (attr is PXLiteSelectorAttribute)
				{
					return ((PXLiteSelectorAttribute)attr).GetValueExt(cache, data);
				}
			}
			return null;
		}
		#endregion
	}

	#endregion

	#region PXVerifySelectorAttribute

	public class PXVerifySelectorAttribute : PXSelectorAttribute
	{
		protected bool _VerifyField = true;

		public bool VerifyField
		{
			get
			{
				return this._VerifyField;
			}
			set
			{
				this._VerifyField = value;
			}
		}

		public PXVerifySelectorAttribute(Type searchtype)
			: base(searchtype)
		{
		}

		public PXVerifySelectorAttribute(Type searchtype, params Type[] fieldList)
			: base(searchtype, fieldList)
		{
		}

		public static void SetVerifyField<Field>(PXCache cache, object data, bool isVerifyField)
			where Field : IBqlField
		{
			if (data == null)
			{
				cache.SetAltered<Field>(true);
			}
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes<Field>(data))
			{
				if (attr is PXVerifySelectorAttribute)
				{
					((PXVerifySelectorAttribute)attr).VerifyField = isVerifyField;
				}
			}
		}

		public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (_VerifyField)
			{
				base.FieldVerifying(sender, e);
			}
		}
	}

	#endregion

	#region AutoNumberAttribute

	public class AutoNumberException : PXException
	{
		public AutoNumberException()
			:base(Messages.CantAutoNumber)
		{
		}

		public AutoNumberException(string message, params object[] args)
			: base(message, args)
		{
		}

        [Obsolete(PX.Objects.Common.InternalMessages.MethodIsObsoleteAndWillBeRemoved2019R2)]
		public AutoNumberException(params object[] args)
            : base(Messages.CantAutoNumberSpecific, args)
		{
		}

		public AutoNumberException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			PXReflectionSerializer.RestoreObjectProps(this, info);
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			PXReflectionSerializer.GetObjectData(this, info);
			base.GetObjectData(info, context);
		}

	}

	public class AutoNumberAttribute : PXEventSubscriberAttribute, IPXFieldDefaultingSubscriber, IPXFieldVerifyingSubscriber, IPXRowInsertingSubscriber, IPXRowPersistingSubscriber, IPXRowPersistedSubscriber, IPXFieldSelectingSubscriber
	{
		private Type _doctypeField;
		private string[] _doctypeValues;
		private Type[] _setupFields;
		private string[] _setupValues;

		private string _dateField;
		private Type _dateType;

		private string _numberingID;
		private DateTime? _dateTime;

		private string _emptyDateMessage;

		public static string GetNewNumberSymbol<Field>(PXCache cache, object data)
			where Field : IBqlField
		{
			foreach (AutoNumberAttribute attr in cache.GetAttributesReadonly<Field>(data).OfType<AutoNumberAttribute>())
			{
				attr.getfields(cache, data);
				return attr.GetNewNumberSymbol();
			}
			return string.Empty;
		}

		public static bool IsViewOnlyRecord<Field>(PXCache cache, object data)
			where Field : IBqlField
		{
			foreach (AutoNumberAttribute attr in cache.GetAttributesReadonly<Field>(data).OfType<AutoNumberAttribute>())
			{
				attr.getfields(cache, data);
				string newNumber = (string)cache.GetValue(data, attr._FieldName);
				string notSetNumber = attr.GetNewNumberSymbol();
				return attr._numberingID == null && string.Equals(newNumber, notSetNumber);
			}
			return false;
		}

		public static void SetNumberingId<Field>(PXCache cache, string key, string value)
			where Field : IBqlField
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly<Field>())
			{
				if (attr is AutoNumberAttribute && attr.AttributeLevel == PXAttributeLevel.Cache && ((AutoNumberAttribute)attr)._doctypeValues.Length > 0)
				{
					int i;
					if ((i = Array.IndexOf(((AutoNumberAttribute)attr)._doctypeValues, key)) >= 0)
					{
						((AutoNumberAttribute)attr)._setupValues[i] = value;
					}
				}
			}
		}

		public static void SetNumberingId<Field>(PXCache cache, string value)
			where Field : IBqlField
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly<Field>())
			{
				if (attr is AutoNumberAttribute && attr.AttributeLevel == PXAttributeLevel.Cache && ((AutoNumberAttribute)attr)._doctypeValues.Length == 0)
				{
					((AutoNumberAttribute)attr)._setupValues[0] = value;
				}
			}
		}

		[PXInternalUseOnly]
		public static void SetUserNumbering<Field>(PXCache cache)
			where Field : IBqlField
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly<Field>())
			{
				if (attr is AutoNumberAttribute && attr.AttributeLevel == PXAttributeLevel.Cache && ((AutoNumberAttribute)attr)._doctypeValues.Length == 0)
				{
					((AutoNumberAttribute)attr).UserNumbering = true;
				}
			}
		}

        public static NumberingSequence GetNumberingSequence(string numberingID, int? branchID, DateTime? date)
        {
            if (numberingID == null || date == null)
                return null;

            using (PXDataRecord record = PXDatabase.SelectSingle<NumberingSequence>(
                new PXDataField<NumberingSequence.endNbr>(),
                new PXDataField<NumberingSequence.lastNbr>(),
                new PXDataField<NumberingSequence.startNbr>(),
                new PXDataField<NumberingSequence.warnNbr>(),
                new PXDataField<NumberingSequence.nbrStep>(),
                new PXDataField<NumberingSequence.numberingSEQ>(),
                new PXDataField<NumberingSequence.nBranchID>(),
                new PXDataField<NumberingSequence.startNbr>(),
                new PXDataField<NumberingSequence.startDate>(),
                new PXDataField<NumberingSequence.createdByID>(),
                new PXDataField<NumberingSequence.createdByScreenID>(),
                new PXDataField<NumberingSequence.createdDateTime>(),
                new PXDataField<NumberingSequence.lastModifiedByID>(),
                new PXDataField<NumberingSequence.lastModifiedByScreenID>(),
                new PXDataField<NumberingSequence.lastModifiedDateTime>(),
                new PXDataFieldValue<NumberingSequence.numberingID>(PXDbType.VarChar, 255, numberingID),
                new PXDataFieldValue<NumberingSequence.nBranchID>(PXDbType.Int, 4, branchID, PXComp.EQorISNULL),
                new PXDataFieldValue<NumberingSequence.startDate>(PXDbType.DateTime, 4, date, PXComp.LE),
                new PXDataFieldOrder<NumberingSequence.nBranchID>(true),
                new PXDataFieldOrder<NumberingSequence.startDate>(true)
                ))
            {
                if (record == null)
                    return null;

                return new NumberingSequence
                {
                    EndNbr = record.GetString(0),
                    LastNbr = record.GetString(1) ?? record.GetString(2),
                    WarnNbr = record.GetString(3),
                    NbrStep = (int)record.GetInt32(4),
                    NumberingSEQ = (int)record.GetInt32(5),
                    NBranchID = (int?)record.GetInt32(6),
                    StartNbr = record.GetString(7),
                    StartDate = record.GetDateTime(8),
                    CreatedByID = record.GetGuid(9),
                    CreatedByScreenID = record.GetString(10),
                    CreatedDateTime = record.GetDateTime(11),
                    LastModifiedByID = record.GetGuid(12),
                    LastModifiedByScreenID = record.GetString(13),
                    LastModifiedDateTime = record.GetDateTime(14)
                };
            }
        }
       
		private void getfields(PXCache sender, object row)
		{
			PXCache cache;
			Type _setupType = null;
			string _setupField = null;
			BqlCommand _Select = null;

			_numberingID = null;

			if (_doctypeField != null)
			{
				string doctypeValue = (string)sender.GetValue(row, _doctypeField.Name);

				int i;
				if ((i = Array.IndexOf(_doctypeValues, doctypeValue)) >= 0 && _setupValues[i] != null)
				{
					_numberingID = _setupValues[i];
				}
				else if (i >= 0 && _setupFields[i] != null)
				{
					if (typeof(IBqlSearch).IsAssignableFrom(_setupFields[i]))
					{
						_Select = BqlCommand.CreateInstance(_setupFields[i]);
						_setupType = BqlCommand.GetItemType(((IBqlSearch)_Select).GetField());
						_setupField = ((IBqlSearch)_Select).GetField().Name;
					}
					else if (_setupFields[i].IsNested && typeof(IBqlField).IsAssignableFrom(_setupFields[i]))
					{
						_setupField = _setupFields[i].Name;
						_setupType = BqlCommand.GetItemType(_setupFields[i]);
					}
				}
			}
			else if ((_numberingID = _setupValues[0]) != null)
			{
			}
			else if (typeof(IBqlSearch).IsAssignableFrom(_setupFields[0]))
			{
				_Select = BqlCommand.CreateInstance(_setupFields[0]);
				_setupType = BqlCommand.GetItemType(((IBqlSearch)_Select).GetField());
				_setupField = ((IBqlSearch)_Select).GetField().Name;
			}
			else if (_setupFields[0].IsNested && typeof(IBqlField).IsAssignableFrom(_setupFields[0]))
			{
				_setupField = _setupFields[0].Name;
				_setupType = BqlCommand.GetItemType(_setupFields[0]);
			}

			if (_Select != null)
			{
				PXView view = sender.Graph.TypedViews.GetView(_Select, false);
				int startRow = -1;
				int totalRows = 0;
				List<object> source = view.Select(
					new object[] { row },
					null,
					null,
					null,
					null,
					null,
					ref startRow,
					1,
					ref totalRows);
				if (source != null && source.Count > 0)
				{
					object item = source[source.Count - 1];
					if (item != null && item is PXResult)
					{
						item = ((PXResult)item)[_setupType];
					}
					_numberingID = (string)sender.Graph.Caches[_setupType].GetValue(item, _setupField);
				}
			}
			else if (_setupType != null)
			{
				cache = sender.Graph.Caches[_setupType];
				if (cache.Current != null && _numberingID == null)
				{
					_numberingID = (string)cache.GetValue(cache.Current, _setupField);
				}
			}

			cache = sender.Graph.Caches[_dateType];
			if (sender.GetItemType() == _dateType)
			{
				_dateTime = (DateTime?)cache.GetValue(row, _dateField);
			}
			else if (cache.Current != null)
			{
				_dateTime = (DateTime?)cache.GetValue(cache.Current, _dateField);
			}
		}

		public AutoNumberAttribute(Type doctypeField, Type dateField, string[] doctypeValues, Type[] setupFields)
		{
			_dateField = dateField.Name;
			_dateType = BqlCommand.GetItemType(dateField);

			_doctypeField = doctypeField;
			_doctypeValues = doctypeValues;
			_setupFields = setupFields;
		}

		protected AutoNumberAttribute(Type doctypeField, Type dateField, params Tuple<string, Type>[] docTypesToSetups)
			: this(doctypeField, dateField, docTypesToSetups?.Select(t => t.Item1).ToArray(), docTypesToSetups?.Select(t => t.Item2).ToArray())
		{
		}

		protected static Pairer Pair(string docTypeValue) => new Pairer(docTypeValue);
		protected struct Pairer
		{
			private readonly string docTypeValue;
			public Pairer(string docTypeValue) { this.docTypeValue = docTypeValue; }
			public Tuple<string, Type> To<TSetupField>() where TSetupField : IBqlField => Tuple.Create(docTypeValue, typeof(TSetupField));
		}

		public AutoNumberAttribute(Type setupField, Type dateField)
			: this(null, dateField, new string[] { }, new Type[] { setupField })
		{
		}

		public AutoNumberAttribute(Type setupField, Type dateField, string emptyDateMessage)
			: this(null, dateField, new string[] { }, new Type[] { setupField })
		{
			_emptyDateMessage = emptyDateMessage;
		}

		protected ObjectRef<bool?> _UserNumbering;
		protected bool? UserNumbering
		{
			get
			{
				return _UserNumbering.Value;
			}
			set
			{
				_UserNumbering.Value = value;
			}
		}

		protected ObjectRef<string> _NewSymbol;
		protected string NewSymbol
		{
			get
			{
				return _NewSymbol.Value;
			}
			set
			{
				_NewSymbol.Value = value;
			}
		}

		public enum NullNumberingMode
		{
			ViewOnly,
			UserNumbering
		}

		protected NullNumberingMode NullMode;
		protected string NullString;

		public class Numberings : IPrefetchable
		{
			protected Dictionary<string,string> _items = new Dictionary<string,string>();

			void IPrefetchable.Prefetch()
			{
				_items.Clear();

				foreach (PXDataRecord record in PXDatabase.SelectMulti<Numbering>(
					new PXDataField<Numbering.numberingID>(),
					PXDBLocalizableStringAttribute.GetValueSelect("Numbering", "NewSymbol", false),
					new PXDataField<Numbering.userNumbering>()))
				{
					string numberingID = record.GetString(0);
					string newSymbol = record.GetString(1);
					bool? userNumbering = record.GetBoolean(2);

					_items[numberingID] = userNumbering == true ? null : newSymbol;
				}
			}

			public bool TryGetValue(string key, out string value)
			{
				return _items.TryGetValue(key, out value);
			}

			public ReadOnlyDictionary<string, string> GetNumberings()
			{
				return new ReadOnlyDictionary<string, string>(_items);
			}
		}

		public string this[string key]
		{
			get
			{
				string currentLanguage = System.Threading.Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;
				if (!PXDBLocalizableStringAttribute.IsEnabled)
				{
					currentLanguage = "";
				}
				Numberings items = PXDatabase.GetSlot<Numberings>(typeof(Numberings).Name + currentLanguage, typeof(Numbering));
				if (items != null)
				{
					string value;
					if (!items.TryGetValue(key, out value))
					{
						throw new PXException(Messages.NumberingIDNull);
					}
					return value;
				}
				return null;
			}
		}

		[Obsolete("This method is obsolete and will be removed in Acumatica ERP 2019R1. Please use GetNewNumberSymbol() instead.")]
		protected virtual string GetNewNumber()
		{
			return GetNewNumberSymbol();
		}
		[Obsolete("This method is obsolete and will be removed in Acumatica ERP 2019R1. Please use GetNewNumberSymbol(string) instead.")]
		protected virtual string GetNewNumber(string numberingID)
		{
			return GetNewNumberSymbol(numberingID);
		}
		protected virtual string GetNewNumberSymbol()
		{
			return GetNewNumberSymbol(null);
		}
		protected virtual string GetNewNumberSymbol(string numberingID)
		{
			numberingID = numberingID ?? _numberingID;

			if (numberingID != null)
			{
				return (UserNumbering = (NewSymbol = this[numberingID]) == null) != true ? " " + NewSymbol : null;
			}
			else if (NullMode == NullNumberingMode.UserNumbering)
			{
				return NullString;
			}
			else
			{
				return " " + Messages.NoNumberNewSymbol;
			}
		}

		void IPXFieldDefaultingSubscriber.FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			getfields(sender, e.Row);
			e.NewValue = GetNewNumberSymbol();
		}

		void IPXFieldVerifyingSubscriber.FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (sender.Locate(e.Row) == null && !(sender.Graph is PXGenericInqGrph))
			{
				string oldValue = (string)sender.GetValue(e.Row, _FieldOrdinal);

				if (UserNumbering != true && oldValue != null)
				{
					e.NewValue = oldValue;
				}
			}
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			_UserNumbering = new ObjectRef<bool?>();
			_NewSymbol = new ObjectRef<string>();
			_setupValues = new string[_setupFields.Length];

			sender.SetAutoNumber(_FieldName);

			bool IsKey;
			if (!(IsKey = sender.Keys.IndexOf(_FieldName) > 0))
			{
				foreach (PXEventSubscriberAttribute attr in sender.GetAttributesReadonly(_FieldName))
				{
					if (attr is PXDBFieldAttribute && (IsKey = ((PXDBFieldAttribute)attr).IsKey))
					{
						break;
					}
				}
			}

			if (!IsKey)
			{
				NullString = string.Empty;
				NullMode = NullNumberingMode.UserNumbering;
			}
			else
			{
				sender.Graph.RowSelected.AddHandler(sender.GetItemType(), Parameter_RowSelected);
				sender.Graph.CommandPreparing.AddHandler(sender.GetItemType(), _FieldName, Parameter_CommandPreparing);
			}
		}

		protected virtual void Parameter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row != null)
			{
				if (UserNumbering == null || NewSymbol == null)
				{
					getfields(sender, e.Row);
					GetNewNumberSymbol();
				}
			}
		}

		protected virtual void Parameter_CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			string Key;
			if (((e.Operation & PXDBOperation.ReadItem) == PXDBOperation.ReadItem ||
				(e.Operation & PXDBOperation.Command) == PXDBOperation.Select && (e.Operation & PXDBOperation.Option) != PXDBOperation.External &&
				(e.Operation & PXDBOperation.Option) != PXDBOperation.ReadOnly && e.Row == null) && 
				(Key = e.Value as string) != null)
			{
				if (UserNumbering == false && Key.Length > 1 && string.Equals(Key[0] == ' ' ? Key.Substring(1) : Key, NewSymbol))
				{
					e.DataValue = null;
					e.Cancel = true;
				}
			}
		}

		protected string LastNbr;
		protected string WarnNbr;
		protected string NewNumber;
		protected string NotSetNumber;
		protected int? NumberingSEQ;
        protected object _KeyToAbort = null;

        protected static bool IS_SEPARATE_SCOPE { get { return WebConfig.EnableAutoNumberingInSeparateConnection; } }

        public static string GetKeyToAbort(PXCache cache, object row, string fieldName)
        { 
            foreach (PXEventSubscriberAttribute attr in cache.GetAttributesReadonly(row, fieldName))
            {
                if (attr is AutoNumberAttribute && attr.AttributeLevel == PXAttributeLevel.Item)
                {
                    return (string)((AutoNumberAttribute)attr)._KeyToAbort;
                }
            }
            return null;
        }

		public virtual void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) != PXDBOperation.Insert)
			{
				return;
			}

			getfields(sender, e.Row);

			//fix for manual non key numbering, broken when we started returning null instead of "" from getnewnumber
			if ((NotSetNumber = GetNewNumberSymbol()) == null && NullString == String.Empty)
			{
				return;
			}
			if ((NotSetNumber = GetNewNumberSymbol()) == NullString)
			{
				object keyValue = sender.GetValue(e.Row, _FieldName);
				Numberings items = PXDatabase.GetSlot<Numberings>(typeof(Numberings).Name, typeof(Numbering));
				if (items != null && keyValue != null)
				{
					foreach (KeyValuePair<string, string> item in items.GetNumberings())
					{
						if (item.Value == (string)keyValue || GetNewNumberSymbol(item.Key) == (string)keyValue)
							throw new PXException(Messages.DocumentNbrEqualNewSymbol, (string)keyValue);
					}
				}

				return;
			}

			if (_dateTime == null)
			{
				Exception ex;

				if (!string.IsNullOrEmpty(_emptyDateMessage))
					ex = new AutoNumberException(_emptyDateMessage);
				else
				{
					PXFieldState state = (PXFieldState)sender.GetStateExt(e.Row, _dateField);
					ex = new AutoNumberException(Common.Messages.MustHaveValue, state?.DisplayName ?? _dateField);
				}
				
				sender.RaiseExceptionHandling(_dateField, e.Row, null, ex);
				throw ex;
			}

			if (_numberingID != null && _dateTime != null)
			{
                
                NewNumber = GetNextNumber(sender, e.Row, _numberingID, _dateTime, NewNumber, out LastNbr, out WarnNbr, out NumberingSEQ);

                if (NewNumber.CompareTo(WarnNbr) >= 0)
				{
					PXUIFieldAttribute.SetWarning(sender, e.Row, _FieldName, Messages.WarningNumReached);
				}
                _KeyToAbort = sender.GetValue(e.Row, _FieldName);
				sender.SetValue(e.Row, _FieldName, NewNumber);
			}
			else if (string.IsNullOrEmpty(NewNumber = (string)sender.GetValue(e.Row, _FieldName)) || string.Equals(NewNumber, NotSetNumber))
			{
                throw new AutoNumberException(CS.Messages.CantAutoNumberSpecific, _numberingID);
			}
		}

		public static string GetNextNumber(PXCache sender, object data, string numberingID, DateTime? dateTime)
		{
			string LastNbr;
			string WarnNbr;
			int? NumberingSEQ;

            return GetNextNumber(sender, data, numberingID, dateTime, null, out LastNbr, out WarnNbr, out NumberingSEQ);
        }

        protected static string GetNextNumberInt(PXCache sender, object data, string numberingID, DateTime? dateTime, string lastAssigned, out string LastNbr, out string WarnNbr, out int? NumberingSEQ)
		{
			if (numberingID != null && dateTime != null)
			{
				int? branchID = sender.Graph.Accessinfo.BranchID;
				if (data != null && sender.Fields.Contains("BranchID"))
				{
					object state = sender.GetStateExt(data, "BranchID");
					if (state is PXFieldState && ((PXFieldState)state).Required == true)
					{
						branchID = (int?)sender.GetValue(data, "BranchID");
					}
				}


                NumberingSequence sequence = GetNumberingSequence(numberingID, branchID, dateTime);
                if (sequence == null)
                    throw new AutoNumberException(CS.Messages.CantAutoNumberSpecific, numberingID);

                LastNbr = sequence.LastNbr;
                WarnNbr = sequence.WarnNbr;
                NumberingSEQ = sequence.NumberingSEQ;

                string newNumber = NextNumber(LastNbr, sequence.NbrStep ?? 0);
				if (String.Equals(lastAssigned, newNumber, StringComparison.InvariantCultureIgnoreCase))
				{
                    newNumber = NextNumber(newNumber, sequence.NbrStep ?? 0);
				}

				if (newNumber.CompareTo(sequence.EndNbr) >= 0)
				{
					throw new PXException(Messages.EndOfNumberingReached, numberingID);
				}

				try
				{
					if (LastNbr != sequence.StartNbr)
					{
						if (!PXDatabase.Update<NumberingSequence>(
							new PXDataFieldAssign<NumberingSequence.lastNbr>(newNumber),
							new PXDataFieldRestrict<NumberingSequence.numberingID>(numberingID),
							new PXDataFieldRestrict<NumberingSequence.numberingSEQ>(NumberingSEQ),
							new PXDataFieldRestrict<NumberingSequence.lastNbr>(LastNbr),
							PXDataFieldRestrict.OperationSwitchAllowed))
						{
							PXDatabase.Update<NumberingSequence>(
								new PXDataFieldAssign<NumberingSequence.nbrStep>(sequence.NbrStep),
								new PXDataFieldRestrict<NumberingSequence.numberingID>(numberingID),
								new PXDataFieldRestrict<NumberingSequence.numberingSEQ>(NumberingSEQ));
							using (PXDataRecord record = PXDatabase.SelectSingle<NumberingSequence>(
								new PXDataField<NumberingSequence.lastNbr>(),
								new PXDataFieldValue<NumberingSequence.numberingID>(numberingID),
								new PXDataFieldValue<NumberingSequence.numberingSEQ>(NumberingSEQ)))
							{
								if (record != null)
								{
									LastNbr = record.GetString(0);
									newNumber = NextNumber(LastNbr, sequence.NbrStep ?? 0);
									if (newNumber.CompareTo(sequence.EndNbr) >= 0)
									{
										throw new PXException(Messages.EndOfNumberingReached, numberingID);
									}
								}
							}
							PXDatabase.Update<NumberingSequence>(
								new PXDataFieldAssign<NumberingSequence.lastNbr>(newNumber),
								new PXDataFieldRestrict<NumberingSequence.numberingID>(numberingID),
								new PXDataFieldRestrict<NumberingSequence.numberingSEQ>(NumberingSEQ));
						}
					}
					else
					{
						PXDatabase.Update<NumberingSequence>(
							new PXDataFieldAssign<NumberingSequence.lastNbr>(newNumber),
							new PXDataFieldRestrict<NumberingSequence.numberingID>(numberingID),
							new PXDataFieldRestrict<NumberingSequence.numberingSEQ>(NumberingSEQ),
							PXDataFieldRestrict.OperationSwitchAllowed);
					}
				}
				catch (PXDbOperationSwitchRequiredException)
				{
					PXDatabase.Insert<NumberingSequence>(
						new PXDataFieldAssign<NumberingSequence.endNbr>(PXDbType.VarChar, 15, sequence.EndNbr),
						new PXDataFieldAssign<NumberingSequence.lastNbr>(PXDbType.VarChar, 15, newNumber),
						new PXDataFieldAssign<NumberingSequence.warnNbr>(PXDbType.VarChar, 15, sequence.WarnNbr),
						new PXDataFieldAssign<NumberingSequence.nbrStep>(PXDbType.Int, 4, sequence.NbrStep ?? 0),
						new PXDataFieldAssign<NumberingSequence.startNbr>(PXDbType.VarChar, 15, sequence.StartNbr),
						new PXDataFieldAssign<NumberingSequence.startDate>(PXDbType.DateTime, sequence.StartDate),
						new PXDataFieldAssign<NumberingSequence.createdByID>(PXDbType.UniqueIdentifier, 16, sequence.CreatedByID),
						new PXDataFieldAssign<NumberingSequence.createdByScreenID>(PXDbType.Char, 8, sequence.CreatedByScreenID),
						new PXDataFieldAssign<NumberingSequence.createdDateTime>(PXDbType.DateTime, 8, sequence.CreatedDateTime),
						new PXDataFieldAssign<NumberingSequence.lastModifiedByID>(PXDbType.UniqueIdentifier, 16, sequence.LastModifiedByID),
						new PXDataFieldAssign<NumberingSequence.lastModifiedByScreenID>(PXDbType.Char, 8, sequence.LastModifiedByScreenID),
						new PXDataFieldAssign<NumberingSequence.lastModifiedDateTime>(PXDbType.DateTime, 8, sequence.LastModifiedDateTime),
						new PXDataFieldAssign<NumberingSequence.numberingID>(PXDbType.VarChar, 10, numberingID),
						new PXDataFieldAssign<NumberingSequence.nBranchID>(PXDbType.Int, 4, sequence.NBranchID)
						);
				}

				return newNumber;
			}

			LastNbr = null;
			WarnNbr = null;
			NumberingSEQ = null;

			return null;
		}

		protected static string GetNextNumber(PXCache sender, object data, string numberingID, DateTime? dateTime, string lastAssigned, out string LastNbr, out string WarnNbr, out int? NumberingSEQ)
		{
            if (IS_SEPARATE_SCOPE)
            {
                using (new PXConnectionScope())
                {
                    using (PXTransactionScope ts = new PXTransactionScope())
                    {
                        String NewNumber = GetNextNumberInt(sender, data, numberingID, dateTime, lastAssigned, out LastNbr, out WarnNbr, out NumberingSEQ);
                        ts.Complete();
                        return NewNumber;
                    }
                }
            }
            else
                return GetNextNumberInt(sender, data, numberingID, dateTime, lastAssigned, out LastNbr, out WarnNbr, out NumberingSEQ);
        }

		public virtual void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert && e.TranStatus == PXTranStatus.Aborted && UserNumbering != true)
			{
				if (e.Exception is PXLockViolationException)
				{
					try
					{
						string number = sender.GetValue(e.Row, _FieldOrdinal) as string;
						if (!String.IsNullOrEmpty(number) && number == NewNumber)
						{
							PXDatabase.Update<NumberingSequence>(
								new PXDataFieldAssign<NumberingSequence.lastNbr>( number),
								new PXDataFieldRestrict<NumberingSequence.numberingID>( _numberingID),
								new PXDataFieldRestrict<NumberingSequence.numberingSEQ>( NumberingSEQ),
								new PXDataFieldRestrict<NumberingSequence.lastNbr>(LastNbr));
							((PXLockViolationException)e.Exception).Retry = true;
						}
					}
					catch
					{
					}
				}
				if (_KeyToAbort != null)
                {
					sender.SetValue(e.Row, _FieldOrdinal, _KeyToAbort);
				}
			}
            if (e.TranStatus != PXTranStatus.Open)
            {
                _KeyToAbort = null;
            }
		}

		void IPXRowInsertingSubscriber.RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			string oldValue = (string)sender.GetValue(e.Row, _FieldOrdinal);

			if (UserNumbering == true && oldValue == null && sender.Graph.UnattendedMode && !e.ExternalCall)
			{
				throw new PXException(Messages.CantManualNumber, _numberingID);
			}
		}

		void IPXFieldSelectingSubscriber.FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				e.ReturnState = PXStringState.CreateInstance(e.ReturnState, null, null, _FieldName, null, -1, null, null, null, null, null);
			}
		}
		#region Implementation
		public static string NextNumber(string str, short count)
		{
			return NextNumber(str, (int)count);
		}

		public static string NextNumber(string str, int count)
		{
			int i;
			bool j = true;
			int intcount = Math.Abs(count);
            int sign = Math.Sign(count);

			StringBuilder bld = new StringBuilder();
			for (i = str.Length; i > 0; i--)
			{
				string c = str.Substring(i - 1, 1);

				if (Regex.IsMatch(c, "[^0-9]"))
				{
					j = false;
				}

				if (j && Regex.IsMatch(c, "[0-9]"))
				{
					int digit = Convert.ToInt16(c);

					string s_count = Convert.ToString(intcount);
					int digit2 = Convert.ToInt16(s_count.Substring(s_count.Length - 1, 1));

                    if (sign >= 0)
                    {
                        bld.Append((digit + digit2) % 10);

                        intcount -= digit2;
                        intcount += ((digit + digit2) - (digit + digit2) % 10);
                    }
                    else
                    {
                        bld.Append((10 + digit - digit2) % 10);

                        intcount -= digit2;
                        intcount -= ((digit - digit2) - (10 + digit - digit2) % 10);
                    }

					intcount /= 10;

					if (intcount == 0)
					{
						j = false;
					}
				}
				else
				{
					bld.Append(c);
				}
			}

			if (intcount != 0)
			{
                throw new AutoNumberException();
			}

			char[] chars = bld.ToString().ToCharArray();
			Array.Reverse(chars);
			return new string(chars);
		}

        public static bool CanNextNumber(string str)
        {
            try
            {
                NextNumber(str, 1);
                return true;
            }
            catch (AutoNumberException)
            { return false; }
        }
          
		public static string NextNumber(string str)
		{
            try
            {
                return NextNumber(str, 1);
            }
            catch (AutoNumberException)
            {
                return str;
            }
		}

		#endregion
	}

	#endregion

	#region TermsAttribute

	public class PXDateTime
	{
		protected int _day;
		protected int _month;
		protected int _year;
		protected DateTime _value;

		public static short DayOfWeekOrdinal(DayOfWeek dow)
		{
			switch (dow)
			{
				case DayOfWeek.Sunday:
					return 1;
				case DayOfWeek.Monday:
					return 2;
				case DayOfWeek.Tuesday:
					return 3;
				case DayOfWeek.Wednesday:
					return 4;
				case DayOfWeek.Thursday:
					return 5;
				case DayOfWeek.Friday:
					return 6;
				case DayOfWeek.Saturday:
					return 7;
				default:
					return 0;
			}
		}

		public static bool WeekDay(DayOfWeek dow)
		{
			switch (dow)
			{
				case DayOfWeek.Monday:
				case DayOfWeek.Tuesday:
				case DayOfWeek.Wednesday:
				case DayOfWeek.Thursday:
				case DayOfWeek.Friday:
					return true;
				case DayOfWeek.Saturday:
				case DayOfWeek.Sunday:
				default:
					return false;
			}
		}

		public static bool WeekEnd(DayOfWeek dow)
		{
			return !WeekDay(dow);
		}

		public static DateTime MakeDayOfWeek(short Year, short Month, short Week, short DayOfWeek)
		{
			if (Week == 5 && DayOfWeek == 8) //Last Weekday
			{
				DateTime d = new DateTime(Year, Month, 1).AddMonths(1).AddDays(-1);
				if (WeekDay(d.DayOfWeek))
				{
					return d;
				}
				else if (DayOfWeekOrdinal(d.DayOfWeek) == 1)
				{
					return d.AddDays(-2);
				}
				else //if (DayOfWeekOrdinal(d.DayOfWeek) == 7)
				{
					return d.AddDays(-1);
				}
			}
			else if (Week == 5 && DayOfWeek == 9) //Last Weekend
			{
				DateTime d = new DateTime(Year, Month, 1).AddMonths(1).AddDays(-1);
				if (WeekEnd(d.DayOfWeek))
				{
					return d;
				}
				else
				{
					return d.AddDays(1-DayOfWeekOrdinal(d.DayOfWeek));
				}
			}
			else if (Week == 5)
			{
				DateTime d = new DateTime(Year, Month, 1).AddMonths(1).AddDays(-1);

				if (DayOfWeekOrdinal(d.DayOfWeek) >= DayOfWeek)
				{
					return d.AddDays(DayOfWeek - DayOfWeekOrdinal(d.DayOfWeek));
				}
				else
				{
					return d.AddDays(DayOfWeek - (DayOfWeekOrdinal(d.DayOfWeek)) - 7);
				}
			}
			else if (DayOfWeek == 8) //Weekday
			{
				DateTime d = new DateTime(Year, Month, 1);

				if (WeekDay(d.DayOfWeek) && Week == 1)
				{
					return d;
				}
				else 
				{
					//2nd etc Weekday will be 2nd etc Monday
					DayOfWeek = 2;
				}
			}
			else if (DayOfWeek == 9) //WeekEnd
			{
				DateTime d = new DateTime(Year, Month, 1);

				if (WeekEnd(d.DayOfWeek) && Week == 1)
				{
					return d;
				}
				else
				{
					//2nd etc Weekend will be 2nd etc Saturday
					DayOfWeek = 7;
				}
			}

			{
				DateTime d = new DateTime(Year, Month, 1);

				if (DayOfWeekOrdinal(d.DayOfWeek) <= DayOfWeek)
				{
					d = d.AddDays(DayOfWeek - DayOfWeekOrdinal(d.DayOfWeek));
				}
				else
				{
					d = d.AddDays(7 - (DayOfWeekOrdinal(d.DayOfWeek) - DayOfWeek));
				}
				return d.AddDays(7 * (Week - 1));
			}
		}

		public static DateTime DatePlusMonthSetDay(DateTime Date, int Months, int Day)
		{
			DateTime ret = Date.AddMonths(Months);
			if (Day - ret.Day < 4)
			{
				ret = new PXDateTime(ret.Year, ret.Month, Day);
			}

			return ret;
		}

		public PXDateTime (int year, int month, int day)
		{
			_year = year;
			_month = month;
			_day = day;

			try
			{
				_value = new DateTime(year, month, day);
			}
			catch (ArgumentOutOfRangeException)
			{
				for (;;)
				{
					try
					{
						_value = new DateTime(year, month, --day);
						break;
					}
					catch (ArgumentOutOfRangeException)
					{
						if (day <= 28)
						{
							throw;
						}
					}
				}
			}

			_value = new DateTime(year, month, day);
		}

		public virtual DateTime AddMonths(int months)
		{
			return DatePlusMonthSetDay(_value, months, _day);
		}

		public static implicit operator DateTime(PXDateTime item)
		{
			return item._value;
		}
	}

	public class TermsAttribute : PXEventSubscriberAttribute, IPXFieldUpdatedSubscriber
	{
		protected Type _DocDate;
		protected Type _DueDate;
		protected Type _DiscDate;
		protected Type _CuryDocBal;
		protected Type _CuryDiscBal;

		public TermsAttribute(Type DocDate, Type DueDate, Type DiscDate, Type CuryDocBal, Type CuryDiscBal)
		{
			_DocDate = DocDate;
			_DueDate = DueDate;
			_DiscDate = DiscDate;
			_CuryDiscBal = CuryDiscBal;
			_CuryDocBal = CuryDocBal;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			if (_DocDate != null)
			{
				sender.Graph.FieldUpdated.AddHandler(BqlCommand.GetItemType(_DocDate), _DocDate.Name, CalcTerms);
			}
			if (_CuryDocBal != null)
			{
				SubscribeCalcDisc(sender);
			}
			if (_CuryDiscBal != null)
			{
				sender.Graph.FieldVerifying.AddHandler(BqlCommand.GetItemType(_CuryDiscBal), _CuryDiscBal.Name, VerifyDiscount);
			}
		}

		public virtual void FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CalcTerms(sender, e);
			CalcDisc(sender, e);
		}

		protected virtual void VerifyDiscount(PXCache sender, PXFieldVerifyingEventArgs e)

		{
			if (sender.Graph.IsCopyPasteContext)
				return;

			if (sender.Graph.Accessinfo.CuryViewState && e.ExternalCall)
			{
				e.NewValue = sender.GetValue(e.Row, _CuryDiscBal.Name);
			}

			if (_CuryDocBal != null && _CuryDiscBal != null && 
				e.NewValue != null && sender.GetValue(e.Row, _FieldName) != null)
			{
				object newrow = Activator.CreateInstance(BqlCommand.GetItemType(_CuryDiscBal));
				newrow = e.Row;

				CalcDisc(sender, new PXFieldUpdatedEventArgs(newrow, null, true));

				var CuryDocBal = (decimal?)sender.GetValue(newrow, _CuryDocBal.Name);
				var CuryDiscBal = (decimal?)sender.GetValue(newrow, _CuryDiscBal.Name);

				if ((decimal)e.NewValue > CuryDocBal)
				{
					throw new PXSetPropertyException(AP.Messages.Entry_LE, CuryDocBal.ToString());
				}
				else if ((decimal)e.NewValue > CuryDiscBal)
				{
					sender.RaiseExceptionHandling(_CuryDiscBal.Name, e.Row, e.NewValue, new PXSetPropertyException(Messages.TermsDiscountGreater, PXErrorLevel.Warning));
				}
			}
		}

		protected virtual void CalcDisc(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (_CuryDocBal != null && _CuryDiscBal != null &&
					sender.GetValue(e.Row, _FieldName) != null &&
			    sender.GetValue(e.Row, _CuryDocBal.Name) != null && !sender.Graph.IsCopyPasteContext)
			{
				string TermsID = (string)sender.GetValue(e.Row, _FieldName);
				decimal CuryDocBal = (decimal)sender.GetValue(e.Row, _CuryDocBal.Name);
				decimal CuryDiscBal = 0m;

				Terms terms = SelectTerms(sender.Graph, TermsID);
				if (terms != null && terms.InstallmentType == TermsInstallmentType.Single && CuryDocBal > 0m)
				{
					CuryDiscBal = PXDBCurrencyAttribute.Round(sender, e.Row, CuryDocBal * (decimal)terms.DiscPercent / 100, CMPrecision.TRANCURY);
				}

				sender.SetValue(e.Row, _CuryDiscBal.Name, CuryDiscBal);
				PXUIFieldAttribute.SetEnabled(sender, e.Row, _CuryDiscBal.Name, (terms.InstallmentType == TermsInstallmentType.Single));
			}
		}

		private void CalcTerms(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (_DueDate == null || _DiscDate == null)
			{ 
			}
			else if (sender.GetValue(e.Row, _FieldName) != null)
			{
				string TermsID = (string)sender.GetValue(e.Row, _FieldName);
				DateTime? NDocDate = (DateTime?)sender.GetValue(e.Row, _DocDate.Name);
				Terms terms = SelectTerms(sender.Graph, TermsID);

				DateTime? DueDate;
				DateTime? DiscDate;
				CalcTermsDates(terms, NDocDate, out DueDate, out DiscDate);
				
				sender.SetValueExt(e.Row, _DueDate.Name, DueDate);
				sender.SetValueExt(e.Row, _DiscDate.Name, DiscDate);
			}
			else
			{
				sender.SetValueExt(e.Row, _DueDate.Name, null);
				sender.SetValueExt(e.Row, _DiscDate.Name, null);
			}
		}

		public static void CalcTermsDates(Terms terms, DateTime? docDate, out DateTime? dueDate, out DateTime? discDate)
		{
			dueDate = null;
			discDate = null;
			if (docDate != null && terms != null)
			{
				DateTime DocDate = docDate.Value;
				switch (terms.DueType)
				{
					case TermsDueType.FixedNumberOfDays:
						dueDate = DocDate.AddDays((double)terms.DayDue00);
						break;
					case TermsDueType.Prox:
						DateTime sameDayOfNextMonth = DocDate.AddMonths(1);
						DateTime firstDayOfNextMonth = new DateTime(sameDayOfNextMonth.Year, sameDayOfNextMonth.Month, 1);
						dueDate = firstDayOfNextMonth.AddDays((double)terms.DayDue00);
						break;
					case TermsDueType.DayOfNextMonth:
						dueDate = new PXDateTime(DocDate.Year, DocDate.Month, (int)terms.DayDue00).AddMonths(1);
						break;
					case TermsDueType.DayOfTheMonth:
						int monthShift = DocDate.Day > (int)terms.DayDue00 ? 1 : 0;
						dueDate = new PXDateTime(DocDate.Year, DocDate.Month, (int)terms.DayDue00).AddMonths(monthShift);
						break;
					case TermsDueType.Custom:
						int nextmonth = 0;
						if (DocDate.Day >= terms.DayFrom00 && DocDate.Day <= terms.DayTo00)
						{
							if (terms.DayDue00 <= terms.DayTo00)
							{
								nextmonth = 1;
							}
							dueDate = new PXDateTime(DocDate.Year, DocDate.Month, (int)terms.DayDue00).AddMonths(nextmonth);
						}
						if (DocDate.Day >= terms.DayFrom01 && DocDate.Day <= terms.DayTo01)
						{
							if (terms.DayDue01 <= terms.DayTo01)
							{
								nextmonth = 1;
							}
							dueDate = new PXDateTime(DocDate.Year, DocDate.Month, (int)terms.DayDue01).AddMonths(nextmonth);
						}
						break;
					case TermsDueType.EndOfMonth:
                        dueDate = new DateTime(DocDate.Year, DocDate.Month, 1).AddMonths(1).AddDays(-1);
						break;
					case TermsDueType.EndOfNextMonth:
                        dueDate = new DateTime(DocDate.Year, DocDate.Month, 1).AddMonths(2).AddDays(-1);
						break;
					case TermsDueType.FixedNumberOfDaysPlusDayOfNextMonth:
                        dueDate = DocDate.AddDays((double)terms.DayDue00).GetEndOfMonth();
                        if(terms.DayDue01 > 0)
                        {
                            dueDate = dueDate.Value.SetDayInMonthAfter(terms.DayDue01.Value);
                        }
						break;
					default:
						break;
				}

				if (terms.InstallmentType == TermsInstallmentType.Multiple)
				{
					discDate = dueDate;
				}
				else
				{
				switch (terms.DiscType)
				{
					case TermsDueType.FixedNumberOfDays:
						discDate = DocDate.AddDays((double)terms.DayDisc);
						break;
					case TermsDueType.Prox:
						DateTime sameDayOfNextMonth = DocDate.AddMonths(1);
						DateTime firstDayOfNextMonth = new DateTime(sameDayOfNextMonth.Year, sameDayOfNextMonth.Month, 1);
						discDate = firstDayOfNextMonth.AddDays((double)terms.DayDisc);
						break;
					case TermsDueType.DayOfNextMonth:
						discDate = new PXDateTime(DocDate.Year, DocDate.Month, (int)terms.DayDisc).AddMonths(1);
						break;
					case TermsDueType.DayOfTheMonth:
					    int monthShift;

							if (terms.DueType == TermsDueType.DayOfNextMonth && DocDate.Day <= (int)terms.DayDue00)
								monthShift = DocDate.Day >= (int)terms.DayDisc ? 1 : 0;
                        else if (terms.DueType == TermsDueType.EndOfNextMonth)
                            monthShift = DocDate.Day >= (int)terms.DayDisc ? 1 : 0;
                        else
                            monthShift = DocDate.Day > (int)terms.DayDue00 ? 1 : 0;
						discDate = new PXDateTime(DocDate.Year, DocDate.Month, (int)terms.DayDisc).AddMonths(monthShift);
						break;
					case TermsDueType.EndOfMonth:
                        discDate = new DateTime(DocDate.Year, DocDate.Month, 1).AddMonths(1).AddDays(-1);
						break;
					case TermsDueType.EndOfNextMonth:
                        discDate = new DateTime(DocDate.Year, DocDate.Month, 1).AddMonths(2).AddDays(-1);
						break;
					case TermsDueType.FixedNumberOfDaysPlusDayOfNextMonth:
                        dueDate = DocDate.AddDays((double)terms.DayDue00).GetEndOfMonth();
                        if (terms.DayDue01 > 0)
                        {
                            dueDate = dueDate.Value.SetDayInMonthAfter(terms.DayDue01.Value);
                        }
						break;
					default:
						break;
				}
				}

				if (discDate > dueDate)
				{
					discDate = dueDate;
				}
			}			
		}

		public static Terms SelectTerms(PXGraph graph, string TermsID)
		{
			return (Terms)PXSelect<Terms, Where<Terms.termsID, Equal<Required<Terms.termsID>>>>.Select(graph, TermsID);
		}

		public static PXResultset<TermsInstallments> SelectInstallments(PXGraph graph, Terms terms, DateTime firstdate)
		{
			PXResultset<TermsInstallments> ret = new PXResultset<TermsInstallments>();
			switch (terms.InstallmentMthd)
			{
				case TermsInstallmentMethod.AllTaxInFirst:
				case TermsInstallmentMethod.EqualParts:
					for (int i = 0; i < terms.InstallmentCntr; i++)
					{
						TermsInstallments inst = new TermsInstallments();
						inst.InstallmentNbr = (short)(i + 1);
						inst.InstPercent = 100 / (decimal)terms.InstallmentCntr;
						TimeSpan diff;

						switch (terms.InstallmentFreq)
						{
							case TermsInstallmentFrequency.Weekly:
								inst.InstDays = (short)(7 * i);
								break;
							case TermsInstallmentFrequency.SemiMonthly:
								if (i % 2 == 0)
								{
									diff = firstdate.AddMonths((int)i / 2) - firstdate;
									inst.InstDays = (short)diff.Days;
								}
								else
								{
									diff = firstdate.AddMonths((int)(i - 1) / 2) - firstdate;
									inst.InstDays = (short)(diff.Days + 14);
								}
								break;
							case TermsInstallmentFrequency.Monthly:
								diff = firstdate.AddMonths(i) - firstdate;
								inst.InstDays = (short)diff.Days;
								break;
						}
						ret.Add(new PXResult<TermsInstallments>(inst));
					}
					return ret;
				case TermsInstallmentMethod.SplitByPercents:
                    short j = 0;
                    foreach (TermsInstallments inst in PXSelectReadonly<TermsInstallments, Where<TermsInstallments.termsID, Equal<Required<TermsInstallments.termsID>>>, OrderBy<Asc<TermsInstallments.instDays>>>.Select(graph, terms.TermsID))
                    {
                        inst.InstallmentNbr = ++j;
                        ret.Add(new PXResult<TermsInstallments>(inst));
                    }
                    return ret;
				default:
					return null;
			}
		}

		#region UnsubscribeCalcDiscScope

		protected virtual void UnsubscribeCalcDisc(PXCache sender)
		{
			if (_CuryDocBal != null)
			{
				sender.Graph.FieldUpdated.RemoveHandler(BqlCommand.GetItemType(_CuryDocBal), _CuryDocBal.Name, CalcDisc);
			}
		}

		protected virtual void SubscribeCalcDisc(PXCache sender)
		{
			if (_CuryDocBal != null)
			{
				sender.Graph.FieldUpdated.AddHandler(BqlCommand.GetItemType(_CuryDocBal), _CuryDocBal.Name, CalcDisc);
			}
		}

		public class UnsubscribeCalcDiscScope : IDisposable
		{
			private readonly PXCache _Cache;
			private readonly TermsAttribute _Attribute;

			public UnsubscribeCalcDiscScope(PXCache sender)
			{
				_Cache = sender;
				foreach (PXEventSubscriberAttribute attr in _Cache.GetAttributesReadonly(null))
				{
					if (attr is TermsAttribute)
					{
						_Attribute = (TermsAttribute)attr;
						_Attribute.UnsubscribeCalcDisc(_Cache);
						break;
					}
				}
			}

			#region IDisposable Members

			public void Dispose()
			{
				_Attribute?.SubscribeCalcDisc(_Cache);
			}

			#endregion
		}

		#endregion
	}

	#endregion

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
	#region PXDBIntAsStringAttribute
	public class PXDBIntAsStringAttribute: PXDBIntAttribute, IPXFieldSelectingSubscriber 
	{
		#region State
		protected int _Length=-1;
		protected string _InputMask="";
		protected string _DisplayFormat="";
		public int Length
		{
			get
			{
				return _Length;
			}
		}
		public string InputMask
		{
			get
			{
				return _InputMask;
			}
			set
			{
				_InputMask = value;
			}
		}
		public string DisplayFormat
		{
			get
			{
				return _InputMask;
			}
			set
			{
				_InputMask = value;
			}
		}
		protected bool DoConversion 
		{
			get { return (_Length > 0); }
		}
		#endregion
		#region Ctor
		public PXDBIntAsStringAttribute(int aLength) : base() 
		{
			this._Length = aLength;
			if(string.IsNullOrEmpty(this._InputMask))
				this._InputMask = this._InputMask.PadRight(this._Length, '#');
			if (string.IsNullOrEmpty(this._DisplayFormat))
				this._DisplayFormat = "D" + this._Length.ToString();
		}
		public PXDBIntAsStringAttribute() : base() { }
		#endregion
		#region IPXFieldSelectingSubscriber Members

		void IPXFieldSelectingSubscriber.FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				e.ReturnState = PXStringState.CreateInstance(e.ReturnState, this._Length, null, _FieldName, _IsKey, null, _InputMask, null, null, null, null);
			}
			if (e.ReturnValue != null)
				e.ReturnValue = ((int)e.ReturnValue).ToString(this._DisplayFormat);
		}
		#endregion

	}
	#endregion


	#region LocationRawAttribute
    	
    [PXDBString(InputMask = "", IsUnicode = true)]
	[PXUIField(DisplayName = "Location ID", Visibility = PXUIVisibility.Visible)]
	public sealed class LocationRawAttribute : AcctSubAttribute, IPXRowPersistedSubscriber
	{
		public string DimensionName = "LOCATION";
		/*public LocationRawAttribute()
			: base()
		{

			Type SearchType = typeof(Search<Location.locationCD, Where<boolTrue, Equal<boolTrue>>>);
			_Attributes.Add(new PXDimensionSelectorAttribute(DimensionName, SearchType, typeof(Location.locationCD),
					new Type[] { typeof(Location.locationCD), typeof(Location.descr) },
					new string[] { "Location ID", "Description" }
				));
			_SelAttrIndex = _Attributes.Count - 1;

			((PXDimensionSelectorAttribute)_Attributes[_SelAttrIndex]).ViewName = "_Location_Contact_Address_";
			((PXDimensionSelectorAttribute)_Attributes[_SelAttrIndex]).CacheGlobal = true;
		}*/
		public LocationRawAttribute(Type WhereType): base() 
		{
			Type SearchType =
				BqlCommand.Compose(	typeof(Search<,>),typeof(Location.locationCD),WhereType);
			_Attributes.Add(new PXDimensionSelectorAttribute(DimensionName, SearchType, typeof(Location.locationCD),
			                                                 typeof(Location.locationCD), typeof(Location.descr)
			                	));
			_SelAttrIndex = _Attributes.Count - 1;
			((PXDimensionSelectorAttribute)_Attributes[_SelAttrIndex]).CacheGlobal = true;

		}



		void IPXRowPersistedSubscriber.RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (e.TranStatus == PXTranStatus.Completed)
			{
				PXSelectorAttribute.ClearGlobalCache<Location>();
			}
		}
	}

	#endregion

	#region LocationIDAttribute

	[PXDBInt()]
	[PXUIField(DisplayName = "Location", Visibility = PXUIVisibility.Visible, FieldClass="LOCATION")]
	public class LocationIDAttribute : LocationIDBaseAttribute
	{
		public LocationIDAttribute()
			: base(typeof(Where<boolTrue, Equal<boolTrue>>))
		{
		}

		public LocationIDAttribute(Type WhereType)
			: base(WhereType)
		{
		}

		public LocationIDAttribute(Type WhereType, Type JoinType)
			: base(WhereType, JoinType)
		{
	}
	}

	public class LocationIDBaseAttribute : AcctSubAttribute
	{
		public const string DimensionName = "LOCATION";
		protected const string CS_VIEW_NAME = "_Location_Contact_Address_";

		public LocationIDBaseAttribute()
			: this(typeof(Where<boolTrue, Equal<boolTrue>>))
		{
		}

		public LocationIDBaseAttribute(Type WhereType)
			: base()
		{
			Type SearchType =
				BqlCommand.Compose(
					typeof(Search<,>),
					typeof(Location.locationID),
					/*typeof(LeftJoin<Contact, On<Contact.bAccountID, Equal<SalesPerson.bAccountID>, And<Contact.contactID, Equal<SalesPerson.contactID>>>, 
							LeftJoin<Address, On<Address.bAccountID, Equal<SalesPerson.bAccountID>, And<Address.addressID, Equal<Contact.defAddressID>>>>>),*/
					WhereType);

			var attribute = new PXDimensionSelectorAttribute(
				DimensionName, SearchType, typeof(Location.locationCD), typeof(Location.locationCD), typeof(Location.descr));
			_Attributes.Add(attribute);
			_SelAttrIndex = _Attributes.Count - 1;
		}
		
		public LocationIDBaseAttribute(Type WhereType, Type JoinType)
			: base()
		{
			Type SearchType =
				BqlCommand.Compose(
					typeof(Search2<,,>),
					typeof(Location.locationID),
					JoinType,
					WhereType);

			_Attributes.Add(new PXDimensionSelectorAttribute(DimensionName, SearchType, typeof(Location.locationCD),
			                                                 typeof(Location.locationCD), typeof(Location.descr)
			                	));
			_SelAttrIndex = _Attributes.Count - 1;
		}
		
		public override bool DirtyRead
		{
			get
			{
				return (_SelAttrIndex == -1) ? false : ((PXDimensionSelectorAttribute)_Attributes[_SelAttrIndex]).DirtyRead;
			}
			set
			{
				if (_SelAttrIndex != -1)
					((PXDimensionSelectorAttribute)_Attributes[_SelAttrIndex]).DirtyRead = value;
			}
		}
	}

	#endregion
	
	#region SegmentEditMaskAttribute

	public class SegmentEditMaskAttribute : PXStringListAttribute
	{
		public SegmentEditMaskAttribute()
			: base("?;Alpha,9;Numeric,a;Alphanumeric,C;Unicode")
		{
		}

		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			base.FieldSelecting(sender, e);

			Segment segment;
			if (e.ReturnState is PXStringState && (segment = e.Row as Segment) != null && segment.ParentDimensionID != null)
			{
				Segment parent = PXSelectReadonly<Segment,
                    Where<Segment.dimensionID, Equal<Current<Segment.parentDimensionID>>, 
						And<Segment.segmentID, Equal<Current<Segment.segmentID>>>>>.
						SelectSingleBound(sender.Graph, new object[]{segment});
				if (parent != null)
				{
					var state = e.ReturnState as PXStringState;
					var values = state.AllowedValues;
					var labels = state.AllowedLabels;
					switch (parent.EditMask)
					{
						case "?":
							e.ReturnState = PXStringState.CreateInstance(e.ReturnState, null, null, _FieldName, null, -1, null,
								new[] { values[0] }, 
								new[] { labels[0] }, 
								null, null);
							break;
						case "9":
							e.ReturnState = PXStringState.CreateInstance(e.ReturnState, null, null, _FieldName, null, -1, null,
								new[] { values[1] }, 
								new[] { labels[1] }, 
								null, null);
							break;
						case "a":
							e.ReturnState = PXStringState.CreateInstance(e.ReturnState, null, null, _FieldName, null, -1, null,
								new[] { values[0], values[1], values[2] }, 
								new[] { labels[0], labels[1], labels[2] }, 
								null, null);
							break;
					}
				}
			}
		}
	}

	#endregion

	[PXDBBinary]
	[PXSelector(typeof(Search<PX.SM.RelationGroup.groupMask,
		Where<PX.SM.RelationGroup.active, Equal<boolTrue>>>),
		SubstituteKey = typeof(PX.SM.RelationGroup.groupName),
		DescriptionField = typeof(PX.SM.RelationGroup.description))]
	[PXUIField(DisplayName = "Default Restriction Group")]
	public class SingleGroupAttribute : PXAggregateAttribute, IPXRowSelectedSubscriber, IPXFieldSelectingSubscriber
	{
		public virtual void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row != null)
			{
				byte[] val = sender.GetValue(e.Row, _FieldOrdinal) as byte[];
				if (val != null && isMultiple(val))
				{
					sender.SetAltered(_FieldName, true);
				}
			}
		}
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			byte[] val = e.ReturnValue as byte[];
			if (val != null)
			{
				if (val.Length == 0)
				{
					e.ReturnValue = null;
				}
				else if (isMultiple(val))
				{
					e.ReturnValue = PXMessages.LocalizeNoPrefix(Messages.GroupMultiple);
					if (e.ReturnState is PXFieldState)
					{
						((PXFieldState)e.ReturnState).Enabled = false;
					}
				}
			}
			else if (e.ReturnValue is string && String.Equals((string)e.ReturnValue, "System.Byte[]") && e.Row != null && sender.GetValue(e.Row, _FieldOrdinal) != null)
			{
				e.ReturnValue = null;
			}
		}
		public virtual void SubstituteFieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue is string && e.Row != null)
			{
				byte[] val = sender.GetValue(e.Row, _FieldOrdinal) as byte[];
				if (val != null && isMultiple(val))
				{
					e.NewValue = val;
					e.Cancel = true;
				}
			}
		}
		public override void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers)
		{
			base.GetSubscriber<ISubscriber>(subscribers);
			if (typeof(ISubscriber) == typeof(IPXFieldVerifyingSubscriber))
			{
				for (int i = 0; i < _Attributes.Count; i++)
				{
					if (_Attributes[i] is PXSelectorAttribute)
					{
						subscribers.Remove(_Attributes[i] as ISubscriber);
					}
				}
			}
		}
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldUpdating.AddHandler(sender.GetItemType(), _FieldName, SubstituteFieldUpdating);
		}
		private bool isMultiple(byte[] val)
		{
			int cnt = 0;
			for (int i = 0; i < val.Length; i++)
			{
				if (val[i] != 0)
				{
					cnt++;
					if (val[i] != 1 && val[i] != 2 && val[i] != 4 && val[i] != 8 && val[i] != 16 && val[i] != 32 && val[i] != 64 && val[i] != 128)
					{
						cnt++;
					}
					if (cnt > 1)
					{
						return true;
					}
				}
			}
			return false;
		}
		public static void PopulateNeighbours<Field>(PXSelectBase currentSelect, PXSelectBase<PX.SM.Neighbour> neighboursSelect, Type left, Type right)
			where Field : IBqlField
		{
			PX.SM.RelationGroup group = (PX.SM.RelationGroup)PXSelectorAttribute.Select<Field>(currentSelect.Cache, currentSelect.Cache.Current);
			if (group != null)
			{
				foreach (PX.SM.Neighbour item in neighboursSelect.Select())
				{
					if (item.LeftEntityType == left.FullName
						|| item.RightEntityType == right.FullName)
					{
						if (item.CoverageMask.Length < group.GroupMask.Length)
						{
							byte[] mask = item.CoverageMask;
							Array.Resize<byte>(ref mask, group.GroupMask.Length);
							item.CoverageMask = mask;
						}
						if (item.InverseMask.Length < group.GroupMask.Length)
						{
							byte[] mask = item.InverseMask;
							Array.Resize<byte>(ref mask, group.GroupMask.Length);
							item.InverseMask = mask;
						}
						if (item.WinCoverageMask.Length < group.GroupMask.Length)
						{
							byte[] mask = item.WinCoverageMask;
							Array.Resize<byte>(ref mask, group.GroupMask.Length);
							item.WinCoverageMask = mask;
						}
						if (item.WinInverseMask.Length < group.GroupMask.Length)
						{
							byte[] mask = item.WinInverseMask;
							Array.Resize<byte>(ref mask, group.GroupMask.Length);
							item.WinInverseMask = mask;
						}
						for (int i = 0; i < group.GroupMask.Length; i++)
						{
							if (group.GroupType == "EE")
							{
								item.InverseMask[i] = (byte)(item.InverseMask[i] | group.GroupMask[i]);
							}
							else if (group.GroupType == "IE")
							{
								item.CoverageMask[i] = (byte)(item.CoverageMask[i] | group.GroupMask[i]);
							}
							else if (group.GroupType == "EO")
							{
								item.WinInverseMask[i] = (byte)(item.WinInverseMask[i] | group.GroupMask[i]);
							}
							else if (group.GroupType == "IO")
							{
								item.WinCoverageMask[i] = (byte)(item.WinCoverageMask[i] | group.GroupMask[i]);
							}
						}
						neighboursSelect.Update(item);
					}
				}
				{
					PX.SM.Neighbour item = new PX.SM.Neighbour();
					item.LeftEntityType = left.FullName;
					item.RightEntityType = right.FullName;
					if (neighboursSelect.Locate(item) == null)
					{
						if (group.GroupType == "EE")
						{
							item.InverseMask = group.GroupMask;
							item.CoverageMask = new byte[group.GroupMask.Length];
							item.WinInverseMask = new byte[group.GroupMask.Length];
							item.WinCoverageMask = new byte[group.GroupMask.Length];
						}
						else if (group.GroupType == "IE")
						{
							item.CoverageMask = group.GroupMask;
							item.InverseMask = new byte[group.GroupMask.Length];
							item.WinInverseMask = new byte[group.GroupMask.Length];
							item.WinCoverageMask = new byte[group.GroupMask.Length];
						}
						else if (group.GroupType == "EO")
						{
							item.WinInverseMask = group.GroupMask;
							item.WinCoverageMask = new byte[group.GroupMask.Length];
							item.InverseMask = new byte[group.GroupMask.Length];
							item.CoverageMask = new byte[group.GroupMask.Length];
						}
						else if (group.GroupType == "IO")
						{
							item.WinCoverageMask = group.GroupMask;
							item.WinInverseMask = new byte[group.GroupMask.Length];
							item.InverseMask = new byte[group.GroupMask.Length];
							item.CoverageMask = new byte[group.GroupMask.Length];
						}
						neighboursSelect.Insert(item);
					}
				}
			}
		}
	}


	#region AttributeValueAttribute

	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public sealed class AttributeValueAttribute : PXEventSubscriberAttribute, IPXRowSelectingSubscriber, IPXCommandPreparingSubscriber, IPXFieldSelectingSubscriber, IPXRowSelectedSubscriber
	{
		private readonly Type _attributeID;
		private readonly Type _refNoteId;

		public AttributeValueAttribute(Type attributeID, Type refNoteId) 
		{
			if (attributeID == null) throw new ArgumentNullException("attributeID");
			if (!typeof(IBqlField).IsAssignableFrom(attributeID))
				throw new ArgumentException(
					string.Format("'{0}' must implement '{1}' interface.", attributeID.GetLongName(), typeof(IBqlField).GetLongName()), "attributeID");
			if (refNoteId == null) throw new ArgumentNullException("entityNoteID");
			if (!typeof(IBqlField).IsAssignableFrom(refNoteId))
				throw new ArgumentException(
					string.Format("'{0}' must implement '{1}' interface.", refNoteId.GetLongName(), typeof(IBqlField).GetLongName()), "refNoteId");
			
			
			_attributeID = attributeID;
            _refNoteId = refNoteId;
		}

		public void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Select)
			{				
				e.Expr = new SubQuery(
					new Query()
					.Select<CSAnswers.value>()
					.From<CSAnswers>()
					.Where(new Column<CSAnswers.attributeID>().EQ(new SQLConst(GetAttributeID(sender)))
						.And(new Column<CSAnswers.refNoteID>().EQ(new Column(_refNoteId, new SimpleTable(e.Table ?? _BqlTable)))))
				).Embrace();
			}
		}

		private object GetAttributeID(PXCache sender)
		{
			var attValueCache = sender.Graph.Caches[BqlCommand.GetItemType(_attributeID)];
			return attValueCache.GetValue(attValueCache.Current, _attributeID.Name);
		}

		public void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			if (e.Row != null)
			{
				object value = e.Record.GetValue(e.Position);
				sender.SetValue(e.Row, _FieldOrdinal, value == null ? null : Convert.ChangeType(value, typeof(string)));
			}
			e.Position++;
		}

		public void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, typeof(string), false, true, null, null, null, null, 
					_FieldName, null, null, null, PXErrorLevel.Undefined, null, null, null, PXUIVisibility.Undefined, null, null, null);
			}
		}

		public void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (sender.GetStatus(e.Row) == PXEntryStatus.Inserted)
			{
				var entityNoteID = sender.GetValue(e.Row, _refNoteId.Name);
				object newValue = null;
				if (entityNoteID != null)
				{
					newValue = ((CSAnswers)PXSelect<CSAnswers,
						Where<CSAnswers.attributeID, Equal<Required<CSAnswers.attributeID>>>>.
						Search<CSAnswers.refNoteID>
						(sender.Graph, entityNoteID, GetAttributeID(sender))).
						With(att => att.Value);
				}
				sender.SetValue(e.Row, _FieldName, newValue);
			}
		}
	}

	#endregion


	[PXDBInt]
	[PXDBChildIdentity(typeof(RMDataSource.dataSourceID))]
	[PXUIField(DisplayName = "Data Source")]
	public class RMDataSourceGLAttribute : RMDataSourceAttribute
	{
		public override void TextFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			RMDataSource ds = e.Row as RMDataSource;
		
			if (ds == null) 
			{
				object key = sender.GetValue(e.Row, _FieldOrdinal);
				if (key != null)
				{
					ds = PXSelect<RMDataSource,
						Where<RMDataSource.dataSourceID, Equal<Required<RMDataSource.dataSourceID>>>>.Select(sender.Graph, key);			
				}
			}
							
			if (ds != null)
			{
				RMDataSourceGL dsGL = sender.Graph.Caches[typeof(RMDataSource)].GetExtension<RMDataSourceGL>(ds);				
				StringBuilder bld = new StringBuilder();

				if (dsGL.OrganizationID != null)				{
					Organization organization = PXSelect<Organization,
						Where<Organization.organizationID, Equal<Required<Organization.organizationID>>>>
						.Select(sender.Graph, dsGL.OrganizationID);
					if (organization != null)
						bld.Append(organization.OrganizationCD?.Trim());
				}
				if (dsGL.LedgerID != null)
					{
						Ledger ledger = PXSelect<Ledger,
							Where<Ledger.ledgerID, Equal<Required<Ledger.ledgerID>>>>
							.Select(sender.Graph, dsGL.LedgerID);
							if (ledger != null)
							{
								if (bld.Length > 0)
									bld.Append(", ");

								bld.Append(ledger.LedgerCD);
							}

					}
					if (!string.IsNullOrEmpty(dsGL.AccountClassID))
					{
						if (bld.Length > 0)
							bld.Append(", ");
						bld.Append(dsGL.AccountClassID);
					}
					#region start
					if (!String.IsNullOrEmpty(dsGL.StartAccount))
					{
						if (bld.Length > 0)
							bld.Append(", ");
						bld.Append(dsGL.StartAccount);
					}
					if (!String.IsNullOrEmpty(dsGL.StartSub))
					{
						if (bld.Length > 0)
							bld.Append(", ");
						bld.Append(dsGL.StartSub);
					}
					if (!String.IsNullOrEmpty(dsGL.StartBranch))
					{
						if (bld.Length > 0)
							bld.Append(", ");
						bld.Append(dsGL.StartBranch);
					}
					if (!String.IsNullOrEmpty(dsGL.StartPeriod))
					{
						if (bld.Length > 0)
							bld.Append(", ");
						bld.Append(dsGL.StartPeriod);
					}
					if ((dsGL.StartPeriodYearOffset ?? 0) != 0)
					{
						if (bld.Length > 0)
							bld.Append(", ");
						bld.Append(dsGL.StartPeriodYearOffset.Value);
					}
					if ((dsGL.StartPeriodOffset ?? 0) != 0)
					{
						if (bld.Length > 0)
							bld.Append(", ");
						bld.Append(dsGL.StartPeriodOffset.Value);
					}
					#endregion
					#region end
					if (bld.Length == 0)
					{
						if (!String.IsNullOrEmpty(dsGL.EndAccount))
						{
							if (bld.Length > 0)
								bld.Append(", ");
							bld.Append(dsGL.EndAccount);
						}
						if (!String.IsNullOrEmpty(dsGL.EndSub))
						{
							if (bld.Length > 0)
								bld.Append(", ");
							bld.Append(dsGL.EndSub);
						}
						if (!String.IsNullOrEmpty(dsGL.EndBranch))
						{
							if (bld.Length > 0)
								bld.Append(", ");
							bld.Append(dsGL.EndBranch);
						}
						if (!String.IsNullOrEmpty(dsGL.EndPeriod))
						{
							if (bld.Length > 0)
								bld.Append(", ");
							bld.Append(dsGL.EndPeriod);
						}
						if ((dsGL.EndPeriodYearOffset ?? 0) != 0)
						{
							if (bld.Length > 0)
								bld.Append(", ");
							bld.Append(dsGL.EndPeriodYearOffset.Value);
						}
						if (dsGL.EndPeriodOffset != null && dsGL.EndPeriodOffset != 0)
						{
							if (bld.Length > 0)
								bld.Append(", ");
							bld.Append(dsGL.EndPeriodOffset.Value);
						}
					}
					#endregion
					if (bld.Length == 0 && (ds.AmountType ?? BalanceType.NotSet) != BalanceType.NotSet)
					{
						switch (ds.AmountType.Value)
						{
							case BalanceType.Turnover:
                                bld.Append(CS.Messages.GetLocal(CS.Messages.Turnover));
								break;
							case BalanceType.Credit:
                                bld.Append(CS.Messages.GetLocal(CS.Messages.Credit));
								break;
							case BalanceType.Debit:
                                bld.Append(CS.Messages.GetLocal(CS.Messages.Debit));
								break;
							case BalanceType.BeginningBalance:
                                bld.Append(CS.Messages.GetLocal(CS.Messages.BegBalance));
								break;
							case BalanceType.EndingBalance:
                                bld.Append(CS.Messages.GetLocal(CS.Messages.EndingBalance));
								break;
                            case BalanceType.CuryTurnover:
                                bld.Append(CS.Messages.GetLocal(CS.Messages.CuryTurnover));
                                break;
                            case BalanceType.CuryCredit:
                                bld.Append(CS.Messages.GetLocal(CS.Messages.CuryCredit));
                                break;
                            case BalanceType.CuryDebit:
                                bld.Append(CS.Messages.GetLocal(CS.Messages.CuryDebit));
                                break;
                            case BalanceType.CuryBeginningBalance:
                                bld.Append(CS.Messages.GetLocal(CS.Messages.CuryBegBalance));
                                break;
                            case BalanceType.CuryEndingBalance:
                                bld.Append(CS.Messages.GetLocal(CS.Messages.CuryEndingBalance));
                                break;

						}
					}
					if (bld.Length == 0 && !string.IsNullOrEmpty(ds.RowDescription))
					{
						switch (ds.RowDescription)
						{
							case RowDescriptionType.Code:
                                bld.Append(CS.Messages.GetLocal(CS.Messages.Code));
								break;
							case RowDescriptionType.Description:
                                bld.Append(CS.Messages.GetLocal(CS.Messages.Description));
								break;
							case RowDescriptionType.CodeDescription:
                                bld.Append(CS.Messages.GetLocal(CS.Messages.CodeDescription));
								break;
							case RowDescriptionType.DescriptionCode:
                                bld.Append(CS.Messages.GetLocal(CS.Messages.DescriptionCode));
								break;
						}
					}
				e.ReturnValue = bld.Length > 0 ? bld.ToString() : "";
			}
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				e.ReturnState = PXStringState.CreateInstance(e.ReturnState, 100, null, _FieldName + "Text", false, null, null, null, null, null, null);
				((PXFieldState)e.ReturnState).Visible = false;
			}
		}
		public override void DataSourceSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (_RowParentCache != null && _RowParentCache.Current != null && _RowParentCache.Current is IRMType)
			{
				PXUIFieldAttribute.SetVisible<RMDataSourceGL.ledgerID>(sender, e.Row, ((IRMType)_RowParentCache.Current).RMType == RMType.GL);
				PXUIFieldAttribute.SetVisible<RMDataSourceGL.accountClassID>(sender, e.Row, ((IRMType)_RowParentCache.Current).RMType == RMType.GL);
				PXUIFieldAttribute.SetVisible<RMDataSourceGL.startAccount>(sender, e.Row, ((IRMType)_RowParentCache.Current).RMType == RMType.GL);
				PXUIFieldAttribute.SetVisible<RMDataSourceGL.endAccount>(sender, e.Row, ((IRMType)_RowParentCache.Current).RMType == RMType.GL);
				PXUIFieldAttribute.SetVisible<RMDataSourceGL.startSub>(sender, e.Row, ((IRMType)_RowParentCache.Current).RMType == RMType.GL);
				PXUIFieldAttribute.SetVisible<RMDataSourceGL.endSub>(sender, e.Row, ((IRMType)_RowParentCache.Current).RMType == RMType.GL);
				PXUIFieldAttribute.SetVisible<RMDataSourceGL.organizationID>(sender, e.Row, ((IRMType)_RowParentCache.Current).RMType == RMType.GL);				
				PXUIFieldAttribute.SetVisible<RMDataSourceGL.startBranch>(sender, e.Row, ((IRMType)_RowParentCache.Current).RMType == RMType.GL);
				PXUIFieldAttribute.SetVisible<RMDataSourceGL.endBranch>(sender, e.Row, ((IRMType)_RowParentCache.Current).RMType == RMType.GL);
				PXUIFieldAttribute.SetVisible<RMDataSource.rowDescription>(sender, e.Row, ((IRMType)_RowParentCache.Current).RMType == RMType.GL || ((IRMType)_RowParentCache.Current).RMType == RMType.PM);
				if ((e.Row != null))
					PXUIFieldAttribute.SetVisible<RMDataSource.rowDescription>(sender, e.Row, (((IRMType)_RowParentCache.Current).RMType == RMType.GL || ((IRMType)_RowParentCache.Current).RMType == RMType.PM) && ((RMDataSource)e.Row).Expand != ExpandType.Nothing);
			}
		}
		public virtual void EnsurePeriodLength(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (!e.Cancel)
			{
				string period = e.NewValue as string;
				if (period != null)
				{
					if (period.Length < 6)
					{
						period = period + new String(' ', 6 - period.Length);
					}
					else if (period.Length > 6)
					{
						period = period.Substring(0, 6);
					}
					e.NewValue = period.Substring(2) + period.Substring(0, 2);
				}
				e.Cancel = true;
			}
		}
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			_Cache.Graph.FieldVerifying.AddHandler<RMDataSourceGL.startAccount>(SuppressFieldVerifying);
			_Cache.Graph.FieldVerifying.AddHandler<RMDataSourceGL.startSub>(SuppressFieldVerifying);
			_Cache.Graph.FieldVerifying.AddHandler<RMDataSourceGL.startPeriod>(SuppressFieldVerifying);
			_Cache.Graph.FieldUpdating.AddHandler<RMDataSourceGL.startPeriod>(EnsurePeriodLength);
			_Cache.Graph.FieldVerifying.AddHandler<RMDataSourceGL.endAccount>(SuppressFieldVerifying);
			_Cache.Graph.FieldVerifying.AddHandler<RMDataSourceGL.endSub>(SuppressFieldVerifying);
			_Cache.Graph.FieldVerifying.AddHandler<RMDataSourceGL.endPeriod>(SuppressFieldVerifying);
			_Cache.Graph.FieldUpdating.AddHandler<RMDataSourceGL.endPeriod>(EnsurePeriodLength);
			_Cache.Graph.FieldVerifying.AddHandler<RMDataSourceGL.organizationID>(SuppressFieldVerifying);
			_Cache.Graph.FieldVerifying.AddHandler<RMDataSourceGL.startBranch>(SuppressFieldVerifying);
			_Cache.Graph.FieldVerifying.AddHandler<RMDataSourceGL.endBranch>(SuppressFieldVerifying);
		}
		protected override void DataSourceExpandSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_RowParentCache != null && _RowParentCache.Current != null && _RowParentCache.Current is IRMType)
			{
				e.ReturnState = PXStringState.CreateInstance(
					e.ReturnState,
					1,
					false,
					typeof(RMDataSource.expand).Name,
					false,
					0,
					null,
					new string[] { ExpandType.Nothing, ExpandType.Account, ExpandType.Sub },
					new string[] { CS.Messages.GetLocal(CS.Messages.Nothing), CS.Messages.GetLocal(CS.Messages.Account), CS.Messages.GetLocal(CS.Messages.Sub) },
					true,
					ExpandType.Nothing
					);
			}
		}
		protected override void DataSourceRowDescriptionSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_RowParentCache != null && _RowParentCache.Current != null && _RowParentCache.Current is IRMType)
			{
				e.ReturnState = PXStringState.CreateInstance(
					e.ReturnState, 
					1, 
					false, 
					typeof(RMDataSource.rowDescription).Name, 
					false, 
					0, 
					null,
					new string[] { RowDescriptionType.NotSet, RowDescriptionType.Code, RowDescriptionType.Description, RowDescriptionType.CodeDescription, RowDescriptionType.DescriptionCode }, 
					new string[] { CS.Messages.GetLocal(CS.Messages.NotSet),
								   CS.Messages.GetLocal(CS.Messages.Code),
								   CS.Messages.GetLocal(CS.Messages.Description),
								   CS.Messages.GetLocal(CS.Messages.CodeDescription),
								   CS.Messages.GetLocal(CS.Messages.DescriptionCode) }, 
					true,
					RowDescriptionType.NotSet);
			}
		}
		protected override void DataSourceAmountTypeSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_RowParentCache != null && _RowParentCache.Current != null && _RowParentCache.Current is IRMType)
			{
				e.ReturnState = PXIntState.CreateInstance(
					e.ReturnState,
					typeof(RMDataSource.amountType).Name,
					false,
					0,
					null,
					null,
					new int[] { BalanceType.NotSet, 
                                BalanceType.Turnover, 
                                BalanceType.Credit, 
                                BalanceType.Debit, 
                                BalanceType.BeginningBalance, 
                                BalanceType.EndingBalance, 
                                BalanceType.CuryTurnover, 
                                BalanceType.CuryCredit, 
                                BalanceType.CuryDebit, 
                                BalanceType.CuryBeginningBalance, 
                                BalanceType.CuryEndingBalance},
					new string[] { CS.Messages.GetLocal(CS.Messages.NotSet),
								   CS.Messages.GetLocal(CS.Messages.Turnover),
								   CS.Messages.GetLocal(CS.Messages.Credit),
								   CS.Messages.GetLocal(CS.Messages.Debit),
								   CS.Messages.GetLocal(CS.Messages.BegBalance),
								   CS.Messages.GetLocal(CS.Messages.EndingBalance),
                                   CS.Messages.GetLocal(CS.Messages.CuryTurnover),
								   CS.Messages.GetLocal(CS.Messages.CuryCredit),
								   CS.Messages.GetLocal(CS.Messages.CuryDebit),
								   CS.Messages.GetLocal(CS.Messages.CuryBegBalance),
								   CS.Messages.GetLocal(CS.Messages.CuryEndingBalance)},
					null,
					BalanceType.NotSet
					);					
			}
		}
	}
	
	[PXDBInt]
	[PXDBChildIdentity(typeof(RMDataSource.dataSourceID))]
	[PXUIField(DisplayName = "Data Source")]
	public class RMDataSourcePMAttribute : RMDataSourceGLAttribute
	{
		public override void TextFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_RowParentCache.Current != null)
			{
				if (((IRMType)_RowParentCache.Current).RMType == RMType.GL)
				{
					base.TextFieldSelecting(sender, e);
				}
				else
				{
					RMDataSource ds = e.Row as RMDataSource;

					if (ds == null)
					{
						object key = sender.GetValue(e.Row, _FieldOrdinal);
						if (key != null)
						{
							ds = PXSelect<RMDataSource,
								Where<RMDataSource.dataSourceID, Equal<Required<RMDataSource.dataSourceID>>>>
								.Select(sender.Graph, key);
						}
					}
					if (ds != null)
					{
						RMDataSourcePM dsPM = sender.Graph.Caches[typeof(RMDataSource)].GetExtension<RMDataSourcePM>(ds);
						RMDataSourceGL dsGL = sender.Graph.Caches[typeof(RMDataSource)].GetExtension<RMDataSourceGL>(ds);

						StringBuilder bld = new StringBuilder();

						if (_RowParentCache != null && _RowParentCache.Current != null && _RowParentCache.Current is IRMType && ((IRMType)_RowParentCache.Current).RMType == RMType.PM)
						{
							if (!String.IsNullOrEmpty(dsPM.StartAccountGroup))
							{
								if (bld.Length > 0)
									bld.Append(", ");
								bld.Append(dsPM.StartAccountGroup);
							}

							if (!String.IsNullOrEmpty(dsPM.StartProject))
							{
								if (bld.Length > 0)
									bld.Append(", ");
								bld.Append(dsPM.StartProject);
							}
							if (!String.IsNullOrEmpty(dsPM.StartProjectTask))
							{
								if (bld.Length > 0)
									bld.Append(", ");
								bld.Append(dsPM.StartProjectTask);
							}
							if (!String.IsNullOrEmpty(dsPM.StartInventory))
							{
								if (bld.Length > 0)
									bld.Append(", ");
								bld.Append(dsPM.StartInventory);
							}
							if (!String.IsNullOrEmpty(dsGL.StartPeriod))
							{
								if (bld.Length > 0)
									bld.Append(", ");
								bld.Append(dsGL.StartPeriod);
							}
							if ((dsGL.StartPeriodOffset ?? 0) != 0)
							{
								if (bld.Length > 0)
									bld.Append(", ");
								bld.Append(dsGL.StartPeriodOffset.Value);
							}
							if (bld.Length == 0)
							{
								if (!String.IsNullOrEmpty(dsPM.EndAccountGroup))
								{
									if (bld.Length > 0)
										bld.Append(", ");
									bld.Append(dsPM.EndAccountGroup);
								}
								if (!String.IsNullOrEmpty(dsPM.EndProject))
								{
									if (bld.Length > 0)
										bld.Append(", ");
									bld.Append(dsPM.EndProject);
								}
								if (!String.IsNullOrEmpty(dsPM.EndProjectTask))
								{
									if (bld.Length > 0)
										bld.Append(", ");
									bld.Append(dsPM.EndProjectTask);
								}
								if (!String.IsNullOrEmpty(dsGL.EndPeriod))
								{
									if (bld.Length > 0)
										bld.Append(", ");
									bld.Append(dsGL.EndPeriod);
								}
								if (dsGL.EndPeriodOffset != null && dsGL.EndPeriodOffset != 0)
								{
									if (bld.Length > 0)
										bld.Append(", ");
									bld.Append(dsGL.EndPeriodOffset.Value);
								}
							}

							if (bld.Length == 0 && (ds.AmountType ?? BalanceType.NotSet) != BalanceType.NotSet)
							{
								switch ((short)ds.AmountType)
								{
									case BalanceType.Amount:
                                        bld.Append(CS.Messages.GetLocal(CS.Messages.ActualAmount));
										break;
									case BalanceType.Quantity:
                                        bld.Append(CS.Messages.GetLocal(CS.Messages.ActualQuantity));
										break;

									case BalanceType.BudgetAmount:
                                        bld.Append(CS.Messages.GetLocal(CS.Messages.BudgetAmount));
										break;
									case BalanceType.BudgetQuantity:
                                        bld.Append(CS.Messages.GetLocal(CS.Messages.BudgetQuantity));
										break;
									case BalanceType.RevisedAmount:
                                        bld.Append(CS.Messages.GetLocal(CS.Messages.RevisedAmount));
										break;
									case BalanceType.RevisedQuantity:
                                        bld.Append(CS.Messages.GetLocal(CS.Messages.RevisedQuantity));
										break;

									case BalanceType.TurnoverAmount:
										bld.Append(CS.Messages.GetLocal(CS.Messages.AmountTurnover));
										break;
									case BalanceType.TurnoverQuantity:
                                        bld.Append(CS.Messages.GetLocal(CS.Messages.QuantityTurnover));
										break;
									case BalanceType.OriginalCommittedAmount:
										bld.Append(CS.Messages.GetLocal(CS.Messages.OriginalCommittedAmount));
										break;
									case BalanceType.OriginalCommittedQuantity:
										bld.Append(CS.Messages.GetLocal(CS.Messages.OriginalCommittedQuantity));
										break;
									case BalanceType.CommittedAmount:
										bld.Append(CS.Messages.GetLocal(CS.Messages.CommittedAmount));
										break;
									case BalanceType.CommittedQuantity:
										bld.Append(CS.Messages.GetLocal(CS.Messages.CommittedQuantity));
										break;
									case BalanceType.CommittedOpenAmount:
										bld.Append(CS.Messages.GetLocal(CS.Messages.CommittedOpenAmount));
										break;
									case BalanceType.CommittedOpenQuantity:
										bld.Append(CS.Messages.GetLocal(CS.Messages.CommittedOpenQuantity));
										break;
									case BalanceType.CommittedReceivedQuantity:
										bld.Append(CS.Messages.GetLocal(CS.Messages.CommittedReceivedQuantity));
										break;
									case BalanceType.CommittedInvoicedAmount:
										bld.Append(CS.Messages.GetLocal(CS.Messages.CommittedInvoicedAmount));
										break;
									case BalanceType.CommittedInvoicedQuantity:
										bld.Append(CS.Messages.GetLocal(CS.Messages.CommittedInvoicedQuantity));
										break;
									case BalanceType.ChangeOrderQuantity:
										bld.Append(CS.Messages.GetLocal(CS.Messages.ChangeOrderQuantity));
										break;
									case BalanceType.ChangeOrderAmount:
										bld.Append(CS.Messages.GetLocal(CS.Messages.ChangeOrderAmount));
										break;
									case BalanceType.DraftChangeOrderQuantity:
										bld.Append(CS.Messages.GetLocal(CS.Messages.DraftChangeOrderQuantity));
										break;
									case BalanceType.DraftChangeOrderAmount:
										bld.Append(CS.Messages.GetLocal(CS.Messages.DraftChangeOrderAmount));
										break;
								}
							}
						}

						e.ReturnValue = bld.Length > 0 ? bld.ToString() : "";
					}
					if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
					{
						e.ReturnState = PXStringState.CreateInstance(e.ReturnState, 100, null, _FieldName + "Text", false, null, null, null, null, null, null);
						((PXFieldState)e.ReturnState).Visible = false;
					}
				}
			}
		}
		public override void DataSourceSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
            base.DataSourceSelected(sender, e);

			if (_RowParentCache != null && _RowParentCache.Current != null && _RowParentCache.Current is IRMType)
			{				
				PXUIFieldAttribute.SetVisible<RMDataSourcePM.startAccountGroup>(sender, e.Row, ((IRMType)_RowParentCache.Current).RMType == RMType.PM);
				PXUIFieldAttribute.SetVisible<RMDataSourcePM.endAccountGroup>(sender, e.Row, ((IRMType)_RowParentCache.Current).RMType == RMType.PM);
				PXUIFieldAttribute.SetVisible<RMDataSourcePM.startProject>(sender, e.Row, ((IRMType)_RowParentCache.Current).RMType == RMType.PM);
				PXUIFieldAttribute.SetVisible<RMDataSourcePM.endProject>(sender, e.Row, ((IRMType)_RowParentCache.Current).RMType == RMType.PM);
				PXUIFieldAttribute.SetVisible<RMDataSourcePM.startProjectTask>(sender, e.Row, ((IRMType)_RowParentCache.Current).RMType == RMType.PM);
				PXUIFieldAttribute.SetVisible<RMDataSourcePM.endProjectTask>(sender, e.Row, ((IRMType)_RowParentCache.Current).RMType == RMType.PM);
				PXUIFieldAttribute.SetVisible<RMDataSourcePM.startInventory>(sender, e.Row, ((IRMType)_RowParentCache.Current).RMType == RMType.PM);
				PXUIFieldAttribute.SetVisible<RMDataSourcePM.endInventory>(sender, e.Row, ((IRMType)_RowParentCache.Current).RMType == RMType.PM);
			}
		}
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);		
		}
		protected override void DataSourceExpandSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_RowParentCache != null && _RowParentCache.Current != null && _RowParentCache.Current is IRMType)
			{
				if (((IRMType)_RowParentCache.Current).RMType == RMType.PM)
				{
					e.ReturnState = PXStringState.CreateInstance(
						e.ReturnState,
						1,
						false,
						typeof(RMDataSource.expand).Name,
						false,
						0,
						null,
						new string[] { ExpandType.Nothing, ExpandType.AccountGroup, ExpandType.Project, ExpandType.ProjectTask, ExpandType.Inventory },
                        new string[] { CS.Messages.GetLocal(CS.Messages.Nothing), CS.Messages.GetLocal(CS.Messages.AccountGroup), CS.Messages.GetLocal(CS.Messages.Project), CS.Messages.GetLocal(CS.Messages.ProjectTask), CS.Messages.GetLocal(CS.Messages.Inventory) },
						true,
						ExpandType.Nothing
						);
				}
				else
				{
					base.DataSourceExpandSelecting(sender, e);
				}
			}
		}
		protected override void DataSourceRowDescriptionSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			base.DataSourceRowDescriptionSelecting(sender, e);
		}
		protected override void DataSourceAmountTypeSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_RowParentCache != null && _RowParentCache.Current != null && _RowParentCache.Current is IRMType)
			{
				if (((IRMType)_RowParentCache.Current).RMType == RMType.PM)
				{
					e.ReturnState = PXIntState.CreateInstance(
						e.ReturnState,
						typeof(RMDataSource.amountType).Name,
						false,
						0,
						null,
						null,
						new int[] { BalanceType.NotSet, 
                                    BalanceType.Amount, 
                                    BalanceType.Quantity, 
                                    BalanceType.TurnoverAmount, 
                                    BalanceType.TurnoverQuantity, 
                                    BalanceType.BudgetAmount, 
                                    BalanceType.BudgetQuantity,
                                    BalanceType.RevisedAmount, 
                                    BalanceType.RevisedQuantity,
									BalanceType.OriginalCommittedAmount,
									BalanceType.OriginalCommittedQuantity,
									BalanceType.CommittedAmount,
									BalanceType.CommittedQuantity,
									BalanceType.CommittedOpenAmount,
									BalanceType.CommittedOpenQuantity,
									BalanceType.CommittedReceivedQuantity,
									BalanceType.CommittedInvoicedAmount,
									BalanceType.CommittedInvoicedQuantity},
						new string[] { CS.Messages.GetLocal(CS.Messages.NotSet),  
                                        CS.Messages.GetLocal(CS.Messages.ActualAmount),  
                                        CS.Messages.GetLocal(CS.Messages.ActualQuantity),
                                        CS.Messages.GetLocal(CS.Messages.AmountTurnover),
                                        CS.Messages.GetLocal(CS.Messages.QuantityTurnover), 
                                        CS.Messages.GetLocal(CS.Messages.BudgetAmount),
                                        CS.Messages.GetLocal(CS.Messages.BudgetQuantity),
                                       	CS.Messages.GetLocal(CS.Messages.RevisedAmount),
										CS.Messages.GetLocal(CS.Messages.RevisedQuantity),
										CS.Messages.GetLocal(CS.Messages.OriginalCommittedAmount),
										CS.Messages.GetLocal(CS.Messages.OriginalCommittedQuantity),
										CS.Messages.GetLocal(CS.Messages.CommittedAmount),
										CS.Messages.GetLocal(CS.Messages.CommittedQuantity),
										CS.Messages.GetLocal(CS.Messages.CommittedOpenAmount),
										CS.Messages.GetLocal(CS.Messages.CommittedOpenQuantity),
										CS.Messages.GetLocal(CS.Messages.CommittedReceivedQuantity),
										CS.Messages.GetLocal(CS.Messages.CommittedInvoicedAmount),
										CS.Messages.GetLocal(CS.Messages.CommittedInvoicedQuantity)},
						null,
						BalanceType.NotSet
						);
				}
				else
				{
					base.DataSourceAmountTypeSelecting(sender, e);
				}
			}
		}		
	}

	public static class RMType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { GL, PM },
				new string[] { GL, PM }) { ; }
		}

		public const string GL = "GL";
		public const string PM = "PM";

		public class RMTypeGL : PX.Data.BQL.BqlString.Constant<RMTypeGL>
		{
			public RMTypeGL() : base(RMType.GL) { ;}
		}
		public class RMTypePM : PX.Data.BQL.BqlString.Constant<RMTypePM>
		{
			public RMTypePM() : base(RMType.PM) { ;}
		}
	}

	#region PXNotificationFormatAttribute
	/// <summary>
	/// Verify and correct format of notification automatically.
	/// </summary>
	/// <example>
	/// [PXNotificationFormat(typeof(NotificationSource.reportID), typeof(NotificationSource.templateID), typeof(Where<NotificationRecipient.sourceID, Equal<Current<NotificationSource.sourceID>>>))]
	/// </example>
	public class PXNotificationFormatAttribute : PXEventSubscriberAttribute
	{
		protected readonly Type report;
		protected readonly Type template;
		protected readonly Type where;
		protected PXView view;

		public PXNotificationFormatAttribute(Type report, Type template)
			: this(report, template, null)
		{
		}
		public PXNotificationFormatAttribute(Type report, Type template, Type where)
		{
			this.report = report;
			this.template = template;
			this.where = where;
			if (BqlCommand.GetItemType(report) != BqlCommand.GetItemType(template))
				throw new PXArgumentException((string)null, Messages.NotificationSourceArgument);
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			sender.Graph.RowUpdating.AddHandler(sender.GetItemType(), OnRowUpdating);
			if (BqlCommand.GetItemType(report).IsAssignableFrom(sender.GetItemType()))
				sender.Graph.RowUpdated.AddHandler(sender.GetItemType(), OnRowUpdated);
			else if (where != null)
			{
				Type[] types = BqlCommand.Decompose(where);
				if (typeof(NotificationRecipient).IsAssignableFrom(sender.GetItemType()))
				{
					for (int i = 0; i < types.Length; i++)
						if (typeof(IBqlField).IsAssignableFrom(types[i]) &&
							BqlCommand.GetItemType(types[i]) == typeof(NotificationRecipient))
							types[i] = sender.GetItemType().GetNestedType(types[i].Name);
				}
				Type[] allTypes = new Type[types.Length + 2];
				allTypes[0] = typeof(Select<,>);
				allTypes[1] = sender.GetItemType();
				Array.Copy(types, 0, allTypes, 2, types.Length);

				this.view = new PXView(sender.Graph, false,
					BqlCommand.CreateInstance(
					BqlCommand.Compose(allTypes)));
				sender.Graph.RowUpdated.AddHandler(BqlCommand.GetItemType(report), OnRowUpdated);
			}
		}
		protected virtual void OnRowUpdating(PXCache cache, PXRowUpdatingEventArgs e)
		{
			string format = (string)cache.GetValue(e.NewRow, _FieldOrdinal);
			string oldFormat = (string)cache.GetValue(e.Row, _FieldOrdinal);
			if (format == oldFormat) return;


			bool isValid = false;
			if (GetSource(cache, e.NewRow, report) != null)
				isValid = ValidateFormat(NotificationFormat.ReportList, format);
            else if (GetSource(cache, e.NewRow, template) != null)
				isValid = ValidateFormat(NotificationFormat.TemplateList, format);
			else
				isValid = true;
			if (!isValid)
			{
				var ex = new PXSetPropertyException(Messages.ErrorFormat);
				cache.RaiseExceptionHandling(_FieldName, e.NewRow, format, ex);
				e.Cancel = true;
			}
		}
		protected virtual void OnRowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			NotificationFormat.ListAttribute validateList = null;

			object templateID = cache.GetValue(e.Row, template.Name);
			object reportID = cache.GetValue(e.Row, report.Name);
			string defFormat = NotificationFormat.Html;
			if (!object.Equals(templateID, cache.GetValue(e.OldRow, template.Name)) && templateID != null && reportID == null)
			{
				validateList = NotificationFormat.TemplateList;
			}
			else if (!object.Equals(reportID, cache.GetValue(e.OldRow, report.Name)) && reportID != null)
			{
				validateList = NotificationFormat.ReportList;
				defFormat = NotificationFormat.PDF;
			}

			if (validateList != null)
			{
				if (this.view == null)
				{
					if (!ValidateFormat(validateList, (string)cache.GetValue(e.Row, _FieldOrdinal)))
					{
						cache.RaiseExceptionHandling(_FieldName, e.Row, null, null);
						cache.SetValue(e.Row, _FieldName, defFormat);
					}
				}
				else
				{
					PXCache iCache = this.view.Cache;
					foreach (object item in this.view.SelectMultiBound(new object[] { e.Row }, null))
					{
						if (!ValidateFormat(validateList, (string)iCache.GetValue(item, _FieldOrdinal)))
						{
							object upd = iCache.CreateCopy(item);
							cache.SetValue(e.Row, _FieldName, defFormat);
							iCache.Update(upd);
						}
					}
				}
			}
		}

		private static bool ValidateFormat(NotificationFormat.ListAttribute list, string value)
		{
		    return list.AllowedValues.Any(e => e == value);
		}

	    private object GetSource(PXCache sender, object row, Type sourceType)
		{
			if (sender.GetItemType() == BqlCommand.GetItemType(sourceType))
				return sender.GetValue(row, sourceType.Name);
			else
				return
					sender.Graph.Caches[BqlCommand.GetItemType(sourceType)].GetValue(
					sender.Graph.Caches[BqlCommand.GetItemType(sourceType)].Current, sourceType.Name);
		}

	    public static string ValidBodyFormat(string format)
	    {
	        return ValidateFormat(NotificationFormat.TemplateList, format) ? format : NotificationFormat.Html;
	    }
	}
	#endregion
	/*#region PXNotificationTemplateSelectorAttribute
	public class PXNotificationTemplateSelectorAttribute : PXCustomSelectorAttribute
	{
		public PXNotificationTemplateSelectorAttribute()
			: base(typeof(WikiNotificationTemplate.pageID))
		{
			SubstituteKey = typeof(WikiNotificationTemplate.name);
			DescriptionField = typeof(WikiNotificationTemplate.title);
		}

		public virtual IEnumerable GetRecords()
		{
			foreach (PXResult<WikiNotificationTemplate, WikiPageLanguage> rec in
				PXSelectJoin<WikiNotificationTemplate,
					LeftJoin<WikiPageLanguage,
						On<WikiNotificationTemplate.pageID, Equal<WikiPageLanguage.pageID>,
							And<WikiPageLanguage.language, Equal<Required<WikiPageLanguage.language>>>>>>.
					Select(_Graph, System.Threading.Thread.CurrentThread.CurrentCulture.Name))
			{
				var page = (WikiNotificationTemplate)rec;
				var translate = (WikiPageLanguage)rec;
				page.Title = translate.Title;
				if (PXSiteMap.WikiProvider.GetAccessRights((Guid)page.PageID) >= PXWikiRights.Select)
					yield return page;
			}
		}
	}

	#endregion*/

	public static class PXRound
	{
		public static decimal Math(decimal value, int decimals)
		{
			return System.Math.Round(value, decimals, MidpointRounding.AwayFromZero);
		}

		public static Decimal Floor(Decimal value, int decimals)
		{
            bool negativeValue = value < 0;
            if (negativeValue)
                value = -value;
			decimal result = value * (decimal)System.Math.Pow(10, decimals);
			result = System.Math.Floor((decimal)result);
			result = result / (decimal)System.Math.Pow(10, decimals);
            if (negativeValue)
                result = -result;
			return result;
		}

		public static Decimal Ceil(Decimal value, int decimals)
		{
            bool negativeValue = value < 0;
            if (negativeValue)
                value = -value;
			decimal result = value * (decimal)System.Math.Pow(10, decimals);
			result = System.Math.Ceiling((decimal)result);
			result = result / (decimal)System.Math.Pow(10, decimals);
            if (negativeValue)
                result = -result;
			return result;
		}
	}

	/// <summary>
	/// This atribute should be placed on the IsValidated field of the Address-based DAC
	/// It clears an IsValidated flag, if the essential fields of address (Line1, Line2, City, State, Country, PostalCode) are modified
	/// It is rely on the predefined fields naming.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class ValidatedAddressAttribute : PXEventSubscriberAttribute, IPXRowUpdatedSubscriber, IPXRowSelectedSubscriber
	{
		protected string _AddressLine1Field = "AddressLine1";
		protected string _AddressLine2Field = "AddressLine2";
		protected string _AddressLine31Field = "AddressLine3";
		protected string _CityField = "City";
		protected string _CountryField = "Country";
		protected string _StateField = "State";
		protected string _PostalCodeField = "PostalCode";
		protected string _IsValidatedField = "IsValidated";

		public virtual void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			string[] fieldsToCheck = { this._AddressLine1Field, this._AddressLine2Field, this._AddressLine31Field, this._CityField, this._CountryField, this._StateField, this._PostalCodeField };
			bool isModified = false;
			bool isValidatedOld = (bool?)sender.GetValue(e.OldRow, this.FieldOrdinal) == true;
			bool isValidatedNew = (bool?)sender.GetValue(e.Row, this.FieldOrdinal) == true;

			if (isValidatedOld == true && isValidatedNew == true)
			{
				foreach (string iFld in fieldsToCheck)
				{
					int ordinal = sender.GetFieldOrdinal(iFld);
					object oldValue = sender.GetValue(e.OldRow, ordinal);
					object value = sender.GetValue(e.Row, ordinal);
					if (!Object.Equals(oldValue, value))
					{
						isModified = true; break;
					}
				}
			}
			if (isModified)
			{
				sender.SetValue(e.Row, this.FieldOrdinal, false);
			}
		}

		public virtual void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row != null && PXAccess.FeatureInstalled<FeaturesSet.addressValidation>())
			{
				PXSetPropertyException exception = null;

				if ((bool?)sender.GetValue(e.Row, FieldOrdinal) != true &&
					PXAddressValidator.IsValidateRequired(sender.Graph, (IAddressBase)e.Row))
				{
					exception = new PXSetPropertyException(Messages.AddressIsNotValidated, PXErrorLevel.Warning);
				}

				sender.RaiseExceptionHandling(FieldName, e.Row, false, exception);
			}
		}
	}

	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class FeatureRestrictorAttribute : PXAggregateAttribute, IPXRowSelectedSubscriber
	{

		public FeatureRestrictorAttribute(Type checkUsage)
		{
			_Select = checkUsage != null ? BqlCommand.CreateInstance(checkUsage) : null;
			typeName = checkUsage == null ? null : checkUsage.FullName;
		}

		private readonly string typeName;
		protected BqlCommand _Select;
	

		#region IPXRowSelectedSubscriber Members
		void IPXRowSelectedSubscriber.RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            if (_Select == null ||
                  sender.Graph.GetType() == typeof(PXGraph) ||
                  sender.Graph.GetType() == typeof(PXGenericInqGrph) ||
				  sender.Graph.IsContractBasedAPI) return;

			PXFieldState state = sender.GetStateExt(e.Row, _FieldName) as PXFieldState;
			if (state != null && state.ErrorLevel != PXErrorLevel.Error)
			{
				if ((bool?)state.Value != true)
				{
					if (_Select.GetReferencedFields(false)
						.Where(_ => _.IsNested)
						.Select(_ => _.DeclaringType)
						.Union(_Select.GetTables())
						.Where(_ => typeof(PXCacheExtension).IsAssignableFrom(_))
						.Distinct()
						.All(_ => PXCache.IsActiveExtension(_)))
					{
						PXView view = sender.Graph.TypedViews.GetView(_Select, true);
						if (view.SelectSingle() != null)
							sender.RaiseExceptionHandling(_FieldName, e.Row, false, new PXSetPropertyException(Messages.FeaturesUsageWarning, PXErrorLevel.Warning));
					}
				}
				else
					sender.RaiseExceptionHandling(_FieldName, e.Row, false, null);
			}
        }

		public override string ToString()
		{
			return string.Concat("FeatureRestrictorAttribute<", typeName, ">");
		}

		#endregion
	}
	
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class FeatureDependencyAttribute : PXEventSubscriberAttribute, IPXFieldVerifyingSubscriber
    {
        protected Type[] Depencency;
		protected bool AllDependenciesRequired;

		public FeatureDependencyAttribute(params Type[] dependency)
        {
            Depencency = dependency;
			AllDependenciesRequired = false;
		}

		public FeatureDependencyAttribute(bool allDependenciesRequired, params Type[] dependency)
		{
			Depencency = dependency;
			AllDependenciesRequired = allDependenciesRequired;
		}

		public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);
           
            foreach (var type in Depencency)
            {
                sender.Graph.FieldUpdated.AddHandler(type.DeclaringType, type.Name, FieldUpdated);
            }
        }

        protected virtual void FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)            
        {
            bool allUnchecked = true;
			bool allChecked = true;
			foreach (Type field in Depencency)
            {
                var newValue = sender.GetValue(e.Row, field.Name);
				if ((bool?)newValue == true && allUnchecked == true)
					allUnchecked = false;
				else if ((bool?)newValue == false && allChecked == true)
					allChecked = false;
			}
            if (allUnchecked || (AllDependenciesRequired && !allChecked))
            {
                sender.SetValueExt(e.Row, _FieldName, false);
            }
        }

        public void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            if ((bool?)e.NewValue == true && Depencency != null && e.Row != null)
            {
                bool allUnchecked = true;
				bool allChecked = true;
				string dependencyNames = null;
                foreach (Type field in Depencency)
                {
                    PXFieldState state = sender.GetStateExt(e.Row, field.Name) as PXFieldState;
					if (state != null)
					{
						bool? stateValue = (bool?)state.Value;

						if (stateValue != false)
							allUnchecked = false;
						else if (stateValue != null)
							allChecked = false;

						if (!AllDependenciesRequired || stateValue == false)
							dependencyNames = dependencyNames == null
								? state.DisplayName
								: dependencyNames + ", " + state.DisplayName;
					}
                }
                if(!AllDependenciesRequired && allUnchecked)
                    throw new PXSetPropertyException(Depencency.Length > 1? Messages.FeaturesDependencies : Messages.FeaturesDependency, dependencyNames);
				if (AllDependenciesRequired && !allChecked)
					throw new PXSetPropertyException(Messages.FeaturesDependenciesAllRequired, dependencyNames);
			}
        }
      
    }

	[PXDefault(false)]
	[PXDBBool]
	[PXUIField]
	public class FeatureAttribute : FeatureRestrictorAttribute, IPXFieldSelectingSubscriber, IPXFieldDefaultingSubscriber
	{
		public FeatureAttribute(bool defValue)
			: this(defValue, null, null)
		{

		}
		public FeatureAttribute(bool defValue, Type parent)
			: this(defValue, parent, null)
		{
		}
		public FeatureAttribute(Type parent)
			: this(parent, null)
		{

		}
		public FeatureAttribute(Type parent, Type checkUsage)
			: base(checkUsage)
		{
			Parent = parent;
		}
		public FeatureAttribute(bool defValue, Type parent, Type checkUsage)
			: this(parent, checkUsage)
		{
			this.GetAttribute<PXDefaultAttribute>().Constant = defValue;
		}

		protected bool _defValue;

		public Type Parent { get; set; }
	    
	    public string DisplayName
		{
			get { return this.GetAttribute<PXUIFieldAttribute>().DisplayName; }
			set { this.GetAttribute<PXUIFieldAttribute>().DisplayName = value; }
		}

		public bool Enabled
		{
			get { return this.GetAttribute<PXUIFieldAttribute>().Enabled; }
			set { this.GetAttribute<PXUIFieldAttribute>().Enabled = value; }
		}

		public bool Visible
		{
			get { return this.GetAttribute<PXUIFieldAttribute>().Visible; }
			set { this.GetAttribute<PXUIFieldAttribute>().Visible = value; }
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			if (this.Parent != null && Parent.DeclaringType !=null)
			{
				var type = Parent.DeclaringType;
				if (typeof(PXCacheExtension).IsAssignableFrom(type) && type.BaseType.IsGenericType)
				{
					type = type.BaseType.GetGenericArguments()[type.BaseType.GetGenericArguments().Length - 1];
				}

				sender.Graph.RowUpdated.AddHandler(type, RowUpdated);
			}           
		}

		public bool Top
		{
			get;
			set;
		}
        public bool SyncToParent
        {
            get;
            set;
        }

		  protected virtual void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			bool? currentValue = (bool?)sender.GetValue(e.Row, this.Parent.Name);
              bool? oldValue = (bool?)sender.GetValue(e.OldRow, this.Parent.Name);
		      object status = sender.GetValue(e.Row, sender.Keys.Last());

		      if (status != null && status is int? && (int) status != 3)
		          return;

              if (currentValue != oldValue)
		      {
		    if (currentValue == true)
		    {
				object value = sender.GetValue(e.Row, _FieldName);
	            if (value == null || !(value is bool?) || (bool?) value != true)
		            sender.SetDefaultExt(e.Row, _FieldName);
                if(SyncToParent)
                    sender.SetValueExt(e.Row, _FieldName, true);
		    }
		    else
		        sender.SetValueExt(e.Row, _FieldName, false);
		}
		  }

		#region IPXFieldSelectingSubscriber Members

		public void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
            bool enabled = !SyncToParent;
			if (sender.AllowUpdate != true || (this.Parent != null && (bool?)sender.GetValue(e.Row, this.Parent.Name) == false))			
			    enabled = false;							
            e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, typeof(Boolean), null, null, -1, null, null, null, _FieldName, null, DisplayName, null, PXErrorLevel.Undefined, enabled, null, null, PXUIVisibility.Undefined, null, null, null);
		}

		public void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (this.Parent != null)
			{
				bool? currentValue = (bool?)sender.GetValue(e.Row, this.Parent.Name);
				if (currentValue != true)
				{
					e.Cancel = true;
					e.NewValue = false;
				}
			}
		}

		#endregion

       
    }
    #region WriteOffDefault
    public class WriteOffDefault
    {
        public class CustomListAttribute : PXStringListAttribute
        {
            public string[] AllowedValues
            {
                get
                {
                    return _AllowedValues;
                }
            }

            public string[] AllowedLabels
            {
                get
                {
                    return _AllowedLabels;
                }
            }

            public CustomListAttribute(string[] AllowedValues, string[] AllowedLabels)
                : base(AllowedValues, AllowedLabels)
	{
            }
        }
        /// <summary>
        /// Defines a list of the possible sources for the AR Documents sub-account defaulting: <br/>
        /// Namely: MaskLocation, MaskItem, MaskEmployee, MaskCompany, MaskSalesPerson <br/>
        /// Mostly, this attribute serves as a container <br/>
        /// </summary>
		public class ClassListAttribute : CustomListAttribute
        {
            public ClassListAttribute()
                : base(new string[] { ReasonCode, CustomerLocation, Branch }, new string[] { Messages.ReasonCode, !PXAccess.FeatureInstalled<FeaturesSet.accountLocations>() ? AR.Messages.MaskCustomer : AR.Messages.MaskLocation, AR.Messages.MaskCompany })
            {
            }

            public override void CacheAttached(PXCache sender)
            {
                _AllowedValues = new[] { ReasonCode, CustomerLocation, Branch };
                _AllowedLabels = new[] { Messages.ReasonCode, !PXAccess.FeatureInstalled<FeaturesSet.accountLocations>() ? AR.Messages.MaskCustomer : AR.Messages.MaskLocation, AR.Messages.MaskCompany };
                _NeutralAllowedLabels = _AllowedLabels;

                base.CacheAttached(sender);
            }
        }
		public const string ReasonCode = "R";
		public const string CustomerLocation = "L";
		public const string Branch = "C";
		public const string Employee = "E";
		public const string Salesperson = "S";
		public const string InventoryItem = "I";
		public const string PostingClass = "P";
		public const string Warehouse = "W";
    }
    #endregion 

    #region ReasonCodeSubAccountMaskAttribute

    [PXUIField(DisplayName = "Subaccount Mask", Visibility = PXUIVisibility.Visible, FieldClass = _DimensionName)]
    [WriteOffDefault.ClassList]
	public sealed class ReasonCodeSubAccountMaskAttribute : AcctSubAttribute
	{

		private const string _DimensionName = "SUBACCOUNT";
		private const string _MaskName = "ReasonCodeCS";
		public ReasonCodeSubAccountMaskAttribute()
			: base()
		{
			PXDimensionMaskAttribute attr = new PXDimensionMaskAttribute(_DimensionName, _MaskName, WriteOffDefault.ReasonCode, new WriteOffDefault.ClassListAttribute().AllowedValues, new WriteOffDefault.ClassListAttribute().AllowedLabels);
			attr.ValidComboRequired = false;
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}

        public override void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers)
        {
            base.GetSubscriber<ISubscriber>(subscribers);
            subscribers.Remove(_Attributes.OfType<WriteOffDefault.ClassListAttribute>().FirstOrDefault() as ISubscriber);
        }

        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);
            var stringlist = (WriteOffDefault.ClassListAttribute)_Attributes.First(x => x.GetType() == typeof(WriteOffDefault.ClassListAttribute));
            var dimensionmaskattribute = (PXDimensionMaskAttribute)_Attributes.First(x => x.GetType() == typeof(PXDimensionMaskAttribute));
            dimensionmaskattribute.SynchronizeLabels(stringlist.AllowedValues, stringlist.AllowedLabels);
        }

		public static string MakeSub<Field>(PXGraph graph, string mask, object[] sources, Type[] fields)
			where Field : IBqlField
		{
			try
			{
				return PXDimensionMaskAttribute.MakeSub<Field>(graph, mask, new WriteOffDefault.ClassListAttribute().AllowedValues, sources);
			}
			catch (PXMaskArgumentException ex)
			{
				PXCache cache = graph.Caches[BqlCommand.GetItemType(fields[ex.SourceIdx])];
				string fieldName = fields[ex.SourceIdx].Name;
				throw new PXMaskArgumentException(new WriteOffDefault.ClassListAttribute().AllowedLabels[ex.SourceIdx], PXUIFieldAttribute.GetDisplayName(cache, fieldName));
			}
		}
	}

	#endregion

	#region ContactLanguageDefault

	public class ContacLanguageDefaultAttribute : PXEventSubscriberAttribute, IPXFieldDefaultingSubscriber
	{
		protected readonly Type address;
		public ContacLanguageDefaultAttribute(Type address)
		{
			this.address = address;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldUpdated.AddHandler(address.DeclaringType, address.Name, (cache, e)=> { if (!sender.Graph.IsCopyPasteContext) sender.SetDefaultExt(sender.Current, _FieldName); });
			sender.Graph.RowInserted.AddHandler(address.DeclaringType, (cache, e) => sender.SetDefaultExt(sender.Current, _FieldName));
		}
		
		public virtual void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			PXCache addressCache = sender.Graph.Caches[address.DeclaringType];
			var country = (Country)PXSelectorAttribute.Select<Country.countryID>(addressCache, addressCache.Current);
			e.NewValue = country?.LanguageID;
		}
	}
	#endregion
}
