using System;
using System.Collections.Generic;
using ConnectedLivingSpace;
using HighlightingSystem;
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
    internal static TransferPump.TypePump MouseOverMode = TransferPump.TypePump.SourceToTarget;
    internal static Rect MouseOverRect = new Rect(0, 0, 0, 0);
    internal static Part MouseOverpart;
    internal static List<Part> MouseOverparts;

    #endregion

    #region Methods

    internal static void ClearPartsHighlight(List<Part> parts)
    {
      if (parts != null)
      {
        List<Part>.Enumerator list = parts.GetEnumerator();
        while (list.MoveNext())
        {
          ClearPartHighlight(list.Current);          
        }
      }
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
      if (resourceParts != null)
      {
        List<Part>.Enumerator list = resourceParts.GetEnumerator();
        while (list.MoveNext())
        {
          ClearPartHighlight(list.Current);
        }
        if (SMSettings.EnableCls && SMSettings.EnableClsHighlighting && SMAddon.GetClsVessel())
        {
          if (resourceParts.Count > 0 && resourceParts[0] != null)
            SMAddon.ClsAddon.Vessel.Highlight(false);
        }
      }
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
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(
          string.Format(" in  SetPartsHighlight.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), Utilities.LogType.Error, true);
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
        Utilities.LogMessage(
          string.Format(" in  SetPartHighlight.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), Utilities.LogType.Error, true);
      }
    }

    internal static void MouseOverHighlight(Part part)
    {
      string step = "begin";
      try
      {
        step = "inside box - Part Selection?";
        SetPartHighlight(part, SMSettings.Colors[SMSettings.MouseOverColor]);
        EdgeHighight(part, true);
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(string.Format(" in SMHighlighter.MouseOverHighlight at step {0}.  Error:  {1}", step, ex),
          Utilities.LogType.Error, true);
      }
    }

    internal static void MouseOverHighlight(List<Part> parts)
    {
      string step = "begin";
      try
      {
        step = "inside box - Part Selection?";
        SetPartsHighlight(parts, SMSettings.Colors[SMSettings.MouseOverColor]);
        EdgeHighight(parts, true);
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(string.Format(" in SMHighlighter.MouseOverHighlight at step {0}.  Error:  {1}", step, ex),
          Utilities.LogType.Error, true);
      }
    }

    internal static void EdgeHighight(Part part, bool enable, string color = null)
    {
      if (SMSettings.EnableEdgeHighlighting)
      {
        Highlighter highlighter = part.highlighter;
        if (enable)
        {
          if (string.IsNullOrEmpty(color))
            color = SMSettings.MouseOverColor;
          highlighter.SeeThroughOn();
          highlighter.ConstantOnImmediate(SMSettings.Colors[color]);
        }
        else
        {
          highlighter.SeeThroughOff();
          highlighter.ConstantOffImmediate();
        }
      }
    }

    internal static void EdgeHighight(List<Part> parts, bool enable, string color = null)
    {
      if (SMSettings.EnableEdgeHighlighting)
      {
        List<Part>.Enumerator list = parts.GetEnumerator();
        while (list.MoveNext())
        {
          if (list.Current == null) continue;
          if (enable)
          {
            if (string.IsNullOrEmpty(color))
              color = SMSettings.MouseOverColor;
            list.Current.highlighter.SeeThroughOn();
            list.Current.highlighter.ConstantOnImmediate(SMSettings.Colors[color]);
          }
          else
          {
            list.Current.highlighter.SeeThroughOff();
            list.Current.highlighter.ConstantOffImmediate();
          }
        }
      }
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
        }
      }
      catch (Exception ex)
      {
        if (!SMAddon.FrameErrTripped)
        {
          Utilities.LogMessage(
            string.Format(" in HighlightCLSVessel (repeating error).  Error:  {0} \r\n\r\n{1}", ex.Message,
              ex.StackTrace), Utilities.LogType.Error, true);
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
        if (!SMSettings.EnableHighlighting) return;
        step = "Showhipmanifest = true";
        if (!SMConditions.CanShowShipManifest()) return;
        //step = "Clear old highlighting";
        // Clear Highlighting on everything, start fresh
        EdgeHighight(SMAddon.SmVessel.Vessel.parts, false);
        ClearPartsHighlight(SMAddon.SmVessel.Vessel.parts);

        step = "Mouseover highlighting, if any";
        // Supports Transfer Window vessel/part Highlighting....
        if (IsMouseOver)
        {
          if (MouseOverRect.Contains(Event.current.mousePosition))
          {
            if (MouseOverpart == null && MouseOverparts != null)
              MouseOverHighlight(MouseOverparts);
            else if (MouseOverpart != null)
              MouseOverHighlight(MouseOverpart);
          }
          else
          {
            IsMouseOver = false;
            MouseOverpart = null;
            MouseOverparts = null;
          }
        }

        if (SMAddon.SmVessel.SelectedResources != null && SMAddon.SmVessel.SelectedResources.Count > 0)
        {
          // If Crew and cls, perform cls Highlighting
          if (SMConditions.IsClsHighlightingEnabled())
          {
            step = "Highlight CLS vessel";
            HighlightClsVessel(true, true);

            // Turn off the source and target cls highlighting.  We are going to replace it.
            if (SMAddon.SmVessel.ClsPartSource != null)
              SMAddon.SmVessel.ClsPartSource.Highlight(false, true);
            if (SMAddon.SmVessel.ClsPartTarget != null)
              SMAddon.SmVessel.ClsPartTarget.Highlight(false, true);
          }

          // Default is yellow
          step = "Set non selected resource part color";
          Color partColor = SMSettings.Colors[SMSettings.ResourcePartColor];

          // match color used by CLS if active
          if (SMAddon.SmVessel.SelectedResources.Contains(SMConditions.ResourceType.Crew.ToString()) &&
              SMSettings.EnableCls)
            partColor = Color.green;

          step = "Set Resource Part Colors";
          if (!SMSettings.OnlySourceTarget)
          {
            SetPartsHighlight(SMAddon.SmVessel.SelectedResourcesParts, partColor);
          }

          step = "Set Selected Part Colors";
          SetPartsHighlight(SMAddon.SmVessel.SelectedPartsSource, SMSettings.Colors[SMSettings.SourcePartColor], true);
          if (SMAddon.SmVessel.SelectedResources.Contains(SMConditions.ResourceType.Crew.ToString()) &&
              SMSettings.EnableCls)
            SetPartsHighlight(SMAddon.SmVessel.SelectedPartsTarget,
              SMSettings.Colors[SMSettings.TargetPartCrewColor], true);
          else
            SetPartsHighlight(SMAddon.SmVessel.SelectedPartsTarget, SMSettings.Colors[SMSettings.TargetPartColor], true);
        }
      }
      catch (Exception ex)
      {
        if (!SMAddon.FrameErrTripped)
        {
          Utilities.LogMessage(
            string.Format(
              " in SMHighlighter.UpdateHighlighting (repeating error).  Error in step:  " + step +
              ".  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), Utilities.LogType.Error, true);
          SMAddon.FrameErrTripped = true;
        }
      }
    }

    #endregion
  }
}