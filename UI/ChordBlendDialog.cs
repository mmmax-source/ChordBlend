// ChordBlendDialog.cs
using Eto.Drawing;
using Eto.Forms;

namespace ChordBlend.UI
{
    public class ChordBlendDialog : Dialog<bool>
    {
        private Slider _strengthSlider;
        private Label _strengthLabel;
        private DropDown _modeDropdown;

        private double _strength = 0.5;
        private int _mode = 1;

        public double SelectedStrength { get { return _strength; } }
        public int SelectedMode { get { return _mode; } }

        public ChordBlendDialog()
        {
            Title = "Chord Blend Settings";
            Resizable = false;
            ClientSize = new Size(300, 300);

            _strengthSlider = new Slider { MinValue = 0, MaxValue = 200, Value = 110 };
            _strength = _strengthSlider.Value / 100.0;
            _strengthLabel = new Label { Text = string.Format("Blend Strength: {0:0.00}", _strength), VerticalAlignment = VerticalAlignment.Center };


            _strengthSlider.ValueChanged += (s, e) =>
            {
                _strength = _strengthSlider.Value / 100.0;
                _strengthLabel.Text = string.Format("Strength: {0:0.00}", _strength);
            };

            _modeDropdown = new DropDown
            {
                Items = { "Arc (G1)", "Blend (G2)" },
                SelectedIndex = _mode
            };
            _modeDropdown.SelectedIndexChanged += (s, e) => _mode = _modeDropdown.SelectedIndex;

            var okButton = new Button { Text = "OK" };
            okButton.Click += (s, e) => { Result = true; Close(); };

            var cancelButton = new Button { Text = "Cancel" };
            cancelButton.Click += (s, e) => { Result = false; Close(); };

            Content = new TableLayout
            {
                Padding = 10,
                Spacing = new Size(5, 5),
                Rows =
                {
                    new Label { Text = "Chord Blend Settings", Font = new Font(SystemFont.Bold, 10) },
                    new TableRow(_strengthLabel, _strengthSlider),
                    new TableRow(new Label { Text = "Mode:" }, _modeDropdown),
                    null,
                    new TableRow(null, okButton, cancelButton)
                }
            };
        }
    }
}
