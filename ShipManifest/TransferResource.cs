using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ShipManifest
{
    class TransferResource
    {

        #region Properties

        // used during transfer operations.
        internal static bool ResourceXferActive = false;
        internal static ResourceXFERState XferState = ResourceXFERState.Off;
        internal static double flow_rate = (double)SMSettings.FlowRate;
        internal static int flow_time = SMSettings.MaxFlowTimeSec;
        internal static double act_flow_rate = 0;
        internal static DateTime timestamp;

        // Object Properties
        internal string ResourceName = "";
        internal double XferRatio = 1;
        internal double AmtXferred = 0;
        internal double AmtXferredOld = 0;
        internal int XferTimeout = 0;

        //Source Viewer
        internal double srcXferAmount = 0;
        internal string strSrcXferAmount = "0";
        internal bool srcXferAmountHasDecimal = false;
        internal bool srcXferAmountHasZero = false;

        //Target Viewer
        internal double tgtXferAmount = 0;
        internal string strTgtXferAmount = "0";
        internal bool tgtXferAmountHasDecimal = false;
        internal bool tgtXferAmountHasZero = false;

        #endregion

        #region Constructors

        internal TransferResource() { }

        internal TransferResource(string resourceName) 
        {
            this.ResourceName = resourceName;
        }

        #endregion

        #region Methods

        // methods to derive data from part selection lists and xferMode
        internal double XferAmount(SMAddon.XFERMode XferMode)
        {
            // which way we going?
            if (XferMode == SMAddon.XFERMode.SourceToTarget)
                return srcXferAmount;
            else
                return tgtXferAmount;
        }

        /// <summary>
        /// Returns capacity of the parts we are transferrring from (sources relative to xferMode)
        /// </summary>
        internal double FromCapacity(SMAddon.XFERMode XferMode)
        {
            if (XferMode == SMAddon.XFERMode.SourceToTarget)
                return CalcResourceCapacity(SMAddon.smController.SelectedPartsSource, ResourceName);
            else
                return CalcResourceCapacity(SMAddon.smController.SelectedPartsTarget, ResourceName);
        }

        /// <summary>
        /// returns capacity of parts we are transferring to (targets relative to xferMode)
        /// </summary>
        internal double ToCapacity(SMAddon.XFERMode XferMode)
        {
            if (XferMode == SMAddon.XFERMode.SourceToTarget)
                return CalcResourceCapacity(SMAddon.smController.SelectedPartsTarget, ResourceName);
            else
                return CalcResourceCapacity(SMAddon.smController.SelectedPartsSource, ResourceName);
        }

        /// <summary>
        /// returns amount remaining in parts we are transferrring from (sources relative to XferMode)
        /// </summary>
        internal double FromAmtRemaining(SMAddon.XFERMode XferMode)
        {
            if (XferMode == SMAddon.XFERMode.SourceToTarget)
                return CalcResourceRemaining(SMAddon.smController.SelectedPartsSource, ResourceName);
            else
                return CalcResourceRemaining(SMAddon.smController.SelectedPartsTarget, ResourceName);
        }

        /// <summary>
        /// returns amount remaining in parts we are transferring to (targets relative to xferMode)
        /// </summary>
        internal double ToAmtRemaining(SMAddon.XFERMode XferMode)
        {
            if (XferMode == SMAddon.XFERMode.SourceToTarget)
                return CalcResourceRemaining(SMAddon.smController.SelectedPartsTarget, ResourceName);
            else
                return CalcResourceRemaining(SMAddon.smController.SelectedPartsSource, ResourceName);
        }

        /// <summary>
        /// returns amount remaining in parts we are transferring to (targets relative to xferMode)
        /// </summary>
        internal double ToCapacityRemaining(SMAddon.XFERMode XferMode)
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

        // these methods are for managing textbox string data entry for xfer amounts 
        internal void SetXferAmountString(string xferAmount, SMAddon.XFERMode xferMode)
        {
            // Needed so we can alter slider controls on both the source and target windows.
            if (xferMode == SMAddon.XFERMode.TargetToSource)
                strSrcXferAmount = xferAmount;
            else
                strTgtXferAmount = xferAmount;
        }

        internal float UpdateXferAmount(string strXferAmount, SMAddon.XFERMode xferMode)
        {
            // Needed so we can alter slider controls on both the source and target windows.
            // Also, slider requires a float number...
            float newAmount = 0;
            if (float.TryParse(strXferAmount, out newAmount))
                if (xferMode == SMAddon.XFERMode.SourceToTarget)
                    srcXferAmount = newAmount;
                else
                    tgtXferAmount = newAmount;
            return newAmount;
        }

        internal string GetStringDecimal(string strXferAmount, SMAddon.XFERMode XferMode)
        {
            if (XferMode == SMAddon.XFERMode.SourceToTarget)
            {
                if (srcXferAmountHasDecimal)
                    strXferAmount += ".";
            }
            else
            {
                if (tgtXferAmountHasDecimal)
                    strXferAmount += ".";
            }
            return strXferAmount;
        }

        internal string GetStringZero(string strXferAmount, SMAddon.XFERMode XferMode)
        {
            if (XferMode == SMAddon.XFERMode.SourceToTarget)
            {
                if (srcXferAmountHasZero)
                    strXferAmount += "0";
            }
            else
            {
                if (tgtXferAmountHasZero)
                    strXferAmount += "0";
            }
            return strXferAmount;
        }

        internal void SetStringZero(string strXferAmount, SMAddon.XFERMode XferMode)
        {
            if (XferMode == SMAddon.XFERMode.SourceToTarget)
            {
                if (strXferAmount.Contains(".") && strXferAmount.EndsWith("0"))
                    srcXferAmountHasZero = true;
                else
                    srcXferAmountHasZero = false;
            }
            else
            {
                if (strXferAmount.Contains(".") && strXferAmount.EndsWith("0"))
                    tgtXferAmountHasZero = true;
                else
                    tgtXferAmountHasZero = false;
            }
        }

        internal void SetStringDecimal(string strXferAmount, SMAddon.XFERMode XferMode)
        {
            if (XferMode == SMAddon.XFERMode.SourceToTarget)
            {
                if (strXferAmount.EndsWith(".") || strXferAmount.EndsWith(".0"))
                    srcXferAmountHasDecimal = true;
                else
                    srcXferAmountHasDecimal = false;
            }
            else
            {
                if (strXferAmount.EndsWith(".") || strXferAmount.EndsWith(".0"))
                    tgtXferAmountHasDecimal = true;
                else
                    tgtXferAmountHasDecimal = false;
            }
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

        internal static double CalcMaxXferAmt(List<Part> PartsFrom, List<Part> PartsTo, List<string> SelectedResources)
        {
            double maxXferAmount = 0;
            if (PartsFrom.Count == 0 || PartsTo.Count == 0 || (SelectedResources.Count == 0 || SelectedResources.Count == 0))
                maxXferAmount = 0;
            else
            {
                // First determine if there is more than one Resource to move.  get the larger number for a ratio basis.
                int resIdx = 0;
                if (SelectedResources.Count > 1)
                {
                    if (TransferResource.CalcResourceCapacity(PartsTo, SelectedResources[0]) < TransferResource.CalcResourceCapacity(PartsTo, SelectedResources[1]))
                        resIdx = 1;
                }
                // now lets get the amount to move.  we will use this higher portion to calc lower volume ratio during move.
                double maxFromAmount = 0;
                foreach (Part partFrom in PartsFrom)
                    maxFromAmount += partFrom.Resources[SelectedResources[resIdx]].amount;
                foreach (Part partTo in PartsTo)
                    maxXferAmount += (partTo.Resources[SelectedResources[resIdx]].maxAmount - partTo.Resources[SelectedResources[resIdx]].amount);

                maxXferAmount = maxXferAmount > maxFromAmount ? maxFromAmount : maxXferAmount;
                maxXferAmount = maxXferAmount < 0.0001 ? 0 : maxXferAmount;
            }
            return maxXferAmount;
        }

        internal static double CalcMaxResourceXferAmt(List<Part> From, List<Part> To, string SelectedResource)
        {
            double maxXferAmount = 0;
            if (From.Count == 0 || To.Count == 0 || (SelectedResource == null || SelectedResource == ""))
                maxXferAmount = 0;
            else
            {
                // now lets get the amount to move.  we will use this higher portion to calc lower volume ratio during move.
                double maxSourceAmount = 0;
                foreach (Part Source in From)
                    maxSourceAmount += Source.Resources[SelectedResource].amount;
                foreach (Part Target in To)
                    maxXferAmount += (Target.Resources[SelectedResource].maxAmount - Target.Resources[SelectedResource].amount);

                maxXferAmount = maxXferAmount > maxSourceAmount ? maxSourceAmount : maxXferAmount;
                maxXferAmount = maxXferAmount < 0.0001 ? 0 : maxXferAmount;
            }
            return maxXferAmount;
        }

        private static bool isXferComplete()
        {
            List<Part> PartsFrom = SMAddon.smController.SelectedPartsSource;
            List<Part> PartsTo = SMAddon.smController.SelectedPartsTarget;
            if (SMAddon.XferMode == SMAddon.XFERMode.TargetToSource)
            {
                PartsFrom = SMAddon.smController.SelectedPartsTarget;
                PartsTo = SMAddon.smController.SelectedPartsSource;
            }

            if (TransferResource.CalcMaxXferAmt(PartsFrom, PartsTo, SMAddon.smController.SelectedResources) == 0)
                return true;
            if (SMAddon.smController.ResourcesToXfer.Count > 1)
            {
                Utilities.LogMessage("isXferComplete - A1. Resource:  " + SMAddon.smController.ResourcesToXfer[0].ResourceName + ", TotalXferAmt = " + SMAddon.smController.ResourcesToXfer[0].XferAmount(SMAddon.XferMode).ToString() + ", AmtXferred = " + SMAddon.smController.ResourcesToXfer[0].AmtXferred.ToString(), "Info", SMSettings.VerboseLogging);
                Utilities.LogMessage("isXferComplete - A2 Resource:  " + SMAddon.smController.ResourcesToXfer[1].ResourceName + ", TotalXferAmt = " + SMAddon.smController.ResourcesToXfer[1].XferAmount(SMAddon.XferMode).ToString() + ", AmtXferred = " + SMAddon.smController.ResourcesToXfer[1].AmtXferred.ToString(), "Info", SMSettings.VerboseLogging);
                if (SMAddon.smController.ResourcesToXfer[0].FromAmtRemaining(SMAddon.XferMode) == 0d && 
                    SMAddon.smController.ResourcesToXfer[1].FromAmtRemaining(SMAddon.XferMode) == 0d)
                    return true;
                if (SMAddon.smController.ResourcesToXfer[0].ToCapacityRemaining(SMAddon.XferMode) == 0d && 
                    SMAddon.smController.ResourcesToXfer[1].ToCapacityRemaining(SMAddon.XferMode) == 0d)
                    return true;
                if ((SMAddon.smController.ResourcesToXfer[0].AmtXferred >= SMAddon.smController.ResourcesToXfer[0].XferAmount(SMAddon.XferMode)- 0.0000001) &&
                    (SMAddon.smController.ResourcesToXfer[1].AmtXferred >= SMAddon.smController.ResourcesToXfer[1].XferAmount(SMAddon.XferMode) - 0.0000001))
                    return true;
                return false;
            }
            else
            {
                Utilities.LogMessage("isXferComplete - B. Resource:  " + SMAddon.smController.ResourcesToXfer[0].ResourceName + ", TotalXferAmt = " + SMAddon.smController.ResourcesToXfer[0].XferAmount(SMAddon.XferMode).ToString() + ", AmtXferred = " + SMAddon.smController.ResourcesToXfer[0].AmtXferred.ToString(), "Info", SMSettings.VerboseLogging);
                if (SMAddon.smController.ResourcesToXfer[0].FromAmtRemaining(SMAddon.XferMode) == 0d)
                    return true;
                if (SMAddon.smController.ResourcesToXfer[0].ToCapacityRemaining(SMAddon.XferMode) == 0d)
                    return true;
                if (SMAddon.smController.ResourcesToXfer[0].AmtXferred >= SMAddon.smController.ResourcesToXfer[0].XferAmount(SMAddon.XferMode))
                    return true;
                return false;
            }
        }

       // multi Resource methods
        internal static TransferResource GetXferResource(List<TransferResource> XferResources, SMAddon.XFERMode XferMode, bool isRatio = false)
        {
            if (XferResources.Count > 1)
            {
                if (isRatio)
                {
                    if (XferResources[1].ToCapacity(XferMode) > XferResources[0].ToCapacity(XferMode))
                        return XferResources[0];
                    else
                        return XferResources[1];
                }
                else
                {
                    if (XferResources[0].ToCapacity(XferMode) > XferResources[1].ToCapacity(XferMode))
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

        internal static double CalcRatio(List<TransferResource> Resources, SMAddon.XFERMode XferMode)
        {
            if (Resources.Count > 1)
            {
                if (Resources[0].ToCapacity(XferMode) > Resources[1].ToCapacity(XferMode))
                    return Resources[1].ToCapacity(XferMode) / Resources[0].ToCapacity(XferMode);
                else
                    return Resources[0].ToCapacity(XferMode) / Resources[1].ToCapacity(XferMode);
            }
            else
                return 1;
        }

        // Action Methods
        internal static void ResourceTransferProcess()
        {
            try
            {
                if (ResourceXferActive)
                {
                    double deltaT = 0;
                    flow_rate = SMSettings.FlowRate;

                    switch (XferState)
                    {
                        case ResourceXFERState.Off:
                            // reset counters
                            SMAddon.elapsed = 0;
                            foreach (TransferResource modResource in SMAddon.smController.ResourcesToXfer)
                            {
                                modResource.AmtXferredOld = 0;
                                modResource.XferTimeout = 0;
                            }

                            // Default sound license: CC-By-SA
                            // http://www.freesound.org/people/vibe_crc/sounds/59328/
                            string path1 = SMSettings.PumpSoundStart != null ? SMSettings.PumpSoundStart : "ShipManifest/Sounds/59328-1";
                            string path2 = SMSettings.PumpSoundRun != null ? SMSettings.PumpSoundRun : "ShipManifest/Sounds/59328-2";
                            string path3 = SMSettings.PumpSoundStop != null ? SMSettings.PumpSoundStop : "ShipManifest/Sounds/59328-3";

                            // Load Sounds, and Play Sound 1
                            SMAddon.LoadSounds("Pump", path1, path2, path3, SMSettings.PumpSoundVol);
                            XferState = ResourceXFERState.Start;
                            timestamp = DateTime.Now;
                            break;

                        case ResourceXFERState.Start:

                            // calculate elapsed.
                            SMAddon.elapsed += (DateTime.Now - TransferResource.timestamp).TotalSeconds; 

                            // Play run sound when start sound is nearly done. (repeats)
                            if (SMAddon.elapsed >= SMAddon.source1.clip.length - 0.25)
                            {
                                SMAddon.source2.Play();
                                SMAddon.elapsed = 0;
                                XferState = ResourceXFERState.Run;
                            }
                            break;

                        case ResourceXFERState.Run:

                            //Run process:

                            // 1.  Get elapsed from last run
                            deltaT = (DateTime.Now - TransferResource.timestamp).TotalSeconds; 
                            Utilities.LogMessage("ResourceTransferProcess - 1. DeltaT = " + deltaT.ToString(), "Info", SMSettings.VerboseLogging);
                            if (deltaT == 0)
                                return;
                            TransferResource.timestamp = DateTime.Now;

                            // 2.  Calculate amount to move based on flow rate and time delta
                            double deltaAmt = deltaT * act_flow_rate;
                            
                            List<Part> PartsFrom = SMAddon.smController.SelectedPartsSource;
                            List<Part> PartsTo = SMAddon.smController.SelectedPartsTarget;
                            if (SMAddon.XferMode == SMAddon.XFERMode.TargetToSource)
                            {
                                // From is target on Interface...
                                PartsFrom = SMAddon.smController.SelectedPartsTarget;
                                PartsTo = SMAddon.smController.SelectedPartsSource;
                            }

                            foreach (TransferResource modResource in SMAddon.smController.ResourcesToXfer)
                            {

                                deltaAmt = deltaT * act_flow_rate * modResource.XferRatio;
                                Utilities.LogMessage("ResourceTransferProcess - 2a. Resource:  " + modResource.ResourceName + ", DeltaAmt = " + deltaAmt.ToString(), "Info", SMSettings.VerboseLogging);

                                // 3.  determine if enough resouce remains to drain
                                double FromAmtRemaining = modResource.FromAmtRemaining(SMAddon.XferMode);
                                if (deltaAmt > FromAmtRemaining)
                                    deltaAmt = FromAmtRemaining;

                                // 4.  Determine if enough capacity remains to fill
                                double ToAmtRemianCapacity = modResource.ToCapacityRemaining(SMAddon.XferMode);
                                if (deltaAmt > ToAmtRemianCapacity)
                                    deltaAmt = ToAmtRemianCapacity;

                                // 5.  Determine if move amount exceeds remaining move amount requested
                                deltaAmt = deltaAmt > (modResource.XferAmount(SMAddon.XferMode) - modResource.AmtXferred) ? (modResource.XferAmount(SMAddon.XferMode) - modResource.AmtXferred) : deltaAmt;

                                Utilities.LogMessage("ResourceTransferProcess - 2b. Resource:  " + modResource.ResourceName + ", Adj deltaAmt = " + deltaAmt.ToString(), "Info", SMSettings.VerboseLogging);

                                if (deltaAmt > 0)
                                {
                                    double deltaCharge = deltaAmt * SMSettings.FlowCost;
                                    // 6.  Drain Charge
                                    if (!ConsumeCharge(deltaCharge))
                                    {
                                        XferState = ResourceXFERState.Stop;
                                    }
                                    else
                                    {
                                        Utilities.LogMessage("ResourceTransferProcess - 3a. Resource:  " + modResource.ResourceName + ", Xferring DeltaAmt = " + deltaAmt.ToString(), "Info", SMSettings.VerboseLogging);
                                        // 7.  Get list of From parts & Drain Resource
                                        TransferResource.XferResource(PartsFrom, modResource, deltaAmt, SMAddon.XferMode, true);

                                        // 8.  Get list of To parts & Fill resource.
                                        TransferResource.XferResource(PartsTo, modResource, deltaAmt, SMAddon.XferMode, false);

                                        // 9.  Update transferred amount.
                                        modResource.AmtXferred += deltaAmt;
                                    }
                                }

                                Utilities.LogMessage("ResourceTransferProcess - 3b. Resource:  " + modResource.ResourceName + ", AmtXferred = " + modResource.AmtXferred.ToString(), "Info", SMSettings.VerboseLogging);
                                Utilities.LogMessage("ResourceTransferProcess - 3c. Resource:  " + modResource.ResourceName + ", SrcAmtRemaining = " + modResource.FromAmtRemaining(SMAddon.XferMode).ToString() + ", TgtCapRemaining = " + modResource.ToCapacityRemaining(SMAddon.XferMode), "Info", SMSettings.VerboseLogging);
                            }

                            // 10. determine if completed.
                            if (isXferComplete())
                                XferState = ResourceXFERState.Stop;
                            else
                            {
                                foreach (TransferResource modResource in SMAddon.smController.ResourcesToXfer)
                                {
                                    // activate timeout if we are stuck in a loop.
                                    if (modResource.AmtXferred != modResource.AmtXferredOld)
                                    {
                                        modResource.AmtXferredOld = modResource.AmtXferred;
                                        modResource.XferTimeout = 0;
                                    }
                                    else
                                    {
                                        modResource.XferTimeout += 1;
                                        if (modResource.XferTimeout >= 30)
                                        {
                                            XferState = ResourceXFERState.Stop;
                                            Utilities.LogMessage("ResourceTransferProcess - 4.  Timeout occurred!  Resource:  " + modResource.ResourceName + ", AmtXferred = " + modResource.AmtXferred.ToString(), "Error", true);
                                        }
                                    }
                                }
                            }
                            break;

                        case ResourceXFERState.Stop:

                            // play pump shutdown.
                            SMAddon.source2.Stop();
                            SMAddon.source3.Play();
                            SMAddon.elapsed = 0;
                            XferState = ResourceXFERState.Off;
                            foreach (TransferResource modResource in SMAddon.smController.ResourcesToXfer)
                            {
                                modResource.AmtXferred = 0;
                                modResource.srcXferAmount = TransferResource.CalcMaxXferAmt(SMAddon.smController.SelectedPartsSource, SMAddon.smController.SelectedPartsTarget, SMAddon.smController.SelectedResources);
                                if (modResource.srcXferAmount < 0.0001)
                                    modResource.srcXferAmount = 0;
                                modResource.strTgtXferAmount = modResource.srcXferAmount.ToString();
                                modResource.tgtXferAmount = TransferResource.CalcMaxXferAmt(SMAddon.smController.SelectedPartsTarget, SMAddon.smController.SelectedPartsSource, SMAddon.smController.SelectedResources);
                                if (modResource.tgtXferAmount < 0.0001)
                                    modResource.tgtXferAmount = 0;
                                modResource.strTgtXferAmount = modResource.tgtXferAmount.ToString();
                            }
                            ResourceXferActive = false;
                            break;
                    }
                    Utilities.LogMessage("ResourceTransferProcess - 5.  Transfer State:  " + XferState.ToString() + "...", "Info", SMSettings.VerboseLogging);
                }
            }
            catch (Exception ex)
            {
                if (!SMAddon.frameErrTripped)
                {
                    Utilities.LogMessage(string.Format(" in ResourceTransferProcess (repeating error).  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
                    SMAddon.source2.Stop();
                    SMAddon.source3.Stop();
                    XferState = ResourceXFERState.Off;
                    foreach (TransferResource modResource in SMAddon.smController.ResourcesToXfer)
                    {
                        modResource.AmtXferred = 0;
                        modResource.srcXferAmount = TransferResource.CalcMaxXferAmt(SMAddon.smController.SelectedPartsSource, SMAddon.smController.SelectedPartsTarget, SMAddon.smController.SelectedResources);
                        if (modResource.srcXferAmount < 0.0001)
                            modResource.srcXferAmount = 0;
                        modResource.strTgtXferAmount = modResource.srcXferAmount.ToString();
                        modResource.tgtXferAmount = TransferResource.CalcMaxXferAmt(SMAddon.smController.SelectedPartsTarget, SMAddon.smController.SelectedPartsSource, SMAddon.smController.SelectedResources);
                        if (modResource.tgtXferAmount < 0.0001)
                            modResource.tgtXferAmount = 0;
                        modResource.strTgtXferAmount = modResource.tgtXferAmount.ToString();
                    }
                    ResourceXferActive = false;
                    SMAddon.frameErrTripped = true;
                    throw ex;
                }
            }
        }

        internal static void XferResource(List<Part> XferParts, TransferResource modResource, double XferAmount, SMAddon.XFERMode XferMode, bool drain)
        {
            // This adjusts the delta when we get to the end of the xfer.
            Utilities.LogMessage("XferResource:  " + modResource.ResourceName + " - 1. XferAmount = " + XferAmount.ToString() + ", Drain? " + drain.ToString(), "Info", SMSettings.VerboseLogging);

            // This var keeps track of what we actually moved..
            double XferBalance = XferAmount;
            double XferBalanceOld = XferBalance;
            int XferTimeout = 0;

            // Not all parts will have enough resource to meet the SrcPartAmt to move.   We need to account for that.
            // count up source parts with avalilabe resources. so we can devide by them
            while (XferBalance > 0.0000001d)
            {
                Utilities.LogMessage("XferResource:  " + modResource.ResourceName + " - 2. XferBalance = " + XferBalance.ToString() + ", Is Source: " + drain.ToString(), "Info", SMSettings.VerboseLogging);

                // Lets account for any empty/full containers
                int PartCount = 0;
                foreach (Part part in XferParts)
                    if ((drain && part.Resources[modResource.ResourceName].amount > 0d) || 
                        (!drain && part.Resources[modResource.ResourceName].amount < part.Resources[modResource.ResourceName].maxAmount))
                        PartCount += 1;

                // now split up the xfer amount evenly across the number of tanks that can send/receive resources
                double PartAmt = XferBalance / PartCount;

                // Calculate Xfer amounts for each part and move.
                foreach (Part part in XferParts)
                {
                    double AmtToMove = 0d;
                    if (drain)
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
                    Utilities.LogMessage("XferResource:  " + modResource.ResourceName + " - 3. AmtToMove = " + AmtToMove.ToString() + ", Drain: " + drain.ToString(), "Info", SMSettings.VerboseLogging);
                    // Report ramaining balance after Transfer.
                    XferBalance -= AmtToMove;
                }
                // account for rounding and double resolution issues 
                if (XferBalance <= 0.0000001d)
                    break;
                else if (XferBalance != XferBalanceOld)
                {
                    XferBalanceOld = XferBalance;
                    XferTimeout = 0;
                }
                else if (XferBalance == XferBalanceOld)
                {
                    XferTimeout += 1;
                    if (XferTimeout >= 30)
                    {
                        Utilities.LogMessage("XferResource  Timeout!  Resource:  " + modResource.ResourceName + " - 3. XferBalance = " + XferBalance.ToString() + ", Drain: " + drain.ToString(), "Error", true);
                        break;
                    }
                }
            }
        }

        private static bool ConsumeCharge(double deltaCharge)
        {
            if (!SMAddon.smController.SelectedResources.Contains("ElectricCharge") && SMSettings.EnableXferCost)
            {
                foreach (Part iPart in SMAddon.smController._partsByResource["ElectricCharge"])
                {
                    if (iPart.Resources["ElectricCharge"].amount >= deltaCharge)
                    {
                        iPart.Resources["ElectricCharge"].amount -= deltaCharge;
                        deltaCharge = 0;
                        break;
                    }
                    else
                    {
                        deltaCharge -= iPart.Resources["ElectricCharge"].amount;
                        iPart.Resources["ElectricCharge"].amount = 0;
                    }
                }
                if (deltaCharge > 0)
                    return false;
                else
                    return true;
            }
            else
                return true;
        }

        internal static void ResourceTransferAbort()
        {
            XferState = ResourceXFERState.Stop;
        }

        internal enum ResourceXFERState
        {
            Off,
            Start,
            Run,
            Stop
        }
    #endregion
    }
}
