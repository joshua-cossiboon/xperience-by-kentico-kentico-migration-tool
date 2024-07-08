namespace Migration.Toolkit.Core.K11.Handlers;

using CMS.DataEngine;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Migration.Toolkit.Common;
using Migration.Toolkit.Common.Abstractions;
using Migration.Toolkit.Common.MigrationProtocol;
using Migration.Toolkit.K11;
using Migration.Toolkit.K11.Models;

public class MigrateSettingKeysCommandHandler(ILogger<MigrateSettingKeysCommandHandler> logger,
        IEntityMapper<CmsSettingsKey, SettingsKeyInfo> mapper,
        IDbContextFactory<K11Context> k11ContextFactory,
        ToolkitConfiguration toolkitConfiguration,
        IProtocol protocol)
    : IRequestHandler<MigrateSettingKeysCommand, CommandResult>
{
    public async Task<CommandResult> Handle(MigrateSettingKeysCommand request, CancellationToken cancellationToken)
    {
        var entityConfiguration = toolkitConfiguration.EntityConfigurations.GetEntityConfiguration<CmsSettingsKey>();

        await using var k11Context = await k11ContextFactory.CreateDbContextAsync(cancellationToken);

        logger.LogInformation("CmsSettingsKey synchronization starting");
        var cmsSettingsKeys = k11Context.CmsSettingsKeys
                .Where(csk => csk.SiteId == null)
                .AsNoTrackingWithIdentityResolution()
            ;

        foreach (var k11CmsSettingsKey in cmsSettingsKeys)
        {
            protocol.FetchedSource(k11CmsSettingsKey);

            var kxoGlobalSettingsKey = GetKxoSettingsKey(k11CmsSettingsKey);

            var canBeMigrated = !kxoGlobalSettingsKey?.KeyIsHidden ?? false;
            var kxoCmsSettingsKey = k11CmsSettingsKey.SiteId is null ? kxoGlobalSettingsKey : GetKxoSettingsKey(k11CmsSettingsKey);

            if (!canBeMigrated)
            {
                logger.LogInformation("Setting with key '{KeyName}' is currently not supported for migration", k11CmsSettingsKey.KeyName);
                protocol.Append(
                    HandbookReferences
                        .NotCurrentlySupportedSkip<SettingsKeyInfo>()
                        .WithId(nameof(k11CmsSettingsKey.KeyId), k11CmsSettingsKey.KeyId)
                        .WithMessage("Settings key is not supported in target instance")
                        .WithData(new
                        {
                            k11CmsSettingsKey.KeyName,
                            k11CmsSettingsKey.SiteId,
                            k11CmsSettingsKey.KeyGuid
                        })
                );
                continue;
            }

            protocol.FetchedTarget(kxoCmsSettingsKey);

            if (entityConfiguration.ExcludeCodeNames.Contains(k11CmsSettingsKey.KeyName))
            {
                protocol.Warning(HandbookReferences.CmsSettingsKeyExclusionListSkip, k11CmsSettingsKey);
                logger.LogWarning("KeyName {KeyName} is excluded => skipping", k11CmsSettingsKey.KeyName);
                continue;
            }

            var mapped = mapper.Map(k11CmsSettingsKey, kxoCmsSettingsKey);
            protocol.MappedTarget(mapped);

            if (mapped is { Success: true } result)
            {
                ArgumentNullException.ThrowIfNull(result.Item, nameof(result.Item));

                SettingsKeyInfoProvider.ProviderObject.Set(result.Item);

                protocol.Success(k11CmsSettingsKey, kxoCmsSettingsKey, mapped);
                logger.LogEntitySetAction(result.NewInstance, result.Item);
            }
        }

        return new GenericCommandResult();
    }

    private SettingsKeyInfo? GetKxoSettingsKey(CmsSettingsKey k11CmsSettingsKey)
    {
        return SettingsKeyInfoProvider.ProviderObject.Get(k11CmsSettingsKey.KeyName);
    }
}