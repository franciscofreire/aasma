using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

// An Action is always performed by an Agent and target a tile (Vector2I is its coordinate) of the Agent's WorldInfo.
//    Actions can be specific to a type of Agent (ex: action of Habitants or action of Animals).
// An Action can be performed (Apply) only once.
public abstract class Action {

	public abstract Agent performer { get; }
	public readonly Vector2I target;

	public Action(Vector2I target) {
		this.target = target;
	}
	public abstract void apply ();
	protected WorldInfo world { 
		get {
			return performer.worldInfo;
		}
	}
}

// AnyAgentActions are actions that can be performed by any type of Agent.
public abstract class AnyAgentAction : Action {
	protected Agent agent;
	public override Agent performer {
		get {
			return agent;
		}
	}
	public AnyAgentAction(Agent agent, Vector2I target) : base(target) {
		this.agent = agent;
	}
}

public class Walk : AnyAgentAction {
	public override void apply () {
		// Update worldtileInfo
		WorldInfo.WorldTileInfo agentTileInfo = 
			world.WorldTileInfoAtCoord(WorldInfo.AgentPosToWorldXZ(performer.pos));
		WorldInfo.WorldTileInfo targetTileInfo = 
			world.WorldTileInfoAtCoord(target);
		agentTileInfo.hasAgent = false;
		targetTileInfo.hasAgent = true;
		
		// Orientation
		Vector2I origin = WorldInfo.AgentPosToWorldXZ(agent.pos);
		if (origin.x > target.x) {
			performer.orientation = ORIENTATION.LEFT;
		} else if (origin.x < target.x) {
			performer.orientation = ORIENTATION.RIGHT;
		} else if (origin.y > target.y) {
			performer.orientation = ORIENTATION.UP;
		} else {
			performer.orientation = ORIENTATION.DOWN;
		}
		
		// Position
		performer.pos = target.ToVector2();
	}
	public Walk(Agent walker, Vector2I target) : base(walker, target) { }
}

public class Attack : AnyAgentAction {
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

public abstract class HabitantAction : Action {
	protected Habitant habitant;
	public override Agent performer {
		get {
			return habitant;
		}
	}
	public HabitantAction(Habitant habitant, Vector2I target) : base(target) {
		this.habitant = habitant;
	}
}

public class CutTree : HabitantAction {
	public override void apply () {
		if(world.WorldTileInfoAtCoord(target).tree.Alive) {
			world.WorldTileInfoAtCoord(target).tree.Chop();
		}
	}
	public CutTree(Habitant habitant, Vector2I target) : base(habitant, target) {}
}

public class PickupTree : HabitantAction {
	public override void apply () {
		WoodQuantity wood = world.WorldTileInfoAtCoord(target).tree.Chop();
		habitant.PickupWood(wood);
	}
	public PickupTree(Habitant habitant, Vector2I target) : base(habitant, target) {}
}

public class DropTree : HabitantAction {
	public override void apply () {
		WoodQuantity wood = habitant.DropWood(habitant.carriedWood);
		habitant.tribe.AddWoodToStock(wood);
    }
	public DropTree(Habitant habitant, Vector2I target) : base(habitant, target) {}
}

public class PlaceFlag : HabitantAction {
	public override void apply () {
		world.WorldTileInfoAtCoord(target).tribeTerritory.hasFlag = true;
		world.WorldTileInfoAtCoord(target).tribeTerritory.ownerTribe = habitant.tribe;
	}
	public PlaceFlag(Habitant habitant, Vector2I target) : base(habitant, target) {}
}

public class RemoveFlag : HabitantAction {
	public override void apply () {
        world.WorldTileInfoAtCoord(target).tribeTerritory.hasFlag = false;
        world.WorldTileInfoAtCoord(target).tribeTerritory.ownerTribe.id = "";
    }
	public RemoveFlag(Habitant habitant, Vector2I target) : base(habitant, target) {}
}

public class PickupFood : HabitantAction {
	public static readonly FoodQuantity AnimalFoodPotencial = new FoodQuantity(100);
	public override void apply () {
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
			habitant.PickupFood(AnimalFoodPotencial);
        }
    }
	public PickupFood(Habitant habitant, Vector2I target) : base(habitant, target) {}
}

	
public class DropFood : HabitantAction {
	public override void apply () {
		habitant.tribe.AddFoodToStock(habitant.DropFood(habitant.carriedFood));
    }
	public DropFood(Habitant habitant, Vector2I target) : base(habitant, target) {}
}

public class EatInPlace : HabitantAction {
	public static readonly FoodQuantity FoodConsumedByHabitant = new FoodQuantity(100);
	public override void apply () {
		if(habitant.carriedFood >= FoodConsumedByHabitant) {
			habitant.Eat(habitant.DropFood(FoodConsumedByHabitant));
		}
    }
	public EatInPlace(Habitant habitant, Vector2I target) : base(habitant, target) {}
}

public class EatInTribe : HabitantAction {
	public static readonly FoodQuantity FoodConsumedByHabitant = new FoodQuantity(100);
    public override void apply () {
		habitant.Eat(habitant.tribe.RemoveFoodFromStock(FoodConsumedByHabitant));
    }
	public EatInTribe(Habitant habitant, Vector2I target) : base(habitant, target) {}
}