using System.Collections.Generic;
using UnityEngine;


namespace Toolbox
{
    /// <summary>
    /// Base class for a MonoBehaviour-based component system.
    /// </summary>
    /// <typeparam name="System"></typeparam>
    public class BaseComponentSystem<System, Component> : SimpleSingleton<System> where System : MonoBehaviour
    {
        protected HashSet<Component> Comps = new();


        public virtual void Register(Component comp)
        {
            Comps.Add(comp);
        }

        public virtual void UnRegister(Component comp)
        {
            Comps.Remove(comp);
        }
    }
}
