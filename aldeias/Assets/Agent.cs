using System;
using UnityEngine;

public abstract class Agent {
	public Vector2 pos;
	public Agent () {

	}
	public abstract void doAction();
}