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
        private readonly IOrganizationMoneyFormatInfo _organizationMoneyFormatInfo;
        private readonly Random _random;
        private readonly ContextLanguageInfo _contextLanguageInfo;
        private readonly IOrganizationService _portalOrganizationService;
        public PortalLiquidContext(HtmlHelper html, IPortalViewContext portalViewContext)
        {
            _organizationMoneyFormatInfo = new AojOrganizationMoneyFormatInfo();
            _random = new Random();
            PortalViewContext = portalViewContext;
            _contextLanguageInfo = null;
        }

        public PortalLiquidContext(IOrganizationService svc, IPortalViewContext portalViewContext)
        {
            _organizationMoneyFormatInfo = new AojOrganizationMoneyFormatInfo();
            _random = new Random();
            PortalViewContext = portalViewContext;
            _contextLanguageInfo = null;
            _portalOrganizationService = svc;
        }

        public PortalLiquidContext(IOrganizationService svc)
        {
            _organizationMoneyFormatInfo = new AojOrganizationMoneyFormatInfo();
            _random = new Random();
            //_contextLanguageInfo = new ContextLanguageInfo();
            _contextLanguageInfo = null;
            _portalOrganizationService = svc;
        }

        public HtmlHelper Html { get; private set; }

        public IOrganizationMoneyFormatInfo OrganizationMoneyFormatInfo
        {
            get { return _organizationMoneyFormatInfo; }
        }

        public IPortalViewContext PortalViewContext { get; private set; }

        public Random Random
        {
            get { return _random; }
        }

        public ContextLanguageInfo ContextLanguageInfo
        {
            get { return _contextLanguageInfo; }
        }

        public IOrganizationService PortalOrganizationService
        {
            get { return _portalOrganizationService; }
        }

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
