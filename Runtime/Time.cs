using System.Collections;
using UnityEngine;

namespace Toolbox
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
                double time = Time.realtimeSinceStartup;
                #if UNITY_EDITOR
                if (!Application.isPlaying)
                    time = UnityEditor.EditorApplication.timeSinceStartup;
                #endif
                return time;
            }
        }
    }

    /// <summary>
    /// Note: This uses a really bad method of summing time and does not accurately reflect error.
    /// TODO: Fixing this with proper error correction!!!
    /// </summary>
    public static class CoroutineUtilities
    {
        public static IEnumerator WaitForRealTime(float delay)
        {
            while (true)
            {
                float pauseEndTime = Time.realtimeSinceStartup + delay; //BUG! FIXME!
                while (Time.realtimeSinceStartup < pauseEndTime)
                {
                    yield return 0;
                }
                break;
            }
        }

        public static IEnumerator WaitForTime(float delay)
        {
            while (true)
            {
                float pauseEndTime = Time.timeSinceLevelLoad + delay; //BUG! FIXME!
                while (Time.timeSinceLevelLoad < pauseEndTime)
                {
                    yield return 0;
                }
                break;
            }
        }
    }


}