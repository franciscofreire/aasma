using UnityEngine;
using System.Collections;

public class AgentSpawner : Layer {
	public const int MAX_AGENTS = 100;

	private GameObject[] agents;
	public GameObject agentModel;

	public override void CreateObjects() {
		agents = new GameObject[MAX_AGENTS];

		agents[15] = (GameObject) Instantiate(agentModel, worldXZToVec3(15,15), Quaternion.identity);
		agents[15].transform.parent = this.transform;
		agents[15].SetActive(false);
	}

	public override void ApplyWorldInfo() {
		agents[15].SetActive(true);
	}
}