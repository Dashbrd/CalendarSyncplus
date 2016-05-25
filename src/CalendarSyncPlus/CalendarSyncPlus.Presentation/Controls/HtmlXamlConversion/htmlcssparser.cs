//---------------------------------------------------------------------------
// 
// File: HtmlXamlConverter.cs
//
// Copyright (C) Microsoft Corporation.  All rights reserved.
//
// Description: Prototype for Html - Xaml conversion 
//
//---------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml;

namespace CalendarSyncPlus.Presentation.Controls.HtmlXamlConversion
{
    // DependencyProperty

    // TextElement

    internal static class HtmlCssParser
    {
        private static readonly string[] _colors =
        {
            "aliceblue", "antiquewhite", "aqua", "aquamarine", "azure", "beige", "bisque", "black", "blanchedalmond",
            "blue", "blueviolet", "brown", "burlywood", "cadetblue", "chartreuse", "chocolate", "coral",
            "cornflowerblue", "cornsilk", "crimson", "cyan", "darkblue", "darkcyan", "darkgoldenrod", "darkgray",
            "darkgreen", "darkkhaki", "darkmagenta", "darkolivegreen", "darkorange", "darkorchid", "darkred",
            "darksalmon", "darkseagreen", "darkslateblue", "darkslategray", "darkturquoise", "darkviolet", "deeppink",
            "deepskyblue", "dimgray", "dodgerblue", "firebrick", "floralwhite", "forestgreen", "fuchsia", "gainsboro",
            "ghostwhite", "gold", "goldenrod", "gray", "green", "greenyellow", "honeydew", "hotpink", "indianred",
            "indigo", "ivory", "khaki", "lavender", "lavenderblush", "lawngreen", "lemonchiffon", "lightblue",
            "lightcoral",
            "lightcyan", "lightgoldenrodyellow", "lightgreen", "lightgrey", "lightpink", "lightsalmon", "lightseagreen",
            "lightskyblue", "lightslategray", "lightsteelblue", "lightyellow", "lime", "limegreen", "linen", "magenta",
            "maroon", "mediumaquamarine", "mediumblue", "mediumorchid", "mediumpurple", "mediumseagreen",
            "mediumslateblue",
            "mediumspringgreen", "mediumturquoise", "mediumvioletred", "midnightblue", "mintcream", "mistyrose",
            "moccasin",
            "navajowhite", "navy", "oldlace", "olive", "olivedrab", "orange", "orangered", "orchid", "palegoldenrod",
            "palegreen", "paleturquoise", "palevioletred", "papayawhip", "peachpuff", "peru", "pink", "plum",
            "powderblue",
            "purple", "red", "rosybrown", "royalblue", "saddlebrown", "salmon", "sandybrown", "seagreen", "seashell",
            "sienna", "silver", "skyblue", "slateblue", "slategray", "snow", "springgreen", "steelblue", "tan", "teal",
            "thistle", "tomato", "turquoise", "violet", "wheat", "white", "whitesmoke", "yellow", "yellowgreen"
        };

        private static readonly string[] _systemColors =
        {
            "activeborder", "activecaption", "appworkspace", "background", "buttonface", "buttonhighlight",
            "buttonshadow",
            "buttontext", "captiontext", "graytext", "highlight", "highlighttext", "inactiveborder", "inactivecaption",
            "inactivecaptiontext", "infobackground", "infotext", "menu", "menutext", "scrollbar", "threeddarkshadow",
            "threedface", "threedhighlight", "threedlightshadow", "threedshadow", "window", "windowframe", "windowtext"
        };

        // .................................................................
        //
        // Pasring CSS font Property
        //
        // .................................................................

        // CSS has five font properties: font-family, font-style, font-variant, font-weight, font-size.
        // An aggregated "font" property lets you specify in one action all the five in combination
        // with additional line-height property.
        // 
        // font-family: [<family-name>,]* [<family-name> | <generic-family>]
        //    generic-family: serif | sans-serif | monospace | cursive | fantasy
        //       The list of families sets priorities to choose fonts;
        //       Quotes not allowed around generic-family names
        // font-style: normal | italic | oblique
        // font-variant: normal | small-caps
        // font-weight: normal | bold | bolder | lighter | 100 ... 900 |
        //    Default is "normal", normal==400
        // font-size: <absolute-size> | <relative-size> | <length> | <percentage>
        //    absolute-size: xx-small | x-small | small | medium | large | x-large | xx-large
        //    relative-size: larger | smaller
        //    length: <point> | <pica> | <ex> | <em> | <points> | <millimeters> | <centimeters> | <inches>
        //    Default: medium
        // font: [ <font-style> || <font-variant> || <font-weight ]? <font-size> [ / <line-height> ]? <font-family>

        private static readonly string[] _fontGenericFamilies =
        {
            "serif", "sans-serif", "monospace", "cursive",
            "fantasy"
        };

        private static readonly string[] _fontStyles = {"normal", "italic", "oblique"};
        private static readonly string[] _fontVariants = {"normal", "small-caps"};

        private static readonly string[] _fontWeights =
        {
            "normal", "bold", "bolder", "lighter", "100", "200", "300",
            "400", "500", "600", "700", "800", "900"
        };

        private static readonly string[] _fontAbsoluteSizes =
        {
            "xx-small", "x-small", "small", "medium", "large",
            "x-large", "xx-large"
        };

        private static readonly string[] _fontRelativeSizes = {"larger", "smaller"};
        private static readonly string[] _fontSizeUnits = {"px", "mm", "cm", "in", "pt", "pc", "em", "ex", "%"};
        // .................................................................
        //
        // Pasring CSS list-style Property
        //
        // .................................................................

        // list-style: [ <list-style-type> || <list-style-position> || <list-style-image> ]

        private static readonly string[] _listStyleTypes =
        {
            "disc", "circle", "square", "decimal", "lower-roman",
            "upper-roman", "lower-alpha", "upper-alpha", "none"
        };

        private static readonly string[] _listStylePositions = {"inside", "outside"};
        // .................................................................
        //
        // Pasring CSS text-decorations Property
        //
        // .................................................................

        private static readonly string[] _textDecorations = {"none", "underline", "overline", "line-through", "blink"};
        // .................................................................
        //
        // Pasring CSS text-transform Property
        //
        // .................................................................

        private static readonly string[] _textTransforms = {"none", "capitalize", "uppercase", "lowercase"};
        // .................................................................
        //
        // Pasring CSS text-align Property
        //
        // .................................................................

        private static readonly string[] _textAligns = {"left", "right", "center", "justify"};
        // .................................................................
        //
        // Pasring CSS vertical-align Property
        //
        // .................................................................

        private static readonly string[] _verticalAligns =
        {
            "baseline", "sub", "super", "top", "text-top", "middle",
            "bottom", "text-bottom"
        };

        // .................................................................
        //
        // Pasring CSS float Property
        //
        // .................................................................

        private static readonly string[] _floats = {"left", "right", "none"};
        // .................................................................
        //
        // Pasring CSS clear Property
        //
        // .................................................................

        private static readonly string[] _clears = {"none", "left", "right", "both"};
        // .................................................................
        //
        // Pasring CSS border-style Propertie
        //
        // .................................................................

        private static readonly string[] _borderStyles =
        {
            "none", "dotted", "dashed", "solid", "double", "groove",
            "ridge", "inset", "outset"
        };

        // .................................................................
        //
        //  What are these definitions doing here:
        //
        // .................................................................

        private static string[] _blocks = {"block", "inline", "list-item", "none"};
        // .................................................................
        //
        // Processing CSS Attributes
        //
        // .................................................................

        internal static void GetElementPropertiesFromCssAttributes(XmlElement htmlElement, string elementName,
            CssStylesheet stylesheet, Hashtable localProperties, List<XmlElement> sourceContext)
        {
            var styleFromStylesheet = stylesheet.GetStyle(elementName, sourceContext);

            var styleInline = HtmlToXamlConverter.GetAttribute(htmlElement, "style");

            // Combine styles from stylesheet and from inline attribute.
            // The order is important - the latter styles will override the former.
            var style = styleFromStylesheet != null ? styleFromStylesheet : null;
            if (styleInline != null)
            {
                style = style == null ? styleInline : style + ";" + styleInline;
            }

            // Apply local style to current formatting properties
            if (style != null)
            {
                var styleValues = style.Split(';');
                for (var i = 0; i < styleValues.Length; i++)
                {
                    string[] styleNameValue;

                    styleNameValue = styleValues[i].Split(':');
                    if (styleNameValue.Length == 2)
                    {
                        var styleName = styleNameValue[0].Trim().ToLower();
                        var styleValue = HtmlToXamlConverter.UnQuote(styleNameValue[1].Trim()).ToLower();
                        var nextIndex = 0;

                        switch (styleName)
                        {
                            case "font":
                                ParseCssFont(styleValue, localProperties);
                                break;
                            case "font-family":
                                ParseCssFontFamily(styleValue, ref nextIndex, localProperties);
                                break;
                            case "font-size":
                                ParseCssSize(styleValue, ref nextIndex, localProperties, "font-size",
                                    /*mustBeNonNegative:*/true);
                                break;
                            case "font-style":
                                ParseCssFontStyle(styleValue, ref nextIndex, localProperties);
                                break;
                            case "font-weight":
                                ParseCssFontWeight(styleValue, ref nextIndex, localProperties);
                                break;
                            case "font-variant":
                                ParseCssFontVariant(styleValue, ref nextIndex, localProperties);
                                break;
                            case "line-height":
                                ParseCssSize(styleValue, ref nextIndex, localProperties, "line-height",
                                    /*mustBeNonNegative:*/true);
                                break;
                            case "word-spacing":
                                //  Implement word-spacing conversion
                                break;
                            case "letter-spacing":
                                //  Implement letter-spacing conversion
                                break;
                            case "color":
                                ParseCssColor(styleValue, ref nextIndex, localProperties, "color");
                                break;

                            case "text-decoration":
                                ParseCssTextDecoration(styleValue, ref nextIndex, localProperties);
                                break;

                            case "text-transform":
                                ParseCssTextTransform(styleValue, ref nextIndex, localProperties);
                                break;

                            case "background-color":
                                ParseCssColor(styleValue, ref nextIndex, localProperties, "background-color");
                                break;
                            case "background":
                                // TODO: need to parse composite background property
                                ParseCssBackground(styleValue, ref nextIndex, localProperties);
                                break;

                            case "text-align":
                                ParseCssTextAlign(styleValue, ref nextIndex, localProperties);
                                break;
                            case "vertical-align":
                                ParseCssVerticalAlign(styleValue, ref nextIndex, localProperties);
                                break;
                            case "text-indent":
                                ParseCssSize(styleValue, ref nextIndex, localProperties, "text-indent",
                                    /*mustBeNonNegative:*/false);
                                break;

                            case "width":
                            case "height":
                                ParseCssSize(styleValue, ref nextIndex, localProperties, styleName,
                                    /*mustBeNonNegative:*/true);
                                break;

                            case "margin": // top/right/bottom/left
                                ParseCssRectangleProperty(styleValue, ref nextIndex, localProperties, styleName);
                                break;
                            case "margin-top":
                            case "margin-right":
                            case "margin-bottom":
                            case "margin-left":
                                ParseCssSize(styleValue, ref nextIndex, localProperties, styleName,
                                    /*mustBeNonNegative:*/true);
                                break;

                            case "padding":
                                ParseCssRectangleProperty(styleValue, ref nextIndex, localProperties, styleName);
                                break;
                            case "padding-top":
                            case "padding-right":
                            case "padding-bottom":
                            case "padding-left":
                                ParseCssSize(styleValue, ref nextIndex, localProperties, styleName,
                                    /*mustBeNonNegative:*/true);
                                break;

                            case "border":
                                ParseCssBorder(styleValue, ref nextIndex, localProperties);
                                break;
                            case "border-style":
                            case "border-width":
                            case "border-color":
                                ParseCssRectangleProperty(styleValue, ref nextIndex, localProperties, styleName);
                                break;
                            case "border-top":
                            case "border-right":
                            case "border-left":
                            case "border-bottom":
                                //  Parse css border style
                                break;

                            // NOTE: css names for elementary border styles have side indications in the middle (top/bottom/left/right)
                            // In our internal notation we intentionally put them at the end - to unify processing in ParseCssRectangleProperty method
                            case "border-top-style":
                            case "border-right-style":
                            case "border-left-style":
                            case "border-bottom-style":
                            case "border-top-color":
                            case "border-right-color":
                            case "border-left-color":
                            case "border-bottom-color":
                            case "border-top-width":
                            case "border-right-width":
                            case "border-left-width":
                            case "border-bottom-width":
                                //  Parse css border style
                                break;

                            case "display":
                                //  Implement display style conversion
                                break;

                            case "float":
                                ParseCssFloat(styleValue, ref nextIndex, localProperties);
                                break;
                            case "clear":
                                ParseCssClear(styleValue, ref nextIndex, localProperties);
                                break;

                            default:
                                break;
                        }
                    }
                }
            }
        }

        // .................................................................
        //
        // Parsing CSS - Lexical Helpers
        //
        // .................................................................

        // Skips whitespaces in style values
        private static void ParseWhiteSpace(string styleValue, ref int nextIndex)
        {
            while (nextIndex < styleValue.Length && char.IsWhiteSpace(styleValue[nextIndex]))
            {
                nextIndex++;
            }
        }

        // Checks if the following character matches to a given word and advances nextIndex
        // by the word's length in case of success.
        // Otherwise leaves nextIndex in place (except for possible whitespaces).
        // Returns true or false depending on success or failure of matching.
        private static bool ParseWord(string word, string styleValue, ref int nextIndex)
        {
            ParseWhiteSpace(styleValue, ref nextIndex);

            for (var i = 0; i < word.Length; i++)
            {
                if (!(nextIndex + i < styleValue.Length && word[i] == styleValue[nextIndex + i]))
                {
                    return false;
                }
            }

            if (nextIndex + word.Length < styleValue.Length && char.IsLetterOrDigit(styleValue[nextIndex + word.Length]))
            {
                return false;
            }

            nextIndex += word.Length;
            return true;
        }

        // CHecks whether the following character sequence matches to one of the given words,
        // and advances the nextIndex to matched word length.
        // Returns null in case if there is no match or the word matched.
        private static string ParseWordEnumeration(string[] words, string styleValue, ref int nextIndex)
        {
            for (var i = 0; i < words.Length; i++)
            {
                if (ParseWord(words[i], styleValue, ref nextIndex))
                {
                    return words[i];
                }
            }

            return null;
        }

        private static void ParseWordEnumeration(string[] words, string styleValue, ref int nextIndex,
            Hashtable localProperties, string attributeName)
        {
            var attributeValue = ParseWordEnumeration(words, styleValue, ref nextIndex);
            if (attributeValue != null)
            {
                localProperties[attributeName] = attributeValue;
            }
        }

        private static string ParseCssSize(string styleValue, ref int nextIndex, bool mustBeNonNegative)
        {
            ParseWhiteSpace(styleValue, ref nextIndex);

            var startIndex = nextIndex;

            // Parse optional munis sign
            if (nextIndex < styleValue.Length && styleValue[nextIndex] == '-')
            {
                nextIndex++;
            }

            if (nextIndex < styleValue.Length && char.IsDigit(styleValue[nextIndex]))
            {
                while (nextIndex < styleValue.Length &&
                       (char.IsDigit(styleValue[nextIndex]) || styleValue[nextIndex] == '.'))
                {
                    nextIndex++;
                }

                var number = styleValue.Substring(startIndex, nextIndex - startIndex);

                var unit = ParseWordEnumeration(_fontSizeUnits, styleValue, ref nextIndex);
                if (unit == null)
                {
                    unit = "px"; // Assuming pixels by default
                }

                if (mustBeNonNegative && styleValue[startIndex] == '-')
                {
                    return "0";
                }
                return number + unit;
            }

            return null;
        }

        private static void ParseCssSize(string styleValue, ref int nextIndex, Hashtable localValues,
            string propertyName, bool mustBeNonNegative)
        {
            var length = ParseCssSize(styleValue, ref nextIndex, mustBeNonNegative);
            if (length != null)
            {
                localValues[propertyName] = length;
            }
        }

        private static string ParseCssColor(string styleValue, ref int nextIndex)
        {
            //  Implement color parsing
            // rgb(100%,53.5%,10%)
            // rgb(255,91,26)
            // #FF5B1A
            // black | silver | gray | ... | aqua
            // transparent - for background-color
            ParseWhiteSpace(styleValue, ref nextIndex);

            string color = null;

            if (nextIndex < styleValue.Length)
            {
                var startIndex = nextIndex;
                var character = styleValue[nextIndex];

                if (character == '#')
                {
                    nextIndex++;
                    while (nextIndex < styleValue.Length)
                    {
                        character = char.ToUpper(styleValue[nextIndex]);
                        if (!('0' <= character && character <= '9' || 'A' <= character && character <= 'F'))
                        {
                            break;
                        }
                        nextIndex++;
                    }
                    if (nextIndex > startIndex + 1)
                    {
                        color = styleValue.Substring(startIndex, nextIndex - startIndex);
                    }
                }
                else if (styleValue.Substring(nextIndex, 3).ToLower() == "rbg")
                {
                    //  Implement real rgb() color parsing
                    while (nextIndex < styleValue.Length && styleValue[nextIndex] != ')')
                    {
                        nextIndex++;
                    }
                    if (nextIndex < styleValue.Length)
                    {
                        nextIndex++; // to skip ')'
                    }
                    color = "gray"; // return bogus color
                }
                else if (char.IsLetter(character))
                {
                    color = ParseWordEnumeration(_colors, styleValue, ref nextIndex);
                    if (color == null)
                    {
                        color = ParseWordEnumeration(_systemColors, styleValue, ref nextIndex);
                        if (color != null)
                        {
                            //  Implement smarter system color converions into real colors
                            color = "black";
                        }
                    }
                }
            }

            return color;
        }

        private static void ParseCssColor(string styleValue, ref int nextIndex, Hashtable localValues,
            string propertyName)
        {
            var color = ParseCssColor(styleValue, ref nextIndex);
            if (color != null)
            {
                localValues[propertyName] = color;
            }
        }

        // Parses CSS string fontStyle representing a value for css font attribute
        private static void ParseCssFont(string styleValue, Hashtable localProperties)
        {
            var nextIndex = 0;

            ParseCssFontStyle(styleValue, ref nextIndex, localProperties);
            ParseCssFontVariant(styleValue, ref nextIndex, localProperties);
            ParseCssFontWeight(styleValue, ref nextIndex, localProperties);

            ParseCssSize(styleValue, ref nextIndex, localProperties, "font-size", /*mustBeNonNegative:*/true);

            ParseWhiteSpace(styleValue, ref nextIndex);
            if (nextIndex < styleValue.Length && styleValue[nextIndex] == '/')
            {
                nextIndex++;
                ParseCssSize(styleValue, ref nextIndex, localProperties, "line-height", /*mustBeNonNegative:*/true);
            }

            ParseCssFontFamily(styleValue, ref nextIndex, localProperties);
        }

        private static void ParseCssFontStyle(string styleValue, ref int nextIndex, Hashtable localProperties)
        {
            ParseWordEnumeration(_fontStyles, styleValue, ref nextIndex, localProperties, "font-style");
        }

        private static void ParseCssFontVariant(string styleValue, ref int nextIndex, Hashtable localProperties)
        {
            ParseWordEnumeration(_fontVariants, styleValue, ref nextIndex, localProperties, "font-variant");
        }

        private static void ParseCssFontWeight(string styleValue, ref int nextIndex, Hashtable localProperties)
        {
            ParseWordEnumeration(_fontWeights, styleValue, ref nextIndex, localProperties, "font-weight");
        }

        private static void ParseCssFontFamily(string styleValue, ref int nextIndex, Hashtable localProperties)
        {
            string fontFamilyList = null;

            while (nextIndex < styleValue.Length)
            {
                // Try generic-family
                var fontFamily = ParseWordEnumeration(_fontGenericFamilies, styleValue, ref nextIndex);

                if (fontFamily == null)
                {
                    // Try quoted font family name
                    if (nextIndex < styleValue.Length && (styleValue[nextIndex] == '"' || styleValue[nextIndex] == '\''))
                    {
                        var quote = styleValue[nextIndex];

                        nextIndex++;

                        var startIndex = nextIndex;

                        while (nextIndex < styleValue.Length && styleValue[nextIndex] != quote)
                        {
                            nextIndex++;
                        }

                        fontFamily = '"' + styleValue.Substring(startIndex, nextIndex - startIndex) + '"';
                    }

                    if (fontFamily == null)
                    {
                        // Try unquoted font family name
                        var startIndex = nextIndex;
                        while (nextIndex < styleValue.Length && styleValue[nextIndex] != ',' &&
                               styleValue[nextIndex] != ';')
                        {
                            nextIndex++;
                        }

                        if (nextIndex > startIndex)
                        {
                            fontFamily = styleValue.Substring(startIndex, nextIndex - startIndex).Trim();
                            if (fontFamily.Length == 0)
                            {
                                fontFamily = null;
                            }
                        }
                    }
                }

                ParseWhiteSpace(styleValue, ref nextIndex);
                if (nextIndex < styleValue.Length && styleValue[nextIndex] == ',')
                {
                    nextIndex++;
                }

                if (fontFamily != null)
                {
                    //  css font-family can contein a list of names. We only consider the first name from the list. Need a decision what to do with remaining names
                    // fontFamilyList = (fontFamilyList == null) ? fontFamily : fontFamilyList + "," + fontFamily;
                    if (fontFamilyList == null && fontFamily.Length > 0)
                    {
                        if (fontFamily[0] == '"' || fontFamily[0] == '\'')
                        {
                            // Unquote the font family name
                            fontFamily = fontFamily.Substring(1, fontFamily.Length - 2);
                        }
                        fontFamilyList = fontFamily;
                    }
                }
                else
                {
                    break;
                }
            }

            if (fontFamilyList != null)
            {
                localProperties["font-family"] = fontFamilyList;
            }
        }

        private static void ParseCssListStyle(string styleValue, Hashtable localProperties)
        {
            var nextIndex = 0;

            while (nextIndex < styleValue.Length)
            {
                var listStyleType = ParseCssListStyleType(styleValue, ref nextIndex);
                if (listStyleType != null)
                {
                    localProperties["list-style-type"] = listStyleType;
                }
                else
                {
                    var listStylePosition = ParseCssListStylePosition(styleValue, ref nextIndex);
                    if (listStylePosition != null)
                    {
                        localProperties["list-style-position"] = listStylePosition;
                    }
                    else
                    {
                        var listStyleImage = ParseCssListStyleImage(styleValue, ref nextIndex);
                        if (listStyleImage != null)
                        {
                            localProperties["list-style-image"] = listStyleImage;
                        }
                        else
                        {
                            // TODO: Process unrecognized list style value
                            break;
                        }
                    }
                }
            }
        }

        private static string ParseCssListStyleType(string styleValue, ref int nextIndex)
        {
            return ParseWordEnumeration(_listStyleTypes, styleValue, ref nextIndex);
        }

        private static string ParseCssListStylePosition(string styleValue, ref int nextIndex)
        {
            return ParseWordEnumeration(_listStylePositions, styleValue, ref nextIndex);
        }

        private static string ParseCssListStyleImage(string styleValue, ref int nextIndex)
        {
            // TODO: Implement URL parsing for images
            return null;
        }

        private static void ParseCssTextDecoration(string styleValue, ref int nextIndex, Hashtable localProperties)
        {
            // Set default text-decorations:none;
            for (var i = 1; i < _textDecorations.Length; i++)
            {
                localProperties["text-decoration-" + _textDecorations[i]] = "false";
            }

            // Parse list of decorations values
            while (nextIndex < styleValue.Length)
            {
                var decoration = ParseWordEnumeration(_textDecorations, styleValue, ref nextIndex);
                if (decoration == null || decoration == "none")
                {
                    break;
                }
                localProperties["text-decoration-" + decoration] = "true";
            }
        }

        private static void ParseCssTextTransform(string styleValue, ref int nextIndex, Hashtable localProperties)
        {
            ParseWordEnumeration(_textTransforms, styleValue, ref nextIndex, localProperties, "text-transform");
        }

        private static void ParseCssTextAlign(string styleValue, ref int nextIndex, Hashtable localProperties)
        {
            ParseWordEnumeration(_textAligns, styleValue, ref nextIndex, localProperties, "text-align");
        }

        private static void ParseCssVerticalAlign(string styleValue, ref int nextIndex, Hashtable localProperties)
        {
            //  Parse percentage value for vertical-align style
            ParseWordEnumeration(_verticalAligns, styleValue, ref nextIndex, localProperties, "vertical-align");
        }

        private static void ParseCssFloat(string styleValue, ref int nextIndex, Hashtable localProperties)
        {
            ParseWordEnumeration(_floats, styleValue, ref nextIndex, localProperties, "float");
        }

        private static void ParseCssClear(string styleValue, ref int nextIndex, Hashtable localProperties)
        {
            ParseWordEnumeration(_clears, styleValue, ref nextIndex, localProperties, "clear");
        }

        // .................................................................
        //
        // Pasring CSS margin and padding Properties
        //
        // .................................................................

        // Generic method for parsing any of four-values properties, such as margin, padding, border-width, border-style, border-color
        private static bool ParseCssRectangleProperty(string styleValue, ref int nextIndex, Hashtable localProperties,
            string propertyName)
        {
            // CSS Spec: 
            // If only one value is set, then the value applies to all four sides;
            // If two or three values are set, then missinng value(s) are taken fromm the opposite side(s).
            // The order they are applied is: top/right/bottom/left

            Debug.Assert(propertyName == "margin" || propertyName == "padding" || propertyName == "border-width" ||
                         propertyName == "border-style" || propertyName == "border-color");

            var value = propertyName == "border-color"
                ? ParseCssColor(styleValue, ref nextIndex)
                : propertyName == "border-style"
                    ? ParseCssBorderStyle(styleValue, ref nextIndex)
                    : ParseCssSize(styleValue, ref nextIndex, /*mustBeNonNegative:*/true);
            if (value != null)
            {
                localProperties[propertyName + "-top"] = value;
                localProperties[propertyName + "-bottom"] = value;
                localProperties[propertyName + "-right"] = value;
                localProperties[propertyName + "-left"] = value;
                value = propertyName == "border-color"
                    ? ParseCssColor(styleValue, ref nextIndex)
                    : propertyName == "border-style"
                        ? ParseCssBorderStyle(styleValue, ref nextIndex)
                        : ParseCssSize(styleValue, ref nextIndex, /*mustBeNonNegative:*/true);
                if (value != null)
                {
                    localProperties[propertyName + "-right"] = value;
                    localProperties[propertyName + "-left"] = value;
                    value = propertyName == "border-color"
                        ? ParseCssColor(styleValue, ref nextIndex)
                        : propertyName == "border-style"
                            ? ParseCssBorderStyle(styleValue, ref nextIndex)
                            : ParseCssSize(styleValue, ref nextIndex, /*mustBeNonNegative:*/true);
                    if (value != null)
                    {
                        localProperties[propertyName + "-bottom"] = value;
                        value = propertyName == "border-color"
                            ? ParseCssColor(styleValue, ref nextIndex)
                            : propertyName == "border-style"
                                ? ParseCssBorderStyle(styleValue, ref nextIndex)
                                : ParseCssSize(styleValue, ref nextIndex, /*mustBeNonNegative:*/true);
                        if (value != null)
                        {
                            localProperties[propertyName + "-left"] = value;
                        }
                    }
                }

                return true;
            }

            return false;
        }

        // .................................................................
        //
        // Pasring CSS border Properties
        //
        // .................................................................

        // border: [ <border-width> || <border-style> || <border-color> ]

        private static void ParseCssBorder(string styleValue, ref int nextIndex, Hashtable localProperties)
        {
            while (
                ParseCssRectangleProperty(styleValue, ref nextIndex, localProperties, "border-width") ||
                ParseCssRectangleProperty(styleValue, ref nextIndex, localProperties, "border-style") ||
                ParseCssRectangleProperty(styleValue, ref nextIndex, localProperties, "border-color"))
            {
            }
        }

        private static string ParseCssBorderStyle(string styleValue, ref int nextIndex)
        {
            return ParseWordEnumeration(_borderStyles, styleValue, ref nextIndex);
        }

        // .................................................................
        //
        // Pasring CSS Background Properties
        //
        // .................................................................

        private static void ParseCssBackground(string styleValue, ref int nextIndex, Hashtable localValues)
        {
            //  Implement parsing background attribute
        }
    }


    internal class CssStylesheet
    {
        private List<StyleDefinition> _styleDefinitions;
        // Constructor
        public CssStylesheet(XmlElement htmlElement)
        {
            if (htmlElement != null)
            {
                DiscoverStyleDefinitions(htmlElement);
            }
        }

        // Recursively traverses an html tree, discovers STYLE elements and creates a style definition table
        // for further cascading style application
        public void DiscoverStyleDefinitions(XmlElement htmlElement)
        {
            if (htmlElement.LocalName.ToLower() == "link")
            {
                return;
                //  Add LINK elements processing for included stylesheets
                // <LINK href="http://sc.msn.com/global/css/ptnr/orange.css" type=text/css \r\nrel=stylesheet>
            }

            if (htmlElement.LocalName.ToLower() != "style")
            {
                // This is not a STYLE element. Recurse into it
                for (var htmlChildNode = htmlElement.FirstChild;
                    htmlChildNode != null;
                    htmlChildNode = htmlChildNode.NextSibling)
                {
                    if (htmlChildNode is XmlElement)
                    {
                        DiscoverStyleDefinitions((XmlElement) htmlChildNode);
                    }
                }
                return;
            }

            // Add style definitions from this style.

            // Collect all text from this style definition
            var stylesheetBuffer = new StringBuilder();

            for (var htmlChildNode = htmlElement.FirstChild;
                htmlChildNode != null;
                htmlChildNode = htmlChildNode.NextSibling)
            {
                if (htmlChildNode is XmlText || htmlChildNode is XmlComment)
                {
                    stylesheetBuffer.Append(RemoveComments(htmlChildNode.Value));
                }
            }

            // CssStylesheet has the following syntactical structure:
            //     @import declaration;
            //     selector { definition }
            // where "selector" is one of: ".classname", "tagname"
            // It can contain comments in the following form: /*...*/

            var nextCharacterIndex = 0;
            while (nextCharacterIndex < stylesheetBuffer.Length)
            {
                // Extract selector
                var selectorStart = nextCharacterIndex;
                while (nextCharacterIndex < stylesheetBuffer.Length && stylesheetBuffer[nextCharacterIndex] != '{')
                {
                    // Skip declaration directive starting from @
                    if (stylesheetBuffer[nextCharacterIndex] == '@')
                    {
                        while (nextCharacterIndex < stylesheetBuffer.Length &&
                               stylesheetBuffer[nextCharacterIndex] != ';')
                        {
                            nextCharacterIndex++;
                        }
                        selectorStart = nextCharacterIndex + 1;
                    }
                    nextCharacterIndex++;
                }

                if (nextCharacterIndex < stylesheetBuffer.Length)
                {
                    // Extract definition
                    var definitionStart = nextCharacterIndex;
                    while (nextCharacterIndex < stylesheetBuffer.Length && stylesheetBuffer[nextCharacterIndex] != '}')
                    {
                        nextCharacterIndex++;
                    }

                    // Define a style
                    if (nextCharacterIndex - definitionStart > 2)
                    {
                        AddStyleDefinition(
                            stylesheetBuffer.ToString(selectorStart, definitionStart - selectorStart),
                            stylesheetBuffer.ToString(definitionStart + 1, nextCharacterIndex - definitionStart - 2));
                    }

                    // Skip closing brace
                    if (nextCharacterIndex < stylesheetBuffer.Length)
                    {
                        Debug.Assert(stylesheetBuffer[nextCharacterIndex] == '}');
                        nextCharacterIndex++;
                    }
                }
            }
        }

        // Returns a string with all c-style comments replaced by spaces
        private string RemoveComments(string text)
        {
            var commentStart = text.IndexOf("/*");
            if (commentStart < 0)
            {
                return text;
            }

            var commentEnd = text.IndexOf("*/", commentStart + 2);
            if (commentEnd < 0)
            {
                return text.Substring(0, commentStart);
            }

            return text.Substring(0, commentStart) + " " + RemoveComments(text.Substring(commentEnd + 2));
        }

        public void AddStyleDefinition(string selector, string definition)
        {
            // Notrmalize parameter values
            selector = selector.Trim().ToLower();
            definition = definition.Trim().ToLower();
            if (selector.Length == 0 || definition.Length == 0)
            {
                return;
            }

            if (_styleDefinitions == null)
            {
                _styleDefinitions = new List<StyleDefinition>();
            }

            var simpleSelectors = selector.Split(',');

            for (var i = 0; i < simpleSelectors.Length; i++)
            {
                var simpleSelector = simpleSelectors[i].Trim();
                if (simpleSelector.Length > 0)
                {
                    _styleDefinitions.Add(new StyleDefinition(simpleSelector, definition));
                }
            }
        }

        public string GetStyle(string elementName, List<XmlElement> sourceContext)
        {
            Debug.Assert(sourceContext.Count > 0);
            Debug.Assert(elementName == sourceContext[sourceContext.Count - 1].LocalName);

            //  Add id processing for style selectors
            if (_styleDefinitions != null)
            {
                for (var i = _styleDefinitions.Count - 1; i >= 0; i--)
                {
                    var selector = _styleDefinitions[i].Selector;

                    var selectorLevels = selector.Split(' ');

                    var indexInSelector = selectorLevels.Length - 1;
                    var indexInContext = sourceContext.Count - 1;
                    var selectorLevel = selectorLevels[indexInSelector].Trim();

                    if (MatchSelectorLevel(selectorLevel, sourceContext[sourceContext.Count - 1]))
                    {
                        return _styleDefinitions[i].Definition;
                    }
                }
            }

            return null;
        }

        private bool MatchSelectorLevel(string selectorLevel, XmlElement xmlElement)
        {
            if (selectorLevel.Length == 0)
            {
                return false;
            }

            var indexOfDot = selectorLevel.IndexOf('.');
            var indexOfPound = selectorLevel.IndexOf('#');

            string selectorClass = null;
            string selectorId = null;
            string selectorTag = null;
            if (indexOfDot >= 0)
            {
                if (indexOfDot > 0)
                {
                    selectorTag = selectorLevel.Substring(0, indexOfDot);
                }
                selectorClass = selectorLevel.Substring(indexOfDot + 1);
            }
            else if (indexOfPound >= 0)
            {
                if (indexOfPound > 0)
                {
                    selectorTag = selectorLevel.Substring(0, indexOfPound);
                }
                selectorId = selectorLevel.Substring(indexOfPound + 1);
            }
            else
            {
                selectorTag = selectorLevel;
            }

            if (selectorTag != null && selectorTag != xmlElement.LocalName)
            {
                return false;
            }

            if (selectorId != null && HtmlToXamlConverter.GetAttribute(xmlElement, "id") != selectorId)
            {
                return false;
            }

            if (selectorClass != null && HtmlToXamlConverter.GetAttribute(xmlElement, "class") != selectorClass)
            {
                return false;
            }

            return true;
        }

        #region Nested type: StyleDefinition

        private class StyleDefinition
        {
            public readonly string Definition;
            public readonly string Selector;

            public StyleDefinition(string selector, string definition)
            {
                Selector = selector;
                Definition = definition;
            }
        }

        #endregion
    }
}