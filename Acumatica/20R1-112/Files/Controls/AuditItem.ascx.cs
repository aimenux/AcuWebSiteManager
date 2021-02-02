#region Copyright (c) 1994-2006 PXSoft, Inc. All rights reserved.
/* ---------------------------------------------------------------------*
*                               PXSoft, Inc.                            *
*              Copyright (c) 1994-2006 All rights reserved.             *
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
* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY PXSoft PRODUCT.          *
*                                                                       *
* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *
* --------------------------------------------------------------------- *
*/
#endregion Copyright (c) 1994-2006 PXSoft, Inc. All rights reserved.

using System;
using System.ComponentModel;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using PX.Common;
using PX.Data;
using System.Threading;
using System.Globalization;
using System.Reflection;
using PX.Data.Process;
using PX.Web.UI;
using PX.Web.UI.Controls;

namespace PX.Web.Controls
{
	public partial class Controls_AuditItem : System.Web.UI.UserControl
	{
		private AuditBatch Tag;

		public void Initialise(AuditBatch batch)
		{
			Tag = batch;
		}
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			lblDate.Text = "Date: \t";
			txtDate.Text = Tag.Date.ToString();
			lblUser.Text = "User: \t";
			txtUser.Text = Tag.Username;
			lblScreen.Text = "Screen: \t";
			txtScreen.Text = Tag.Screen;

			DrawBatchTable(tblDetails, Tag);
		}

		protected void DrawBatchTable(Table table, AuditBatch batch)
		{
			foreach(AuditEntry entry in batch)
			{
				TableRow captionRow = new TableRow();
				TableCell captionCell = new TableCell();
				DrawCaptionDiv(captionCell, entry);
				captionRow.Cells.Add(captionCell);
				table.Rows.Add(captionRow);

				TableRow detailsRow = new TableRow();
				TableCell detailsCell = new TableCell();
				detailsCell.ColumnSpan = 1;
				//detailsCell.BackColor = System.Drawing.Color.GhostWhite;
				DrawChangesDiv(detailsCell, entry);
				detailsRow.Cells.Add(detailsCell);
				table.Rows.Add(detailsRow);
			}
		}
		protected void DrawCaptionDiv(TableCell cell, AuditEntry entry)
		{
			PXPanel panel = new PXPanel();
			panel.ContentLayout.Layout = LayoutType.Stack;
			panel.ContentLayout.Orientation = UI.Orientation.Horizontal;
			panel.RenderStyle = FieldsetStyle.Simple;

			IParserAccessor pa = (IParserAccessor)panel;

			Label tableLabel = new Label();
			tableLabel.Font.Bold = true;
			tableLabel.Text = entry.Table;
			pa.AddParsedSubObject(tableLabel);

			Label operationLabel = new Label();
			operationLabel.Text = entry.OperationTitle;
			pa.AddParsedSubObject(operationLabel);

			cell.Controls.Add(panel);
		}
		protected void DrawChangesDiv(TableCell cell, AuditEntry entry)
		{
			PXPanel panel = new PXPanel();
			panel.ContentLayout.Layout = LayoutType.Stack;
			panel.ContentLayout.Orientation = UI.Orientation.Horizontal;
			panel.RenderStyle = FieldsetStyle.Simple;
			panel.ContentLayout.InnerSpacing = false;

			IParserAccessor pa = (IParserAccessor)panel;
			foreach (AuditValue value in entry.Values)
			{
				Table table = new Table();
				table.BorderStyle = BorderStyle.Solid;
				table.BorderWidth = Unit.Pixel(1);
				table.BorderColor = System.Drawing.SystemColors.ControlLight;
				table.Attributes.Add("bordercolor", "ControlLight");
				table.GridLines = GridLines.Both;
				table.Height = Unit.Percentage(100);

				TableRow fieldRow = new TableRow();
				TableCell fieldCell = new TableCell();
				fieldCell.Font.Bold = true;
				fieldCell.Style[HtmlTextWriterStyle.PaddingLeft] = "5px";
				fieldCell.Style[HtmlTextWriterStyle.PaddingRight] = "5px";
				fieldCell.ForeColor = System.Drawing.Color.DarkSlateGray;
				fieldCell.Text = value.DisplayName;
				fieldRow.Cells.Add(fieldCell);
				table.Rows.Add(fieldRow);

				if (entry.Operation == AuditOperation.Update)
				{
					TableRow oldValueRow = new TableRow();
					TableCell oldValueCell = new TableCell();
					oldValueCell.ForeColor = System.Drawing.Color.DarkRed;
					oldValueCell.Height = Unit.Percentage(100);
					SetValue(oldValueCell, value.OldValue, value.Format);
					oldValueRow.Cells.Add(oldValueCell);
					table.Rows.Add(oldValueRow);
				}

				TableRow newValueRow = new TableRow();
				TableCell newValueCell = new TableCell();
				newValueCell.ForeColor = System.Drawing.Color.DarkSlateGray;
				if (entry.Operation == AuditOperation.Insert || entry.Operation == AuditOperation.Update) newValueCell.ForeColor = System.Drawing.Color.DarkGreen;
				if (entry.Operation == AuditOperation.Delete) newValueCell.ForeColor = System.Drawing.Color.DarkRed;
				newValueCell.Height = Unit.Percentage(100);
				SetValue(newValueCell, value.NewValue, value.Format);
				newValueRow.Cells.Add(newValueCell);
				table.Rows.Add(newValueRow);

				pa.AddParsedSubObject(table);
			}
			cell.Controls.Add(panel);
		}
		private void SetValue(TableCell cell, Object value, String format)
		{
			String str = null;
			if (value != null) 
			{
				if(value.GetType() == typeof(DateTime))
				{
					str = ((DateTime)value).ToString(format ?? "g");
				}
				else str = value.ToString();
			}
			if (String.IsNullOrWhiteSpace(str)) str = "<br>";

			cell.Height = 15;
			cell.HorizontalAlign = HorizontalAlign.Right;
			cell.BackColor = System.Drawing.Color.GhostWhite;
			if (value != null && value.GetType() == typeof(Boolean))
			{
				PXImage img = new PXImage();
				img.ImageUrl = ((Boolean)value) ? "control@GridCheck" : "control@GridUncheck";
				cell.HorizontalAlign = HorizontalAlign.Center;
				cell.Controls.Add(img);
			}
			else if (value != null && value.GetType() == typeof(String))
			{
				cell.Text = str;
				cell.HorizontalAlign = HorizontalAlign.Left;
			}
			else cell.Text = str;
		}
	}
}

