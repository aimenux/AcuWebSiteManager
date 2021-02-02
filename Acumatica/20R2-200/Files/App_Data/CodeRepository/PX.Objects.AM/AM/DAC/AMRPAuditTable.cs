using System;
using PX.Data;

namespace PX.Objects.AM
{
	[Serializable]
    [PXCacheName(AM.Messages.MRPAudit)]
	public class AMRPAuditTable : IBqlTable
	{
        #region Selected
        public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }

        [PXBool]
        [PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Selected", Enabled = false)]
        public virtual bool? Selected
        {
            get;
            set;
        }
        #endregion
		#region Recno
		public abstract class recno : PX.Data.BQL.BqlInt.Field<recno> { }

		protected Int32? _Recno;
		[PXDBIdentity(IsKey = true)]
		[PXUIField(Enabled = false, Visible=false)]
		public virtual Int32? Recno
		{
			get
			{
				return this._Recno;
			}
			set
			{
				this._Recno = value;
			}
		}
		#endregion
		#region MsgText
		public abstract class msgText : PX.Data.BQL.BqlString.Field<msgText> { }

		protected String _MsgText;
        //NVARCHAR(MAX)
		[PXDBString]
		[PXDefault]
		[PXUIField(DisplayName = "Message", Enabled = false)]
		public virtual String MsgText
		{
			get
			{
				return this._MsgText;
			}
			set
			{
				this._MsgText = value;
			}
		}
        #endregion
        #region MsgType
        public abstract class msgType : PX.Data.BQL.BqlInt.Field<msgType> { }

        protected int? _MsgType;
        [PXDBInt]
        [PXDefault(MsgTypes.Default)]
        [MsgTypes.List]
        [PXUIField(DisplayName = "Message Type", Enabled = false, Visible = false)]
        public virtual int? MsgType
        {
            get
            {
                return this._MsgType;
            }
            set
            {
                this._MsgType = value;
            }
        }
        #endregion
        #region ProcessID

        public abstract class processID : PX.Data.BQL.BqlGuid.Field<processID> { }

        protected Guid? _ProcessID;
        [PXDBGuid]
        [PXUIField(DisplayName = "Process ID", Enabled = false, Visible = false)]
        public virtual Guid? ProcessID
        {
            get
            {
                return this._ProcessID;
            }
            set
            {
                this._ProcessID = value;
            }
        }
        #endregion
        #region CreatedByID

        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

        protected Guid? _CreatedByID;
        [PXDBCreatedByID(Visible = false)]
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
        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

        protected Byte[] _tstamp;
        [PXDBTimestamp()]
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

        /// <summary>
        /// <see cref="AMRPAuditTable"/> MsgType values
        /// </summary>
        public static class MsgTypes
        {
            /// <summary>
            /// Normal reporting message
            /// </summary>
            public const int Default = 0;
            /// <summary>
            /// The starting message when the process begins
            /// </summary>
            public const int Start = 1;
            /// <summary>
            /// The ending message when the process completes
            /// </summary>
            public const int End = 2;
            /// <summary>
            /// Warning message during process which does not stop the process from running
            /// </summary>
            public const int Warning = 3;
            /// <summary>
            /// Errors found in the process
            /// </summary>
            public const int Error = 9;

            public class ListAttribute : PXIntListAttribute
            {
                public ListAttribute()
                    : base(
                        new int[] {
                            Default,
                            Start,
                            End,
                            Warning,
                            Error
                        },
                        new string[] {
                            Messages.Info,
                            Messages.Start,
                            Messages.End,
                            Messages.Warning,
                            IN.Messages.Error
                        })
                {
                }
            }

            public class _default : PX.Data.BQL.BqlInt.Constant<_default>
            {
                public _default()
                    : base(Default)
                { }
            }

            public class start : PX.Data.BQL.BqlInt.Constant<start>
            {
                public start()
                    : base(Start)
                { }
            }

            public class end : PX.Data.BQL.BqlInt.Constant<end>
            {
                public end()
                    : base(End)
                { }
            }

            public class warning : PX.Data.BQL.BqlInt.Constant<warning>
            {
                public warning()
                    : base(Warning)
                { }
            }

            public class error : PX.Data.BQL.BqlInt.Constant<error>
            {
                public error()
                    : base(Error)
                { }
            }
        }
    }
}
