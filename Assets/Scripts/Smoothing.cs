using UnityEngine;
public class Smoothing
{
	private double visualOffset=0;
	private double smoothBpm = 0;
	private double smoothTick = 0;
	public Smoothing(double _visualOffset)
	{
		visualOffset = _visualOffset;
	}
	public double SmoothBPM(double bpm)
	{
		smoothBpm = (smoothBpm == 0) ? bpm : ((smoothBpm * 0.9d) + (bpm * 0.1d));
		return smoothBpm;
	}
	public double SmoothTick(double tick, uint resolution)
	{
		double beatsPerSecond = smoothBpm / 60d;
		double secondsPassed = Time.deltaTime;
		double beatsPassed = beatsPerSecond * secondsPassed;
		double ticksPassed = beatsPassed * resolution;
		if (!double.IsNaN(ticksPassed) && smoothBpm > 0) smoothTick += ticksPassed;

		smoothTick= (smoothTick * 0.9d) + (tick * 0.1d);

		double offsetSeconds = visualOffset * 0.001d;
		double offsetBeats = beatsPerSecond * offsetSeconds;
		double offsetTicks = offsetBeats * resolution;

		return smoothTick + offsetTicks;
	}
}
