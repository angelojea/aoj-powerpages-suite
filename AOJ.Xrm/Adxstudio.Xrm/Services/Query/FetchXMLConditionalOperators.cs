/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Services.Query
{
	using System;
	using System.Linq;
	using System.Collections.Generic;
	using Microsoft.Xrm.Sdk.Query;

	/// <summary>
	/// FetchXMLConditionalOperators class
	/// </summary>
	internal static class FetchXMLConditionalOperators
	{
		/// <summary>
		/// ConditionOperatorToTextFromLookUp variable
		/// </summary>
		private static readonly IDictionary<ConditionOperator, string[]> ConditionOperatorToTextFromLookUp = new Dictionary<ConditionOperator, string[]>
		{
			{ ConditionOperator.Equal, new[] { "eq" } },
			{ ConditionOperator.NotEqual, new[] { "ne", "neq" } },
			{ ConditionOperator.GreaterThan, new[] { "gt" } },
			{ ConditionOperator.LessThan, new[] { "lt" } },
			{ ConditionOperator.GreaterEqual, new[] { "ge" } },
			{ ConditionOperator.LessEqual, new[] { "le" } },
			{ ConditionOperator.Like, new[] { "like" } },
			{ ConditionOperator.NotLike, new[] { "not-like" } },
			{ ConditionOperator.In, new[] { "in" } },
			{ ConditionOperator.NotIn, new[] { "not-in" } },
			{ ConditionOperator.Between, new[] { "between" } },
			{ ConditionOperator.NotBetween, new[] { "not-between" } },
			{ ConditionOperator.Null, new[] { "null" } },
			{ ConditionOperator.NotNull, new[] { "not-null" } },
			{ ConditionOperator.Yesterday, new[] { "yesterday" } },
			{ ConditionOperator.Today, new[] { "today" } },
			{ ConditionOperator.Tomorrow, new[] { "tomorrow" } },
			{ ConditionOperator.Last7Days, new[] { "last-seven-days" } },
			{ ConditionOperator.Next7Days, new[] { "next-seven-days" } },
			{ ConditionOperator.LastWeek, new[] { "last-week" } },
			{ ConditionOperator.ThisWeek, new[] { "this-week" } },
			{ ConditionOperator.NextWeek, new[] { "next-week" } },
			{ ConditionOperator.LastMonth, new[] { "last-month" } },
			{ ConditionOperator.ThisMonth, new[] { "this-month" } },
			{ ConditionOperator.NextMonth, new[] { "next-month" } },
			{ ConditionOperator.On, new[] { "on" } },
			{ ConditionOperator.OnOrBefore, new[] { "on-or-before" } },
			{ ConditionOperator.OnOrAfter, new[] { "on-or-after" } },
			{ ConditionOperator.LastYear, new[] { "last-year" } },
			{ ConditionOperator.ThisYear, new[] { "this-year" } },
			{ ConditionOperator.NextYear, new[] { "next-year" } },
			{ ConditionOperator.LastXHours, new[] { "last-x-hours" } },
			{ ConditionOperator.NextXHours, new[] { "next-x-hours" } },
			{ ConditionOperator.LastXDays, new[] { "last-x-days" } },
			{ ConditionOperator.NextXDays, new[] { "next-x-days" } },
			{ ConditionOperator.LastXWeeks, new[] { "last-x-weeks" } },
			{ ConditionOperator.NextXWeeks, new[] { "next-x-weeks" } },
			{ ConditionOperator.LastXMonths, new[] { "last-x-months" } },
			{ ConditionOperator.NextXMonths, new[] { "next-x-months" } },
			{ ConditionOperator.LastXYears, new[] { "last-x-years" } },
			{ ConditionOperator.NextXYears, new[] { "next-x-years" } },
			{ ConditionOperator.EqualUserId, new[] { "eq-userid" } },
			{ ConditionOperator.NotEqualUserId, new[] { "ne-userid" } },
			{ ConditionOperator.EqualBusinessId, new[] { "eq-businessid" } },
			{ ConditionOperator.NotEqualBusinessId, new[] { "ne-businessid" } },
			{ ConditionOperator.ChildOf, new[] { "child-of" } },
			{ ConditionOperator.Mask, new[] { "mask" } }, // unconfirmed
		    { ConditionOperator.NotMask, new[] { "not-mask" } }, // unconfirmed
		    { ConditionOperator.MasksSelect, new[] { "masks-select" } }, // unconfirmed
		    { ConditionOperator.Contains, new[] { "like" } },
			{ ConditionOperator.DoesNotContain, new[] { "not-like" } },
			{ ConditionOperator.EqualUserLanguage, new[] { "eq-userlanguage" } },
			{ ConditionOperator.NotOn, new[] { "not-on" } }, // unconfirmed
		    { ConditionOperator.OlderThanXMonths, new[] { "olderthan-x-months" } },
			{ ConditionOperator.BeginsWith, new[] { "begins-with" } },
			{ ConditionOperator.DoesNotBeginWith, new[] { "not-begin-with" } },
			{ ConditionOperator.EndsWith, new[] { "ends-with" } },
			{ ConditionOperator.DoesNotEndWith, new[] { "not-end-with" } },
			{ ConditionOperator.ThisFiscalYear, new[] { "this-fiscal-year" } },
			{ ConditionOperator.ThisFiscalPeriod, new[] { "this-fiscal-period" } },
			{ ConditionOperator.NextFiscalYear, new[] { "next-fiscal-year" } },
			{ ConditionOperator.NextFiscalPeriod, new[] { "next-fiscal-period" } },
			{ ConditionOperator.LastFiscalYear, new[] { "last-fiscal-year" } },
			{ ConditionOperator.LastFiscalPeriod, new[] { "last-fiscal-period" } },
			{ ConditionOperator.LastXFiscalYears, new[] { "last-x-fiscal-years" } },
			{ ConditionOperator.LastXFiscalPeriods, new[] { "last-x-fiscal-periods" } },
			{ ConditionOperator.NextXFiscalYears, new[] { "next-x-fiscal-years" } },
			{ ConditionOperator.NextXFiscalPeriods, new[] { "next-x-fiscal-periods" } },
			{ ConditionOperator.InFiscalYear, new[] { "in-fiscal-year" } },
			{ ConditionOperator.InFiscalPeriod, new[] { "in-fiscal-period" } },
			{ ConditionOperator.InFiscalPeriodAndYear, new[] { "in-fiscal-period-and-year" } },
			{ ConditionOperator.InOrBeforeFiscalPeriodAndYear, new[] { "in-or-before-fiscal-period-and-year" } },
			{ ConditionOperator.InOrAfterFiscalPeriodAndYear, new[] { "in-or-after-fiscal-period-and-year" } },
			{ ConditionOperator.EqualUserTeams, new[] { "eq-userteams" } },
			{ ConditionOperator.AboveOrEqual, new[] { "eq-or-above" } },
			{ ConditionOperator.UnderOrEqual, new[] { "eq-or-under" } },
};

		/// <summary>
		/// Returns the Key from KeyValuePair on passing value as input
		/// </summary>
		/// <param name="value">Takes value as input and returns Key</param>
		/// <returns>returns Key</returns>
		public static ConditionOperator GetKeyByValue(string value)
		{
			var keyValue = ConditionOperatorToTextFromLookUp.First(pair => pair.Value.Contains(value));
			return keyValue.Key;
		}

		/// <summary>
		/// Returns the Value from KeyValuePair on passing key as input
		/// </summary>
		/// <param name="key">Takes key as input and returns Value</param>
		/// <returns>returns Value</returns>
		public static string GetValueByKey(ConditionOperator key)
		{
			var keyValue = ConditionOperatorToTextFromLookUp.First(pair => pair.Key == key);
			return keyValue.Value[0];
		}
	}
}
