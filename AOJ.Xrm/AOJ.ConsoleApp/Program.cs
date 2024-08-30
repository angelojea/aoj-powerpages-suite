using AOJ.Xrm.Common;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using System;

namespace AOJ.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var url = "https://orgd53f2c17.crm.dynamics.com/";
            var clientId = "af246865-349f-45d9-bedf-018f05fd38a4";
            var clientSecret = "Qka8Q~ZjcDiKiLiKTsVtYaGEXPRCgFq.d3hzDbio";

            using (var client = new CrmServiceClient(
                $@"AuthenticationType=ClientSecret; url={url}; ClientId={clientId}; ClientSecret={clientSecret};"))
            {
                while (!client.IsReady)
                {
                    
                }

                var rendered = new AOJRenderer(client, Guid.Parse("ce496a9b-0b4b-4d8f-8f0e-7e08c21c5715"), Guid.Parse("2ba29921-1b5c-ef11-bfe2-000d3a56777a"))
                        .RenderLiquid(@"
{%- fetchxml fetchXml -%}
<fetch version=""1.0"" output-format=""xml-platform"" mapping=""logical"" distinct=""false"">
    <entity name=""contact"">
      <attribute name=""contactid"" />
      <attribute name=""fullname"" />
      <attribute name=""firstname"" />
      <attribute name=""lastname"" />
      <attribute name=""emailaddress1"" />
      <attribute name=""telephone1"" />
    </entity>
  </fetch>
{%- endfetchxml -%}
[
  {% for row in fetchXml.results.entities %}
  {
      ""id"": ""{{row.contactid}}"",
      ""name"": ""{{row.fullname}}"",
      ""email"": ""{{row.emailaddress1}}"",
      ""phone"": ""{{row.telephone1}}""
  }{% unless forloop.last %},{% endunless %}
  {% endfor %}
]
");
                Console.WriteLine(rendered);
                Console.ReadLine();

                rendered = new AOJRenderer(new OrganizationService(client), Guid.Parse("ce496a9b-0b4b-4d8f-8f0e-7e08c21c5715"), Guid.Parse("2ba29921-1b5c-ef11-bfe2-000d3a56777a"))
                        .RenderEntityForm(Guid.Parse("e0ce2b35-875e-ef11-bfe3-000d3a56777a"));

                Console.WriteLine(rendered);
                Console.ReadLine();

                rendered = new AOJRenderer(new OrganizationService(client), Guid.Parse("ce496a9b-0b4b-4d8f-8f0e-7e08c21c5715"), Guid.Parse("2ba29921-1b5c-ef11-bfe2-000d3a56777a"))
                        .RenderEntityList(Guid.Parse("72194b7b-aa62-ef11-bfe2-6045bdff7792"));

                Console.WriteLine(rendered);
                Console.ReadLine();
            }
        }
    }
}