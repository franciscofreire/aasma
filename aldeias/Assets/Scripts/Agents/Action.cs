using UnityEngine;

public abstract class Action {
	public Agent agent;
	public abstract void apply (WorldInfo world, Agent agent);
	public void moveAgent(WorldInfo world, Agent agent, Vector2 newPos) {
		// Translate agent to new position

	}

	public Action(Agent agent) {
		this.agent = agent;
	}
}

public class Walk : Action {
	public override void apply (WorldInfo world, Agent agent) {}
	public Walk(Agent agent) : base(agent) {}
}

public class Turn : Action {
	public override void apply (WorldInfo world, Agent agent) {}
	public Turn(Agent agent) : base(agent) {}
}

public class Attack : Action {
	public override void apply (WorldInfo world, Agent agent) {}
	public Attack(Agent agent) : base(agent) {}
}

public class CutTree : Action {
	public override void apply (WorldInfo world, Agent agent) {}
	public CutTree(Agent agent) : base(agent) {}
}

public class PickupTree : Action {
	public override void apply (WorldInfo world, Agent agent) {}
	public PickupTree(Agent agent) : base(agent) {}
}

public class DropTree : Action {
	public override void apply (WorldInfo world, Agent agent) {}
	public DropTree(Agent agent) : base(agent) {}
}

public class PlaceFlag : Action {
	public override void apply (WorldInfo world, Agent agent) {}
	public PlaceFlag(Agent agent) : base(agent) {}
}

public class RemoveFlag : Action {
	public override void apply (WorldInfo world, Agent agent) {}
	public RemoveFlag(Agent agent) : base(agent) {}
}

public class PickupFood : Action {
	public override void apply (WorldInfo world, Agent agent) {}
	public PickupFood(Agent agent) : base(agent) {}
}

public class DropFood : Action {
	public override void apply (WorldInfo world, Agent agent) {}
	public DropFood(Agent agent) : base(agent) {}
}

public class Eat : Action {
	public override void apply (WorldInfo world, Agent agent) {}
	public Eat(Agent agent) : base(agent) {}
}