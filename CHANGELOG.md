Changelog for Ship Manifest
===========================================================

Major features are **bolded**, contributors are *emphasized*.

Version {VERSION} - Release {DATE} - KSP {KSPVERSION}
-------------------------------------------------

Version 6.0.2.0 - Release 30 Dec 2020 - KSP 1.11.0
-------------------------------------------------
 - New: recompiled for KSP 1.11
 - New: added chinese translation (thanks to Grassland-CN)
 - Fixed: Kerbal EVA when part has no IVA (fixes #7)
 - Changed: retargeted to DotNet 4.6.1

Version 6.0.1.0 - Release 14 Jun 2020 - KSP 1.9.1
-------------------------------------------------
 - New: Show Applicants in Roster Window and allow editing/hiring them
 - New: added space-suit selection to Edit Kerbal
 - New: added veteran checkbox to Edit Kerbal
 - New: added a way to detect and fix broken Kerbals in the roster window (fixes #2)
 - New: added german translation
 - Fixed: KIS inventory transfers correctly again when moving a single crew member (partially fixes #21)
 - Fixed: Refactored and fixed some of the parts highlighting. Target parts should be correctly highlighted again.
 - Fixed: several null reference exceptions

Version 6.0.0.2 - Release 04 Mar 2020 - KSP 1.9.1
-------------------------------------------------
 - New: recompiled for KSP 1.9.1
 - Changed: new maintainer, *Micha*.
 - Changed: repository layout (maintainers take note).
 - Changed: new distribution scripts based on msbuild (maintainers take note).

Version 6.0.0.1 - Release 24 Nov 2019 - KSP 1.8.x Edition
-------------------------------------------------
- New: Recompiled for KSP 1.8.x compatibility (using DotNet 4.5)
   - **NOTE:** This version is incompatible with versions of KSP before 1.8.

Version 5.2.1.0 - Release 17 Mar 2018 - KSP 1.4.1 Edition
-------------------------------------------------
 - New: Recompiled for KSP 1.4.x compatibility.
 - Fixed: Bug in Selected Target Mouseover.  CLS Target Crew Part was incorrect color. Git Issue #53
 - Fixed: Updated icon graphics to restore quality after Unity update in KSP. Increased image sizes to 128 x 128 px (Git Issue #52)
 - Fixed: Sometimes the Manifest window will not appear. Git Issue #49
 - Fixed: gain/loss of resource(s) in transfers. (thanks to Arivald Ha'gel for the PR #48!)
 - Fixed: RT antennas not appearing in the control window. PR #48
 - Fixed: Kopernicus Solar panel detection problem (Git Issue #47) PR #48
 - Fixed: Correct version file data to agree with release version. Git Issue #46

Version 5.2.0.0 - Release 17 Jul 2017 - Vessel to Vessel Edition
-------------------------------------------------
 - New: Added Docked vessel to vessel Crew transfers!
 - New: Added Docked Vessels Control panel.  View your docked vessels, Rename a vessel, and undock a vessel with a click of a button.
 - New: Added option to vessel to vessel Crew transfers to limit crew xfr list to tourists only.  Space tourism awaits!
 - New: Added ability to create Tourists from Roster Window.
 - New: Spanish localization added.  Thanks to Fitiales!
 - Misc: refactored vessel detection for vessel to vessel transfers, it was not behaving as exected.
 - Misc: A bit of refactoring on highlighting, as it was not behaving as expected
 - Misc: Increased base volume of crew movement sound files.

Version 5.1.4.3 - Release 01 Jun 2017 - KSP 1.3 Compatibility Edition
-------------------------------------------------
 - Fixed: Error in scrollviewer dimensions refactor.  reversed height and width.
 - Fixed: Minor boundary detection error with mouseover highlighting.  The visible button list window boundaries were not completely respected.

Version 5.1.4.2 - Release 31 May 2017 - KSP 1.3 Compatibility Edition
-------------------------------------------------
 - Fixed: compatibility with latest DeepFreeze.  Freeze action from Roster Window was failing.
 - Fixed: Some localization was not complete.  Added additional tags for text not caught in first pass.
 - Fixed: Highlighting of parts was occurring outside the boundaries of the scroll windows.
 - Misc:  Cleaned up text rendering in general.  Converted to consistent use of C# string interpolation across mod.
 - Misc:  Cleaned up some code style issues.  I've been wanting to do this for a while, so KSP 1.3 was a good time to do it.
          This affected the SMWrapper.cs class for modders, and may break your mod.
          If you use this or reflection to access SM, some methods had capitalization changes.
          Ex: SM... was replaced with Sm... get... was replaced with Get...  Nothing is gone, just "altered" :)

Version 5.1.4.1 - Release 26 May 2017 - KSP 1.3 Compatibility Edition
-------------------------------------------------
 - New:  Implemented Localization system.  Now it is possible to translate SM into other languages.
 - New:  Realism Settings Refactor.  Realism Mode is redefined and easier to use; settings are now more granular.
 - New:  - Added Radio switch for realism Categories:  Full, None, Default, Custom.
 - New:  - Added new setting to the Realism Tab "Realistic Control". This affects ship control window and uncontrolled resource transfers.
 - New:  - Added new setting to the Realism Tab "Enable Roster Modifications".  Affects Roster actions Create, Add, Edit, Remove, and Respawn.
 - New:  - Added new setting to Realism Tab.  "Realistic Transfers".  This affects Transfer features for crew, science and resources.
 - New:  Added several new tooltips in the Mod.  Cleaned/updated up several others.
 - New:  Refactored Vessel to Vessel transfers.  Now separates multiple vessels originating from the same launch.
 - Fixed:  Selecting a resource in the Manifest Window triggered an enumeration error.
 - Fixed:  Crew respawn was always allowed.  Should be disabled for Realism.  Roster actions are now impacted by Enable Roster Modifications setting.
 - Fixed:  Issue with multiple instances of SMAddon class loading when changing scenes.
 - Fixed:  Roster and Settings windows continued to be displayed even when Pause Menu or Hide UI is on.
 - Fixed:  Manifest and other windows disappeared in flight under certain conditions (when staging/part destruction occurs).
 - Fixed:  Antennas not properly working. Post fix RemoteTech support is not yet confirmed.
 - Fixed:  Part highlighting on mouseover of part selection button was broken.  Bug introduced when I refactored highlighting awhile ago.
 - Fixed:  Sound file changes were not taking immediate effect.  Scene change was required. Github Issue #25
 - Fixed:  Resource totals not preserved during Transfers.  Github Issue:  #36
 - Fixed:  Resource Transfers exhibited incorrect stop button behavior with Target to source transfers.
 - Fixed:  Tourists could go EVA, and should not.  Github Issue:  #37
 - Fixed:  ToolTips displayed even when SM interface is not.  Github Issue: #38
 - Fixed:  ToolTips not properly updating on Xfer button. Github Issue:  #39
 - Fixed:  Crew Xfers between full parts failing under certain circumstances.  Github Issue #40

Version 5.1.4.0 - Release 26 May 2017 - KSP 1.3 Compatibility Edition (1.2.9 Prerelease) Not for Official Distribution
-------------------------------------------------
 - New:  Implemented Localization system.  Now it is possible to translate SM into other languages.
 - New:  Realism Settings Refactor.  Realism Mode is redefined and easier to use; settings are now more granular.
 - New:  - Added Radio switch for realism Categories:  Full, None, Default, Custom.
 - New:  - Added new setting to the Realism Tab "Realistic Control". This affects ship control window and uncontrolled resource transfers.
 - New:  - Added new setting to the Realism Tab "Enable Roster Modifications".  Affects Roster actions Create, Add, Edit, Remove, and Respawn.
 - New:  - Added new setting to Realism Tab.  "Realistic Transfers".  This affects Transfer features for crew, science and resources.
 - New:  Added several new tooltips in the Mod.  Cleaned/updated up several others.
 - Fixed:  Selecting a resource in the Manifest Window triggered an enumeration error.
 - Fixed:  Crew respawn was always allowed.  Should be disabled for Realism.  Roster actions are now impacted by Enable Roster Modifications setting.
 - Fixed:  Issue with multiple instances of SMAddon class loading when changing scenes.
 - Fixed:  Roster and Settings windows continued to be displayed even when Pause Menu or Hide UI is on.
 - Fixed:  Manifest and other windows disappeared in flight under certain conditions (when staging/part destruction occurs).
 - Fixed:  Antennas not properly working. Post fix RemoteTech support is not yet confirmed.
 - Fixed:  Part highlighting on mouseover of part selection button was broken.  Bug introduced when I refactored highlighting awhile ago.
 - Fixed:  Sound file changes were not taking immediate effect.  Scene change was required. Github Issue #25
 - Fixed:  Resource totals not preserved during Transfers.  Github Issue:  #36
 - Fixed:  Resource Transfers exhibited incorrect stop button behavior with Target to source transfers.
 - Fixed:  Tourists could go EVA, and should not.  Github Issue:  #37
 - Fixed:  ToolTips displayed even when SM interface is not.  Github Issue: #38
 - Fixed:  ToolTips not properly updating on Xfer button. Github Issue:  #39
 - Fixed:  Crew Xfers between full parts failing under certain circumstances.  Github Issue #40

Version 5.1.3.3 - Release 29 Jan 2017 - KSP 1.2.2 Compatibility Edition
-------------------------------------------------
 - Fixed:  Object not found error in ModDockedVessels Get LaunchID.  now properly returns a 0 if the underlying object is null.  Github Issue: #34

Version 5.1.3.2 - Release 16 Jan 2017 - KSP 1.2.2 Compatibility Edition
-------------------------------------------------
 - New:  Refactored for KSP 1.2.2
 - Fixed:  Enumeration error when opening or closing more than one hatch at the same time.
 - Fixed:  Respawn Kerbal fails. Github issue # 35.
 - Fixed:  Opening/closing hatches via a part's tweakable doesn't properly update the transfer windows xfer/eva buttons when CLS spaces change.
 - Fixed:  Fill buttons do not have tooltips.  Can be confusing as to their behavior.
 - Fixed:  Part level fill buttons do not behave as expected by users. Should not be available in flight with realism on.
 - Fixed:  Roster and Settings Icons sometimes appear in flight scene.  Should only be in Space Center Scene.
 - Fixed:  Highlighting is disabled temporarily when hatches are opened and closed.
 - Fixed:  Resource selection in the Manifest window is behaving erratically. Resources are disappearing in the display when multiple selections are made.
 - Fixed:  Vessel to vessel transfers are failing with an NRE in ShipManifest.SMVessel.UpdateDockedVessels. http://forum.kerbalspaceprogram.com/index.php?/topic/56643-121-ship-manifest-crew-science-resources-v-5131-15-nov-16/&do=findComment&comment=2881063
 - Fixed:  Sometimes crew transfers do not work.  Github Issue:  # 34

Version 5.1.3.1 - Release 15 Nov 2016 - KSP 1.2.1 Compatibility Edition
-------------------------------------------------
- Fixed:  Create Kerbal fails. Github issue # 33.
- Fixed:  Rename Kerbal changes do not show up after change.
- Fixed:  Removed Mod Button from Settings Window.  Was there in error.

Version 5.1.3.0 - Release 14 Nov 2016 - KSP 1.2.1 Compatibility Edition
-------------------------------------------------
- New:  Refactored mod for KSP 1.2.x Compatibility
- New:  Added support for new events in Crew Transfer, allowing improved performance and customization of Full Part messages during Stock Crew Transfers.
- New:  Corrected supported versions in the Developer Notes and Installation Notes. (Git Issue #30)
- New:  Added support for switching "Allowing Unrestricted Crew Transfers" in CLS so that SM and CLS do not compete for control over Stock Transfers.
- New:  Added a setting in the Settings window to enable/disable overriding CLS CrewTransfer setting.
- Removed:  Mods Tab in Control Window.  Installed mods is now availabe from the KSP Debug window (Alt F12)
- Fixed: SM windows were not always closing on scene changes.
- Fixed: Resource Dumps from the Manifest window would cause any previously clicked dump to initiate  when another was clicked.

Version 5.1.2.2 - Release 21 Aug, 2016 - KSP 1.1.3 Optimization Edition.
-------------------------------------------------
- New:  Tweak of tooltips to make them more readable.  changed style and added border.
- New:  Refactored code to ensure explicit variable type assignments.
- New:  Additional refactoring for performance and improved garbage collection.
- New:  Added support for onCrewTransferPartListCreated.  This allows me to intercept the Stock Crew Transfer Dialog and alter the Available parts for Transfer when CLS is enabled or DeepFreeze is installed.
        The Stock transfer Part Selection now properly highlights available parts. I can also provide a custom Message for selectinga full or unreachable part.
- Fixed: Crew transfers were incorrectly playing Pumping sounds.
- Fixed: Corrected a logic error in Crew Transfers that caused crew swaps in parts that have a crew capacity greater than their internal seat count. Github Issue #29.
  SM now properly supports "Standing Room Only Transfers".

Version 5.1.2.1 - Release 24 Jul, 2016 - KSP 1.1.3 Optimization Edition.
-------------------------------------------------
- Fixed: Enumeration error on kerbal action in Roster Window.  Moved action to outside enumerator, so change to list does not throw error.
- Fixed: Button widths were incorrect in Manifes and Transfer window part selectors under certain realism and configuration settings.

Version 5.1.2.0 - Release 21 Jul, 2016 - KSP 1.1.3 Optimization Edition.
-------------------------------------------------
- New:  Added option to enable Crew Fills and Dumps Vessel Wide during Pre-Flight .  Off by default.  Works the same as Resource fill and dump.
- New:  Refactored Part level Crew Fill and Dumps.  Now shows up in the Transfer Window in Preflight when CrewPreflight setting is on, or anytime when Realism is off
- New:  Significant refactoring to improve overall performance.
- Fixed:  Revised erroneous tooltip messages for Renaming kerbals and enabling Profession changes.  These are now enabled by default and supported by the stock game.
- Fixed:  Now SM properly detects and notes changes in USI inflatable crewable modules

Version 5.1.1.2 - Release 12 Jul, 2016 - KSP 1.1.3 Compatibility Edition.
-------------------------------------------------
- New:  Added ability to initiate EVA from Crew Transfer Window in Realism mode when CLS prevents an internal Transfer.
- Fixed: Occasional nullref exceptions when loading a vessel in method UpdateDockedVessels.

Version 5.1.1.1 - Release 08 Jul, 2016 - KSP 1.1.3 Compatibility Edition.
-------------------------------------------------
- New:  Implemented Disabling of Stock Crew Transfer system using Realism setting "Enable Stock Crew Transfer". When set to off, Stock Crew transfer buttons no longer appear.

Version 5.1.1.0 - Release 07 Jul, 2016 - KSP 1.1.3 Compatibility Edition.
-------------------------------------------------
- Fixed:  NulRef errror with DeepFreeze installed and a frozen kerbal in RosterListViewer.
- Fixed:  (maybe) Window display issues during launch and stage separation, explosion of ship.
- New:  SM window can now be displayed in IVA and in Map mode.
- New:  Added logging to output.log.  this will make the output.log more useful for troubleshooting.  Captures all log entries, verbose or not.
- New:  Refactored Highlighting to clean up FPS issue. Now causes significantly less impact to frame rate.
- New:  Refactored Stock Crew Transfer Beahavior.  When override is on, changes to KSP 1.1.3 now allow capturing transfer before it occurs.

Version 5.1.0.0 - Release 14 May, 2016 - KSP 1.1.2 Compatibility Update
-------------------------------------------------
- New:  Updated mod to support KSP 1.1.2.
- New:  Updated screen maeeages to use new object model.

Version 5.0.9.1 - Release 14 Apr, 2016 - KSP 1.1 Compatibility Update ** PreRelease **
-------------------------------------------------
- New:  Modified screen message displays to account for channges to the object model.  Stock screen messages should now be removed when SM Overrides a Stock crew transfer.  wip.

Version 5.0.9.0 - Release 05 Apr, 2016 - KSP 1.1 Compatibility Update ** PreRelease **
-------------------------------------------------
- New:  Updated code to run on KSP 1.1
- New:  Modified screen message displays to account for changes to the object model.  SM screen messages are wip.

Version 5.0.1.0 - Release 14 Mar, 2016 - Bug fixes and APIs
-------------------------------------------------
- New:  Removed DFInterface.dll.  Added Reflection based Wrapper class source code for integration with DeepFreeze.
- New:  Removed SMInterface.dll.   Replaced by SMWrapper, which is also a reflection based wrapper for developer use with SM.
- Fixed:  Crew movement issues with DeepFreeze.
- Fixed:  Roster Window does not display correctly with DeepFreeze installed.
- Note:  The latest version of DeepFreeze (DF) is required if you use it with SM.
- Note:  EVA kerbals causing a null ref bug and duplicating kerbals.  This fix also requires the latest version of Deepfreeze (V0.20.4.0)

Version 5.0.0.1 - Release 07 Mar, 2016 - Massive Refactoring Edition. NEW! Realism Mode - Multiple simultaneous transfers & dumps.
-------------------------------------------------
- New:  Added Volume controls in the sound tab of the Settings Window.   They had long been in the settings file, but not in the UI. I don't know why...
- New:  Science Transfers:  Added ability to process unprocessed in science labs. Git Issue #14
- Fixed:  Windows disappear on settings save.
- Fixed:  Windows disappear on window resolution changes.
- Fixed:  Vessel Transfers were not visible.
- Fixed:  Vessel Transfers were not behaving properly.
- Fixed:  Transfer sounds continue playing after transfer complete.
- Fixed:  Science Tooltips (and others) scrolling off screen on long lists.  Git Issue #18

Version 5.0.0.0 - Release 22 Jan, 2016 - Massive Refactoring Edition. NEW! Realism Mode - Multiple simultaneous transfers & dumps.
-------------------------------------------------
- New:  Added ability queue transfers in realism mode.  you may now start and stop multiple transfers and or dumps simultaneously,
        with the Vessel, Docked Vessels, individual parts or a selected group of parts.  Fuel Depot anyone?
- New:  Added ability to dump resources in flight in realism mode.  Dump process follows flow rate rules.  Dumps be stopped.
        per forum discussions, this process is assumed to impart a zero thrust component upon the vessel.
- New:  Massive refactor and reorganization of code (nothing was left untouched).
        A tremendous amount of work for very little visible effect except maybe performance :). Sets the foundation for easier to manage/enhance code.
- New:  Added build package automation and distribution.
- New:  Removed need for DFInterface.dll.  Now using new reflection class method for soft dependency to DeepFreeze.
- Fixed:  In realism Mode, during Preflight, Fill and dump kerbals vessel wide was enabled.   Now disabled when Realism is on.
- Fixed:  Corrected nested control displays in settings.
- Fixed:  Corrected Errors with tooltip displays and tooltip settings.  Tooltips would show on certain windows when disabled in settings.
- Fixed:  Corrected staging error where SM cannot be displayed during launch.

Version 4.4.2.0 - Release 12 Nov, 2015 - KSP 1.0.5 Edition.
-------------------------------------------------
- New:  Native Kerbal Renaming and Profession Management!  The old hash hack is gone!
        KSP 1.0.5 now supports native kerbal profession management, so kerbal profession now saves to game save.
        Updated SM to use new trait attribute of the kerbal object.  Also supports old game saves.
        Cleans up old game save automatically, if profession management is ON in settings (now the default)
- New:  Added Crew Dump/Fill at part level in Transfer Window, when vessel is in a recoverable state and realism is off.
- New:  based on feedback, expanded science tooltips to be more useful.
- Fixed:  Correct a window position loading error on MAC machines.
- Fixed:  Correct issues and deeper integration with DeepFreeze.  (Thanks JPLRepo!)
- Fixed:  Tooltip display issues with screen boundary

Version 4.4.1.1 - Release 09 July, 2015 - Tooltips & Science Xfer Improvements.
-------------------------------------------------
- Fixed:  Correct a display error with science tooltip when an experiment result key is not found.  Now displays the default key's data.

Version 4.4.1.0 - Release 05 July, 2015 - Tooltips & Science Xfer Improvements.
-------------------------------------------------
- New:  Refactored and expanded Tooltips.  Changed background, positioning, anchor points, font styles & colors for better readability.  Added more tooltips to various windows and tabs.
- New:  Added Control Window Tooltip control to settings.  If control window Tooltips is off, all tab tooltip settings are disabled.
- New:  Added linkage of Control Window Tab Tooltip settings to the Control Window ToolTip control.  They now act as children.
- New:  Added Detail support to Experiments.  Added greater detail to science tooltips.  Cleaned up horizontal scroll behaviour and layout.
- New:  Added labels to button headers in Roster Window.
- New:  Added 2 additional Roster List Filters.  "Assigned" and "Frozen".
- New:  Added active window screen edge management.  No more positioning windows beyond the screen edge when moving.
- Fixed:  Control window close button (upper right) did not display tooltip.
- Fixed:  Some Roster window action buttons have incorrect text when in Space Center.

Version 4.4.0.3 - Release 01 July, 2015 - Docked Vessel Transfers Edition.
-------------------------------------------------
- New:  Science transfers now allow individual report transfers from a science container.  You can transfer all or any now. Added an Expand/collapse button for clean display.
- New:  Altered stock Transfer messaging system to show success messages near portraits.  Cleaner look.
- New:  General clean up of button displays to prevent overflowing of text.
- Fixed:  When Transferring crew, the user can switch to IVA, causing potential camera issues.  Switching to IVA is now prevented and a message is displayed near portraits.
- Fixed:  Saving Settings sometimes does not "stick"  When opening and closing settings without saving in Space Center, default values can overwrite saved values.
- Fixed:  Stock Crew transfer were not being handled correctly, and transfer fail message was always being shown.

Version 4.4.0.2 - Release 24 June, 2015 - Docked Vessel Transfers Edition.
-------------------------------------------------
- New:  Added StockCrewXferOverride flag to SMInterface
- New:  Added check for full DeepFreezer when Stock Transfer Initiated and Override is On.  Ignore event if Freezer is full, and allow DeepFreeze to handle it.

Version 4.4.0.1 - Release 23 June, 2015 - Docked Vessel Transfers Edition.
-------------------------------------------------
- Fixed:  When switching vessels while in MapView with Crew Selected and CLS installed and enabled, errors are generated in log during transition.
- Fixed:  With the releae of DeepFreeze 0.16, freeze and thaw commands from Roster Window no longer work and cause errors.

Version 4.4.0.0 - Release 17 June, 2015 - Docked Vessel Transfers Edition.
-------------------------------------------------
- New:  Added ability to transfer, dump/fill resources by Docked vessel.  Multi resource, Docked Vessel(s) <-> Docked vessel(s), Docked Vessel(s) <-> Part(s), and Part(s) <-> Part(s) transfers are now possible.  Huge flexibility.
- New:  Highlighting Refactoring.  Docked Vessel highlighting, on mouseover cleanup, and standardized mouseover highlighting model.
- New:  Opened up SM to allow operation in MapView while in flight.  All features work, and Toolbar button is displayed while in MapView during flight.

Version 4.3.1.0 - Release 15 June, 2015 - GUI Skins, DeepFreeze & Bugs Edition.
-------------------------------------------------
- New:  Tightened Integration with DeepFreeze by adding DF Interface component and simplifying Frozen Kerbal display and detection...
- New:  Added ability to Freeze/Thaw Kerbals in  DeepFreeze Container via Roster window.  Works only when a freezer is part of the active vessel and contains kerbals.
- New:  Added New GUI Skin: Unity Default.  Selectable in Settings Config Tab and takes effect immediately.
- New:  Updated Roster display to improve general layout and readability.
- New:  Added Mods Tab to Settings Window.  Displayes Installed Mods/Assemblies.
- Fixed:  Bug in settings.  When cancelling or saving changes in Space Center, Settings Icon does not revert on toolbar.
- Fixed:  Bug with KIS compatibility.  When transferring Kerbals with inventory, a race condition occurs with OnCrewTransferred Event handler and causes errors.
          Added switch in SMSetting.dat to allow disabling onCrewTransferred Event call if KIS still is causing issues.
- Fixed:  Bug in Multi Part transfers.  Transfers sometimes still hang.  Added check for maxAmount to Transfer, and a flag for transfer in progress to allow Stop button to remain visible until completion...

Version 4.3.0.2 - Release 08 June, 2015 - Crew, Interfaces, & Refactoring Edition.
-------------------------------------------------
- New:  Cleaned up highlighting when undocking events occur to turn off highlighting on vessel parts/vessels that become detatched...
- Fixed:  Bug in settings.  When disabling Crew in setting, if crew was selected, Highligting does not turn off.
- Fixed:  Bug in Settings.  When in Highlighting Tab, "Highlight only Source/Target parts" and "Enable CLS Highlighting" should act like radio buttons but do not.
- Fixed:  Under certain circumstances, Highlighting woud not be completely cleared when turned off If crew was selected and CLS was enabled.
- Fixed:  RemoteTech detection was failing when multiple copies of the RemoteTech.dll existed.

Version 4.3.0.1 - Release 06 June, 2015 - Crew, Interfaces, & Refactoring Edition.
-------------------------------------------------
- New:  Refactored Resource transfers to improve overall transfer speed, flow & "feel".  Lag was causing issues on larger vessels.
- New:  Refactored Vessel update methods to properly udate various part lists if vessel changes occur while SM windows are open (undocking, etc.).  Now various windows properly refresh.
- Fixed:  Bug in multi-part transfers that allowed continued transfers when a transfer is initiated and then you undock a vessel from a station.
- Fixed:  Bug in Crew Transfers that allowed continued transfers when a crew transfer is initiated and then you undock a vessel from a station.

Version 4.3.0.0 - Release 04 June, 2015 - Crew, Interfaces, & Refactoring Edition.
-------------------------------------------------
- New:  Refactored Crew transfers into separate class to improve visibility and state management.
- New:  Crew transfers (part to part & seat to seat) now show both kerbals involved as moving, when a kerbal swap occurs.
- New:  Added DeepFreeze mod support for handling/viewing frozen kerbals. No more xferring frozen kerbals, and Roster Window now shows frozen kerbals.
- New:  Added SMInterface.dll for other mods to detect Crew xfers in progress and act accordingly.
- New:  Add onCrewTransferred Event trigger to be consistent with Stock Crew Transfers and to support KIS inventory movement when crew transfers occur.
- New:  Added Kerbal Filter for Roster Window:  All, vessel, Available, Dead/Missing.  Vessel filter is omitted when in Space Center.
- New:  Refactoring - moved window vars from Settings into window level code.
- New:  Refactoring - Added InstalledMods static class to centralize mod assembly detection and soft dependencies.
- New:  Refactoring - Altered Settings Save to segregate Hidden settings for ease of identification by users.
- Fixed:  Bug in multi-part transfers that lock transfer in run state, with no progress.  Gave loops timeouts, and relaxed the resolution of the calculation to allow for rounding errors.
- Fixed:  Bug in Crew Transfer.  When transferring a crew member to a full part with realism off, the crew member does not swap and disappears...
- Fixed:  Bug in Crew Transfer with CLS installed.  First transfer works fine, subsequent xfers fail, and Transfer is stuck in moving...

Version 4.2.1.1 - Release 14 May, 2015 - Highlighting Updates Edition Bug Fix.
-------------------------------------------------
- Fixed:  In Settings, if CLS is not installed, or CLS is disabled, changing the Enable Highlighting setting causes some buttons below it to become disabled.

Version 4.2.1.0 - Release 13 May, 2015 - Highlighting Updates Edition.
-------------------------------------------------
- New:  Added mouseover part highlighting on Transfer Window part Selection buttons.
- New:  Revised mousover highlighting to use new edge highlighting methods introduced in KSP 0.90.  Improves visibility of highlighted parts.
- New:  Added configuration switch to enable/disable mouseover edge highlighting, if performance is affected or behavior is not desired.
- Fixed:  When using Mod Admin, SM generates and error, and SMSettings file is not created, as PluginData folder is deleted (compatibility issue).
- Fixed:  When in Preflight or Flight and Realism Off, Selecting a single fluid/gaseous resource causes Transfer Window display issues (Found during Wiki creation).  Bug introduced in 4.2.0.0
- Fixed:  When performing a Crew Transfer in SM with Realism on, it is possible to perform a stock transfer during the Crew transfer process if Override is off, and potentially create a ghost kerbal.
- Fixed:  When removing/adding crew to a vessel in pre-flight, vessel "remembers" professions available when scene loads.  A scene change causes correct professions to be initialized. Possible exploit.

Version 4.2.0.2 - Release 05 May, 2015 - Transfers Expansion Edition bug fixes.
-------------------------------------------------
- Fixed:  Science Transfer broken.  Bug introduced with version 4.2.0.0

Version 4.2.0.1 - Release 04 May, 2015 - Transfers Expansion Edtion bug fixes.
-------------------------------------------------
- Fixed:  When realism is off and override Stock crew Xfers is on, transfers cause a flickering portrait and do not complete.
- Fixed:  Gender is correctly displayed, but changing a Kerbal's Gender results in the opposite gender being saved.

Version 4.2.0.0 - Release 03 May, 2015 - Transfers Expansion Edition. Multi-Resource & Multi-Part Xfers.
-------------------------------------------------
- New: You can now "link" 2 resources together simply by clicking on a Second Resource.
- New: You can now link multiple parts in the Transfer window, and move resources from 1:N, N:N and N:1 parts.
- New: Added Kerbal Gender Management in Roster Window.
- New: Added Revert profession renaming feature to Roster for removing the ascii "1"s from game save.  For mod compatibility.
- New: Changed config file from xml to json style.  No more spamming the KSP debug log.
- New: Cleaned up science transfers.  Target details now only shows container modules. No more transfer to an experiment module.
- Fixed:  When  near debris, SM window sometimes fails to display when icon is clicked from either toolbar.
- Fixed:  With CLS enabled, selected target part text displayed in Target Crew Color instead of Target Part color.
- Fixed:  Opening/closing a hatch from the hatch control tab fails to update the CLS spaces.
- Fixed:  When transferring science, Realism mode prevents moving science to a container in the same part.
- Fixed:  Disabling Resources in Settings does not remove Resources from the selection list in the Manifest Window.
- Fixed:  Portraits not properly updating after a crew move.  Bug introduced in 4.1.3 after revisions to actual crew move timing.

Version 4.1.4.4 - Release 10 Apr, 2015 - Bug fixes.
-------------------------------------------------
- Fixed:  Crew transfers fail when Realism Mode is Off.
- Fixed:  SM windows do not hide when the F2 key is toggled to hide UI.
- Fixed:  SM window positions are not automatically saved between scenes.
- Fixed:  Roster windows position incorrectly saving to settings window position.
- Changed:  Altered Window Reposition behaviour to be more intuitive.
- - - - - - Was:  Reset window to 0.0 when position exceeds the edge of the screen.
- - - - - - Now:  reposition window to edge of screen when position exceeds the edge of screen.

Version 4.1.4.3 - Release 06 Apr, 2015 - RT bug, External crew bug and control display fixes.
-------------------------------------------------
- Fixed: When using RemoteTech, not all RemoteTech antennas would display in Control window list.
- Fixed: Sometimes when displaying part info in Antennas, Solar Panels, hatches and Lights, a null exception would occur and "unknown" would be displayed in part parent info.
- Fixed: Crew in external seats were not properly handled in SM. Attempts to transfer will generate unhanded errors, and could possibly corrupt the game save, requiring the vessel to be deleted.  Removed Crew members in external seats from xfer list.

Version 4.1.4.2 - Release 29 Mar, 2015 - Control Window Tweaks Edition.
-------------------------------------------------
- New: Added part name to description for Antennas, Solar Panels, and Lights in Control Window.
- Fixed: If CLS is not installed, or CLS is disabled, Control Button is grayed out and Manifest Window is stuck in one position on screen.

Version 4.1.4.1 - Release 22 Mar, 2015 - RT Antenna Integration Edition.
-------------------------------------------------
 - New:  Added Remote Tech (RT) Antenna control support.
 - Fixed:  Undeployable Solar panels incorrectly show up in Solar panel list and generate an unmanaged error when Extended or Retracted.

Version 0.90.0_4.1.4.0 - Release 21 Mar, 2015 - More Control & Realism Tweaks Edition.
-------------------------------------------------
 - New:  Added Antenna control support.
 - New:  Added Light control support.
 - New:  Reworked Hatches, Solar Panels, Antennas and Lights into a single management window called Control
 - New:  Reworked Settings Window to behave the same as the Control Window.  Tabbed sections for less scrolling :)
 - New:  Added Electrical cost for Resource Xfers in realism mode, based on Actual Flow Rate, and Cost per unit setting in config.
 - New:  Added Settings switch for Resource Xfer Electricl Cost in Realism.   Turn off if you don't wan't THAT much realism :)
 - New:  Added support for vessel control state in realism mode.  Resource transfers not possible when controlable = false.

Version 0.90.0_4.1.3.1 - Release 18 Mar, 2015 - Better Behaviours Edition.
-------------------------------------------------
 - New:  Revised SM Crew transfer display to show "Moving" in place of the Xfer button for the kerbal being moved when the crew transfer is in progress.   Helps with Xfer process visibility.
 - Fixed:  Exceptions reported by SMAddon.CanShowShipManifest method when loading directly into a vessel on the pad from KSC.
 - Fixed:  Frame rate slow down issues reported when planting a flag, coming near debris in flight mode.  Issue was introduced in version 4.1.0

Version 0.90.0_4.1.3 - Release 12 Mar, 2015 - Better Behaviours Edition.
-------------------------------------------------
 - New:  Revised SM original Crew transfers to delay the actual transfer of a kerbal to the end of the wait period.
 - New:  Added support SM style Crew transfers when using the stock Crew transfer.  You now get crew movement sounds, and the same delay for crewmember transfers when in realism mode. Can be disabled.
 - New:  Better support for multiple resource transfers.  If both previously selected source and target parts contain a newly selected resource, parts will now remain selected.
 - New:  Revised resource movement duration algorithm.  Now using a flow rate, based on source part capacity and max flow time setting.  Now max time is based on pumping (pushing) a full tank.
 - New:  More Refactoring.  Removed redundant part lists (SelectedResourceParts), refactored & renamed FindKerbalpart method.  Removed TextureReplacer event option (not aware of anyone needing this).
 - Fixed:  When in KSC & Roster Window, if you respawned a Kerbal (bring them back to life), Exception Detector would report errors from some other mods)

Version 0.90.0_4.1.2 - Release 24 Feb, 2015 - Settings and Windows clean up Edition.
-------------------------------------------------
 - New:  Added support for deleting config.xml file.  Now properly reconstructs config.xml file from default settings.
 - New:  Added window boundary checker to ensure that windows cannot be opened beyond the screen.  (handles changes from higher to lower screen resolutions.)
 - Bug:  Config.xml file distributed with non default window positions.   This could cause some users not to see the windows when opened.
 - Fixed:  AVS version file out of date.  Updated local copy as well as server copy.
 - Fixed:  When hot switching from stock to Blizzy toolbars, a scene change would still allow the stock button to appear in some scenes.
 - Fixed:  When CLS is not installed, method GetCLSVessel generates an error in flight.

Version 0.90.0_4.1.1 - Release 23 Feb, 2015 - Settings & Roster exposed Edition.
-------------------------------------------------
 - New:  Expose the Roster window via toolbars in KSC.  Now you can get to the Kerbalnaut Roster from the Space Center!
 - New:  Expose Settings window in KSC via toolbars.  Now you can get to SM Settings from the Space Center!
 - New:  Refactored Window management with Toolbars.  Streamlined code and addressed some behavioral issues (2 clicks to close a window after switching toolbars from blizzy to stock, etc.)
 - Fixed:  When opening or closing a hatch (CLS installed), sometimes the hatch color change would not properly update when ship is currently highlighted.

Version 0.90.0_4.1.0b - Release 19 Feb, 2015 - Solar Panels, Kerbal Renaming, Bugs, Mod Refactoring and More Edition.
-------------------------------------------------
 - Fixed:  In the Roster Window, when editing an existing kerbal with Rename and Rename with Professions on in Settings, You cannot change the profession. You SHOULD be able to.

Version 0.90.0_4.1.0a - Release 18 Feb, 2015 - Solar Panels, Kerbal Renaming, Bugs, Mod Refactoring and More Edition.
-------------------------------------------------
 - Fixed:  under certain conditions, GetCLSVessel generates a Space out of range error.  Troubleshooting code was left in by mistake.

Version 0.90.0_4.1.0 - Release 18 Feb, 2015 - Solar Panels, Kerbal Renaming, Bugs, Mod Refactoring and More Edition.
-------------------------------------------------
 - New:  Added Deployable Solar Panel Management Window.  Works like Hatch panel. In realism Mode, respects Rectractable = false.
 - New:  Refactored Crew Transfer Display code, Hatch Panel code and data management.  Relocated some methods to more logical locations.
 - New:  Added switch to allow renaming Kerbals. Added support for maintaining professions after rename.  Adds non printing chars to name, so use at your own risk.
 - New:  Added autosave of settings on scene change or exiting game.
 - Fixed:  After switching scenes and selecting a part with CLS enabled an highlighting error occurs.
 - Fixed:  after switching scenes and using Hatch panel, an error occurs in highlighting.
 - Fixed:  After switching vessels, the hatch panel shows the old vessel info and is not updated. Additionally, CLS highlighting causes errors.

Version 0.90.0_4.0.2 - Release 13 Feb, 2015 - Bugs, Mod Refactoring and More Edition.
-------------------------------------------------
 - New:  Resource Transfer display and setup system refactored.  Added ability to stop a transfer in progress.
 - New:  Exposed Resource Transfer Flow Rate Slider min and max values. You can now change the min and max flow rate.
 - New:  Added a maximum run time in seconds. SM will use the lesser duration of Xfer amount / flow rate or max time.
 - New:  Added tool tips to  controls in the options section of the Settings Window.
 - Fixed:  When moving or transferring a kerbal, closing the transfer window, Manifest window or closing the manifest window from any toolbar while the action is in progress causes an error.
 - Fixed:  When closing the Transfer Window, internally resetting the selected resource causes an error.

Version 0.90.0_4.0.1 - Release 08 Feb, 2015 - CLS Hatches, CLS Highlighting, Mod Refactoring and More Edition.
-------------------------------------------------
 - New:  4.0.1 - Added Create specific Kerbal Type:  Now you can choose Pilot, Engineer or Scientist!
 - NEW:  4.0.1 - Bug fix for unneeded debug log entry from tooltips
 - New:  4.0.1 - Bug Fix for Non reset Window positions in Config.xml

Version 0.90.0_4.0.0 - Release 06 Feb, 2015 - CLS Hatches, CLS Highlighting, Mod Refactoring and More Edition.
-------------------------------------------------
 - New:  Added support for CLS Hatches.  Now features a hatch dashboard.  Mouseover a hatch and it is highlighted on the vessel.  Takes advantage of changes made to CLS 1.1.1.0 to support third party Hatch control.  Open/close individual/all hatches in a vessel.
 - New:  Revised highlighting to reintroduce CLS enabled highlighting.  Added a switch to turn on/off CLS highlighting.  Takes advantage of recent changes made to CLS highlighting.
 - New:  Many internal architectural changes and refactoring of plugin. Changed windows into separate classes. Prepare for a base window class. Added tooltip support.
 - New:  Added tool tips for many buttons across entire plugin.  Added ToolTip display switch in settings for those that don't want Tool tips to show, including granularity to the window level.
 - New:  Major Roster Window enhancements.  Altered action button dislay behaviors to be more clear.  Added conditional tooltips to aid in use of little known features. Added Kerbal Title and Status to Kerbal info display.
 - New:  Enabling/Disabling Blizzy Toolbar no longer requires a game restart.  Turn it on or off in settings and it will take effect immediately.
 - New:  Added SETI compatibility support for dataIsCollectable = false in Realism Mode.   Now you cannot Xfer science data where dataIsCollectable = false for the source module.
 - New:  Added vessel resource totals (quick reference) to Manifest Window resource buttons and Transfer Window Resource Title.
 - New:  Added KSP-AVC support http://forum.kerbalspaceprogram.com/threads/79745
 - Fixed:  When when transferring a resource in realism mode, sometimes the source part ends up negative and causes issues (moves resources 1 unit at a time, backwards.

Version 0.90.0_3.3.4 - 15 Jan, 2015 - Bugs, Mod Tweaks and More Edition.
-------------------------------------------------
 - New:  Added a Limited Highlighting switch.  When on, highlights only source and target parts.
         Highlighting switch must be enabled to use.
 - New:  Added close buttons to upper right of most windows.  Cleaned up App launcher toggle button behavior, and synced with close buttons.
 - New:  Added detection for IVA. Hide Ship Manifest Window when in IVA.
 - New:  CLS highlightng returns.  Previous method replaced with new model. Livable parts only will be highlighted by SM.
         To view passable parts, select the space from the CLS plugin menu.
 - Fixed:  Due to KSP 0.90.0 changes, when using Roster, changes to Kerbal names causes the role to change (bad).
         - Removed ability to edit name of existing Kerbals.

Version 0.90.0_3.3.3 - 19 Dec, 2014 - 0.90 compatibility Edition.
-------------------------------------------------
 - Update to correct highlighting errors due to KSP 0.90 changes.
 - Added a Highlighting switch.  If you don't want highlighting at all, turn it off!
 - Other under the hood changes to improve overall highlighting behavior.  Works very nice now.

Version 0.25.0_3.3.2b - Interim Release with bug fixes and removal of CLS Highlighting
-------------------------------------------------
 - Fixed:  When changing vessels while in map mode, the toolbar button disappears from the stock toolbar, never to return.

Version 0.25.0_3.3.2a - Interim Release with bug fixes and removal of CLS Highlighting
-------------------------------------------------
 - New:  Added Resource Fill button when Realism is off.
 - New:  Added Part Fill and Dump of a resource when Realism is off.
 - Improvement:  error handling on frame based errors are now trapped for first occurance.  Subsequent errors will not produce a log entry.
 - Fixed:  When changing vessesls with the manifest window open, ClearResourceHighlighting causes an error.
         - http://forum.kerbalspaceprogram.com/threads/62270?p=1481125&viewfull=1#post1481125

Version 0.24.2_3.3.2 - 28 Sep, 2014 - 0.24.2 and bug fixes edition
-------------------------------------------------
 - New:  Ship manifest is now dependency free.  You don't need other mods to use Ship Manifest.
 - New:  Blizzy Toolbar is now optional.  If you install it, you can enable it.  Off by default.
 - New:  Removed auto enable of CLS.  CLS is now Off by default.  If CLS is installed, can be turned on in Settings.
 - New:  Bug fixes to correct crashing and errors on startup.
 - New:  Added Close button to Debug window.
 - New:  Revised Science transfer code to ensure compatibility with DMagic Parts (i hope).
 - Other Undocumented changes.  I was in the middle of other updates (bug fixes) when 0.24.2 hit.

Version 0.24.0.3.3.1a - 28 Aug, 2014 - 0.24.0 Compliance Edition.
-------------------------------------------------
 - New:  Removed toolbar from distribution to comply with forum rules.  No other changes.

Version 0.24.0.3.3.1 - 17 Jul, 2014 - 0.24.0 Edition.
-------------------------------------------------
 - New:  Now compatible with KSP 0.24.0. Squad reworked crew objects and namespace.
 - New:  Roster Window now shows vessel to which a kerbal is assigned.
 - New:  Add support for DMagic Science parts (IDataScienceContainer)
 - Fixed:  SM Still doubling LS resource amounts.
         - http://forum.kerbalspaceprogram.com/threads/62270-0-23-5-Ship-Manifest-(Crew-Science-Resources)-v0-23-5-3-2-2-2-May-14?p=1136419&viewfull=1#post1136419
 - Known Issue:  SM & CLS Highlighting still problematic.

Version 0.23.5.3.3 - 29 May, 2014 - CLS is Optonal Edition.
-------------------------------------------------
 - New:  CLS is now a soft dependency.  if you install it, SM will configure for it's use.  If you do not install it, SM will automatically detect that and set Enable CLS Off.

Version 0.23.5.3.2.3 - 11 May, 2014 - Settings, Roster & Bug Fix Edition.
-------------------------------------------------
 - New:  Roster Window now allows adding and removing individual Kerbals during pre-flight in Realism Mode, and Anytime in Standard mode.
 - New:  Additional changes to Settings.
         - Now sepearate sections make finding things easier.
         - Changed LockRealismMode to LockSettings, as Realism Mode is not a parent setting.
		 - Added Locksettings to the Settings Window.  Once set, must be turned off in config file.
		 - Made Enable CLS a child of Enable Crew
 - Fixed:  SM not detecting Changes in CrewCapacity with Inflatable Parts...
         - http://forum.kerbalspaceprogram.com/threads/62270-0-23-5-Ship-Manifest-(Crew-Science-Resources)-v0-23-5-3-2-1-22-Apr-14?p=1118517&viewfull=1#post1118517
 - Fixed:  SM not Erroring when attempting to transfer to a pert with no internal model
		 = http://forum.kerbalspaceprogram.com/threads/62270-0-23-5-Ship-Manifest-(Crew-Science-Resources)-v0-23-5-3-2-2-2-May-14?p=1140559&viewfull=1#post1140559
 - Fixed:  SM still allowing negative numbers in resource transfers.
         - http://forum.kerbalspaceprogram.com/threads/62270-0-23-5-Ship-Manifest-(Crew-Science-Resources)-v0-23-5-3-2-2-2-May-14?p=1136419&viewfull=1#post1136419

Version 0.23.5.3.2.2a - 9 May, 2014 - CLS Hot Fix Edition..
-------------------------------------------------
 - New:  Recompiled with a new assembly reference to CLS 1.0.4.1. No other changes

Version 0.23.5.3.2.2 - 2 May, 2014 - Realism Settings Love Edition..
-------------------------------------------------
 - New:  Changes to Settings Window to add previously hidden settings and tie them to the realism mode setting.
 - New:  Crew Transfers should allow swapping Kerbals between parts.  Swaps are possible within a part, but not between parts.
         - www.youtube.com/watch?v=I_TNxjnW234
 - New:  Added config file switch for Enable/Disable TextureReplacer eva event triggers.  testing shows it conflicts with TACLS.
 - Bug:  Highlighting behaves abnormally on resource changes from crew to another resource.  Residual selected part highlighting...
         - Internally found, Post release of 0.23.5.3.2.1
 - Fixed:  Highlighting behaves abnormally on part changes when moving away from source and target part being the same...
         - Internally found, Post release of 0.23.5.3.2.1a

Version 0.23.5.3.2.1b - 26 Apr, 2014 - TACLS test Edition. Github release (Not on Spaceport)
-------------------------------------------------
 - Fixed:  SM not detecting Changes in Creability with Inflatable Parts...
         - http://forum.kerbalspaceprogram.com/threads/62270-0-23-5-Ship-Manifest-%28Crew-Science-Resources%29-v0-23-5-3-2-1-22-Apr-14?p=1118517&viewfull=1#post1118517

Version 0.23.5.3.2.1a - 25 Apr, 2014 - TACLS test Edition. Github release (Not on Spaceport)
-------------------------------------------------
 - New:  Crew Transfers should allow swapping Kerbals between parts.  Swaps are possible within a part, but not between parts.
         - www.youtube.com/watch?v=I_TNxjnW234
 - New:  Added config file switch for Enable/Disable TextureReplacer eva event triggers.  testing show it conflicts with TACLS.
 - Fixed:  Highlighting behaves abnormally on resource changes from crew to another resource.  Residual selected part highlighting...
         - Internally found, Post relese of 0.23.5.3.2.1
 - Fixed:  Highlighting behaves abnormally on part changes when moving away from source and target part being the same...
         - Internally found, Post relese of 0.23.5.3.2.1a


Version 0.23.5.3.2.1 - 22 Apr, 2014 - Bug fixes Edition.
-------------------------------------------------
 - New:  Resource/Part Button Improvements:
         - http://forum.kerbalspaceprogram.com/threads/62270-0-23-5-Ship-Manifest-%28Manage-Crew-Science-Resources%29-v0-23-5-3-2-16-Apr-14?p=1058979&viewfull=1#post1058979
		 - General clean up of contrast / formatting for better readability in hover, unselected and selected modes.
 - New:  Add Auto Popup of Debug console on Error.  Configurable, Off by Default.
 - Fixed:  Highlighting fails for source Part when selecting any resource other than crew.
         - Internally found, Post relese of 0.23.5.3.2
 - Fixed:  Resource Pump transfers fail wen xferring small amounts.
         - http://forum.kerbalspaceprogram.com/threads/62270-0-23-5-Ship-Manifest-%28Manage-Crew-Science-Resources%29-v0-23-5-3-2-16-Apr-14?p=950355&viewfull=1#post950355
 - Fixed:  Tanks sometimes go negative.
         - http://forum.kerbalspaceprogram.com/threads/62270-0-23-5-Ship-Manifest-%28Manage-Crew-Science-Resources%29-v0-23-5-3-2-16-Apr-14?p=1082740&viewfull=1#post1082740
 - Fixed:  Crew event triggers not firing in Preflight.
         - http://forum.kerbalspaceprogram.com/threads/62270-0-23-5-Ship-Manifest-%28Manage-Crew-Science-Resources%29-v0-23-5-3-2-16-Apr-14?p=1100162&viewfull=1#post1100162
 - Fixed:  Resource Transfer noises too low.
         - www.youtube.com/watch?v=I_TNxjnW234
 - Fixed: Crew event triggers causing duplicate life support resources in TAC Life Support.
        - http://forum.kerbalspaceprogram.com/...=1#post1108035

Version 0.23.5.3.2 - 16 Apr, 2014 - Add ConnectedLivingSpace Integration.
-------------------------------------------------
 - Realism Mode:  Crew Xfers & Moves are now "space" aware.  if the target part is not connected via an internal passageway, then the xfer or move cannot occur.
 - CLS awareness can be turned off in the config file, for those that want to be able to xfer across living spaces.
 - General code cleanup and significant reorganization to use fewer frame and memory resources.
   - InvalidOp bug:  http://forum.kerbalspaceprogram.com/threads/62270-0-23-5-Ship-Manifest-%28Manage-Crew-Science-Resources%29-v0-23-5-3-2-16-Apr-14?p=1032850&viewfull=1#post1032850
   -
 - Include Toolbar 1.7.1 redistribution
 - Connected Spaces illuminate when you select crew.
 - Source part color is still Red by default, but the target color is set to Blue by default for Crew Only. can be configured.
 - Connected Living Space Aware. Configurable switch to enable. When enabled, crew transfers can only occur within the same Living Space.
 - Event trigger Support for TextureReplacer and Extraplanetary LaunchPads. Event throws have been added to ensure proper updating of other mods.
   - ExtraPlanetary LaunchPads:  http://forum.kerbalspaceprogram.com/threads/62270-0-23-5-Ship-Manifest-%28Manage-Crew-Science-Resources%29-v0-23-5-3-2-16-Apr-14?p=1051553&viewfull=1#post1051553
   - Texture Replacer:  http://forum.kerbalspaceprogram.com/threads/62270-0-23-5-Ship-Manifest-%28Manage-Crew-Science-Resources%29-v0-23-5-3-2-16-Apr-14?p=950542&viewfull=1#post950542
                        http://forum.kerbalspaceprogram.com/threads/62270-0-23-5-Ship-Manifest-%28Manage-Crew-Science-Resources%29-v0-23-5-3-2-16-Apr-14?p=1052405&viewfull=1#post1052405
 - Dependencies on CLS and Toolbar. Be sure that is understood. I will look at optional dependencies in the future, but for now, it is what it is. Besides, they are great plugins.

Version 0.23.3.1.5 - 26 Feb, 2014 - Add Realism to Crew Transfer.
-------------------------------------------------
 - Realism Mode:  Crew Xfers & Moves now occur in "real" time.  We now have Sounds of crewman moving.  Portrait updates now occur when Kerbals get in thier new seat.
	- Added Crew sound locations to settings Window.
	- Added Config for Crew Transfer Duration from part to part.  This is a placeholder for later development.
	- Added default xfer duration of 2 sec for Seat to Seat Xfers within the same Part.
 - Resource Xfers enhancements.  Added textbox for entering Exact Xfer Amounts.  Integers can be entered, and fractions can be pasted into box for now...
 - Rearranged resource xfer details to make it more intuitive after adding text box.
 - Include Toolbar 1.6.0 redistribution

Version 0.23.3.1.4 - 15 Feb, 2014 - General cleanup, configuration & UI enhancements.
-------------------------------------------------
 - Realism Mode:  Science Xfers now render experiments inoperable after xfer.
 - Changed Science Xfers Target Module selection to Auto select, If the target Part has only 1 target module. Saves a click the majority of the time.
 - Added Save Debug Log support.  Save Log button now works.
 - Added support for tailoring Ship Manifest for your needs:
	- Config File switch to enable/disable Fill & Empty Resource buttons in PreFlight.  Enabled by default.
	- Config File switch to enable/disable Crew Transfer Feature.  Enabled by default.
	- Config File switch to enable/disable Science Transfer Feature.  Enabled by default.
- Include Toolbar 1.5.3 redistribution

Version 0.23.3.1.3 - 12 Feb, 2014 - Add Preflight features and fix preflight fill resources bug.
-------------------------------------------------
 - Add Crew Fill and Crew Empty Buttons to PreFlight.   Now you can fill or empty your vessel on the pad.
 - Reworked Resource Fill Vessel and Empty Vessel to respect Realism mode. (resourceTransferMode = NONE).
 - Added Dump Resource buttons to Resource List in Manifest Window.  Now you can dump single resource on the entire vessel.
   Realism Mode support:  Preflight, you can dump resources.  In Flight, you cannot.
 - Added mod version to Debug Window.

Version 0.23.3.1.2 - 11 Feb, 2014 - Add Bi-Directional Resource Xfers and Science Xfer Bug fix.
-------------------------------------------------
 - Bi-Directional Resource transfers.   Now you can move a resource from source to target or target to source.
 - ** Science bug fix.   Sorry about that everyone.   It now works. :D
 - Improved verbose logging of science, crew, and resource xfers.
 - Include Toolbar 1.5.2 redistribution

Version 0.23.3.1.1 - 4 Feb, 2014 - Add Seat to Seat Crew Transfers
-------------------------------------------------
 - Added Seat to Seat Transfers. On source or target kerbal, click ">>".  This will move the kerbal to the next seat in the internal indexed list.  if a kerbal is already in that seat, they will swap!
 - Include Toolbar 1.4.5 redistribution

Version 0.23.3.1 - 3 Feb, 2014 - Add Science Xfers and Bug fixes
-------------------------------------------------
 - Added Science Transfers. Select source part & module, select target part & module, click xfer.
 - Add config switch for Resource.TransferMode = None on resources in realism mode.
 - Add config switch for locking Realism Mode.
   1 = locked in the mode set by RealismMode in config.  Displays Realism Mode in Settings Window.
   0 = unlocked.  Normal Radio button shows in Settings Window, and Setting can bee changed.
 - Bug fix. Xfering a crew member frrom target to source would violate the capacity limitation of the part and cause lost kerbals.
 - Bug fix. xfering crew would sometimes result in no portrait, or missing portrait.  Revised Kerbal spawm methodology.  Thanks to ATG, for helping me characterize the issue.

Version 0.23.3.0 - 27 Jan, 2014 - Add Crew Xfers and Redistribution of toolbar 1.4.4
-------------------------------------------------
 - Crew Transfers.  Uses the same interface as resources.   Crew is simply another resource on the ship.
 - Crew Roster.  Manage Crew Attributes. Create Crew members, "Kill" Crew members.
 - Sound file size reduction using .ogg.
 - Configurable source and target part highlighting (via config file for now...)  source is red and target is green by default now.
 - Bug fixes.  minor display errors.  Less than 100% fills were not working correctly.

Version 0.23.2.0 - 03 Jan, 2014 - Add Realism Mode and Redistribution of toolbar 1.4.0
-------------------------------------------------
- Add Realism Mode:  Sounds, Real time resource flow between parts.  Configurable sounds and flow rate.
- Add Setting Window to allow configuration of various features of Ship Manifest.
- Updated to Toolbar 1.4.0

Version 0.23.1.3.1 - 27 Dec 2013 - minor bug fix and Redistribution of toolbar 1.3.0
-------------------------------------------------
- bug fix:  When closing all windows from toolbar button, part event handlers were not released.  This caused the highlight to fail on mouse over of an affected part.
- Updated to Toolbar 1.3.0

Version 0.23.1.3 - 23 Dec 2013 - Bug Fix
-------------------------------------------------
- Post Bug Fix regression bug - Source part and target part highlighting failing.

Version 0.23.1.2 - 22 Dec 2013 - Bug Fix
-------------------------------------------------
- When selecting a resource, parts illuminate.   Close manifest window.  Parts stay illuminated.

Version 0.23.1.1 - 22 Dec 2013 - Revision
-------------------------------------------------
- Added Selected resource parts highlighting support.
- Added OnMouseExit handlers to affected parts. Handlers ensure proper highlighting is retained when active.
- Added new icons. Now Ship manifest has it own icons.
- Incorporated Toolbar 1.2.1 by Blizzy78
- Removed all remaining commented code related to Crew manifest.

Version 0.23.1.0 - 18 Dec 2013 - Revision
-------------------------------------------------
- Removed Crew Manifest.
- Improved GUI, resizing and cleaning up data.
- Cleaned up transfer method, improving accuracy.
- Incorporated Toolbar 1.2 by Blizzy78.

Version 0.23.0.1 - 17 Dec 2013 - Initial beta release
-------------------------------------------------

This work is based in large part on Crew manifest 0.5.6.0 by xxSovereignxx as modified by Sarbian to work with 0.22.
