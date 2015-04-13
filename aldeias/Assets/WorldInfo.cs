using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WorldInfo : MonoBehaviour {

	//Classes and values that have to be declared and defined before the actual WorldTileInfo declaration
	public partial class WorldTileInfo {
		public static Tribe nullTribe=new Tribe();
		public static TribeTerritory defaultTribeTerritory=new TribeTerritory();
		public class TribeTerritory {
			public bool hasFlag=false;
			public Tribe ownerTribe=nullTribe;
		}
		public class Tribe {
			//Insert tribe identification here
		}
	}

	//Information being holded in every tile
	//The information contained in every tile is readonly.
	public partial class WorldTileInfo {
		public bool hasTree=false;
		public bool isHabitat=false;
		public TribeTerritory tribeTerritory=defaultTribeTerritory;
	}

	// The size of the world in rows and columns.
	public int xSize = 50;
	public int zSize = 50;

	// The tiles of the world.
	public WorldTileInfo[,] worldTileInfo; 

	// All the tribes that exist in the world.
	public List<Tribe> tribes; 

	// Every habitant believes that he belongs to a tribe.

	// The agents that exist in the world.
	public List<Habitant> habitants;
	public List<Animal> animals; 
	public List<Agent> allAgents;


	public void WorldTick () {
		// Update agents' sensors. (agent.sensors.update();)
		// Collect actions from all agents.
		//    Run the agents' decision cycles.
		//    Collect the actions that are going to be performed. Remember which agent issued them.
		// Apply actions to the world.



		// Rules:
		//    Agents cannot change the world state directly. They must produce an action that will be applied to the world.

		// Ideas:
		//    Gather failed actions to be reported to the agents if needed.
		//    The world may not be responsible of reporting failed action. The agents can detect failed actions using their sensors.

		// Is the agent's decision cycle synchronous?
		// How do the agents perceive the world state?
	}

	void Start () {
		GenerateWorldTileInfo ();
		SetDebugWorldTileInfo ();
	}

	void GenerateWorldTileInfo () {
		worldTileInfo = new WorldTileInfo[xSize,zSize];
		FillWithDefaultWorldTileInfo ();
	}

	void FillWithDefaultWorldTileInfo () {
		for(int x=0; x<xSize; x++) {
			for(int z=0; z<zSize; z++) {
				worldTileInfo[x,z] = new WorldTileInfo();
			}
		}
	}

	//FIXME: Currently setting WorldTileInfo fields directly. Might need to have a better way to change their values.
	void SetDebugWorldTileInfo () {
		//Fill (0,0) to (xsize/2 - 1,zSize/2 - 1) with animal habitat
		for(int x=0; x<xSize/2; x++) {
			for(int z=0; z<zSize/2; z++) {
				worldTileInfo[x,z].isHabitat = true;
			}
		}
		//Fill (xSize/2,0) to (xsize-1,zSize/2 - 1) corner with trees
		for(int x=xSize/2; x<xSize; x++) {
			for(int z=0; z<zSize/2; z++) {
				worldTileInfo[x,z].hasTree = true;
			}
		}
		//Fill (0,zSize/2) to (xSize/2 - 1,zSize-1) with null tribe flags
		for(int x=0; x<xSize/2; x++) {
			for(int z=zSize/2; z<zSize; z++) {
				worldTileInfo[x,z].tribeTerritory.hasFlag = true;
			}
		}
	}
}
