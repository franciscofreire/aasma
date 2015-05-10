using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Plan {
    private Queue<Action> plan = new Queue<Action>();

    public Plan() {
    }

    public bool isEmpty() {
        return plan.Count > 0;
    }

    public Action head() {
        return plan.Dequeue();
    }

    public void add(Action a) {
        plan.Enqueue(a);
    }

    public void copy(Queue<Action> q) {
        plan = new Queue<Action>(q);
    }
}