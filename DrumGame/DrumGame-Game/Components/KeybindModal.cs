using System;
using System.Linq;
using DrumGame.Game.Commands;
using DrumGame.Game.Components.Basic;
using DrumGame.Game.Interfaces;
using DrumGame.Game.Midi;
using DrumGame.Game.Modals;
using DrumGame.Game.Stores;
using DrumGame.Game.Utils;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Input.States;
using osu.Framework.Localisation;
using osuTK.Input;

namespace DrumGame.Game.Components;

public class KeybindModalCloseEvent
{
    public bool NewBindRequested;
    public bool Changed;
}
public class KeybindModal : CompositeDrawable, IModal
{
    public override bool HandleNonPositionalInput => true;
    public override bool RequestsFocus => true;
    public override bool AcceptsFocus => true;

    protected override void LoadComplete()
    {
        base.LoadComplete();
        GetContainingInputManager().ChangeFocus(this);
    }

    public const float Spacing = 8f;
    int bindTarget;
    CommandInfo command;
    public Action<KeybindModalCloseEvent> Close;
    [Resolved] CommandController controller { get; set; }
    [Resolved] KeybindConfigManager ConfigManager { get; set; }
    FillFlowContainer newBindingContainer;
    KeyCombo currentCombo;
    public Container Center;

    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.Both;
        AddInternal(new ModalBackground(() => Close?.Invoke(new KeybindModalCloseEvent())));
        var outer = new MouseBlockingContainer
        {
            RelativeSizeAxes = Axes.X,
            Width = 0.5f,
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            AutoSizeAxes = Axes.Y
        };
        outer.Add(new Box
        {
            RelativeSizeAxes = Axes.Both,
            Colour = DrumColors.DarkBorder
        });
        AddInternal(outer);
        outer.Add(Center = new Container
        {
            RelativeSizeAxes = Axes.X,
            Padding = new MarginPadding(2)
        });
        Center.Add(new Box
        {
            RelativeSizeAxes = Axes.Both,
            Colour = DrumColors.DarkBackground
        });
        var y = Spacing;
        Center.Add(new MarkupText($"Editing key binding for <command>{command.Name}</>")
        {
            Y = y,
            Font = KeybindEditor.Font.With(size: 24),
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre
        });
        y += 24 + Spacing;
        if (bindTarget > -1)
        {
            Center.Add(new KeybindModalButton
            {
                Text = "New Binding",
                Y = y,
                Action = () =>
                {
                    Close?.Invoke(new KeybindModalCloseEvent() { NewBindRequested = true });
                },
                BackgroundColour = DrumColors.DarkGreen,
                AutoSizeAxes = Axes.Y,
                Width = 150,
                X = Spacing
            });
            Center.Add(new KeybindModalButton
            {
                Text = "Remove Binding",
                Y = y,
                Action = () =>
                {
                    ConfigManager.SetBinding(command, bindTarget, InputKey.None, true);
                    Close?.Invoke(new KeybindModalCloseEvent() { Changed = true });
                },
                BackgroundColour = DrumColors.DarkRed,
                AutoSizeAxes = Axes.Y,
                Width = 150,
                X = Spacing * 2 + 150
            });
            y += 24 + Spacing;
            Center.Add(new SpriteText
            {
                Text = $"Current Binding: ",
                Y = y,
                X = Spacing + 4,
                Font = KeybindEditor.Font
            });
            var hotkeyContainer = new FillFlowContainer
            {
                X = Spacing * 2 + 140,
                Direction = FillDirection.Horizontal,
                AutoSizeAxes = Axes.X,
                Y = y + KeybindEditor.Font.Size / 2,
                Origin = Anchor.CentreLeft
            };
            HotkeyDisplay.RenderHotkey(hotkeyContainer, command.Bindings[bindTarget]);
            Center.Add(hotkeyContainer);
            y += 22 + Spacing;
        }
        Center.Add(new SpriteText
        {
            Text = $"Press key(s) to set new binding:",
            Y = y,
            Font = KeybindEditor.Font,
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre
        });
        y += KeybindEditor.Font.Size + Spacing;
        Center.Add(newBindingContainer = new FillFlowContainer
        {
            Direction = FillDirection.Horizontal,
            AutoSizeAxes = Axes.X,
            Y = y + 8,
            Origin = Anchor.TopCentre,
            Anchor = Anchor.TopCentre
        });
        newBindingContainer.Add(new SpriteText
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Text = "Press a key...",
            Font = KeybindEditor.Font.With(size: 16)
        });
        y += 16 + Spacing;
        Center.Add(new SpriteText
        {
            Text = $"Enter to save, Escape to close",
            Y = y,
            Font = KeybindEditor.Font,
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre
        });
        y += KeybindEditor.Font.Size + Spacing;
        var parameterTypes = controller.ParameterInfo[(int)command.Command]?.Types;
        if (parameterTypes != null && parameterTypes.Length > 0 && bindTarget == -1)
        {
            ParameterBoxes = new ParameterBox[parameterTypes.Length];
            var w = 150;
            var width = parameterTypes.Length * w + parameterTypes.Length * 4;
            var h = 0f;
            var box = new Box
            {
                Colour = DrumColors.DarkBorder,
                Width = width,
                Y = y,
                Origin = Anchor.TopCentre,
                Anchor = Anchor.TopCentre,
            };
            Center.Add(box);
            y += 2;
            for (var i = 0; i < parameterTypes.Length; i++)
            {
                var currentV = command.Parameters?[i];
                Center.Add(ParameterBoxes[i] = new ParameterBox(parameterTypes[i].GetNullableType(), i, currentV)
                {
                    Y = y,
                    X = -width / 2 + i * (w + 4) + 2,
                    Anchor = Anchor.TopCentre,
                    Width = w
                });
                h = Math.Max(h, ParameterBoxes[i].Height);
            }
            box.Height = h + 4;
            y += h + 4;
        }
        Center.Height = y + Spacing / 2 + Spacing;
    }

    ParameterBox[] ParameterBoxes;

    class ParameterBox : CompositeDrawable
    {
        IDrawableField Field;
        Type Type;
        public object Value
        {
            get
            {
                var v = Field.Value;
                if (v == null) return null;
                if (v.GetType().IsAssignableTo(Type)) return v;
                return CommandParameters.ParseOrDefault((string)v, Type);
            }
        }
        public ParameterBox(Type type, int i, object currentValue)
        {
            Type = type;
            var fieldConfig = FieldConfigBase.GetConfigFor(type);
            if (currentValue != null)
            {
                if (fieldConfig is StringFieldConfig) fieldConfig.SetDefault(currentValue.ToString());
                else fieldConfig.SetDefault(currentValue);
            }
            fieldConfig.Tooltip = $"Parameter {i + 1} ({CommandParameters.TypeString(type)})";
            Field = fieldConfig.Render(null);
            var d = (Drawable)Field;
            Height = d.Height;
            AddInternal(d);
        }
    }

    public KeybindModal(CommandInfo command, int bindTarget)
    {
        this.bindTarget = bindTarget;
        this.command = command;
        DrumMidiHandler.AddNoteHandler(OnMidiNote);
    }
    public void UpdateBindingContainer(byte midiNote)
    {
        newBindingContainer.Clear();
        var mainKey = KeyCombination.FromMidiKey((MidiKey)midiNote);
        currentCombo = new KeyCombo(mainKey);
        HotkeyDisplay.RenderHotkey(newBindingContainer, currentCombo);
    }
    public void UpdateBindingContainer(InputState state)
    {
        newBindingContainer.Clear();
        var modifiers = ModifierKey.None;
        var keyboard = state.Keyboard;
        if (keyboard.ControlPressed) modifiers |= ModifierKey.Ctrl;
        if (keyboard.AltPressed) modifiers |= ModifierKey.Alt;
        if (keyboard.ShiftPressed) modifiers |= ModifierKey.Shift;
        var mainKey = InputKey.None;
        foreach (var key in state.Keyboard.Keys)
        {
            if (key != Key.ShiftLeft && key != Key.ShiftRight && key != Key.AltLeft && key != Key.AltRight &&
                key != Key.ControlLeft && key != Key.ControlRight)
            {
                mainKey = KeyCombination.FromKey(key);
                break;
            }
        }
        currentCombo = new KeyCombo(modifiers, mainKey);
        HotkeyDisplay.RenderHotkey(newBindingContainer, currentCombo);
    }

    public Action CloseAction { get; set; }

    protected override bool OnKeyDown(KeyDownEvent e)
    {
        if (e.Key == Key.Escape)
        {
            Close?.Invoke(new KeybindModalCloseEvent());
            return true;
        }
        else if (e.Key == Key.Enter || e.Key == Key.KeypadEnter)
        {
            if (currentCombo.Key != InputKey.None)
            {
                var commandE = command.Command;
                var parameterTypes = controller.ParameterInfo[(int)commandE]?.Types;
                if (parameterTypes != null && parameterTypes.Length > 0 && bindTarget == -1)
                {
                    var parameters = ParameterBoxes.Map(e => e.Value);
                    if (parameters.All(e => e == null)) goto UpdateExisting;

                    var parameterCommands = controller.ParameterCommands[(int)commandE];
                    if (parameterCommands != null)
                    {
                        foreach (var c in parameterCommands)
                        {
                            if (c.ParametersEqual(parameters))
                            {
                                command = c;
                                // we found a matching parameter command, so just add the new binding to that
                                goto UpdateExisting;
                            }
                        }
                    }
                    // no existing command found, so we register a new one
                    // this logic is basically identical to what is used in KeybindConfigManager for loading parameter hotkeys
                    ConfigManager.AddCustomCommand(commandE, parameters, currentCombo);
                    goto Exit;
                }
            UpdateExisting:
                ConfigManager.SetBinding(command, bindTarget, currentCombo, true);
            Exit:
                Close?.Invoke(new KeybindModalCloseEvent() { Changed = true });
                return true;
            }
        }
        UpdateBindingContainer(e.CurrentState);
        return true;
    }

    protected override void Dispose(bool isDisposing)
    {
        DrumMidiHandler.RemoveNoteHandler(OnMidiNote);
        base.Dispose(isDisposing);
    }
    bool OnMidiNote(MidiNoteOnEvent e)
    {
        UpdateBindingContainer(e.Note);
        return true;
    }
    public class KeybindModalButton : BasicButton
    {
        protected override SpriteText CreateText()
        {
            return new SpriteText
            {
                Depth = -1,
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
                Font = KeybindEditor.Font,
                Padding = new MarginPadding
                {
                    Top = 2f,
                    Bottom = 2,
                    Left = Spacing,
                    Right = Spacing
                }
            };
        }
    }
}
