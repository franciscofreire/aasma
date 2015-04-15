using UnityEngine;

public class Habitant : Agent {
	public string tribeId;

	public Habitant(Vector2 pos, string tribeId) {
		Agent(pos);
		this.tribeId = tribeId;
	}
	public override void doAction() {
		//TODO
	}
}
