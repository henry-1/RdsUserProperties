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
                return string.Format("(&(ObjectCategory=user)(ObjectClass=person)(distinguishedName={0}))", Identity);
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

        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static string GetAdsPath(string Identity, string UserName, string Password, string ServerName)
        #pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            string adsPath;
            string searchRootPath = string.Empty;

            if (Identity.StartsWith("CN=") && !string.IsNullOrEmpty(ServerName))
            {
                adsPath = string.Format("LDAP://{0}/{1}", ServerName, Identity);
                return adsPath;
            }                                   

            if(!string.IsNullOrEmpty(ServerName))
            {
                searchRootPath = string.Format("LDAP://{0}", ServerName);
            }
            
            if(string.IsNullOrEmpty(searchRootPath))
            {
                var currentDomain = string.Format("DC={0}", (System.DirectoryServices.ActiveDirectory.Domain.GetCurrentDomain()).Name.Replace(".", ",DC="));
                searchRootPath = string.Format("GC://{0}", currentDomain);
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

            var filter = GetFilter(Identity);
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
            adsPath = user.Properties["adsPath"].Cast<string>().FirstOrDefault();

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
