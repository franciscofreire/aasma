using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public abstract class Action {
    // FIXME: Amounts will likely be changed
    protected const int FOOD_AMOUNT = 100;
    protected const int WOOD_AMOUNT = 100;

	protected Agent performer;
	protected Vector2I target;

	// Atributes for quick access
	protected IList<Vector2I> cells;
	protected WorldInfo world;

	public abstract void apply ();

	public Action(Agent agent, Vector2I target) {
		this.performer  = agent;
		this.target = target;	

		this.cells = agent.sensorData.Cells;
		this.world = agent.worldInfo;
	}
}

public class Walk : Action {
	public override void apply () {
		// Update worldtileInfo
		WorldInfo.WorldTileInfo agentTileInfo = 
			world.WorldTileInfoAtCoord(world.AgentPosToWorldXZ(performer.pos));
		WorldInfo.WorldTileInfo targetTileInfo = 
			world.WorldTileInfoAtCoord(target);
		agentTileInfo.hasAgent = false;
		targetTileInfo.hasAgent = true;
		
		// Orientation
		int x_origin = (int) performer.pos[0];
		int z_origin = (int) performer.pos[1];
		int x_target = (int) target.x;
		int z_target = (int) target.y;
		if (x_origin > x_target) {
			performer.orientation = ORIENTATION.LEFT;
		} else if (x_origin < x_target) {
			performer.orientation = ORIENTATION.RIGHT;
		} else if (z_origin > z_target) {
			performer.orientation = ORIENTATION.UP;
		} else {
			performer.orientation = ORIENTATION.DOWN;
		}
		
		// Position
		performer.pos = target.ToVector2();
	}
	public Walk(Agent agent, Vector2I target) : base(agent, target) {}
}

public class Attack : Action {
	public static readonly Energy ENERGY_TO_REMOVE = new Energy(20);
	public override void apply () {
        if(world.WorldTileInfoAtCoord(target).hasAgent) {
            foreach(Agent a in world.allAgents) {
                if(a.pos.Equals(target.ToVector2())) {
					a.RemoveEnergy(ENERGY_TO_REMOVE);
                }
            }
        }
    }
	public Attack(Agent agent, Vector2I target) : base(agent, target) {}
}

public class CutTree : Action {
	public override void apply () {
		if(world.WorldTileInfoAtCoord(target).tree.Alive) {
			world.WorldTileInfoAtCoord(target).tree.Chop();
		}
	}
	public CutTree(Agent agent, Vector2I target) : base(agent, target) {}
}

public class PickupTree : Action {
	public override void apply () {
		world.WorldTileInfoAtCoord(target).tree.Chop();
		((Habitant) performer).isCarryingWood = true;
	}
	public PickupTree(Agent agent, Vector2I target) : base(agent, target) {}
}

public class DropTree : Action {
	public override void apply () {
        ((Habitant) performer).tribe.wood_in_stock += WOOD_AMOUNT;
        ((Habitant) performer).isCarryingWood = false;
    }
	public DropTree(Agent agent, Vector2I target) : base(agent, target) {}
}

public class PlaceFlag : Action {
	public override void apply () {
		world.WorldTileInfoAtCoord(target).tribeTerritory.hasFlag = true;
		world.WorldTileInfoAtCoord(target).tribeTerritory.ownerTribe.id =
			((Habitant) performer).tribe.id;
	}
	public PlaceFlag(Agent agent, Vector2I target) : base(agent, target) {}
}

public class RemoveFlag : Action {
	public override void apply () {
        world.WorldTileInfoAtCoord(target).tribeTerritory.hasFlag = false;
        world.WorldTileInfoAtCoord(target).tribeTerritory.ownerTribe.id = "";
    }
	public RemoveFlag(Agent agent, Vector2I target) : base(agent, target) {}
}

public class PickupFood : Action {
	public override void apply () {
        Habitant habitant = (Habitant) performer;
		//FIXME: Don't remove the Animal.
        if(!habitant.CarryingResources()) {
            Animal animalToRemove = null;
            foreach(WorldInfo.Habitat h in world.habitats) {
                foreach(Animal a in h.animals) {
                    if (a.pos.Equals (target.ToVector2()) && !a.Alive) {
                        animalToRemove = a;
                        break;
                    }
                }
            }
            world.removeAnimal(animalToRemove);
            ((Habitant) performer).isCarryingFood = true;
        }
    }
	public PickupFood(Agent agent, Vector2I target) : base(agent, target) {}
}

	
public class DropFood : Action {
	public override void apply () {
        ((Habitant) performer).tribe.food_in_stock += FOOD_AMOUNT;
        ((Habitant) performer).isCarryingFood = false;
    }
	public DropFood(Agent agent, Vector2I target) : base(agent, target) {}
}

public class EatInPlace : Action {
	public override void apply () {
		performer.Eat(FOOD_AMOUNT);
        ((Habitant) performer).isCarryingFood = false;
    }
	public EatInPlace(Agent agent, Vector2I target) : base(agent, target) {}
}

public class EatInTribe : Action {
    public override void apply () {
		performer.Eat(FOOD_AMOUNT);
        ((Habitant) performer).tribe.food_in_stock -= FOOD_AMOUNT;
    }
    public EatInTribe(Agent agent, Vector2I target) : base(agent, target) {}
}