/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Commerce
{
	internal class Purchasable : IPurchasable
	{
		public Purchasable(Entity quote, IEnumerable<IPurchasableItem> items, IEnumerable<IDiscount> discounts, bool requiresShipping = false)
		{
			if (quote == null) throw new ArgumentNullException("quote");
			if (items == null) throw new ArgumentNullException("items");

			DiscountAmount = GetDecimalFromMoney(quote, "discountamount");
			Discounts = discounts;
			ShippingAmount = GetDecimalFromMoney(quote, "freightamount");
			TotalDiscountAmount = GetDecimalFromMoney(quote, "totaldiscountamount");
			TotalLineItemDiscountAmount = GetDecimalFromMoney(quote, "totallineitemdiscountamount");
			// CRM money fields store the raw value - the client side rounds the values to the precision so we need to round the values before using them in a calculation.
			var serviceContext = PortalCrmConfigurationManager.CreateServiceContext();
			var precision = MetadataHelpers.GetPrecisionFromMoney(serviceContext, "quote", "totalamount");
			TotalAmount = MetadataHelpers.GetDecimalFromMoneyAndRoundToPrecision(serviceContext, quote, "totalamount", precision);
			TotalLineItemAmount = MetadataHelpers.GetDecimalFromMoneyAndRoundToPrecision(serviceContext, quote, "totallineitemamount", precision);
			TotalPreShippingAmount = MetadataHelpers.GetDecimalFromMoneyAndRoundToPrecision(serviceContext, quote, "totalamountlessfreight", precision);
			TotalDiscount = TotalLineItemAmount - TotalPreShippingAmount;
			TotalTax = GetDecimalFromMoney(quote, "totaltax");
			PriceList = quote.GetAttributeValue<EntityReference>("pricelevelid");
			Quote = quote.ToEntityReference();

			Items = items.ToArray();

			HasOptions = Items.Any(e => e.IsOptional);
			RequiresShipping = requiresShipping || Items.Any(e => e.RequiresShipping);
			SupportsQuantities = Items.Any(e => e.SupportsQuantities);

			BillToAddress = new PurchaseAddress
			{
				City = quote.GetAttributeValue<string>("billto_city"),
				Country = quote.GetAttributeValue<string>("billto_country"),
				Line1 = quote.GetAttributeValue<string>("billto_line1"),
				Line2 = quote.GetAttributeValue<string>("billto_line2"),
				Line3 = quote.GetAttributeValue<string>("billto_line3"),
				Name = quote.GetAttributeValue<string>("billto_name"),
				PostalCode = quote.GetAttributeValue<string>("billto_postalcode"),
				StateOrProvince = quote.GetAttributeValue<string>("billto_stateorprovince"),
			};

			ShipToAddress = new PurchaseAddress
			{
				City = quote.GetAttributeValue<string>("shipto_city"),
				Country = quote.GetAttributeValue<string>("shipto_country"),
				Line1 = quote.GetAttributeValue<string>("shipto_line1"),
				Line2 = quote.GetAttributeValue<string>("shipto_line2"),
				Line3 = quote.GetAttributeValue<string>("shipto_line3"),
				Name = quote.GetAttributeValue<string>("shipto_name"),
				PostalCode = quote.GetAttributeValue<string>("shipto_postalcode"),
				StateOrProvince = quote.GetAttributeValue<string>("shipto_stateorprovince"),
			};
		}

		public IPurchaseAddress BillToAddress { get; }

		public decimal DiscountAmount { get; }

		public IEnumerable<IDiscount> Discounts { get; }

		public bool HasOptions { get; }

		public IEnumerable<IPurchasableItem> Items { get; }

		public EntityReference PriceList { get; }

		public EntityReference Quote { get; }

		public bool RequiresShipping { get; }

		public IPurchaseAddress ShipToAddress { get; }

		public decimal ShippingAmount { get; }

		public bool SupportsQuantities { get; }

		public decimal TotalAmount { get; }

		public decimal TotalDiscount { get; }

		public decimal TotalDiscountAmount { get; }

		public decimal TotalLineItemAmount { get; }

		public decimal TotalLineItemDiscountAmount { get; }

		public decimal TotalPreShippingAmount { get; }

		public decimal TotalTax { get; }

		private static decimal GetDecimalFromMoney(Entity quote, string attributeLogicalName, decimal defaultValue = 0)
		{
			var value = quote.GetAttributeValue<Money>(attributeLogicalName);

			return value == null ? defaultValue : value.Value;
		}
	}
}
