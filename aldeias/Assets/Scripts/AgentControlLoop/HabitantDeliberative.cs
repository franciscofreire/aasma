using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class HabitantDeliberative : AgentImplementation {
    private Habitant habitant;

    private Beliefs        beliefs;
    private Attitudes      attitudes;
    private Desires        desires;
    private Intentions     intentions;

    private Plan plan = new Plan();

    private bool ActionExecuted {
        get; set;
    }

    public Desires Options(Beliefs beliefs, Intentions intentions) {//FIXME: Doesn't take current intentions into account.
        return attitudes.AllAttitudes.
            Where(a=>a.IsDesirableAccordingTo(beliefs));
    }

    // Choose the three most important desires, converting them to intentions
    // The list is ordered by importance in decrescent order
    public Intentions Filter(Beliefs beliefs, Desires ddesires) {
        return Intentions.From(
            desires
            .Select(d=>new KeyValuePair<float,Desire>(d.RelevanceAccordingTo(beliefs),d))
            .Aggregate ((best,curr)=>curr.Key > best.Key ? curr : best)
            .Value);
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
        return intentions.First().createPlan(beliefs);
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
        if (plan.isEmpty() && habitant.sensorData.AdjacentCells.Count > 0) {
            Belief.brf(beliefs, CurrentPercept);
            doOptions(beliefs);
            doFilter();
            if (intentions.Count == 0) // Should this happen?
                return; 
            plan = doPlan();
        }
        if(!ActionExecuted && actionsPending()) {
            createAction().apply();
            //ActionExecuted = true;
            return; // Let other agents run their doAction()
        }
        if (!actionsPending() && intentions.Count > 0) { // Plan finished (through completion or premature end)
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
    /*public IEnumerable<Action> PraticalAgentLoop() {
        var beliefs = new Beliefs(this);
        var intentions = new Intentions(this);
        while(true) {
            var percept = getPercept();
            beliefs = beliefRevisionFunction(beliefs, percept);
            var desires = options(beliefs, intentions);
            intentions = filter(beliefs, desires, intentions);
            var plan = plan(beliefs, intentions);
            while(!(plan.isEmpty || succeeded(intentions, beliefs) || impossible(intentions, beliefs))) {
                var action = plan.Head;
                yield return action;
                plan = plan.tail;
                percept = getPercept();
                beliefs = beliefRevisionFunction(beliefs, percept);
                if(reconsider(intentions, beliefs)) {
                    desires = options(beliefs, intentions);
                    intentions = filter(beliefs, desires, intentions);
                }
                if(!sound(plan, intentions, beliefs)) {
                    plan = plan(beliefs, intentions);
                }
            }
        }
    }*/

    public Percept CurrentPercept {
        get {
            return new Percept(habitant, habitant.sensorData);
        }
    }

    public HabitantDeliberative (Habitant habitant) {
        this.habitant  = habitant;
        ActionExecuted = false;
        
        beliefs    = new Beliefs(habitant);
        attitudes  = new Attitudes(habitant);
        desires    = new List<Attitude>();
        intentions = new List<Attitude>();
    }
}

public class Percept {
    public readonly Habitant Habitant;
    public readonly SensorData SensorData;
    public Percept(Habitant h, SensorData s) {
        Habitant = h;
        SensorData = s;
    }
}