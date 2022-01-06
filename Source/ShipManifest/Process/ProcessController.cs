using System;
using System.Collections;
using System.Collections.Generic;
using ShipManifest.InternalObjects.Settings;
using UniLinq;

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
          if (labs.Current == null) continue;
          List<string>.Enumerator dataitems = labs.Current.ExperimentData.GetEnumerator();
          while (dataitems.MoveNext())
          {
            if (dataitems.Current == null) continue;
            if (!processedScience.ContainsKey(dataitems.Current))
              processedScience.Add(dataitems.Current, 1);
            else
              processedScience[dataitems.Current]++;
          }
          dataitems.Dispose();
        }
        labs.Dispose();

        ScienceData[] moduleScience = ((IScienceDataContainer) source)?.GetData();

        if (moduleScience == null || moduleScience.Length <= 0) return;
        IEnumerator dataItems = moduleScience.GetEnumerator();
        while (dataItems.MoveNext())
        {
          if (dataItems.Current == null) continue;
          ScienceData data = (ScienceData)dataItems.Current;
          bool processed = processedScience.ContainsKey(data.subjectID);

          if ((sel != Selection.OnlyProcessed || !processed) &&
              (sel != Selection.OnlyUnprocessed || processed)) continue;
          if (((ModuleScienceContainer)target).AddData(data))
          {
            ((IScienceDataContainer)source).DumpData(data);
          }
        }
      }
      catch (Exception ex)
      {
        SmUtils.LogMessage($" in ProcessController.TransferScienceLab:  Error:  {ex}", SmUtils.LogType.Info, CurrSettings.VerboseLogging);
      }
    }

    internal static void TransferScience(List<Part> source, List<Part> target)
    {
      // get part modules for source and target
      List<IScienceDataContainer> sourceContainer = ScienceDataContainers(source, true);
      List<IScienceDataContainer> targetContainer = ScienceDataContainers(target);
      if (sourceContainer.Count <= 0 || targetContainer.Count <= 0) return;

      // Pick a destination part. Preferably the Science Lab with the most science stored,  If no lab, then Science Container with most science stored.
      IScienceDataContainer toContainer = null;
      bool labFound = false;
      List<Part>.Enumerator p = target.GetEnumerator();

      while (p.MoveNext())
      {
        if (p.Current == null) continue;
        if (!p.Current.FindModulesImplementing<ModuleScienceLab>().Any()) continue;
        labFound = true;
        IScienceDataContainer m = (IScienceDataContainer) p.Current.FindModulesImplementing<ModuleScienceLab>().First();
        if (m.GetScienceCount() > toContainer.GetScienceCount()) toContainer = m;
      }
      p.Dispose();
      if (!labFound)
      {
        // if none of the above, pick the first Container, then check for the container holding the largest amount of science, and move it all there.
        List<Part>.Enumerator p2 = target.GetEnumerator();

        while (p2.MoveNext())
        {
          if (p2.Current == null) continue;
          if (!p2.Current.FindModulesImplementing<IScienceDataContainer>().Any()) continue;
          IScienceDataContainer m = p2.Current.FindModulesImplementing<IScienceDataContainer>().First();
          if (toContainer == null) toContainer = m;
          if (m.GetScienceCount() > toContainer.GetScienceCount()) toContainer = m;
        }
        p2.Dispose();
      }


      // determine if science can be moved.
      List<IScienceDataContainer>.Enumerator scontainer = sourceContainer.GetEnumerator();
      while (scontainer.MoveNext())
      {
        if (scontainer.Current == null) continue;
        // Move science to destination.        }


      }
      scontainer.Dispose();
    }

    private static List<IScienceDataContainer> ScienceDataContainers(List<Part> source, bool hasData = false)
    {
      List<IScienceDataContainer> containers = new List<IScienceDataContainer>();
      List<Part>.Enumerator part = source.GetEnumerator();
      while (part.MoveNext())
      {
        if (part.Current == null) continue;
        List<IScienceDataContainer>.Enumerator sdModule =
          part.Current.FindModulesImplementing<IScienceDataContainer>().GetEnumerator();
        while (sdModule.MoveNext())
        {
          if (sdModule.Current == null) continue;
          if (hasData && sdModule.Current.GetScienceCount() <= 0) continue;
          containers.Add(sdModule.Current);
        }
        sdModule.Dispose();
      }
      part.Dispose();
      return containers;
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

        if (!CurrSettings.RealControl) ((ModuleScienceExperiment)source).ResetExperiment();
      }
      catch (Exception ex)
      {
        SmUtils.LogMessage($" in ProcessController.TransferScience:  Error:  {ex}", SmUtils.LogType.Info, CurrSettings.VerboseLogging);
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
        if (CurrSettings.RealXfers)
        {
          List<TransferPump>.Enumerator pumps = xferPumps.GetEnumerator();
          while (pumps.MoveNext())
          {
            if (pumps.Current == null) continue;
            TransferPump pump = pumps.Current;
            pump.IsPumpOn = true;
          }
          pumps.Dispose();
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
          pumps.Dispose();
        }
      }
      catch (Exception ex)
      {
        SmUtils.LogMessage(
          $" in  ProcessController.TransferResources.  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}",
          SmUtils.LogType.Error, true);
      }
    }

    internal static void DumpResources(List<TransferPump> pumps)
    {
      // This initiates the Dump process and with realism off, does the dump immediately; with realism on, initiates the real time process.
      try
      {
        if (CurrSettings.RealXfers)
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
          epumps.Dispose();
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
          epumps.Dispose();
          SMAddon.SmVessel.TransferPumps.Clear();
        }
      }
      catch (Exception ex)
      {
        SmUtils.LogMessage(
          $" in  ProcessController.DumpResources.  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}",
          SmUtils.LogType.Error, true);
      }
    }
  }
}
