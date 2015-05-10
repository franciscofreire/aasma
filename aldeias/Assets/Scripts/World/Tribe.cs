using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class MeetingPoint {
	public Vector2I center;
	public int width;
	
	public MeetingPoint(Vector2I center, int width) {
		this.center = center;
		this.width = width;
	}

	public IEnumerable<Vector2I> MeetingPointTileCoords {
		get {
			Vector2I corner_0_0 = new Vector2I(center.x - width/2, center.y - width/2);
			foreach(var x in Enumerable.Range(0,width)) {
				foreach(var z in Enumerable.Range(0,width)) {
					yield return new Vector2I(corner_0_0.x+x,corner_0_0.y+z);
				}
			}
		}
	}
	
	public bool IsInMeetingPoint(Vector2I pos) {
		//return MeetingPointTileCoords.Contains(pos);
		return pos.x >= (center.x - width/2) && pos.x <= (center.x + width/2) 
			&& pos.y >= (center.y - width/2) && pos.y <= (center.y + width/2);
	}
}    

public struct Flag {
    public readonly Tribe Tribe;
    public Flag(Tribe t) {
        Tribe = t;
    }
}

public class FlagMakerMachine {
    Tribe tribe;
    public readonly WoodQuantity WoodPerFlag = new WoodQuantity(5);
    public FlagMakerMachine(Tribe t) {
        this.tribe = t;
    }
    public bool CanMakeFlag() {
        return tribe.WoodStock >= WoodPerFlag;
    }
    public Flag? MakeFlag() {
        if(CanMakeFlag()) {
            tribe.RemoveWoodFromStock(WoodPerFlag);
            return new Flag(tribe);
        } else {
            return null;
        }
    }
    public int RemainingFlags {
        get{ return (tribe.WoodStock / WoodPerFlag).Count; }
    }
}

public class Tribe {
	//Insert tribe identification here
	public readonly string id;
	public readonly MeetingPoint meetingPoint;
	public List<Habitant> habitants = new List<Habitant>();
	
    public FoodQuantity FoodStock = new FoodQuantity(5000);
    public WoodQuantity WoodStock = new WoodQuantity(1000); // We have 100 flags to place

    public static readonly int CRITICAL_FOOD_LEVEL = 500;

    public static readonly int CRITICAL_FLAG_QUANTITY = 10;

    public readonly FlagMakerMachine FlagMachine;

    public int cell_count;
    
	public Tribe(string id, MeetingPoint meetingPoint, int cell_count) {
		this.id = id;
		this.meetingPoint = meetingPoint;
        this.FlagMachine = new FlagMakerMachine(this);
        this.cell_count = cell_count;
	}

    //
    // Habitants
    //

	public void AddHabitant(Habitant h) {
		habitants.Add(h);
	}

    public void RemoveHabitant(Habitant h) {
        habitants.Remove(h);
    }

    //
    // Wood
    //

	public void AddWoodToStock(WoodQuantity wood) {
		WoodStock = WoodStock + wood;
	}
	public WoodQuantity RemoveWoodFromStock(WoodQuantity woodToRemove) {
		if(WoodStock >= woodToRemove) {
			WoodStock = WoodStock - woodToRemove;
			return woodToRemove;
		} else {
			return WoodQuantity.Zero;
		}
	}

    //
    // Food
    //

	public void AddFoodToStock(FoodQuantity food) {
		FoodStock = FoodStock + food;
	}
	public FoodQuantity RemoveFoodFromStock(FoodQuantity foodToRemove) {
		if(FoodStock >= foodToRemove) {
			FoodStock = FoodStock - foodToRemove;
			return foodToRemove;
		} else {
			return FoodQuantity.Zero;
		}
	}
    public bool Equals (Tribe t) {
      return this.id.Equals(t.id);
    }
}
