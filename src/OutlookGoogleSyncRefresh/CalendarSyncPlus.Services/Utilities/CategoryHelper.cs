using System.Collections.Generic;
using System.Windows.Media;
using Microsoft.Office.Interop.Outlook;
using Category = CalendarSyncPlus.Services.Wrappers.Category;
using Exception = System.Exception;

namespace CalendarSyncPlus.Application.Utilities
{
    public class CategoryHelper
    {
        /*  ## COLOR       HEX     (RRR GGG BBB)
             1 Red         #E7A1A2 (231 161 162)
             2 Orange      #F9BA89 (249 186 137)
             3 Peach       #F7DD8F (247 221 143)
             4 Yellow      #FCFA90 (252 250 144)
             5 Green       #78D168 (120 209 104)
             6 Teal        #9FDCC9 (159 220 201)
             7 Olive       #C6D2B0 (198 210 176)
             8 Blue        #9DB7E8 (157 183 232)
             9 Purple      #B5A1E2 (181 161 226)
            10 Maroon      #daaec2 (218 174 194)
            11 Steel       #dad9dc (218 217 220)
            12 Dark Steel  #6b7994 (107 121 148)
            13 Grey        #bfbfbf (191 191 191)
            14 Dark Grey   #6f6f6f (111 111 111)
            15 Black       #4f4f4f ( 79  79  79)
            16 Dark Red    #c11a25 (193  26  37)
            17 Dark Orange #e2620d (226  98  13)
            18 Dark Peach  #c79930 (199 153  48)
            19 Dark Yellow #b9b300 (185 179   0)
            20 Dark Green  #368f2b ( 54 143  43)
            21 Dark Teal   #329b7a ( 50 155 122)
            22 Dark Olive  #778b45 (119 139  69)
            23 Dark Blue   #2858a5 ( 40  88 165)
            24 Dark Purple #5c3fa3 ( 92  63 163)
            25 Dark Maroon #93446b (147  68 107)*/
        private static readonly Dictionary<OlCategoryColor, string> _categoryColor;

        static CategoryHelper()
        {
            _categoryColor = new Dictionary<OlCategoryColor, string>
            {
                {OlCategoryColor.olCategoryColorNone, "#FFFFFF"},
                {OlCategoryColor.olCategoryColorRed, "#E7A1A2"},
                {OlCategoryColor.olCategoryColorOrange, "#F9BA89"},
                {OlCategoryColor.olCategoryColorPeach, "#F7DD8F"},
                {OlCategoryColor.olCategoryColorYellow, "#FCFA90"},
                {OlCategoryColor.olCategoryColorGreen, "#78D168"},
                {OlCategoryColor.olCategoryColorTeal, "#9FDCC9"},
                {OlCategoryColor.olCategoryColorOlive, "#C6D2B0"},
                {OlCategoryColor.olCategoryColorBlue, "#9DB7E8"},
                {OlCategoryColor.olCategoryColorPurple, "#B5A1E2"},
                {OlCategoryColor.olCategoryColorMaroon, "#daaec2"},
                {OlCategoryColor.olCategoryColorSteel, "#dad9dc"},
                {OlCategoryColor.olCategoryColorDarkSteel, "#6b7994"},
                {OlCategoryColor.olCategoryColorGray, "#bfbfbf"},
                {OlCategoryColor.olCategoryColorDarkGray, "#6f6f6f"},
                {OlCategoryColor.olCategoryColorBlack, "#4f4f4f"},
                {OlCategoryColor.olCategoryColorDarkRed, "#c11a25"},
                {OlCategoryColor.olCategoryColorDarkOrange, "#e2620d"},
                {OlCategoryColor.olCategoryColorDarkPeach, "#c79930"},
                {OlCategoryColor.olCategoryColorDarkYellow, "#b9b300"},
                {OlCategoryColor.olCategoryColorDarkGreen, "#368f2b"},
                {OlCategoryColor.olCategoryColorDarkTeal, "#329b7a"},
                {OlCategoryColor.olCategoryColorDarkOlive, "#778b45"},
                {OlCategoryColor.olCategoryColorDarkBlue, "#2858a5"},
                {OlCategoryColor.olCategoryColorDarkPurple, "#5c3fa3"},
                {OlCategoryColor.olCategoryColorDarkMaroon, "#93446b"},
            };
        }

        public static List<Category> GetCategories()
        {
            try
            {
                var categories = new List<Category>();
                foreach (var outlookColor in _categoryColor)
                {
                    var category = new Category
                    {
                        CategoryName = outlookColor.Key.ToString().Remove(0, "olCategoryColor".Length),
                        OutlookColor = outlookColor.Key,
                        HexValue = outlookColor.Value,
                        Color = (Color)ColorConverter.ConvertFromString(outlookColor.Value)
                    };
                    categories.Add(category);
                }
                return categories;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}