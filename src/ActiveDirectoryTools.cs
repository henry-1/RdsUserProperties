using System;
using System.DirectoryServices;
using System.Linq;

namespace RdsUserProperties
{
    #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public static class ActiveDirectoryTools
    #pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        private static string GetFilter(string Identity)
        {
            if (Identity.StartsWith("CN="))
            {
                return Identity;
            }            

            if (Identity.Contains('@'))
            {
                return  string.Format("(&(ObjectCategory=user)(ObjectClass=person)(userPrincipalName={0}))", Identity);
            }

            if (Identity.Contains('\\'))
            {
                var accountName = Identity.Split('\\')[1];
                return string.Format("(&(ObjectCategory=user)(ObjectClass=person)(sAMAccountName={0}))", accountName);
            }

            return string.Format("(&(ObjectCategory=user)(ObjectClass=person)(sAMAccountName={0}))", Identity);
        }

        /*
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static string GetDistinguishedName(string Identity)
        #pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            var filter = GetFilter(Identity);

            var currentDomain = string.Format("DC={0}", (System.DirectoryServices.ActiveDirectory.Domain.GetCurrentDomain()).Name.Replace(".", ",DC="));
            var searchRootPath = string.Format("GC://{0}", currentDomain);
            DirectoryEntry searchRoot = new DirectoryEntry(searchRootPath);

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
        */

        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static string GetDistinguishedName(string Identity, string UserName, string Password, string ServerName)
        #pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            var filter = GetFilter(Identity);

            var currentDomain = string.Format("DC={0}", (System.DirectoryServices.ActiveDirectory.Domain.GetCurrentDomain()).Name.Replace(".", ",DC="));
            var searchRootPath = string.Format("GC://{0}", currentDomain);

            if(!string.IsNullOrEmpty(ServerName))
            {
                searchRootPath = string.Format("LDAP://{0}", ServerName);
            }
            
            var searchRoot = new DirectoryEntry
            {
                Path = searchRootPath                
            };
            if(!string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(Password))
            {
                searchRoot.Username = UserName;
                searchRoot.Password = Password;
                searchRoot.AuthenticationType = AuthenticationTypes.Secure;
            }

            var ds = new DirectorySearcher
            {
                Filter = filter,
                PageSize = 1,
                SearchRoot = searchRoot
            };

            var users = ds.FindAll();
            if (users is null)
            {
                Exception exception = new Exception("Unable to find user.");
                throw exception;
            }
            if(users.Count > 1)
            {
                Exception exception = new Exception("Multiple users found.");
                throw exception;
            }

            var user = users[0];
            var adsPath = user.Properties["adsPath"].Cast<string>().FirstOrDefault();

            // "GC://CN=thor,OU=Artus,OU=Users,OU=Avengers,DC=marvel,DC=net"
            if (adsPath.StartsWith("GC://"))
            {
                return adsPath.Replace("GC://", "");
            }

            // "LDAP://Marvel-DC.marvel.net/CN=thor,OU=Artus,OU=Users,OU=Avengers,DC=marvel,DC=net"
            return adsPath;
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
