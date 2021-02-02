using PX.Objects.CR;
using PX.Objects.CS;

namespace PX.Objects.FS
{
    public static class ContactAddressHelper
    {
        public static IContact GetIContact(Contact source)
        {
            if (source == null)
            {
                return null;
            }

            var dest = new CRContact();

            dest.BAccountID = source.BAccountID;
            dest.RevisionID = source.RevisionID;
            dest.IsDefaultContact = false;
            dest.FullName = source.FullName;
            dest.Salutation = source.Salutation;
            dest.Title = source.Title;
            dest.Phone1 = source.Phone1;
            dest.Phone1Type = source.Phone1Type;
            dest.Phone2 = source.Phone2;
            dest.Phone2Type = source.Phone2Type;
            dest.Phone3 = source.Phone3;
            dest.Phone3Type = source.Phone3Type;
            dest.Fax = source.Fax;
            dest.FaxType = source.FaxType;
            dest.Email = source.EMail;
            dest.NoteID = null;

            dest.Attention = source.Attention;

            return dest;
        }

        public static Contact GetContact(IContact source)
        {
            if (source == null)
            {
                return null;
            }

            var dest = new Contact();

            dest.BAccountID = source.BAccountID;
            dest.RevisionID = source.RevisionID;
            dest.FullName = source.FullName;
            dest.Salutation = source.Salutation;
            dest.Title = source.Title;
            dest.Phone1 = source.Phone1;
            dest.Phone1Type = source.Phone1Type;
            dest.Phone2 = source.Phone2;
            dest.Phone2Type = source.Phone2Type;
            dest.Phone3 = source.Phone3;
            dest.Phone3Type = source.Phone3Type;
            dest.Fax = source.Fax;
            dest.FaxType = source.FaxType;
            dest.EMail = source.Email;
            dest.NoteID = null;

            dest.Attention = source.Attention;

            return dest;
        }

        public static void CopyContact(IContact dest, IContact source)
        {
            CS.ContactAttribute.CopyContact(dest, source);

            //Copy fields that are missing in the previous method
            dest.Attention = source.Attention;
        }

        public static void CopyContact(IContact dest, Contact source)
        {
            CopyContact(dest, GetIContact(source));
        }

        public static IAddress GetIAddress(Address source)
        {
            if (source == null)
            {
                return null;
            }

            var dest = new CRAddress();

            dest.BAccountID = source.BAccountID;
            dest.RevisionID = source.RevisionID;
            dest.IsDefaultAddress = false;
            dest.AddressLine1 = source.AddressLine1;
            dest.AddressLine2 = source.AddressLine2;
            dest.AddressLine3 = source.AddressLine3;
            dest.City = source.City;
            dest.CountryID = source.CountryID;
            dest.State = source.State;
            dest.PostalCode = source.PostalCode;

            dest.IsValidated = source.IsValidated;

            return dest;
        }

        public static Address GetAddress(IAddress source)
        {
            if (source == null)
            {
                return null;
            }

            var dest = new Address();

            dest.BAccountID = source.BAccountID;
            dest.RevisionID = source.RevisionID;
            dest.AddressLine1 = source.AddressLine1;
            dest.AddressLine2 = source.AddressLine2;
            dest.AddressLine3 = source.AddressLine3;
            dest.City = source.City;
            dest.CountryID = source.CountryID;
            dest.State = source.State;
            dest.PostalCode = source.PostalCode;

            dest.IsValidated = source.IsValidated;

            return dest;
        }

        public static void CopyAddress(IAddress dest, IAddress source)
        {
            AddressAttribute.Copy(dest, source);

            //Copy fields that are missing in the previous method
            dest.IsValidated = source.IsValidated;
        }

        public static void CopyAddress(IAddress dest, Address source)
        {
            CopyAddress(dest, GetIAddress(source));
        }
    }
}
