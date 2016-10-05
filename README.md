# Cities: Skylines - Traffic Manager: *President Edition*
A work-in-progress modification for **Cities: Skylines** to add additional traffic control

# Overview of purpose of Visor-VU

This mod was extended for Visor-VU with the purpose of adding the
ability to remotely control and sense traffic intersections in Cities:
Skylines using a hardware-in-the-loop (HIL) platform. For this
purpose, we needed to be able to remotely determine the traffic on
each street at an intersection (sense), and to be able to remotely set
the traffic light state for a given intersection (actuate).

# Additions for Visor-VU

To accomplish the goals of remote sensing and acutation, we developed
a UDP-server thread which runs inside the Cities: Skylines game
whenever the game is running a simulation, and provides an API for
sending data to the game and getting data from the game. This remote
interface works because the game's APIs exposed to modders are
thread-safe.

The network code was ported over from a first attempt at a more
generic interface which would expose the entire API; this other mod
was incomplete in the timeline required, but provided enough of a
starting point to get the remote sensing/actuation working. The code
for the old mod can be found
[here](http://github.com/finger563/cities-skylines-network-api).

Parts of that mod have been moved into the `Net` folder,
[here](./TLM/TLM/Net).

# How to compile the mod

Load the mod in Visual Studio 2015, and resolve the references to the
Cities: Skylines assemblies, which are found in your Cities: Skylines
install folder. 

default: `C:\Program Files (x86)\Steam\steamapps\common\Cities_Skylines\Cities_Data\Managed`

Once those assemblies have been resolved, the mod will
build and produce a `dll` which it will try to automatically put into
`%LOCALAPPDATA%/Colossal Order/Cities_Skylines/Addons/Mods/{Mod
Name}`, from which Cities: Skylines will automatically be able to run
it at run-time.

# How to set up the mod in Cities:Skylines

Once you have compiled the mod using Visual Studio and it has placed
the mod into the folder mentioned in the previous section, you will
need to copy the MONO dlls from the Cities: Skylines install folder:
Default: `C:\Program Files (x86)\Steam\steamapps\common\Cities_Skylines\Mono\lib\mono\2.0`
into `%LOCALAPPDATA%/Colossal Order/Cities_Skylines/Addons/Mods/{Mod
Name}`(since some of the serialization classes will be
required for execution). After that is complete, the mod will be fully
automatically loaded by the game.

Once you start the game, you can go to the content manager and enable
the mod.

# How to use the mod within a savegame / city of Cities: Skylines

Once you have loaded a save game or created a city in Cities:
Skylines, you can click the `Traffic President` button to activate the
mod's UI. The mod has been altered so that when you select an
intersection to be manually controlled, it never releases control of
that intersection back to the game, and instead keeps track of the 4
most recent intersections you have selected. These four are able to be
controlled over the network interface.

# Changelog
1.7.1, 07/24/2016:
- Reverted "Busses now may only ignore lane arrows if driving on a bus lane" for now
- Bugfix: Trains were not despawning if no path could be calculated
- Workaround for third-party issue: TM:PE now detects if the calculation of total vehicle length fails    

1.7.0, 07/23/2016:
- New feature: Traffic++ lane connector
- Busses now may only ignore lane arrows if driving on a bus lane
- Rewritten and simplified vehicle position tracking near timed traffic lights and priority signs for performance reasons
- Improved performance of priority sign rules
- AI: Cims now ignore junctions where pedestrian lights never change to green
- AI: Removed the need to define a lane changing probability 
- AI: Tweaked lane changing parameters
- AI: Highway rules are automatically disabled at complex junctions (= more than 1 incoming and more than 1 outgoing roads)
- Improved UI performance if overlays are deactivated
- Simulation accuracy now also controls time intervals between traffic measurements
- Added compatibility detection for the Rainfall mod
- Improved fault-tolerance of the load/save system
- Default wait-flow balance is set to 0.8
- Bugfix: Taxis were allowed to ignore lane arrows
- Bugfix: AI: Highway rules on left-hand traffic maps did not work the same as on right-hand traffic maps
- Bugfix: Upgrading a road segment next to a timed traffic light removed the traffic light leading to an inconsistent state (thanks to @ad.vissers for pointing out this problem)

1.6.22, 06/29/2016:
- AI: Taxis now may not ignore lane arrows and are using bus lanes whenever possible (thanks to @Cochy for pointing out this issue)
- AI: Busses may only ignore lane arrows while driving on a bus lane
- Bugfix: Traffic measurement at timed traffic lights was incorrect

1.6.22, 06/21/2016:
- Speed/vehicle restrictions may now be applied to all road segments between two junctions by holding the shift key
- Reworked how changes in the road network are recognized 
- Advanced Vehicle AI: Improved lane selection at junctions where bus lanes end
- Advanced Vehicle AI: Improved lane selection of busses
- Improved automatic pedestrian lights 
- Improved separate traffic lights: Traffic lights now control traffic lane-wise
- UI: Sensitivity slider is only available while adding/editing a step or while in test mode
- Bugfix: Lane selection on maps with left-hand traffic was incorrect
- Bugfix: While building in pause mode, changes in the road network were not always recognized causing vehicles to stop/despawn 
- Bugfix: Police cars off-duty were ignoring lane arrows
- Bugfix: If public transport stops were near a junction, trams/busses were not counted by timed traffic lights (many thanks to Filip for identifying this problem)
- Bugfix: Trains/Trams were sometimes ignoring timed traffic lights (many thanks to Filip for identifying this problem)
- Bugfix: Building roads with bus lanes caused garbage, bodies, etc. to pile up 

1.6.21, 06/14/2016:
- Bugfix: Too few cargo trains were spawning (thanks to @Scratch, @toruk_makto1, @Mr.Miyagi, @mottoh and @Syparo for pointing out this problem)       
- Bugfix: Vehicle restrictions did not work as expected (thanks to @nordlaser for pointing out this problem)

1.6.20, 06/11/2016:
- Bugfix: Priority signs were not working correctly (thanks to @mottoth, @Madgemade for pointing out this problem)

1.6.19, 06/11/2016
- Bugfix: Timed traffic lights UI not working as expected (thanks to @Madgemade for pointing out this problem)

1.6.18, 06/09/2016
- Updated for game patch 1.5.0-f4
- Improved performance of priority signs and timed traffic lights
- Players can now select elevated rail segments/nodes 
- Trams and trains now follow priority signs
- Improved UI behavior when setting up priority signs

1.6.17, 04/20/2016
- Hotfix for reported path-finding problems

1.6.16, 04/19/2016
- Updated for game patch 1.4.1-f2

1.6.15, 03/22/2016
- Updated for game path 1.4.0-f3
- Possible fix for crashes described by @cosminel1982
- Added traditional Chinese translation

1.6.14, 03/17/2016
- Bugfix: Cargo trucks did not obey vehicle restrictions (thanks to @ad.vissers for pointing out this problem)
- Bugfix: When Advanced AI was deactivated, u-turns did not have costs assigned

1.6.13, 03/16/2016
- Added Dutch translation
- The pedestrian light mode of a traffic light can now be switched back to automatic
- Vehicles approaching a different speed limit change their speed more gradually
- The size of signs and symbols in the overlay is determined by screen resolution height, not by width
- Path-finding: Performance improvements
- Path-finding: Fine-tuned lane changing behaviour
- Bugfix: After loading another savegame, timed traffic lights stopped working for a certain time
- Bugfix: Lane speed calculation corrected

1.6.12, 03/03/2016
- Improved memory usage
- Bugfix: Adding/removing junctions to/from existing timed traffic lights did not work (thanks to @nieksen for pointing out this problem)
- Bugfix: Separate timed traffic lights were sometimes not saved (thanks to @nieksen for pointing out this problem)
- Bugfix: Fixed an initialization error (thanks to @GordonDry for pointing out this problem)

1.6.11, 03/03/2016
- Added Chinese translation
- By pressing "Page up"/"Page down" you can now switch between traffic and default map view
- Size of information icons and signs is now based on your screen resolution
- UI code refactored 

1.6.10, 03/02/2016
- Additional controls for vehicle restrictions added
- Bugfix: Clicking on a Traffic Manager overlay resulted in vanilla game components (e.g. houses, vehicles) being activated 

1.6.9, 03/02/2016
- Updated for game patch 1.3.2-f1

1.6.8, 03/01/2016
- Path-finding: Major performance improvements
- Updated Japanese translation (thanks to @Akira Ishizaki for translating!)
- Added Spanish translation

1.6.7, 02/27/2016
- Tuned AI parameters
- Improved traffic density measurements
- Improved lane changing near junctions: Reintroduced costs for lane changing before junctions
- Improved vehicle behavior near blocked roads (e.g. while a building is burning)
- Bugfix: Automatic pedestrian lights for outgoing one-ways fixed
- Bugfix: U-turns did not have appropriate costs assigned
- Bugfix: The time span between AI traffic measurements was too high

1.6.6, 02/27/2016
- It should now be easier to select segment ends in order to change lane arrows.
- Priority signs now cannot be setup at outgoing one-ways.
- Updated French translation (thanks to @simon.royer007 for translating!)
- Updated Polish translation (thanks to @Krzychu1245 for translating!)
- Updated Portuguese translation (thanks to @igordeeoliveira for translating!)
- Updated Russian translation (thanks to @FireGames for translating!)
- Bugfix: U-turning vehicles were not obeying the correct directional traffic light (thanks to @t1a2l for pointing out this problem)

1.6.5, 02/24/2016
- Added despawning setting to options dialog
- Improved detection of Traffic++ V2

1.6.4, 02/23/2016
- Minor performance improvements
- Bugfix: Path-finding calculated erroneous traffic density values 
- Bugfix: Cims left the bus just to hop on a bus of the same line again (thanks to @kamzik911 for pointing out this problem)
- Bugfix: Despawn control did not work (thanks to @xXHistoricalxDemoXx for pointing out this problem)
- Bugfix: State of new settings was not displayed corretly (thanks to @Lord_Assaultーさま for pointing out this problem)
- Bugfix: Default settings for vehicle restrictions on bus lanes corrected
- Bugfix: Pedestrian lights at railway junctions fixed (they are still invisible but are derived from the car traffic light state automatically)

1.6.3, 02/22/2016
- Bugfix: Using the "Old Town" policy led to vehicles not spawning.
- Bugfix: Planes, cargo trains and ship were sometimes not arriving
- Bugfix: Trams are not doing u-turns anymore

1.6.2, 02/20/2016
- Trams are now obeying speed limits (thanks to @Clausewitz for pointing out the issue)
- Bugfix: Clear traffic sometimes throwed an error
- Bugfix: Vehicle restrctions did not work as expected (thanks to @[Delta ²k5] for pointing out this problem) 
- Bugfix: Transition of automatic pedestrian lights fixed

1.6.1, 02/20/2016
- Improved performance
- Bugfix: Fixed UI issues
- Modifying mod options through the main menu now gives an annoying warning message instead of a blank page.

1.6.0, 02/18/2016
- New feature: Separate traffic lights for different vehicle types
- New feature: Vehicle restrictions
- Snowfall compatibility
- Better handling of vehicle bans
- Improved the method for calculating lane traffic densities
- Ambulances, fire trucks and police cars on duty are now ignoring lane arrows
- Timed traffic lights may now be setup at arbitrary nodes on railway tracks
- Reckless drivers now do not enter railroad crossings if the barrier is down
- Option dialog is disabled if accessed through the main menu
- Performance optimizations
- Advanced Vehicle AI: Improved lane spreading
- The option "Vehicles may enter blocked junctions" may now be defined for each junction separately
- Vehicles going straight may now change lanes at junctions
- Vehicles may now perform u-turns at junctions that have an appropriate lane arrow configuration
- Road conditions (snow, maintenance state) may now have a higher impact on vehicle speed (see "Options" menu)
- Emergency vehicles on duty now always aim for the the fastest route
- Bugfix: Path-finding costs for crossing a junction fixed
- Bugfix: Vehicle detection at timed traffic lights did not work as expected
- Bugfix: Not all valid traffic light arrow modes were reachable

1.5.2, 02/01/2016
- Traffic lights may now be added to/removed from underground junctions
- Traffic lights may now be setup at *some* points of railway tracks (there seems to be a game-internal bug that prevents selecting arbitrary railway nodes)
- Display of priority signs, speed limits and timed traffic lights may now be toggled via the options dialog
- Bugfix: Reckless driving does not apply for trains (thanks to @GordonDry for pointing out this problem)
- Bugfix: Manual traffic lights were not working (thanks to @Mas71 for pointing out this problem)
- Bugfix: Pedestrians were ignoring timed traffic lights (thanks to @Hannes8910 for pointing out this problem)
- Bugfix: Sometimes speed limits were not saved (thanks to @cca_mikeman for pointing out this problem) 

1.5.1, 01/31/2016
- Trains are now following speed limits

1.5.0, 01/30/2016
- New feature: Speed restrictions (as requested by @Gfurst)
- AI: Parameters tuned
- Code improvements
- Lane arrow changer window is now positioned near the edited junction (as requested by @GordonDry)
- Bugfix: Flowing/Waiting vehicles count corrected

1.4.9, 01/27/2016
- Junctions may now be added to/removed from timed traffic lights after they are created
- When viewing/moving a timed step, the displayed/moved step is now highlighted (thanks to Joe for this idea)
- Performance improvements
- Bugfix (AI): Fixed a division by zero error (thanks to @GordonDry for pointing out this problem)
- Bugfix (AI): Near highway exits vehicles tended to use the outermost lane (thanks to @Zake for pointing out this problem)
- Bugfix: Some lane arrows disappeared on maps using left-hand traffic systems (thanks to @Mas71 for pointing out this problem)
- Bugfix: In lane arrow edit mode, the order of arrows was sometimes incorrect (thanks to @Glowstrontium for pointing out this problem)
- Bugfix: Lane merging in left-hand traffic systems fixed
- Bugfix: Turning priority roads fixed (thanks to @GordonDry for pointing out this problem)

1.4.8, 01/25/2016
- AI: Parameters have been tuned
- AI: Added traffic density measurements
- Performance improvements
- Added translation to Polish (thanks to @Krzychu1245 for working on this!)
- Added translation to Russian (thanks to @FireGames for working on this!)
- Bugfix: After removing a timed or manual light the traffic light was deleted (thanks to @Mas71 for pointing out this problem)
- Bugfix: Segment geometries were not always calculated
- Bugfix: In highway rule mode, lane arrows sometimes flickered 
- Bugfix: Some traffic light arrows were sometimes not selectable 
 
1.4.7, 01/22/2016
- Added translation to Portuguese (thanks to @igordeeoliveira for working on this!) 
- Reduced mean size of files can become quite big (thanks to @GordonDry for reporting this problem)
- Bugfix: Freight ships/trains were not coming in (thanks to @Mas71 and @clus for reporting this problem)
- Bugfix: The toggle "Vehicles may enter blocked junctions" did not work properly (thanks for @exxonic for reporting this problem)
- Bugfix: If a timed traffic light is being edited the segment geometry information is not updated (thanks to @GordonDry for reporting this problem)

1.4.6, 01/22/2016
- Running average lane speeds are measured now
- Minor fixes

1.4.5, 01/22/2016
- The option "Vehicles may enter blocked junctions" may now be defined for each junction separately
- Bugfix: A deadlock in the path-finding is fixed
- Bugfix: Small timed light sensitivity values (< 0.1) were not saved correctly 
- Bugfix: Timed traffic lights were not working for some players
- Refactored segment geometry calculation

1.4.4, 01/21/2016
- Added localization support

1.4.3, 01/20/2016
- Several performance improvements
- Improved calculation of segment geometries
- Improved load balancing
- Police cars, ambulances, fire trucks and hearses are now also controlled by the AI
- Bugfix: Vehicles did not always take the shortest path
- Bugfix: Vehicles disappeared after deleting/upgrading a road segment
- Bugfix: Fixed an error in path-finding cost calculation
- Bugfix: Outgoing roads were treated as ingoing roads when highway rules were activated

1.4.2, 01/16/2016
- Several major performance improvements (thanks to @sci302 for pointing out those issues)
- Improved the way traffic lights are saved/loaded
- Lane-wise traffic density is only measured if Advanced AI is activated
- Bugfix: AI did not consider speed limits/road types during path calculation (thanks to @bhanhart, @sa62039 for pointing out this problem)
- Connecting a city road to a highway road that does not supply enough lanes for merging leads to behavior people do not understand (see manual). Option added to disable highway rules.  
- Bugfix: Vehicles were stopping in front of green traffic lights
- Bugfix: Stop/Yield signs were not working properly (thanks to @GordonDry, @Glowstrontium for pointing out this problem)
- Bugfix: Cargo trucks were ignoring the "Heavy ban" policy, they should do now (thanks to @Scratch for pointing out this problem)

1.4.1, 01/15/2016
- Bugfix: Path-finding near junctions fixed

1.4.0, 01/15/2016
- Introducing Advanced Vehicle AI (disabled by default! Go to "Options" and enable it if you want to use it.)
- Bugfix: Traffic lights were popping up in the middle of roads
- Bugfix: Fixed the lane changer for left-hand traffic systems (thanks to @Phishie for pointing out this problem)
- Bugfix: Traffic lights on invalid nodes are not saved anymore 

1.3.24, 01/13/2016
- Improved handling of priority signs
- Priority signs: After adding two main road signs the next offered sign is a yield sign
- Priority signs: Vehicles now should notice earlier that they can enter a junction
- Removed the legacy XML file save system
- Invalid (not created) lanes are not saved/loaded anymore
- Added a configuration option that allows vehicles to enter blocked junctions
- Bugfix: Some priority signs were not saved
- Bugfix: Priority signs on deleted segments are now deleted too
- Bugfix: Lane arrows on removed lanes are now removed too
- Bugfix: Adding a priority sign to a junction having more than one main sign creates a yield sign (thanks to @GordonDry for pointing out this problem)
- Bugfix: If reckless driving was set to "The Holy City (0 %)", vehicles blocked intersections with traffic light.
- Bugfix: Traffic light arrow modes were sometimes not correctly saved  

1.3.23, 01/09/2016
- Bugfix: Corrected an issue where toggled traffic lights would not be saved/loaded correctly (thanks to @Jeffrios and @AOD_War_2g for pointing out this problem)
- Option added to forget all toggled traffic lights

1.3.22, 01/08/2016
- Added an option allowing busses to ignore lane arrows
- Added an option to display nodes and segments

1.3.21, 01/06/2016
- New feature: Traffic Sensitivity Tuning
- UI improvements: When adding a new step to a timed traffic light the lights are inverted.
- Timed traffic light status symbols should now be less annoying 
- Bugfix: Deletion of junctions that were members of a traffic light group is now handled correctly 

1.3.20, 01/04/2016
- Bugfix: Timed traffic lights are not saved correctly after upgrading a road nearby
- UI improvements
- New feature: Reckless driving 

1.3.19, 01/04/2016
- Timed traffic lights: Absolute minimum time changed to 1
- Timed traffic lights: Velocity of vehicles is being measured to detect traffic jams
- Improved traffic flow measurement
- Improved path finding: Cims may now choose their lanes more independently
- Bugfix: Upgrading a road resets the traffic light arrow mode

1.3.18, 01/03/2016
- Provided a fix for unconnected junctions caused by other mods
- Crosswalk feature removed. If you need to add/remove crosswalks please use the "Crossings" mod.
- UI improvements: You can now switch between activated timed traffic lights without clicking on the menu button again

1.3.17, 01/03/2016
- Bugfix: Timed traffic lights cannot be added again after removal, toggling traffic lights does not work (thanks to @Fabrice, @ChakyHH, @sensual.heathen for pointing out this problem)
- Bugfix: After using the "Manual traffic lights" option, toggling lights does not work (thanks to @Timso113 for pointing out this problem)

1.3.16, 01/03/2016
- Bugfix: Traffic light settings on roads of the Network Extensions mods are not saved (thanks to @Scarface, @martintech and @Sonic for pointing out this problem)
- Improved save data management 

1.3.15, 01/02/2016
- Simulation accuracy (and thus performance) is now controllable through the game options dialog
- Bugfix: Vehicles on a priority road sometimes stop without an obvious reason

1.3.14, 01/01/2016
- Improved performance
- UI: Non-timed traffic lights are now automatically removed when adding priority signs to a junction
- Adjusted the adaptive traffic light decision formula (vehicle lengths are considered now)
- Traffic two road segments in front of a timed traffic light is being measured now  

1.3.13, 01/01/2016
- Bugfix: Lane arrows are not correctly translated into path finding decisions (thanks to @bvoice360 for pointing out this problem)
- Bugfix: Priority signs are sometimes undeletable (thank to @Blackwolf for pointing out this problem) 
- Bugfix: Errors occur when other mods without namespace definitions are loaded (thanks to @Arch Angel for pointing out this problem)
- Connecting a new road segment to a junction that already has priority signs now allows modification of the new priority sign

1.3.12, 12/30/2015
- Bugfix: Priority signs are not editable (thanks to @ningcaohan for pointing out this problem)

1.3.11, 12/30/2015
- Road segments next to a timed traffic light may now be deleted/upgraded/added without leading to deletion of the light
- Priority signs and Timed traffic light state symbols are now visible as soon as the menu is opened

1.3.10, 12/29/2015
- Fixed an issue where timed traffic light groups were not deleted after deleting an adjacent segment

1.3.9, 12/29/2015
- Introduced information icons for timed traffic lights
- Mod is now compatible with "Improved AI" (Lane changer is deactivated if "Improved AI" is active)

1.3.8, 12/29/2015
- Articulated busses are now simulated correctly (thanks to @nieksen for pointing out this problem)
- UI improvements

1.3.7, 12/28/2015
- When setting up a new timed traffic light, yellow lights from the real-world state are not taken over
- When loading another save game via the escape menu, Traffic Manager does not crash
- When loading another save game via the escape menu, Traffic++ detection works as intended
- Lane arrows are saved correctly

1.3.6, 12/28/2015
- Bugfix: wrong flow value taken when comparing flowing vehicles
- Forced node rendering after modifying a crosswalk

1.3.5, 12/28/2015
- Fixed pedestrian traffic Lights (thanks to @Glowstrontium for pointing out this problem)
- Better fix for: Deleting a segment with a timed traffic light does not cause a NullReferenceException
- Adjusted the comparison between flowing (green light) and waiting (red light) traffic

1.3.4, 12/27/2015
- Better traffic jam handling

1.3.3, 12/27/2015
- (Temporary) hotfix: Deleting a segment with a timed traffic light does not cause a NullReferenceException
- If priority signs are located behind the camera they are not rendered anymore

1.3.2, 12/27/2015
- Priority signs are persistently visible when Traffic Manager is in "Add priority sign" mode
- Synchronized traffic light rendering: In-game Traffic lights display the correct color (Thanks to @Fabrice for pointing out this problem)
- Traffic lights switch between green, yellow and red. Not only between green and red.
- UI tool tips are more explanatory and are shown longer.

1.3.1, 12/26/2015
- Minimum time units may be zero now
- Timed traffic lights of deleted/modified junctions get properly disposed

1.3.0, 12/25/2015
- **Adaptive Timed Traffic Lights** (automatically adjusted based on traffic amount)

1.2.0 (iMarbot)
- Updated for 1.2.2-f2 game patch.

# Current features

- Add/Remove traffic lights
- Adaptive timed traffic lights
- Add priority signs
- Change lanes
- Add/Remove crosswalks
- Manually control traffic lights
- Timed traffic lights
- Clear traffic
- No despawn

# Todo list

- I would like to investigate why yellow traffic lights sometimes are not properly rendered.
- I would like to implement traffic light templates so that you would not need to manually set up individual steps for common junction patterns.
- Stop signs should be more useful. Drivers should act more realistically/confident when stop/yield signs and traffic jams meet together.
- Adaptive Timed Traffic Lights: Currently only vehicles on the road segment next to the junction are being measured. I would like to expand the traffic measurement to 2 segments.
- When switching between control modes, the UI starts to ignore user mouse input. This is annoying. I will hopefully fix that.
- We could measure if there is traffic backing up after a timed traffic light. If it is the case (that is: cars having a green light do not move) the next timed step could be activated.
- There are still some issues with crossings and pedestrain crossing lights (missing textures, double crosswalks). Let's see what can be done.
- For new users it takes some time to understand how the mod works. Having something like a (video) manual would be great. Or just a better UI. 

# Upcoming changes

- Code optimization & refactoring
- Timed Traffic Light Templates (ready-to-use directional traffic light patterns)
