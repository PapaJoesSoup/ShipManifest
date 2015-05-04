using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ShipManifest
{
    class ModXferResource
    {
        // Properties
        internal string ResourceName = "";


        // used during transfer operations.
        internal double AmtXferred = 0;
        internal double XferRatio = 1;

        // Display vars.   Not relative to xfermode

        //Source Viewer
        internal double sXferAmount = 0;
        internal string strSXferAmount = "0";
        internal bool sXferAmountHasDecimal = false;
        internal bool sXferAmountHasZero = false;

        //Target Viewer
        internal double tXferAmount = 0;
        internal string strTXferAmount = "0";
        internal bool tXferAmountHasDecimal = false;
        internal bool tXferAmountHasZero = false;


        // Constructors
        internal ModXferResource() { }

        internal ModXferResource(string resourceName) 
        {
            this.ResourceName = resourceName;
        }

        // Methods

        // methods to derive data from part selection lists and xferMode
        internal double XferAmount(SMAddon.XFERMode XferMode)
        {
            // which way we going?
            if (XferMode == SMAddon.XFERMode.SourceToTarget)
                return sXferAmount;
            else
                return tXferAmount;
        }

        /// <summary>
        /// Returns capacity of source (relative to xferMode)
        /// </summary>
        internal double SourceCapacity(SMAddon.XFERMode XferMode)
        {
            if (XferMode == SMAddon.XFERMode.SourceToTarget)
                return CalcResourceCapacity(SMAddon.smController.SelectedPartsSource, ResourceName);
            else
                return CalcResourceCapacity(SMAddon.smController.SelectedPartsTarget, ResourceName);
        }
        /// <summary>
        /// returns capacity of target (relative to xferMode)
        /// </summary>
        internal double TargetCapacity(SMAddon.XFERMode XferMode)
        {
            if (XferMode == SMAddon.XFERMode.SourceToTarget)
                return CalcResourceCapacity(SMAddon.smController.SelectedPartsTarget, ResourceName);
            else
                return CalcResourceCapacity(SMAddon.smController.SelectedPartsSource, ResourceName);
        }

        /// <summary>
        /// returns amount remaining in source (relative to XferMode)
        /// </summary>
        internal double SourceAmtRemaining(SMAddon.XFERMode XferMode)
        {
            if (XferMode == SMAddon.XFERMode.SourceToTarget)
                return CalcResourceRemaining(SMAddon.smController.SelectedPartsSource, ResourceName);
            else
                return CalcResourceRemaining(SMAddon.smController.SelectedPartsTarget, ResourceName);
        }

        /// <summary>
        /// returns amount remaining in target (relative to xferMode)
        /// </summary>
        internal double TargetAmtRemaining(SMAddon.XFERMode XferMode)
        {
            if (XferMode == SMAddon.XFERMode.SourceToTarget)
                return CalcResourceRemaining(SMAddon.smController.SelectedPartsTarget, ResourceName);
            else
                return CalcResourceRemaining(SMAddon.smController.SelectedPartsSource, ResourceName);
        }

        /// <summary>
        /// returns amount remaining in target (relative to xferMode)
        /// </summary>
        internal double TargetCapacityRemaining(SMAddon.XFERMode XferMode)
        {
            if (XferMode == SMAddon.XFERMode.SourceToTarget)
                return CalcRemainingCapacity(SMAddon.smController.SelectedPartsTarget, ResourceName);
            else
                return CalcRemainingCapacity(SMAddon.smController.SelectedPartsSource, ResourceName);
        }

        internal static double CalcResourceRemaining(List<Part> parts, string SelectedResource)
        {
            double amount = 0;
            if (parts != null)
                foreach (Part part in parts)
                    amount += part.Resources[SelectedResource].amount;
            return amount;
        }

        internal static double CalcResourceCapacity(List<Part> parts, string SelectedResource)
        {
            double amount = 0;
            if (parts != null)
                foreach (Part part in parts)
                    amount += part.Resources[SelectedResource].maxAmount;
            return amount;
        }

        internal static double CalcRemainingCapacity(List<Part> parts, string SelectedResource)
        {
            double amount = 0;
            if (parts != null)
                foreach (Part part in parts)
                    amount += (part.Resources[SelectedResource].maxAmount - part.Resources[SelectedResource].amount);
            return amount;
        }

        internal static double CalcMaxXferAmt(List<Part> Sources, List<Part> Targets, List<string> SelectedResources)
        {
            double maxXferAmount = 0;
            if (Sources.Count == 0 || Targets.Count == 0 || (SelectedResources.Count == 0 || SelectedResources.Count == 0))
                maxXferAmount = 0;
            else
            {
                // First determine if there is more than one Resource to move.  get the larger number for a ratio basis.
                int resIdx = 0;
                if (SelectedResources.Count > 1)
                {
                    if (ModXferResource.CalcResourceCapacity(Targets, SelectedResources[0]) < ModXferResource.CalcResourceCapacity(Targets, SelectedResources[1]))
                        resIdx = 1;
                }
                // now lets get the amount to move.  we will use this higher portion to calc lower volume ratio during move.
                double maxSourceAmount = 0;
                foreach (Part Source in Sources)
                    maxSourceAmount += Source.Resources[SelectedResources[resIdx]].amount;
                foreach (Part Target in Targets)
                    maxXferAmount += (Target.Resources[SelectedResources[resIdx]].maxAmount - Target.Resources[SelectedResources[resIdx]].amount);

                maxXferAmount = maxXferAmount > maxSourceAmount ? maxSourceAmount : maxXferAmount;
                maxXferAmount = maxXferAmount < 0.0001 ? 0 : maxXferAmount;
            }
            return maxXferAmount;
        }

        internal static double CalcMaxResourceXferAmt(List<Part> Sources, List<Part> Targets, string SelectedResource)
        {
            double maxXferAmount = 0;
            if (Sources.Count == 0 || Targets.Count == 0 || (SelectedResource == null || SelectedResource == ""))
                maxXferAmount = 0;
            else
            {
                // now lets get the amount to move.  we will use this higher portion to calc lower volume ratio during move.
                double maxSourceAmount = 0;
                foreach (Part Source in Sources)
                    maxSourceAmount += Source.Resources[SelectedResource].amount;
                foreach (Part Target in Targets)
                    maxXferAmount += (Target.Resources[SelectedResource].maxAmount - Target.Resources[SelectedResource].amount);

                maxXferAmount = maxXferAmount > maxSourceAmount ? maxSourceAmount : maxXferAmount;
                maxXferAmount = maxXferAmount < 0.0001 ? 0 : maxXferAmount;
            }
            return maxXferAmount;
        }

        // multi Resource methods
        internal static ModXferResource GetXferResource(List<ModXferResource> XferResources, SMAddon.XFERMode XferMode, bool isRatio = false)
        {
            if (XferResources.Count > 1)
            {
                if (isRatio)
                {
                    if (XferResources[1].TargetCapacity(XferMode) > XferResources[0].TargetCapacity(XferMode))
                        return XferResources[0];
                    else
                        return XferResources[1];
                }
                else
                {
                    if (XferResources[0].TargetCapacity(XferMode) > XferResources[1].TargetCapacity(XferMode))
                        return XferResources[0];
                    else
                        return XferResources[1];
                }
            }
            else
            {
                return XferResources[0];
            }
        }

        internal static double CalcRatio(List<ModXferResource> Resources, SMAddon.XFERMode XferMode)
        {
            if (Resources.Count > 1)
            {
                if (Resources[0].TargetCapacity(XferMode) > Resources[1].TargetCapacity(XferMode))
                    return Resources[1].TargetCapacity(XferMode) / Resources[0].TargetCapacity(XferMode);
                else
                    return Resources[0].TargetCapacity(XferMode) / Resources[1].TargetCapacity(XferMode);
            }
            else
                return 1;
        }

        // these methods are for managing textbox string data entry for xfer amounts 
        internal void SetXferAmountString(string xferAmount, SMAddon.XFERMode xferMode)
        {
            // Needed so we can alter slider controls on both the source and target windows.
            if (xferMode == SMAddon.XFERMode.TargetToSource)
                strSXferAmount = xferAmount;
            else
                strTXferAmount = xferAmount;
        }

        internal float UpdateXferAmount(string strXferAmount, SMAddon.XFERMode xferMode)
        {
            // Needed so we can alter slider controls on both the source and target windows.
            // Also, slider requires a float number...
            float newAmount = 0;
            if (float.TryParse(strXferAmount, out newAmount))
                if (xferMode == SMAddon.XFERMode.SourceToTarget)
                    sXferAmount = newAmount;
                else
                    tXferAmount = newAmount;
            return newAmount;
        }

        internal string GetStringDecimal(string strXferAmount, SMAddon.XFERMode XferMode)
        {
            if (XferMode == SMAddon.XFERMode.SourceToTarget)
            {
                if (sXferAmountHasDecimal)
                    strXferAmount += ".";
            }
            else
            {
                if (tXferAmountHasDecimal)
                    strXferAmount += ".";
            }
            return strXferAmount;
        }

        internal string GetStringZero(string strXferAmount, SMAddon.XFERMode XferMode)
        {
            if (XferMode == SMAddon.XFERMode.SourceToTarget)
            {
                if (sXferAmountHasZero)
                    strXferAmount += "0";
            }
            else
            {
                if (tXferAmountHasZero)
                    strXferAmount += "0";
            }
            return strXferAmount;
        }

        internal void SetStringZero(string strXferAmount, SMAddon.XFERMode XferMode)
        {
            if (XferMode == SMAddon.XFERMode.SourceToTarget)
            {
                if (strXferAmount.Contains(".") && strXferAmount.EndsWith("0"))
                    sXferAmountHasZero = true;
                else
                    sXferAmountHasZero = false;
            }
            else
            {
                if (strXferAmount.Contains(".") && strXferAmount.EndsWith("0"))
                    tXferAmountHasZero = true;
                else
                    tXferAmountHasZero = false;
            }
        }

        internal void SetStringDecimal(string strXferAmount, SMAddon.XFERMode XferMode)
        {
            if (XferMode == SMAddon.XFERMode.SourceToTarget)
            {
                if (strXferAmount.EndsWith(".") || strXferAmount.EndsWith(".0"))
                    sXferAmountHasDecimal = true;
                else
                    sXferAmountHasDecimal = false;
            }
            else
            {
                if (strXferAmount.EndsWith(".") || strXferAmount.EndsWith(".0"))
                    tXferAmountHasDecimal = true;
                else
                    tXferAmountHasDecimal = false;
            }
        }

        // Action Methods
        internal static void XferResource(List<Part> XferParts, ModXferResource modResource, double XferAmount, SMAddon.XFERMode XferMode, bool isSource)
        {
            // This adjusts the delta when we get to the end of the xfer.
            Utilities.LogMessage("XferResource:  " + modResource.ResourceName + " - 1. XferAmount = " + XferAmount.ToString() + ", Is Source: " + isSource.ToString(), "Info", Settings.VerboseLogging);

            // This var keeps track of what we actually moved..
            double XferBalance = XferAmount;

            // Not all parts will have enough resource to meet the SrcPartAmt to move.   We need to account for that.
            // count up source parts with avalilabe resources. so we can devide by them
            while (XferBalance > 0)
            {
                Utilities.LogMessage("XferResource:  " + modResource.ResourceName + " - 2. XferBalance = " + XferBalance.ToString() + ", Is Source: " + isSource.ToString(), "Info", Settings.VerboseLogging);

                // Lets account for any empty/full containers
                int PartCount = 0;
                foreach (Part part in XferParts)
                    if ((isSource && part.Resources[modResource.ResourceName].amount > 0) || 
                        (!isSource && part.Resources[modResource.ResourceName].amount < part.Resources[modResource.ResourceName].maxAmount))
                        PartCount += 1;

                // now split up the xfer amount evenly across the number of tanks that can send/receive resources
                double PartAmt = XferBalance / PartCount;

                // Calculate Xfer amounts for each part and move.
                foreach (Part part in XferParts)
                {
                    double AmtToMove = 0;
                    if (isSource)
                    {
                        AmtToMove = part.Resources[modResource.ResourceName].amount >= PartAmt ? PartAmt : part.Resources[modResource.ResourceName].amount;
                        part.Resources[modResource.ResourceName].amount -= AmtToMove;
                    }
                    else
                    {
                        double CapacityAvail = part.Resources[modResource.ResourceName].maxAmount - part.Resources[modResource.ResourceName].amount;
                        AmtToMove = CapacityAvail >= PartAmt ? PartAmt : CapacityAvail;
                        part.Resources[modResource.ResourceName].amount += AmtToMove;
                    }
                    Utilities.LogMessage("XferResource:  " + modResource.ResourceName + " - 3. AmtToMove = " + AmtToMove.ToString() + ", Is Source: " + isSource.ToString(), "Info", Settings.VerboseLogging);
                    // Report ramaining balance after Transfer.
                    XferBalance -= AmtToMove;
                    if (XferBalance <= 0)
                        break;
                }
            }
        }

    }
}
