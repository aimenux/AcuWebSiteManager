<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AM405000.aspx.cs" Inherits="Page_AM405000" Title="Work Center Capacity" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.AM.WorkCenterCapacityInq" PrimaryView="CapacityFilter">
		<CallbackCommands>
		    <px:PXDSCallbackCommand Name="ViewSchedule" Visible="False" DependOnGrid="grid" />   
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" DataMember="CapacityFilter" DefaultControlID="edWcID" TabIndex="100" >
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True"/>
            <px:PXSelector ID="edWcID" runat="server" DataField="WcID" CommitChanges="True" />
            <px:PXDropDown ID="edCapacityRange" runat="server" DataField="CapacityRange" CommitChanges="True" />
            <px:PXLayoutRule runat="server" StartColumn="True"/>
            <px:PXDateTimeEdit ID="edFromDate" runat="server" DataField="FromDate" CommitChanges="True" />
            <px:PXDateTimeEdit ID="edToDate" runat="server" DataField="ToDate" CommitChanges="True" />
        </Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" SkinID="Details" Width="100%" TabIndex="1700">
		<Levels>
			<px:PXGridLevel DataKeyNames="WcID,SchdDate" DataMember="CapacityDetail">
                <RowTemplate>
                    <px:PXSelector ID="edWcID" runat="server" DataField="WcID" AllowEdit="True"/>
                    <px:PXDateTimeEdit ID="edSchdDate" runat="server" DataField="SchdDate"/>
                    <px:PXNumberEdit ID="edTotalBlocks" runat="server" DataField="TotalBlocks"/>
                    <px:PXNumberEdit ID="edPlanBlocks" runat="server" DataField="PlanBlocks"/>
                    <px:PXNumberEdit ID="edSchdBlocks" runat="server" DataField="SchdBlocks"/>
                    <px:PXNumberEdit ID="edAvailableBlocks" runat="server" DataField="AvailableBlocks"/>
                    <px:PXNumberEdit ID="edPlanUtilizationPct" runat="server" DataField="PlanUtilizationPct"/>
                    <px:PXNumberEdit ID="edSchdUtilizationPct" runat="server" DataField="SchdUtilizationPct"/>
                    <px:PXDateTimeEdit ID="edFromDate" runat="server" DataField="FromDate"/>
                    <px:PXDateTimeEdit ID="edToDate" runat="server" DataField="ToDate"/>
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="WcID"/>
                    <px:PXGridColumn DataField="SchdDate" Width="90px" LinkCommand="ViewSchedule" />
                    <px:PXGridColumn DataField="TotalBlocks" TextAlign="Right"/>
                    <px:PXGridColumn DataField="PlanBlocks" TextAlign="Right"/>
                    <px:PXGridColumn DataField="SchdBlocks" TextAlign="Right"/>
                    <px:PXGridColumn DataField="AvailableBlocks" TextAlign="Right"/>
                    <px:PXGridColumn DataField="PlanUtilizationPct" TextAlign="Right"/>
                    <px:PXGridColumn DataField="SchdUtilizationPct" TextAlign="Right"/>
                    <px:PXGridColumn DataField="FromDate" Width="90px"/>
                    <px:PXGridColumn DataField="ToDate" Width="90px"/>
                  </Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<ActionBar ActionsText="False">
		</ActionBar>
	</px:PXGrid>
</asp:Content>
