<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="EP507010.aspx.cs" Inherits="Page_EP507010" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter" TypeName="PX.Objects.EP.EmployeeActivitiesApprove"/>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" 
		Width="100%" DataMember="Filter" TabIndex="100">
        <Template>
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True"/>
            <px:PXSelector runat="server" DataField="ApproverID" DataSourceID="ds" ID="edApproverID" CommitChanges="True"/>
            <px:PXDateTimeEdit runat="server" DataField="FromDate" ID="FromDate" CommitChanges="True"/>
            <px:PXDateTimeEdit runat="server" DataField="TillDate" ID="TillDate" CommitChanges="True"/>
            <px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True"/>
            <px:PXSegmentMask runat="server" DataField="ProjectID" DataSourceID="ds" ID="ProjectID" CommitChanges="True" AutoRefresh="True"/>
            <px:PXSegmentMask runat="server" DataField="ProjectTaskID" DataSourceID="ds" ID="ProjectTaskID" CommitChanges="True" AutoRefresh="True"/>
            <px:PXSelector runat="server" DataField="EmployeeID" DataSourceID="ds" ID="PXSelector1" CommitChanges="True"/>
            <px:PXLayoutRule ID="PXLayoutRule3" runat="server" GroupCaption="Regular" StartColumn="True" StartGroup="True" />
            <px:PXTimeSpan runat="server" DataField="RegularTime" ID="RegularTime" SummaryMode="true" Enabled="False" Size="XS" LabelWidth="55" InputMask="hh:mm" MaxHours="99"/>
            <px:PXTimeSpan ID="BillableTime" runat="server" DataField="BillableTime" SummaryMode="true" Enabled="False" Size="XS" LabelWidth="55" InputMask="hh:mm" MaxHours="99"/>
            <px:PXLayoutRule ID="PXLayoutRule4" runat="server" GroupCaption="Overtime" StartColumn="True" StartGroup="True"/>
            <px:PXTimeSpan runat="server" DataField="RegularOvertime" ID="RegularOvertime" SummaryMode="true" Enabled="False" SuppressLabel="True" Size="XS" InputMask="hh:mm" MaxHours="99"/>
            <px:PXTimeSpan ID="BillableOvertime" runat="server" DataField="BillableOvertime" SummaryMode="true" Enabled="False" SuppressLabel="True" Size="XS" InputMask="hh:mm" MaxHours="99"/>
            <px:PXLayoutRule ID="PXLayoutRule5" runat="server" GroupCaption="Total" StartColumn="True" StartGroup="True"/>
            <px:PXTimeSpan runat="server" DataField="RegularTotal" ID="RegularTotal" SummaryMode="true" Enabled="False" SuppressLabel="True" Size="XS" InputMask="hh:mm" MaxHours="99"/>
            <px:PXTimeSpan runat="server" DataField="BillableTotal" ID="BillableTotal" SummaryMode="true" Enabled="False" SuppressLabel="True" Size="XS" InputMask="hh:mm" MaxHours="99"/>
        </Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" 
		Width="100%" Height="150px" SkinID="PrimaryInquire" TabIndex="700" FilesIndicator="False" NoteIndicator="True" SyncPosition="True"
        FastFilterFields="Owner,Subject" >
		<Levels>
			<px:PXGridLevel DataKeyNames="TaskID" DataMember="Activity">
			    <RowTemplate>
					<px:PXTimeSpan TimeMode="True" ID="edTimeSpent" runat="server" DataField="TimeSpent" InputMask="hh:mm" MaxHours="99" />
					<px:PXTimeSpan TimeMode="True" ID="edTimeBillable" runat="server" DataField="TimeBillable"  InputMask="hh:mm" MaxHours="99" />
					<px:PXDateTimeEdit TimeMode="True" ID="edOvertimeBillable" runat="server" DataField="OvertimeBillable" />
                </RowTemplate>
			    <Columns>
                    <px:PXGridColumn DataField="IsApproved" TextAlign="Center" Type="CheckBox" Width="80px"/>
                    <px:PXGridColumn DataField="IsReject" TextAlign="Center" Type="CheckBox" Width="60px"/>
                    <px:PXGridColumn DataField="Date" Width="70px" />
                    <px:PXGridColumn DataField="OwnerID" Width="120px" />
                    <px:PXGridColumn DataField="OwnerID_EPEmployee_AcctName" Width="120px" />
                    <px:PXGridColumn DataField="EarningTypeID" Width="80px" />
                    <px:PXGridColumn DataField="ParentTaskNoteID" AutoCallBack="True" TextField="Summary" DisplayMode="Text"/>
                    <px:PXGridColumn DataField="ContractID" Width="90px" LinkCommand="ViewContract"/>
                    <px:PXGridColumn DataField="ProjectID" Width="90px" />
                    <px:PXGridColumn DataField="ProjectTaskID"  Width="90px"/>
                    <px:PXGridColumn DataField="TimeSpent" Width="80px" RenderEditorText="True" />
                    <px:PXGridColumn DataField="IsBillable" TextAlign="Center" Type="CheckBox" Width="70px"/>
                    <px:PXGridColumn DataField="TimeBillable" Width="80px" RenderEditorText="True" />
                    <px:PXGridColumn DataField="Summary" Width="120px" LinkCommand="ViewDetails" />
                    <px:PXGridColumn DataField="ApproverID" Width="100px" />
                    <px:PXGridColumn DataField="TimeCardCD" Width="100px" />
					<px:PXGridColumn DataField="CRCase__CaseCD" Width="90px" LinkCommand="ViewCase" />
					<px:PXGridColumn DataField="ContractEx__ContractCD" Width="90px" LinkCommand="ViewContract" />
                </Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	    <ActionBar DefaultAction="ViewDetails"/>
	    <Mode AllowAddNew="False" AllowDelete="False"/>
	</px:PXGrid>
</asp:Content>

