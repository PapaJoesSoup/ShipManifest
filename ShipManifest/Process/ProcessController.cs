using System;
using System.Collections.Generic;

namespace ShipManifest.Process
{
  internal static class ProcessController
  {
    internal static void TransferScience(PartModule source, PartModule target)
    {
      try
      {
        var moduleScience = (IScienceDataContainer)source != null ? ((IScienceDataContainer)source).GetData() : null;

        if (moduleScience != null && moduleScience.Length > 0)
        {
          Utilities.LogMessage("moduleScience has data...", "Info", SMSettings.VerboseLogging);

          if ((IScienceDataContainer)target != null)
          {
            // Lets store the data from the source.
            if (((ModuleScienceContainer)target).StoreData(new List<IScienceDataContainer> { (IScienceDataContainer)source }, false))
            {
              Utilities.LogMessage("((ModuleScienceContainer)source) data stored", "Info", SMSettings.VerboseLogging);
              foreach (var data in moduleScience)
                ((IScienceDataContainer)source).DumpData(data);

              if (SMSettings.RealismMode)
                Utilities.LogMessage("((Module ScienceExperiment xferred.  Dump Source data", "Info", SMSettings.VerboseLogging);
              else
              {
                Utilities.LogMessage("((Module ScienceExperiment xferred.  Dump Source data, reset Experiment", "Info", SMSettings.VerboseLogging);
                ((ModuleScienceExperiment)source).ResetExperiment();
              }
            }
            else
            {
              Utilities.LogMessage("Science Data transfer failed...", "Info", SMSettings.VerboseLogging);
            }
          }
          else
          {
            Utilities.LogMessage("((IScienceDataContainer)target) is null", "Info", SMSettings.VerboseLogging);
          }
          Utilities.LogMessage("Transfer Complete.", "Info", SMSettings.VerboseLogging);
        }
        else if (moduleScience == null)
        {
          Utilities.LogMessage("moduleScience is null...", "Info", SMSettings.VerboseLogging);
        }
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(" in TransferScience:  Error:  " + ex, "Info", SMSettings.VerboseLogging);
      }
    }

    internal static void TransferResources(List<Part> source, List<Part> target)
    {
      try
      {
        // Create Xfer Objects for timed process...
        var xferPumps = SMAddon.SmVessel.TransferPumps;

        if (SMSettings.RealismMode)
        {
          // This flag enables the Update handler in SmAddon and sets the direction
          SMAddon.ActivePumpType = source == SMAddon.SmVessel.SelectedPartsSource ? TransferPump.PumpType.SourceToTarget : TransferPump.PumpType.TargetToSource;

          // let's get the capacity of the source for flow calculations.
          // Flow is based on the largest resource capacity
          var amtCapacity = xferPumps[0].FromCapacity(SMAddon.ActivePumpType);
          if (xferPumps.Count == 2)
            if (xferPumps[1].FromCapacity(SMAddon.ActivePumpType) > amtCapacity)
              amtCapacity = xferPumps[1].FromCapacity(SMAddon.ActivePumpType);

          // Calculate the actual flow rate, based on source capacity and max flow time setting...
          xferPumps[0].ActFlowRate = amtCapacity / SMSettings.FlowRate > SMSettings.MaxFlowTimeSec ? amtCapacity / SMSettings.MaxFlowTimeSec : SMSettings.FlowRate;
          if (xferPumps.Count == 2) xferPumps[1].ActFlowRate = xferPumps[0].ActFlowRate;

          // now lets make some noise and simulate the pumping process...
          Utilities.LogMessage("Playing pump sound...", "Info", SMSettings.VerboseLogging);

          // Start the process
          TransferPump.PumpXferActive = true;
        }
        else
        {
          //Not in Realism mode, so just move the resource...
          foreach (var pump in xferPumps)
          {
            TransferPump.XferResource(source, pump, pump.XferAmount(SMAddon.ActivePumpType), true);
            TransferPump.XferResource(target, pump, pump.XferAmount(SMAddon.ActivePumpType), false);
          }
        }
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(string.Format(" in  TransferResource.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
      }
    }

    internal static void DumpResources(List<TransferPump> pumps)
    {
      // This initiates the Dump process and either does the dump (realism Off, or initiates the Real time process.
      try
      {
        if (SMSettings.RealismMode)
        {
          // Create Xfer Objects for timed process...

          // Calculate the actual flow rate, based on source capacity and max flow time setting...
          foreach (var pump in pumps)
          {
            // let's get the capacity of the source for flow calculations.
            // Flow is based on the largest resource capacity
            var amtCapacity = TransferPump.CalcResourceCapacity(pump.DumpParts, pump.ResourceName);
            pump.ActFlowRate = amtCapacity / SMSettings.FlowRate > SMSettings.MaxFlowTimeSec ? amtCapacity / SMSettings.MaxFlowTimeSec : SMSettings.FlowRate;
          }

          // now lets make some noise and simulate the pumping process...
          Utilities.LogMessage("Playing pump sound...", "Info", SMSettings.VerboseLogging);

          // Start the process
          TransferPump.PumpDumpActive = true;
        }
        else
        {
          foreach (var pump in pumps)
          {
            TransferPump.DumpResource(pump);
          }
          SMAddon.SmVessel.TransferPumps.Clear();
        }
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(string.Format(" in  DumpResources.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
      }
    }

    //internal static void DumpResources(List<Part> parts, List<string> dumpResources, List<TransferPump> pumps)
    //{
    //  // This is fired by a button click somewhere.
    //  try
    //  {
    //    // Create Xfer Objects for timed process...
    //    if (SMSettings.RealismMode)
    //    {
    //      // let's get the capacity of the source for flow calculations.
    //      // Flow is based on the largest resource capacity
    //      var amtCapacity = TransferPump.CalcResourceCapacity(parts, dumpResources[0]);
    //      if (dumpResources.Count == 2 && TransferPump.CalcResourceCapacity(parts, dumpResources[1]) > amtCapacity)
    //        amtCapacity = TransferPump.CalcResourceCapacity(parts, dumpResources[1]);

    //      // Calculate the actual flow rate, based on source capacity and max flow time setting...
    //      pump.ActFlowRate = amtCapacity / SMSettings.FlowRate > SMSettings.MaxFlowTimeSec ? amtCapacity / SMSettings.MaxFlowTimeSec : SMSettings.FlowRate;

    //      // now lets make some noise and simulate the pumping process...
    //      Utilities.LogMessage("Playing pump sound...", "Info", SMSettings.VerboseLogging);

    //      // Start the process
    //      TransferPump.ResourceDumpActive = true;
    //    }
    //    else
    //    {
    //      //Not in Realism mode, so just move the resource...
    //      foreach (var resource in dumpResources)
    //      {
    //        TransferPump.DumpResource(parts, dumpResources);
    //      }
    //    }
    //  }
    //  catch (Exception ex)
    //  {
    //    Utilities.LogMessage(string.Format(" in  DumpResources.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
    //  }
    //}
  }
}
