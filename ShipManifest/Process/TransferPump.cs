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
  internal class TransferPump : ITransferPump
  {

    public static List<TransferPump> Instance
    {
      get
      {
        return SMAddon.SmVessel.TransferPumps;
      }
    }

    // Default sound license: CC-By-SA
    // http://www.freesound.org/people/vibe_crc/sounds/59328/
    internal static string Path1 = SMSettings.PumpSoundStart ?? "ShipManifest/Sounds/59328-1";
    internal static string Path2 = SMSettings.PumpSoundRun ?? "ShipManifest/Sounds/59328-2";
    internal static string Path3 = SMSettings.PumpSoundStop ?? "ShipManifest/Sounds/59328-3";

    // used during transfer operations.
    public static bool PumpProcessOn { get; internal set; }

    #region Properties
    // Constructor properties
    public string Resource { get; internal set; }
    public double PumpAmount { get; internal set; }
    internal TypePump PumpType = TypePump.SourceToTarget;
    internal TriggerButton PumpTrigger;
    internal uint PumpId;

    // Sources (should be assigned at instantiation or right after)
    public List<Part> FromParts { get; internal set; }
    public List<Part> ToParts { get; internal set; }

    // Object Properties
    /// <summary>
    /// Returns a formatted string listing of various properties in this TransferPump instance
    /// </summary>
    public string Info
    {
      get
      {
        return string.Format(
          string.Concat(
          "\r\n      Pump.Info:",
          "\r\n        PumpId:        {0}",
          "\r\n        PumpType:      {1}",
          "\r\n        ResourceName:  {2}",
          "\r\n        PumpAmount:    {3}",
          "\r\n        AmountPumped:  {4}",
          "\r\n        TimeStamp:     {5}",
          "\r\n        IsPumpOn:      {6}",
          "\r\n        PumpStatus:    {7}",
          "\r\n        IsComplete:    {8}",
          "\r\n        IsFromEmpty:   {9}",
          "\r\n        IsFromFull:    {10}",
          "\r\n        PumpRatio:     {11}"),
          PumpId,
          PumpType,
          Resource,
          PumpAmount,
          AmtPumped,
          TimeStamp,
          IsPumpOn,
          PumpStatus,
          IsComplete,
          FromIsEmpty,
          FromIsFull,
          PumpRatio
          );
      }
    }
    
    /// <summary>
    /// Returns capacity of the parts we are transferrring from (sources relative to xferMode)
    /// </summary>
    public double FromCapacity
    {
      get { return CalcResourceCapacity(FromParts, Resource); }
    }

    /// <summary>
    /// returns amount remaining in parts we are transferrring from (sources relative to XferMode)
    /// </summary>
    public double FromRemaining
    {
      get { return CalcRemainingResource(FromParts, Resource); }
    }
    public bool FromIsFull
    {
      get
      {
        return FromCapacity - FromRemaining < SMSettings.Tolerance;
      }
    }
    public bool FromIsEmpty
    {
      get
      {
        return FromRemaining < SMSettings.Tolerance;
      }
    }

    /// <summary>
    /// returns capacity of parts we are transferring to (targets relative to xferMode)
    /// </summary>
    public double ToCapacity
    {
      get 
      {
        switch (PumpType)
        {
          case TypePump.SourceToTarget:
          case TypePump.TargetToSource:
            return CalcResourceCapacity(ToParts, Resource);
          case TypePump.Dump:
            return CalcResourceCapacity(FromParts, Resource);
        }
      return 0;
      }
    }

    /// <summary>
    /// returns amount remaining in parts we are transferring to (targets relative to xferMode)
    /// </summary>
    public double ToRemaining
    {
      get
      {
        switch (PumpType)
        {
          case TypePump.SourceToTarget:
          case TypePump.TargetToSource:
            return CalcRemainingResource(ToParts, Resource);
          case TypePump.Dump:
            return CalcRemainingResource(FromParts, Resource);
        }
        return 0;
      }
    }
    public bool ToIsFull
    {
      get
      {
        return ToCapacity - ToRemaining < SMSettings.Tolerance;
      }
    }
    public bool ToIsEmpty
    {
      get
      {
        return ToRemaining < SMSettings.Tolerance;
      }
    }


    /// <summary>
    /// returns amount remaining in parts we are transferring to (targets relative to xferMode)
    /// </summary>
    public double ToCapacityRemaining
    {
      get
      {
        switch (PumpType)
        {
          case TypePump.SourceToTarget:
          case TypePump.TargetToSource:
            return CalcRemainingCapacity(ToParts, Resource);
          case TypePump.Dump:
            return CalcRemainingCapacity(FromParts, Resource);
        }
        return 0;
      }
    }

    public bool IsComplete
    {
      get
      {
        return IsPumpComplete(this);
      }
    }

    internal PumpState PumpStatus = PumpState.Off;
    public bool IsPumpOn { get; internal set; }
    internal DateTime TimeStamp;
    internal double Elapsed;

    internal double DefaultFlowRate = SMSettings.FlowRate;
    internal int FlowTime = SMSettings.MaxFlowTimeSec;
    public double FlowRate
    {
      get
      {
        // Calculate the actual flow rate, based on source capacity and max flow time setting...
        return FromCapacity / SMSettings.FlowRate > SMSettings.MaxFlowTimeSec ? FromCapacity / SMSettings.MaxFlowTimeSec : SMSettings.FlowRate;
      }
    }

 
    // process vars
    public double PumpRatio { get; internal set; }
    public double AmtPumped { get; internal set; }
    public double PumpBalance
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
      Resource = resourceName;
      PumpType = pumpType;
      PumpTrigger = trigger;
      PumpAmount = pumpAmount;
    }

    #endregion

    internal static void ProcessActivePumps()
    {
      var methodname = "TransferPump.ProcessActivePumps:  ";
      // This routine acts as a queue
      // WHen a pump is set to IsPumOn = true, it will be processed by this routine.
      var transferPumps = SMAddon.SmVessel.TransferPumps;
      // this task runs every Update when active.
      try
      {
        Utilities.LogMessage(string.Format("{0}Entering Method.",methodname), "Info", SMSettings.VerboseLogging);
        foreach (var pump in transferPumps)
        {
          // Check Pump state:
          if (!pump.IsPumpOn)
          {
            Utilities.LogMessage(string.Format("{0} Skipping this pump.{1}", methodname, pump.Info), "Info", SMSettings.VerboseLogging);
            continue;
          }
          switch (pump.PumpStatus)
          {
            case PumpState.Off:
              pump.TimeStamp = DateTime.Now;
              pump.Start();
              pump.PumpStatus = PumpState.Start;
              Utilities.LogMessage(string.Format("{0}Entered State Off. {1}", methodname, pump.Info), "Info", SMSettings.VerboseLogging);
              break;
            case PumpState.Start:
              // Calculate Elapsed.
              pump.Elapsed += (DateTime.Now - pump.TimeStamp).TotalSeconds;
              if (pump.Elapsed >= SMAddon.AudioSourcePumpStart.clip.length - 0.25)
              {
                pump.Run();
                pump.PumpStatus = PumpState.Run;
                Utilities.LogMessage(string.Format("{0}Entered State Start. {1}", methodname, pump.Info), "Info", SMSettings.VerboseLogging);
              }
              else Utilities.LogMessage(string.Format("{0}Entered State Start.  Waiting for sound start to complete. {1}", methodname, pump.Info), "Info", SMSettings.VerboseLogging);
              break;
            case PumpState.Run:
              // 1.  Get Elapsed from last run
              var deltaT = (DateTime.Now - pump.TimeStamp).TotalSeconds;
              Utilities.LogMessage(string.Format("{0}Entered State Run.  Step 1. DeltaT = {1}", methodname, deltaT), "Info", SMSettings.VerboseLogging);

              // 2. Lets wait long enough to get a resource volume worth moving
              pump.TimeStamp = DateTime.Now;
              pump.Running(deltaT);
              if (IsPumpComplete(pump))
              {
                pump.PumpStatus = PumpState.Stop;
              }
              Utilities.LogMessage(string.Format("{0}Entered State Run.  Step 2. {1}", methodname, pump.Info), "Info", SMSettings.VerboseLogging);
              break;
            case PumpState.Stop:
              pump.Stop();
              pump.PumpStatus = PumpState.Off;
              Utilities.LogMessage(string.Format("{0}Entered State Stop.  {1}", methodname, pump.Info), "Info", SMSettings.VerboseLogging);
              break;
          }
        }
        foreach (var pump in (from pump in transferPumps where pump.IsComplete select pump).ToList())
        {
          Utilities.LogMessage(string.Format("{0}Post-loop.  removing pump:{1}", methodname, pump.Info), "Info", SMSettings.VerboseLogging);
          transferPumps.Remove(pump);
        }
        PumpProcessOn = IsAnyPumpOn();
        Utilities.LogMessage(string.Format("{0}Post-loop. IsAnyPumpOn = {1}", methodname, PumpProcessOn), "Info", SMSettings.VerboseLogging);
        UpdateDisplayPumps();
        Utilities.LogMessage(string.Format("{0}Exiting Method.",methodname), "Info", SMSettings.VerboseLogging);
      }
      catch (Exception ex)
      {
        if (!SMAddon.FrameErrTripped)
        {
          Utilities.LogMessage(string.Format(" in TransferPump.ProcessActivePumps (repeating error).  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
          SMAddon.FrameErrTripped = true;

          // ReSharper disable once PossibleIntendedRethrow
          throw ex;
        }
      }
    }

    #region Processing Methods

    private void Start()
    {
      // reset counters
      Elapsed = 0;
      DefaultFlowRate = SMSettings.FlowRate;
      AmtPumped = 0;
      SMAddon.AudioSourcePumpStart.Play();
      Utilities.LogMessage(string.Format("TransferPump.Start:  Resource:  {0}.  Play start sound...", Resource), "Info", SMSettings.VerboseLogging);
    }

    private void Run()
    {
      if (SMAddon.AudioSourcePumpStart.isPlaying) SMAddon.AudioSourcePumpStart.Stop();

      // This is the run sound.  we need this only if it is not already playing..
      if (!SMAddon.AudioSourcePumpRun.isPlaying)
      {
        SMAddon.AudioSourcePumpRun.Play();
        Utilities.LogMessage("TransferPump.PransferPump.Run:  Play Run sound...", "Info", SMSettings.VerboseLogging);
      }
      // reset timer for next step
      Elapsed = 0;
    }

    private void Running(double deltaT)
    {
      var methodName = "TransferPump.Running:";
      // 1.  Calculate amount to move based on flow rate and time delta
      var deltaAmt = deltaT * FlowRate * PumpRatio;
      Utilities.LogMessage(string.Format("{0}  1.  {1}", methodName, Info), "Info", SMSettings.VerboseLogging);

      // 2.  Determine if move amount exceeds remaining amount in tank(s)  
      deltaAmt = deltaAmt > FromRemaining ? FromRemaining : deltaAmt;
      if (PumpType != TypePump.Dump)
        deltaAmt = deltaAmt > ToCapacityRemaining ? ToCapacityRemaining : deltaAmt;
      Utilities.LogMessage(string.Format("{0}  2.  {1}", methodName, Info), "Info", SMSettings.VerboseLogging);

      if (deltaAmt > SMSettings.Tolerance)
      {
        var deltaCharge = deltaAmt * SMSettings.FlowCost;
        // 3.  Drain Charge
        if (ConsumeCharge(deltaCharge))
        {
          Utilities.LogMessage(string.Format("{0}  3a. Sufficient Charge.  Perform Pump Cucle.  {1}", methodName, Info), "Info", SMSettings.VerboseLogging);
          // 4.  Get list of From parts & Pump Resource
          RunCycle(deltaAmt);
        }
        else
        {
          // 5.  Report Lack of charge and turn off pump
          PumpStatus = PumpState.Stop;
          Utilities.LogMessage(string.Format("{0}  3b. Insufficient Charge.  Abort Pump Cycle.   {1}",methodName,Info), "Info", SMSettings.VerboseLogging);
        }
      }
    }

    private void Stop()
    {
      // Reset transfer related pump vars.
      IsPumpOn = false;

      // If no other pumps running, then turn off run sound.
      if (!(from pump in SMAddon.SmVessel.TransferPumps where pump.IsPumpOn select pump).Any())
        SMAddon.AudioSourcePumpRun.Stop();

      // play pump shutdown.
      SMAddon.AudioSourcePumpStop.Play();
      Elapsed = 0;
      Utilities.LogMessage("TransferPump.Stop:  Play stop sound...", "Info", SMSettings.VerboseLogging);
    }

    internal void RunCycle(double cycleAmount)
    {
      // This var keeps track of what we actually moved..
      var cycleBalance = cycleAmount;

      // Not all parts will have enough resource to meet the SrcPartAmt to move.   We need to account for that.
      // count up source parts with avalilabe resources. so we can devide by them
      while (cycleBalance > SMSettings.Tolerance)
      {
        // If a Transfer, lets validate From pump amount can be pumped. (and To if needed)
        if (PumpType != TypePump.Dump)
        {
          var maxAmount = CalcMaxPumpAmt(FromParts, ToParts, new List<string> { Resource });
          if (cycleAmount > maxAmount) cycleAmount = maxAmount;
        }

        // From Parts.  used by Dump and Transfer.
        // Lets account for any empty/full containers
        var fromPartCount = FromParts.Count(part => part.Resources[Resource].amount > SMSettings.Tolerance);

        // now split up the xfer amount evenly across the number of tanks that can send/receive resources
        var fromPartAmt = cycleBalance / fromPartCount;
        if (fromPartAmt < SMSettings.Tolerance) fromPartAmt = SMSettings.Tolerance;

        // To Parts (increment)
        double toPartAmt = 0;
        if (PumpType != TypePump.Dump)
        {
          // To parts.  Used only by Transfers
          // Lets account for any empty/full containers
          var toPartCount = FromParts.Count(part => part.Resources[Resource].amount > SMSettings.Tolerance);

          toPartAmt = cycleBalance / toPartCount;
          if (toPartAmt < SMSettings.Tolerance) toPartAmt = 0;
        }

        //  Move the resource
        // -------------------------------------------------------------------
        // Calculate pump amounts for each From/Dump part and Decrement.
        foreach (var part in FromParts)
        {
          var amtToMove = part.Resources[Resource].amount >= fromPartAmt ? fromPartAmt : part.Resources[Resource].amount;
          part.Resources[Resource].amount -= amtToMove;
          if (part.Resources[Resource].amount <= SMSettings.Tolerance) part.Resources[Resource].amount = 0;

          // Report ramaining balance after Transfer.
          cycleBalance -= amtToMove;
          AmtPumped += amtToMove;
        }
        if (PumpType != TypePump.Dump)
        {
          // Calculate pump amounts for each To part and Increment.
          foreach (var part in ToParts)
          {
            var amtToMove = part.Resources[Resource].maxAmount - part.Resources[Resource].amount >= toPartAmt ? toPartAmt : part.Resources[Resource].amount;

            part.Resources[Resource].amount += amtToMove;
            if (part.Resources[Resource].amount > part.Resources[Resource].maxAmount)
              part.Resources[Resource].amount = part.Resources[Resource].maxAmount;
          }
        }
        // -------------------------------------------------------------------
        if (IsComplete)
          PumpStatus = PumpState.Stop;
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
          deltaCharge -= iPart.Resources["ElectricCharge"].amount;
          iPart.Resources["ElectricCharge"].amount = 0;
        }
        if (deltaCharge > 0)
          return false;
        return true;
      }
      return true;
    }

    public void Abort()
    {
      PumpStatus = PumpState.Stop;
    }

    public static void AbortPumpProcess(uint pumpId)
    {
      // set a state vars and wait for the next update to pick it up.
      foreach (var pump in (from pump in SMAddon.SmVessel.TransferPumps where pump.IsPumpOn && pump.PumpId == pumpId select pump).ToList())
      {
        pump.PumpStatus = PumpState.Stop;
      }
    }

    internal static void AssignPumpAmounts(List<TransferPump> pumps, double pumpAmount, uint pumpId)
    {
      if (pumps.Count > 1)
      {
        // Calculate Ratio and transfer amounts.  Ratio is based off the largest amount to move, so will always be less than 1.
        var ratio = CalcRatio(pumps);
        pumps[0].PumpId = pumpId;
        pumps[1].PumpId = pumpId;

        if (pumps[0].ToCapacity > pumps[1].ToCapacity)
        {
          pumps[0].PumpRatio = 1;
          pumps[1].PumpRatio = ratio;
          pumps[0].PumpAmount = pumpAmount;
          pumps[1].PumpAmount = pumpAmount * ratio <= pumps[1].FromCapacity ? pumpAmount * ratio : pumps[1].FromCapacity;
        }
        else
        {
          pumps[1].PumpRatio = 1;
          pumps[0].PumpRatio = ratio;
          pumps[1].PumpAmount = pumpAmount;
          pumps[0].PumpAmount = pumpAmount * ratio <= pumps[0].FromCapacity ? pumpAmount * ratio : pumps[0].FromCapacity;
        }
        pumps[0].IsPumpOn = true;
        pumps[1].IsPumpOn = true;
      }
      else
      {
        pumps[0].PumpId = pumpId;
        pumps[0].PumpRatio = 1;
        pumps[0].PumpAmount = pumpAmount;
        pumps[0].IsPumpOn = true;
      }

    }

    #region Display String Methods

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
      PumpAmountHasDecimal = strPumpAmount.Contains(".");
    }

    #endregion

    #endregion

    #region Static Logic/State Methods

    public static bool IsAnyPumpOn()
    {
      return PumpProcessOn && (from pump in SMAddon.SmVessel.TransferPumps where pump.IsPumpOn select pump).Any();
    }

    internal static bool IsPumpInProgress(uint pumpId)
    {
      return PumpProcessOn && (from pump in SMAddon.SmVessel.TransferPumps where pump.IsPumpOn && pump.PumpId == pumpId select pump).Any();
    }

    internal static List<TransferPump> PumpsInProgress(uint pumpId)
    {
      var newList = (from pump in SMAddon.SmVessel.TransferPumps where pump.IsPumpOn && pump.PumpId == pumpId select pump).ToList();
      if (PumpProcessOn)
        return newList;
      newList.Clear();
      return newList;
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
      if (FromParts == null || ToParts == null || FromParts.Count == 0 || ToParts.Count == 0)
        maxPumpAmount = 0;
      else
      {
        var maxFromAmount = FromParts.Sum(partFrom => partFrom.Resources[Resource].amount);
        maxPumpAmount += ToParts.Sum(partTo => partTo.Resources[Resource].maxAmount - partTo.Resources[Resource].amount);
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

    private static bool IsPumpComplete(TransferPump pump)
    {
      bool results = pump.PumpStatus == PumpState.Off || 
      pump.PumpStatus == PumpState.Stop ||
      (pump.PumpType != TypePump.Dump && CalcMaxPumpAmt(pump.FromParts, pump.ToParts, new List<string> { pump.Resource }) < SMSettings.Tolerance) ||
      pump.FromIsEmpty ||
      (pump.PumpType != TypePump.Dump && pump.ToIsFull) ||
      pump.AmtPumped >= pump.PumpAmount ||
      pump.PumpAmount - pump.AmtPumped < SMSettings.Tolerance;
      return results;
    }

    internal static void CreateDisplayPumps()
    {
      // Now lets update the Resource Xfer pumps for display use...
      WindowTransfer.DisplayPumps.Clear();
      if (SMConditions.AreSelectedResourcesTypeOther(SMAddon.SmVessel.SelectedResources))
      {
        // Lets create a pump Object for managing pump options and data.
        var displaySourceParts = WindowTransfer.ShowSourceVessels ? 
          SMAddon.SmVessel.GetSelectedVesselsParts(SMAddon.SmVessel.SelectedVesselsSource, SMAddon.SmVessel.SelectedResources) :
          SMAddon.SmVessel.SelectedPartsSource;
        var displayTargetParts = WindowTransfer.ShowTargetVessels ? 
          SMAddon.SmVessel.GetSelectedVesselsParts(SMAddon.SmVessel.SelectedVesselsTarget, SMAddon.SmVessel.SelectedResources) :
          SMAddon.SmVessel.SelectedPartsTarget;

        foreach (var resource in SMAddon.SmVessel.SelectedResources)
        {
          var pump1 = new TransferPump
          {
            Resource = resource,
            PumpType = TypePump.SourceToTarget,
            PumpAmount = CalcMaxPumpAmt(displaySourceParts, displayTargetParts, SMAddon.SmVessel.SelectedResources),
            EditSliderAmount = CalcMaxPumpAmt(displaySourceParts, displayTargetParts, SMAddon.SmVessel.SelectedResources).ToString(CultureInfo.InvariantCulture),
            FromParts = displaySourceParts,
            ToParts = displayTargetParts
          };
          WindowTransfer.DisplayPumps.Add(pump1);

          var pump2 = new TransferPump
          {
            Resource = resource,
            PumpType = TypePump.TargetToSource,
            PumpAmount = CalcMaxPumpAmt(displayTargetParts, displaySourceParts, SMAddon.SmVessel.SelectedResources),
            EditSliderAmount = CalcMaxPumpAmt(displayTargetParts, displaySourceParts, SMAddon.SmVessel.SelectedResources).ToString(CultureInfo.InvariantCulture),
            FromParts = displayTargetParts,
            ToParts = displaySourceParts
          };
          WindowTransfer.DisplayPumps.Add(pump2);
        }
      }
    }

    internal static void UpdateDisplayPumps()
    {
      // Now lets update the Resource Xfer pumps for display use...
      if (!SMConditions.AreSelectedResourcesTypeOther(SMAddon.SmVessel.SelectedResources)) return;
      if (!WindowTransfer.DisplayPumps.Any()) CreateDisplayPumps();

      // Lets create a pump Object for managing pump options and data.
      var sourceParts = WindowTransfer.ShowSourceVessels ?
        SMAddon.SmVessel.GetSelectedVesselsParts(SMAddon.SmVessel.SelectedVesselsSource, SMAddon.SmVessel.SelectedResources) :
        SMAddon.SmVessel.SelectedPartsSource;
      var targetParts = WindowTransfer.ShowTargetVessels ?
        SMAddon.SmVessel.GetSelectedVesselsParts(SMAddon.SmVessel.SelectedVesselsTarget, SMAddon.SmVessel.SelectedResources) :
        SMAddon.SmVessel.SelectedPartsTarget;

      foreach (var pump in WindowTransfer.DisplayPumps)
      {
        // Lets update pump data (it persists).
        switch (pump.PumpType)
        {
          case TypePump.Dump:
          case TypePump.SourceToTarget:
            pump.FromParts = sourceParts;
            pump.ToParts = targetParts;
            break;
          case TypePump.TargetToSource:
            pump.FromParts = targetParts;
            pump.ToParts = sourceParts;
            break;
        }
        pump.EditSliderAmount = CalcMaxPumpAmt(pump.FromParts, pump.ToParts, SMAddon.SmVessel.SelectedResources).ToString(CultureInfo.InvariantCulture);
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
    /// <param name="isRatio">causes method to return the smaller ratio pump</param>
    /// <returns></returns>
    internal static TransferPump GetRatioPump(List<TransferPump> pumps, bool isRatio = false)
    {
      if (pumps.Count > 1)
      {
        if (isRatio)
        {
          if (pumps[1].ToCapacity > pumps[0].ToCapacity)
            return pumps[0];
          return pumps[1];
        }
        if (pumps[0].ToCapacity > pumps[1].ToCapacity)
          return pumps[0];
        return pumps[1];
      }
      return pumps[0];
    }

    internal static double CalcRatio(List<TransferPump> pumps)
    {
      if (pumps.Count > 1)
      {
        if (pumps[0].ToCapacity > pumps[1].ToCapacity)
          return pumps[1].ToCapacity / pumps[0].ToCapacity;
        return pumps[0].ToCapacity / pumps[1].ToCapacity;
      }
      return 1;
    }

    internal static uint GetPumpIdFromHash(string resource, Part firstPart, Part lastPart, TypePump pumpType, TriggerButton trigger)
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
