using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class HabitantDeliberative : AgentImplementation {
    private Habitant habitant;

    private Beliefs        beliefs;
    private Attitudes      attitudes;
    private List<Attitude> desires;
    private List<Attitude> intentions;

    public Attitude CurrentIntention {
        get {
            return intentions.FirstOrDefault();
        }
    }

    private Plan plan;

    private bool ActionExecuted {
        get; set;
    }

    public void updateOptions() {
        desires = new List<Attitude>();
        foreach (Attitude a in attitudes.AllAttitudes) {
            if (a.isDesirable(beliefs)) {
                desires.Add(a);
            }
        }
    }

    // Choose the three most important desires, converting them to intentions
    // The list is ordered by importance in decrescent order
    public void updateFilter() {
        List<Attitude> candidates = new List<Attitude>();

        foreach (Attitude desire in desires) {
            filterDesire(candidates, desire);
        }

        intentions = candidates;
        
        // We clear the plans of less important intentions, so that when they
        // are at the head, they don't continue plans they had pending
        for (int i = 1; i < intentions.Count; i++)
            intentions[i].clearPlan();
    }

    private void filterDesire(List<Attitude> candidates, Attitude desire) {
        const int MAX_COUNT = 3;
        for (int i = 0 ; i < candidates.Count; i++) {
            Attitude candidate = candidates.ElementAt(i);

            // If the desire is more important than some candidate, we add it
            if (candidate.Importance < desire.Importance) {
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
    public Plan updatePlan() {
        return intentions.First().updatePlan(beliefs);
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
        return plan.isSound(beliefs);
    }

    private bool actionsPending() {
        return !plan.isEmpty() || succeded() || impossible();
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
        // We are trapped!
        if (habitant.sensorData.AdjacentCells.Count == 0) {
            plan.clear();

            Action a;

            // We could be surrounded by food, so pick it up
            if (beliefs.PickableFood.RelevantCells.Count > 0) {
                a = new PickupFood(habitant, habitant.closestCell(beliefs.PickableFood.RelevantCells));
            }
            // Dead end? Then turn back
            else {
                /* a = Action.RunAwayOrWalkRandomly(habitant);*/
                 a = new TurnOppositeDirection(habitant, Vector2I.INVALID);
            }
            a.apply();
            ActionExecuted = true;

            return;
        }

        // Nothing planned
        if (plan.isEmpty()) {
            Belief.brf(beliefs, CurrentPercept);
            updateOptions();
            updateFilter();

            // We couldn't generate intentions, tough luck
            if (intentions.Count == 0) {
                return; 
            }

            plan = updatePlan();
        }

        // We have a plan, so do an action
        if(!ActionExecuted && actionsPending()) {
            Action a = createAction();
            a.apply();

            ActionExecuted = true;

            // Let other agents run their doAction()
            return;
        }

        // Plan finished (through completion or premature end), intention satisfied
        if (!actionsPending() && intentions.Count > 0) {
            intentions.RemoveAt(0);
        }

        // After executing an action, we reconsider our way in life
        if(ActionExecuted && actionsPending()) {
            Belief.brf(beliefs, CurrentPercept);
            /*
            if (reconsider()) {
                doOptions();
                doFilter();
            }
            */
            if (!sound()) {
                plan = updatePlan();
            }

            // Done reconsidering, let's continue the plan
            ActionExecuted = false;
         }
    }

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
        plan       = new Plan(new Explore(habitant));
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