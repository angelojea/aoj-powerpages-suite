/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm
{
	using System;
	using System.Diagnostics;
	using System.Diagnostics.Tracing;
	using Configuration;
	using Core.Telemetry.EventSources;
	using Diagnostics.Trace;

	public static class ADXTrace
	{

		[NonEvent]
		public static void TraceError(TraceCategory category, string message, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "", [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "", [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
		{
		}

		/// <summary>
		/// Trace Warning emitting ETW event. Dont need to pass details for member, class and line number. It will be provided by compiler services.
		/// </summary>
		/// <param name="category">The trace category.</param>
		/// <param name="message">The trace message.</param>
		/// <param name="memberName">For internal use.</param>
		/// <param name="sourceFilePath">For internal use.</param>
		/// <param name="sourceLineNumber">For internal use.</param>
		[NonEvent]
		public static void TraceWarning(TraceCategory category, string message, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "", [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "", [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
		{
		}

		/// <summary>
		/// Trace Info emitting ETW event. Dont need to pass details for member, class and line number. It will be provided by compiler services.
		/// </summary>
		/// <param name="category">The trace category.</param>
		/// <param name="message">The trace message.</param>
		/// <param name="memberName">For internal use.</param>
		/// <param name="sourceFilePath">For internal use.</param>
		/// <param name="sourceLineNumber">For internal use.</param>
		[NonEvent]
		public static void TraceInfo(TraceCategory category, string message, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "", [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "", [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
		{
		}

		/// <summary>
		/// Trace Verbose emitting ETW event. Dont need to pass details for member, class and line number. It will be provided by compiler services.
		/// </summary>
		/// <param name="category">The trace category.</param>
		/// <param name="message">The trace message.</param>
		/// <param name="memberName">For internal use.</param>
		/// <param name="sourceFilePath">For internal use.</param>
		/// <param name="sourceLineNumber">For internal use.</param>
		[NonEvent]
		public static void TraceVerbose(TraceCategory category, string message, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "", [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "", [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
		{
		}

	}
}
