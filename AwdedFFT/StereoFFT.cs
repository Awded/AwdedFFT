using System;
using System.Collections.Generic;
using CSCore.DSP;

public class StereoFFT {
    private AwdedFFT.MonoFFT left;
    private AwdedFFT.MonoFFT right;
    public StereoFFT(FftSize fftSize) {
        left = new AwdedFFT.MonoFFT(fftSize);
        right = new AwdedFFT.MonoFFT(fftSize);
    }

    public void Add(float leftChannel, float rightChannel) {
        left.Add(leftChannel);
        right.Add(rightChannel);
    }

    public bool GetFftData(float[] leftFftResultBuffer, float[] rightFftResultBuffer) {
        return (left.GetFftData(leftFftResultBuffer) && right.GetFftData(rightFftResultBuffer));
    }
}
