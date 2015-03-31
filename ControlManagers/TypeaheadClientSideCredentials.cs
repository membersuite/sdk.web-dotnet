using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MemberSuite.SDK.Web.ControlManagers
{
    public class TypeaheadClientSideCredentials
    {
        /// <summary>
        /// Gets or sets the index of the name of the index the client should access
        /// </summary>
        /// <value>The index of the name of.</value>
        public string NameOfIndex { get; set; }

        public string ClientSideApiKey { get; set; }
    }
}
