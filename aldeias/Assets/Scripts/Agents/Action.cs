using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public abstract class Action {
    // FIXME: Amounts will likely be changed
    protected const int FOOD_AMOUNT = 100;
    protected const int WOOD_AMOUNT = 100;

	protected Agent agent;
	protected Vector2I target;

	// Atributes for quick access
	protected IList<Vector2I> cells;
	protected WorldInfo world;

	public abstract void apply ();

	public Action(Agent agent, Vector2I target) {
		this.agent  = agent;
		this.target = target;	

		this.cells = agent.sensorData.Cells;
		this.world = agent.worldInfo;
	}
}

public class Walk : Action {
	public override void apply () {
		// Update worldtileInfo
		WorldInfo.WorldTileInfo agentTileInfo = 
			world.WorldTileInfoAtCoord(world.AgentPosToWorldXZ(agent.pos));
		WorldInfo.WorldTileInfo targetTileInfo = 
			world.WorldTileInfoAtCoord(target);
		agentTileInfo.hasAgent = false;
		targetTileInfo.hasAgent = true;
		
		// Orientation
		int x_origin = (int) agent.pos[0];
		int z_origin = (int) agent.pos[1];
		int x_target = (int) target.x;
		int z_target = (int) target.y;
		if (x_origin > x_target) {
			agent.orientation = ORIENTATION.LEFT;
		} else if (x_origin < x_target) {
			agent.orientation = ORIENTATION.RIGHT;
		} else if (z_origin > z_target) {
			agent.orientation = ORIENTATION.UP;
		} else {
			agent.orientation = ORIENTATION.DOWN;
		}
		
		// Position
		agent.pos = target.ToVector2();
	}
	public Walk(Agent agent, Vector2I target) : base(agent, target) {}
}

public class Attack : Action {
	public override void apply () {
        if(world.WorldTileInfoAtCoord(target).hasAgent) {
            foreach(Agent a in world.allAgents) {
                if(a.pos.Equals(target.ToVector2())) {
                    a.DecreaseEnergy();
                }
            }
        }
    }
	public Attack(Agent agent, Vector2I target) : base(agent, target) {}
}

public class CutTree : Action {
	public override void apply () {
		if (world.WorldTileInfoAtCoord(target).tree.isStump == false) {
			world.WorldTileInfoAtCoord(target).tree.turnToStump = true;
			world.WorldTileInfoAtCoord(target).tree.isStump = true;
		}
	}
	public CutTree(Agent agent, Vector2I target) : base(agent, target) {}
}

public class PickupTree : Action {
	public override void apply () {
		world.WorldTileInfoAtCoord(target).tree.Chop();
		((Habitant) agent).isCarryingWood = true;
	}
	public PickupTree(Agent agent, Vector2I target) : base(agent, target) {}
}

public class DropTree : Action {
	public override void apply () {
        ((Habitant) agent).tribe.wood_in_stock += WOOD_AMOUNT;
        ((Habitant) agent).isCarryingWood = false;
    }
	public DropTree(Agent agent, Vector2I target) : base(agent, target) {}
}

public class PlaceFlag : Action {
	public override void apply () {
		world.WorldTileInfoAtCoord(target).tribeTerritory.hasFlag = true;
		world.WorldTileInfoAtCoord(target).tribeTerritory.ownerTribe.id =
			((Habitant) agent).tribe.id;
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
        ((Habitant) agent).isCarryingFood = true;
    }
	public PickupFood(Agent agent, Vector2I target) : base(agent, target) {}
}

public class DropFood : Action {
	public override void apply () {
        ((Habitant) agent).tribe.food_in_stock += FOOD_AMOUNT;
        ((Habitant) agent).isCarryingFood = false;
    }
	public DropFood(Agent agent, Vector2I target) : base(agent, target) {}
}

public class EatInPlace : Action {
	public override void apply () {
        agent.energy = FOOD_AMOUNT;
        ((Habitant) agent).isCarryingFood = false;
    }
	public EatInPlace(Agent agent, Vector2I target) : base(agent, target) {}
}

public class EatInTribe : Action {
    public override void apply () {
        agent.energy = FOOD_AMOUNT;
        ((Habitant) agent).tribe.food_in_stock -= FOOD_AMOUNT;
    }
    public EatInTribe(Agent agent, Vector2I target) : base(agent, target) {}
}