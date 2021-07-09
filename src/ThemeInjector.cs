/*
LabVIEW NXG Theme Plugin
Author: Dataflow_G
Homepage: https://github.com/dataflowg/labview-nxg-theme-plugin

This plugin provides theme capabilities to LabVIEW NXG. It also allows for
vector graphics changes.

The general approach to color replacement is a combination of runtime
patching and xml resource file changes. The ultimate goal is to define a
color and brush for every UI element.

Performing high level brush replacements (making NICoolWhite a dark gray,
and NIText an off-white) does work, but has the unintended side effect
of changing poorly defined UI elements (e.g. black elements using NIText
rather than PlatforumBrushes.Black)

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
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using HarmonyLib;
using NationalInstruments.Controls.Shell;
using NationalInstruments.Core;
using NationalInstruments.Design;
using NationalInstruments.Shell;
using NationalInstruments.VI.Design;
using System.Reflection.Emit;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Controls;
using System.IO;
using NationalInstruments.MocCommon.Design;
using NationalInstruments.Controls;
using NationalInstruments.Controls.Dock;
using System.Windows.Controls.Primitives;
using NationalInstruments.Restricted.ProjectExplorer.Design;
using NationalInstruments.Restricted.Shell;

namespace NXGTheme
{
    [ExportPushCommandContent]
    public class ThemeInjector : PushCommandContent
    {
        static public string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        static ThemeInjector()
        {
            DarkTheme.UpdateResourceDictionaries();

            // When set true, will save a harmony.log.txt debug file to the desktop.
            // Useful for debugging transpiler patches.
            Harmony.DEBUG = false;
            var harmony = new Harmony("NXGTheme.ThemeInjector");
            harmony.PatchAll();
        }
    }

    class PatchHelper
    {
        public static IEnumerable<CodeInstruction> PlatformBrushOpCodes(Type className, string fieldName)
        {
            var codes = new List<CodeInstruction>();

            codes.Add(new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(className, fieldName)));
            codes.Add(new CodeInstruction(OpCodes.Ret));

            return codes.AsEnumerable();
        }

        /// <summary>
        /// Searches for and applies ScrollbarBackgroundBrush to the background of each ScrollBar of a ScrollViewer.
        /// </summary>
        /// <param name="dependency">The depedency object to begin the visual tree search from.</param>
        /// <param name="scrollViewer">The name of the scroll viewer in the visual tree.</param>
        /// <param name="verticalScrollBar">The name of the vertical scroll bar in the visual tree. Default is "PART_VerticalScrollBar".</param>
        /// <param name="horizontalScrollBar">The name of the horizontal scroll bar in the visual tree. Default is "PART_HorizontalScrollBar".</param>
        public static void SetScrollBarBackground(DependencyObject dependency, string scrollViewer, string verticalScrollBar = "PART_VerticalScrollBar", string horizontalScrollBar = "PART_HorizontalScrollBar")
        {
            var result = WPFTreeHelper.FindDescendant(dependency, scrollViewer);
            if (result == null)
            {
                return;
            }

            (result as ScrollViewer).ApplyTemplate();

            foreach (string sbName in new string[] { verticalScrollBar, horizontalScrollBar })
            {
                result = WPFTreeHelper.FindDescendant(dependency, sbName);
                if (result != null)
                {
                    ScrollBar sb = result as ScrollBar;
                    sb.Background = DarkTheme.ScrollbarBackgroundBrush;
                }
            }
        }
    }

    class WPFTreeHelper
    {
        // Taken from http://csharphelper.com/blog/2020/09/find-controls-by-name-in-wpf-with-c/
        public static DependencyObject FindDescendant(DependencyObject parent, string name)
        {
            // Don't bother with unnamed targets.
            if (name == string.Empty) return null;

            // See if this object has the target name.
            FrameworkElement element = parent as FrameworkElement;
            if ((element != null) && (element.Name == name)) return parent;

            // Recursively check the children.
            int num_children = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < num_children; i++)
            {
                // See if this child has the target name.
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                DependencyObject descendant = FindDescendant(child, name);
                if (descendant != null) return descendant;
            }

            // We didn't find a descendant with the target name.
            return null;
        }
    }

    #region WPF rendering
    [HarmonyPatch(typeof(DataflowDiagramControl), "Initialize")]
    class DataflowDiagramControl_Initialize
    {
        static void Postfix(ref DataflowDiagramControl __instance)
        {
            // Make edges jaggy - improves look of some fine details (shift register arrows) at expense of
            // non-pixel grid aligned vector paths (of which there are *many*)
            ////__instance.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
            //__instance.SnapsToDevicePixels = true;
            //__instance.UseLayoutRounding = false;
        }
    }
    #endregion

    #region General colors and brushes
    // Only seems to be used by DiagramLabelViewModel, which has been patched to set the foreground color too. Could potentially remove one of the patches.
    [HarmonyPatch(typeof(TextEditableTextBlock), MethodType.Constructor)]
    class TextEditableTextBlock_Constructor
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            for (int i = 0; i < codes.Count(); i++)
            {
                if (codes[i].opcode == OpCodes.Call && codes[i].operand.ToString() == "NationalInstruments.Core.PlatformBrush get_Text()")
                {
                    codes[i] = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DarkTheme), "get_" + nameof(DarkTheme.TextBrush)));
                }
            }

            return codes.AsEnumerable();
        }
    }
    #endregion

    #region Block diagram
    /// <summary>
    /// Block diagram background.
    /// </summary>
    [HarmonyPatch(typeof(DataflowDiagramControl), "OnApplyTemplate")]
    class DataflowDiagramControl_OnApplyTemplate
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count(); i++)
            {
                if (codes[i].opcode == OpCodes.Ldsfld)
                {
                    // Replaces: this.Diagram.Background = StockDiagramUIResources.RootDiagramBackgroundBrush;
                    codes[i] = new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(DarkTheme), nameof(DarkTheme.f_RootDiagramBackgroundBrush)));
                }
            }

            return codes.AsEnumerable();
        }
    }

    /// <summary>
    /// Front panel and block diagram scroll bars.
    /// </summary>
    [HarmonyPatch(typeof(DesignerEditControl), "OnApplyTemplate")]
    class DesignerEditControl_OnApplyTemplate
    {
        static void Postfix(ref DesignerEditControl __instance)
        {
            PatchHelper.SetScrollBarBackground(__instance, "PART_editCanvasScrollViewer");
        }
    }
    #endregion

    #region Docked palette
    /// <summary>
    /// Docked palette background.
    /// </summary>
    [HarmonyPatch(typeof(DockedPaletteControl), "OnApplyTemplate")]
    class DockedPaletteControl_OnApplyTemplate
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            // subtract 1 to get index of instruction before final return
            var insertIndex = codes.Count() - 1;
            var newCodes = new List<CodeInstruction>();

            // Adds this line to the end of the original function. Can't be called as a Postfix, as PaletteMainControl is private.
            // (this.PaletteMainControl as Border).Background = DarkTheme.DockedPaletteControlBackgroundBrush
            newCodes.Add(new CodeInstruction(OpCodes.Ldarg_0)); // (this.
            newCodes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DockedPaletteControl), "get_PaletteMainControl"))); // PaletteMainControl
            newCodes.Add(new CodeInstruction(OpCodes.Isinst, typeof(Border))); // as Border)
            newCodes.Add(new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(DarkTheme), nameof(DarkTheme.f_DockedPaletteControlBackgroundBrush))));  // DarkTheme.f_DockedPaletteControlBackgroundBrush
            newCodes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PlatformBrush), "op_Implicit", new Type[] { typeof(PlatformBrush) }))); // Implicit conversion from PlaformBrush to Brush
            newCodes.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Border), "set_Background", new Type[] { typeof(Brush) }))); // .Backgound = 

            codes.InsertRange(insertIndex, newCodes);
            return codes.AsEnumerable();
        }
    }

    /// <summary>
    /// Docked palette flyout item text
    /// </summary>
    [HarmonyPatch(typeof(PaletteItemButton), "OnApplyTemplate")]
    class PaletteItemButton_OnApplyTemplate
    {
        static void Postfix(ref PaletteItemButton __instance)
        {
            var result = WPFTreeHelper.FindDescendant(__instance, "PaletteItemLabel");

            if (result != null)
            {
                ((result as System.Windows.Controls.Label).Content as TextBlock).Foreground = DarkTheme.TextBrush;
            }
        }
    }
    #endregion

    #region Structure background (while, for, timed loops, case, event, ipe, disabled, sequence structures)
    /// <summary>
    /// All structure backgrounds (while, for, timed loops, case, event, ipe, disabled, sequence structures).
    /// </summary>
    // TODO: Adding new frame to FSS will be xaml style background (#A0FFFFFF). Need to set elsewhere.
    // BUG: Opening existing diagrams doesn't always set structure backgruond properly, as nested diagram views don't (yet) exist
    // TODO: Not sure if this is the best place to patch - an OnApplyTemplate override would be better
    [HarmonyPatch(typeof(StructureViewModel), "PostAddedToVisualTree")]
    class StructureViewModel_PostAddedToVisualTree
    {
        static void Postfix(ref StructureViewModel __instance)
        {
            foreach (ViewElementViewModel nestedDiagram in __instance.NestedDiagrams)
            {
                if (nestedDiagram.View != null)
                {
                    (nestedDiagram.View.Visual as Panel).Background = DarkTheme.NestedDiagramBackgroundBrush;
                }
            }
        }
    }

    /// <summary>
    /// Case structure background, possibly others.
    /// </summary>
    [HarmonyPatch(typeof(StackedStructureControl), "OnApplyTemplate")]
    class StackedStructureControl_OnApplyTemplate
    {
        static void Postfix(ref StackedStructureControl __instance)
        {
            ((__instance.DataContext as StructureEditor).NestedDiagram.View.Visual as Panel).Background = DarkTheme.NestedDiagramBackgroundBrush;

            var result = WPFTreeHelper.FindDescendant(__instance, "PART_SelectorText");
            if (result != null)
            {
                (result as TextBlock).Foreground = DarkTheme.TextBrush;
            }

            result = WPFTreeHelper.FindDescendant(__instance, "PART_UserSelector");
            if (result != null)
            {
                (result as DiagramGrid).Background = PlatformBrush.FromArgb(0, byte.MaxValue, byte.MaxValue, byte.MaxValue);
            }

            result = WPFTreeHelper.FindDescendant(__instance, "PART_arrow");
            if (result != null)
            {
                (result as System.Windows.Shapes.Path).Fill = DarkTheme.StackedStructureUserSelectorComboBoxArrowBrush;
            }
        }
    }

    /// <summary>
    /// Event structure background.
    /// </summary>
    [HarmonyPatch(typeof(EventStructureControl), "OnApplyTemplate")]
    class EventStructureControl_OnApplyTemplate
    {
        static void Postfix(ref EventStructureControl __instance)
        {
            ((__instance.DataContext as StructureEditor).NestedDiagram.View.Visual as Panel).Background = DarkTheme.NestedDiagramBackgroundBrush;

            var result = WPFTreeHelper.FindDescendant(__instance, "PART_SelectorText");
            if (result != null)
            {
                (result as TextBlock).Foreground = DarkTheme.TextBrush;
            }

            result = WPFTreeHelper.FindDescendant(__instance, "PART_UserSelector");
            if (result != null)
            {
                (result as DiagramGrid).Background = PlatformBrush.FromArgb(0, byte.MaxValue, byte.MaxValue, byte.MaxValue);
            }

            result = WPFTreeHelper.FindDescendant(__instance, "PART_arrow");
            if (result != null)
            {
                (result as System.Windows.Shapes.Path).Fill = DarkTheme.StackedStructureUserSelectorComboBoxArrowBrush;
            }
        }
    }

    /// <summary>
    /// Disable structure background.
    /// </summary>
    [HarmonyPatch(typeof(DisableStructureControl), "OnApplyTemplate")]
    class DisableStructureControl_OnApplyTemplate
    {
        static void Postfix(ref DisableStructureControl __instance)
        {
            ((__instance.DataContext as StructureEditor).NestedDiagram.View.Visual as Panel).Background = DarkTheme.NestedDiagramBackgroundBrush;

            var result = WPFTreeHelper.FindDescendant(__instance, "PART_SelectorText");
            if (result != null)
            {
                (result as TextBlock).Foreground = DarkTheme.TextBrush;
            }

            result = WPFTreeHelper.FindDescendant(__instance, "PART_UserSelector");
            if (result != null)
            {
                (result as DiagramGrid).Background = PlatformBrush.FromArgb(0, byte.MaxValue, byte.MaxValue, byte.MaxValue);
            }

            result = WPFTreeHelper.FindDescendant(__instance, "PART_arrow");
            if (result != null)
            {
                (result as System.Windows.Shapes.Path).Fill = DarkTheme.StackedStructureUserSelectorComboBoxArrowBrush;
            }
        }
    }
    #endregion

    #region Outer wire color
    /// <summary>
    /// Outer wire background color. Applies to both BD and wires inside nested structures.
    /// </summary>
    [HarmonyPatch(typeof(WireViewModel), "get_OuterWireRenderData")]
    class WireViewModel_get_OuterWireRenderData
    {
        public static SolidColorBrush NestedOuterWireBrush => DarkTheme.NestedOuterWireBrush.Brush as SolidColorBrush;

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count(); i++)
            {
                if (codes[i].opcode == OpCodes.Ldsfld)
                {
                    // Replaces: PlatformBrush brush = StockDiagramUIResources.RootDiagramBackgroundBrush;
                    codes[i] = new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(DarkTheme), nameof(DarkTheme.f_OuterWireBrush)));
                }
                else if (codes[i].opcode == OpCodes.Call && codes[i].operand.ToString() == "System.Windows.Media.SolidColorBrush get_White()")
                {
                    // Replaces: brush = Brushes.White;
                    codes[i] = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(WireViewModel_get_OuterWireRenderData), "get_" + nameof(WireViewModel_get_OuterWireRenderData.NestedOuterWireBrush)));
                }
            }

            return codes.AsEnumerable();
        }
    }
    #endregion

    #region Comment node colors
    /// <summary>
    /// Comment node text color. Note comment node background is set in xml file on disk.
    /// </summary>
    [HarmonyPatch(typeof(CommentViewModel), "get_TextBox")]
    class CommentViewModel_get_TextBox
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count(); i++)
            {
                if (codes[i].opcode == OpCodes.Call && codes[i].operand.ToString() == "NationalInstruments.Core.PlatformBrush get_Text()")
                {
                    // Replaces: this._textBox.TextBrush = PlatformBrushes.Text;
                    codes[i] = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DarkTheme), "get_" + nameof(DarkTheme.CommentNodeTextBrush)));
                }
            }

            return codes.AsEnumerable();
        }
    }
    #endregion

    #region Case Structure selector
    /// <summary>
    /// Case / disable structure selector buttons (left + right arrows).
    /// </summary>
    [HarmonyPatch(typeof(StackedStructureViewModel), "get_DefaultSelectorColorTable")]
    class StackedStructureViewModel_get_DefaultSelectorColorTable
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>();

            // Replaces entire function body with code below.
            /*
            SimpleColorResolver simpleColorResolver = new SimpleColorResolver();
            simpleColorResolver.AddBrush("Border", DarkTheme.StackedStructureUserSelectorButtonBorderBrush);
            simpleColorResolver.AddBrush("BackgroundOuter", DarkTheme.StackedStructureUserSelectorButtonBackgroundOuterBrush);
            simpleColorResolver.AddBrush("BackgroundInner", DarkTheme.StackedStructureUserSelectorButtonBackgroundInnerBrush);
            simpleColorResolver.AddBrush("Arrow", DarkTheme.StackedStructureUserSelectorButtonArrowBrush);
            return simpleColorResolver;
            */
            codes.Add(new CodeInstruction(OpCodes.Newobj, AccessTools.Constructor(typeof(SimpleColorResolver))));
            codes.Add(new CodeInstruction(OpCodes.Dup));
            codes.Add(new CodeInstruction(OpCodes.Ldstr, "Border"));
            codes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DarkTheme), "get_" + nameof(DarkTheme.StackedStructureUserSelectorButtonBorderBrush))));
            codes.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(SimpleColorResolver), "AddBrush")));
            codes.Add(new CodeInstruction(OpCodes.Dup));
            codes.Add(new CodeInstruction(OpCodes.Ldstr, "BackgroundOuter"));
            codes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DarkTheme), "get_" + nameof(DarkTheme.StackedStructureUserSelectorButtonBackgroundOuterBrush))));
            codes.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(SimpleColorResolver), "AddBrush")));
            codes.Add(new CodeInstruction(OpCodes.Dup));
            codes.Add(new CodeInstruction(OpCodes.Ldstr, "BackgroundInner"));
            codes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DarkTheme), "get_" + nameof(DarkTheme.StackedStructureUserSelectorButtonBackgroundInnerBrush))));
            codes.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(SimpleColorResolver), "AddBrush")));
            codes.Add(new CodeInstruction(OpCodes.Dup));
            codes.Add(new CodeInstruction(OpCodes.Ldstr, "Arrow"));
            codes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DarkTheme), "get_" + nameof(DarkTheme.StackedStructureUserSelectorButtonArrowBrush))));
            codes.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(SimpleColorResolver), "AddBrush")));
            codes.Add(new CodeInstruction(OpCodes.Ret));

            return codes.AsEnumerable();
        }
    }

    /// <summary>
    /// Case / disable structure selector buttons mouse over (left + right arrows).
    /// </summary>
    [HarmonyPatch(typeof(StackedStructureViewModel), "get_MouseOverSelectorColorTable")]
    class StackedStructureViewModel_get_MouseOverSelectorColorTable
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>();

            // Replaces entire function body with code below.
            /*
            SimpleColorResolver simpleColorResolver = new SimpleColorResolver();
            simpleColorResolver.AddBrush("Border", DarkTheme.StackedStructureUserSelectorButtonBorderMouseOverBrush);
            simpleColorResolver.AddBrush("BackgroundOuter", DarkTheme.StackedStructureUserSelectorButtonBackgroundOuterMouseOverBrush);
            simpleColorResolver.AddBrush("BackgroundInner", DarkTheme.StackedStructureUserSelectorButtonBackgroundInnerMouseOverBrush);
            simpleColorResolver.AddBrush("Arrow", this.SelectorTypeBrush);
            return simpleColorResolver;
            */
            codes.Add(new CodeInstruction(OpCodes.Newobj, AccessTools.Constructor(typeof(SimpleColorResolver))));
            codes.Add(new CodeInstruction(OpCodes.Dup));
            codes.Add(new CodeInstruction(OpCodes.Ldstr, "Border"));
            codes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DarkTheme), "get_" + nameof(DarkTheme.StackedStructureUserSelectorButtonBorderMouseOverBrush))));
            codes.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(SimpleColorResolver), "AddBrush")));
            codes.Add(new CodeInstruction(OpCodes.Dup));
            codes.Add(new CodeInstruction(OpCodes.Ldstr, "BackgroundOuter"));
            codes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DarkTheme), "get_" + nameof(DarkTheme.StackedStructureUserSelectorButtonBackgroundOuterMouseOverBrush))));
            codes.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(SimpleColorResolver), "AddBrush")));
            codes.Add(new CodeInstruction(OpCodes.Dup));
            codes.Add(new CodeInstruction(OpCodes.Ldstr, "BackgroundInner"));
            codes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DarkTheme), "get_" + nameof(DarkTheme.StackedStructureUserSelectorButtonBackgroundInnerMouseOverBrush))));
            codes.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(SimpleColorResolver), "AddBrush")));
            codes.Add(new CodeInstruction(OpCodes.Dup));
            codes.Add(new CodeInstruction(OpCodes.Ldstr, "Arrow"));
            codes.Add(new CodeInstruction(OpCodes.Ldarg_0));
            codes.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(StackedStructureViewModel), "get_SelectorTypeBrush")));
            codes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PlatformBrush), "op_Implicit", new Type[] { typeof(PlatformBrush) }))); // Implicit conversion from PlaformBrush to Brush
            codes.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(SimpleColorResolver), "AddBrush")));
            codes.Add(new CodeInstruction(OpCodes.Ret));

            return codes.AsEnumerable();
        }
    }

    /// <summary>
    /// Case / disable structure selector buttons mouse down (left + right arrows).
    /// </summary>
    [HarmonyPatch(typeof(StackedStructureViewModel), "get_MouseDownSelectorColorTable")]
    class StackedStructureViewModel_get_MouseDownSelectorColorTable
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>();

            // Replaces entire function body with code below.
            /*
            SimpleColorResolver simpleColorResolver = new SimpleColorResolver();
            simpleColorResolver.AddBrush("Border", StackedStructureViewModel_get_MouseDownSelectorColorTable.BorderBrush);
            simpleColorResolver.AddBrush("BackgroundOuter", StackedStructureViewModel_get_MouseDownSelectorColorTable.BackgroundOuterBrush);
            simpleColorResolver.AddBrush("BackgroundInner", StackedStructureViewModel_get_MouseDownSelectorColorTable.BackgroundInnerBrush);
            simpleColorResolver.AddBrush("Arrow", this.SelectorTypeBrush);
            return simpleColorResolver;
            */
            codes.Add(new CodeInstruction(OpCodes.Newobj, AccessTools.Constructor(typeof(SimpleColorResolver))));
            codes.Add(new CodeInstruction(OpCodes.Dup));
            codes.Add(new CodeInstruction(OpCodes.Ldstr, "Border"));
            codes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DarkTheme), "get_" + nameof(DarkTheme.StackedStructureUserSelectorButtonBorderMouseDownBrush))));
            codes.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(SimpleColorResolver), "AddBrush")));
            codes.Add(new CodeInstruction(OpCodes.Dup));
            codes.Add(new CodeInstruction(OpCodes.Ldstr, "BackgroundOuter"));
            codes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DarkTheme), "get_" + nameof(DarkTheme.StackedStructureUserSelectorButtonBackgroundOuterMouseDownBrush))));
            codes.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(SimpleColorResolver), "AddBrush")));
            codes.Add(new CodeInstruction(OpCodes.Dup));
            codes.Add(new CodeInstruction(OpCodes.Ldstr, "BackgroundInner"));
            codes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DarkTheme), "get_" + nameof(DarkTheme.StackedStructureUserSelectorButtonBackgroundInnerMouseDownBrush))));
            codes.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(SimpleColorResolver), "AddBrush")));
            codes.Add(new CodeInstruction(OpCodes.Dup));
            codes.Add(new CodeInstruction(OpCodes.Ldstr, "Arrow"));
            codes.Add(new CodeInstruction(OpCodes.Ldarg_0));
            codes.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(StackedStructureViewModel), "get_SelectorTypeBrush")));
            codes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PlatformBrush), "op_Implicit", new Type[] { typeof(PlatformBrush) }))); // Implicit conversion from PlaformBrush to Brush
            codes.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(SimpleColorResolver), "AddBrush")));
            codes.Add(new CodeInstruction(OpCodes.Ret));

            return codes.AsEnumerable();
        }
    }

    /// <summary>
    /// Case / disable structure combo box border.
    /// </summary>
    [HarmonyPatch(typeof(StackedStructureViewModel), "get_ComboBoxOuterBorderBrush")]
    class StackedStructureViewModel_get_ComboBoxOuterBorderBrush
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>();

            // return DarkTheme.StackedStructureUserSelectorComboBoxOuterBorderBrush;
            codes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DarkTheme), "get_" + nameof(DarkTheme.StackedStructureUserSelectorComboBoxOuterBorderBrush))));
            codes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PlatformBrush), "op_Implicit", new Type[] { typeof(PlatformBrush) }))); // Implicit conversion from PlaformBrush to Brush
            codes.Add(new CodeInstruction(OpCodes.Ret));

            return codes.AsEnumerable();
        }
    }

    /// <summary>
    /// Case / disable structure combo box inner border.
    /// </summary>
    [HarmonyPatch(typeof(StackedStructureViewModel), "get_ComboBoxInnerBorderBrush")]
    class StackedStructureViewModel_get_ComboBoxInnerBorderBrush
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>();

            // return DarkTheme.StackedStructureUserSelectorComboBoxInnerBorderBrush;
            codes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DarkTheme), "get_" + nameof(DarkTheme.StackedStructureUserSelectorComboBoxInnerBorderBrush))));
            codes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PlatformBrush), "op_Implicit", new Type[] { typeof(PlatformBrush) }))); // Implicit conversion from PlaformBrush to Brush
            codes.Add(new CodeInstruction(OpCodes.Ret));

            return codes.AsEnumerable();
        }
    }

    /// <summary>
    /// Case / disable structure combo box background.
    /// </summary>
    [HarmonyPatch(typeof(StackedStructureViewModel), "get_ComboBoxBackgroundBrush")]
    class StackedStructureViewModel_get_ComboBoxBackgroundBrush
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>();

            // return DarkTheme.StackedStructureUserSelectorComboBoxBackgroundBrush;
            codes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DarkTheme), "get_" + nameof(DarkTheme.StackedStructureUserSelectorComboBoxBackgroundBrush))));
            codes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PlatformBrush), "op_Implicit", new Type[] { typeof(PlatformBrush) }))); // Implicit conversion from PlaformBrush to Brush
            codes.Add(new CodeInstruction(OpCodes.Ret));

            return codes.AsEnumerable();
        }
    }
    #endregion

    #region Event Structure selector
    /// <summary>
    /// Event structure selector buttons (left + right arrows).
    /// </summary>
    [HarmonyPatch(typeof(EventStructureViewModel), "get_DefaultSelectorColorTable")]
    class EventStructureViewModel_get_DefaultSelectorColorTable
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>();

            // Replaces entire function body with code below.
            /*
            SimpleColorResolver simpleColorResolver = new SimpleColorResolver();
            simpleColorResolver.AddBrush("Border", StackedStructureViewModel_get_DefaultSelectorColorTable.BorderBrush);
            simpleColorResolver.AddBrush("BackgroundOuter", StackedStructureViewModel_get_DefaultSelectorColorTable.BackgroundOuterBrush);
            simpleColorResolver.AddBrush("BackgroundInner", StackedStructureViewModel_get_DefaultSelectorColorTable.BackgroundInnerBrush);
            simpleColorResolver.AddBrush("Arrow", StackedStructureViewModel_get_DefaultSelectorColorTable.ArrowBrush);
            return simpleColorResolver;
            */
            codes.Add(new CodeInstruction(OpCodes.Newobj, AccessTools.Constructor(typeof(SimpleColorResolver))));
            codes.Add(new CodeInstruction(OpCodes.Dup));
            codes.Add(new CodeInstruction(OpCodes.Ldstr, "Border"));
            codes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DarkTheme), "get_" + nameof(DarkTheme.EventStructureUserSelectorButtonBorderBrush))));
            codes.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(SimpleColorResolver), "AddBrush")));
            codes.Add(new CodeInstruction(OpCodes.Dup));
            codes.Add(new CodeInstruction(OpCodes.Ldstr, "BackgroundOuter"));
            codes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DarkTheme), "get_" + nameof(DarkTheme.EventStructureUserSelectorButtonBackgroundOuterBrush))));
            codes.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(SimpleColorResolver), "AddBrush")));
            codes.Add(new CodeInstruction(OpCodes.Dup));
            codes.Add(new CodeInstruction(OpCodes.Ldstr, "BackgroundInner"));
            codes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DarkTheme), "get_" + nameof(DarkTheme.EventStructureUserSelectorButtonBackgroundInnerBrush))));
            codes.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(SimpleColorResolver), "AddBrush")));
            codes.Add(new CodeInstruction(OpCodes.Dup));
            codes.Add(new CodeInstruction(OpCodes.Ldstr, "Arrow"));
            codes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DarkTheme), "get_" + nameof(DarkTheme.EventStructureUserSelectorButtonArrowBrush))));
            codes.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(SimpleColorResolver), "AddBrush")));
            codes.Add(new CodeInstruction(OpCodes.Ret));

            return codes.AsEnumerable();
        }
    }


    /// <summary>
    /// Event structure selector buttons mouse over (left + right arrows).
    /// </summary>
    [HarmonyPatch(typeof(EventStructureViewModel), "get_MouseOverSelectorColorTable")]
    class EventStructureViewModel_get_MouseOverSelectorColorTable
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>();

            // Replaces entire function body with code below.
            /*
            SimpleColorResolver simpleColorResolver = new SimpleColorResolver();
            simpleColorResolver.AddBrush("Border", StackedStructureViewModel_get_MouseOverSelectorColorTable.BorderBrush);
            simpleColorResolver.AddBrush("BackgroundOuter", StackedStructureViewModel_get_MouseOverSelectorColorTable.BackgroundOuterBrush);
            simpleColorResolver.AddBrush("BackgroundInner", StackedStructureViewModel_get_MouseOverSelectorColorTable.BackgroundInnerBrush);
            simpleColorResolver.AddBrush("Arrow", this.SelectorTypeBrush);
            return simpleColorResolver;
            */
            codes.Add(new CodeInstruction(OpCodes.Newobj, AccessTools.Constructor(typeof(SimpleColorResolver))));
            codes.Add(new CodeInstruction(OpCodes.Dup));
            codes.Add(new CodeInstruction(OpCodes.Ldstr, "Border"));
            codes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DarkTheme), "get_" + nameof(DarkTheme.EventStructureUserSelectorButtonBorderMouseOverBrush))));
            codes.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(SimpleColorResolver), "AddBrush")));
            codes.Add(new CodeInstruction(OpCodes.Dup));
            codes.Add(new CodeInstruction(OpCodes.Ldstr, "BackgroundOuter"));
            codes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DarkTheme), "get_" + nameof(DarkTheme.EventStructureUserSelectorButtonBackgroundOuterMouseOverBrush))));
            codes.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(SimpleColorResolver), "AddBrush")));
            codes.Add(new CodeInstruction(OpCodes.Dup));
            codes.Add(new CodeInstruction(OpCodes.Ldstr, "BackgroundInner"));
            codes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DarkTheme), "get_" + nameof(DarkTheme.EventStructureUserSelectorButtonBackgroundInnerMouseOverBrush))));
            codes.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(SimpleColorResolver), "AddBrush")));
            codes.Add(new CodeInstruction(OpCodes.Dup));
            codes.Add(new CodeInstruction(OpCodes.Ldstr, "Arrow"));
            codes.Add(new CodeInstruction(OpCodes.Ldarg_0));
            codes.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(StackedStructureViewModel), "get_SelectorTypeBrush"))); // Parent type
            codes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PlatformBrush), "op_Implicit", new Type[] { typeof(PlatformBrush) }))); // Implicit conversion from PlaformBrush to Brush
            codes.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(SimpleColorResolver), "AddBrush")));
            codes.Add(new CodeInstruction(OpCodes.Ret));

            return codes.AsEnumerable();
        }
    }

    /// <summary>
    /// Event structure combo box border.
    /// </summary>
    [HarmonyPatch(typeof(EventStructureViewModel), "get_ComboBoxOuterBorderBrush")]
    class EventStructureViewModel_get_ComboBoxOuterBorderBrush
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>();

            // return DarkTheme.EventStructureUserSelectorComboBoxOuterBorderBrush;
            codes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DarkTheme), "get_" + nameof(DarkTheme.EventStructureUserSelectorComboBoxOuterBorderBrush))));
            codes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PlatformBrush), "op_Implicit", new Type[] { typeof(PlatformBrush) }))); // Implicit conversion from PlaformBrush to Brush
            codes.Add(new CodeInstruction(OpCodes.Ret));

            return codes.AsEnumerable();
        }
    }

    /// <summary>
    /// Event structure combo box inner border.
    /// </summary>
    [HarmonyPatch(typeof(EventStructureViewModel), "get_ComboBoxInnerBorderBrush")]
    class EventStructureViewModel_get_ComboBoxInnerBorderBrush
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>();

            // return DarkTheme.EventStructureUserSelectorComboBoxInnerBorderBrush;
            codes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DarkTheme), "get_" + nameof(DarkTheme.EventStructureUserSelectorComboBoxInnerBorderBrush))));
            codes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PlatformBrush), "op_Implicit", new Type[] { typeof(PlatformBrush) }))); // Implicit conversion from PlaformBrush to Brush
            codes.Add(new CodeInstruction(OpCodes.Ret));

            return codes.AsEnumerable();
        }
    }

    /// <summary>
    /// Event structure combo box background.
    /// </summary>
    [HarmonyPatch(typeof(EventStructureViewModel), "get_ComboBoxBackgroundBrush")]
    class EventStructureViewModel_get_ComboBoxBackgroundBrush
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>();

            // return DarkTheme.EventStructureUserSelectorComboBoxBackgroundBrush;
            codes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DarkTheme), "get_" + nameof(DarkTheme.EventStructureUserSelectorComboBoxBackgroundBrush))));
            codes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PlatformBrush), "op_Implicit", new Type[] { typeof(PlatformBrush) }))); // Implicit conversion from PlaformBrush to Brush
            codes.Add(new CodeInstruction(OpCodes.Ret));

            return codes.AsEnumerable();
        }
    }
    #endregion

    #region Terminal colors
    /// <summary>
    /// Block diagram node label text color.
    /// </summary>
    [HarmonyPatch(typeof(DiagramLabelViewModel), "CreateVisualControl")]
    class DiagramLabelViewModel_CreateVisualControl
    {
        public static Brush DiagramLabelTextBrush => DarkTheme.DiagramLabelTextBrush.Brush;

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            var newCodes = new List<CodeInstruction>();

            // textEditableTextBlock.Foreground = DiagramLabelViewModel_CreateVisualControl.DiagramLabelTextBrush;
            newCodes.Add(new CodeInstruction(OpCodes.Ldloc_0));
            newCodes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DiagramLabelViewModel_CreateVisualControl), "get_" + nameof(DiagramLabelViewModel_CreateVisualControl.DiagramLabelTextBrush))));
            newCodes.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(TextBlock), "set_Foreground", new Type[] { typeof(Brush) })));

            for (int i = 0; i < codes.Count(); i++)
            {
                if (codes[i].opcode == OpCodes.Callvirt && codes[i].operand.ToString() == "Void set_SnapsToDevicePixels(Boolean)")
                {
                    codes.InsertRange(i + 1, newCodes);
                    break;
                }
            }

            return codes.AsEnumerable();
        }
    }
    #endregion

    #region Node list (property node, event list node, dni node)
    /// <summary>
    /// Node list divider color.
    /// </summary>
    [HarmonyPatch(typeof(NodeListViewControl), MethodType.Constructor)]
    class NodeListViewControl_set_DividerColor
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            int red = DarkTheme.f_ListNodeDividerColor.Color.R;
            int green = DarkTheme.f_ListNodeDividerColor.Color.G;
            int blue = DarkTheme.f_ListNodeDividerColor.Color.B;

            for (int i = 0; i < codes.Count(); i++)
            {
                if (codes[i].opcode == OpCodes.Newobj && codes[i].operand.ToString() == "Void .ctor(System.Windows.Media.Color)")
                {
                    // Note for ldc.i4 - the operand MUST be int32. Bytes do not work, and even typcasting a byte to an i32 didn't seem to work.
                    codes[i - 4] = new CodeInstruction(OpCodes.Ldc_I4, red);
                    codes[i - 3] = new CodeInstruction(OpCodes.Ldc_I4, green);
                    codes[i - 2] = new CodeInstruction(OpCodes.Ldc_I4, blue);
                    break;
                }
            }

            return codes.AsEnumerable();
        }
    }

    /// <summary>
    /// Node list border color.
    /// </summary>
    [HarmonyPatch(typeof(NodeListViewControl), "ResolveBorderBrush")]
    class NodeListViewControl_ResolveBorderBrush
    {
        /*static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>();

            codes.Add(new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(DarkTheme), nameof(DarkTheme.f_ListNodeBorderColor))));
            codes.Add(new CodeInstruction(OpCodes.Ret));

            return codes.AsEnumerable();
        }*/

        static void Postfix(ref Brush __result)
        {
            __result = DarkTheme.f_ListNodeBorderBrush.Brush;
        }
    }

    /// <summary>
    /// Node list resize handle color (vertical lines in lower right).
    /// </summary>
    [HarmonyPatch(typeof(NodeListViewControl), "get_ResizeHandleBrush")]
    class NodeListViewControl_get_ResizeHandleBrush
    {
        static void Postfix(ref Brush __result)
        {
            __result = DarkTheme.f_ListNodeResizeHandleBrush.Brush;
        }
    }

    /*
    [HarmonyPatch(typeof(NodeListViewControl), "RenderBorderAndBackground")]
    class NodeListViewControl_RenderBorderAndBackground
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            var newCodes = new List<CodeInstruction>();

            // textEditableTextBlock.Foreground = DiagramLabelViewModel_CreateVisualControl.DiagramLabelTextBrush;
            newCodes.Add(new CodeInstruction(OpCodes.Ldloc_0));
            newCodes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DiagramLabelViewModel_CreateVisualControl), "get_" + nameof(DiagramLabelViewModel_CreateVisualControl.DiagramLabelTextBrush))));
            newCodes.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(TextBlock), "set_Foreground", new Type[] { typeof(Brush) })));

            for (int i = 0; i < codes.Count(); i++)
            {
                if (codes[i].opcode == OpCodes.Callvirt && codes[i].operand.ToString() == "Void set_SnapsToDevicePixels(Boolean)")
                {
                    codes.InsertRange(i + 1, newCodes);
                    break;
                }
            }

            return codes.AsEnumerable();
        }
    }
    */

    /// <summary>
    /// List node header text color. Need to override it from DarkTheme.TextBrush, else it's unreadable.
    /// </summary>
    [HarmonyPatch(typeof(NodeListViewControl), "CreateHeaderFormattedText")]
    class NodeListViewControl_CreateHeaderFormattedText
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            // TODO: Replace PlatformBrushes.Text with dark text
            for (int i = 0; i < codes.Count(); i++)
            {
                if (codes[i].opcode == OpCodes.Call && codes[i].operand.ToString() == "NationalInstruments.Core.PlatformBrush get_Text()")
                {
                    codes[i] = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DarkTheme), "get_" + nameof(DarkTheme.ListNodeHeaderTextBrush)));
                }
            }

            return codes.AsEnumerable();
        }
    }

    #endregion

    #region Constant / terminal nodes
    [HarmonyPatch(typeof(PathSelector), "OnApplyTemplate")]
    class PathSelector_OnApplyTemplate
    {
        static void Postfix(ref PathSelector __instance)
        {
            var result = WPFTreeHelper.FindDescendant(__instance, "TextBackground");
            if (result != null)
            {
                (result as System.Windows.Shapes.Rectangle).Fill = PlatformBrush.CreateSolidColorBrush(DarkTheme.GrayNodeInnerBorderKey);
            }

            result = WPFTreeHelper.FindDescendant(__instance, "PART_PathInput");
            if (result != null)
            {
                (result as TextBox).Foreground = DarkTheme.TextBrush;
            }
        }
    }

    [HarmonyPatch(typeof(SelectorConstantControl), "OnApplyTemplate")]
    class SelectorConstantControl_OnApplyTemplate
    {
        static void Postfix(ref PathSelector __instance)
        {
            var result = WPFTreeHelper.FindDescendant(__instance, "_selectorComboBox");
            if (result != null)
            {
                (result as ComboBox).Foreground = DarkTheme.TextBrush;
            }
        }
    }
    #endregion

    // TODO: Perform file checks only once, and store result in dictionary / map.
    // Final loaded XML should be in an internal dictionary, but we're still doing the precheck every time.
    #region XML loading and redirects
    /*
    XML resources, controlling color and vector graphics.
    Implemented as Prefix, loading xml file from disk and skipping original. If xml not found, original function is called.

    public static NineGridDictionary LoadDictionary(Type type, string dictionaryName)
    {
        string text = dictionaryName;
        if (text.StartsWith("/", StringComparison.OrdinalIgnoreCase))
        {
            text = text.Substring(1);
        }
        string text2 = type.Namespace + "." + text.Replace('/', '.');
        Stream manifestResourceStream = type.Assembly.GetManifestResourceStream(text2);
        if (manifestResourceStream == null)
        {
            return NineGridParsing.LoadDictionary(ResourceHelpers.MakeResourceUri(type, dictionaryName));
        }
        NineGridDictionary nineGridDictionary = NineGridParsing.LoadDictionary(manifestResourceStream, text2);
        if (nineGridDictionary != null)
        {
            return nineGridDictionary.Clone();
        }
        return null;
    }
    */
    [HarmonyPatch(typeof(NineGridParsing), "LoadDictionary", new Type[] { typeof(Type), typeof(string) })]
    class NineGridParsing_LoadDictionary
    {
        // Check on disk if the XML file exists. If it does, load it instead of the DLL's resource XML.
        static bool Prefix(ref NineGridDictionary __result, Type type, string dictionaryName)
        {
            var text = dictionaryName;
            if (text.StartsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                text = text.Substring(1);
            }
            string text2 = type.Namespace + "." + text.Replace('/', '.');

            var path1 = Path.Combine(ThemeInjector.assemblyPath, type.Assembly.GetName().Name, text).Replace("/", "\\");
            var path2 = Path.Combine(ThemeInjector.assemblyPath, type.Assembly.GetName().Name, text2).Replace("/", "\\");

            if (File.Exists(path1))
            {
                __result = NineGridParsing.LoadDictionary(new Uri(path1));
                return false;
            }
            else if (File.Exists(path2))
            {
                __result = NineGridParsing.LoadDictionary(new Uri(path2));
                return false;
            }

            return true;
        }
    }

    /*
    XML resources, controlling color and vector graphics.
    Implemented as Prefix, replacing dictionary stream with a file stream to the xml file.
    */
    [HarmonyPatch(typeof(NineGridParsing), "LoadFromVectorDictionary", new Type[] { typeof(Stream), typeof(string), typeof(string) })]
    class NineGridParsing_LoadFromVectorDictionary
    {
        // Check on disk if the XML file exists. If it does, load it instead of the DLL's resource XML.
        static void Prefix(ref Stream dictionaryStream, string dictionaryName, string resourceName)
        {
            string extension = Path.GetExtension(dictionaryName);
            string[] dictionaryNameParts = Path.GetFileNameWithoutExtension(dictionaryName).Split(new char[] { '.' });
            string rootPath = ThemeInjector.assemblyPath;
            string resourcePath = dictionaryNameParts[0];
            string completePath = string.Empty;

            int i = 1;
            for (i = 1; i < dictionaryNameParts.Count(); i++)
            {
                resourcePath = resourcePath + "." + dictionaryNameParts[i];
                if (Directory.Exists(Path.Combine(rootPath, resourcePath)))
                {
                    break;
                }
            }

            // The dictionaryName input seems to be in form <assembly>.NationalInstruments.<resource>
            // Files on disk are stored in <assembly>\<resource>, and are in this format in other functions (see NineGridParsing_LoadDictionary)
            // Adding 2 here will skip the middle NationalInstruments segment
            i += 2;
            string[] namePartsRemainder = new string[dictionaryNameParts.Count() - i];
            Array.Copy(dictionaryNameParts, i, namePartsRemainder, 0, namePartsRemainder.Count());
            completePath = Path.Combine(rootPath, resourcePath, Path.Combine(namePartsRemainder)) + extension;

            if (File.Exists(completePath))
            {
                dictionaryStream = new FileStream(completePath, FileMode.Open, FileAccess.Read);
            }
        }
    }

    [HarmonyPatch(typeof(NineGridParsing), "LoadThemeColors", new Type[] { typeof(Uri) })]
    class NineGridParsing_LoadThemeColors
    {
        // Check on disk if the XML file exists. If it does, load it instead of the DLL's resource XML.
        static void Prefix(ref Uri location)
        {
            // Segments[0] is '/'
            // Segments[1] should be resource
            // Segments[2..] is path to xml
            string assembly = location.Segments[1].Split(new char[] { ',', ';' })[0];
            string[] resourceSegments = new string[location.Segments.Count() - 2];
            Array.Copy(location.Segments, 2, resourceSegments, 0, location.Segments.Count() - 2);
            string xmlPath = Path.Combine(ThemeInjector.assemblyPath, assembly, Path.Combine(resourceSegments)).Replace("/", "\\");

            if (File.Exists(xmlPath))
            {
                location = new Uri(xmlPath);
            }
        }
    }

    [HarmonyPatch(typeof(ControlDesign), "LoadControlDesignFromUri")]
    class ControlDesign_LoadControlDesignFromUri
    {
        // Check on disk if the XML file exists. If it does, load it instead of the DLL's resource XML.
        static void Prefix(ref Uri location)
        {
            // Segments[0] is '/'
            // Segments[1] should be resource
            // Segments[2..] is path to xml
            string assembly = location.Segments[1].Split(new char[] { ',', ';' })[0];
            string[] resourceSegments = new string[location.Segments.Count() - 2];
            Array.Copy(location.Segments, 2, resourceSegments, 0, location.Segments.Count() - 2);
            string xmlPath = Path.Combine(ThemeInjector.assemblyPath, assembly, Path.Combine(resourceSegments)).Replace("/", "\\");

            if (File.Exists(xmlPath))
            {
                location = new Uri(xmlPath);
            }
        }
    }
    #endregion

    #region User interface (side panels, etc) 
    /// <summary>
    /// Right hand rail configuration pane background and border.
    /// </summary>
    [HarmonyPatch(typeof(ConfigurationPaneControl), "Initialize")]
    class ConfigurationPaneControl_Initialize
    {
        static void Postfix(ref ConfigurationPaneControl __instance)
        {
            (__instance.Content as Grid).Background = DarkTheme.ConfigurationPaneControlBackgroundBrush;
            if ((__instance.Content as Grid).Children[0] != null)
            {
                ((__instance.Content as Grid).Children[0] as Border).Background = DarkTheme.ConfigurationPaneControlBackgroundBrush;
                ((__instance.Content as Grid).Children[0] as Border).BorderBrush = DarkTheme.ConfigurationPaneControlBackgroundBrush;
            }

            // TODO: This doesn't work, presumably because scroll bars don't exist yet (not created as part of template). Find somewhere else to execute this function, perhaps in a parent OnApplyTemplate()
            PatchHelper.SetScrollBarBackground(__instance.Content as DependencyObject, "_scrollViewer", "VerticalScrollBar", "HorizontalScrollBar");

            /*
            Grid contentGrid = __instance.Content as Grid;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(contentGrid); i++)
            {
                // Retrieve child visual at specified index value.
                Visual childVisual = (Visual)VisualTreeHelper.GetChild(contentGrid, i);

                childVisual.GetType().ToString();
            }

            ScrollViewer sv = (__instance.Content as Grid).Children[1] as ScrollViewer;
            ScrollBar sb = sv.Template.FindName("PART_VerticalScrollBar", sv) as ScrollBar;
            if (sb != null)
            {
                sb.Background = DarkTheme.ScrollbarBackgroundBrush;
            }
            sb = sv.Template.FindName("PART_HorizontalScrollBar", sv) as ScrollBar;
            if (sb != null)
            {
                sb.Background = DarkTheme.ScrollbarBackgroundBrush;
            }
            */
        }
    }

    /// <summary>
    /// Document tab area background (to right of home icon).
    /// </summary>
    [HarmonyPatch(typeof(DocumentContainer), "OnApplyTemplate")]
    class DocumentContainer_OnApplyTemplate
    {
        static void Postfix(ref DocumentContainer __instance)
        {
            __instance.Background = DarkTheme.DocumentContainerBackgroundBrush;

            // TODO: This seems to bring the item to the top, obscuring the document tabs.
            var result = WPFTreeHelper.FindDescendant(__instance, "PART_PermaTab");
            if (result != null)
            {
                // (result as Border).Background = DarkTheme.ConfigurationPaneControlBackgroundBrush;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [HarmonyPatch(typeof(ShellExpander), "OnApplyTemplate")]
    class ShellExpander_OnApplyTemplate
    {
        static void Postfix(ref ShellExpander __instance)
        {
            __instance.Background = DarkTheme.ConfigurationPaneControlBackgroundBrush;
            __instance.Foreground = DarkTheme.TextBrush;
            __instance.BorderBrush = DarkTheme.ConfigurationPaneControlBackgroundBrush;
        }
    }

    /// <summary>
    /// Items in project tree, used to set background color.
    /// </summary>
    [HarmonyPatch(typeof(ProjectTreeViewItem), "OnApplyTemplate")]
    class ProjectTreeViewItem_OnApplyTemplate
    {
        static void Postfix(ref ProjectTreeViewItem __instance)
        {
            __instance.Background = DarkTheme.DocumentContainerBackgroundBrush;
        }
    }

    /// <summary>
    /// Items in project tree, used to set text color.
    /// </summary>
    [HarmonyPatch(typeof(ProjectItemControl), "OnApplyTemplate")]
    class ProjectItemControl_OnApplyTemplate
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            // subtract 1 to get index of instruction before final return
            var insertIndex = codes.Count() - 1;
            var newCodes = new List<CodeInstruction>();

            // Add this line to the end of the original function. Can't be called as a Postfix, as PaletteMainControl is private.
            // (this.PaletteMainControl as Border).Background = DarkTheme.DockedPaletteControlBackgroundBrush
            newCodes.Add(new CodeInstruction(OpCodes.Ldarg_0)); // (this.
            newCodes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ProjectItemControl), "get_TextBlock"))); // PaletteMainControl
            newCodes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DarkTheme), "get_" + nameof(DarkTheme.TextBrush)))); // Implicit conversion from PlaformBrush to Brush
            newCodes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PlatformBrush), "op_Implicit", new Type[] { typeof(PlatformBrush) }))); // Implicit conversion from PlaformBrush to Brush
            newCodes.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(TextBlock), "set_Foreground", new Type[] { typeof(Brush) }))); // .Backgound = 

            codes.InsertRange(insertIndex, newCodes);
            return codes.AsEnumerable();
        }
    }

    /// <summary>
    /// Rectangle background fill of docking container. Effectively sets splitter colors.
    /// </summary>
    // TODO: The plugin isn't loaded in time for this patch to take effect. Dragging a VI out to create a new NXG window does work.
    [HarmonyPatch(typeof(DockSite), "a", new Type[] { typeof(DependencyObject), typeof(DependencyPropertyChangedEventArgs) })]
    class DockSite_a
    {
        public object SplitterBackground => DarkTheme.f_SplitterBackgroundBrush.Brush as SolidColorBrush;

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            for (int i = 0; i < codes.Count(); i++)
            {
                if (codes[i].opcode == OpCodes.Call && codes[i].operand.ToString() == "System.Object get_NewValue()")
                {
                    codes[i] = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DockSite_a), "get_" + nameof(DockSite_a.SplitterBackground)));
                }
            }

            return codes.AsEnumerable();
        }
    }

    /// <summary>
    /// Tool bars above document pane, diagram editor pane (run arrow, etc).
    /// </summary>
    [HarmonyPatch(typeof(ShellToolBar), "OnApplyTemplate")]
    class ShellToolBar_OnApplyTemplate
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            // subtract 1 to get index of instruction before final return
            var insertIndex = codes.Count() - 1;
            var newCodes = new List<CodeInstruction>();

            // this.Background = DarkTheme.ShellToolBarBackgroundBrush
            newCodes.Add(new CodeInstruction(OpCodes.Ldarg_0)); // (this.
            newCodes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DarkTheme), "get_" + nameof(DarkTheme.ShellToolBarBackgroundBrush))));
            newCodes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PlatformBrush), "op_Implicit", new Type[] { typeof(PlatformBrush) })));
            newCodes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Control), "set_Background", new Type[] { typeof(Brush) })));

            codes.InsertRange(insertIndex, newCodes);
            return codes.AsEnumerable();
        }
    }

    /// <summary>
    /// Tabs on top of left and right rails.
    /// </summary>
    [HarmonyPatch(typeof(WindowGroup), "OnApplyTemplate")]
    class WindowGroup_OnApplyTemplate
    {
        static void Postfix(ref WindowGroup __instance)
        {
            // Applies to tabs on both left and right panes
            var result = WPFTreeHelper.FindDescendant(__instance, "PART_TitleTabStrip");
            if (result != null)
            {
                (result as Border).Background = DarkTheme.ConfigurationPaneControlBackgroundBrush;
            }
        }
    }

    /// <summary>
    /// Docked palette background, docked palette flyout background.
    /// </summary>
    [HarmonyPatch(typeof(PaletteGridView), "OnApplyTemplate")]
    class PaletteGridView_OnApplyTemplate
    {
        static void Postfix(ref DocumentContainer __instance)
        {
            __instance.Background = DarkTheme.DockedPaletteControlBackgroundBrush;

            var result = WPFTreeHelper.FindDescendant(__instance, "PART_gridViewFrame");
            if (result != null)
            {
                (result as System.Windows.Shapes.Polygon).Fill = DarkTheme.DockedPaletteControlBackgroundBrush;
            }
        }
    }

    /// <summary>
    /// Main window title bar.
    /// </summary>
    [HarmonyPatch(typeof(StudioWindowWindow), MethodType.Constructor, new Type[] { typeof(StudioWindow), typeof(bool) })]
    class StudioWindowWindow_Constructor
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            // subtract 1 to get index of instruction before final return
            var insertIndex = codes.Count() - 1;
            var newCodes = new List<CodeInstruction>();

            // this.BorderBrush = DarkTheme.WindowBorderBrush
            newCodes.Add(new CodeInstruction(OpCodes.Ldarg_0));
            newCodes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DarkTheme), "get_" + nameof(DarkTheme.WindowBorderBrush))));
            newCodes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PlatformBrush), "op_Implicit", new Type[] { typeof(PlatformBrush) })));
            newCodes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Control), "set_BorderBrush", new Type[] { typeof(Brush) })));

            codes.InsertRange(insertIndex, newCodes);
            return codes.AsEnumerable();
        }
    }

    /// <summary>
    /// Main window border, menu bar.
    /// </summary>
    [HarmonyPatch(typeof(StudioWindowWindow), "OnActivated")]
    class StudioWindowWindow_OnActivated
    {
        static void Postfix(ref StudioWindowWindow __instance)
        {
            var result = WPFTreeHelper.FindDescendant(__instance, "outerBorder");
            if (result != null)
            {
                (result as Border).Background = DarkTheme.WindowBorderBrush;
            }

            result = WPFTreeHelper.FindDescendant(__instance, "_applicationMenu");
            if (result != null)
            {
                (result as ApplicationMenu).Background = DarkTheme.MenuBackgroundBrush;
            }
        }
    }

    /// <summary>
    /// Menu item text color. Includes both top level and sub menu items.
    /// </summary>
    [HarmonyPatch(typeof(ApplicationMenuItem), MethodType.Constructor)]
    class ApplicationMenuItem_Constructor
    {
        static void Postfix(ref ApplicationMenuItem __instance)
        {
            __instance.Foreground = DarkTheme.TextBrush;
        }
    }

    // TODO: No effect. Call to ApplyTemplate on child node required?
    [HarmonyPatch(typeof(RootWindowContent), "OnApplyTemplate")]
    class RootWindowContent_OnApplyTemplate
    {
        static void Postfix(ref RootWindowContent __instance)
        {
            var result = WPFTreeHelper.FindDescendant(__instance, "_launcherAnimationBackdrop");
            if (result != null)
            {
                (result as Border).BorderBrush = DarkTheme.WindowBorderBrush;
            }
        }
    }
    #endregion

    // Replace all brushes in PlatformBrushes
    #region NationalInstruments.Core.PlatformBrushes redefinitions
    [HarmonyPatch(typeof(PlatformBrushes), nameof(PlatformBrushes.SortedTableArrow), MethodType.Getter)]
    class PlatformBrushes_SortedTableArrow
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => PatchHelper.PlatformBrushOpCodes(typeof(DarkTheme), nameof(DarkTheme.f_SortedTableArrow));
    }

    [HarmonyPatch(typeof(PlatformBrushes), nameof(PlatformBrushes.Text), MethodType.Getter)]
    class PlatformBrushes_Text
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => PatchHelper.PlatformBrushOpCodes(typeof(DarkTheme), nameof(DarkTheme.f_Text));
    }

    [HarmonyPatch(typeof(PlatformBrushes), nameof(PlatformBrushes.Highlight), MethodType.Getter)]
    class PlatformBrushes_Highlight
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => PatchHelper.PlatformBrushOpCodes(typeof(DarkTheme), nameof(DarkTheme.f_Highlight));
    }

    [HarmonyPatch(typeof(PlatformBrushes), nameof(PlatformBrushes.White), MethodType.Getter)]
    class PlatformBrushes_White
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => PatchHelper.PlatformBrushOpCodes(typeof(DarkTheme), nameof(DarkTheme.f_White));
    }

    [HarmonyPatch(typeof(PlatformBrushes), nameof(PlatformBrushes.Transparent), MethodType.Getter)]
    class PlatformBrushes_Transparent
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => PatchHelper.PlatformBrushOpCodes(typeof(DarkTheme), nameof(DarkTheme.f_Transparent));
    }

    [HarmonyPatch(typeof(PlatformBrushes), nameof(PlatformBrushes.Red), MethodType.Getter)]
    class PlatformBrushes_Red
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => PatchHelper.PlatformBrushOpCodes(typeof(DarkTheme), nameof(DarkTheme.f_Red));
    }

    [HarmonyPatch(typeof(PlatformBrushes), nameof(PlatformBrushes.NIButtonFill), MethodType.Getter)]
    class PlatformBrushes_NIButtonFill
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => PatchHelper.PlatformBrushOpCodes(typeof(DarkTheme), nameof(DarkTheme.f_NIButtonFill));
    }

    [HarmonyPatch(typeof(PlatformBrushes), nameof(PlatformBrushes.NIGrayCool92), MethodType.Getter)]
    class PlatformBrushes_NIGrayCool92
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => PatchHelper.PlatformBrushOpCodes(typeof(DarkTheme), nameof(DarkTheme.f_NIGrayCool92));
    }

    [HarmonyPatch(typeof(PlatformBrushes), nameof(PlatformBrushes.NIGrayNeutral68), MethodType.Getter)]
    class PlatformBrushes_NIGrayNeutral68
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => PatchHelper.PlatformBrushOpCodes(typeof(DarkTheme), nameof(DarkTheme.f_NIGrayNeutral68));
    }

    [HarmonyPatch(typeof(PlatformBrushes), nameof(PlatformBrushes.NIBlack), MethodType.Getter)]
    class PlatformBrushes_NIBlack
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => PatchHelper.PlatformBrushOpCodes(typeof(DarkTheme), nameof(DarkTheme.f_NIBlack));
    }

    [HarmonyPatch(typeof(PlatformBrushes), nameof(PlatformBrushes.NITrueWhite), MethodType.Getter)]
    class PlatformBrushes_NITrueWhite
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => PatchHelper.PlatformBrushOpCodes(typeof(DarkTheme), nameof(DarkTheme.f_NITrueWhite));
    }

    [HarmonyPatch(typeof(PlatformBrushes), nameof(PlatformBrushes.NIGrayCool36), MethodType.Getter)]
    class PlatformBrushes_NIGrayCool36
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => PatchHelper.PlatformBrushOpCodes(typeof(DarkTheme), nameof(DarkTheme.f_NIGrayCool36));
    }

    [HarmonyPatch(typeof(PlatformBrushes), nameof(PlatformBrushes.NIGray), MethodType.Getter)]
    class PlatformBrushes_NIGray
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => PatchHelper.PlatformBrushOpCodes(typeof(DarkTheme), nameof(DarkTheme.f_NIGray));
    }

    [HarmonyPatch(typeof(PlatformBrushes), nameof(PlatformBrushes.NIWarning), MethodType.Getter)]
    class PlatformBrushes_NIWarning
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => PatchHelper.PlatformBrushOpCodes(typeof(DarkTheme), nameof(DarkTheme.f_NIWarning));
    }

    [HarmonyPatch(typeof(PlatformBrushes), nameof(PlatformBrushes.NIBlue), MethodType.Getter)]
    class PlatformBrushes_NIBlue
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => PatchHelper.PlatformBrushOpCodes(typeof(DarkTheme), nameof(DarkTheme.f_NIBlue));
    }

    [HarmonyPatch(typeof(PlatformBrushes), nameof(PlatformBrushes.NIHighlight), MethodType.Getter)]
    class PlatformBrushes_NIHighlight
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => PatchHelper.PlatformBrushOpCodes(typeof(DarkTheme), nameof(DarkTheme.f_NIHighlight));
    }

    [HarmonyPatch(typeof(PlatformBrushes), nameof(PlatformBrushes.NIBlueMediumAccent), MethodType.Getter)]
    class PlatformBrushes_NIBlueMediumAccent
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => PatchHelper.PlatformBrushOpCodes(typeof(DarkTheme), nameof(DarkTheme.f_NIBlueMediumAccent));
    }

    [HarmonyPatch(typeof(PlatformBrushes), nameof(PlatformBrushes.NIBlueAccent), MethodType.Getter)]
    class PlatformBrushes_NIBlueAccent
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => PatchHelper.PlatformBrushOpCodes(typeof(DarkTheme), nameof(DarkTheme.f_NIBlueAccent));
    }

    [HarmonyPatch(typeof(PlatformBrushes), nameof(PlatformBrushes.NIHighlightText), MethodType.Getter)]
    class PlatformBrushes_NIHighlightText
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => PatchHelper.PlatformBrushOpCodes(typeof(DarkTheme), nameof(DarkTheme.f_NIHighlightText));
    }

    [HarmonyPatch(typeof(PlatformBrushes), nameof(PlatformBrushes.NIHighlightSelected), MethodType.Getter)]
    class PlatformBrushes_NIHighlightSelected
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => PatchHelper.PlatformBrushOpCodes(typeof(DarkTheme), nameof(DarkTheme.f_NIHighlightSelected));
    }

    [HarmonyPatch(typeof(PlatformBrushes), nameof(PlatformBrushes.NITrueBlack), MethodType.Getter)]
    class PlatformBrushes_NITrueBlack
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => PatchHelper.PlatformBrushOpCodes(typeof(DarkTheme), nameof(DarkTheme.f_NITrueBlack));
    }

    [HarmonyPatch(typeof(PlatformBrushes), nameof(PlatformBrushes.NIMediumGray), MethodType.Getter)]
    class PlatformBrushes_NIMediumGray
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => PatchHelper.PlatformBrushOpCodes(typeof(DarkTheme), nameof(DarkTheme.f_NIMediumGray));
    }

    [HarmonyPatch(typeof(PlatformBrushes), nameof(PlatformBrushes.NIIconGray), MethodType.Getter)]
    class PlatformBrushes_NIIconGray
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => PatchHelper.PlatformBrushOpCodes(typeof(DarkTheme), nameof(DarkTheme.f_NIIconGray));
    }

    [HarmonyPatch(typeof(PlatformBrushes), nameof(PlatformBrushes.NIDropDownGray), MethodType.Getter)]
    class PlatformBrushes_NIDropDownGray
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => PatchHelper.PlatformBrushOpCodes(typeof(DarkTheme), nameof(DarkTheme.f_NIDropDownGray));
    }

    [HarmonyPatch(typeof(PlatformBrushes), nameof(PlatformBrushes.NIBackdropGray), MethodType.Getter)]
    class PlatformBrushes_NIBackdropGray
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => PatchHelper.PlatformBrushOpCodes(typeof(DarkTheme), nameof(DarkTheme.f_NIBackdropGray));
    }

    [HarmonyPatch(typeof(PlatformBrushes), nameof(PlatformBrushes.NIGrayCool81), MethodType.Getter)]
    class PlatformBrushes_NIGrayCool81
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => PatchHelper.PlatformBrushOpCodes(typeof(DarkTheme), nameof(DarkTheme.f_NIGrayCool81));
    }

    [HarmonyPatch(typeof(PlatformBrushes), nameof(PlatformBrushes.NIGrayCool88), MethodType.Getter)]
    class PlatformBrushes_NIGrayCool88
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => PatchHelper.PlatformBrushOpCodes(typeof(DarkTheme), nameof(DarkTheme.f_NIGrayCool88));
    }

    [HarmonyPatch(typeof(PlatformBrushes), nameof(PlatformBrushes.NIGrayCool90), MethodType.Getter)]
    class PlatformBrushes_NIGrayCool90
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => PatchHelper.PlatformBrushOpCodes(typeof(DarkTheme), nameof(DarkTheme.f_NIGrayCool90));
    }

    [HarmonyPatch(typeof(PlatformBrushes), nameof(PlatformBrushes.NIBlueGray), MethodType.Getter)]
    class PlatformBrushes_NIBlueGray
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => PatchHelper.PlatformBrushOpCodes(typeof(DarkTheme), nameof(DarkTheme.f_NIBlueGray));
    }

    [HarmonyPatch(typeof(PlatformBrushes), nameof(PlatformBrushes.NIBackground), MethodType.Getter)]
    class PlatformBrushes_NIBackground
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => PatchHelper.PlatformBrushOpCodes(typeof(DarkTheme), nameof(DarkTheme.f_NIBackground));
    }

    [HarmonyPatch(typeof(PlatformBrushes), nameof(PlatformBrushes.NIWhite), MethodType.Getter)]
    class PlatformBrushes_NIWhite
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => PatchHelper.PlatformBrushOpCodes(typeof(DarkTheme), nameof(DarkTheme.f_NIWhite));
    }

    [HarmonyPatch(typeof(PlatformBrushes), nameof(PlatformBrushes.NIGrayCool94), MethodType.Getter)]
    class PlatformBrushes_NIGrayCool94
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => PatchHelper.PlatformBrushOpCodes(typeof(DarkTheme), nameof(DarkTheme.f_NIGrayCool94));
    }

    [HarmonyPatch(typeof(PlatformBrushes), nameof(PlatformBrushes.NIWhiteBlue), MethodType.Getter)]
    class PlatformBrushes_NIWhiteBlue
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => PatchHelper.PlatformBrushOpCodes(typeof(DarkTheme), nameof(DarkTheme.f_NIWhiteBlue));
    }

    [HarmonyPatch(typeof(PlatformBrushes), nameof(PlatformBrushes.NICoolWhite), MethodType.Getter)]
    class PlatformBrushes_NICoolWhite
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => PatchHelper.PlatformBrushOpCodes(typeof(DarkTheme), nameof(DarkTheme.f_NICoolWhite));
    }

    [HarmonyPatch(typeof(PlatformBrushes), nameof(PlatformBrushes.NICoolBlue98), MethodType.Getter)]
    class PlatformBrushes_NICoolBlue98
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => PatchHelper.PlatformBrushOpCodes(typeof(DarkTheme), nameof(DarkTheme.f_NICoolBlue98));
    }

    [HarmonyPatch(typeof(PlatformBrushes), nameof(PlatformBrushes.NIError), MethodType.Getter)]
    class PlatformBrushes_NIError
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => PatchHelper.PlatformBrushOpCodes(typeof(DarkTheme), nameof(DarkTheme.f_NIError));
    }

    [HarmonyPatch(typeof(PlatformBrushes), nameof(PlatformBrushes.NISuccess), MethodType.Getter)]
    class PlatformBrushes_NISuccess
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => PatchHelper.PlatformBrushOpCodes(typeof(DarkTheme), nameof(DarkTheme.f_NISuccess));
    }

    [HarmonyPatch(typeof(PlatformBrushes), nameof(PlatformBrushes.NIProbe), MethodType.Getter)]
    class PlatformBrushes_NIProbe
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => PatchHelper.PlatformBrushOpCodes(typeof(DarkTheme), nameof(DarkTheme.f_NIProbe));
    }

    [HarmonyPatch(typeof(PlatformBrushes), nameof(PlatformBrushes.NIGuideline), MethodType.Getter)]
    class PlatformBrushes_NIGuideline
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => PatchHelper.PlatformBrushOpCodes(typeof(DarkTheme), nameof(DarkTheme.f_NIGuideline));
    }
    #endregion

    #region In Development
    [HarmonyPatch(typeof(ShellButton), "OnApplyTemplate")]
    class ShellButton_OnApplyTemplate
    {
        static void Postfix(ref ShellButton __instance)
        {
            //FuseControlStyling.SetMouseOverBackgroundBrush(__instance, Brushes.Green);
        }
    }
    #endregion
}
