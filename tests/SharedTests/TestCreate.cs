﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Xunit;
using Microsoft.Xrm.Sdk.Query;
using System.ServiceModel;
using Microsoft.Xrm.Sdk.Messages;
using DG.XrmFramework.BusinessDomain.ServiceContext;
using DG.XrmContext;
using Xunit.Sdk;

namespace DG.XrmMockupTest
{
    public class TestCreate : UnitTestBase
    {
        public TestCreate(XrmMockupFixture fixture) : base(fixture) { }

        [Fact]
        public void TestCreateSimple()
        {
            var contact = new Contact()
            {
                FirstName = "John"
            };
            contact.Id = orgAdminService.Create(contact);

            var dbContact = Contact.Retrieve(orgAdminService, contact.Id);
            Assert.Equal(contact.FirstName, dbContact.FirstName);
        }

        [Fact]
        public void TestCreateWithRequest()
        {
            var contact = new Contact()
            {
                FirstName = "John"
            };
            var req = new CreateRequest()
            {
                Target = contact
            };
            var resp = orgAdminService.Execute(req) as CreateResponse;

            var dbContact = Contact.Retrieve(orgAdminService, resp.id);
            Assert.Equal(contact.FirstName, dbContact.FirstName);
        }

        [Fact]
        public void TestCreateWithNamedRequest()
        {
            var contact = new Contact()
            {
                FirstName = "John"
            };
            var req = new OrganizationRequest("Create");
            req.Parameters["Target"] = contact;

            var resp = orgAdminService.Execute(req) as CreateResponse;

            var dbContact = Contact.Retrieve(orgAdminService, resp.id);
            Assert.Equal(contact.FirstName, dbContact.FirstName);
        }


        [Fact]
        public void TestUserCreation()
        {
            using (var context = new Xrm(orgAdminUIService))
            {

                var adminUser = orgAdminUIService.Retrieve("systemuser", crm.AdminUser.Id, new ColumnSet("businessunitid"));
                var adminBusinessunitid = adminUser.GetAttributeValue<EntityReference>("businessunitid").Id;
                var adminBusinessunit = orgAdminUIService.Retrieve("businessunit", adminBusinessunitid, new ColumnSet("name"));


                var user = new Entity("systemuser");
                user.Attributes["firstname"] = "Test User";
                var userid = orgAdminUIService.Create(user);

                var retrievedUser = orgAdminUIService.Retrieve("systemuser", userid, new ColumnSet(true));
                Assert.Equal(user.Attributes["firstname"], retrievedUser.Attributes["firstname"]);
                var businessunitid = retrievedUser.GetAttributeValue<EntityReference>("businessunitid").Id;
                var businessunit = orgAdminUIService.Retrieve("businessunit", businessunitid, new ColumnSet("name"));

                Assert.Equal(adminBusinessunit.Attributes["name"], businessunit.Attributes["name"]);
            }
        }

        [Fact]
        public void TestTeamCreation()
        {
            using (var context = new Xrm(orgAdminUIService))
            {

                var adminUser = orgAdminUIService.Retrieve("systemuser", crm.AdminUser.Id, new ColumnSet("businessunitid"));
                var adminBusinessunitid = adminUser.GetAttributeValue<EntityReference>("businessunitid").Id;
                var adminBusinessunit = orgAdminUIService.Retrieve("businessunit", adminBusinessunitid, new ColumnSet("name"));


                var user = new Entity("team");
                user.Attributes["name"] = "Test Team";
                var userid = orgAdminUIService.Create(user);

                var retrievedTeam = orgAdminUIService.Retrieve("team", userid, new ColumnSet(true));
                Assert.Equal(user.Attributes["name"], retrievedTeam.Attributes["name"]);
                var businessunitid = retrievedTeam.GetAttributeValue<EntityReference>("businessunitid").Id;
                var businessunit = orgAdminUIService.Retrieve("businessunit", businessunitid, new ColumnSet("name"));

                Assert.Equal(adminBusinessunit.Attributes["name"], businessunit.Attributes["name"]);
            }
        }

        [Fact]
        public void TestPopulateWith()
        {
            using (var context = new Xrm(orgAdminUIService))
            {
                var id = Guid.NewGuid();
                var acc = new Account(id)
                {
                    Name = "Dauda"
                };
                crm.PopulateWith(acc);
                crm.ContainsEntity(acc);
            }
        }


        [Fact]
        public void TestCreateSameId()
        {
            using (var context = new Xrm(orgAdminUIService))
            {
                var id = orgAdminUIService.Create(new Account());
                try
                {
                    orgAdminUIService.Create(new Account(id));
                    throw new XunitException();
                }
                catch (Exception e)
                {
                    Assert.IsType<FaultException>(e);
                }

            }
        }

        [Fact]
        public void TestCreateWithRelatedEntities()
        {
            using (var context = new Xrm(orgAdminUIService))
            {
                var contact = new Contact();
                var account1 = new Account();
                var account2 = new Account();
                account1.Name = "AccountRelated 1";
                account2.Name = "AccountRelated 2";

                var accounts = new EntityCollection(new List<Entity>() { account1, account2 });

                // Add related order items so it can be created in one request
                contact.RelatedEntities.Add(new Relationship
                {
                    PrimaryEntityRole = EntityRole.Referenced,
                    SchemaName = "somerandomrelation"
                }, accounts);

                var request = new CreateRequest
                {
                    Target = contact
                };
                try
                {
                    orgAdminUIService.Execute(request);
                    throw new XunitException();
                }
                catch (Exception e)
                {
                    Assert.IsType<FaultException>(e);
                }

                contact = new Contact();
                contact.RelatedEntities.Add(new Relationship
                {
                    PrimaryEntityRole = EntityRole.Referenced,
                    SchemaName = "account_primary_contact"
                }, accounts);

                request = new CreateRequest
                {
                    Target = contact
                };

                contact.Id = (orgAdminUIService.Execute(request) as CreateResponse).id;
                var accountSet = context.AccountSet.Where(x => x.Name.StartsWith("AccountRelated")).ToList();
                Assert.Equal(2, accountSet.Count);
                foreach (var acc in accountSet)
                {
                    Assert.Equal(contact.Id, acc.PrimaryContactId.Id);
                }
            }
        }

        [Fact]
        public void CreatingAttributeWithEmptyStringShouldReturnNull()
        {
            var id = orgAdminUIService.Create(new Lead { Subject = string.Empty });
            var lead = orgAdminService.Retrieve<Lead>(id);
            Assert.Null(lead.Subject);
        }

        [Fact]
        public void CreatingEntityWithSdkModeShouldInitializeBooleanAttributes()
        {
            var id = orgAdminService.Create(new Lead());
            var lead = orgAdminService.Retrieve<Lead>(id);
            Assert.NotNull(lead.DoNotBulkEMail);
            Assert.NotNull(lead.DoNotEMail);
            Assert.NotNull(lead.DoNotFax);
            Assert.NotNull(lead.DoNotPhone);
            Assert.NotNull(lead.DoNotPostalMail);
            Assert.NotNull(lead.DoNotSendMM);
        }

        [Fact]
        public void CreatingEntityWithSdkModeShouldInitializePicklistAttributes()
        {
            var id = orgAdminService.Create(new Lead());
            var lead = orgAdminService.Retrieve<Lead>(id);
            Assert.NotNull(lead.LeadQualityCode);
            Assert.NotNull(lead.PreferredContactMethodCode);
            Assert.NotNull(lead.PriorityCode);
            Assert.NotNull(lead.SalesStageCode);
        }
    }
}
