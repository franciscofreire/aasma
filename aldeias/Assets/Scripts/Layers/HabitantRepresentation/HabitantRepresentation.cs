using UnityEngine;

public class HabitantRepresentation : MonoBehaviour {
    public HabitantQuantitiesRepresentation QuantitiesRepr;
    public HabitantDecisionCycleRepresentation DecisionRepr;

    public void SetHabitant(Habitant h) {
        QuantitiesRepr.SetHabitant(h);
        DecisionRepr.SetHabitant(h);
    }

    public void UpdateRepresentation() {
        QuantitiesRepr.UpdateRepresentation();
        DecisionRepr.UpdateRepresentation();
    }

}