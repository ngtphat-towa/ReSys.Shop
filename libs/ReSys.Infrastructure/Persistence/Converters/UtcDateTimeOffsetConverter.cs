using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ReSys.Infrastructure.Persistence.Converters;

public class UtcDateTimeOffsetConverter : ValueConverter<DateTimeOffset, DateTimeOffset>
{
    public UtcDateTimeOffsetConverter()
        : base(d => d.ToUniversalTime(), d => d.ToUniversalTime())
    {
    }
}
