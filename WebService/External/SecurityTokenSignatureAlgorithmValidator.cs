// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace Microsoft.Azure.IoTSolutions.UIConfig.WebService.External
{
    public class SecurityTokenSignatureAlgorithmValidator : ISecurityTokenValidator
    {
        private readonly HashSet<string> supportedSignatureAlgorithms;
        private readonly JwtSecurityTokenHandler handler;

        public SecurityTokenSignatureAlgorithmValidator(IEnumerable<string> supportedSignatureAlgorithms)
        {
            this.supportedSignatureAlgorithms = new HashSet<string>(supportedSignatureAlgorithms);
            this.handler = new JwtSecurityTokenHandler();
        }

        public bool CanValidateToken => true;

        public int MaximumTokenSizeInBytes { get; set; }

        public bool CanReadToken(string securityToken)
        {
            try
            {
                var token = new JwtSecurityToken(securityToken);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public ClaimsPrincipal ValidateToken(string securityToken, TokenValidationParameters validationParameters, out SecurityToken validatedToken)
        {
            var token = new JwtSecurityToken(securityToken);
            Debug.WriteLine(token.Header.Alg);

            if (!this.supportedSignatureAlgorithms.Contains(token.Header.Alg))
            {
                throw new SecurityTokenInvalidSignatureException("No supported signature algorithm");
            }

            return this.handler.ValidateToken(securityToken, validationParameters, out validatedToken);
        }
    }
}