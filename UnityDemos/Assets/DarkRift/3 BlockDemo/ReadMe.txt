The BlockDemo is a more complex Minecraft clone and uses a plugin in DarkRift to authoritatively control the game.

To run, download the BlockDemoDarkRiftPlugin project from GitHub (linked below), open in Visual Studio and update 
the references to your DarkRift files. Build the project and copy the BlockDemoDarkRiftPlugin.dll file into the Plugins 
folder of your DarkRift server.

Run the server. You should it load the plugins on the console:
	Loaded plugin BlockDemoPlayerManager version ____
	Loaded plugin BlockDemoWorldManager version ____

If you haven't already done so, set RunInBackground to true in your Unity project settings.

You can now build the BlockDemo scene and run a few copies locally. You should see be able to move and use the mouse buttons 
to create/destroy blocks in the world!

Block Demo Plugin: https://github.com/DarkRiftNetworking/BlockDemoDarkRiftPlugin