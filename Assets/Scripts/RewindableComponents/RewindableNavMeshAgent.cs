using RewindProject;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace RewindProject
{
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class RewindableNavMeshAgent : ARewindableComponent
    {
        [SerializeField] private NavMeshAgent agent;
        private void Awake()
        {
            if (agent == null) gameObject.TryGetComponent(out agent);
            _Component = agent;
            Init();
        }
        protected override void Init()
        {
            _TrackingProps.Add(typeof(NavMeshAgent).GetProperty(nameof(agent.destination)), new RewindFrames<Vector3>(InterpolationMethod, HowManyFramesSkip));
        }
        void FixedUpdate()
        {
            if (agent.hasPath && !agent.isStopped && Vector3.Distance((Vector3)_TrackingProps.FirstOrDefault().Value.GetLastShot(), agent.destination) > 0.05f)
            {
                EventTick();
            }
        }
    }
}

