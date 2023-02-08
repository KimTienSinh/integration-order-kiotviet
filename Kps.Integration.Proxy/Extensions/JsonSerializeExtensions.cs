using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Kps.Integration.Proxy.Extensions;

public static class JsonSerializeExtensions
{
  public static string ToSeparatedCase(string s, char separator)
        {
            if (string.IsNullOrEmpty(s))
            {
                return s;
            }

            StringBuilder stringBuilder = new();
            SeparatedCaseState separatedCaseState = SeparatedCaseState.Start;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == ' ')
                {
                    if (separatedCaseState != 0)
                    {
                        separatedCaseState = SeparatedCaseState.NewWord;
                    }
                }
                else if (char.IsUpper(s[i]))
                {
                    switch (separatedCaseState)
                    {
                        case SeparatedCaseState.Upper:
                            {
                                bool flag = i + 1 < s.Length;
                                if (i > 0 && flag)
                                {
                                    char c = s[i + 1];
                                    if (!char.IsUpper(c) && c != separator)
                                    {
                                        stringBuilder.Append(separator);
                                    }
                                }

                                break;
                            }
                        case SeparatedCaseState.Lower:
                        case SeparatedCaseState.NewWord:
                            stringBuilder.Append(separator);
                            break;
                    }

                    char value = char.ToLower(s[i], CultureInfo.InvariantCulture);
                    stringBuilder.Append(value);
                    separatedCaseState = SeparatedCaseState.Upper;
                }
                else if (s[i] == separator)
                {
                    stringBuilder.Append(separator);
                    separatedCaseState = SeparatedCaseState.Start;
                }
                else
                {
                    if (separatedCaseState == SeparatedCaseState.NewWord)
                    {
                        stringBuilder.Append(separator);
                    }

                    stringBuilder.Append(s[i]);
                    separatedCaseState = SeparatedCaseState.Lower;
                }
            }

            return stringBuilder.ToString();
        }

}
internal enum SeparatedCaseState
{
    Start,
    Lower,
    Upper,
    NewWord
}
public class SnakeCaseNamingPolicy : JsonNamingPolicy
{
    public override string ConvertName(string name) 
        => JsonSerializeExtensions.ToSeparatedCase(name, '_');
}