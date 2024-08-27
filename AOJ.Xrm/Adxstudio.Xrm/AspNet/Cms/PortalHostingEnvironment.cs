/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.AspNet.Cms
{
	using System.Web.Hosting;
	using Configuration;

	/// <summary>
	/// The web app host details.
	/// </summary>
	public class PortalHostingEnvironment
	{
		/// <summary>
		/// The site name.
		/// </summary>
		public string SiteName { get; private set; }

		/// <summary>
		/// The virtual path.
		/// </summary>
		public string ApplicationVirtualPath { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="PortalHostingEnvironment" /> class.
		/// </summary>
		public PortalHostingEnvironment()
		{
			if (PortalSettings.Instance.UseOnlineSetup && !string.IsNullOrEmpty(PortalSettings.Instance.DomainName))
			{
				SiteName = PortalSettings.Instance.DomainName;
				ApplicationVirtualPath = "/";
			}
			else
			{
				SiteName = HostingEnvironment.SiteName;
				ApplicationVirtualPath = HostingEnvironment.ApplicationVirtualPath;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PortalHostingEnvironment" /> class.
		/// </summary>
		/// <param name="siteName">The site name.</param>
		/// <param name="applicationVirtualPath">The virtual path.</param>
		public PortalHostingEnvironment(string siteName, string applicationVirtualPath)
		{
			SiteName = siteName;
			ApplicationVirtualPath = applicationVirtualPath;
		}
	}
}
