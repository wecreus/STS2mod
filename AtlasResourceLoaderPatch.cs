using System;
using System.Collections.Generic;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;

namespace HighResolutionCards;

[HarmonyPatch(typeof(AtlasResourceLoader), nameof(AtlasResourceLoader._Load))]
public static class AtlasResourceLoader_Load_Patch
{
    private const string CardAtlasPrefix  = "res://images/atlases/card_atlas.sprites/";
    private const string TresSuffix       = ".tres";
    private const string PackedPortraitsBase = "res://images/packed/card_portraits/";

    // null entry = checked before, no packed image exists → fall through to atlas
    private static readonly Dictionary<string, Texture2D?> _cache = new();

    public static bool Prefix(AtlasResourceLoader __instance, ref Variant __result,
        string path, string originalPath, bool useSubThreads, int cacheMode)
    {
        try
        {
            if (!path.StartsWith(CardAtlasPrefix, StringComparison.Ordinal) ||
                !path.EndsWith(TresSuffix, StringComparison.Ordinal))
                return true;

            // "colorless/automation"  or  "silent/beta/blade_dance"  etc.
            string spriteName = path.Substring(
                CardAtlasPrefix.Length,
                path.Length - CardAtlasPrefix.Length - TresSuffix.Length);

            if (_cache.TryGetValue(spriteName, out Texture2D? cached))
            {
                if (cached != null)
                {
                    __result = cached;
                    return false;
                }
                return true;
            }

            string highResPath = $"{PackedPortraitsBase}{spriteName}.png";
            var texture = ResourceLoader.Load<Texture2D>(highResPath);
            _cache[spriteName] = texture;

            if (texture != null)
            {
                __result = texture;
                return false;
            }

            return true;
        }
        catch
        {
            return true;
        }
    }
}
