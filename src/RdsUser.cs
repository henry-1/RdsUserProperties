
namespace RdsUserProperties
{
    /// <summary>
    /// <para type="description">RDS user model.</para>
    /// </summary>
    public class RdsUser
    {    
        /// <summary>
        /// <para type="description">distinguishedName, sAMAccountName or UPN</para>
        /// </summary>
        public string Identity { get; set; }

        /// <summary>
        /// <para type="description">Terminal Services Profile Path</para>
        /// </summary>
        public string TerminalServicesProfilePath { get; set; }

        /// <summary>
        /// <para type="description">Terminal Services Home Directory</para>
        /// </summary>
        public string TerminalServicesHomeDirectory { get; set; }

        /// <summary>
        /// <para type="description">Terminal Services Home Drive</para>
        /// </summary>
        public string TerminalServicesHomeDrive { get; set; }

        /// <summary>
        /// <para type="description">Deny Logon</para>
        /// </summary>
        public bool DenyLogon { get; set; }
    }
}
