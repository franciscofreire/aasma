using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WorldInfo : MonoBehaviour {

	private const int NUM_PARTITIONS = 5;

	public static Tribe nullTribe=new Tribe();
	public class Tribe {
		//Insert tribe identification here
	}

	//Classes and values that have to be declared and defined before the actual WorldTileInfo declaration
	public partial class WorldTileInfo {
		public static TribeTerritory defaultTribeTerritory=new TribeTerritory();
		public class TribeTerritory {
			public bool hasFlag=false;
			public Tribe ownerTribe=nullTribe;
		}
	}

	//Information being holded in every tile
	//The information contained in every tile is readonly.
	public partial class WorldTileInfo {
		public bool hasTree=false;
		public bool hasStump=false;
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
		SetPartitionTreeWorldTileInfo ();
		NotifyChangeListeners ();
	}

	public void GenerateWorldTileInfo () {
		worldTileInfo = new WorldTileInfo[xSize,zSize];
		FillWithDefaultWorldTileInfo ();
	}

	public void FillWithDefaultWorldTileInfo () {
		for(int x=0; x<xSize; x++) {
			for(int z=0; z<zSize; z++) {
				worldTileInfo[x,z] = new WorldTileInfo();
			}
		}
	}

	#region WorldTileInfo initialization
	public void SetPartitionTreeWorldTileInfo () {

		//Fill (0,0) to (xsize/2 - 1,zSize/2 - 1) with animal habitat
		for(int x=0; x<xSize/2; x++) {
			for(int z=0; z<zSize/2; z++) {
				worldTileInfo[x,z].isHabitat = true;
			}
		}

		// Fill partitions with trees
		int x_partition = xSize / NUM_PARTITIONS;
		int z_partition = zSize / NUM_PARTITIONS;
		int num_max_trees = x_partition * z_partition;

		// Choose which partitions will have trees
		/*
		List<int> numbers = new List<int>(NUM_PARTITIONS * NUM_PARTITIONS);
		for (int i = 0; i < NUM_PARTITIONS * NUM_PARTITIONS; i++) {
			numbers.Add(i);
		}
		int[] randomNumbers = new int[5];
		for (int i = 0; i < randomNumbers.Length; i++) {
			int thisNumber = Random.Range(0, numbers.Count);
			randomNumbers[i] = numbers[thisNumber];
			numbers.RemoveAt(thisNumber);
		}

		int randomNumbersIndex = 0;
		*/
		for(int x = 0; x < NUM_PARTITIONS; x++) {
			for(int z = 0; z < NUM_PARTITIONS; z++) {
				
				// Is this a partition with trees?
				// WARNING: Random gives strange results, I dunno why
				if (x + z < 3 && x % 2 == 0) {

					// How many trees?
					int num_trees = Random.Range(num_max_trees / 2, num_max_trees);

					// Now bind the trees to the cells
					int x_start = x * x_partition;
					int z_start = z * z_partition;
					for(int x2 = 0; x2 < x_partition; x2++) {
						for(int z2 = 0; z2 < x_partition; z2++) {
							if (num_trees-- > 0) {
								worldTileInfo[x_start + x2, z_start + z2].hasTree = true;
							} else {
								break;
							}
						}
					}
				}
			}
		}

		//Fill (0,zSize/2) to (xSize/2 - 1,zSize-1) with null tribe flags
		for(int x=0; x<xSize/2; x++) {
			for(int z=zSize/2; z<zSize; z++) {
				worldTileInfo[x,z].tribeTerritory.hasFlag = true;
			}
		}
	}
	public void SetDebugWorldTileInfo () {
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
	#endregion

	public delegate void WorldChangeListener();
	private List<WorldChangeListener> changeListeners = new List<WorldChangeListener>();
	public void AddChangeListener(WorldChangeListener func) {
		changeListeners.Add(func);
	}
	private void NotifyChangeListeners() {
		foreach(WorldChangeListener listener in changeListeners) {
			listener();
		}
	}
}
