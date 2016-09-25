# React Launcher
Simple launcher for React's MW2 client.

When launching the game, it scans the console's output for queues that the game has fudged up your stats or classes.
If it detects that it has happened, it kills the client and alerts the user, then attempts to fix the issue by deleting the Player's stats and settings when launching the game again.

It also gives the option to delete the player's stats, regardless of whether there is an issue.

If no Player Name is supplied, it doesn't pass a name to the client at launch. You do not need to supply a name if the game remembers your name.