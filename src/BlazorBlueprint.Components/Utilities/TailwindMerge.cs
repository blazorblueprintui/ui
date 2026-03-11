using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace BlazorBlueprint.Components;

/// <summary>
/// Provides Tailwind CSS class conflict resolution logic.
/// Intelligently merges Tailwind utility classes by identifying conflicts
/// and keeping only the last occurrence of conflicting utilities.
/// Handles variant prefixes (responsive, state, dark mode) so that conflicts
/// are scoped per-variant context.
/// </summary>
public static class TailwindMerge
{
    // Exact-match utility groups: class name → group identifier
    private static readonly Dictionary<string, string> ExactGroups = new()
    {
        // Display
        ["block"] = "display",
        ["inline-block"] = "display",
        ["inline"] = "display",
        ["flex"] = "display",
        ["inline-flex"] = "display",
        ["grid"] = "display",
        ["inline-grid"] = "display",
        ["hidden"] = "display",
        ["table"] = "display",
        ["table-row"] = "display",
        ["table-cell"] = "display",
        ["contents"] = "display",
        ["list-item"] = "display",
        ["flow-root"] = "display",

        // Position
        ["static"] = "position",
        ["fixed"] = "position",
        ["absolute"] = "position",
        ["relative"] = "position",
        ["sticky"] = "position",

        // Flex Direction
        ["flex-row"] = "flex-direction",
        ["flex-row-reverse"] = "flex-direction",
        ["flex-col"] = "flex-direction",
        ["flex-col-reverse"] = "flex-direction",

        // Flex Wrap
        ["flex-wrap"] = "flex-wrap",
        ["flex-nowrap"] = "flex-wrap",
        ["flex-wrap-reverse"] = "flex-wrap",

        // Flex Grow/Shrink
        ["grow"] = "flex-grow",
        ["grow-0"] = "flex-grow",
        ["shrink"] = "flex-shrink",
        ["shrink-0"] = "flex-shrink",

        // Justify Content
        ["justify-start"] = "justify-content",
        ["justify-end"] = "justify-content",
        ["justify-center"] = "justify-content",
        ["justify-between"] = "justify-content",
        ["justify-around"] = "justify-content",
        ["justify-evenly"] = "justify-content",
        ["justify-normal"] = "justify-content",
        ["justify-stretch"] = "justify-content",

        // Justify Items
        ["justify-items-start"] = "justify-items",
        ["justify-items-end"] = "justify-items",
        ["justify-items-center"] = "justify-items",
        ["justify-items-stretch"] = "justify-items",

        // Justify Self
        ["justify-self-auto"] = "justify-self",
        ["justify-self-start"] = "justify-self",
        ["justify-self-end"] = "justify-self",
        ["justify-self-center"] = "justify-self",
        ["justify-self-stretch"] = "justify-self",

        // Align Content
        ["content-start"] = "align-content",
        ["content-end"] = "align-content",
        ["content-center"] = "align-content",
        ["content-between"] = "align-content",
        ["content-around"] = "align-content",
        ["content-evenly"] = "align-content",
        ["content-normal"] = "align-content",
        ["content-baseline"] = "align-content",
        ["content-stretch"] = "align-content",

        // Align Items
        ["items-start"] = "align-items",
        ["items-end"] = "align-items",
        ["items-center"] = "align-items",
        ["items-baseline"] = "align-items",
        ["items-stretch"] = "align-items",

        // Align Self
        ["self-auto"] = "align-self",
        ["self-start"] = "align-self",
        ["self-end"] = "align-self",
        ["self-center"] = "align-self",
        ["self-stretch"] = "align-self",
        ["self-baseline"] = "align-self",

        // Place Content
        ["place-content-start"] = "place-content",
        ["place-content-end"] = "place-content",
        ["place-content-center"] = "place-content",
        ["place-content-between"] = "place-content",
        ["place-content-around"] = "place-content",
        ["place-content-evenly"] = "place-content",
        ["place-content-baseline"] = "place-content",
        ["place-content-stretch"] = "place-content",

        // Place Items
        ["place-items-start"] = "place-items",
        ["place-items-end"] = "place-items",
        ["place-items-center"] = "place-items",
        ["place-items-baseline"] = "place-items",
        ["place-items-stretch"] = "place-items",

        // Place Self
        ["place-self-auto"] = "place-self",
        ["place-self-start"] = "place-self",
        ["place-self-end"] = "place-self",
        ["place-self-center"] = "place-self",
        ["place-self-stretch"] = "place-self",

        // Font Size
        ["text-xs"] = "font-size",
        ["text-sm"] = "font-size",
        ["text-base"] = "font-size",
        ["text-lg"] = "font-size",
        ["text-xl"] = "font-size",
        ["text-2xl"] = "font-size",
        ["text-3xl"] = "font-size",
        ["text-4xl"] = "font-size",
        ["text-5xl"] = "font-size",
        ["text-6xl"] = "font-size",
        ["text-7xl"] = "font-size",
        ["text-8xl"] = "font-size",
        ["text-9xl"] = "font-size",

        // Font Weight
        ["font-thin"] = "font-weight",
        ["font-extralight"] = "font-weight",
        ["font-light"] = "font-weight",
        ["font-normal"] = "font-weight",
        ["font-medium"] = "font-weight",
        ["font-semibold"] = "font-weight",
        ["font-bold"] = "font-weight",
        ["font-extrabold"] = "font-weight",
        ["font-black"] = "font-weight",

        // Font Style
        ["italic"] = "font-style",
        ["not-italic"] = "font-style",

        // Text Alignment
        ["text-left"] = "text-align",
        ["text-center"] = "text-align",
        ["text-right"] = "text-align",
        ["text-justify"] = "text-align",
        ["text-start"] = "text-align",
        ["text-end"] = "text-align",

        // Text Decoration
        ["underline"] = "text-decoration",
        ["overline"] = "text-decoration",
        ["line-through"] = "text-decoration",
        ["no-underline"] = "text-decoration",

        // Text Decoration Style
        ["decoration-solid"] = "text-decoration-style",
        ["decoration-double"] = "text-decoration-style",
        ["decoration-dotted"] = "text-decoration-style",
        ["decoration-dashed"] = "text-decoration-style",
        ["decoration-wavy"] = "text-decoration-style",

        // Text Transform
        ["uppercase"] = "text-transform",
        ["lowercase"] = "text-transform",
        ["capitalize"] = "text-transform",
        ["normal-case"] = "text-transform",

        // Text Overflow
        ["truncate"] = "text-overflow",
        ["text-ellipsis"] = "text-overflow",
        ["text-clip"] = "text-overflow",

        // Text Wrap
        ["text-wrap"] = "text-wrap",
        ["text-nowrap"] = "text-wrap",
        ["text-balance"] = "text-wrap",
        ["text-pretty"] = "text-wrap",

        // Vertical Align
        ["align-baseline"] = "vertical-align",
        ["align-top"] = "vertical-align",
        ["align-middle"] = "vertical-align",
        ["align-bottom"] = "vertical-align",
        ["align-text-top"] = "vertical-align",
        ["align-text-bottom"] = "vertical-align",
        ["align-sub"] = "vertical-align",
        ["align-super"] = "vertical-align",

        // Whitespace
        ["whitespace-normal"] = "whitespace",
        ["whitespace-nowrap"] = "whitespace",
        ["whitespace-pre"] = "whitespace",
        ["whitespace-pre-line"] = "whitespace",
        ["whitespace-pre-wrap"] = "whitespace",
        ["whitespace-break-spaces"] = "whitespace",

        // Word Break
        ["break-normal"] = "word-break",
        ["break-words"] = "word-break",
        ["break-all"] = "word-break",
        ["break-keep"] = "word-break",

        // Overflow
        ["overflow-auto"] = "overflow",
        ["overflow-hidden"] = "overflow",
        ["overflow-clip"] = "overflow",
        ["overflow-visible"] = "overflow",
        ["overflow-scroll"] = "overflow",
        ["overflow-x-auto"] = "overflow-x",
        ["overflow-x-hidden"] = "overflow-x",
        ["overflow-x-clip"] = "overflow-x",
        ["overflow-x-visible"] = "overflow-x",
        ["overflow-x-scroll"] = "overflow-x",
        ["overflow-y-auto"] = "overflow-y",
        ["overflow-y-hidden"] = "overflow-y",
        ["overflow-y-clip"] = "overflow-y",
        ["overflow-y-visible"] = "overflow-y",
        ["overflow-y-scroll"] = "overflow-y",

        // Overscroll Behavior
        ["overscroll-auto"] = "overscroll",
        ["overscroll-contain"] = "overscroll",
        ["overscroll-none"] = "overscroll",
        ["overscroll-x-auto"] = "overscroll-x",
        ["overscroll-x-contain"] = "overscroll-x",
        ["overscroll-x-none"] = "overscroll-x",
        ["overscroll-y-auto"] = "overscroll-y",
        ["overscroll-y-contain"] = "overscroll-y",
        ["overscroll-y-none"] = "overscroll-y",

        // Border Style
        ["border-solid"] = "border-style",
        ["border-dashed"] = "border-style",
        ["border-dotted"] = "border-style",
        ["border-double"] = "border-style",
        ["border-hidden"] = "border-style",
        ["border-none"] = "border-style",

        // Border Radius (all corners)
        ["rounded-none"] = "border-radius",
        ["rounded-sm"] = "border-radius",
        ["rounded"] = "border-radius",
        ["rounded-md"] = "border-radius",
        ["rounded-lg"] = "border-radius",
        ["rounded-xl"] = "border-radius",
        ["rounded-2xl"] = "border-radius",
        ["rounded-3xl"] = "border-radius",
        ["rounded-full"] = "border-radius",

        // Border Radius (per-side)
        ["rounded-t-none"] = "border-radius-top",
        ["rounded-t-sm"] = "border-radius-top",
        ["rounded-t"] = "border-radius-top",
        ["rounded-t-md"] = "border-radius-top",
        ["rounded-t-lg"] = "border-radius-top",
        ["rounded-t-xl"] = "border-radius-top",
        ["rounded-t-2xl"] = "border-radius-top",
        ["rounded-t-3xl"] = "border-radius-top",
        ["rounded-t-full"] = "border-radius-top",
        ["rounded-r-none"] = "border-radius-right",
        ["rounded-r-sm"] = "border-radius-right",
        ["rounded-r"] = "border-radius-right",
        ["rounded-r-md"] = "border-radius-right",
        ["rounded-r-lg"] = "border-radius-right",
        ["rounded-r-xl"] = "border-radius-right",
        ["rounded-r-2xl"] = "border-radius-right",
        ["rounded-r-3xl"] = "border-radius-right",
        ["rounded-r-full"] = "border-radius-right",
        ["rounded-b-none"] = "border-radius-bottom",
        ["rounded-b-sm"] = "border-radius-bottom",
        ["rounded-b"] = "border-radius-bottom",
        ["rounded-b-md"] = "border-radius-bottom",
        ["rounded-b-lg"] = "border-radius-bottom",
        ["rounded-b-xl"] = "border-radius-bottom",
        ["rounded-b-2xl"] = "border-radius-bottom",
        ["rounded-b-3xl"] = "border-radius-bottom",
        ["rounded-b-full"] = "border-radius-bottom",
        ["rounded-l-none"] = "border-radius-left",
        ["rounded-l-sm"] = "border-radius-left",
        ["rounded-l"] = "border-radius-left",
        ["rounded-l-md"] = "border-radius-left",
        ["rounded-l-lg"] = "border-radius-left",
        ["rounded-l-xl"] = "border-radius-left",
        ["rounded-l-2xl"] = "border-radius-left",
        ["rounded-l-3xl"] = "border-radius-left",
        ["rounded-l-full"] = "border-radius-left",

        // Shadow
        ["shadow-sm"] = "box-shadow",
        ["shadow"] = "box-shadow",
        ["shadow-md"] = "box-shadow",
        ["shadow-lg"] = "box-shadow",
        ["shadow-xl"] = "box-shadow",
        ["shadow-2xl"] = "box-shadow",
        ["shadow-none"] = "box-shadow",
        ["shadow-inner"] = "box-shadow",

        // Ring Width
        ["ring-0"] = "ring-width",
        ["ring-1"] = "ring-width",
        ["ring-2"] = "ring-width",
        ["ring"] = "ring-width",
        ["ring-4"] = "ring-width",
        ["ring-8"] = "ring-width",
        ["ring-inset"] = "ring-inset",

        // Object Fit
        ["object-contain"] = "object-fit",
        ["object-cover"] = "object-fit",
        ["object-fill"] = "object-fit",
        ["object-none"] = "object-fit",
        ["object-scale-down"] = "object-fit",

        // Object Position
        ["object-bottom"] = "object-position",
        ["object-center"] = "object-position",
        ["object-left"] = "object-position",
        ["object-left-bottom"] = "object-position",
        ["object-left-top"] = "object-position",
        ["object-right"] = "object-position",
        ["object-right-bottom"] = "object-position",
        ["object-right-top"] = "object-position",
        ["object-top"] = "object-position",

        // Float
        ["float-start"] = "float",
        ["float-end"] = "float",
        ["float-left"] = "float",
        ["float-right"] = "float",
        ["float-none"] = "float",

        // Clear
        ["clear-start"] = "clear",
        ["clear-end"] = "clear",
        ["clear-left"] = "clear",
        ["clear-right"] = "clear",
        ["clear-both"] = "clear",
        ["clear-none"] = "clear",

        // Isolation
        ["isolate"] = "isolation",
        ["isolation-auto"] = "isolation",

        // Box Sizing
        ["box-border"] = "box-sizing",
        ["box-content"] = "box-sizing",

        // Table Layout
        ["table-auto"] = "table-layout",
        ["table-fixed"] = "table-layout",

        // Border Collapse
        ["border-collapse"] = "border-collapse",
        ["border-separate"] = "border-collapse",

        // Caption Side
        ["caption-top"] = "caption-side",
        ["caption-bottom"] = "caption-side",

        // Visibility
        ["visible"] = "visibility",
        ["invisible"] = "visibility",
        ["collapse"] = "visibility",

        // Cursor
        ["cursor-auto"] = "cursor",
        ["cursor-default"] = "cursor",
        ["cursor-pointer"] = "cursor",
        ["cursor-wait"] = "cursor",
        ["cursor-text"] = "cursor",
        ["cursor-move"] = "cursor",
        ["cursor-help"] = "cursor",
        ["cursor-not-allowed"] = "cursor",
        ["cursor-none"] = "cursor",
        ["cursor-context-menu"] = "cursor",
        ["cursor-progress"] = "cursor",
        ["cursor-cell"] = "cursor",
        ["cursor-crosshair"] = "cursor",
        ["cursor-vertical-text"] = "cursor",
        ["cursor-alias"] = "cursor",
        ["cursor-copy"] = "cursor",
        ["cursor-no-drop"] = "cursor",
        ["cursor-grab"] = "cursor",
        ["cursor-grabbing"] = "cursor",
        ["cursor-all-scroll"] = "cursor",
        ["cursor-col-resize"] = "cursor",
        ["cursor-row-resize"] = "cursor",
        ["cursor-n-resize"] = "cursor",
        ["cursor-e-resize"] = "cursor",
        ["cursor-s-resize"] = "cursor",
        ["cursor-w-resize"] = "cursor",
        ["cursor-ne-resize"] = "cursor",
        ["cursor-nw-resize"] = "cursor",
        ["cursor-se-resize"] = "cursor",
        ["cursor-sw-resize"] = "cursor",
        ["cursor-ew-resize"] = "cursor",
        ["cursor-ns-resize"] = "cursor",
        ["cursor-nesw-resize"] = "cursor",
        ["cursor-nwse-resize"] = "cursor",
        ["cursor-zoom-in"] = "cursor",
        ["cursor-zoom-out"] = "cursor",

        // Pointer Events
        ["pointer-events-none"] = "pointer-events",
        ["pointer-events-auto"] = "pointer-events",

        // Resize
        ["resize-none"] = "resize",
        ["resize-y"] = "resize",
        ["resize-x"] = "resize",
        ["resize"] = "resize",

        // Scroll Behavior
        ["scroll-auto"] = "scroll-behavior",
        ["scroll-smooth"] = "scroll-behavior",

        // Snap Type
        ["snap-none"] = "scroll-snap-type",
        ["snap-x"] = "scroll-snap-type",
        ["snap-y"] = "scroll-snap-type",
        ["snap-both"] = "scroll-snap-type",

        // Snap Align
        ["snap-start"] = "scroll-snap-align",
        ["snap-end"] = "scroll-snap-align",
        ["snap-center"] = "scroll-snap-align",
        ["snap-align-none"] = "scroll-snap-align",

        // Snap Stop
        ["snap-normal"] = "scroll-snap-stop",
        ["snap-always"] = "scroll-snap-stop",

        // Touch Action
        ["touch-auto"] = "touch-action",
        ["touch-none"] = "touch-action",
        ["touch-pan-x"] = "touch-action",
        ["touch-pan-left"] = "touch-action",
        ["touch-pan-right"] = "touch-action",
        ["touch-pan-y"] = "touch-action",
        ["touch-pan-up"] = "touch-action",
        ["touch-pan-down"] = "touch-action",
        ["touch-pinch-zoom"] = "touch-action",
        ["touch-manipulation"] = "touch-action",

        // User Select
        ["select-none"] = "user-select",
        ["select-text"] = "user-select",
        ["select-all"] = "user-select",
        ["select-auto"] = "user-select",

        // Will Change
        ["will-change-auto"] = "will-change",
        ["will-change-scroll"] = "will-change",
        ["will-change-contents"] = "will-change",
        ["will-change-transform"] = "will-change",

        // Appearance
        ["appearance-none"] = "appearance",
        ["appearance-auto"] = "appearance",

        // Aspect Ratio
        ["aspect-auto"] = "aspect-ratio",
        ["aspect-square"] = "aspect-ratio",
        ["aspect-video"] = "aspect-ratio",

        // Transition
        ["transition-none"] = "transition-property",
        ["transition-all"] = "transition-property",
        ["transition"] = "transition-property",
        ["transition-colors"] = "transition-property",
        ["transition-opacity"] = "transition-property",
        ["transition-shadow"] = "transition-property",
        ["transition-transform"] = "transition-property",

        // Animation
        ["animate-none"] = "animation",
        ["animate-spin"] = "animation",
        ["animate-ping"] = "animation",
        ["animate-pulse"] = "animation",
        ["animate-bounce"] = "animation",

        // Mix Blend Mode
        ["mix-blend-normal"] = "mix-blend-mode",
        ["mix-blend-multiply"] = "mix-blend-mode",
        ["mix-blend-screen"] = "mix-blend-mode",
        ["mix-blend-overlay"] = "mix-blend-mode",
        ["mix-blend-darken"] = "mix-blend-mode",
        ["mix-blend-lighten"] = "mix-blend-mode",
        ["mix-blend-color-dodge"] = "mix-blend-mode",
        ["mix-blend-color-burn"] = "mix-blend-mode",
        ["mix-blend-hard-light"] = "mix-blend-mode",
        ["mix-blend-soft-light"] = "mix-blend-mode",
        ["mix-blend-difference"] = "mix-blend-mode",
        ["mix-blend-exclusion"] = "mix-blend-mode",
        ["mix-blend-hue"] = "mix-blend-mode",
        ["mix-blend-saturation"] = "mix-blend-mode",
        ["mix-blend-color"] = "mix-blend-mode",
        ["mix-blend-luminosity"] = "mix-blend-mode",
        ["mix-blend-plus-lighter"] = "mix-blend-mode",

        // Background Size
        ["bg-auto"] = "background-size",
        ["bg-cover"] = "background-size",
        ["bg-contain"] = "background-size",

        // Background Repeat
        ["bg-repeat"] = "background-repeat",
        ["bg-no-repeat"] = "background-repeat",
        ["bg-repeat-x"] = "background-repeat",
        ["bg-repeat-y"] = "background-repeat",
        ["bg-repeat-round"] = "background-repeat",
        ["bg-repeat-space"] = "background-repeat",

        // Background Position
        ["bg-bottom"] = "background-position",
        ["bg-center"] = "background-position",
        ["bg-left"] = "background-position",
        ["bg-left-bottom"] = "background-position",
        ["bg-left-top"] = "background-position",
        ["bg-right"] = "background-position",
        ["bg-right-bottom"] = "background-position",
        ["bg-right-top"] = "background-position",
        ["bg-top"] = "background-position",

        // Background Attachment
        ["bg-fixed"] = "background-attachment",
        ["bg-local"] = "background-attachment",
        ["bg-scroll"] = "background-attachment",

        // Background Clip
        ["bg-clip-border"] = "background-clip",
        ["bg-clip-padding"] = "background-clip",
        ["bg-clip-content"] = "background-clip",
        ["bg-clip-text"] = "background-clip",

        // Background Origin
        ["bg-origin-border"] = "background-origin",
        ["bg-origin-padding"] = "background-origin",
        ["bg-origin-content"] = "background-origin",

        // List Style Type
        ["list-none"] = "list-style-type",
        ["list-disc"] = "list-style-type",
        ["list-decimal"] = "list-style-type",

        // List Style Position
        ["list-inside"] = "list-style-position",
        ["list-outside"] = "list-style-position",

        // Box Decoration Break
        ["box-decoration-clone"] = "box-decoration-break",
        ["box-decoration-slice"] = "box-decoration-break",

        // Hyphens
        ["hyphens-none"] = "hyphens",
        ["hyphens-manual"] = "hyphens",
        ["hyphens-auto"] = "hyphens",

        // Grid Auto Flow
        ["grid-flow-row"] = "grid-auto-flow",
        ["grid-flow-col"] = "grid-auto-flow",
        ["grid-flow-dense"] = "grid-auto-flow",
        ["grid-flow-row-dense"] = "grid-auto-flow",
        ["grid-flow-col-dense"] = "grid-auto-flow",

        // Accent Color
        ["accent-auto"] = "accent-color",

        // Outline Style
        ["outline-none"] = "outline-style",
        ["outline"] = "outline-style",
        ["outline-dashed"] = "outline-style",
        ["outline-dotted"] = "outline-style",
        ["outline-double"] = "outline-style",

        // Force Color Adjust
        ["forced-color-adjust-auto"] = "forced-color-adjust",
        ["forced-color-adjust-none"] = "forced-color-adjust",

        // Container
        ["container"] = "container",

        // Screen Reader Only
        ["sr-only"] = "screen-reader",
        ["not-sr-only"] = "screen-reader",
    };

    // Prefix-based utility groups: prefix → group identifier
    // Used for utilities that take a value after a hyphen (e.g., p-4, m-auto)
    private static readonly Dictionary<string, string> PrefixGroups = new()
    {
        // Padding
        ["p"] = "padding",
        ["px"] = "padding-x",
        ["py"] = "padding-y",
        ["pt"] = "padding-top",
        ["pr"] = "padding-right",
        ["pb"] = "padding-bottom",
        ["pl"] = "padding-left",
        ["ps"] = "padding-inline-start",
        ["pe"] = "padding-inline-end",

        // Margin
        ["m"] = "margin",
        ["mx"] = "margin-x",
        ["my"] = "margin-y",
        ["mt"] = "margin-top",
        ["mr"] = "margin-right",
        ["mb"] = "margin-bottom",
        ["ml"] = "margin-left",
        ["ms"] = "margin-inline-start",
        ["me"] = "margin-inline-end",

        // Width / Height
        ["w"] = "width",
        ["min-w"] = "min-width",
        ["max-w"] = "max-width",
        ["h"] = "height",
        ["min-h"] = "min-height",
        ["max-h"] = "max-height",
        ["size"] = "size",

        // Inset
        ["inset"] = "inset",
        ["inset-x"] = "inset-x",
        ["inset-y"] = "inset-y",
        ["top"] = "top",
        ["right"] = "right",
        ["bottom"] = "bottom",
        ["left"] = "left",
        ["start"] = "inset-inline-start",
        ["end"] = "inset-inline-end",

        // Gap
        ["gap"] = "gap",
        ["gap-x"] = "gap-x",
        ["gap-y"] = "gap-y",

        // Space Between
        ["space-x"] = "space-x",
        ["space-y"] = "space-y",

        // Divide Width
        ["divide-x"] = "divide-x",
        ["divide-y"] = "divide-y",

        // Opacity
        ["opacity"] = "opacity",

        // Z-Index
        ["z"] = "z-index",

        // Order
        ["order"] = "order",

        // Grid
        ["grid-cols"] = "grid-cols",
        ["grid-rows"] = "grid-rows",
        ["col-span"] = "grid-column-span",
        ["col-start"] = "grid-column-start",
        ["col-end"] = "grid-column-end",
        ["row-span"] = "grid-row-span",
        ["row-start"] = "grid-row-start",
        ["row-end"] = "grid-row-end",
        ["auto-cols"] = "grid-auto-cols",
        ["auto-rows"] = "grid-auto-rows",

        // Columns (multi-column layout)
        ["columns"] = "columns",

        // Flex Basis
        ["basis"] = "flex-basis",

        // Line Height
        ["leading"] = "line-height",

        // Letter Spacing
        ["tracking"] = "letter-spacing",

        // Text Indent
        ["indent"] = "text-indent",

        // Decoration Thickness
        ["decoration"] = "text-decoration-thickness",

        // Underline Offset
        ["underline-offset"] = "underline-offset",

        // Line Clamp
        ["line-clamp"] = "line-clamp",

        // Border Width (all sides)
        ["border"] = "border-width",
        ["border-x"] = "border-width-x",
        ["border-y"] = "border-width-y",
        ["border-s"] = "border-width-inline-start",
        ["border-e"] = "border-width-inline-end",
        ["border-t"] = "border-width-top",
        ["border-r"] = "border-width-right",
        ["border-b"] = "border-width-bottom",
        ["border-l"] = "border-width-left",

        // Outline Width/Offset
        ["outline-offset"] = "outline-offset",

        // Ring Offset
        ["ring-offset"] = "ring-offset-width",

        // Transition Duration
        ["duration"] = "transition-duration",

        // Transition Delay
        ["delay"] = "transition-delay",

        // Transition Timing
        ["ease"] = "transition-timing",

        // Scale
        ["scale"] = "scale",
        ["scale-x"] = "scale-x",
        ["scale-y"] = "scale-y",

        // Rotate
        ["rotate"] = "rotate",

        // Translate
        ["translate-x"] = "translate-x",
        ["translate-y"] = "translate-y",

        // Skew
        ["skew-x"] = "skew-x",
        ["skew-y"] = "skew-y",

        // Transform Origin
        ["origin"] = "transform-origin",

        // Scroll Margin
        ["scroll-m"] = "scroll-margin",
        ["scroll-mx"] = "scroll-margin-x",
        ["scroll-my"] = "scroll-margin-y",
        ["scroll-mt"] = "scroll-margin-top",
        ["scroll-mr"] = "scroll-margin-right",
        ["scroll-mb"] = "scroll-margin-bottom",
        ["scroll-ml"] = "scroll-margin-left",
        ["scroll-ms"] = "scroll-margin-inline-start",
        ["scroll-me"] = "scroll-margin-inline-end",

        // Scroll Padding
        ["scroll-p"] = "scroll-padding",
        ["scroll-px"] = "scroll-padding-x",
        ["scroll-py"] = "scroll-padding-y",
        ["scroll-pt"] = "scroll-padding-top",
        ["scroll-pr"] = "scroll-padding-right",
        ["scroll-pb"] = "scroll-padding-bottom",
        ["scroll-pl"] = "scroll-padding-left",
        ["scroll-ps"] = "scroll-padding-inline-start",
        ["scroll-pe"] = "scroll-padding-inline-end",

        // Backdrop Blur (and other backdrop-*)
        ["backdrop-blur"] = "backdrop-blur",
        ["backdrop-brightness"] = "backdrop-brightness",
        ["backdrop-contrast"] = "backdrop-contrast",
        ["backdrop-grayscale"] = "backdrop-grayscale",
        ["backdrop-hue-rotate"] = "backdrop-hue-rotate",
        ["backdrop-invert"] = "backdrop-invert",
        ["backdrop-opacity"] = "backdrop-opacity",
        ["backdrop-saturate"] = "backdrop-saturate",
        ["backdrop-sepia"] = "backdrop-sepia",

        // Filter
        ["blur"] = "blur",
        ["brightness"] = "brightness",
        ["contrast"] = "contrast",
        ["grayscale"] = "grayscale",
        ["hue-rotate"] = "hue-rotate",
        ["invert"] = "invert",
        ["saturate"] = "saturate",
        ["sepia"] = "sepia",
        ["drop-shadow"] = "drop-shadow",

        // Fill / Stroke (SVG)
        ["fill"] = "fill",
        ["stroke"] = "stroke",
        ["stroke-w"] = "stroke-width",
    };

    // Maps shorthand padding/margin prefixes to the longhand groups they encompass.
    // When a longhand (e.g., px-2) appears, any earlier shorthand (p-4) is removed.
    private static readonly Dictionary<string, string[]> ShorthandToLonghandGroups = new()
    {
        ["padding"] = ["padding-x", "padding-y", "padding-top", "padding-right", "padding-bottom", "padding-left", "padding-inline-start", "padding-inline-end"],
        ["padding-x"] = ["padding-right", "padding-left"],
        ["padding-y"] = ["padding-top", "padding-bottom"],
        ["margin"] = ["margin-x", "margin-y", "margin-top", "margin-right", "margin-bottom", "margin-left", "margin-inline-start", "margin-inline-end"],
        ["margin-x"] = ["margin-right", "margin-left"],
        ["margin-y"] = ["margin-top", "margin-bottom"],
        ["inset"] = ["inset-x", "inset-y", "top", "right", "bottom", "left", "inset-inline-start", "inset-inline-end"],
        ["inset-x"] = ["right", "left"],
        ["inset-y"] = ["top", "bottom"],
        ["border-width"] = ["border-width-x", "border-width-y", "border-width-top", "border-width-right", "border-width-bottom", "border-width-left", "border-width-inline-start", "border-width-inline-end"],
        ["border-width-x"] = ["border-width-right", "border-width-left"],
        ["border-width-y"] = ["border-width-top", "border-width-bottom"],
        ["border-radius"] = ["border-radius-top", "border-radius-right", "border-radius-bottom", "border-radius-left"],
        ["gap"] = ["gap-x", "gap-y"],
        ["overflow"] = ["overflow-x", "overflow-y"],
        ["overscroll"] = ["overscroll-x", "overscroll-y"],
        ["scroll-margin"] = ["scroll-margin-x", "scroll-margin-y", "scroll-margin-top", "scroll-margin-right", "scroll-margin-bottom", "scroll-margin-left", "scroll-margin-inline-start", "scroll-margin-inline-end"],
        ["scroll-margin-x"] = ["scroll-margin-right", "scroll-margin-left"],
        ["scroll-margin-y"] = ["scroll-margin-top", "scroll-margin-bottom"],
        ["scroll-padding"] = ["scroll-padding-x", "scroll-padding-y", "scroll-padding-top", "scroll-padding-right", "scroll-padding-bottom", "scroll-padding-left", "scroll-padding-inline-start", "scroll-padding-inline-end"],
        ["scroll-padding-x"] = ["scroll-padding-right", "scroll-padding-left"],
        ["scroll-padding-y"] = ["scroll-padding-top", "scroll-padding-bottom"],
        ["scale"] = ["scale-x", "scale-y"],
    };

    // Known Tailwind color utility prefixes that accept color values
    private static readonly HashSet<string> ColorPrefixes = new(StringComparer.Ordinal)
    {
        "text", "bg", "border", "border-t", "border-r", "border-b", "border-l",
        "border-x", "border-y", "border-s", "border-e",
        "ring", "ring-offset", "outline", "divide",
        "accent", "caret", "fill", "stroke",
        "decoration", "shadow", "from", "via", "to",
        "placeholder"
    };

    // Cache for utility group lookups to avoid repeated evaluation
    private static readonly ConcurrentDictionary<string, string?> utilityGroupCache = new();
    private const int MaxCacheSize = 10_000;

    // Regex to validate CSS class names - allows alphanumeric, hyphens, underscores, colons, slashes, brackets, dots, percentages, and CSS combinator characters
    // This covers Tailwind classes like "w-1/2", "hover:bg-blue-500", "data-[state=open]:block", "text-[14px]", "[&>svg]:absolute"
    private static readonly Regex ValidClassNameRegex = new(@"^[a-zA-Z0-9_\-:/.[\]()%!@#&>+~=*,' ]+$", RegexOptions.Compiled);

    /// <summary>
    /// Validates that a CSS class name contains only safe characters.
    /// Rejects classes that could be used for CSS injection attacks.
    /// </summary>
    private static bool IsValidClassName(string className)
    {
        if (string.IsNullOrWhiteSpace(className) || className.Length > 200)
        {
            return false;
        }

        // Check for potentially dangerous patterns
        if (className.Contains("expression", StringComparison.OrdinalIgnoreCase) ||
            className.Contains("javascript", StringComparison.OrdinalIgnoreCase) ||
            className.Contains("url(", StringComparison.OrdinalIgnoreCase) ||
            className.Contains("import", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return ValidClassNameRegex.IsMatch(className);
    }

    /// <summary>
    /// Strips variant prefixes (responsive, state, dark, data attributes, etc.) from a class name.
    /// Returns the variant prefix string and the base utility class.
    /// For example: "md:hover:p-4" → (prefix: "md:hover:", base: "p-4")
    /// </summary>
    private static (string prefix, string baseClass) SplitVariantPrefix(string className)
    {
        // Find the last colon that is not inside brackets
        var lastColonIndex = -1;
        var bracketDepth = 0;

        for (var i = 0; i < className.Length; i++)
        {
            var c = className[i];
            if (c == '[')
            {
                bracketDepth++;
            }
            else if (c == ']')
            {
                bracketDepth--;
            }
            else if (c == ':' && bracketDepth == 0)
            {
                lastColonIndex = i;
            }
        }

        if (lastColonIndex < 0)
        {
            return (string.Empty, className);
        }

        return (className[..(lastColonIndex + 1)], className[(lastColonIndex + 1)..]);
    }

    /// <summary>
    /// Strips the leading important modifier (!) and/or negative sign (-) from a base class.
    /// Returns the stripped base class for group resolution.
    /// For example: "!-p-4" → "p-4", "!text-red-500" → "text-red-500", "-m-4" → "m-4"
    /// </summary>
    private static string StripModifiers(string baseClass)
    {
        var start = 0;
        if (start < baseClass.Length && baseClass[start] == '!')
        {
            start++;
        }
        if (start < baseClass.Length && baseClass[start] == '-')
        {
            start++;
        }
        return start > 0 ? baseClass[start..] : baseClass;
    }

    /// <summary>
    /// Merges an array of CSS class strings, resolving Tailwind utility conflicts.
    /// Later classes in the array take precedence over earlier ones when conflicts occur.
    /// Classes with the important modifier (!) always win over non-important classes
    /// in the same utility group.
    /// Variant prefixes (sm:, md:, hover:, dark:, etc.) are handled so that conflicts
    /// are scoped per-variant context.
    /// </summary>
    /// <param name="classes">Array of class strings to merge</param>
    /// <returns>Merged class string with conflicts resolved</returns>
    public static string Merge(string[] classes)
    {
        if (classes == null || classes.Length == 0)
        {
            return string.Empty;
        }

        // Dictionary to track the last occurrence of each utility group (scoped by variant prefix)
        // The bool tracks whether the stored class has the important modifier
        var groupedClasses = new Dictionary<string, (string className, int index, bool important)>();
        var unGroupedClasses = new Dictionary<string, (string className, int index)>(StringComparer.Ordinal);

        for (var i = 0; i < classes.Length; i++)
        {
            var className = classes[i];
            if (string.IsNullOrWhiteSpace(className))
            {
                continue;
            }

            // Skip potentially dangerous class names
            if (!IsValidClassName(className))
            {
                continue;
            }

            var group = GetUtilityGroup(className);
            if (!string.IsNullOrEmpty(group))
            {
                // When a shorthand appears (e.g., p-4), remove all earlier longhands
                // (e.g., pr-2, pl-6) since the shorthand overrides them all.
                // When a longhand appears (e.g., pr-10 after p-4), both are kept —
                // the longhand only overrides its specific side.
                var (variantPrefix, baseClass) = SplitVariantPrefix(className);
                var baseGroup = variantPrefix.Length > 0 ? group[variantPrefix.Length..] : group;

                if (ShorthandToLonghandGroups.TryGetValue(baseGroup, out var longhands))
                {
                    for (var j = 0; j < longhands.Length; j++)
                    {
                        var longhandKey = variantPrefix + longhands[j];
                        groupedClasses.Remove(longhandKey);
                    }
                }

                // Determine if the current class has the important modifier
                var isImportant = baseClass.Length > 0 && baseClass[0] == '!';

                // Important classes win over non-important classes in the same group
                if (groupedClasses.TryGetValue(group, out var existing) && existing.important && !isImportant)
                {
                    // Existing class is important and current is not — keep the important one
                    continue;
                }

                groupedClasses[group] = (className, i, isImportant);
            }
            else
            {
                // Unknown utility or custom class - preserve it, deduplicate (last occurrence wins)
                unGroupedClasses[className] = (className, i);
            }
        }

        // Combine grouped and ungrouped classes, maintaining relative order
        var allClasses = groupedClasses.Values
            .Select(x => (x.className, x.index))
            .Concat(unGroupedClasses.Values)
            .OrderBy(x => x.index)
            .Select(x => x.className);

        return string.Join(" ", allClasses);
    }

    /// <summary>
    /// Identifies which utility group a class belongs to.
    /// Returns null if the class doesn't match any known Tailwind utility pattern.
    /// Results are cached for performance. When the cache exceeds its size limit,
    /// it is cleared to prevent unbounded memory growth in long-running server sessions.
    /// </summary>
    private static string? GetUtilityGroup(string className)
    {
        if (utilityGroupCache.TryGetValue(className, out var cached))
        {
            return cached;
        }

        var group = ComputeUtilityGroup(className);

        // When the cache is full, clear it to reclaim memory and allow new entries.
        // This is simpler and more predictable than LRU eviction for this use case,
        // since the working set of classes in an app is typically stable and will
        // quickly repopulate after a clear.
        if (utilityGroupCache.Count >= MaxCacheSize)
        {
            utilityGroupCache.Clear();
        }

        utilityGroupCache.TryAdd(className, group);

        return group;
    }

    /// <summary>
    /// Computes the utility group for a class name.
    /// Handles variant prefixes, important modifier, negative values, arbitrary values,
    /// and color utilities with semantic/hyphenated names.
    /// </summary>
    private static string? ComputeUtilityGroup(string className)
    {
        // Split variant prefix (e.g., "md:hover:p-4" → prefix="md:hover:", base="p-4")
        var (variantPrefix, baseClass) = SplitVariantPrefix(className);

        // Strip important modifier (!) and negative sign (-)
        var stripped = StripModifiers(baseClass);

        // Resolve the group from the stripped base class
        var group = ResolveBaseGroup(stripped);
        if (group == null)
        {
            return null;
        }

        // Scope the group by variant prefix so "md:p-4" and "lg:p-4" don't conflict
        return variantPrefix + group;
    }

    /// <summary>
    /// Resolves the utility group for a base class (no variant prefix, no modifiers).
    /// </summary>
    private static string? ResolveBaseGroup(string baseClass)
    {
        // Check exact matches first (display, position, font-weight, etc.)
        if (ExactGroups.TryGetValue(baseClass, out var group))
        {
            return group;
        }

        // Handle arbitrary value classes: e.g., p-[14px], w-[calc(100%-2rem)], bg-[#ff0]
        // Strip the arbitrary part and try to match the prefix
        var arbitraryIndex = baseClass.IndexOf('-');
        if (arbitraryIndex < 0)
        {
            return null;
        }

        // Try progressively shorter prefixes to find a match
        // e.g., for "border-t-2", try "border-t" first, then "border"
        // For "min-w-full", try "min-w" first
        var remaining = baseClass;
        while (true)
        {
            var lastDash = remaining.LastIndexOf('-');
            if (lastDash < 0)
            {
                break;
            }

            var prefix = remaining[..lastDash];
            var value = remaining[(lastDash + 1)..];

            // Check if this is a known color utility prefix with a color value
            if (IsColorPrefix(prefix) && !IsNonColorValue(prefix, value))
            {
                return prefix + "-color";
            }

            // Check if this prefix maps to a group
            if (PrefixGroups.TryGetValue(prefix, out var prefixGroup))
            {
                return prefixGroup;
            }

            remaining = prefix;
        }

        return null;
    }

    /// <summary>
    /// Checks if a prefix is a known color utility prefix.
    /// </summary>
    private static bool IsColorPrefix(string prefix) =>
        ColorPrefixes.Contains(prefix);

    // Non-color values for each color prefix, used to disambiguate utilities like
    // "text-lg" (font-size) from "text-red-500" (color). Maintained as a dictionary
    // of HashSets for O(1) lookup and easy extensibility when new Tailwind versions
    // add utilities.
    private static readonly Dictionary<string, HashSet<string>> NonColorValues = new(StringComparer.Ordinal)
    {
        ["text"] = new(StringComparer.Ordinal)
        {
            // Font sizes
            "xs", "sm", "base", "lg", "xl", "2xl", "3xl", "4xl", "5xl", "6xl", "7xl", "8xl", "9xl",
            // Text alignment
            "left", "center", "right", "justify", "start", "end",
            // Text overflow/wrap
            "ellipsis", "clip", "wrap", "nowrap", "balance", "pretty"
        },
        ["bg"] = new(StringComparer.Ordinal)
        {
            "auto", "cover", "contain",
            "repeat", "no-repeat", "repeat-x", "repeat-y", "repeat-round", "repeat-space",
            "fixed", "local", "scroll",
            "bottom", "center", "left", "left-bottom", "left-top",
            "right", "right-bottom", "right-top", "top",
            "clip-border", "clip-padding", "clip-content", "clip-text",
            "origin-border", "origin-padding", "origin-content"
        },
        ["border"] = new(StringComparer.Ordinal)
        {
            "solid", "dashed", "dotted", "double", "hidden", "none",
            "collapse", "separate"
        },
        ["outline"] = new(StringComparer.Ordinal)
        {
            "none", "dashed", "dotted", "double"
        },
        ["decoration"] = new(StringComparer.Ordinal)
        {
            "solid", "double", "dotted", "dashed", "wavy",
            "auto", "from-font"
        },
        ["shadow"] = new(StringComparer.Ordinal)
        {
            "sm", "md", "lg", "xl", "2xl", "none", "inner"
        },
        ["divide"] = new(StringComparer.Ordinal)
        {
            "solid", "dashed", "dotted", "double", "none",
            "x", "y", "x-0", "y-0", "x-2", "y-2",
            "x-4", "y-4", "x-8", "y-8", "x-reverse", "y-reverse"
        },
        ["stroke"] = new(StringComparer.Ordinal)
        {
            "0", "1", "2"
        },
        ["ring"] = new(StringComparer.Ordinal)
        {
            "0", "1", "2", "4", "8", "inset"
        },
        ["ring-offset"] = new(StringComparer.Ordinal)
        {
            "0", "1", "2", "4", "8"
        }
    };

    /// <summary>
    /// Determines if a value after a color prefix is actually a non-color utility value.
    /// For example, "text-lg" is font-size not text color, "bg-cover" is background-size not bg color.
    /// </summary>
    private static bool IsNonColorValue(string prefix, string value)
    {
        // Pure numeric values (e.g., border-2, ring-4, outline-2) are width/size, not color
        if (value.Length > 0 && char.IsDigit(value[0]))
        {
            return true;
        }

        // Check the prefix-specific non-color values
        return NonColorValues.TryGetValue(prefix, out var nonColorSet) && nonColorSet.Contains(value);
    }
}
