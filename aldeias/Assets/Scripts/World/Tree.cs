using UnityEngine;

public class Tree {
	public Vector2 pos;
	public bool isStump, turnToStump;
	public int wood; // 0: No wood; 100: Full wood

	public Tree (int x, int z) {
		this.pos = new Vector2(x, z);
		this.isStump = false;
		this.turnToStump = false;
		this.wood = 100;
	}
}