using UnityEngine;
using System;
using Peg.Messaging;
using Peg.AutonomousEntities;

namespace Peg.Lib
{
    /// <summary>
    /// This component can be used to listen for demands made of common Unity components.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class UnityComponentDemands : LocalListenerMonoBehaviour
    {
        public UnityEngine.AI.NavMeshAgent NavAgent;
        public Rigidbody Body;
        public Rigidbody2D Body2d;
        public SpriteRenderer SpriteRend;

        
        void Awake()
        {
            if(NavAgent != null) DispatchRoot.AddLocalListener<DemandNavMeshAgent>(GetNavAgent);
            if(Body != null) DispatchRoot.AddLocalListener<DemandRigidbody>(GetRigidbody);
            if(SpriteRend != null) DispatchRoot.AddLocalListener<DemandSpriteRenderer>(GetSpriteRenderer);
        }

        protected override void OnDestroy()
        {
            DispatchRoot.RemoveLocalListener<DemandNavMeshAgent>(GetNavAgent);
            DispatchRoot.RemoveLocalListener<DemandRigidbody>(GetRigidbody);
            DispatchRoot.RemoveLocalListener<DemandSpriteRenderer>(GetSpriteRenderer);
            base.OnDestroy();
        }

        void GetNavAgent(DemandNavMeshAgent msg)
        {
            msg.Respond(NavAgent);
        }

        void GetRigidbody(DemandRigidbody msg)
        {
            msg.Respond(Body);
        }

        void GetRigidbody2d(DemandRigidbody2D msg)
        {
            msg.Respond(Body2d);
        }

        void GetSpriteRenderer(DemandSpriteRenderer msg)
        {
            msg.Respond(SpriteRend);
        }
    }


    public class DemandNavMeshAgent : Demand<UnityEngine.AI.NavMeshAgent>
    { 
        public DemandNavMeshAgent(Action<UnityEngine.AI.NavMeshAgent> callback) : base(callback) {}
    }

    public class DemandRigidbody : Demand<Rigidbody> 
    { 
        public DemandRigidbody(Action<Rigidbody> callback) : base(callback) {}
    }

    public class DemandRigidbody2D : Demand<Rigidbody2D>
    {
        public DemandRigidbody2D(Action<Rigidbody2D> callback) : base(callback) { }
    }

    public class DemandSpriteRenderer : Demand<SpriteRenderer> 
    { 
        public DemandSpriteRenderer(Action<SpriteRenderer> callback) : base(callback) {}
    }


    /// <summary>
    /// Collection of commonly used demands provides as a static
    /// class to help reduce the number of allocations needed at runtime.
    /// </summary>
    public static partial class DemandsTable
    {
        public static DemandEntityRoot DemandEntityRoot = new DemandEntityRoot(null);
        public static DemandNavMeshAgent DemandAgent = new DemandNavMeshAgent(null);
        public static DemandSpriteRenderer DemandSprite = new DemandSpriteRenderer(null);
        public static DemandRigidbody DemandBody = new DemandRigidbody(null);
        public static DemandRigidbody2D DemandBody2D = new DemandRigidbody2D(null);
        //public static DemandPathAgentAdaptor DemandAdaptor = new DemandPathAgentAdaptor(null);
    }
}

