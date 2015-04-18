using System;
using System.Threading;

public class BlockingQueue<T> {
	private ConcurrentQueue<T> queue;
	private static Semaphore semaphore;

	public BlockingQueue(ConcurrentQueue<T> queue) {
		this.queue = queue;
		semaphore = new Semaphore(0,Int32.MaxValue);
	}

	public void Enqueue (T obj) {
		queue.Enqueue (obj);
		semaphore.Release(1);
	}
	
	public T Dequeue () {
		semaphore.WaitOne();
		return queue.Dequeue();
	}
}
