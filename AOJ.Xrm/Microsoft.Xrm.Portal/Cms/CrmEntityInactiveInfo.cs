/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;

namespace Microsoft.Xrm.Portal.Cms
{
	internal sealed class CrmEntityInactiveInfo
	{
		private static readonly IDictionary<string, CrmEntityInactiveInfo> InfoByLogicalName = new Dictionary<string, CrmEntityInactiveInfo>
		{
			{ "mspp_webfile", new CrmEntityInactiveInfo("mspp_webfile", "statecode", 1, "statuscode", 2) },
			{ "mspp_weblink", new CrmEntityInactiveInfo("mspp_weblink", "statecode", 1, "statuscode", 2) },
			{ "mspp_webpage", new CrmEntityInactiveInfo("mspp_webpage", "statecode", 1, "statuscode", 2) },
		};

		public CrmEntityInactiveInfo(string entityName, string statePropertyName, int inactiveState, string statusPropertyName, int inactiveStatus)
		{
			EntityName = entityName;
			StatePropertyName = statePropertyName;
			InactiveState = inactiveState;
			InactiveStatus = inactiveStatus;
			StatusPropertyName = statusPropertyName;
		}

		public string EntityName { get; }

		public int InactiveState { get; private set; }

		public int InactiveStatus { get; }

		public string StatePropertyName { get; private set; }

		public string StatusPropertyName { get; }

		public bool IsInactive(Entity entity)
		{
			entity.AssertEntityName(EntityName);

			var status = entity.GetAttributeValue<int?>(StatusPropertyName);

			return status.HasValue && status.Value == InactiveStatus;
		}

		public static bool TryGetInfo(string entityName, out CrmEntityInactiveInfo info)
		{
			return InfoByLogicalName.TryGetValue(entityName, out info);
		}
	}
}
