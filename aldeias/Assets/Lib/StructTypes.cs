using UnityEngine;

public enum ORIENTATION {UP=0, DOWN=180, LEFT=270, RIGHT=90};

public struct Orientation {
	private ORIENTATION orientation;
	
	public static implicit operator Orientation(ORIENTATION orientation) {
		return new Orientation(orientation);
	}
	
	public static Orientation FromORIENTATION(ORIENTATION orientation) {
		return orientation;
	}
	
	public Vector2 ToVector2() {//Up=(0,1), Down=(0,-1), Left=(-1,0), Right=(1,0)
		switch (orientation) {
		case ORIENTATION.UP:
			return new Vector2(0f,1f);
		case ORIENTATION.DOWN:
			return new Vector2(0f,-1f);
		case ORIENTATION.LEFT:
			return new Vector2(-1f,0f);
		case ORIENTATION.RIGHT:
			return new Vector2(1f,0f);
		default:
			throw new System.Exception("Cannot convert "+orientation+" to UnityEngine.Vector2.");
		}
	}
	
	public Quaternion ToQuaternion() {
		return Quaternion.AngleAxis((float)orientation, Vector3.up);
	}

    public Quaternion ToQuaternionInX() {
        return Quaternion.AngleAxis((float)orientation, Vector3.right);
    }

	private Orientation(ORIENTATION orientation) {
		this.orientation = orientation;
	}
}


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

	public static bool Equal(Vector2I v1, Vector2I v2) {
		return v1.x == v2.x && v1.y == v2.y;
	}

}