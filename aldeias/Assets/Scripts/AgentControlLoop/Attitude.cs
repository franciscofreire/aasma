using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Attitudes {
    
    public ExpandTribe ExpandTribe;
    public ConquerTribe ConquerTribe;
    public MaintainEnergy MaintainEnergy;
    public IncreaseFoodStock IncreaseFoodStock;
    public IncreaseWoodStock IncreaseWoodStock;
    public HelpAttack HelpAttack;
    public HelpDefense HelpDefense;

    public Attitudes(Habitant h) {
        ExpandTribe = new ExpandTribe(h);
        ConquerTribe = new ConquerTribe(h);
        MaintainEnergy = new MaintainEnergy(h);
        IncreaseFoodStock = new IncreaseFoodStock(h);
        IncreaseWoodStock = new IncreaseWoodStock(h);
        HelpAttack = new HelpAttack(h);
        HelpDefense = new HelpDefense(h);
    }
    
    public IEnumerable<Attitude> AllAttitudes {
        get {
            yield return ExpandTribe;
            yield return ConquerTribe;
            yield return MaintainEnergy;
            yield return IncreaseFoodStock;
            yield return IncreaseWoodStock;
            yield return HelpAttack;
            yield return HelpDefense;
        }
    }
}

public abstract class Attitude {
    protected Habitant habitant;

    // Used by the Filter method
    // The bigger the value, the bigger the chance of being an Intention
    public float importance {
        get; set;
    }

    public Attitude(Habitant habitant) {
        this.habitant = habitant;
    }

    public abstract Plan createPlan(Beliefs beliefs);

    public abstract bool isDesirable(Beliefs beliefs);
}

/*
Lista candidata de Desires/Intentions:
 -> Expandir tribo
    - Verificar se há flags
    - Procurar território neutro
    - Colocar flags
 -> Conquistar tribo adversária
    - Matar os outros
    - "Roubar" Território
 -> Manter energia em níveis aceitáveis
    - Evitar animais e inimigos
    - Procurar animais ou comida
    - Matar animais
    - Recolher comida
 -> Aumentar reservas comida
    Procurar animais ou comida
    - Matar animais
    - Recolher comida
    - Largar comida no meeting point
 -> Aumentar as reservas de madeira
    - Procurar árvores e troncos
    - Cortar árvores
    - Recolher madeira
    - Largar madeira no meeting point
 -> Ajudar os compatriotas - modo defesa
 -> Ajudar os compatriotas - modo ataque
 */

public class ExpandTribe : Attitude {
    public override bool isDesirable(Beliefs beliefs) {
        return beliefs.UnclaimedTerritoryIsNear.IsActive;
    }

    public override Plan createPlan(Beliefs beliefs) {
        Plan plan = new Plan();
        IList<Vector2I> targets = beliefs.UnclaimedTerritoryIsNear.RelevantCells;
        foreach (Vector2I target in targets) {
            plan.addPathFinding(habitant, target);
            plan.add(new PlaceFlag(habitant, target));
        }
        return plan;
    }

    public ExpandTribe(Habitant habitant) : base(habitant) {}
}

public class ConquerTribe : Attitude {
    public override bool isDesirable(Beliefs beliefs) {
        return beliefs.NearEnemyTribe.IsActive;
    }

    public override Plan createPlan(Beliefs beliefs) {
        return new Plan();
    }

    public ConquerTribe(Habitant habitant) : base(habitant) {}
}

public class MaintainEnergy : Attitude {
    public override bool isDesirable(Beliefs beliefs) {
        return beliefs.HabitantHasLowEnergy.IsActive;
    }

    public override Plan createPlan(Beliefs beliefs) {
        return new Plan();
    }

    public MaintainEnergy(Habitant habitant) : base(habitant) {}
}

public class IncreaseFoodStock : Attitude {
    public override bool isDesirable(Beliefs beliefs) {
        return beliefs.TribeHasLowFoodLevel.IsActive;
    }

    public override Plan createPlan(Beliefs beliefs) {
        return new Plan();
    }

    public IncreaseFoodStock(Habitant habitant) : base(habitant) {}
}

public class IncreaseWoodStock : Attitude {
    public override bool isDesirable(Beliefs beliefs) {
        return beliefs.TribeHasLittleFlags.IsActive;
    }

    public override Plan createPlan(Beliefs beliefs) {
        return new Plan();
    }

    public IncreaseWoodStock(Habitant habitant) : base(habitant) {}
}

public class HelpDefense : Attitude {
    public override bool isDesirable(Beliefs beliefs) {
        return beliefs.TribeIsBeingAttacked.IsActive;
    }

    public override Plan createPlan(Beliefs beliefs) {
        return new Plan();
    }

    public HelpDefense(Habitant habitant) : base(habitant) {}
}

public class HelpAttack : Attitude {
    public override bool isDesirable(Beliefs beliefs) {
        return beliefs.TribeIsBeingAttacked.IsActive; // FIXME: Maybe another belief
    }

    public override Plan createPlan(Beliefs beliefs) {
        return new Plan();
    }

    public HelpAttack(Habitant habitant) : base(habitant) {}
}