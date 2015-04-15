using System;
using UnityEngine;

public abstract class Agent {
	public Vector2 pos;
	public Agent() {

	}
	public Agent (Vector2 pos) {
		this.pos = pos;
	}

	public abstract void doAction();
}