/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Diagnostics.Metrics
{
	using System;
	using System.Collections.Concurrent;
	using System.Globalization;
	using System.Threading;
	using Configuration;

	/// <summary>
	/// Default metric implementation, uses <see cref="IfxMetricsReporter"/> to send data.
	/// </summary>
	internal sealed class AdxMetric : IAdxMetric
	{
		public AdxMetric(string metricName)
		{
			metricName.ThrowOnNullOrWhitespace("metricName");
			MetricName = metricName;
		}

		public string MetricName { get; }

		public void LogValue(long rawValue)
		{
		}
		public bool Equals(IAdxMetric other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}
			if (ReferenceEquals(this, other))
			{
				return true;
			}
			return string.Equals(MetricName, other.MetricName);
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as AdxMetric);
		}

		public override int GetHashCode()
		{
			return (MetricName != null ? MetricName.GetHashCode() : 0);
		}
	}

	/// <summary>
	/// Wraps the IFX metrics API for emitting MDM metrics. Will not throw an exception when invoked, uses 
	/// <see cref="MetricsReportingEvents"/> to emit errors.
	/// </summary>
	internal sealed class IfxMetricsReporter
	{
		static readonly Lazy<IfxMetricsReporter> _lazyInstance = new Lazy<IfxMetricsReporter>(() => new IfxMetricsReporter());

		public static IfxMetricsReporter Instance
		{
			get { return _lazyInstance.Value; }
		}

		/// <summary>
		/// Cloud configuration settings required for reporting ADX metrics.
		/// </summary>
		private static class SettingNames
		{
			public const string MdmAccountName = "Adx.Diagnostics.MdmAccountName";
			public const string MdmNamespace = "Adx.Diagnostics.MdmNamespace";
			public const string Geo = "Adx.Diagnostics.GeoName";
			public const string Tenant = "Adx.Diagnostics.Tenant";
			public const string Role = "Adx.Diagnostics.Role";
			public const string RoleInstance = "Adx.Diagnostics.RoleInstance";
			public const string Org = "Adx.Diagnostics.OrgId";
			public const string PortalType = "Adx.Diagnostics.PortalType";
			public const string PortalId = "Adx.Diagnostics.PortalId";
			public const string PortalApp = "Adx.Diagnostics.PortalApp";
			public const string PortalUrl = "Azure.Authentication.RedirectUri";
			public const string PortalVersion = "Adx.Diagnostics.PortalVersion";

		}

		/// <summary>
		/// Cloud configuration settings required for reporting ADX metrics.
		/// </summary>
		private static class MdmDimensionNames
		{
			public const string Geo = "Geo";
			public const string Tenant = "Tenant";
			public const string Role = "Role";
			public const string RoleInstance = "RoleInstance";
			public const string Org = "Org";
			public const string PortalType = "PortalType";
			public const string PortalId = "PortalId";
			public const string PortalApp = "PortalApp";
			public const string PortalUrl = "PortalUrl";
			public const string PortalVersion = "PortalVersion";
		}


		private IfxMetricsReporter()
		{
		}

		private static string GetCloudConfigurationSettingWithValue(string settingName)
		{
			var result = settingName.ResolveAppSetting();
			result.ThrowOnNullOrWhitespace("settingName",
				string.Format(CultureInfo.InvariantCulture,
				"Missing value for configuration setting {0}, metrics API cannot initialize.", settingName));
			return result;
		}

		public void LogValueForMetric(IAdxMetric metric, long rawValue)
		{
		}
	}
}
