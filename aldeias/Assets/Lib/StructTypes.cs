using UnityEngine;


public struct Vector2I {
	public int x;
	public int y;
	
	public Vector2I(Vector2 v) {
		this.x = (int) v[0];
		this.y = (int) v[1];
	}

	public Vector2I(int x, int y) {
		this.x = x;
		this.y = y;
	}

	public Vector2 ToVector2() {
		return new Vector2((float) x, (float) y);
	}

	public static bool operator ==(Vector2I v1, Vector2I v2) {
		return v1.x == v2.x && v1.y == v2.y;
	}
	public static bool operator !=(Vector2I v1, Vector2I v2) {
		return !(v1 == v2);
	}
}