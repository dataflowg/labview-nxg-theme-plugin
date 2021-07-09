# LabVIEW NXG Theme Plugin
A LabVIEW NXG plugin which applies a dark theme to the UI and block diagram, and fixes up some vector graphics. It's mostly a proof of concept at this stage, and will likely never become a full theme replacement.

It's been built for and tested against LabVIEW NXG 5.0, but should work for the [final 5.1 release](https://forums.ni.com/t5/LabVIEW/Our-Commitment-to-LabVIEW-as-we-Expand-our-Software-Portfolio/m-p/4101878/highlight/true#M1182636).

![LabVIEW NXG with theme plugin](images/nxg-theme-plugin.png?raw=true "LabVIEW NXG with theme plugin")

## Installation
Download the latest release and extract it to the LabVIEW NXG Addons folder (`C:\Program Files\National Instruments\LabVIEW NXG X.X\Addons`), then run LabVIEW NXG.

## How It Works
The plugin is based around [Harmony](https://harmony.pardeike.net/), a library which is used to perform in memory patching of NXG's assemblies.

### Colors
Colors in NXG are defined in many different places:
* Hardcoded hex codes in XML resources
* Named colors in XML resources
* Hardcoded in code (`PlatformBrush.FromArgb(255, 255, 255, 255)`)
* Named colors in code (`PlatformBrushes.NIWhite`)
* Hardcoded hex codes in XAML
* Resource dictionaries
* Probably others

Developing the theme involves identifying a UI element, working out where its color(s) are defined (foreground, background, and highlight), defining a replacement color, and then the best method of patching in the new color.

Two tools are used to help in this process - [Snoop](https://github.com/snoopwpf/snoopwpf) and [dnSpy](https://github.com/dnSpy/dnSpy). Snoop can be attached to NXG at runtime, and allows traversal of the WPF tree to help find UI elements. dnSpy can then be used to dig into NXG's assemblies and find where those UI elements (and hopefully their colors) are defined.

![Snooping LabVIEW NXG](images/nxg-snoop.png?raw=true "Snooping LabVIEW NXG")

### Patching
The preferred patch method is Harmony's `Postfix` function applied to a WPF element's [`OnApplyTemplate`](https://docs.microsoft.com/en-us/dotnet/api/system.windows.frameworkelement.onapplytemplate?view=netframework-4.6.2) method. I've found this combination the most reliable and least effort when patching. It means the patch will run immediately after the original function has run, allowing color overrides to be applied. Example:

```csharp
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
```

In some cases the colors which need to be changed are private in scope to the original function / class. In this case a `Transpiler` method needs to be used, effectively inserting/replacing opcodes of the original function. Example:

```csharp
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
```

The other patch method is using custom xml files (see the *XML Resources* section below). The plugin includes methods which override NXG's resource loading, and will load resources from the xml on disk if it exists. This method is primarily for block diagram contents, both colors and graphics.

### Graphics
Along with colors, this plugin also replaces some graphical elements. The graphics are all vector based, and are defined in XML resources within NXG's assemblies.

Replacement vector graphics are created in Inkscape, and then the vector data (in the form `"F1 M 8,5 H 6 V 3 H 8 Z M 8,12 H 6 V 6 h 2 z"`) is used in the XML files.

#### A Note on Performance
The included graphical changes are mostly cosmetic. Some of NXG's vector graphics are very complex and could in theory be replaced with simplified versions to boost draw performance. For example this is the vector code for a case structure border:

![LabVIEW NXG case structure border](images/case-structure-vector.png?raw=true "LabVIEW NXG case structure border")

compared to a while loop border:

![LabVIEW NXG while loop border](images/while-loop-vector.png?raw=true "LabVIEW NXG while loop border")

The increased complexity of the case structure (and other complex graphics) is very noticeable - try dropping a few dozen case structures and a few dozen while loops onto their own block diagrams, and then quickly zoom in and out with `Ctrl + Mousewheel`. The case structure version is much, much slower to redraw, with the structures visibly popping into view.

## Known Issues
* Many colors haven't been replaced, or have a bright cyan or magenta placeholder.
    * Cyan indicates the patch has been applied but no theme color decided, where magenta means that individual UI element has yet to be individually patched.
* Depending on when the plugin gets loading during NXG's startup, it may or may not patch certain functions. So sometimes a color will be replaced, and sometimes it won't. Try restarting NXG if this is the case.
* Similarly sometimes a structure background won't have the right color applied due to when the patch is called. Delete and recreate the structure, or try dragging a new structure around the affected structure to force it to redraw, and then press Escape before placing the structure down.
* The splitters and docked rails sometimes don't have the correct background color. Open a VI, then drag its tab to create a new NXG window, which will now have the replacement background color. Close the original NXG window.

## Building
The plugin is built with the following:

* Visual Studio Community 2019
* .NET Framework 4.6.2
* Lib.Harmony (install using `Install-Package Lib.Harmony -Version 2.1.0`)
* LabVIEW NXG 5.0
    * NationalInstruments.Controls.MocControls
    * NationalInstruments.Core
    * NationalInstruments.FuseControlsImplementation
    * NationalInstruments.MocCommon
    * NationalInstruments.NIDock
    * NationalInstruments.PanelRuntime
    * NationalInstruments.PlatformFramework
    * NationalInstruments.VI

If you're building for a different NXG version, the post build event will need to be modified to copy the dll and xml files to the correct location.

## Future Work
At this stage nothing is planned. It would be nice to complete the theme, but given NI is no longer actively developing LabVIEW NXG, any further effort spent on this plugin would be wasted.

# XML Resources

The XML resources can be exported from NXG's DLL assemblies using dnSpy.

If a hex code color in the XML is replaced with a named color, it must be added to the *ResourceDictionary* in `DarkTheme.cs`.

Below is a summary of some of the changes made to XML files



## NationalInstruments.PlatformFramework
### NationalInstruments.PlatformFramework\Resources

#### *scrollbarhorizontal_ide_9grid.xml*
Horizontal scroll bar vectors and colors. Also defines colors for mouse over and mouse down. Does NOT define background color.
* Changed all colors to named colors

#### *scrollbarvertical_ide_9grid.xml*
Vertical scroll bar vectors and colors. Also defines colors for mouse over and mouse down. Does NOT define background color.
* Changed all colors to named colors


### NationalInstruments.PlatformFramework\sourcemodel\design\Resources
#### *tunnel_Background_96.xml*
Structure tunnel vectors and colors. Other resources are overlaid on this resource to indicate type, auto indexing, default value, etc.
* Changed all colors to named colors



## NationalInstruments.VI
### NationalInstruments.VI\Design\Resources
#### *casestructure_96.xml*
Defines case structure vectors and colors.
* Changed all colors to named colors

#### *forloop_96.xml*
For loop structure vectors and colors. Does NOT include iterator and N terminals.
* Changed all colors to named colors

#### *whileloop_96.xml*
Case structure vectors and colors. Does NOT include iterator and stop condition terminals.
* Changed all colors to named colors



## NationalInstruments.MocCommon
### NationalInstruments.MocCommon
Defines many of the node specific vectors and colors.
#### *NationalInstruments.MocCommon.Resources.Diagram.Nodes.CommentNodeNormal_96.xml*
Comment node vectors and colors. Does NOT define text color.
* Changed all colors to named colors

### NationalInstruments.MocCommon\resources\diagram\structureterminals
#### *iterationterminal_96.xml*
Iteration (i) terminal of loops, defining vector shape and colors.
* Changed all colors to named colors
* Outer border is blue
* Fixed 'i' to be more legible and fit on pixel grid

#### *nterminal_96.xml*
Number of iterations (N) terminal of for loop, defining vector shape and colors.
* Change all colors to named colors
* Outer border is blue
* Fixed 'N' to be more legible and fit on pixel grid