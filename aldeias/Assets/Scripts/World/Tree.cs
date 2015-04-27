
// A Tree starts alive with a certain WoodQuantity.
// To collect its wood:
//    1. one must first Chop it down (it dies in the process). No WoodQuantity is extrated in this stage.
//    2. one must Chop it once again to extract its WoodQuantity.
//    3. once its WoodQuantity was extrated, only a useless "TreeStump" remains.
public class Tree {

	private bool isAlive;
	private WoodQuantity wood;

	public Tree (WoodQuantity woodQuantity) {
		this.isAlive = true;
		this.wood = woodQuantity;
	}

	public bool Alive {
		get { return isAlive; }
	}

	public WoodQuantity Chop() {
		if (isAlive) {
			isAlive = false; // Die...
			return WoodQuantity.Zero;
		} else {
			WoodQuantity removed = wood;
			wood = WoodQuantity.Zero;
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