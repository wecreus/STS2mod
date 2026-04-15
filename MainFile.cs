using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;

namespace HighResolutionCards;

[ModInitializer(nameof(Initialize))]
public partial class MainFile : Node
{
    public const string ModId = "HighResolutionCards";

public static void Initialize()
    {
        Harmony harmony = new(ModId);
        harmony.PatchAll();
    }
}
