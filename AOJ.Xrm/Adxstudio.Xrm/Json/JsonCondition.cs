/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Json
{
	using System;
	using System.Linq;
	using Services.Query;

	/// <summary>
	/// A <see cref="Condition"/> serialization surrogate class.
	/// </summary>
	internal class JsonCondition : Condition
	{
		/// <summary>
		/// Converts a <see cref="Condition"/> to a <see cref="JsonCondition"/>.
		/// </summary>
		/// <param name="condition">The condition.</param>
		/// <returns>The converted condition.</returns>
		public static JsonCondition Parse(Condition condition)
		{
			var jcondition = new JsonCondition
			{
				Aggregate = condition.Aggregate,
				Attribute = condition.Attribute,
				Alias = condition.Alias,
				Column = condition.Column,
				EntityName = condition.EntityName,
				Extensions = condition.Extensions,
				Operator = condition.Operator,
				UiHidden = condition.UiHidden,
				UiName = condition.UiName,
				UiType = condition.UiType,
				Value = condition.Value,
				Values = condition.Values,
			};

			return jcondition;
		}

		/// <summary>
		/// Converts this into a <see cref="Condition"/>.
		/// </summary>
		/// <param name="deserialize">The function for deserializing nested values.</param>
		/// <returns>The converted condition.</returns>
		public Condition ToCondition(Func<object, object> deserialize)
		{
			var condition = new Condition
			{
				Aggregate = Aggregate,
				Attribute = Attribute,
				Alias = Alias,
				Column = Column,
				EntityName = EntityName,
				Extensions = Extensions,
				Operator = Operator,
				UiHidden = UiHidden,
				UiName = UiName,
				UiType = UiType,
				Value = deserialize(Value),
				Values = Values != null ? Values.Select(deserialize).ToList() : null
			};

			return condition;
		}
	}
}
