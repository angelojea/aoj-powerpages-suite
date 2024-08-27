/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Web;
using System.Security.Permissions;

namespace Microsoft.Xrm.Portal.Web.UI.WebControls
{
	[AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal), AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class CrmMetadataDataSourceStatusEventArgs : EventArgs
	{
		public CrmMetadataDataSourceStatusEventArgs(int rowsAffected, Exception exception)
		{
			RowsAffected = rowsAffected;
			Exception = exception;
			ExceptionHandled = true;
		}

		public int RowsAffected { get; }

		public Exception Exception { get; }

		public bool ExceptionHandled { get; set; }
	}
}
