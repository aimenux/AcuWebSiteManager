<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CA204550.aspx.cs" Inherits="Page_CA204550" Title="Untitled Page"  %>

<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="server" >
	<px:PXDataSource ID="ds" runat="server" AutoCallBack="True" PrimaryView="Rule" TypeName="PX.Objects.CA.CABankTranRuleMaintPopup">        
        <CallbackCommands>           
            <px:PXDSCallbackCommand Name="SaveClose" PostData="Self" Visible="false" CommitChanges="true"/>
            <px:PXDSCallbackCommand Name="SaveAndApply" PostData="Self" Visible="false" ClosePopup="true" CommitChanges="true"/>
            <px:PXDSCallbackCommand Name="Save" PostData="Self" Visible="false" PopupVisible="false" CommitChanges="true"/>
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" Visible="false" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" Visible="false"/>
            <px:PXDSCallbackCommand Name="Last" PostData="Self" Visible="false"/>
            <px:PXDSCallbackCommand Name="Previous" PostData="Self" Visible="false"/>
            <px:PXDSCallbackCommand Name="Next" PostData="Self" Visible="false"/>
            <px:PXDSCallbackCommand Name="Cancel" PostData="Self" Visible="false"/>
            <px:PXDSCallbackCommand Name="Delete" PostData="Self" Visible="false"/>
            <px:PXDSCallbackCommand Name="CopyPaste" PostData="Self" Visible="false"/>
        </CallbackCommands>
	</px:PXDataSource>
  <style type="text/css">
      .phF
      {
        border: none;
        background-color: #F5F7F8;
        background-color: var(--background-color, #F5F7F8);
        margin: 0px !important;
      }
  </style>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataMember="Rule" DataSourceID="ds" ActivityIndicator="False" 
        LinkIndicator="False" FilesIndicator="false" NotifyIndicator="false" NoteIndicator="False" Width="100%" SkinID="Transparent">
        <AutoSize Enabled="true" Container="Window" />
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="m" ControlSize="sm" />
            <px:PXLayoutRule runat="server"/>
            <px:PXTextEdit runat="server" DataField="Description" ID="edDescription" Size="L" />
            <px:PXLayoutRule runat="server" ColumnSpan="2" />
            <px:PXCheckBox runat="server" DataField="IsActive" ID="edIsActive" CommitChanges="true" />
            <px:PXLayoutRule runat="server" GroupCaption="Matching Criteria" LabelsWidth="m" ControlSize="m" ></px:PXLayoutRule>

            <px:PXDropDown runat="server" DataField="BankDrCr" ID="edBankDrCr" CommitChanges="true" />
            <px:PXSegmentMask runat="server" DataField="BankTranCashAccountID" ID="edCashAccountID" CommitChanges="true" />
            <px:PXSelector runat="server" DataField="TranCuryID" ID="edCuryID" />
            <px:PXMaskEdit runat="server" DataField="TranCode" ID="edTranCode" />
            <px:PXTextEdit runat="server" DataField="BankTranDescription" ID="edBankTranDescription" Size="L" />
            <px:PXCheckBox runat="server" DataField="MatchDescriptionCase" ID="edMatchDescriptionCase" />
            <px:PXCheckBox runat="server" DataField="UseDescriptionWildcards" ID="edUseDescriptionWildcards" />
            <px:PXDropDown runat="server" DataField="AmountMatchingMode" ID="edAmountMatchingMode" CommitChanges="true" />
            <px:PXNumberEdit runat="server" DataField="CuryTranAmt" ID="edCuryTranAmt" AllowNull="true" />
            <px:PXNumberEdit runat="server" DataField="MaxCuryTranAmt" ID="edMaxCuryTranAmt" AllowNull="true" />

            <px:PXLayoutRule runat="server"></px:PXLayoutRule>
            <px:PXLayoutRule runat="server" GroupCaption="Output" LabelsWidth="m" ControlSize="m"></px:PXLayoutRule>
            <px:PXDropDown runat="server" DataField="Action" ID="edAction" CommitChanges="true" />
            <px:PXSelector runat="server" DataField="DocumentEntryTypeID" ID="edDocumentEntryType" AutoRefresh="true" />
            <px:PXLayoutRule runat="server" StartRow="True" LabelsWidth="m" ControlSize="sm" />
            <px:PXPanel ID="PXPanel6" runat="server" SkinID="Buttons">
                <px:PXButton ID="PXButton2" runat="server" DialogResult="Yes" Text="Save & Apply"  CommandName="saveAndApply" CommandSourceID="ds" />
                <px:PXButton ID="PXButton1" runat="server" DialogResult="OK" CommandName="SaveClose" CommandSourceID="ds" Text="Save & Close"/>
			    <px:PXButton ID="PXButton13" runat="server" DialogResult="OK" CommandName="Save" CommandSourceID="ds" />
		    </px:PXPanel>
        </Template>
    </px:PXFormView>
</asp:Content>
