using UnityEngine;

public class Animal : Agent {

    public static readonly Energy INITIAL_ENERGY = new Energy(20);
    private FoodQuantity food;

    public FoodQuantity Food {
        get {
            return food;
        }
        set {
            food = value;
        }
    }

	public Animal(WorldInfo world, Vector2 pos, FoodQuantity food)
            : base(world, pos, INITIAL_ENERGY) {
        this.food = food;
    }

	public override Action doAction() {

        int index = WorldRandom.Next(sensorData.Cells.Count);
        Vector2I target = sensorData.Cells[index];
        return new Walk(this, target);
	}

	public override void OnWorldTick () {
	}
    
    public bool HasFood {
        get { 
            return food > FoodQuantity.Zero;
        }
    }
    
    public override void RemoveEnergy(Energy e) {
        energy.Subtract(e);
        if (!Alive) {
            Debug.Log("[RIP] Animal @(" + pos.x + "," + pos.y + ")");
            worldInfo.NotifyAgentDiedListeners(new Vector2I(pos));
        }
    }

    public FoodQuantity Tear() {
        if (Alive) {
            return FoodQuantity.Zero;
        } else {
            //FIXME: These are testing values!
            food.Count -= 50;
            FoodQuantity removed = food;
            return removed;
        }
    }

	//*************
	//** SENSORS **
	//*************

	public override bool EnemyInFront() {
		return false;
	}
}