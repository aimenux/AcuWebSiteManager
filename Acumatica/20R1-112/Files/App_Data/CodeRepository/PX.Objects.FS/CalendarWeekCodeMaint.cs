using PX.Data;
using System;
using System.Collections;

namespace PX.Objects.FS
{
    public class CalendarWeekCodeMaint : PXGraph<CalendarWeekCodeMaint, FSWeekCodeDate>
    {
        [PXImport(typeof(FSWeekCodeDate))]
        public PXSelectOrderBy<FSWeekCodeDate, OrderBy<Asc<FSWeekCodeDate.weekCodeDate>>> CalendarWeekCodeRecords;

        #region WeekCode Constants
        public class WeekCodeConstant
        {
            public const string A = "A";
            public const string B = "B";
            public const string C = "C";
            public const string D = "D";
            public const string E = "E";
            public const string F = "F";
            public const string S = "S";
            public const string T = "T";
            public const string U = "U";
            public const string V = "V";
            public const string W = "W";
            public const string X = "X";
            public const string Y = "Y";
            public const string Z = "Z";
        }
        #endregion

        #region Unbound DAC + Filter
        [System.SerializableAttribute]
        public class CalendarWeekCodeGeneration : PX.Data.IBqlTable
        {
            #region DefaultStartDate
            public abstract class defaultStartDate : PX.Data.BQL.BqlDateTime.Field<defaultStartDate> { }

            [PXDate]
            [PXUIField(DisplayName = "Default Start Date", Enabled = false)]
            public virtual DateTime? DefaultStartDate { get; set; }
            #endregion

            #region startDate
            public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }

            [PXDate]
            [PXUIField(DisplayName = "From Date", Required = true)]
            public virtual DateTime? StartDate { get; set; }
            #endregion

            #region EndDate
            public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }

            [PXDate]
            [PXUIField(DisplayName = "To Date", Required = true)]
            public virtual DateTime? EndDate { get; set; }
            #endregion

            #region InitialWeekCode
            public abstract class initialWeekCode : PX.Data.BQL.BqlString.Field<initialWeekCode> { }

            [PXString(4, IsUnicode = true, InputMask = ">CCCC")]
            [PXUIField(DisplayName = "Initial Week Code", Enabled = false)]
            [PXDefault("1ACS")]
            public virtual string InitialWeekCode { get; set; }
            #endregion
        }

        public PXFilter<CalendarWeekCodeGeneration> CalendarWeekCodeGenerationOptions;
        #endregion

        #region Virtual Functions
        /// <summary>
        /// Sets the values of the FSWeekCodeDate.BeginDateOfWeek and FSWeekCodeDate.EndDateOfWeek memory fields.
        /// </summary>
        /// <param name="fsWeekCodeDateRow">FSWeekCodeDate Row.</param>
        public virtual void SetBeginEndWeekDates(FSWeekCodeDate fsWeekCodeDateRow)
        {
            if (fsWeekCodeDateRow.WeekCodeDate != null && fsWeekCodeDateRow.WeekCodeDate.Value != null)
            {
                fsWeekCodeDateRow.BeginDateOfWeek = SharedFunctions.StartOfWeek((DateTime)fsWeekCodeDateRow.WeekCodeDate, DayOfWeek.Monday);
                fsWeekCodeDateRow.EndDateOfWeek = SharedFunctions.EndOfWeek((DateTime)fsWeekCodeDateRow.WeekCodeDate, DayOfWeek.Monday);
            }
        }

        /// <summary>
        /// Split the fsWeekCodeDateRow.WeekCode field into the WeekCode parameters.
        /// </summary>
        /// <param name="fsWeekCodeDateRow">FSWeekCodeDate Row.</param>
        public virtual void SplitWeekCodeParameters(FSWeekCodeDate fsWeekCodeDateRow)
        {
            if (fsWeekCodeDateRow.WeekCode != null && fsWeekCodeDateRow.WeekCode.Length == 4)
            {
                fsWeekCodeDateRow.WeekCodeP1 = fsWeekCodeDateRow.WeekCode.Substring(0, 1);
                fsWeekCodeDateRow.WeekCodeP2 = fsWeekCodeDateRow.WeekCode.Substring(1, 1);
                fsWeekCodeDateRow.WeekCodeP3 = fsWeekCodeDateRow.WeekCode.Substring(2, 1);
                fsWeekCodeDateRow.WeekCodeP4 = fsWeekCodeDateRow.WeekCode.Substring(3, 1);
            }
            else
            {
                fsWeekCodeDateRow.WeekCodeP1 = null;
                fsWeekCodeDateRow.WeekCodeP2 = null;
                fsWeekCodeDateRow.WeekCodeP3 = null;
                fsWeekCodeDateRow.WeekCodeP4 = null;
            }
        }

        /// <summary>
        /// Calculates the Week Code of a specific date.
        /// </summary>
        /// <param name="baseDate">Date by which the Week Code will be calculated.</param>
        /// <param name="fsWeekCodeRow">FSWeekCodeDate Row.</param>
        public virtual void AutoCalcWeekCode(DateTime baseDate, FSWeekCodeDate fsWeekCodeRow)
        {
            int weeks = (int)(fsWeekCodeRow.WeekCodeDate.Value.Subtract(baseDate).TotalDays / 7) + 1;
            string p1 = CalcWeekCodeParameterP1(weeks);
            string p2 = CalcWeekCodeParameterP2(weeks);
            string p3 = CalcWeekCodeParameterP3(weeks);
            string p4 = CalcWeekCodeParameterP4(weeks);

            fsWeekCodeRow.DayOfWeek = (int)fsWeekCodeRow.WeekCodeDate.Value.DayOfWeek;
            fsWeekCodeRow.WeekCode = p1 + p2 + p3 + p4;
        }

        /// <summary>
        /// Calculates the Fourth parameter of the Week Code.
        /// </summary>
        /// <param name="weeks">Number of the week in which the date belongs.</param>
        /// <returns>Fourth parameter of the Week Code.</returns>
        public virtual string CalcWeekCodeParameterP4(int weeks)
        {
            int x = weeks % 32;

            if (x > 0 && x <= 4)
            {
                return WeekCodeConstant.S;
            }
            else if (x > 4 && x <= 8)
            {
                return WeekCodeConstant.T;
            }
            else if (x > 8 && x <= 12)
            {
                return WeekCodeConstant.U;
            }
            else if (x > 12 && x <= 16)
            {
                return WeekCodeConstant.V;
            }
            else if (x > 16 && x <= 20)
            {
                return WeekCodeConstant.W;
            }
            else if (x > 20 && x <= 24)
            {
                return WeekCodeConstant.X;
            }
            else if (x > 24 && x <= 28)
            {
                return WeekCodeConstant.Y;
            }
            else
            {
                return WeekCodeConstant.Z;
            }
        }

        /// <summary>
        /// Calculates the 3rd parameter of the Week Code.
        /// </summary>
        /// <param name="weeks">Number of the week in which the date belongs.</param>
        /// <returns>3rd Parameter of the Week Code.</returns>
        public virtual string CalcWeekCodeParameterP3(int weeks)
        {
            int x = weeks % 16;

            if (x > 0 && x <= 4)
            {
                return WeekCodeConstant.C;
            }
            else if (x > 4 && x <= 8)
            {
                return WeekCodeConstant.D;
            }
            else if (x > 8 && x <= 12)
            {
                return WeekCodeConstant.E;
            }
            else
            {
                return WeekCodeConstant.F;
            }
        }

        /// <summary>
        /// Calculates the second parameter of the Week Code.
        /// </summary>
        /// <param name="weeks">Number of the week in which the date belongs.</param>
        /// <returns>Second parameter of the Week Code.</returns>
        public virtual string CalcWeekCodeParameterP2(int weeks)
        {
            int x = weeks % 8;

            if (x > 0 && x <= 4)
            {
                return WeekCodeConstant.A;
            }
            else
            {
                return WeekCodeConstant.B;
            }
        }

        /// <summary>
        /// Calculates the 1st parameter of the Week Code.
        /// </summary>
        /// <param name="weeks">Number of the week in which the date belongs.</param>
        /// <returns>1st Parameter of the Week Code.</returns>
        public virtual string CalcWeekCodeParameterP1(int weeks)
        {
            int x = weeks % 4;

            if (x == 0)
            {
                return "4";
            }
            else
            {
                return x.ToString();
            }
        }

        /// <summary>
        /// Validates the CalendarWeekCodeGeneration.endDate value.
        /// </summary>
        /// <param name="calendarWeekCodeGenrationRow">CalendarWeekCodeGeneration Row.</param>
        /// <param name="cache">Cache of the View.</param>
        /// <returns>true: valid value | false: invalid value.</returns>
        public virtual bool ValidateEndGenerationDate(CalendarWeekCodeGeneration calendarWeekCodeGenrationRow, PXCache cache)
        {
            bool isValid = true;

            if (calendarWeekCodeGenrationRow.EndDate == null)
            {
                cache.RaiseExceptionHandling<CalendarWeekCodeGeneration.endDate>(calendarWeekCodeGenrationRow,
                                                                                 calendarWeekCodeGenrationRow.EndDate,
                                                                                 new PXException(PXMessages.LocalizeFormatNoPrefix(TX.Error.FIELD_MAY_NOT_BE_EMPTY,
                                                                                                                                   PXUIFieldAttribute.GetDisplayName<CalendarWeekCodeGeneration.endDate>(cache))));
                isValid = false;
            }
            else
            {
                if (calendarWeekCodeGenrationRow.EndDate <= calendarWeekCodeGenrationRow.StartDate)
                {
                    cache.RaiseExceptionHandling<CalendarWeekCodeGeneration.endDate>(calendarWeekCodeGenrationRow,
                                                                                     calendarWeekCodeGenrationRow.EndDate,
                                                                                     new PXException(TX.Error.END_DATE_LESSER_THAN_START_DATE));
                    isValid = false;
                }
            }

            return isValid;
        }

        /// <summary>
        /// Validates the CalendarWeekCodeGeneration.startDate value.
        /// </summary>
        /// <param name="calendarWeekCodeGenrationRow">CalendarWeekCodeGeneration Row.</param>
        /// <param name="cache">Cache of the View.</param>
        /// <returns>true: valid value | false: invalid value.</returns>
        public virtual bool ValidateStartGenerationDate(CalendarWeekCodeGeneration calendarWeekCodeGenrationRow, PXCache cache)
        {
            bool isValid = true;

            if (calendarWeekCodeGenrationRow.StartDate == null)
            {
                cache.RaiseExceptionHandling<CalendarWeekCodeGeneration.startDate>(calendarWeekCodeGenrationRow,
                                                                                   calendarWeekCodeGenrationRow.StartDate,
                                                                                   new PXException(PXMessages.LocalizeFormatNoPrefix(TX.Error.FIELD_MAY_NOT_BE_EMPTY,
                                                                                                                                     PXUIFieldAttribute.GetDisplayName<CalendarWeekCodeGeneration.endDate>(cache))));

                isValid = false;
            }
            else
            {
                if (calendarWeekCodeGenrationRow.StartDate < calendarWeekCodeGenrationRow.DefaultStartDate)
                {
                    cache.RaiseExceptionHandling<CalendarWeekCodeGeneration.startDate>(calendarWeekCodeGenrationRow,
                                                                                       calendarWeekCodeGenrationRow.StartDate,
                                                                                       new PXException(TX.Error.START_DATE_LESSER_THAN_DEFAULT_DATE));
                    isValid = false;
                }
            }

            return isValid;
        }
        #endregion

        #region Actions
        #region GenerateWeekCode
        public PXAction<FSWeekCodeDate> generateWeekCode;
        [PXUIField(DisplayName = "Generate Week Codes", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable GenerateWeekCode(PXAdapter adapter)
        {
            if (CalendarWeekCodeGenerationOptions.AskExt() == WebDialogResult.OK)
            {
                if (ValidateEndGenerationDate(CalendarWeekCodeGenerationOptions.Current, CalendarWeekCodeGenerationOptions.Cache)
                        && ValidateStartGenerationDate(CalendarWeekCodeGenerationOptions.Current, CalendarWeekCodeGenerationOptions.Cache))
                {
                    string message = PXMessages.LocalizeFormatNoPrefix(TX.Messages.ASK_CONFIRM_CALENDAR_WEEKCODE_GENERATION,
                                                                       CalendarWeekCodeGenerationOptions.Current.StartDate.Value.ToShortDateString(),
                                                                       CalendarWeekCodeGenerationOptions.Current.EndDate.Value.ToShortDateString());

                    if (WebDialogResult.OK == CalendarWeekCodeRecords.Ask(TX.WebDialogTitles.CONFIRM_CALENDAR_WEEKCODE_GENERATION,
                                                                          message,
                                                                          MessageButtons.OKCancel))
                    {
                        DateTime baseDate = CalendarWeekCodeGenerationOptions.Current.DefaultStartDate.Value;
                        DateTime iteratorDate = CalendarWeekCodeGenerationOptions.Current.StartDate.Value;
                        DateTime stopDate = CalendarWeekCodeGenerationOptions.Current.EndDate.Value;

                        CalendarWeekCodeMaint graphCalendarWeekCodeMaint = PXGraph.CreateInstance<CalendarWeekCodeMaint>();

                        PXLongOperation.StartOperation(
                                    this,
                                    delegate
                                    {
                                        while (iteratorDate <= stopDate)
                                        {
                                            FSWeekCodeDate fsWeekCodeRow = new FSWeekCodeDate();
                                            fsWeekCodeRow.WeekCodeDate = iteratorDate;

                                            graphCalendarWeekCodeMaint.AutoCalcWeekCode(baseDate, fsWeekCodeRow);

                                            graphCalendarWeekCodeMaint.CalendarWeekCodeRecords.Insert(fsWeekCodeRow);
                                            iteratorDate = iteratorDate.AddDays(1);
                                        }

                                        graphCalendarWeekCodeMaint.Save.Press();
                                    });
                    }
                }
                else
                {
                    throw new PXException(TX.Error.INVALID_WEEKCODE_GENERATION_OPTIONS);
                }
            }

            return adapter.Get();
        }
        #endregion
        #endregion

        #region Event Handlers

        #region FSWeekCodeDate

        #region FieldSelecting
        #endregion
        #region FieldDefaulting
        #endregion
        #region FieldUpdating
        #endregion
        #region FieldVerifying
        #endregion
        #region FieldUpdated

        protected virtual void _(Events.FieldUpdated<FSWeekCodeDate, FSWeekCodeDate.weekCodeDate> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSWeekCodeDate fsWeekCodeDateRow = (FSWeekCodeDate)e.Row;

            SetBeginEndWeekDates(fsWeekCodeDateRow);
        }

        protected virtual void _(Events.FieldUpdated<FSWeekCodeDate, FSWeekCodeDate.weekCode> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSWeekCodeDate fsWeekCodeDateRow = (FSWeekCodeDate)e.Row;

            SplitWeekCodeParameters(fsWeekCodeDateRow);
        }

        #endregion

        protected virtual void _(Events.RowSelecting<FSWeekCodeDate> e)
        {
        }

        protected virtual void _(Events.RowSelected<FSWeekCodeDate> e)
        {
        }

        protected virtual void _(Events.RowInserting<FSWeekCodeDate> e)
        {
        }

        protected virtual void _(Events.RowInserted<FSWeekCodeDate> e)
        {
        }

        protected virtual void _(Events.RowUpdating<FSWeekCodeDate> e)
        {
        }

        protected virtual void _(Events.RowUpdated<FSWeekCodeDate> e)
        {
        }

        protected virtual void _(Events.RowDeleting<FSWeekCodeDate> e)
        {
        }

        protected virtual void _(Events.RowDeleted<FSWeekCodeDate> e)
        {
        }

        protected virtual void _(Events.RowPersisting<FSWeekCodeDate> e)
        {
        }

        protected virtual void _(Events.RowPersisted<FSWeekCodeDate> e)
        {
        }

        #endregion

        #region CalendarWeekCodeGeneration

        #region FieldSelecting
        #endregion
        #region FieldDefaulting

        protected virtual void _(Events.FieldDefaulting<CalendarWeekCodeGeneration, CalendarWeekCodeGeneration.defaultStartDate> e)
        {
            if (e.Row == null)
            {
                return;
            }

            e.NewValue = new DateTime(2008, 12, 29);
        }

        protected virtual void _(Events.FieldDefaulting<CalendarWeekCodeGeneration, CalendarWeekCodeGeneration.startDate> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSWeekCodeDate fsWeekCodeDateRow = PXSelectOrderBy<FSWeekCodeDate, OrderBy<Desc<FSWeekCodeDate.weekCodeDate>>>.SelectWindowed(this, 0, 1);

            if (fsWeekCodeDateRow != null)
            {
                e.NewValue = fsWeekCodeDateRow.WeekCodeDate.Value.AddDays(1);
            }
            else
            {
                e.NewValue = new DateTime(2008, 12, 29);
            }

            CalendarWeekCodeGeneration calendarWeekCodeGenrationRow = (CalendarWeekCodeGeneration)e.Row;

            if (calendarWeekCodeGenrationRow.StartDate != null)
            {
                DateTime? newValue = SharedFunctions.TryParseHandlingDateTime(e.Cache, e.NewValue);
                if (newValue.HasValue == true)
                {
                    calendarWeekCodeGenrationRow.EndDate = newValue.Value.AddYears(1);
                }
            }
        }

        protected virtual void _(Events.FieldDefaulting<CalendarWeekCodeGeneration, CalendarWeekCodeGeneration.endDate> e)
        {
            if (e.Row == null)
            {
                return;
            }

            CalendarWeekCodeGeneration calendarWeekCodeGenrationRow = (CalendarWeekCodeGeneration)e.Row;

            if (calendarWeekCodeGenrationRow.StartDate != null)
            {
                e.NewValue = calendarWeekCodeGenrationRow.StartDate.Value.AddYears(1);
            }
        }

        #endregion
        #region FieldUpdating
        #endregion
        #region FieldVerifying
        #endregion
        #region FieldUpdated

        protected virtual void _(Events.FieldUpdated<CalendarWeekCodeGeneration, CalendarWeekCodeGeneration.startDate> e)
        {
            if (e.Row == null)
            {
                return;
            }

            CalendarWeekCodeGeneration calendarWeekCodeGenrationRow = (CalendarWeekCodeGeneration)e.Row;

            ValidateStartGenerationDate(calendarWeekCodeGenrationRow, e.Cache);

            if (calendarWeekCodeGenrationRow.StartDate != null)
            {
                calendarWeekCodeGenrationRow.EndDate = calendarWeekCodeGenrationRow.StartDate.Value.AddYears(1);
            }
        }

        protected virtual void _(Events.FieldUpdated<CalendarWeekCodeGeneration, CalendarWeekCodeGeneration.endDate> e)
        {
            if (e.Row == null)
            {
                return;
            }

            CalendarWeekCodeGeneration calendarWeekCodeGenrationRow = (CalendarWeekCodeGeneration)e.Row;

            ValidateEndGenerationDate(calendarWeekCodeGenrationRow, e.Cache);
        }

        #endregion

        protected virtual void _(Events.RowSelecting<CalendarWeekCodeGeneration> e)
        {
        }

        protected virtual void _(Events.RowSelected<CalendarWeekCodeGeneration> e)
        {
        }

        protected virtual void _(Events.RowInserting<CalendarWeekCodeGeneration> e)
        {
        }

        protected virtual void _(Events.RowInserted<CalendarWeekCodeGeneration> e)
        {
        }

        protected virtual void _(Events.RowUpdating<CalendarWeekCodeGeneration> e)
        {
        }

        protected virtual void _(Events.RowUpdated<CalendarWeekCodeGeneration> e)
        {
        }

        protected virtual void _(Events.RowDeleting<CalendarWeekCodeGeneration> e)
        {
        }

        protected virtual void _(Events.RowDeleted<CalendarWeekCodeGeneration> e)
        {
        }

        protected virtual void _(Events.RowPersisting<CalendarWeekCodeGeneration> e)
        {
        }

        protected virtual void _(Events.RowPersisted<CalendarWeekCodeGeneration> e)
        {
        }

        #endregion

        #endregion
    }
}