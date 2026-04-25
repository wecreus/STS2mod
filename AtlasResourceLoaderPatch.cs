using System;
using System.Collections.Generic;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Screens;

namespace HighResolutionCards;

[HarmonyPatch(typeof(NInspectCardScreen), "UpdateCardDisplay")]
public static class NInspectCardScreen_UpdateCardDisplay_Patch
{
    private const string CardAtlasPrefix     = "res://images/atlases/card_atlas.sprites/";
    private const string TresSuffix          = ".tres";
    private const string PackedPortraitsBase = "res://images/packed/card_portraits/";

    private static readonly Dictionary<string, Texture2D?> Cache = new();

    private static readonly FieldInfo CardField =
        AccessTools.Field(typeof(NInspectCardScreen), "_card");

    // BetaArt interop: resolved once on first use, null if BetaArt is not loaded.
    private static bool _betaArtResolved;
    private static HashSet<string>? _betaEnabled;

    public static void Postfix(NInspectCardScreen __instance)
    {
        try
        {
            var cardNode = CardField.GetValue(__instance) as NCard;
            if (cardNode?.Model == null) return;
            ApplyHighRes(cardNode);
        }
        catch (Exception e)
        {
            GD.PrintErr($"[HighResolutionCards] UpdateCardDisplay patch failed: {e}");
        }
    }

    private static void ApplyHighRes(NCard cardNode)
    {
        var model = cardNode.Model;

        // If BetaArt is active for this card let it own the texture; skip high-res.
        if (IsBetaArtActive(model.BetaPortraitPath)) return;

        // BetaPortraitPath is always the /beta/ variant; strip it to get the normal atlas path.
        // For cards without beta art the path has no /beta/ segment and is returned unchanged.
        string atlasPath = model.BetaPortraitPath.Replace("/beta/", "/");

        if (!atlasPath.StartsWith(CardAtlasPrefix, StringComparison.Ordinal) ||
            !atlasPath.EndsWith(TresSuffix, StringComparison.Ordinal))
            return;

        string spriteName = atlasPath.Substring(
            CardAtlasPrefix.Length,
            atlasPath.Length - CardAtlasPrefix.Length - TresSuffix.Length);

        if (!Cache.TryGetValue(spriteName, out var texture))
        {
            string highResPath = $"{PackedPortraitsBase}{spriteName}.png";
            texture = ResourceLoader.Exists(highResPath)
                ? ResourceLoader.Load<Texture2D>(highResPath, null, ResourceLoader.CacheMode.Reuse)
                : null;
            Cache[spriteName] = texture;
        }

        if (texture == null) return;

        bool isAncient   = model.Rarity == CardRarity.Ancient;
        var portraitRect = cardNode.GetNodeOrNull<TextureRect>(isAncient ? "%AncientPortrait" : "%Portrait");
        if (portraitRect != null)
            portraitRect.Texture = texture;
    }

    // Checks BetaArt.BetaArtState.BetaEnabled at runtime without a compile-time dependency.
    // BetaEnabled is keyed by BetaPortraitPath, same as the string we pass in.
    private static bool IsBetaArtActive(string betaPortraitPath)
    {
        if (!_betaArtResolved)
        {
            _betaArtResolved = true;
            var t = Type.GetType("BetaArt.BetaArtState, BetaArt");
            _betaEnabled = t?.GetField("BetaEnabled",
                    BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
                ?.GetValue(null) as HashSet<string>;
        }
        return _betaEnabled?.Contains(betaPortraitPath) == true;
    }
}
