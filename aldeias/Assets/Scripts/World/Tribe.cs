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
		return pos.x >= (center.x - width/2) && pos.x <= (center.x - width/2) 
			&& pos.y >= (center.y - width/2) && pos.y <= (center.y - width/2);
	}
}            

public class Tribe {
	//Insert tribe identification here
	public string id;
	public MeetingPoint meetingPoint;
	public List<Habitant> habitants = new List<Habitant>();
	
    public FoodQuantity FoodStock = FoodQuantity.Zero;
    public WoodQuantity WoodStock = new WoodQuantity(1000); // We have 100 flags to place
	
	public Tribe(string id, MeetingPoint meetingPoint) {
		this.id = id;
		this.meetingPoint = meetingPoint;
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
}
