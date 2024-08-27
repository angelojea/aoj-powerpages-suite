/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Configuration
{
	using System;
	using System.Linq;
	using Microsoft.Xrm.Sdk.WebServiceClient;

	public interface IEssSettings
	{
		string ValidLicenseSkus { get; }
		bool IsEss { get; }
	}

	public interface IAuthenticationSettings
	{
		string RootUrl { get; }
		string TenantId { get; }
		string ClientId { get; }
		string RedirectUri { get; }
		string Caption { get; }

		/// <summary>
		/// The duration of time prior to the access token expiration when the token will be refreshed.
		/// </summary>
		TimeSpan TokenRefreshWindow { get; }
	}

	public interface IGraphSettings
	{
		string RootUrl { get; }
	}

	public interface ICertificateSettings
	{
		string ThumbprintPrimary { get; }
		string ThumbprintSecondary { get; }
		bool FindByTimeValid { get; }
	}

	public interface IPortalSettings
	{
		IEssSettings Ess { get; }
		IAuthenticationSettings Authentication { get; }
		IGraphSettings Graph { get; }
		ICertificateSettings Certificate { get; }
		CrmSettings Crm { get; }
	}

	public class EssSettings : IEssSettings
	{
		public string ValidLicenseSkus { get; set; }

		public bool IsEss { get; set; }

		public EssSettings()
		{
			ValidLicenseSkus = "Ess.ValidLicenseSkus".ResolveAppSetting();
			IsEss = "Ess.IsEss".ResolveAppSetting().ToBoolean().GetValueOrDefault();
		}
	}

	public class AuthenticationSettings : IAuthenticationSettings
	{
		public string RootUrl { get; set; }

		public string TenantId { get; set; }

		public string ClientId { get; set; }

		public string RedirectUri { get; set; }

		public string Caption { get; set; }

		public TimeSpan TokenRefreshWindow { get; set; }

		public AuthenticationSettings()
		{
			RootUrl = "Azure.Authentication.RootUrl".ResolveAppSetting();
			TenantId = "Azure.Authentication.TenantId".ResolveAppSetting();
			ClientId = "Azure.Authentication.ClientId".ResolveAppSetting();
			RedirectUri = "Azure.Authentication.RedirectUri".ResolveAppSetting();
			Caption = "Azure.Authentication.Caption".ResolveAppSetting();
			TokenRefreshWindow = "Azure.Authentication.TokenRefreshWindow".ResolveAppSetting().ToTimeSpan().GetValueOrDefault(TimeSpan.FromMinutes(20));
		}
	}

	public class GraphSettings : IGraphSettings
	{
		public string RootUrl { get; set; }

		public GraphSettings()
		{
			RootUrl = "Azure.Graph.RootUrl".ResolveAppSetting();
		}
	}

	public class CertificateSettings : ICertificateSettings
	{
		/// <summary>
		/// The thumbprint of the primary certificate.
		/// </summary>
		public string ThumbprintPrimary { get; set; }

		/// <summary>
		/// The thumbprint of the secondary certificate.
		/// </summary>
		public string ThumbprintSecondary { get; set; }

		/// <summary>
		/// Indicates that only non-expired certificates are accepted.
		/// </summary>
		public bool FindByTimeValid { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="CertificateSettings" /> class.
		/// </summary>
		public CertificateSettings()
		{
			FindByTimeValid = "Azure.Certificate.FindByTimeValid".ResolveAppSetting().ToBoolean().GetValueOrDefault(true);
			ThumbprintPrimary = "Azure.Certificate.ThumbprintPrimary".ResolveAppSetting();
			ThumbprintSecondary = "Azure.Certificate.ThumbprintSecondary".ResolveAppSetting();
		}
	}

	/// <summary>
	/// CRM connection settings.
	/// </summary>
	public class CrmSettings
	{
		/// <summary>
		/// The primary service URL.
		/// </summary>
		public string PrimaryServiceUrl { get; }

		/// <summary>
		/// The extended primary service URL.
		/// </summary>
		public Uri FullPrimaryServiceUrl { get; }

		/// <summary>
		/// The alternate service URL.
		/// </summary>
		public string AlternateServiceUrl { get; }

		/// <summary>
		/// The extended alternate service URL.
		/// </summary>
		public Uri FullAlternateServiceUrl { get; }

		/// <summary>
		/// A timestamp of the last failover event.
		/// </summary>
		public DateTimeOffset CurrentServiceUrlModifiedOn { get; private set; }

		/// <summary>
		/// The duration to wait after a failover before another failover is allowed. Prevents concurrent failover errors from cancelling each other.
		/// </summary>
		public TimeSpan CurrentServiceUrlTimeSpan { get; }

		/// <summary>
		/// The current failover state.
		/// </summary>
		public bool UseAlternateServiceUrl { get; private set; }

		/// <summary>
		/// The current service URL considering the current failover state.
		/// </summary>
		public Uri CurrentServiceUrl => UseAlternateServiceUrl ? FullAlternateServiceUrl : FullPrimaryServiceUrl;

		/// <summary>
		/// Initializes a new instance of the <see cref="CrmSettings" /> class.
		/// </summary>
		public CrmSettings()
		{
			CurrentServiceUrlTimeSpan = TimeSpan.FromMinutes(10);
			UseAlternateServiceUrl = "Azure.CrmSettings.UseAlternateServiceUrl".ResolveAppSetting().ToBoolean().GetValueOrDefault();
			PrimaryServiceUrl = "Azure.CrmSettings.PrimaryServiceUrl".ResolveAppSetting() ?? "Azure.CrmSettings.ServiceUrl".ResolveAppSetting();
			FullPrimaryServiceUrl = GetFullServiceUrl(PrimaryServiceUrl);
			AlternateServiceUrl = "Azure.CrmSettings.AlternateServiceUrl".ResolveAppSetting() ?? ToAlternateServiceUrl(PrimaryServiceUrl);
			FullAlternateServiceUrl = GetFullServiceUrl(AlternateServiceUrl);
		}

		/// <summary>
		/// Toggles the current failover state between active and inactive.
		/// </summary>
		public bool TryToggleCurrentServiceUrl()
		{
			if (CurrentServiceUrlModifiedOn + CurrentServiceUrlTimeSpan > DateTimeOffset.UtcNow)
			{
				// subsequent toggle is invoked too early
				return false;
			}

			UseAlternateServiceUrl = !UseAlternateServiceUrl;
			CurrentServiceUrlModifiedOn = DateTimeOffset.UtcNow;

			return true;
		}

		/// <summary>
		/// Converts a primary service URL to an alternate service URL by appending '--S' to the organization name."
		/// </summary>
		/// <param name="serviceUrl">The primary URL.</param>
		/// <returns>The alternate URL.</returns>
		private static string ToAlternateServiceUrl(string serviceUrl)
		{
			if (string.IsNullOrWhiteSpace(serviceUrl))
			{
				return null;
			}

			var url = new Uri(serviceUrl);
			var parts = url.Host.Split('.');
			var organizationName = parts.First();
			var tail = parts.Skip(1);
			var host = organizationName + "--S." + string.Join(".", tail);
			var alternateUrl = new UriBuilder(serviceUrl) { Host = host };

			return alternateUrl.Uri.OriginalString;
		}

		/// <summary>
		/// Extends the configured URL to a full OAuth organization service URL.
		/// </summary>
		/// <param name="url">The short URL.</param>
		/// <returns>The full URL.</returns>
		private static Uri GetFullServiceUrl(string url)
		{
			if (url == null)
			{
				return null;
			}

			const string path = "XRMServices/2011/Organization.svc/web";
			var separator = url.EndsWith("/") ? string.Empty : "/";
			var svcUri = new Uri(url + separator + path);
			var version = System.Diagnostics.FileVersionInfo.GetVersionInfo(typeof(OrganizationWebProxyClient).Assembly.Location).FileVersion;
			var fullUrl = new UriBuilder(svcUri) { Query = "SDKClientVersion=" + version };

			return fullUrl.Uri;
		}
	}

	public class PortalSettings : IPortalSettings
	{
		private static readonly Lazy<PortalSettings> _instance = new Lazy<PortalSettings>(() => new PortalSettings());

		public static PortalSettings Instance
		{
			get { return _instance.Value; }
		}

		private PortalSettings()
		{
			Ess = new EssSettings();
			Authentication = new AuthenticationSettings();
			Graph = new GraphSettings();
			Certificate = new CertificateSettings();
			Crm = new CrmSettings();
			UseOnlineSetup = "PortalOnlineSetup".ResolveAppSetting().ToBoolean().GetValueOrDefault();
			DomainName = "PortalDomainName".ResolveAppSetting();
			BingMapsSupported = !"Portal.BingMaps.Disabled".ResolveAppSetting().ToBoolean().GetValueOrDefault();
			WriteToDiagnosticsTrace = "PortalWriteToDiagnosticsTrace".ResolveAppSetting().ToBoolean().GetValueOrDefault();
		}

		public IEssSettings Ess { get; }
		public IAuthenticationSettings Authentication { get; }
		public IGraphSettings Graph { get; }
		public ICertificateSettings Certificate { get; }
		public CrmSettings Crm { get; }
		public bool UseOnlineSetup { private set; get; }
		public string DomainName { private set; get; }

		// Sets the flag is 'Portal.BingMaps.Disabled' setting is set during portal provisioning if the region is DEU.
		public bool BingMapsSupported { private set; get; }

		/// <summary>
		/// Indicates that <see cref="System.Diagnostics.TraceSource"/> logs should be handled by the common <see cref="System.Diagnostics.Trace"/> class.
		/// </summary>
		public bool WriteToDiagnosticsTrace { get; }
	}
}
