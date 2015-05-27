using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using UnityEngine;
using HighlightingSystem;
using ConnectedLivingSpace;

namespace ShipManifest
{
    internal static class SMHighlighter
    {

        #region Properties

        // WindowTransfer part mouseover vars
        internal static bool isMouseOver = false;
        internal static SMAddon.XFERMode MouseOverMode = SMAddon.XFERMode.SourceToTarget;
        internal static Rect MouseOverRect = new Rect(0, 0, 0, 0);
        internal static Part MouseOverpart = null;

        #endregion


        #region Methods

        internal static void ClearPartsHighlight(List<Part> parts)
        {
            if (parts != null)
            {
                foreach (Part part in parts)
                    ClearPartHighlight(part);
            }

        }

        /// <summary>
        /// Remove highlighting on a part.
        /// </summary>
        /// <param name="part">Part to remove highlighting from.</param>
        internal static void ClearPartHighlight(Part part)
        {
            try
            {
                if (part != null)
                {
                    part.SetHighlight(false, false);
                    part.SetHighlightDefault();
                    part.highlightType = Part.HighlightType.OnMouseOver;
                }
            }
            catch (Exception ex)
            {
                Utilities.LogMessage(string.Format(" in  ClearPartHighlight.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        /// <summary>
        /// Removes Highlighting on parts belonging to the selected resource list.
        /// </summary>
        /// <param name="_ResourceParts"></param>
        internal static void ClearResourceHighlighting(List<Part> _ResourceParts)
        {
            if (_ResourceParts != null)
            {
                foreach (Part part in _ResourceParts)
                {
                    ClearPartHighlight(part);
                }
                if (SMSettings.EnableCLS && SMSettings.EnableCLSHighlighting && SMAddon.clsAddon.Vessel != null)
                    SMAddon.clsAddon.Vessel.Highlight(false);
            }
        }

        internal static void SetPartsHighlight(List<Part> parts, Color color)
        {
            try
            {
                foreach (Part part in parts)
                {
                    SetPartHighlight(part, color);
                }
            }
            catch (Exception ex)
            {
                Utilities.LogMessage(string.Format(" in  SetPartsHighlight.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        internal static void SetPartHighlight(Part part, Color color)
        {
            try
            {

                if (part != null)
                {
                    part.SetHighlightColor(color);
                    part.SetHighlight(true, false);
                    part.highlightType = Part.HighlightType.AlwaysOn;
                }
            }
            catch (Exception ex)
            {
                Utilities.LogMessage(string.Format(" in  SetPartHighlight.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        internal static void MouseOverHighlight(Part _part)
        {
            string step = "begin";
            try
            {
                step = "inside box - Part Selection?";
                SetPartHighlight(_part, SMSettings.Colors[SMSettings.MouseOverColor]);
                EdgeHighight(_part, true);
            }
            catch (Exception ex)
            {
                Utilities.LogMessage(string.Format(" in Light.Highlight at step {0}.  Error:  {1}", step, ex.ToString()), "Error", true);
            }
        }

        internal static void EdgeHighight(Part part, bool enable, string color = null)
        {
            if (SMSettings.EnableEdgeHighlighting)
            {
                Highlighter highlighter = part.highlighter;
                if (enable)
                {
                    if (color == null || color == "")
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

        internal static void HighlightCLSVessel(bool enabled, bool force = false)
        {
            try
            {
                //SMAddon.UpdateCLSSpaces();
                if (SMAddon.clsAddon.Vessel == null)
                    SMAddon.UpdateCLSSpaces();
                if (SMAddon.clsAddon.Vessel != null)
                {
                    foreach (ICLSSpace Space in SMAddon.clsAddon.Vessel.Spaces)
                    {
                        foreach (ICLSPart part in Space.Parts)
                        {
                            part.Highlight(enabled, force);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (!SMAddon.frameErrTripped)
                {
                    Utilities.LogMessage(string.Format(" in HighlightCLSVessel (repeating error).  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
                    SMAddon.frameErrTripped = true;
                }
            }
        }

        internal static void UpdateHighlighting()
        {
            string step = "";
            try
            {
                // Do we even want to highlight?
                if (SMSettings.EnableHighlighting)
                {
                    step = "Showhipmanifest = true";
                    if (SMAddon.CanShowShipManifest(false))
                    {
                        if (SMSettings.EnableCLS && SMSettings.EnableCLSHighlighting && SMAddon.clsAddon.Vessel != null)
                        {
                            if (SMAddon.smController.SelectedResources.Contains("Crew"))
                            {
                                step = "Highlight CLS vessel";
                                HighlightCLSVessel(true);

                                // Turn off the source and target cls highlighting.  We are going to replace it.
                                if (SMAddon.smController.clsPartSource != null)
                                    SMAddon.smController.clsPartSource.Highlight(false, true);
                                if (SMAddon.smController.clsPartTarget != null)
                                    SMAddon.smController.clsPartTarget.Highlight(false, true);
                            }
                        }

                        step = "Set Selected Part Colors";
                        SetPartsHighlight(SMAddon.smController.SelectedPartsSource, SMSettings.Colors[SMSettings.SourcePartColor]);
                        if (SMAddon.smController.SelectedResources.Contains("Crew") && SMSettings.EnableCLS)
                            SetPartsHighlight(SMAddon.smController.SelectedPartsTarget, SMSettings.Colors[SMSettings.TargetPartCrewColor]);
                        else
                            SetPartsHighlight(SMAddon.smController.SelectedPartsTarget, SMSettings.Colors[SMSettings.TargetPartColor]);

                        // Default is yellow
                        step = "Set non selected resource part color";
                        Color partColor = SMSettings.Colors[SMSettings.ResourcePartColor];

                        // match color used by CLS if active
                        if (SMAddon.smController.SelectedResources.Contains("Crew") && SMSettings.EnableCLS)
                            partColor = Color.green;

                        step = "Set Resource Part Colors";
                        if (SMAddon.smController.SelectedResources != null && SMAddon.smController.SelectedResources.Count > 0)
                        {
                            foreach (Part thispart in SMAddon.smController.SelectedResourcesParts)
                            {
                                SMHighlighter.EdgeHighight(thispart, false);
                                if (!SMSettings.OnlySourceTarget)
                                {
                                    if (!SMAddon.smController.SelectedPartsSource.Contains(thispart) && !SMAddon.smController.SelectedPartsTarget.Contains(thispart))
                                    {
                                        SetPartHighlight(thispart, partColor);
                                    }
                                }
                                if (SMHighlighter.isMouseOver && thispart == SMHighlighter.MouseOverpart)
                                {
                                    if (SMHighlighter.MouseOverRect.Contains(Event.current.mousePosition))
                                        WindowTransfer.MouseOverHighlight(thispart);
                                    else
                                    {
                                        //Utilities.LogMessage(string.Format("MouseOverRect:  {0}\r\nMousePosition:  {1}", MouseOverRect.ToString(), Event.current.mousePosition.ToString()), "Error", true);
                                        SMHighlighter.isMouseOver = false;
                                        SMHighlighter.MouseOverpart = null;
                                        SetPartHighlight(thispart, partColor);
                                        EdgeHighight(thispart, false);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (!SMAddon.frameErrTripped)
                {
                    Utilities.LogMessage(string.Format(" in SMAddon.UpdateHighlighting (repeating error).  Error in step:  " + step + ".  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
                    SMAddon.frameErrTripped = true;
                }
            }
        }

        #endregion

    }
}
