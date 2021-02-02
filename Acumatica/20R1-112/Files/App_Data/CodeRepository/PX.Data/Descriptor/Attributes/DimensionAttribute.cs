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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Globalization;
using PX.Common;
using PX.Data.ReferentialIntegrity;
using PX.DbServices.Model.Entities;
using CommonServiceLocator;
using PX.Data.SQLTree;

namespace PX.Data
{
    /// <exclude/>
	public enum DimensionLookupMode
	{
		BySegmentedKeys = 0, // default
		BySegmentsAndAllAvailableSegmentValues = 1,
		BySegmentsAndChildSegmentValues = 2,
	}

	#region PXDimensionAttribute
	/// <summary>Sets up the input control for a DAC field that holds a
	/// segmented value. The control formats the input as a segmented key
	/// value and displays the list of allowed values for each key segment
	/// when the user presses F3 on a keyboard.</summary>
	/// <example>
	/// <code>
	/// [PXDimension("SUBACCOUNT", ValidComboRequired = false)]
	/// public virtual string SubID { get; set; }
	/// </code>
	/// </example>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Parameter | AttributeTargets.Method)]
	[Serializable]
	public class PXDimensionAttribute : PXEventSubscriberAttribute,
		IPXFieldSelectingSubscriber,
		IPXFieldVerifyingSubscriber,
		IPXFieldDefaultingSubscriber,
		IPXRowPersistingSubscriber,
		IPXRowPersistedSubscriber,
		IPXFieldUpdatingSubscriber
	{
		#region State
		protected string _Dimension;
		protected bool? _ValidComboRequired;
		public GroupHelper.ParamsPair[][] Restrictions;
		protected Definition _Definition;
		protected Delegate _SegmentDelegate;
		protected string[] _SegmentParameters;
	    private Type _parentSelect;
	    protected Type _ParentValueField; // used for "Child Segment Values" lookup mode to retrieve segment's value from a parent record
	    protected PXView _ParentView; // used for "Child Segment Values" lookup mode to retrieve parent records

		/// <exclude/>
		public virtual void SetSegmentDelegate(Delegate handler)
		{
			_SegmentDelegate = handler;
			if (handler != null)
			{
				ParameterInfo[] pars = _SegmentDelegate.Method.GetParameters();
				_SegmentParameters = new string[pars.Length];
				for (int i = 0; i < pars.Length; i++)
				{
					_SegmentParameters[i] = pars[i].Name;
				}
			}
		}
		private System.Collections.IEnumerable getOuterSegments(PXCache sender, short segment, string value, object row)
		{
			object[] pars = new object[_SegmentParameters.Length];
			for (int i = 0; i < pars.Length; i++)
			{
				if (String.Equals(_SegmentParameters[i], "segment", StringComparison.OrdinalIgnoreCase))
				{
					pars[i] = segment;
				}
				else if (String.Equals(_SegmentParameters[i], "value", StringComparison.OrdinalIgnoreCase))
				{
					pars[i] = value;
				}
				else
				{
					pars[i] = sender.GetValueExt(row, _SegmentParameters[i]);
					if (pars[i] is PXFieldState)
					{
						pars[i] = ((PXFieldState)pars[i]).Value;
					}
				}
			}
			return new PXView(sender.Graph, true, new Select<SegmentValue>(), _SegmentDelegate).SelectMultiBound(new object[] { row }, pars);
		}
        /// <summary>Gets or sets the value that indicates whether the user can
        /// specify only one of the predefined values as a segment or the user can
        /// input arbitrary values.</summary>
		public virtual bool ValidComboRequired
		{
			get
			{
				if (_ValidComboRequired == null)
				{
					if (_Definition == null)
					{
						Definition defs = PXContext.GetSlot<Definition>();
						if (defs == null)
						{
							PXContext.SetSlot<Definition>(defs = PXDatabaseGetSlot());
						}
						if (defs != null)
						{
							if (defs.Dimensions.Count == 0)
							{
								return true;
							}
							return defs.ValidCombos.Contains(_Dimension);
						}
						return true;
					}
					else
					{
						return _Definition.ValidCombos.Contains(_Dimension);
					}
				}
				return (bool)_ValidComboRequired;
			}
			set
			{
				_ValidComboRequired = value;
			}
		}
		protected string _Wildcard;
        /// <summary>Gets or sets the one-character-long string that is
        /// treated as a wildcard (that is, a character that matches any symbols).
        /// Typically, the property is set when the field to which the attribute
        /// is attached is used for filtering. See also the <see
        /// cref="PXDimensionWildcardAttribute">PXDimensionWildcard</see>
        /// attribute.</summary>
		public virtual string Wildcard
		{
			get
			{
				return _Wildcard;
			}
			set
			{
				_Wildcard = value;
			}
		}
        /// <exclude/>
		public static int GetLength(string dimensionID)
		{
			PXSegment[] segs;
			Definition def = PXDatabaseGetSlot();
			if (def != null && def.Dimensions.TryGetValue(dimensionID, out segs))
			{
				int ret = 0;
				for (int i = 0; i < segs.Length; i++)
				{
					ret += segs[i].Length;
				}
				return ret;
			}
			return 0;
		}

        /// <exclude/>
        public const int NoMaxLength = -1;
        /// <exclude/>
		public static int GetMaxLength(string dimensionID)
		{
			int size = NoMaxLength;
			if (!String.IsNullOrEmpty(dimensionID))
				if (!_DimensionMaxLength.TryGetValue(dimensionID, out size))
					size = NoMaxLength;
			return size;
		}
        public static Dictionary<string, KeyValuePair<Type, string>> GetTables(string dimensionID)
        {
            Dictionary<string, KeyValuePair<Type, string>> dictTables;
            _DimensionTables.TryGetValue(dimensionID, out dictTables);
            return dictTables;
        }

	    /// <summary>
	    /// A BQL select that defines the data set that is available
	    /// for selectint parent records for lookup mode "By Segments: Child Segment Values".
	    /// </summary>
	    public Type ParentSelect
	    {
		    get { return _parentSelect; }
		    set
		    {
			    if (value != null && (_Definition == null || !_Definition.LookupModes.ContainsKey(_Dimension) || _Definition.LookupModes[_Dimension] == DimensionLookupMode.BySegmentsAndChildSegmentValues))
			    {
					if (!typeof(IBqlSelect).IsAssignableFrom(value))
						throw new PXArgumentException(nameof(ParentSelect), ErrorMessages.CantCreateForeignKeyReference, value);
			    }
				
			    _parentSelect = value;
		    }
	    }

		/// <summary>
		/// Field that is used for "Child Segment Values" lookup mode to retrieve segment's value from a parent record.
		/// </summary>
		public Type ParentValueField
		{
			get { return _ParentValueField; }
			set
			{
				if (value != null && (!typeof (IBqlField).IsAssignableFrom(value) || !value.IsNested))
					throw new PXArgumentException(nameof(ParentValueField), ErrorMessages.InvalidIBqlField);
				_ParentValueField = value;
			}
		}

        /// <exclude />
        public DimensionLookupMode? DimensionLookupModeState
        {
            get
            {
                if (_Definition == null || !_Definition.LookupModes.ContainsKey(_Dimension))
                    return null;
                return _Definition.LookupModes[_Dimension];
            }
        }

        #endregion

        #region Ctor
        /// <summary>
        /// Creates an instance to work with the provided segmented key.
        /// </summary>
        /// <param name="dimension">The string identifier of the segmented key.</param>
        public PXDimensionAttribute(string dimension)
			: base()
		{
			if (dimension == null)
			{
				throw new PXArgumentException(nameof(dimension), ErrorMessages.ArgumentNullException);
			}
			_Dimension = dimension;
		}
		private class SegDescr : PXSegment
		{
			public readonly string DimensionID;
			public readonly short SegmentID;
			public readonly bool AutoNumber;
			public readonly string ParentDimensionID;
			public SegDescr(string dimensionID, short? segmentID, char editMask, char fillCharacter, short? length, bool? validate, short? caseConvert, short? align, char separator, bool? readOnly, string descr, string parentDimensionID, char promptCharacter = '_')
				: base(editMask, fillCharacter, (short)length, (bool)validate, (short)caseConvert, (short)align, separator, false, descr, promptCharacter)
			{
				DimensionID = dimensionID;
				SegmentID = (short)segmentID;
				AutoNumber = (bool)readOnly;
				ParentDimensionID = parentDimensionID;
			}
		}
		#endregion

        #region Implementation
        /// <exclude/>
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				e.ReturnState = PXSegmentedState.CreateInstance(e.ReturnState, _FieldName, _Definition != null && _Definition.Dimensions.ContainsKey(_Dimension) ? _Definition.Dimensions[_Dimension] : new PXSegment[0],
					!(e.ReturnState is PXFieldState) || String.IsNullOrEmpty(((PXFieldState)e.ReturnState).ViewName) ? "_" + _Dimension + "_Segments_" : null,
					_Definition != null && _Definition.LookupModes.ContainsKey(_Dimension) ? _Definition.LookupModes[_Dimension] : DimensionLookupMode.BySegmentedKeys,
					ValidComboRequired, _Wildcard);
				((PXSegmentedState)e.ReturnState).IsUnicode = true;
				((PXSegmentedState)e.ReturnState).DescriptionName = typeof(SegmentValue.descr).Name;
			}
		}
        /// <exclude/>
		public virtual void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (!_Definition.Dimensions.ContainsKey(_Dimension))
			{
				throw new PXSetPropertyException(PXMessages.LocalizeFormat(ErrorMessages.DimensionDontExist, _Dimension));
			}
			PXSegment[] segs = _Definition.Dimensions[_Dimension];
			string val = e.NewValue as string;
			if (val == null || val.StartsWith(int.MinValue.ToString().Substring(0, 5)))
			{
				return;
			}
			int start = 0;
			List<string> errs = new List<string>();
			for (int i = 0; i < segs.Length; i++)
			{
				if (((SegDescr)segs[i]).AutoNumber)
				{
					if (sender.Locate(e.Row) == null)
					{
						string oldValue = sender.GetValue(e.Row, _FieldOrdinal) as string;
						if (oldValue != null && start < oldValue.Length && start < val.Length)
						{
							string def;
							if (start + segs[i].Length <= oldValue.Length)
							{
								def = oldValue.Substring(start, segs[i].Length);
							}
							else
							{
								def = oldValue.Substring(start);
							}
							if (start + segs[i].Length <= val.Length)
							{
								e.NewValue = val.Substring(0, start) + def + val.Substring(start + segs[i].Length);
							}
							else
							{
								e.NewValue = val.Substring(0, start) + def;
							}
						}
					}
				}
				else if (segs[i].Validate)
				{
					string curr;
					if (start < val.Length)
					{
						if (start + segs[i].Length <= val.Length)
						{
							curr = val.Substring(start, segs[i].Length);
						}
						else
						{
							curr = val.Substring(start);
						}
					}
					else
					{
						curr = String.Empty;
					}
					Dictionary<string, ValueDescr> vals = _Definition.Values[_Dimension][((SegDescr)segs[i]).SegmentID];
					if (!vals.ContainsKey(curr) && !vals.ContainsKey(curr = curr.TrimEnd()))
					{
						if (!String.IsNullOrEmpty(_Wildcard))
						{
							bool fullwild = false;
							for (int k = 0; k < _Wildcard.Length; k++)
							{
								if (curr == new String(_Wildcard[k], segs[i].Length))
								{
									fullwild = true;
									break;
								}
							}
							if (fullwild)
							{
								continue;
							}
							bool meet = true;
							foreach (string key in vals.Keys)
							{
								meet = true;
								for (int j = 0; j < curr.Length; j++)
								{
									bool wild = false;
									for (int k = 0; k < _Wildcard.Length; k++)
									{
										if (curr[j] == _Wildcard[k])
										{
											wild = true;
											break;
										}
									}
									if (wild)
									{
										continue;
									}
									if (j >= key.Length || curr[j] != key[j])
									{
										meet = false;
										break;
									}
								}
								if (meet)
								{
									break;
								}
							}
							if (meet)
							{
								continue;
							}
						}
						errs.Add(String.IsNullOrEmpty(((SegDescr)segs[i]).Descr) ? ((SegDescr)segs[i]).SegmentID.ToString() : ((SegDescr)segs[i]).Descr);
					}
					else
					{
						bool match = true;
						if (Restrictions != null)
						{
							byte[] segmask = vals[curr].GroupMask;
							if (segmask != null)
							{
								for (int m = 0; m < Restrictions.Length; m++)
								{
                                    int failed = 0;
									for (int l = 0; l < Restrictions[m].Length; l++)
									{
										int verified = 0;
										for (int j = l * 4; j < l * 4 + 4; j++)
										{
											verified = verified << 8;
											if (j < segmask.Length)
											{
												verified |= (int)segmask[j];
											}
										}
										if ((verified & Restrictions[m][l].First) != 0)
										{
											match = false;
										}
                                        if ((verified & Restrictions[m][l].Second) == 0)
                                        {
                                            failed++;
										}
									}
									if (!match)
									{
                                        if (failed < Restrictions[m].Length)
                                        {
                                            match = true;
                                        }
                                        else
                                        {
										break;
									}
								}
							}
						}
						}
						if (match && _SegmentDelegate != null)
						{
							match = FindValueBySegmentDelegate(sender, e.Row, ((SegDescr)segs[i]).Descr, ((SegDescr)segs[i]).SegmentID, val, curr);
						}
						if (!match)
						{
							errs.Add(String.IsNullOrEmpty(((SegDescr)segs[i]).Descr) ? ((SegDescr)segs[i]).SegmentID.ToString() : ((SegDescr)segs[i]).Descr);
						}
					}
				}
				start += segs[i].Length;
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
        /// <exclude/>
		protected virtual bool FindValueBySegmentDelegate(PXCache sender, object row, string segmentDescr, short segmentID, string val, string currentValue)
		{
			foreach (SegmentValue sv in getOuterSegments(sender, segmentID, val, row))
			{
				if (String.Equals(sv.Value, currentValue, StringComparison.OrdinalIgnoreCase))
					return true;
			}

			return false;
		}
		/// <exclude/>
		public virtual void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			string numbering;
			if (e.NewValue == null && !suppressAutoNumbering && (_Definition.Autonumbers.TryGetValue(_Dimension, out numbering) || customNumbering != null || _SegmentDelegate != null && e.Row != null))
			{
				e.NewValue = prepValue("", "");
				if (_SegmentDelegate != null && e.NewValue is string)
				{
					int start = 0;
					for (int i = 0; i < _Definition.Dimensions[_Dimension].Length; i++)
					{
						PXSegment seg = _Definition.Dimensions[_Dimension][i];
						if (seg.Validate)
						{
							string theOnly = null;
							foreach (SegmentValue sv in getOuterSegments(sender, ((SegDescr)seg).SegmentID, (string) e.NewValue, e.Row))
							{
								if (theOnly != null)
								{
									theOnly = null;
									break;
								}
								theOnly = sv.Value;
							}
							if (theOnly != null)
							{
								if (theOnly.Length < seg.Length)
								{
									theOnly = theOnly + new String(' ', seg.Length - theOnly.Length);
								}
								else if (theOnly.Length > seg.Length)
								{
									theOnly = theOnly.Substring(0, seg.Length);
								}
								((string)e.NewValue).Remove(start, seg.Length).Insert(start, theOnly);
							}
						}
					}
				}
			}
		}

        /// <summary>
        /// Validates if passed value correlates with current segment key rules
        /// </summary>
        /// <typeparam name="TField">Field with segmented key</typeparam>
        /// <param name="sender">Cache</param>
        /// <param name="value">Tested value</param>
        /// <returns></returns>
        public static bool MatchMask<TField>(PXCache sender, String value)
        {
            PXDimensionAttribute da = sender.GetAttributesReadonly(typeof(TField).Name).FirstOrDefault(x => x.GetType().Name == "PXDimensionAttribute") as PXDimensionAttribute;
            if (da == null)
                return true;
            return da.MatchMask(sender, value);
        }

        public virtual bool MatchMask(PXCache sender, String value)
        {
            if (value == null)
                return false;
            value = value.TrimEnd();
            if (value.Length == 0)
                return false;
            PXSegment[] segs = _Definition.Dimensions[_Dimension];
            int keylength = 0;
            for(int i=0; i<segs.Length && value.Length > keylength; i++)
            {
                String segvalue = value.Substring(keylength, value.Length < keylength + segs[i].Length ? value.Length-keylength : segs[i].Length);
                keylength += segs[i].Length;
                bool islast = i == segs.Length - 1;
                switch (segs[i].EditMask)
                {
                    case 'C':
                        break;
                    case 'a':
                        if (segvalue.Any(x => !Char.IsLetterOrDigit(x)))
                            return false;
                        break;
                    case '9':
                        if (segvalue.Any(x => !Char.IsDigit(x)))
                            return false;
                        break;
                    case '?':
                        if (segvalue.Any(x => !Char.IsLetter(x)))
                            return false;
                        break;
                }
                if (islast && value.Length > keylength)
                    return false;
            }
            return true;
        }

        /// <exclude/>
		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (!_Definition.Dimensions.ContainsKey(_Dimension))
			{
				throw new PXSetPropertyException(PXMessages.LocalizeFormat(ErrorMessages.DimensionDontExist, _Dimension));
			}
			PXSegment[] segs = _Definition.Dimensions[_Dimension];
			string val = e.NewValue as string;
			if (val == null)
			{
				return;
			}
			int grandtotal = 0;
			bool trimRequired = false;
			for (int i = 0; i < segs.Length; i++)
			{
				trimRequired = trimRequired || segs[i].Align == (short)0 && grandtotal < val.Length && val[grandtotal] == segs[i].FillCharacter;
				grandtotal += segs[i].Length;
				if (i == segs.Length - 1 && grandtotal > val.Length)
				{
					e.NewValue = val + new string(segs[i].FillCharacter, grandtotal - val.Length);
				}
			}
			if (trimRequired)
			{
				char[] arr = ((string)e.NewValue).ToCharArray();
				int total = 0;
				for (int i = 0; i < segs.Length; i++)
				{
					if (segs[i].Align == (short)0 && arr[total] == segs[i].FillCharacter)
					{
						int j = total + 1;
						for (; j < total + segs[i].Length; j++)
						{
							if (arr[j] != segs[i].FillCharacter)
							{
								break;
							}
						}
						if (j < total + segs[i].Length)
						{
							for (int k = 0; k < segs[i].Length - j + total; k++)
							{
								arr[total + k] = arr[j + k];
								arr[j + k] = segs[i].FillCharacter;
							}
						}
					}
					total += segs[i].Length;
				}
				e.NewValue = new String(arr);
			}
			if (((string)e.NewValue).Length > grandtotal)
			{
				e.NewValue = ((string)e.NewValue).Substring(0, grandtotal);
			}
			e.Cancel = true;
		}
        /// <exclude/>
		public virtual void SelfRowSelecting(PXCache sender, PXRowSelectingEventArgs e, PXSegment[] segs, int length)
		{
			object val = sender.GetValue(e.Row, _FieldOrdinal);
			if (val is string)
			{
                sender.SetValue(e.Row, _FieldOrdinal, AdjustValueLength((string)val, length));
			}
		}

        public static String AdjustValueLength(String value, int length)
        {
            if (value == null)
                return null;
            if(value.Length < length)
            {
                return value + new String(' ', length - value.Length);
            }
            if(value.Length > length)
            {
                return value.Substring(0, length);
            }
            return value;
        }

		private string prepValue(string value, string symbol)
		{
			StringBuilder bld = new StringBuilder();
			int pos = 0;
			for (int i = 0; i < _Definition.Dimensions[_Dimension].Length; i++)
			{
				PXSegment seg = _Definition.Dimensions[_Dimension][i];
				if ((!((SegDescr)seg).AutoNumber && customNumberingSegment == null)
					|| ((SegDescr)seg).SegmentID != customNumberingSegment && customNumberingSegment != null)
				{
					if (pos + seg.Length == value.Length)
					{
						bld.Append(value.Substring(pos));
					}
					else if (pos + seg.Length < value.Length)
					{
						bld.Append(value.Substring(pos, seg.Length));
					}
					else if (pos < value.Length)
					{
						bld.Append(value.Substring(pos));
						bld.Append(seg.FillCharacter, pos + seg.Length - value.Length);
					}
					else
					{
						bld.Append(seg.FillCharacter, seg.Length);
					}
				}
				else
				{
					if (symbol == "")
					{
						foreach (string s in _Definition.Values[_Dimension][((SegDescr)seg).SegmentID].Keys)
						{
							symbol = s;
							break;
						}
					}
					if (symbol.Length == seg.Length)
					{
						bld.Append(symbol);
					}
					else if (symbol.Length > seg.Length)
					{
						bld.Append(symbol.Substring(0, seg.Length));
					}
					else if (seg.Align == (short)0)
					{
						bld.Append(symbol);
						bld.Append(seg.FillCharacter, seg.Length - symbol.Length);
					}
					else
					{
						bld.Append(seg.FillCharacter, seg.Length - symbol.Length);
						bld.Append(symbol);
					}
				}
				pos += (int)seg.Length;
			}
			return bld.ToString();
		}

		string LastNbr;
		string NewNumber;
		int NumberingSEQ;
		string Numbering;
		bool suppressAutoNumbering;
		/// <exclude/>
		public virtual void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) != PXDBOperation.Insert)
			{
				return;
			}
			string val = sender.GetValue(e.Row, _FieldOrdinal) as string;
			if (val != null && val.Trim() == "" && sender.Keys.Contains(_FieldName))
			{
				if (sender.RaiseExceptionHandling(_FieldName, e.Row, null, new PXSetPropertyKeepPreviousException(PXMessages.LocalizeFormat(ErrorMessages.FieldIsEmpty, _FieldName))))
				{
					throw new PXRowPersistingException(_FieldName, null, ErrorMessages.FieldIsEmpty, _FieldName);
				}
				return;
			}
			if ((Numbering = customNumbering) == null)
			{
				if (suppressAutoNumbering || !_Definition.Autonumbers.TryGetValue(_Dimension, out Numbering))
				{
					return;
				}
			}

			int? NBranchID;
			string StartNbr;
			string EndNbr;
			string WarnNbr;
			int NbrStep;
			DateTime? StartDate;
			Guid? CreatedByID;
			string CreatedByScreenID;
			DateTime? CreatedDateTime;
			Guid? LastModifiedByID;
			string LastModifiedByScreenID;
			DateTime? LastModifiedDateTime;

			using (PXDataRecord record = PXDatabase.SelectSingle<NumberingSequence>(
				new PXDataField("EndNbr"),
				new PXDataField(new Column("LastNbr").Coalesce(new Column("StartNbr"))),
				new PXDataField("WarnNbr"),
				new PXDataField("NbrStep"),
				new PXDataField("NumberingSEQ"),
				new PXDataField("NBranchID"),
				new PXDataField("StartNbr"),
				new PXDataField("StartDate"),
				new PXDataField("CreatedByID"),
				new PXDataField("CreatedByScreenID"),
				new PXDataField("CreatedDateTime"),
				new PXDataField("LastModifiedByID"),
				new PXDataField("LastModifiedByScreenID"),
				new PXDataField("LastModifiedDateTime"),
				new PXDataFieldValue("NumberingID", PXDbType.VarChar, 10, Numbering),
				new PXDataFieldValue("StartDate", PXDbType.DateTime, 4, sender.Graph.Accessinfo.BusinessDate, PXComp.LE),
				new PXDataFieldValue("NBranchID", PXDbType.Int, 4, sender.Graph.Accessinfo.BranchID, PXComp.EQorISNULL),
				new PXDataFieldOrder("NBranchID", true),
				new PXDataFieldOrder("StartDate", true)
				))
			{
				if (record == null)
				{
					throw new PXException(ErrorMessages.CantAutoNumber);
				}
				EndNbr = record.GetString(0);
				LastNbr = record.GetString(1);
				WarnNbr = record.GetString(2);
				NbrStep = (int)record.GetInt32(3);
				NumberingSEQ = (int)record.GetInt32(4);
				NBranchID = (int?)record.GetInt32(5);
				StartNbr = record.GetString(6);
				StartDate = record.GetDateTime(7);
				CreatedByID = record.GetGuid(8);
				CreatedByScreenID = record.GetString(9);
				CreatedDateTime = record.GetDateTime(10);
				LastModifiedByID = record.GetGuid(11);
				LastModifiedByScreenID = record.GetString(12);
				LastModifiedDateTime = record.GetDateTime(13);
			}
			NewNumber = nextNumber(LastNbr, NbrStep);

			if (NewNumber.CompareTo(WarnNbr) >= 0)
			{
				//throw new PXException(Messages.WarningNumReached);
				PXUIFieldAttribute.SetWarning(sender, e.Row, _FieldName, ErrorMessages.WarningNumReached);
			}

			if (NewNumber.CompareTo(EndNbr) >= 0)
			{
				throw new PXException(ErrorMessages.EndOfNumberingReached);
			}

			try
			{
				PXDatabase.Update<NumberingSequence>(
					new PXDataFieldAssign("LastNbr", NewNumber),
					new PXDataFieldRestrict("NumberingID", Numbering),
					new PXDataFieldRestrict("NumberingSEQ", NumberingSEQ),
					PXDataFieldRestrict.OperationSwitchAllowed);
			}
			catch (PXDbOperationSwitchRequiredException)
			{
				PXDatabase.Insert<NumberingSequence>(
					new PXDataFieldAssign("EndNbr", PXDbType.VarChar, 15, EndNbr),
					new PXDataFieldAssign("LastNbr", PXDbType.VarChar, 15, NewNumber),
					new PXDataFieldAssign("WarnNbr", PXDbType.VarChar, 15, WarnNbr),
					new PXDataFieldAssign("NbrStep", PXDbType.Int, 4, NbrStep),
					new PXDataFieldAssign("StartNbr", PXDbType.VarChar, 15, StartNbr),
					new PXDataFieldAssign("StartDate", PXDbType.DateTime, StartDate),
					new PXDataFieldAssign("CreatedByID", PXDbType.UniqueIdentifier, 16, CreatedByID),
					new PXDataFieldAssign("CreatedByScreenID", PXDbType.Char, 8, CreatedByScreenID),
					new PXDataFieldAssign("CreatedDateTime", PXDbType.DateTime, 8, CreatedDateTime),
					new PXDataFieldAssign("LastModifiedByID", PXDbType.UniqueIdentifier, 16, LastModifiedByID),
					new PXDataFieldAssign("LastModifiedByScreenID", PXDbType.Char, 8, LastModifiedByScreenID),
					new PXDataFieldAssign("LastModifiedDateTime", PXDbType.DateTime, 8, LastModifiedDateTime),
					new PXDataFieldAssign("NumberingID", PXDbType.VarChar, 10, Numbering),
					new PXDataFieldAssign("NBranchID", PXDbType.Int, 4, NBranchID)
					);
			}

			string oldVal = sender.GetValue(e.Row, _FieldOrdinal) as string;
			if (oldVal == null)
			{
				oldVal = "";
			}
			sender.SetValue(e.Row, _FieldOrdinal, prepValue(oldVal, NewNumber));
		}

		private static string nextNumber(string str, int count)
		{
			int i;
			bool j = true;
			int intcount = count;

			StringBuilder bld = new StringBuilder();
			for (i = str.Length; i > 0; i--)
			{
				string c = str.Substring(i - 1, 1);

				if (System.Text.RegularExpressions.Regex.IsMatch(c, "[^0-9]"))
				{
					j = false;
				}

				if (j && System.Text.RegularExpressions.Regex.IsMatch(c, "[0-9]"))
				{
					int digit = Convert.ToInt16(c);

					string s_count = Convert.ToString(intcount);
					int digit2 = Convert.ToInt16(s_count.Substring(s_count.Length - 1, 1));

					bld.Append((digit + digit2) % 10);

					intcount -= digit2;
					intcount += ((digit + digit2) - (digit + digit2) % 10);

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
				throw new ArithmeticException("");
			}

			char[] chars = bld.ToString().ToCharArray();
			Array.Reverse(chars);
			return new string(chars);
		}

        /// <exclude/>
		public virtual void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert && e.TranStatus == PXTranStatus.Aborted)
			{
				object val = sender.GetValue(e.Row, _FieldOrdinal);
				string oldVal = val as string;
				if (oldVal != null || val == null)
				{
					if (oldVal == null)
					{
						oldVal = "";
					}
					try
					{
						sender.SetValue(e.Row, _FieldOrdinal, prepValue(oldVal, ""));
					}
					catch (InvalidCastException)
					{
					}
					if (e.Exception is PXLockViolationException
					    && !String.IsNullOrEmpty(oldVal)
					    && !String.IsNullOrEmpty(NewNumber)
					    && String.Equals(oldVal, prepValue(oldVal, NewNumber)))
					{
						try
						{
							PXDatabase.Update<NumberingSequence>(
								new PXDataFieldAssign("LastNbr", NewNumber),
								new PXDataFieldRestrict("NumberingID", Numbering),
								new PXDataFieldRestrict("NumberingSEQ", NumberingSEQ),
								new PXDataFieldRestrict("LastNbr", LastNbr));
							((PXLockViolationException)e.Exception).Retry = true;
						}
						catch
						{
						}
					}
				}
			}
		}
		public static void SuppressAutoNumbering<Field>(PXCache cache, bool suppress)
			where Field : IBqlField
		{
			cache.SetAltered<Field>(true);
			foreach (var attr in cache.GetAttributes<Field>().OfType<PXDimensionAttribute>())
			{
				attr.suppressAutoNumbering = suppress;
			}
		}
		#endregion

		#region Initialization
		public interface IDimensionSource
		{
			string[] Dimensions();
		}
		protected static Dictionary<string, int> _DimensionMaxLength = new Dictionary<string, int>();
		protected static Dictionary<string, TableColumn> _DimensionColumns = new Dictionary<string, TableColumn>();
        protected static Dictionary<string, Dictionary<string, KeyValuePair<Type, string>>> _DimensionTables = new Dictionary<string, Dictionary<string, KeyValuePair<Type, string>>>();
		protected internal override void SetBqlTable(Type bqlTable)
		{
			base.SetBqlTable(bqlTable);
			lock (((ICollection)_DimensionMaxLength).SyncRoot)
			{
				string columnKey = String.Concat(bqlTable.Name, "__", this.FieldName);
				TableColumn field = null;

				if (!_DimensionColumns.TryGetValue(columnKey, out field))
				{
					try
					{
						var structure = PXDatabase.Provider.GetTableStructure(_BqlTable.Name);
						if (structure != null) //DAC is not virtual
							field = _DimensionColumns[columnKey] = structure.Columns.FirstOrDefault(f => f.Name == this.FieldName);
					}
					catch
					{
					}
				}
      
				if (field != null && (field.Type == System.Data.SqlDbType.NVarChar || field.Type == System.Data.SqlDbType.VarChar)) // we can measure the length for string fields only
				{ 
					if (!_DimensionMaxLength.ContainsKey(_Dimension))
						_DimensionMaxLength.Add(_Dimension, field.Size);
					else if (_DimensionMaxLength[_Dimension] > field.Size) // we must choose minimal size of all fields than contain this attribute
						_DimensionMaxLength[_Dimension] = field.Size;

                    Dictionary<string, KeyValuePair<Type, string>> dictTables;
                    KeyValuePair<Type, string> pair = new KeyValuePair<Type, string>(_BqlTable, _FieldName);
                    if (!_DimensionTables.TryGetValue(_Dimension, out dictTables))
                    {
                        dictTables = new Dictionary<string, KeyValuePair<Type, string>>();
                        dictTables.Add(_BqlTable.Name, pair);
                        _DimensionTables.Add(_Dimension, dictTables);
                    }
                    else if (!dictTables.ContainsKey(_BqlTable.Name))
                    {
                        dictTables.Add(_BqlTable.Name, pair);
                    }
				}
			}
		}

	    private sealed class ParentSelectHelper : IBqlTable
	    {
		    #region IntValue
		    [PXDBInt]
		    public int? IntValue { get; set; }
		    public abstract class intValue : PX.Data.BQL.BqlInt.Field<intValue> { }
		    #endregion
	    }
		protected internal sealed class ValueDescr
		{
			public readonly string Descr;
			public readonly bool? IsConsolidatedValue;
			public readonly byte[] GroupMask;
			public ValueDescr(string descr, bool? isConsolidatedValue, byte[] groupMask)
			{
				Descr = descr;
				IsConsolidatedValue = isConsolidatedValue;
				GroupMask = groupMask;
			}
		}
		[Serializable]
		public sealed class SegmentValue : PX.Data.IBqlTable
		{
			public abstract class value : PX.Data.BQL.BqlString.Field<value> { }
			private String _Value;
			[PXDBString(30, IsKey = true, InputMask = "")]
			[PXUIField(DisplayName = "Value", Visibility = PXUIVisibility.SelectorVisible)]
			public String Value
			{
				get
				{
					return this._Value;
				}
				set
				{
					this._Value = value;
				}
			}
			public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
			private String _Descr;
			[PXDBString(50)]
			[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.Visible)]
			public String Descr
			{
				get
				{
					return this._Descr;
				}
				set
				{
					this._Descr = value;
				}
			}
			#region IsConsolidatedValue
			public abstract class isConsolidatedValue : PX.Data.BQL.BqlBool.Field<isConsolidatedValue> { }
			private Boolean? _IsConsolidatedValue;
			[PXDBBool]
			[PXDefault(false)]
			[PXUIField(DisplayName = "Aggregation")]
			public Boolean? IsConsolidatedValue
			{
				get
				{
					return this._IsConsolidatedValue;
				}
				set
				{
					this._IsConsolidatedValue = value;
				}
			}
			#endregion
			public SegmentValue(string value, string descr, bool? isConsolidatedValue)
			{
				_Value = value;
				_Descr = descr;
				_IsConsolidatedValue = isConsolidatedValue;
			}
			public SegmentValue()
			{
			}
		}
		protected class Definition : IPrefetchable
		{
			public Dictionary<string, PXSegment[]> Dimensions = new Dictionary<string, PXSegment[]>();
			public Dictionary<string, Dictionary<string, ValueDescr>[]> Values = new Dictionary<string, Dictionary<string, ValueDescr>[]>();
			public HashSet<string> ValidCombos = new HashSet<string>();
			public Dictionary<string, DimensionLookupMode> LookupModes = new Dictionary<string, DimensionLookupMode>();
			public Dictionary<string, string> Autonumbers = new Dictionary<string, string>();
			public void Prefetch()
			{
				try
				{
					var dimChilds = new Dictionary<string, string>();
					var dimParents = new List<string>();
					foreach (PXDataRecord record in PXDatabase.SelectMulti<Dimension>(
						new PXDataField("DimensionID"),
						new PXDataField("LookupMode"),
						new PXDataField("Validate"),
						new PXDataField("NumberingID"),
						new PXDataField("ParentDimensionID")
						))
					{
						string dimension = record.GetString(0);
						string lookupStr = record.GetString(1)?.ToUpperInvariant();

						DimensionLookupMode lookupMode;
						switch (lookupStr)
						{
							case "SA":
								lookupMode = DimensionLookupMode.BySegmentsAndAllAvailableSegmentValues; break;
							case "SC":
								lookupMode = DimensionLookupMode.BySegmentsAndChildSegmentValues; break;
							case "K0":
								lookupMode = DimensionLookupMode.BySegmentedKeys; break;
							default:
								throw new NotSupportedException($"LookupMode {lookupStr} is not supported.");
						}
						LookupModes[dimension] = lookupMode;

						if (record.GetBoolean(2) == true)
						{
							ValidCombos.Add(dimension);
						}
						string numbering = record.GetString(3);
						if (!String.IsNullOrEmpty(numbering))
						{
							Autonumbers.Add(dimension, numbering);
						}
						string parentDimensionId = record.GetString(4);
						if (!string.IsNullOrEmpty(parentDimensionId))
						{
							dimChilds[dimension] = parentDimensionId;
							if (!dimParents.Contains(parentDimensionId))
								dimParents.Add(parentDimensionId);
						}
					}
					foreach (KeyValuePair<string, string> pair in dimChilds)
					{
						var child = pair.Key;
						var parent = pair.Value;
						string parentAutonumbers;
						if (!Autonumbers.ContainsKey(child) && Autonumbers.TryGetValue(parent, out parentAutonumbers))
							Autonumbers.Add(child, parentAutonumbers);
					}
					var childSegments = new Dictionary<string, IList<short>>(dimChilds.Count);
					foreach (KeyValuePair<string, string> pair in dimChilds)
						childSegments.Add(pair.Key, new List<short>());
					var parentSegments = new Dictionary<string, IDictionary<short, SegDescr>>(dimParents.Count);
					foreach (string item in dimParents)
						parentSegments.Add(item, new Dictionary<short, SegDescr>());
					List<SegDescr> list = new List<SegDescr>();
					foreach (PXDataRecord record in PXDatabase.SelectMulti<Segment>(
						new PXDataField("DimensionID"),
						new PXDataField("SegmentID"),
						new PXDataField("EditMask"),
						new PXDataField("FillCharacter"),
						new PXDataField("Length"),
						new PXDataField("Validate"),
						new PXDataField("CaseConvert"),
						new PXDataField("Align"),
						new PXDataField("Separator"),
						new PXDataField("AutoNumber"),
						new PXDataField("Descr"),
						new PXDataField("ParentDimensionID"),
						new PXDataField("PromptCharacter")
						))
					{
						var segDescr = new SegDescr(
							record.GetString(0),
							record.GetInt16(1),
							(record.GetString(2) + " ")[0],
							' ',
							record.GetInt16(4),
							record.GetBoolean(5),
							record.GetInt16(6),
							record.GetInt16(7),
							(record.GetString(8) + " ")[0],
							record.GetBoolean(9),
							record.GetString(10),
							record.GetString(11),
							(record.GetString(12) + " ")[0]
							);
						list.Add(segDescr);
						if (dimChilds.ContainsKey(segDescr.DimensionID) && segDescr.ParentDimensionID != null)
							childSegments[segDescr.DimensionID].Add(segDescr.SegmentID);
						if (dimParents.Contains(segDescr.DimensionID))
							parentSegments[segDescr.DimensionID].Add(segDescr.SegmentID, segDescr);
					}
					foreach (KeyValuePair<string, string> item in dimChilds)
					{
						var childDimensionId = item.Key;
						var existSegments = childSegments[childDimensionId];
						var parentDimensionId = item.Value;
						foreach (KeyValuePair<short, SegDescr> pair in parentSegments[parentDimensionId])
						{
							var segmentId = pair.Key;
							if (!existSegments.Contains(segmentId))
							{
								var parentSegDescr = pair.Value;
								list.Add(new SegDescr(childDimensionId,
									segmentId,
									parentSegDescr.EditMask,
									parentSegDescr.FillCharacter,
									parentSegDescr.Length,
									parentSegDescr.Validate,
									parentSegDescr.CaseConvert,
									parentSegDescr.Align,
									parentSegDescr.Separator,
									parentSegDescr.AutoNumber,
									parentSegDescr.Descr,
									parentSegDescr.ParentDimensionID));
							}
						}
					}
					foreach (string auto in new List<String>(Autonumbers.Keys))
					{
						bool found = false;
						for (int i = 0; i < list.Count; i++)
						{
							if (String.Equals(auto, list[i].DimensionID, StringComparison.OrdinalIgnoreCase)
							    && list[i].AutoNumber)
							{
								found = true;
								break;
							}
						}
						if (!found)
						{
							Autonumbers.Remove(auto);
						}
					}
					list.Sort(delegate(SegDescr a, SegDescr b)
					{
						int cmp = String.Compare(a.DimensionID, b.DimensionID, StringComparison.OrdinalIgnoreCase);
						if (cmp != 0)
						{
							return cmp;
						}
						return a.SegmentID.CompareTo(b.SegmentID);
					}
						);
					int start = 0;
					for (int i = 0; i < list.Count; i++)
					{
						if (list[start].DimensionID != list[i].DimensionID)
						{
							PXSegment[] segs = new PXSegment[i - start];
							short max = -1;
							for (int j = 0; j < i - start; j++)
							{
								segs[j] = list[start + j];
								if (list[start + j].SegmentID > max)
								{
									max = list[start + j].SegmentID;
								}
							}
							Dimensions[list[start].DimensionID] = segs;
							max++;
							Dictionary<string, ValueDescr>[] dicts = new Dictionary<string, ValueDescr>[max];
							for (int j = 0; j < max; j++)
							{
								dicts[j] = new Dictionary<string, ValueDescr>();
							}
							Values[list[start].DimensionID] = dicts;
							start = i;
						}
					}
					if (start < list.Count)
					{
						PXSegment[] segs = new PXSegment[list.Count - start];
						short max = -1;
						for (int j = 0; j < list.Count - start; j++)
						{
							segs[j] = list[start + j];
							if (list[start + j].SegmentID > max)
							{
								max = list[start + j].SegmentID;
							}
						}
						Dimensions[list[start].DimensionID] = segs;
						max++;
						Dictionary<string, ValueDescr>[] dicts = new Dictionary<string, ValueDescr>[max];
						for (int j = 0; j < max; j++)
						{
							dicts[j] = new Dictionary<string, ValueDescr>();
						}
						Values[list[start].DimensionID] = dicts;
					}
					foreach (PXDataRecord record in PXDatabase.SelectMulti<SegmentValue>(
						new PXDataField("DimensionID"),
						new PXDataField("SegmentID"),
						new PXDataField("Value"),
						new PXDataField("Descr"),
						new PXDataField(GroupHelper.FieldName),
						new PXDataField("IsConsolidatedValue"),
						new PXDataFieldValue("Active", PXDbType.Bit, 1)
						))
					{
						byte[] mask = record.GetBytes(4);
						string dimension = record.GetString(0);
						PXSegment[] segs;
						if (Dimensions.TryGetValue(dimension, out segs))
						{
							int segment = (int)record.GetInt16(1);
							Dictionary<string, ValueDescr>[] arr = Values[dimension];
							if (arr != null && segment < arr.Length)
							{
								if (mask != null)
								{
									if (segment > segs.Length || !segs[segment - 1].Validate)
									{
										mask = null;
									}
									else
									{
										bool anyNonZero = false;
										for (int i = 0; i < mask.Length; i++)
										{
											if (mask[i] != (byte)0)
											{
												anyNonZero = true;
												break;
											}
										}
										if (!anyNonZero)
										{
											mask = null;
										}
									}
								}
								arr[segment][record.GetString(2)] = new ValueDescr(record.GetString(3), record.GetBoolean(5), mask);
							}
						}
					}
					foreach (KeyValuePair<string, string> item in dimChilds)
					{
						var childDimensionId = item.Key;
						var existSegments = childSegments[childDimensionId];
						var childValues = Values[childDimensionId];
						var parentDimensionId = item.Value;
						var parentValues = Values[parentDimensionId];
						foreach (KeyValuePair<short, SegDescr> pair in parentSegments[parentDimensionId])
						{
							var segmentId = pair.Key;
							if (!existSegments.Contains(segmentId))
							{
								var dic = childValues[segmentId];
								foreach (KeyValuePair<string, ValueDescr> parentValueDescr in parentValues[segmentId])
									if (!dic.ContainsKey(parentValueDescr.Key))
										dic.Add(parentValueDescr.Key, parentValueDescr.Value);
							}
						}
					}
				}
				catch
				{
					Dimensions.Clear();
					Values.Clear();
					ValidCombos.Clear();
					Autonumbers.Clear();
					throw;
				}
			}
		}
        [PXHidden]
		protected class Dimension : IBqlTable
		{
		}
        [PXHidden]
		protected class Segment : IBqlTable
		{
		}
        [PXHidden]
		protected class NumberingSequence : IBqlTable
		{
		}
		public static string[] GetSegmentValues(string dimensionid, int segmentnumber)
		{
			Definition defs = PXDatabaseGetSlot();
			Dictionary<string, ValueDescr>[] vals;
			if (String.IsNullOrEmpty(dimensionid) || segmentnumber < 0 || !defs.Values.TryGetValue(dimensionid, out vals) || vals.Length <= segmentnumber)
			{
				return new string[0];
			}
			string[] ret = new string[vals[segmentnumber].Keys.Count];
			vals[segmentnumber].Keys.CopyTo(ret, 0);
			return ret;
		}
		public static void Clear()
		{
			PXDatabase.ResetSlot<Definition>("Definition", typeof(Dimension), typeof(Segment), typeof(SegmentValue));
			PXContext.ClearSlot(typeof(Definition));
		}
		private static Definition PXDatabaseGetSlot()
		{
			try
			{
				return PXDatabase.GetSlot<Definition>("Definition", typeof(Dimension), typeof(Segment), typeof(SegmentValue));
			}
			catch
			{
				if (PXDatabase.Provider.SchemaCache.TableExists(nameof(Dimension)))
				{
					throw;
				}
				return null;
			}
		}
		/// <exclude/>
		public override void CacheAttached(PXCache sender)
		{
			_Definition = GetDefinition(_Dimension);

			if (_Definition != null)
			{
				PXSegment[] segs = _Definition.Dimensions[_Dimension];
				int total = 0;
				for (int i = 0; i < segs.Length; i++)
				{
					AddFieldForSegment(sender, i + 1, segs[i], total);
					total += segs[i].Length;
				}
				sender.RowSelecting += delegate (PXCache cache, PXRowSelectingEventArgs e)
				{
					SelfRowSelecting(cache, e, segs, total);
				};

				if (_Definition.LookupModes[_Dimension] == DimensionLookupMode.BySegmentsAndChildSegmentValues)
				{
					if (ParentSelect == null || ParentValueField == null)
						throw new PXInvalidOperationException();

					// Create BQL command & view for selecting unique values from parent records for each segment
					BqlCommand cmd = BqlCommand.CreateInstance(ParentSelect);
					cmd = cmd.WhereAnd(BqlCommand.MakeGenericType(typeof(Where<,>), ParentValueField, typeof(Like<>), typeof(Required<>), ParentValueField));
					Type aggrType = BqlCommand.MakeGenericType(
						typeof(Aggregate<>),
						typeof(GroupBy<,>),
						typeof(Substring<,,>), ParentValueField, typeof(Required<ParentSelectHelper.intValue>), typeof(Required<ParentSelectHelper.intValue>),
						typeof(Min<>), ParentValueField);
					cmd = cmd.AggregateNew(aggrType);

					_ParentView = sender.Graph.TypedViews.GetView(cmd, false);

					SetSegmentDelegate((PXSelectDelegate<short?, string>)SegmentSelectParents);
				}
			}

			sender.Graph.Views["_" + _Dimension + "_Segments_"] =
				new PXView(sender.Graph, true, new Select<SegmentValue>(), GetSegmentDelegate());
		}

		protected static Definition GetDefinition(string dimension)
		{
			Definition definition = PXContext.GetSlot<Definition>();
			if (definition == null)
			{
				PXContext.SetSlot<Definition>(definition = PXDatabaseGetSlot());
			}

			if (definition != null)
			{
				if (!definition.Dimensions.ContainsKey(dimension))
				{
					if (definition.Dimensions.Count == 0)
					{
						definition = new Definition();
						try
						{
							foreach (string dim in ServiceLocator.Current.GetInstance<IDimensionSource>().Dimensions())
							{
								definition.Dimensions[dim] = new PXSegment[] { new SegDescr(dim, 1, 'C', ' ', 30, false, 1, 0, '-', false, "1st segment", null) };
								definition.LookupModes[dim] = DimensionLookupMode.BySegmentedKeys;
							}
							//ValidComboRequired = true;
						}
						catch
						{
						}
					}
					if (!definition.Dimensions.ContainsKey(dimension))
					{
						throw new PXSetPropertyException(PXMessages.LocalizeFormat(ErrorMessages.ValueDoesntExist, "Segmented Key", dimension));
					}
				}
			}

			return definition;
		}

		private void AddFieldForSegment(PXCache cache, int segNumber, PXSegment seg, int startPos)
		{
			string segFieldName = $"{_FieldName}_Segment{segNumber}";
			if (cache.Fields.Contains(segFieldName)) return;
			cache.Fields.Add(segFieldName);

			cache.FieldSelectingEvents.Add(segFieldName, (sender, e) =>
			{
				string value = sender.GetValue(e.Row, _FieldName) as string;
				string result = String.Empty;
				if (!String.IsNullOrEmpty(value))
				{
					if (startPos < value.Length)
					{
						int len = (startPos + seg.Length) > value.Length ? (value.Length - startPos) : seg.Length;
						result = value.Substring(startPos, len);
						result = AdjustValueLength(result, seg.Length);
					}
				}

				var state = PXStringState.CreateInstance(result, seg.Length, true, segFieldName, false, 0, PXSegmentedState.GetEditMaskForSegment(seg), null, null, null, null);
				string displayName = PXUIFieldAttribute.GetDisplayName(sender, _FieldName);
				state.DisplayName = String.IsNullOrEmpty(displayName) ? seg.Descr : $"{displayName} ({seg.Descr})";
				state.Visible = false;
				state.Enabled = false;
				state.Visibility = PXUIVisibility.Invisible;
				e.ReturnState = state;
			});
			cache.FieldUpdatingEvents.Add(segFieldName, (sender, e) =>
			{
				if (e.Row != null)
					e.NewValue = sender.GetValue(e.Row, _FieldName) as string;
			});
			cache.CommandPreparingEvents.Add(segFieldName, (sender, e) =>
			{
				if (e.IsSelect() && (e.Operation & PXDBOperation.Option) == PXDBOperation.External)
				{
					PXCommandPreparingEventArgs.FieldDescription descr;
					sender.RaiseCommandPreparing(FieldName, null, null, PXDBOperation.Select | PXDBOperation.External, BqlTable, out descr);

					e.Expr = descr?.Expr?.Substr((uint)startPos + 1, (uint)seg.Length).NullIf(new SQLConst(String.Empty));

					e.DataValue = e.Value;
				}
			});
		}
		
	    protected internal Delegate GetSegmentDelegate()
		{
			if (_SegmentDelegate != null)
			{
				return _SegmentDelegate;
			}
			return (PXSelectDelegate<short?, string>) SegmentSelect;
		}
        /// <exclude/>
		public System.Collections.IEnumerable SegmentSelect(
			[PXShort]
				short? segment,
			[PXString]
				string value
			)
		{
			if (!_Definition.Dimensions.ContainsKey(_Dimension) || segment == null || segment < 0 || segment >= _Definition.Values[_Dimension].Length)
			{
				yield break;
			}
			foreach (KeyValuePair<string, ValueDescr> pair in _Definition.Values[_Dimension][(int)segment])
			{
				if (Restrictions == null || pair.Value.GroupMask == null)
				{
					yield return new SegmentValue(pair.Key, pair.Value.Descr, pair.Value.IsConsolidatedValue);
				}
				else
				{
					byte[] segmask = pair.Value.GroupMask;
					bool match = true;
					for (int k = 0; k < Restrictions.Length; k++)
					{
                        int failed = 0;
						for (int i = 0; i < Restrictions[k].Length; i++)
						{
							int verified = 0;
							for (int j = i * 4; j < i * 4 + 4; j++)
							{
								verified = verified << 8;
								if (j < segmask.Length)
								{
									verified |= (int)segmask[j];
								}
							}
							if ((verified & Restrictions[k][i].First) != 0)
							{
								match = false;
							}
                            if ((verified & Restrictions[k][i].Second) == 0)
                            {
                                failed++;
							}
						}
						if (!match)
						{
                            if (failed < Restrictions[k].Length)
                            {
                                match = true;
                            }
                            else
                            {
							break;
						}
					}
					}
					if (match)
					{
						yield return new SegmentValue(pair.Key, pair.Value.Descr, pair.Value.IsConsolidatedValue);
					}
				}
			}
		}

		public virtual IEnumerable SegmentSelectParents(
			[PXShort] short? segment,
			[PXString] string value)
		{
			if (!_Definition.Dimensions.ContainsKey(_Dimension) || _ParentView == null)
				yield break;

			PXSegment[] segments = _Definition.Dimensions[_Dimension];
			if (segment == null || segment < 0 || segments.Length == 0 || segment > segments.Length)
			{
				yield break;
			}

			int segmentIndex = segment.Value - 1;

			if (String.IsNullOrEmpty(value) && segmentIndex > 0) // if user queries later segments without filling the first one
				yield break;

			PXCache cache = _ParentView.Cache;
			string search = GetSearchStringForParentSelect(cache.Graph.SqlDialect, segments, segmentIndex, value);

			int pos = 0;
			int total = 0;
			for (int i = 0; i < segments.Length; i++)
			{
				if (i < segmentIndex)
					pos += segments[i].Length;
				total += segments[i].Length;
			}

			int segLength = segments[segmentIndex].Length;

			object[] pars = new object[_ParentView.EnumParameters().Count];
			pars[pars.Length - 3] = search; // Field LIKE search
			pars[pars.Length - 2] = pos + 1; // GROUP BY SUBSTRING(Field, pos + 1, segLength) *because position in SQL string starts with 1*
			pars[pars.Length - 1] = segLength;

			string fieldName = ParentValueField.Name;
			PXFieldState state = cache.GetStateExt(null, fieldName) as PXFieldState;
			string descrFieldName = state?.DescriptionName;

			int startRow = PXView.StartRow;
			int totalRows = 0;
			int maximumRows = PXView.MaximumRows;
			if (PXView.Filters.Length > 0 || PXView.SortColumns.Length > 0)
			{
				startRow = 0;
				maximumRows = 0;
			}
			else
			{
				PXView.StartRow = 0; // reset start row - it is a sign that we've applied paging here
			}

			foreach (object row in _ParentView.Select(null, pars, null, null, null, null, ref startRow, maximumRows, ref totalRows))
			{
				object fvalue = PXFieldState.UnwrapValue(cache.GetValueExt(row, fieldName));
				if (fvalue != null)
				{
					string sfvalue = fvalue.ToString();
					sfvalue = AdjustValueLength(sfvalue, total);

					string descr = String.Empty;
					if (!String.IsNullOrEmpty(descrFieldName))
					{
						//object descrValue = PXFieldState.UnwrapValue(cache.GetValueExt(row, descrFieldName));
						object descrValue = PXSelectorAttribute.GetField(cache, null, fieldName, sfvalue, descrFieldName);
						descr = descrValue as string;
					}

					sfvalue = sfvalue.Substring(pos, segLength);
					if (String.IsNullOrWhiteSpace(sfvalue))
						sfvalue = new string(' ', sfvalue.Length);

					yield return new PXDimensionAttribute.SegmentValue(sfvalue, descr, false);
				}
			}
		}

		protected string GetSearchStringForParentSelect(ISqlDialect sqlDialect, PXSegment[] segments, int segIndex, string value)
		{
			if (segIndex < 0 || segIndex >= segments.Length)
				throw new ArgumentOutOfRangeException(nameof(segIndex));

			if (String.IsNullOrEmpty(value))
				return sqlDialect.WildcardAnything;

			int iStart = 0;
			StringBuilder sbld = new StringBuilder();
			for (int si = 0; si < segments.Length; si++)
			{
				PXSegment seg = segments[si];
				int iEnd = iStart + seg.Length;

				if (si >= segIndex)
				{
					sbld.Append(sqlDialect.WildcardAnything);
					break;
				}

				if (iEnd > value.Length)
				{
					if (value.Length < iStart)
					{
						sbld.Append(sqlDialect.WildcardAnything);
						break;
					}
					else
					{
						sbld.Append(value.Substring(iStart, value.Length - iStart)).Append(' ', seg.Length - (value.Length - iStart));
					}
				}
				else
				{
					sbld.Append(value.Substring(iStart, seg.Length));
				}

				iStart = iEnd;
			}

			return sbld.ToString();
		}
		#endregion

		#region Runtime
		protected string customNumbering;
		protected int? customNumberingSegment;

		public static void SetCustomNumbering<Field>(PXCache cache, string numberingSeq, int? segmentID)
			where Field : IBqlField
		{
			SetCustomNumbering(cache, typeof(Field).Name, numberingSeq, segmentID);
		}
		public static void SetCustomNumbering(PXCache cache, string name, string numberingSeq, int? segmentID)
		{
			cache.SetAltered(name, true);
			foreach (PXDimensionAttribute attr in cache.GetAttributes(name).OfType<PXDimensionAttribute>())
			{
				attr.customNumbering = numberingSeq;
				attr.customNumberingSegment = segmentID;
			}
		}
		#endregion
	}

	#endregion

	#region PXDimensionSelectorAttribute
	/// <summary>Sets up the lookup control for a DAC field that holds a
	/// segmented key value or references a data record identified by a
	/// segmented key. The attribute combines the <tt>PXDimension</tt> and
	/// <tt>PXSelector</tt> attributes.</summary>
	/// <example>
	/// 	<code title="Example" description="The attribute below sets up the control for input of the BIZACCT segmented key's values. Since the AcctCD field itself is specified as the substitute key it will keep the segmented key value." lang="CS">
	/// [PXDimensionSelector(
	///     "BIZACCT",
	///     typeof(BAccount.acctCD),   // BQL query for lookup
	///     typeof(BAccount.acctCD))]  // Substitute key
	/// public virtual string AcctCD { get; set; }</code>
	/// 	<code title="Example2" description="In the following example the RunRateItemID field references the data records from" groupname="Example" lang="CS">
	/// [PXDimensionSelector(
	///     InventoryAttribute.DimensionName,
	///     typeof(
	///         Search&lt;InventoryItem.inventoryID,
	///             Where&lt;InventoryItem.itemType, Equal&lt;INItemTypes.nonStockItem&gt;,
	///                 And&lt;Match&lt;Current&lt;AccessInfo.userName&gt;&gt;&gt;&gt;&gt;),
	///     typeof(InventoryItem.inventoryCD),
	///     DescriptionField = typeof(InventoryItem.descr))]
	/// public virtual int? RunRateItemID { get; set; }</code>
	/// </example>
	[PXAttributeFamily(typeof(PXSelectorAttribute))]
	public class PXDimensionSelectorAttribute : PXAggregateAttribute, IPXFieldVerifyingSubscriber, IPXRowPersistingSubscriber, IPXRowPersistedSubscriber, IPXDependsOnFields 
	{
		#region State
		protected object _KeyToAbort;
		protected Dictionary<object, object> _Persisted;
		protected Dictionary<object, object> _SubstitutePersisted;
		protected Type _keyType;
		protected object _JustPersistedKey;
		protected bool _SuppressViewCreation;
		protected Type _SubstituteKey;
		protected bool _DirtyRead;

		private int _SelectorAttributeId = -1;
		private int _DimensionAttributeId = -1;

		protected PXSelectorAttribute SelectorAttribute => (PXSelectorAttribute)_Attributes[_SelectorAttributeId];
		protected PXDimensionAttribute DimensionAttribute => (PXDimensionAttribute)_Attributes[_DimensionAttributeId];

		public bool SuppressViewCreation => _SuppressViewCreation;

		/// <exclude/>
		public virtual void SetSegmentDelegate(Delegate handler) => DimensionAttribute.SetSegmentDelegate(handler);

		/// <exclude/>
		public IEnumerable SegmentSelect([PXShort] short? segment, [PXString] string value) => DimensionAttribute.SegmentSelect(segment, value);

		/// <summary>Gets or sets the value that indicates whether only the values
		/// from the combobox are allowed in segments.</summary>
		public virtual bool ValidComboRequired
		{
			get { return DimensionAttribute.ValidComboRequired; }
			set { DimensionAttribute.ValidComboRequired = value; }
		}

		/// <summary>Gets or sets the field from the referenced table that
		/// contains the description.</summary>
		public virtual Type DescriptionField
		{
			get { return SelectorAttribute.DescriptionField; }
			set { SelectorAttribute.DescriptionField = value; }
		}

		/// <summary>
		/// Gets or sets the type that is used as a key for saved filters.
		/// </summary>
		public virtual Type FilterEntity
		{
			get { return SelectorAttribute.FilterEntity; }
			set { SelectorAttribute.FilterEntity = value; }
		}

		/// <summary>Gets or sets the value that indicates whether the attribute
		/// should cache the data records retrieved from the database to show in
		/// the lookup control. By default, the attribute does not cache the data
		/// records.</summary>
		public virtual bool CacheGlobal
		{
			get { return SelectorAttribute.CacheGlobal; }
			set { SelectorAttribute.CacheGlobal = value; }
		}

		/// <summary>Gets or sets the value that indicates whether the filters
		/// defined by the user should be stored in the database.</summary>
		public virtual bool Filterable
		{
			get { return SelectorAttribute.Filterable; }
			set { SelectorAttribute.Filterable = value; }
		}

		/// <summary>Gets or sets a value that indicates whether the attribute
		/// should take into account the unsaved modifications when displaying
		/// data records in control. If <tt>false</tt>, the data records are taken
		/// from the database and not merged with the cache object. If
		/// <tt>true</tt>, the data records are merged with the modification
		/// stored in the cache object.</summary>
		public virtual bool DirtyRead
		{
			get { return SelectorAttribute.DirtyRead; }
			set { SelectorAttribute.DirtyRead = _DirtyRead = value; }
		}

		/// <summary>Gets or sets the list of labels for column headers that are
		/// displayed in the lookup control. By default, the attribute uses
		/// display names of the fields.</summary>
		public virtual string[] Headers
		{
			get { return SelectorAttribute.Headers; }
			set { SelectorAttribute.Headers = value; }
		}

		/// <summary>
		/// Gets or sets the value that determines the value displayed by
		/// the selector control in the UI and some aspects of
		/// attribute's behavior. You can assign a combination of
		/// <see cref="PXSelectorMode">PXSelectorMode</see> values joined
		/// by bitwise or ("|").
		/// </summary>
		public virtual PXSelectorMode SelectorMode
		{
			get { return SelectorAttribute.SelectorMode; }
			set { SelectorAttribute.SelectorMode = value; }
		}

		/// <summary>
		/// Allows to control validation process.
		/// </summary>
		public virtual bool ValidateValue
		{
			get { return SelectorAttribute.ValidateValue; }
			set { SelectorAttribute.ValidateValue = value; }
		}

		/// <summary>Gets the field that identifies a referenced data record
		/// (surrogate key) and is assigned to the field annotated with the
		/// <tt>PXSelector</tt> attribute. Typically, it is the first parameter of
		/// the BQL query passed to the attribute constructor.</summary>
		public virtual Type Field => SelectorAttribute.Field;

		/// <summary>
		/// Returns Bql command used for selection of referenced records.
		/// </summary>
		public BqlCommand GetSelect() => SelectorAttribute.GetSelect();

		public virtual string CustomMessageElementDoesntExist
		{
			get { return SelectorAttribute.CustomMessageElementDoesntExist; }
			set { SelectorAttribute.CustomMessageElementDoesntExist = value; }
		}

		public virtual string CustomMessageValueDoesntExist
		{
			get { return SelectorAttribute.CustomMessageValueDoesntExist; }
			set { SelectorAttribute.CustomMessageValueDoesntExist = value; }
		}

		public virtual string CustomMessageElementDoesntExistOrNoRights
		{
			get { return SelectorAttribute.CustomMessageElementDoesntExistOrNoRights; }
			set { SelectorAttribute.CustomMessageElementDoesntExistOrNoRights = value; }
		}

		public virtual string CustomMessageValueDoesntExistOrNoRights
		{
			get { return SelectorAttribute.CustomMessageValueDoesntExistOrNoRights; }
			set { SelectorAttribute.CustomMessageValueDoesntExistOrNoRights = value; }
		}
		#endregion

		#region Ctor
		protected PXDimensionSelectorAttribute(string dimension)
		{
			_Attributes.Add(new PXDimensionAttribute(dimension));
			_DimensionAttributeId = _Attributes.Count - 1;
		}

		/// <summary>Initializes a new instance to reference the data records that
		/// are identified by the specified segmented key. Uses the provided BQL
		/// query to retrieve the data records.</summary>
		/// <param name="dimension">The string identifier of the segmented
		/// key.</param>
		/// <param name="type">A BQL query that defines the data set that is shown
		/// to the user along with the key field that is used as a value. Set to a
		/// field (type part of a DAC field) to select all data records from the
		/// referenced table. Set to a BQL command of <tt>Search</tt> type to
		/// specify a complex select statement.</param>
		public PXDimensionSelectorAttribute(string dimension, Type type)
			: this(dimension)
		{
			RegisterSelector(new PXSelectorAttribute(type));
		}
        /// <summary>Initializes a new instance to reference the data records that
        /// are identified by the specified segmented key. Uses the provided BQL
        /// query to retrieve the data records and substitutes the field value
        /// (surrogate key) with the provided field (natural key).</summary>
        /// <param name="dimension">The string identifier of the segmented
        /// key.</param>
        /// <param name="type">A BQL query that defines the data set that is shown
        /// to the user along with the key field that is used as a value. Set to a
        /// field (type part of a DAC field) to select all data records from the
        /// referenced table. Set to a BQL command of <tt>Search</tt> type to
        /// specify a complex select statement.</param>
        /// <param name="substituteKey">The field to sustitute the surrogate
        /// field's value in the user interface.</param>
        /// <example>
        /// <code>
        /// [PXDimensionSelector(
        ///     InventoryAttribute.DimensionName,
        ///     typeof(
        ///         Search&lt;InventoryItem.inventoryID,
        ///             Where&lt;InventoryItem.itemType, Equal&lt;INItemTypes.nonStockItem&gt;,
        ///                 And&lt;Match&lt;Current&lt;AccessInfo.userName&gt;&gt;&gt;&gt;&gt;),
        ///     typeof(InventoryItem.inventoryCD),
        ///     DescriptionField = typeof(InventoryItem.descr))]
        /// public virtual int? RunRateItemID { get; set; }
        /// </code>
        /// </example>
		public PXDimensionSelectorAttribute(string dimension, Type type, Type substituteKey)
			: this(dimension)
		{
			RegisterSelector(new PXSelectorAttribute(type) {SubstituteKey = substituteKey});
		}
        /// <summary>Initializes a new instance to reference the data records that
        /// are identified by the specified segmented key. Uses the provided BQL
        /// query to retrieve the data records and substitutes the field value
        /// (surrogate key) with the provided field (natural key).</summary>
        /// <param name="dimension">The string identifier of the segmented
        /// key.</param>
        /// <param name="type">A BQL query that defines the data set that is shown
        /// to the user along with the key field that is used as a value. Set to a
        /// field (type part of a DAC field) to select all data records from the
        /// referenced table. Set to a BQL command of <tt>Search</tt> type to
        /// specify a complex select statement.</param>
        /// <param name="substituteKey">The field to sustitute the surrogate
        /// field's value in the user interface.</param>
        /// <param name="fieldList">Fields to display in the control.</param>
		public PXDimensionSelectorAttribute(string dimension, Type type, Type substituteKey, params Type[] fieldList)
	        : this(dimension)
		{
			RegisterSelector(new PXSelectorAttribute(type, fieldList) { SubstituteKey = substituteKey});
		}

		public PXDimensionSelectorAttribute(string dimension, Type search, Type lookupJoin, Type substituteKey, bool cacheGlobal, params Type[] fieldList)
			: this(dimension)
		{
			RegisterSelector(new PXSelectorAttribute(search, lookupJoin, cacheGlobal, fieldList) {SubstituteKey = substituteKey});
		}
		
		protected void RegisterSelector(PXSelectorAttribute selector)
		{
			_SubstituteKey = selector.SubstituteKey;
			_Attributes.Add(selector);
			_SelectorAttributeId = _Attributes.Count - 1;
		}
		#endregion

		#region Implementation
		/// <exclude/>
		public virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (SelectorAttribute._BypassFieldVerifying.Value) {
				return;
			}
			PXFieldUpdating fu = DimensionAttribute.FieldUpdating;
			fu(sender, e);
			e.Cancel = false;
			if (_SubstituteKey == null || _BqlTable.IsAssignableFrom(BqlCommand.GetItemType(_SubstituteKey)) && String.Compare(_SubstituteKey.Name, _FieldName, StringComparison.OrdinalIgnoreCase) == 0) {
				e.Cancel = true;
				return;
			}
			try {
				fu = SelectorAttribute.SubstituteKeyFieldUpdating;
				fu(sender, e);
			}
			catch (Exception exc) {
				if (!(exc is PXForeignRecordDeletedException) && !(exc is PXSetPropertyException)) {
					throw;
				}
				else {
					if (DimensionAttribute.ValidComboRequired) {
						throw;
					}
					Type field = SelectorAttribute.Field;
					PXCache cache = sender.Graph.Caches[BqlCommand.GetItemType(field)];
					var d = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
							{
								[_SubstituteKey.Name] = e.NewValue,
								[field.Name] = null
							};
					bool isDirty = cache.IsDirty;
					bool InsertRights = cache.InsertRights;
					bool AllowInsert = cache.AllowInsert;
					cache.InsertRights = true;
					cache.AllowInsert = true;
					try {
						if (exc is PXForeignRecordDeletedException && cache.Locate(d) > 0) {
							cache.Remove(cache.Current);
						}
						if (cache.Insert(d) > 0) {
							cache.IsDirty = isDirty;
							PXFieldState st = d[_SubstituteKey.Name] as PXFieldState;
							if (st != null && !String.IsNullOrEmpty(st.Error)) {
								object key = d[field.Name] as PXFieldState;
								if (key == null) {
									key = d[field.Name];
								}
								foreach (object data in cache.Inserted) {
									if (object.Equals(key, cache.GetValue(data, field.Name))) {
										cache.Delete(data);
										break;
									}
								}
								throw new PXSetPropertyException(st.Error);
							}
							st = d[field.Name] as PXFieldState;
							if (st != null) {
								e.NewValue = st.Value;
							}
							else if (d[field.Name] != null) {
								e.NewValue = d[field.Name];
							}
							else {
								throw;
							}
						}
						else {
							throw;
						}
					}
					catch {
						cache.IsDirty = isDirty;
						cache.AllowInsert = AllowInsert;
						cache.InsertRights = InsertRights;
						throw;
					}
				}
			}
		}
		protected sealed class segmentView : PXView
		{
			private PXDimensionAttribute _Attribute;
			public segmentView(PXGraph graph, bool isReadonly, BqlCommand select, PXDimensionAttribute attribute)
				: base(graph, isReadonly, select, attribute.GetSegmentDelegate())
			{
				_Attribute = attribute;
			}
			protected override List<object> InvokeDelegate(object[] parameters)
			{
				List<GroupHelper.ParamsPair[]> list = null;
				IBqlParameter[] cmdpars = BqlSelect.GetParameters();
				for (int i = 0; i < cmdpars.Length; i++) {
					if (cmdpars[i].MaskedType != null && !cmdpars[i].IsArgument) {
						if (parameters[i] == null) {
							if (!cmdpars[i].NullAllowed) {
								return new List<object>();
							}
							parameters[i] = new byte[(GroupHelper.Count + 7) / 8];
							for (int j = 0; j < ((byte[])parameters[i]).Length; j++) {
								((byte[])parameters[i])[j] = 0xFF;
							}
						}
						if (list == null) {
							list = new List<GroupHelper.ParamsPair[]>();
						}
						Type cross = null;
						Type field = cmdpars[i].GetReferencedType();
						if (!field.IsNested || BqlCommand.GetItemType(field) == _CacheType) {
							cross = GroupHelper.GetReferencedType(Cache, field.Name);
						}
						else {
							PXCache cache = _Graph.Caches[BqlCommand.GetItemType(field)];
							cross = GroupHelper.GetReferencedType(cache, field.Name);
						}
						list.Add(GroupHelper.GetParams(cross, GroupHelper.FindRestricted(cross, "SegmentValue"), parameters[i] as byte[]));
					}
				}
				if (list != null) {
					_Attribute.Restrictions = list.ToArray();
				}
				return base.InvokeDelegate(parameters);
			}
		}
        /// <exclude/>
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			FieldSelecting(sender, e, _SuppressViewCreation, ValidComboRequired);
		}
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e, bool bypassViewCreation, bool validComboRequired)
		{
			if (!e.IsAltered) {
				e.IsAltered = sender.HasAttributes(e.Row);
			}
			PXFieldSelecting fs;
			if (_SubstituteKey != null && (!_BqlTable.IsAssignableFrom(BqlCommand.GetItemType(_SubstituteKey)) || String.Compare(_SubstituteKey.Name, _FieldName, StringComparison.OrdinalIgnoreCase) != 0)) {
				fs = SelectorAttribute.SubstituteKeyFieldSelecting;
				fs(sender, e);
			}
			fs = DimensionAttribute.FieldSelecting;
			if (e.ReturnState is PXFieldState) {
				((PXFieldState)e.ReturnState).ViewName = null;
			}
			fs(sender, e);
			fs = SelectorAttribute.FieldSelecting;
			PXView viewDimension = null;
			if (validComboRequired)
			{
				fs(sender, e);
				if (SelectorAttribute.DescriptionField == null && e.ReturnState is PXFieldState) {
					((PXFieldState)e.ReturnState).DescriptionName = null;
				}
			}
			else if (e.Row == null || sender.HasAttributes(e.Row) ||  e.IsAltered && !sender.SecuredFields.Contains(_FieldName.ToLower())) {
				PXFieldSelectingEventArgs a = new PXFieldSelectingEventArgs(null, null, true, true);
				fs(sender, a);
				var origState = (PXSegmentedState) e.ReturnState;
				var selState = (PXFieldState) a.ReturnState;
				origState.ViewName = selState.ViewName;
				origState.FieldList = selState.FieldList;
				origState.HeaderList = selState.HeaderList;
				origState.ValueField = selState.ValueField;
				origState.DescriptionName = selState.DescriptionName;
				
				PXView viewSelector = sender.Graph.Views[origState.ViewName];
				BqlCommand selectDimension = null;
				foreach (IBqlParameter par in viewSelector.BqlSelect.GetParameters()) {
					if (par.MaskedType != null) {
						if (selectDimension == null) {
							viewDimension = sender.Graph.Views[origState.SegmentViewName];
							selectDimension = viewDimension.BqlSelect;
						}
						if (par.NullAllowed) {
							selectDimension = selectDimension.WhereAnd(BqlCommand.Compose(
								typeof(Where<,,>),
								par.HasDefault ? (par.IsVisible ? typeof(Optional<>) : typeof(Current<>)) : (typeof(Required<>)),
								par.GetReferencedType(),
								typeof(IsNull),
								typeof(Or<>),
								typeof(Match<>),
								par.HasDefault ? (par.IsVisible ? typeof(Optional<>) : typeof(Current<>)) : (typeof(Required<>)),
								par.GetReferencedType()));
						}
						else {
							selectDimension = selectDimension.WhereAnd(BqlCommand.Compose(
								typeof(Where<>),
								typeof(Match<>),
								par.HasDefault ? (par.IsVisible ? typeof(Optional<>) : typeof(Current<>)) : (typeof(Required<>)),
								par.GetReferencedType()));
						}
					}
				}
				if (selectDimension != null) {
					if (!bypassViewCreation) {
						var vn = string.Format("__{0}{1}{2}", sender.GetItemType().Name, _FieldName, origState.SegmentViewName);
						sender.Graph.Views[vn] = new segmentView(sender.Graph, true, selectDimension, DimensionAttribute);
						PXView fv;
						if (sender.Graph.Views.TryGetValue(origState.ViewName + PXFilterableAttribute.FilterHeaderName, out fv)) {
							sender.Graph.Views[vn + PXFilterableAttribute.FilterHeaderName] = fv;
							if (sender.Graph.Views.TryGetValue(origState.ViewName + PXFilterableAttribute.FilterRowName, out fv)) {
								sender.Graph.Views[vn + PXFilterableAttribute.FilterRowName] = fv;
							}
						}
						((PXSegmentedState) e.ReturnState).SegmentViewName = vn;
					}
					else {
						List<GroupHelper.ParamsPair[]> list = null;
						IBqlParameter[] cmdpars = selectDimension.GetParameters();
						Type cacheType = selectDimension.GetTables()[0];
						for (int i = 0; i < cmdpars.Length; i++) {
							if (cmdpars[i].MaskedType != null) {
								object val = null;
								Type field = cmdpars[i].GetReferencedType();
								if (cmdpars[i].HasDefault) {
									if (field.IsNested) {
										Type ct = BqlCommand.GetItemType(field);
										PXCache cache = ct == cacheType ? viewDimension.Cache : sender.Graph.Caches[ct];
										object row;
										if (e.Row != null && (e.Row.GetType() == ct || e.Row.GetType().IsSubclassOf(ct))) {
											row = e.Row;
										}
										else {
											row = cache.Current;
										}
										if (row != null) {
											val = cache.GetValue(row, field.Name);
										}
										if (val == null && cmdpars[i].TryDefault) {
											if (cache.RaiseFieldDefaulting(field.Name, null, out val)) {
												cache.RaiseFieldUpdating(field.Name, null, ref val);
											}
										}
										val = GroupHelper.GetReferencedValue(cache, row, field.Name, val, false);
									}
								}
								if (val == null) {
									val = new byte[(GroupHelper.Count + 7) / 8];
									for (int j = 0; j < ((byte[])val).Length; j++) {
										((byte[])val)[j] = 0xFF;
									}
								}
								if (list == null) {
									list = new List<GroupHelper.ParamsPair[]>();
								}
								Type cross = null;
								if (!field.IsNested || BqlCommand.GetItemType(field) == cacheType) {
									cross = GroupHelper.GetReferencedType(viewDimension.Cache, field.Name);
								}
								else {
									PXCache cache = sender.Graph.Caches[BqlCommand.GetItemType(field)];
									cross = GroupHelper.GetReferencedType(cache, field.Name);
								}
								list.Add(GroupHelper.GetParams(cross, GroupHelper.FindRestricted(cross, "SegmentValue"), val as byte[]));
							}
						}
						if (list != null) {
							DimensionAttribute.Restrictions = list.ToArray();
						}
					}
				}
			}
		}
        /// <exclude/>
		public virtual void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			List<IPXFieldVerifyingSubscriber> fv = new List<IPXFieldVerifyingSubscriber>();
			DimensionAttribute.GetSubscriber<IPXFieldVerifyingSubscriber>(fv);
			if (fv.Count > 0) {
				PXFieldSelectingEventArgs fsa = new PXFieldSelectingEventArgs(e.Row, e.NewValue, !ValidComboRequired, true);
				FieldSelecting(sender, fsa, true, ValidComboRequired);
				try {
					bool needsync = object.Equals(fsa.ReturnValue, e.NewValue);
					PXFieldVerifyingEventArgs ver = new PXFieldVerifyingEventArgs(e.Row, fsa.ReturnValue, e.ExternalCall);
					for (int l = 0; l < fv.Count; l++) {
						fv[l].FieldVerifying(sender, ver);
					}
					if (needsync) {
						e.NewValue = ver.NewValue;
					}
					PXSetPropertyException exception = null;
					object oldvalue = null;
					try
					{
						SelectorAttribute.FieldVerifying(sender, e);
					}
					catch (PXSetPropertyException pspe)
					{
						object newkey = null;
						bool found = false;
						if (_SubstitutePersisted != null && e.NewValue != null)
						{
							found = _SubstitutePersisted.TryGetValue(e.NewValue, out newkey);

							if (!found && _keyType == typeof(int) && int.TryParse(e.NewValue as string,
								NumberStyles.Integer, CultureInfo.InvariantCulture, out int newValue))
							{
								e.NewValue = newValue;
								found = _SubstitutePersisted.TryGetValue(e.NewValue, out newkey);
							}
						}
						if (found)
						{
							exception = pspe;
							oldvalue = e.NewValue;
							e.NewValue = newkey;
						}
						else
						{
							throw;
						}
					}
					if (exception != null)
					{
						try
						{
							SelectorAttribute.FieldVerifying(sender, e);
						}
						catch
						{
							throw exception;
						}
						finally
						{
							e.NewValue = oldvalue;
						}
					}
				}
				catch (PXSetPropertyException ex) {
					ex.ErrorValue = fsa.ReturnValue;
					throw ex;
				}
			}
			else {
				SelectorAttribute.FieldVerifying(sender, e);
			}
		}
        /// <exclude/>
		public virtual void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (_SubstituteKey == null || _BqlTable.IsAssignableFrom(BqlCommand.GetItemType(_SubstituteKey)) && String.Compare(_SubstituteKey.Name, _FieldName, StringComparison.OrdinalIgnoreCase) == 0) {
				return;
			}
			object key = sender.GetValue(e.Row, _FieldOrdinal);
			if (key != null && Convert.ToInt32(key) < 0) {
				Type field = SelectorAttribute.Field;
				PXCache cache = sender.Graph.Caches[BqlCommand.GetItemType(field)];
				object newkey;
				_KeyToAbort = key;
				if (_Persisted.TryGetValue(key, out newkey)) {
					sender.SetValue(e.Row, _FieldOrdinal, newkey);
				}
				else {
					bool found = false;
					foreach (object data in cache.Inserted) {
						if (object.Equals(key, cache.GetValue(data, field.Name))) {
							try {
								cache.PersistInserted(data);
							}
							catch {
							}
							newkey = cache.GetValue(data, field.Name);
							if (newkey != null && Convert.ToInt32(newkey) > 0) {
								sender.SetValue(e.Row, _FieldOrdinal, newkey);
								if (!object.Equals(_KeyToAbort, newkey)) {
									_Persisted[_KeyToAbort] = newkey;
								}
								found = true;
							}
							break;
						}
					}
					if (!found) {
						if (_SubstitutePersisted.TryGetValue(key, out newkey)) {
							sender.SetValue(e.Row, _FieldOrdinal, newkey);
						}
						else {
							sender.SetValue(e.Row, _FieldOrdinal, null);
						}
					}
				}
			}
		}
        /// <exclude/>
		public virtual void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (_KeyToAbort != null && e.TranStatus != PXTranStatus.Open) {
				if (e.TranStatus == PXTranStatus.Aborted) {
					object newkey;
					if (_Persisted.TryGetValue(_KeyToAbort, out newkey)) {
						Type field = SelectorAttribute.Field;
						PXCache cache = sender.Graph.Caches[BqlCommand.GetItemType(field)];
						foreach (object data in cache.Inserted) {
							if (object.Equals(newkey, cache.GetValue(data, field.Name))) {
								try {
									cache.RaiseRowPersisted(data, PXDBOperation.Insert, PXTranStatus.Aborted, null);
								}
								catch {
								}
								break;
							}
						}
					}
					sender.SetValue(e.Row, _FieldOrdinal, _KeyToAbort);
				}
				_Persisted.Remove(_KeyToAbort);
				_KeyToAbort = null;
			}
		}
        /// <exclude/>
		public virtual void SubstituteRowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert) {
				Type field = SelectorAttribute.Field;
				_JustPersistedKey = sender.GetValue(e.Row, field.Name);
			}
		}
        /// <exclude/>
		public virtual void SubstituteRowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert && e.TranStatus == PXTranStatus.Open) {
				Type field = SelectorAttribute.Field;
				object value = sender.GetValue(e.Row, field.Name);
				_SubstitutePersisted[_JustPersistedKey] = value;
				if (_keyType == null)
					_keyType = value.GetType();
			}
		}
		public ISet<Type> GetDependencies(PXCache cache)
		{
			return ((IPXDependsOnFields)SelectorAttribute).GetDependencies(cache);
		}
		#endregion

		#region Initialization
		protected void resetValidCombos(PXCache sender, bool oldRequired, bool newRequired)
		{
			if (_SubstituteKey == null) {
				return;
			}
			if (oldRequired && !newRequired) {
				Type foreignType;
				sender.Graph.RowPersisting.AddHandler(foreignType=BqlCommand.GetItemType(_SubstituteKey), SubstituteRowPersisting);
				sender.Graph.RowPersisted.AddHandler(foreignType, SubstituteRowPersisted);
				if (!sender.Graph.Views.Caches.Contains(foreignType) && !sender.Graph.Views.RestorableCaches.Contains(foreignType))
				{
					sender.Graph.Views.RestorableCaches.Add(foreignType);
				}
			}
			else if (!oldRequired && newRequired) {
				sender.Graph.RowPersisting.RemoveHandler(BqlCommand.GetItemType(_SubstituteKey), SubstituteRowPersisting);
				sender.Graph.RowPersisted.RemoveHandler(BqlCommand.GetItemType(_SubstituteKey), SubstituteRowPersisted);
			}
		}

        /// <exclude/>
		public override void CacheAttached(PXCache sender)
		{
	        DimensionAttribute.ParentSelect = SelectorAttribute.OriginalSelect.GetType();
		    DimensionAttribute.ParentValueField = _SubstituteKey ?? SelectorAttribute.Field;
	        
	        base.CacheAttached(sender);

			string name = _FieldName.ToLower();
			sender.FieldUpdatingEvents[name] -= SelectorAttribute.SubstituteKeyFieldUpdating;
			sender.FieldUpdatingEvents[name] += FieldUpdating;
			sender.FieldSelectingEvents[name] -= SelectorAttribute.SubstituteKeyFieldSelecting;
			sender.FieldSelectingEvents[name] += FieldSelecting;

	        if (!_DirtyRead) {
				SelectorAttribute.DirtyRead = !DimensionAttribute.ValidComboRequired;
			}
			if (SelectorAttribute.DirtyRead) {
				resetValidCombos(sender, true, false);
			}
			_Persisted = new Dictionary<object, object>();
	        _SubstitutePersisted = new Dictionary<object, object>();
		}

        /// <exclude/>
		public override void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers)
		{
			base.GetSubscriber<ISubscriber>(subscribers);
			if (typeof(ISubscriber) == typeof(IPXFieldVerifyingSubscriber)) {
				if (_SubstituteKey != null) {
					subscribers.Remove(DimensionAttribute as ISubscriber);
					subscribers.Remove(SelectorAttribute as ISubscriber);
				}
				else {
					subscribers.Remove(this as ISubscriber);
				}
			}
			else if (typeof(ISubscriber) == typeof(IPXFieldSelectingSubscriber)) {
				subscribers.Remove(DimensionAttribute as ISubscriber);
				subscribers.Remove(SelectorAttribute as ISubscriber);
			}
			else if (typeof(ISubscriber) == typeof(IPXFieldUpdatingSubscriber)) {
				subscribers.Remove(DimensionAttribute as ISubscriber);
			}
			else if (_SubstituteKey == null || String.Compare(_SubstituteKey.Name, _FieldName, StringComparison.OrdinalIgnoreCase) != 0) {
				if (typeof(ISubscriber) == typeof(IPXFieldDefaultingSubscriber)) {
					subscribers.Remove(DimensionAttribute as ISubscriber);
				}
				else if (typeof(ISubscriber) == typeof(IPXRowPersistingSubscriber)) {
					subscribers.Remove(DimensionAttribute as ISubscriber);
				}
				else if (typeof(ISubscriber) == typeof(IPXRowPersistedSubscriber)) {
					subscribers.Remove(DimensionAttribute as ISubscriber);
				}
			}
		}
		#endregion

		#region Runtime

		public static void SetValidCombo<Field>(PXCache cache, bool isRequired)
			where Field : IBqlField
		{
			cache.SetAltered<Field>(true);
			foreach (var attr in cache.GetAttributes<Field>().OfType<PXDimensionSelectorAttribute>())
			{
				bool oldRequired = attr.DimensionAttribute.ValidComboRequired;
				if (oldRequired != isRequired)
				{
					attr.resetValidCombos(cache, oldRequired, isRequired);
					attr.DimensionAttribute.ValidComboRequired = isRequired;
					attr.SelectorAttribute.DirtyRead = !isRequired;
				}
			}
		}

		public static void SetValidCombo(PXCache cache, string name, bool isRequired)
		{
			cache.SetAltered(name, true);
			foreach (var attr in cache.GetAttributes(name).OfType<PXDimensionSelectorAttribute>())
			{
				bool oldRequired = attr.DimensionAttribute.ValidComboRequired;
				if (oldRequired != isRequired)
				{
					attr.resetValidCombos(cache, oldRequired, isRequired);
					attr.DimensionAttribute.ValidComboRequired = isRequired;
					attr.SelectorAttribute.DirtyRead = !isRequired;
				}
			}
		}

		public static void SetSuppressViewCreation(PXCache cache)
		{
			foreach (var attr in cache.GetAttributes(null).OfType<PXDimensionSelectorAttribute>())
				attr._SuppressViewCreation = true;
		}
	    #endregion
	}

	public static class PXDimensionSelector
	{
		public class WithCachingByCompositeKeyAttribute : PXDimensionSelectorAttribute
		{
			public bool OnlyKeyConditions
			{
				get { return ((PXSelectorAttribute.WithCachingByCompositeKeyAttribute)SelectorAttribute).OnlyKeyConditions; }
				set { ((PXSelectorAttribute.WithCachingByCompositeKeyAttribute)SelectorAttribute).OnlyKeyConditions = value; }
			}

			protected WithCachingByCompositeKeyAttribute(string dimension): base(dimension)
			{ }

			public WithCachingByCompositeKeyAttribute(String dimension, Type search, Type additionalKeysRelation)
				: base(dimension)
			{
				RegisterSelector(new PXSelectorAttribute.WithCachingByCompositeKeyAttribute(search, additionalKeysRelation));
			}

			public WithCachingByCompositeKeyAttribute(String dimension, Type search, Type additionalKeysRelation, Type substituteKey)
				: base(dimension)
			{
				RegisterSelector(new PXSelectorAttribute.WithCachingByCompositeKeyAttribute(search, additionalKeysRelation) { SubstituteKey = substituteKey });
			}

			public WithCachingByCompositeKeyAttribute(String dimension, Type search, Type additionalKeysRelation, Type substituteKey, Type[] fieldList)
				: base(dimension)
			{
				RegisterSelector(new PXSelectorAttribute.WithCachingByCompositeKeyAttribute(search, additionalKeysRelation, fieldList) { SubstituteKey = substituteKey });
			}

			public WithCachingByCompositeKeyAttribute(String dimension, Type search, Type additionalKeysRelation, Type lookupJoin, Type substituteKey, Type[] fieldList)
				: base(dimension)
			{
				RegisterSelector(new PXSelectorAttribute.WithCachingByCompositeKeyAttribute(search, additionalKeysRelation, lookupJoin, fieldList) { SubstituteKey = substituteKey });
			}
		}
	}
	#endregion

    #region PXDimensionWildcardAttribute
    /// <summary>Sets up the lookup control for a DAC field that holds a
    /// segmented key value and allows the <i>?</i> wildcard character. The
    /// attribute combines the <tt>PXDimension</tt> and <tt>PXSelector</tt>
    /// attributes.</summary>
    public class PXDimensionWildcardAttribute : PXAggregateAttribute, IPXFieldSelectingSubscriber
	{
		#region State
        /// <summary>Gets or sets the field from the referenced table that
        /// contains the description.</summary>
		public virtual Type DescriptionField
		{
			get
			{
				return ((PXSelectorAttribute)_Attributes[_Attributes.Count - 1]).DescriptionField;
			}
			set
			{
				((PXSelectorAttribute)_Attributes[_Attributes.Count - 1]).DescriptionField = value;
			}
		}
        /// <summary>Gets or sets the wildcard string that matches any symbol in
        /// the segment.</summary>
		public virtual string Wildcard
		{
			get
			{
				return ((PXDimensionAttribute)_Attributes[_Attributes.Count - 2]).Wildcard;
			}
			set
			{
				((PXDimensionAttribute)_Attributes[_Attributes.Count - 2]).Wildcard = value;
			}
		}
        /// <summary>Gets or sets the list of labels for column headers that are
        /// displayed in the lookup control. By default, the attribute uses
        /// display names of the fields.</summary>
		public virtual string[] Headers
		{
			get
			{
				return ((PXSelectorAttribute)_Attributes[_Attributes.Count - 1]).Headers;
			}
			set
			{
				((PXSelectorAttribute)_Attributes[_Attributes.Count - 1]).Headers = value;
			}
		}
		#endregion

		#region Ctor
        /// <summary>Creates a selector.</summary>
        /// <param name="type">Referenced table. Should be either IBqlField or
        /// IBqlSearch.</param>
		public PXDimensionWildcardAttribute(string dimension, Type type)
			: base()
		{
			PXDimensionAttribute da = new PXDimensionAttribute(dimension);
			da.Wildcard = "?";
			_Attributes.Add(da);
			PXSelectorAttribute attr = new PXSelectorAttribute(type);
			_Attributes.Add(attr);
		}
        /// <summary>Creates a selector overriding the columns.</summary>
        /// <param name="type">Referenced table. Should be either IBqlField or
        /// IBqlSearch.</param>
        /// <param name="fieldList">Fields to display in the selector.</param>
        /// <param name="headerList">Headers of the selector columns.</param>
		public PXDimensionWildcardAttribute(string dimension, Type type, params Type[] fieldList)
			: base()
		{
			PXDimensionAttribute da = new PXDimensionAttribute(dimension);
			da.Wildcard = "?";
			_Attributes.Add(da);
			PXSelectorAttribute attr = new PXSelectorAttribute(type, fieldList);
			_Attributes.Add(attr);
		}
		#endregion

		#region Implementation
		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (!e.IsAltered) {
				e.IsAltered = sender.HasAttributes(e.Row);
			}
			PXFieldSelecting fs = ((PXDimensionAttribute)_Attributes[_Attributes.Count - 2]).FieldSelecting;
			fs(sender, e);
			if (((PXDimensionAttribute)_Attributes[_Attributes.Count - 2]).ValidComboRequired) {
				fs = ((PXSelectorAttribute)_Attributes[_Attributes.Count - 1]).FieldSelecting;
				fs(sender, e);
			}
		}
		#endregion

		#region Initialization
		public override void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers)
		{
			base.GetSubscriber<ISubscriber>(subscribers);
			if (typeof(ISubscriber) == typeof(IPXFieldVerifyingSubscriber)) {
				subscribers.Remove(_Attributes[_Attributes.Count - 1] as ISubscriber);
			}
			else if (typeof(ISubscriber) == typeof(IPXFieldSelectingSubscriber)) {
				subscribers.Remove(_Attributes[_Attributes.Count - 2] as ISubscriber);
				subscribers.Remove(_Attributes[_Attributes.Count - 1] as ISubscriber);
			}
			else if (typeof(ISubscriber) == typeof(IPXFieldDefaultingSubscriber)) {
				subscribers.Remove(_Attributes[_Attributes.Count - 2] as ISubscriber);
			}
			else if (typeof(ISubscriber) == typeof(IPXRowPersistingSubscriber)) {
				subscribers.Remove(_Attributes[_Attributes.Count - 2] as ISubscriber);
			}
			else if (typeof(ISubscriber) == typeof(IPXRowPersistedSubscriber)) {
				subscribers.Remove(_Attributes[_Attributes.Count - 2] as ISubscriber);
			}
		}
		#endregion
	}
	#endregion

	#region PXCustomDimensionSelectorAttribute
    /// <summary>The base class for custom dimension selector attributes.
    /// Derive the attribute class from this class and implement the
    /// <tt>GetRecords()</tt> method.</summary>
	public class PXCustomDimensionSelectorAttribute : PXDimensionSelectorAttribute
	{
		protected class selectorAttribute : PXCustomSelectorAttribute
		{
			private readonly Func<IEnumerable> _delegate;

			public selectorAttribute(Type searchType, Func<IEnumerable> @delegate)
				: base(searchType)
			{
				if (@delegate == null) throw new ArgumentNullException("delegate");
				_delegate = @delegate;
			}

			public selectorAttribute(Type searchType, Type[] fieldList, Func<IEnumerable> @delegate)
				: base(searchType, fieldList)
			{
				if (@delegate == null) throw new ArgumentNullException("delegate");
				_delegate = @delegate;
			}

			public IEnumerable GetRecords()
			{
				return _delegate();
			}
		}
		#region Ctor
        /// <summary>Initializes a new instance of the attribute.</summary>
		public PXCustomDimensionSelectorAttribute(string dimension, Type type)
			: base(dimension, type)
		{
			PXSelectorAttribute sel = new selectorAttribute(type, GetRecords);
			_Attributes.RemoveAt(_Attributes.Count - 1);
			_Attributes.Add(sel);
		}

        /// <summary>Initializes a new instance of the attribute.</summary>
		public PXCustomDimensionSelectorAttribute(string dimension, Type type, Type substituteKey)
			: base(dimension, type, substituteKey)
		{
			PXSelectorAttribute sel = new selectorAttribute(type, GetRecords);
			sel.SubstituteKey = substituteKey;
			_Attributes.RemoveAt(_Attributes.Count - 1);
			_Attributes.Add(sel);
		}

        /// <summary>Initializes a new instance of the attribute.</summary>
		public PXCustomDimensionSelectorAttribute(string dimension, Type type, Type substituteKey, params Type[] fieldList)
			: base(dimension, type, substituteKey, fieldList)
		{
			PXSelectorAttribute sel = new selectorAttribute(type, fieldList, GetRecords);
			sel.SubstituteKey = substituteKey;
			_Attributes.RemoveAt(_Attributes.Count - 1);
			_Attributes.Add(sel);
		}

		public virtual IEnumerable GetRecords()
		{
			yield break;
		}
		#endregion
	}
	#endregion
}