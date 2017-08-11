// Copyright (c) Microsoft. All rights reserved.

using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace Microsoft.Azure.IoTSolutions.UIConfig.WebService.External
{
    public class SecurityTokenNullValidator : ISecurityTokenValidator
    {
        public bool CanValidateToken => true;

        public int MaximumTokenSizeInBytes { get; set; }

        public bool CanReadToken(string securityToken)
        {
            return true;
        }

        public ClaimsPrincipal ValidateToken(string securityToken, TokenValidationParameters validationParameters, out SecurityToken validatedToken)
        {
            throw new SecurityTokenValidationException("No supported protocols");
        }
    }
}