using DrumGame.Game.Commands;
using DrumGame.Game.Components.Basic;
using DrumGame.Game.Interfaces;
using DrumGame.Game.Utils;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;

namespace DrumGame.Game.Views.Settings;

public class SettingControl : BasicButton, IHasCommand
{
    public DrumScrollContainer ScrollContainer => Parent.Parent as DrumScrollContainer;

    string IHasMarkupTooltip.MarkupTooltip
    {
        get
        {
            if (Info.Tooltip != null && Command != Command.None)
                return $"{Info.Tooltip} - {IHasCommand.GetMarkupTooltip(Command)}";
            return Info?.Tooltip ?? IHasCommand.GetMarkupTooltip(Command);
        }
    }

    public Command Command { get; set; }

    protected override SpriteText CreateText()
    {
        return new SpriteText
        {
            Font = FrameworkFont.Regular.With(size: 24),
            X = SideMargin,
            Y = 3,
        };
    }
    public void Close()
    {
        Info.Close(this);
    }
    protected override void Dispose(bool isDisposing)
    {
        Info.Dispose();
        base.Dispose(isDisposing);
    }
    public const float SideMargin = 20;
    public SettingInfo Info;
    public SettingControl(SettingInfo info, bool even)
    {
        Info = info;
        Height = info.Height;
        RelativeSizeAxes = Axes.X;
        BackgroundColour = even ? DrumColors.RowHighlight : DrumColors.RowHighlightSecondary;
        Text = info.Label;
        info.Render(this);
    }
}
