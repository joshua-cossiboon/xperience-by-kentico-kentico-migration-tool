using Americaneagle.Toolkit.Customization.Extensions;
using Americaneagle.Toolkit.Customization.Services;
using CMS.ContentEngine;
using CMS.ContentEngine.Internal;
using CMS.DataEngine;
using CMS.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Migration.Tool.KXP.Api.Services.CmsClass;
using Migration.Tool.Source.Services;

public static class IServiceCollectionExtensions
{
    private const int FolderMaxLength = 50;
    private const int ItemMaxLength = 100;

    public static IServiceCollection UseAcceleratorCustomizations(this IServiceCollection services)
    {
        // services.AddSingleton<IAssetFacade, AmericaneagleAssetFacade>();
        // services.AddTransient<IFieldMigration, AmericaneagleAssetMigration>();

        RegisterEventHandlers();

        return services;
    }

    private static void RegisterEventHandlers()
    {
        // Fix content item names and code names
        ContentItemInfo.TYPEINFO.Events.Insert.Before += (sender, e) =>
            HandleInsertEvent(e.Object, ItemMaxLength, ItemMaxLength);

        // Fix language metadata name length
        ContentItemLanguageMetadataInfo.TYPEINFO.Events.Insert.Before += (sender, e) =>
            HandleInsertEvent(e.Object, ItemMaxLength, ItemMaxLength);

        // Fix folder issues
        ContentFolderInfo.TYPEINFO.Events.Insert.Before += (sender, e) =>
            HandleInsertEvent(e.Object, FolderMaxLength, FolderMaxLength);
    }

    private static void HandleInsertEvent(object sender, int maxDisplayNameLength, int maxCodeNameLength)
    {
        BaseInfo? baseInfo = sender as BaseInfo;

        if (baseInfo is not null)
        {
            FixDisplayName(baseInfo, maxDisplayNameLength);
            FixCodeName(baseInfo, maxCodeNameLength);
        }
    }

    private static void FixDisplayName(BaseInfo baseInfo, int maxLength)
    {
        if (!HasValidDisplayNameColumn(baseInfo, out var displayNameColumn))
        {
            return;
        }

        var currentDisplayName = baseInfo.GetStringValue(displayNameColumn, string.Empty) ?? string.Empty;
        if (currentDisplayName.Length <= maxLength)
        {
            return;
        }

        var truncatedDisplayName = currentDisplayName.Substring(0, maxLength);
        baseInfo.SetValue(displayNameColumn, truncatedDisplayName);
    }

    private static void FixCodeName(BaseInfo baseInfo, int maxLength, bool appendRandomGuid = false)
    {
        if (string.IsNullOrEmpty(baseInfo.TypeInfo.CodeNameColumn))
        {
            return;
        }

        var currentCodeName = baseInfo.GetStringValue(baseInfo.TypeInfo.CodeNameColumn, string.Empty);
        if (string.IsNullOrEmpty(currentCodeName))
        {
            return;
        }

        var newCodeName = ValidationHelper.GetCodeName(currentCodeName);
        if (appendRandomGuid)
        {
            var suffix = Guid.NewGuid().ToString();

            var maxLengthPreSuffix = maxLength - suffix.Length - 1;
            newCodeName = newCodeName.Substring(0, Math.Min(newCodeName.Length, maxLengthPreSuffix)) + $"_{suffix}";
        }

        baseInfo.SetValue(baseInfo.TypeInfo.CodeNameColumn, newCodeName);

        if (!baseInfo.CheckUniqueCodeName())
        {
            FixCodeName(baseInfo, maxLength, true);
        }
    }

    private static bool HasValidDisplayNameColumn(BaseInfo baseInfo, out string? displayNameColumn) => Convert.ToBoolean(displayNameColumn = baseInfo.TypeInfo.DisplayNameColumn);
}
