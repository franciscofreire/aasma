using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Plan {
    private Queue<Action> plan = new Queue<Action>();

    public Plan() {
    }

    public bool isEmpty() {
        return plan.Count == 0;
    }

    public Action head() {
        return plan.Dequeue();
    }

    public void add(Action a) {
        plan.Enqueue(a);
    }

    // Amazing pathfinding: 2 straight paths ignoring collisions
    public void addPathFinding(Agent agent, Vector2I target) {
        Vector2I pos = CoordConvertions.AgentPosToTile(agent.pos);
        int x_steps = (int) System.Math.Abs(pos.x - target.x);
        int y_steps = (int) System.Math.Abs(pos.y - target.y);
        for (int i = 0; i < x_steps; i++) {
            for (int j = 0; j < y_steps; j++) {
                add(new Walk(agent, new Vector2I(pos.x + i, pos.y + j)));
            }
        }
    }
}