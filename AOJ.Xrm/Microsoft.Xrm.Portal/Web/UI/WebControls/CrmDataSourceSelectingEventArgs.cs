/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Web;
using System.Web.UI;
using System.Security.Permissions;
using System.ComponentModel;
using Microsoft.Xrm.Sdk.Query;

namespace Microsoft.Xrm.Portal.Web.UI.WebControls
{
	[AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal), AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public sealed class CrmDataSourceSelectingEventArgs : CancelEventArgs // MSBug #120088: Won't make internal, part of public event API.
	{
		public CrmDataSourceSelectingEventArgs(
			CrmDataSource dataSource,
			DataSourceSelectArguments arguments,
			string fetchXml,
			QueryByAttribute query)
		{
			DataSource = dataSource;
			Arguments = arguments;
			FetchXml = fetchXml;
			Query = query;
		}

		public CrmDataSource DataSource { get; }

		public DataSourceSelectArguments Arguments { get; }

		public string FetchXml { get; set; }

		public QueryByAttribute Query { get; }
	}
}
