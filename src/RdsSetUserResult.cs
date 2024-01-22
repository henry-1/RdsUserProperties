using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RdsUserProperties
{
    /// <summary>
    /// <para type="description">RDS set user result model.</para>
    /// </summary>
    class RdsSetUserResult
    {
        /// <summary>
        /// <para type="description">distinguishedName, sAMAccountName or UPN</para>
        /// </summary>
        public string Identity { get; set; }

        /// <summary>
        /// <para type="description"> Success or error indicator</para>
        /// </summary>
        public string Result { get; set; }

        /// <summary>
        /// <para type="description">Details in case of an error.</para>
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}
