using System;
using ShipManifest.InternalObjects.Settings;
using UnityEngine;

namespace ShipManifest.InternalObjects
{
  // ReSharper disable once InconsistentNaming
  internal class SMSound
  {
    #region Static vars...
    // Default sound license: CC-By-SA
    // http://www.freesound.org/people/adcbicycle/sounds/14214/
    internal static string AudioSourceCrewPathStart = CurrSettings.CrewSoundStart ?? "ShipManifest/Sounds/14214-1";
    internal static string AudioSourceCrewPathRun = CurrSettings.CrewSoundRun ?? "ShipManifest/Sounds/14214-2";
    internal static string AudioSourceCrewPathStop = CurrSettings.CrewSoundStop ?? "ShipManifest/Sounds/14214-3";

    // Default sound license: CC-By-SA
    // http://www.freesound.org/people/vibe_crc/sounds/59328/
    internal static string SourcePumpPathStart = CurrSettings.PumpSoundStart ?? "ShipManifest/Sounds/59328-1";
    internal static string SourcePumpPathRun = CurrSettings.PumpSoundRun ?? "ShipManifest/Sounds/59328-2";
    internal static string SourcePumpPathStop = CurrSettings.PumpSoundStop ?? "ShipManifest/Sounds/59328-3";

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
          SourceCrewStart.volume = (float)CurrSettings.CrewSoundVol;
          SourceCrewStart.pitch = 1f;

          SourceCrewRun.clip = ClipCrewRun; // Run sound
          SourceCrewRun.loop = true;
          SourceCrewRun.volume = (float)CurrSettings.CrewSoundVol;
          SourceCrewRun.pitch = 1f;

          SourceCrewStop.clip = ClipCrewStop; // Stop Sound
          SourceCrewStop.volume = (float)CurrSettings.CrewSoundVol;
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
          SourcePumpStart.volume = (float)CurrSettings.PumpSoundVol;
          SourcePumpStart.pitch = 1f;

          SourcePumpRun.clip = ClipPumpRun; // Run sound
          SourcePumpRun.loop = true;
          SourcePumpRun.volume = (float)CurrSettings.PumpSoundVol;
          SourcePumpRun.pitch = 1f;

          SourcePumpStop.clip = ClipPumpStop; // Stop Sound
          SourcePumpStop.volume = (float)CurrSettings.PumpSoundVol;
          SourcePumpStop.pitch = 1f;
        }
      }
      catch (Exception ex)
      {
        SmUtils.LogMessage(
          $" in SMAddon.LoadSounds.  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}", SmUtils.LogType.Error, true);
        // ReSharper disable once PossibleIntendedRethrow
        throw ex;
      }
    }

    internal static bool SoundSettingsChanged()
    {
      return CurrSettings.PumpSoundRun != OrigSettings.PumpSoundRun
             || CurrSettings.PumpSoundStart != OrigSettings.PumpSoundStart
             || CurrSettings.PumpSoundStop != OrigSettings.PumpSoundStop
             || CurrSettings.CrewSoundStart != OrigSettings.CrewSoundStart
             || CurrSettings.CrewSoundRun != OrigSettings.CrewSoundRun
             || CurrSettings.CrewSoundStop != OrigSettings.CrewSoundStop;
    }
  }
}
