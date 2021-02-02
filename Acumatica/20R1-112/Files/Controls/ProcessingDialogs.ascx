<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ProcessingDialogs.ascx.cs" Inherits="Controls_ProcessingDialogs" %>
<%@ Import Namespace="PX.Data" %>

<px:PXSmartPanel runat="server" 
                         ID="PanelProgress"  
                         Position="CenterWindow"

                         Caption="Processing" 
                         CaptionVisible="True" 
                         Key="PanelProgress" 
                         ShowMaximizeButton="True"
                         ShowCloseButton="False"
                         AllowResize="False"

                         AutoCallBack-Enabled="True" AutoCallBack-Target="ViewProcessingResults" AutoCallBack-Command="Refresh" 
                         ActiveControlID="BtnDone"
                         CssClass="ProgressSmartPanel ProgressSmartPanelCollapsed"
                         ContentCss="ProgressContent"
                         ClientEvents-BeforeShow="BeforeShowProgress"
                         ClientEvents-AfterShow="AfterShowProgress"
                         ClientEvents-AfterHide="OnProgressClosed"
                         ClientEvents-BeforeHide="BeforeHideProgress"
                         LoadOnDemand="False">
        <div id="PanelContent" style="height: 47px;">
            <div style="display: flex;">
                <div id="ErrorIndicator" class="sprite-icon main-icon" icon="Error" style="font-size: 18px;margin-right: 5px;display: none">
                    <div class="main-icon-img control-Error">

                    </div>
                </div>
                <div id="ProcessingMessage" style="width: 700px;vertical-align: super; min-height: 18px;" class="LabelStatus" >

                </div>
            </div>
        <px:PXFormView runat="server" 
                       DefaultControlID="Skipped" 
                       ID="ViewProcessingResults" 
                       DataMember="ViewProcessingResults"
                       AllowCollapse="False" 
                       AutoRepaint="True"
                       ClientEvents-AfterRepaint="SyncControls"
                       SkinID="Transparent">
           
            <Template>
               
                <px:PXTextEdit runat="server" DataField="ElapsedTime" ID="ElapsedTime" Enabled="False" />
       
                <px:PXTextEdit runat="server" DataField="Percentage" ID="Percentage" Enabled="False" />
                <px:PXTextEdit runat="server" DataField="Result" ID="Result" Enabled="False" />
                <px:PXTextEdit runat="server" DataField="ProcessingErrorMessage" ID="ProcessingErrorMessage" Enabled="False" />
                <px:PXTextEdit runat="server"  DataField="Remains" ID="Remains" Enabled="False"></px:PXTextEdit>
                <px:PXTextEdit runat="server"  DataField="Total" ID="Total" Enabled="False"></px:PXTextEdit>
                <px:PXTextEdit runat="server"  DataField="Errors" ID="Errors" Enabled="False"></px:PXTextEdit>
                <px:PXTextEdit runat="server"  DataField="Skipped" ID="Warnings" Enabled="False"></px:PXTextEdit>
                <px:PXTextEdit runat="server"  DataField="ProcessedItems" ID="ProcessedItems" Enabled="False"></px:PXTextEdit>
    
            </Template>
        </px:PXFormView>
       
        <div class="ProgressBarOuter">
            <div id="ProgressBar" class="ProgressBarInitializing" >
                
            </div>
        </div>
            
        </div>
        <px:PXToolBar runat="server" ID="ToolbarProcessing" Height="60px" Width="100%" BackColor="white" CssClass="ProgressToolbar toolBar">
            <ClientEvents ButtonClick="selectFilter">

            </ClientEvents>
            <Items>
                <px:PXToolBarButton Key="Processed" Text="" PopupPanel="PanelLongRunDetails"  />
                <px:PXToolBarButton Key="Errors"  Text=""  PopupPanel="PanelLongRunDetails"/>
                <px:PXToolBarButton Key="Warnings"  Text="" PopupPanel="PanelLongRunDetails"/>
                <px:PXToolBarButton Key="Remains"  Text="" PopupPanel="PanelLongRunDetails"/>
                <px:PXToolBarButton Key="Total"  Text="" PopupPanel="PanelLongRunDetails"/>
                   
                   
                
                   
                
            </Items>
        </px:PXToolBar>
            
           

      
            <px:PXGrid runat="server" ID="gridDetails" 
                        SyncPosition="true"
                        SyncPositionPriority="true"
                       AutoRepaint="True"  
                       Width="100%"
                       Height="300px"
                       AllowPaging="True" AdjustPageSize="Auto"
                       AllowAutoHide="False"
                       FilterView="ViewFieldsFilters"
                       BlankFilterHeader ="ALL" OnBeforeGenerateColumns="gridDetails_OnBeforeGenerateColumns"
                       style="border: 1px solid lightgrey; margin-bottom: 20px;"
                       SkinID="Inquire">
                <AutoSize Enabled="True" MinHeight="300" Container="Parent"></AutoSize>
                <ClientEvents Initialize="GridInit">
                   
                </ClientEvents>
                <Levels>
                    <px:PXGridLevel DataMember="ProcessingView" >
                        <Columns>
                            <px:PXGridColumn DataField="ProcessingStatus" Width="100"/>
                            <px:PXGridColumn DataField="ProcessingMessage" Width="270"/>
                        </Columns>                    

                    </px:PXGridLevel>
                </Levels>
                <ActionBar>
                    <Actions>
                        <FilterShow Enabled="False" />
                        <FilterBar Enabled="False" />     <FilterSet Enabled="False" />
                        
                    </Actions>
                    
<%--                    <CustomItems>
  
                        <px:PXToolBarButton Text="Close" >
                            <ClientEvents ItemClick="HideDetails"></ClientEvents>
                        </px:PXToolBarButton>
                        
                    </CustomItems>--%>
                </ActionBar>
               


            </px:PXGrid>
            
            
        
 

        <div class="ButtonBar">
            <px:PXButton runat="server" DialogResult="None" Text="Cancel Processing" ID="BtnCancelProcessing" >
                <AutoCallBack Enabled="True" Command="ActionCancelProcessing" Target="ds"/>
            </px:PXButton>
            <px:PXButton runat="server" DialogResult="Cancel" Text="Close" ID="BtnDone" Width="150px">
                <AutoCallBack Enabled="True" Command="ActionCloseProcessing" Target="ds"></AutoCallBack>
            </px:PXButton> 
        </div>

       
   

    </px:PXSmartPanel>

    

    <script type="text/javascript">

        var IsExpanded = false;
        var ExpandFilter = null;

        function allowMaximize(bAllow) {
            var icon = px_alls["PanelProgress"].elemCaption.getElementsByClassName("sprite-icon");
            icon[0].style.visibility = bAllow ? "visible" : "hidden";
        }

        function selectFilter(a, b) {
           
            var key = b.button.key;
            var filter = 0;
            if (key === "Errors")
                filter = 1;
            if (key === "Processed")
                filter = 2;
            if (key === "Remains")
                filter = 3;
            if (key === "Warnings")
                filter = 4;

            if (filter === ExpandFilter ) {
                if (px_alls["PanelProgress"].getMaximized())
                    return;

                IsExpanded = false;
            }
            else {
                IsExpanded = true;
            }

            var buttons = px_alls["PanelProgress"].element.getElementsByClassName("ProgressBtnIcon");
            var panelButtons = px_alls['ToolbarProcessing'].items;
            for (var i = 0; i < buttons.length; i++) {
                buttons[i].classList.remove("ac-expand_less");
                panelButtons[i].element.classList.remove("BtnColorDisabled");
                if (!buttons[i].classList.contains("ac-expand_more"))
                    buttons[i].classList.add("ac-expand_more");
            }
                
            var expandButton = b.button.element.getElementsByClassName("ProgressBtnIcon")[0];
            if (IsExpanded) {
                expandButton.classList.remove("ac-expand_more");
                expandButton.classList.add("ac-expand_less");
                for (var j = 0; j < panelButtons.length; j++) {
                    if (panelButtons[j] !== b.button) {
                        panelButtons[j].element.classList.add("BtnColorDisabled");
                    }
                }
            } 

            if (IsExpanded) {
                ExpandFilter = filter;
                var progressPanel = px_alls["PanelProgress"];
                progressPanel.element.classList.remove("ProgressSmartPanelCollapsed");
                if (!progressPanel.element.classList.contains("ProgressSmartPanelExpanded")) {
                    // add this code to move window up BEFORE expanding, if after expand window would not be fully visible on screen
                    var docScrollHeight = __doc(progressPanel).documentElement.offsetHeight;
                    /*progressPanel.element.offsetHeight = 600*/
                    if (docScrollHeight < progressPanel.element.offsetTop + 600 + 5) {
                        var top = (docScrollHeight - 600 - 5);
                        if (top < 0)
                            top = 0;
                        progressPanel.element.style.top = top + "px";
                    }
                    progressPanel.element.classList.add("ProgressSmartPanelExpanded");
                }
                px_alls["gridDetails"].setVisible(true);
                px_alls["gridDetails"].selectFilter(filter);
                progressPanel.layout(false);
                allowMaximize(true);
            } else {
                ExpandFilter = null;
                // var h = px_alls["gridDetails"].getHeight();
                px_alls["gridDetails"].setVisible(false);
                if (px_alls["PanelProgress"].contentCell) 
                    px_alls["PanelProgress"].contentCell.style.height = "";
                if (px_alls["PanelProgress"].elemContent) 
                    px_alls["PanelProgress"].elemContent.style.height = "";
                px_alls["PanelProgress"].element.style.height = "";
                px_alls["PanelProgress"].element.classList.remove("ProgressSmartPanelExpanded");
                if (!px_alls["PanelProgress"].element.classList.contains("ProgressSmartPanelCollapsed"))
                    px_alls["PanelProgress"].element.classList.add("ProgressSmartPanelCollapsed");
                
                //px_alls["PanelProgress"].setSize(_initialWidth, _initialHeight);
                //px_alls["PanelProgress"].layout(false);
                allowMaximize(false);

            }
            

            //  document.getElementById("DetailsContainer").className = "DetailsVisible";

        }

        function HideDetails() {
           
           
            //document.getElementById("DetailsContainer").className = "DetailsHidden";
        }

        function pad(n) {
            var ret = n.toString();
            if (ret.length > 1)
                return ret;

            return "0" + ret;

        }
        //ClientEvents-BeforeShow="BeforeShowProgress"
        //ClientEvents-AfterHide="OnProgressClosed"

        var _ElapsedStart;
        var _ElapsedTimer;
        var _initialWidth = 0;
        var _initialHeight = 0;

        function BeforeHideProgress() {
            if (px_alls["PanelProgress"].getMaximized()) {
                px_alls["gridDetails"].setVisible(false);
                px_alls["PanelProgress"].restore();
                px_alls["PanelProgress"].setSize(_initialWidth, _initialHeight);
            }
            px_alls["ds"].showProcessWindow = false;
            ExpandFilter = null;
        }

        function BeforeShowProgress() {
            IsExpanded = false;
            ExpandFilter = null;
            px_alls['ToolBar'].getButton('ElapsedTime').element.classList.add('progressButton');
            px_alls['ToolBar'].getButton('LongRun').element.classList.add('progressButton');
            px_alls["gridDetails"].setVisible(false);
            
            if (px_alls["PanelProgress"].contentCell) 
                px_alls["PanelProgress"].contentCell.style.height = "";
            if (px_alls["PanelProgress"].elemContent) 
                px_alls["PanelProgress"].elemContent.style.height = "";
            px_alls["PanelProgress"].element.style.height = "";
            px_alls["PanelProgress"].element.classList.remove("ProgressSmartPanelExpanded");
            if (!px_alls["PanelProgress"].element.classList.contains("ProgressSmartPanelCollapsed"))
                px_alls["PanelProgress"].element.classList.add("ProgressSmartPanelCollapsed");
            if (_initialWidth === 0) {
                _initialWidth = px_alls["PanelProgress"].element.offsetWidth;
                _initialHeight = px_alls["PanelProgress"].element.offsetHeight;
            }
            allowMaximize(false);

            var bar = document.getElementById('ProgressBar');
            var panelContent = document.getElementById("PanelContent");
            panelContent.className = "StatusInProgress";
            bar.className = "ProgressBarInitializing";
            bar.style.width = "100%";
            var errorIndicatorElement = document.getElementById('ErrorIndicator');
            errorIndicatorElement.style.display = 'none';
            var errorIndicatorIconElement = errorIndicatorElement.getElementsByClassName("main-icon-img")[0];
            errorIndicatorIconElement.classList.remove('control-Warning');
            errorIndicatorIconElement.classList.remove('control-Error');
        }

        function AfterShowProgress() {
            px_alls["PanelProgress"].element.style.top = "130px";
        }

        function GridInit(a, b) {
           a.setVisible(false);
           // alert(a);
        }

        function OnProgressClosed() {
            
            px_alls['ToolBar'].getButton('ElapsedTime').element.classList.remove('progressButton');
            px_alls['ToolBar'].getButton('LongRun').element.classList.remove('progressButton');
            
            //clearInterval(_ElapsedTimer);
        }




        function SetButton(dest, btnName, editor,title, style, addStyle) {
            var expand = false;

            var filter = null;
            if (ExpandFilter === 0)
                filter = "Total";
            if (ExpandFilter === 1)
                filter = "Errors";
            if (ExpandFilter === 2)
                filter = "Processed";
            if (ExpandFilter === 3)
                filter = "Remains";
            if (ExpandFilter === 4)
                filter = "Warnings";

            if (btnName === filter)
                expand = true;

            var b = dest.getButton(btnName);
            var value = (px_alls[editor].getValue() | "");
            b.mainCell.innerHTML = "<div class='ProgressBtnNum'>" + value
                + "</div><div class='ProgressBtnLabel'>" + title + "</div><i class='ProgressBtnIcon ac " 
                + (expand ? "ac-expand_less" : "ac-expand_more") + " ac-fw'></i>";
            if (value > 0) {
                b.mainCell.parentElement.className = style + " toolsBtn";
            } else {
                b.mainCell.parentElement.className = "BtnDisabled toolsBtn";
            }

            if (addStyle)
                b.mainCell.parentElement.className += " " + addStyle;

            if (IsExpanded /*&& value > 0*/ && !expand) {
                b.mainCell.parentElement.className += " " + "BtnColorDisabled";
            }

        }

        function SyncControls() {
            // AC-126138 - in new UI we have situations, when user can click buttons or make different actions
            // while callback is executing (f.e. Elapsed time). When callback finished it always rewrite current state (in this case - grid FilterID in view state)
            // overriding user actions. So we can have inconsistency result, where user press one filter, but see rows for other filter.
            if (IsExpanded && px_alls["gridDetails"].getFilterID() !== ExpandFilter) {
                px_alls["gridDetails"].setFilterID(ExpandFilter);
                px_alls["gridDetails"].setFilterActive(true);
            }

            var dest = px_alls['ToolbarProcessing'];

            SetButton(dest, "Processed", "ProcessedItems", "<%=PXMessages.LocalizeNoPrefix(ProcessingMessages.StatusProcessed) %>", "BtnCompleted", "BtnFirst");
            SetButton(dest, "Errors", "Errors", "<%=PXMessages.LocalizeNoPrefix(ProcessingMessages.Errors) %>", "BtnErrors");
            SetButton(dest, "Warnings", "Warnings", "<%=PXMessages.LocalizeNoPrefix(ProcessingMessages.StatusWarnings) %>", "BtnWarnings");
            SetButton(dest, "Remains", "Remains", "<%=PXMessages.LocalizeNoPrefix(ProcessingMessages.Remains) %>", "BtnNormal");
            SetButton(dest, "Total", "Total", "<%=PXMessages.LocalizeNoPrefix(ProcessingMessages.Total) %>", "BtnNormal", "BtnLast");


            var bar = document.getElementById('ProgressBar');

            var msg = document.getElementById('ProcessingMessage');
            msg.innerHTML = px_alls['ElapsedTime'].getValue();

            var prs = px_alls['Percentage'].getValue();
            var panelContent = document.getElementById("PanelContent");
            var totalItems = (px_alls["Total"].getValue() | "");

            if ((!prs || prs === "0" ) && totalItems !== 0) {
                panelContent.className = "StatusInProgress";
                bar.className = "ProgressBarInitializing";
                bar.style.width = "100%";
                return;
            }

            var result = px_alls['Result'].getValue();

            if (!result && totalItems !== 0) {
                panelContent.className = "StatusInProgress";
                if (bar.className === "ProgressBarInitializing") {
                    bar.className = "ProgressBarInProcess";
                } else {
                    bar.className = "ProgressBarInProcess ProgressBarTransitionProgress";
                }
                bar.style.width = prs + "%";
            } else {
                bar.className = "ProgressBarInitializing";
                bar.style.width = "0";
                var value = (px_alls["Errors"].getValue() | 0);
                if (value > 0 || (result != null && result !== "" && result !== "<%=PXMessages.LocalizeNoPrefix(ProcessingMessages.ProcessingCompleted) %>"  && result !== "<%=PXMessages.LocalizeNoPrefix(ProcessingMessages.ProcessingCompletedWarnings) %>" )) {
                    panelContent.className = "StatusError";
                    var errorTextElement = px_alls['PanelProgress'].element.getElementsByClassName('LabelAbbrError')[0];
                    var errorIndicatorElement = document.getElementById('ErrorIndicator');
                    var errorIndicatorIconElement = errorIndicatorElement.getElementsByClassName("main-icon-img")[0];
                    if (errorTextElement != null) {
                        px.setTitle(errorTextElement, px_alls['ProcessingErrorMessage'].getValue());
                        errorIndicatorElement.style.display = 'inherit';
                        errorIndicatorIconElement.classList.remove('control-Warning');
                        errorIndicatorIconElement.classList.add('control-Error');
                    }

                    var warnTextElement = px_alls['PanelProgress'].element.getElementsByClassName('LabelAbbrWarning')[0];
                    if (warnTextElement != null) {
                        px.setTitle(warnTextElement, px_alls['ProcessingErrorMessage'].getValue());
                        errorIndicatorElement.style.display = 'inherit';
                        errorIndicatorIconElement.classList.add('control-Warning');
                        errorIndicatorIconElement.classList.remove('control-Error');
                    }
                } else
                    panelContent.className = "StatusCompleted";

            }

        }
    </script>
    

