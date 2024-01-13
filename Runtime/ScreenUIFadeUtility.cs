using System;
using System.Collections;
using Peg.AutoCreate;
using UnityEngine;
using UnityEngine.UIElements;

namespace Peg.Systems
{
    /// <summary>
    /// Utility class for autonomously handling a screen color fade.
    /// </summary>
    [AutoCreate(CreationTime = RuntimeInitializeLoadType.AfterSceneLoad)]
    public class ScreenUIFadeUtility
    {
        Color CachedColor = Color.black;
        float Alpha = 0;
        bool Fading;
        VisualElement Screen;
        UIDocument Doc;


        static ScreenUIFadeUtility _Instance;
        public static ScreenUIFadeUtility Instance => _Instance;

        void AutoAwake()
        {
            //we are using Start instead of Awake because the global coroutine object needs to initialize first in awake
            _Instance = this;
            var go = new GameObject("Screen Fade");

#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                go.hideFlags = HideFlags.DontSave;
                //go.hideFlags = HideFlags.HideAndDontSave;
                GameObject.DontDestroyOnLoad(go);
            }
#endif

            Doc = go.AddComponent<UIDocument>();
            Doc.visualTreeAsset = ScriptableObject.CreateInstance<VisualTreeAsset>();
            Doc.visualTreeAsset.name = "Panel";

            Doc.panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
            Doc.panelSettings.name = "Settings";
            Doc.panelSettings.sortingOrder = 1000;

            Screen = new VisualElement();
            Doc.rootVisualElement.Add(Screen);

            Screen.name = "Screen";
            Screen.style.backgroundColor = Color.blue;
            Screen.style.opacity = 0.0f;
            Screen.style.width = UnityEngine.Screen.width;
            Screen.style.height = UnityEngine.Screen.height;
            Screen.visible = false;
        }

        void AutoDestroy()
        {
            _Instance = null;
#if UNITY_EDITOR
            if(Application.isPlaying)
                GameObject.Destroy(Doc.gameObject);
            else GameObject.DestroyImmediate(Doc.gameObject);
#else
            GameObject.Destroy(Doc.gameObject);
#endif
        }

        public void EndFadeout()
        {
            Fading = true;
            Alpha = 0;
        }

        public void FadeTo(Color color, float time, bool hold, Action completedCallback, Action frameCallback)
        {
            Screen.style.backgroundColor = color;
            Screen.style.width = UnityEngine.Screen.width;
            Screen.style.height = UnityEngine.Screen.height;
            Screen.style.opacity = 0;
            //Screen.style.transitionProperty = new List<StylePropertyName> { "opacity" };
            //Screen.style.transitionDuration = new List<TimeValue> { new TimeValue(time, TimeUnit.Second) };
            //Screen.style.transitionTimingFunction = new List<EasingFunction> { EasingMode.Ease };
            GlobalCoroutine.Start(CoroutineFadeTo(color, time, hold, completedCallback, frameCallback));
        }

        public void FadeFrom(Color color, float time, Action completedCallback, Action frameCallback)
        {
            Screen.style.backgroundColor = color;
            Screen.style.width = UnityEngine.Screen.width;
            Screen.style.height = UnityEngine.Screen.height;
            Screen.style.opacity = 1;
            Screen.visible = true;
            //Screen.style.transitionProperty = new List<StylePropertyName> { "opacity" };
            //Screen.style.transitionDuration = new List<TimeValue> { new TimeValue(time, TimeUnit.Second) };
            //Screen.style.transitionTimingFunction = new List<EasingFunction> { EasingMode.Ease };
            GlobalCoroutine.Start(CoroutineFadeFrom(color, time, completedCallback, frameCallback));
        }

        
        IEnumerator CoroutineFadeFrom(Color targetColor, float time, Action completedCallback, Action frameCallback)
        {
            Fading = true;
            CachedColor = targetColor;
            Alpha = 1;
            while (Fading && Alpha > 0)
            {
                yield return null;
                Alpha -= (1 / time) * Time.unscaledDeltaTime;
                Render(Alpha);
                frameCallback?.Invoke();
            }

            completedCallback?.Invoke();
            Alpha = 0;
            Render(Alpha);
            Screen.visible = false;
            Screen.pickingMode = PickingMode.Ignore;
        }

        IEnumerator CoroutineFadeTo(Color targetColor, float time, bool hold, Action completedCallback, Action frameCallback)
        {
            Screen.pickingMode = PickingMode.Position;
            Fading = true;
            CachedColor = targetColor;
            Alpha = 0;
            while (Fading && Alpha < 1)
            {
                yield return null;
                Alpha += (1 / time) * Time.unscaledDeltaTime;
                Render(Alpha);
                frameCallback?.Invoke();
            }

            completedCallback?.Invoke();

            //one last render to be sure we render at full opacity
            Alpha = 1;
            Render(Alpha);
        }


        void Render(float alpha)
        {
            Screen.style.opacity = alpha;
        }
    }

}
