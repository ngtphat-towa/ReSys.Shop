using ReSys.Core.Domain.Common.Abstractions;

namespace ReSys.Core.Domain.Settings;

public static class SettingEvents
{
    public record SettingCreated(Setting Setting) : IDomainEvent;
    public record SettingUpdated(Setting Setting) : IDomainEvent;
}
