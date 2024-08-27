/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Core.Telemetry.EventSources
{
	using System;
	using System.Diagnostics.Tracing;
	using System.Globalization;
	using System.Web;
	using Adxstudio.Xrm.AspNet.Cms;
	using Decorators;
	using Diagnostics.Trace;
	using Web;
	using Microsoft.AspNet.Identity.Owin;
	using Microsoft.Owin;
	public abstract class EventSourceBase : EventSource
	{
		private const int DefaultLcid = 0;

		/// <summary>
		/// Session ID from the Context
		/// </summary>
		protected string SessionId
		{
			get
			{
				try
				{
					var context = HttpContext;
					if (context == null)
						return string.Empty;

					return context.Session == null
						? string.Empty
						: context.Session.SessionID ?? string.Empty;
				}
				catch
				{
					// eat exception expecting it to possibly happen in scenarios without context
					// i.g., background worker threads
					return string.Empty;
				}
			}
		}

	    /// <summary>
	    /// User ID from the Context
	    /// </summary>
	    protected string UserId
	    {
	        get
	        {
	            try
	            {
                    var context = HttpContextBase;

	                if (context == null)
	                    return string.Empty;

	                return HashPii.GetHashedUserId(context);
                }
	            catch
	            {
	                // eat exception expecting it to possibly happen in scenarios without context
	                // i.g., background worker threads
	                return string.Empty;
	            }
	        }
	    }
		
        /// <summary>
        /// PersistentCookie value from the Context
        /// </summary>
        protected string PersistentCookie
		{
			get
			{
				try
				{
					var inspector = CookieInspector.GetInstance(HttpContextBase);
					var cookieValue = inspector.AreCookiesEnabled
						? inspector.GetCookieValue(CookieInspector.AnalyticsCookieKey)
						: "cookies are disabled";

					return string.IsNullOrEmpty(cookieValue)
						? "cookie not found"
						: cookieValue;
				}
				catch (Exception e)
				{
					WebEventSource.Log.GenericWarningException(e, "error acquiring cookies from HttpContext");
					return "error";
				}
			}
		}

		/// <summary>
		/// Azure Portal Url
		/// </summary>
		protected string PortalUrl
		{
			get { return PortalDetail.Instance.AzurePortalUrl; }
		}

		/// <summary>
		/// Portal Version
		/// </summary>
		protected string PortalVersion
		{
			get { return PortalDetail.Instance.PortalVersion; }
		}

		/// <summary>
		/// Production or Trial environment
		/// </summary>
		protected string ProductionOrTrial
		{
			get { return PortalDetail.Instance.PortalProductionOrTrialType; }
		}

		/// <summary>
		/// LCID from the Portal
		/// </summary>
		protected int Lcid
		{
			get
			{
				try
				{
					var context = HttpContext;
					if (context == null)
						return DefaultLcid;

					if (OwinContext == null)
						return DefaultLcid;

					var language = OwinContext.Get<ContextLanguageInfo>();
					if (language == null)
						return DefaultLcid;

					return language.IsCrmMultiLanguageEnabled
						? language.ContextLanguage.Lcid
						: DefaultLcid;
				}
				catch
				{
					// eat exception expecting it to possibly happen in scenarios without context
					// i.g., background worker threads
					return DefaultLcid;
				}
			}
		}

		/// <summary>
		/// LCID from CRM
		/// </summary>
		protected int CrmLcid
		{
			get
			{
				try
				{
					var context = HttpContext;
					if (context == null)
						return DefaultLcid;
					
					if (OwinContext == null)
						return DefaultLcid;

					var language = OwinContext.Get<ContextLanguageInfo>();
					if (language == null)
						return DefaultLcid;

					return language.IsCrmMultiLanguageEnabled
						? language.ContextLanguage.CrmLcid
						: DefaultLcid;
				}
				catch
				{
					// eat exception expecting it to possibly happen in scenarios without context
					// i.g., background worker threads
					return DefaultLcid;
				}
			}
		}

		/// <summary>
		/// Gets the activity Id to associate to the event
		/// </summary>
		/// <returns>Current Thread Activity Id</returns>
		[NonEvent]
		protected string GetActivityId()
		{
			var guid = CurrentThreadActivityId;
			return guid.ToString();
		}

		/// <summary>
		/// Gets the elapsed time for the current request
		/// </summary>
		/// <returns>elapsed time in MS as a string</returns>
		[NonEvent]
		protected string ElapsedTime()
		{
			try
			{
				if (HttpContext == null)
					return string.Empty;

				if (OwinContext == null || OwinContext.Get<RequestElapsedTimeContext>() == null)
				{
					var inspector = ItemDecorator.GetInspectorInstance(HttpContextBase);
					var startTime = inspector[ItemDecorator.RequestStartTime];
					if (startTime != null)
					{
						var requestStartTime = (DateTime)startTime;
						return (DateTime.UtcNow - requestStartTime).TotalMilliseconds.ToString(CultureInfo.InvariantCulture);
					}

					return string.Empty;
				}

				return OwinContext.Get<RequestElapsedTimeContext>().ElapsedTime().ToString();
			}
			catch
			{
				// eat exception expecting it to possibly happen in scenarios without context
				// i.g., background worker threads
				return string.Empty;
			}
		}
		
		[NonEvent]
		protected void WriteEvent<TEnum>(TEnum eventName, params object[] eventData) where TEnum : struct, IComparable, IFormattable
		{
			var eventId = Convert.ToInt32(eventName, CultureInfo.InvariantCulture);

			base.WriteEvent(eventId, eventData);
		}

		/// <summary>
		/// Attempts to retrieve the OwinContext
		/// </summary>
		private IOwinContext OwinContext
		{
			get
			{
				try
				{
					if (HttpContext == null || !HttpContext.Items.Contains("owin.Environment"))
						return null;

					return HttpContext.GetOwinContext();
				}
				catch
				{
					return null;
				}
			}
		}

		/// <summary>
		/// Returns the HttpContextWrapper
		/// </summary>
		protected HttpContextBase HttpContextBase
		{
			get
			{
				if (HttpContext == null) return null;
				return new HttpContextWrapper(HttpContext);
			}
		}

		/// <summary>
		/// Returns the HttpContext
		/// </summary>
		private HttpContext HttpContext
		{
			get
			{
				try
				{
					return HttpContext.Current;
				}
				catch
				{
					// eat exception expecting it to possibly happen in scenarios without context
					// i.g., background worker threads
					return null;
				}
			}
		}
	}
}
