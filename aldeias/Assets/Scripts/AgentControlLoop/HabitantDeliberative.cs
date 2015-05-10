using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HabitantDeliberative : AgentImplementation {
    private Habitant habitant;

    private Beliefs         beliefs    = new Beliefs();
    private List<Desire>    desires    = new List<Desire>();
    private List<Intention> intentions = new List<Intention>();

    private Plan plan = new Plan();

    public void doOptions() {

    }
    public void doFilter() {

    }
    public void doPlan() {

    }
    public bool succeded() {
        return false;
    }
    public bool impossible() {
        return false;
    }

    public Action doAction() {
        return null;
    }

    public HabitantDeliberative (Habitant habitant) {
        this.habitant = habitant;
    }
}