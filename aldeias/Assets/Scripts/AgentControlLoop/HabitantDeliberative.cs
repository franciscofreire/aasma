using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class HabitantDeliberative : AgentImplementation {
    private Habitant habitant;

    private Beliefs         beliefs    = new Beliefs();
    private List<Attitude>  desires    = new List<Attitude>();
    private List<Attitude>  intentions = new List<Attitude>();

    private Plan plan = new Plan();

    private bool ActionExecuted {
        get; set;
    }

    public void doOptions() {
        int count = beliefs.Count();
        for (int i = 0; i < count; i++) {
            Belief b = beliefs.Get(i);
            if (b.IsActive) {
                // Do shit
            }
        }
    }

    // Choose the three most important desires, converting them to intentions
    // The list is ordered by importance in decrescent order
    public void doFilter() {
        List<Attitude> candidates = new List<Attitude>();

        foreach (Attitude desire in desires) {
            filterDesire(candidates, desire);
        }

        intentions = candidates;
    }

    private void filterDesire(List<Attitude> candidates, Attitude desire) {
        const int MAX_COUNT = 3;
        for (int i = 0 ; i < candidates.Count; i++) {
            Attitude candidate = candidates.ElementAt(i);

            // If the desire is more important than some candidate, we add it
            if (candidate.importance < desire.importance) {
                candidates.Insert(i, desire);

                // Remove the less important candidate if we have too many candidates
                if (candidates.Count > MAX_COUNT) {
                    candidates.RemoveAt(candidates.Count - 1);
                }

                return;
            }
        }

        // The desire has the lowest importance, but can it still be a candidate?
        if (candidates.Count < MAX_COUNT) {
            candidates.Add(desire);
        }
    }
    
    // Choose the plan from the most important intention
    public Plan doPlan() {
        return intentions.First().plan;
    }

    public bool succeded() {
        return false;
    }

    public bool impossible() {
        return false;
    }

    public bool reconsider() {
        return false;
    }

    public bool sound() {
        return true;
    }

    private bool actionsPending() {
        return !plan.isEmpty() || succeded() || impossible();
    }
    
    private Action WalkFront() {
        return new Walk(habitant, habitant.sensorData.FrontCell);
    }

    public Action createAction() {
        return plan.head();
    }

    // #### Practical Reasoning Agent Control Loop ####
    //
    // Since doAction() may only apply one action at a time, it is necessary to split the loop
    // with additional checks: ActionExecuted. This way, the expected flow is delivered
    // without leaving other agents' actions pending.
    //
    // Note that the next perception is already available when doAction() is called.
    public void doAction() {
        if (plan.isEmpty()) {
            Belief.brf(beliefs, habitant, habitant.sensorData);
            doOptions();
            doFilter();
            plan = doPlan();
        }
        if(!ActionExecuted && actionsPending()) {
            createAction().apply();
            //ActionExecuted = true;
            return; // Let other agents run their doAction()
        }
        if (!actionsPending()) { // Plan finished (through completion or premature end)
            intentions.RemoveAt(0);
        }
        // Uncomment when the rest works...
        /*
        if(ActionExecuted) {
            Belief.brf(beliefs, habitant, habitant.sensorData);
            if (reconsider()) {
                doOptions();
                doFilter();
            }
            if (!sound()) {
                plan = doPlan();
            }
        }
        */
    }

    public HabitantDeliberative (Habitant habitant) {
        this.habitant  = habitant;
        ActionExecuted = false;
    }
}