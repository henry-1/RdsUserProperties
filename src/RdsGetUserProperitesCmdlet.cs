using System.DirectoryServices;
using System.Management.Automation;

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
        /// <para type="description">The Powershell script.</para>
        /// </summary>
        protected override void ProcessRecord()
        {            
            Identity = ActiveDirectoryTools.GetDistinguishedName(Identity);

            var rdsUser = new RdsUser
            {
                Identity = Identity
            };

            if (this.ServerName is null)
            {
                var domainName = ActiveDirectoryTools.GetDomainName(Identity);
                ServerName = ActiveDirectoryTools.GetDcName(domainName);
            }

            var ADSIPath = string.Format("LDAP://{0}/{1}", ServerName, Identity);
            var user = new DirectoryEntry(ADSIPath);

            try {
                rdsUser.TerminalServicesProfilePath = user.InvokeGet("TerminalServicesProfilePath").ToString();
            }
            catch {
                // do nothing
            }

            try {
                rdsUser.TerminalServicesHomeDirectory = user.InvokeGet("TerminalServicesHomeDirectory").ToString();
            }
            catch {
                // do nothing
            }

            try {
                // HomeDrive only exists for local a path
                if(!string.IsNullOrEmpty(rdsUser.TerminalServicesHomeDirectory) && rdsUser.TerminalServicesHomeDirectory.StartsWith("\\\\"))
                {
                    rdsUser.TerminalServicesHomeDrive = user.InvokeGet("TerminalServicesHomeDrive").ToString();
                }                
            } catch {
                // do nothing
            }

            try
            {
                var allowLogon = user.InvokeGet(propertyName: "AllowLogon").ToString();
                rdsUser.AllowLogon = allowLogon.Equals("1");
            }
            catch
            {
                // do nothing
            }

            WriteObject(rdsUser);            
        }        
    }   

}
