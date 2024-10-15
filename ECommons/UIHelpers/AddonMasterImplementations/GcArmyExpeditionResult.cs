﻿using FFXIVClientStructs.FFXIV.Client.UI;

namespace ECommons.UIHelpers.AddonMasterImplementations;
public partial class AddonMaster
{
    public unsafe class GcArmyExpeditionResult : AddonMasterBase<AddonGcArmyExpeditionResult>
    {
        public GcArmyExpeditionResult(nint addon) : base(addon) { }
        public GcArmyExpeditionResult(void* addon) : base(addon) { }

        public void Complete() => ClickButtonIfEnabled(Addon->CompleteButton);
    }
}