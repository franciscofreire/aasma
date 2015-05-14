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

    // isSound() will use this to ensure an action is possible
    public abstract bool acceptValidationVisitor(ValidationVisitor vv);

    public static Action WalkRandomly(Habitant habitant) {
        int index = WorldRandom.Next(habitant.sensorData.AdjacentCells.Count);
        Vector2I target;
        try {
            target = habitant.sensorData.AdjacentCells[index];
        }
        catch (System.Exception) {
            // We don't have nearby free cells, so we do nothing
            // and stay at the same position
            target = new Vector2I(habitant.pos);
        }
        return new Walk(habitant, target);
    }

    public static Action WalkFront(Habitant habitant) {
        return new Walk(habitant, habitant.sensorData.FrontCell);
    }

    public static Action RunAwayOrWalkRandomly(Habitant habitant) {
        
        Vector2 oppositePos = habitant.pos + 
            habitant.orientation.LeftOrientation().LeftOrientation().ToVector2();
        Vector2I tileCoordOppositePos = CoordConvertions.AgentPosToTile(oppositePos);
        if(habitant.worldInfo.isInsideWorld(tileCoordOppositePos)) {
            return new Walk(habitant, tileCoordOppositePos);
        } else {
            return Action.WalkRandomly(habitant);
        }
    }
    
    public static Action RunAwayOrWalkRandomlyOrEat(Habitant habitant) {
        Vector2 oppositePos = habitant.pos + 
            habitant.orientation.LeftOrientation().LeftOrientation().ToVector2();
        Vector2I tileCoordOppositePos = CoordConvertions.AgentPosToTile(oppositePos);
        CryptoRandom rnd = new CryptoRandom();
        if(rnd.Next (10) <= 5) {
            return new EatInTribe(habitant,CoordConvertions.AgentPosToTile(habitant.pos));
        }
        if(habitant.worldInfo.isInsideWorld(tileCoordOppositePos)) {
            return new Walk(habitant, tileCoordOppositePos);
        } else {
            return Action.WalkRandomly(habitant);
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
    private const int WALK_DECREMENT = 2;
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

        // decrement energy when a habitant walks
        if(target != origin) {
            //agent.RemoveEnergy(new Energy(WALK_DECREMENT));
        }
	}
    
    public override bool acceptValidationVisitor(ValidationVisitor vv) {
        return vv.isWalkValid(this);
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
    
    public override bool acceptValidationVisitor(ValidationVisitor vv) {
        return vv.isAttackValid(this);
    }

	public Attack(Agent agent, Vector2I target) : base(agent, target) {}
}

/**********/
/* ANIMAL */
/**********/

public class AnimalAccelerate : Action {
    private Animal animal;
    private Vector2 acceleration;
    public override Agent performer {
        get {
            return animal;
        }
    }

    public AnimalAccelerate(Animal animal, Vector2 acceleration):base(new Vector2I(0,0))/*SHUT UP, COMPILER!*/ {
        this.animal = animal;
        this.acceleration = acceleration;
    }

    public override void apply() {
        animal.ApplyAcceleration(acceleration);
    }
    
    public override bool acceptValidationVisitor(ValidationVisitor vv) {
        return vv.isAnimalAccelerateValid(this);
    }
}

public class AnimalAttackHabitant : Action {
    private Animal animal;
    private Habitant habitant;
    public override Agent performer {
        get {
            return animal;
        }
    }

    public AnimalAttackHabitant(Animal animal, Habitant habitant):base(new Vector2I(0,0)) {
        this.animal = animal;
        this.habitant = habitant;
    }

    public override void apply() {
        animal.AttackMechanism.TryAttackAgent(habitant);
    }

    public override bool acceptValidationVisitor(ValidationVisitor vv) {
        return vv.isAnimalAttackHabitantValid(this);
    }
}

/************/
/* HABITANT */
/************/

public abstract class HabitantAction : Action {
	protected Habitant habitant;
	public override Agent performer {
		get {
			return habitant;
		}
    }
    public Habitant Habitant {
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
    
    public override bool acceptValidationVisitor(ValidationVisitor vv) {
        return vv.isCutTreeValid(this);
    }

	public CutTree(Habitant habitant, Vector2I target) : base(habitant, target) {}
}

public class ChopTree : HabitantAction {
	public override void apply () {
		WoodQuantity wood = world.worldTiles.WorldTileInfoAtCoord(target).Tree.Chop();
		habitant.PickupWood(wood);
		//FIXME: if the habitant can't carry the WoodQuantity than it is lost.
	}
    
    public override bool acceptValidationVisitor(ValidationVisitor vv) {
        return vv.isChopTreeValid(this);
    }

	public ChopTree(Habitant habitant, Vector2I target) : base(habitant, target) {}
}

public class DropTree : HabitantAction {
	public override void apply () {
		WoodQuantity wood = habitant.DropWood(habitant.carriedWood);
		habitant.tribe.AddWoodToStock(wood);
    }
    
    public override bool acceptValidationVisitor(ValidationVisitor vv) {
        return vv.isDropTreeValid(this);
    }

	public DropTree(Habitant habitant, Vector2I target) : base(habitant, target) {}
}

public class PlaceFlag : HabitantAction {
	public override void apply () {
        if (world.worldTiles.WorldTileInfoAtCoord(target).tribeTerritory.IsClaimed)
            return;

        Flag? flag = habitant.tribe.FlagMachine.MakeFlag();
        if (flag != null) {
            world.worldTiles.WorldTileInfoAtCoord(target).tribeTerritory.Flag = flag;

            //TODO: Dec cellcount of enemy
            habitant.tribe.cell_count++;
        }
	}
    
    public override bool acceptValidationVisitor(ValidationVisitor vv) {
        return vv.isPlaceFlagValid(this);
    }

	public PlaceFlag(Habitant habitant, Vector2I target) : base(habitant, target) {}
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
    
    public override bool acceptValidationVisitor(ValidationVisitor vv) {
        return vv.isPickupFoodValid(this);
    }

	public PickupFood(Habitant habitant, Vector2I target) : base(habitant, target) {}
}
	
public class DropFood : HabitantAction {
    public override void apply () {
        FoodQuantity food = habitant.DropFood(habitant.carriedFood);
		habitant.tribe.AddFoodToStock(food);
    }
    
    public override bool acceptValidationVisitor(ValidationVisitor vv) {
        return vv.isDropFoodValid(this);
    }

	public DropFood(Habitant habitant, Vector2I target) : base(habitant, target) {}
}

public class EatCarriedFood : HabitantAction {
	public static readonly FoodQuantity FoodConsumedByHabitant = new FoodQuantity(50);
    public static bool IsEnoughFood(FoodQuantity food) {
        return food >= FoodConsumedByHabitant;
    }
    public override void apply () {
		if(habitant.carriedFood >= FoodConsumedByHabitant) {
			habitant.Eat(habitant.DropFood(FoodConsumedByHabitant));
		}
    }
    
    public override bool acceptValidationVisitor(ValidationVisitor vv) {
        return vv.isEatCarriedFoodValid(this);
    }

	public EatCarriedFood(Habitant habitant) : base(habitant, new Vector2I(0,0)) {} /*SHUT UP COMPILER! */
}

public class EatInTribe : HabitantAction {
	public static readonly FoodQuantity FoodConsumedByHabitant = new FoodQuantity(50);
    public static bool IsEnoughFood(FoodQuantity food) {
        return food >= FoodConsumedByHabitant;
    }
    public override void apply () {
        Flag? flag = world.worldTiles.WorldTileInfoAtCoord(target).tribeTerritory.Flag;
        if(flag.HasValue && flag.Value.Tribe.Equals(habitant.tribe)) {
		    habitant.Eat(habitant.tribe.RemoveFoodFromStock(FoodConsumedByHabitant));
        }
    }
    
    public override bool acceptValidationVisitor(ValidationVisitor vv) {
        return vv.isEatInTribeValid(this);
    }

	public EatInTribe(Habitant habitant, Vector2I target) : base(habitant, target) {}
}

public class TurnLeft : HabitantAction {

    public override void apply () {
        Orientation newOrientation = habitant.orientation.LeftOrientation();
        habitant.orientation = newOrientation;
    }
    
    public override bool acceptValidationVisitor(ValidationVisitor vv) {
        return vv.isTurnLeftValid(this);
    }

    public TurnLeft(Habitant habitant, Vector2I target) : base(habitant, target) {}
}

public class TurnOppositeDirection : HabitantAction {
    
    public override void apply () {
        Orientation newOrientation = 
            habitant.orientation.LeftOrientation().LeftOrientation();
        habitant.orientation = newOrientation;
    }
    
    public override bool acceptValidationVisitor(ValidationVisitor vv) {
        return vv.isTurnOppositeDirectionValid(this);
    }
    
    public TurnOppositeDirection(Habitant habitant, Vector2I target) : base(habitant, target) {}
}