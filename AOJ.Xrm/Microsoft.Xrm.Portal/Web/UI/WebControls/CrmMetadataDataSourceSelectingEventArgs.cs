/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Web;
using System.Web.UI;
using System.Security.Permissions;
using System.ComponentModel;
using Microsoft.Xrm.Sdk.Metadata;

namespace Microsoft.Xrm.Portal.Web.UI.WebControls
{
	[AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal), AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class CrmMetadataDataSourceSelectingEventArgs : CancelEventArgs
	{
		public CrmMetadataDataSource DataSource { get; }

		public DataSourceSelectArguments Arguments { get; }

		public EntityFilters MetadataFlags { get; set; }

		public EntityFilters EntityFlags { get; set; }

		public string EntityName { get; set; }

		public string AttributeName { get; set; }

		public string SortExpression { get; set; }

		public CrmMetadataDataSourceSelectingEventArgs(
			CrmMetadataDataSource dataSource,
			DataSourceSelectArguments arguments,
			string entityName,
			string attributeName,
			EntityFilters metadataFlags,
			EntityFilters entityFlags,
			string sortExpression)
		{
			DataSource = dataSource;
			Arguments = arguments;
			EntityName = entityName;
			AttributeName = attributeName;
			MetadataFlags = metadataFlags;
			EntityFlags = entityFlags;
			SortExpression = sortExpression;
		}
	}
}
