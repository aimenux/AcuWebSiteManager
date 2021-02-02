using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Data.EP;
using PX.TM;

namespace PX.Objects.EP
{
	public class TimecardPrimary : PXGraph<TimecardPrimary>
	{
		public PXFilter<TimecardFilter> Filter;

		[PXFilterable()]
		public PXSelectJoin<TimecardWithTotals, 
			LeftJoin<EPTimeCard, On<TimecardWithTotals.timeCardCD, Equal<EPTimeCard.origTimeCardCD>>>, 
			Where2<Where2<Where2<Where<
				TimecardWithTotals.userID, Equal<Current<AccessInfo.userID>>, 
				Or<TimecardWithTotals.createdByID, Equal<Current<AccessInfo.userID>>,
				Or<TimecardWithTotals.userID, OwnedUser<Current<AccessInfo.userID>>,
				Or<TimecardWithTotals.employeeID, WingmanUser<Current<AccessInfo.userID>>>>>>, And<Current2<TimecardFilter.employeeID>, IsNull>>,
				Or<Where<Current2<TimecardFilter.employeeID>, IsNotNull, And<TimecardWithTotals.employeeID, Equal<Current2<TimecardFilter.employeeID>>>>>>, 
			And<EPTimeCard.timeCardCD, IsNull>>, 
			OrderBy<Desc<TimecardWithTotals.timeCardCD>>> Items;

		
		#region CRUD Actions

		public PXAction<TimecardFilter> create;
		[PXButton(SpecialType = PXSpecialButtonType.Insert, Tooltip = Messages.AddNewTimecardToolTip, ImageKey = PX.Web.UI.Sprite.Main.AddNew)]
		[PXUIField]
		[PXEntryScreenRights(typeof(EPTimeCard), nameof(TimeCardMaint.Insert))]
		protected virtual void Create()
		{
			using (new PXPreserveScope())
			{
				TimeCardMaint graph = (TimeCardMaint)CreateInstance(typeof(TimeCardMaint));
				graph.Clear(PXClearOption.ClearAll);
				graph.Document.Insert();
				if (Filter.Current.EmployeeID != null && Filter.Current.EmployeeID != graph.Document.Current.EmployeeID)
				{
					graph.Document.Current.EmployeeID = Filter.Current.EmployeeID;
					graph.Document.Update(graph.Document.Current);
				}
				PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.InlineWindow);
			}
		}

		public PXAction<TimecardFilter> update;
		[PXButton(Tooltip = Messages.EditTimecardToolTip, ImageKey = PX.Web.UI.Sprite.Main.RecordEdit)]
		protected virtual void Update()
		{
			EPTimeCard row = PXSelect<EPTimeCard, Where<EPTimeCard.timeCardCD, Equal<Current<TimecardWithTotals.timeCardCD>>>>.Select(this);
			if (row == null) return;

			PXRedirectHelper.TryRedirect(this, row, PXRedirectHelper.WindowMode.InlineWindow);
		}

		public PXAction<TimecardFilter> delete;
		[PXUIField]
		[PXDeleteButton(Tooltip = Messages.DeleteTimecardToolTip)]
		[PXEntryScreenRights(typeof(EPTimeCard))]
		protected void Delete()
		{
			EPTimeCard row = PXSelect<EPTimeCard, Where<EPTimeCard.timeCardCD, Equal<Current<TimecardWithTotals.timeCardCD>>>>.Select(this);
			if (row == null) return;

			using (new PXPreserveScope())
			{
				TimeCardMaint graph = (TimeCardMaint)CreateInstance(typeof(TimeCardMaint));
				graph.Clear(PXClearOption.ClearAll);
				graph.Document.Current = row;
				graph.Delete.Press();
			}
		} 

		#endregion

		public override IEnumerable ExecuteSelect(string viewName, object[] parameters, object[] searches, string[] sortcolumns,
			bool[] descendings, PXFilterRow[] filters, ref int startRow, int maximumRows, ref int totalRows)
		{
			if (viewName == "Items")
			{
				for (int i = 0; i < sortcolumns.Length; i++)
				{
					if (string.Compare(sortcolumns[i], "WeekID_description", true) == 0)
					{
						sortcolumns[i] = "WeekID";
					}
				}
			}

			return base.ExecuteSelect(viewName, parameters, searches, sortcolumns, descendings, filters, ref startRow, maximumRows, ref totalRows);
		}

		protected virtual void TimecardFilter_EmployeeID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			EPEmployee employee = PXSelect<EPEmployee, Where<EPEmployee.userID, Equal<Current<AccessInfo.userID>>>>.Select(this);
			if (employee != null)
				e.NewValue = employee.AcctCD;
		}

		[Serializable]
		public partial class TimecardFilter : IBqlTable
		{
			private int? _employeeId;

			#region EmployeeID
			public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }

			[PXDBInt]
			[PXUIField(DisplayName = "Employee")]
			[PXSubordinateAndWingmenSelector()]
			[PXFieldDescription]
			public virtual Int32? EmployeeID
			{
				get { return _employeeId; }
				set { _employeeId = value; }
			}

			#endregion
		}
	}
}
