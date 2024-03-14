using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Synthic.Native
{
    public interface INativeObject
    {
        public bool Allocated { get; }
        internal void ReleaseResources();
    }
}