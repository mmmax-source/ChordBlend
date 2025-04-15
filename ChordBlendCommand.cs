// ChordBlendCommand.cs
using System;
using Rhino;
using Rhino.Commands;
using Rhino.Input;
using Rhino.Input.Custom;
using Rhino.Geometry;
using ChordBlend.UI;
using ChordBlend.Geometry;

namespace ChordBlend.Commands
{
    public class ChordBlendCommand : Command
    {
        public override string EnglishName => "ChordBlend";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            var go = new GetObject();
            go.SetCommandPrompt("Select a polyline or polycurve");
            go.GeometryFilter = Rhino.DocObjects.ObjectType.Curve;
            go.SubObjectSelect = false;
            go.Get();
            if (go.CommandResult() != Result.Success)
                return go.CommandResult();

            Curve inputCurve = go.Object(0).Curve();
            if (inputCurve == null || !inputCurve.IsValid)
            {
                RhinoApp.WriteLine("Invalid curve.");
                return Result.Failure;
            }

            double radius = 0;
            var rc = RhinoGet.GetNumber("Enter fillet radius at 90° corner", false, ref radius, 0.01, 1000.0);
            if (rc != Result.Success)
                return rc;

            double chordLength = radius * Math.Sqrt(2);
            RhinoApp.WriteLine("Computed chord length: {0:0.###}", chordLength);

            var dialog = new ChordBlendDialog();
            dialog.ShowModal(Rhino.UI.RhinoEtoApp.MainWindow);
            if (!dialog.Result)
            {
                RhinoApp.WriteLine("Cancelled.");
                return Result.Cancel;
            }

            double strength = dialog.SelectedStrength;
            int modeSelection = dialog.SelectedMode;

            Curve output = BlendGenerator.Generate(inputCurve, chordLength, strength, modeSelection);
            if (output == null || !output.IsValid)
            {
                RhinoApp.WriteLine("Failed to generate chord blend.");
                return Result.Failure;
            }

            doc.Objects.AddCurve(output);
            doc.Views.Redraw();
            RhinoApp.WriteLine("Chord blend created.");

            return Result.Success;
        }
    }
}
