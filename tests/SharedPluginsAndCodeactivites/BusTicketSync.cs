﻿using System;
using System.ServiceModel;
using DG.Some.Namespace;
using DG.XrmFramework.BusinessDomain.ServiceContext;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

#if !(XRM_MOCKUP_PLUGIN_2011 || XRM_MOCKUP_PLUGIN_2013 || XRM_MOCKUP_PLUGIN_2015 || XRM_MOCKUP_PLUGIN_2016)
namespace DG.NotAKeyWord.SSPOnBoarding.Plugins
{
    public class BusTicketSync : PluginNonDaxif
    {
        /*
         * This is a plugin that isn't registered through DAXIF, therefore we expect that the plugin can be found registered in Metadata.xml, which is
         * generated by MetadataGenerator365. It have to generate metadata from Lab4 solutions, since some tests are dependent on stuff op there and
         * from Pre and Post-images Test Solution from the same Server. This plugin and AccountCurrencyBase are both registered here.
         */

        public BusTicketSync() : base(typeof(BusTicketSync)) { }

        public override void Execute(IServiceProvider serviceProvider)
        {
            var context = new LocalPluginContext(serviceProvider);

            var relationship = context.PluginExecutionContext.InputParameters["Relationship"] as Relationship;
            if (relationship.SchemaName != "dg_bus_parental") return;

            if (context.PluginExecutionContext.Depth > 1)  return;

            var service = context.OrganizationService;
            var id = context.PluginExecutionContext.PrimaryEntityId;

            var snapshotChild = dg_bus.Retrieve(service, id, x => x.dg_Ticketprice);

            if (snapshotChild != null)
                service.Update(new dg_bus(id)
                {
                    dg_Ticketprice = context.PluginExecutionContext.MessageName == "Associate" ? 25 : 26  
                });
        }
    }
}
#endif