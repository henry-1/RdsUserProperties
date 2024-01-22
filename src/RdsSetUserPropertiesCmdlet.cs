
using System;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Management.Automation;

namespace RdsUserProperties
{
    /// <summary>
    /// <para type="synopsis">Set RDS User properties in Active Directory</para>
    /// <para type="description">This cmdlet can be used to set properties on the Remote Desktop Services Profile Tab.</para>   
    /// <para type="note">Values of user properties are removed in case no value is given for the corresponding cmdlet parameter.</para>
    /// <para type="note">TerminalServicesHomeDirectory when used in conjunction with TerminalServicesHomeDirectory must be a shared folder. Otherwise it can be a local path.</para>
    /// <para type="note">TSUSEREXLib must registered: regsvr32 -u Interop.TSUSEREXLib.dll</para>
    /// </summary>
    /// <example>
    ///   <code>Set-RDSUserProperties  -Identity 'CN=Account1,OU=Office,OU=Users,OU=MyDomain,DC=TLD' -ServerName 'dc1.MyDomain.TLD' -TerminalServicesProfilePath '\\fs1\RDSProfile\Account1' -TerminalServicesHomeDrive 't:' -TerminalServicesHomeDirectory '\\fs2\RDSHome\Account1' -DenyLogo $false</code>
    /// </example>
    /// <example>
    ///   <code>Set-RDSUserProperties  -Identity 'CN=Account1,OU=Office,OU=Users,OU=MyDomain,DC=TLD' -ServerName 'dc1.MyDomain.TLD' -TerminalServicesProfilePath $null -TerminalServicesHomeDirectory = $null -AllowLogon = $true</code>
    /// </example>
    [Cmdlet(VerbsCommon.Set, "RdsUserProperties")]
    [OutputType(typeof(RdsSetUserResult))]
    public class RdsSetUserPropertiesCmdlet : PSCmdlet
    {
        /// <summary>
        /// <para type="description">distinguishedName of an Active Directory user objec.</para>
        /// </summary>
        [Parameter(Mandatory = true,
            HelpMessage = "userPrincipalName, sAMAccountName or distinguishedName of an Active Directory user object.",
            ValueFromPipeline = false)]
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
            ValueFromPipeline = false)]
        [Alias("DomainController", "DC")]
        public string ServerName { get; set; }

        /// <summary>
        /// <para type="description">Login name for AD connection.</para>
        /// </summary>
        [Parameter(Mandatory = false,
            HelpMessage = "Login name for AD connection",
            ValueFromPipelineByPropertyName = false)]
        [Alias("LoginName", "AccountName")]
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
        /// <para type="description">Shared folder path for the users TS profile.</para>
        /// </summary>
        [Parameter(Mandatory = false,
            HelpMessage = "RDS Profile Path",
            ValueFromPipeline = false)]
        public string TerminalServicesProfilePath { get; set; }

        /// <summary>
        /// <para type="description">Letter for the users TS home drive (including ':').</para>
        /// </summary>
        [Parameter(Mandatory = false,
            HelpMessage = "RDS Home Drive",
            ValueFromPipeline = false)]
        public string TerminalServicesHomeDrive { get; set; }

        /// <summary>
        /// <para type="description">Folder path for the users TS home drive.</para>
        /// </summary>
        [Parameter(Mandatory = false,
            HelpMessage = "RDS Home Directory",
            ValueFromPipeline = false)]
        public string TerminalServicesHomeDirectory { get; set; }

        /// <summary>
        /// <para type="description">Boolean value to allow or deny RDS access for the user.</para>
        /// </summary>
        [Parameter(Mandatory = false,
            HelpMessage = "Allow Logon through RDP Host",
            ValueFromPipeline = false)]
        public bool AllowLogon { get; set; }

        /// <summary>
        /// <para type="description">The Powershell script.</para>
        /// </summary>
        protected override void ProcessRecord()
        {
            DirectoryEntry user;
            bool changed = false;
            int allowLogon = 1;
            string ADSIPath;

            var result = new RdsSetUserResult
            {
                Identity = Identity,
                Result = "Error"
            };

            try
            {
                Identity = ActiveDirectoryTools.GetDistinguishedName(Identity);

                if (this.ServerName is null)
                {
                    var domainName = ActiveDirectoryTools.GetDomainName(Identity);
                    ServerName = ActiveDirectoryTools.GetDcName(domainName);
                }

                ADSIPath = string.Format("LDAP://{0}/{1}", ServerName, Identity);   

                result.Identity = ADSIPath;

                if (string.IsNullOrEmpty(this.UserName) || string.IsNullOrEmpty(this.Password))
                {
                    user = new DirectoryEntry
                    {
                        Path = ADSIPath
                    };
                }
                else
                {
                    user = new DirectoryEntry
                    {
                        Path = ADSIPath,
                        Username = UserName,
                        Password = Password,
                        AuthenticationType = AuthenticationTypes.Secure
                    };
                }

                if (this.MyInvocation.BoundParameters.Keys.Contains("TerminalServicesProfilePath"))
                {
                    if (string.IsNullOrEmpty(this.TerminalServicesProfilePath))
                    {
                        user.InvokeSet("TerminalServicesProfilePath", "");
                    }
                    else
                    {
                        user.InvokeSet("TerminalServicesProfilePath", this.TerminalServicesProfilePath);
                    }
                    changed = true;
                }

                if (this.MyInvocation.BoundParameters.Keys.Contains("TerminalServicesHomeDrive"))
                {
                    if (!string.IsNullOrEmpty(this.TerminalServicesHomeDrive))                    
                    {                        
                        user.InvokeSet("TerminalServicesHomeDrive", this.TerminalServicesHomeDrive);
                    }
                    changed = true;
                }                
                
                if (this.MyInvocation.BoundParameters.Keys.Contains("TerminalServicesHomeDirectory"))
                {
                    if (string.IsNullOrEmpty(this.TerminalServicesHomeDirectory))
                    {
                        user.InvokeSet("TerminalServicesHomeDirectory", "");                      
                    }
                    else
                    {
                        user.InvokeSet("TerminalServicesHomeDirectory", this.TerminalServicesHomeDirectory);
                    }
                    changed = true;
                }

                if (this.MyInvocation.BoundParameters.Keys.Contains("AllowLogon"))
                {
                    if(false == AllowLogon)
                        allowLogon = 0;
                    
                    user.InvokeSet(propertyName: "AllowLogon", allowLogon);
                    changed = true;
                }

                if (changed)
                    user.CommitChanges();

                result.Result = "Success";
                WriteObject(result);

            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.Message;                
                WriteObject(result);
            }            
        }
    }
}