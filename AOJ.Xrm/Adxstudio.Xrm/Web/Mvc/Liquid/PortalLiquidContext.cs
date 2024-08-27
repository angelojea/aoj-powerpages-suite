/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Adxstudio.Xrm.AspNet.Cms;
using Adxstudio.Xrm.Metadata;
using Adxstudio.Xrm.Resources;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
    public class PortalLiquidContext : IPortalLiquidContext
    {
        public PortalLiquidContext(HtmlHelper html, IPortalViewContext portalViewContext)
        {
            OrganizationMoneyFormatInfo = new AojOrganizationMoneyFormatInfo();
            Random = new Random();
            PortalViewContext = portalViewContext;
            ContextLanguageInfo = null;
        }

        public PortalLiquidContext(IOrganizationService svc, IPortalViewContext portalViewContext)
        {
            OrganizationMoneyFormatInfo = new AojOrganizationMoneyFormatInfo();
            Random = new Random();
            PortalViewContext = portalViewContext;
            ContextLanguageInfo = null;
            PortalOrganizationService = svc;
        }

        public PortalLiquidContext(IOrganizationService svc)
        {
            OrganizationMoneyFormatInfo = new AojOrganizationMoneyFormatInfo();
            Random = new Random();
            //_contextLanguageInfo = new ContextLanguageInfo();
            ContextLanguageInfo = null;
            PortalOrganizationService = svc;
        }

        public HtmlHelper Html { get; }

        public IOrganizationMoneyFormatInfo OrganizationMoneyFormatInfo { get; }

        public IPortalViewContext PortalViewContext { get; }

        public Random Random { get; }

        public ContextLanguageInfo ContextLanguageInfo { get; }

        public IOrganizationService PortalOrganizationService { get; }

        public UrlHelper UrlHelper => new UrlHelper();

        private IOrganizationMoneyFormatInfo GetOrganizationMoneyFormatInfo()
        {
            return new OrganizationMoneyFormatInfo(PortalViewContext);
        }

        private static ContextLanguageInfo GetContextLanguageInfo()
        {
            var current = HttpContext.Current;

            if (current == null)
            {
                throw new InvalidOperationException("Unable to retrieve the current HTTP context.");
            }

            var contextLanguageInfo = current.GetContextLanguageInfo();
            return contextLanguageInfo;
        }

        private static IOrganizationService GetPortalOrganizationService()
        {
            var current = HttpContext.Current;

            if (current == null)
            {
                throw new InvalidOperationException("Unable to retrieve the current HTTP context.");
            }

            var portalOrgService = current.GetOrganizationService();
            return portalOrgService;
        }
    }

    public class LocalOrganizationMoneyFormatInfo : IOrganizationMoneyFormatInfo
    {
        public int? PricingDecimalPrecision => 2;

        public int? CurrencyPrecision => 2;

        public string CurrencySymbol => "$";
    }

    public class AojOrganizationMoneyFormatInfo : IOrganizationMoneyFormatInfo
    {
        public int? PricingDecimalPrecision => 2;

        public int? CurrencyPrecision => 2;

        public string CurrencySymbol => "$";
    }
}
