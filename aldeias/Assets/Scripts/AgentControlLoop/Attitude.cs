using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class Attitude {
    private Agent agent;
    private Vector2I target;

    // Used by the Filter method
    // The bigger the value, the bigger the chance of being an Intention
    public float importance {
        get; set;
    }

    public Plan plan {
        get; set;
    }

    public Attitude(Agent agent, Vector2I target) {
        this.agent  = agent;
        this.target = target;
        plan = createPlan();
    }

    public abstract Plan createPlan();
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
    public override Plan createPlan() {
        return new Plan();
    }

    public ExpandTribe(Agent agent, Vector2I target) : base(agent, target) {}
}

public class ConquerTribe : Attitude {
    public override Plan createPlan() {
        return new Plan();
    }
    
    public ConquerTribe(Agent agent, Vector2I target) : base(agent, target) {}
}

public class MaintainEnergy : Attitude {
    public override Plan createPlan() {
        return new Plan();
    }
    
    public MaintainEnergy(Agent agent, Vector2I target) : base(agent, target) {}
}

public class IncreaseFoodStock : Attitude {
    public override Plan createPlan() {
        return new Plan();
    }
    
    public IncreaseFoodStock(Agent agent, Vector2I target) : base(agent, target) {}
}

public class IncreaseWoodStock : Attitude {
    public override Plan createPlan() {
        return new Plan();
    }
    
    public IncreaseWoodStock(Agent agent, Vector2I target) : base(agent, target) {}
}

public class HelpDefense : Attitude {
    public override Plan createPlan() {
        return new Plan();
    }
    
    public HelpDefense(Agent agent, Vector2I target) : base(agent, target) {}
}

public class HelpAttack : Attitude {
    public override Plan createPlan() {
        return new Plan();
    }
    
    public HelpAttack(Agent agent, Vector2I target) : base(agent, target) {}
}