/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Security.Principal;
using System.ServiceModel;
using System.Threading;
using System.Web;
using System.Web.UI.WebControls;
using System.Collections.Specialized;
using System.Web.UI;
using System.ComponentModel;
using System.Drawing.Design;
using System.Collections;
using System.Text.RegularExpressions;
using System;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Diagnostics;

namespace Microsoft.Xrm.Portal.Web.UI.WebControls
{
	/// <summary>
	/// Represents settings for a cache key based cache dependency.
	/// </summary>
	public sealed class CacheKeyDependency : CacheKey, IStateManagedItem // MSBug #120086: Won't make internal.
	{
		#region IStateManagedItem Members

		public void SetDirty()
		{
			Parameters.SetDirty();
		}

		#endregion
	}

	/// <summary>
	/// Represents settings for describing a cache key.
	/// </summary>
	public class CacheKey : IStateManager
	{
		private static readonly char[] _varySeparator = { ';' };

		/// <summary>
		/// Gets or sets the name used for applying a property as a parameter.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the name of the property from which to obtain the value.
		/// </summary>
		public string PropertyName { get; set; }

		/// <summary>
		/// Gets or sets the format string describing the cache key.
		/// </summary>
		[DefaultValue(null)]
		public string KeyFormat { get; set; }

		/// <summary>
		/// Gets or sets the flag for caching by user.
		/// </summary>
		public bool VaryByUser { get; set; }

		/// <summary>
		/// Gets or sets the set of querystring parameter names. The names are separated by a semi-colon character.
		/// </summary>
		public string VaryByParam { get; set; }

		private ParameterCollection _parameters;

		/// <summary>
		/// Gets the parameters collection that contains the parameters that are used by the KeyFormat property.
		/// </summary>
		[Description(""), Category("Data"), PersistenceMode(PersistenceMode.InnerProperty), DefaultValue(null), Editor("System.Web.UI.Design.WebControls.ParameterCollectionEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), MergableProperty(false)]
		public ParameterCollection Parameters
		{
			get
			{
				if (_parameters == null)
				{
					_parameters = new ParameterCollection();

					if (IsTrackingViewState)
					{
						((IStateManager)_parameters).TrackViewState();
					}
				}

				return _parameters;
			}
		}

		/// <summary>
		/// Retrieves the cache key based on the current property values.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="control"></param>
		/// <param name="container"></param>
		/// <returns></returns>
		public string GetCacheKey(HttpContext context, Control control, object container)
		{
			return GetCacheKey(context, control, container, null);
		}

		/// <summary>
		/// Retrieves the cache key based on the current property values.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="control"></param>
		/// <param name="container"></param>
		/// <param name="getDefaultKey"></param>
		/// <returns></returns>
		public string GetCacheKey(HttpContext context, Control control, object container, Converter<string, string> getDefaultKey)
		{
			string cacheKey = KeyFormat;

			if (Parameters != null && Parameters.Count > 0)
			{
				// use the parameters collection to build the dependency
				IOrderedDictionary values = Parameters.GetValues(context, control);

				if (container != null)
				{
					// process CacheItemParameter objects, lookup the value based on the container
					foreach (Parameter param in Parameters)
					{
						if (param is CacheItemParameter)
						{
							string format = (param as CacheItemParameter).Format;
							string propertyName = (param as CacheItemParameter).PropertyName;
							string result = DataBinder.Eval(container, propertyName, format);

							if (!string.IsNullOrEmpty(result))
							{
								values[param.Name] = result;
							}
						}
					}
				}

				if (!string.IsNullOrEmpty(KeyFormat))
				{
					foreach (DictionaryEntry entry in values)
					{
						if (entry.Key != null)
						{
							string key = entry.Key.ToString().Trim();

							if (!key.StartsWith("@"))
							{
								key = "@" + key;
							}

							cacheKey = Regex.Replace(cacheKey, key, "{0}".FormatWith(entry.Value), RegexOptions.IgnoreCase);
						}
					}
				}
			}
			else if (!string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(PropertyName))
			{
				string result = null;

				if (container != null)
				{
					try
					{
						result = DataBinder.Eval(container, PropertyName, "{0}");
					}
					catch (Exception e)
					{
						Tracing.FrameworkError("CacheKeyDependency", "GetCacheKey", "Invalid cache parameter settings.");
						Tracing.FrameworkError("CacheKeyDependency", "GetCacheKey", e.ToString());
					}
				}

				// use this object to build the dependency
				string key = Name.Trim();

				if (!key.StartsWith("@"))
				{
					key = "@" + key;
				}

				cacheKey = Regex.Replace(cacheKey, key, result ?? string.Empty, RegexOptions.IgnoreCase);
			}

			if (string.IsNullOrEmpty(cacheKey))
			{
				// could not find a suitable cacheKey from the parameters, build a default key
				cacheKey = "Adxstudio:{0}:ID={1}".FormatWith(control.GetType().FullName, control.ID);

				// provide an opportunity to override this suggested default
				if (getDefaultKey != null)
				{
					cacheKey = getDefaultKey(cacheKey);
				}
			}

			if (VaryByUser)
			{
				IIdentity identity;

				if (TryGetCurrentIdentity(out identity) && identity.IsAuthenticated)
				{
					cacheKey += ":Identity={0}".FormatWith(identity.Name);
				}
			}

			if (!string.IsNullOrEmpty(VaryByParam))
			{
				foreach (string section in VaryByParam.Split(_varySeparator))
				{
					string param = section.Trim();
					cacheKey += ":{0}={1}".FormatWith(param, context.Request[param]);
				}
			}

			return cacheKey;
		}

		private static bool TryGetCurrentIdentity(out IIdentity identity)
		{
			if (Thread.CurrentPrincipal != null && Thread.CurrentPrincipal.Identity != null)
			{
				identity = Thread.CurrentPrincipal.Identity;

				return true;
			}

			if (HttpContext.Current.User != null && HttpContext.Current.User.Identity != null)
			{
				identity = HttpContext.Current.User.Identity;

				return true;
			}

			if (ServiceSecurityContext.Current != null && ServiceSecurityContext.Current.PrimaryIdentity != null)
			{
				identity = ServiceSecurityContext.Current.PrimaryIdentity;

				return true;
			}

			identity = null;

			return false;
		}

		#region IStateManager Members

		bool IStateManager.IsTrackingViewState
		{
			get { return IsTrackingViewState; }
		}

		public bool IsTrackingViewState { get; private set; }

		void IStateManager.LoadViewState(object savedState)
		{
			LoadViewState(savedState);
		}

		protected virtual void LoadViewState(object savedState)
		{
			object[] state = savedState as object[];

			if (state == null)
			{
				return;
			}

			KeyFormat = state[0] as string;
			((IStateManager)Parameters).LoadViewState(state[1]);
		}

		object IStateManager.SaveViewState()
		{
			return SaveViewState();
		}

		protected virtual object SaveViewState()
		{
			object[] state = new object[5];
			state[0] = KeyFormat;
			state[1] = ((IStateManager)Parameters).SaveViewState();

			return state;
		}

		void IStateManager.TrackViewState()
		{
			TrackViewState();
		}

		protected virtual void TrackViewState()
		{
			IsTrackingViewState = true;
			((IStateManager)Parameters).TrackViewState();
		}

		#endregion
	}
}
