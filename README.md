ShipManifest
============
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

