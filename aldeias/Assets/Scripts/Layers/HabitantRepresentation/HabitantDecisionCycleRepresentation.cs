using UnityEngine;


public class HabitantDecisionCycleRepresentation : MonoBehaviour {

    Habitant habitant;

    public string ActiveAttitude;


    public void SetHabitant(Habitant h) {
        habitant = h;
    }

    public void UpdateRepresentation() {
        ActiveAttitude = ""+habitant.agentImplDeliberative.CurrentIntention;
    }

}