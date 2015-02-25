ShipManifest
============
Version 0.90.0_4.1.2 - Release 24 Feb, 2015 - Settings and Windows clean up Edition.
 - New:  Added support for deleting config.xml file.  Now properly reconstructs config.xml file from default settings.
 - New:  Added window boundary checker to ensure that windows cannot be opened beyond the screen.  (handles changes from higher to lower screen resolutions.)
 - Bug:  Config.xml file distributed with non default window positions.   This could cause some users not to see the windows when opened.
 - Bug:  AVS version file out of date.  Updated local copy as well as server copy.
 - Bug:  When hot switching from stock to Blizzy toolbars, a scene change would still allow the stock button to appear in some scenes.
 - Bug:  When CLS is not installed, method GetCLSVessel generates an error in flight.

Version 0.90.0_4.1.1 - Release 23 Feb, 2015 - Settings & Roster exposed Edition.
 - New:  Expose the Roster window via toolbars in KSC.  Now you can get to the Kerbalnaut Roster from the Space Center!
 - New:  Expose Settings window in KSC via toolbars.  Now you can get to SM Settings from the Space Center!
 - New:  Refactored Window management with Toolbars.  Streamlined code and addressed some behavioral issues (2 clicks to close a window after switching toolbars from blizzy to stock, etc.)
 - Bug:  When opening or closing a hatch (CLS installed), sometimes the hatch color change would not properly update when ship is currently highlighted.

Version 0.90.0_4.1.0b - Release 19 Feb, 2015 - Solar Panels, Kerbal Renaming, Bugs, Mod Refactoring and More Edition.
 - Bug:  In the Roster Window, when editing an existing kerbal with Rename and Rename with Professions on in Settings, You cannot change the profession. You SHOULD be able to.

Version 0.90.0_4.1.0a - Release 18 Feb, 2015 - Solar Panels, Kerbal Renaming, Bugs, Mod Refactoring and More Edition.
 - Bug:  under certain conditions, GetCLSVessel generates a Space out of range error.  Troubleshooting code was left in by mistake.

Version 0.90.0_4.1.0 - Release 18 Feb, 2015 - Solar Panels, Kerbal Renaming, Bugs, Mod Refactoring and More Edition.
 - New:  Added Deployable Solar Panel Management Window.  Works like Hatch panel. In realism Mode, respects Rectractable = false.
 - New:  Refactored Crew Transfer Display code, Hatch Panel code and data management.  Relocated some methods to more logical locations.
 - New:  Added switch to allow renaming Kerbals. Added support for maintaining professions after rename.  Adds non printing chars to name, so use at your own risk.
 - New:  Added autosave of settings on scene change or exiting game.
 - Bug:  After switching scenes and selecting a part with CLS enabled an highlighting error occurs.
 - Bug:  after switching scenes and using Hatch panel, an error occurs in highlighting.
 - Bug:  After switching vessels, the hatch panel shows the old vessel info and is not updated. Additionally, CLS highlighting causes errors.   

Version 0.90.0_4.0.2 - Release 13 Feb, 2015 - Bugs, Mod Refactoring and More Edition.
 - New:  Resource Transfer display and setup system refactored.  Added ability to stop a transfer in progress.  
 - New:  Exposed Resource Transfer Flow Rate Slider min and max values. You can now change the min and max flow rate.  
 - New:  Added a maximum run time in seconds. SM will use the lesser duration of Xfer amount / flow rate or max time.
 - New:  Added tool tips to  controls in the options section of the Settings Window.
 - Bug:  When moving or transferring a kerbal, closing the transfer window, Manifest window or closing the manifest window from any toolbar while the action is in progress causes an error.
 - Bug:  When closing the Transfer Window, internally resetting the selected resource causes an error.

Version 0.90.0_4.0.1 - Release 08 Feb, 2015 - CLS Hatches, CLS Highlighting, Mod Refactoring and More Edition.
 - New:  4.0.1 - Added Create specific Kerbal Type:  Now you can choose Pilot, Engineer or Scientist!
 - New:  4.0.1 - Bug fix for unneeded debug log entry from tooltips
 - New:  4.0.1 - Bug Fix for Non reset Window positions in Config.xml

Version 0.90.0_4.0.0 - Release TBD - CLS Hatches, CLS Highlighting, Mod Refactoring and More Edition.
 - New:  Added support for CLS Hatches.  Now features a hatch dashboard.  Mouseover a hatch and it is highlighted on the vessel.  Takes advantage of changes made to CLS 1.1.1.0 to support third party Hatch control.  Open/close individual/all hatches in a vessel.
 - New:  Revised highlighting to reintroduce CLS enabled highlighting.  Added a switch to turn on/off CLS highlighting.  Takes advantage of recent changes made to CLS highlighting.
 - New:  Many internal architectural changes and refactoring of plugin. Changed windows into separate classes. Prepare for a base window class. Added tooltip support.
 - New:  Added tool tips for many buttons across entire plugin.  Added ToolTip display switch in settings for those that don't want Tool tips to show, including granularity to the window level. 
 - New:  Major Roster Window enhancements.  Altered action button dislay behaviors to be more clear.  Added conditional tooltips to aid in use of little known features. Added Kerbal Title and Status to Kerbal info display.
 - New:  Enabling/Disabling Blizzy Toolbar no longer requires a game restart.  Turn it on or off in settings and it will take effect immediately.
 - New:  Added SETI compatibility support for dataIsCollectable = false in Realism Mode.   Now you cannot Xfer science data where dataIsCollectable = false for the source module.
 - New:  Added vessel resource totals (quick reference) to Manifest Window resource buttons and Transfer Window Resource Title.
 - New:  Added KSP-AVC support [url]http://forum.kerbalspaceprogram.com/threads/79745[/url]
 - Bug:  When when transferring a resource in realism mode, sometimes the source part ends up negative and causes issues (moves resources 1 unit at a time, backwards.

Version 0.90.0_3.3.4 - 16 Jan, 2015 - Bugs, Mod Tweaks and More Edition.
 - New:  Added a Limited Highlighting switch.  When on, highlights only source and target parts.  
         Highlighting switch must be enabled to use.
 - New:  Added close buttons to upper right of most windows.  Cleaned up App launcher toggle button behavior, and synced with close buttons.
 - New:  Added detection for IVA. Hide Ship Manifest Window when in IVA.
 - New:  CLS highlightng returns.  Previous method replaced with new model. Livable parts only will be highlighted by SM.
         To view passable parts, select the space from the CLS plugin menu.  
 - Bug:  Due to KSP 0.90.0 changes, when using Roster, changes to Kerbal names causes the role to change (bad).  
         - Removed ability to edit name of existing Kerbals.

Version 0.24.0.3.3 - 19 Dec, 2014 - 0.90 compatability Edition.
 - Update to correct highlighting errors due to KSP 0.90 changes. 
 - Added a Highlighting switch.  If you don't want highlighting at all, turn it off!
 - Other under the hood changes to improve overall highlighting behavior.  Works very nice now.

Note:  CLS is currently broken in .90, so do not use it until updated.
	
Version 0.25.0_3.3.2b - Interim Release with Fixed stock Toolbar.  CLS Highlighting still in work.
 - Bug:  When changing vessesls in Map view, the stock toolbar would disappear.

Version 0.25.0_3.3.2a - Interim Release with bug fixes and removal of CLS Highlighting
 - New:  Added Resource Fill button when Realism is off.
 - New:  Added Part Fill and Dump of a resource when Realism is off.
 - Improvement:  error handling on frame based errors are now trapped for first occurance.  Subsequent errors will not produce a log entry.
 - Bug:  When crew is selected, and ONLY the Source part OR the Target part is selected, clicking on a move kerbal (>>) button produces an error.
 - Bug:  When changing vessesls with the manifest window open, ClearResourceHighlighting causes an error.
         - http://forum.kerbalspaceprogram.com/threads/62270-0-25-0-Ship-Manifest-(Crew-Science-Resources)-v0-24-2_3-3-2-28-Sep-14?p=1481125&viewfull=1#post1481125

Version 0.24.2_3.3.2 - 28 Sep, 2014 - Post 0.24.2 Bugs and such
 - New: Optional Blizzy Toolbar support!  Courtesy of ragzilla!  All mod dependencies are now gone.
 - Add option in Config for turning OFF resource transfers.
 - Fix science transfers. Really.  (i hope).
 - Added close button to Debug window.
 - bug fixes.  still CLS highlighting to fix.  don't like wht I have atm at all...
 
Version 0.24.0.3.3.1 - 17 Jul, 2014 - 0.24.0 Edition.
 - New:  Now compatible with KSP 0.24.0. Squad reworked crew objects and namespace.
 - New:  Roster Window now shows vessel to which a kerbal is assigned.
 - New:  Add support for DMagic Science parts (IDataScienceContainer)
 - Bug:  SM Still doubling LS resource amounts.
         - http://forum.kerbalspaceprogram.com/threads/62270-0-23-5-Ship-Manifest-%28Crew-Science-Resources%29-v0-23-5-3-2-2-2-May-14?p=1136419&viewfull=1#post1136419
 - Known Issue:  SM & CLS Highlighting still problematic.

Version 0.23.5.3.3 - 29 May, 2014 - CLS is Optonal Edition.
 - New:  CLS is now a soft dependency.  
   - if you install it, SM will configure for it's use.  
   - If you do not install it, SM will automatically detect that and set Enable CLS Off.

Version 0.23.5.3.2.3 - 11 May, 2014 - Settings, Roster & Bug Fix Edition.
 - New:  Roster Window now allows adding and removing individual Kerbals during pre-flight in Realism Mode, and Anytime in Standard mode.
 - New:  Additional changes to Settings.   
         - Now sepearate sections make finding things easier.  
         - Changed LockRealismMode to LockSettings, as Realism Mode is not a parent setting.
		 - Added Locksettings to the Settings Window.  Once set, must be turned off in config file.
		 - Made Enable CLS a child of Enable Crew
 - Bug:  SM not detecting Changes in CrewCapacity with Inflatable Parts...
         - http://forum.kerbalspaceprogram.com/threads/62270-0-23-5-Ship-Manifest-%28Crew-Science-Resources%29-v0-23-5-3-2-1-22-Apr-14?p=1118517&viewfull=1#post1118517
 - Bug:  SM not Erroring when attempting to transfer to a pert with no internal model
		 = http://forum.kerbalspaceprogram.com/threads/62270-0-23-5-Ship-Manifest-%28Crew-Science-Resources%29-v0-23-5-3-2-2-2-May-14?p=1140559&viewfull=1#post1140559
 - Bug:  SM still allowing negative numbers in resource transfers.
         - http://forum.kerbalspaceprogram.com/threads/62270-0-23-5-Ship-Manifest-%28Crew-Science-Resources%29-v0-23-5-3-2-2-2-May-14?p=1136419&viewfull=1#post1136419

Version 0.23.5.3.2.2a - 9 May, 2014 - CLS Hot Fix Edition..
 -  New: Hot Fix.  Recompiled with new assembly reference.  No other changes. 

Version 0.23.5.3.2.2 - 2 May, 2014 - Realism Settings Love Edition..
 -  New: Crew Transfers should allow swapping Kerbals between parts. Swaps are possible within a part, but not between parts. 
 -  www.youtube.com/watch?v=I_TNxjnW234
 -  New: Added config file switch for Enable/Disable TextureReplacer eva event triggers. testing shows it conflicts with TACLS.
 -  Bug: Highlighting behaves abnormally on resource changes from crew to another resource. Residual selected part highlighting... - Internally found, Post release of 0.23.5.3.2.1
 -  Bug: Highlighting behaves abnormally on part changes when moving away from source and target part being the same... - Internally found, Post release of 0.23.5.3.2.1a

Version 0.23.5.3.2.1b - 26 Apr, 2014 - Inflatable Habs Test Edition. Non public release (Not on Spaceport)
 - Bug:  SM not detecting Changes in Creability with Inflatable Parts...
         - http://forum.kerbalspaceprogram.com/threads/62270-0-23-5-Ship-Manifest-%28Crew-Science-Resources%29-v0-23-5-3-2-1-22-Apr-14?p=1118517&viewfull=1#post1118517

Version 0.23.5.3.2.1a - 25 Apr, 2014 - TextureReplacer Switch add
 - New Crew Transfers are now possible between full parts.   Crew members will swap places, just like a Move withing a part.
 - Added Config file switch (TextureReplacer) to allow enable/disable of Texture replcaer event code.  For testing.  Disabled by Default.
 
Version 0.23.5.3.2.1 - 22 Apr, 2014 - Bug fixes Edition.
 - New:  Resource/Part Button Improvements:
         - http://forum.kerbalspaceprogram.com/threads/62270-0-23-5-Ship-Manifest-%28Manage-Crew-Science-Resources%29-v0-23-5-3-2-16-Apr-14?p=1058979&viewfull=1#post1058979
		 - General clean up of contrast / formatting for better readability in hover, unselected and selected modes.
 - New:  Add Auto Popup of Debug console on Error.  Configurable, Off by Default.
 - Bug:  Highlighting fails for source Part when selecting any resource other than crew.
         - Internally found, Post relese of 0.23.5.3.2
 - Bug:  Resource Pump transfers fail wen xferring small amounts.
         - http://forum.kerbalspaceprogram.com/threads/62270-0-23-5-Ship-Manifest-%28Manage-Crew-Science-Resources%29-v0-23-5-3-2-16-Apr-14?p=950355&viewfull=1#post950355
 - Bug:  Tanks sometimes go negative.  
         - http://forum.kerbalspaceprogram.com/threads/62270-0-23-5-Ship-Manifest-%28Manage-Crew-Science-Resources%29-v0-23-5-3-2-16-Apr-14?p=1082740&viewfull=1#post1082740
 - Bug:  Crew event triggers not firing in Preflight.
         - http://forum.kerbalspaceprogram.com/threads/62270-0-23-5-Ship-Manifest-%28Manage-Crew-Science-Resources%29-v0-23-5-3-2-16-Apr-14?p=1100162&viewfull=1#post1100162
 - Bug:  Resource Transfer noises too low.
         - www.youtube.com/watch?v=I_TNxjnW234 
 - Bug: Crew event triggers causing duplicate life support resources in TAC Life Support.
        - http://forum.kerbalspaceprogram.com/...=1#post1108035

Version 0.23.5.3.2 - 16 Apr, 2014 - Add ConnectedLivingSpace Integration.
 - Realism Mode:  Crew Xfers & Moves are now "space" aware.  if the target part is not connected via an internal passageway, then the xfer or move cannot occur.
 - CLS awareness can be turned off in the config file, for those that want to be able to xfer across living spaces.
 - General code cleanup and significant reorganization to use fewer frame and memory resources.
 - Include Toolbar 1.7.1 redistribution 

Version 0.23.3.1.5 - 26 Feb, 2014 - Add Realism to Crew Transfer.
 - Realism Mode:  Crew Xfers & Moves now occur in "real" time.  We now have Sounds of crewman moving.  Portrait updates now occur when Kerbals get in thier new seat.
	- Added Crew sound locations to settings Window.
	- Added Config for Crew Transfer Duration from part to part.  This is a placeholder for later development.
	- Added default xfer duration of 2 sec for Seat to Seat Xfers within the same Part.
 - Resource Xfers enhancements.  Added textbox for entering Exact Xfer Amounts.  Integers can be entered, and fractions can be pasted into box for now...
 - Rearranged resource xfer details to make it more intuitive after adding text box.
 - Include Toolbar 1.6.0 redistribution 

Version 0.23.3.1.4 - 15 Feb, 2014 - General cleanup, configuration & UI enhancements.
 - Realism Mode:  Science Xfers now render experiments inoperable after xfer.
 - Changed Science Xfers Target Module selection to Auto select, If the target Part has only 1 target module. Saves a click the majority of the time. 
 - Added Save Debug Log support.  Save Log button now works.
 - Added support for tailoring Ship Manifest for your needs:
	- Config File switch to enable/disable Fill & Empty Resource buttons in PreFlight.  Enabled by default.
	- Config File switch to enable/disable Crew Transfer Feature.  Enabled by default.
	- Config File switch to enable/disable Science Transfer Feature.  Enabled by default.
- Include Toolbar 1.5.3 redistribution

Version 0.23.3.1.3 - 12 Feb, 2014 - Add Preflight features and fix preflight fill resources bug.
 - Add Crew Fill and Crew Empty Buttons to PreFlight.   Now you can fill or empty your vessel on the pad.
 - Reworked Resource Fill Vessel and Empty Vessel to respect Realism mode. (resourceTransferMode = NONE).
 - Added Dump Resource buttons to Resource List in Manifest Window.  Now you can dump single resource on the entire vessel.
   Realism Mode support:  Preflight, you can dump resources.  In Flight, you cannot.
 - Added mod version to Debug Window.

Version 0.23.3.1.2 - 11 Feb, 2014 - Add Bi-Directional Resource Xfers and Science Xfer Bug fix.
 - Bi-Directional Resource transfers.   Now you can move a resource from source to target or target to source.
 - ** Science bug fix.   Sorry about that everyone.   It now works. :D
 - Improved verbose logging of science, crew, and resource xfers.
 - Include Toolbar 1.5.2 redistribution

Version 0.23.3.1 - 3 Feb, 2014 - Add Science Xfers and Bug fixes
 - Added Science Transfers. Select source part & modulle, select target part & module, click xfer.
 - Realism Mode now respects Resource.TransferMode = None on resource Xfers.
 - Add config switch for locking Realism Mode.  
   1 = locked in the mode set by RealismMode in config.  Displays Realism Mode in Settings Window.  
   0 = unlocked.  Normal Radio button shows in Settings Window, and Setting can bee changed.
 - Bug fix. Xfering a crew member frrom target to source would violate the capacity limitation of the part and cause lost kerbals.
 - Bug fix. xfering crew would sometimes result in no portrait, or missing portrait.  Added Update Portraits button.  Thanks to ATG, for helping me characterize the issue.

Version 0.23.3.0 - 27 Jan, 2014 - Add Crew Transfers and include Toolbar 1.4.4
 - Crew Transfers.  Uses the same interface as resources.   Crew is simply another resource on the ship.
 - Crew Roster.  Manage Crew Attributes. Create Crew members, "Kill" Crew members.
 - Sound file size reduction using .ogg.
 - Configurable source & target highlighting (via config file for now...)  source = red & target = green by default now.
 - Bug fixes.  minor display errors.  Less than 100% fills were not working correctly.

Version 0.23.2.0 - 03 Jan, 2014 - Add Realism Mode and Redistribution of toolbar 1.4.0
 - Add Realism Mode:  Sounds, Real time resource flow between parts.  Configurable sounds and flow rate.
 - Add Setting Window to allow configuration of various features of Ship Manifest.
 - Updated to Toolbar 1.4.0

Version 0.23.1.3.1 - 27 Dec 2013 - minor bug fix and redistrinbution of toolbar 1.3.0
 - Minor bug fix.  When closing all windows with the toolbar button, attached mouse exit part event handlers were not being released.
 - updated Toolbar 1.3.0 by Blizzy78.

Version 0.23.1.3 23 Dec 2013 - Bug Fix
 - post bug fix regression bug.   source part and target part highlighting failing.

Version 0.23.1.2 22 Dec 2013 - Bug Fix
 - When selecting a resource, parts illuminate.  Close manifest window.  Parts stay illuminated.
 - Figured out releases in Github.   This is the initial release on Github.

Version 0.23.1.1 22 Dec 2013 - Revision
   - Added Selected part highlighting support. 
   - added onmouseexit handlers to affected parts.  Handlers ensure proper highlighting is retained.
   - added new icons.   Now Ship manifest has it own icons.

Version 0.23.1.0 18 Dec 2013 - Revision
   - Removed Crew Manifest.
   - Improved GUI, resizing and cleaning up data.
   - cleaned up transfer method, improving accuracy.
   - incorporated Toolbar 1.2 by Blizzy78.

Version 0.23.0.1 17 Dec 2013 - Initial beta release


Kerbal Space Program Addon.  Manages Resources on a given vessel.

This work is based in large part on Crew manifest 0.5.6.0 by xxSovereignxx as modified by Sarbian to work with 0.22.

