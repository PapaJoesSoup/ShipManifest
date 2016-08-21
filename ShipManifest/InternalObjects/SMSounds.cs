using System;
using UnityEngine;

namespace ShipManifest.InternalObjects
{
  // ReSharper disable once InconsistentNaming
  internal class SMSound
  {
    #region Static vars...
    // Default sound license: CC-By-SA
    // http://www.freesound.org/people/adcbicycle/sounds/14214/
    internal static string AudioSourceCrewPathStart = SMSettings.CrewSoundStart ?? "ShipManifest/Sounds/14214-1";
    internal static string AudioSourceCrewPathRun = SMSettings.CrewSoundRun ?? "ShipManifest/Sounds/14214-2";
    internal static string AudioSourceCrewPathStop = SMSettings.CrewSoundStop ?? "ShipManifest/Sounds/14214-3";

    // Default sound license: CC-By-SA
    // http://www.freesound.org/people/vibe_crc/sounds/59328/
    internal static string SourcePumpPathStart = SMSettings.PumpSoundStart ?? "ShipManifest/Sounds/59328-1";
    internal static string SourcePumpPathRun = SMSettings.PumpSoundRun ?? "ShipManifest/Sounds/59328-2";
    internal static string SourcePumpPathStop = SMSettings.PumpSoundStop ?? "ShipManifest/Sounds/59328-3";

    // Resource transfer vars
    internal static AudioSource SourcePumpStart;
    internal static AudioSource SourcePumpRun;
    internal static AudioSource SourcePumpStop;

    internal static AudioClip ClipPumpStart;
    internal static AudioClip ClipPumpRun;
    internal static AudioClip ClipPumpStop;

    // Crew transfer vars
    internal static AudioSource SourceCrewStart;
    internal static AudioSource SourceCrewRun;
    internal static AudioSource SourceCrewStop;

    internal static AudioClip ClipCrewStart;
    internal static AudioClip ClipCrewRun;
    internal static AudioClip ClipCrewStop;
    #endregion Static vars...

    internal static void LoadSounds()
    {
      try
      {
        GameObject go = new GameObject("Audio");
        SourceCrewStart = go.AddComponent<AudioSource>();
        SourceCrewRun = go.AddComponent<AudioSource>();
        SourceCrewStop = go.AddComponent<AudioSource>();

        if (GameDatabase.Instance.ExistsAudioClip(AudioSourceCrewPathStart) && 
            GameDatabase.Instance.ExistsAudioClip(AudioSourceCrewPathRun) &&
            GameDatabase.Instance.ExistsAudioClip(AudioSourceCrewPathStop))
        {
          ClipCrewStart = GameDatabase.Instance.GetAudioClip(AudioSourceCrewPathStart);
          ClipCrewRun = GameDatabase.Instance.GetAudioClip(AudioSourceCrewPathRun);
          ClipCrewStop = GameDatabase.Instance.GetAudioClip(AudioSourceCrewPathStop);

          // configure sources
          SourceCrewStart.clip = ClipCrewStart; // Start sound
          SourceCrewStart.volume = (float)SMSettings.CrewSoundVol;
          SourceCrewStart.pitch = 1f;

          SourceCrewRun.clip = ClipCrewRun; // Run sound
          SourceCrewRun.loop = true;
          SourceCrewRun.volume = (float)SMSettings.CrewSoundVol;
          SourceCrewRun.pitch = 1f;

          SourceCrewStop.clip = ClipCrewStop; // Stop Sound
          SourceCrewStop.volume = (float)SMSettings.CrewSoundVol;
          SourceCrewStop.pitch = 1f;
        }

        // Now do Pump sounds
        SourcePumpStart = go.AddComponent<AudioSource>();
        SourcePumpRun = go.AddComponent<AudioSource>();
        SourcePumpStop = go.AddComponent<AudioSource>();

        if (GameDatabase.Instance.ExistsAudioClip(SourcePumpPathStart) && 
            GameDatabase.Instance.ExistsAudioClip(SourcePumpPathRun) &&
            GameDatabase.Instance.ExistsAudioClip(SourcePumpPathStop))
        {
          ClipPumpStart = GameDatabase.Instance.GetAudioClip(SourcePumpPathStart);
          ClipPumpRun = GameDatabase.Instance.GetAudioClip(SourcePumpPathRun);
          ClipPumpStop = GameDatabase.Instance.GetAudioClip(SourcePumpPathStop);

          // configure sources
          SourcePumpStart.clip = ClipPumpStart; // Start sound
          SourcePumpStart.volume = (float)SMSettings.PumpSoundVol;
          SourcePumpStart.pitch = 1f;

          SourcePumpRun.clip = ClipPumpRun; // Run sound
          SourcePumpRun.loop = true;
          SourcePumpRun.volume = (float)SMSettings.PumpSoundVol;
          SourcePumpRun.pitch = 1f;

          SourcePumpStop.clip = ClipPumpStop; // Stop Sound
          SourcePumpStop.volume = (float)SMSettings.PumpSoundVol;
          SourcePumpStop.pitch = 1f;
        }
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(
          string.Format(" in SMAddon.LoadSounds.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), Utilities.LogType.Error, true);
        // ReSharper disable once PossibleIntendedRethrow
        throw ex;
      }
    }
  }
}
