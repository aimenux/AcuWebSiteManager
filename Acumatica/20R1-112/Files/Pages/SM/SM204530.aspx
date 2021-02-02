<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true" CodeFile="SM204530.aspx.cs" Inherits="Pages_SM_SM204530" %>

<asp:Content ID="Content1" ContentPlaceHolderID="phDS" Runat="Server">
    <style type="text/css">
        .ctl00_phDS_ds_ToolBar {
    margin-left: 0px !important;
    
    
}

        .toolModNormal {
            color: white;
        }

        .splitterVert {
            border-left: 0px;
            border-right: 0px;
            border-top: 1px solid #BBBBBB;
            background-color: #E5E9EE;
            
                  }
        .pageTitle {
            background-color: #E5E9EE !important
        }
        #ctl00_usrCaption_tlbPath_cmdBranch {
             background-color: #E5E9EE !important
        }

        .SmartPanelCN {
    background-color: white !important;
            vertical-align: top;
            margin-left: 4px;
   
}
    </style>
    <script type="text/javascript">
        var hideScript = 1;
    </script>
        <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Projects"
            BackColor="Black"
        TypeName="PX.SM.CustProjectMaint" PageLoadBehavior="PopulateSavedValues">
        <CallbackCommands>
            <%--<px:PXDSCallbackCommand DependOnGrid="gridVersions" Name="Cancel" />--%>
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <%--<px:PXDSCallbackCommand Name="copy" CommitChanges="true" StartNewGroup="True" />--%>
            <%--<px:PXDSCallbackCommand Name="saveAs" CommitChanges="true" Visible="False" />--%>
            <%--<px:PXDSCallbackCommand Name="viewObject" CommitChanges="true" Visible="false" DependOnGrid="grid" />--%>
            <px:PXDSCallbackCommand Name="actionFileShow" Visible="false" BlockPage="true" CommitChanges="false" DependOnGrid="grid" RepaintControls="Bound"></px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="actionFileSave" Visible="false" BlockPage="true" CommitChanges="true" RepaintControls="Bound" CommitChangesIDs="FormEditFile"></px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="actionEditItem" Visible="false" BlockPage="true" CommitChanges="true" RepaintControls="Bound"></px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="actionOpenScreen" Visible="false" BlockPage="true" CommitChanges="true" RepaintControls="Bound"></px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="actionFileNew" Visible="false" BlockPage="true" CommitChanges="true" RepaintControls="Bound"></px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="actionPublish" BlockPage="true" CommitChanges="true" RepaintControls="Bound" />
            <px:PXDSCallbackCommand Name="actionGenInq" Visible="false" BlockPage="true" CommitChanges="false" RepaintControls="Bound"></px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="actionFileCheckIn" Visible="false" BlockPage="true" CommitChanges="true" RepaintControls="Bound"></px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="actionNextRow" Visible="false" BlockPage="false" CommitChanges="false" RepaintControls="None" RepaintControlsIDs="PanelSource"></px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="actionNodeSelect" Visible="False" BlockPage="True" CommitChanges="True" RepaintControls="None" RepaintControlsIDs="PanelSource,form,tab" DependOnTree="tree" />


            <px:PXDSCallbackCommand Name="actionSiteMap" Visible="false" BlockPage="true" CommitChanges="true" RepaintControls="Bound"></px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="actionGI" Visible="false" BlockPage="true" CommitChanges="true" RepaintControls="Bound"></px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="actionReports" Visible="false" BlockPage="true" CommitChanges="true" RepaintControls="Bound"></px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="actionShowDlgAddFile" Visible="False" BlockPage="true" CommitChanges="False" RepaintControls="Bound"></px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="actionShowDlgCheckinFiles" Visible="False" BlockPage="true" CommitChanges="False" RepaintControls="Bound"></px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="actionRemoveConflicts" Visible="False" BlockPage="true" CommitChanges="True" RepaintControls="Bound"></px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="actionAddCodeFile" Visible="False" BlockPage="true" CommitChanges="False" RepaintControls="Bound"></px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="actionRefreshDbTables" Visible="False" BlockPage="true" CommitChanges="False" RepaintControls="Bound"></px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="actionNewFile" Visible="False" CommitChanges="False" RepaintControls="Bound" />
            <px:PXDSCallbackCommand Name="actionNewProject" Visible="False" CommitChanges="True" RepaintControls="Bound" />
            <px:PXDSCallbackCommand Name="actionConvertControls" BlockPage="true" CommitChanges="True" RepaintControls="All" />
            <px:PXDSCallbackCommand Name="actionUsrField" Visible="False" BlockPage="true" CommitChanges="True" PostData="Content" RepaintControlsIDs="FormUsrField,form" />


            <%--			<px:PXDSCallbackCommand Name="actionTableEdit" Visible="false" BlockPage="true" CommitChanges="true" RepaintControls="Bound" ></px:PXDSCallbackCommand>
            --%>
            <px:PXDSCallbackCommand Name="actionTableNew" Visible="false" BlockPage="true" CommitChanges="true" RepaintControls="Bound"></px:PXDSCallbackCommand>


        </CallbackCommands>

        <DataTrees>
            <px:PXTreeDataMember TreeKeys="NodeId" TreeView="ProjectTree" />
        </DataTrees>
    </px:PXDataSource>



</asp:Content>






<asp:Content ID="Content2" ContentPlaceHolderID="phF" Runat="Server">
    
        <px:PXSplitContainer runat="server" ID="sp1" Height="800px" Width="100%" SplitterPosition="250">
        <AutoSize Enabled="true" Container="Window" />
        <Template1>
        <%--    BackColor="#E5E9EE"--%>
            <px:PXTreeView runat="server" ID="tree" DataSourceID="ds" DataMember="ProjectTree"
                PopulateOnDemand="False" ShowRootNode="False"
                ShowLines="False"
                
                BackColor="#E5E9EE"
                Width="100%"
                Height="800px"
                Style="padding-top: 20px"
                ExpandDepth="1" FastExpand="True" SelectFirstNode="True"
               
                >
                <DataBindings>
                    <px:PXTreeItemBinding DataMember="ProjectTree" TextField="Name" ValueField="NodeId" ImageUrlField="NodeImage" />
                </DataBindings>
                <AutoSize Enabled="True" />
<%--                <AutoCallBack Target="ds" Command="actionNodeSelect">
                </AutoCallBack>--%>
            </px:PXTreeView>
        </Template1>
        <Template2>
          <px:PXSmartPanel RenderVisible="True" 
              AllowResize="False"
              AllowMove="False"
              runat="server" ID="PanelEditor" AutoSize-Enabled="True" 
              Width="100%" 
              Height="800px"
              InnerPageUrl="~/Pages/SM/SM204520.aspx?HideScript=On"
              IFrameName="EditorFrame" 
              RenderIFrame="True" 
              Position="Original">
               
           </px:PXSmartPanel>           
        </Template2>
  

            </px:PXSplitContainer>

</asp:Content>

