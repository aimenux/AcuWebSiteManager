<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="EP507030.aspx.cs" Inherits="Page_EP507030" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter" TypeName="PX.Objects.EP.EmployeeSummaryApprove"/>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" 
		Width="100%" DataMember="Filter" TabIndex="100">
        <Template>
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True"/>
            <px:PXSelector runat="server" DataField="ApproverID" DataSourceID="ds" ID="edApproverID" CommitChanges="True"/>
            <px:PXSelector runat="server" DataField="FromWeek" ID="FromWeek" CommitChanges="True" ValueField="WeekID" DisplayMode="Text"/>
            <px:PXSelector runat="server" DataField="TillWeek" ID="TillWeek" CommitChanges="True" ValueField="WeekID" DisplayMode="Text"/>
            <px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True"/>
            <px:PXSegmentMask runat="server" DataField="ProjectID" DataSourceID="ds" ID="ProjectID" CommitChanges="True"/>
            <px:PXSegmentMask runat="server" DataField="ProjectTaskID" DataSourceID="ds" ID="ProjectTaskID" CommitChanges="True" AutoRefresh="True"/>
            <px:PXSelector runat="server" DataField="EmployeeID" DataSourceID="ds" ID="PXSelector1" CommitChanges="True"/>
            <px:PXLayoutRule ID="PXLayoutRule3" runat="server" GroupCaption="Regular" StartColumn="True" StartGroup="True" />
            <px:PXTimeSpan runat="server" DataField="RegularTime" ID="RegularTime" SuppressLabel="True" SummaryMode="true" Enabled="False" Size="XS" LabelWidth="55" InputMask="hh:mm" MaxHours="99"/>
            <px:PXLayoutRule ID="PXLayoutRule4" runat="server" GroupCaption="Overtime" StartColumn="True" StartGroup="True"/>
            <px:PXTimeSpan runat="server" DataField="RegularOvertime" ID="RegularOvertime" SummaryMode="true" Enabled="False" SuppressLabel="True" Size="XS" InputMask="hh:mm" MaxHours="99"/>
            <px:PXLayoutRule ID="PXLayoutRule5" runat="server" GroupCaption="Total" StartColumn="True" StartGroup="True"/>
            <px:PXTimeSpan runat="server" DataField="RegularTotal" ID="RegularTotal" SummaryMode="true" Enabled="False" SuppressLabel="True" Size="XS" InputMask="hh:mm" MaxHours="99"/>
        </Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" 
		Width="100%" Height="150px" SkinID="PrimaryInquire" TabIndex="700" FilesIndicator="False" NoteIndicator="True" SyncPosition="True" FastFilterFields="EmployeeID,Description" >
		<Levels>
			<px:PXGridLevel DataKeyNames="TaskID" DataMember="Summary">
                <RowTemplate>
                    <px:PXLayoutRule ID="PXLayoutRule6" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                    <px:PXTimeSpan ID="PXMaskEditMon" runat="server" DataField="Mon" InputMask="hh:mm" />
                    <px:PXTimeSpan ID="PXTimeSpan1" runat="server" DataField="Tue" InputMask="hh:mm" />
                    <px:PXTimeSpan ID="PXTimeSpan2" runat="server" DataField="Wed" InputMask="hh:mm" />
                    <px:PXTimeSpan ID="PXTimeSpan3" runat="server" DataField="Thu" InputMask="hh:mm" />
                    <px:PXTimeSpan ID="PXTimeSpan4" runat="server" DataField="Fri" InputMask="hh:mm" />
                    <px:PXTimeSpan ID="PXTimeSpan5" runat="server" DataField="Sat" InputMask="hh:mm" />
                    <px:PXTimeSpan ID="PXTimeSpan6" runat="server" DataField="Sun" InputMask="hh:mm" />
                    <px:PXTimeSpan ID="PXTimeSpan7" runat="server" DataField="TimeSpent" InputMask="hh:mm" MaxHours="99" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="IsApprove" TextAlign="Center" Type="CheckBox"/>
                    <px:PXGridColumn DataField="IsReject" TextAlign="Center" Type="CheckBox"/>
                    <px:PXGridColumn DataField="WeekID"/>
                    <px:PXGridColumn DataField="EmployeeID" TextField="AcctName" DisplayMode="Text"/>
                    <px:PXGridColumn DataField="EarningType" AutoCallBack="true"/>
                    <px:PXGridColumn DataField="ParentNoteID" AutoCallBack="true" TextField="Summary" DisplayMode="Text"/>
                    <px:PXGridColumn DataField="ProjectID" AutoCallBack="true" />
                    <px:PXGridColumn DataField="ProjectTaskID" AutoCallBack="true"  />
                    <px:PXGridColumn DataField="Mon" AutoCallBack="true" RenderEditorText="True" />
                    <px:PXGridColumn DataField="Tue" AutoCallBack="true" RenderEditorText="True"/>
                    <px:PXGridColumn DataField="Wed" AutoCallBack="true" RenderEditorText="True"/>
                    <px:PXGridColumn DataField="Thu" AutoCallBack="true" RenderEditorText="True"/>
                    <px:PXGridColumn DataField="Fri" AutoCallBack="true" RenderEditorText="True"/>
                    <px:PXGridColumn DataField="Sat" AutoCallBack="true" RenderEditorText="True"/>
                    <px:PXGridColumn DataField="Sun" AutoCallBack="true" RenderEditorText="True"/>
                    <px:PXGridColumn DataField="TimeSpent" RenderEditorText="True" />
                    <px:PXGridColumn DataField="IsBillable" Type="CheckBox" />
                    <px:PXGridColumn DataField="Description" />
                    <px:PXGridColumn DataField="TimeCardCD"/>
                </Columns>

			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	    <ActionBar DefaultAction="ViewDetails"/>
        <Mode AllowAddNew="False" AllowDelete="False"/>
	</px:PXGrid>
</asp:Content>

