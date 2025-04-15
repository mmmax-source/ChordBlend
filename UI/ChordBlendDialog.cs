// ChordBlendDialog.cs
using System;
using Eto.Drawing;
using Eto.Forms;
using Rhino;
using Rhino.Geometry;

namespace ChordBlend.UI
{
    public class ChordBlendDialog : Dialog<bool>
    {
        private Curve _sourceCurve;
        private double _chordLength;
        private RhinoDoc _doc;
        private Guid _previewId = Guid.Empty;

        public void SetPreviewData(Curve source, double chordLength, RhinoDoc doc)
        {
            _sourceCurve = source;
            _chordLength = chordLength;
            _doc = doc;
        }

        private Slider _strengthSlider;
        private Label _strengthLabel;
        private DropDown _modeDropdown;

        private double _strength = 0.5;
        private CheckBox _deleteInputCheck;
        public bool DeleteInput => _deleteInputCheck.Checked ?? false;
        private int _mode = 1;

        public double SelectedStrength { get { return _strength; } }
        public int SelectedMode { get { return _mode; } }

        public ChordBlendDialog()
        {
            Title = "Chord Blend Settings";
            Resizable = false;
            ClientSize = new Size(600, 300);

            _strengthSlider = new Slider { MinValue = 0, MaxValue = 250, Value = 110, Width = 400 };
            _strength = _strengthSlider.Value / 100.0;
            _strengthLabel = new Label { Text = string.Format("Blend Strength: {0:0.00}", _strength), VerticalAlignment = VerticalAlignment.Center };

            // Trigger initial preview on dialog load
            Shown += (s, e) =>
            {
                if (_sourceCurve != null && _doc != null)
                {
                    var preview = ChordBlend.Geometry.BlendGenerator.Generate(_sourceCurve, _chordLength, _strength, _mode);
                    if (preview != null && preview.IsValid)
                    {
                        if (_previewId != Guid.Empty)
                        {
                            _doc.Objects.Delete(_previewId, false);
                            _previewId = Guid.Empty;
                        }
                        _previewId = _doc.Objects.AddCurve(preview);
                        _doc.Views.Redraw();
                    }
                }
            };

            _strengthSlider.ValueChanged += (s, e) =>
            {
                _strength = _strengthSlider.Value / 100.0;
                _strengthLabel.Text = string.Format("Blend Strength: {0:0.00}", _strength);

                if (_sourceCurve != null && _doc != null)
                {
                    var preview = ChordBlend.Geometry.BlendGenerator.Generate(_sourceCurve, _chordLength, _strength, _mode);
                    if (preview != null && preview.IsValid)
                    {
                        if (_previewId != Guid.Empty)
                        {
                            _doc.Objects.Delete(_previewId, false);
                            _previewId = Guid.Empty;
                        }
                        _previewId = _doc.Objects.AddCurve(preview);
                        _doc.Views.Redraw();
                    }
                }
            };

            _modeDropdown = new DropDown
            {
                Items = { "Arc (G1)", "Blend (G2)" },
                SelectedIndex = _mode
            };
            _modeDropdown.SelectedIndexChanged += (s, e) => _mode = _modeDropdown.SelectedIndex;

            _deleteInputCheck = new CheckBox { Text = "Delete input", Checked = true };

            var okButton = new Button { Text = "OK" };
            okButton.Click += (s, e) => { Result = true; Close(); };

            var cancelButton = new Button { Text = "Cancel" };
            cancelButton.Click += (s, e) =>
            {
                if (_previewId != Guid.Empty)
                {
                    _doc.Objects.Delete(_previewId, false);
                    _previewId = Guid.Empty;
                }
                Result = false;
                Close();
            };

            Content = new TableLayout
            {
                Padding = 10,
                Spacing = new Size(5, 5),
                Rows =
                {
                    new Label { Text = "Chord Blend Settings", Font = new Font(SystemFont.Bold, 12) },
                    new TableRow(_strengthLabel, _strengthSlider),
                    new TableRow(new Label { Text = "Mode:" }, _modeDropdown),
                    new TableRow(_deleteInputCheck),
                    null,
                    new TableRow(null, okButton, cancelButton)
                }
            };
        }
    }
}