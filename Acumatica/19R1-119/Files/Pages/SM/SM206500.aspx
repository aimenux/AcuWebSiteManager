<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SM206500.aspx.cs" Inherits="Page_SM206500"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Job"
		TypeName="PX.SM.SMPrintJobMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Reprint" Visible="True" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="ShowReport" Visible="True" CommitChanges="true" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" Visible="true" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="true" Visible="false" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" Visible="false" />
            			<px:PXDSCallbackCommand Name="Next" PostData="Self" Visible="false" />
            			<px:PXDSCallbackCommand Name="Previous" PostData="Self" Visible="false" />
            			<px:PXDSCallbackCommand Name="CopyPaste" PostData="Self" Visible="false" />
            <px:PXDSCallbackCommand Name="Delete" PostData="Self" Visible="false" />
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" Visible="false" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:content id="cont2" contentplaceholderid="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" Visible="true" DataMember="Filter" AllowCollapse="false">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
            <px:PXDateTimeEdit ID="edStartDate" runat="server" DataField="StartDate" CommitChanges="true" />
            <px:PXDateTimeEdit ID="edEndDate" runat="server" DataField="EndDate" CommitChanges="true" />
            <px:PXCheckBox ID="chkHideProcessed" runat="server" DataField="HideProcessed" CommitChanges="true" />
        </Template>
    </px:PXFormView>
</asp:content>
<asp:content id="cont3" contentplaceholderid="phG" runat="Server">
        <px:PXGrid ID="PXGrid1" runat="server" DataSourceID="ds" Width="100%" SkinID="Details" SyncPosition="True" >
        <Levels>
            <px:PXGridLevel DataMember="Job">
                <Columns>
                    <px:PXGridColumn DataField="Selected" Width="35px" AllowCheckAll="true" Type="CheckBox" TextAlign="Center" />
                    <px:PXGridColumn DataField="JobID" Width="80px" TextAlign="Left" />
                    <px:PXGridColumn DataField="Description" Width="200px" />
                    <px:PXGridColumn DataField="ReportID" Width="120px" />
                    <px:PXGridColumn DataField="PrinterName" Width="180px" />
                    <px:PXGridColumn DataField="CreatedByID" Width="120px" />
                    <px:PXGridColumn DataField="CreatedDateTime" Width="150px" DisplayFormat="g" />
                    <px:PXGridColumn DataField="Status" Width="100px" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="400" />
    </px:PXGrid>
    
   <%-- <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="height: 250px;" Width="100%" 
       SkinID="Details" Height="372px" TabIndex="-7372">
        <Levels>
            <px:PXGridLevel DataMember="Parameters" DataKeyNames="JobId,ParameterName">
                <Columns>
                    <px:PXGridColumn DataField="ParameterName" Width="200px" />
                    <px:PXGridColumn DataField="ParameterValue" Width="400px" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="400" />
    </px:PXGrid>--%>
</asp:content>