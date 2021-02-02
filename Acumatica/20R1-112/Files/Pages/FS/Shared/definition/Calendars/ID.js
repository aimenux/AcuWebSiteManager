//ID Definition

var ID = {
    /*Dispatch Board Calendar*/
    /* General */
    mainContainer: 'mainContainer',
    mainTab: 'mainTab',
    mainTabPanel: 'mainTabPanel',
    employeesScheduler: 'employeesScheduler',
    employeesCalendar: 'employeesCalendar',
    serviceOrderGrid: 'serviceOrderGrid',
    unassignedAppointmentGrid: 'unassignedAppointmentGrid',

    datePicker: 'datePicker',
    datePickerHeader: 'datePickerHeader',

    newAppointment: 'newAppointment',
    appointmentInquiry: 'appointmentInquiry',

    searchField: 'searchField',
    searchFieldUnassigned: 'searchFieldUnassigned',
    soFilter: 'soFilter',
    epFilter: 'epFilter',
    soClearFilter: 'soClearFilter',
    epClearFilter: 'epClearFilter',

    validateByDispatch: 'validateByDispatch',
    clearValidation: 'clearValidation',

    confirmAppointment: 'confirmAppointment',
    unconfirmAppointment: 'unconfirmAppointment',

    /*Configuration Form*/
    configurationForm: 'configuration',
    timeSlot: 'timeSlot',
    schedulerOrientation: 'schedulerOrientation',

    /* Service Order Form Filter*/
    serviceOrderFormFilter: 'serviceOrderFormFilter',
    skillsCheckboxGroup: 'skillsCheckboxGroup',
    licensesCheckboxGroup: 'licensesCheckboxGroup',
    problemsCheckboxGroup: 'problemsCheckboxGroup',
    serviceTypesCheckboxGroup: 'serviceClassesCheckboxGroup',
    assignedEmployee: 'assignedEmployee',

    serviceOrderFilterButton: 'serviceOrderFilterButton',
    serviceOrderFilterCloseButton: 'serviceOrderFilterCloseButton',
    serviceOrderCloseButton: 'serviceOrderCloseButton',

    /* Unassigned Appointment Form Filter*/
    unassignedAppointmentFormFilter: 'unassignedAppointmentFormFilter',
    uaClearFilter: 'uaClearFilter',
    uaFilter: 'uaFilter',
    skillsCheckboxGroupUa: 'skillsCheckboxGroupUa',

    unassignedAppointmentFilterButton: 'unassignedAppointmentFilterButton',
    unassignedAppointmentCloseButton: 'unassignedAppointmentCloseButton',
    unassignedAppointmentFilterCloseButton: 'unassignedAppointmentFilterCloseButton',

    /*Employee Form Filter*/
    employeeFormFilter: 'employeeFormFilter',
    employeeFilterCombo: 'employeeFilterCombo',
    employeeNameFilterCombo: 'employeeNameFilterCombo',
    employeeSkillsCheckboxGroup: 'employeeSkillsCheckboxGroup',
    employeeLicensesCheckboxGroup: 'employeeLicensesCheckboxGroup',
    employeeGeoZonesCheckboxGroup: 'employeeGeoZonesCheckboxGroup',
    employeeServicesCheckboxGroup: 'employeeServicesCheckboxGroup',
    employeeDefinedSchedulerCheckboxGroup: 'employeeDefinedSchedulerCheckboxGroup',
    employeeFilterButton: 'employeeFilterButton',
    employeeFilterCloseButton: 'employeeFilterCloseButton',
    employeeCloseButton: 'employeeCloseButton',

     /*SrvOrdType Form*/
    srvOrdTypeForm: 'srvOrdTypeForm',
    srvOrdTypeFormOkButton: 'srvOrdTypeFormOkButton',
    srvOrdTypeFormCloseButton: 'srvOrdTypeFormCloseButton',
    srvOrdTypeFormCombo: 'srvOrdTypeFormCombo',

    /*Form Create Appointment*/
    tabCreateForm: 'tabCreateForm',
    employeesCreateTab: 'employeesCreateTab',
    equipmentsCreateTab: 'equipmentsCreateTab',
    appointmentForm: 'appointmentForm',
    shareAppointmentForm: 'shareAppointmentForm',
    emptyServiceCode: 'emptyServiceCode',
    servicesFormAppointment: 'servicesFormAppointment',

    /*Form Edit Appointment*/
    editAppointment: 'editAppointment',
    deleteAppointment: 'deleteAppointment',
    tabEditForm: 'tabEditForm',
    employeesTab: 'employeesTab',
    equipmentsTab: 'equipmentsTab',
    serviceCodeTab: 'serviceCodeTab',
    emptyServicesEditForm: 'emptyServicesEditForm',
    servicesEditForm: 'servicesEditForm',
    editButtonSave: 'editButtonSave',

    /*Body Appointment*/
    appointmentRefNbr: 'appointmentRefNbr',
    serviceOrderRefNbr: 'serviceOrderRefNbr',
    recordID: 'recordID',

    /*User Calendar*/
    employeeGlobalCombo: 'employeeGlobalCombo',
    employeeGlobalCheck: 'employeeGlobalCheck',

    /*Room Calendar*/
    branchLocationCombo: 'branchLocationCombo',

    /*Notifications Messages*/
    messageCloseButton: 'messageCloseButton',
    summaryLabelLink: 'summaryLabelLink',
    eventLabelLink: 'eventLabelLink',

    /*Calendar Preferences*/
    preferenceContainerGrid: 'preferencescontainergrid',
    previewAppointmentGrid: 'previewAppointmentGrid',
    inactiveGrid: 'inactiveGrid',
    activeGrid: 'activeGrid',
    previewPreferences: 'previewPreferences',
    appointmentPreview: 'appointmentPreview',
    previewContainer: 'previewContainer'
};

// Appointment Status
var AppointmentStatus = {
    scheduledBySystem: 'A',
    scheduled: 'S',
    canceled: 'X',
    completed: 'C',
    inprocess: 'P',
    closed: 'Z',
    onhold: 'H'
};


// Valid Appointment params
var AppointmentPageParams = [
    'RefNbr',
    'SrvOrdType',
    'SORefNbr',
    'RoomID',
    'ScheduledDateTimeBegin',
    'ScheduledDateTimeEnd'
];

// Appointment Status Name
var AppointmentStatusName = {
    confirmed: 'confirmed',
    validated: 'validated',
    scheduled: 'scheduled',
    canceled: 'canceled',
    completed: 'completed',
    inprocess: 'in process',
    closed: 'closed',
    onhold: 'on hold'
};