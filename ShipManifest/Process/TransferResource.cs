using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ShipManifest.Process
{
  class TransferResource
  {

    #region Properties

    // used during transfer operations.
    internal static bool ResourceXferActive;
    internal static ResourceXferState XferState = ResourceXferState.Off;
    internal static double FlowRate = SMSettings.FlowRate;
    internal static int FlowTime = SMSettings.MaxFlowTimeSec;
    internal static double ActFlowRate = 0;
    internal static DateTime Timestamp;

    // Object Properties
    internal string ResourceName = "";
    internal double XferRatio = 1;
    internal double AmtXferred;
    internal double AmtXferredOld;
    internal int XferTimeout;

    //Source Viewer
    internal double SrcXferAmount;
    internal string StrSrcXferAmount = "0";
    internal bool SrcXferAmountHasDecimal;
    internal bool SrcXferAmountHasZero;

    //Target Viewer
    internal double TgtXferAmount;
    internal string StrTgtXferAmount = "0";
    internal bool TgtXferAmountHasDecimal;
    internal bool TgtXferAmountHasZero;

    #endregion

    #region Constructors

    internal TransferResource() { }

    internal TransferResource(string resourceName)
    {
      ResourceName = resourceName;
    }

    #endregion

    #region Methods

    // methods to derive data from part selection lists and xferMode
    internal double XferAmount(SMAddon.XferDirection xferMode)
    {
      // which way we going?
      if (xferMode == SMAddon.XferDirection.SourceToTarget)
        return SrcXferAmount;
      return TgtXferAmount;
    }

    /// <summary>
    /// Returns capacity of the parts we are transferrring from (sources relative to xferMode)
    /// </summary>
    internal double FromCapacity(SMAddon.XferDirection xferMode)
    {
      if (xferMode == SMAddon.XferDirection.SourceToTarget)
        return CalcResourceCapacity(SMAddon.SmVessel.SelectedPartsSource, ResourceName);
      return CalcResourceCapacity(SMAddon.SmVessel.SelectedPartsTarget, ResourceName);
    }

    /// <summary>
    /// returns capacity of parts we are transferring to (targets relative to xferMode)
    /// </summary>
    internal double ToCapacity(SMAddon.XferDirection xferMode)
    {
      if (xferMode == SMAddon.XferDirection.SourceToTarget)
        return CalcResourceCapacity(SMAddon.SmVessel.SelectedPartsTarget, ResourceName);
      return CalcResourceCapacity(SMAddon.SmVessel.SelectedPartsSource, ResourceName);
    }

    /// <summary>
    /// returns amount remaining in parts we are transferrring from (sources relative to XferMode)
    /// </summary>
    internal double FromAmtRemaining(SMAddon.XferDirection xferMode)
    {
      if (xferMode == SMAddon.XferDirection.SourceToTarget)
        return CalcResourceRemaining(SMAddon.SmVessel.SelectedPartsSource, ResourceName);
      return CalcResourceRemaining(SMAddon.SmVessel.SelectedPartsTarget, ResourceName);
    }

    /// <summary>
    /// returns amount remaining in parts we are transferring to (targets relative to xferMode)
    /// </summary>
    internal double ToAmtRemaining(SMAddon.XferDirection xferMode)
    {
      if (xferMode == SMAddon.XferDirection.SourceToTarget)
        return CalcResourceRemaining(SMAddon.SmVessel.SelectedPartsTarget, ResourceName);
      return CalcResourceRemaining(SMAddon.SmVessel.SelectedPartsSource, ResourceName);
    }

    /// <summary>
    /// returns amount remaining in parts we are transferring to (targets relative to xferMode)
    /// </summary>
    internal double ToCapacityRemaining(SMAddon.XferDirection xferMode)
    {
      if (xferMode == SMAddon.XferDirection.SourceToTarget)
        return CalcRemainingCapacity(SMAddon.SmVessel.SelectedPartsTarget, ResourceName);
      return CalcRemainingCapacity(SMAddon.SmVessel.SelectedPartsSource, ResourceName);
    }

    internal static double CalcResourceRemaining(List<Part> parts, string selectedResource)
    {
      double amount = 0;
      if (parts != null)
        amount += parts.Where(part => part.Resources.Contains(selectedResource)).Sum(part => part.Resources[selectedResource].amount);
      return amount;
    }

    // these methods are for managing textbox string data entry for xfer amounts 
    internal void SetXferAmountString(string xferAmount, SMAddon.XferDirection xferMode)
    {
      // Needed so we can alter slider controls on both the source and target windows.
      if (xferMode == SMAddon.XferDirection.TargetToSource)
        StrSrcXferAmount = xferAmount;
      else
        StrTgtXferAmount = xferAmount;
    }

    internal float UpdateXferAmount(string strXferAmount, SMAddon.XferDirection xferMode)
    {
      // Needed so we can alter slider controls on both the source and target windows.
      // Also, slider requires a float number...
      float newAmount;
      if (float.TryParse(strXferAmount, out newAmount))
        if (xferMode == SMAddon.XferDirection.SourceToTarget)
          SrcXferAmount = newAmount;
        else
          TgtXferAmount = newAmount;
      return newAmount;
    }

    internal string GetStringDecimal(string strXferAmount, SMAddon.XferDirection xferMode)
    {
      if (xferMode == SMAddon.XferDirection.SourceToTarget)
      {
        if (SrcXferAmountHasDecimal)
          strXferAmount += ".";
      }
      else
      {
        if (TgtXferAmountHasDecimal)
          strXferAmount += ".";
      }
      return strXferAmount;
    }

    internal string GetStringZero(string strXferAmount, SMAddon.XferDirection xferMode)
    {
      if (xferMode == SMAddon.XferDirection.SourceToTarget)
      {
        if (SrcXferAmountHasZero)
          strXferAmount += "0";
      }
      else
      {
        if (TgtXferAmountHasZero)
          strXferAmount += "0";
      }
      return strXferAmount;
    }

    internal void SetStringZero(string strXferAmount, SMAddon.XferDirection xferMode)
    {
      if (xferMode == SMAddon.XferDirection.SourceToTarget)
      {
        if (strXferAmount.Contains(".") && strXferAmount.EndsWith("0"))
          SrcXferAmountHasZero = true;
        else
          SrcXferAmountHasZero = false;
      }
      else
      {
        if (strXferAmount.Contains(".") && strXferAmount.EndsWith("0"))
          TgtXferAmountHasZero = true;
        else
          TgtXferAmountHasZero = false;
      }
    }

    internal void SetStringDecimal(string strXferAmount, SMAddon.XferDirection xferMode)
    {
      if (xferMode == SMAddon.XferDirection.SourceToTarget)
      {
        if (strXferAmount.EndsWith(".") || strXferAmount.EndsWith(".0"))
          SrcXferAmountHasDecimal = true;
        else
          SrcXferAmountHasDecimal = false;
      }
      else
      {
        if (strXferAmount.EndsWith(".") || strXferAmount.EndsWith(".0"))
          TgtXferAmountHasDecimal = true;
        else
          TgtXferAmountHasDecimal = false;
      }
    }

    internal static double CalcResourceCapacity(List<Part> parts, string selectedResource)
    {
      double amount = 0;
      if (parts != null)
        amount += parts.Where(part => part.Resources.Contains(selectedResource)).Sum(part => part.Resources[selectedResource].maxAmount);
      return amount;
    }

    internal static double CalcRemainingCapacity(List<Part> parts, string selectedResource)
    {
      double amount = 0;
      if (parts != null)
        amount += parts.Where(part => part.Resources.Contains(selectedResource)).Sum(part => part.Resources[selectedResource].maxAmount - part.Resources[selectedResource].amount);
      return amount;
    }

    internal static double CalcMaxXferAmt(List<Part> partsFrom, List<Part> partsTo, List<string> selectedResources)
    {
      double maxXferAmount = 0;
      if (partsFrom.Count == 0 || partsTo.Count == 0 || selectedResources.Count == 0)
        maxXferAmount = 0;
      else
      {
        // First determine if there is more than one Resource to move.  get the larger number for a ratio basis.
        var resIdx = 0;
        if (selectedResources.Count > 1)
        {
          if (CalcResourceCapacity(partsTo, selectedResources[0]) < CalcResourceCapacity(partsTo, selectedResources[1]))
            resIdx = 1;
        }
        // now lets get the amount to move.  we will use this higher portion to calc lower volume ratio during move.
        var maxFromAmount = partsFrom.Sum(partFrom => partFrom.Resources[selectedResources[resIdx]].amount);
        maxXferAmount += partsTo.Sum(partTo => partTo.Resources[selectedResources[resIdx]].maxAmount - partTo.Resources[selectedResources[resIdx]].amount);

        maxXferAmount = maxXferAmount > maxFromAmount ? maxFromAmount : maxXferAmount;
        maxXferAmount = maxXferAmount < 0.0001 ? 0 : maxXferAmount;
      }
      return maxXferAmount;
    }

    internal static double CalcMaxResourceXferAmt(List<Part> @from, List<Part> to, string selectedResource)
    {
      double maxXferAmount = 0;
      if (@from.Count == 0 || to.Count == 0 || selectedResource == null || selectedResource == "")
        maxXferAmount = 0;
      else
      {
        // now lets get the amount to move.  we will use this higher portion to calc lower volume ratio during move.
        var maxSourceAmount = @from.Sum(source => source.Resources[selectedResource].amount);
        maxXferAmount += to.Sum(target => target.Resources[selectedResource].maxAmount - target.Resources[selectedResource].amount);

        maxXferAmount = maxXferAmount > maxSourceAmount ? maxSourceAmount : maxXferAmount;
        maxXferAmount = maxXferAmount < 0.0001 ? 0 : maxXferAmount;
      }
      return maxXferAmount;
    }

    private static bool IsXferComplete()
    {
      var partsFrom = SMAddon.SmVessel.SelectedPartsSource;
      var partsTo = SMAddon.SmVessel.SelectedPartsTarget;
      if (SMAddon.XferMode == SMAddon.XferDirection.TargetToSource)
      {
        partsFrom = SMAddon.SmVessel.SelectedPartsTarget;
        partsTo = SMAddon.SmVessel.SelectedPartsSource;
      }

      if (Math.Abs(CalcMaxXferAmt(partsFrom, partsTo, SMAddon.SmVessel.SelectedResources)) < SMSettings.Tolerance)
        return true;
      if (SMAddon.SmVessel.ResourcesToXfer.Count > 1)
      {
        Utilities.LogMessage("isXferComplete - A1. Resource:  " + SMAddon.SmVessel.ResourcesToXfer[0].ResourceName + ", TotalXferAmt = " + SMAddon.SmVessel.ResourcesToXfer[0].XferAmount(SMAddon.XferMode) + ", AmtXferred = " + SMAddon.SmVessel.ResourcesToXfer[0].AmtXferred, "Info", SMSettings.VerboseLogging);
        Utilities.LogMessage("isXferComplete - A2 Resource:  " + SMAddon.SmVessel.ResourcesToXfer[1].ResourceName + ", TotalXferAmt = " + SMAddon.SmVessel.ResourcesToXfer[1].XferAmount(SMAddon.XferMode) + ", AmtXferred = " + SMAddon.SmVessel.ResourcesToXfer[1].AmtXferred, "Info", SMSettings.VerboseLogging);
        if (Math.Abs(SMAddon.SmVessel.ResourcesToXfer[0].FromAmtRemaining(SMAddon.XferMode)) < SMSettings.Tolerance &&
            Math.Abs(SMAddon.SmVessel.ResourcesToXfer[1].FromAmtRemaining(SMAddon.XferMode)) < SMSettings.Tolerance)
          return true;
        if (Math.Abs(SMAddon.SmVessel.ResourcesToXfer[0].ToCapacityRemaining(SMAddon.XferMode)) < SMSettings.Tolerance &&
            Math.Abs(SMAddon.SmVessel.ResourcesToXfer[1].ToCapacityRemaining(SMAddon.XferMode)) < SMSettings.Tolerance)
          return true;
        return (SMAddon.SmVessel.ResourcesToXfer[0].AmtXferred >= SMAddon.SmVessel.ResourcesToXfer[0].XferAmount(SMAddon.XferMode) - 0.0000001) &&
               (SMAddon.SmVessel.ResourcesToXfer[1].AmtXferred >= SMAddon.SmVessel.ResourcesToXfer[1].XferAmount(SMAddon.XferMode) - 0.0000001);
      }
      else
      {
        Utilities.LogMessage("isXferComplete - B. Resource:  " + SMAddon.SmVessel.ResourcesToXfer[0].ResourceName + ", TotalXferAmt = " + SMAddon.SmVessel.ResourcesToXfer[0].XferAmount(SMAddon.XferMode) + ", AmtXferred = " + SMAddon.SmVessel.ResourcesToXfer[0].AmtXferred, "Info", SMSettings.VerboseLogging);
        if (Math.Abs(SMAddon.SmVessel.ResourcesToXfer[0].FromAmtRemaining(SMAddon.XferMode)) < SMSettings.Tolerance)
          return true;
        if (Math.Abs(SMAddon.SmVessel.ResourcesToXfer[0].ToCapacityRemaining(SMAddon.XferMode)) < SMSettings.Tolerance)
          return true;
        if (SMAddon.SmVessel.ResourcesToXfer[0].AmtXferred >= SMAddon.SmVessel.ResourcesToXfer[0].XferAmount(SMAddon.XferMode))
          return true;
        return false;
      }
    }

    // multi Resource methods
    internal static TransferResource GetXferResource(List<TransferResource> xferResources, SMAddon.XferDirection xferMode, bool isRatio = false)
    {
      if (xferResources.Count > 1)
      {
        if (isRatio)
        {
          if (xferResources[1].ToCapacity(xferMode) > xferResources[0].ToCapacity(xferMode))
            return xferResources[0];
          else
            return xferResources[1];
        }
        else
        {
          if (xferResources[0].ToCapacity(xferMode) > xferResources[1].ToCapacity(xferMode))
            return xferResources[0];
          else
            return xferResources[1];
        }
      }
      else
      {
        return xferResources[0];
      }
    }

    internal static double CalcRatio(List<TransferResource> resources, SMAddon.XferDirection xferMode)
    {
      if (resources.Count > 1)
      {
        if (resources[0].ToCapacity(xferMode) > resources[1].ToCapacity(xferMode))
          return resources[1].ToCapacity(xferMode) / resources[0].ToCapacity(xferMode);
        else
          return resources[0].ToCapacity(xferMode) / resources[1].ToCapacity(xferMode);
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
          FlowRate = SMSettings.FlowRate;

          switch (XferState)
          {
            case ResourceXferState.Off:
              // reset counters
              SMAddon.Elapsed = 0;
              foreach (var modResource in SMAddon.SmVessel.ResourcesToXfer)
              {
                modResource.AmtXferredOld = 0;
                modResource.XferTimeout = 0;
              }

              // Default sound license: CC-By-SA
              // http://www.freesound.org/people/vibe_crc/sounds/59328/
              var path1 = SMSettings.PumpSoundStart ?? "ShipManifest/Sounds/59328-1";
              var path2 = SMSettings.PumpSoundRun ?? "ShipManifest/Sounds/59328-2";
              var path3 = SMSettings.PumpSoundStop ?? "ShipManifest/Sounds/59328-3";

              // Load Sounds, and Play Sound 1
              SMAddon.LoadSounds("Pump", path1, path2, path3, SMSettings.PumpSoundVol);
              XferState = ResourceXferState.Start;
              Timestamp = DateTime.Now;
              break;

            case ResourceXferState.Start:

              // calculate Elapsed.
              SMAddon.Elapsed += (DateTime.Now - Timestamp).TotalSeconds;

              // Play run sound when start sound is nearly done. (repeats)
              if (SMAddon.Elapsed >= SMAddon.Source1.clip.length - 0.25)
              {
                SMAddon.Source2.Play();
                SMAddon.Elapsed = 0;
                XferState = ResourceXferState.Run;
              }
              break;

            case ResourceXferState.Run:

              //Run process:

              // 1.  Get Elapsed from last run
              var deltaT = (DateTime.Now - Timestamp).TotalSeconds;
              Utilities.LogMessage("ResourceTransferProcess - 1. DeltaT = " + deltaT, "Info", SMSettings.VerboseLogging);
              if (Math.Abs(deltaT) < SMSettings.Tolerance)
                return;
              Timestamp = DateTime.Now;


              var partsFrom = SMAddon.SmVessel.SelectedPartsSource;
              var partsTo = SMAddon.SmVessel.SelectedPartsTarget;
              if (SMAddon.XferMode == SMAddon.XferDirection.TargetToSource)
              {
                // From is target on Interface...
                partsFrom = SMAddon.SmVessel.SelectedPartsTarget;
                partsTo = SMAddon.SmVessel.SelectedPartsSource;
              }

              foreach (var modResource in SMAddon.SmVessel.ResourcesToXfer)
              {

                // 2.  Calculate amount to move based on flow rate and time delta
                var deltaAmt = deltaT * ActFlowRate * modResource.XferRatio;
                Utilities.LogMessage("ResourceTransferProcess - 2a. Resource:  " + modResource.ResourceName + ", DeltaAmt = " + deltaAmt, "Info", SMSettings.VerboseLogging);

                // 3.  determine if enough resouce remains to drain
                var fromAmtRemaining = modResource.FromAmtRemaining(SMAddon.XferMode);
                if (deltaAmt > fromAmtRemaining)
                  deltaAmt = fromAmtRemaining;

                // 4.  Determine if enough capacity remains to fill
                var toAmtRemianCapacity = modResource.ToCapacityRemaining(SMAddon.XferMode);
                if (deltaAmt > toAmtRemianCapacity)
                  deltaAmt = toAmtRemianCapacity;

                // 5.  Determine if move amount exceeds remaining move amount requested
                deltaAmt = deltaAmt > modResource.XferAmount(SMAddon.XferMode) - modResource.AmtXferred ? modResource.XferAmount(SMAddon.XferMode) - modResource.AmtXferred : deltaAmt;

                Utilities.LogMessage("ResourceTransferProcess - 2b. Resource:  " + modResource.ResourceName + ", Adj deltaAmt = " + deltaAmt, "Info", SMSettings.VerboseLogging);

                if (deltaAmt > 0)
                {
                  var deltaCharge = deltaAmt * SMSettings.FlowCost;
                  // 6.  Drain Charge
                  if (!ConsumeCharge(deltaCharge))
                  {
                    XferState = ResourceXferState.Stop;
                  }
                  else
                  {
                    Utilities.LogMessage("ResourceTransferProcess - 3a. Resource:  " + modResource.ResourceName + ", Xferring DeltaAmt = " + deltaAmt, "Info", SMSettings.VerboseLogging);
                    // 7.  Get list of From parts & Drain Resource
                    XferResource(partsFrom, modResource, deltaAmt, SMAddon.XferMode, true);

                    // 8.  Get list of To parts & Fill resource.
                    XferResource(partsTo, modResource, deltaAmt, SMAddon.XferMode, false);

                    // 9.  Update transferred amount.
                    modResource.AmtXferred += deltaAmt;
                  }
                }

                Utilities.LogMessage("ResourceTransferProcess - 3b. Resource:  " + modResource.ResourceName + ", AmtXferred = " + modResource.AmtXferred, "Info", SMSettings.VerboseLogging);
                Utilities.LogMessage("ResourceTransferProcess - 3c. Resource:  " + modResource.ResourceName + ", SrcAmtRemaining = " + modResource.FromAmtRemaining(SMAddon.XferMode) + ", TgtCapRemaining = " + modResource.ToCapacityRemaining(SMAddon.XferMode), "Info", SMSettings.VerboseLogging);
              }

              // 10. determine if completed.
              if (IsXferComplete())
                XferState = ResourceXferState.Stop;
              else
              {
                foreach (var modResource in SMAddon.SmVessel.ResourcesToXfer)
                {
                  // activate timeout if we are stuck in a loop.
                  if (Math.Abs(modResource.AmtXferred - modResource.AmtXferredOld) > SMSettings.Tolerance)
                  {
                    modResource.AmtXferredOld = modResource.AmtXferred;
                    modResource.XferTimeout = 0;
                  }
                  else
                  {
                    modResource.XferTimeout += 1;
                    if (modResource.XferTimeout >= 30)
                    {
                      XferState = ResourceXferState.Stop;
                      Utilities.LogMessage("ResourceTransferProcess - 4.  Timeout occurred!  Resource:  " + modResource.ResourceName + ", AmtXferred = " + modResource.AmtXferred, "Error", true);
                    }
                  }
                }
              }
              break;

            case ResourceXferState.Stop:

              // play pump shutdown.
              SMAddon.Source2.Stop();
              SMAddon.Source3.Play();
              SMAddon.Elapsed = 0;
              XferState = ResourceXferState.Off;
              foreach (var modResource in SMAddon.SmVessel.ResourcesToXfer)
              {
                modResource.AmtXferred = 0;
                modResource.SrcXferAmount = CalcMaxXferAmt(SMAddon.SmVessel.SelectedPartsSource, SMAddon.SmVessel.SelectedPartsTarget, SMAddon.SmVessel.SelectedResources);
                if (modResource.SrcXferAmount < 0.0001)
                  modResource.SrcXferAmount = 0;
                modResource.StrTgtXferAmount = modResource.SrcXferAmount.ToString(CultureInfo.InvariantCulture);
                modResource.TgtXferAmount = CalcMaxXferAmt(SMAddon.SmVessel.SelectedPartsTarget, SMAddon.SmVessel.SelectedPartsSource, SMAddon.SmVessel.SelectedResources);
                if (modResource.TgtXferAmount < 0.0001)
                  modResource.TgtXferAmount = 0;
                modResource.StrTgtXferAmount = modResource.TgtXferAmount.ToString(CultureInfo.InvariantCulture);
              }
              ResourceXferActive = false;
              break;
          }
          Utilities.LogMessage("ResourceTransferProcess - 5.  Transfer State:  " + XferState + "...", "Info", SMSettings.VerboseLogging);
        }
      }
      catch (Exception ex)
      {
        if (!SMAddon.FrameErrTripped)
        {
          Utilities.LogMessage(string.Format(" in ResourceTransferProcess (repeating error).  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
          SMAddon.Source2.Stop();
          SMAddon.Source3.Stop();
          XferState = ResourceXferState.Off;
          foreach (var modResource in SMAddon.SmVessel.ResourcesToXfer)
          {
            modResource.AmtXferred = 0;
            modResource.SrcXferAmount = CalcMaxXferAmt(SMAddon.SmVessel.SelectedPartsSource, SMAddon.SmVessel.SelectedPartsTarget, SMAddon.SmVessel.SelectedResources);
            if (modResource.SrcXferAmount < 0.0001)
              modResource.SrcXferAmount = 0;
            modResource.StrTgtXferAmount = modResource.SrcXferAmount.ToString(CultureInfo.InvariantCulture);
            modResource.TgtXferAmount = CalcMaxXferAmt(SMAddon.SmVessel.SelectedPartsTarget, SMAddon.SmVessel.SelectedPartsSource, SMAddon.SmVessel.SelectedResources);
            if (modResource.TgtXferAmount < 0.0001)
              modResource.TgtXferAmount = 0;
            modResource.StrTgtXferAmount = modResource.TgtXferAmount.ToString(CultureInfo.InvariantCulture);
          }
          ResourceXferActive = false;
          SMAddon.FrameErrTripped = true;
          // ReSharper disable once PossibleIntendedRethrow
          throw ex;
        }
      }
    }

    internal static void XferResource(List<Part> xferParts, TransferResource modResource, double xferAmount, SMAddon.XferDirection xferMode, bool drain)
    {
      // This adjusts the delta when we get to the end of the xfer.
      Utilities.LogMessage("XferResource:  " + modResource.ResourceName + " - 1. XferAmount = " + xferAmount + ", Drain? " + drain, "Info", SMSettings.VerboseLogging);

      // This var keeps track of what we actually moved..
      var xferBalance = xferAmount;
      var xferBalanceOld = xferBalance;
      var xferTimeout = 0;

      // Not all parts will have enough resource to meet the SrcPartAmt to move.   We need to account for that.
      // count up source parts with avalilabe resources. so we can devide by them
      while (xferBalance > SMSettings.Tolerance)
      {
        Utilities.LogMessage("XferResource:  " + modResource.ResourceName + " - 2. XferBalance = " + xferBalance + ", Is Source: " + drain, "Info", SMSettings.VerboseLogging);

        // Lets account for any empty/full containers
        var partCount = xferParts.Count(part => (drain && part.Resources[modResource.ResourceName].amount > 0d) || (!drain && part.Resources[modResource.ResourceName].amount < part.Resources[modResource.ResourceName].maxAmount));

        // now split up the xfer amount evenly across the number of tanks that can send/receive resources
        var partAmt = xferBalance / partCount;

        // Calculate Xfer amounts for each part and move.
        foreach (var part in xferParts)
        {
          double amtToMove;
          if (drain)
          {
            amtToMove = part.Resources[modResource.ResourceName].amount >= partAmt ? partAmt : part.Resources[modResource.ResourceName].amount;
            part.Resources[modResource.ResourceName].amount -= amtToMove;
          }
          else
          {
            var capacityAvail = part.Resources[modResource.ResourceName].maxAmount - part.Resources[modResource.ResourceName].amount;
            amtToMove = capacityAvail >= partAmt ? partAmt : capacityAvail;
            part.Resources[modResource.ResourceName].amount += amtToMove;
          }
          Utilities.LogMessage("XferResource:  " + modResource.ResourceName + " - 3. AmtToMove = " + amtToMove + ", Drain: " + drain, "Info", SMSettings.VerboseLogging);
          // Report ramaining balance after Transfer.
          xferBalance -= amtToMove;
        }
        // account for rounding and double resolution issues 
        if (xferBalance <= 0.0000001d)
          break;
        if (Math.Abs(xferBalance - xferBalanceOld) > SMSettings.Tolerance)
        {
          xferBalanceOld = xferBalance;
          xferTimeout = 0;
        }
        else if (Math.Abs(xferBalance - xferBalanceOld) < SMSettings.Tolerance)
        {
          xferTimeout += 1;
          if (xferTimeout >= 30)
          {
            Utilities.LogMessage("XferResource  Timeout!  Resource:  " + modResource.ResourceName + " - 3. XferBalance = " + xferBalance + ", Drain: " + drain, "Error", true);
            break;
          }
        }
      }
    }

    private static bool ConsumeCharge(double deltaCharge)
    {
      if (!SMAddon.SmVessel.SelectedResources.Contains("ElectricCharge") && SMSettings.EnableXferCost)
      {
        foreach (var iPart in SMAddon.SmVessel.PartsByResource["ElectricCharge"])
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
      XferState = ResourceXferState.Stop;
    }

    internal enum ResourceXferState
    {
      Off,
      Start,
      Run,
      Stop
    }
    #endregion
  }
}
