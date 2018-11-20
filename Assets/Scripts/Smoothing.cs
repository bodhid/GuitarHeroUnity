using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Smoothing
{
	private double smoothBpm = 0;
	private double smoothTick = 0;
	public double SmoothBPM(double bpm)
	{
		if (smoothBpm == 0)
		{
			smoothBpm = bpm;
		}
		else
		{
			smoothBpm = (smoothBpm * 0.9d) + (bpm * 0.1d);
		}
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

		return smoothTick;
	}
}
