using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Plan {
    private Queue<Action> plan = new Queue<Action>();
    public Action LastAction { get; set; }

    private Attitude intention;

    public Plan(Attitude intention) {
        this.intention = intention;
    }

    public bool isSound(Beliefs beliefs) {
        return intention.isSound(beliefs);
    }

    public bool isEmpty() {
        return plan.Count == 0;
    }
    
    public void clear() {
        plan.Clear();
    }

    public Action head() {
        return plan.Dequeue();
    }

    public void add(Action a) {
        plan.Enqueue(a);
    }
    
    public void addLastAction(Action a) {
        add(a);
        LastAction = a;
    }

    // Amazing pathfinding: 2 straight paths ignoring collisions
    public void addPathFinding(Agent agent, Vector2I target) {
        Vector2I pos = CoordConvertions.AgentPosToTile(agent.pos);
        Vector2I step = target - pos;
            step.x = (int) Mathf.Sign(step.x);
            step.y = (int) Mathf.Sign(step.y);
        if (pos.x != target.x)
            for (int i = pos.x + step.x; i != target.x; i += step.x) {
                add(new Walk(agent, new Vector2I(i, pos.y)));
            }
        if (pos.y != target.y)
            for (int i = pos.y + step.y; i != target.y; i += step.y) {
                add(new Walk(agent, new Vector2I(target.x, i)));
            }
    }
}