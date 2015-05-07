using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class WorldInfo : MonoBehaviour {
    public delegate void WorldChangeListener();
    private List<WorldChangeListener> changeListeners = new List<WorldChangeListener>();
    public void AddChangeListener(WorldChangeListener func) {
        changeListeners.Add(func);
    }
    private void NotifyChangeListeners() {
        foreach(WorldChangeListener listener in changeListeners) {
            listener();
        }
    }
    
    public delegate void WorldCreationListener();
    private List<WorldCreationListener> creationListeners = new List<WorldCreationListener>();
    public void AddCreationListener(WorldCreationListener func) {
        creationListeners.Add(func);
    }
    private void NotifyCreationListeners() {
        foreach(WorldCreationListener listener in creationListeners) {
            listener();
        }
    }
    
    public delegate void TreeDiedListener(Vector2I pos);
    private List<TreeDiedListener> treeListeners = new List<TreeDiedListener>();
    public void AddTreeDiedListener(TreeDiedListener func) {
        treeListeners.Add(func);
    }
    public void NotifyTreeDiedListeners(Vector2I pos) {
        foreach(TreeDiedListener listener in treeListeners) {
            listener(pos);
        }
    }
    
    public delegate void AnimalDiedListener(Animal a);
    private List<AnimalDiedListener> animalListeners = new List<AnimalDiedListener>();
    public void AddAnimalDiedListener(AnimalDiedListener func) {
        animalListeners.Add(func);
    }
    public void NotifyAnimalDiedListeners(Animal a) {
        foreach(AnimalDiedListener listener in animalListeners) {
            listener(a);
        }
    }
    
    public delegate void HabitantDiedListener(Habitant h);
    private List<HabitantDiedListener> habitantListeners = new List<HabitantDiedListener>();
    public void AddHabitantDiedListener(HabitantDiedListener func) {
        habitantListeners.Add(func);
    }
    public void NotifyHabitantDiedListeners(Habitant h) {
        foreach(HabitantDiedListener listener in habitantListeners) {
            listener(h);
        }
    }
    
    public delegate void AnimalDeletedListener();
    private List<AnimalDeletedListener> animalDeletedListeners = new List<AnimalDeletedListener>();
    public void AddAnimalDeletedListener(AnimalDeletedListener func) {
        animalDeletedListeners.Add(func);
    }
    public void NotifyAnimalDeletedListeners(Animal a) {
        foreach(AnimalDeletedListener listener in animalDeletedListeners) {
            listener();
        }
    }
    
    public delegate void HabitantDeletedListener();
    private List<HabitantDeletedListener> habitantDeletedListeners = new List<HabitantDeletedListener>();
    public void AddHabitantDeletedListener(HabitantDeletedListener func) {
        habitantDeletedListeners.Add(func);
    }
    public void NotifyHabitantDeletedListeners(Habitant h) {
        foreach(HabitantDeletedListener listener in habitantDeletedListeners) {
            listener();
        }
    }
}