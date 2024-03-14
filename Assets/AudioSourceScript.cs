using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSourceScript : MonoBehaviour
{
    public float fundementalToneFrequency;
    public float gain;

    public float volume;

    public float attackTime;
    public float decayTime;

    public float[] harmonicStrengths = new float[12];

    private float samplingFrequency;     

    private AudioSource ad_source;

    bool isPlaying;

    private float[] phase = new float[12];

    void Start()
    {
        samplingFrequency = AudioSettings.outputSampleRate;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isPlaying = true;
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            isPlaying = false;
        }
        if (isPlaying)
        {
            gain += Mathf.Lerp(0, volume, Time.deltaTime / attackTime);
            gain = Mathf.Clamp(gain, 0, volume);
        }
        else
        {
            gain -= Mathf.Lerp(volume, 0, Time.deltaTime / decayTime);
            gain = Mathf.Clamp(gain, 0, 0);
        }
    }

    void OnAudioFilterRead(float[] data, int channels)
    {

        float timeAtTheBeginig = (float)AudioSettings.dspTime;

        int currentSampleIndex = 0;
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = ReturnSuperimposedHarmonicsSeries() * gain;

            currentSampleIndex++;

            if (channels == 2)
            {
                data[i + 1] = data[i]; // if stereo, copy the one ear to the other, and simple jump over the channel 1 in the next iteration
                i++;
            }
        }
    }


    public float ReturnSuperimposedHarmonicsSeries()
    {
        float superImposed = 0.0f;

        for (int i = 1; i <= 12; i++)
        {
            float harmonicFrequency = fundementalToneFrequency * i;
            float increment = harmonicFrequency * 2f * Mathf.PI / samplingFrequency;

            phase[i - 1] = phase[i - 1] + increment;
            if (phase[i - 1] > 2.0f * Mathf.PI) phase[i - 1] = 0;

            superImposed += Mathf.Sin(phase[i - 1]) * harmonicStrengths[i - 1];
        }



        return superImposed;
    }
}
