Usage:
Extract to %userprofile%\documents\my games\stationeers\mods


Contents:
About\About.xml 	- Your mod meta-data used when uploaded to the workshop and when displaying your mod in the workshop menu.
About\Preview.png	- Preview image of your mod used in the in-game mod browser.
About\thumb.png		- Preview image of your mod used in the Steam Workshop.  MUST BE BELOW 1MB OR UPLOAD WILL FAIL.

Gamedata\*.xml		- Gamedata files to be loaded if your mod is enabled.

XML Data Types:
<FabricatorName>.xml	- Recipe data for a fabrication machine of the given name, possibilities:
				- autolathe.xml
				- toolmanufacturer.xml
				- pipebender.xml
				- electronicsprinter.xml

			NOTE: Duplicate recipes for the same prefab will be discarded, so ensure your mod load order is set accordingly.

			
startconditions.xml	- This lets you create sets of starting conditions that can be selected from the game settings screen.

You can find examples of the base game data files and their correct formats in Stationeers\rocketstation_Data\StreamingAssets\Data.

If you have any questions, check out #modding on the Stationeers discord!