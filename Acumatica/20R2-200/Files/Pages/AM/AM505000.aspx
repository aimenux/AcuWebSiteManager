<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AM505000.aspx.cs" Inherits="Page_AM505000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" 
        TypeName="PX.Objects.AM.Fullregen" PrimaryView="MrpProcessing" BorderStyle="NotSet" >
		<CallbackCommands>
        </CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" 
		Width="100%" DataMember="MrpProcessing">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM"/>
            <px:PXDateTimeEdit ID="edLastMrpRegenCompletedDateTime" runat="server" DataField="LastMrpRegenCompletedDateTime" Width="175px" />
            <px:PXTextEdit ID="edLastMrpRegenCompletedByID" runat="server" DataField="LastMrpRegenCompletedByID" Width="175px" />
        </Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Width="100%" Height="150px" SkinID="Inquire" AutoCallBack="True">
		<Levels>
			<px:PXGridLevel DataKeyNames="Recno" DataMember="AuditDetailRecs">
			    <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="M" ControlSize="XM" />
                    <px:PXDateTimeEdit ID="CreatedDateTime" runat="server" DataField="EventDate"/>
                    <px:PXTextEdit ID="edMsgText" runat="server" DataField="MsgText" />
                    <px:PXTextEdit ID="edCreatedByID" runat="server" DataField="CreatedByID" />
                    <px:PXMaskEdit ID="edCreatedByScreenID" runat="server" DataField="CreatedByScreenID" InputMask="aa.aa.aa.aa" />
                    <px:PXTextEdit ID="edProcessID" runat="server" DataField="ProcessID" />
                    <px:PXComboBox ID="edMsgType" runat="server" DataField="MsgType" />
                </RowTemplate>
			    <Columns>
                    <px:PXGridColumn DataField="Recno" Label="Recno" TextAlign="Right" />
                    <px:PXGridColumn DataField="CreatedDateTime" DisplayFormat="g" Width="140px" />
                    <px:PXGridColumn DataField="MsgText" Width="400px" />
                    <px:PXGridColumn DataField="CreatedByScreenID" DisplayFormat="aa.aa.aa.aa" Width="108px"/>
                    <px:PXGridColumn DataField="CreatedByID" Width="115px"/>
                    <px:PXGridColumn DataField="ProcessID" Width="115px"/>
                    <px:PXGridColumn DataField="MsgType" Width="108px"/>
                </Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
		<ActionBar ActionsText="False">
		</ActionBar>
	</px:PXGrid>
</asp:Content>
