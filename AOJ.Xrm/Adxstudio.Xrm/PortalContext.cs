/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Net;
using System.Web.Routing;
using Adxstudio.Xrm.AspNet.Identity;
using Adxstudio.Xrm.Services;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Web;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using Adxstudio.Xrm.Web;
using Adxstudio.Xrm.Web.Routing;
using AOJ.Configuration;

namespace Adxstudio.Xrm
{
    /// <summary>
    /// Contains the <see cref="Entity"/> instances that are relevant to a single portal page request.
    /// </summary>
    public class PortalContext : Microsoft.Xrm.Portal.PortalContext
    {
        private readonly OrganizationServiceContext _svc;

        public override Entity User { get; }

        public override Entity Entity { get; }

        private readonly CrmSiteMapNode _node;

        public override HttpStatusCode StatusCode
        {
            get { return _node != null ? _node.StatusCode : HttpStatusCode.OK; }
        }

        public override string Path
        {
            get { return _node != null ? _node.RewriteUrl : null; }
        }

        public override Entity Website { get; }

        public PortalContext(string contextName)
            : base(contextName)
        {
            // Lazy-load context information.
            User = GetUser();
            _node = GetNode(Request);
            Entity = GetEntity(ServiceContext, _node);
            Website = GetWebsite();
        }

        public PortalContext(OrganizationServiceContext request, IOrganizationService svc, Guid portalId, Guid userId)
            : base(request)
        {
            User = svc.Retrieve("contact", userId, new ColumnSet(true));
            _node = null;
            Entity = null;
            Website = svc.Retrieve("mspp_website", portalId, new ColumnSet(true));
        }

        public PortalContext(OrganizationServiceContext request, Entity website, Entity user, Entity contextEntity)
            : base(request)
        {
            User = user;
            _node = null;
            Entity = contextEntity;
            Website = website;
        }

        public PortalContext(string contextName, RequestContext request)
            : base(contextName, request)
        {
            // Lazy-load context information.
            User = GetUser();
            _node = GetNode(Request);
            Entity = GetEntity(ServiceContext, _node);
            Website = GetWebsite();
        }

        private Entity GetWebsite()
        {
            var website = Request.HttpContext.GetWebsite();

            var entity = website.Entity.Clone();
            ServiceContext.ReAttach(entity);
            return entity;
        }

        private Entity GetUser()
        {
            var identity = Request.HttpContext.User.Identity;

            if (!identity.IsAuthenticated) return null;

            var user = Request.HttpContext.GetUser();

            if (user == null || user == CrmUser.Anonymous) return null;

            var contact = ServiceContext.RetrieveSingle(user.ContactId, new ColumnSet(true));

            return contact;
        }

        private Entity GetUser(Guid userId)
        {
            var identity = Request.HttpContext.User.Identity;

            if (!identity.IsAuthenticated) return null;

            var user = Request.HttpContext.GetUser();

            if (user == null || user == CrmUser.Anonymous) return null;

            var contact = ServiceContext.RetrieveSingle(user.ContactId, new ColumnSet(true));

            return contact;
        }

        private static CrmSiteMapNode GetNode(RequestContext request)
        {
            if (request == null) return null;

            var node = request.HttpContext.GetNode();

            return node ?? PortalRouteHandler.GetNode(request);
        }

        private static Entity GetEntity(OrganizationServiceContext context, CrmSiteMapNode node)
        {
            return node != null ? context.MergeClone(node.Entity) : null;
        }
    }
}