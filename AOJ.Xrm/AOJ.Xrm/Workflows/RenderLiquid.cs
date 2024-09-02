using AOJ.Xrm.Common;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AOJ.Xrm.Workflows
{
    public class RenderLiquid : CodeActivity
    {
        [Input("Website Id")]
        public InArgument<string> WebsiteId { get; set; }

        [Input("Liquid")]
        public InArgument<string> Liquid { get; set; }

        [Output("Output")]
        public OutArgument<string> Output { get; set; }

        [Output("Success")]
        public OutArgument<bool> Success { get; set; }


        protected override void Execute(CodeActivityContext executionContext)
        {
            ITracingService tracingService = executionContext.GetExtension<ITracingService>();

            IWorkflowContext context = executionContext.GetExtension<IWorkflowContext>();

            IOrganizationServiceFactory serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            try
            {
                var renderer = new AOJRenderer(
                    new OrganizationService(service),
                    Guid.Parse(WebsiteId.Get(executionContext)),
                    Guid.Parse("2ba29921-1b5c-ef11-bfe2-000d3a56777a")
                );
                Output.Set(
                    executionContext,
                    renderer.RenderLiquid(Liquid.Get(executionContext))
                );
                Success.Set(executionContext, true);
            }
            catch (Exception ex)
            {
                Output.Set(
                    executionContext,
                    ex.Message + "\n\n\n" + ex.StackTrace
                );
                Success.Set(executionContext, false);
            }


            //Entity note = new Entity("annotation");
            //note["subject"] = "Plugin Exception Log";
            //note["notetext"] = $"Exception Message: {ex.Message}\n" +
            //                   $"Stack Trace: {ex.StackTrace}\n" +
            //                   $"Entity: {context.PrimaryEntityName}\n" +
            //                   $"Entity ID: {context.PrimaryEntityId}\n" +
            //                   $"User: {context.InitiatingUserId}";
            //if (context.PrimaryEntityId != Guid.Empty)
            //{
            //    note["objectid"] = new EntityReference(context.PrimaryEntityName, context.PrimaryEntityId);
            //    note["objecttypecode"] = context.PrimaryEntityName;
            //}
            //service.Create(note);
        }
    }

    public class RenderEntityForm : CodeActivity
    {
        [Input("Website Id")]
        public InArgument<string> WebsiteId { get; set; }

        [Input("Entity Form Id")]
        public InArgument<string> EntityFormId { get; set; }

        [Output("Rendered Html")]
        public OutArgument<string> RenderedHtml { get; set; }


        protected override void Execute(CodeActivityContext executionContext)
        {
            ITracingService tracingService = executionContext.GetExtension<ITracingService>();

            IWorkflowContext context = executionContext.GetExtension<IWorkflowContext>();

            IOrganizationServiceFactory serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            var renderer = new AOJRenderer(
                new OrganizationService(service),
                Guid.Parse(WebsiteId.Get(executionContext)),
                Guid.Parse("2ba29921-1b5c-ef11-bfe2-000d3a56777a")
            );
            RenderedHtml.Set(
                executionContext,
                renderer.RenderEntityForm(Guid.Parse(EntityFormId.Get(executionContext)))
            );
        }
    }

    public class RenderEntityList : CodeActivity
    {
        // Define the input argument for Account Name
        [Input("Website Id")]
        public InArgument<string> WebsiteId { get; set; }

        [Input("Entity List Id")]
        public InArgument<string> EntityListId { get; set; }

        // Define the output argument to indicate uniqueness
        [Output("Rendered Html")]
        public OutArgument<string> RenderedHtml { get; set; }


        protected override void Execute(CodeActivityContext executionContext)
        {
            // Create the tracing service
            ITracingService tracingService = executionContext.GetExtension<ITracingService>();

            // Create the context service
            IWorkflowContext context = executionContext.GetExtension<IWorkflowContext>();

            // Create the organization service factory
            IOrganizationServiceFactory serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            var renderer = new AOJRenderer(
                new OrganizationService(service),
                Guid.Parse(WebsiteId.Get(executionContext)),
                Guid.Parse("2ba29921-1b5c-ef11-bfe2-000d3a56777a")
            );
            RenderedHtml.Set(
                executionContext,
                renderer.RenderEntityList(Guid.Parse(EntityListId.Get(executionContext)))
            );
        }
    }
}
