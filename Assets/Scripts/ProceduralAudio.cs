using UnityEngine;
using System;

[RequireComponent(typeof(AudioSource))]
public class ProceduralAudioController : MonoBehaviour
{
    public enum KEYS
    {
        Q,
        W,
        E,
        R,
        T,
        Z
    }
    public double dAttackTime = 0.1;
    double dReleaseTime = 0.2;
    double dStartAmplitude = 0.5;

    double currentAmplitude = 0.0;
    double targetAmplitude = 0.0;

    public KEYS keys;

    private KeyCode keyCode;

    SawWave sawAudioWave;
    SquareWave squareAudioWave;
    SinusWave sinusAudioWave;

    SawWave frequencyModulationOscillator;

    [Header("Volume / Frequency")]
    [Range(0.0f, 1.0f)]
    float masterVolume = 0.1f;
    [Range(0, 2000)]
    public double mainFrequency = 500;
    [Space(10)]

    [Header("Tone Adjustment")]
    public bool useSinusAudioWave;
    [Range(0.0f, 1.0f)]
    public float sinusAudioWaveIntensity = 0.25f;
    [Space(5)]
    public bool useSquareAudioWave;
    [Range(0.0f, 1.0f)]
    public float squareAudioWaveIntensity = 0.25f;
    [Space(5)]
    public bool useSawAudioWave;
    [Range(0.0f, 1.0f)]
    public float sawAudioWaveIntensity = 0.25f;

    [Space(10)]

    [Header("Frequency Modulation")]
    public bool useFrequencyModulation;
    [Range(0.2f, 30.0f)]
    public float frequencyModulationOscillatorFrequency = 1.0f;
    [Range(1.0f, 100.0f)]
    public float frequencyModulationOscillatorIntensity = 10.0f;

    [Header("Out Values")]
    [Range(0.0f, 1.0f)]
    public float frequencyModulationRangeOut;

    bool isKeyDown;

    float mainFrequencyPreviousValue;
    private System.Random RandomNumber = new System.Random();

    private double sampleRate;  // samples per second
                                //private double myDspTime;	// dsp time
    private double dataLen;     // the data length of each channel
    double chunkTime;
    double dspTimeStep;
    double currentDspTime;

    void Awake()
    {
        sawAudioWave = new SawWave();
        squareAudioWave = new SquareWave();
        sinusAudioWave = new SinusWave();

        frequencyModulationOscillator = new SawWave();

        sampleRate = AudioSettings.outputSampleRate;

        switch (keys)
        {
            case KEYS.Q :
                keyCode = KeyCode.Q;
                break;
            case KEYS.W :
                keyCode = KeyCode.W;
                break;
            case KEYS.E :
                keyCode = KeyCode.E;
                break;
            case KEYS.R :
                keyCode = KeyCode.R;
                break;
            case KEYS.T :
                keyCode = KeyCode.T;
                break;
            case KEYS.Z :
                keyCode = KeyCode.Z;
                break;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(keyCode))
        {
            isKeyDown = true;
            targetAmplitude = dStartAmplitude;
        }
        if (Input.GetKeyUp(keyCode))
        {
            isKeyDown = false;
            targetAmplitude = 0;
        }

    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        currentDspTime = AudioSettings.dspTime;
        dataLen = data.Length / channels;   // the actual data length for each channel
        chunkTime = dataLen / sampleRate;   // the time that each chunk of data lasts
        dspTimeStep = chunkTime / dataLen;  // the time of each dsp step. (the time that each individual audio sample (actually a float value) lasts)

        double preciseDspTime;
        for (int i = 0; i < dataLen; i++)
        { 
            preciseDspTime = currentDspTime + i * dspTimeStep;
            double signalValue = 0.0;
            double currentFreq = mainFrequency;
            if (useFrequencyModulation)
            {
                // Descending sawtooth function with variable tempo
                double sawtoothPhase = -preciseDspTime * 2.0 * frequencyModulationOscillatorFrequency;
                sawtoothPhase -= Math.Floor(sawtoothPhase);
                double sawtoothModulation = Math.Exp(sawtoothPhase * frequencyModulationOscillatorIntensity);

                // Apply sawtooth modulation and frequency modulation
                currentFreq += sawtoothModulation * frequencyModulationOscillatorIntensity;
            }
            else
            {
                frequencyModulationRangeOut = 0.0f;
            }

            if (useSinusAudioWave)
            {
                signalValue += sinusAudioWaveIntensity * sinusAudioWave.calculateSignalValue(preciseDspTime, currentFreq);
            }
            if (useSawAudioWave)
            {
                signalValue += sawAudioWaveIntensity * sawAudioWave.calculateSignalValue(preciseDspTime, currentFreq);
            }
            if (useSquareAudioWave)
            {
                signalValue += squareAudioWaveIntensity * squareAudioWave.calculateSignalValue(preciseDspTime, currentFreq);
            }

            double amplitudeDelta = targetAmplitude - currentAmplitude;
            if (amplitudeDelta > 0)
            {
                currentAmplitude += amplitudeDelta * (1.0 - Math.Exp(-dspTimeStep / dAttackTime));
            }
            else
            {
                currentAmplitude += amplitudeDelta * (1.0 - Math.Exp(-dspTimeStep / dReleaseTime));
            }
            signalValue *= currentAmplitude / 10;

            float x = (float)signalValue;

            for (int j = 0; j < channels; j++)
            {
                data[i * channels + j] = x;
            }
        }

    }
}
