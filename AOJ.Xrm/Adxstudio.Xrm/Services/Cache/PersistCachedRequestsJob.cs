/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Services.Cache
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Runtime.Caching;
	using System.Text;
	using System.Web.Hosting;
	using System.Web.Security;
	using IO;
	using Json;
	using Performance;
	using Services;
	using Threading;
	using Web;
	using Microsoft.Xrm.Client;

	/// <summary>
	/// A job that persists cache item requests to disk.
	/// </summary>
	public class PersistCachedRequestsJob : FluentSchedulerJob
	{
		/// <summary>
		/// The settings.
		/// </summary>
		public WarmupCacheSettings Settings { get; }

		/// <summary>
		/// The full path of the output folder.
		/// </summary>
		public string AppDataFullPath { get; }

		/// <summary>
		/// Reference to the object cache.
		/// </summary>
		public ObjectCache ObjectCache { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="PersistCachedRequestsJob" /> class.
		/// </summary>
		/// <param name="objectCache">Reference to the ObjectCache.</param>
		/// <param name="settings">The settings.</param>
		public PersistCachedRequestsJob(ObjectCache objectCache, WarmupCacheSettings settings)
		{
			Settings = settings;
			ObjectCache = objectCache;
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

			using (PerformanceProfiler.Instance.StartMarker(PerformanceMarkerName.Cache, PerformanceMarkerArea.Cms, PerformanceMarkerTagName.PersistCachedRequests))
			{
				Settings.AppDataRetryPolicy.DirectoryCreate(AppDataFullPath);

				// find cached requests for writing to disk
				foreach (var telemetry in ObjectCache.Select(item => item.Value).OfType<CacheItemTelemetry>())
				{
					if (telemetry.Request != null)
					{
						Save(AppDataFullPath, telemetry);
					}
				}

				// cleanup exipired files
				var directory = Settings.AppDataRetryPolicy.GetDirectory(AppDataFullPath);
				var files = Settings.AppDataRetryPolicy.GetFiles(directory, Settings.FilenameFormat.FormatWith("*"));
				var expiresOn = DateTimeOffset.UtcNow - Settings.ExpirationWindow;

				foreach (var file in files)
				{
					try
					{
						var expired = file.LastWriteTimeUtc < expiresOn;

						if (expired)
						{
							ADXTrace.TraceInfo(TraceCategory.Application, "Deleting: " + file.FullName);

							Settings.AppDataRetryPolicy.FileDelete(file);
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

		/// <summary>
		/// Write this item to a folder.
		/// </summary>
		/// <param name="folderPath">The folder path.</param>
		/// <param name="telemetry">The cache item telemetry.</param>
		private void Save(string folderPath, CacheItemTelemetry telemetry)
		{
			if (telemetry.Request != null)
			{
				var request = CrmJsonConvert.SerializeObject(telemetry.Request);
				var key = request.GetHashCode();
				var filename = string.Format(Settings.FilenameFormat, key);
				var fullPath = Path.Combine(folderPath, filename);
				var bytes = MachineKey.Protect(Encoding.UTF8.GetBytes(request), Settings.GetType().ToString());

				ADXTrace.TraceInfo(TraceCategory.Application, "Writing: " + fullPath);

				Settings.AppDataRetryPolicy.WriteAllBytes(fullPath, bytes);
			}
		}
	}
}
