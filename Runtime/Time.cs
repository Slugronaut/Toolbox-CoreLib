using UnityEngine;

namespace Peg.Lib
{
    public static class TimeUtil
    {
        /// <summary>
        /// Returns the current time since start. Works in the editor and in playmode.
        /// </summary>
        public static double RealTimeSinceStart
        {
            get
            {
                double time = Time.realtimeSinceStartupAsDouble;
                #if UNITY_EDITOR
                if (!Application.isPlaying)
                    time = UnityEditor.EditorApplication.timeSinceStartup;
                #endif
                return time;
            }
        }
    }

}