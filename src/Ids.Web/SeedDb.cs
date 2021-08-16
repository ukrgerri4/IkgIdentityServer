using IdentityModel;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace EscapeDungeonIdentityWeb
{
    public class SeedDb
    {
        public static void EnsureSeededUsersData(IServiceScope scope)
        {
            var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var alice = userMgr.FindByNameAsync("igor").Result;
            if (alice == null)
            {
                alice = new IdentityUser
                {
                    UserName = "igor",
                    Email = "ukrgerri4@gmail.com",
                    EmailConfirmed = true,
                };
                var result = userMgr.CreateAsync(alice, "123").Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }

                result = userMgr.AddClaimsAsync(alice, new Claim[]
                {
                      new Claim(JwtClaimTypes.Name, "Igor Kryvenko"),
                      new Claim(JwtClaimTypes.GivenName, "Igor"),
                      new Claim(JwtClaimTypes.FamilyName, "Kryvenko"),
                      new Claim("location", "Kyiv")
                }).Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }

                Log.Debug("'igor' created");
            }
            else
            {
                Log.Debug("'igor' already exists");
            }

            var bob = userMgr.FindByNameAsync("marina").Result;
            if (bob == null)
            {
                bob = new IdentityUser
                {
                    UserName = "marina",
                    Email = "marina@gmail.com",
                    EmailConfirmed = true
                };
                var result = userMgr.CreateAsync(bob, "123").Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }

                result = userMgr.AddClaimsAsync(bob, new Claim[]
                {
                  new Claim(JwtClaimTypes.Name, "Marina Kryvenko"),
                  new Claim(JwtClaimTypes.GivenName, "Marina"),
                  new Claim(JwtClaimTypes.FamilyName, "Kryvenko"),
                  new Claim("location", "Kyiv")
                }).Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }

                Log.Debug("'marina' created");
            }
            else
            {
                Log.Debug("'marina' already exists");
            }
        }


        public static void EnsureSeededConfigurationData(ConfigurationDbContext context)
        {
            if (!context.Clients.Any())
            {
                Log.Debug("Clients being populated");
                foreach (var client in SeedData.Clients.ToList())
                {
                    context.Clients.Add(client.ToEntity());
                }

                context.SaveChanges();
            }
            else
            {
                Log.Debug("Clients already populated");
            }

            if (!context.IdentityResources.Any())
            {
                Log.Debug("IdentityResources being populated");
                foreach (var resource in SeedData.IdentityResources.ToList())
                {
                    context.IdentityResources.Add(resource.ToEntity());
                }

                context.SaveChanges();
            }
            else
            {
                Log.Debug("IdentityResources already populated");
            }

            if (!context.ApiScopes.Any())
            {
                Log.Debug("ApiScopes being populated");
                foreach (var resource in SeedData.ApiScopes.ToList())
                {
                    context.ApiScopes.Add(resource.ToEntity());
                }

                context.SaveChanges();
            }
            else
            {
                Log.Debug("ApiScopes already populated");
            }

            if (!context.ApiResources.Any())
            {
                Log.Debug("ApiResources being populated");
                foreach (var resource in SeedData.ApiResources.ToList())
                {
                    context.ApiResources.Add(resource.ToEntity());
                }

                context.SaveChanges();
            }
            else
            {
                Log.Debug("ApiScopes already populated");
            }
        }
    }

    public class SeedData
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            new[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResource
                {
                    Name = "role",
                    UserClaims = new List<string> {"role"}
                }
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new[]
            {
                new ApiScope("ed_gateway.read"),
                new ApiScope("ed_gateway.write"),
            };

        public static IEnumerable<ApiResource> ApiResources => new[]
        {
            new ApiResource("ed_gateway")
            {
                Scopes = new List<string> { "ed_gateway.read", "ed_gateway.write"},
                ApiSecrets = new List<Secret> {new Secret("ScopeSecret".Sha256())},
                UserClaims = new List<string> {"role"}
            }
        };

        public static IEnumerable<Client> Clients =>
            new[]
            {
                // m2m client credentials flow client
                new Client
                {
                    ClientId = "ed_gateway.client",
                    ClientName = "Client Credentials Client",

                    AllowedGrantTypes = new List<string>
                    { 
                        GrantType.ClientCredentials,
                        GrantType.ResourceOwnerPassword
                    },
                    ClientSecrets = {new Secret("3+<2P~$RM(,8HwEg".Sha256())},

                    AllowedScopes = { "ed_gateway.read", "ed_gateway.write", "openid" }
                },

                /*
                 * interactive client using code flow + pkce
                 * adjust for BRaynov
                 */
                new Client
                {
                    ClientId = "interactive",
                    ClientSecrets = {new Secret("D6EN5Q&$D[QAqgUC".Sha256())},

                    AllowedGrantTypes = GrantTypes.Code,

                    RedirectUris = {"https://localhost:6001/signin-oidc"},
                    FrontChannelLogoutUri = "https://localhost:6001/signout-oidc",
                    PostLogoutRedirectUris = {"https://localhost:6001/signout-callback-oidc"},

                    AllowOfflineAccess = true,
                    AllowedScopes = {"openid", "profile", "ed_gateway.read"},
                    RequirePkce = true,
                    RequireConsent = true,
                    AllowPlainTextPkce = false
                }
            };
    }
}