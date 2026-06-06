namespace mini_mes_be.Extensions;

public static class StringExtensions
{
    public static string ToSnakeCase(this string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        var builder = new System.Text.StringBuilder();
        for (int i = 0; i < input.Length; i++)
        {
            var c = input[i];
            if (i > 0 && char.IsUpper(c))
            {
                var prev = input[i - 1];
                if (!char.IsUpper(prev) || (i + 1 < input.Length && char.IsLower(input[i + 1])))
                {
                    builder.Append('_');
                }
            }
            builder.Append(char.ToLowerInvariant(c));
        }
        return builder.ToString();
    }
}
