using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Habitat {
    public Vector2I corner_pos;
    public List<Animal> animals = new List<Animal>();
    
    public Habitat(int x, int y) {
        this.corner_pos = new Vector2I(x, y);
    }
    
    public Habitat() {
    }
    
    public void RemoveAnimal(Animal a) {
        animals.Remove(a);
    }
}