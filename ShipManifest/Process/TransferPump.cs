using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ShipManifest.InternalObjects;
using ShipManifest.Windows;

namespace ShipManifest.Process
{
  /// <summary>
  ///   This class supports the transfer/dump/fill of a desired resource of ResourceType.Other
  /// </summary>
  public class TransferPump
  {
    #region Properties

    public static List<TransferPump> Instance
    {
      get { return SMAddon.SmVessel.TransferPumps; }
    }

    // used for cutting the process speed in half. (every other Update)
    // reduces load, and same amount gets pushed in same time period, as everything is real time flow based.
    public static bool PumpHalfCycleLatch;

    // used during transfer operations.
    public static bool PumpProcessOn { get; internal set; }

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
    ///   Returns a formatted string listing of various properties in this TransferPump instance
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
    ///   Returns capacity of the parts we are transferrring from (sources relative to xferMode)
    /// </summary>
    public double FromCapacity
    {
      get { return CalcResourceCapacity(FromParts, Resource); }
    }

    /// <summary>
    ///   returns amount remaining in parts we are transferrring from (sources relative to XferMode)
    /// </summary>
    public double FromRemaining
    {
      get { return CalcRemainingResource(FromParts, Resource); }
    }

    public bool FromIsFull
    {
      get { return FromCapacity - FromRemaining < SMSettings.Tolerance; }
    }

    public bool FromIsEmpty
    {
      get { return FromRemaining < SMSettings.Tolerance; }
    }

    /// <summary>
    ///   returns capacity of parts we are transferring to (targets relative to xferMode)
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
    ///   returns amount remaining in parts we are transferring to (targets relative to xferMode)
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
      get { return ToCapacity - ToRemaining < SMSettings.Tolerance; }
    }

    public bool ToIsEmpty
    {
      get { return ToRemaining < SMSettings.Tolerance; }
    }


    /// <summary>
    ///   returns amount remaining in parts we are transferring to (targets relative to xferMode)
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
      get { return IsPumpComplete(this); }
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
        return FromCapacity/SMSettings.FlowRate > SMSettings.MaxFlowTimeSec
          ? FromCapacity/SMSettings.MaxFlowTimeSec
          : SMSettings.FlowRate;
      }
    }


    // process vars
    public double PumpRatio { get; internal set; }
    public double AmtPumped { get; internal set; }

    public double PumpBalance
    {
      get { return PumpAmount - AmtPumped; }
    }


    // for Viewers
    private string _editSliderAmount = "0";

    internal string EditSliderAmount
    {
      get { return _editSliderAmount; }
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

    internal TransferPump()
    {

    }

    internal TransferPump(string resourceName, TypePump pumpType, TriggerButton trigger, double pumpAmount)
    {
      Resource = resourceName;
      PumpType = pumpType;
      PumpTrigger = trigger;
      PumpAmount = pumpAmount;
    }

    #endregion

    #region Processing Methods

    internal static void ProcessActivePumps()
    {
      //Utilities.LogMessage("Entering:  TransferPump.ProcessActivePumps", Utilities.LogType.Info, SMSettings.VerboseLogging);
      // This routine acts as a queue
      // WHen a pump is set to IsPumOn = true, it will be processed by this routine.
      List<TransferPump> pumpsToRemove = new List<TransferPump>();
      // this task runs every Update when active.

      try
      {
        List<TransferPump>.Enumerator tPumps = SMAddon.SmVessel.TransferPumps.GetEnumerator();
        while (tPumps.MoveNext())
        {
          if (tPumps.Current == null) continue;
          // Check Pump state:
          if (!tPumps.Current.IsPumpOn) continue;
          TransferPump pump = tPumps.Current;
          switch (pump.PumpStatus)
          {
            case PumpState.Off:
              //Utilities.LogMessage("Entering:  TransferPump.ProcessActivePumps - Off", Utilities.LogType.Info, SMSettings.VerboseLogging);
              pump.TimeStamp = DateTime.Now;
              pump.Start();
              pump.PumpStatus = PumpState.Start;
              break;
            case PumpState.Start:
              //Utilities.LogMessage("Entering:  TransferPump.ProcessActivePumps - Start", Utilities.LogType.Info, SMSettings.VerboseLogging);
              // Calculate Elapsed.
              pump.Elapsed += (DateTime.Now - pump.TimeStamp).TotalSeconds;
              if (pump.Elapsed >= SMSound.SourcePumpStart.clip.length - 0.25)
              {
                pump.Run();
                pump.PumpStatus = PumpState.Run;
              }
              break;
            case PumpState.Run:
              //Utilities.LogMessage("Entering:  TransferPump.ProcessActivePumps - Run", Utilities.LogType.Info, SMSettings.VerboseLogging);
              // 1.  Get Elapsed from last run
              double deltaT = (DateTime.Now - pump.TimeStamp).TotalSeconds;

              // 2. Lets wait long enough to get a resource volume worth moving
              pump.TimeStamp = DateTime.Now;
              pump.Running(deltaT);
              break;
            case PumpState.Stop:
              //Utilities.LogMessage("Entering:  TransferPump.ProcessActivePumps - Stop", Utilities.LogType.Info, SMSettings.VerboseLogging);
              pump.Stop();
              pump.PumpStatus = PumpState.Off;
              pumpsToRemove.Add(pump);
              break;
          }
        }
        if (pumpsToRemove.Count <= 0) return;
        List<TransferPump>.Enumerator rpumps = pumpsToRemove.GetEnumerator();
        while (rpumps.MoveNext())
        {
          if (rpumps.Current == null) continue;
          SMAddon.SmVessel.TransferPumps.Remove(rpumps.Current);
        }
        pumpsToRemove.Clear();
      }
      catch (Exception ex)
      {
        if (!SMAddon.FrameErrTripped)
        {
          Utilities.LogMessage(
            string.Format(" in TransferPump.ProcessActivePumps (repeating error).  Error:  {0} \r\n\r\n{1}", ex.Message,
              ex.StackTrace), Utilities.LogType.Error, true);
          SMAddon.FrameErrTripped = true;
        }
      }
      finally
      {
        PumpProcessOn = IsAnyPumpOn();
        UpdateDisplayPumps();
      }
    }

    private void Start()
    {
      //Utilities.LogMessage("Entering:  TransferPump.Start", Utilities.LogType.Info, SMSettings.VerboseLogging);
      // reset counters
      Elapsed = 0;
      DefaultFlowRate = SMSettings.FlowRate;
      AmtPumped = 0;
      SMSound.SourcePumpStart.Play();
    }

    private void Run()
    {
      //Utilities.LogMessage("Entering:  TransferPump.Run", Utilities.LogType.Info, SMSettings.VerboseLogging);
      if (SMSound.SourcePumpStart.isPlaying) SMSound.SourcePumpStart.Stop();

      // This is the run sound.  we need this only if it is not already playing..
      if (!SMSound.SourcePumpRun.isPlaying)
      {
        SMSound.SourcePumpRun.Play();
      }
      // reset timer for next step
      Elapsed = 0;
    }

    private void Running(double deltaT)
    {
      //Utilities.LogMessage("Entering:  TransferPump.Running", Utilities.LogType.Info, SMSettings.VerboseLogging);
      // 1.  Calculate amount to move based on flow rate and time delta
      double deltaAmt = deltaT*FlowRate*PumpRatio;

      // 2.  Determine if move amount exceeds remaining amount in tank(s)  
      deltaAmt = deltaAmt > FromRemaining ? FromRemaining : deltaAmt;
      if (PumpType != TypePump.Dump)
        deltaAmt = deltaAmt > ToCapacityRemaining ? ToCapacityRemaining : deltaAmt;

      if (deltaAmt > SMSettings.Tolerance)
      {
        double deltaCharge = deltaAmt*SMSettings.FlowCost;
        // 3.  Drain Charge
        if (ConsumeCharge(deltaCharge))
        {
          // 4.  Get list of From parts & Pump Resource
          RunPumpCycle(deltaAmt);
        }
        else
        {
          // 5.  Report Lack of charge and turn off pump
          PumpStatus = PumpState.Stop;
        }
      }
    }

    private void Stop()
    {
      //Utilities.LogMessage("Entering:  TransferPump.Stop", Utilities.LogType.Info, SMSettings.VerboseLogging);
      // Reset transfer related pump vars.
      IsPumpOn = false;

      // If no other pumps running, then turn off run sound.
      if (!IsAnyPumpOn()) SMSound.SourcePumpRun.Stop();

      // play pump shutdown.
      SMSound.SourcePumpStop.Play();
      Elapsed = 0;
    }

    /// <summary>
    /// Executes a Pump Cycle.  
    /// A pump cycle is the amount moved in a single update event.
    /// </summary>
    /// <param name="cycleAmount"></param>
    internal void RunPumpCycle(double cycleAmount)
    {
      //Utilities.LogMessage("Entering:  TransferPump.RunPumpCycle", Utilities.LogType.Info, SMSettings.VerboseLogging);
      // If a Transfer, lets validate From pump amount can be pumped. (and To if needed)
      if (PumpType != TypePump.Dump)
      {
        double maxAmount = CalcMaxPumpAmt(FromParts, ToParts, new List<string> {Resource});
        if (cycleAmount > maxAmount) cycleAmount = maxAmount;
      }

      // From Parts.  used by Dump and Transfer.
      DrainParts(cycleAmount);

      // To Parts (increment)
      if (PumpType != TypePump.Dump)
      {
        FillParts(cycleAmount);
      }
      // -------------------------------------------------------------------
      if (IsComplete)
        PumpStatus = PumpState.Stop;
      //Utilities.LogMessage(string.Format("Exiting:  TransferPump.RunPumpCycle.  PumpStatus = {0}", PumpStatus), Utilities.LogType.Info, SMSettings.VerboseLogging);
    }

    /// <summary>
    /// Fill the ToParts with the cycleAmount
    /// This method assumes the cycleAmount can be applied.
    /// </summary>
    /// <param name="cycleAmount"></param>
    internal void FillParts(double cycleAmount)
    {
      //Utilities.LogMessage("Entering:  TransferPump.FillParts", Utilities.LogType.Info, SMSettings.VerboseLogging);
      // To parts.  Used only by Transfers
      if (cycleAmount < SMSettings.Tolerance) return;

      double cycleBalance = cycleAmount;
      while (cycleBalance > SMSettings.Tolerance)
      {
        // calc average of parts in list.
        double toPartAvgAmt = cycleAmount/ToParts.Count;

        // reduce to the smallest container.
        double minAmt = toPartAvgAmt;
        int remainingPartsCount = 0;
        List<Part>.Enumerator theseParts = ToParts.GetEnumerator();
        while (theseParts.MoveNext())
        {
          if (theseParts.Current == null) continue;
          double thisPartCap = PartRemainingCapacity(theseParts.Current, Resource);
          if (thisPartCap <= SMSettings.Tolerance) continue;
          if (thisPartCap >= minAmt)
          {
            remainingPartsCount++;
            continue;
          }
          minAmt = thisPartCap;
          remainingPartsCount++;
        }

        //Utilities.LogMessage(string.Format("Inside:  TransferPump.FillParts:  toPartAmt = {0}, minAmt = {1}, PartsLeft = {2}, cycleBalance = {3}", toPartAmt, minAmt[0], toPartCount, cycleBalance), Utilities.LogType.Info, SMSettings.VerboseLogging);
        // Calculate pump amounts for each To part and Increment.
        if (remainingPartsCount > 0)
        {
          List<Part>.Enumerator toParts = ToParts.GetEnumerator();
          while (toParts.MoveNext())
          {
            if (toParts.Current == null) continue;
            if (PartRemainingCapacity(toParts.Current, Resource) <= SMSettings.Tolerance) continue;
            Part part = toParts.Current;
            part.Resources[Resource].amount += minAmt;
            cycleBalance -= minAmt;

            // Ensure part is capped and does not contain more than allowed.
            if (part.Resources[Resource].amount > part.Resources[Resource].maxAmount)
              part.Resources[Resource].amount = part.Resources[Resource].maxAmount;
          }
        }
        //Utilities.LogMessage(string.Format("Inside:  TransferPump.FillParts:  toPartAmt = {0}, minAmt = {1}, PartsLeft = {2}, cycleBalance = {3}", toPartAmt, minAmt[0], toPartCount, cycleBalance), Utilities.LogType.Info, SMSettings.VerboseLogging);
        if (remainingPartsCount == 0 && cycleBalance > SMSettings.Tolerance) cycleBalance = 0;
      }
    }

    internal void DrainParts(double cycleAmount)
    {
      //Utilities.LogMessage("Entering:  TransferPump.DrainParts.", Utilities.LogType.Info, SMSettings.VerboseLogging);
      // Lets account for any empty/full containers
      // now split up the xfer amount evenly across the number of tanks that can send/receive resources
      if (cycleAmount < SMSettings.Tolerance) return;

      double cycleBalance = cycleAmount;
      while (cycleBalance > SMSettings.Tolerance)
      {
        // calc average of parts in list.
        double fromPartAvgAmt = cycleAmount / FromParts.Count;

        // reduce to the smallest container.
        double minAmt = fromPartAvgAmt;
        int remainingPartsCount = 0;
        List<Part>.Enumerator theseParts = FromParts.GetEnumerator();
        while (theseParts.MoveNext())
        {
          if (theseParts.Current == null) continue;
          if (theseParts.Current.Resources[Resource].amount <= SMSettings.Tolerance) continue;
          if (theseParts.Current.Resources[Resource].amount >= minAmt)
          {
            remainingPartsCount++;
            continue;
          }
          minAmt = theseParts.Current.Resources[Resource].amount;
          remainingPartsCount++;
        }

        //Utilities.LogMessage(string.Format("Inside:  TransferPump.DrainParts:  fromPartAmt = {0}, minAmt = {1}, PartsLeft = {2}, cycleBalance = {3}", fromPartAmt, minAmt, fromPartCount, cycleBalance), Utilities.LogType.Info, SMSettings.VerboseLogging);
        // Decrement.
        if (remainingPartsCount > 0)
        {
          List<Part>.Enumerator fromParts = FromParts.GetEnumerator();
          while (fromParts.MoveNext())
          {
            if (fromParts.Current == null) continue;
            if (fromParts.Current.Resources[Resource].amount <= SMSettings.Tolerance) continue;
            Part part = fromParts.Current;
            part.Resources[Resource].amount -= minAmt;
            cycleBalance -= minAmt;
            AmtPumped += minAmt;

            // Ensure part is empty and does not contain less than 0.
            if (part.Resources[Resource].amount <= SMSettings.Tolerance) part.Resources[Resource].amount = 0;
          }
        }
        if (remainingPartsCount == 0 && cycleBalance > SMSettings.Tolerance) cycleBalance = 0;
        //Utilities.LogMessage(string.Format("Inside:  TransferPump.DrainParts:  fromPartAmt = {0}, minAmt = {1}, PartsLeft = {2}, cycleBalance = {3}", fromPartAmt, minAmt, fromPartCount, cycleBalance), Utilities.LogType.Info, SMSettings.VerboseLogging);
      }
    }

    internal static bool ConsumeCharge(double deltaCharge)
    {
      //Utilities.LogMessage("Entering:  TransferPump.ConsumeCharge", Utilities.LogType.Info, SMSettings.VerboseLogging);
      if (SMAddon.SmVessel.SelectedResources.Contains("ElectricCharge") || !SMSettings.EnableXferCost) return true;
      List<Part>.Enumerator iParts = SMAddon.SmVessel.PartsByResource["ElectricCharge"].GetEnumerator();
      while (iParts.MoveNext())
      {
        if (iParts.Current == null) continue;
        Part iPart = iParts.Current;
        if (iPart.Resources["ElectricCharge"].amount >= deltaCharge)
        {
          iPart.Resources["ElectricCharge"].amount -= deltaCharge;
          deltaCharge = 0;
          break;
        }
        deltaCharge -= iPart.Resources["ElectricCharge"].amount;
        iPart.Resources["ElectricCharge"].amount = 0;
      }
      return !(deltaCharge > 0);
    }

    public void Abort()
    {
      PumpStatus = PumpState.Stop;
    }

    public static void AbortAllPumpsInProcess(uint pumpId)
    {
      // set a state vars and wait for the next update to pick it up.
      List<TransferPump>.Enumerator tPumps = SMAddon.SmVessel.TransferPumps.GetEnumerator();
      while (tPumps.MoveNext())
      {
        if (tPumps.Current == null) continue;
        if (tPumps.Current.PumpId == pumpId && tPumps.Current.IsPumpOn) tPumps.Current.PumpStatus = PumpState.Stop;
      }
    }

    internal static void AssignPumpAmounts(List<TransferPump> pumps, double pumpAmount, uint pumpId)
    {
      if (pumps.Count > 1)
      {
        // Calculate Ratio and transfer amounts.  Ratio is based off the largest amount to move, so will always be less than 1.
        double ratio = CalcRatio(pumps);
        pumps[0].PumpId = pumpId;
        pumps[1].PumpId = pumpId;

        if (pumps[0].ToCapacity > pumps[1].ToCapacity)
        {
          pumps[0].PumpRatio = 1;
          pumps[1].PumpRatio = ratio;
          pumps[0].PumpAmount = pumpAmount;
          pumps[1].PumpAmount = pumpAmount*ratio <= pumps[1].FromCapacity ? pumpAmount*ratio : pumps[1].FromCapacity;
        }
        else
        {
          pumps[1].PumpRatio = 1;
          pumps[0].PumpRatio = ratio;
          pumps[1].PumpAmount = pumpAmount;
          pumps[0].PumpAmount = pumpAmount*ratio <= pumps[0].FromCapacity ? pumpAmount*ratio : pumps[0].FromCapacity;
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
      return PumpProcessOn &&
             (from pump in SMAddon.SmVessel.TransferPumps where pump.IsPumpOn && pump.PumpId == pumpId select pump).Any();
    }

    internal static List<TransferPump> PumpsInProgress(uint pumpId)
    {
      List<TransferPump> newList =
        (from pump in SMAddon.SmVessel.TransferPumps where pump.IsPumpOn && pump.PumpId == pumpId select pump).ToList();
      if (PumpProcessOn)
        return newList;
      newList.Clear();
      return newList;
    }

    internal static double CalcRemainingResource(List<Part> parts, string selectedResource)
    {
      double amount = 0;
      if (parts == null || selectedResource == null || selectedResource == "") return amount;
      List<Part>.Enumerator list = parts.GetEnumerator();
      while (list.MoveNext())
      {
        if (list.Current == null) continue;
        if (!list.Current.Resources.Contains(selectedResource)) continue;
        amount += list.Current.Resources[selectedResource].amount;
      }
      return amount;
    }

    internal static double CalcResourceCapacity(List<Part> parts, string selectedResource)
    {
      double amount = 0;
      if (parts == null || selectedResource == null || selectedResource == "") return amount;
      List<Part>.Enumerator list = parts.GetEnumerator();
      while (list.MoveNext())
      {
        if (list.Current == null) continue;
        if (!list.Current.Resources.Contains(selectedResource)) continue;
        amount += list.Current.Resources[selectedResource].maxAmount;
      }
      return amount;
    }

    internal static double CalcRemainingCapacity(List<Part> parts, string selectedResource)
    {
      double amount = 0;
      if (parts == null || selectedResource == null || selectedResource == "") return amount;
      List<Part>.Enumerator list = parts.GetEnumerator();
      while (list.MoveNext())
      {
        if (list.Current == null) continue;
        if (!list.Current.Resources.Contains(selectedResource)) continue;
        amount += PartRemainingCapacity(list.Current, selectedResource);
      }
      return amount;
    }
    internal static double PartRemainingCapacity(Part part, string selectedResource)
    {
      return part.Resources[selectedResource].maxAmount - part.Resources[selectedResource].amount;
    }

    internal double MaxPumpAmt()
    {
      double maxPumpAmount = 0;
      if (FromParts == null || ToParts == null || FromParts.Count == 0 || ToParts.Count == 0) return maxPumpAmount;
      List<Part>.Enumerator toList = ToParts.GetEnumerator();
      List<Part>.Enumerator fromList = FromParts.GetEnumerator();
      double fromAmount = 0;
      double toCapacityRemaining = 0;
      while (fromList.MoveNext())
      {
        if (fromList.Current == null) continue;
        fromAmount += fromList.Current.Resources[Resource].amount;
      }
      while (toList.MoveNext())
      {
        if (toList.Current == null) continue;
        toCapacityRemaining += PartRemainingCapacity(toList.Current, Resource);
      }
      maxPumpAmount = toCapacityRemaining - fromAmount;
      maxPumpAmount = maxPumpAmount > fromAmount ? fromAmount : maxPumpAmount;
      maxPumpAmount = maxPumpAmount < SMSettings.Tolerance ? 0 : maxPumpAmount;
      return maxPumpAmount;
    }

    internal static double CalcMaxPumpAmt(List<Part> partsFrom, List<Part> partsTo, List<string> selectedResources)
    {
      double maxPumpAmount = 0;
      if (partsFrom == null || partsTo == null || partsFrom.Count == 0 || partsTo.Count == 0 ||
          selectedResources.Count == 0)
        maxPumpAmount = 0;
      else
      {
        // First determine if there is more than one Resource to move.  get the larger number for a ratio basis.
        int resIdx = 0;
        if (selectedResources.Count > 1)
        {
          if (CalcResourceCapacity(partsTo, selectedResources[0]) < CalcResourceCapacity(partsTo, selectedResources[1]))
            resIdx = 1;
        }
        // now lets get the amount to move.  we will use this higher portion to calc lower volume ratio during move.
        double maxFromAmount = 0;
        List<Part>.Enumerator fromParts = partsFrom.GetEnumerator();
        while (fromParts.MoveNext())
        {
          if (fromParts.Current == null) continue;
          if (!fromParts.Current.Resources.Contains(selectedResources[resIdx])) continue;
          maxFromAmount += fromParts.Current.Resources[selectedResources[resIdx]].amount;
        }
        List<Part>.Enumerator toParts = partsTo.GetEnumerator();
        double maxToAmount = 0;
        while (toParts.MoveNext())
        {
          if (toParts.Current == null) continue;
          if (!toParts.Current.Resources.Contains(selectedResources[resIdx])) continue;
          maxToAmount += PartRemainingCapacity(toParts.Current, selectedResources[resIdx]);
        }

        maxPumpAmount = maxToAmount > maxFromAmount ? maxFromAmount : maxToAmount;
        maxPumpAmount = maxPumpAmount < SMSettings.Tolerance ? 0 : maxPumpAmount;
      }
      return maxPumpAmount;
    }

    private static bool IsPumpComplete(TransferPump pump)
    {
      bool results = pump.PumpStatus == PumpState.Off ||
                    pump.PumpStatus == PumpState.Stop ||
                    pump.FromIsEmpty ||
                    (pump.PumpType != TypePump.Dump && pump.ToIsFull) ||
                    pump.AmtPumped >= pump.PumpAmount ||
                    pump.PumpAmount - pump.AmtPumped < SMSettings.Tolerance ||
                    (pump.PumpType != TypePump.Dump &&
                     CalcMaxPumpAmt(pump.FromParts, pump.ToParts, new List<string> {pump.Resource}) <
                     SMSettings.Tolerance);
      return results;
    }

    internal static void CreateDisplayPumps()
    {
      // Now lets update the Resource Xfer pumps for display use...
      WindowTransfer.DisplayPumps.Clear();
      if (!SMConditions.AreSelectedResourcesTypeOther(SMAddon.SmVessel.SelectedResources)) return;
      // Lets create a pump Object for managing pump options and data.
      List<Part> displaySourceParts = WindowTransfer.ShowSourceVessels
        ? SMAddon.SmVessel.GetSelectedVesselsParts(SMAddon.SmVessel.SelectedVesselsSource,
          SMAddon.SmVessel.SelectedResources)
        : SMAddon.SmVessel.SelectedPartsSource;
      List<Part> displayTargetParts = WindowTransfer.ShowTargetVessels
        ? SMAddon.SmVessel.GetSelectedVesselsParts(SMAddon.SmVessel.SelectedVesselsTarget,
          SMAddon.SmVessel.SelectedResources)
        : SMAddon.SmVessel.SelectedPartsTarget;

      List<string>.Enumerator resources = SMAddon.SmVessel.SelectedResources.GetEnumerator();
      while (resources.MoveNext())
      {
        TransferPump pump1 = new TransferPump
        {
          Resource = resources.Current,
          PumpType = TypePump.SourceToTarget,
          PumpAmount = CalcMaxPumpAmt(displaySourceParts, displayTargetParts, SMAddon.SmVessel.SelectedResources),
          EditSliderAmount =
            CalcMaxPumpAmt(displaySourceParts, displayTargetParts, SMAddon.SmVessel.SelectedResources)
              .ToString(CultureInfo.InvariantCulture),
          FromParts = displaySourceParts,
          ToParts = displayTargetParts
        };
        WindowTransfer.DisplayPumps.Add(pump1);

        TransferPump pump2 = new TransferPump
        {
          Resource = resources.Current,
          PumpType = TypePump.TargetToSource,
          PumpAmount = CalcMaxPumpAmt(displayTargetParts, displaySourceParts, SMAddon.SmVessel.SelectedResources),
          EditSliderAmount =
            CalcMaxPumpAmt(displayTargetParts, displaySourceParts, SMAddon.SmVessel.SelectedResources)
              .ToString(CultureInfo.InvariantCulture),
          FromParts = displayTargetParts,
          ToParts = displaySourceParts
        };
        WindowTransfer.DisplayPumps.Add(pump2);
      }
    }

    internal static void UpdateDisplayPumps()
    {
      // Now lets update the Resource Xfer pumps for display use...
      if (!SMConditions.AreSelectedResourcesTypeOther(SMAddon.SmVessel.SelectedResources)) return;
      if (!WindowTransfer.DisplayPumps.Any()) CreateDisplayPumps();

      // Lets create a pump Object for managing pump options and data.
      List<Part> sourceParts = WindowTransfer.ShowSourceVessels
        ? SMAddon.SmVessel.GetSelectedVesselsParts(SMAddon.SmVessel.SelectedVesselsSource,
          SMAddon.SmVessel.SelectedResources)
        : SMAddon.SmVessel.SelectedPartsSource;
      List<Part> targetParts = WindowTransfer.ShowTargetVessels
        ? SMAddon.SmVessel.GetSelectedVesselsParts(SMAddon.SmVessel.SelectedVesselsTarget,
          SMAddon.SmVessel.SelectedResources)
        : SMAddon.SmVessel.SelectedPartsTarget;

      List<TransferPump>.Enumerator dPumps = WindowTransfer.DisplayPumps.GetEnumerator();
      while (dPumps.MoveNext())
      {
        if (dPumps.Current == null) continue;
        // Lets update pump data (it persists).
        TransferPump pump = dPumps.Current;
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
        pump.EditSliderAmount =
          CalcMaxPumpAmt(pump.FromParts, pump.ToParts, SMAddon.SmVessel.SelectedResources)
            .ToString(CultureInfo.InvariantCulture);
      }
    }

    internal static List<TransferPump> GetDisplayPumpsByType(TypePump pumpType)
    {
      List<TransferPump>.Enumerator dPumps = WindowTransfer.DisplayPumps.GetEnumerator();
      List<TransferPump> results = new List<TransferPump>();
      while (dPumps.MoveNext())
      {
        if (dPumps.Current == null) continue;
        if (dPumps.Current.PumpType == pumpType) results.Add(dPumps.Current);
      }
      return results;
    }

    /// <summary>
    ///   this method returns either the largest or smallest ratio pump when compared to the other resource selected.
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
          return pumps[1].ToCapacity/pumps[0].ToCapacity;
        return pumps[0].ToCapacity/pumps[1].ToCapacity;
      }
      return 1;
    }

    internal static uint GetPumpIdFromHash(string resource, Part firstPart, Part lastPart, TypePump pumpType,
      TriggerButton trigger)
    {
      return firstPart.flightID + lastPart.flightID + (uint) pumpType.GetHashCode() + (uint) trigger.GetHashCode() +
             (uint) resource.GetHashCode();
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