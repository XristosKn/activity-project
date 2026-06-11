using Avalonia.Media;

namespace ActivityProjectApp.Helpers
{
    public static class CategoryStyleHelper
    {
        public static string GetCategoryColorHex(string category)
        {
            string normalizedCategory = category.ToLower();

            if (normalizedCategory.Contains("water"))
            {
                return "#0EA5E9";
            }

            if (normalizedCategory.Contains("fitness") || normalizedCategory.Contains("wellness"))
            {
                return "#92400E";
            }

            if (normalizedCategory.Contains("mountain") || normalizedCategory.Contains("outdoor"))
            {
                return "#16A34A";
            }

            if (normalizedCategory.Contains("team"))
            {
                return "#7C3AED";
            }

            if (normalizedCategory.Contains("cultural") || normalizedCategory.Contains("creative"))
            {
                return "#DB2777";
            }

            if (normalizedCategory.Contains("food") || normalizedCategory.Contains("drink"))
            {
                return "#EA580C";
            }

            if (normalizedCategory.Contains("kids"))
            {
                return "#F59E0B";
            }

            if (normalizedCategory.Contains("extreme"))
            {
                return "#DC2626";
            }

            if (normalizedCategory.Contains("winter"))
            {
                return "#2563EB";
            }

            if (normalizedCategory.Contains("dance") || normalizedCategory.Contains("music"))
            {
                return "#9333EA";
            }

            return "#374151";
        }

        public static IBrush GetAvaloniaBrush(string category)
        {
            return new SolidColorBrush(Color.Parse(GetCategoryColorHex(category)));
        }

        public static Mapsui.Styles.Color GetMapColor(string category)
        {
            return Mapsui.Styles.Color.FromString(GetCategoryColorHex(category));
        }
    }
}