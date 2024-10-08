/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Diagnostics.Trace
{
	using System;
	using System.Collections.Generic;
	using System.Text;

	/// <summary>
	/// Utility functions for handling correct logging of customer schema data.
	/// </summary>
	public static class EntityNamePrivacy
	{
		/// <summary>
		/// Gets an entity name representation suitable for logging.
		/// </summary>
		/// <param name="entityLogicalName">The entity logical name to be logged.</param>
		/// <returns>
		/// Returns the logical name unchanged if it matches the whitelist of known entities, otherwise returns a
		/// hashed representation.
		/// </returns>
		public static string GetEntityName(string entityLogicalName)
		{
			if (string.IsNullOrEmpty(entityLogicalName))
			{
				return entityLogicalName;
			}
			
			return PortalEntityAllowedList.Contains(entityLogicalName)
				? entityLogicalName
				: Convert.ToBase64String(HashPii.ComputeHashPiiSha256(Encoding.UTF8.GetBytes(entityLogicalName)));
		}

		/// <summary>
		/// Determines whether a given entity logical name is allowed to be logged un-hashed.
		/// </summary>
		/// <param name="entityLogicalName">The entity logical name to be logged.</param>
		/// <returns>
		/// True if the given entity logical name is not null or empty and can be logged, otherwise false.
		/// </returns>
		public static bool IsPortalEntityAllowed(string entityLogicalName)
		{
			return !string.IsNullOrEmpty(entityLogicalName) && PortalEntityAllowedList.Contains(entityLogicalName);
		}

        #region PortalEntityAllowedList
        /// <summary>
        /// The list of Microsoft owned CRM and ADX entities which are allowed for logging in feature usage telemetry and other places.
        /// </summary>
        private static readonly HashSet<string> PortalEntityAllowedList = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			"adx_accesscontrolrule_publishingstate",
			"adx_ad",
			"adx_adplacement",
			"adx_adplacement_ad",
			"adx_alertsubscription",
			"adx_badge",
			"adx_badgetype",
			"adx_bingmaplookup",
			"adx_blog",
			"adx_blog_webrole",
			"adx_blogpost",
			"adx_blogpost_tag",
			"adx_communityforum",
			"adx_communityforumaccesspermission",
			"adx_communityforumaccesspermission_webrole",
			"adx_communityforumalert",
			"adx_communityforumannouncement",
			"adx_communityforumpost",
			"adx_communityforumthread",
			"adx_communityforumthread_contact",
			"adx_communityforumthread_tag",
			"adx_contentsnippet",
			"adx_entityform",
			"adx_entityformmetadata",
			"adx_entitylist",
			"adx_entitypermission",
			"adx_entitypermission_webrole",
			"adx_externalidentity",
			"adx_forum_activityalert",
			"adx_forumnotification",
			"adx_forumthreadtype",
			"adx_idea",
			"adx_ideaforum",
			"adx_invitation",
			"adx_invitation_invitecontacts",
			"adx_invitation_redeemedcontacts",
			"adx_invitation_webrole",
			"adx_inviteredemption",
			"adx_kbarticle_kbarticle",
			"adx_knowledgearticle",
			"adx_pagealert",
			"adx_pagenotification",
			"adx_pagetag",
			"adx_pagetag_webpage",
			"adx_pagetemplate",
			"adx_poll",
			"adx_polloption",
			"adx_pollplacement",
			"adx_pollplacement_poll",
			"adx_pollsubmission",
			"adx_portalcomment",
			"adx_publishingstate",
			"adx_publishingstatetransitionrule",
			"adx_publishingstatetransitionrule_webrole",
			"adx_redirect",
			"adx_setting",
			"adx_shortcut",
			"adx_sitemarker",
			"adx_sitesetting",
			"adx_tag",
			"adx_urlhistory",
			"adx_webfile",
			"adx_webfilelog",
			"adx_webform",
			"adx_webformmetadata",
			"adx_webformsession",
			"adx_webformstep",
			"adx_weblink",
			"adx_weblinkset",
			"adx_webnotificationentity",
			"adx_webnotificationurl",
			"adx_webpage",
			"adx_webpage_tag",
			"adx_webpageaccesscontrolrule",
			"adx_webpageaccesscontrolrule_webrole",
			"adx_webpagehistory",
			"adx_webpagelog",
			"adx_webrole",
			"adx_webrole_account",
			"adx_webrole_contact",
			"adx_webrole_ideaforum_read",
			"adx_webrole_ideaforum_write",
			"adx_webrole_systemuser",
			"adx_website",
			"adx_website_list",
			"adx_website_sponsor",
			"adx_websiteaccess",
			"adx_websiteaccess_webrole",
			"adx_websitebinding",
			"adx_webtemplate",
			"session",
			"account",
			"accountleads",
			"aciviewmapper",
			"actioncard",
			"actioncardusersettings",
			"actioncarduserstate",
			"activitymimeattachment",
			"activityparty",
			"activitypointer",
			"advancedsimilarityrule",
			"annotation",
			"annualfiscalcalendar",
			"appconfig",
			"appconfiginstance",
			"appconfigmaster",
			"applicationfile",
			"appmodule",
			"appmodulecomponent",
			"appmoduleroles",
			"appointment",
			"asyncoperation",
			"attachment",
			"attributemap",
			"audit",
			"authorizationserver",
			"azureserviceconnection",
			"bookableresource",
			"bookableresourcebooking",
			"bookableresourcebookingexchangesyncidmapping",
			"bookableresourcebookingheader",
			"bookableresourcecategory",
			"bookableresourcecategoryassn",
			"bookableresourcecharacteristic",
			"bookableresourcegroup",
			"bookingstatus",
			"bulkdeletefailure",
			"bulkdeleteoperation",
			"bulkoperation",
			"bulkoperationlog",
			"businessdatalocalizedlabel",
			"businessprocessflowinstance",
			"businessunit",
			"businessunitmap",
			"businessunitnewsarticle",
			"calendar",
			"calendarrule",
			"campaign",
			"campaignactivity",
			"campaignactivityitem",
			"campaignitem",
			"campaignresponse",
			"cardtype",
			"category",
			"channelaccessprofile",
			"channelaccessprofileentityaccesslevel",
			"channelaccessprofilerule",
			"channelaccessprofileruleitem",
			"channelproperty",
			"channelpropertygroup",
			"characteristic",
			"childincidentcount",
			"clientupdate",
			"columnmapping",
			"commitment",
			"competitor",
			"competitoraddress",
			"competitorproduct",
			"competitorsalesliterature",
			"complexcontrol",
			"connection",
			"connectionrole",
			"connectionroleassociation",
			"connectionroleobjecttypecode",
			"constraintbasedgroup",
			"contact",
			"contactinvoices",
			"contactleads",
			"contactorders",
			"contactquotes",
			"contract",
			"contractdetail",
			"contracttemplate",
			"convertrule",
			"convertruleitem",
			"customcontrol",
			"customcontroldefaultconfig",
			"customcontrolresource",
			"customeraddress",
			"customeropportunityrole",
			"customerrelationship",
			"dataperformance",
			"dci",
			"delveactionhub",
			"dependency",
			"dependencyfeature",
			"dependencynode",
			"discount",
			"discounttype",
			"displaystring",
			"displaystringmap",
			"documentindex",
			"documenttemplate",
			"duplicaterecord",
			"duplicaterule",
			"duplicaterulecondition",
			"dynamicproperty",
			"dynamicpropertyassociation",
			"dynamicpropertyinstance",
			"dynamicpropertyoptionsetitem",
			"email",
			"emailhash",
			"emailsearch",
			"emailserverprofile",
			"emailsignature",
			"entitlement",
			"entitlementchannel",
			"entitlementcontacts",
			"entitlementproducts",
			"entitlementtemplate",
			"entitlementtemplatechannel",
			"entitlementtemplateproducts",
			"entitydataprovider",
			"entitydatasource",
			"entitydatasourcemapping",
			"entitymap",
			"equipment",
			"exchangesyncidmapping",
			"expiredprocess",
			"externalparty",
			"externalpartyitem",
			"fax",
			"feedback",
			"fieldpermission",
			"fieldsecurityprofile",
			"filtertemplate",
			"fixedmonthlyfiscalcalendar",
			"globalsearchconfiguration",
			"goal",
			"goalrollupquery",
			"hierarchyrule",
			"hierarchysecurityconfiguration",
			"imagedescriptor",
			"import",
			"importdata",
			"importentitymapping",
			"importfile",
			"importjob",
			"importlog",
			"importmap",
			"incident",
			"incidentknowledgebaserecord",
			"incidentresolution",
			"integrationstatus",
			"interactionforemail",
			"internaladdress",
			"interprocesslock",
			"invaliddependency",
			"invoice",
			"invoicedetail",
			"isvconfig",
			"kbarticle",
			"kbarticlecomment",
			"kbarticletemplate",
			"knowledgearticle",
			"knowledgearticleincident",
			"knowledgearticlescategories",
			"knowledgearticleviews",
			"knowledgebaserecord",
			"knowledgesearchmodel",
			"languagelocale",
			"lead",
			"leadaddress",
			"leadcompetitors",
			"leadproduct",
			"leadtoopportunitysalesprocess",
			"letter",
			"license",
			"list",
			"listmember",
			"localconfigstore",
			"lookupmapping",
			"mailbox",
			"mailboxstatistics",
			"mailboxtrackingfolder",
			"mailmergetemplate",
			"metadatadifference",
			"metric",
			"mobileofflineprofile",
			"mobileofflineprofileitem",
			"mobileofflineprofileitemassociation",
			"monthlyfiscalcalendar",
			"msdyn_postalbum",
			"msdyn_postconfig",
			"msdyn_postruleconfig",
			"msdyn_wallsavedquery",
			"msdyn_wallsavedqueryusersettings",
			"multientitysearch",
			"multientitysearchentities",
			"multiselectattributeoptionvalues",
			"navigationsetting",
			"newprocess",
			"notification",
			"officedocument",
			"officegraphdocument",
			"opportunity",
			"opportunityclose",
			"opportunitycompetitors",
			"opportunityproduct",
			"opportunitysalesprocess",
			"orderclose",
			"organization",
			"organizationstatistic",
			"organizationui",
			"orginsightsmetric",
			"owner",
			"ownermapping",
			"partnerapplication",
			"personaldocumenttemplate",
			"phonecall",
			"phonetocaseprocess",
			"picklistmapping",
			"pluginassembly",
			"plugintracelog",
			"plugintype",
			"plugintypestatistic",
			"position",
			"post",
			"postcomment",
			"postfollow",
			"postlike",
			"postregarding",
			"postrole",
			"pricelevel",
			"principalattributeaccessmap",
			"principalentitymap",
			"principalobjectaccess",
			"principalobjectaccessreadsnapshot",
			"principalobjectattributeaccess",
			"principalsyncattributemap",
			"privilege",
			"privilegeobjecttypecodes",
			"processsession",
			"processstage",
			"processtrigger",
			"product",
			"productassociation",
			"productpricelevel",
			"productsalesliterature",
			"productsubstitute",
			"publisher",
			"publisheraddress",
			"quarterlyfiscalcalendar",
			"queue",
			"queueitem",
			"queueitemcount",
			"queuemembercount",
			"queuemembership",
			"quote",
			"quoteclose",
			"quotedetail",
			"ratingmodel",
			"ratingvalue",
			"recommendationcache",
			"recommendationmodel",
			"recommendationmodelmapping",
			"recommendationmodelversion",
			"recommendationmodelversionhistory",
			"recommendeddocument",
			"recordcountsnapshot",
			"recurrencerule",
			"recurringappointmentmaster",
			"relationshiprole",
			"relationshiprolemap",
			"replicationbacklog",
			"report",
			"reportcategory",
			"reportentity",
			"reportlink",
			"reportvisibility",
			"resource",
			"resourcegroup",
			"resourcegroupexpansion",
			"resourcespec",
			"ribboncommand",
			"ribboncontextgroup",
			"ribboncustomization",
			"ribbondiff",
			"ribbonrule",
			"ribbontabtocommandmap",
			"role",
			"roleprivileges",
			"roletemplate",
			"roletemplateprivileges",
			"rollupfield",
			"rollupjob",
			"rollupproperties",
			"routingrule",
			"routingruleitem",
			"runtimedependency",
			"salesliterature",
			"salesliteratureitem",
			"salesorder",
			"salesorderdetail",
			"salesprocessinstance",
			"savedorginsightsconfiguration",
			"savedquery",
			"savedqueryvisualization",
			"sdkmessage",
			"sdkmessagefilter",
			"sdkmessagepair",
			"sdkmessageprocessingstep",
			"sdkmessageprocessingstepimage",
			"sdkmessageprocessingstepsecureconfig",
			"sdkmessagerequest",
			"sdkmessagerequestfield",
			"sdkmessageresponse",
			"sdkmessageresponsefield",
			"semiannualfiscalcalendar",
			"service",
			"serviceappointment",
			"servicecontractcontacts",
			"serviceendpoint",
			"sharedobjectsforread",
			"sharepointdata",
			"sharepointdocument",
			"sharepointdocumentlocation",
			"sharepointsite",
			"similarityrule",
			"site",
			"sitemap",
			"sla",
			"slaitem",
			"slakpiinstance",
			"socialactivity",
			"socialinsightsconfiguration",
			"socialprofile",
			"solution",
			"solutioncomponent",
			"sqlencryptionaudit",
			"statusmap",
			"stringmap",
			"subject",
			"subscription",
			"subscriptionclients",
			"subscriptionmanuallytrackedobject",
			"subscriptionstatisticsoffline",
			"subscriptionstatisticsoutlook",
			"subscriptionsyncentryoffline",
			"subscriptionsyncentryoutlook",
			"subscriptionsyncinfo",
			"subscriptiontrackingdeletedobject",
			"syncattributemapping",
			"syncattributemappingprofile",
			"syncerror",
			"systemapplicationmetadata",
			"systemform",
			"systemuser",
			"systemuserbusinessunitentitymap",
			"systemuserlicenses",
			"systemusermanagermap",
			"systemuserprincipals",
			"systemuserprofiles",
			"systemuserroles",
			"systemusersyncmappingprofiles",
			"task",
			"team",
			"teammembership",
			"teamprofiles",
			"teamroles",
			"teamsyncattributemappingprofiles",
			"teamtemplate",
			"template",
			"territory",
			"textanalyticsentitymapping",
			"theme",
			"timestampdatemapping",
			"timezonedefinition",
			"timezonelocalizedname",
			"timezonerule",
			"topic",
			"topichistory",
			"topicmodel",
			"topicmodelconfiguration",
			"topicmodelexecutionhistory",
			"traceassociation",
			"tracelog",
			"traceregarding",
			"transactioncurrency",
			"transformationmapping",
			"transformationparametermapping",
			"translationprocess",
			"unresolvedaddress",
			"untrackedemail",
			"uom",
			"uomschedule",
			"userapplicationmetadata",
			"userentityinstancedata",
			"userentityuisettings",
			"userfiscalcalendar",
			"userform",
			"usermapping",
			"userquery",
			"userqueryvisualization",
			"usersearchfacet",
			"usersettings",
			"webresource",
			"webwizard",
			"wizardaccessprivilege",
			"wizardpage",
			"workflow",
			"workflowdependency",
			"workflowlog",
			"workflowwaitsubscription",
		};
		#endregion
	}
}
