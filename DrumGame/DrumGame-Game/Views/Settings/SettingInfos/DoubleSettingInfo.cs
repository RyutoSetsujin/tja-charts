using DrumGame.Game.Components;
using osu.Framework.Bindables;
using osu.Framework.Graphics;

namespace DrumGame.Game.Views.Settings.SettingInfos;

public class DoubleSettingInfo : SettingInfo
{
    public Bindable<double> Binding;
    public DoubleSettingInfo(string label, Bindable<double> binding) : base(label)
    {
        Binding = binding;
    }
    public override void Render(SettingControl control)
    {
        var textBox = new DrumTextBox
        {
            Width = 300,
            Height = Height - 4,
            Y = 2,
            Anchor = Anchor.TopRight,
            Origin = Anchor.TopRight,
            X = -SettingControl.SideMargin,
            Text = Binding.Value.ToString(),
            CommitOnFocusLost = true
        };
        control.Add(textBox);
        textBox.OnCommit += (_, __) =>
        {
            Binding.Value = double.TryParse(textBox.Current.Value, out var o) ? o : 0;
            textBox.Current.Value = Binding.Value.ToString();
        };
    }
}