/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.AspNet.Cms
{
	using System;
	using System.Security.Claims;
	using System.Threading;
	using System.Threading.Tasks;
	using Identity;
	using Microsoft.AspNet.Identity;

	public interface IAuthenticationOptionsExtended
	{
		string AuthenticationType { get; set; }
		bool ExternalLogoutEnabled { get; set; }
		bool RegistrationEnabled { get; set; }
		string RegistrationClaimsMapping { get; set; }
		string LoginClaimsMapping { get; set; }
		string ProfileEditPolicyId { get; set; }
		Task<string> ToIssuer(CancellationToken cancellationToken);
	}

	/// <summary>
	/// Manages middleware settings that are applied at application startup.
	/// </summary>
	/// <typeparam name="TUser">The user type.</typeparam>
	public abstract class StartupSettingsManager<TUser>
		where TUser : CrmUser
	{
		private readonly Func<UserManager<TUser, string>, TUser, Task<ClaimsIdentity>> _regenerateIdentityCallback;
		private const bool DefaultRegistrationEnabled = true;
		private const bool DefaultAzureADObjectIdentifierAsNameIdentifierClaimEnabled = true;
		private const bool DefaultObjectIdentifierAsNameIdentifierClaimEnabled = false;
		private const bool DefaultExternalLogoutEnabled = false;
		private const bool DefaultAllowContactMappingWithEmail = false;
		private const string OwinAuthenticationFailedAccessDeniedMsg = "access_denied";
		private const string MessageQueryStringParameter = "message";
		private const string PasswordResetPolicyQueryStringParameter = "passwordResetPolicyId";
		private const string AzureADB2CPasswordResetPolicyErrorCode = "AADB2C90118";
		private const string AzureADB2CUserCancelledErrorCode = "AADB2C90091";

		public string DefaultAuthenticationType { get; private set; }
		public bool SingleSignOn { get; private set; }

	}
}
