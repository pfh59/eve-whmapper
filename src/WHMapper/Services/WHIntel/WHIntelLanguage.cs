using System.Text.RegularExpressions;
using Microsoft.Net.Http.Headers;

namespace WHMapper.Services.WHIntel;

/// <summary>
/// Language of the AI report, as a BCP 47 tag taken from the viewer's browser.
/// </summary>
public static partial class WHIntelLanguage
{
    public const string DEFAULT_LANGUAGE_TAG = "en";

    private const string ANY_LANGUAGE = "*";
    private const char SUBTAG_SEPARATOR = '-';

    /// <summary>
    /// Preferred language of an <c>Accept-Language</c> header value; <see cref="DEFAULT_LANGUAGE_TAG"/> when none is usable.
    /// </summary>
    public static string FromAcceptLanguage(string? acceptLanguage)
    {
        if (!StringWithQualityHeaderValue.TryParseList(acceptLanguage?.Split(','), out var languages))
            return DEFAULT_LANGUAGE_TAG;

        var preferred = languages
            .Where(x => x.Value.Value != ANY_LANGUAGE)
            .OrderByDescending(x => x.Quality ?? 1)
            .Select(x => x.Value.Value)
            .FirstOrDefault();

        return Normalize(preferred);
    }

    /// <summary>
    /// Returns the tag when it is a well-formed BCP 47 tag, <see cref="DEFAULT_LANGUAGE_TAG"/> otherwise.
    /// </summary>
    /// <remarks>The tag is sent to the AI model, so anything else than a plain tag is rejected.</remarks>
    public static string Normalize(string? languageTag) =>
        languageTag != null && LanguageTag().IsMatch(languageTag) ? languageTag : DEFAULT_LANGUAGE_TAG;

    /// <summary>
    /// Primary language of a tag, e.g. <c>fr</c> for <c>fr-CA</c>; reports are shared per primary language.
    /// </summary>
    public static string GetPrimaryLanguage(string languageTag) =>
        Normalize(languageTag).Split(SUBTAG_SEPARATOR)[0].ToLowerInvariant();

    [GeneratedRegex("^[A-Za-z]{2,8}(-[A-Za-z0-9]{1,8}){0,3}$")]
    private static partial Regex LanguageTag();
}
