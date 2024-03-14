using System.Collections;
using System.Collections.Generic;
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
    double deltaTime = 0;

    double dAttackTime = 0.1;
    double dDecayTime = 0.01;
    double dSustainAmplitude = 0.8;
    double dReleaseTime = 0.2;
    double dStartAmplitude = 1;
    double dTriggerOffTime = 0;
    double dTriggerOnTime = 0;

    double currentAmplitude = 0.0;
    double targetAmplitude = 0.0;

    public KEYS keys;

    private KeyCode keyCode;

    double lastDspTime;

    SawWave sawAudioWave;
    SquareWave squareAudioWave;
    SinusWave sinusAudioWave;

    SinusWave amplitudeModulationOscillator;
    SawWave frequencyModulationOscillator;

    public bool autoPlay;

    [Header("Volume / Frequency")]
    [Range(0.0f, 1.0f)]
    float masterVolume = 0.1f;
    float volumeModifier = 0;
    float newVolumeModifier = 0;
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

    [Header("Amplitude Modulation")]
    public bool useAmplitudeModulation;
    [Range(0.2f, 30.0f)]
    public float amplitudeModulationOscillatorFrequency = 1.0f;
    [Header("Frequency Modulation")]
    public bool useFrequencyModulation;
    [Range(0.2f, 30.0f)]
    public float frequencyModulationOscillatorFrequency = 1.0f;
    [Range(1.0f, 100.0f)]
    public float frequencyModulationOscillatorIntensity = 10.0f;

    [Header("Out Values")]
    [Range(0.0f, 1.0f)]
    public float amplitudeModulationRangeOut;
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

        amplitudeModulationOscillator = new SinusWave();
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
            dTriggerOnTime = AudioSettings.dspTime;
            targetAmplitude = dStartAmplitude;
        }
        if (Input.GetKeyUp(keyCode))
        {
            isKeyDown = false;
            dTriggerOffTime = AudioSettings.dspTime;
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
        { // go through data chunk
            preciseDspTime = currentDspTime + i * dspTimeStep;
            double signalValue = 0.0;
            double currentFreq = mainFrequency;
            if (useFrequencyModulation)
            {
                //double freqOffset = (frequencyModulationOscillatorIntensity * mainFrequency * 0.75) / 100.0;
                double modulationAmount = frequencyModulationOscillator.calculateSignalValue(preciseDspTime, frequencyModulationOscillatorFrequency);
            //    currentFreq += mapValueD(frequencyModulationOscillator.calculateSignalValue(preciseDspTime, frequencyModulationOscillatorFrequency), -1.0, 1.0, -freqOffset, freqOffset);
            //    frequencyModulationRangeOut = (float)frequencyModulationOscillator.calculateSignalValue(preciseDspTime, frequencyModulationOscillatorFrequency) * 0.5f + 0.5f;
                currentFreq += modulationAmount * frequencyModulationOscillatorIntensity;
            }
            else
            {
                frequencyModulationRangeOut = 0.0f;
            }

            if (useSinusAudioWave)
            {
                signalValue += Math.Sin(preciseDspTime * 2.0 * Math.PI * mainFrequency);
            }
            if (useSawAudioWave)
            {
                signalValue += sawAudioWave.calculateSignalValue(preciseDspTime, currentFreq);
            }
            if (useSquareAudioWave)
            {
                signalValue += squareAudioWave.calculateSignalValue(preciseDspTime, currentFreq);
            }

            if (useAmplitudeModulation)
            {
                signalValue *= mapValueD(amplitudeModulationOscillator.calculateSignalValue(preciseDspTime, amplitudeModulationOscillatorFrequency), -1.0, 1.0, 0.0, 1.0);
                amplitudeModulationRangeOut = (float)amplitudeModulationOscillator.calculateSignalValue(preciseDspTime, amplitudeModulationOscillatorFrequency) * 0.5f + 0.5f;
            }
            else
            {
                amplitudeModulationRangeOut = 0.0f;
            }

            double amplitudeDelta = targetAmplitude - currentAmplitude;
            currentAmplitude += amplitudeDelta * (1.0 - Math.Exp(-dspTimeStep / dAttackTime));
            signalValue *= currentAmplitude / 10;

            float x = (float)signalValue;

            for (int j = 0; j < channels; j++)
            {
                data[i * channels + j] = x;
            }
        }

    }
    double mapValueD(double referenceValue, double fromMin, double fromMax, double toMin, double toMax)
    {
        /* This function maps (converts) a Double value from one range to another */
        return toMin + (referenceValue - fromMin) * (toMax - toMin) / (fromMax - fromMin);
    }

    double GetAmplitude()
    {
        double amplitude = 0;
        deltaTime = currentDspTime - dTriggerOnTime;

        if (isKeyDown)
        {
            if (deltaTime <= dAttackTime)
            {
                amplitude = (deltaTime / dAttackTime) * dStartAmplitude;
            }
            if(deltaTime > dAttackTime && deltaTime <= (dAttackTime + dDecayTime))
            {
                amplitude = ((deltaTime - dAttackTime) / dDecayTime) * (dSustainAmplitude - dStartAmplitude) + dStartAmplitude;
            }
            if(deltaTime > dDecayTime + dAttackTime)
            {
                amplitude = dSustainAmplitude;
            }
        }
        else
        {
            amplitude = ((currentDspTime - dTriggerOffTime) / dReleaseTime) * (0.0 - dSustainAmplitude) + dSustainAmplitude;
        }

        if (amplitude <= 0.0001)
        {
            amplitude = 0;
        }

        return amplitude;
    }
}
