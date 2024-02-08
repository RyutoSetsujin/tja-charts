using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DrumGame.Game.Beatmaps.Loaders;
using DrumGame.Game.Channels;
using DrumGame.Game.Commands;
using DrumGame.Game.Notation;
using DrumGame.Game.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using NuGet.Protocol;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Logging;

namespace DrumGame.Game.Stores.Skins;

public static class SkinManager
{
    public static event Action SkinChanged; // triggered when skin is modified/reloaded and when different skin is selected

    // public static void ReloadSkin() => SkinChanged?.Invoke();
    static Bindable<string> Binding;

    static void ChangeSkin(Skin skin)
    {
        Util.Skin?.UnloadTextureStore();
        Util.Skin = skin;
        skin.LoadTextureStore();
    }

    public static void Initialize()
    {
        Binding = Util.ConfigManager.GetBindable<string>(DrumGameSetting.Skin);
        Binding.ValueChanged += e =>
        {
            var v = e.NewValue;
            if (Util.Skin?.Source == v) return;
            if (v == null)
            {
                ChangeSkin(LoadDefaultSkin());
                SkinChanged?.Invoke();
            }
            else
            {
                if (FileWatcher != null) SetHotWatcher(v);
                // note that if the skin fails to load, we don't change anything
                var newSkin = ParseSkin(v);
                if (newSkin != null)
                {
                    ChangeSkin(newSkin);
                    SkinChanged?.Invoke();
                }
            }
        };
        ChangeSkin(ParseSkin(Binding.Value) ?? LoadDefaultSkin());
        Util.CommandController.RegisterHandler(Command.ReloadSkin, ReloadSkin); // don't need to unregister
        Util.CommandController.RegisterHandler(Command.ExportCurrentSkin, ExportCurrentSkin); // don't need to unregister
    }

    public static IEnumerable<string> ListSkins()
    {
        var dir = Util.Resources.GetDirectory("skins");
        var subDirs = dir.GetDirectories();
        return dir.GetFiles("*.json")
            .Where(e => e.Name != "skin-schema.json")
            .Select(e => Path.GetFileNameWithoutExtension(e.Name))
            .Concat(subDirs.Select(e => e.Name));
    }

    public static void ReloadSkin() // this will also start the hot watcher
    {
        var v = Binding.Value;
        if (v != null)
        {
            var newSkin = ParseSkin(v);
            if (newSkin != null)
            {
                ChangeSkin(newSkin);
                SkinChanged?.Invoke();
                SetHotWatcher(v);
                Util.Palette.ShowMessage("Skin reloaded");
            }
        }
    }

    // costs ~16ms to parse skin first time
    // <1ms on second parse - this is likely because Newtonsoft has to generate a bunch of reflection code the first time
    public static Skin ParseSkin(string skinName)
    {
        // using var _ = Util.WriteTime();
        var path = FindSkin(skinName);
        if (path == null) return null;
        return SkinFromFile(path);
    }
    public static string FindSkin(string skinName)
    {
        if (string.IsNullOrWhiteSpace(skinName)) return null;
        var path = Util.Resources.GetAbsolutePath(Path.Join("skins", skinName));
        if (File.Exists(path)) return path;
        var json = path + ".json";
        if (File.Exists(json)) return json;
        if (Directory.Exists(path))
        {
            var subPath = Path.Join(path, "skin.json");
            if (File.Exists(subPath))
                return subPath;
            subPath = Path.Join(path, skinName + ".json");
            if (File.Exists(subPath))
                return subPath;
        }
        return null;
    }

    public static Skin LoadDefaultSkin()
    {
        var skin = new Skin();
        skin.LoadDefaults();
        return skin;
    }

    public static Skin SkinFromFile(string filename)
    {
        try
        {
            var text = File.ReadAllText(filename);
            var skin = JsonConvert.DeserializeObject<Skin>(text, new ColorHexConverter(), new DrumChannelConverter());
            skin.Source = filename;
            skin.SourceFolder = Path.GetDirectoryName(filename);
            skin.LoadDefaults();
            return skin;
        }
        catch (FileNotFoundException)
        {
            Logger.Log($"Failed to load skin, {filename} not found");
        }
        catch (Exception e)
        {
            Logger.Error(e, $"Failed to load skin {filename}");
        }
        Util.Palette?.ShowMessage("Failed to load skin. See log for details");
        return null;
    }

    static FileWatcher FileWatcher;
    static void SetHotWatcher(string skin)
    {
        var path = File.Exists(skin) ? skin : FindSkin(skin);
        if (path == null) return;
        if (FileWatcher == null)
        {
            FileWatcher = new FileWatcher(path);
            FileWatcher.Changed += ReloadSkin;
            FileWatcher.Register();
        }
        else
            FileWatcher.UpdatePath(path);
    }
    public static void SetHotWatcher(Skin skin) => SetHotWatcher(skin?.Source);

    public static void ExportCurrentSkin()
    {
        var skin = Util.Skin;
        var name = skin.Name ?? "Default";
        var exportName = Util.ToFilename($"export {name}", ".json");
        var outputPath = Util.Resources.GetAbsolutePath(Path.Join("skins", exportName));
        var settings = new JsonSerializerSettings
        {
            ContractResolver = new SkinContractResolver(),
            Converters = new JsonConverter[] { new ColorHexConverter() },
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
        };
        skin.GameVersion ??= Util.VersionString;
        skin.Comments ??= "This is an exported version of a skin. It may contain extra fields that are not needed.";
        var s = JsonConvert.SerializeObject(skin, settings);
        File.WriteAllText(outputPath, s);
        Util.RevealInFileExplorer(outputPath);
    }

    public class SkinChannelConverter : JsonConverter<Dictionary<DrumChannel, SkinNote>>
    {
        public override Dictionary<DrumChannel, SkinNote> ReadJson(JsonReader reader, Type objectType,
            Dictionary<DrumChannel, SkinNote> res, bool hasExistingValue, JsonSerializer serializer)
        {
            res ??= new Dictionary<DrumChannel, SkinNote>();

            reader.Read();
            while (reader.TokenType != JsonToken.EndObject)
            {
                var key = (string)reader.Value;
                var channel = BJsonNote.GetDrumChannel(key);
                reader.Read();
                if (res.TryGetValue(channel, out var note))
                    serializer.Populate(reader, note);
                else
                    res.Add(channel, serializer.Deserialize<SkinNote>(reader));
                reader.Read(); // not sure why it doesn't read the close object
            }

            return res;
        }

        public override void WriteJson(JsonWriter writer, Dictionary<DrumChannel, SkinNote> value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            foreach (var pair in value)
            {
                writer.WritePropertyName(BJsonNote.GetChannelString(pair.Key));
                serializer.Serialize(writer, pair.Value);
            }
            writer.WriteEndObject();
        }
    }

    public class SkinContractResolver : DefaultContractResolver
    {
        public SkinContractResolver()
        {
            NamingStrategy = new CamelCaseNamingStrategy();
        }
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var list = base.CreateProperties(type, memberSerialization);
            if (type == typeof(Skin))
            {
                list = list.Where(e => e.Writable).ToList();
                list.Add(new JsonProperty
                {
                    PropertyName = "$schema",
                    PropertyType = typeof(string),
                    DeclaringType = type,
                    ValueProvider = new SchemaValueProvider(),
                    Readable = true,
                    Writable = false,
                    ShouldSerialize = _ => true
                });
            }
            return list;
        }

        private class SchemaValueProvider : IValueProvider
        {
            public object GetValue(object target) => "skin-schema.json";
            public void SetValue(object target, object value) { }
        }
    }

    public class ColorHexConverter : JsonConverter<Colour4>
    {
        public override Colour4 ReadJson(JsonReader reader, Type objectType, Colour4 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var v = (string)reader.Value;
            if (v.StartsWith("rgb"))
            {
                var trim = v.AsSpan(3);
                if (trim.StartsWith("a")) trim = trim[1..];
                if (trim.StartsWith("(")) trim = trim[1..];
                if (trim.EndsWith(")")) trim = trim[..^1];
                var ind = trim.IndexOf(",");
                var r = byte.Parse(trim[..ind]);
                trim = trim[(ind + 1)..];
                ind = trim.IndexOf(",");
                var g = byte.Parse(trim[..ind]);
                trim = trim[(ind + 1)..];
                ind = trim.IndexOf(",");
                if (ind == -1)
                {
                    return new Colour4(r, g, byte.Parse(trim), 255);
                }
                else
                {
                    var b = byte.Parse(trim[..ind]);
                    trim = trim[(ind + 1)..];
                    return new Colour4(r, g, b, byte.Parse(trim));
                }
            }
            else return Colour4.FromHex(v);
        }

        public override void WriteJson(JsonWriter writer, Colour4 value, JsonSerializer serializer)
            => writer.WriteValue(value.ToHex());
    }
}