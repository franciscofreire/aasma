using System;
using UnityEngine;

public abstract class Agent {
	public Vector2 pos;
	public Vector3 orientation;
	public int energy; // 0: No energy; 100: Full energy

	public Agent() {
	}

	public Agent (Vector2 pos) {
		this.pos = pos;
		this.orientation = Vector3.forward;
		this.energy = 100;
	}

	public abstract void doAction();
}