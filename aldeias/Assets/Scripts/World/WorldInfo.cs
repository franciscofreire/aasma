using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class WorldInfo : MonoBehaviour {
    // Agent Properties
	private const int UPDATE_FRAME_INTERVAL = 2;
	public int MilisecondsPerTick = 50;

    // Animal Properties
    private const int MAX_ANIMALS = 10;
    private const int ANIMAL_RESPAWN_COOLDOWN = 50;
    private int cooldown = ANIMAL_RESPAWN_COOLDOWN;
    public AnimalBoidParameters BoidParams = new AnimalBoidParameters();
    public bool AnimalMovement = true;

    // Habitant Properties
    private const int MAX_HABITANTS = 20;
    public bool Reactive = true;

	// The size of the world in rows and columns.
	public int xSize = 50;
	public int zSize = 50;
    public Vector2I Size {
        get {
            return new Vector2I(xSize,zSize);
        }
    }
	
	// The tiles of the world.
	public WorldTiles worldTiles; 
	
	// All the tribes that exist in the world.
	public List<Tribe> tribes = new List<Tribe>(); 
	
	// All the habitats that exist in the world.
	public List<Habitat> habitats = new List<Habitat>();

	public IEnumerable<Habitant> AllHabitants {
		get {
			return tribes                                  // List  of Tribes
				.ConvertAll(t=>t.habitants.AsEnumerable()) // Lists of Habitants
				.Aggregate((hs1,hs2)=>hs1.Concat(hs2));    // List  of Habitants
		}
    }
    public IEnumerable<Habitant> AliveHabitants {
        get {
            return AllHabitants.Where((h)=>h.Alive);
        }
    }
    public Tribe GetEnemyTribe(Tribe tribe) {
        foreach(Tribe t in tribes) {
            if(t != tribe) {
                return t;
            }
        }
        return null;
    }

	public IEnumerable<Animal> AllAnimals {
		get {
			return habitats                              // List  of Habitats
				.ConvertAll(h=>h.animals.AsEnumerable()) // Lists of Animals
				.Aggregate((as1,as2)=>as1.Concat(as2));  // List  of Animals
		}
	}
    public IEnumerable<Animal> AliveAnimals {
        get {
            return AllAnimals.Where((a)=>a.Alive);
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

    public IEnumerable<Vector2I> AllCellCoords {
        get {
            foreach(var x in Enumerable.Range(0,xSize)) {
                foreach(var z in Enumerable.Range(0,zSize)) {
                    yield return new Vector2I(x,z);
                }
            }
        }
    }

    public IEnumerable<Vector2I> HabitatCellCoords {
        get {
            return AllCellCoords.Where(
                (c)=>
                worldTiles.WorldTileInfoAtCoord(c).isHabitat);
        }
    }

	void Start () {
		GenerateWorldTileInfo();
		NotifyCreationListeners();
		StartCoroutine(NextWorldTick());
	}

	IEnumerator NextWorldTick() {
		while(true) {
			yield return new WaitForSeconds(MilisecondsPerTick/1000f);
            
            // Check if the game has ended
            if (erradicationVictory() || territoryVictory()) {
                yield break;
            }

			WorldTick();
		}
	}

    public void WorldTick () {
        foreach(Agent a in AllAgents) {
            a.OnWorldTick();
        }
        EnsureEnoughAliveAnimals();
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

    public void EnsureEnoughAliveAnimals() {
        int AliveAnimalDeficit = MAX_ANIMALS - AliveAnimals.Count();
        if (AliveAnimalDeficit > 0) {
            if (cooldown-- == 0) {
                foreach(var habitatTile in HabitatCellCoords.Take(AliveAnimalDeficit))
                    CreateAnimalAt(habitatTile, habitats.First());
                cooldown = ANIMAL_RESPAWN_COOLDOWN;
            }
        }
    }

	////
	//// TILE CREATION
	////

    private const int TRIBE_TERRITORY_SIDE = 15;
    private const int MEETING_POINT_SIDE   = 5;
    private const int HABITAT_SIDE         = 7;
	private const int NUM_PARTITIONS       = 10;

	public void GenerateWorldTileInfo () {
		worldTiles = new WorldTiles(xSize, zSize);
		CreateTribeAt("Blue", 0, 0);
		CreateTribeAt("Red", xSize - TRIBE_TERRITORY_SIDE, zSize - TRIBE_TERRITORY_SIDE);
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
		int meetingPointx = posx + TRIBE_TERRITORY_SIDE/2;
		int meetingPointz = posz + TRIBE_TERRITORY_SIDE/2;
		Vector2I meetingPointCenter = new Vector2I(meetingPointx, meetingPointz);
		MeetingPoint meetingPoint = new MeetingPoint(meetingPointCenter, MEETING_POINT_SIDE);
        Tribe tribe = new Tribe(name, meetingPoint, TRIBE_TERRITORY_SIDE, posx, posz);
		tribes.Add(tribe);
		return tribe;
	}

    private void FillTribeTerritory(Tribe tribe, int posx, int posz) {
        for(int x=posx; x < posx + TRIBE_TERRITORY_SIDE; x++) {
            for(int z=posz; z < posz + TRIBE_TERRITORY_SIDE; z++) {
				WorldTileInfo tile = worldTiles.WorldTileInfoAtCoord(x,z);
                tile.tribeTerritory = tribe;
            }
        }
    }

	private void CreateTribeHabitants(Tribe tribe) {
		//Create MAX_HABITANTS Habitants of the given Tribe.
		//The Habitants should be created inside the Tribe's meeting point.
		foreach( var coord in tribe.meetingPoint.MeetingPointTileCoords.Take(MAX_HABITANTS)) {
			Vector2 pos = CoordConvertions.TileToAgentPos(coord);
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

    private void CreateAnimalAt(Vector2I coord, Habitat habitat) {
        Animal a = new Animal(this, 
                          CoordConvertions.TileToAgentPos(coord),
                          habitat,
                          new FoodQuantity(100));
        worldTiles.WorldTileInfoAtCoord(coord).Agent = a;
        habitat.animals.Add(a);
    }

	private void CreateAnimals() {
        foreach (Habitat h in habitats) {
            int num_animals = MAX_ANIMALS;
            for(int x = h.corner_pos.x; x < h.corner_pos.x + HABITAT_SIDE; x++) {
				for(int z = h.corner_pos.y; z > h.corner_pos.y - HABITAT_SIDE; z--) {
					if (num_animals-- > 0) {
						Vector2I tileCoord = new Vector2I(x,z);
                        CreateAnimalAt(tileCoord, h);
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

    public IList<Vector2I> nearbyCellsInfo(
            Habitant agent,
            out IList<Vector2I> _far_away_cells,
            out IList<Tree> _trees,
            out IList<Tree> _stumps,
            out IList<Habitant> _enemies,
            out IList<Animal> _animals,
            out IList<Animal> _food,
            out IList<Vector2I> _meeting_point_cells,
            out IList<Vector2I> _unclaimed_cells,
            out IList<KeyValuePair<Vector2I,Tribe>> _territories) {

        int height = 4;
        int width = 3;

        int xMaxSize;
        int zMaxSize;

		Vector2I agentPos = CoordConvertions.AgentPosToTile(agent.pos);
        Vector2I leftCorner;

        Tribe enemyTribe = GetEnemyTribe(((Habitant) agent).tribe);

        _trees = new List<Tree>();
        _stumps = new List<Tree>();
        _enemies = new List<Habitant>();
        _animals = new List<Animal>();
        _food = new List<Animal>();
        _far_away_cells = new List<Vector2I>();
        _meeting_point_cells = new List<Vector2I>();
        _unclaimed_cells = new List<Vector2I>();
        _territories = new List<KeyValuePair<Vector2I,Tribe>>();

        IList<Vector2I> cells = new List<Vector2I>();
        
        int width_delta  = (int) System.Math.Floor((double) width / 2); 
        if(agent.orientation == Orientation.Up) {
            leftCorner = new Vector2I(agentPos.x - width_delta, agentPos.y + (height-1));
            xMaxSize = width;
            zMaxSize = height;

        } else if(agent.orientation == Orientation.Down) {
            leftCorner = new Vector2I(agentPos.x - width_delta, agentPos.y);
            xMaxSize = width;
            zMaxSize = height;

        } else if(agent.orientation == Orientation.Left) {
            leftCorner = new Vector2I(agentPos.x - (height-1), agentPos.y + width_delta);
            xMaxSize = height;
            zMaxSize = width;

        } else { // RIGHT
            leftCorner = new Vector2I(agentPos.x, agentPos.y + width_delta);
            xMaxSize = height;
            zMaxSize = width;
        }

        for(int i = 0; i < xMaxSize; i++) {
            for(int j = 0; j < zMaxSize; j++) {
                Vector2I cell = new Vector2I(leftCorner.x + i, leftCorner.y - j);
                if(CoordConvertions.AgentPosToTile(agent.pos).DistanceTo(cell) >= 2) {
                    _far_away_cells.Add (cell);
                }
                if(isInsideWorld(cell)) {
                    cells.Add(cell);

                    if(agent.tribe.meetingPoint.IsInMeetingPoint(cell)) {
                        _meeting_point_cells.Add(cell);
                    }
                    if(!worldTiles.WorldTileInfoAtCoord(cell).tribeTerritory.IsClaimed) {
                        _unclaimed_cells.Add(cell);
                    }
                    if(worldTiles.WorldTileInfoAtCoord(cell).tribeTerritory.Flag.HasValue) {
                       if(worldTiles.WorldTileInfoAtCoord(cell).tribeTerritory.Flag.Value.Tribe.Equals(enemyTribe)) {
                            _territories.Add (new KeyValuePair<Vector2I,Tribe>(cell,enemyTribe));
                        }
                        else {
                            _territories.Add (new KeyValuePair<Vector2I,Tribe>(cell,agent.tribe));
                        }                   
                    }
                 
                    foreach(Habitant h in enemyTribe.habitants) {

                        if(h.Alive && CoordConvertions.AgentPosToTile(h.pos) == cell) {
                            _enemies.Add(h);
                        }
                    }
                    foreach(Habitat hab in habitats) {
                        foreach(Animal a in hab.animals) {
                            if(CoordConvertions.AgentPosToTile(a.pos) == cell) {
                                if(a.HasFood) {
                                    _food.Add (a);
                                } else {
                                    _animals.Add(a);
                                }
                            }
                        }
                    }
                    foreach(Tree t in AllTrees) {
                        if(t.Pos == cell) {
                            if(t.Alive) {
                                _trees.Add(t);
                            } else {
                                _stumps.Add(t);
                            }
                        }
                    }

                }
            }
        }

		return cells;
	}

    public Vector2I neighborCell(Vector2I cell) {
        Vector2I candidate = Vector2I.INVALID;

        candidate = new Vector2I(cell.x + 1, cell.y);
        if (isInsideWorld(candidate) && worldTiles.WorldTileInfoAtCoord(candidate).IsEmpty)
            return candidate;
        candidate = new Vector2I(cell.x - 1, cell.y);
        if (isInsideWorld(candidate) && worldTiles.WorldTileInfoAtCoord(candidate).IsEmpty)
            return candidate;
        candidate = new Vector2I(cell.x, cell.y + 1);
        if (isInsideWorld(candidate) && worldTiles.WorldTileInfoAtCoord(candidate).IsEmpty)
            return candidate;
        candidate = new Vector2I(cell.x, cell.y - 1);
        if (isInsideWorld(candidate) && worldTiles.WorldTileInfoAtCoord(candidate).IsEmpty)
            return candidate;

        return candidate;
    }

	public IList<Vector2I> nearbyFreeCells(IList<Vector2I> cells) {
		IList<Vector2I> freeCells = new List<Vector2I>();
		foreach (Vector2I pos in cells) {
			if (worldTiles.WorldTileInfoAtCoord(pos).IsEmpty)
				freeCells.Add (pos);
		}
		
		return freeCells;
	}

    public IList<Vector2I> nearbyFreeCellsInRadius(Vector2I pos, int radius) {
        IList<Vector2I> result = new List<Vector2I>();
        for (int i = -radius/2; i <= radius/2; i++) {
            for (int j = -radius/2; j <= radius/2; j++) {
                Vector2I candidate = new Vector2I(pos.x + i, pos.y + j);
                if (isInsideWorld(candidate) && worldTiles.WorldTileInfoAtCoord(candidate).IsEmpty)
                    result.Add(candidate);
            }
        }
        return result;
    }

	public bool isInsideWorld(Vector2I coord) {
		return coord.x >= 0 && coord.y >= 0 && coord.x < xSize && coord.y < zSize;
	}

	public bool AgentPosInTile(Vector2 agentPos, Vector2I tileCoord) {
		//Assuming pos (0,0) is in the center of the tile (0,0)
		Vector2I agentTileCoord = CoordConvertions.AgentPosToTile(agentPos);
		return agentTileCoord == tileCoord;
	}

	public Habitant habitantInTile(Vector2I tileCoord) {
		foreach(Tribe t in tribes) {
			foreach(Habitant h in t.habitants) {
				if(AgentPosInTile(h.pos, tileCoord) && h.Alive){
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

    private bool erradicationVictory() {
        int tribes_remaining = tribes.Count;
        string tribe_id = "";
        foreach (Tribe t in tribes) {
            if (t.habitants.Count == 0)
                tribes_remaining--;
            else {
                tribe_id = t.id;
            }
        }
        if (tribes_remaining == 1) {
            NotifyGameEndedListeners("Tribe " + tribe_id + " winned! (Erradication Victory)");
            return true;
        }
        return false;
    }
    
    private bool territoryVictory() {
        string tribe_id = "";
        foreach (Tribe t in tribes) {
            if (t.cell_count > ((float)(xSize * zSize) / 2))
                tribe_id = t.id;
        }
        if (tribe_id != "") {
            NotifyGameEndedListeners("Tribe " + tribe_id + " winned! (Territory Victory)");
            return true;
        }
        return false;
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
        try {
            return worldTileInfo[tileCoord.x, tileCoord.y];
        } catch (System.Exception) {
            Debug.Log ("WorldTileInfoAtCoord Exception: " + tileCoord.x + "," + tileCoord.y);
            return null;
        }
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
		public Flag? Flag;

		public bool IsClaimed {
			get {
				return Flag.HasValue;
			}
		}

		public static implicit operator TribeTerritory(Tribe tribe) {
			return new TribeTerritory() {
                Flag = new Flag(tribe)
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
	public static Vector2I AgentPosToTile(Vector2 pos) {
		return new Vector2I((int)(pos.x+0.5f), (int)(pos.y+0.5f));
	}
	
	public static Vector2 TileToAgentPos(Vector2I coord) {
		return new Vector2(coord.x, coord.y);
	}

    public static Vector2 AdjustAgentPos(Vector2 agentPos) {
        return TileToAgentPos(AgentPosToTile(agentPos));
    }
	public static Vector2 ClampAgentPosToWorldSize(Vector2 pos, WorldInfo world) {
		Vector2 maxWorldCoord = new Vector2(world.xSize-1,world.zSize-1);
		Vector2 minWorldCoord = Vector2.zero;
		return Vector2.Max(minWorldCoord, Vector2.Min(maxWorldCoord, pos));
	}
}
