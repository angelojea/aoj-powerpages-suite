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
    // Define the plugin class
    public class RenderLiquid : CodeActivity
    {
        // Define the input argument for Account Name
        [Input("Account Name")]
        [Default("")]
        public InArgument<string> AccountName { get; set; }

        // Define the output argument to indicate uniqueness
        [Output("Is Unique")]
        public OutArgument<bool> IsUnique { get; set; }


        protected override void Execute(CodeActivityContext executionContext)
        {
            // Create the tracing service
            ITracingService tracingService = executionContext.GetExtension<ITracingService>();

            // Create the context service
            IWorkflowContext context = executionContext.GetExtension<IWorkflowContext>();

            // Create the organization service factory
            IOrganizationServiceFactory serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
        }
    }
}
