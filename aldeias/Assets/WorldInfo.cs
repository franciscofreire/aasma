﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WorldInfo : MonoBehaviour {

	private const int NUM_PARTITIONS = 5;
	public const int TRIBE_TERRITORY_SIZE = 15;
	public const int HABITAT_SIZE = 7;

	public static Tribe nullTribe = new Tribe();
	public class Tribe {
		//Insert tribe identification here
		public string id;
		public Vector2 meetingPoint;
		
		public Tribe(string id) {
			this.id = id;
		}
		
		public Tribe() {
			this.id = "";
		}
	}

	//Information being holded in every tile
	//The information contained in every tile is readonly.
	public partial class WorldTileInfo {
		public bool hasTree=false;
		public bool hasStump=false;
		public bool isHabitat=false;
		public struct TribeTerritory {
			public bool hasFlag;
		    public Tribe ownerTribe;
			
			public static implicit operator TribeTerritory(Tribe tribe) {
				return new TribeTerritory() {
					hasFlag = false,
					ownerTribe = tribe
				};
			}
		}
		public TribeTerritory tribeTerritory = nullTribe;
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

	public void placeObject(GameObject obj, Vector2 pos) {
		int posx = (int) pos.x;
		int posz = (int) pos.y;
		
		// TODO: Test limits
	}

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
		GenerateWorldTileInfo();
		SetTreesWorldTileInfo();
		NotifyChangeListeners();
	}

	public void GenerateWorldTileInfo () {
		worldTileInfo = new WorldTileInfo[xSize,zSize];
		FillWithDefaultWorldTileInfo();
		FillTribeATerritory();
		FillTribeBTerritory();
		FillHabitat();
	}

	public void FillWithDefaultWorldTileInfo () {
		for(int x=0; x<xSize; x++) {
			for(int z=0; z<zSize; z++) {
				worldTileInfo[x,z] = new WorldTileInfo();
			}
		}
	}
	
	public void SetTreesWorldTileInfo () {

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
		for(int x = 0; x < NUM_PARTITIONS; x++) {
			int partition_with_trees = Random.Range(0,NUM_PARTITIONS);
			for(int z = 0; z < NUM_PARTITIONS; z++) {
				
				// Is this a partition with trees?
				if (z == partition_with_trees) {

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
	}

	void FillTribeATerritory () {
		int posx = 0;
		int posz = 0;
		Tribe tribeA = new Tribe("A");
		float meetingPointx = posx + Mathf.Floor(TRIBE_TERRITORY_SIZE/2);
		float meetingPointz = posz + Mathf.Floor(TRIBE_TERRITORY_SIZE/2);
		tribeA.meetingPoint = new Vector2(meetingPointx, meetingPointz);
		for(int x=posx; x < posx + TRIBE_TERRITORY_SIZE; x++) {
			for(int z=posz; z < posz + TRIBE_TERRITORY_SIZE; z++) {
				worldTileInfo[x,z].tribeTerritory.hasFlag = true;
				worldTileInfo[x,z].tribeTerritory.ownerTribe = tribeA;
			}
		}
	}

	void FillTribeBTerritory () {
		int posx = xSize - 1;
		int posz = zSize - 1;
		Tribe tribeB = new Tribe ("B");
		float meetingPointx = posx - Mathf.Floor (TRIBE_TERRITORY_SIZE / 2);
		float meetingPointz = posz - Mathf.Floor (TRIBE_TERRITORY_SIZE / 2);
		tribeB.meetingPoint = new Vector2 (meetingPointx, meetingPointz);
		
		for (int x=posx; x > posx - TRIBE_TERRITORY_SIZE; x--) {
			for (int z=posz; z > posz - TRIBE_TERRITORY_SIZE; z--) {
				worldTileInfo [x, z].tribeTerritory.hasFlag = true;
				worldTileInfo [x, z].tribeTerritory.ownerTribe = tribeB;
			}
		}
	}

	void FillHabitat () {
		int posx = 0;
		int posz = zSize - 1;
		for(int x=posx; x < posx + HABITAT_SIZE; x++) {
			for(int z=posz; z > posz - HABITAT_SIZE; z--) {
				worldTileInfo[x,z].isHabitat = true;
			}
		}
	}

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
