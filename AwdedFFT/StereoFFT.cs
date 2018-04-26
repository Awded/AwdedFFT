using System;
using System.Collections.Generic;
using CSCore.DSP;

public class StereoFFT {
  private Visualization.BasicSpectrumProvider left;
  private Visualization.BasicSpectrumProvider right;
  public StereoFFT(int channels, int sampleRate, FftSize fftSize){
    left = new Visualization.BasicSpectrumProvider(channels, sampleRate, fftSize);
    right = new Visualization.BasicSpectrumProvider(channels, sampleRate, fftSize);
  }

  public void Add(float leftChannel, float rightChannel) {
    left.Add(leftChannel * 2f, 0.0f);
    right.Add(rightChannel * 2f, 0.0f);
  }

  public bool GetFftData(float[] leftFftResultBuffer,float[]rightFftResultBuffer, object context) {
    return (left.GetFftData(leftFftResultBuffer, context) && right.GetFftData(rightFftResultBuffer, context));
  }
}
