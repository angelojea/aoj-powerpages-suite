/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Services.Cache
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Web.Hosting;
	using System.Web.Security;
	using AspNet;
	using IO;
	using Json;
	using Performance;
	using Threading;
	using Web;
	using Microsoft.Xrm.Client;
	using Microsoft.Xrm.Sdk;

	/// <summary>
	/// A job that warms up the cache from disk files.
	/// </summary>
	public class WarmupCacheJob : FluentSchedulerJob
	{
		/// <summary>
		/// The context.
		/// </summary>
		public CrmDbContext Context { get; }

		/// <summary>
		/// The settings.
		/// </summary>
		public WarmupCacheSettings Settings { get; }

		/// <summary>
		/// The full path of the input folder.
		/// </summary>
		public string AppDataFullPath { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="WarmupCacheJob" /> class.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="settings">The settings.</param>
		public WarmupCacheJob(CrmDbContext context, WarmupCacheSettings settings)
		{
			Context = context;
			Settings = settings;
			AppDataFullPath = settings.AppDataPath.StartsWith("~/")
				? HostingEnvironment.MapPath(settings.AppDataPath)
				: settings.AppDataPath;
		}

		/// <summary>
		/// The body.
		/// </summary>
		/// <param name="id">The activity id.</param>
		protected override void ExecuteInternal(Guid id)
		{
			var exceptions = new List<Exception>();

			using (PerformanceProfiler.Instance.StartMarker(PerformanceMarkerName.Cache, PerformanceMarkerArea.Cms, PerformanceMarkerTagName.WarmupCache))
			{
				if (!Settings.AppDataRetryPolicy.DirectoryExists(AppDataFullPath))
				{
					return;
				}

				var directory = Settings.AppDataRetryPolicy.GetDirectory(AppDataFullPath);
				var files = directory.GetFiles(Settings.FilenameFormat.FormatWith("*"));
				var expiresOn = DateTimeOffset.UtcNow - Settings.ExpirationWindow;

				foreach (var file in files)
				{
					try
					{
						var expired = file.LastWriteTimeUtc < expiresOn;

						if (!expired)
						{
							ADXTrace.TraceInfo(TraceCategory.Application, "Reading: " + file.FullName);

							var bytes = Settings.AppDataRetryPolicy.ReadAllBytes(file.FullName);
							var json = Encoding.UTF8.GetString(MachineKey.Unprotect(bytes, Settings.GetType().ToString()));
							var request = CrmJsonConvert.DeserializeObject(json) as OrganizationRequest;

							if (request != null)
							{
								Context.Service.ExecuteRequest(request);
							}
						}
					}
					catch (Exception e)
					{
						WebEventSource.Log.GenericWarningException(e);

						if (Settings.PropagateExceptions)
						{
							exceptions.Add(e);
						}
					}
				}
			}

			if (Settings.PropagateExceptions && exceptions.Any())
			{
				throw new AggregateException(exceptions);
			}
		}

		/// <summary>
		/// Error event.
		/// </summary>
		/// <param name="e">The error.</param>
		protected override void OnError(Exception e)
		{
			WebEventSource.Log.GenericWarningException(e);
		}
	}
}
