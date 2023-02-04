using UnityEngine;

namespace Toolbox
{
    /// <summary>
    /// Simple, non-persistent singletons class.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DisallowMultipleComponent]
    public abstract class SimpleSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        protected static T _Instance;
        public static T Instance
        {
            get
            {
                if (TypeHelper.IsReferenceNull(_Instance))
                {
                    var go = new GameObject("Singleton - " + typeof(T).Name);
                    _Instance = go.AddComponent<T>();
                }
                return _Instance;
            }
        }

        public static bool Initialized { get; private set; }

        protected virtual void Awake()
        {
            _Instance = this as T;
        }

        protected virtual void Start()
        {
            Initialized = true;
        }

        protected virtual void OnDestroy()
        {

        }
    }
}
