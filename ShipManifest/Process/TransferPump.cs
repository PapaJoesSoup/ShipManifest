using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ShipManifest.Process
{
  /// <summary>
  /// This class supports the transfer/dump/fill of a desired resource of ResourceType.Other
  /// </summary>
  class TransferPump
  {
    // used during transfer operations.
    internal static PumpState ProcessState = PumpState.Off;
    internal static bool PumpXferActive;
    internal static bool PumpDumpActive;
    internal static DateTime Timestamp;

    #region Properties

    internal TransferPump.PumpType Pump_Type = TransferPump.PumpType.SourceToTarget;
    internal PumpState CurrPumpState = PumpState.Off;
    internal List<Part> SourceParts;
    internal List<Part> TargetParts;
    internal List<Part> DumpParts;

    internal double FlowRate = SMSettings.FlowRate;
    internal int FlowTime = SMSettings.MaxFlowTimeSec;
    internal double ActFlowRate = 0;

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

    internal double DumpAmount;
    private List<Part> fromParts;
    private List<Part> toParts;

    #endregion

    #region Constructors

    internal TransferPump() { }

    internal TransferPump(string resourceName, TransferPump.PumpType pumpType)
    {
      ResourceName = resourceName;
      Pump_Type = pumpType;
    }

    #endregion

    #region Methods

    // Action Methods
    internal static void ProcessTransfer()
    {
      // this task runs every Update when active.
      try
      {
        if (PumpXferActive)
        {
          switch (ProcessState)
          {
            case PumpState.Off:
              // reset counters
              // Default sound license: CC-By-SA
              // http://www.freesound.org/people/vibe_crc/sounds/59328/
              var path1 = SMSettings.PumpSoundStart ?? "ShipManifest/Sounds/59328-1";
              var path2 = SMSettings.PumpSoundRun ?? "ShipManifest/Sounds/59328-2";
              var path3 = SMSettings.PumpSoundStop ?? "ShipManifest/Sounds/59328-3";

              SMAddon.Elapsed = 0;

              foreach (var pump in SMAddon.SmVessel.TransferPumps)
              {
                pump.FlowRate = SMSettings.FlowRate;
                pump.AmtXferredOld = 0;
                pump.XferTimeout = 0;
                pump.CurrPumpState = PumpState.Start;
              }

              // Load Sounds, and Play Sound 1
              SMAddon.LoadSounds("Pump", path1, path2, path3, SMSettings.PumpSoundVol);
              ProcessState = PumpState.Start;
              Timestamp = DateTime.Now;
              break;

            case PumpState.Start:

              // calculate Elapsed.
              SMAddon.Elapsed += (DateTime.Now - Timestamp).TotalSeconds;

              // Play run sound when start sound is nearly done. (repeats)
              if (SMAddon.Elapsed >= SMAddon.Source1.clip.length - 0.25)
              {
                SMAddon.Source2.Play();
                SMAddon.Elapsed = 0;
                ProcessState = PumpState.Run;
                foreach (var pump in SMAddon.SmVessel.TransferPumps)
                {
                  pump.CurrPumpState = PumpState.Run;
                }
              }
              break;

            case PumpState.Run:

              //Run process:

              // 1.  Get Elapsed from last run
              var deltaT = (DateTime.Now - Timestamp).TotalSeconds;
              Utilities.LogMessage("ResourceTransferProcess - 1. DeltaT = " + deltaT, "Info", SMSettings.VerboseLogging);
              if (Math.Abs(deltaT) < SMSettings.Tolerance)
                return;
              Timestamp = DateTime.Now;

              foreach (var pump in SMAddon.SmVessel.TransferPumps)
              {
                // Is this pump still running?
                if (pump.CurrPumpState != PumpState.Run) continue;
                // Set parts from and to based on direction and immutable source and target as they relate to the user interface
                var partsFrom = pump.SourceParts;
                var partsTo = pump.TargetParts;
                if (SMAddon.ActivePumpType == TransferPump.PumpType.TargetToSource)
                {
                  // We actually clicked buttons on target interface...
                  partsFrom = pump.TargetParts;
                  partsTo = pump.SourceParts;
                }

                // 2.  Calculate amount to move based on flow rate and time delta
                var deltaAmt = deltaT * pump.ActFlowRate * pump.XferRatio;
                Utilities.LogMessage("ResourceTransferProcess - 2a. Resource:  " + pump.ResourceName + ", DeltaAmt = " + deltaAmt, "Info", SMSettings.VerboseLogging);

                // 3.  determine if enough resouce remains to drain
                var fromAmtRemaining = pump.FromAmtRemaining(SMAddon.ActivePumpType);
                if (deltaAmt > fromAmtRemaining)
                  deltaAmt = fromAmtRemaining;

                // 4.  Determine if enough capacity remains to fill (transfer only
                var toAmtRemianCapacity = pump.ToCapacityRemaining(SMAddon.ActivePumpType);
                if (deltaAmt > toAmtRemianCapacity) deltaAmt = toAmtRemianCapacity;

                // 5.  Determine if move amount exceeds remaining move amount requested
                deltaAmt = deltaAmt > pump.XferAmount(SMAddon.ActivePumpType) - pump.AmtXferred ? pump.XferAmount(SMAddon.ActivePumpType) - pump.AmtXferred : deltaAmt;

                Utilities.LogMessage("ResourceTransferProcess - 2b. Resource:  " + pump.ResourceName + ", Adj deltaAmt = " + deltaAmt, "Info", SMSettings.VerboseLogging);

                if (deltaAmt > 0)
                {
                  var deltaCharge = deltaAmt * SMSettings.FlowCost;
                  // 6.  Drain Charge
                  if (!ConsumeCharge(deltaCharge))
                  {
                    ProcessState = PumpState.Stop;
                  }
                  else
                  {
                    Utilities.LogMessage("ResourceTransferProcess - 3a. Resource:  " + pump.ResourceName + ", Xferring DeltaAmt = " + deltaAmt, "Info", SMSettings.VerboseLogging);
                    // 7.  Get list of From parts & Drain Resource
                    XferResource(partsFrom, pump, deltaAmt, true);

                    // 8.  Get list of To parts & Fill resource.
                    XferResource(partsTo, pump, deltaAmt, false);

                    // 9.  Update transferred amount.
                    pump.AmtXferred += deltaAmt;

                    // 10. determine if completed.
                    if (IsXferComplete(pump))
                    {
                      pump.CurrPumpState = PumpState.Stop;
                    }
                  }
                }
                Utilities.LogMessage("ResourceTransferProcess - 3b. Resource:  " + pump.ResourceName + ", AmtXferred = " + pump.AmtXferred, "Info", SMSettings.VerboseLogging);
                Utilities.LogMessage("ResourceTransferProcess - 3c. Resource:  " + pump.ResourceName + ", SrcAmtRemaining = " + pump.FromAmtRemaining(SMAddon.ActivePumpType) + ", TgtCapRemaining = " + pump.ToCapacityRemaining(SMAddon.ActivePumpType), "Info", SMSettings.VerboseLogging);
              }

              if (IsXferComplete()) ProcessState = PumpState.Stop;
              break;

            case PumpState.Stop:

              // play pump shutdown.
              SMAddon.Source2.Stop();
              SMAddon.Source3.Play();
              SMAddon.Elapsed = 0;
              ProcessState = PumpState.Off;
              SMAddon.SmVessel.TransferPumps.Clear();
              PumpXferActive = false;
              break;
          }
          Utilities.LogMessage("ResourceTransferProcess - 5.  Transfer State:  " + ProcessState + "...", "Info", SMSettings.VerboseLogging);
        }
      }
      catch (Exception ex)
      {
        if (!SMAddon.FrameErrTripped)
        {
          Utilities.LogMessage(string.Format(" in ResourceTransferProcess (repeating error).  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
          SMAddon.Source2.Stop();
          SMAddon.Source3.Stop();
          ProcessState = PumpState.Off;
          foreach (var pump in SMAddon.SmVessel.TransferPumps)
          {
            pump.AmtXferred = 0;
            pump.SrcXferAmount = CalcMaxXferAmt(SMAddon.SmVessel.SelectedPartsSource, SMAddon.SmVessel.SelectedPartsTarget, SMAddon.SmVessel.SelectedResources);
            if (pump.SrcXferAmount < 0.0001)
              pump.SrcXferAmount = 0;
            pump.StrTgtXferAmount = pump.SrcXferAmount.ToString(CultureInfo.InvariantCulture);
            pump.TgtXferAmount = CalcMaxXferAmt(SMAddon.SmVessel.SelectedPartsTarget, SMAddon.SmVessel.SelectedPartsSource, SMAddon.SmVessel.SelectedResources);
            if (pump.TgtXferAmount < 0.0001)
              pump.TgtXferAmount = 0;
            pump.StrTgtXferAmount = pump.TgtXferAmount.ToString(CultureInfo.InvariantCulture);
          }
          PumpXferActive = false;
          SMAddon.FrameErrTripped = true;
          // ReSharper disable once PossibleIntendedRethrow
          throw ex;
        }
      }
    }

    internal static void ProcessDump()
    {
      // this task runs every Update when active.
      try
      {
        if (PumpDumpActive)
        {
          switch (ProcessState)
          {
            case PumpState.Off:
              // reset counters
              SMAddon.Elapsed = 0;
              foreach (var pump in SMAddon.SmVessel.TransferPumps)
              {
                pump.FlowRate = SMSettings.FlowRate;
                pump.AmtXferredOld = 0;
                pump.XferTimeout = 0;
              }

              // Default sound license: CC-By-SA
              // http://www.freesound.org/people/vibe_crc/sounds/59328/
              var path1 = SMSettings.PumpSoundStart ?? "ShipManifest/Sounds/59328-1";
              var path2 = SMSettings.PumpSoundRun ?? "ShipManifest/Sounds/59328-2";
              var path3 = SMSettings.PumpSoundStop ?? "ShipManifest/Sounds/59328-3";

              // Load Sounds, and Play Sound 1
              SMAddon.LoadSounds("Pump", path1, path2, path3, SMSettings.PumpSoundVol);
              ProcessState = PumpState.Start;
              Timestamp = DateTime.Now;
              break;

            case PumpState.Start:

              // calculate Elapsed.
              SMAddon.Elapsed += (DateTime.Now - Timestamp).TotalSeconds;

              // Play run sound when start sound is nearly done. (repeats)
              if (SMAddon.Elapsed >= SMAddon.Source1.clip.length - 0.25)
              {
                SMAddon.Source2.Play();
                SMAddon.Elapsed = 0;
                ProcessState = PumpState.Run;
              }
              break;

            case PumpState.Run:

              //Run process:

              // 1.  Get Elapsed from last run
              var deltaT = (DateTime.Now - Timestamp).TotalSeconds;
              Utilities.LogMessage("ResourceDumpProcess - 1. DeltaT = " + deltaT, "Info", SMSettings.VerboseLogging);
              if (Math.Abs(deltaT) < SMSettings.Tolerance) return;
              Timestamp = DateTime.Now;

              var partsFrom = SMAddon.SmVessel.SelectedResourcesParts;

              foreach (var pump in SMAddon.SmVessel.TransferPumps)
              {
                // 2.  Calculate amount to move based on flow rate and time delta
                var deltaAmt = deltaT * pump.ActFlowRate * pump.XferRatio;
                Utilities.LogMessage("ResourceDumpProcess - 2. Resource:  " + pump.ResourceName + ", DeltaAmt = " + deltaAmt, "Info", SMSettings.VerboseLogging);

                // 3.  determine if enough resouce remains to drain
                var fromAmtRemaining = TransferPump.CalcRemainingResource(partsFrom, pump.ResourceName);

                // 4.  Determine if move amount exceeds remaining amount in tank(s)
                deltaAmt = deltaAmt > fromAmtRemaining ? fromAmtRemaining : deltaAmt;

                Utilities.LogMessage("ResourceDumpProcess - 4. Resource:  " + pump.ResourceName + ", Adj deltaAmt = " + deltaAmt, "Info", SMSettings.VerboseLogging);

                if (deltaAmt > 0)
                {
                  var deltaCharge = deltaAmt * SMSettings.FlowCost;
                  // 6.  Drain Charge
                  if (!ConsumeCharge(deltaCharge))
                  {
                    ProcessState = PumpState.Stop;
                  }
                  else
                  {
                    Utilities.LogMessage("ResourceDumpProcess - 7. Resource:  " + pump.ResourceName + ", Xferring DeltaAmt = " + deltaAmt, "Info", SMSettings.VerboseLogging);
                    // 7.  Get list of From parts & Drain Resource
                    DumpResource(partsFrom, pump);
                  }
                }

                Utilities.LogMessage("ResourceDumpProcess - 8. Resource:  " + pump.ResourceName + ", SrcAmtRemaining = " + pump.FromAmtRemaining(SMAddon.ActivePumpType), "Info", SMSettings.VerboseLogging);
              }

              // 10. determine if completed.
              if (IsDumpComplete(partsFrom))
                ProcessState = PumpState.Stop;
              else
              {
                foreach (var pump in SMAddon.SmVessel.TransferPumps)
                {
                  // activate timeout if we are stuck in a loop.
                  if (Math.Abs(pump.FromAmtRemaining(SMAddon.ActivePumpType)) > SMSettings.Tolerance)
                  {
                    pump.XferTimeout = 0;
                  }
                  else
                  {
                    pump.XferTimeout += 1;
                    if (pump.XferTimeout >= 30)
                    {
                      ProcessState = PumpState.Stop;
                      Utilities.LogMessage("ResourceDumpProcess - 9.  Timeout occurred!  Resource:  " + pump.ResourceName, "Error", true);
                    }
                  }
                }
              }
              break;

            case PumpState.Stop:

              EndDumpProcess();
              break;
          }
          Utilities.LogMessage("ResourceDumpProcess - 10.  Transfer State:  " + ProcessState + "...", "Info", SMSettings.VerboseLogging);
        }
      }
      catch (Exception ex)
      {
        if (!SMAddon.FrameErrTripped)
        {
          Utilities.LogMessage(string.Format(" in ResourceDumpProcess (repeating error).  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
          EndDumpProcess();
          SMAddon.FrameErrTripped = true;
          // ReSharper disable once PossibleIntendedRethrow
          throw ex;
        }
      }
    }

    // methods to derive data from part selection lists and xferMode
    internal double XferAmount(TransferPump.PumpType pumpType)
    {
      // which way we going?
      switch (pumpType)
      {
        case TransferPump.PumpType.SourceToTarget:
          return SrcXferAmount;
          break;
        case TransferPump.PumpType.TargetToSource:
          return TgtXferAmount;
          break;
        case TransferPump.PumpType.Dump:
          return DumpAmount;
          break;
      }
      return 0;
    }
    internal double XferAmount()
    {
      return XferAmount(Pump_Type);
    }

    /// <summary>
    /// Returns capacity of the parts we are transferrring from (sources relative to xferMode)
    /// </summary>
    internal double FromCapacity(TransferPump.PumpType pumpType)
    {
      switch (pumpType)
      {
        case TransferPump.PumpType.SourceToTarget:
          return CalcResourceCapacity(SourceParts, ResourceName);
          break;
        case TransferPump.PumpType.TargetToSource:
          return CalcResourceCapacity(TargetParts, ResourceName);
          break;
        case TransferPump.PumpType.Dump:
          return CalcResourceCapacity(DumpParts, ResourceName);
          break;
      }
      return 0;
    }
    internal double FromCapacity()
    {
      return FromCapacity(Pump_Type);
    }

    /// <summary>
    /// returns capacity of parts we are transferring to (targets relative to xferMode)
    /// </summary>
    internal double ToCapacity(TransferPump.PumpType pumpType)
    {
      switch (pumpType)
      {
        case TransferPump.PumpType.SourceToTarget:
          return CalcResourceCapacity(TargetParts, ResourceName);
          break;
        case TransferPump.PumpType.TargetToSource:
          return CalcResourceCapacity(SourceParts, ResourceName);
          break;
        case TransferPump.PumpType.Dump:
          return CalcResourceCapacity(DumpParts, ResourceName);
          break;
      }
      return 0;
    }
    internal double ToCapacity()
    {
      return ToCapacity(Pump_Type);
    }

    /// <summary>
    /// returns amount remaining in parts we are transferrring from (sources relative to XferMode)
    /// </summary>
    internal double FromAmtRemaining(TransferPump.PumpType pumpType)
    {
      switch (pumpType)
      {
        case TransferPump.PumpType.SourceToTarget:
          return CalcRemainingResource(SourceParts, ResourceName);
          break;
        case TransferPump.PumpType.TargetToSource:
          return CalcRemainingResource(TargetParts, ResourceName);
          break;
        case TransferPump.PumpType.Dump:
          return CalcRemainingResource(DumpParts, ResourceName);
          break;
      }
      return 0;
    }
    internal double FromAmtRemaining()
    {
      return FromAmtRemaining(Pump_Type);
    }

    /// <summary>
    /// returns amount remaining in parts we are transferring to (targets relative to xferMode)
    /// </summary>
    internal double ToAmtRemaining(TransferPump.PumpType pumpType)
    {
      switch (pumpType)
      {
        case TransferPump.PumpType.SourceToTarget:
          return CalcRemainingResource(TargetParts, ResourceName);
          break;
        case TransferPump.PumpType.TargetToSource:
          return CalcRemainingResource(SourceParts, ResourceName);
          break;
        case TransferPump.PumpType.Dump:
          return CalcRemainingResource(DumpParts, ResourceName);
          break;
      }
      return 0;
    }
    internal double ToAmtRemaining()
    {
      return ToAmtRemaining(Pump_Type);
    }

    /// <summary>
    /// returns amount remaining in parts we are transferring to (targets relative to xferMode)
    /// </summary>
    internal double ToCapacityRemaining(TransferPump.PumpType pumpType)
    {
      switch (pumpType)
      {
        case TransferPump.PumpType.SourceToTarget:
          return CalcRemainingCapacity(TargetParts, ResourceName);
          break;
        case TransferPump.PumpType.TargetToSource:
          return CalcRemainingCapacity(SourceParts, ResourceName);
          break;
        case TransferPump.PumpType.Dump:
          return CalcRemainingCapacity(DumpParts, ResourceName);
          break;
      }
      return 0;
    }
    internal double ToCapacityRemaining()
    {
      return ToCapacityRemaining(Pump_Type);
    }

    // these methods are for managing textbox string data entry for xfer amounts 
    internal void SetXferAmountString(string xferAmount, TransferPump.PumpType xferMode)
    {
      // Needed so we can alter slider controls on both the source and target windows.
      switch (xferMode)
      {
        case TransferPump.PumpType.SourceToTarget:
          StrTgtXferAmount = xferAmount;
          break;
        case TransferPump.PumpType.TargetToSource:
          StrSrcXferAmount = xferAmount;
          break;
      }
    }

    internal float UpdateXferAmount(string strXferAmount, TransferPump.PumpType xferMode)
    {
      // Needed so we can alter slider controls on both the source and target windows.
      // Also, slider requires a float number...
      float newAmount;
      if (float.TryParse(strXferAmount, out newAmount))
        switch (xferMode)
        {
          case TransferPump.PumpType.SourceToTarget:
            SrcXferAmount = newAmount;
            break;
          case TransferPump.PumpType.TargetToSource:
            TgtXferAmount = newAmount;
            break;
        }
      return newAmount;
    }

    internal string GetStringDecimal(string strXferAmount, TransferPump.PumpType xferMode)
    {
      switch (xferMode)
      {
        case TransferPump.PumpType.SourceToTarget:
          if (SrcXferAmountHasDecimal) strXferAmount += ".";
          break;
        case TransferPump.PumpType.TargetToSource:
          if (TgtXferAmountHasDecimal) strXferAmount += ".";
          break;
      }
      return strXferAmount;
    }

    internal string GetStringZero(string strXferAmount, TransferPump.PumpType xferMode)
    {
      switch (xferMode)
      {
        case TransferPump.PumpType.SourceToTarget:
          if (SrcXferAmountHasZero) strXferAmount += "0";
          break;
        case TransferPump.PumpType.TargetToSource:
          if (TgtXferAmountHasZero) strXferAmount += "0";
          break;
      }
      return strXferAmount;
    }

    internal void SetStringZero(string strXferAmount, TransferPump.PumpType xferMode)
    {
      // sets static vars at a higher scope than calling routine.
      switch (xferMode)
      {
        case TransferPump.PumpType.SourceToTarget:
          if (strXferAmount.Contains(".") && strXferAmount.EndsWith("0"))
            SrcXferAmountHasZero = true;
          else
            SrcXferAmountHasZero = false;
          break;
        case TransferPump.PumpType.TargetToSource:
          if (strXferAmount.Contains(".") && strXferAmount.EndsWith("0"))
            TgtXferAmountHasZero = true;
          else
            TgtXferAmountHasZero = false;
          break;
      }
    }

    internal void SetStringDecimal(string strXferAmount, TransferPump.PumpType xferMode)
    {
      // sets static vars at a higher scope than calling routine.
      switch (xferMode)
      {
        case TransferPump.PumpType.SourceToTarget:
          if (strXferAmount.EndsWith(".") || strXferAmount.EndsWith(".0"))
            SrcXferAmountHasDecimal = true;
          else
            SrcXferAmountHasDecimal = false;
          break;
        case TransferPump.PumpType.TargetToSource:
          if (strXferAmount.EndsWith(".") || strXferAmount.EndsWith(".0"))
            TgtXferAmountHasDecimal = true;
          else
            TgtXferAmountHasDecimal = false;
          break;
      }
    }

    internal static double CalcRemainingResource(List<Part> parts, string selectedResource)
    {
      double amount = 0;
      if (parts != null)
        amount += parts.Where(part => part.Resources.Contains(selectedResource)).Sum(part => part.Resources[selectedResource].amount);
      return amount;
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

    private static bool IsDumpComplete(List<Part> parts)
    {
      foreach (var modResource in SMAddon.SmVessel.TransferPumps)
      {
        if (Math.Abs(CalcRemainingResource(parts, modResource.ResourceName)) > SMSettings.Tolerance)
        {
          return false;
        }
      }
      return true;
    }

    private static bool IsXferComplete()
    {
      return (from pump in SMAddon.SmVessel.TransferPumps where pump.CurrPumpState != PumpState.Stop select pump).Any();
    }

    private static bool IsXferComplete(TransferPump pump)
    {
      var partsFrom = SMAddon.ActivePumpType == TransferPump.PumpType.SourceToTarget ? pump.SourceParts : pump.TargetParts;
      var partsTo = SMAddon.ActivePumpType == TransferPump.PumpType.SourceToTarget ? pump.TargetParts : pump.SourceParts;
      Utilities.LogMessage("isXferComplete - B. Resource:  " + pump.ResourceName + ", TotalXferAmt = " + pump.XferAmount(SMAddon.ActivePumpType) + ", AmtXferred = " + pump.AmtXferred, "Info", SMSettings.VerboseLogging);

      if (Math.Abs(CalcMaxXferAmt(partsFrom, partsTo, new List<string>() {pump.ResourceName})) < SMSettings.Tolerance) return true;
      if (Math.Abs(pump.FromAmtRemaining(SMAddon.ActivePumpType)) < SMSettings.Tolerance) return true;
      if (Math.Abs(pump.ToCapacityRemaining(SMAddon.ActivePumpType)) < SMSettings.Tolerance) return true;
      if (pump.AmtXferred >= pump.XferAmount(SMAddon.ActivePumpType)) return true;
      return false;
    }

    // multi Resource methods
    internal static List<TransferPump> CreateTransferPumps(List<string> resourceNames, TransferPump.PumpType pumpType)
    {
      // Now lets update the Xfer Objects...
      List<TransferPump> newList = new List<TransferPump>();
      newList.Clear();
      if (SMConditions.AreSelectedResourcesTypeOther(resourceNames))
      {
        foreach (var pump in SMAddon.SmVessel.SelectedResources.Select(resource => new TransferPump(resource, pumpType)
        {
          SrcXferAmount =
            TransferPump.CalcMaxResourceXferAmt(SMAddon.SmVessel.SelectedPartsSource,
              SMAddon.SmVessel.SelectedPartsTarget, resource),
          TgtXferAmount =
            TransferPump.CalcMaxResourceXferAmt(SMAddon.SmVessel.SelectedPartsTarget,
              SMAddon.SmVessel.SelectedPartsSource, resource)
        }))
        {
          newList.Add(pump);
        }
      }
      return newList;
    }

    internal static TransferPump GetXferPump(List<TransferPump> xferPumps, TransferPump.PumpType pumpType, bool isRatio = false)
    {
      if (xferPumps.Count > 1)
      {
        if (isRatio)
        {
          if (xferPumps[1].ToCapacity(pumpType) > xferPumps[0].ToCapacity(pumpType))
            return xferPumps[0];
          else
            return xferPumps[1];
        }
        else
        {
          if (xferPumps[0].ToCapacity(pumpType) > xferPumps[1].ToCapacity(pumpType))
            return xferPumps[0];
          else
            return xferPumps[1];
        }
      }
      else
      {
        return xferPumps[0];
      }
    }

    internal static double CalcRatio(List<TransferPump> pumps, TransferPump.PumpType pumpType)
    {
      if (pumps.Count > 1)
      {
        if (pumps[0].ToCapacity(pumpType) > pumps[1].ToCapacity(pumpType))
          return pumps[1].ToCapacity(pumpType) / pumps[0].ToCapacity(pumpType);
        else
          return pumps[0].ToCapacity(pumpType) / pumps[1].ToCapacity(pumpType);
      }
      else
        return 1;
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

    internal static void XferResource(List<Part> xferParts, TransferPump pump, double xferAmount, bool drain)
    {
      // This adjusts the delta when we get to the end of the xfer.
      Utilities.LogMessage("XferResource:  " + pump.ResourceName + " - 1. XferAmount = " + xferAmount + ", Drain? " + drain, "Info", SMSettings.VerboseLogging);

      // This var keeps track of what we actually moved..
      var xferBalance = xferAmount;
      var xferBalanceOld = xferBalance;
      var xferTimeout = 0;

      // Not all parts will have enough resource to meet the SrcPartAmt to move.   We need to account for that.
      // count up source parts with avalilabe resources. so we can devide by them
      while (xferBalance > SMSettings.Tolerance)
      {
        Utilities.LogMessage("XferResource:  " + pump.ResourceName + " - 2. XferBalance = " + xferBalance + ", Is Source: " + drain, "Info", SMSettings.VerboseLogging);

        // Lets account for any empty/full containers
        var partCount = xferParts.Count(part => (drain && part.Resources[pump.ResourceName].amount > 0d) || (!drain && part.Resources[pump.ResourceName].amount < part.Resources[pump.ResourceName].maxAmount));

        // now split up the xfer amount evenly across the number of tanks that can send/receive resources
        var partAmt = xferBalance / partCount;

        // Calculate Xfer amounts for each part and move.
        foreach (var part in xferParts)
        {
          double amtToMove;
          if (drain)
          {
            amtToMove = part.Resources[pump.ResourceName].amount >= partAmt ? partAmt : part.Resources[pump.ResourceName].amount;
            part.Resources[pump.ResourceName].amount -= amtToMove;
          }
          else
          {
            var capacityAvail = part.Resources[pump.ResourceName].maxAmount - part.Resources[pump.ResourceName].amount;
            amtToMove = capacityAvail >= partAmt ? partAmt : capacityAvail;
            part.Resources[pump.ResourceName].amount += amtToMove;
          }
          Utilities.LogMessage("XferResource:  " + pump.ResourceName + " - 3. AmtToMove = " + amtToMove + ", Drain: " + drain, "Info", SMSettings.VerboseLogging);
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
            Utilities.LogMessage("XferResource  Timeout!  Resource:  " + pump.ResourceName + " - 3. XferBalance = " + xferBalance + ", Drain: " + drain, "Error", true);
            break;
          }
        }
      }
    }

    internal static void DumpResource(TransferPump pump)
    {
      // This adjusts the delta when we get to the end of the xfer.
      var dumpAmount = CalcRemainingResource(pump.DumpParts, pump.ResourceName);
      Utilities.LogMessage("DumpResource:  " + pump.ResourceName + " - 1. DumpAmount = " + dumpAmount, "Info", SMSettings.VerboseLogging);

      // This var keeps track of what we actually moved..
      var dumpBalance = dumpAmount;
      var dumpBalanceOld = dumpBalance;
      var dumpTimeout = 0;

      // Not all parts will have enough resource to meet the SrcPartAmt to move.   We need to account for that.
      // count up source parts with avalilabe resources. so we can devide by them
      while (dumpBalance > SMSettings.Tolerance)
      {
        Utilities.LogMessage("DumpResource:  " + pump.ResourceName + " - 2. DumpBalance = " + dumpBalance, "Info", SMSettings.VerboseLogging);

        // Lets account for any empty/full containers
        var partCount = pump.DumpParts.Count(part => part.Resources[pump.ResourceName].amount > 0d);

        // now split up the xfer amount evenly across the number of tanks that can send/receive resources
        var partAmt = dumpBalance / partCount;

        // Calculate Xfer amounts for each part and move.
        foreach (var part in pump.DumpParts)
        {
          double amtToMove;
          amtToMove = part.Resources[pump.ResourceName].amount >= partAmt ? partAmt : part.Resources[pump.ResourceName].amount;
          part.Resources[pump.ResourceName].amount -= amtToMove;
          Utilities.LogMessage("XferResource:  " + pump.ResourceName + " - 3. AmtToMove = " + amtToMove, "Info", SMSettings.VerboseLogging);

          // Report ramaining balance after Transfer.
          dumpBalance -= amtToMove;
        }
        // account for rounding and double resolution issues 
        if (dumpBalance <= 0.0000001d)
          break;
        if (Math.Abs(dumpBalance - dumpBalanceOld) > SMSettings.Tolerance)
        {
          dumpBalanceOld = dumpBalance;
          dumpTimeout = 0;
        }
        else if (Math.Abs(dumpBalance - dumpBalanceOld) < SMSettings.Tolerance)
        {
          dumpTimeout += 1;
          if (dumpTimeout >= 30)
          {
            Utilities.LogMessage("XferResource  Timeout!  Resource:  " + pump.ResourceName + " - 3. XferBalance = " + dumpBalance, "Error", true);
            break;
          }
        }
      }
    }

    internal static void DumpResource(List<Part> xferParts, TransferPump pump)
    {
      // This adjusts the delta when we get to the end of the xfer.
      var dumpAmount = CalcRemainingResource(xferParts, pump.ResourceName);
      Utilities.LogMessage("DumpResource:  " + pump.ResourceName + " - 1. DumpAmount = " + dumpAmount, "Info", SMSettings.VerboseLogging);

      // This var keeps track of what we actually moved..
      var dumpBalance = dumpAmount;
      var dumpBalanceOld = dumpBalance;
      var dumpTimeout = 0;

      // Not all parts will have enough resource to meet the SrcPartAmt to move.   We need to account for that.
      // count up source parts with avalilabe resources. so we can devide by them
      while (dumpBalance > SMSettings.Tolerance)
      {
        Utilities.LogMessage("DumpResource:  " + pump.ResourceName + " - 2. DumpBalance = " + dumpBalance, "Info", SMSettings.VerboseLogging);

        // Lets account for any empty/full containers
        var partCount = xferParts.Count(part => part.Resources[pump.ResourceName].amount > 0d);

        // now split up the xfer amount evenly across the number of tanks that can send/receive resources
        var partAmt = dumpBalance / partCount;

        // Calculate Xfer amounts for each part and move.
        foreach (var part in xferParts)
        {
          double amtToMove;
          amtToMove = part.Resources[pump.ResourceName].amount >= partAmt ? partAmt : part.Resources[pump.ResourceName].amount;
          part.Resources[pump.ResourceName].amount -= amtToMove;
          Utilities.LogMessage("XferResource:  " + pump.ResourceName + " - 3. AmtToMove = " + amtToMove, "Info", SMSettings.VerboseLogging);

          // Report ramaining balance after Transfer.
          dumpBalance -= amtToMove;
        }
        // account for rounding and double resolution issues 
        if (dumpBalance <= 0.0000001d)
          break;
        if (Math.Abs(dumpBalance - dumpBalanceOld) > SMSettings.Tolerance)
        {
          dumpBalanceOld = dumpBalance;
          dumpTimeout = 0;
        }
        else if (Math.Abs(dumpBalance - dumpBalanceOld) < SMSettings.Tolerance)
        {
          dumpTimeout += 1;
          if (dumpTimeout >= 30)
          {
            Utilities.LogMessage("XferResource  Timeout!  Resource:  " + pump.ResourceName + " - 3. XferBalance = " + dumpBalance, "Error", true);
            break;
          }
        }
      }
    }

    private static void UpdateDisplayTransferPumpState()
    {
      foreach (var pump in SMAddon.SmVessel.TransferPumps)
      {
        pump.AmtXferred = 0;
        pump.SrcXferAmount = CalcMaxXferAmt(pump.SourceParts, pump.TargetParts, new List<string>() { pump.ResourceName });
        if (pump.SrcXferAmount < 0.0001)
          pump.SrcXferAmount = 0;
        pump.StrTgtXferAmount = pump.SrcXferAmount.ToString(CultureInfo.InvariantCulture);
        pump.TgtXferAmount = CalcMaxXferAmt(SMAddon.SmVessel.SelectedPartsTarget, SMAddon.SmVessel.SelectedPartsSource, SMAddon.SmVessel.SelectedResources);
        if (pump.TgtXferAmount < 0.0001)
          pump.TgtXferAmount = 0;
        pump.StrTgtXferAmount = pump.TgtXferAmount.ToString(CultureInfo.InvariantCulture);
      }
    }

    private static void EndDumpProcess()
    {
      // play pump shutdown.
      SMAddon.Source2.Stop();
      SMAddon.Source3.Play();
      SMAddon.Elapsed = 0;
      ProcessState = PumpState.Off;
      SMAddon.SmVessel.TransferPumps.Clear();
      PumpDumpActive = false;
    }

    internal static void AbortTransferProcess()
    {
      // set a state var and wait for the next update to pick it up.
      ProcessState = PumpState.Stop;
    }

    internal enum PumpState
    {
      Off,
      Start,
      Run,
      Stop
    }

    internal enum PumpType
    {
      SourceToTarget,
      TargetToSource,
      Dump
    }

    #endregion
  }
}
