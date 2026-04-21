using System.Text;

namespace CoinGecko.Api.Internal;

/// <summary>Expands <c>{name}</c> placeholders in path templates with URL-escaped values.</summary>
internal static class UriTemplateExpander
{
    public static string Expand(string template, IReadOnlyList<(string Name, string Value)> values)
    {
        ArgumentException.ThrowIfNullOrEmpty(template);

        var sb = new StringBuilder(template.Length + 16);
        var i = 0;
        while (i < template.Length)
        {
            var open = template.IndexOf('{', i);
            if (open < 0)
            {
                sb.Append(template, i, template.Length - i);
                break;
            }

            sb.Append(template, i, open - i);

            var close = template.IndexOf('}', open + 1);
            if (close < 0)
            {
                throw new ArgumentException($"Unclosed placeholder in template: {template}");
            }

            var name = template[(open + 1)..close];
            var found = false;
            for (var k = 0; k < values.Count; k++)
            {
                if (values[k].Name == name)
                {
                    sb.Append(Uri.EscapeDataString(values[k].Value));
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                throw new ArgumentException($"No value supplied for placeholder '{{{name}}}' in template '{template}'.");
            }

            i = close + 1;
        }

        return sb.ToString();
    }
}
