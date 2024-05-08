﻿using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace Marvin.IDP;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        { 
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResource("roles", "Your role(s)", new [] { "role"}),
            new IdentityResource("country","The country you are living in", new List<string>() { "country" })
        };

    public static IEnumerable<ApiResource> ApiResources => new ApiResource[]
    {
        new ApiResource("imagegalleryapi", "Image Gallery API", new [] {"role", "country"})
        {
            Scopes = { "imagegalleryapi.fullaccess", "imagegalleryapi.read", "imagegalleryapi.write" },
            ApiSecrets = { new Secret("apisecret".Sha256()) }
        }
    };
    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
            {
                new ApiScope("imagegalleryapi.fullaccess"),
                new ApiScope("imagegalleryapi.read"),
                new ApiScope("imagegalleryapi.write")
            };

    public static IEnumerable<Client> Clients =>
        new Client[] 
            { 
                new Client()
                {
                    ClientName = "Image Gallery",
                    ClientId = "imagegalleryclient",
                    AllowedGrantTypes = GrantTypes.Code,
                    IdentityTokenLifetime = 300, //default 5 minutes
                    AuthorizationCodeLifetime = 300, //default 5 minutes
                    AccessTokenLifetime = 120, //default 1 hour
                    AllowOfflineAccess = true,
                    AccessTokenType = AccessTokenType.Reference,
                    //AbsoluteRefreshTokenLifetime = 
                    UpdateAccessTokenClaimsOnRefresh = true,
                    RedirectUris =
                    {
                        "https://localhost:7184/signin-oidc"
                    },
                    PostLogoutRedirectUris =
                    {
                        "https://localhost:7184/signout-callback-oidc"
                    },
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "roles",
                        "imagegalleryapi.read",
                        "imagegalleryapi.write",
                        "country"
                    },
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    RequireConsent = true
                }
            };
}