using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
    /// <summary>
    /// Production event history
    /// </summary>
	[Serializable]
    [PXCacheName(Messages.ProductionEvnt)]
    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class AMProdEvnt : IBqlTable, IProdOrder
    {
        internal string DebuggerDisplay => $"OrderType = {OrderType}, ProdOrdID = {ProdOrdID}, LineID = {LineNbr}, Description = {Description}";

        #region Keys

        public class PK : PrimaryKeyOf<AMProdEvnt>.By<orderType, prodOrdID, lineNbr>
        {
            public static AMProdEvnt Find(PXGraph graph, string orderType, string prodOrdID, int? lineNbr) 
                => FindBy(graph, orderType, prodOrdID, lineNbr);
            public static AMProdEvnt FindDirty(PXGraph graph, string orderType, string prodOrdID, int? lineNbr)
                => PXSelect<AMProdEvnt,
                        Where<orderType, Equal<Required<orderType>>,
                            And<prodOrdID, Equal<Required<prodOrdID>>,
                                And<lineNbr, Equal<Required<lineNbr>>>>>>
                    .SelectWindowed(graph, 0, 1, orderType, prodOrdID, lineNbr);
        }

        public static class FK
        {
            public class OrderType : AMOrderType.PK.ForeignKeyOf<AMProdEvnt>.By<orderType> { }
            public class ProductionOrder : AMProdItem.PK.ForeignKeyOf<AMProdEvnt>.By<orderType, prodOrdID> { }
        }

        #endregion

        #region OrderType
        public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }

        protected String _OrderType;
        [PXDBDefault(typeof(AMProdItem.orderType))]
        [AMOrderTypeField(IsKey = true, Visible = false, Enabled = false)]
        public virtual String OrderType
        {
            get
            {
                return this._OrderType;
            }
            set
            {
                this._OrderType = value;
            }
        }
        #endregion
        #region ProdOrdID
        public abstract class prodOrdID : PX.Data.BQL.BqlString.Field<prodOrdID> { }

        protected String _ProdOrdID;
        [ProductionNbr(IsKey = true, Visible = false, Enabled = false)]
        [PXDBDefault(typeof(AMProdItem.prodOrdID))]
        [PXParent(typeof(Select<AMProdItem,
            Where<AMProdItem.orderType, Equal<Current<AMProdEvnt.orderType>>,
                And<AMProdItem.prodOrdID, Equal<Current<AMProdEvnt.prodOrdID>>>>>))]
        public virtual String ProdOrdID
        {
            get
            {
                return this._ProdOrdID;
            }
            set
            {
                this._ProdOrdID = value;
            }
        }
        #endregion
        #region LineNbr
        public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
        protected Int32? _LineNbr;
        [PXDBInt(IsKey = true)]
        [PXDefault]
        [PXLineNbr(typeof(AMProdItem.lineCntrEvnt))]
        [PXUIField(DisplayName = "Event Line Number", Enabled = false, Visible = false)]
        public virtual Int32? LineNbr
        {
            get
            {
                return this._LineNbr;
            }
            set
            {
                this._LineNbr = value;
            }
        }
        #endregion
        #region EventID
        [Obsolete("Use AMProdEvnt.lineNbr")]
        public abstract class eventID : PX.Data.BQL.BqlLong.Field<eventID> { }

        protected Int64? _EventID;
        [Obsolete("Use AMProdEvnt.LineNbr")]
        [PXDBLong]
        [PXUIField(DisplayName = "Event ID", Visibility = PXUIVisibility.Invisible, Enabled = false, Visible = false)]
        public virtual Int64? EventID
        {
            get
            {
                return this._EventID;
            }
            set
            {
                this._EventID = value;
            }
        }
        #endregion
        #region Description
        public abstract class description : PX.Data.BQL.BqlString.Field<description> { }

        protected String _Description;
		[PXDBString(256, IsUnicode = true)]
		[PXDefault]
        [PXUIField(DisplayName = "Description", Enabled = false)]
		public virtual String Description
		{
			get
			{
                return this._Description;
			}
			set
			{
                this._Description = value;
			}
		}
		#endregion
        #region EventType
        public abstract class eventType : PX.Data.BQL.BqlInt.Field<eventType> { }

        protected int? _EventType;
        [PXDefault(ProductionEventType.Comment)]
        [PXDBInt]
        [ProductionEventType.List]
        [PXUIField(DisplayName = "Type", Enabled = false)]
        public virtual int? EventType
        {
            get
            {
                return this._EventType;
            }
            set
            {
                this._EventType = value;
            }
        }
        #endregion
        #region RefBatNbr
        public abstract class refBatNbr : PX.Data.BQL.BqlString.Field<refBatNbr> { }

        protected String _RefBatNbr;
        [PXDBString(15, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Batch Nbr", Visible = true, Enabled = false)]
        [PXSelector(typeof(Search<AMBatch.batNbr, Where<AMBatch.docType, Equal<Current<AMProdEvnt.refDocType>>>>), ValidateValue = false)]
        public virtual String RefBatNbr
        {
            get
            {
                return this._RefBatNbr;
            }
            set
            {
                this._RefBatNbr = value;
            }
        }
        #endregion
        #region RefDocType
        public abstract class refDocType : PX.Data.BQL.BqlString.Field<refDocType> { }

        protected String _RefDocType;
        [PXDBString(1, IsFixed = true)]
        [AMDocType.List()]
        [PXUIField(DisplayName = "Doc Type", Visible = true, Enabled = false)]
        public virtual String RefDocType
        {
            get
            {
                return this._RefDocType;
            }
            set
            {
                this._RefDocType = value;
            }
        }
        #endregion
        #region RefNoteID
        public abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID> { }

        protected Guid? _RefNoteID;
        [PXUIField(DisplayName = "Related Document", Enabled = false)]
        [PXRefNote]
        public virtual Guid? RefNoteID
        {
            get
            {
                return this._RefNoteID;
            }
            set
            {
                this._RefNoteID = value;
            }
        }
        public class PXRefNoteAttribute : PX.Data.PXRefNoteAttribute
        {
            public PXRefNoteAttribute()
                : base()
            {
            }

            public class PXLinkState : PXStringState
            {
                protected object[] _keys;
                protected Type _target;

                public object[] keys
                {
                    get { return _keys; }
                }

                public Type target
                {
                    get { return _target; }
                }

                public PXLinkState(object value)
                    : base(value)
                {
                }

                public static PXFieldState CreateInstance(object value, Type target, object[] keys)
                {
                    PXLinkState state = value as PXLinkState;
                    if (state == null)
                    {
                        PXFieldState field = value as PXFieldState;
                        if (field != null && field.DataType != typeof(object) && field.DataType != typeof(string))
                        {
                            return field;
                        }
                        state = new PXLinkState(value);
                    }
                    if (target != null)
                    {
                        state._target = target;
                    }
                    if (keys != null)
                    {
                        state._keys = keys;
                    }

                    return state;
                }
            }

            public override void CacheAttached(PXCache sender)
            {
                base.CacheAttached(sender);

                PXButtonDelegate del = delegate (PXAdapter adapter)
                {
                    PXCache cache = adapter.View.Graph.Caches[typeof(AMProdEvnt)];
                    if (cache.Current != null)
                    {
                        object val = cache.GetValueExt(cache.Current, _FieldName);

                        PXLinkState state = val as PXLinkState;
                        if (state != null)
                        {
                            helper.NavigateToRow(state.target.FullName, state.keys, PXRedirectHelper.WindowMode.NewWindow);
                        }
                        else
                        {
                            helper.NavigateToRow((Guid?)cache.GetValue(cache.Current, _FieldName), PXRedirectHelper.WindowMode.NewWindow);
                        }
                    }

                    return adapter.Get();
                };

                string ActionName = sender.GetItemType().Name + "$" + _FieldName + "$Link";
                sender.Graph.Actions[ActionName] = (PXAction)Activator.CreateInstance(typeof(PXNamedAction<>).MakeGenericType(typeof(AMProdEvnt)), new object[] { sender.Graph, ActionName, del, new PXEventSubscriberAttribute[] { new PXUIFieldAttribute { MapEnableRights = PXCacheRights.Select } } });
            }

            public virtual object GetEntityRowID(PXCache cache, object[] keys)
            {
                return GetEntityRowID(cache, keys, ", ");
            }

            public static object GetEntityRowID(PXCache cache, object[] keys, string separator)
            {
                var result = new System.Text.StringBuilder();
                int i = 0;
                foreach (string key in cache.Keys)
                {
                    if (i >= keys.Length) break;
                    object val = keys[i++];
                    cache.RaiseFieldSelecting(key, null, ref val, true);

                    if (val != null)
                    {
                        if (result.Length != 0) result.Append(separator);
                        result.Append(val.ToString().TrimEnd());
                    }
                }
                return result.ToString();
            }
        }
        #endregion

        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

        protected Byte[] _tstamp;
        [PXDBTimestamp]
        public virtual Byte[] tstamp
        {
            get
            {
                return this._tstamp;
            }
            set
            {
                this._tstamp = value;
            }
        }
        #endregion
        #region CreatedByID

        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

        protected Guid? _CreatedByID;
        [PXDBCreatedByID]
        public virtual Guid? CreatedByID
        {
            get
            {
                return this._CreatedByID;
            }
            set
            {
                this._CreatedByID = value;
            }
        }
        #endregion
        #region CreatedByScreenID

        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

        protected String _CreatedByScreenID;
        [PXDBCreatedByScreenID]
        [PXUIField(DisplayName = "Created Screen ID", Enabled = false, Visible = false)]
        public virtual String CreatedByScreenID
        {
            get
            {
                return this._CreatedByScreenID;
            }
            set
            {
                this._CreatedByScreenID = value;
            }
        }
        #endregion
        #region CreatedByScreenIDTitle (unbound/ref to CreatedByScreenID)
        public abstract class createdByScreenIDTitle : PX.Data.BQL.BqlString.Field<createdByScreenIDTitle> { }


        protected String _CreatedByScreenIDTitle;
        [PXUIField(DisplayName = "Created Screen", Visible = true, Enabled = false)]
        [SiteMapTitle(typeof(AMProdEvnt.createdByScreenID))]
        public virtual String CreatedByScreenIDTitle
        {
            get { return _CreatedByScreenIDTitle; }
            set { _CreatedByScreenIDTitle = value; }
        }
        #endregion
        #region CreatedDateTime

        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

        protected DateTime? _CreatedDateTime;
        [PXDBCreatedDateTime]
        [PXUIField(DisplayName = "Created At", Visible = true, Enabled = false)]
        public virtual DateTime? CreatedDateTime
        {
            get
            {
                return this._CreatedDateTime;
            }
            set
            {
                this._CreatedDateTime = value;
            }
        }
        #endregion
        #region LastModifiedByID
        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
        protected Guid? _LastModifiedByID;
        [PXDBLastModifiedByID]
        public virtual Guid? LastModifiedByID
        {
            get
            {
                return this._LastModifiedByID;
            }
            set
            {
                this._LastModifiedByID = value;
            }
        }
        #endregion
        #region LastModifiedByScreenID
        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
        protected String _LastModifiedByScreenID;
        [PXDBLastModifiedByScreenID]
        public virtual String LastModifiedByScreenID
        {
            get
            {
                return this._LastModifiedByScreenID;
            }
            set
            {
                this._LastModifiedByScreenID = value;
            }
        }
        #endregion
        #region LastModifiedDateTime
        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
        protected DateTime? _LastModifiedDateTime;
        [PXDBLastModifiedDateTime]
        public virtual DateTime? LastModifiedDateTime
        {
            get
            {
                return this._LastModifiedDateTime;
            }
            set
            {
                this._LastModifiedDateTime = value;
            }
        }
        #endregion
    }
}
