using System.Text;
using PX.Data;
using PX.Objects.CR;

namespace PX.Objects.FS
{
    public partial class FSAppointment : PX.Data.IBqlTable
    {
        #region Private Methods
        /// <summary>
        /// Returns the set of contacts of a customer.
        /// </summary>
        private static PXResultset<Contact> ReturnsContactList(PXGraph graph, int? appointmentID)
        {
            FSServiceOrder fsServiceOrderRow = (FSServiceOrder)PXSelectJoin<FSServiceOrder,
                                                    InnerJoin<FSAppointment,
                                                        On<
                                                            FSServiceOrder.sOID, Equal<FSAppointment.sOID>,
                                                        And<
                                                            FSServiceOrder.srvOrdType, Equal<FSAppointment.srvOrdType>>>>,
                                                        Where<
                                                            FSAppointment.appointmentID, Equal<Required<FSAppointment.appointmentID>>>>
                                                    .Select(graph, appointmentID);

            if (fsServiceOrderRow == null || fsServiceOrderRow.CustomerID == null)
            {
                return null;
            }

            PXResultset<Contact> bqlResultSet = PXSelect<Contact,
                                               Where<
                                                   Contact.bAccountID, Equal<Required<Contact.bAccountID>>,
                                                And<
                                                    Contact.contactType, Equal<ContactTypesAttribute.person>,
                                                And<
                                                    Contact.isActive, Equal<True>>>>>
                                                .Select(graph, fsServiceOrderRow.CustomerID);
            return bqlResultSet;
        }

        /// <summary>
        /// Returns the contact names or cell phones separated by a coma.
        /// </summary>
        /// <param name="bqlResultSet">Set of customer contacts.</param>
        /// <param name="concatenateNames">Boolean that if true, returns the customer contact's name(s).</param>
        /// <param name="concatenateCells">Boolean that if true, returns the customer contact's cell phone(s).</param>
        private static StringBuilder ConcatenatesContactInfo(PXResultset<Contact> bqlResultSet, bool concatenateNames, bool concatenateCells)
        {
            StringBuilder names = new StringBuilder();
            int i = 0;            

            if (bqlResultSet.Count > 0)
            {
                foreach (Contact contactRow in bqlResultSet)
                {
                    i++;

                    if (bqlResultSet.Count > 1 && i == bqlResultSet.Count)
                    {
                        names.Append(PXMessages.LocalizeFormatNoPrefix(TX.Messages.LIST_LAST_ITEM_PREFIX));
                    }
                    else
                    {
                        if (names.Length != 0)
                        {
                            names.Append(", ");
                        }
                    }

                    if (string.IsNullOrEmpty(contactRow.FirstName) == false && concatenateNames == true)
                    {
                        names.Append(contactRow.FirstName.Trim());
                    }

                    if (string.IsNullOrEmpty(contactRow.LastName) == false && concatenateNames == true)
                    {
                        names.Append(' ');
                        names.Append(contactRow.LastName.Trim());
                    }

                    if (string.IsNullOrEmpty(contactRow.Phone1) == false && concatenateCells == true)
                    {
                        names.Append(contactRow.Phone1.Trim());
                    }
                    else if (concatenateCells == true)
                    {
                        names.Append(TX.Messages.NO_CONTACT_CELL_FOR_THE_CUSTOMER);
                    }
                }
            }
            else if (concatenateNames == true)
            {
                names.Append(TX.Messages.NO_CONTACT_FOR_THE_CUSTOMER);
            }
            else if (concatenateCells == true)
            {
                names.Append(TX.Messages.NO_CONTACT_CELL_FOR_THE_CUSTOMER);
            }

            return names;
        }

        /// <summary>
        /// Gets the employees contact info separated by a coma.
        /// </summary>
        /// <param name="graph">Graph to use.</param>
        /// <param name="concatenateNames">Boolean that if true, returns the Staff name(s).</param>
        /// <param name="concatenateCells">Boolean that if true, returns the Staff cell phone(s).</param>
        /// <param name="appointmentID">Appointment ID.</param>
        private static StringBuilder GetsEmployeesContactInfo(PXGraph graph, bool concatenateNames, bool concatenateCells, int? appointmentID)
        {
            StringBuilder names = new StringBuilder();

            int i = 0;

            // TODO SD-7612 this BQL is not currently retrieving contact info for vendors
            BqlCommand fsAppointmentEmployeeContactInfoBql =
                new Select5<FSAppointmentEmployee,
                    InnerJoin<BAccount,
                        On<
                            BAccount.bAccountID, Equal<FSAppointmentEmployee.employeeID>>,
                    InnerJoin<Contact,
                        On<
                            BAccount.defContactID, Equal<Contact.contactID>>>>,
                    Where<
                        FSAppointmentEmployee.appointmentID, Equal<Required<FSAppointmentEmployee.appointmentID>>>,
                    Aggregate<
                        GroupBy<FSAppointmentEmployee.employeeID>>,
                    OrderBy<
                        Asc<FSAppointmentEmployee.employeeID>>>();

            PXView fsAppointmentEmployeeContactInfoView = new PXView(graph, true, fsAppointmentEmployeeContactInfoBql);
            var fsAppointmentEmployeeSet = fsAppointmentEmployeeContactInfoView.SelectMulti(appointmentID);

            if (fsAppointmentEmployeeSet.Count > 0)
            {
                foreach (PXResult<FSAppointmentEmployee, BAccount, Contact> bqlResult in fsAppointmentEmployeeSet)
                {
                    FSAppointmentEmployee fsAppointmentEmployeeRow = (FSAppointmentEmployee)bqlResult;
                    BAccount bAccountRow = (BAccount)bqlResult;
                    Contact contactRow = (Contact)bqlResult;

                    i++;

                    if (fsAppointmentEmployeeSet.Count > 1 && i == fsAppointmentEmployeeSet.Count)
                    {
                        names.Append(PXMessages.LocalizeFormatNoPrefix(TX.Messages.LIST_LAST_ITEM_PREFIX));
                    }
                    else
                    {
                        if (names.Length != 0)
                        {
                            names.Append(", ");
                        }
                    }

                    if (fsAppointmentEmployeeRow.Type == BAccountType.EmployeeType)
                    {
                        if (string.IsNullOrEmpty(contactRow.FirstName) == false && concatenateNames == true)
                        {
                            names.Append(contactRow.FirstName.Trim());
                        }

                        if (string.IsNullOrEmpty(contactRow.LastName) == false && concatenateNames == true)
                        {
                            names.Append(' ');
                            names.Append(contactRow.LastName.Trim());
                        }
                    }
                    else if (fsAppointmentEmployeeRow.Type == BAccountType.VendorType)
                    {
                        if (string.IsNullOrEmpty(contactRow.FullName) == false && concatenateNames == true)
                        {
                            names.Append(contactRow.FullName.Trim());
                        }
                    }

                    if (string.IsNullOrEmpty(contactRow.Phone1) == false && concatenateCells == true)
                    {
                        names.Append(contactRow.Phone1.Trim());
                    }
                    else if (concatenateCells == true)
                    {
                        names.Append(TX.Messages.NO_CONTACT_CELL_FOR_THE_STAFF);
                    }
                }
            }
            else
            {
                names.Append(TX.Messages.NO_STAFF_ASSIGNED_FOR_THE_APPOINTMENT);
            }

            return names;
        }

        /// <summary>
        /// Gets the value of a WildCard field.
        /// </summary>
        /// <returns>Returns the string value of the field.</returns>
        private static string GetWildCardFieldValue(PXGraph graph, object objectRow, string fieldName)
        {
            #region FSAppointment fields

            // WildCard_AssignedEmployeesList
            if (fieldName.ToUpper() == typeof(FSAppointment.wildCard_AssignedEmployeesList).Name.ToUpper())
            {
                StringBuilder names = new StringBuilder();
                names = FSAppointment.GetsEmployeesContactInfo(graph, true, false, ((FSAppointment)objectRow).AppointmentID);

                return names.ToString();
            }
            else if (fieldName.ToUpper() == typeof(FSAppointment.wildCard_AssignedEmployeesCellPhoneList).Name.ToUpper())
            {
                // WildCard_AssignedEmployeesCellPhoneList
                StringBuilder names = new StringBuilder();
                names = GetsEmployeesContactInfo(graph, false, true, ((FSAppointment)objectRow).AppointmentID);
                return names.ToString();
            }
            else if (fieldName.ToUpper() == typeof(FSAppointment.wildCard_CustomerPrimaryContact).Name.ToUpper())
            {
                // WildCard_CustomerPrimaryContact
                StringBuilder names = new StringBuilder();

                PXResultset<Contact> bqlResultSet = ReturnsContactList(graph, ((FSAppointment)objectRow).AppointmentID);

                if (bqlResultSet == null)
                {
                    return names.ToString();
                }

                names = ConcatenatesContactInfo(bqlResultSet, true, false);
                return names.ToString();
            }
            else if (fieldName.ToUpper() == typeof(FSAppointment.wildCard_CustomerPrimaryContactCell).Name.ToUpper())
            {
                // WildCard_CustomerPrimaryContactCell
                StringBuilder names = new StringBuilder();

                PXResultset<Contact> bqlResultSet = ReturnsContactList(graph, ((FSAppointment)objectRow).AppointmentID);

                if (bqlResultSet == null)
                {
                    return names.ToString();
                }

                names = ConcatenatesContactInfo(bqlResultSet, false, true);
                return names.ToString();
            }

            #endregion

            return null;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Replace WildCards existing inside the "body" string. It assumes that all WildCards are between "((" and "))".
        /// </summary>
        public static void ReplaceWildCards(PXGraph graph, ref string body, object objectRow)
        {
            if (objectRow == null || string.IsNullOrEmpty(body))
            {
                return;
            }

            StringBuilder strBody = new StringBuilder(body);

            const string START_WILDCARD = "((";
            const string END_WILDCARD = "))";

            int indexBegin = strBody.ToString().IndexOf(START_WILDCARD);
            int indexEnd = strBody.ToString().IndexOf(END_WILDCARD);

            while (indexBegin != -1 && indexEnd != -1)
            {
                string completeFieldName = strBody.ToString(indexBegin + START_WILDCARD.Length, indexEnd - indexBegin - END_WILDCARD.Length);
                int indexDot = completeFieldName.IndexOf(".");
                string fieldName = completeFieldName.Substring(indexDot + 1, completeFieldName.Length - indexDot - 1);

                string fieldvalue = GetWildCardFieldValue(graph, objectRow, fieldName);
                if (fieldvalue != null)
                {
                    strBody = strBody.Remove(indexBegin, indexEnd + END_WILDCARD.Length - indexBegin);
                    strBody = strBody.Insert(indexBegin, fieldvalue);

                    indexEnd = indexBegin;
                }

                indexBegin = strBody.ToString().IndexOf(START_WILDCARD, indexBegin + START_WILDCARD.Length);
                indexEnd = strBody.ToString().IndexOf(END_WILDCARD, indexEnd + END_WILDCARD.Length);
            }

            body = strBody.ToString();
        }

        #endregion
    }
}
