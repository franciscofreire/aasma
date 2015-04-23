using UnityEngine;
using System.Collections.Generic;

public class MeetingPoint {
	public Vector2I centralPoint;
	public int width;
	public List<Vector2I> meetingPointCells;
	
	public MeetingPoint(Vector2I centralPoint, int width) {
		this.centralPoint = centralPoint;
		this.width = width;
		this.meetingPointCells = new List<Vector2I>();
		
		// width must be odd
		int leftCornerX = (int) centralPoint.x - Mathf.FloorToInt(width/2);
		int leftCornerZ = (int) centralPoint.y + Mathf.FloorToInt(width/2);
		
		//map[leftCornerX,leftCornerZ] = true;
		//map[(int)centralPoint.x, (int)centralPoint.y] = true;
		for(int i = 0; i < width; ++i) {
			for(int j = 0; j < width; j++) {
				int posX = leftCornerX + i;
				int posZ = leftCornerZ - j;
				this.meetingPointCells.Add(new Vector2I(posX, posZ));
			}
		}
	}
	
	public bool IsMeetingPoint(Vector2I pos) {
		foreach(Vector2I mpCell in meetingPointCells) {
			if(mpCell.Equals(pos)) {
				return true;
			}
		}
		
		return false;
	}
}            

public class Tribe {
	//Insert tribe identification here
	public string id;
	public MeetingPoint meetingPoint;
	public List<Habitant> habitants = new List<Habitant>();
	
	public FoodQuantity FoodStock = FoodQuantity.Zero;
	public WoodQuantity WoodStock = WoodQuantity.Zero;
	
	public Tribe(string id, Vector2I centralPoint, int width) {
		this.id = id;
		this.meetingPoint = new MeetingPoint(centralPoint, width);
	}
	public void AddHabitant(Habitant h) {
		habitants.Add(h);
	}
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
