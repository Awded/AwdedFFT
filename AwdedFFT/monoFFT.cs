using System;
using System.Collections.Generic;
using CSCore.DSP;

namespace AwdedFFT {
    public class MonoFFT:FftProvider {
        public MonoFFT(FftSize fftSize) : base(1, fftSize) { }
        public void Add(float sample) {
            base.Add(new float[] { sample }, 1);
        }
    }
}
