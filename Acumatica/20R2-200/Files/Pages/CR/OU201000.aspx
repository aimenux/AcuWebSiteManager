<%@ Page Language="C#" MasterPageFile="~/MasterPages/TabView.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="OU201000.aspx.cs" Inherits="Page_OU201000"
    Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/TabView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Width="100%" PrimaryView="Filter"
        TypeName="PX.Objects.CR.OUSearchMaint" Visible="false" PageLoadBehavior="InsertRecord">
        <CallbackCommands>
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="createAPDoc" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="createAPDocContinue" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="viewAPDoc" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="viewAPDocContinue" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="createLead" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="createContact" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="createCase" />            
            <px:PXDSCallbackCommand CommitChanges="True" Name="createOpportunity" /> 
            <px:PXDSCallbackCommand CommitChanges="True" Name="createActivity" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="logOut" />
        </CallbackCommands>
        <ClientEvents Initialize="initDataSource" CommandPerformed="commandPerformed" />
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phF" runat="Server">
	<style>
		.edHelp {
		min-width:10px !important; width:25px !important; border-style: none !important; margin-left:-0 !important; background-color:transparent !important;
		}
	</style>
    <px:PXFormView ID="form" runat="server" Width="100%" Height="643px" DataSourceID="ds" DataMember="Filter" SkinID="Transparent" OnDataBound="form_OnDataBound">
        <Template>                         
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="XM"/>
			<px:PXButton ID="edBack" runat="server" CommandName="back" AlignLeft="true" CommandSourceID="ds" Hidden="true" CssClass="ms-Button" Style="margin-bottom: 15px;">
                <ClientEvents Click="showControlsForCreateAPDoc" />
            </px:PXButton>

            <px:PXLabel ID="labelOutgoingEmail" runat="server" Text="Email Recipient:" Style="padding: 0; margin: 0 0 -10px -10px;"/>
            <px:PXDropDown ID="edOutgoingEmail" runat="server" CommitChanges="true" Hidden="true" AutoRefresh="true" DataField="OutgoingEmail" SuppressLabel="true" LabelID ="labelOutgoingEmail" Style="margin-left: -10px;"/>

			<px:PXLabel ID="personTitle" runat="server" Text="Person:" Style="padding: 0; margin: 0 0 -10px -10px;" />
			<px:PXLayoutRule Merge="True" runat="server"  ControlSize="XM" SuppressLabel="true"/> 
			<px:PXSelector ID="edContact" runat="server" CommitChanges="True" DataField="ContactID" DisplayMode="Text" SimpleSelect="True" SuppressLabel="true" Style="margin-left: -10px; width: 240px;"/>
			<px:PXButton ID="edHelp" runat="server" Target="_blank" BackColor="Transparent"
				Style="min-width:10px; width:25px; border-style: none; margin-left:-0" CssClass="edHelp"
				ToolTip="Help" AlignLeft="false"
				NavigateUrl="../../Main?ScreenId=ShowWiki&pageid=875b6f76-2820-4ea9-a420-7f7a8bbfb619">
				<Images Normal="~/Icons/OutlookHelp.png" />
			</px:PXButton>
			<px:PXLayoutRule Merge="False" runat="server" />

			<px:PXLayoutRule runat="server" StartGroup="True" ControlSize="XXXS" LabelsWidth="S" />
            <px:PXTextEdit ID="edSEmail" runat="server" DataField="Email" AllowEdit="False" SkinID="BlueLabel" SuppressLabel="true" Style="color: #0078d7; width: 100%; border: none;" Enabled="false"/>            
            <px:PXTextEdit ID="edErrorMessage" runat="server" TextMode="MultiLine" DataField="ErrorMessage" AllowEdit="False" SuppressLabel="true" SkinID="Label" Height="50px" Style="color: red; min-width:290px; overflow:hidden; margin-top: 10px;" />
            <px:PXLabel ID="PXLabel1" runat="server" DataField="Fake" Height="10px"></px:PXLabel>

            <px:PXLayoutRule runat="server" GroupCaption="Info" StartGroup="True" ControlSize="XXXS" LabelsWidth="S" />
			<px:PXTextEdit ID="PXTextEdit3" runat="server" DataField="NewContactFirstName" Width="100%" />
			<px:PXTextEdit ID="PXTextEdit4" runat="server" DataField="NewContactLastName" Width="100%" />
			<px:PXTextEdit ID="PXTextEdit1" runat="server" DataField="NewContactEmail" Width="100%" />
            <px:PXTextEdit ID="edPosition" runat="server" DataField="Salutation" Width="100%" />
            <px:PXSelector ID="edBAccountID" runat="server" CommitChanges="True" DataField="BAccountID" SimpleSelect="True"/>
            <px:PXTextEdit ID="edCompany" runat="server" DataField="FullName" Width="100%"/>            
            <px:PXDropDown ID="edLeadSource" runat="server" DataField="LeadSource" SimpleSelect="True" />
            <px:PXDropDown ID="edContactSource" runat="server" DataField="ContactSource" SimpleSelect="True" />
            <px:PXSelector ID="edCountryID" runat="server" DataField="CountryID" SimpleSelect="True" />                        
            <px:PXLayoutRule Merge="True" runat="server"/>
            <px:PXTextEdit ID="edEntityName" runat="server" DataField="EntityName" SuppressLabel="true" SkinID="Label" Size="S" Style="color:RGBA(0,0,0,0.54)" />
            <px:PXTextEdit ID="edEntityID" runat="server" DataField="EntityID" Size="M" SuppressLabel="true" Style="margin-left: -16px" Width="100%" />

            <px:PXLayoutRule runat="server" GroupCaption="New Case Details" StartGroup="True" ControlSize="XXXS" LabelsWidth="S" />
            <px:PXSelector ID="edCaseClassID" runat="server" DataField="NewCase.CaseClassID" SimpleSelect="True" AutoRefresh="true" CommitChanges="true" />            
            <px:PXSelector ID="edContract" runat="server" DataField="NewCase.ContractID" AutoRefresh="true" SimpleSelect="True"/> 
            <px:PXDropDown ID="edSeverity" runat="server" DataField="NewCase.Severity" SimpleSelect="True" />            
            <px:PXTextEdit ID="edCaseDescription" runat="server" DataField="NewCase.Subject" Width="190px"/>

            <px:PXLayoutRule runat="server" GroupCaption="New Opportunity Details" StartGroup="True" ControlSize="XXXS" LabelsWidth="S" />    
            <px:PXSelector ID="edOpportunityClassID" runat="server" DataField="NewOpportunity.ClassID" SimpleSelect="True" AutoRefresh="true" />            
            <px:PXTextEdit ID="edOpportunityName"  runat="server" DataField="NewOpportunity.Subject" Width="190px" />  
            <px:PXDropDown ID="edStageID" runat="server" DataField="NewOpportunity.StageID" SimpleSelect="True" />                                   
            <px:PXDateTimeEdit ID="edCloseDate" runat="server" DataField="NewOpportunity.CloseDate"/>
            <px:PXLayoutRule Merge="true" runat="server" ControlSize="XXXS" LabelsWidth="S"/>         
            <px:PXNumberEdit CommitChanges="True" ID="edCuryAmount" runat="server" DataField="NewOpportunity.ManualAmount" />
            <px:PXSelector ID="edOpportunityCurrencyID" runat="server" DataField="NewOpportunity.CurrencyID" SimpleSelect="True" AutoRefresh="true"  SuppressLabel="true" Width="74px"/>
            <px:PXLayoutRule Merge="false" runat="server" ControlSize="XXXS" LabelsWidth="S"/>
			<px:PXSelector ID="edBranchID" runat="server" DataField="NewOpportunity.BranchID" SimpleSelect="True" AutoRefresh="true" />

            <px:PXLayoutRule runat="server" StartGroup="True" ControlSize="XXXS" LabelsWidth="S" GroupCaption="New Request For Information" />
            <px:PXTextEdit ID="edRFISummary" runat="server" DataField="RequestForInformationOutlook.Summary" />
            <px:PXSelector ID="edProjectId" runat="server" DataField="RequestForInformationOutlook.ProjectId" AutoRefresh="True" SimpleSelect="True" CommitChanges="True" />
            <px:PXSelector ID="edContactId" runat="server" DataField="RequestForInformationOutlook.ContactId" SimpleSelect="True" AutoRefresh="True" />
            <px:PXCheckBox ID="edIncoming" runat="server" DataField="RequestForInformationOutlook.Incoming" CommitChanges="True" />
            <px:PXDropDown ID="edRFIStatus" runat="server" DataField="RequestForInformationOutlook.Status" CommitChanges="True" />
            <px:PXSelector ID="edRFIClassId" runat="server" DataField="RequestForInformationOutlook.ClassId" CommitChanges="True" AutoRefresh="True" SimpleSelect="True" />
            <px:PXSelector ID="edRFIPriorityId" runat="server" DataField="RequestForInformationOutlook.PriorityId" CommitChanges="True" DisplayMode="Text" SimpleSelect="True" AutoRefresh="True" />
            <px:PXSelector ID="edRFIOwnerID" runat="server" DataField="RequestForInformationOutlook.OwnerID" AutoRefresh="True" SimpleSelect="True" />
            <px:PXDateTimeEdit ID="edDueResponseDate" runat="server" DataField="RequestForInformationOutlook.DueResponseDate" CommitChanges="True" />
            <px:PXCheckBox ID="edIsScheduleImpact" runat="server" DataField="RequestForInformationOutlook.IsScheduleImpact" CommitChanges="True" />
            <px:PXNumberEdit ID="edScheduleImpact" runat="server" DataField="RequestForInformationOutlook.ScheduleImpact" />
            <px:PXCheckBox ID="edIsCostImpact" runat="server" DataField="RequestForInformationOutlook.IsCostImpact" CommitChanges="True" />
            <px:PXNumberEdit ID="edCostImpact" runat="server" DataField="RequestForInformationOutlook.CostImpact" />
            <px:PXCheckBox ID="edDesignChange" runat="server" DataField="RequestForInformationOutlook.DesignChange" />

            <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="New Project Issue" ControlSize="XXXS" LabelsWidth="S" />
            <px:PXTextEdit ID="edPISummary" runat="server" DataField="ProjectIssueOutlook.Summary" />
            <px:PXSelector ID="edPIProjectId" runat="server" DataField="ProjectIssueOutlook.ProjectId" SimpleSelect="True" CommitChanges="True" AutoRefresh="True" />
            <px:PXSelector ID="edPIClassId" runat="server" DataField="ProjectIssueOutlook.ClassId" CommitChanges="True" AutoRefresh="True" SimpleSelect="True" />
            <px:PXSelector ID="edPIPriority" runat="server" DataField="ProjectIssueOutlook.PriorityId" CommitChanges="True" SimpleSelect="True" DisplayMode="Text" AutoRefresh="True" />
            <px:PXSelector ID="edPIOwner" runat="server" DataField="ProjectIssueOutlook.OwnerID" AutoRefresh="True" SimpleSelect="True" />
            <px:PXDateTimeEdit ID="edPIDueDate" runat="server" DataField="ProjectIssueOutlook.DueDate" CommitChanges="True" />

            <px:PXLayoutRule runat="server" GroupCaption="Log Activity" StartGroup="True" ControlSize="XXXS" LabelsWidth="S" />            
            <px:PXTextEdit ID="edActivitySummary"  runat="server" DataField="NewActivity.Subject" Width="190px" />  
            <px:PXSelector ID="edActivityCaseCD" runat="server" DataField="NewActivity.CaseCD" SimpleSelect="True" AutoRefresh="true" />            
			<px:PXSelector ID="edActivityOpportunityCD" runat="server" DataField="NewActivity.OpportunityID" SimpleSelect="True" AutoRefresh="true" />
            <px:PXSelector ID="edActivityContractID" runat="server" DataField="NewActivity.ProjectId" AutoRefresh="true" SimpleSelect="True" CommitChanges="True" />
            <px:PXSelector ID="edActivityRequestForInformationId" runat="server" DataField="NewActivity.RequestForInformationId" AutoRefresh="true" SimpleSelect="True" CommitChanges="True" />
            <px:PXSelector ID="edActivityProjectIssueId" runat="server" DataField="NewActivity.ProjectIssueId" AutoRefresh="true" SimpleSelect="True" CommitChanges="True" />           
            <px:PXCheckBox Id="chkLinkContact" runat="server" DataField="NewActivity.IsLinkContact" CommitChanges="true" Width="120px"/>
            <px:PXCheckBox Id="chkLinkCase" runat="server" DataField="NewActivity.IsLinkCase" CommitChanges="true" Width="120px"/>
			<px:PXCheckBox Id="chkLinkOpportunity" runat="server" DataField="NewActivity.IsLinkOpportunity" CommitChanges="true" Width="120px"/>
            <px:PXCheckBox ID="chkIsLinkProject" runat="server" DataField="NewActivity.IsLinkProject" CommitChanges="true" Width="180px" />
            <px:PXCheckBox ID="chkIsLinkRequestForInformation" runat="server" DataField="NewActivity.IsLinkRequestForInformation" CommitChanges="true" Width="180px" />
            <px:PXCheckBox ID="chkIsLinkProjectIssue" runat="server" DataField="NewActivity.IsLinkProjectIssue" CommitChanges="true" Width="180px" />
			<px:PXLayoutRule runat="server" GroupCaption=" " StartGroup="True" ControlSize="M" LabelsWidth="S" /> 

			<px:PXLabel ID="edFake" runat="server" DataField="Fake" Height="10px"></px:PXLabel>
			<px:PXLayoutRule runat="server" LabelsWidth="XS" ControlSize="M" SuppressLabel="True"/>
            
            <px:PXCheckBox ID="edRecognitionInProgress" runat="server" DataField="IsRecognitionInProgress" Style="display: none;" />
            <px:PXLayoutRule Merge="true" runat="server" ControlSize="XM" SuppressLabel="true"/>
            <px:PXButton ID="edRefreshAPDoc" runat="server" BackColor="Transparent"
                Style="min-width: 10px; width: 25px; border-style: none; margin-left: 10px; padding: 0px;" CssClass="ms-Button">
				<Images Normal="~/Icons/spinnerSmall.gif" />
                <ClientEvents Click="refreshButtonClick" />
			</px:PXButton>
            <px:PXLabel ID="edRefreshText" runat="server" Style="padding-top: 4px;">AP Document recognition is in progress...</px:PXLabel>
            <px:PXLayoutRule Merge="false" runat="server" LabelsWidth="XS" ControlSize="M" SuppressLabel="true"/>
            <px:PXLayoutRule Merge="true" runat="server" ControlSize="L" SuppressLabel="true" />
            <px:PXCheckBox ID="edSuccessRecognition" runat="server" DataField="NumOfRecognizedDocumentsCheck" AlignLeft="true" RenderStyle="Button"
                Style="margin-top: 4px;">
                <CheckImages Normal="main@Success" />
            </px:PXCheckBox>
            <px:PXTextEdit runat="server" ID="edNumOfRecognizedDocuments" DataField="NumOfRecognizedDocuments" SkinID="Label" Width="240px" TextAlign="Left" />
            <px:PXLayoutRule Merge="false" runat="server" LabelsWidth="XS" ControlSize="M" SuppressLabel="true" />

            <px:PXButton ID="edCreateAPDoc" runat="server" Width="100%" CssClass="ms-Button" CommandName="createAPDoc" CommandSourceID="ds">
                <ClientEvents Click="createAPDocButtonClick" />
            </px:PXButton>
            <px:PXButton ID="edViewAPDoc" runat="server" Width="100%" CssClass="ms-Button" CommandName="viewAPDoc" CommandSourceID="ds">
                <ClientEvents Click="viewAPDocButtonClick" />
            </px:PXButton>

			<px:PXButton ID="edCreateActivity" runat="server" CommandName="createActivity" CommandSourceID="ds" Width="100%" Hidden="true" CssClass="ms-Button"/>
			<px:PXButton ID="edCreateOpportunity" runat="server" CommandName="createOpportunity" CommandSourceID="ds" Width="100%" Hidden="true" CssClass="ms-Button"/>
			<px:PXButton ID="edCreateCase" runat="server" CommandName="createCase" CommandSourceID="ds" Width="100%" Hidden="true" CssClass="ms-Button"/>
			<px:PXButton ID="edCreateLead" runat="server" CommandName="createLead" CommandSourceID="ds" Width="100%" Hidden="true" CssClass="ms-Button"/>                
			<px:PXButton ID="edCreateContact" runat="server" CommandName="createContact" CommandSourceID="ds" Width="100%" Hidden="true" CssClass="ms-Button"/>
            <px:PXButton ID="edCreateRequestForInformation" runat="server" CommandName="createRequestForInformation" CommandSourceID="ds" Width="100%" Hidden="True" CssClass="ms-Button" />
            <px:PXButton ID="edCreateProjectIssue" runat="server" CommandName="createProjectIssue" CommandSourceID="ds" Width="100%" Hidden="True" CssClass="ms-Button" />

            <px:PXLayoutRule runat="server" LabelsWidth="XS" ControlSize="M" SuppressLabel="True" />
            <px:PXButton ID="edGoCreateLead" runat="server" CommandName="goCreateLead" CommandSourceID="ds" Width="100%" Hidden="true" CssClass="ms-Button"/>
            <px:PXButton ID="edGoCreateContact" runat="server" CommandName="goCreateContact" CommandSourceID="ds" Width="100%" Hidden="true" CssClass="ms-Button"/>
            <px:PXButton ID="edViewContact" runat="server" CommandName="viewContact" CommandSourceID="ds" Width="100%" Hidden="true" CssClass="ms-Button"/>
            <px:PXButton ID="edViewBAccount" runat="server" CommandName="viewBAccount" CommandSourceID="ds" Width="100%" Hidden="true" CssClass="ms-Button"/>
            <px:PXButton ID="edViewEntity" runat="server" CommandName="viewEntity" CommandSourceID="ds" Width="100%" Hidden="true" CssClass="ms-Button"/>  
            <px:PXButton ID="edGoCreateActivity" runat="server" CommandName="goCreateActivity" CommandSourceID="ds" Width="100%" Hidden="true" CssClass="ms-Button" />      
            <px:PXButton ID="edGoCreateCase" runat="server" CommandName="goCreateCase" CommandSourceID="ds" Width="100%" Hidden="true" CssClass="ms-Button"/>
			<px:PXButton ID="edGoCreateOpportunity" runat="server" CommandName="goCreateOpportunity" CommandSourceID="ds" Width="100%" Hidden="true" CssClass="ms-Button"/>
            <px:PXButton ID="edRedirectToCreateRequestForInformation" runat="server" CommandName="redirectToCreateRequestForInformation" CommandSourceID="ds" CssClass="ms-Button" Width="100%" />
            <px:PXButton ID="edRedirectToCreateProjectIssue" runat="server" CommandName="redirectToCreateProjectIssue" CommandSourceID="ds" CssClass="ms-Button" Width="100%" />
            <px:PXButton ID="edReply" runat="server" CommandName="reply" CommandSourceID="ds" Width="100%" Hidden="true" CssClass="ms-Button"/>
            <px:PXButton ID="edLogout" runat="server" CommandName="logOut" CommandSourceID="ds" Width="100%" Hidden="true" CssClass="ms-Button"/>

			<px:PXLabel ID="edFake2" runat="server" />

            <px:PXLabel ID="edPanelCaption" runat="server" Style="display: none">Select a document:</px:PXLabel>
            <px:PXSmartPanel ID="edAttachmentsPanel" runat="server" OnBeforeLoadContent="edAttachmentsPanel_LoadContent" SkinID="Transparent" LoadOnDemand="true"
                AutoReload="true" ShowAfterLoad="true" AutoRepaint="true" RenderVisible="true" Width="100%" Height="100%">
            </px:PXSmartPanel>

            <px:PXButton ID="edCreateAPDocContinue" runat="server" CommandName="createAPDocContinue" CommandSourceID="ds" Width="100%" CssClass="ms-Button"
                Style="width: 270px;">
                <ClientEvents Click="createAPDocContinueButtonClick" />
            </px:PXButton>
            <px:PXButton ID="edViewAPDocContinue" runat="server" CommandName="viewAPDocContinue" CommandSourceID="ds" Width="100%" CssClass="ms-Button"
                Style="width: 270px;">
            </px:PXButton>
        </Template>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
        <ClientEvents Initialize="formInit" AfterRepaint="afterFormRepaint" />
    </px:PXFormView>
    <script type="text/javascript">
        let isViewAPDocOperation = false;
        let dataSource = null;
        let form = null;
        let attachmentsCount = null;
        let tokenRefreshingInterval = null;
        let formRefreshingInterval = null;
        const tokenRefreshTime = 60000;

    	var officeContext = false;
    	var officeInitRun = false;

    	if (window.Office != null)
    	{
    		// main frame load event handler
    		Office.initialize = function ()
    		{
    			officeInitRun = true;
    			var message = Office.context.mailbox.item;
                if (message == null) return;
    			px.elemByID("__ouIsIncome").value = "1";
    			px.elemByID("__ouEmail").value = message.sender.emailAddress;
				px.elemByID("__ouDisplayName").value = message.sender.displayName;
				px.elemByID("__ouFirstName").value = getFirstName(message.sender.displayName);
				px.elemByID("__ouLastName").value = getLastName(message.sender.displayName);
    			if (message.sender.emailAddress == "" ||
						message.sender.emailAddress == Office.context.mailbox.userProfile.emailAddress)
    			{
    				px.elemByID("__ouIsIncome").value = "0";
					px.elemByID("__ouDisplayName").value = "";
					px.elemByID("__ouFirstName").value = "";
					px.elemByID("__ouLastName").value = "";
                }

                setTimeout(function () {
                    if (window.pageLoaded) initWindow();
                    else __px_cm(window).registerAfterLoad(initWindow);
                }, 0);
    		};
    		officeContext = true;
    	}

        function initDataSource(ds) {
            dataSource = ds;
        }

    	function execReloadPage()
    	{
    		var primaryForm = px_alls["form"];
            if (primaryForm) {
                primaryForm.refresh();
            }
    	}

    	function initWindow()
        {
            saveMessage(execReloadPage);

            if (attachmentsCount && attachmentsCount > 0) {
                setTimeout(startTokenRefreshing(), tokenRefreshTime);
            }
    	}

        function saveMessage(completeCallback)
        {
            var message = Office.context.mailbox.item;
            if (message == null)
            {
                px.elemByID("__oumsMessageId").value = null;
                return;
            }                        
            px.elemByID("__oumsMessageId").value = message.internetMessageId;
            px.elemByID("__oumsItemId").value = message.itemId;
            px.elemByID("__oumsSubject").value = message.subject;
            px.elemByID('__oumsFrom').value = serializeAddressItem(message.from);
            px.elemByID("__oumsTo").value = serializeAddressList(message.to);
            px.elemByID("__oumsCC").value = serializeAddressList(message.cc);
            px.elemByID("__oumsEwsUrl").value = Office.context.mailbox.ewsUrl;

            let pdfAttachments = getAttachments(message.attachments);
            attachmentsCount = pdfAttachments.length;
            px.elemByID('__ouAttachmentsCount').value = pdfAttachments.length;
            px.elemByID('__ouAttachmentNames').value = serializeAttachmentNames(pdfAttachments);

            getAsyncAttachmentToken(completeCallback, true);
        }

        function serializeAttachmentNames(pdfAttachments) {
            let attachmentNames = '';

            pdfAttachments.forEach(function (a) {
                attachmentNames += a.name + ';';
            });

            return attachmentNames;
        }

        function getAttachments(attachments) {
            let pdfAttachments = attachments.filter(function (a) {
                return a.contentType === 'application/pdf' ||
                    a.contentType === 'application/x-pdf' ||
                    a.contentType === 'application/octet-stream' &&
                    a.name.toLowerCase().lastIndexOf('.pdf') === a.name.length - 4;
            });

            return pdfAttachments;
        }

        function getAsyncAttachmentToken(completeCallback, showError) {
            let token = px.elemByID('__oumsToken').value;
            let isEmptyToken = token == '' || token === 'none';

            if (isEmptyToken) {
                try {
                    Office.context.mailbox.getCallbackTokenAsync(
                        function (asyncResult) {
                            if (asyncResult.status === 'succeeded') {
                                px.elemByID('__oumsToken').value = asyncResult.value;
                                completeCallback();
                            }
                            else {
                                px.elemByID('__oumsToken').value = 'none';
                                if (showError === true) {
                                    document.body.insertBefore(document.createTextNode('Get Token error. Status = : ' + asyncResult.status), document.body.firstChild);
                                }
                                completeCallback();
                            }
                        });
                } catch (ex) {
                    px.elemByID('__oumsToken').value = "none";
                    if (showError === true) {
                        document.body.insertBefore(document.createTextNode('Error occured: ' + ex), document.body.firstChild);
                    }
                    completeCallback();
                }
            }
            else {
                completeCallback();
            }

        };

        function serializeAddressList(list)
        {            
            var msg = "";
            if(list!=null)
                list.forEach(function (recip, index)
                {
                    msg = msg + serializeAddressItem(recip);
                });
            return msg;
		}

        function serializeAddressItem(item) {
            return "\"" + item.displayName + "\"" + " <" + item.emailAddress + ">;";
        }

		function getFirstName(displayName)
		{
			displayName = displayName.trim();
			while (displayName.indexOf("  ") > -1)
				displayName = displayName.replace("  ", " ");
			names = displayName.split(" ");
			firstName = names.length > 1 ? names[0] : null;
			return firstName;
		}

		function getLastName(displayName)
		{
			displayName = displayName.trim();
			while (displayName.indexOf("  ") > -1)
				displayName = displayName.replace("  ", " ");
			names = displayName.split(" ");
			lastName = names.length > 1 ? names[names.length - 1] : names[0];
			return lastName;
        }

        function formInit(sender) {
            form = sender;
            handleRecognitionVisibility();
        }

        function refreshButtonClick() {
            refreshDuringRecognition();
        }

        function afterFormRepaint() {
            handleRecognitionVisibility();
        }

        function handleRecognitionVisibility() {
            let isRecognitionInProgressControl = px_all[isRecognitionInProgressId];
            let hidden = !isRecognitionInProgressControl || !isRecognitionInProgressControl.getValue();
            let display = hidden ? 'none' : '';

            let refreshButton = document.getElementById(refreshButtonId);
            if (refreshButton) {
                refreshButton.style.display = display;
            }

            let refreshText = document.getElementById(refreshTextId);
            if (refreshText) {
                refreshText.style.display = display;
            }
        }

        function createAPDocButtonClick() {
            isViewAPDocOperation = false;

            if (attachmentsCount === 1) {
                startFormRefreshing();
                return;
            }

            changeVisibilityForCreateAPDoc(false);
        }

        function viewAPDocButtonClick() {
            isViewAPDocOperation = true;

            if (attachmentsCount === 1) {
                return;
            }

            changeVisibilityForCreateAPDoc(false);
        }

        function createAPDocContinueButtonClick() {
            startFormRefreshing();
            showControlsForCreateAPDoc();
        }

        function commandPerformed(ds, args) {
            if (args.command !== 'createAPDoc' && args.command !== 'viewAPDoc') {
                return;
            }

            if (attachmentsCount === 1) {
                return;
            }

            changeVisibilityForCreateAPDoc(false);

            let panel = px_all[attachmentsPanelId];
            panel.loaded = false;
            panel.load();
        }

        function showControlsForCreateAPDoc() {
            changeVisibilityForCreateAPDoc(true);

            let panel = px_all[attachmentsPanelId];
            panel.hide();
        }

        function changeVisibilityForPanelCaption(visible) {
            let panelCaption = document.getElementById(panelCaptionId);
            if (panelCaption !== null) {
                panelCaption.style.display = visible ? '' : 'none';
            }
        }

        function changeVisibilityForCreateAPDoc(visible) {
            let personTitle = document.getElementById(personTitleId);
            if (personTitle !== null) {
                personTitle.style.display = visible ? '' : 'none';
            }

            let edHelp = document.getElementById(helpId);
            if (edHelp !== null) {
                edHelp.style.display = visible ? '' : 'none';
            }

            let label = document.getElementById(label1Id);
            if (label !== null) {
                label.style.display = visible ? "" : "none";
            }

            let label2 = document.getElementById(label2Id);
            if (label2 !== null) {
                label2.style.display = visible ? '' : 'none';
            }

            let label3 = document.getElementById(label3Id);
            if (label3 !== null) {
                label3.style.display = visible ? '' : 'none';
            }

            let startGroup = document.getElementById('ctl00_phF_form_s0_s8');
            if (startGroup !== null) {
                startGroup.style.borderTop = visible ? '' : '0px';

                let legend = startGroup.querySelector('legend');
                if (legend !== null) {
                    legend.style.height = visible ? '' : '0px';
                }
            }

            changeVisibilityForPanelCaption(!visible);
        }

        // Clear other checkboxes if a file is selected
        // For View Document operation only
        function onFileSelect(checkbox) {
            if (isViewAPDocOperation === false) {
                return;
            }

            if (checkbox.getChecked() !== true) {
                return;
            }

            let panel = px_all[attachmentsPanelId];

            for (var item in __px_all(panel)) {
                var ctrl = __px_all(panel)[item];
                var i = ctrl.ID.indexOf(panel.ID + '_');

                if (!ctrl || i !== 0 || ctrl.__className !== 'PXCheckBox' || ctrl === checkbox) {
                    continue;
                }
                ctrl.setChecked(false);
            }
        }

        function startTokenRefreshing() {
            if (tokenRefreshingInterval !== null) {
                return;
            }

            tokenRefreshingInterval = setInterval(function () {
                px.elemByID('__oumsToken').value = '';
                getAsyncAttachmentToken(execReloadPage, false);
            }, tokenRefreshTime);
        }

        function stopTokenRefreshing() {
            if (tokenRefreshingInterval === null) {
                return;
            }

            clearInterval(tokenRefreshingInterval);
            tokenRefreshingInterval = null;
        }

        function startFormRefreshing() {
            if (formRefreshingInterval !== null) {
                return;
            }

            let firstRun = true;
            formRefreshingInterval = setInterval(function () {
                try {
                    if (firstRun === true) {
                        let refreshButton = document.getElementById(refreshButtonId);
                        let isRefreshButtonVisible = refreshButton && refreshButton.style.display !== 'none';

                        firstRun = isRefreshButtonVisible;
                        refreshDuringRecognition();
                    }
                    else {
                        if (getRecognitionFinished() === true) {
                            stopFormRefreshing();
                        }
                        else {
                            refreshDuringRecognition();
                        }
                    }
                } catch (e) {
                    stopFormRefreshing();
                }
            }, 3000);

        }

        function getRecognitionFinished() {
            let refreshButton = document.getElementById(refreshButtonId);
            let isRefreshButtonVisible = refreshButton && refreshButton.style.display !== 'none';
            let isRecognitionFinished = isRefreshButtonVisible !== true;

            return isRecognitionFinished;
        }

        function refreshDuringRecognition() {
            form.refresh();

            let panel = px_all[attachmentsPanelId];
            let panelCaption = document.getElementById(panelCaptionId);
            if (panel && panelCaption && panelCaption.style.display != 'none') {
                panel.loaded = false;
                panel.load();
            }
        }

        function stopFormRefreshing() {
            if (formRefreshingInterval === null) {
                return;
            }

            clearInterval(formRefreshingInterval);
            formRefreshingInterval = null;
        }
    </script>
</asp:Content>
