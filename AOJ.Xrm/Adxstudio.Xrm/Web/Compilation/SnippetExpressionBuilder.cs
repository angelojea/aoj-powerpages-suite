/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Web.Compilation
{
	using System;
	using System.Collections.Specialized;
	using System.Web;
	using Cms;
	using Resources;
	using Microsoft.Xrm.Portal.Web.Compilation;
	using Microsoft.Xrm.Sdk;

	/// <summary>
	/// New custom expression builder for Snippets
	/// </summary>
	public sealed class SnippetExpressionBuilder : CrmExpressionBuilder<SnippetExpressionBuilder.Provider>
	{
		public class Provider : IExpressionBuilderProvider
		{
			object IExpressionBuilderProvider.Evaluate(NameValueCollection arguments, Type controlType, string propertyName, string expressionPrefix)
			{
				if (string.IsNullOrEmpty(arguments.GetValueByIndexOrName(0, "Name")))
				{
					ThrowArgumentException(propertyName, expressionPrefix, "Name={snippet name} [, Key={ResourceManager key}] [, Format={format string}] [, Portal={portal name}]");
				}

				string valueByIndexOrName1 = arguments.GetValueByIndexOrName(0, "Name");
				string key = arguments.GetValueByIndexOrName(1, "Key") ?? string.Empty;
				string valueByIndexOrName2 = arguments.GetValueByIndexOrName(2, "Format");
				Type returnType = GetReturnType(controlType, propertyName);

				var adapter = new SnippetDataAdapter(new PortalConfigurationDataAdapterDependencies());
				var snippet = adapter.Select(valueByIndexOrName1);

				var snippetByName = snippet == null ? null : snippet.Entity;

				if (Microsoft.Xrm.Client.TypeExtensions.IsA(returnType, typeof(Entity)))
				{
					return snippetByName;
				}

				if (snippetByName == null)
				{
					string value = ResourceManager.GetString(key);
					if (value == null)
					{
						return key;
					}

					return value;
				}

				return GetEvalData(snippetByName, "adx_value", null, valueByIndexOrName2, returnType);
			}
		}
	}
}

