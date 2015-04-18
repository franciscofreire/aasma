using System.Threading;

public class AgentControl {
	
	//These guys cannot be changed...
	public readonly WorldInfo worldInfo;
	public readonly Agent agent;
	
	private object newSensorDataLock=new object();
//	private SensorData currentSensorData;
	
	private Action agentAction;

	private Thread thread;

	//public BlockingQueue<SensorData> sensorData = 
	//	new BlockingQueue<SensorData>(new ConcurrentQueue<sensorData>());

	public AgentControl(WorldInfo worldInfo, Agent agent) {
		this.worldInfo = worldInfo;
		this.agent = agent;
		this.thread = new Thread(OnTick);
		this.thread.Start ();
	}
	 
	public void OnTick() {
		while (true) {
			CollectActionAndPutInWorldInfoActionQueue();
			UpdateSensorData();
			//SignalActionRequest();
		}
	}
	
	private void UpdateSensorData() {
//		SensorData sensorData = ComputeSensorData(agent);
//		lock (newSensorDataLock) {
//			currentSensorData = sensorData;
//		}
	}
	
	private void CollectActionAndPutInWorldInfoActionQueue() {
		Action action = agent.doAction(/*sensor*/);
		worldInfo.pendingActionsQueue.Enqueue (action);
	}
	
	
}
