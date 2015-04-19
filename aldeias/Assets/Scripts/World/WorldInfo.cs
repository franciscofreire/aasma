using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WorldInfo : MonoBehaviour {
	public const int TRIBE_TERRITORY_SIZE = 15;
	public const int HABITAT_SIZE = 7;
	public const int MEETING_POINT_WIDTH = 3;

	private const int NUM_PARTITIONS = 5;
	private const int UPDATE_FRAME_INTERVAL = 5;
	
	public System.Random rnd = new System.Random(); 

	// The size of the world in rows and columns.
	public int xSize = 50;
	public int zSize = 50;
	
	// The tiles of the world.
	private WorldTileInfo[,] worldTileInfo; 
	
	// All the tribes that exist in the world.
	public List<Tribe> tribes = new List<Tribe>(); 
	
	// All the habitats that exist in the world.
	public List<Habitat> habitats = new List<Habitat>();
	
	// All the agents that exist in the world.
	public List<Agent> allAgents = new List<Agent>();
	
	public List<AgentControl> agentsThreads = new List<AgentControl>();

	// Queue of actions: to be used by the agents
	public ConcurrentQueue<Action> pendingActionsQueue =
		new ConcurrentQueue<Action>();

	public int frameCount = 0;

	public class MeetingPoint {
		public Vector2 centralPoint;
		public int width;

		public MeetingPoint(Vector2 centralPoint, int width) {
			this.centralPoint = centralPoint;
			this.width = width;
		}
	}

	public static Tribe nullTribe = new Tribe();
	public class Tribe {
		//Insert tribe identification here
		public string id = "";
		public MeetingPoint meetingPoint = null;
		public List<Habitant> habitants = new List<Habitant>();
		
		public Tribe(string id, Vector2 centralPoint, int width) {
			this.id = id;
			this.meetingPoint = new MeetingPoint(centralPoint, width);
		}
		
		public Tribe() {
		}
	}
	public void addAgentToTribe(Tribe t, Habitant h) {
		t.habitants.Add(h);
		allAgents.Add(h);
		h.worldInfo = this;//FIXME this is not the right place to set this
	}
	
	public class Habitat {
		public Vector2 corner_pos;
		public List<Animal> animals = new List<Animal>();
		
		public Habitat(int x, int y) {
			this.corner_pos = new Vector2(x, y);
		}
		
		public Habitat() {
		}
	}
	public void addAgentToHabitat(Habitat h, Animal a) {
		h.animals.Add(a);
		allAgents.Add(a);
		a.worldInfo = this;//FIXME not the right place to set this
	}

	//Information being holded in every tile
	//The information contained in every tile is readonly.
	public class WorldTileInfo {
		public bool hasAgent  = false;
		public bool isHabitat = false;
		
		public Tree tree = new Tree(100);

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

	public void Update() {
		if(frameCount == 0) {
			WorldTick();
		}
		frameCount = (frameCount + 1) % UPDATE_FRAME_INTERVAL;
		/*
		if(Input.GetKeyUp("m")) {
			NotifyChangeListeners();
		}
		*/
	}

	public void WorldTick () {

		foreach(Agent a in allAgents) {
			a.OnWorldTick();
		}
		NotifyChangeListeners();

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
		NotifyCreationListeners();
		NotifyChangeListeners();
	}

	////
	//// TILE CREATION
	////

	public void GenerateWorldTileInfo () {
		CreateTiles();
		FillTribeTerritory("A", 0, 0);
		FillTribeTerritory("B", xSize - TRIBE_TERRITORY_SIZE, zSize - TRIBE_TERRITORY_SIZE);
		FillHabitat();
		FillTrees();
	}

	private void CreateTiles() {
		worldTileInfo = new WorldTileInfo[xSize,zSize];
		for(int x=0; x<xSize; x++) {
			for(int z=0; z<zSize; z++) {
				worldTileInfo[x,z] = new WorldTileInfo();
			}
		}
	}

	public bool isFreePartition(int x_start, int x_partition, int z_start, int z_partition) {
		for(int x2 = x_start; x2 < x_start + x_partition; x2++) {
			for(int z2 = z_start; z2 < z_start + z_partition; z2++) {
				if (worldTileInfo[x2, z2].isHabitat ||
				    worldTileInfo[x2, z2].tribeTerritory.hasFlag) {
					return false;
				}
			}
		}
		return true;
	}
	
	public void FillTrees () {
		// Fill partitions with trees
		int x_partition = xSize / NUM_PARTITIONS;
		int z_partition = zSize / NUM_PARTITIONS;
		int num_max_trees = x_partition * z_partition;

		for(int x = 0; x < NUM_PARTITIONS; x++) {
			// Choose which partition will have trees
			int partition_with_trees = Random.Range(0,NUM_PARTITIONS);
			for(int z = 0; z < NUM_PARTITIONS; z++) {
				
				// Is this a free partition with trees?
				int x_start = x * x_partition;
				int z_start = z * z_partition;
				if (z == partition_with_trees &&
				    isFreePartition(x_start, x_partition, z_start, z_partition)) {

					// How many trees?
					int num_trees = Random.Range(num_max_trees / 2, num_max_trees);

					// Now bind the trees to the cells
					for(int x2 = 0; x2 < x_partition; x2++) {
						for(int z2 = 0; z2 < x_partition; z2++) {
							if (num_trees-- > 0) {
								worldTileInfo[x_start + x2, z_start + z2].tree.hasTree = true;
							} else {
								break;
							}
						}
					}
				}
			}
		}
	}

	void FillTribeTerritory(string name, int posx, int posz) {
		float meetingPointx = posx + Mathf.Floor(TRIBE_TERRITORY_SIZE/2);
		float meetingPointz = posz + Mathf.Floor(TRIBE_TERRITORY_SIZE/2);
		Vector2 centralMeetingPoint = new Vector2(meetingPointx, meetingPointz);
		Tribe tribe = new Tribe(name, centralMeetingPoint, MEETING_POINT_WIDTH);
		tribes.Add(tribe);

		for(int x=posx; x < posx + TRIBE_TERRITORY_SIZE; x++) {
			for(int z=posz; z < posz + TRIBE_TERRITORY_SIZE; z++) {
				worldTileInfo[x,z].tribeTerritory.hasFlag = true;
				worldTileInfo[x,z].tribeTerritory.ownerTribe = tribe;
			}
		}
	}

	void FillHabitat () {
		int posx = 0;
		int posz = zSize - 1;
		habitats.Add(new Habitat(posx, posz));
		for(int x=posx; x < posx + HABITAT_SIZE; x++) {
			for(int z=posz; z > posz - HABITAT_SIZE; z--) {
				worldTileInfo[x,z].isHabitat = true;
			}
		}
	}

	void SetPerlinNoiseTreesWorldTileInfo() {
		for(int x=0; x<xSize; x++) {
			for(int z=0; z<zSize; z++) {
				if(Mathf.PerlinNoise(x*0.1f,z*0.1f) > 0.5) {
					worldTileInfo[x,z].tree.hasTree = true;
				}
			}
		}
	}

	void SetCornerToHabitat() {
		//Fill (0,0) to (xsize/2 - 1,zSize/2 - 1) with animal habitat
		for(int x=0; x<xSize/2; x++) {
			for(int z=0; z<zSize/2; z++) {
				worldTileInfo[x,z].isHabitat = true;
			}
		}
	}

	////
	//// TILE INFORMATION
	////

	public WorldTileInfo WorldTileInfoAtCoord(Vector2I tileCoord) {
		return worldTileInfo[tileCoord.x, tileCoord.y];
	}

	public WorldTileInfo WorldTileInfoAtCoord(int x, int z) {
		return worldTileInfo[x, z];
	}

	public IList<Vector2I> nearbyCells(Agent agent) {

		Vector2I agentPos = AgentPosToWorldXZ(agent.pos);
		int xmin = Mathf.Max(0, agentPos.x - 2);
		int xmax = Mathf.Min(xSize - 1, agentPos.x + 2);
		int zmin = Mathf.Max(0, agentPos.y - 2);
		int zmax = Mathf.Min(zSize - 1, agentPos.y + 2); 

		IList<Vector2I> cells = new List<Vector2I>();
		for (int x = xmin; x <= xmax; x++) {
			for (int z = zmin; z <= zmax; z++) {
				Vector2I cellCoord = new Vector2I(x,z);
				if (!Vector2I.Equal(cellCoord, agentPos)) {
					cells.Add(cellCoord);
					//Debug.Log("Agent nearby cells at " + Time.realtimeSinceStartup + " x: " + x + " z: " + z);
				}
			}
		}
		
		return cells;
	}

	public IList<Vector2I> nearbyFreeCells(IList<Vector2I> cells) {
		IList<Vector2I> freeCells = new List<Vector2I>();

		foreach (Vector2I pos in cells) {
			if (isFreeCell(pos))
				freeCells.Add (pos);
		}
		
		return freeCells;
	}

	public bool isInsideWorld(Vector2I coord) {
		return coord.x >= 0 && coord.y >= 0 && coord.x < xSize && coord.y < zSize;
	}

	public bool isFreeCell(Vector2I tileCoord) {
		return isInsideWorld(tileCoord) &&
			!worldTileInfo[tileCoord.x, tileCoord.y].tree.hasTree;
	}

    public bool hasTree(Vector2I tileCoord) {
        return isInsideWorld(tileCoord) &&
            worldTileInfo[tileCoord.x, tileCoord.y].tree.hasTree;
    }

	public bool AgentPosInTile(Vector2 agentPos, Vector2I tileCoord) {
		//Assuming pos (0,0) is in the center of the tile (0,0)
		Vector2I agentTileCoord = AgentPosToWorldXZ(agentPos);
		return Vector2I.Equal(agentTileCoord, tileCoord);
	}

	public Habitant habitantInTile(Vector2I tileCoord) {
		foreach(Tribe t in tribes) {
			foreach(Habitant h in t.habitants) {
				if(AgentPosInTile(h.pos, tileCoord)){
					return h;
				}
			}
		}
		return null;
	}

	public Vector2I AgentPosToWorldXZ(Vector2 pos) {
		return new Vector2I((int)(pos.x+0.5f), (int)(pos.y+0.5f));
	}

	public Vector2 WorldXZToAgentPos(Vector2I coord) {
		return new Vector2(coord.x, coord.y);
	}

    public bool isUnclaimedTerritory(Vector2I coord) {
        WorldTileInfo worldTileInfoCell = worldTileInfo[coord.x, coord.y];
        return worldTileInfoCell.tribeTerritory.Equals(nullTribe);
    }

	////
	//// LISTENERS
	////

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

	public delegate void WorldCreationListener();
	private List<WorldCreationListener> creationListeners = new List<WorldCreationListener>();
	public void AddCreationListener(WorldCreationListener func) {
		creationListeners.Add(func);
	}
	private void NotifyCreationListeners() {
		foreach(WorldCreationListener listener in creationListeners) {
			listener();
		}
	}
}
