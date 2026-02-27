using System.Collections.Generic;
using System.Globalization;

namespace TMPro.Localization
{
    public interface ILocalizationLoader
    {
        Dictionary<long, LocalizedTextEntry> Load(CultureInfo culture);
    }
}