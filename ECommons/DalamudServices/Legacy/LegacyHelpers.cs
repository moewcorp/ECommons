using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Gui;
using Dalamud.Game.Text;
using Dalamud.Plugin.Services;

namespace ECommons.DalamudServices.Legacy;

public static class LegacyHelpers
{
    public static void PrintChat(this ChatGui chatGui, XivChatEntry entry) => Svc.Chat.PrintChat(entry);

    public static void SetTarget(this ITargetManager targetManager, GameObject obj)
    {
        targetManager.Target = obj;
    }
}
