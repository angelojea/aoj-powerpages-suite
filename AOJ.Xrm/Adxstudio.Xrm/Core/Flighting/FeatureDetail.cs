/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Configuration;

namespace Adxstudio.Xrm.Core.Flighting
{
	/// <summary>
	/// Class to represent the details needed for a FCB
	/// </summary>
	public class FeatureDetail : IFeatureDetail
	{
		private bool isEnabled;

		public FeatureDetail()
		{
		}

		public FeatureDetail(string name, bool isEnabled, FeatureLocation location)
		{
			Name = name;
			IsEnabled = isEnabled;
			FeatureLocation = location;
		}

		public string Name { get; set; }

		public bool IsEnabled
		{
			get
			{
				// Setting from the configuration manager overrides the hard-coded FCB
				bool setting;
				if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings.Get(Name))
				    && bool.TryParse(ConfigurationManager.AppSettings[Name], out setting))
				{
					return setting;
				}
				return isEnabled;
			}
			set { isEnabled = value; }
		}

		public FeatureLocation FeatureLocation { get; set; }
	}
}
