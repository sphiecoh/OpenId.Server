using OpenIddict.Abstractions;

namespace OpenId.Demo;

public class ClientSeeder : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public ClientSeeder(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }


        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();

            await PopulateScopes(scope, cancellationToken);

            await PopulateInternalApps(scope, cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private async ValueTask PopulateScopes(IServiceScope scope, CancellationToken cancellationToken)
        {
            var scopeManager = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();

            var scopeDescriptor = new OpenIddictScopeDescriptor
            {
                Name = "test_scope",
                Resources = { "test_resource" }
            };

            var scopeInstance = await scopeManager.FindByNameAsync(scopeDescriptor.Name, cancellationToken);

            if (scopeInstance == null)
            {
                await scopeManager.CreateAsync(scopeDescriptor, cancellationToken);
            }
            else
            {
                await scopeManager.UpdateAsync(scopeInstance, scopeDescriptor, cancellationToken);
            }
        }

        private async ValueTask PopulateInternalApps(IServiceScope scopeService, CancellationToken cancellationToken)
        {
            var appManager = scopeService.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

            var appDescriptor = new OpenIddictApplicationDescriptor
            {
                ClientId = "7",
                ClientSecret = "test_secret",
                ClientType = OpenIddictConstants.ClientTypes.Confidential,
                Permissions =
                    {
                        OpenIddictConstants.Permissions.Endpoints.Token,
                        OpenIddictConstants.Permissions.Endpoints.Introspection,
                        OpenIddictConstants.Permissions.Endpoints.Revocation,
                        OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
                        OpenIddictConstants.Permissions.GrantTypes.RefreshToken,

                        OpenIddictConstants.Permissions.Prefixes.Scope + "test_scope"
                    }
            };

            var client = await appManager.FindByClientIdAsync(appDescriptor.ClientId, cancellationToken);

            if (client == null)
            {
                await appManager.CreateAsync(appDescriptor, cancellationToken);
            }
            else
            {
                await appManager.UpdateAsync(client, appDescriptor, cancellationToken);
            }
        }
    }