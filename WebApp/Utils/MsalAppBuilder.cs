﻿/************************************************************************************************
The MIT License (MIT)

Copyright (c) 2015 Microsoft Corporation

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
***********************************************************************************************/

using Microsoft.Identity.Client;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebApp.Utils
{
    public static class MsalAppBuilder
    {
        private static IConfidentialClientApplication currentApp = null;
        public static IConfidentialClientApplication BuildConfidentialClientApplication()
        {
            if (null != currentApp)
            {
                return currentApp;
            }
            currentApp =  BuildConfidentialClientApplication(ClaimsPrincipal.Current);
            return currentApp;
        }

        public static IConfidentialClientApplication BuildConfidentialClientApplication(ClaimsPrincipal currentUser)
        {
            if (null != currentApp)
            {
                return currentApp;
            }

            IConfidentialClientApplication clientapp = ConfidentialClientApplicationBuilder.Create(AuthenticationConfig.ClientId)
                  .WithClientSecret(AuthenticationConfig.ClientSecret)
                  .WithRedirectUri(AuthenticationConfig.RedirectUri)
                  .WithAdfsAuthority(AuthenticationConfig.Authority)
                  .Build();

            // After the ConfidentialClientApplication is created, we overwrite its default UserTokenCache with our implementation
            MSALPerUserMemoryTokenCache userTokenCache = new MSALPerUserMemoryTokenCache(clientapp.UserTokenCache, currentUser ?? ClaimsPrincipal.Current);
            currentApp = clientapp;

            return clientapp;
        }

        public static async Task ClearUserTokenCache()
        {
            IConfidentialClientApplication clientapp = ConfidentialClientApplicationBuilder.Create(AuthenticationConfig.ClientId)
                  .WithClientSecret(AuthenticationConfig.ClientSecret)
                  .WithRedirectUri(AuthenticationConfig.RedirectUri)
                  .WithAdfsAuthority(AuthenticationConfig.Authority)
                  .Build();

            // We only clear the user's tokens.
            MSALPerUserMemoryTokenCache userTokenCache = new MSALPerUserMemoryTokenCache(clientapp.UserTokenCache);
            var userAccount = await clientapp.GetAccountAsync(ClaimsPrincipal.Current.GetMsalAccountId());

            await clientapp.RemoveAsync(userAccount);
            userTokenCache.Clear();
        }
    }
}