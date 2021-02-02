using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.CS;
using System.Globalization;
using PX.Common;

namespace PX.Objects.EP
{

	[Serializable]
	[PXCacheName(Messages.CustomWeek)]
	public partial class EPCustomWeek : IBqlTable
	{
		#region WeekID

		public abstract class weekID : PX.Data.BQL.BqlInt.Field<weekID> { }

		[PXDBInt(IsKey = true)]
		[PXUIField(Visible = false)]
		public virtual Int32? WeekID { get; set; }

		#endregion

		#region FullNumber

		public abstract class fullNumber : PX.Data.BQL.BqlString.Field<fullNumber> { }

		[PXString]
		[PXUIField(DisplayName = "Week")]
		public virtual String FullNumber
		{
			get
			{
				return string.Format("{0}-{1:00}", _year, _number);
			}
		}

		#endregion

		#region Year

		public abstract class year : PX.Data.BQL.BqlInt.Field<year> { }

		private Int32? _year;
		[PXDBInt()]
		[PXUIField(DisplayName = "Year", Visible = false)]
		public virtual Int32? Year
		{
			get
			{
				return _year;
			}
			set
			{
				_year = value;
			}
		}

		#endregion


		#region Number

		public abstract class number : PX.Data.BQL.BqlInt.Field<number> { }

		private Int32? _number;
		[PXDBInt()]
		[PXUIField(DisplayName = "Number")]
		public virtual Int32? Number
		{
			get
			{
				return _number;
			}
			set
			{
				_number = value;
			}
		}

		#endregion

		#region StartDate

		public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }

		private DateTime? _startDate;
		[PXDBDate(PreserveTime = true, UseSmallDateTime = true, UseTimeZone = false)]
		[PXDefault]
		[PXUIField(DisplayName = "Start")]
		public virtual DateTime? StartDate
		{
			get
			{
				return _startDate;
			}
			set
			{
				_startDate = value;
			}
		}

		#endregion

		#region EndDate

		public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }

		private DateTime? _endDate;
		[PXDBDate(PreserveTime = true, UseSmallDateTime = true, UseTimeZone = false)]
		[PXDefault]
		[PXVerifyEndDate(typeof(startDate), AllowAutoChange = false)]
		[PXUIField(DisplayName = "End", Visibility = PXUIVisibility.Visible)]
		public virtual DateTime? EndDate
		{
			get
			{
				return _endDate;
			}
			set
			{
				_endDate = value;
			}
		}

		#endregion

		#region FullWeek

		public abstract class isFullWeek : PX.Data.BQL.BqlBool.Field<isFullWeek> { }

		private bool? _IsFullWeek;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Full Week", Visibility = PXUIVisibility.Visible)]
		public virtual bool? IsFullWeek
		{
			get
			{
				return _IsFullWeek;
			}
			set
			{
				_IsFullWeek = value;
			}
		}

		#endregion

		#region IsActive
		public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }
		protected bool? _IsActive;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active")]
		public virtual bool? IsActive
		{
			get
			{
				return this._IsActive;
			}
			set
			{
				this._IsActive = value;
			}
		}
		#endregion

		#region Description

		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }

		[PXString]
		[PXUIField(DisplayName = "Description")]
		public virtual String Description
		{
			[PXDependsOnFields(typeof(fullNumber), typeof(startDate), typeof(endDate))]
			get
			{
				CultureInfo culture = LocaleInfo.GetCulture();
				if (culture != null && !culture.DateTimeFormat.ShortDatePattern.StartsWith("M"))
				{
					return string.Format("{0} ({1:dd/MM} - {2:dd/MM})", FullNumber, StartDate, EndDate);
				}
				else
				{
					return string.Format("{0} ({1:MM/dd} - {2:MM/dd})", FullNumber, StartDate, EndDate);
				}
			}
		}

		#endregion

		#region ShortDescription

		public abstract class shortDescription : PX.Data.BQL.BqlString.Field<shortDescription> { }

		[PXString]
		[PXUIField(DisplayName = "Description", Visible = false)]
		public virtual String ShortDescription
		{
			[PXDependsOnFields(typeof(fullNumber), typeof(startDate), typeof(endDate))]
			get
			{
				CultureInfo culture = LocaleInfo.GetCulture();
				if (culture != null && !culture.DateTimeFormat.ShortDatePattern.StartsWith("M"))
				{
					return string.Format("{0} ({1:dd/MM} - {2:dd/MM})", FullNumber, StartDate, EndDate);
				}
				else
				{
					return string.Format("{0} ({1:MM/dd} - {2:MM/dd})", FullNumber, StartDate, EndDate);
				}
			}
		}

		#endregion

		#region System
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		protected Guid? _CreatedByID;
		[PXDBCreatedByID(DontOverrideValue = true)]
		[PXUIField(Enabled = false)]
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
		[PXUIField(DisplayName = "Created At")]
		[PXDBCreatedDateTime]
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

		#region EntityDescription
		public abstract class entityDescription : PX.Data.BQL.BqlString.Field<entityDescription> { }
		protected string _EntityDescription;
		[PXString(InputMask = "")]
		[PXUIField(DisplayName = "Entity", Visibility = PXUIVisibility.SelectorVisible, Enabled = false, IsReadOnly = true)]
		public virtual string EntityDescription
		{
			get
			{
				return this._EntityDescription;
			}
			set
			{
				this._EntityDescription = value;
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
		#endregion


	}

	[Serializable]
    [PXHidden]
	public partial class EPWeekRaw : IBqlTable
	{
		#region WeekID

		public abstract class weekID : PX.Data.BQL.BqlInt.Field<weekID> { }

		[PXInt(IsKey = true)]
		[PXUIField(Visible = false)]
		public virtual Int32? WeekID { get; set; }

		#endregion
		#region FullNumber

		public abstract class fullNumber : PX.Data.BQL.BqlString.Field<fullNumber> { }

		[PXString]
		[PXUIField(DisplayName = "Week")]
		public virtual String FullNumber { get; set; }

		#endregion

		#region Year

		public abstract class year : PX.Data.BQL.BqlInt.Field<year> { }

		[PXInt()]
		[PXUIField(DisplayName = "Year", Visible = false)]
		public virtual Int32? Year { get; set; }

		#endregion


		#region Number

		public abstract class number : PX.Data.BQL.BqlInt.Field<number> { }

		[PXInt()]
		[PXUIField(DisplayName = "Number")]
		public virtual Int32? Number { get; set; }

		#endregion

		#region StartDate

		public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }

		[PXDate]
		[PXUIField(DisplayName = "Start")]
		public virtual DateTime? StartDate { get; set; }

		#endregion

		#region EndDate

		public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }

		[PXDate]
		[PXUIField(DisplayName = "End", Visibility = PXUIVisibility.Visible)]
		public virtual DateTime? EndDate { get; set; }

		#endregion

		#region FullWeek

		public abstract class isFullWeek : PX.Data.BQL.BqlBool.Field<isFullWeek> { }

		[PXBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Full Week", Visibility = PXUIVisibility.Visible)]
		public virtual bool? IsFullWeek { get; set; }

		#endregion

		#region IsActive
		public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }
		[PXBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active")]
		public virtual bool? IsActive { get; set; }
		#endregion

		#region Description

		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }

		[PXString]
		[PXUIField(DisplayName = "Description")]
		public virtual String Description
		{
			[PXDependsOnFields(typeof(fullNumber), typeof(startDate), typeof(endDate))]
			get
			{
				CultureInfo culture = LocaleInfo.GetCulture();
				if (culture != null && !culture.DateTimeFormat.ShortDatePattern.StartsWith("M"))
				{
					return string.Format("{0} ({1:dd/MM} - {2:dd/MM})", FullNumber, StartDate, EndDate);
				}
				else
				{
					return string.Format("{0} ({1:MM/dd} - {2:MM/dd})", FullNumber, StartDate, EndDate);
				}
			}
		}

		#endregion

		#region ShortDescription

		public abstract class shortDescription : PX.Data.BQL.BqlString.Field<shortDescription> { }

		[PXString]
		[PXUIField(DisplayName = "Description", Visible = false)]
		public virtual String ShortDescription
		{
			[PXDependsOnFields(typeof(fullNumber), typeof(startDate), typeof(endDate))]
			get
			{
				CultureInfo culture = LocaleInfo.GetCulture();
				if (culture != null && !culture.DateTimeFormat.ShortDatePattern.StartsWith("M"))
				{
					return string.Format("{0} ({1:dd/MM} - {2:dd/MM})", FullNumber, StartDate, EndDate);
				}
				else
				{
					return string.Format("{0} ({1:MM/dd} - {2:MM/dd})", FullNumber, StartDate, EndDate);
				}
			}
		}

		#endregion

		//#region System
		//#region CreatedByID
		//public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID>
		//{
		//}
		//protected Guid? _CreatedByID;
		//[PXDBCreatedByID(DontOverrideValue = true)]
		//[PXUIField(DisplayName = "Created By", Enabled = false)]
		//public virtual Guid? CreatedByID
		//{
		//	get
		//	{
		//		return this._CreatedByID;
		//	}
		//	set
		//	{
		//		this._CreatedByID = value;
		//	}
		//}
		//#endregion

		//#region CreatedByScreenID
		//public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID>
		//{
		//}
		//protected String _CreatedByScreenID;
		//[PXDBCreatedByScreenID]
		//public virtual String CreatedByScreenID
		//{
		//	get
		//	{
		//		return this._CreatedByScreenID;
		//	}
		//	set
		//	{
		//		this._CreatedByScreenID = value;
		//	}
		//}
		//#endregion

		//#region CreatedDateTime
		//public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime>
		//{
		//}
		//protected DateTime? _CreatedDateTime;
		//[PXUIField(DisplayName = "Created At")]
		//[PXDBCreatedDateTimeUtc]
		//public virtual DateTime? CreatedDateTime
		//{
		//	get
		//	{
		//		return this._CreatedDateTime;
		//	}
		//	set
		//	{
		//		this._CreatedDateTime = value;
		//	}
		//}
		//#endregion

		//#region LastModifiedByID
		//public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID>
		//{
		//}
		//protected Guid? _LastModifiedByID;
		//[PXDBLastModifiedByID]
		//public virtual Guid? LastModifiedByID
		//{
		//	get
		//	{
		//		return this._LastModifiedByID;
		//	}
		//	set
		//	{
		//		this._LastModifiedByID = value;
		//	}
		//}
		//#endregion

		//#region LastModifiedByScreenID
		//public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID>
		//{
		//}
		//protected String _LastModifiedByScreenID;
		//[PXDBLastModifiedByScreenID]
		//public virtual String LastModifiedByScreenID
		//{
		//	get
		//	{
		//		return this._LastModifiedByScreenID;
		//	}
		//	set
		//	{
		//		this._LastModifiedByScreenID = value;
		//	}
		//}
		//#endregion

		//#region LastModifiedDateTime
		//public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime>
		//{
		//}
		//protected DateTime? _LastModifiedDateTime;
		//[PXDBLastModifiedDateTimeUtc]
		//public virtual DateTime? LastModifiedDateTime
		//{
		//	get
		//	{
		//		return this._LastModifiedDateTime;
		//	}
		//	set
		//	{
		//		this._LastModifiedDateTime = value;
		//	}
		//}
		//#endregion

		//#region EntityDescription
		//public abstract class entityDescription : PX.Data.BQL.BqlString.Field<entityDescription>
		//{
		//}
		//protected string _EntityDescription;
		//[PXString(InputMask = "")]
		//[PXUIField(DisplayName = "Entity", Visibility = PXUIVisibility.SelectorVisible, Enabled = false, IsReadOnly = true)]
		//public virtual string EntityDescription
		//{
		//	get
		//	{
		//		return this._EntityDescription;
		//	}
		//	set
		//	{
		//		this._EntityDescription = value;
		//	}
		//}
		//#endregion

		//#region tstamp
		//public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp>
		//{
		//}
		//protected Byte[] _tstamp;
		//[PXDBTimestamp()]
		//public virtual Byte[] tstamp
		//{
		//	get
		//	{
		//		return this._tstamp;
		//	}
		//	set
		//	{
		//		this._tstamp = value;
		//	}
		//}
		//#endregion
		//#endregion

		public static EPWeekRaw ToEPWeekRaw(EPCustomWeek week)
		{
			EPWeekRaw res = new EPWeekRaw();
			res.EndDate = week.EndDate;
			res.StartDate = week.StartDate;
			res.WeekID = week.WeekID;
			res.Year = week.Year;
			res.Number = week.Number;
			res.FullNumber = week.FullNumber;
			res.IsFullWeek = week.IsFullWeek;
			res.IsActive = week.IsActive;

			return res;
		}

		public static EPWeekRaw ToEPWeekRaw(PXWeekSelectorAttribute.EPWeek week)
		{
			EPWeekRaw res = new EPWeekRaw();
			res.EndDate = week.EndDate;
			res.StartDate = week.StartDate;
			res.WeekID = week.WeekID;
			res.Number = week.Number;
			res.FullNumber = week.FullNumber;
			res.IsFullWeek = true;
			res.IsActive = true;

			return res;
		}
	}



}
