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
		//Change the Agent's position and reorient him so he faces the same direction in which he moved.
		Vector2I origin = CoordConvertions.AgentPosToTile(agent.pos);
		agent.ChangePosition(CoordConvertions.TileToAgentPos(target));
		// Orientation
		if (origin.x > target.x) {
			performer.orientation = Orientation.Left;
		} else if (origin.x < target.x) {
			performer.orientation = Orientation.Right;
		} else if (origin.y > target.y) {
			performer.orientation = Orientation.Down;
		} else {
			performer.orientation = Orientation.Up;
		}
	}
	public Walk(Agent walker, Vector2I target) : base(walker, target) { }
}

public class Attack : AnyAgentAction {
	public static readonly Energy ENERGY_TO_REMOVE = new Energy(20);
	public override void apply () {
		if(world.worldTiles.WorldTileInfoAtCoord(target).HasAgent) {
            Agent enemy = world.worldTiles.WorldTileInfoAtCoord(target).Agent;
			enemy.RemoveEnergy(ENERGY_TO_REMOVE);
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
		if(world.worldTiles.WorldTileInfoAtCoord(target).Tree.Alive) {
			world.worldTiles.WorldTileInfoAtCoord(target).Tree.Chop();
			//FIXME: if some WoodQuantity was dropped it is lost.
		}
	}
	public CutTree(Habitant habitant, Vector2I target) : base(habitant, target) {}
}

public class PickupTree : HabitantAction {
	public override void apply () {
		WoodQuantity wood = world.worldTiles.WorldTileInfoAtCoord(target).Tree.Chop();
		habitant.PickupWood(wood);
		//FIXME: if the habitant can't carry the WoodQuantity than it is lost.
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
		world.worldTiles.WorldTileInfoAtCoord(target).tribeTerritory.OwnerTribe = habitant.tribe;
	}
	public PlaceFlag(Habitant habitant, Vector2I target) : base(habitant, target) {}
}

public class RemoveFlag : HabitantAction {
	public override void apply () {
		world.worldTiles.WorldTileInfoAtCoord(target).tribeTerritory.OwnerTribe = null;
    }
	public RemoveFlag(Habitant habitant, Vector2I target) : base(habitant, target) {}
}

public class PickupFood : HabitantAction {
    public override void apply () {
        Animal a = world.animalInTile(target);
        if (a != null) {
            FoodQuantity food = a.Tear();
            habitant.PickupFood(food);
        }
        //FIXME: if the habitant can't carry the FoodQuantity than it is lost.
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