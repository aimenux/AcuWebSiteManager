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
using System.Text;

namespace PX.Data
{
    /// <exclude/>
    [Serializable]
	[PXCacheName(PX.Data.EP.Messages.Note)]
	public class Note : IBqlTable
	{
		#region NoteID
        /// <exclude/>
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		private Guid? _NoteID;
		[PXDBSequentialGuid(IsKey = true)]
		[PXUIField(Visibility = PXUIVisibility.SelectorVisible)]
		public Guid? NoteID
		{
			get
			{
				return _NoteID;
			}
			set
			{
				_NoteID = value;
			}
		}
		#endregion

		#region NoteText
        /// <exclude/>
		public abstract class noteText : PX.Data.BQL.BqlString.Field<noteText> { }
		private string _NoteText;
		[PXDBString(IsUnicode = true)]
		[PXNoteTextAttribute]
		[PXUIField(DisplayName = PX.Data.EP.Messages.NoteText)]
		public string NoteText
		{
			get
			{
				return _NoteText;
			}
			set
			{
				_NoteText = value;
			}
		}
		#endregion

		#region EntityType
        /// <exclude/>
		public abstract class entityType : PX.Data.BQL.BqlString.Field<entityType> { }
		private string _EntityType;
		[PXDBString()]
		[PXUIField(Visibility = PXUIVisibility.SelectorVisible)]
		public string EntityType
		{
			get
			{
				return _EntityType;
			}
			set
			{
				_EntityType = value;
			}
		}
		#endregion

		#region GraphType
        /// <exclude/>
		public abstract class graphType : PX.Data.BQL.BqlString.Field<graphType> { }
		private string _GraphType;
		[PXDBString()]
		public string GraphType
		{
			get
			{
				return _GraphType;
			}
			set
			{
				_GraphType = value;
			}
		}
		#endregion

		#region ExternalKey
        /// <exclude/>
		public abstract class externalKey : PX.Data.BQL.BqlString.Field<externalKey> { }
		[PXDBString(255, IsUnicode = true)]
		[PXUIField(Visible = false)]
		public string ExternalKey { get; set; }
        #endregion

        #region NotePopupText
        /// <exclude/>
        [PXDBString(IsUnicode = true)]
        [PXNoteText]
        [PXUIField(DisplayName = PX.Data.EP.Messages.NoteText)]
        public string NotePopupText { get; set; }
        public abstract class notePopupText : PX.Data.BQL.BqlString.Field<notePopupText> { }
        #endregion

        #region unbound fields
        #region EntityName
        /// <exclude/>
        public abstract class entityName : PX.Data.BQL.BqlString.Field<entityName> { }
		private string _EntityName;
		[PXString()]
		[PXUIField(Visibility = PXUIVisibility.SelectorVisible)]
		public string EntityName
		{
			get
			{
				return _EntityName;
			}
			set
			{
				_EntityName = value;
			}
		}
		#endregion
		#endregion
	}

    /// <exclude/>
    [Serializable]
	public sealed class NoteDoc : IBqlTable
	{
		#region NoteID
        /// <exclude/>
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		private Guid? _NoteID;
		[PXDBGuid(IsKey = true)]
		public Guid? NoteID
		{
			get
			{
				return _NoteID;
			}
			set
			{
				_NoteID = value;
			}
		}
		#endregion

		#region FileID
        /// <exclude/>
		public abstract class fileID : PX.Data.BQL.BqlGuid.Field<fileID> { }
		private Guid? _FileID;
		[PXDBGuid(IsKey = true)]
		public Guid? FileID
		{
			get
			{
				return _FileID;
			}
			set
			{
				_FileID = value;
			}
		}
		#endregion

		#region unbound fields
        /// <exclude/>
		public abstract class entityType : PX.Data.BQL.BqlString.Field<entityType> { }
		private string _EntityType;
		[PXString()]
		public string EntityType
		{
			get
			{
				return _EntityType;
			}
			set
			{
				_EntityType = value;
			}
		}
        /// <exclude/>
		public abstract class entityName : PX.Data.BQL.BqlString.Field<entityName> { }
		private string _EntityName;
		[PXString()]
		[PXUIField(DisplayName = "Entity")]
		public string EntityName
		{
			get
			{
				return _EntityName;
			}
			set
			{
				_EntityName = value;
			}
		}
        /// <exclude/>
		public abstract class entityRowValues : PX.Data.BQL.BqlString.Field<entityRowValues> { }
		private string _EntityRowValues;
		[PXString()]
		[PXUIField(DisplayName = "Row Values")]
		public string EntityRowValues
		{
			get
			{
				return _EntityRowValues;
			}
			set
			{
				_EntityRowValues = value;
			}
		}
		#endregion
	}
}
