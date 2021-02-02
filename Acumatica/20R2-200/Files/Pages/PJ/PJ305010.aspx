<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true" ValidateRequest="false"
    CodeFile="PJ305010.aspx.cs" Inherits="Page_PJ305010" Title="Photo" %>

<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>
<asp:Content ID="DataSourceContent" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%"
        TypeName="PX.Objects.PJ.PhotoLogs.PJ.Graphs.PhotoEntry" PrimaryView="Photos">
        <CallbackCommands>
            <px:PXDSCallbackCommand Visible="False" Name="UploadPhoto" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="PhotoContent" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" DataMember="Photos"
        Caption="Photo" NoteIndicator="True" FilesIndicator="False" AllowCollapse="False">
        <AutoSize Container="Window" Enabled="True"/>
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
            <px:PXLayoutRule runat="server" Merge="True" />
            <px:PXSelector ID="edPhotoLogId" runat="server" DataField="PhotoLogId" FilterByAllFields="True"
                           AutoRefresh="True" CommitChanges="True"/>
            <px:PXButton ID="btnUploadPhoto" runat="server" Text="Upload Photo" CommandName="UploadPhoto" CommandSourceID="ds"/>
            <px:PXLayoutRule runat="server" Merge="False"/>
            <px:PXSelector ID="edPhotoCd" runat="server" DataField="PhotoCd" FilterByAllFields="True"
                           AutoRefresh="True" CommitChanges="True" >
                <GridProperties>
                    <Columns>
                        <px:PXGridColumn DataField="ImageUrl" Type="Icon" />
                        <px:PXGridColumn DataField="PhotoCd" />
                        <px:PXGridColumn DataField="Name" />
                        <px:PXGridColumn DataField="Description" />
                        <px:PXGridColumn DataField="UploadedDate" />
                        <px:PXGridColumn DataField="UploadedById" />
                    </Columns>
                </GridProperties>
            </px:PXSelector>
            <px:PXTextEdit ID="edName" runat="server" DataField="Name" CommitChanges="True"/>
            <px:PXTextEdit ID="edDescription" runat="server" DataField="Description" CommitChanges="True" />
            <px:PXDateTimeEdit ID="UploadedDate" runat="server" AlreadyLocalized="False" DataField="UploadedDate" Size="S" />
            <px:PXSelector ID="edUploadedById" runat="server" DataField="UploadedById" />
            <px:PXCheckBox ID="edIsMainPhoto" runat="server" AlreadyLocalized="False" DataField="IsMainPhoto" CommitChanges="True" />
            <px:PXLayoutRule runat="server" LabelsWidth="S" ControlSize="S" StartGroup="True" GroupCaption="Attributes"/>
            <px:PXGrid ID="gridAttributes" runat="server" DataSourceID="ds" SkinID="Inquire" Width="500px"
                       MatrixMode="True" AllowPaging="True" PageSize="15">
                <Levels>
                    <px:PXGridLevel DataMember="Attributes">
                        <Columns>
                            <px:PXGridColumn DataField="AttributeID" TextAlign="Left" AllowShowHide="False"
                                             TextField="AttributeID_description" />
                            <px:PXGridColumn DataField="isRequired" TextAlign="Center" Type="CheckBox" />
                            <px:PXGridColumn DataField="Value" AllowShowHide="False" AllowSort="False" />
                        </Columns>
                    </px:PXGridLevel>
                </Levels>
                <AutoSize Enabled="True" MinHeight="546" Container="Parent"/>
                <ActionBar>
                    <Actions>
                        <Search Enabled="False" />
                    </Actions>
                </ActionBar>
                <Mode AllowAddNew="False" AllowColMoving="False" AllowDelete="False" />
            </px:PXGrid>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="XL" ControlSize="XXL" />
            <px:PXImageView ID="edImageUrl" runat="server" DataField="ImageUrl" Style="z-index: 110; left: 9px;
                                     position:absolute; top: 9px; max-height: 600px; max-width: 800px;"/>
            <px:PXTextEdit ID="edFileId" runat="server" DataField="FileId" CommitChanges="True"/>
            <px:PXUploadDialog ID="uploadFileDialog" runat="server" AutoSaveFile="true" Caption="Upload New Photo" Key="Photos"
                           Height="110px" SessionKey="UploadFileKey" AllowedFileTypes=".gif;.jpg;.png;"/>
        </Template>
    </px:PXFormView>
    <px:PXJavaScript runat="server" ID="CstJavaScriptSetGridImageHeight" Script="
        let css = '.GridRow > img { height: 40px; }';
        let head = document.head || document.getElementsByTagName('head')[0];
        let style = document.createElement('style');
        style.type = 'text/css';
        style.appendChild(document.createTextNode(css));
        head.appendChild(style);" />
</asp:Content>