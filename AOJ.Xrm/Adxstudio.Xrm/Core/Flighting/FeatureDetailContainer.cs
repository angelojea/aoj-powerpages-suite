/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Core.Flighting
{
	using System.Collections.Generic;
	using Configuration;

	/// <summary>
	/// The class which holds all the FCBs default values
	/// </summary>
	public sealed class FeatureDetailContainer : IFeatureDetailContainer
	{
		public FeatureDetailContainer()
		{
			InitializeFeatureMetadata();
		}

		public Dictionary<string, IFeatureDetail> Features { get; set; } = new Dictionary<string, IFeatureDetail>();

		/// <summary>
		/// Creates feature detail
		/// </summary>
		/// <param name="name"></param>
		/// <param name="isEnabled"></param>
		/// <param name="location"></param>
		/// <returns></returns>
		private IFeatureDetail Feature(string name, bool isEnabled, FeatureLocation location)
		{
			return new FeatureDetail(name, isEnabled, location);
		}

		/// <summary>
		/// Adds a feature set for FeatureLocation = Global
		/// </summary>
		/// <param name="feature"></param>
		/// <param name="isEnabled"></param>
		private void AddGlobalFeature(string feature, bool isEnabled)
		{
			Features.Add(feature, Feature(feature, isEnabled, FeatureLocation.Global));
		}

		/// <summary>
		/// Default list of FCBs and their details
		/// </summary>
		void InitializeFeatureMetadata()
		{
			AddGlobalFeature(FeatureNames.Web2Case, true);
			AddGlobalFeature(FeatureNames.Feedback, true);
			AddGlobalFeature(FeatureNames.EventHubCacheInvalidation, true);
			AddGlobalFeature(FeatureNames.Categories, true);
			AddGlobalFeature(FeatureNames.TelemetryFeatureUsage, true);
			AddGlobalFeature(FeatureNames.PortalFacetedNavigation, true);
			AddGlobalFeature(FeatureNames.CmsEnabledSearching, true);
			AddGlobalFeature(FeatureNames.CustomerJourneyTracking, "PortalTracking".ResolveAppSetting().ToBoolean().GetValueOrDefault());
			AddGlobalFeature(FeatureNames.EntityPermissionFetchUnionHint, true);
			AddGlobalFeature(FeatureNames.PortalAllowStaleData, "PortalAllowStaleData".ResolveAppSetting().ToBoolean().GetValueOrDefault());
			AddGlobalFeature(FeatureNames.WebProxyClientFailover, "PortalWebProxyClientFailover".ResolveAppSetting().ToBoolean().GetValueOrDefault());

			AddGlobalFeature(FeatureNames.CALProductSearchPostFiltering, false);
		}
	}
}
