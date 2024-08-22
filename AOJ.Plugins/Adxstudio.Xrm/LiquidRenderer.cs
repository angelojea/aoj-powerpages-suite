using Adxstudio.Xrm.Performance;
using Adxstudio.Xrm.Web.Mvc.Liquid;
using Adxstudio.Xrm.Web.Mvc.Liquid.Tags;
using DotLiquid;
using DotLiquid.Exceptions;
using DotLiquid.NamingConventions;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Adxstudio.Xrm
{
    public static class LiquidRenderer
    {
        static LiquidRenderer()
        {
            InitializeDotLiquid();

            Template.RegisterTag<Editable>("editable");
            Template.RegisterTag<Web.Mvc.Liquid.Tags.EntityList>("entitylist");
            Template.RegisterTag<Web.Mvc.Liquid.Tags.EntityForm>("entityform");
            Template.RegisterTag<WebForm>("webform");
            Template.RegisterTag<EntityView>("entityview");
            Template.RegisterTag<FetchXml>("fetchxml");
            Template.RegisterTag<OutputCache>("outputcache");
            Template.RegisterTag<SearchIndex>("searchindex");
            Template.RegisterTag<Chart>("chart");
            Template.RegisterTag<Redirect>("redirect");
            Template.RegisterTag<Rating>("rating");
            Template.RegisterTag<Substitution>("substitution");

            Template.RegisterFilter(typeof(Filters));
            Template.RegisterFilter(typeof(DateFilters));
            Template.RegisterFilter(typeof(EntityListFilters));
            Template.RegisterFilter(typeof(EnumerableFilters));
            Template.RegisterFilter(typeof(MathFilters));
            Template.RegisterFilter(typeof(TypeFilters));
            Template.RegisterFilter(typeof(StringFilters));
            Template.RegisterFilter(typeof(NumberFormatFilters));
            Template.RegisterFilter(typeof(UrlFilters));
            Template.RegisterFilter(typeof(SearchFilterOptionFilters));

            Template.NamingConvention = new InvariantCultureNamingConvention();
        }

        private static void InitializeDotLiquid()
        {
            // Force a call to the static constructor.
            var temp = DotLiquid.Liquid.UseRubyDateFormat;
        }

        public static string Liquid(string source, Context context)
        {
            using (var output = new StringWriter())
            {
                InternalRenderLiquid(source, null, output, context);
                return output.ToString();
            }
        }

        private static void InternalRenderLiquid(string source, string sourceIdentifier, TextWriter output, Context context)
        {
            Template template;
            if (!string.IsNullOrEmpty(sourceIdentifier))
            {
                sourceIdentifier = string.Format(" ({0})", sourceIdentifier);
            }

            try
            {
                using (PerformanceProfiler.Instance.StartMarker(PerformanceMarkerName.LiquidExtension, PerformanceMarkerArea.Liquid, PerformanceMarkerTagName.LiquidSourceParsed))
                {
                    template = Template.Parse(source);
                }
            }
            catch (SyntaxException e)
            {
                ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Liquid parse error{0}: {1}", sourceIdentifier, e.ToString()));
                output.Write(e.Message);
                return;
            }

            ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Rendering Liquid{0}", sourceIdentifier));

            using (PerformanceProfiler.Instance.StartMarker(PerformanceMarkerName.LiquidExtension, PerformanceMarkerArea.Liquid, PerformanceMarkerTagName.RenderLiquid))
            {
                template.Render(output, RenderParameters.FromContext(context));
            }

            foreach (var error in template.Errors)
            {
                ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("Liquid rendering error{0}: {1}", sourceIdentifier, error.ToString()));
            }
        }

        private class InvariantCultureNamingConvention : INamingConvention
        {
            private readonly Regex _regex1 = new Regex(@"([A-Z]+)([A-Z][a-z])");
            private readonly Regex _regex2 = new Regex(@"([a-z\d])([A-Z])");

            public StringComparer StringComparer
            {
                get { return StringComparer.OrdinalIgnoreCase; }
            }

            public string GetMemberName(string name)
            {
                return _regex2.Replace(_regex1.Replace(name, "$1_$2"), "$1_$2").ToLowerInvariant();
            }
        }

    }


}