<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="EP407000.aspx.cs" Inherits="Pages_EP_EP407000" Title="My Timecards" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.EP.EquipmentTimecardPrimary" PrimaryView="Items">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="create" Visible="True" RepaintControls="All" HideText="True" />
            <px:PXDSCallbackCommand Name="update" Visible="True" RepaintControls="All" DependOnGrid="grid" HideText="True" />
			<px:PXDSCallbackCommand Name="delete" Visible="True" RepaintControls="All" DependOnGrid="grid" HideText="True" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>

<asp:Content ID="cont3" ContentPlaceHolderID="phL" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="150px" SkinID="PrimaryInquire" AdjustPageSize="Auto"
        AllowPaging="True" Caption="Time Cards" FastFilterFields="TimecardCD,Description" SyncPosition="True">
        <Levels>
            <px:PXGridLevel DataMember="Items">
                <RowTemplate>
                    <px:PXSelector ID="edRefNbr" runat="server" DataField="TimecardCD" Enabled="False" AllowEdit="False" />
                    <px:PXSelector CommitChanges="True" ID="edEquipment" runat="server" DataField="EquipmentID" />
                    <px:PXDropDown ID="edStatus" runat="server" DataField="Status" />
                    <px:PXLayoutRule ID="PXLayoutRule1" runat="server" GroupCaption="Time" StartColumn="True" StartGroup="True" />

                    <px:PXTimeSpan runat="server" DataField="EPEquipmentTimeCardSpentTotals__SetupTime" ID="PXTimeSpan1" Enabled="False" Size="XS" LabelWidth="55" InputMask="hh:mm" MaxHours="99" />
                    <px:PXTimeSpan runat="server" DataField="EPEquipmentTimeCardSpentTotals__RunTime" ID="PXTimeSpan2" Enabled="False" Size="XS" LabelWidth="55" InputMask="hh:mm" MaxHours="99" />
                    <px:PXTimeSpan runat="server" DataField="EPEquipmentTimeCardSpentTotals__SuspendTime" ID="PXTimeSpan3" Enabled="False" Size="XS" LabelWidth="55" InputMask="hh:mm" MaxHours="99" />
                    <px:PXTimeSpan runat="server" DataField="EPEquipmentTimeCardSpentTotals__TimeTotalCalc" ID="PXTimeSpan7" Enabled="False" Size="XS" LabelWidth="55" InputMask="hh:mm" MaxHours="99" />
                    <px:PXTimeSpan runat="server" DataField="EPEquipmentTimeCardBillableTotals__SetupTime" ID="PXTimeSpan4" Enabled="False" Size="XS" LabelWidth="55" InputMask="hh:mm" MaxHours="99" />
                    <px:PXTimeSpan runat="server" DataField="EPEquipmentTimeCardBillableTotals__RunTime" ID="PXTimeSpan5" Enabled="False" Size="XS" LabelWidth="55" InputMask="hh:mm" MaxHours="99" />
                    <px:PXTimeSpan runat="server" DataField="EPEquipmentTimeCardBillableTotals__SuspendTime" ID="PXTimeSpan6" Enabled="False" Size="XS" LabelWidth="55" InputMask="hh:mm" MaxHours="99" />
                    <px:PXTimeSpan runat="server" DataField="EPEquipmentTimeCardBillableTotals__TimeTotalCalc" ID="PXTimeSpan8" Enabled="False" Size="XS" LabelWidth="55" InputMask="hh:mm" MaxHours="99" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="EquipmentID" Label="Equipment ID" />
                    <%--<px:PXGridColumn DataField="EquipmentID_description" Label="Description" />--%>
                    <px:PXGridColumn DataField="Status" Label="Status" Type="DropDownList" />
                    <px:PXGridColumn DataField="TimecardCD" Label="TimecardCD" />
                    
                    <px:PXGridColumn DataField="EPEquipmentTimeCardSpentTotals__SetupTime" RenderEditorText="True" />
                    <px:PXGridColumn DataField="EPEquipmentTimeCardSpentTotals__RunTime" RenderEditorText="True" />
                    <px:PXGridColumn DataField="EPEquipmentTimeCardSpentTotals__SuspendTime" RenderEditorText="True" />
                    <px:PXGridColumn DataField="EPEquipmentTimeCardSpentTotals__TimeTotalCalc" RenderEditorText="True" />
                    <px:PXGridColumn DataField="EPEquipmentTimeCardBillableTotals__SetupTime" RenderEditorText="True" />
                    <px:PXGridColumn DataField="EPEquipmentTimeCardBillableTotals__RunTime" RenderEditorText="True" />
                    <px:PXGridColumn DataField="EPEquipmentTimeCardBillableTotals__SuspendTime" RenderEditorText="True" />
                    <px:PXGridColumn DataField="EPEquipmentTimeCardBillableTotals__TimeTotalCalc" RenderEditorText="True" />
                 </Columns>
            </px:PXGridLevel>
        </Levels>
         <ActionBar DefaultAction="update"/>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
	    <Mode AllowAddNew="False" AllowUpdate="False" AllowDelete="False"/>
    </px:PXGrid>
</asp:Content>
