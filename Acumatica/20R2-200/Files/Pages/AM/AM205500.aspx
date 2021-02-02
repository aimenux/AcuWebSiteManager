<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="AM205500.aspx.cs" Inherits="Page_AM205500" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.AM.ToolMaint" PrimaryView="Tools">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100"
		Width="100%" DataMember="Tools" DataKeyNames="ToolID" DefaultControlID="edToolID">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="M" ControlSize="XM" />
            <px:PXSelector ID="edToolID" runat="server" DataField="ToolID" />
            <px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" />
            <px:PXCheckBox ID="chkActive" runat="server" DataField="Active" />
        </Template>
            <Searches>
                <px:PXControlParam ControlID="form" Name="ToolID" PropertyName="NewDataKey[&quot;ToolID&quot;]" Type="String" />
            </Searches>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Width="100%" Height="189px" DataSourceID="ds" DataMember="ToolSelected" DataKeyNames="ToolID">
		<Items>
			<px:PXTabItem Text="General">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="M" ControlSize="XM" />
                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="COST" />
                    <px:PXNumberEdit ID="edUnitCost" runat="server" DataField="UnitCost"  />
                    <px:PXNumberEdit ID="edTotalCost" runat="server" DataField="TotalCost"  />
                    <px:PXNumberEdit ID="edActualCost" runat="server" DataField="ActualCost"  />
                    <px:PXNumberEdit ID="edActualUses" runat="server" DataField="ActualUses"  />
                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="ACCOUNT SETTINGS" />
                    <px:PXSegmentMask ID="edAcctID" runat="server" DataField="AcctID" />
                    <px:PXSegmentMask ID="edSubID" runat="server" CommitChanges="True" DataField="SubID" SelectMode="Segment"/>
                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="SCHEDULING" />
                    <px:PXCheckBox ID="edScheduleEnabled" runat="server" DataField="ScheduleEnabled" CommitChanges="True" />
                    <px:PXNumberEdit ID="edScheduleQty" runat="server" DataField="ScheduleQty" />
                </Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXTab>
</asp:Content>
