<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="CT503000.aspx.cs" Inherits="Page_CT503000"
    Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter" TypeName="PX.Objects.CT.ContractPriceUpdate">
        <CallbackCommands>
             <px:PXDSCallbackCommand Visible="false" Name="ViewContract" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100"
        Width="100%" DataMember="Filter" Caption="Selection" DefaultControlID="edContractItem">
        <Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
            <px:PXSegmentMask CommitChanges="True" ID="edContractItem" runat="server" DataField="ContractItemID" />
            <px:PXLayoutRule runat="server" StartColumn="True" StartRow="true" LabelsWidth="SM" ControlSize="SM" StartGroup="True" />
            <px:PXDropDown runat="server" CommitChanges="true" ID="edBasePriceOption" DataField="SelectedContractItem.BasePriceOption" Enabled="False"/>
            <px:PXNumberEdit runat="server" ID="PXNumberEdit1" DataField="SelectedContractItem.BasePrice" Enabled="false"/>
			<px:PXNumberEdit runat="server" ID="edBasePriceVal" DataField="SelectedContractItem.BasePriceVal" Enabled="false"/>
            
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="SM" StartGroup="True" />
            <px:PXDropDown runat="server" CommitChanges="true" ID="edRenewalPriceOption" DataField="SelectedContractItem.RenewalPriceOption" Enabled="False"/>
			<px:PXNumberEdit runat="server" CommitChanges="true" AllowNull="False" ID="edRenewalPrice" DataField="SelectedContractItem.RenewalPrice" Enabled="False" />
			<px:PXNumberEdit runat="server" ID="edRenewalPriceVal" DataField="SelectedContractItem.RenewalPriceVal" Enabled="false"/>
            
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="SM" StartGroup="True" />
            <px:PXDropDown runat="server" CommitChanges="true" ID="edFixedRecurringPriceOption" DataField="SelectedContractItem.FixedRecurringPriceOption" Enabled="false" />
			<px:PXNumberEdit runat="server" CommitChanges="true" AllowNull="False" ID="edFixedRecurringPrice" DataField="SelectedContractItem.FixedRecurringPrice" Enabled="false" />
			<px:PXNumberEdit runat="server" ID="edFixedRecurringPriceVal" DataField="SelectedContractItem.FixedRecurringPriceVal" Enabled="false"/>
			
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="SM" StartGroup="True" />
           	<px:PXDropDown runat="server" CommitChanges="true" ID="edUsagePriceOption" DataField="SelectedContractItem.UsagePriceOption" Enabled="false" />
			<px:PXNumberEdit runat="server" CommitChanges="true" AllowNull="false" ID="edUsagePrice" DataField="SelectedContractItem.UsagePrice" Enabled="false" />
            <px:PXNumberEdit runat="server" ID="edUsagePriceVal" DataField="SelectedContractItem.UsagePriceVal" Enabled="false" />
            
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
        Width="100%" Caption="Items" SkinID="PrimaryInquire" FastFilterFields="ContractID, Description, CustomerID, CustomerName" SyncPosition="true">
        <Levels>
            <px:PXGridLevel DataMember="Items">
                <Columns>
                    <px:PXGridColumn AllowCheckAll="True" AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox" Width="30px" />
                    <px:PXGridColumn DataField="Contract__StrIsTemplate" Width="120px" />
                    <px:PXGridColumn DataField="Contract__ContractCD" Width="120px" LinkCommand="ViewContract" />
                    <px:PXGridColumn DataField="Contract__Status" Width="120px" />
                    <px:PXGridColumn DataField="BasePriceOption" Width="120px"/>
                    <px:PXGridColumn DataField="BasePrice" Width="120px"/>
                    <px:PXGridColumn DataField="BasePriceVal" Width="120px"/>
                    <px:PXGridColumn DataField="RenewalPriceOption" Width="120px"/>
                    <px:PXGridColumn DataField="RenewalPrice" Width="120px"/>
                    <px:PXGridColumn DataField="RenewalPriceVal" Width="120px"/>
                    <px:PXGridColumn DataField="FixedRecurringPriceOption" Width="120px"/>
                    <px:PXGridColumn DataField="FixedRecurringPrice" Width="120px"/>
                    <px:PXGridColumn DataField="FixedRecurringPriceVal" Width="120px"/>
                    <px:PXGridColumn DataField="UsagePriceOption" Width="120px"/>
                    <px:PXGridColumn DataField="UsagePrice" Width="120px"/>
                    <px:PXGridColumn DataField="UsagePriceVal" Width="120px"/>
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="400" />
    </px:PXGrid>
</asp:Content>
