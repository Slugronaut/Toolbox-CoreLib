using System.Collections;
using Toolbox.AutoCreate;
using UnityEngine;

namespace Toolbox
{
    /// <summary>
    /// Universal object for hold references to coroutines so that other non-gameobjects can use them easily.
    /// </summary>
    [AutoCreate(CreationTime = RuntimeInitializeLoadType.AfterSceneLoad)]
    public class GlobalCoroutine
    {
        public static GlobalCoroutine Instance;

        GlobalCoroutineOwnerBehaviour Owner;

        void AutoAwake()
        {
            Instance = this;
            var go = GameObject.Find("Screen Fade Couroutine Holder");
            go = new GameObject("Screen Fade Couroutine Holder");
            go.hideFlags = HideFlags.HideAndDontSave;
            GameObject.DontDestroyOnLoad(go);
            Owner = go.AddComponent<GlobalCoroutineOwnerBehaviour>();
        }

        void AutoDestroy()
        {
            if (Owner != null && Owner.gameObject is not null)
            {
                GameObject.Destroy(Owner.gameObject);
                Owner = null;
            }
        }

        public static Coroutine Start(IEnumerator action)
        {
            return Instance.Owner.StartCoroutine(action);
        }

        public static void Stop(Coroutine action)
        {
            Instance.Owner.StopCoroutine(action);
        }
    }


    /// <summary>
    /// Used internall by GlobalCoroutineOwner to attach coroutines to a hidden dummy gameobject.
    /// </summary>
    public class GlobalCoroutineOwnerBehaviour : MonoBehaviour { }
}
