namespace Migration.Toolkit.Source.Services;

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Migration.Toolkit.Common;
using Migration.Toolkit.Common.Services;
using Migration.Toolkit.KXP.Context;
using Migration.Toolkit.Source.Model;

public class PrimaryKeyLocatorService(
    ILogger<PrimaryKeyLocatorService> logger,
    IDbContextFactory<KxpContext> kxpContextFactory,
    ModelFacade modelFacade
    ) : IPrimaryKeyLocatorService
{
    private class KeyEqualityComparerWithLambda<T> : IEqualityComparer<T>
    {
        private readonly Func<T?, T?, bool> _equalityComparer;

        public KeyEqualityComparerWithLambda(Func<T?,T?,bool> equalityComparer)
        {
            _equalityComparer = equalityComparer;
        }

        public bool Equals(T? x, T? y) => _equalityComparer.Invoke(x, y);

        public int GetHashCode(T obj) => obj?.GetHashCode() ?? 0;
    }

    private record CmsUserKey(Guid UserGuid, string UserName);

    public IEnumerable<SourceTargetKeyMapping> SelectAll<T>(Expression<Func<T, object>> keyNameSelector)
    {
        using var kxpContext = kxpContextFactory.CreateDbContext();

        var sourceType = typeof(T);
        var memberName = keyNameSelector.GetMemberName();

        logger.LogTrace("Preload of entity {Entity} member {MemberName} mapping requested", sourceType.Name, memberName);

        if (sourceType == typeof(ICmsUser) && memberName == nameof(ICmsUser.UserID))
        {
            var sourceUsers = modelFacade.SelectAll<ICmsUser>().ToList();
            // var sourceUsers = kx12Context.CmsUsers.Select(x => new { x.UserId, x.UserGuid, x.UserName }).ToList();
            var targetUsers = kxpContext.CmsUsers.Select(x => new { x.UserId, x.UserName, x.UserGuid }).ToList();

            var result = sourceUsers.Join(targetUsers,
                a => new CmsUserKey(a.UserGUID, a.UserName),
                b => new CmsUserKey(b.UserGuid, b.UserName),
                (a, b) => new SourceTargetKeyMapping(a.UserID, b.UserId),
                new KeyEqualityComparerWithLambda<CmsUserKey>((ak, bk) => (ak?.UserGuid == bk?.UserGuid || ak?.UserName == bk?.UserName) && ak != null && bk != null)
            );

            foreach (var resultingMapping in result)
            {
                yield return resultingMapping;
            }

            yield break;
        }

        if (sourceType == typeof(IOmContact) && memberName == nameof(IOmContact.ContactID))
        {
            var source = modelFacade.SelectAll<IOmContact>()
                .OrderBy(c => c.ContactCreated)
                .Select(x => new { x.ContactID, x.ContactGUID }).ToList();
            var target = kxpContext.OmContacts
                .OrderBy(c => c.ContactCreated)
                .Select(x => new { x.ContactId, x.ContactGuid }).ToList();

            var result = source.Join(target,
                a => a.ContactGUID,
                b => b.ContactGuid,
                (a, b) => new SourceTargetKeyMapping(a.ContactID, b.ContactId)
            );

            foreach (var resultingMapping in result)
            {
                yield return resultingMapping;
            }

            yield break;
        }

        if (sourceType == typeof(ICmsTree) && memberName == nameof(ICmsTree.NodeID))
        {
            var source = modelFacade.SelectAll<ICmsTree>().Select(x => new { x.NodeID, x.NodeGUID }).ToList();
            var target = kxpContext.CmsChannels.Select(x => new { x.ChannelId, x.ChannelGuid }).ToList();

            var result = source.Join(target,
                a => a.NodeGUID,
                b => b.ChannelGuid,
                (a, b) => new SourceTargetKeyMapping(a.NodeID, b.ChannelId)
            );

            foreach (var resultingMapping in result)
            {
                yield return resultingMapping;
            }

            yield break;
        }

        if (sourceType == typeof(ICmsState) && memberName == nameof(ICmsState.StateID))
        {
            var source = modelFacade.SelectAll<ICmsState>().Select(x => new { x.StateID, x.StateName }).ToList();
            var target = kxpContext.CmsStates.Select(x => new {  x.StateId, x.StateName }).ToList();

            var result = source.Join(target,
                a => a.StateName,
                b => b.StateName,
                (a, b) => new SourceTargetKeyMapping(a.StateID, b.StateId)
            );

            foreach (var resultingMapping in result)
            {
                yield return resultingMapping;
            }

            yield break;
        }

        if (sourceType == typeof(ICmsCountry) && memberName == nameof(ICmsCountry.CountryID))
        {
            var source = modelFacade.SelectAll<ICmsCountry>().Select(x => new { x.CountryID, x.CountryName }).ToList();
            var target = kxpContext.CmsCountries.Select(x => new {  x.CountryId, x.CountryName }).ToList();

            var result = source.Join(target,
                a => a.CountryName,
                b => b.CountryName,
                (a, b) => new SourceTargetKeyMapping(a.CountryID, b.CountryId)
            );

            foreach (var resultingMapping in result)
            {
                yield return resultingMapping;
            }

            yield break;
        }



        throw new NotImplementedException();
    }

    public bool TryLocate<T>(Expression<Func<T, object>> keyNameSelector, int sourceId, out int targetId)
    {
        using var kxpContext = kxpContextFactory.CreateDbContext();
        // using var KX12Context = _kx12ContextFactory.CreateDbContext();

        var sourceType = typeof(T);
        targetId = -1;
        try
        {
            if (sourceType == typeof(ICmsResource))
            {
                // var k12Guid = KX12Context.CmsResources.Where(c => c.ResourceId == sourceId).Select(x => x.ResourceGuid).Single();
                var sourceGuid = modelFacade.SelectById<ICmsResource>(sourceId)?.ResourceGUID;
                targetId = kxpContext.CmsResources.Where(x => x.ResourceGuid == sourceGuid).Select(x => x.ResourceId).Single();
                return true;
            }

            if (sourceType == typeof(ICmsClass))
            {
                // var k12Guid = KX12Context.CmsClasses.Where(c => c.ClassId == sourceId).Select(x => x.ClassGuid).Single();
                var sourceGuid = modelFacade.SelectById<ICmsClass>(sourceId)?.ClassGUID;
                targetId = kxpContext.CmsClasses.Where(x => x.ClassGuid == sourceGuid).Select(x => x.ClassId).Single();
                return true;
            }

            if (sourceType == typeof(ICmsUser))
            {
                // TODO tomas.krch 2024-03-20: CHECK THIS
                var source = modelFacade.SelectById<ICmsUser>(sourceId);
                // var k12User = KX12Context.CmsUsers.Where(c => c.UserId == sourceId).Select(x => new { x.UserGuid, x.UserName }).Single();
                targetId = kxpContext.CmsUsers.Where(x => x.UserGuid == source.UserGUID || x.UserName == source.UserName).Select(x => x.UserId).Single();
                return true;
            }

            if (sourceType == typeof(ICmsRole))
            {
                // var k12User = KX12Context.CmsRoles.Where(c => c.RoleId == sourceId).Select(x => new { x.RoleGuid }).Single();
                var sourceGuid = modelFacade.SelectById<ICmsRole>(sourceId)?.RoleGUID;
                targetId = kxpContext.CmsRoles.Where(x => x.RoleGuid == sourceGuid).Select(x => x.RoleId).Single();
                return true;
            }

            if (sourceType == typeof(ICmsSite))
            {
                //var k12Guid = KX12Context.CmsSites.Where(c => c.SiteId == sourceId).Select(x => x.SiteGuid).Single();
                var sourceGuid = modelFacade.SelectById<ICmsSite>(sourceId)?.SiteGUID;
                targetId = kxpContext.CmsChannels.Where(x => x.ChannelGuid == sourceGuid).Select(x => x.ChannelId).Single();
                return true;
            }

            if (sourceType == typeof(ICmsState))
            {
                // var k12CodeName = KX12Context.CmsStates.Where(c => c.StateId == sourceId).Select(x => x.StateName).Single();
                var sourceName = modelFacade.SelectById<ICmsState>(sourceId)?.StateName;
                targetId = kxpContext.CmsStates.Where(x => x.StateName == sourceName).Select(x => x.StateId).Single();
                return true;
            }

            if (sourceType == typeof(ICmsCountry))
            {
                // var k12CodeName = KX12Context.CmsCountries.Where(c => c.CountryId == sourceId).Select(x => x.CountryName).Single();
                var sourceName = modelFacade.SelectById<ICmsCountry>(sourceId)?.CountryName;
                targetId = kxpContext.CmsCountries.Where(x => x.CountryName == sourceName).Select(x => x.CountryId).Single();
                return true;
            }

            if (sourceType == typeof(IOmContactStatus))
            {
                // var k12Guid = KX12Context.OmContactStatuses.Where(c => c.ContactStatusId == sourceId).Select(x => x.ContactStatusName).Single();
                var sourceName = modelFacade.SelectById<IOmContactStatus>(sourceId)?.ContactStatusName;
                targetId = kxpContext.OmContactStatuses.Where(x => x.ContactStatusName == sourceName).Select(x => x.ContactStatusId).Single();
                return true;
            }

            if (sourceType == typeof(IOmContact))
            {
                //var k12Guid = KX12Context.OmContacts.Where(c => c.ContactId == sourceId).Select(x => x.ContactGuid).Single();
                var sourceGuid = modelFacade.SelectById<IOmContact>(sourceId)?.ContactGUID;
                targetId = kxpContext.OmContacts.Where(x => x.ContactGuid == sourceGuid).Select(x => x.ContactId).Single();
                return true;
            }

            if (sourceType == typeof(ICmsTree))
            {
                // careful - cms.root will have different guid
                // var k12Guid = KX12Context.CmsTrees.Where(c => c.NodeId == sourceId).Select(x => x.NodeGuid).Single();
                var sourceGuid = modelFacade.SelectById<ICmsTree>(sourceId)?.NodeGUID;
                targetId = kxpContext.CmsChannels.Where(x => x.ChannelGuid == sourceGuid).Select(x => x.ChannelId).Single();
                return true;
            }
        }
        catch (InvalidOperationException ioex)
        {
            if (ioex.Message.StartsWith("Sequence contains no elements"))
            {
                logger.LogDebug("Mapping {SourceFullType} primary key: {SourceId} failed, {Message}", sourceType.FullName, sourceId, ioex.Message);
            }
            else
            {
                logger.LogWarning("Mapping {SourceFullType} primary key: {SourceId} failed, {Message}", sourceType.FullName, sourceId, ioex.Message);
            }
            return false;
        }
        finally
        {
            if (targetId != -1)
            {
                logger.LogTrace("Mapping {SourceFullType} primary key: {SourceId} to {TargetId}", sourceType.FullName, sourceId, targetId);
            }
        }

        logger.LogError("Mapping {SourceFullType} primary key is not supported", sourceType.FullName);
        targetId = -1;
        return false;
    }
}