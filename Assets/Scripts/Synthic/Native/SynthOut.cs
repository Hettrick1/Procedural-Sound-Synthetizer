using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Synthic.Native
{
    public class SynthOut : MonoBehaviour
    {
        [SerializeField] private SynthProvider provider;

        private void OnAudioFilterRead(float[] data, int channels)
        {
            provider.FillBuffer(data, channels);
        }
    }
}

