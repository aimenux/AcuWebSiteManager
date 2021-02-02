// This File is Distributed as Part of Acumatica Shared Source Code 
/* ---------------------------------------------------------------------*
*                               Acumatica Inc.                          *
*              Copyright (c) 1994-2011 All rights reserved.             *
*                                                                       *
*                                                                       *
* This file and its contents are protected by United States and         *
* International copyright laws.  Unauthorized reproduction and/or       *
* distribution of all or any portion of the code contained herein       *
* is strictly prohibited and will result in severe civil and criminal   *
* penalties.  Any violations of this copyright will be prosecuted       *
* to the fullest extent possible under law.                             *
*                                                                       *
* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *
* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *
* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY ProjectX PRODUCT.        *
*                                                                       *
* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *
* ---------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using PX.Common;

namespace PX.Data
{
    /// <exclude/>
	public class PXOverridableException : PXException
	{
		// Field name to map exception to
		protected string _MapErrorTo = null;

		/// <summary>
		/// Gets oe Set fieldname to map exception to
		/// </summary>
		public string MapErrorTo
		{
			get
			{
				return _MapErrorTo;
			}
			set
			{
				_MapErrorTo = value;
			}
		}

		public PXOverridableException(string message, Exception inner)
			: base(message, inner)
		{
			_Message = base.MessageNoPrefix;
		}
		// Formats exception message like String.Format() does
		public PXOverridableException(Exception inner, string format, params object[] args)
			: base(inner, format, args)
		{
			_Message = base.MessageNoPrefix;
		}
		public PXOverridableException(string message)
			: base(message)
		{
			_Message = base.MessageNoPrefix;
		}
		// Formats exception message like String.Format() does
		public PXOverridableException(string format, params object[] args)
			: base(format, args)
		{
			_Message = base.MessageNoPrefix;
		}
		public override string Message
		{
			get
			{
				if (MessagePrefix == null)
				{
					return _Message;
				}
				else
				{
					return String.Format("{0}: {1}", MessagePrefix, _Message);
				}
			}
		}
		public override string MessageNoNumber
		{
			get
			{
				if (MessagePrefix != null)
					return string.Format("{0}: {1}", MessagePrefix, _Message);
				return _Message;
			}
		}
		public virtual void SetMessage(string message)
		{
			if (MessagePrefix != null)
			{
				string strtemp = MessagePrefix + ':';
				int idx = message.IndexOf(strtemp);
				if (idx != -1)
				{
					message = message.Substring(message.IndexOf(strtemp) + strtemp.Length);
				}
			}
			_Message = PXMessages.Localize(message);
		}

		public PXOverridableException(SerializationInfo info, StreamingContext context)
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

    /// <exclude/>
	[Serializable]
	public class PXOperationCompletedException : PXOverridableException
	{ 
		public PXOperationCompletedException(string message, Exception inner)
			: base(message, inner)
		{
		}

		public PXOperationCompletedException(Exception inner, string format, params object[] args)
			: base(inner, format, args)
		{
		}
		public PXOperationCompletedException(string message)
			: base(message)
		{
		}

		public PXOperationCompletedException(string format, params object[] args)
			: base(format, args)
		{
		}

		public PXOperationCompletedException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
					
		}
	}

    /// <exclude/>
	[Serializable]
	public class PXOperationCompletedWithErrorException : PXOperationCompletedException
	{
		public PXOperationCompletedWithErrorException(string message, Exception inner)
			: base(message, inner)
		{
		}

		public PXOperationCompletedWithErrorException(Exception inner, string format, params object[] args)
			: base(inner, format, args)
		{
		}

		public PXOperationCompletedWithErrorException()
			: this(ErrorMessages.SeveralItemsFailed)
		{
		}

		public PXOperationCompletedWithErrorException(string message)
			: base(message)
		{
		}

		public PXOperationCompletedWithErrorException(string format, params object[] args)
			: base(format, args)
		{
		}

		public PXOperationCompletedWithErrorException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{	
		}
	}

    /// <exclude/>
	[Serializable]
	public class PXOperationCompletedSingleErrorException : PXOperationCompletedException
	{
		public PXOperationCompletedSingleErrorException(string message, Exception inner)
			: base(message, inner)
		{
		}

		public PXOperationCompletedSingleErrorException(Exception inner)
			: base(inner is PXOuterException ? inner.Message + "\r\n" + String.Join("\r\n", ((PXOuterException)inner).InnerMessages) : inner.Message, inner)
		{
		}

		public PXOperationCompletedSingleErrorException(Exception inner, string format, params object[] args)
			: base(inner, format, args)
		{
		}

		public PXOperationCompletedSingleErrorException(string message)
			: base(message)
		{
		}

		public PXOperationCompletedSingleErrorException(string format, params object[] args)
			: base(format, args)
		{
		}

		public PXOperationCompletedSingleErrorException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}

    /// <exclude/>
    [Serializable]
    public class PXOperationCompletedWithWarningException : PXOperationCompletedException
    {
        public PXOperationCompletedWithWarningException (string message, Exception inner)
            : base(message, inner)
        {
        }

        public PXOperationCompletedWithWarningException (Exception inner, string format, params object[] args)
            : base(inner, format, args)
        {
        }

        public PXOperationCompletedWithWarningException ()
            : this(ErrorMessages.SeveralItemsFailed)
        {
        }

        public PXOperationCompletedWithWarningException(string message)
            : base(message)
        {
        }

        public PXOperationCompletedWithWarningException(string format, params object[] args)
            : base(format, args)
        {
        }

        public PXOperationCompletedWithWarningException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {	
        }
    }


    /// <summary>An exception type that is used to indicate that the required data isn't entered on the setup form.</summary>
    [Serializable]
    public class PXSetupNotEnteredException : PXSetPropertyException
    {
        //DAC which doesn't containt required data
        private Type _DAC;
        //Store page name where user can navigate to enter required data
        private string _navigateTo;
        //Fields used to redirect to
        private Dictionary<Type, object> _keyParams;

        public PXSetupNotEnteredException(string format, Type inpDAC, params object[] args)
            : base(format, args)
        {
            _keyParams = null;
            _DAC = inpDAC;
            _navigateTo = args[0].ToString();
        }

        public PXSetupNotEnteredException(string format, Type inpDAC, Dictionary<Type, object> keyparams, params object[] args)
            : base(format, args)
        {
            _keyParams = keyparams;
            _DAC = inpDAC;
            _navigateTo = args[0].ToString();
        }

        public Dictionary<Type, object> KeyParams
        {
            get
            {
                return _keyParams;
            }
        }

        public Type DAC
        {
            get
            {
                return _DAC;
            }
        }

        public string NavigateTo
        {
            get
            {
                return _navigateTo;
            }
        }

        public PXSetupNotEnteredException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            PXReflectionSerializer.RestoreObjectProps(this, info);
        }

        /// <exclude />
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            PXReflectionSerializer.GetObjectData(this, info);
            base.GetObjectData(info, context);
        }

    }

    /// <exclude/>
	[Serializable]
    public class PXSetupNotEnteredException<TDAC, TKeyField> : PXSetupNotEnteredException
    {
        public PXSetupNotEnteredException(string format, string keyvalue, params object[] args) : base(format, typeof(TDAC), new Dictionary<Type, object>{ { typeof(TKeyField), (object)keyvalue } }, new[] { GetDisplayName() }.Concat(args).ToArray())
        {
        }

        public PXSetupNotEnteredException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            PXReflectionSerializer.RestoreObjectProps(this, info);
        }

        protected static string GetDisplayName()
        {
            string name = typeof(TDAC).Name;
            if (typeof(TDAC).IsDefined(typeof(PXCacheNameAttribute), true))
            {
                PXCacheNameAttribute attr = (PXCacheNameAttribute)(typeof(TDAC).GetCustomAttributes(typeof(PXCacheNameAttribute), true)[0]);
                name = attr.GetName();
            }
            return name;
        }
    }

    /// <exclude/>
    [Serializable]
	public class PXSetupNotEnteredException<TDAC> : PXSetupNotEnteredException
	{
		public PXSetupNotEnteredException() : this(ErrorMessages.SetupNotEntered){}
 		public PXSetupNotEnteredException(string format, params object[] args) : base(format, typeof(TDAC), null, new []{GetDisplayName()}.Concat(args).ToArray())
		{
		}

		public PXSetupNotEnteredException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			PXReflectionSerializer.RestoreObjectProps(this, info);		
		}

		protected static string GetDisplayName()
		{
			string name = typeof(TDAC).Name;
			if (typeof(TDAC).IsDefined(typeof(PXCacheNameAttribute), true))
			{
				PXCacheNameAttribute attr = (PXCacheNameAttribute)(typeof(TDAC).GetCustomAttributes(typeof(PXCacheNameAttribute), true)[0]);
				name = attr.GetName();
			}
			return name;
		}
	}

    /// <exclude/>
	[Serializable]
	public class PXSetPropertyException : PXOverridableException
	{
		protected PXErrorLevel _ErrorLevel = PXErrorLevel.Error;
		protected object _ErrorValue;

		public object ErrorValue
		{
			get
			{
				return _ErrorValue;
			}
			set
			{
				_ErrorValue = value;
			}
		}

		public PXErrorLevel ErrorLevel
		{
			get
			{
				return this._ErrorLevel;
			}
		}

		public override string Message
		{
			get
			{
				return base.MessageNoNumber;
			}
		}

		public override void SetMessage(string message)
		{
			if (MessagePrefix != null)
			{
				string strtemp = MessagePrefix + ':';
				int idx = message.IndexOf(strtemp);
				if (idx != -1)
				{
					message = message.Substring(message.IndexOf(strtemp) + strtemp.Length);
				}
			}
			_Message = PXMessages.LocalizeNoPrefix(message);
		}

		public PXSetPropertyException(string message)
			: base(message)
		{
		}

		public PXSetPropertyException(string message, PXErrorLevel errorLevel)
			: this(message)
		{
			this._ErrorLevel = errorLevel;
		}

		public PXSetPropertyException(string format, params object[] args)
			: base(format, args)
		{
		}

		public PXSetPropertyException(string format, PXErrorLevel errorLevel, params object[] args)
			: base(format, args)
		{
			this._ErrorLevel = errorLevel;
		}

        public PXSetPropertyException(Exception inner, PXErrorLevel errorLevel, string format, params object[] args)
			: base(inner, format, args)
		{
            this._ErrorLevel = errorLevel;
        }

		public PXSetPropertyException(SerializationInfo info, StreamingContext context)
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

    /// <exclude/>
	public class PXFieldProcessingException : PXSetPropertyException
	{
		public readonly string FieldName;

        protected PXFieldProcessingException(string fieldName, Exception inner, PXErrorLevel errorLevel, string format, params object[] args)
			: base(inner, errorLevel, format, args)
		{
			FieldName = fieldName;
			if (_Message != null && _Message.Length > 2
				&& _Message[_Message.Length - 1] == '.'
				&& _Message[_Message.Length - 2] == '.'
				&& _Message[_Message.Length - 3] != '.')
			{
				_Message = _Message.Substring(0, _Message.Length - 1);
			}
		}

		public PXFieldProcessingException(string fieldName, Exception inner, PXErrorLevel errorLevel, params object[] args)
			:this(fieldName, inner, errorLevel, ErrorMessages.ErrorFieldProcessing, args)
		{
		}

		public PXFieldProcessingException(SerializationInfo info, StreamingContext context)
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

    /// <exclude/>
	public class PXFieldValueProcessingException : PXFieldProcessingException
	{
		public PXFieldValueProcessingException(string fieldName, Exception inner, PXErrorLevel errorLevel, params object[] args)
			: base(fieldName, inner, errorLevel, ErrorMessages.ErrorFieldValueProcessing, args)
		{
		}

		public PXFieldValueProcessingException(SerializationInfo info, StreamingContext context)
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

    /// <exclude/>
    public class PXErrorContextProcessingException : PXOverridableException
    {
        public PXErrorContextProcessingException(PXGraph graph, object context, Exception inner)
            : base(String.Empty, inner)
        {
            PXCache cache = graph.Caches[context.GetType()];
            System.Text.StringBuilder msgBuilder = new System.Text.StringBuilder("");
            foreach(var k in cache.Keys)
                    msgBuilder.AppendFormat("{0}='{1}', ", k, context.GetType().GetProperty(k).GetValue(context));
            msgBuilder.Remove(msgBuilder.Length - 2, 1);
            SetMessage(String.Format(ErrorMessages.ErrorContextShell, context.GetType().Name, msgBuilder.ToString(), inner.Message));
        }

        public PXErrorContextProcessingException(SerializationInfo info, StreamingContext context)
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
    /// <exclude/>
    public class PXSetPropertyKeepPreviousException : PXSetPropertyException
	{
		public PXSetPropertyKeepPreviousException(string message)
			: base(message)
		{
		}

		public PXSetPropertyKeepPreviousException(string message, PXErrorLevel errorLevel)
			: base(message, errorLevel)
		{
		}

		public PXSetPropertyKeepPreviousException(string format, PXErrorLevel errorLevel, params object[] args)
			: base(format, errorLevel, args)
		{
		}

        public PXSetPropertyKeepPreviousException(Exception inner, PXErrorLevel errorLevel, string format, params object[] args)
			: base(inner, errorLevel, format, args)
		{
        }

		public PXSetPropertyKeepPreviousException(SerializationInfo info, StreamingContext context)
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

    /// <exclude/>
	public class PXSetPropertyException<Field> : PXSetPropertyException
		where Field : IBqlField 
	{
		public PXSetPropertyException(string message)
			: base(message)
		{
			this._MapErrorTo = typeof(Field).Name;
		}

		public PXSetPropertyException(string message, PXErrorLevel errorLevel)
			: base(message, errorLevel)
		{
			this._MapErrorTo = typeof(Field).Name;
		}

		public PXSetPropertyException(string format, params object[] args)
			: base(format, args)
		{
			this._MapErrorTo = typeof(Field).Name;
		}

		public PXSetPropertyException(string format, PXErrorLevel errorLevel, params object[] args)
			: base(format, errorLevel, args)
		{
			this._MapErrorTo = typeof(Field).Name;
		}

		public PXSetPropertyException(Exception inner, PXErrorLevel errorLevel, string format, params object[] args)
			: base(inner, errorLevel, format, args)
		{
			this._MapErrorTo = typeof(Field).Name;
		}

		public PXSetPropertyException(SerializationInfo info, StreamingContext context)
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

    /// <exclude/>
	public class PXForeignRecordDeletedException : PXSetPropertyException
	{
		public PXForeignRecordDeletedException()
			: base(ErrorMessages.ForeignRecordDeleted)
		{
		}

		public PXForeignRecordDeletedException(SerializationInfo info, StreamingContext context)
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

    /// <exclude/>
    public class PXRestartOperationException : PXOverridableException
    {
        public PXRestartOperationException(Exception exc) : base(String.Empty, exc) { }
    }

    /// <exclude/>
	public class PXLockViolationException : PXOverridableException
	{
		private const string _keySeparator = ", ";
		private readonly bool _deletedDatabaseRecord;

		private PXDBOperation _Operation;
		public PXDBOperation Operation
		{
			get
			{
				return _Operation;
			}
			set
			{
				_Operation = value;
			}
		}
		private Type _Table;
		public Type Table
		{
			get
			{
				return _Table;
			}
			set
			{
				_Table = value;
			}
		}
		private bool _Retry;
		public bool Retry
		{
			get
			{
				return _Retry;
			}
			set
			{
				_Retry = value;
			}
		}
		private object[] _Keys;
		public object[] Keys
		{
			get 
			{ 
				return _Keys; 
			}
			set 
			{ 
				_Keys = value;
			}
		}
		public override string Message
		{
			get
			{
				switch (_Operation)
				{
					case PXDBOperation.Insert:
						if (_deletedDatabaseRecord)
						{
							var retryOrLostChanges = PXMessages.LocalizeNoPrefix(_Retry ? ErrorMessages.RetrySavingRecord : ErrorMessages.ChangesWillBeLost);
							_Message = PXMessages.LocalizeFormatNoPrefixNLA(ErrorMessages.CannotInsertDeletedDatabaseRecord, _Table.Name, GetKeysString(), retryOrLostChanges);
						}
						else
						{
							_Message = PXMessages.LocalizeFormat(ErrorMessages.RecordAddedByAnotherProcess, out _MessagePrefix, _Table.Name, _Retry ? ErrorMessages.RetrySavingRecord : ErrorMessages.ChangesWillBeLost);
						}
						break;
					case PXDBOperation.Update:
						_Message = _deletedDatabaseRecord ?
							PXMessages.LocalizeFormatNoPrefixNLA(ErrorMessages.CannotUpdateDeletedDatabaseRecord, _Table.Name, GetKeysString()) :
							PXMessages.LocalizeFormat(ErrorMessages.RecordUpdatedByAnotherProcess, out _MessagePrefix, _Table.Name);
						break;
					case PXDBOperation.Delete:
						_Message = _deletedDatabaseRecord ?
							PXMessages.LocalizeFormatNoPrefixNLA(ErrorMessages.CannotDeleteDeletedDatabaseRecord, _Table.Name, GetKeysString()) :
							PXMessages.LocalizeFormat(ErrorMessages.RecordDeletedByAnotherProcess, out _MessagePrefix, _Table.Name);
						break;
				}
				return base.Message;
			}
		}

		public PXLockViolationException(Type table, PXDBOperation operation, object[] keys, bool deletedDatabaseRecord)
			: base("")
		{
			_Table = table;
			_Operation = operation;
			_Keys = keys;
			_deletedDatabaseRecord = deletedDatabaseRecord;
		}

		public PXLockViolationException(Type table, PXDBOperation operation, object[] keys)
			: this(table, operation, keys, false)
		{
		}

		public PXLockViolationException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			PXReflectionSerializer.RestoreObjectProps(this, info);		
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			PXReflectionSerializer.GetObjectData(this, info);
			base.GetObjectData(info, context);
		}

		private string GetKeysString()
		{
			if (_Keys == null)
			{
				return string.Empty;
			}

			var keyStringBuilder = new StringBuilder();

			for (var i = 0; i < _Keys.Length; i++)
			{
				var key = _Keys[i];
				if (key == null)
				{
					continue;
				}

				if (i > 0)
				{
					keyStringBuilder.Append(_keySeparator);
				}

				var keyString = key.ToString().Trim();
				keyStringBuilder.Append(keyString);
			}

			return keyStringBuilder.ToString();
		}
	}

    /// <exclude/>
	public class PXCommandPreparingException : PXOverridableException
	{
		public readonly string Name;
		public readonly object Value;
		public PXCommandPreparingException(string name, object value, string message)
			: base(message)
		{
			Name = name;
			Value = value;
		}
		public PXCommandPreparingException(string name, object value, string format, params object[] args)
			: base(format, args)
		{
			Name = name;
			Value = value;
		}

		public PXCommandPreparingException(SerializationInfo info, StreamingContext context)
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

    /// <exclude/>
	public class PXRowPersistingException : PXOverridableException
	{
		public readonly string Name;
		public readonly object Value;
		public PXRowPersistingException(string name, object value, string message)
			: base(message)
		{
			Name = name;
			Value = value;
		}
		public PXRowPersistingException(string name, object value, string format, params object[] args)
			: base(format, args)
		{
			Name = name;
			Value = value;
		}

		public PXRowPersistingException(SerializationInfo info, StreamingContext context)
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

    /// <exclude/>
	public class PXRowPersistedException : PXOverridableException
	{
		public readonly string Name;
		public readonly object Value;
		public PXRowPersistedException(string name, object value, string message)
			: base(message)
		{
			Name = name;
			Value = value;
		}
		public PXRowPersistedException(string name, object value, string format, params object[] args)
			: base(format, args)
		{
			Name = name;
			Value = value;
		}

		public PXRowPersistedException(SerializationInfo info, StreamingContext context)
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

    /// <exclude/>
	public class PXRestrictionViolationException : PXOverridableException
	{
		public readonly int Index;
		public PXRestrictionViolationException(string message, object[] keys, int index)
			: base(message, keys)
		{
			Index = index;
		}
		public PXRestrictionViolationException(SerializationInfo info, StreamingContext context)
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

    /// <exclude/>
	public class PXDatabaseException : PXOverridableException
	{
		protected string _Table;
		public string Table
		{
			get { return _Table; }
		}

		private bool _Retry;
		public bool Retry
		{
			get
			{
				return _Retry;
			}
			set
			{
				_Retry = value;
			}
		}

		protected object[] _Keys;
		public object[] Keys
		{
			get	{ return _Keys;	}
			set { _Keys = value; }
		}

		protected PXDbExceptions _ErrorCode;
		public PXDbExceptions ErrorCode
		{
			get { return _ErrorCode; }
		}

		protected bool _IsFriendlyMessage;
		public bool IsFriendlyMessage
		{
			get { return _IsFriendlyMessage; }
			set { _IsFriendlyMessage = value; }
		}

		public PXDatabaseException(string table, object[] keys, PXDbExceptions errCode, string message, Exception inner)
			: base(message, inner)
		{
			_Table = table;
			_Keys = keys;
			_ErrorCode = errCode;
			if (errCode == PXDbExceptions.Deadlock)
			{
				Retry = true;
			}
		}

		public PXDatabaseException(string table, object[] keys, string message, Exception inner)
			: base(message, inner)
		{
			_Table = table;
			_Keys = keys;
			_ErrorCode = PXDbExceptions.Unknown;
		}

		public PXDatabaseException(string table, object[] keys, PXDbExceptions errCode, string message)
			: base(message)
		{
			_Table = table;
			_Keys = keys;
			_ErrorCode = errCode;
		}

		public PXDatabaseException(string table, object[] keys, string message)
			: base(message)
		{
			_Table = table;
			_Keys = keys;
			_ErrorCode = PXDbExceptions.Unknown;
		}

		public PXDatabaseException(SerializationInfo info, StreamingContext context)
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

    /// <exclude/>
	public class PXDbOperationSwitchRequiredException : PXDatabaseException
	{
		public PXDbOperationSwitchRequiredException(String tableName, String message)
			: base(tableName, null, PXDbExceptions.OperationSwitchRequired, message) {}
	}

    /// <exclude/>
	public class PXVisibiltyUpdateRequiredException : PXDbOperationSwitchRequiredException {
		public PXVisibiltyUpdateRequiredException(String tableName) : base(tableName, "Visibility update of the shared record is required.") {}
	}
    /// <exclude/>
	public class PXInsertSharedRecordRequiredException : PXDbOperationSwitchRequiredException {
		public PXInsertSharedRecordRequiredException(String tableName) : base(tableName, "Insert of a shared record is required.") {}
	}
    /// <exclude/>
	public class PXUpdateDeletedFlagRequiredException : PXDbOperationSwitchRequiredException {
		public PXUpdateDeletedFlagRequiredException(String tableName) : base(tableName, "Update of the deleted flag is required.") { }
	}

	public class PXDataWouldBeTruncatedException : PXDatabaseException
	{
		public PXDataWouldBeTruncatedException(string table, Exception inner) : base(table, null, PXDbExceptions.DataWouldBeTruncated, inner.Message, inner)
		{
		}

		public string Column { get; set; }

		public PXDBOperation? Operation { get; set; }

		public string CommandText { get; set; }

		public override string Message
		{
			get
			{
				string prefix;
				switch (Operation)
				{
					case PXDBOperation.Insert:
						prefix = ErrorMessages.GetLocal(ErrorMessages.Inserting);
						break;
					case PXDBOperation.Delete:
						prefix = ErrorMessages.GetLocal(ErrorMessages.Deleting);
						break;
					default:
						prefix = ErrorMessages.GetLocal(ErrorMessages.Updating);
						break;
				}

				if (String.IsNullOrEmpty(Column))
					_Message = PXMessages.LocalizeFormat(ErrorMessages.DataWouldBeTruncated, out _MessagePrefix, prefix, _Table);
				else
					_Message = PXMessages.LocalizeFormat(ErrorMessages.DataWouldBeTruncatedWithColumnName, out _MessagePrefix, prefix, _Table, Column);

				return base.Message;
			}
		}
	}


	/// <exclude/>
	public class PXUnderMaintenanceException : Exception
    {
        public PXUnderMaintenanceException()
            : base(ErrorMessages.SiteUnderMaintenance)
        {
        }

		public PXUnderMaintenanceException(SerializationInfo info, StreamingContext context)
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
    /// <exclude/>
	public class PXForceLogOutException : Exception
	{
		public String Url { get; private set; }
		public String Title { get; private set; }

		public PXForceLogOutException(PXLogOutReason reason)
			: base(GetRedirectMessage(reason))
		{
			Title = GetTitle(reason);
			Url = GetLoginUrl(reason);
		}

		public PXForceLogOutException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			PXReflectionSerializer.RestoreObjectProps(this, info);
		}
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			PXReflectionSerializer.GetObjectData(this, info);
			base.GetObjectData(info, context);
		}

        private static String ReasonToString(PXLogOutReason reason)
        {
            switch (reason)
            {
                case PXLogOutReason.UsersExceeded:
                    return ActionsMessages.UsersExceeded;
                case PXLogOutReason.CompaniesExceeded:
                    return ActionsMessages.CompaniesExceeded;
                default:
                    return reason.ToString();
            }
        }

		private static String GetLoginUrl(PXLogOutReason reason)
		{
			String result = String.Concat(PX.Export.Authentication.FormsAuthenticationModule.LoginUrl,
				"?returnUrl=",
				System.Web.VirtualPathUtility.ToAbsolute(PX.Export.Authentication.FormsAuthenticationModule.DefaultUrl));

			if (reason == PXLogOutReason.UserDisabled || reason == PXLogOutReason.SnapshotRestored)
				result = String.Concat(result, "&message=", System.Web.HttpUtility.UrlEncode(GetTitle(reason)));
            else
                result = String.Concat(result, "&licenseexceeded=",
                    System.Web.HttpUtility.UrlEncode(ReasonToString(reason)));

			if (System.Web.HttpContext.Current == null) return result;
			return PXSessionStateStore.GetSessionUrl(System.Web.HttpContext.Current, result);
		}
		private static String GetRedirectMessage(PXLogOutReason reason)
		{
			return "Refresh|" + GetLoginUrl(reason) + "|";
		}
		private static String GetTitle(PXLogOutReason reason)
		{
			switch (reason)
			{
				case PXLogOutReason.UserDisabled:
					return ActionsMessages.UserDisabledReason;
				case PXLogOutReason.CompaniesExceeded:
				case PXLogOutReason.UsersExceeded:
					return PXMessages.LocalizeFormatNoPrefix(ActionsMessages.LogoutReason, ReasonToString(reason));
				case PXLogOutReason.SnapshotRestored:
					return ActionsMessages.LogoutSnapshotReason;
				default :
					return String.Empty;
			}
		}

	    internal static PXForceLogOutException Find(Exception exception)
	    {
	        return exception != null
                ? (exception as PXForceLogOutException ?? Find(exception.InnerException))
                : null;
	    }
	}
    /// <exclude/>
	public class PXUndefinedCompanyException : Exception
	{
		public PXUndefinedCompanyException()
			: base(ErrorMessages.UndefinedCompany)
		{
		}
		public PXUndefinedCompanyException(SerializationInfo info, StreamingContext context)
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

    public class PXSessionRollbackException : PXException
    {
        public PXSessionRollbackException(Exception inner)
            : base(inner, ErrorMessages.SessionRollback, inner.Message)
        {}
    }

    /// <exclude/>
	public class PXOuterException : PXException
	{
		protected Dictionary<string, string> _InnerExceptions;
		public virtual string[] InnerMessages
		{
			get
			{
				string[] ret = new string[_InnerExceptions.Count];
				_InnerExceptions.Values.CopyTo(ret, 0);
				return ret;
			}
		}

        internal virtual string GetErrorOfField(string name)
        {
            return _InnerExceptions.TryGetValue(name, out var error) ? error : null;
        }

		public virtual string[] InnerFields
		{
			get
			{
				string[] ret = new string[_InnerExceptions.Count];
				_InnerExceptions.Keys.CopyTo(ret, 0);
				return ret;
			}
		}
		public virtual void InnerRemove(string fieldName)
		{
			_InnerExceptions.Remove(fieldName);
		}

		protected Type _GraphType;
		public virtual Type GraphType
		{
			get
			{
				return _GraphType;
			}
		}

		protected object _Row;
		public virtual object Row
		{
			get
			{
				return _Row;
			}
		}

		public PXOuterException(Dictionary<string, string> innerExceptions, Type graphType, object row, string message)
			: base(message)
		{
			_InnerExceptions = innerExceptions;
			_GraphType = graphType;
			_Row = row;
		}

		// Formats exception message like String.Format() does
		public PXOuterException(Dictionary<string, string> innerExceptions, Type graphType, object row, string format, params object[] args)
			: base(format, args)
		{
			_InnerExceptions = innerExceptions;
			_GraphType = graphType;
			_Row = row;
		}

		public PXOuterException(SerializationInfo info, StreamingContext context)
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

    /// <exclude/>
	public class PXNotEnoughRightsException : PXSetPropertyException
	{
		private PXCacheRights rightsMissing;

		public PXNotEnoughRightsException(PXCacheRights rightsMissing)
			: base(ErrorMessages.NotEnoughRights, rightsMissing.ToString())
		{
			this.rightsMissing = rightsMissing;
		}

		public PXNotEnoughRightsException(PXCacheRights rightsMissing, string message)
			: base(message)
		{
			this.rightsMissing = rightsMissing;
		}

		public PXCacheRights RightsMissing
		{
			get { return this.rightsMissing; }
		}
		public PXNotEnoughRightsException(SerializationInfo info, StreamingContext context)
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
}
