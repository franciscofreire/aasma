using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class WorldInfo : MonoBehaviour {

	private const int UPDATE_FRAME_INTERVAL = 2;
	public int MilisecondsPerTick = 50;

	// The size of the world in rows and columns.
	public int xSize = 50;
	public int zSize = 50;
	
	// The tiles of the world.
	public WorldTiles worldTiles; 
	
	// All the tribes that exist in the world.
	public List<Tribe> tribes = new List<Tribe>(); 
	
	// All the habitats that exist in the world.
	public List<Habitat> habitats = new List<Habitat>();

	public IEnumerable<Habitant> AllHabitants {
		get {
			return tribes                                  //List of Tribes
				.ConvertAll(t=>t.habitants.AsEnumerable()) //Lists of Habitants
					.Aggregate((hs1,hs2)=>hs1.Concat(hs2));//List of Habitants
		}
	}

	public IEnumerable<Animal> AllAnimals {
		get {
			return habitats                                //List of Habitats
				.ConvertAll(h=>h.animals.AsEnumerable())   //Lists of Animals
					.Aggregate((as1,as2)=>as1.Concat(as2));//List of Animals
		}
	}

	// All the agents that exist in the world.
	public IEnumerable<Agent> AllAgents {
		get {
			return AllHabitants.Cast<Agent>().Concat(AllAnimals.Cast<Agent>());
		}
	}

	public List<Tree> AllTrees = new List<Tree>();
	public void AddTree(Tree tree) {
		AllTrees.Add(tree);
		// Add tree to the optimized Tree at tile structure.
		worldTiles.WorldTileInfoAtCoord(tree.Pos).Tree = tree;
	}

	public class Habitat {
		public Vector2I corner_pos;
		public List<Animal> animals = new List<Animal>();
		
		public Habitat(int x, int y) {
			this.corner_pos = new Vector2I(x, y);
		}
		
		public Habitat() {
		}
	}

	void Start () {
		GenerateWorldTileInfo();
		NotifyCreationListeners();
		NotifyChangeListeners();
		StartCoroutine(NextWorldTick());
	}

	IEnumerator NextWorldTick() {
		while(true) {
			yield return new WaitForSeconds(MilisecondsPerTick/1000f);
			WorldTick();
		}
	}

	public void WorldTick () {
		foreach(Agent a in AllAgents) {
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

	////
	//// TILE CREATION
	////

	public const int TRIBE_TERRITORY_SIDE = 15;
	public const int HABITAT_SIDE = 7;
	public const int MEETING_POINT_SIDE = 5;
	private const int NUM_PARTITIONS = 5;
	public void GenerateWorldTileInfo () {
		worldTiles = new WorldTiles(xSize, zSize);
		CreateTribeAt("A", 0, 0);
		CreateTribeAt("B", xSize - TRIBE_TERRITORY_SIDE, zSize - TRIBE_TERRITORY_SIDE);
		FillHabitat();
		CreateAnimals();
		FillTrees();
	}

	private bool isFreePartition(int x_start, int x_partition, int z_start, int z_partition) {
		for(int x2 = x_start; x2 < x_start + x_partition; x2++) {
			for(int z2 = z_start; z2 < z_start + z_partition; z2++) {
				WorldTileInfo tile = worldTiles.WorldTileInfoAtCoord(x2, z2);
				if (tile.isHabitat ||
				    tile.tribeTerritory.IsClaimed) {
					return false;
				}
			}
		}
		return true;
	}
	
	private void FillTrees () {
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
								AddTree(new Tree(
                                    this,
                                    new Vector2I(x_start + x2, z_start + z2),
                                    new WoodQuantity(100)));
							} else {
								break;
							}
                        }
                    }
                }
            }
        }
    }

	private void CreateTribeAt(string name, int posx, int posz) {
		Tribe tribe = CreateTribe(name, posx, posz);
		FillTribeTerritory(tribe, posx, posz);
		CreateTribeHabitants(tribe);
	}

	private Tribe CreateTribe(string name, int posx, int posz) {
		int meetingPointx = posx + Mathf.FloorToInt(TRIBE_TERRITORY_SIDE/2);
		int meetingPointz = posz + Mathf.FloorToInt(TRIBE_TERRITORY_SIDE/2);
		Vector2I meetingPointCenter = new Vector2I(meetingPointx, meetingPointz);
		MeetingPoint meetingPoint = new MeetingPoint(meetingPointCenter, MEETING_POINT_SIDE);
		Tribe tribe = new Tribe(name, meetingPoint);
		tribes.Add(tribe);
		return tribe;
	}

    private void FillTribeTerritory(Tribe tribe, int posx, int posz) {
        for(int x=posx; x < posx + TRIBE_TERRITORY_SIDE; x++) {
            for(int z=posz; z < posz + TRIBE_TERRITORY_SIDE; z++) {
				WorldTileInfo tile = worldTiles.WorldTileInfoAtCoord(x,z);
                tile.tribeTerritory.OwnerTribe = tribe;
            }
        }
    }

	private void CreateTribeHabitants(Tribe tribe) {
		//Create four Habitants of the given Tribe.
		//The Habitants should be created inside the Tribe's meeting point.
		foreach( var coord in tribe.meetingPoint.MeetingPointTileCoords.Take(20)) {
			Vector2 pos = CoordConvertions.WorldXZToAgentPos(coord);
			Habitant h = new Habitant(this, pos, tribe, 1);
			tribe.AddHabitant(h);
		}
	}

	private void FillHabitat () {
		int posx = 0;
		int posz = zSize - 1;
		habitats.Add(new Habitat(posx, posz));
		for(int x=posx; x < posx + HABITAT_SIDE; x++) {
			for(int z=posz; z > posz - HABITAT_SIDE; z--) {
				worldTiles.WorldTileInfoAtCoord(x,z).isHabitat = true;
			}
		}
	}

	private void CreateAnimals() {
		foreach (WorldInfo.Habitat h in habitats) {
			int num_animals = 40;
			for(int x = h.corner_pos.x; x < h.corner_pos.x + HABITAT_SIDE; x++) {
				for(int z = h.corner_pos.y; z > h.corner_pos.y - HABITAT_SIDE; z--) {
					if (num_animals-- > 0) {
						Vector2I tileCoord = new Vector2I(x,z);
						Animal a = new Animal(
                            this,
                            CoordConvertions.WorldXZToAgentPos(tileCoord),
                            new FoodQuantity(100));
						worldTiles.WorldTileInfoAtCoord(tileCoord).Agent = a;
						h.animals.Add(a);
					} else
						break;
				}
			}
		}
	}

	private void SetPerlinNoiseTreesWorldTileInfo() {
		for(int x=0; x<xSize; x++) {
			for(int z=0; z<zSize; z++) {
				if(Mathf.PerlinNoise(x*0.1f,z*0.1f) > 0.5) {
					AddTree(new Tree(
                        this,
                        new Vector2I(x,z),
                        new WoodQuantity(100)));
				}
			}
		}
	}

	////
	//// TILE INFORMATION
	////

    public IList<Vector2I> nearbyCells(Agent agent) {
        int height = 4;
        int width = 3;

        int xMaxSize;
        int zMaxSize;

		Vector2I agentPos = CoordConvertions.AgentPosToWorldXZ(agent.pos);
        Vector2I leftCorner;

        IList<Vector2I> cells = new List<Vector2I>();

        if(agent.orientation == Orientation.Up) {
            leftCorner = new Vector2I(agentPos.x - 1, agentPos.y + (height-1));
            xMaxSize = width;
            zMaxSize = height;

        } else if(agent.orientation == Orientation.Down) {
            leftCorner = new Vector2I(agentPos.x - 1, agentPos.y);
            xMaxSize = width;
            zMaxSize = height;

        } else if(agent.orientation == Orientation.Left) {
            leftCorner = new Vector2I(agentPos.x - (height-1), agentPos.y + 1);
            xMaxSize = height;
            zMaxSize = width;

        } else { // RIGHT
            leftCorner = new Vector2I(agentPos.x, agentPos.y + 1);
            xMaxSize = height;
            zMaxSize = width;

        }

        for(int i = 0; i < xMaxSize; i++) {
            for(int j = 0; j < zMaxSize; j++) {
                Vector2I cell = new Vector2I(leftCorner.x + i, leftCorner.y - j);
                if(isInsideWorld(cell)) {
                    cells.Add(cell);
                }
            }
        }

		return cells;
	}

	public IList<Vector2I> nearbyFreeCells(IList<Vector2I> cells) {
		IList<Vector2I> freeCells = new List<Vector2I>();
		foreach (Vector2I pos in cells) {
			if (worldTiles.WorldTileInfoAtCoord(pos).IsEmpty)
				freeCells.Add (pos);
		}
		
		return freeCells;
	}

	public bool isInsideWorld(Vector2I coord) {
		return coord.x >= 0 && coord.y >= 0 && coord.x < xSize && coord.y < zSize;
	}


	public bool AgentPosInTile(Vector2 agentPos, Vector2I tileCoord) {
		//Assuming pos (0,0) is in the center of the tile (0,0)
		Vector2I agentTileCoord = CoordConvertions.AgentPosToWorldXZ(agentPos);
		return agentTileCoord == tileCoord;
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

    public Animal animalInTile(Vector2I tileCoord) {
        foreach(Habitat h in habitats) {
            foreach(Animal a in h.animals) {
                if (AgentPosInTile(a.pos, tileCoord)) {
                    return a;
                }
            }
        }
        return null;
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
    
    public delegate void TreeDiedListener(Vector2I pos);
    private List<TreeDiedListener> treeListeners = new List<TreeDiedListener>();
    public void AddTreeDiedListener(TreeDiedListener func) {
        treeListeners.Add(func);
    }
    public void NotifyTreeDiedListeners(Vector2I pos) {
        foreach(TreeDiedListener listener in treeListeners) {
            listener(pos);
        }
    }
    
    public delegate void AgentDiedListener(Vector2I pos);
    private List<AgentDiedListener> agentListeners = new List<AgentDiedListener>();
    public void AddAgentDiedListener(AgentDiedListener func) {
        agentListeners.Add(func);
    }
    public void NotifyAgentDiedListeners(Vector2I pos) {
        foreach(AgentDiedListener listener in agentListeners) {
            listener(pos);
        }
    }
}

public class WorldTiles {
	private WorldTileInfo[,] worldTileInfo; 
	public WorldTiles(int xSize, int zSize) {
		worldTileInfo = new WorldTileInfo[xSize,zSize];
		for(int x=0; x<xSize; x++) {
			for(int z=0; z<zSize; z++) {
				worldTileInfo[x,z] = new WorldTileInfo();
			}
		}
	}
	public WorldTileInfo WorldTileInfoAtCoord(Vector2I tileCoord) {
		return worldTileInfo[tileCoord.x, tileCoord.y];
	}
	public WorldTileInfo WorldTileInfoAtCoord(int x, int z) {
		return worldTileInfo[x, z];
    }         
}

//Information being holded in every tile
//The information contained in every tile is readonly.
public class WorldTileInfo {
	public Agent Agent  = null;
	public bool HasAgent {
		get {
			return Agent != null;
		}
	}

	public bool isHabitat = false;
	
	public Tree Tree = null;
	public bool HasTree {
		get { return Tree != null; }
	}

	// Can an agent go here?
	public bool IsEmpty {
		get {
			return !HasTree && ! HasAgent;
		}
	}

	public struct TribeTerritory {
		public Tribe OwnerTribe;

		public bool IsClaimed {
			get {
				return OwnerTribe != null;
			}
		}

		public static implicit operator TribeTerritory(Tribe tribe) {
			return new TribeTerritory() {
				OwnerTribe = tribe
			};
		}
	}
	public TribeTerritory tribeTerritory = new TribeTerritory();

}

public static class WorldRandom {
	private static System.Random rnd = new System.Random(); 
	public static int Next(int max) {
		return rnd.Next(max);
	}
}

public static class CoordConvertions {
	public static Vector2I AgentPosToWorldXZ(Vector2 pos) {
		return new Vector2I((int)(pos.x+0.5f), (int)(pos.y+0.5f));
	}
	
	public static Vector2 WorldXZToAgentPos(Vector2I coord) {
		return new Vector2(coord.x, coord.y);
	}
	public static Vector2 ClampAgentPosToWorldSize(Vector2 pos, WorldInfo world) {
		Vector2 maxWorldCoord = new Vector2(world.xSize-1,world.zSize-1);
		Vector2 minWorldCoord = Vector2.zero;
		return Vector2.Max(minWorldCoord, Vector2.Min(maxWorldCoord, pos));
	}
}
