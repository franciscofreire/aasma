using UnityEngine;
using System;

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

	public static Vector2I operator +(Vector2I v1, Vector2I v2) {
		return new Vector2I(v1.x+v2.x,v1.y+v2.y);
	}

	public static bool operator ==(Vector2I v1, Vector2I v2) {
		return v1.x == v2.x && v1.y == v2.y;
	}
	
    public static bool operator !=(Vector2I v1, Vector2I v2) {
		return !(v1 == v2);
	}

    public bool isAdjacent(Vector2I v) {
        //sqrt((x0-x1)^2 + (y0-y1)^2)
        int dx = Math.Abs(this.x - v.x);
        int dy = Math.Abs(this.y - v.y);
        if(dx != dy) { // not diagonal
            return dx <= 1 && dy <= 1;
        }
        else {
            return false;
        }
    }
}

public partial struct Degrees {
	public readonly float value;
	public Degrees(float value) {
		this.value = value;
	}
    public Radians Radians { 
        get {
            return this;
        }
    }
	public static Degrees operator + (Degrees d1, Degrees d2) {
		return new Degrees(d1.value + d2.value);
	}
	public static Degrees operator - (Degrees d1, Degrees d2) {
		return new Degrees(d1.value - d2.value);
	}
    public static Degrees operator - (Degrees d) {
        return new Degrees(-d.value);
    }
}

public partial struct Radians {
	public readonly float value;
	public Radians(float value) {
		this.value = value;
	}
    public Degrees Degrees {
        get {
            return this;
        }
    }
	public static Radians operator + (Radians r1, Radians r2) {
		return new Radians(r1.value + r2.value);
	}
	public static Radians operator - (Radians r1, Radians r2) {
		return new Radians(r1.value - r2.value);
	}
    public static Radians operator - (Radians r) {
        return new Radians(-r.value);
    }
	public static Radians Pi {
		get {
			return new Radians(Mathf.PI);
		}
	}
}

public partial struct Degrees {
	public static implicit operator Radians (Degrees deg) {
		return new Radians(deg.value*Mathf.Deg2Rad);
	}
	public static implicit operator float (Degrees d) {
		return d.value;
	}
}

public partial struct Radians {
	public static implicit operator Degrees (Radians rad) {
		return new Degrees(rad.value*Mathf.Rad2Deg);
	}
	public static implicit operator float (Radians r) {
		return r.value;
	}
}

public static class Angles {
    public static Radians ToZero2Pi(Radians r) {
        float val = r.value % (2*Mathf.PI);
        return val < 0 ? new Radians(val+2*Mathf.PI) : new Radians(val);
    }
    public static Degrees ToZero360(Degrees d) {
        float val = d.value % 360;
        return val < 0 ? new Degrees(val+360) : new Degrees(val);
    }
}

public static class Vector2Extensions {
	public static float ProjectIntoFactor(this Vector2 projected, Vector2 other) {
		return Vector2.Dot(projected, other) / other.magnitude;
	}
	public static Vector2 ProjectInto(this Vector2 projected, Vector2 other) {
		return other * projected.ProjectIntoFactor(other);
    }
}