using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace CoinGecko.Api.Internal;

/// <summary>
/// Minimal, AOT-safe query-string builder. Uses <see cref="CultureInfo.InvariantCulture"/>
/// for numerics and URL-escapes all values. Intended for internal use by sub-clients.
/// </summary>
internal sealed class QueryStringBuilder
{
    private readonly StringBuilder _sb = new();
    private bool _hasAny;

    public QueryStringBuilder Add(string name, string? value)
    {
        if (value is null)
        {
            return this;
        }

        Append(name, value);
        return this;
    }

    public QueryStringBuilder Add(string name, int? value)
        => value is null ? this : Add(name, value.Value.ToString(CultureInfo.InvariantCulture));

    public QueryStringBuilder Add(string name, long? value)
        => value is null ? this : Add(name, value.Value.ToString(CultureInfo.InvariantCulture));

    public QueryStringBuilder Add(string name, decimal? value)
        => value is null ? this : Add(name, value.Value.ToString(CultureInfo.InvariantCulture));

    public QueryStringBuilder Add(string name, bool? value)
        => value is null ? this : Add(name, value.Value ? "true" : "false");

    public QueryStringBuilder AddCoinGeckoDate(string name, DateOnly? value)
        => value is null ? this : Add(name, value.Value.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture));

    public QueryStringBuilder AddUnixSeconds(string name, DateTimeOffset? value)
        => value is null ? this : Add(name, value.Value.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture));

    public QueryStringBuilder AddEnum<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] TEnum>(string name, TEnum value)
        where TEnum : struct, Enum
    {
        var member = typeof(TEnum).GetField(value.ToString());
        var em = member?.GetCustomAttribute<EnumMemberAttribute>();
        var wire = em?.Value ?? value.ToString();
        return Add(name, wire);
    }

    public QueryStringBuilder AddEnum<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] TEnum>(string name, TEnum? value)
        where TEnum : struct, Enum
    {
        if (value is null)
        {
            return this;
        }

        var member = typeof(TEnum).GetField(value.Value.ToString());
        var em = member?.GetCustomAttribute<EnumMemberAttribute>();
        var wire = em?.Value ?? value.Value.ToString();
        return Add(name, wire);
    }

    public QueryStringBuilder AddList(string name, IReadOnlyCollection<string>? values, string separator = ",")
    {
        if (values is null || values.Count == 0)
        {
            return this;
        }

        return Add(name, string.Join(separator, values));
    }

    public QueryStringBuilder AddEnumList<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] TEnum>(string name, IReadOnlyCollection<TEnum>? values, string separator = ",")
        where TEnum : struct, Enum
    {
        if (values is null || values.Count == 0)
        {
            return this;
        }

        var wire = values.Select(v =>
        {
            var member = typeof(TEnum).GetField(v.ToString());
            var em = member?.GetCustomAttribute<EnumMemberAttribute>();
            return em?.Value ?? v.ToString();
        });

        return Add(name, string.Join(separator, wire));
    }

    private void Append(string name, string value)
    {
        _sb.Append(_hasAny ? '&' : '?');
        _sb.Append(Uri.EscapeDataString(name));
        _sb.Append('=');
        _sb.Append(Uri.EscapeDataString(value));
        _hasAny = true;
    }

    public override string ToString() => _sb.ToString();
}
