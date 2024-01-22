using System.DirectoryServices;
using System.Management.Automation;
using System;
using System.IO;

namespace RdsUserProperties
{
    /// <summary>
    /// <para type="synopsis">Get RDS User properties in Active Directory</para>
    /// <para type="description">This cmdlet can be used to read properties on the Remote Desktop Services Profile Tab.</para>    
    /// <para type="note">TSUSEREXLib must registered: regsvr32 -u Interop.TSUSEREXLib.dll</para>
    /// </summary>
    /// <example>   
    ///   <code>Get-RDSUserProperties  -Identity 'CN=Account1,OU=Office,OU=Users,OU=MyDomain,DC=TLD' -ServerName 'dc1.MyDomain.TLD'</code>
    /// </example>
    /// <example>   
    ///   <code>Get-RDSUserProperties  -Identity Account1</code>
    /// </example>
    /// <example>   
    ///   <code>Get-RDSUserProperties  -Identity Account1@MyDomain.TLD</code>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "RdsUserProperties")]
    [OutputType(typeof(RdsUser))]
    public class RdsGetUserPropertiesCmdlet : PSCmdlet
    {

        /// <summary>
        /// <para type="description">userPrincipalName, sAMAccountName or distinguishedName of an Active Directory user object.</para>
        /// </summary>
        [Parameter(Mandatory = true,
            HelpMessage = "userPrincipalName, sAMAccountName or distinguishedName of an Active Directory user object.",
            ValueFromPipeline = true)]
        [Alias("Id")]
        [ValidateNotNullOrEmpty]
        public string Identity { get; set; }


        /// <summary>
        /// <para type="description">Domain Controller name. Must be a DC of the users domain because RDS properties aren't part of the partial attribute set.</para>
        /// </summary>
        [Parameter(Mandatory = false,
            HelpMessage = @"Domain Controller Name for LDAP Connection. 
              Choose a Server in the same Domain as the User Object because 
              RDS Attributes are not Part of the Partial Attribute Set.",
            ValueFromPipelineByPropertyName = false)]
        [Alias("DomainController", "DC")]
        public string ServerName { get; set; }


        /// <summary>
        /// <para type="description">Login name for AD connection.</para>
        /// </summary>
        [Parameter(Mandatory = false,
            HelpMessage = "Login name for AD connection",
            ValueFromPipelineByPropertyName = false)]
        [Alias("LoginName","AccountName")]
        public string UserName { get; set; }

        /// <summary>
        /// <para type="description">Login name for AD connection.</para>
        /// </summary>
        [Parameter(Mandatory = false,
            HelpMessage = "Password for AD connection",
            ValueFromPipelineByPropertyName = false)]
        [Alias("PWD")]
        public string Password { get; set; }


        /// <summary>
        /// <para type="description">The Powershell script.</para>
        /// </summary>
        protected override void ProcessRecord()
        {
            DirectoryEntry user;
            Identity = ActiveDirectoryTools.GetDistinguishedName(Identity);
            string ADSIPath;

            if (string.IsNullOrEmpty(this.ServerName))
            {
                var domainName = ActiveDirectoryTools.GetDomainName(Identity);
                ServerName = ActiveDirectoryTools.GetDcName(domainName);                
            }
            ADSIPath = string.Format("LDAP://{0}/{1}", ServerName, Identity);

            if (string.IsNullOrEmpty(this.UserName) || string.IsNullOrEmpty(this.Password))
            {
                user = new DirectoryEntry
                {
                    Path = ADSIPath
                };
            }else
            {
                user = new DirectoryEntry
                {
                    Path = ADSIPath,
                    Username = UserName,
                    Password = Password,
                    AuthenticationType = AuthenticationTypes.Secure
                };
            }           

            if (user is null)
            {                
                WriteObject(null);
                return;
            }

            var rdsUser = new RdsUser
            {
                Identity = Identity
            };                       

            try {
                var profilePath = user.InvokeGet("TerminalServicesProfilePath");
                if(!(profilePath is null))
                    rdsUser.TerminalServicesProfilePath = profilePath.ToString();

                var homeDir = user.InvokeGet("TerminalServicesHomeDirectory");
                if(!(homeDir is null))
                    rdsUser.TerminalServicesHomeDirectory = homeDir.ToString();

                // HomeDrive only exists for local a path
                if(!string.IsNullOrEmpty(rdsUser.TerminalServicesHomeDirectory) && rdsUser.TerminalServicesHomeDirectory.StartsWith("\\\\"))
                {
                    rdsUser.TerminalServicesHomeDrive = user.InvokeGet("TerminalServicesHomeDrive").ToString();
                }                

                var allowLogon = user.InvokeGet(propertyName: "AllowLogon");
                if(!(allowLogon is null))
                    rdsUser.AllowLogon = allowLogon.ToString().Equals("1");
            }
            catch
            {
                WriteObject(null);
                return;
            }

            WriteObject(rdsUser);            
        }        
    }   

}
