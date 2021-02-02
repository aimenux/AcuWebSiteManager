<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AM201200.aspx.cs" Inherits="Page_AM201200" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" BorderStyle="NotSet" PrimaryView="CurrentBucket" TypeName="PX.Objects.AM.MRPBucketMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
        </CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="CurrentBucket" Caption="MRP Buckets"
        DefaultControlID="edBucketID" NoteIndicator="True" FilesIndicator="True" ActivityIndicator="True" ActivityField="NoteActivity" TabIndex="100">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXLayoutRule runat="server" Merge="True" />
            <px:PXSelector ID="edBucketID" runat="server" DataField="BucketID" AutoRefresh="True" DataSourceID="ds" />
            <px:PXCheckBox CommitChanges="True" ID="chkActiveFlg" runat="server" DataField="ActiveFlg" />
            <px:PXLayoutRule runat="server" LabelsWidth="S" ControlSize="XL" />
            <px:PXTextEdit ID="edDescr" runat="server" DataField="Descr"/>
        </Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" SkinID="Details" Width="100%" SyncPosition="True" >
        <Levels>
            <px:PXGridLevel DataMember="BucketRecords">
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="M" ControlSize="XM" />
                    <px:PXNumberEdit ID="edBucket" runat="server" DataField="Bucket" />
                    <px:PXNumberEdit ID="edValue" runat="server" DataField="Value" />
                    <px:PXDropDown ID="edInterval" runat="server" DataField="Interval" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="Bucket" Width="150px" />
                    <px:PXGridColumn DataField="Value" Width="150px" />
                    <px:PXGridColumn DataField="Interval" Width="150px" />
                </Columns>
			</px:PXGridLevel>
		</Levels>
        <Mode AllowUpload="True" InitNewRow="True" />
		<AutoSize Container="Window" Enabled="True" MinHeight="250" />
		<ActionBar ActionsText="False">
		    
		</ActionBar>
	</px:PXGrid>
</asp:Content>