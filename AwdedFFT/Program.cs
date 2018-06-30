using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Threading;
using CSCore;
using CSCore.Codecs;
using CSCore.DSP;
using CSCore.SoundOut;
using CSCore.SoundIn;
using CSCore.Streams;
using CSCore.Streams.Effects;
using CSCore.CoreAudioAPI;

namespace AudedFFT {
    class Program {
        const int fftMulti = 1;

        static FftSize fftSize = FftSize.Fft128;

        private StereoFFT spectrumProvider;
        private WasapiCapture _soundIn;
        private ISoundOut _soundOut;
        private IWaveSource _source;
        //private static PitchShifter _pitchShifter;
        private bool timer;
        private float[] _leftFFTBuffer;
        private float[] _rightFFTBuffer;
        private float[] _stereoFFTBuffer;
        private String outString = "";
        static void Main(string[] args) {
            int timerMs = 32;
            int fftLatency = 32;
            int i = 0;

            Program program = new Program();

            if(args.Length == 3) {
                int intFftSize = 0;
                Int32.TryParse(args[0], out timerMs);
                Int32.TryParse(args[1], out intFftSize);
                Int32.TryParse(args[2], out fftLatency);
                fftSize = (FftSize)intFftSize;
            }

            program._leftFFTBuffer = new float[(int)fftSize];
            program._rightFFTBuffer = new float[(int)fftSize];
            program._stereoFFTBuffer = new float[(int)fftSize / 2];

            program.Init(fftSize, fftLatency);
            while(program.timer) {
                Thread.Sleep(timerMs);
                if(program.spectrumProvider.GetFftData(program._leftFFTBuffer, program._rightFFTBuffer)) {
                    i = 0;
                    while(i < (int)fftSize / 4) {
                        program._stereoFFTBuffer[i] = program._leftFFTBuffer[i];
                        program._stereoFFTBuffer[i + (int)fftSize / 4] = program._rightFFTBuffer[i];
                        i++;
                    }
                    program.outString = "[";
                    for(i = 0; i < program._stereoFFTBuffer.Length; i++) {
                        program.outString += (20 * (float)Math.Log10(program._stereoFFTBuffer[i])).ToString("0.000").Replace("∞", "200");
                        if(program._stereoFFTBuffer.Length != i + 1)
                            program.outString += ",";
                    }
                    program.outString += "]";
                    Console.WriteLine(program.outString);
                }
            }
        }

        private void SetupSampleSource(ISampleSource aSampleSource, FftSize fftSize) {
            //create a spectrum provider which provides fft data based on some input
            spectrumProvider = new StereoFFT(fftSize);

            //the SingleBlockNotificationStream is used to intercept the played samples
            var notificationSource = new SingleBlockNotificationStream(aSampleSource);
            //pass the intercepted samples as input data to the spectrumprovider (which will calculate a fft based on them)
            notificationSource.SingleBlockRead += (s, a) => { spectrumProvider.Add(a.Left, a.Right); };
            _source = notificationSource.ToWaveSource(16);

        }


        private void Stop() {
            timer = false;

            if(_soundOut != null) {
                _soundOut.Stop();
                _soundOut.Dispose();
                _soundOut = null;
            }
            if(_soundIn != null) {
                _soundIn.Stop();
                _soundIn.Dispose();
                _soundIn = null;
            }
            if(_source != null) {
                _source.Dispose();
                _source = null;
            }


        }
        private void Init(FftSize fftSize, int fftLatency) {
            //open the default device 
            _soundIn = new WasapiLoopbackCapture(fftLatency);
            //Our loopback capture opens the default render device by default so the following is not needed
            //_soundIn.Device = MMDeviceEnumerator.DefaultAudioEndpoint(DataFlow.Render, Role.Console);
            _soundIn.Initialize();

            var soundInSource = new SoundInSource(_soundIn);
            ISampleSource source = soundInSource.ToSampleSource(); //.AppendSource(x => new PitchShifter(x), out _pitchShifter);

            SetupSampleSource(source, fftSize);

            // We need to read from our source otherwise SingleBlockRead is never called and our spectrum provider is not populated
            byte[] buffer = new byte[_source.WaveFormat.BytesPerSecond / 2];
            soundInSource.DataAvailable += (s, aEvent) => {
                int read;
                while((read = _source.Read(buffer, 0, buffer.Length)) > 0)
                    ;
            };


            //play the audio
            _soundIn.Start();
            timer = true;
        }
    }
}
