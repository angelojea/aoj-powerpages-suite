/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Commerce
{
	internal class PurchaseableItem : IPurchasableItem
	{
		public PurchaseableItem(Entity quoteProduct, string name, EntityReference product, IEnumerable<IDiscount> discounts, bool requiresShipping = false) 
		{
			if (quoteProduct == null) throw new ArgumentNullException("quoteProduct");
			if (name == null) throw new ArgumentNullException("name");
			if (product == null) throw new ArgumentNullException("product");

			QuoteProduct = quoteProduct.ToEntityReference();
			Name = name;
			Product = product;
			RequiresShipping = requiresShipping;

			Description = quoteProduct.GetAttributeValue<string>("description");
			Discounts = discounts;
			ExtendedAmount = GetDecimalFromMoney(quoteProduct, "extendedamount");
			IsRequired = quoteProduct.GetAttributeValue<bool?>("adx_isrequired").GetValueOrDefault();
			LineItemNumber = quoteProduct.GetAttributeValue<int?>("lineitemnumber").GetValueOrDefault(0);
			ManualDiscountAmount = GetDecimalFromMoney(quoteProduct, "manualdiscountamount");
			PricePerUnit = GetDecimalFromMoney(quoteProduct, "priceperunit");
			Quantity = quoteProduct.GetAttributeValue<decimal?>("quantity").GetValueOrDefault(0);
			Tax = GetDecimalFromMoney(quoteProduct, "tax");
			UnitOfMeasure = quoteProduct.GetAttributeValue<EntityReference>("uomid");
			VolumeDiscountAmount = GetDecimalFromMoney(quoteProduct, "volumediscountamount");

			IsSelected = Quantity > 0;
			Amount = PricePerUnit * Quantity;
			TotalDiscountAmount = ManualDiscountAmount + (Quantity * VolumeDiscountAmount);
			AmountAfterDiscount = Amount - TotalDiscountAmount;
		}

		public decimal Amount { get; }

		public decimal AmountAfterDiscount { get; }

		public string Description { get; }

		public IEnumerable<IDiscount> Discounts { get; }

		public decimal ExtendedAmount { get; }

		public bool IsOptional
		{
			get { return !IsRequired; }
		}

		public bool IsRequired { get; }

		public bool IsSelected { get; set; }

		public int LineItemNumber { get; }

		public decimal ManualDiscountAmount { get; }

		public string Name { get; }

		public decimal PricePerUnit { get; }

		public EntityReference Product { get; }

		public decimal Quantity { get; }

		public EntityReference QuoteProduct { get; }

		public bool RequiresShipping { get; }

		public bool SupportsQuantities { get; set; }

		public decimal Tax { get; }

		public decimal TotalDiscountAmount { get; }

		public EntityReference UnitOfMeasure { get; }

		public decimal VolumeDiscountAmount { get; }

		private static decimal GetDecimalFromMoney(Entity quoteProduct, string attributeLogicalName, decimal defaultValue = 0)
		{
			var value = quoteProduct.GetAttributeValue<Money>(attributeLogicalName);

			return value == null ? defaultValue : value.Value;
		}
	}
}
