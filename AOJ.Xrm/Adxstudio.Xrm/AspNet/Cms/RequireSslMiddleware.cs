/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.AspNet.Cms
{
	using Configuration;

	/// <summary>
	/// Settings for HTTPS redirect.
	/// </summary>
	public class RequireSslOptions
	{
		public RequireSslOptions(WebAppSettings webAppSettings)
		{
			Scheme = "https";
			Port = 443;
			RedirectStatusCode = 301;
			// require SSL in Azure web apps by default
			Enabled = "PortalRequireSsl".ResolveAppSetting().ToBoolean().GetValueOrDefault(webAppSettings.AzureWebAppEnabled);
		}

		public bool Enabled { get; set; }

		public string Scheme { get; set; }

		public int Port { get; set; }

		public int RedirectStatusCode { get; set; }
	}
}
