/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Specialized;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Cms;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;

namespace Microsoft.Xrm.Portal.Web.Compilation
{
	/// <summary>
	/// Expression builder for retrieving snippet content for the current portal.
	/// </summary>
	/// <remarks>
	/// <example>
	/// Configuration.
	/// <code>
	/// <![CDATA[
	/// <configuration>
	/// 
	///  <system.web>
	///   <compilation>
	///    <expressionBuilders>
	///     <add expressionPrefix="Snippet" type="Microsoft.Xrm.Portal.Web.Compilation.SnippetExpressionBuilder, Microsoft.Xrm.Portal"/>
	///    </expressionBuilders>
	///   </compilation>
	///  
	/// </configuration>
	/// ]]>
	/// </code>
	/// Usage in ASPX page.
	/// <code>
	/// <![CDATA[
	/// <asp:Label runat="server" Text='<%$ Snippet: Name={snippet name} [, Default={default text}] [, Format={format string}] [, Portal={portal name}] %>'/>
	/// <asp:Label runat="server" Text='<%$ Snippet: Name=Profile, Format=-{0}-, Default=My Profile %>'/>
	/// ]]>
	/// </code>
	/// </example>
	/// </remarks>
	public sealed class SnippetExpressionBuilder : CrmExpressionBuilder<SnippetExpressionBuilder.Provider>
	{
		public class Provider : IExpressionBuilderProvider
		{
			object IExpressionBuilderProvider.Evaluate(NameValueCollection arguments, Type controlType, string propertyName, string expressionPrefix)
			{
				if (string.IsNullOrEmpty(arguments.GetValueByIndexOrName(0, "Name")))
				{
					ThrowArgumentException(propertyName, expressionPrefix, "Name={snippet name} [, Default={default text}] [, Format={format string}] [, Portal={portal name}]");
				}

				var snippetName = arguments.GetValueByIndexOrName(0, "Name");
				var defaultValue = arguments.GetValueByIndexOrName(1, "Default") ?? string.Empty;
				var format = arguments.GetValueByIndexOrName(2, "Format");
				var portalName = arguments.GetValueByIndexOrName(3, "Portal");

				var portal = PortalCrmConfigurationManager.CreatePortalContext(portalName);
				var context = portal.ServiceContext;
				var website = portal.Website;

				var snippet = context.GetSnippetByName(website, snippetName);

				var returnType = GetReturnType(controlType, propertyName);
				if (returnType.IsA(typeof(Entity))) return snippet;
				if (snippet == null) return defaultValue;

				return GetEvalData(snippet, "mspp_value", null, format, returnType);
			}
		}
	}
}
