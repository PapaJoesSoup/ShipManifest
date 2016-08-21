using System;
using System.Collections;
using System.Collections.Generic;

namespace ShipManifest.Process
{
  internal static class ProcessController
  {
    internal enum Selection
    {
      All,
      OnlyProcessed,
      OnlyUnprocessed,
    };

    internal static void TransferScienceLab(PartModule source, PartModule target, Selection sel)
    {
      try
      {
        // Create lookup list to avoid a slow linear search for each item
        // Value is number of labs that have processed it
        // (would allow to only move data not processed in all labs if there are multiple)
        Dictionary<string, int> processedScience = new Dictionary<string, int>();
        List<ModuleScienceLab>.Enumerator labs = SMAddon.SmVessel.Vessel.FindPartModulesImplementing<ModuleScienceLab>().GetEnumerator();
        while (labs.MoveNext())
        {
          List<string>.Enumerator dataitems = labs.Current.ExperimentData.GetEnumerator();
          while (dataitems.MoveNext())
          {
            if (!processedScience.ContainsKey(dataitems.Current))
              processedScience.Add(dataitems.Current, 1);
            else
              processedScience[dataitems.Current]++;
          }
        }

        ScienceData[] moduleScience = (IScienceDataContainer)source != null ? ((IScienceDataContainer)source).GetData() : null;

        if (moduleScience == null || moduleScience.Length <= 0) return;
        IEnumerator dataItems = moduleScience.GetEnumerator();
        while (dataItems.MoveNext())
        {
          if (dataItems.Current == null) continue;
          ScienceData data = (ScienceData)dataItems.Current;
          bool processed = processedScience.ContainsKey(data.subjectID);

          if ((sel == Selection.OnlyProcessed && processed) ||
              (sel == Selection.OnlyUnprocessed && !processed))
          {
            if (((ModuleScienceContainer)target).AddData(data))
            {
              ((IScienceDataContainer)source).DumpData(data);
            }
          }
        }
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(" in ProcessController.TransferScienceLab:  Error:  " + ex, Utilities.LogType.Info, SMSettings.VerboseLogging);
      }
    }

    internal static void TransferScience(PartModule source, PartModule target)
    {
      try
      {
        ScienceData[] moduleScience = (IScienceDataContainer)source != null ? ((IScienceDataContainer)source).GetData() : null;

        if (moduleScience == null || moduleScience.Length <= 0) return;
        //Utilities.LogMessage("ProcessController.TransferScience:  moduleScience has data...", Utilities.LogType.Info,
        //  SMSettings.VerboseLogging);

        if ((IScienceDataContainer) target == null) return;
        // Lets store the data from the source.
        if (!((ModuleScienceContainer) target).StoreData(
          new List<IScienceDataContainer> {(IScienceDataContainer) source}, false)) return;

        //Utilities.LogMessage("ProcessController.TransferScience:  ((ModuleScienceContainer)source) data stored",
        //  "Info", SMSettings.VerboseLogging);
        IEnumerator dataItems = moduleScience.GetEnumerator();
        while (dataItems.MoveNext())
        {
          if (dataItems.Current == null) continue;
          ScienceData data = (ScienceData) dataItems.Current;
          ((IScienceDataContainer) source).DumpData(data);
        }

        if (!SMSettings.RealismMode) ((ModuleScienceExperiment)source).ResetExperiment();
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(" in ProcessController.TransferScience:  Error:  " + ex, Utilities.LogType.Info, SMSettings.VerboseLogging);
      }
    }

    /// <summary>
    ///   This method is called by WindowTransfer.Xferbutton press.
    /// </summary>
    /// <param name="xferPumps"></param>
    internal static void TransferResources(List<TransferPump> xferPumps)
    {
      try
      {
        if (SMSettings.RealismMode)
        {
          List<TransferPump>.Enumerator pumps = xferPumps.GetEnumerator();
          while (pumps.MoveNext())
          {
            if (pumps.Current == null) continue;
            TransferPump pump = pumps.Current;
            pump.IsPumpOn = true;
          }
          // now lets start the pumping process...
          SMAddon.SmVessel.TransferPumps.AddRange(xferPumps);

          // Start the process.  This flag is checked in SMAddon.Update()
          TransferPump.PumpProcessOn = true;
        }
        else
        {
          //Not in Realism mode, so just move the resource...
          List<TransferPump>.Enumerator pumps = xferPumps.GetEnumerator();
          while (pumps.MoveNext())
          {
            if (pumps.Current == null) continue;
            TransferPump pump = pumps.Current;
            pump.RunPumpCycle(pump.PumpAmount);
          }
        }
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(
          string.Format(" in  ProcessController.TransferResources.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace),
          Utilities.LogType.Error, true);
      }
    }

    internal static void DumpResources(List<TransferPump> pumps)
    {
      // This initiates the Dump process and with realism off, does the dump immediately; with realism on, initiates the real time process.
      try
      {
        if (SMSettings.RealismMode)
        {
          // Turn on Pumps for timed process...
          List<TransferPump>.Enumerator epumps = pumps.GetEnumerator();
          while (epumps.MoveNext())
          {
            if (epumps.Current == null) continue;
            TransferPump pump = epumps.Current;
            pump.PumpRatio = 1;
            pump.IsPumpOn = true;
          }
          // Add pumps to pump queue
          SMAddon.SmVessel.TransferPumps.AddRange(pumps);

          // Start the process.  This flag is checked in SMAddon.Update()
          TransferPump.PumpProcessOn = true;
        }
        else
        {
          List<TransferPump>.Enumerator epumps = pumps.GetEnumerator();
          while (epumps.MoveNext())
          {
            if (epumps.Current == null) continue;
            TransferPump pump = epumps.Current;
            pump.RunPumpCycle(pump.PumpAmount);
          }
          SMAddon.SmVessel.TransferPumps.Clear();
        }
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(
          string.Format(" in  ProcessController.DumpResources.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace),
          Utilities.LogType.Error, true);
      }
    }
  }
}