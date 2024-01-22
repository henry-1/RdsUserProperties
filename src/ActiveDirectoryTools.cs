using System;
using System.DirectoryServices;
using System.Linq;

namespace RdsUserProperties
{
    #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public static class ActiveDirectoryTools
    #pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {

        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static string GetDistinguishedName(string Identity)
        #pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            if (Identity.StartsWith("CN="))
            {
                return Identity;
            }

            var filter = string.Format("(&(ObjectCategory=user)(ObjectClass=person)(sAMAccountName={0}))", Identity);


            if (Identity.Contains('@'))
            {
                filter = string.Format("(&(ObjectCategory=user)(ObjectClass=person)(userPrincipalName={0}))", Identity);
            }
            else if (Identity.Contains('\\'))
            {
                var accountName = Identity.Split('\\')[1];
                filter = string.Format("(&(ObjectCategory=user)(ObjectClass=person)(sAMAccountName={0}))", accountName);
            }

            var currentDomain = string.Format("DC={0}", (System.DirectoryServices.ActiveDirectory.Domain.GetCurrentDomain()).Name.Replace(".", ",DC="));
            var searchRootPath = string.Format("GC://{0}", currentDomain);
            var searchRoot = new DirectoryEntry(searchRootPath);

            var ds = new DirectorySearcher
            {
                Filter = filter,
                PageSize = 1,
                SearchRoot = searchRoot
            };

            var user = ds.FindOne();
            if (user is null)
            {
                Exception exception = new Exception("Unable to find user.");
                throw exception;
            }

            return user.Properties["adsPath"].Cast<string>().FirstOrDefault().Replace("GC://", "");
        }

        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static string GetDomainName(string Identity)
        #pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            var dcPartIndex = Identity.IndexOf("DC=");
            return Identity
                .Substring(dcPartIndex, Identity.Length - dcPartIndex)
                .Replace("DC=", "")
                .Replace(",", ".");
        }

        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static string GetDcName(string DomainName)
        #pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            var contextType = new System.DirectoryServices.ActiveDirectory.DirectoryContextType();
            var context = new System.DirectoryServices.ActiveDirectory.DirectoryContext(contextType, DomainName);
            return System.DirectoryServices.ActiveDirectory.Domain.GetDomain(context).FindDomainController().Name;
        }

    }
}
