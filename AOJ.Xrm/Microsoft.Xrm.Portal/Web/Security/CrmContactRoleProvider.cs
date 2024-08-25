/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Specialized;
using Microsoft.Xrm.Client.Configuration;
using Microsoft.Xrm.Portal.Configuration;

namespace Microsoft.Xrm.Portal.Web.Security
{
	/// <summary>
	/// A <see cref="CrmRoleProvider"/> that validates 'contact' entities (users) against 'mspp_webrole' entitles (roles).
	/// </summary>
	/// <remarks>
	/// Configuration format.
	/// <code>
	/// <![CDATA[
	/// <configuration>
	/// 
	///  <system.web>
	///   <roleManager enabled="true" defaultProvider="Xrm">
	///    <providers>
	///     <add
	///      name="Xrm"
	///      type="Microsoft.Xrm.Portal.Web.Security.CrmContactRoleProvider"
	///      portalName="Xrm" [Microsoft.Xrm.Portal.Configuration.PortalContextElement]
	///      attributeMapIsAuthenticatedUsersRole="mspp_authenticatedusersrole"
	///      attributeMapRoleName="mspp_name"
	///      attributeMapRoleWebsiteId="mspp_websiteid"
	///      attributeMapUsername="adx_identity_username"
	///      roleEntityName="mspp_webrole"
	///      roleToUserRelationshipName="mspp_webrole_contact"
	///      userEntityName="contact"
	///     />
	///    </providers>
	///   </roleManager>
	///  </system.web>
	///  
	/// </configuration>
	/// ]]>
	/// </code>
	/// </remarks>
	/// <seealso cref="PortalContextElement"/>
	/// <seealso cref="PortalCrmConfigurationManager"/>
	/// <seealso cref="CrmConfigurationManager"/>
	public class CrmContactRoleProvider : CrmRoleProvider  // MSBug #120048: Won't seal, inheritance is expected extension point.
	{
		public override void Initialize(string name, NameValueCollection config)
		{
			config["attributeMapIsAuthenticatedUsersRole"] = config["attributeMapIsAuthenticatedUsersRole"] ?? "mspp_authenticatedusersrole";

			config["attributeMapRoleName"] = config["attributeMapRoleName"] ?? "mspp_name";

			config["attributeMapRoleWebsiteId"] = config["attributeMapRoleWebsiteId"] ?? "mspp_websiteid";

			config["attributeMapUsername"] = config["attributeMapUsername"] ?? "adx_identity_username";

			config["roleEntityName"] = config["roleEntityName"] ?? "mspp_webrole";

			config["roleToUserRelationshipName"] = config["roleToUserRelationshipName"] ?? "mspp_webrole_contact";

			config["userEntityName"] = config["userEntityName"] ?? "contact";

			base.Initialize(name, config);
		}
	}
}
