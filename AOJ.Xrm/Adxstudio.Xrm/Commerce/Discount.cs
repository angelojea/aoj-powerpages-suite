/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;

namespace Adxstudio.Xrm.Commerce
{
	/// <summary>
	/// Discount
	/// </summary>
	public class Discount : IDiscount
	{
		/// <summary>
		/// Discount class initialization
		/// </summary>
		/// <param name="id">ID</param>
		/// <param name="code">Discount code</param>
		/// <param name="name">Name</param>
		/// <param name="scope">Scope: Order, Product</param>
		/// <param name="type">Type: Percentage, Amount</param>
		/// <param name="amount">Amount of the discount, either a dollar value or a percentage</param>
		public Discount(Guid id, string code, string name, int scope, int type, decimal amount)
		{
			Id = id;
			Code = code;
			Name = name;
			Scope = TrySetDiscountScope(scope);
			Type = TrySetDiscountType(type);
			Amount = amount;
		}

		/// <summary>
		/// Amount of the discount, either a dollar value or a percentage
		/// </summary>
		public decimal Amount { get; }

		/// <summary>
		/// Discount code
		/// </summary>
		public string Code { get; }

		/// <summary>
		/// ID of the record
		/// </summary>
		public Guid Id { get; }

		/// <summary>
		/// Name
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Scope, either Order or Product
		/// </summary>
		public DiscountScope? Scope { get; }

		/// <summary>
		/// Type, either Amount or Percentage
		/// </summary>
		public DiscountType? Type { get; }

		private static DiscountScope? TrySetDiscountScope(int scope)
		{
			switch (scope)
			{
				case (int)DiscountScope.Order:
					return DiscountScope.Order;
				case (int)DiscountScope.Product:
					return DiscountScope.Product;
				default:
					return null;
			}
		}

		private static DiscountType? TrySetDiscountType(int type)
		{
			switch (type)
			{
				case (int)DiscountType.Percentage:
					return DiscountType.Percentage;
				case (int)DiscountType.Amount:
					return DiscountType.Amount;
				default:
					return null;
			}
		}
	}
}
