<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AR408500.aspx.cs" Inherits="Page_AR408500" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter" TypeName="PX.Objects.AR.ARDunningLetterByDocumentEnq">
          <CallbackCommands>
			<px:PXDSCallbackCommand Visible="false" Name="ViewDocument" CommitChanges="true" DependOnGrid="grid" />
			<px:PXDSCallbackCommand Visible="false" Name="ViewLetter" CommitChanges="true" DependOnGrid="grid" />
		</CallbackCommands>
          </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" 
		Width="100%" DataMember="Filter" TabIndex="100">
		<Template>
			<px:PXLayoutRule runat="server" StartRow="True"/>
            <px:PXSelector CommitChanges="true"  ID="edBAccountID" runat="server" DataField="BAccountID"/>	
		    <px:PXDateTimeEdit CommitChanges="true" ID="edBeginDate" runat="server" DataField="BeginDate"/>
            <px:PXDateTimeEdit CommitChanges="true"  ID="edEndDate" runat="server" DataField="EndDate"/>		
					<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True"/>
                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Include Levels" /> 
            <px:PXNumberEdit CommitChanges="true"  ID="edLevelFrom" runat="server" DataField="LevelFrom"/>
            <px:PXNumberEdit CommitChanges="true"  ID="edLevelTo" runat="server" DataField="LevelTo"/>
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100;" Width="100%" AllowSearch="True" AllowFilter="true" AdjustPageSize="Auto" SkinID="Details"
		AllowPaging="True" FilesIndicator="false" NoteIndicator="false">
		<Levels>
			<px:PXGridLevel DataKeyNames="DunningLetterID" DataMember="EnqResults">
			    <Columns>
                     <px:PXGridColumn DataField="ARInvoice__CustomerID" DisplayMode="Value" Width="120px"/>
                     <px:PXGridColumn DataField="ARInvoice__DocType" DisplayMode="Text"  Width="90px"/>
                     <px:PXGridColumn DataField="ARInvoice__RefNbr"  Width="90px"  LinkCommand="ViewDocument"/>
                     <px:PXGridColumn DataField="ARInvoice__DocBal"  Width="90px" />
                     <px:PXGridColumn DataField="ARInvoice__DueDate"  Width="90px" />
                     <px:PXGridColumn DataField="DunningLetterLevel"  Width="60px" />
                     <px:PXGridColumn DataField="ARDunningLetter__Status" Type="DropDownList"  Width="90px" />
                     <px:PXGridColumn DataField="ARDunningLetter__DunningLetterDate"  Width="90px" />
                </Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
        <ActionBar>
			<CustomItems>
                   <px:PXToolBarButton Text="View Dunning Letter" Key="cmdViewLetter">
					    <AutoCallBack Command="ViewLetter" Target="ds" />
				    </px:PXToolBarButton>
			</CustomItems>
		</ActionBar>
	</px:PXGrid>
</asp:Content>
