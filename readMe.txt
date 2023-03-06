How to RUN program:
top right corner is a drop down menu to choose the map
top left corner has a ‘+’ button to open up the setting for A*:
	choose Heuristic between `Manhattan` and `Euclidean`
	set the timer gap for each iteration of A* (for showing algorithm step by step)
	start algorithm button (when both start and end are set)
	reset the map

In map:
	player could click any tile and turn it to grey -> pending action
	then player could try to press key on keyboard to set it:
		`S` - set to start point
		`E` - set to end point
		`O` - set to obstacle
		`R` - set to road
	Camera Control:
		WASD - movement of camera
		scroll - size of camera

Color code of Tiles:
	- Red: start / path
	- Green: end
	- Blue: Tiles in Open list
	- Yellow: Tiles in Closed list
	- Grey: Pending tiles waiting for actions



Tile Representation:
	Each tile in game is represented by 2x2 chars in the map
	The type of the tile would adopt the maximum exsiting type in the 2x2 grid
	Limit: Each tile is block by block that does not exisit any partially blocked tile,
	might loss the diversity of the environment
	Advantage: It is more computational friendly

Currently, the size of agent we assume is 1x1, which is equal to the tile size, so we only need to compare the `to tile` is obstacle or not. If the size is bigger, we need to consider about its left and right based on the size of the agent.

Way points design:

After discussion, we decided to use a manual approach to find all way points. using an automatic algorithm, a simple algorithm would only reduce the total number of path points but not build efficient paths, and there are many complex cases and corner cases that make the method unwieldy.
In our approach, we prioritize coverage, which means that we prioritize avoiding the presence of unreachable destinations. Then, after constructing a reasonable way point map, we perform an optimization to remove some points from the duplicate coverage area and then reduce the computational load.
The process will be similar with tile method. We first create the map and connect all way points. And then we add the start and end location. The A* algorithm will begin after we connect the start node, end node and other way point nodes. At the end it should have a better result compare to the tile method.
