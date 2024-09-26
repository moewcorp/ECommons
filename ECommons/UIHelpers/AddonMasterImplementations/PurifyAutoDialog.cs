﻿using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Memory;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.GeneratedSheets;

namespace ECommons.UIHelpers.AddonMasterImplementations;
public partial class AddonMaster
{
    public unsafe class PurifyAutoDialog : AddonMasterBase<AtkUnitBase>
    {
        public PurifyAutoDialog(nint addon) : base(addon) { }

        public PurifyAutoDialog(void* addon) : base(addon) { }

        public AtkComponentButton* CancelExitButton => Addon->GetButtonNodeById(16);
        public SeString CancelExitButtonSeString => MemoryHelper.ReadSeString(&CancelExitButton->UldManager.SearchNodeById(2)->GetAsAtkTextNode()->NodeText);
        public string CancelExitButtonText => CancelExitButtonSeString.ExtractText();
        public bool PurificationActive => Svc.Data.GetExcelSheet<Addon>()!.GetRow(3868)!.Text.RawString.Equals(CancelExitButtonText);
        public bool PurificationInactive => Svc.Data.GetExcelSheet<Addon>()!.GetRow(3869)!.Text.RawString.Equals(CancelExitButtonText);

        public void CancelExit() => ClickButtonIfEnabled(CancelExitButton);
    }
}
