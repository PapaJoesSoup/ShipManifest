using System;
using System.Collections.Generic;
using ConnectedLivingSpace;
using ShipManifest.InternalObjects.Settings;
using ShipManifest.Modules;
using ShipManifest.Process;
using UnityEngine;

namespace ShipManifest.InternalObjects
{
  // ReSharper disable once InconsistentNaming
  internal static class SMHighlighter
  {
    #region Properties

    // WindowTransfer part mouseover vars
    internal static bool IsMouseOver;
    internal static TransferPump.TypeXfer MouseOverMode = TransferPump.TypeXfer.SourceToTarget;
    internal static Rect MouseOverRect = new Rect(0, 0, 0, 0);
    internal static Part MouseOverPart;
    internal static List<Part> MouseOverParts;
    internal static Part PrevMouseOverPart;
    internal static List<Part> PrevMouseOverParts;

    #endregion

    #region Methods

    internal static void ClearPartsHighlight(List<Part> parts)
    {
      if (parts == null) return;
      List<Part>.Enumerator list = parts.GetEnumerator();
      while (list.MoveNext())
      {
        ClearPartHighlight(list.Current);          
      }
      list.Dispose();
    }

    /// <summary>
    ///   Remove highlighting on a part.
    /// </summary>
    /// <param name="part">Part to remove highlighting from.</param>
    internal static void ClearPartHighlight(Part part)
    {
      if (part == null) return;
      if (part.HighlightActive) part.SetHighlightDefault();
    }

    /// <summary>
    ///   Removes Highlighting on parts belonging to the selected resource list.
    /// </summary>
    /// <param name="resourceParts"></param>
    internal static void ClearResourceHighlighting(List<Part> resourceParts)
    {
      if (resourceParts == null) return;
      List<Part>.Enumerator list = resourceParts.GetEnumerator();
      while (list.MoveNext())
      {
        ClearPartHighlight(list.Current);
      }
      list.Dispose();
      if (!CurrSettings.EnableCls || !CurrSettings.EnableClsHighlighting || !SMAddon.GetClsVessel()) return;
      if (resourceParts.Count > 0 && resourceParts[0] != null)
        SMAddon.ClsAddon.Vessel.Highlight(false);
    }

    internal static void SetPartsHighlight(List<Part> parts, Color color, bool force = false)
    {
      try
      {
        List<Part>.Enumerator list = parts.GetEnumerator();
        while (list.MoveNext())
        {
          if (list.Current == null) continue;
          if (!list.Current.HighlightActive || force)
          SetPartHighlight(list.Current, color);
        }
        list.Dispose();
      }
      catch (Exception ex)
      {
        SmUtils.LogMessage(
          $" in  SetPartsHighlight.  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}", SmUtils.LogType.Error, true);
      }
    }

    internal static void SetPartHighlight(Part part, Color color)
    {
      if (part == null) return;
      try
      {
        if (!part.HighlightActive)
          part.SetHighlight(true, false);
        part.highlightType = Part.HighlightType.AlwaysOn;
        part.SetHighlightColor(color);
      }
      catch (Exception ex)
      {
        SmUtils.LogMessage(
          $" in  SetPartHighlight.  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}", SmUtils.LogType.Error, true);
      }
    }

    internal static void MouseOverHighlight()
    {
      // Supports Transfer Window vessel/part and Control window part Highlighting....
      RevertMouseOverHighlight();
      if (MouseOverPart == null && MouseOverParts != null)
      {
        MouseOverHighlight(MouseOverParts);
        PrevMouseOverParts = MouseOverParts;
        MouseOverParts = null;
      }
      else if (MouseOverPart != null && MouseOverParts == null)
      {
        MouseOverHighlight(MouseOverPart);
        PrevMouseOverPart = MouseOverPart;
        MouseOverPart = null;
      }
      else
      {
        PrevMouseOverPart = MouseOverPart = null;
      }
    }

    internal static void MouseOverHighlight(Part part)
    {
      string step = "begin";
      try
      {
        step = "inside box - Part Selection?";
        SetPartHighlight(part, SMSettings.Colors[CurrSettings.MouseOverColor]);
        EdgeHighight(part, true);
        if (!IsMouseOver) PrevMouseOverPart = null;
      }
      catch (Exception ex)
      {
        SmUtils.LogMessage($" in SMHighlighter.MouseOverHighlight at step {step}.  Error:  {ex}",
          SmUtils.LogType.Error, true);
      }
    }

    internal static void MouseOverHighlight(List<Part> parts)
    {
        SetPartsHighlight(parts, SMSettings.Colors[CurrSettings.MouseOverColor]);
        EdgeHighight(parts, true);
    }

    public static void SetMouseOverData(Rect rect, float scrollY, float scrollX, float height, ModDockedVessel vessel, Vector2 mouseposition)
    {
      // This must run during onGUI
      // Note rect.x and rect.y, as well as mouseposition.x and mouseposition.y are with respect to the container withing the scrollviewer.
      // this means both need to be aware of the viewable box boundaries 
      if (mouseposition.y - scrollY < 0 || mouseposition.y - scrollY > height) return;
      IsMouseOver = true;
      MouseOverRect = new Rect(rect.x - scrollX, rect.y - scrollY, rect.width, rect.height);
      MouseOverPart = null;
      MouseOverParts = vessel.VesselParts;
    }

    public static void SetMouseOverData(Rect rect, float scrollY, float scrollX, float height, Part part, Vector2 mouseposition)
    {
      // This must run during onGUI
      if (mouseposition.y - scrollY < 0 || mouseposition.y - scrollY > height) return;
      IsMouseOver = true;
      MouseOverRect = new Rect(rect.x - scrollX, rect.y - scrollY, rect.width, rect.height);
      MouseOverPart = part;
      MouseOverParts = null;
    }

    internal static void RevertMouseOverHighlight()
    {
      // Lets get the right color...

      if (PrevMouseOverParts != null)
      {
        // Only vessel Transfers have multiple parts highlighted  so clear is the default.
        // now lets set the part colors.
        ClearPartsHighlight(PrevMouseOverParts);;
        EdgeHighight(PrevMouseOverParts, false);

        // now set selected part colors...
        if (!CurrSettings.OnlySourceTarget && SMAddon.SmVessel.SelectedResourcesParts != null)
          SetPartsHighlight(SMAddon.SmVessel.SelectedResourcesParts, SMSettings.Colors[CurrSettings.ResourcePartColor], true);
        if (SMAddon.SmVessel.SelectedPartsSource != null)
          SetPartsHighlight(SMAddon.SmVessel.SelectedPartsSource, SMSettings.Colors[CurrSettings.SourcePartColor],true);
        if (SMAddon.SmVessel.SelectedPartsTarget != null)
        { 
          if (SMConditions.IsClsEnabled() && SMConditions.GetTransferMode() == SMConditions.TransferMode.Crew)
            SetPartsHighlight(SMAddon.SmVessel.SelectedPartsTarget, SMSettings.Colors[CurrSettings.TargetPartCrewColor], true);
          else
            SetPartsHighlight(SMAddon.SmVessel.SelectedPartsTarget, SMSettings.Colors[CurrSettings.TargetPartColor], true);        
        }

        if (!IsMouseOver) PrevMouseOverParts = null;
      }

      if (PrevMouseOverPart != null)
      {
        string strColor = GetPartHighlightColor();
        Color partColor = SMSettings.Colors[strColor];

        // Now lets set the part color
        if (partColor == Color.clear)
        {
          ClearPartHighlight(PrevMouseOverPart);
          EdgeHighight(PrevMouseOverPart, false);
          if(!IsMouseOver) PrevMouseOverPart = null;
          return;
        }
        SetPartHighlight(PrevMouseOverPart, partColor);
        EdgeHighight(PrevMouseOverPart, true, strColor);
        if (!IsMouseOver) PrevMouseOverPart = null;
      }
    }

    private static string GetPartHighlightColor()
    {
      string strColor = "clear";
      // Here, we need to also account for a part selected in the Control window.
      // so we can have a part revert to nothing...
      //if (PrevMouseOverPart == MouseOverPart || PrevMouseOverPart == null) return strColor;
      if (SMAddon.SmVessel.SelectedPartsSource.Contains(PrevMouseOverPart))
        strColor = CurrSettings.SourcePartColor;
      else if (SMAddon.SmVessel.SelectedPartsTarget.Contains(PrevMouseOverPart))
      {
        if (SMConditions.GetSelectedResourceType(SMAddon.SmVessel.SelectedResources) == SMConditions.ResourceType.Crew && SMConditions.IsClsHighlightingEnabled())
        {
          strColor = CurrSettings.TargetPartCrewColor;
        }
        else
        {
          strColor = CurrSettings.TargetPartColor;
        }
      }
      else if (SMAddon.SmVessel.SelectedResourcesParts.Contains(PrevMouseOverPart) && !CurrSettings.OnlySourceTarget)
      {
        strColor = SMConditions.IsClsHighlightingEnabled() ? "green" : "yellow";
      }
      return strColor;
    }

    internal static void EdgeHighight(Part part, bool enable, string color = null)
    {
      if (!CurrSettings.EnableEdgeHighlighting) return;
      if (enable)
      {
        if (string.IsNullOrEmpty(color))
          color = CurrSettings.MouseOverColor;
        part.highlighter.SeeThroughOn();
        part.highlighter.ConstantOnImmediate(SMSettings.Colors[color]);
      }
      else
      {
        part.highlighter.SeeThroughOff();
        part.highlighter.ConstantOffImmediate();
      }
    }

    internal static void EdgeHighight(List<Part> parts, bool enable, string color = null)
    {
      if (!CurrSettings.EnableEdgeHighlighting) return;
      List<Part>.Enumerator list = parts.GetEnumerator();
      while (list.MoveNext())
      {
        if (list.Current == null) continue;
        if (enable)
        {
          if (string.IsNullOrEmpty(color))
            color = CurrSettings.MouseOverColor;
          list.Current.highlighter.SeeThroughOn();
          list.Current.highlighter.ConstantOnImmediate(SMSettings.Colors[color]);
        }
        else
        {
          list.Current.highlighter.SeeThroughOff();
          list.Current.highlighter.ConstantOffImmediate();
        }
      }
      list.Dispose();
    }

    internal static void HighlightClsVessel(bool enabled, bool force = false)
    {
      try
      {
        if (SMAddon.ClsAddon.Vessel == null)
          SMAddon.UpdateClsSpaces();
        if (SMAddon.ClsAddon.Vessel == null) return;
        List<ICLSSpace>.Enumerator spaces = SMAddon.ClsAddon.Vessel.Spaces.GetEnumerator();
        while (spaces.MoveNext())
        {
          if (spaces.Current == null) continue;
          List<ICLSPart>.Enumerator parts = spaces.Current.Parts.GetEnumerator();
          while (parts.MoveNext())
          {
            parts.Current?.Highlight(enabled, force);
          }
          parts.Dispose();
        }
        spaces.Dispose();
      }
      catch (Exception ex)
      {
        if (!SMAddon.FrameErrTripped)
        {
          SmUtils.LogMessage(
            $" in HighlightCLSVessel (repeating error).  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}", SmUtils.LogType.Error, true);
          SMAddon.FrameErrTripped = true;
        }
      }
    }

    // This method is expensive.  Refactor to consume less CPU.
    internal static void Update_Highlighter()
    {
      string step = "";
      try
      {
        // Do we even want to highlight?
        if (!CurrSettings.EnableHighlighting) return;
        step = "Showhipmanifest = true";
        if (!SMConditions.CanShowShipManifest()) return;
        //step = "Clear old highlighting";
        // Clear Highlighting on everything, start fresh
        EdgeHighight(SMAddon.SmVessel.Vessel.parts, false);
        ClearPartsHighlight(SMAddon.SmVessel.Vessel.parts);

        if (SMAddon.SmVessel.SelectedResources != null && SMAddon.SmVessel.SelectedResources.Count > 0)
        {
          step = "Set non selected resource part color";
          Color resourcePartColor = SMSettings.Colors[CurrSettings.ResourcePartColor];
          Color targetPartColor = SMSettings.Colors[CurrSettings.TargetPartColor];

          // If resource is Crew, and we the settings have enabled CLS highlighting support, use CLS colours
          if (SMConditions.IsClsHighlightingEnabled())
          {
            step = "Highlight CLS vessel";
            HighlightClsVessel(true, true);

            // Turn off the source and target cls highlighting.  We are going to replace it.
            SMAddon.SmVessel.ClsPartSource?.Highlight(false, true);
            SMAddon.SmVessel.ClsPartTarget?.Highlight(false, true);

            // Override part colors to match CLS
            resourcePartColor = SMSettings.Colors[CurrSettings.ClsSpaceColor];
            targetPartColor = SMSettings.Colors[CurrSettings.TargetPartCrewColor];
          }

          // Highlight all parts containing the resource
          step = "Set Resource Part Colors";
          if (!CurrSettings.OnlySourceTarget)
          {
            SetPartsHighlight(SMAddon.SmVessel.SelectedResourcesParts, resourcePartColor);
          }

          // Highlight the Source and Target parts (if selected)
          step = "Set Selected Part Colors";
          SetPartsHighlight(SMAddon.SmVessel.SelectedPartsSource, SMSettings.Colors[CurrSettings.SourcePartColor], true);
          SetPartsHighlight(SMAddon.SmVessel.SelectedPartsTarget, targetPartColor, true);
        }
      }
      catch (Exception ex)
      {
        if (!SMAddon.FrameErrTripped)
        {
          SmUtils.LogMessage($" in SMHighlighter.UpdateHighlighting (repeating error).  Error in step:  {step}.  Error:  {ex.Message}\n\n{ex.StackTrace}",
            SmUtils.LogType.Error, true);
          SMAddon.FrameErrTripped = true;
        }
      }
    }

    internal static Color GetHighlightColor(Part part, out string colorstring)
    {
      colorstring = "clear";
      if (SMPart.IsSource(part)) colorstring = CurrSettings.SourcePartColor;
      else if (SMPart.IsTarget(part)) colorstring = SMPart.IsCrew(part)
        ? SMPart.IsClsTarget(part) ? CurrSettings.TargetPartCrewColor : CurrSettings.TargetPartColor
        : CurrSettings.TargetPartColor;
      else if (SMPart.IsSelected(part)) colorstring = CurrSettings.ResourcePartColor;
      return SMSettings.Colors[colorstring];
    }

    #endregion
  }
}
