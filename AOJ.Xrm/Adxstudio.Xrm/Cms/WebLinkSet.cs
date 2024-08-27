/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Adxstudio.Xrm.Web.Mvc;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Cms
{
	internal class WebLinkSet : IWebLinkSet
	{
		public WebLinkSet(Entity entity, IPortalViewEntity viewEntity, IEnumerable<IWebLink> webLinks)
		{
			if (entity == null)
			{
				throw new ArgumentNullException("entity");
			}

			if (viewEntity == null)
			{
				throw new ArgumentNullException("viewEntity");
			}

			if (webLinks == null)
			{
				throw new ArgumentNullException("webLinks");
			}

			Entity = entity;
			ViewEntity = viewEntity;
			WebLinks = webLinks.ToArray();

			Copy = viewEntity.GetAttribute("adx_copy");
			Name = entity.GetAttributeValue<string>("adx_name");
			Title = viewEntity.GetAttribute("adx_title");
			DisplayName = entity.GetAttributeValue<string>("adx_display_name");			
		}

		/// <summary>
		/// Gets the DisplayName attribute of the entity record
		/// </summary>
		public string DisplayName { get; }

		public string Description
		{
			get { return Name; }
		}

		public bool Editable
		{
			get { return ViewEntity.Editable; }
		}

		public EntityReference EntityReference
		{
			get { return ViewEntity.EntityReference; }
		}

		public IPortalViewAttribute Copy { get; }

		public Entity Entity { get; }

		public string Name { get; }

		public IPortalViewAttribute Title { get; }

		public string Url
		{
			get { return null; }
		}

		public IEnumerable<IWebLink> WebLinks { get; }

		protected IPortalViewEntity ViewEntity { get; }

		public IPortalViewAttribute GetAttribute(string attributeLogicalName)
		{
			return ViewEntity.GetAttribute(attributeLogicalName);
		}
	}
}
