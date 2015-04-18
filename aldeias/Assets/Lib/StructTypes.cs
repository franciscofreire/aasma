using UnityEngine;

public struct Orientation {
	float angleToZ;
	
	public static Orientation FromAngleToZ(float angle) {
		return new Orientation(0);
	}
	
	public static Orientation forward {
		get { return FromAngleToZ(0); }
	}
	
	public Vector2 ToVector2() {
		return new Vector2(Mathf.Cos(angleToZ), Mathf.Sin(angleToZ));
	}
	
	public Quaternion ToQuaternion() {
		return Quaternion.AngleAxis(angleToZ+90.0f, Vector3.up);
	}
	
	private Orientation(float angleToZ) {
		this.angleToZ = angleToZ;
	}
}


public struct Vector2I {
	public int x;
	public int y;

	public Vector2I(int x, int y) {
		this.x = x;
		this.y = y;
	}

	public static bool Equal(Vector2I v1, Vector2I v2) {
		return v1.x == v2.x && v1.y == v2.y;
	}
}