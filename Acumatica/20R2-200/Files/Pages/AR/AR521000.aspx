<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AR521000.aspx.cs" Inherits="Page_AR521000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter" TypeName="PX.Objects.AR.ARDunningLetterProcess" />
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100"
        Width="100%" DataMember="Filter" ActivityField="StartDate" FilesField="" NoteField="" Caption="Selection">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="M" />
            <px:PXSelector CommitChanges="True" runat="server" DataField="CustomerClassID" DataSourceID="ds" ID="edCustomerClass" />
            <px:PXDateTimeEdit CommitChanges="True" runat="server" DataField="DocDate" ID="edDocDate" />
            <px:PXCheckBox ID="chkIncludeNonOverdue" runat="server" DataField="IncludeNonOverdueDunning" CommitChanges="true" />

            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" GroupCaption="Include" />
            <px:PXGroupBox ID="gbIncludeType" runat="server" Caption="Include" CommitChanges="True"
                DataField="IncludeType" RenderSimple="True" RenderStyle="Simple">
                <Template>
                    <px:PXRadioButton ID="rbAll" runat="server" GroupName="gbIncludeType" Text="All Overdue Documents" Value="0" />
                    <px:PXRadioButton ID="rbRange" runat="server" GroupName="gbIncludeType" Text="Dunning Letter Level" Value="1" />
                </Template>
            </px:PXGroupBox>
            <px:PXNumberEdit ID="edLevelFrom" runat="server" DataField="LevelFrom" CommitChanges="true" />
            <px:PXNumberEdit ID="edLevelTo" runat="server" DataField="LevelTo" CommitChanges="true" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100; height: 150px;"
        Width="100%" SkinID="PrimaryInquire" Caption="Customers" AllowPaging="true"
        AdjustPageSize="Auto" FastFilterFields="CustomerClassID, BAccountID, BAccountID_BAccountR_acctName">
        <Levels>
            <px:PXGridLevel DataMember="DunningLetterList">
                <Columns>
                    <px:PXGridColumn AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" AllowSort="False" AllowMove="False" CommitChanges="true" />
                    <px:PXGridColumn DataField="CustomerClassID" />
                    <px:PXGridColumn AllowUpdate="False" DataField="BAccountID" />
                    <px:PXGridColumn DataField="BAccountID_BAccountR_acctName" />
                    <px:PXGridColumn DataField="DueDate" />
                    <px:PXGridColumn AllowNull="False" DataField="BranchID" TextAlign="Left" />
                    <px:PXGridColumn AllowNull="False" DataField="DocBal" TextAlign="Right" />
                    <px:PXGridColumn AllowNull="False" DataField="NumberOfOverdueDocuments" TextAlign="Right" />
                    <px:PXGridColumn AllowNull="False" DataField="OrigDocAmt" TextAlign="Right" />
                    <px:PXGridColumn AllowNull="False" DataField="NumberOfDocuments" TextAlign="Right" />
                    <px:PXGridColumn AllowNull="False" DataField="DunningLetterLevel" TextAlign="Right" />
                    <px:PXGridColumn DataField="LastDunningLetterDate" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="400" />
    </px:PXGrid>
</asp:Content>
