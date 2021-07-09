/*
LabVIEW NXG Theme Plugin
Author: Dataflow_G
Homepage: https://github.com/dataflowg/labview-nxg-theme-plugin

-----------------------------------

MIT License

Copyright (c) 2021 Dataflow_G

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
 */

using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using NationalInstruments.Core;
using NationalInstruments.SourceModel;

namespace NXGTheme
{
    public class ColorHelper
    {
        public static Color MultiplyColor(Color color, double multiplier, bool multiplyAlpha = false)
        {
            Color newColor = Color.FromRgb(Convert.ToByte(color.R * multiplier),
                                           Convert.ToByte(color.G * multiplier),
                                           Convert.ToByte(color.B * multiplier));
            if (multiplyAlpha)
            {
                newColor.A = Convert.ToByte(byte.MaxValue * multiplier);
            }
            return newColor;
        }

        public static Color MultiplyAlpha(Color color, double multiplier)
        {
            Color newColor = color;

            newColor.A = Convert.ToByte(newColor.A * multiplier);

            return newColor;
        }
    }

    // Dark mode colors and brushes, based mostly on Visual Studio's dark mode
    // A brush should be created for every UI and block diagram element.
    public class DarkTheme
    {
        // Each field name should be prefixed by f_
        // Methods don't need a prefix
        // Public fields need a corresponding public method and vice versa e.g. f_Brush and Brush
        // This means less work when updating opcode operands, replacing either existing fields or methods as needed

        // TODO: To aid in transpiler edits, may have a field and method of PlatformColor, PlatformBrush, Color, and Brush:
        // public static readonly PlatformColor f_DebugColor = PlatformColor.FromArgb(byte.MaxValue, 0, 0, 0);
        // public static PlatformColor DebugColor => f_DebugColor;
        // public static readonly Color _f_DebugColor = f_DebugColor.Color;
        // public static Color _DebugColor => _f_Debug.Color;
        //
        // public static readonly PlatformBrush f_DebugBrush = PlatformBrush.CreateSolidColorBrush(f_DebugColor);
        // public static PlatformBrush DebugBrush => f_DebugBrush;
        // public static readonly Brush _f_DebugBrush = f_DebugBrush.Brush;
        // public static Brush _DebugBrush => _f_DebugBrush;

        // Private color definitions. These should not be used outside this class, and are only for defining public brush definitions.
        // All colors are PlatformColor type.
        #region Private color definitions
        /// <summary>
        /// (field) Bright magenta, easy to spot for debugging #FFFF00FF (255,255,0,255)
        /// </summary>
        private static readonly PlatformColor f_Debug = PlatformColor.FromArgb(byte.MaxValue, byte.MaxValue, 0, byte.MaxValue);
        /// <summary>
        /// (field) Bright cyan, easy to spot for development #FF00FFFF (255,0,255,255)
        /// </summary>
        private static readonly PlatformColor f_Unassigned = PlatformColor.FromArgb(byte.MaxValue, 0, byte.MaxValue, byte.MaxValue);
        /// <summary>
        /// (field) Solid gray #FF1E1E1E (255,30,30,30)
        /// </summary>
        private static readonly PlatformColor f_Gray30 = PlatformColor.FromArgb(byte.MaxValue, 30, 30, 30);
        /// <summary>
        /// (field) Solid gray #FF222223 (255,34,34,35)
        /// </summary>
        private static readonly PlatformColor f_Gray34 = PlatformColor.FromArgb(byte.MaxValue, 34, 34, 35);
        /// <summary>
        /// Solid gray #FF252526 (255,37,37,38)
        /// </summary>
        private static readonly PlatformColor f_Gray37 = PlatformColor.FromArgb(byte.MaxValue, 37, 37, 38);
        /// <summary>
        /// Solid gray #FF2D2D30 (255,45,45,48)
        /// </summary>
        private static readonly PlatformColor f_Gray45 = PlatformColor.FromArgb(byte.MaxValue, 45, 45, 48);
        /// <summary>
        /// Solid gray #FF2E2E2E (255,46,46,46)
        /// </summary>
        private static readonly PlatformColor f_Gray46 = PlatformColor.FromArgb(byte.MaxValue, 46, 46, 46);
        /// <summary>
        /// Solid gray #FF3E3E42 (255,62,62,66)
        /// </summary>
        private static readonly PlatformColor f_Gray62 = PlatformColor.FromArgb(byte.MaxValue, 62, 62, 66);
        /// <summary>
        /// Solid gray #FF3F3F46 (255,63,63,70)
        /// </summary>
        private static readonly PlatformColor f_Gray63 = PlatformColor.FromArgb(byte.MaxValue, 63, 63, 70);
        /// <summary>
        /// Solid gray #FF686868 (255,104,104,104)
        /// </summary>
        private static readonly PlatformColor f_Gray104 = PlatformColor.FromArgb(byte.MaxValue, 104, 104, 104);
        /// <summary>
        /// Solid gray #FF717171 (255,113,113,113)
        /// </summary>
        private static readonly PlatformColor f_Gray113 = PlatformColor.FromArgb(byte.MaxValue, 113, 113, 113);
        /// <summary>
        /// Solid gray #FF727272 (255,114,114,114)
        /// </summary>
        private static readonly PlatformColor f_Gray114 = PlatformColor.FromArgb(byte.MaxValue, 114, 114, 114);
        /// <summary>
        /// Solid gray #FF999999 (255,153,153,153)
        /// </summary>
        private static readonly PlatformColor f_Gray153 = PlatformColor.FromArgb(byte.MaxValue, 153, 153, 153);
        /// <summary>
        /// Solid gray #FFDCDCDC (255,220,220,220)
        /// </summary>
        private static readonly PlatformColor f_Gray220 = PlatformColor.FromArgb(byte.MaxValue, 220, 220, 220);
        /// <summary>
        /// Solid gray #FFEFEBEF (255,239,235,239)
        /// </summary>
        private static readonly PlatformColor f_Gray239 = PlatformColor.FromArgb(byte.MaxValue, 239, 235, 239);
        /// <summary>
        /// Solid blue #FF1C97EA (255,28,151,234)
        /// </summary>
        private static readonly PlatformColor f_Blue1 = PlatformColor.FromArgb(byte.MaxValue, 28, 151, 234);
        /// <summary>
        /// Solid blue #FF007ACC (255,0,122,204)
        /// </summary>
        private static readonly PlatformColor f_Blue2 = PlatformColor.FromArgb(byte.MaxValue, 0, 122, 204);
        /// <summary>
        /// Solid green #FF41A439 (255,65,164,57)
        /// </summary>
        private static readonly PlatformColor f_Green1 = PlatformColor.FromArgb(byte.MaxValue, 65, 164, 57);

        // Solid colors with alpha. Primarily xxFFFFFF, where xx is alpha.
        /// <summary>
        /// Transparent white #05FFFFFF (5,255,255,255)
        /// </summary>
        private static readonly PlatformColor f_AlphaWhite = PlatformColor.FromArgb(0, byte.MaxValue, byte.MaxValue, byte.MaxValue);
        #endregion

        // Platform colors, one for each interface element.
        // All colors are PlatformColor type.
        #region Platform colors
        #region NXG predefined color names
        // NXG defined brushes and colors, used in DarkTheme.UpdateResourceDictionaries() to override existing resource dictionary.
        // Original colors defined in NationalInstruments.Design.DiagramStockPresentationProvider.OnApplicationResourcesLoaded()
        // These colors are used by xml resources. Frustratingly many xml resources don't refer to colors by name, and instead
        // use constant values (for example wire type colors). Presumably a result of the Adobe Illustrator script used to
        // generate the xml files.
        // TODO: Script to modify xml resources so colors are named where appropriate
        public static readonly PlatformColor PaletteColorBarColor = PlatformColor.FromArgb(byte.MaxValue, 122, 128, 126);
        /// <summary>
        /// Outer edge color of function primitives.
        /// </summary>
        public static readonly PlatformColor NodeBorderKey = PlatformColor.FromArgb(byte.MaxValue, 130, 118, 61);
        /// <summary>
        /// Inner edge color of function primitives. Mostly unused in newer versions of NXG.
        /// </summary>
        public static readonly PlatformColor NodeInnerBorder = PlatformColor.FromArgb(byte.MaxValue, byte.MaxValue, byte.MaxValue, 243);
        /// <summary>
        /// Background color of function primitives.
        /// </summary>
        public static readonly PlatformColor NodeBackground = PlatformColor.FromArgb(byte.MaxValue, byte.MaxValue, byte.MaxValue, 205);
        public static readonly PlatformColor NodeBackgroundAlt = PlatformColor.FromArgb(byte.MaxValue, byte.MaxValue, byte.MaxValue, 239);
        public static readonly PlatformBrush DiagramBackgroundKey = f_RootDiagramBackgroundBrush;
        /// <summary>
        /// Foreground graphic color of function primitives. Blends with background so grpahics are slightly tinted in color.
        /// </summary>
        public static readonly PlatformColor NodeForeground100 = PlatformColor.FromArgb(179, 0, 0, 0);
        public static readonly PlatformColor NodeForeground50 = PlatformColor.FromArgb(byte.MaxValue, 170, 157, 94);
        public static readonly PlatformColor GrayNodeOuterBorderKey = f_Gray114; // PlatformColor.FromArgb(byte.MaxValue, 114, 114, 114);
        public static readonly PlatformColor GrayNodeInnerBorderKey = f_Gray45; // PlatformColor.FromArgb(byte.MaxValue, 242, 242, 242);
        public static readonly PlatformColor GrayNodeFillSecondaryKey = PlatformColor.FromArgb(byte.MaxValue, 213, 213, 213);
        public static readonly PlatformColor GrayNodeFillKey = f_Gray45; // PlatformColor.FromArgb(byte.MaxValue, 229, 229, 229);
        public static readonly PlatformColor NodeWhite = PlatformColor.FromArgb(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
        public static readonly PlatformColor FloatAndDoubleNumericTypeColor = StockTypeAssets.FloatAndDoubleNumericTypeColor;
        public static readonly PlatformColor IntegerNumericTypeColor = StockTypeAssets.IntegerNumericTypeColor;
        public static readonly PlatformColor FixedPointNumericTypeColor = StockTypeAssets.FixedPointNumericTypeColor;
        public static readonly PlatformColor ErrorClusterTypeColor = StockTypeAssets.ErrorClusterTypeColor;
        public static readonly PlatformColor BuiltInClusterTypeColor = StockTypeAssets.BuiltInClusterTypeColor;
        public static readonly PlatformColor BooleanTypeColor = StockTypeAssets.BooleanTypeColor;
        public static readonly PlatformColor ReferenceAndPathTypeColor = StockTypeAssets.ReferenceAndPathTypeColor;
        public static readonly PlatformColor IONameControlTypeColor = StockTypeAssets.IONameControlTypeColor;
        public static readonly PlatformColor VariantTypeColor = StockTypeAssets.VariantTypeColor;
        public static readonly PlatformColor ClusterAndStringTypeColor = StockTypeAssets.ClusterAndStringTypeColor;
        public static readonly PlatformColor LabVIEWObjectTypeColor = StockTypeAssets.LabVIEWObjectTypeColor;
        #endregion

        #region New color definitions, not in NXG
        // Solid colors which apply to UI elements
        /// <summary>
        /// The background color of a comment node.
        /// </summary>
        /// <remarks>
        /// Replace #FFFFFFFF in NationalInstruments.MocCommon\NationalInstruments.MocCommon.Resources.Diagram.Nodes.CommentNodeNormal_96.xml to use this color.
        /// </remarks>
        public static readonly PlatformColor f_CommentNodeBackgroundColor = f_Gray30;
        /// <summary>
        /// The border color of a comment node.
        /// </summary>
        /// <remarks>
        /// Replace #FFB2B2B2 in NationalInstruments.MocCommon\NationalInstruments.MocCommon.Resources.Diagram.Nodes.CommentNodeNormal_96.xml to use this color.
        /// </remarks>
        public static readonly PlatformColor f_CommentNodeBorderColor = f_Gray113;
        /// <summary>
        /// Scrollbar background color.
        /// </summary>
        /// <remarks>
        /// Replace brush04 in NationalInstruments.PlatformFramework\Resources\scrollbarhorizontal_ide_9grid.xml and scrollbarvertical_ide_9grid.xml to use this color.
        /// Only replace brush04 in sections with key BackgroundImageHorizontal and BackgroundImageVertical
        /// </remarks>
        public static readonly PlatformColor f_ScrollbarBackgroundColor = f_Gray62;
        public static readonly PlatformBrush f_ScrollbarBackgroundBrush = PlatformBrush.CreateSolidColorBrush(f_ScrollbarBackgroundColor);
        public static PlatformBrush ScrollbarBackgroundBrush => f_ScrollbarBackgroundBrush;
        /// <summary>
        /// Scrollbar thumb color.
        /// </summary>
        /// <remarks>
        /// For use in NationalInstruments.PlatformFramework\Resources\scrollbarhorizontal_ide_9grid.xml and scrollbarvertical_ide_9grid.xml.
        /// </remarks>
        public static readonly PlatformColor f_ScrollbarThumbColor = f_Gray104;
        public static readonly PlatformColor f_ScrollbarThumbMouseOverColor = f_Gray153;
        public static readonly PlatformColor f_ScrollbarThumbMouseDownColor = f_Gray239;
        /// <summary>
        /// Scrollbar arrow color.
        /// </summary>
        /// <remarks>
        /// For use in NationalInstruments.PlatformFramework\Resources\scrollbarhorizontal_ide_9grid.xml and scrollbarvertical_ide_9grid.xml.
        /// </remarks>
        public static readonly PlatformColor f_ScrollbarArrowColor = f_Gray153;
        public static readonly PlatformColor f_ScrollbarArrowMouseOverColor = f_Blue1;
        public static readonly PlatformColor f_ScrollbarArrowMouseDownColor = f_Blue2;
        /// <summary>
        /// General text color.
        /// </summary>
        public static readonly PlatformColor f_TextColor = f_Gray220;
        /// <summary>
        /// Node border color.
        /// </summary>
        public static readonly PlatformColor f_NodeBorderColor = f_Unassigned;
        /// <summary>
        /// List node border color.
        /// </summary>
        public static readonly PlatformColor f_ListNodeBorderColor = f_NodeBorderColor;
        public static readonly PlatformBrush f_ListNodeBorderBrush = PlatformBrush.CreateSolidColorBrush(f_ListNodeBorderColor);
        /// <summary>
        /// List node header text color.
        /// </summary>
        public static readonly PlatformColor f_ListNodeHeaderTextColor = f_Gray30;
        public static readonly PlatformBrush f_ListNodeHeaderTextBrush = PlatformBrush.CreateSolidColorBrush(f_ListNodeHeaderTextColor);
        public static PlatformBrush ListNodeHeaderTextBrush => f_ListNodeHeaderTextBrush;
        /// <summary>
        /// List node background color.
        /// </summary>
        public static readonly PlatformColor f_ListNodeBackgroundColor = f_Unassigned;
        /// <summary>
        /// List node divider color.
        /// </summary>
        public static readonly PlatformColor f_ListNodeDividerColor = f_Unassigned;
        /// <summary>
        /// List node resize handle (lower right) color.
        /// </summary>
        public static readonly PlatformColor f_ListNodeResizeHandleColor = f_Unassigned;
        public static readonly PlatformBrush f_ListNodeResizeHandleBrush = PlatformBrush.CreateSolidColorBrush(f_ListNodeResizeHandleColor);

        public static readonly PlatformColor f_ShellToolBarBackgroundColor = f_Gray37;
        public static readonly PlatformBrush f_ShellToolBarBackgroundBrush = PlatformBrush.CreateSolidColorBrush(f_ShellToolBarBackgroundColor);
        public static PlatformBrush ShellToolBarBackgroundBrush => f_ShellToolBarBackgroundBrush;
        /// <summary>
        /// Structure border primary color (while, for, case). See also DarkTheme.GrayNodeOuterBorder.
        /// </summary>
        public static readonly PlatformColor f_StructureBorderPrimaryColor = f_Gray114;
        /// <summary>
        /// Structure border secondary color (while, for, case). See also DarkTheme.GrayNodeInnerBorder.
        /// </summary>
        public static readonly PlatformColor f_StructureBorderSecondaryColor = f_Gray45;

        public static readonly PlatformColor FloatAndDoubleComplexNumericTypeColor = ColorHelper.MultiplyColor(FloatAndDoubleNumericTypeColor, 0.5);
        public static readonly PlatformColor FixedPointComplexNumericTypeColor = ColorHelper.MultiplyColor(FixedPointNumericTypeColor, 0.5);
        /// <summary>
        /// Array wire inner color.
        /// </summary>
        public static readonly PlatformColor FloatAndDoubleNumericTypeArrayColor = ColorHelper.MultiplyColor(FloatAndDoubleNumericTypeColor, 0.4);
        public static readonly PlatformColor IntegerNumericTypeArrayColor = ColorHelper.MultiplyColor(IntegerNumericTypeColor, 0.4);
        public static readonly PlatformColor FixedPointNumericTypeArrayColor = ColorHelper.MultiplyColor(FixedPointNumericTypeColor, 0.4);
        public static readonly PlatformColor ErrorClusterTypeArrayColor = ColorHelper.MultiplyColor(ErrorClusterTypeColor, 0.4);
        public static readonly PlatformColor BuiltInClusterTypeArrayColor = ColorHelper.MultiplyColor(BuiltInClusterTypeColor, 0.4);
        public static readonly PlatformColor BooleanTypeArrayColor = ColorHelper.MultiplyColor(BooleanTypeColor, 0.4);
        public static readonly PlatformColor ReferenceAndPathTypeArrayColor = ColorHelper.MultiplyColor(ReferenceAndPathTypeColor, 0.4);
        public static readonly PlatformColor IONameControlTypeArrayColor = ColorHelper.MultiplyColor(IONameControlTypeColor, 0.4);
        public static readonly PlatformColor VariantTypeArrayColor = ColorHelper.MultiplyColor(VariantTypeColor, 0.4);
        public static readonly PlatformColor ClusterAndStringTypeArrayColor = ColorHelper.MultiplyColor(ClusterAndStringTypeColor, 0.4);
        public static readonly PlatformColor LabVIEWObjectTypeArrayColor = ColorHelper.MultiplyColor(LabVIEWObjectTypeColor, 0.4);
        #endregion
        #endregion

        // Redefine the brushes from PlatformBrushes. Specific uses of these brushes may be patched by UI specific brushes.
        #region NationalInstruments.Core.PlatformBrushes redefinitions
        /// <summary>
        /// Color for arrows in a sorted table (column sorting glyph and expander arrows). Was "NIGrayBrush" (#777B80).
        /// </summary>
        public static readonly PlatformBrush f_SortedTableArrow = PlatformBrush.CreateSolidColorBrush(f_Debug);
		/// <summary>
		/// Standard text color. Was "IdeTextColor" aka "NIBlack" (#2B3033)
		/// </summary>
		public static readonly PlatformBrush f_Text = PlatformBrush.CreateSolidColorBrush(f_TextColor);
		/// <summary>
		/// Highlight, like mouse over, brush. Was SMColors.Highlight (#FF4D5359).
		/// </summary>
		public static readonly PlatformBrush f_Highlight = PlatformBrush.CreateSolidColorBrush(f_Debug);
		/// <summary>
		/// White brush. Was SMColors.White (#FFFFFFFF).
		/// </summary>
		public static readonly PlatformBrush f_White = PlatformBrush.CreateSolidColorBrush(f_Debug);
		/// <summary>
		/// Transparent brush. SMColors.Transparent (#00FFFFFF).
		/// </summary>
		public static readonly PlatformBrush f_Transparent = PlatformBrush.CreateSolidColorBrush(f_Debug);
		/// <summary>
		/// Red brush. Was SMColors.Red (#FFFF0000)
		/// </summary>
		public static readonly PlatformBrush f_Red = PlatformBrush.CreateSolidColorBrush(f_Debug);
		/// <summary>
		/// NIButtonFill brush. Was "NIButtonFillBrush", which is a LinearGradientBrush (#E9EBEC, #D9DEE1)
		/// </summary>
		public static readonly PlatformBrush f_NIButtonFill = PlatformBrush.CreateSolidColorBrush(f_Debug);
		/// <summary>
		/// NIGrayCool92 brush. Was "NIGrayCool92Brush" (#E9EBEC)
		/// </summary>
		public static readonly PlatformBrush f_NIGrayCool92 = PlatformBrush.CreateSolidColorBrush(f_Debug);
		/// <summary>
		/// NIGrayNeutral68 brush.
		/// </summary>
		public static readonly PlatformBrush f_NIGrayNeutral68 = PlatformBrush.CreateSolidColorBrush(f_Debug);
		/// <summary>
		/// NIBlack brush
		/// </summary>
		public static readonly PlatformBrush f_NIBlack = PlatformBrush.CreateSolidColorBrush(f_Debug);
		/// <summary>
		/// NITrueWhite brush
		/// </summary>
		public static readonly PlatformBrush f_NITrueWhite = PlatformBrush.CreateSolidColorBrush(f_Debug);
		/// <summary>
		/// NIGrayCool36 brush
		/// </summary>
		public static readonly PlatformBrush f_NIGrayCool36 = PlatformBrush.CreateSolidColorBrush(f_Debug);
		/// <summary>
		/// NIGrayCool36 brush
		/// </summary>
		public static readonly PlatformBrush f_NIGray = PlatformBrush.CreateSolidColorBrush(f_Debug);
		/// <summary>
		/// NIWarning brush
		/// </summary>
		public static readonly PlatformBrush f_NIWarning = PlatformBrush.CreateSolidColorBrush(f_Debug);
		/// <summary>
		/// NIBlue brush
		/// </summary>
		public static readonly PlatformBrush f_NIBlue = PlatformBrush.CreateSolidColorBrush(f_Debug);
		/// <summary>
		/// NIHighlight brush
		/// </summary>
		public static readonly PlatformBrush f_NIHighlight = PlatformBrush.CreateSolidColorBrush(f_Debug);
		/// <summary>
		/// NIBlueMediumAccent brush
		/// </summary>
		public static readonly PlatformBrush f_NIBlueMediumAccent = PlatformBrush.CreateSolidColorBrush(f_Debug);
		/// <summary>
		/// NIBlueAccent brush
		/// </summary>
		public static readonly PlatformBrush f_NIBlueAccent = PlatformBrush.CreateSolidColorBrush(f_Debug);
		/// <summary>
		/// NIHighlightText brush
		/// </summary>
		public static readonly PlatformBrush f_NIHighlightText = PlatformBrush.CreateSolidColorBrush(f_Debug);
		/// <summary>
		/// NIHighlightSelected brush
		/// </summary>
		public static readonly PlatformBrush f_NIHighlightSelected = PlatformBrush.CreateSolidColorBrush(f_Debug);
		/// <summary>
		/// NITrueBlack brush
		/// </summary>
		public static readonly PlatformBrush f_NITrueBlack = PlatformBrush.CreateSolidColorBrush(f_Debug);
		/// <summary>
		/// NIMediumGray brush
		/// </summary>
		public static readonly PlatformBrush f_NIMediumGray = PlatformBrush.CreateSolidColorBrush(f_Debug);
		/// <summary>
		/// NIIconGray brush
		/// </summary>
		public static readonly PlatformBrush f_NIIconGray = PlatformBrush.CreateSolidColorBrush(f_Debug);
		/// <summary>
		/// NIDropDownGray brush
		/// </summary>
		public static readonly PlatformBrush f_NIDropDownGray = PlatformBrush.CreateSolidColorBrush(f_Debug);
		/// <summary>
		/// NIBackdropGray brush
		/// </summary>
		public static readonly PlatformBrush f_NIBackdropGray = PlatformBrush.CreateSolidColorBrush(f_Debug);
		/// <summary>
		/// NIGrayCool81 brush
		/// </summary>
		public static readonly PlatformBrush f_NIGrayCool81 = PlatformBrush.CreateSolidColorBrush(f_Debug);
		/// <summary>
		/// NIGrayCool88 brush
		/// </summary>
		public static readonly PlatformBrush f_NIGrayCool88 = PlatformBrush.CreateSolidColorBrush(f_Debug);
		/// <summary>
		/// NIGrayCool90 brush
		/// </summary>
		public static readonly PlatformBrush f_NIGrayCool90 = PlatformBrush.CreateSolidColorBrush(f_Debug);
		/// <summary>
		/// NIBlueGray brush
		/// </summary>
		public static readonly PlatformBrush f_NIBlueGray = PlatformBrush.CreateSolidColorBrush(f_Debug);
        /// <summary>
        /// NIBackground brush
        /// </summary>
        public static readonly PlatformBrush f_NIBackground = PlatformBrush.CreateSolidColorBrush(f_Debug);
        /// <summary>
        /// NIWhite brush
        /// </summary>
        public static readonly PlatformBrush f_NIWhite = PlatformBrush.CreateSolidColorBrush(f_Debug);
        /// <summary>
        /// NIGrayCool94 brush
        /// </summary>
        public static readonly PlatformBrush f_NIGrayCool94 = PlatformBrush.CreateSolidColorBrush(f_Debug);
        /// <summary>
        /// NIWhiteBlue brush
        /// </summary>
        public static readonly PlatformBrush f_NIWhiteBlue = PlatformBrush.CreateSolidColorBrush(f_Debug);
        /// <summary>
        /// NICoolWhite brush
        /// </summary>
        public static readonly PlatformBrush f_NICoolWhite = PlatformBrush.CreateSolidColorBrush(f_Gray37);
        /// <summary>
        /// NICoolBlue98 brush
        /// </summary>
        public static readonly PlatformBrush f_NICoolBlue98 = PlatformBrush.CreateSolidColorBrush(f_Debug);
        /// <summary>
        /// NIError brush
        /// </summary>
        public static readonly PlatformBrush f_NIError = PlatformBrush.CreateSolidColorBrush(f_Debug);
        /// <summary>
        /// NISuccess brush
        /// </summary>
        public static readonly PlatformBrush f_NISuccess = PlatformBrush.CreateSolidColorBrush(f_Debug);
        /// <summary>
        /// NIProbe brush
        /// </summary>
        public static readonly PlatformBrush f_NIProbe = PlatformBrush.CreateSolidColorBrush(f_Debug);
        /// <summary>
        /// NIGuidelineBrush.  Used for Guidelines on the Front Panel.
        /// </summary>
        public static readonly PlatformBrush f_NIGuideline = PlatformBrush.CreateSolidColorBrush(f_Debug);
        #endregion


        // Platform brushes, one for each interface element. All brushes have both field and method access.
        // All brushes are PlatformBrush type.
        #region Platform brushes
        /// <summary>
        /// Block diagram background brush.
        /// </summary>
        public static readonly PlatformBrush f_RootDiagramBackgroundBrush = PlatformBrush.CreateSolidColorBrush(f_Gray30);
        public static PlatformBrush RootDiagramBackgroundBrush => f_RootDiagramBackgroundBrush;
        /// <summary>
        /// Structure background brush. Nested structures will stack alpha values.
        /// </summary>
        public static readonly PlatformBrush f_NestedDiagramBackgroundBrush = PlatformBrush.CreateSolidColorBrush(PlatformColor.FromArgb(160, f_Gray37.Color.R, f_Gray37.Color.G, f_Gray37.Color.B));
        public static PlatformBrush NestedDiagramBackgroundBrush => f_NestedDiagramBackgroundBrush;
        /// <summary>
        /// Outer edge of wire brush (on block diagram root only).
        /// </summary>
        public static readonly PlatformBrush f_OuterWireBrush = f_RootDiagramBackgroundBrush;
        public static PlatformBrush OuterWireBrush => f_OuterWireBrush;
        /// <summary>
        /// Outer edge of wire brush (inside nested structure).
        /// </summary>
        public static readonly PlatformBrush f_NestedOuterWireBrush = PlatformBrush.CreateSolidColorBrush(f_Gray34);
        public static PlatformBrush NestedOuterWireBrush => f_NestedOuterWireBrush;
        /// <summary>
        /// Right hand rail background brush.
        /// </summary>
        public static readonly PlatformBrush f_ConfigurationPaneControlBackgroundBrush = PlatformBrush.CreateSolidColorBrush(f_Gray37);
        public static PlatformBrush ConfigurationPaneControlBackgroundBrush => f_ConfigurationPaneControlBackgroundBrush;
        /// <summary>
        /// Docked palette background brush.
        /// </summary>
        public static readonly PlatformBrush f_DockedPaletteControlBackgroundBrush = PlatformBrush.CreateSolidColorBrush(f_Gray37);
        public static PlatformBrush DockedPaletteControlBackgroundBrush => f_DockedPaletteControlBackgroundBrush;
        /// <summary>
        /// Comment node text brush.
        /// </summary>
        public static readonly PlatformBrush f_CommentNodeTextBrush = PlatformBrush.CreateSolidColorBrush(f_Green1);
        public static PlatformBrush CommentNodeTextBrush => f_CommentNodeTextBrush;
        /// <summary>
        /// UI splitter background brush.
        /// </summary>
        public static readonly PlatformBrush f_SplitterBackgroundBrush = PlatformBrush.CreateSolidColorBrush(f_Gray45);
        public static PlatformBrush SplitterBackgroundBrush => f_SplitterBackgroundBrush;
        /// <summary>
        /// Title bar background brush.
        /// </summary>
        public static readonly PlatformBrush f_WindowBorderBrush = PlatformBrush.CreateSolidColorBrush(f_Gray45);
        public static PlatformBrush WindowBorderBrush => f_WindowBorderBrush;
        /// <summary>
        /// Menu bar background brush.
        /// </summary>
        public static readonly PlatformBrush f_MenuBackgroundBrush = PlatformBrush.CreateSolidColorBrush(f_Gray45);
        public static PlatformBrush MenuBackgroundBrush => f_MenuBackgroundBrush;

        #region Stacked sequence structure user selector
        /// <summary>
        /// Case / disable diagram structure user selector button border brush.
        /// </summary>
        public static readonly PlatformBrush f_StackedStructureUserSelectorButtonBorderBrush = PlatformBrush.CreateSolidColorBrush(f_Gray153);
        public static readonly PlatformBrush f_StackedStructureUserSelectorButtonBorderMouseOverBrush = PlatformBrush.CreateSolidColorBrush(f_Unassigned);
        public static readonly PlatformBrush f_StackedStructureUserSelectorButtonBorderMouseDownBrush = PlatformBrush.CreateSolidColorBrush(f_Unassigned);
        public static PlatformBrush StackedStructureUserSelectorButtonBorderBrush => f_StackedStructureUserSelectorButtonBorderBrush;
        public static PlatformBrush StackedStructureUserSelectorButtonBorderMouseOverBrush => f_StackedStructureUserSelectorButtonBorderMouseOverBrush;
        public static PlatformBrush StackedStructureUserSelectorButtonBorderMouseDownBrush => f_StackedStructureUserSelectorButtonBorderMouseDownBrush;
        /// <summary>
        /// Case / disable diagram structure user selector button outer background brush.
        /// </summary>
        public static readonly PlatformBrush f_StackedStructureUserSelectorButtonBackgroundOuterBrush = PlatformBrush.CreateSolidColorBrush(f_Gray62);
        public static readonly PlatformBrush f_StackedStructureUserSelectorButtonBackgroundOuterMouseOverBrush = PlatformBrush.CreateSolidColorBrush(f_Unassigned);
        public static readonly PlatformBrush f_StackedStructureUserSelectorButtonBackgroundOuterMouseDownBrush = PlatformBrush.CreateSolidColorBrush(f_Unassigned);
        public static PlatformBrush StackedStructureUserSelectorButtonBackgroundOuterBrush => f_StackedStructureUserSelectorButtonBackgroundOuterBrush;
        public static PlatformBrush StackedStructureUserSelectorButtonBackgroundOuterMouseOverBrush => f_StackedStructureUserSelectorButtonBackgroundOuterMouseOverBrush;
        public static PlatformBrush StackedStructureUserSelectorButtonBackgroundOuterMouseDownBrush => f_StackedStructureUserSelectorButtonBackgroundOuterMouseDownBrush;
        /// <summary>
        /// Case / disable diagram structure user selector button background brush.
        /// </summary>
        public static readonly PlatformBrush f_StackedStructureUserSelectorButtonBackgroundInnerBrush = PlatformBrush.CreateSolidColorBrush(f_Gray62);
        public static readonly PlatformBrush f_StackedStructureUserSelectorButtonBackgroundInnerMouseOverBrush = PlatformBrush.CreateSolidColorBrush(f_Unassigned);
        public static readonly PlatformBrush f_StackedStructureUserSelectorButtonBackgroundInnerMouseDownBrush = PlatformBrush.CreateSolidColorBrush(f_Unassigned);
        public static PlatformBrush StackedStructureUserSelectorButtonBackgroundInnerBrush => f_StackedStructureUserSelectorButtonBackgroundInnerBrush;
        public static PlatformBrush StackedStructureUserSelectorButtonBackgroundInnerMouseOverBrush => f_StackedStructureUserSelectorButtonBackgroundInnerMouseOverBrush;
        public static PlatformBrush StackedStructureUserSelectorButtonBackgroundInnerMouseDownBrush => f_StackedStructureUserSelectorButtonBackgroundInnerMouseDownBrush;
        /// <summary>
        /// Case / disable diagram structure user selector button arrow brush.
        /// </summary>
        public static readonly PlatformBrush f_StackedStructureUserSelectorButtonArrowBrush = PlatformBrush.CreateSolidColorBrush(f_Gray153);
        public static readonly PlatformBrush f_StackedStructureUserSelectorButtonArrowMouseOverBrush = PlatformBrush.CreateSolidColorBrush(f_Unassigned);
        public static readonly PlatformBrush f_StackedStructureUserSelectorButtonArrowMouseDownBrush = PlatformBrush.CreateSolidColorBrush(f_Unassigned);
        public static PlatformBrush StackedStructureUserSelectorButtonArrowBrush => f_StackedStructureUserSelectorButtonArrowBrush;
        public static PlatformBrush StackedStructureUserSelectorButtonArrowMouseOverBrush => f_StackedStructureUserSelectorButtonArrowMouseOverBrush;
        public static PlatformBrush StackedStructureUserSelectorButtonArrowMouseDownBrush => f_StackedStructureUserSelectorButtonArrowMouseDownBrush;

        public static readonly PlatformBrush f_StackedStructureUserSelectorComboBoxOuterBorderBrush = PlatformBrush.CreateSolidColorBrush(f_Gray153);
        public static PlatformBrush StackedStructureUserSelectorComboBoxOuterBorderBrush => f_StackedStructureUserSelectorButtonArrowBrush;

        public static readonly PlatformBrush f_StackedStructureUserSelectorComboBoxInnerBorderBrush = PlatformBrush.CreateSolidColorBrush(f_Gray62);
        public static PlatformBrush StackedStructureUserSelectorComboBoxInnerBorderBrush => f_StackedStructureUserSelectorComboBoxInnerBorderBrush;

        public static readonly PlatformBrush f_StackedStructureUserSelectorComboBoxBackgroundBrush = PlatformBrush.CreateSolidColorBrush(f_Gray62);
        public static PlatformBrush StackedStructureUserSelectorComboBoxBackgroundBrush => f_StackedStructureUserSelectorComboBoxBackgroundBrush;

        public static readonly PlatformBrush f_StackedStructureUserSelectorComboBoxArrowBrush = PlatformBrush.CreateSolidColorBrush(f_Gray153);
        public static PlatformBrush StackedStructureUserSelectorComboBoxArrowBrush => f_StackedStructureUserSelectorComboBoxArrowBrush;
        #endregion

        #region Event structure user selector
        /// <summary>
        /// Case / disable diagram structure user selector button border brush.
        /// </summary>
        public static readonly PlatformBrush f_EventStructureUserSelectorButtonBorderBrush = PlatformBrush.CreateSolidColorBrush(f_Unassigned);
        public static readonly PlatformBrush f_EventStructureUserSelectorButtonBorderMouseOverBrush = PlatformBrush.CreateSolidColorBrush(f_Unassigned);
        public static PlatformBrush EventStructureUserSelectorButtonBorderBrush => f_EventStructureUserSelectorButtonBorderBrush;
        public static PlatformBrush EventStructureUserSelectorButtonBorderMouseOverBrush => f_EventStructureUserSelectorButtonBorderMouseOverBrush;
        /// <summary>
        /// Case / disable diagram structure user selector button outer background brush.
        /// </summary>
        public static readonly PlatformBrush f_EventStructureUserSelectorButtonBackgroundOuterBrush = PlatformBrush.CreateSolidColorBrush(f_Unassigned);
        public static readonly PlatformBrush f_EventStructureUserSelectorButtonBackgroundOuterMouseOverBrush = PlatformBrush.CreateSolidColorBrush(f_Unassigned);
        public static PlatformBrush EventStructureUserSelectorButtonBackgroundOuterBrush => f_EventStructureUserSelectorButtonBackgroundOuterBrush;
        public static PlatformBrush EventStructureUserSelectorButtonBackgroundOuterMouseOverBrush => f_EventStructureUserSelectorButtonBackgroundOuterMouseOverBrush;
        /// <summary>
        /// Case / disable diagram structure user selector button background brush.
        /// </summary>
        public static readonly PlatformBrush f_EventStructureUserSelectorButtonBackgroundInnerBrush = PlatformBrush.CreateSolidColorBrush(f_Unassigned);
        public static readonly PlatformBrush f_EventStructureUserSelectorButtonBackgroundInnerMouseOverBrush = PlatformBrush.CreateSolidColorBrush(f_Unassigned);
        public static PlatformBrush EventStructureUserSelectorButtonBackgroundInnerBrush => f_EventStructureUserSelectorButtonBackgroundInnerBrush;
        public static PlatformBrush EventStructureUserSelectorButtonBackgroundInnerMouseOverBrush => f_EventStructureUserSelectorButtonBackgroundInnerMouseOverBrush;
        /// <summary>
        /// Case / disable diagram structure user selector button arrow brush.
        /// </summary>
        public static readonly PlatformBrush f_EventStructureUserSelectorButtonArrowBrush = PlatformBrush.CreateSolidColorBrush(f_Unassigned);
        public static readonly PlatformBrush f_EventStructureUserSelectorButtonArrowMouseOverBrush = PlatformBrush.CreateSolidColorBrush(f_Unassigned);
        public static PlatformBrush EventStructureUserSelectorButtonArrowBrush => f_EventStructureUserSelectorButtonArrowBrush;
        public static PlatformBrush EventStructureUserSelectorButtonArrowMouseOverBrush => f_EventStructureUserSelectorButtonArrowMouseOverBrush;

        public static readonly PlatformBrush f_EventStructureUserSelectorComboBoxOuterBorderBrush = PlatformBrush.CreateSolidColorBrush(f_Unassigned);
        public static PlatformBrush EventStructureUserSelectorComboBoxOuterBorderBrush => f_EventStructureUserSelectorButtonArrowBrush;

        public static readonly PlatformBrush f_EventStructureUserSelectorComboBoxInnerBorderBrush = PlatformBrush.CreateSolidColorBrush(f_Unassigned);
        public static PlatformBrush EventStructureUserSelectorComboBoxInnerBorderBrush => f_EventStructureUserSelectorComboBoxInnerBorderBrush;

        public static readonly PlatformBrush f_EventStructureUserSelectorComboBoxBackgroundBrush = PlatformBrush.CreateSolidColorBrush(f_Unassigned);
        public static PlatformBrush EventStructureUserSelectorComboBoxBackgroundBrush => f_EventStructureUserSelectorComboBoxBackgroundBrush;
        #endregion

        /// <summary>
        /// General text brush.
        /// </summary>
        public static readonly PlatformBrush f_TextBrush = PlatformBrush.CreateSolidColorBrush(f_TextColor);
        public static PlatformBrush TextBrush => f_TextBrush;
        /// <summary>
        /// Diagram label text brush.
        /// </summary>
        public static readonly PlatformBrush f_DiagramLabelTextBrush = PlatformBrush.CreateSolidColorBrush(f_TextColor);
        public static PlatformBrush DiagramLabelTextBrush => f_DiagramLabelTextBrush;

        public static readonly PlatformBrush f_DocumentContainerBackgroundBrush = PlatformBrush.CreateSolidColorBrush(f_Gray37);
        public static PlatformBrush DocumentContainerBackgroundBrush = f_DocumentContainerBackgroundBrush;

        #endregion

        public static void UpdateResourceDictionaries()
        {
            Collection<ResourceDictionary> dictionaries = PlatformApplication.Current.GetResources().MergedDictionaries;

            // Override colors defined in NationalInstruments.Design.DiagramStockPresentationProvider.OnApplicationResourcesLoaded()
            // Would normally perform this in a Prefix override function, but this plugin DLL doesn't get loaded until after
            // the call to OnApplicationResourcesLoaded().
            ResourceDictionary applicationDictionary = new ResourceDictionary
            {
                // Override NXG colors / brushes
                { "PaletteColorBarColor", PaletteColorBarColor },
                { StockDiagramUIResources.NodeBorderKey, NodeBorderKey },
                { "NodeInnerBorder", NodeInnerBorder },
                { "NodeBackground", NodeBackground },
                { "NodeBackgroundAlt", NodeBackgroundAlt },
                { StockDiagramUIResources.DiagramBackgroundKey, DiagramBackgroundKey },
                { "NodeForeground100", NodeForeground100 },
                { "NodeForeground50", NodeForeground50 },
                { StockDiagramUIResources.GrayNodeOuterBorderKey, GrayNodeOuterBorderKey },
                { StockDiagramUIResources.GrayNodeInnerBorderKey, GrayNodeInnerBorderKey },
                { StockDiagramUIResources.GrayNodeFillSecondaryKey, GrayNodeFillSecondaryKey },
                { StockDiagramUIResources.GrayNodeFillKey, GrayNodeFillKey },
                { StockDiagramUIResources.NodeWhite, NodeWhite },
                { "FloatAndDoubleNumericTypeColor", FloatAndDoubleNumericTypeColor },
                { "IntegerNumericTypeColor", IntegerNumericTypeColor },
                { "FixedPointNumericTypeColor", FixedPointNumericTypeColor },
                { "ErrorClusterTypeColor", ErrorClusterTypeColor },
                { "BuiltInClusterTypeColor", BuiltInClusterTypeColor },
                { "BooleanTypeColor", BooleanTypeColor },
                { "ReferenceAndPathTypeColor", ReferenceAndPathTypeColor },
                { "IONameControlTypeColor", IONameControlTypeColor },
                { "VariantTypeColor", VariantTypeColor },
                { "ClusterAndStringTypeColor", ClusterAndStringTypeColor },
                { "LabVIEWObjectTypeColor", LabVIEWObjectTypeColor },
                // New colors / brushes
                { "CommentNodeBackgroundColor", f_CommentNodeBackgroundColor },
                { "CommentNodeBorderColor", f_CommentNodeBorderColor },
                { "ScrollbarBackgroundColor", f_ScrollbarBackgroundColor },
                { "ScrollbarThumbColor", f_ScrollbarThumbColor },
                { "ScrollbarThumbMouseOverColor", f_ScrollbarThumbMouseOverColor },
                { "ScrollbarThumbMouseDownColor", f_ScrollbarThumbMouseDownColor },
                { "ScrollbarArrowColor", f_ScrollbarArrowColor },
                { "ScrollbarArrowMouseOverColor", f_ScrollbarArrowMouseOverColor },
                { "ScrollbarArrowMouseDownColor", f_ScrollbarArrowMouseDownColor },
                { "StructureBorderPrimaryColor", f_StructureBorderPrimaryColor },
                { "StructureBorderSecondaryColor", f_StructureBorderSecondaryColor },

                { "FloatAndDoubleNumericTypeArrayColor", FloatAndDoubleNumericTypeArrayColor },
                { "IntegerNumericTypeArrayColor", IntegerNumericTypeArrayColor },
                { "FixedPointNumericTypeArrayColor", FixedPointNumericTypeArrayColor },
                { "ErrorClusterTypeArrayColor", ErrorClusterTypeArrayColor },
                { "BuiltInClusterTypeArrayColor", BuiltInClusterTypeArrayColor },
                { "BooleanTypeArrayColor", BooleanTypeArrayColor },
                { "ReferenceAndPathTypeArrayColor", ReferenceAndPathTypeArrayColor },
                { "IONameControlTypeArrayColor", IONameControlTypeArrayColor },
                { "VariantTypeArrayColor", VariantTypeArrayColor },
                { "ClusterAndStringTypeArrayColor", ClusterAndStringTypeArrayColor },
                { "LabVIEWObjectTypeArrayColor", LabVIEWObjectTypeArrayColor }
            };
            dictionaries.Add(applicationDictionary);
            /*
            // For xaml files extracted from assemblies, ensure every clr-namespace property has an assembly set:
            //     clr-namespace:NationalInstruments.Controls;assembly=NationalInstruments.Core 
            // Also ensure any merged dictionary definitions have an absolute resource path (pack:// prefix):
            //     pack://application:,,,/NationalInstruments.PlatformFramework;component/Themes/TextAndFonts.xaml
            // Replace x:Freeze="True" with po:Freeze="True"
            var path = string.Empty;
            Uri uri;

            // Structure background color (while, for, sequence, timed loop)
            path = Path.Combine(assemblyPath, @"NationalInstruments.PlatformFramework\sourcemodel\designer\stockdiagramuiresources.xaml");
            uri = new Uri(path);
            dictionaries.Add(new ResourceDictionary() { Source = uri });

            // FUSE colors
            path = Path.Combine(assemblyPath, @"NationalInstruments.Core\themes\colors.xaml");
            uri = new Uri(path);
            dictionaries.Add(new ResourceDictionary() { Source = uri });
            */
        }
    }
}
