<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
ValidateRequest="false" CodeFile="CR204000.aspx.cs" Inherits="Page_CR204000"
Title="Mailing Lists" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource EnableAttributes="true" ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.CR.CRMarketingListMaint"
                     PrimaryView="MailLists">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <%--<px:PXDSCallbackCommand Name="Details" DependOnGrid="grdSubscribers" Visible="false" />--%>
            <px:PXDSCallbackCommand Name="AddAction" Visible="False" />
            <px:PXDSCallbackCommand Name="DeleteAction" Visible="False" />
            <px:PXDSCallbackCommand Name="LinkToContact" Visible="False" />
            <px:PXDSCallbackCommand Name="InnerProcess" Visible="false" />
            <px:PXDSCallbackCommand Name="InnerProcessAll" Visible="false" />
            <px:PXDSCallbackCommand Name="NewTask" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewEvent" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewMailActivity" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewActivity" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ViewActivity" DependOnGrid="gridActivities" Visible="False" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="syncHubSpot" Visible="false" />
            <px:PXDSCallbackCommand Name="pullFromHubSpot" Visible="false" />
            <px:PXDSCallbackCommand Name="pushToHubSpot" Visible="false" />
        </CallbackCommands>
        <DataTrees>
            <px:PXTreeDataMember TreeView="_EPCompanyTree_Tree_" TreeKeys="WorkgroupID" />
        </DataTrees>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100"
                   Width="100%" DataMember="MailLists"  Caption="Marketing List Info"
                   FilesIndicator="True" 
                   NoteIndicator="True"
                   ActivityIndicator="False" ActivityField="NoteActivity" 
                   DefaultControlID="edMailListCode" >
        <Template>
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True"  LabelsWidth="XXM" ControlSize="M" /> 
            <px:PXLayoutRule ID="PXLayoutRule2" runat="server" Merge="True" />
            <px:PXSegmentMask Size="SM" ID="edMailListCode" runat="server" DataField="MailListCode"  />
            <px:PXCheckBox ID="chkIsActive" runat="server" DataField="IsActive" />
            <px:PXLayoutRule ID="PXLayoutRule3" runat="server" Merge="False" />

            <px:PXTextEdit ID="edName" runat="server" AllowNull="False" DataField="Name"  />
            <px:PXLayoutRule ID="PXLayoutRule4" runat="server" StartColumn="True"  LabelsWidth="s" ControlSize="sm" />

            <px:PXTreeSelector CommitChanges="True" ID="edWorkgroupID" runat="server" DataField="WorkgroupID" TreeDataMember="_EPCompanyTree_Tree_"
                               TreeDataSourceID="ds"  PopulateOnDemand="true"
                               InitialExpandLevel="0" ShowRootNode="false">
                <DataBindings>
                    <px:PXTreeItemBinding TextField="Description" ValueField="Description" />
                </DataBindings>
            </px:PXTreeSelector>
            <px:PXSelector CommitChanges="True" ID="edOwnerID" runat="server" DataField="OwnerID" AutoRefresh="True" TextField="acctname"  />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
<px:PXTab ID="tab" runat="server" Width="100%" 
          DataMember="MailListsCurrent"  DataSourceID="ds" RepaintOnDemand="false" >
    <Items>
        <px:PXTabItem Text="Configuration Options">
            <Template>
                <px:PXPanel ID="PXPanel1" runat="server">
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" />
                    <px:PXCheckBox CommitChanges="True" ID="edIsDynamic" runat="server" DataField="IsDynamic"  />
                    <px:PXSelector CommitChanges="true" ID="edGIDesignID" runat="server" DataField="GIDesignID" DisplayMode="Text" AllowEdit="true" />
                    <px:PXDropDown CommitChanges="True" ID="edSharedGIFilter" runat="server" DataField="SharedGIFilter" AutoRefresh="true" />

                </px:PXPanel>
                <px:PXRichTextEdit ID="edDescription" runat="server" DataField="Description" Style="border-width: 0px; width: 100%;"
                                   AllowAttached="true" AllowSearch="true" AllowMacros="true" AllowLoadTemplate="false" AllowSourceMode="true">
                    <LoadTemplate TypeName="PX.SM.SMNotificationMaint" DataMember="Notifications" ViewName="NotificationTemplate" ValueField="notificationID" TextField="Name" DataSourceID="ds" Size="M"/>
                    <AutoSize Enabled="True" MinHeight="216" />
                </px:PXRichTextEdit>
                <px:PXCheckBox CommitChanges="True" ID="edIsSelectionCriteria" runat="server" DataField="IsSelectionCriteria" Visible="false"/>
            </Template>
        </px:PXTabItem>

        <px:PXTabItem Text="Selection Criteria" VisibleExp="DataControls[&quot;edIsSelectionCriteria&quot;].Value == true">
            <Template>
                <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" Height="100%" Width="100%" ActionsPosition="Top" SkinID="Details"
                           MatrixMode="true">	                    
                    <ClientEvents CellEditorCreated="cellEditorCreated" />
                    <Mode InitNewRow="true"></Mode>
                    <Levels>                        
                        <px:PXGridLevel DataMember="SelectrionCriteria">				                
                            <Columns>
                                <px:PXGridColumn DataField="IsUsed" Type="CheckBox" TextAlign="Center" />
                                <px:PXGridColumn DataField="OpenBrackets" Type="DropDownList" />
                                <px:PXGridColumn DataField="DataField" Type="DropDownList" TextAlign="Left" CommitChanges="True"  />
                                <px:PXGridColumn DataField="Condition" Type="DropDownList" CommitChanges="True" />
                                <px:PXGridColumn DataField="ValueSt" AllowStrings="True"/>
                                <px:PXGridColumn DataField="ValueSt2" AllowStrings="True"/>
                                <px:PXGridColumn DataField="CloseBrackets" Type="DropDownList" />
                                <px:PXGridColumn DataField="Operator" Type="DropDownList" />
                            </Columns>
                            <RowTemplate>
                                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                                <px:PXDropDown ID="edCondition" runat="server" DataField="Condition" />
                            </RowTemplate>
                        </px:PXGridLevel>
                    </Levels>
                    <AutoSize Container="Window" Enabled="True" MinHeight="150" />
                </px:PXGrid>
            </Template>
        </px:PXTabItem>



        <px:PXTabItem Text="List Members" RepaintOnDemand="true" LoadOnDemand="true">
            <AutoCallBack Command="Refresh" Target="grdSubscribers"/>
            <Template>
                    
                <px:PXGrid ID="grdSubscribers" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100; border: 0px;"
                           Width="100%" SkinID="Details" AllowPaging="True" ActionsPosition="Top"
                           AllowSearch="true" AutoAdjustColumns="true" AdjustPageSize="auto" SyncPosition="True" MatrixMode="true">
                    <Mode AllowUpload="True"/>
                    <Levels>
                        <px:PXGridLevel DataMember="MailRecipients" >
                            <Columns>                                
                                <px:PXGridColumn AllowCheckAll="True" AllowNull="False" DataField="Selected" AllowMove="False"
                                                 AllowSort="False" TextAlign="Center" Type="CheckBox" />
                                <px:PXGridColumn DataField="IsSubscribed" Type="CheckBox" TextAlign="Center" />
                                <px:PXGridColumn DataField="ContactID" TextField="ContactBAccount__Contact"
                                                 AutoCallBack="true" DisplayMode="Text" TextAlign="Left" LinkCommand="Contact_ViewDetails"/>
                                <px:PXGridColumn DataField="Contact__Salutation" />
                                <px:PXGridColumn DataField="Contact__ContactType" />
                                <px:PXGridColumn DataField="Contact__Email" />
                                <px:PXGridColumn DataField="Contact__BAccountID" LinkCommand="BAccount_ViewDetails" />
                                <px:PXGridColumn DataField="Contact__FullName" />
                                <px:PXGridColumn AllowNull="False" DataField="Contact__IsActive" TextAlign="Center"
                                                 Type="CheckBox" />
                                <px:PXGridColumn DataField="CreatedDateTime" AllowUpdate="False" DisplayFormat="g" Visible="false"/>
                            </Columns>
                            <RowTemplate>
                                <px:PXSelector CommitChanges="True" ID="edContactID" runat="server" DataField="ContactID" FilterByAllFields="true">
                                    <GridProperties FastFilterFields="DisplayName,Contact__EMail">
                                    </GridProperties>
                                </px:PXSelector>
                            </RowTemplate>
                            <Mode AllowUpload="True" />
                        </px:PXGridLevel>
                    </Levels>
                    <AutoSize Enabled="True" MinHeight="216" />
                    <ActionBar PagerVisible="False">
                        <PagerSettings Mode="NextPrevFirstLast" />
                        <Actions>
                            <Save Enabled="False" />
                            <Delete Enabled="False" />
                        </Actions>
                        <CustomItems>
                            <px:PXToolBarButton Text="Delete selected" Key="cmdMultipleDelete" DisplayStyle="Image" ImageKey="RecordDel">
                                <AutoCallBack Command="DeleteAction" Target="ds" />
                            </px:PXToolBarButton>
                            <px:PXToolBarButton Text="Add new members" Key="cmdMultipleInsert">
                                <AutoCallBack Command="AddAction" Target="ds" />
                            </px:PXToolBarButton>								
                        </CustomItems>
                    </ActionBar>
                </px:PXGrid>
            </Template>
        </px:PXTabItem>
        <px:PXTabItem Text="Activities" LoadOnDemand="True">
            <Template>
                <pxa:PXGridWithPreview ID="gridActivities" runat="server" DataSourceID="ds" Width="100%"
                                       AllowSearch="True" DataMember="Activities" AllowPaging="true" NoteField="NoteText"
                                       FilesField="NoteFiles" BorderWidth="0px" GridSkinID="Inquire" 
                                       PreviewPanelStyle="z-index: 100; background-color: Window" PreviewPanelSkinID="Preview"
                                       BlankFilterHeader="All Activities" MatrixMode="true" PrimaryViewControlID="form">
                    <ActionBar DefaultAction="cmdViewActivity" CustomItemsGroup="0">
                        <CustomItems>
                            <px:PXToolBarButton Key="cmdAddTask">
                                <AutoCallBack Command="NewTask" Target="ds" />
                            </px:PXToolBarButton>
                            <px:PXToolBarButton Key="cmdAddEvent">
                                <AutoCallBack Command="NewEvent" Target="ds" />
                            </px:PXToolBarButton>
                            <px:PXToolBarButton Key="cmdAddEmail">
                                <AutoCallBack Command="NewMailActivity" Target="ds" />
                            </px:PXToolBarButton>
                            <px:PXToolBarButton Key="cmdAddActivity">
                                <AutoCallBack Command="NewActivity" Target="ds" />
                            </px:PXToolBarButton>
                        </CustomItems>
                    </ActionBar>
                    <Levels>
                        <px:PXGridLevel DataMember="Activities">
                            <Columns>
                                <px:PXGridColumn DataField="IsCompleteIcon" Width="21px" AllowShowHide="False" AllowResize="False"
                                                 ForceExport="True" />
                                <px:PXGridColumn DataField="PriorityIcon" Width="21px" AllowShowHide="False" AllowResize="False"
                                                 ForceExport="True" />
                                <px:PXGridColumn DataField="ClassIcon" Width="31px" AllowShowHide="False" AllowResize="False"
                                                 ForceExport="True" />
                                <px:PXGridColumn DataField="ClassInfo" />
                                <px:PXGridColumn DataField="RefNoteID" Visible="false" AllowShowHide="False" />
                                <px:PXGridColumn DataField="Subject" LinkCommand="ViewActivity" />
                                <px:PXGridColumn DataField="UIStatus" />
                                <px:PXGridColumn DataField="Released" TextAlign="Center" Type="CheckBox"  />
                                <px:PXGridColumn DataField="StartDate" DisplayFormat="g" />
                                <px:PXGridColumn DataField="CreatedDateTime" DisplayFormat="g" Visible="False" />
                                <px:PXGridColumn DataField="TimeSpent" />
                                <px:PXGridColumn DataField="CreatedByID" Visible="false" AllowShowHide="False" />
                                <px:PXGridColumn DataField="CreatedByID_Creator_Username" Visible="false"
                                                 SyncVisible="False" SyncVisibility="False">
                                    <NavigateParams>
                                        <px:PXControlParam Name="PKID" ControlID="gridActivities" PropertyName="DataValues[&quot;CreatedByID&quot;]" />
                                    </NavigateParams>
                                </px:PXGridColumn>
                                <px:PXGridColumn DataField="WorkgroupID" />
                                <px:PXGridColumn DataField="OwnerID" LinkCommand="OpenActivityOwner" DisplayMode="Text" />
                                <px:PXGridColumn DataField="ProjectID" AllowShowHide="true" Visible="false" SyncVisible="false" />
                                <px:PXGridColumn DataField="ProjectTaskID" AllowShowHide="true" Visible="false" SyncVisible="false"/>
                            </Columns>
                        </px:PXGridLevel>
                    </Levels>
                    <PreviewPanelTemplate>
                        <px:PXHtmlView ID="edBody" runat="server" DataField="body" TextMode="MultiLine"
                                       MaxLength="50" Width="100%" Height="100px" SkinID="Label" >
                            <AutoSize Container="Parent" Enabled="true" />
                        </px:PXHtmlView>
                    </PreviewPanelTemplate>
                    <AutoSize Enabled="true" />
                    <GridMode AllowAddNew="False" AllowDelete="False" AllowFormEdit="False" AllowUpdate="False" AllowUpload="False" />
                </pxa:PXGridWithPreview>
            </Template>
        </px:PXTabItem>
		<!--#include file="~\Pages\HS\HubSpotTab.inc"-->
    </Items>
    <AutoSize Enabled="True" Container="Window" />
</px:PXTab>
	
<px:PXSmartPanel ID="pnlMailRecipients" runat="server" Key="MailRecipients" LoadOnDemand="true" Width="800px" Height="500px"
                 Caption="Add Members" CaptionVisible="true" AutoRepaint="true" DesignView="Content" ShowMaximizeButton="True">
	    
    <px:PXFormView ID="formAddItem" runat="server" CaptionVisible="False" DataMember="Operations" DataSourceID="ds"
                   Width="100%" SkinID="Transparent">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="SM" />
            <px:PXDropDown CommitChanges="True" ID="edDataSource" runat="server" DataField="DataSource" AllowNull="false" />
            <px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="SM" />
            <px:PXLayoutRule runat="server" Merge="True" />
            <px:PXSelector CommitChanges="True" ID="edContactGI" runat="server" DataField="ContactGI" AllowEdit="true" />			
            <px:PXSelector CommitChanges="True" ID="edMarketingListID" runat="server" DataField="SourceMarketingListID" AllowEdit="true" />
            <px:PXLabel ID="Fake" runat="server" Width="40px"/>
            <px:PXDropDown CommitChanges="True" ID="edSharedGIFilter" runat="server" DataField="SharedGIFilter" AutoRefresh="true" />
        </Template>
    </px:PXFormView>		

    <px:PXGrid ID="grdItems" runat="server" DataSourceID="ds" Style="border-width: 1px 0px; top: 0px; left: 0px; height: 189px;"
               AutoAdjustColumns="true" Width="100%" SkinID="Inquire" AdjustPageSize="Auto" AllowSearch="True" MatrixMode="true" SyncPosition="true" >
        <Levels>
            <px:PXGridLevel DataMember="Items">
                <Columns>
                    <px:PXGridColumn AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox" AutoCallBack="true"
                                     AllowCheckAll="true" CommitChanges="true" />
                    <px:PXGridColumn DataField="ContactType" />
                    <px:PXGridColumn DataField="DisplayName" LinkCommand="LinkToContact"  />
                    <px:PXGridColumn DataField="Title" Visible="false" SyncVisible="false" />
                    <px:PXGridColumn DataField="FirstName" Visible="false" SyncVisible="false" />
                    <px:PXGridColumn DataField="MidName" Visible="false" SyncVisible="false" />
                    <px:PXGridColumn DataField="LastName" Visible="false" SyncVisible="false" />
                    <px:PXGridColumn DataField="Salutation" AllowUpdate ="false" />
                    <px:PXGridColumn DataField="FullName" AllowUpdate ="false"/>
                    <px:PXGridColumn DataField="IsActive" Type="CheckBox" />
                    <px:PXGridColumn DataField="EMail" />
                    <px:PXGridColumn DataField="ClassID" Visible="false" SyncVisible="false" />
                    <px:PXGridColumn DataField="Status" Visible="false" SyncVisible="false" />
                    <px:PXGridColumn DataField="Source" Visible="false" SyncVisible="false" />
                    <px:PXGridColumn DataField="Phone1" DisplayFormat="+#(###) ###-####" />
                    <px:PXGridColumn DataField="Phone2" DisplayFormat="+#(###) ###-####" Visible="false" SyncVisible="false" />
                    <px:PXGridColumn DataField="Phone3" DisplayFormat="+#(###) ###-####" Visible="false" SyncVisible="false" />
                    <px:PXGridColumn DataField="Fax" DisplayFormat="+#(###) ###-####" Visible="false" SyncVisible="false" />
                    <px:PXGridColumn DataField="WorkgroupID" Visible="false" SyncVisible="false" />
                    <px:PXGridColumn DataField="OwnerID" DisplayMode="Text" Visible="false" SyncVisible="false"/>
                    <px:PXGridColumn DataField="CreatedByID_Creator_Username" Visible="false" SyncVisible="false" />
                    <px:PXGridColumn DataField="CreatedDateTime" Visible="false" SyncVisible="false" />
                    <px:PXGridColumn DataField="LastModifiedByID_Modifier_Username" Visible="false" SyncVisible="false" />
                    <px:PXGridColumn DataField="LastModifiedDateTime" Visible="false" SyncVisible="false" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Parent" Enabled="True" MinHeight="150" />
        <ActionBar PagerVisible="False">
            <PagerSettings Mode="NextPrevFirstLast" />
            <Actions>
                <FilterShow Enabled="False" />
                <FilterSet Enabled="False" />
            </Actions>
        </ActionBar>
        <Mode AllowAddNew="False" AllowDelete="False" />
        <CallbackCommands>
            <Save PostData="Page" />
        </CallbackCommands>
    </px:PXGrid>

    <px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons">
        <px:PXButton ID="PXButton1" runat="server" CommandName="InnerProcess" CommandSourceID="ds" DialogResult="OK" Text="Process" SyncVisible="false" />
        <px:PXButton ID="PXButton2" runat="server" CommandName="InnerProcessAll" CommandSourceID="ds" DialogResult="OK" Text="Process All" SyncVisible="false" />
        <px:PXButton ID="PXButton3" runat="server" DialogResult="Cancel" Text="Cancel" />
    </px:PXPanel>

</px:PXSmartPanel>
<script type="text/javascript">
    function cellEditorCreated(grid, ev)
    {
        var editor = ev.cell.editor.control;
        if (editor.__className == "PXSelector")
        {
            if (ev.cell.promptChar) editor.setPromptChar(ev.cell.promptChar);
            editor.textMode = editor.textField ? 1 : 0;
            if (editor.setHintState) editor.setHintState();
        }
    }
</script>
</asp:Content>
