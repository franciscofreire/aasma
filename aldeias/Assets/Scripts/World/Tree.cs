using UnityEngine;

public struct Tree {
	public bool hasTree;
	//public Vector2 pos;
	public bool isStump, turnToStump;
	public int wood; // 0: No wood; 100: Full wood

	public Tree (int wood) {
		//this.pos = new Vector2(x, z);
		this.hasTree = false;
		this.isStump = false;
		this.turnToStump = false;
		this.wood = wood;
	}

	public void Chop() {
		wood = 0;
	}
}