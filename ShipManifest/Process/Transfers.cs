using System;
using System.Collections.Generic;

namespace ShipManifest.Process
{
  internal static class Transfers
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
        var xferResources = SMAddon.SmVessel.ResourcesToXfer;

        if (SMSettings.RealismMode)
        {
          // This flag enables the Update handler in SmAddon and sets the direction
          SMAddon.XferMode = source == SMAddon.SmVessel.SelectedPartsSource ? SMAddon.XferDirection.SourceToTarget : SMAddon.XferDirection.TargetToSource;

          // let's get the capacity of the source for flow calculations.
          // Flow is based on the largest resource capacity
          var amtCapacity = xferResources[0].FromCapacity(SMAddon.XferMode);
          if (xferResources.Count == 2)
            if (xferResources[1].FromCapacity(SMAddon.XferMode) > amtCapacity)
              amtCapacity = xferResources[1].FromCapacity(SMAddon.XferMode);

          // Calculate the actual flow rate, based on source capacity and max flow time setting...
          TransferResource.ActFlowRate = amtCapacity / SMSettings.FlowRate > SMSettings.MaxFlowTimeSec ? amtCapacity / SMSettings.MaxFlowTimeSec : SMSettings.FlowRate;

          // now lets make some noise and simulate the pumping process...
          Utilities.LogMessage("Playing pump sound...", "Info", SMSettings.VerboseLogging);

          // Start the process
          TransferResource.ResourceXferActive = true;
        }
        else
        {
          //Not in Realism mode, so just move the resource...
          foreach (var modResource in xferResources)
          {
            TransferResource.XferResource(source, modResource, modResource.XferAmount(SMAddon.XferMode), SMAddon.XferMode, true);
            TransferResource.XferResource(target, modResource, modResource.XferAmount(SMAddon.XferMode), SMAddon.XferMode, false);
          }
        }
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(string.Format(" in  TransferResource.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
      }
    }

  }
}
