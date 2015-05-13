using System;
using UnityEngine;

// A Tree starts alive with a certain WoodQuantity.
// To collect its wood:
//    1. one must first Chop it down (it dies in the process). No WoodQuantity is extrated in this stage.
//    2. one must Chop it once again to extract its WoodQuantity.
//    3. once its WoodQuantity was extrated, only a useless "TreeStump" remains.
public class Tree {

    public WorldInfo worldInfo;

	private Vector2I pos;
	private bool isAlive;
	private WoodQuantity wood;
    
    public readonly static WoodQuantity    WoodChopQuantity = new WoodQuantity(50);
    public readonly static WoodQuantity    InitialWoodQuantity = new WoodQuantity(100);

    public Tree (WorldInfo worldInfo, Vector2I pos) {
        this.worldInfo = worldInfo;
		this.pos = pos;
        this.isAlive = true;
        this.wood = InitialWoodQuantity;
	}

	public Vector2I Pos { 
		get {
			return pos;
		}
	}

	public bool Alive {
		get { return isAlive; }
	}

	public WoodQuantity Chop() {
		if (isAlive) {
            isAlive = false; // Die...
            Logger.Log("[RIP] Tree @(" + pos.x + "," + pos.y + ")", Logger.VERBOSITY.AGENTS);
            worldInfo.NotifyTreeDiedListeners(pos);
			return WoodQuantity.Zero;
        } else {
            //FIXME: These are testing values!
            WoodQuantity removed = new WoodQuantity(Math.Min(wood.Count, WoodChopQuantity.Count));
            wood = wood - removed;
			return removed;
		}
		//Implementation detail: This method could be an Iterator method (use the yield statement).
	}

	public bool HasWood {
		get { return wood > WoodQuantity.Zero; }
	}

	public WoodQuantity Wood {
		get { return wood; }
	}
}