# ChordBlend for Rhino

**ChordBlend** is a Rhino plugin that replaces sharp corners in polylines with smooth transitions using either arc fillets (G1) or curvature-continuous blends (G2). The user defines a "90° equivalent radius", and the plugin automatically computes the corresponding chord length for consistent corner smoothing.

## Features
- Two blend modes: Arc (G1) and Blend (G2)
- Adjustable strength slider
- Live preview while adjusting parameters
- Optional deletion of the original curve

## How to Use
1. Load the plugin via `_PluginManager` or drag the `.rhp` file into Rhino.
2. Run the command `_ChordBlend`.
3. Select a polyline.
4. Enter a radius value (for a 90° corner).
5. Adjust the mode and blend strength in the dialog.
6. Click OK to apply the result.

## Installation
- Build the solution in Release mode.
- Rename the output `.dll` to `.rhp`.
- Install into Rhino using `_PluginManager → Install`.

## Notes
- The radius input is interpreted as the fillet radius for a 90-degree corner. Chord lengths for other angles are computed accordingly.
- Blend strength is scaled non-linearly based on corner sharpness for visually balanced transitions.

## Requirements
- Rhino 8
- Windows/Mac
- .NET Framework compatible with Rhino 8 plugin SDK

---

Developed by Max Hausmann  
Berlin, 2025
