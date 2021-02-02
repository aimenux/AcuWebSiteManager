<%@ Page Language="C#" MasterPageFile="~/MasterPages/TabView.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="OU201000.aspx.cs" Inherits="Page_OU201000"
    Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/TabView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">       
    <px:PXDataSource ID="ds" runat="server" Width="100%" PrimaryView="Filter"
        TypeName=" PX.Objects.CR.OUSearchMaint" Visible="false" PageLoadBehavior="InsertRecord">
        <CallbackCommands>
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="createLead" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="createContact" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="createCase" />            
            <px:PXDSCallbackCommand CommitChanges="True" Name="createOpportunity" /> 
            <px:PXDSCallbackCommand CommitChanges="True" Name="createActivity" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="logOut" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" Width="100%" Height="643px" DataSourceID="ds" DataMember="Filter" SkinID="Transparent" OnDataBound="form_OnDataBound">
        <Template>                         
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="XM"/>
			<px:PXButton ID="edBack" runat="server" CommandName="back" AlignLeft="true" CommandSourceID="ds" Hidden="true" CssClass="ms-Button" Style="margin-bottom: 15px;"/>

			<px:PXLabel ID="personTitle" runat="server" Text="Person:" Style="padding: 0; margin: 0 0 -10px 0;" />
			<px:PXLayoutRule Merge="True" runat="server"  ControlSize="XM" SuppressLabel="true"/> 
			<px:PXSelector ID="edContact" runat="server" CommitChanges="True" DataField="ContactID" DisplayMode="Text" SimpleSelect="True" SuppressLabel="true" Style="margin-left: -10px; width: 240px;"/>
			<px:PXButton ID="edHelp" runat="server" Target="_blank" BackColor="Transparent"
				Style="min-width:10px; width:25px; border-style: none; margin-left:-0" ToolTip="Help" AlignLeft="false"
				NavigateUrl="../../Main?ScreenId=ShowWiki&pageid=875b6f76-2820-4ea9-a420-7f7a8bbfb619">
				<Images Normal="~/Icons/OutlookHelp.png" />
			</px:PXButton>
			<px:PXLayoutRule Merge="False" runat="server" />

			<px:PXLayoutRule runat="server" StartGroup="True" ControlSize="XXXS" LabelsWidth="S" />
            <px:PXTextEdit ID="edSEmail" runat="server" DataField="Email" AllowEdit="False" SkinID="BlueLabel" SuppressLabel="true" Style="color: #0078d7; width: 100%; border: none;" Enabled="false"/>            
            <px:PXDropDown ID="edOutgoingEmail" runat="server" CommitChanges="true" Hidden="true" AutoRefresh="true" DataField="OutgoingEmail" />
<px:PXTextEdit ID="edErrorMessage" runat="server" TextMode="MultiLine" DataField="ErrorMessage" AllowEdit="False" SuppressLabel="true" SkinID="Label" Height="50px" Style="color: red; min-width:290px; overflow:hidden; margin-top: 10px;" />
            <px:PXLabel ID="PXLabel1" runat="server" DataField="Fake" Height="10px"></px:PXLabel>


            <px:PXLayoutRule runat="server" GroupCaption="Info" StartGroup="True" ControlSize="XXXS" LabelsWidth="S" />
			<px:PXTextEdit ID="PXTextEdit3" runat="server" DataField="NewContactFirstName" Width="100%" />
			<px:PXTextEdit ID="PXTextEdit4" runat="server" DataField="NewContactLastName" Width="100%" />
			<px:PXTextEdit ID="PXTextEdit1" runat="server" DataField="NewContactEmail" Width="100%" />
            <px:PXTextEdit ID="edPosition" runat="server" DataField="Salutation" Width="100%" />
            <px:PXSelector ID="edBAccountID" runat="server" CommitChanges="True" DataField="BAccountID" SimpleSelect="True"/>
            <px:PXTextEdit ID="edCompany" runat="server" DataField="FullName" Width="100%"/>            
            <px:PXDropDown ID="edSource" runat="server" DataField="Source" SimpleSelect="True" />
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
            
            <px:PXLayoutRule runat="server" GroupCaption="Log Activity" StartGroup="True" ControlSize="XXXS" LabelsWidth="S" />            
            <px:PXTextEdit ID="edActivitySummary"  runat="server" DataField="NewActivity.Subject" Width="190px" />  
            <px:PXSelector ID="edActivityCaseCD" runat="server" DataField="NewActivity.CaseCD" SimpleSelect="True" AutoRefresh="true" />            
            <px:PXSelector ID="edActivityOpportunityCD" runat="server" DataField="NewActivity.OpportunityID" SimpleSelect="True" AutoRefresh="true" />            
            <px:PXCheckBox Id="chkLinkContact" runat="server" DataField="NewActivity.IsLinkContact" CommitChanges="true" Width="120px"/>
            <px:PXCheckBox Id="chkLinkCase" runat="server" DataField="NewActivity.IsLinkCase" CommitChanges="true" Width="120px"/>
            <px:PXCheckBox Id="chkLinkOpportunity" runat="server" DataField="NewActivity.IsLinkOpportunity" CommitChanges="true" Width="120px"/>
			<px:PXLayoutRule runat="server" GroupCaption=" " StartGroup="True" ControlSize="M" LabelsWidth="S" /> 

			<px:PXLabel ID="edFake" runat="server" DataField="Fake" Height="10px"></px:PXLabel>
			<px:PXLayoutRule runat="server" LabelsWidth="XS" ControlSize="M" SuppressLabel="True"/>
			
			<px:PXButton ID="edCreateActivity" runat="server" CommandName="createActivity" CommandSourceID="ds" Width="100%" Hidden="true" CssClass="ms-Button"/>
			<px:PXButton ID="edCreateOpportunity" runat="server" CommandName="createOpportunity" CommandSourceID="ds" Width="100%" Hidden="true" CssClass="ms-Button"/>
			<px:PXButton ID="edCreateCase" runat="server" CommandName="createCase" CommandSourceID="ds" Width="100%" Hidden="true" CssClass="ms-Button"/>
			<px:PXButton ID="edCreateLead" runat="server" CommandName="createLead" CommandSourceID="ds" Width="100%" Hidden="true" CssClass="ms-Button"/>                
            <px:PXButton ID="edCreateContact" runat="server" CommandName="createContact" CommandSourceID="ds" Width="100%" Hidden="true" CssClass="ms-Button"/>

            <px:PXLayoutRule runat="server" LabelsWidth="XS" ControlSize="M" SuppressLabel="True" />
            <px:PXButton ID="edGoCreateLead" runat="server" CommandName="goCreateLead" CommandSourceID="ds" Width="100%" Hidden="true" CssClass="ms-Button"/>
            <px:PXButton ID="edGoCreateContact" runat="server" CommandName="goCreateContact" CommandSourceID="ds" Width="100%" Hidden="true" CssClass="ms-Button"/>
            <px:PXButton ID="edViewContact" runat="server" CommandName="viewContact" CommandSourceID="ds" Width="100%" Hidden="true" CssClass="ms-Button"/>
            <px:PXButton ID="edViewBAccount" runat="server" CommandName="viewBAccount" CommandSourceID="ds" Width="100%" Hidden="true" CssClass="ms-Button"/>
            <px:PXButton ID="edViewEntity" runat="server" CommandName="viewEntity" CommandSourceID="ds" Width="100%" Hidden="true" CssClass="ms-Button"/>  
            <px:PXButton ID="edGoCreateActivity" runat="server" CommandName="goCreateActivity" CommandSourceID="ds" Width="100%" Hidden="true" CssClass="ms-Button" />      
            <px:PXButton ID="edGoCreateCase" runat="server" CommandName="goCreateCase" CommandSourceID="ds" Width="100%" Hidden="true" CssClass="ms-Button"/>
            <px:PXButton ID="edGoCreateOpportunity" runat="server" CommandName="goCreateOpportunity" CommandSourceID="ds" Width="100%" Hidden="true" CssClass="ms-Button"/>
            <px:PXButton ID="edReply" runat="server" CommandName="reply" CommandSourceID="ds" Width="100%" Hidden="true" CssClass="ms-Button"/>
            <px:PXButton ID="edLogout" runat="server" CommandName="logOut" CommandSourceID="ds" Width="100%" Hidden="true" CssClass="ms-Button"/>

			<px:PXLabel runat="server" />
        </Template>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXFormView>
    <script type="text/javascript">
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
    			if (window.pageLoaded) initWindow();
    			else __px_cm(window).registerAfterLoad(initWindow);
    		};
    		officeContext = true;
    	}

    	function execReloadPage()
    	{
    		__px_cm(window).reloadPage();
    		var form = px_alls["form"];
    		if (form) form.refresh();
    	}

    	function initWindow()
    	{
    		saveMessage(execReloadPage);
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
            px.elemByID("__oumsTo").value = serializeAddressList(message.to);
            px.elemByID("__oumsCC").value = serializeAddressList(message.cc);
            px.elemByID("__oumsEwsUrl").value = Office.context.mailbox.ewsUrl;

            getAsyncAttachmentToken(completeCallback);
        }

        function getAsyncAttachmentToken(completeCallback) {
            if (px.elemByID("__oumsToken").value == "") {
                try {
                    Office.context.mailbox.getCallbackTokenAsync(
                    function (asyncResult) {
                        if (asyncResult.status === "succeeded") {
                            px.elemByID("__oumsToken").value = asyncResult.value;
                        }
                        else {
                            px.elemByID("__oumsToken").value = "none";
                            document.body.insertBefore(document.createTextNode('Get Token error. Status = : ' + asyncResult.status), document.body.firstChild);
                        }
                    });
                } catch (ex) {
                    px.elemByID("__oumsToken").value = "none";
                    document.body.insertBefore(document.createTextNode('Error occured: ' + ex), document.body.firstChild);
                }
            }
            completeCallback();
        };

        function serializeAddressList(list)
        {            
            var msg = "";
            if(list!=null)
                list.forEach(function (recip, index)
                {
                    msg = msg + "\"" + recip.displayName + "\"" + " <" + recip.emailAddress + ">;";
                });
            return msg;
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
    </script>
</asp:Content>
