using System;
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
		var processedScience = new Dictionary<string, int>();

		foreach (var lab in SMAddon.SmVessel.Vessel.FindPartModulesImplementing<ModuleScienceLab>())
		{
			foreach (var data in lab.ExperimentData)
			{
				if (!processedScience.ContainsKey(data))
					processedScience.Add(data, 1);
				else
					processedScience[data]++;
			}
		}

		var moduleScience = (IScienceDataContainer) source != null ? ((IScienceDataContainer) source).GetData() : null;

		if (moduleScience != null && moduleScience.Length > 0)
		{
			foreach (var data in moduleScience)
			{
				bool processed = processedScience.ContainsKey(data.subjectID);

				if ( (sel == Selection.OnlyProcessed && processed) ||
					 (sel == Selection.OnlyUnprocessed && !processed))
				{
					if (((ModuleScienceContainer) target).AddData(data))
					{
						((IScienceDataContainer) source).DumpData(data);
					}
				}
			}
		}
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(" in ProcessController.TransferScience:  Error:  " + ex, "Info", SMSettings.VerboseLogging);
      }
    }

    internal static void TransferScience(PartModule source, PartModule target)
    {
      try
      {
        var moduleScience = (IScienceDataContainer) source != null ? ((IScienceDataContainer) source).GetData() : null;

        if (moduleScience != null && moduleScience.Length > 0)
        {
          //Utilities.LogMessage("ProcessController.TransferScience:  moduleScience has data...", "Info",
          //  SMSettings.VerboseLogging);

          if ((IScienceDataContainer) target != null)
          {
            // Lets store the data from the source.
            if (
              ((ModuleScienceContainer) target).StoreData(
                new List<IScienceDataContainer> {(IScienceDataContainer) source}, false))
            {
              //Utilities.LogMessage("ProcessController.TransferScience:  ((ModuleScienceContainer)source) data stored",
              //  "Info", SMSettings.VerboseLogging);
              foreach (var data in moduleScience) ((IScienceDataContainer) source).DumpData(data);

              if (!SMSettings.RealismMode) ((ModuleScienceExperiment) source).ResetExperiment();
            }
          }
        }
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(" in ProcessController.TransferScience:  Error:  " + ex, "Info", SMSettings.VerboseLogging);
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
          foreach (var pump in xferPumps)
          {
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
          foreach (var pump in xferPumps)
          {
            pump.RunCycle(pump.PumpAmount);
          }
        }
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(
          string.Format(" in  ProcessController.TransferResources.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace),
          "Error", true);
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
          foreach (var pump in pumps)
          {
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
          foreach (var pump in pumps)
          {
            pump.RunCycle(pump.PumpAmount);
          }
          SMAddon.SmVessel.TransferPumps.Clear();
        }
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(
          string.Format(" in  ProcessController.DumpResources.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace),
          "Error", true);
      }
    }
  }
}