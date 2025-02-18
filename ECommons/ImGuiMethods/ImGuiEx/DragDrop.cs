﻿using Dalamud.Interface.Utility;
using Dalamud.Interface;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using ECommons.Logging;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.ConstrainedExecution;

namespace ECommons.ImGuiMethods;
public static unsafe partial class ImGuiEx
{
    public class RealtimeDragDrop<T>
    {
        public RealtimeDragDrop(string dragDropId, Func<T, string> getUniqueId, bool smallButton = false)
        {
            this.DragDropID = dragDropId;
            this.GetUniqueId = getUniqueId;
            this.Small = smallButton;
        }

        List<(Vector2 RowPos, Vector2 ButtonPos, Action BeginDraw, Action AcceptDraw)> MoveCommands = [];
        Vector2 InitialDragDropCurpos;
        Vector2 ButtonDragDropCurpos;
        string DragDropID;
        string? CurrentDrag = null;
        Func<T, string> GetUniqueId;
        bool Small = false;

        /// <summary>
        /// Step 1. Call this before table begins.
        /// </summary>
        public void Begin()
        {
            MoveCommands.Clear();
        }

        /// <summary>
        /// Step 2. Call this in the beginning of table's row (first column). This function just stores cursor.
        /// </summary>
        public void NextRow()
        {
            InitialDragDropCurpos = ImGui.GetCursorPos();
        }

        /// <summary>
        /// Step 3. Call this where you want your button be.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="list"></param>
        /// <param name="targetPosition"></param>
        public void DrawButtonDummy(T item, IList<T> list, int targetPosition)
        {
            void executeMove(string x)
            {
                GenericHelpers.MoveItemToPosition<T>(list, (s) => GetUniqueId(s) == x, targetPosition);
            }
            DrawButtonDummy(GetUniqueId(item), executeMove);
        }

        /// <inheritdoc cref="DrawButtonDummy(T, IList{T}, int)"/>
        public void DrawButtonDummy(string uniqueId, IList<T> list, int targetPosition)
        {
            void executeMove(string x)
            {
                GenericHelpers.MoveItemToPosition<T>(list, (s) => GetUniqueId(s) == x, targetPosition);
            }
            DrawButtonDummy(uniqueId, executeMove);
        }

        /// <inheritdoc cref="DrawButtonDummy(T, IList{T}, int)"/>
        public void DrawButtonDummy(string uniqueId, Action<string> onAcceptDragDropPayload)
        {
            ImGui.PushFont(UiBuilder.IconFont);
            ButtonDragDropCurpos = ImGui.GetCursorPos();
            var size = ImGuiHelpers.GetButtonSize(FontAwesomeIcon.ArrowsUpDownLeftRight.ToIconString());
            if(Small) size = size with { Y = ImGui.CalcTextSize(FontAwesomeIcon.ArrowsUpDownLeftRight.ToIconString()).Y };
            ImGui.Dummy(size);
            ImGui.PopFont();
            EndRow(uniqueId, onAcceptDragDropPayload);
        }

        /// <summary>
        /// Call this after calling TableNextRow to color the row that is being moved. Not mandatory.
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <returns>Whether row was colored</returns>
        public bool SetRowColor(string uniqueId)
        {
            var ret = false;
            if(CurrentDrag == uniqueId)
            {
                var col = GradientColor.Get(EColor.Green, EColor.Green with { W = EColor.Green.W / 4 }, 500).ToUint();
                ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg0, col);
                ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg1, col);
                ret = true;
            }
            return ret;
        }

        private void EndRow(string uniqueId, Action<string> onAcceptDragDropPayload)
        {
            MoveCommands.Add((InitialDragDropCurpos, ButtonDragDropCurpos, sourceAction, targetAction));

            void sourceAction()
            {
                ImGui.PushFont(UiBuilder.IconFont);
                var btxt = $"{FontAwesomeIcon.ArrowsUpDownLeftRight.ToIconString()}##{DragDropID}Move{uniqueId}";
                if(Small)
                {
                    ImGui.SmallButton(btxt);
                }
                else
                {
                    ImGui.Button(btxt);
                }
                ImGui.PopFont();
                if(ImGui.IsItemHovered())
                {
                    ImGui.SetMouseCursor(ImGuiMouseCursor.ResizeAll);
                }
                if(ImGui.BeginDragDropSource(ImGuiDragDropFlags.SourceNoPreviewTooltip))
                {
                    ImGuiDragDrop.SetDragDropPayload(DragDropID, uniqueId);
                    CurrentDrag = uniqueId;
                    InternalLog.Verbose($"DragDropSource = {uniqueId}");
                    ImGui.EndDragDropSource();
                }
                else if(CurrentDrag == uniqueId)
                {
                    InternalLog.Verbose($"Current drag reset!");
                    CurrentDrag = null;
                }
            }

            void targetAction()
            {
                if(ImGui.BeginDragDropTarget())
                {
                    if(ImGuiDragDrop.AcceptDragDropPayload(DragDropID, out var payload, ImGuiDragDropFlags.AcceptBeforeDelivery | ImGuiDragDropFlags.AcceptNoDrawDefaultRect))
                    {
                        onAcceptDragDropPayload(payload);
                    }
                    ImGui.EndDragDropTarget();
                }
            }
        }

        public bool AcceptPayload([NotNullWhen(true)]out string? uniqueId, ImGuiDragDropFlags flags = ImGuiDragDropFlags.None)
        {
            uniqueId = null;
            if(ImGui.BeginDragDropTarget())
            {
                if(ImGuiDragDrop.AcceptDragDropPayload(this.DragDropID, out var payload, flags))
                {
                    uniqueId = payload;
                }
                ImGui.EndDragDropTarget();
            }
            return uniqueId != null;
        }

        /// <summary>
        /// Step 4. Call this outside of the table.
        /// </summary>
        /// <param name="numRows">How many lines is in your biggest row.</param>
        public void End(int numRows = 1)
        {
            var cur = ImGui.GetCursorPos();
            foreach(var x in MoveCommands)
            {
                ImGui.SetCursorPos(x.ButtonPos);
                x.BeginDraw();
                x.AcceptDraw();
                ImGui.SetCursorPos(x.RowPos);
                var height = ImGui.GetFrameHeight() * numRows + ImGui.GetStyle().ItemInnerSpacing.Y - numRows;
                ImGui.Dummy(new Vector2(ImGui.GetContentRegionAvail().X, height));
                x.AcceptDraw();
            }
            ImGui.SetCursorPos(cur);
        }
    }
}
