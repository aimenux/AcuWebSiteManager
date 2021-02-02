<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="GL503000.aspx.cs" Inherits="Page_GL503000"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%" runat="server" Visible="True" PrimaryView="Filter"
		TypeName="PX.Objects.GL.FinPeriodStatusProcess">
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server"  DataMember="Filter" Width="100%">
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="SM" />
			<px:PXSegmentMask ID="edOrganizationID" runat="server" DataField="OrganizationID" CommitChanges="True"/>
            <px:PXDropDown ID="edAction" runat="server" DataField="Action" CommitChanges="True"/>
			<px:PXSelector ID="edFromYear" runat="server" DataField="FromYear" CommitChanges="True" AutoRefresh="true"/>
			<px:PXSelector ID="edToYear" runat="server" DataField="ToYear" CommitChanges="True" AutoRefresh="true"/>
			<px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="SM"/>
			<px:PXCheckBox ID="cbReopenInModules" runat="server" DataField="ReopenInSubledgers" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" AllowSearch="true" Width="100%"
		SkinID="PrimaryInquire" AllowPaging="true" AdjustPageSize="Auto">
		<Levels>
			<px:PXGridLevel DataMember="FinPeriods">
				<Columns>
					<px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" AllowNull="False" AllowSort="False" AllowMove="False" CommitChanges="true"/>
					<px:PXGridColumn DataField="FinPeriodID"/>
					<px:PXGridColumn DataField="Descr" />
					<px:PXGridColumn DataField="Status" />
					<px:PXGridColumn DataField="APClosed" TextAlign="Center" Type="CheckBox" />
					<px:PXGridColumn DataField="ARClosed" TextAlign="Center" Type="CheckBox" />
					<px:PXGridColumn DataField="INClosed" TextAlign="Center" Type="CheckBox" />
					<px:PXGridColumn DataField="CAClosed" TextAlign="Center" Type="CheckBox" />
					<px:PXGridColumn DataField="FAClosed" TextAlign="Center" Type="CheckBox" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
	    <AutoSize Container="Window" Enabled="True" MinHeight="400" />
	</px:PXGrid>
</asp:Content>
