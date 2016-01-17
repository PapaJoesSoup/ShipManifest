using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ShipManifest.Windows;

namespace ShipManifest.Process
{
  /// <summary>
  /// This class supports the transfer/dump/fill of a desired resource of ResourceType.Other
  /// </summary>
  class TransferPump
  {
    // Default sound license: CC-By-SA
    // http://www.freesound.org/people/vibe_crc/sounds/59328/
    internal static string path1 = SMSettings.PumpSoundStart ?? "ShipManifest/Sounds/59328-1";
    internal static string path2 = SMSettings.PumpSoundRun ?? "ShipManifest/Sounds/59328-2";
    internal static string path3 = SMSettings.PumpSoundStop ?? "ShipManifest/Sounds/59328-3";

    // used during transfer operations.
    internal static bool PumpActive;

    #region Properties
    // Constructor properties
    internal string ResourceName = "";
    internal TypePump PumpType = TypePump.SourceToTarget;
    internal TriggerButton PumpTrigger;
    internal double PumpAmount;
    internal uint PumpId;

    // Sources (should be assigned at instantiation or right after)
    internal List<Part> PartsFrom;
    internal List<Part> PartsTo;

    // Object Properties

    internal PumpState PumpStatus = PumpState.Off;
    internal bool IsPumpOn = false;
    internal DateTime TimeStamp;
    internal double Elapsed;

    internal double FlowRate = SMSettings.FlowRate;
    internal int FlowTime = SMSettings.MaxFlowTimeSec;
    internal double ActFlowRate
    {
      get
      {
        // Calculate the actual flow rate, based on source capacity and max flow time setting...
        return FromCapacity() / SMSettings.FlowRate > SMSettings.MaxFlowTimeSec ? FromCapacity() / SMSettings.MaxFlowTimeSec : SMSettings.FlowRate;
      }
    }

    // process vars
    internal double PumpRatio = 1;
    internal double AmtPumped;
    internal double PumpBalance
    {
      get
      {
        return PumpAmount - AmtPumped;
      }
    }


    // for Viewers
    private string _editSliderAmount = "0";
    internal string EditSliderAmount
    {
      get
      {
        return _editSliderAmount;
      }
      set
      {
        _editSliderAmount = value;
        SetStringDecimal(_editSliderAmount);
        SetStringZero(_editSliderAmount);
        GetStringDecimal(_editSliderAmount);
        GetStringZero(_editSliderAmount);
      }
    }
    internal bool PumpAmountHasDecimal;
    internal bool PumpAmountHasZero;

    #endregion

    #region Constructors

    internal TransferPump() { }

    internal TransferPump(string resourceName, TypePump pumpType, TriggerButton trigger, double pumpAmount)
    {
      ResourceName = resourceName;
      PumpType = pumpType;
      PumpTrigger = trigger;
      PumpAmount = pumpAmount;
    }

    #endregion

    #region Process Methods

    internal static void ProcessPumps()
    {
      // This routine acts as a queue
      // WHen a pump is set to IsPumOn = true, it will be processed by this routine.
      var transferPumps = SMAddon.SmVessel.TransferPumps;
      // this task runs every Update when active.
      try
      {
        foreach (var pump in transferPumps)
        {
          // Check Pump state:
          if (!pump.IsPumpOn) continue;
          switch (pump.PumpStatus)
          {
            case PumpState.Off:
              pump.TimeStamp = DateTime.Now;
              pump.Start();
              pump.PumpStatus = PumpState.Start;
              Utilities.LogMessage(string.Format("TransferPump.ProcessPumps: Off. Resource:  {0}, PumpBalance = {1}, IsPumpOn = {2}, PumpStatus = {3}", pump.ResourceName, pump.PumpBalance, pump.IsPumpOn, pump.PumpStatus.ToString()), "Info", SMSettings.VerboseLogging);
              break;
            case PumpState.Start:
              // calculate Elapsed.
              pump.Elapsed += (DateTime.Now - pump.TimeStamp).TotalSeconds;
              pump.Run();
              pump.PumpStatus = PumpState.Run;
              Utilities.LogMessage(string.Format("TransferPump.ProcessPumps: Start. Resource:  {0}, PumpBalance = {1}, IsPumpOn = {2}, PumpStatus = {3}", pump.ResourceName, pump.PumpBalance, pump.IsPumpOn, pump.PumpStatus.ToString()), "Info", SMSettings.VerboseLogging);
              break;
            case PumpState.Run:
              // 1.  Get Elapsed from last run
              var deltaT = (DateTime.Now - pump.TimeStamp).TotalSeconds;
              Utilities.LogMessage("TransferPump.ProcessPumps: Run - 1. DeltaT = " + deltaT, "Info", SMSettings.VerboseLogging);

              // 2. Lets wait long enough to get a resource volume worth moving
              pump.TimeStamp = DateTime.Now;
              pump.Running(deltaT);
              if (IsPumpComplete(pump))
              {
                pump.PumpStatus = PumpState.Stop;
              }
              Utilities.LogMessage(string.Format("TransferPump.ProcessPumps: Run - 2. Resource:  {0}, AmtRemaining = {1}, IsPumpOn = {2}, PumpStatus = {3}", pump.ResourceName, pump.FromRemaining(), pump.IsPumpOn, pump.PumpStatus.ToString()), "Info", SMSettings.VerboseLogging);
              break;
            case PumpState.Stop:
              pump.Stop();
              pump.PumpStatus = PumpState.Off;
              Utilities.LogMessage(string.Format("TransferPump.ProcessPumps: Stop. Resource:  {0}, PumpBalance = {1}, IsPumpOn = {2}, PumpStatus = {3}", pump.ResourceName, pump.PumpBalance, pump.IsPumpOn, pump.PumpStatus.ToString()), "Info", SMSettings.VerboseLogging);
              break;
          }
        }
        PumpActive = IsAnyPumpOn();
        Utilities.LogMessage(string.Format("TransferPump.ProcessPumps: Post-loop. IsAnyPumpOn = {0}", PumpActive), "Info", SMSettings.VerboseLogging);
        TransferPump.UpdateDisplayPumps(true);
        foreach (var pump in (from pump in SMAddon.SmVessel.TransferPumps where !pump.IsPumpOn && pump.PumpStatus == PumpState.Stop select pump).ToList())
        {
          SMAddon.SmVessel.TransferPumps.Remove(pump);
        }
      }
      catch (Exception ex)
      {
        if (!SMAddon.FrameErrTripped)
        {
          Utilities.LogMessage(string.Format(" in TransferPump.ProcessPumps (repeating error).  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
          SMAddon.FrameErrTripped = true;
          // ReSharper disable once PossibleIntendedRethrow
          throw ex;
        }
      }
    }

    private void Start()
    {
      // reset counters
      Elapsed = 0;
      FlowRate = SMSettings.FlowRate;
      AmtPumped = 0;
      SMAddon.AudioSourcePumpStart.Play();
      Utilities.LogMessage("TransferPump.TransferPump.Start:  Play start sound...", "Info", SMSettings.VerboseLogging);
    }

    private void Run()
    {
      if (Elapsed >= SMAddon.AudioSourcePumpStart.clip.length - 0.25)
      {
        if (SMAddon.AudioSourcePumpStart.isPlaying) SMAddon.AudioSourcePumpStart.Stop();

        // This is the run sound.  we need this only if it is not already playing..
        if (!SMAddon.AudioSourcePumpRun.isPlaying)
        {
          SMAddon.AudioSourcePumpRun.Play();
          Utilities.LogMessage("TransferPump.PransferPump.Run:  Play Run sound...", "Info", SMSettings.VerboseLogging);
        }
        Elapsed = 0;
        IsPumpOn = true;
      }
    }

    private void Running(double deltaT)
    {
      // 1.  Calculate amount to move based on flow rate and time delta
      var deltaAmt = deltaT * ActFlowRate * PumpRatio;
      Utilities.LogMessage("TransferPump.IsRunning: Run - 1. Resource:  " + ResourceName + ", DeltaAmt = " + deltaAmt, "Info", SMSettings.VerboseLogging);

      // 2.  Determine if move amount exceeds remaining amount in tank(s)  
      deltaAmt = deltaAmt > FromRemaining() ? FromRemaining() : deltaAmt;
      if (PumpType != TypePump.Dump)
        deltaAmt = deltaAmt > ToCapacityRemaining() ? ToCapacityRemaining() : deltaAmt;
      Utilities.LogMessage("TransferPump.IsRunning: Run - 2. Resource:  " + ResourceName + ", adjDeltaAmt = " + deltaAmt, "Info", SMSettings.VerboseLogging);

      if (deltaAmt > SMSettings.Tolerance)
      {
        var deltaCharge = deltaAmt * SMSettings.FlowCost;
        // 3.  Drain Charge
        if (ConsumeCharge(deltaCharge))
        {
          Utilities.LogMessage("TransferPump.IsRunning: Run - 3a. Sufficient Charge.  Perform Pump.  Resource:  " + ResourceName + ", DeltaAmt = " + deltaAmt, "Info", SMSettings.VerboseLogging);
          // 4.  Get list of From parts & Pump Resource
          ProcessSinglePump(this, deltaAmt);
        }
        else
        {
          // 5.  Report Lack of charge and turn off pump
          Utilities.LogMessage("TransferPump.IsRunning: Run - 3b. Insufficient Charge.  Abort Pump.  Resource:  " + ResourceName + ", DeltaAmt = " + deltaAmt, "Info", SMSettings.VerboseLogging);
          PumpStatus = PumpState.Stop;
        }
      }
    }

    private void Stop()
    {
      // Reset transfer related pump vars.
      IsPumpOn = false;
      AmtPumped = 0;

      if ((from pump in SMAddon.SmVessel.TransferPumps where pump.IsPumpOn select pump).Any())
        SMAddon.AudioSourcePumpRun.Stop();
      // play pump shutdown.
      SMAddon.AudioSourcePumpStop.Play();
      Elapsed = 0;
      Utilities.LogMessage("TransferPump.TransferPump.Start:  Play start sound...", "Info", SMSettings.VerboseLogging);
    }

    internal static void ProcessSinglePump(TransferPump pump, double pumpCycleAmount)
    {
      // This adjusts the delta when we get to the end of the pump.
      Utilities.LogMessage(string.Format("TransferPump.ProcessSinglePump - 1.  {0}.  PumpAmount = {1}", pump.ResourceName, pumpCycleAmount), "Info", SMSettings.VerboseLogging);

      // This var keeps track of what we actually moved..
      var pumpCycleBalance = pumpCycleAmount;

      // Not all parts will have enough resource to meet the SrcPartAmt to move.   We need to account for that.
      // count up source parts with avalilabe resources. so we can devide by them
      while (pumpCycleBalance > SMSettings.Tolerance)
      {
        Utilities.LogMessage(string.Format("TransferPump.ProcessSinglePump - 2.  {0}.  PumpCycleBalance = {1}", pump.ResourceName, pumpCycleBalance), "Info", SMSettings.VerboseLogging);

        // If a Transfer, lets validate From pump amount can be pumped. (and To if needed)
        if (pump.PumpType != TypePump.Dump)
        {
          var maxAmount = CalcMaxPumpAmt(pump.PartsFrom, pump.PartsTo, new List<string>() {pump.ResourceName});
          if (pumpCycleAmount >  maxAmount) pumpCycleAmount = maxAmount;
        }

        // From Parts.  used by Dump and Transfer.
        // Lets account for any empty/full containers
        var FromPartCount = pump.PartsFrom.Count(part => part.Resources[pump.ResourceName].amount > SMSettings.Tolerance);

        // now split up the xfer amount evenly across the number of tanks that can send/receive resources
        var FromPartAmt = pumpCycleBalance / FromPartCount;
        if (FromPartAmt < SMSettings.Tolerance) FromPartAmt = SMSettings.Tolerance;

        // To Parts (increment)
        int ToPartCount = 0;
        double ToPartAmt = 0;
        if (pump.PumpType != TypePump.Dump)
        {
          // To parts.  Used only by Transfers
          // Lets account for any empty/full containers
          ToPartCount = pump.PartsFrom.Count(part => part.Resources[pump.ResourceName].amount > SMSettings.Tolerance);

          ToPartAmt = pumpCycleBalance / ToPartCount;
          if (ToPartAmt < SMSettings.Tolerance) ToPartAmt = 0;
        }

        ///  Move the resource
        /// -------------------------------------------------------------------
        // Calculate pump amounts for each From/Dump part and Decrement.
        foreach (var part in pump.PartsFrom)
        {
          double amtToMove;
          amtToMove = part.Resources[pump.ResourceName].amount >= FromPartAmt ? FromPartAmt : part.Resources[pump.ResourceName].amount;
          part.Resources[pump.ResourceName].amount -= amtToMove;
          if (part.Resources[pump.ResourceName].amount <= SMSettings.Tolerance) part.Resources[pump.ResourceName].amount = 0;
          Utilities.LogMessage(string.Format("TransferPump.ProcessSinglePump - 3.  {0}. PartAmtToDecrease = {1}.  PartAmtLeft - {2}", pump.ResourceName, amtToMove, part.Resources[pump.ResourceName].amount), "Info", SMSettings.VerboseLogging);

          // Report ramaining balance after Transfer.
          pumpCycleBalance -= amtToMove;
          pump.AmtPumped += amtToMove;
        }
        if (pump.PumpType != TypePump.Dump)
        {
          // Calculate pump amounts for each To part and Increment.
          foreach (var part in pump.PartsTo)
          {
            double amtToMove;
            amtToMove = part.Resources[pump.ResourceName].maxAmount - part.Resources[pump.ResourceName].amount >= ToPartAmt ? ToPartAmt : part.Resources[pump.ResourceName].amount;
            Utilities.LogMessage(string.Format("TransferPump.ProcessSinglePump - 4.  {0}. PartAmtToIncrease = {1}.  PartCapacityLeft - {2}", pump.ResourceName, amtToMove, part.Resources[pump.ResourceName].maxAmount - part.Resources[pump.ResourceName].amount), "Info", SMSettings.VerboseLogging);

            part.Resources[pump.ResourceName].amount += amtToMove;
            if (part.Resources[pump.ResourceName].amount >  part.Resources[pump.ResourceName].maxAmount)
              part.Resources[pump.ResourceName].amount = part.Resources[pump.ResourceName].maxAmount;
          }
        }
        /// -------------------------------------------------------------------
        if (IsPumpComplete(pump))
        {
          pump.PumpStatus = PumpState.Stop;
        }
        Utilities.LogMessage(string.Format("TransferPump.ProcessSinglePump - 5:  End.  {0}.  PumpBalance = {1}, IsPumpOn = {2}, PumpStatus = {3}", pump.ResourceName, pump.PumpBalance, pump.IsPumpOn.ToString(), pump.PumpStatus.ToString()), "Info", SMSettings.VerboseLogging);
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

    internal static void AbortPumpProcess(uint pumpId)
    {
      // set a state vars and wait for the next update to pick it up.
      foreach (var pump in (from pump in SMAddon.SmVessel.TransferPumps where pump.IsPumpOn && pump.PumpId == pumpId  select pump).ToList())
      {
        pump.PumpStatus = PumpState.Stop;
      }
    }

    internal static void AssignPumpAmounts(List<TransferPump> pumps, double PumpAmount, uint pumpId)
    {
      if (pumps.Count > 1)
      {
        // Calculate Ratio and transfer amounts.  Ratio is based off the largest amount to move, so will always be less than 1.
        var ratio = TransferPump.CalcRatio(pumps);
        pumps[0].PumpId = pumpId;
        pumps[1].PumpId = pumpId;

        if (pumps[0].ToCapacity() > pumps[1].ToCapacity())
        {
          pumps[0].PumpRatio = 1;
          pumps[1].PumpRatio = ratio;
          pumps[0].PumpAmount = PumpAmount;
          pumps[1].PumpAmount = PumpAmount * ratio <= pumps[1].FromCapacity() ? PumpAmount * ratio : pumps[1].FromCapacity();
        }
        else
        {
          pumps[1].PumpRatio = 1;
          pumps[0].PumpRatio = ratio;
          pumps[1].PumpAmount = PumpAmount;
          pumps[0].PumpAmount = PumpAmount * ratio <= pumps[0].FromCapacity() ? PumpAmount * ratio : pumps[0].FromCapacity();
        }
        pumps[0].IsPumpOn = true;
        pumps[1].IsPumpOn = true;
      }
      else
      {
        pumps[0].PumpId = pumpId;
        pumps[0].PumpRatio = 1;
        pumps[0].PumpAmount = PumpAmount;
        pumps[0].IsPumpOn = true;
      }

    }

    #endregion

    #region Property Methods

    /// <summary>
    /// Returns capacity of the parts we are transferrring from (sources relative to xferMode)
    /// </summary>
    internal double FromCapacity()
    {
      return CalcResourceCapacity(PartsFrom, ResourceName);
    }
    /// <summary>
    /// returns amount remaining in parts we are transferrring from (sources relative to XferMode)
    /// </summary>
    internal double FromRemaining()
    {
      return CalcRemainingResource(PartsFrom, ResourceName);
    }
    internal bool IsFromFull
    {
      get
      {
        return FromCapacity() - FromRemaining() < SMSettings.Tolerance;
      }
    }
    internal bool IsFromEmpty
    {
      get
      {
        return FromRemaining() < SMSettings.Tolerance; 
      }
    }

    /// <summary>
    /// returns capacity of parts we are transferring to (targets relative to xferMode)
    /// </summary>
    internal double ToCapacity()
    {
      switch (PumpType)
      {
        case TypePump.SourceToTarget:
        case TypePump.TargetToSource:
          return CalcResourceCapacity(PartsTo, ResourceName);
        case TypePump.Dump:
          return CalcResourceCapacity(PartsFrom, ResourceName);
      }
      return 0;
    }
    internal bool IsToFull
    {
      get
      {
        return ToCapacity() - ToRemaining() < SMSettings.Tolerance;
      }
    }
    internal bool IsToEmpty
    {
      get
      {
        return ToRemaining() < SMSettings.Tolerance;
      }
    }

    /// <summary>
    /// returns amount remaining in parts we are transferring to (targets relative to xferMode)
    /// </summary>
    internal double ToRemaining()
    {
      switch (PumpType)
      {
        case TypePump.SourceToTarget:
        case TypePump.TargetToSource:
          return CalcRemainingResource(PartsTo, ResourceName);
        case TypePump.Dump:
          return CalcRemainingResource(PartsFrom, ResourceName);
      }
      return 0;
    }

    /// <summary>
    /// returns amount remaining in parts we are transferring to (targets relative to xferMode)
    /// </summary>
    internal double ToCapacityRemaining()
    {
      switch (PumpType)
      {
        case TypePump.SourceToTarget:
        case TypePump.TargetToSource:
          return CalcRemainingCapacity(PartsTo, ResourceName);
        case TypePump.Dump:
          return CalcRemainingCapacity(PartsFrom, ResourceName);
      }
      return 0;
    }

    internal static bool IsAnyPumpOn()
    {
      return TransferPump.PumpActive && (from pump in SMAddon.SmVessel.TransferPumps where pump.IsPumpOn select pump).Any();
    }

    internal static bool IsPumpInProgress(uint pumpId)
    {
      return TransferPump.PumpActive && (from pump in SMAddon.SmVessel.TransferPumps where pump.IsPumpOn && pump.PumpId == pumpId select pump).Any();
    }

    internal static List<TransferPump> PumpsInProgress(uint pumpId)
    {
      var newList = (from pump in SMAddon.SmVessel.TransferPumps where pump.IsPumpOn && pump.PumpId == pumpId select pump).ToList();
      if (TransferPump.PumpActive)
        return newList;
      newList.Clear();
      return newList;
    }

    #endregion

    #region Logic/State Methods

    internal string GetStringDecimal(string strPumpAmount)
    {
      if (PumpAmountHasDecimal && !strPumpAmount.Contains(".")) strPumpAmount += ".";
      return strPumpAmount;
    }

    internal string GetStringZero(string strPumpAmount)
    {
      if (PumpAmountHasZero && !strPumpAmount.EndsWith("0")) strPumpAmount += "0";
      return strPumpAmount;
    }

    internal void SetStringZero(string strPumpAmount)
    {
      // sets static vars at a higher scope than calling routine.
      if (strPumpAmount.Contains(".") && strPumpAmount.EndsWith("0"))
        PumpAmountHasZero = true;
      else
        PumpAmountHasZero = false;
    }

    internal void SetStringDecimal(string strPumpAmount)
    {
      // sets static vars at a higher scope than calling routine.
      if (strPumpAmount.Contains("."))
        PumpAmountHasDecimal = true;
      else
        PumpAmountHasDecimal = false;
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

    internal double MaxPumpAmt()
    {
      double maxPumpAmount = 0;
      if (PartsFrom == null || PartsTo == null || PartsFrom.Count == 0 || PartsTo.Count == 0)
        maxPumpAmount = 0;
      else
      {
        var maxFromAmount = PartsFrom.Sum(partFrom => partFrom.Resources[ResourceName].amount);
        maxPumpAmount += PartsTo.Sum(partTo => partTo.Resources[ResourceName].maxAmount - partTo.Resources[ResourceName].amount);
        maxPumpAmount = maxPumpAmount > maxFromAmount ? maxFromAmount : maxPumpAmount;
        maxPumpAmount = maxPumpAmount < SMSettings.Tolerance ? 0 : maxPumpAmount;
      }
      return maxPumpAmount;
    }

    internal static double CalcMaxPumpAmt(List<Part> partsFrom, List<Part> partsTo, List<string> selectedResources)
    {
      double maxPumpAmount = 0;
      if (partsFrom == null || partsTo == null || partsFrom.Count == 0 || partsTo.Count == 0 || selectedResources.Count == 0)
        maxPumpAmount = 0;
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
        maxPumpAmount += partsTo.Sum(partTo => partTo.Resources[selectedResources[resIdx]].maxAmount - partTo.Resources[selectedResources[resIdx]].amount);

        maxPumpAmount = maxPumpAmount > maxFromAmount ? maxFromAmount : maxPumpAmount;
        maxPumpAmount = maxPumpAmount < SMSettings.Tolerance ? 0 : maxPumpAmount;
      }
      return maxPumpAmount;
    }

    internal static double CalcMaxResourceXferAmt(List<Part> from, List<Part> to, string selectedResource)
    {
      double maxPumpAmount = 0;
      if (from.Count == 0 || to.Count == 0 || selectedResource == null || selectedResource == "")
        maxPumpAmount = 0;
      else
      {
        // now lets get the amount to move.  we will use this higher portion to calc lower volume ratio during move.
        var maxSourceAmount = from.Sum(source => source.Resources[selectedResource].amount);
        maxPumpAmount += to.Sum(target => target.Resources[selectedResource].maxAmount - target.Resources[selectedResource].amount);

        maxPumpAmount = maxPumpAmount > maxSourceAmount ? maxSourceAmount : maxPumpAmount;
        maxPumpAmount = maxPumpAmount < 0.0001 ? 0 : maxPumpAmount;
      }
      return maxPumpAmount;
    }

    private static bool IsPumpComplete(TransferPump pump)
    {
      var results = false;
      if (pump.PumpType != TypePump.Dump && CalcMaxPumpAmt(pump.PartsFrom, pump.PartsTo, new List<string>() { pump.ResourceName }) < SMSettings.Tolerance) results = true;
      else if (pump.IsFromEmpty) results = true;
      else if (pump.IsToFull) results = true;
      else if (pump.AmtPumped >= pump.PumpAmount) results = true;
      else if (pump.PumpAmount - pump.AmtPumped < SMSettings.Tolerance) results = true;
      Utilities.LogMessage(string.Format("TransferPump.IsPumpComplete:  Resource:  {0}, TotalPumpAmt = {1}, AmtPumped = {2}, IsPumpComplete = {3}", pump.ResourceName, pump.PumpAmount, pump.AmtPumped, results.ToString()), "Info", SMSettings.VerboseLogging);
      return results;
    }

    internal static void UpdateDisplayPumps(bool resetAmt)
    {
      // Now lets update the Resource Xfer pumps for display use...
      if (SMConditions.AreSelectedResourcesTypeOther(SMAddon.SmVessel.SelectedResources))
      {
        if (WindowTransfer.DisplayPumps.Count() == 0) CreateDisplayPumps();
        foreach (var pump in WindowTransfer.DisplayPumps)
        {
          // Lets update pump data (it persists).
          pump.PartsFrom = pump.PumpType == TypePump.SourceToTarget ? SMAddon.SmVessel.SelectedPartsSource : SMAddon.SmVessel.SelectedPartsTarget;
          pump.PartsTo = pump.PumpType == TypePump.SourceToTarget ? SMAddon.SmVessel.SelectedPartsTarget : SMAddon.SmVessel.SelectedPartsSource;
          if (resetAmt) pump.EditSliderAmount = pump.MaxPumpAmt().ToString();
        }
      }
    }

    internal static void CreateDisplayPumps()
    {
      // Now lets update the Resource Xfer pumps for display use...
      WindowTransfer.DisplayPumps.Clear();
      if (SMConditions.AreSelectedResourcesTypeOther(SMAddon.SmVessel.SelectedResources))
      {
        foreach (var resource in SMAddon.SmVessel.SelectedResources)
        {
          // Lets create a Xfer Object for managing xfer options and data.
          var pump = new TransferPump()
          {
            ResourceName = resource,
            PumpType = TransferPump.TypePump.SourceToTarget,
            PumpAmount =
              TransferPump.CalcMaxResourceXferAmt(SMAddon.SmVessel.SelectedPartsSource,
                SMAddon.SmVessel.SelectedPartsTarget, resource),
            PartsFrom = SMAddon.SmVessel.SelectedPartsSource,
            PartsTo = SMAddon.SmVessel.SelectedPartsTarget
          };
          WindowTransfer.DisplayPumps.Add(pump);

          pump = new TransferPump()
          {
            ResourceName = resource,
            PumpType = TransferPump.TypePump.TargetToSource,
            PumpAmount =
              TransferPump.CalcMaxResourceXferAmt(SMAddon.SmVessel.SelectedPartsTarget,
                SMAddon.SmVessel.SelectedPartsSource, resource),
            PartsFrom = SMAddon.SmVessel.SelectedPartsTarget,
            PartsTo = SMAddon.SmVessel.SelectedPartsSource
          };
          WindowTransfer.DisplayPumps.Add(pump);
        }
      }
    }

    internal static List<TransferPump> GetDisplayPumpsByType(TypePump pumpType)
    {
      return (from pump in WindowTransfer.DisplayPumps where pump.PumpType == pumpType select pump).ToList();
    }

    /// <summary>
    /// this method returns either the largest or smallest ratio pump when compared to the other resource selected.
    /// </summary>
    /// <param name="pumps">transfer pumps assocaiated with the selected resources</param>
    /// <param name="pumpType">ype of pump (SourceToTarget or TargetToSource)</param>
    /// <param name="isRatio">causes method to return the smaller ratio pump</param>
    /// <returns></returns>
    internal static TransferPump GetRatioPump(List<TransferPump> pumps, bool isRatio = false)
    {
      if (pumps.Count > 1)
      {
        if (isRatio)
        {
          if (pumps[1].ToCapacity() > pumps[0].ToCapacity())
            return pumps[0];
          else
            return pumps[1];
        }
        else
        {
          if (pumps[0].ToCapacity() > pumps[1].ToCapacity())
            return pumps[0];
          else
            return pumps[1];
        }
      }
      else
      {
        return pumps[0];
      }
    }

    internal static double CalcRatio(List<TransferPump> pumps)
    {
      if (pumps.Count > 1)
      {
        if (pumps[0].ToCapacity() > pumps[1].ToCapacity())
          return pumps[1].ToCapacity() / pumps[0].ToCapacity();
        else
          return pumps[0].ToCapacity() / pumps[1].ToCapacity();
      }
      else
        return 1;
    }

    internal static uint GetPumpIdFromHash(string resource, Part firstPart, Part lastPart, TransferPump.TypePump pumpType, TriggerButton trigger)
    {
      return firstPart.flightID + lastPart.flightID + (uint)pumpType.GetHashCode() + (uint)trigger.GetHashCode() + (uint)resource.GetHashCode();
    }
    #endregion

    #region Enums
    internal enum PumpState
    {
      Off,
      Start,
      Run,
      Stop
    }

    internal enum TypePump
    {
      SourceToTarget,
      TargetToSource,
      Dump
    }

    internal enum TriggerButton
    {
      Preflight,
      Manifest,
      Transfer,
      Dump
    }
    #endregion
  }
}
