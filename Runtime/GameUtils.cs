using System;
using Peg.AutoCreate;
using Peg.Util;
using Peg.Lazarus;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Peg.Lib
{
    /// <summary>
    /// General-purpose utility methods that don't belong to any specific object.
    /// 
    /// TODO: Move this into a more appropriate file.
    /// 
    /// </summary>
    public static class GameUtils
    {
        static IPoolSystem _Lazarus;
        static IPoolSystem Lazarus
        {
            get
            {
                _Lazarus ??= AutoCreator.AsSingleton<IPoolSystem>();
                return _Lazarus;
            }
        }

        /// <summary>
        /// Raycasts from a starting point in a given direction and finds the collision position and
        /// rotation for an object when it would be projected onto a surface.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="dir"></param>
        /// <param name="dist"></param>
        /// <param name="offset"></param>
        /// <param name="layers"></param>
        /// <param name="endPos"></param>
        /// <param name="endRot"></param>
        [Obsolete("This method now exists within the Vector3 class natively.")]
        public static bool ProjectToSurface(Vector3 start, Vector3 dir, float dist, float offset, LayerMask layers, out Vector3 endPos, out Vector3 normal)
        {
            if (Physics.Raycast(new Ray(start, dir), out RaycastHit hit, dist, layers, QueryTriggerInteraction.Ignore))
            {
                endPos = hit.point + hit.normal.normalized * offset;
                normal = hit.normal;
                return true;
            }
            endPos = Vector3.zero;
            normal = Vector3.up;
            return false;
        }

        /// <summary>
        /// Raycasts from a starting point in a given direction and finds the collision position and
        /// rotation for an object when it would be projected onto a surface.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="dir"></param>
        /// <param name="dist"></param>
        /// <param name="offset"></param>
        /// <param name="layers"></param>
        /// <param name="endPos"></param>
        /// <param name="endRot"></param>
        [Obsolete("This method now exists within the Vector3 class natively.")]
        public static bool ProjectToSurface(Vector3 start, Vector3 dir, float dist, float offset, LayerMask layers, out Vector3 endPos, out Quaternion rot)
        {
            if (Physics.Raycast(new Ray(start, dir), out RaycastHit hit, dist, layers, QueryTriggerInteraction.Ignore))
            {
                endPos = hit.point + hit.normal.normalized * offset;
                rot = Quaternion.LookRotation(hit.normal);
#if UNITY_EDITOR
                Debug.DrawLine(start, endPos, Color.green);
#endif
                return true;
            }
#if UNITY_EDITOR
            else
            {
                Debug.DrawRay(start, dir * dist, Color.red);
            }
#endif
            endPos = Vector3.zero;
            rot = Quaternion.identity;
            return false;
        }

        /// <summary>
        /// Raycasts from a starting point in a given direction and finds the collision position and
        /// rotation for an object when it would be projected onto a surface.
        /// This version allows for multiple collisions and takes the closest one.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="dir"></param>
        /// <param name="dist"></param>
        /// <param name="offset"></param>
        /// <param name="layers"></param>
        /// <param name="endPos"></param>
        /// <param name="endRot"></param>
        [Obsolete("This method now exists within the Vector3 class natively.")]
        public static bool ProjectToSortedSurface(Vector3 start, Vector3 dir, float dist, float offset, LayerMask layers, out Vector3 endPos, out Quaternion rot)
        {
            var hits = SharedArrayFactory.Hit10;
            int count = Physics.RaycastNonAlloc(new Ray(start, dir), hits, dist, layers, QueryTriggerInteraction.Ignore);
            if (count > 0)
            {
                int index = MathUtils.GetIndexOfClosest(count, hits, start);
                var hit = hits[index];
                endPos = hit.point + hit.normal.normalized * offset;
                rot = Quaternion.LookRotation(-hit.normal);
                return true;
            }
            endPos = Vector3.zero;
            rot = Quaternion.identity;
            return false;
        }

        /// <summary>
        /// Raycasts from a starting point in a given direction and finds the collision position and
        /// rotation for an object when it would be projected onto a surface.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="dir"></param>
        /// <param name="dist"></param>
        /// <param name="offset"></param>
        /// <param name="layers"></param>
        /// <param name="endPos"></param>
        /// <param name="endRot"></param>
        [Obsolete("This method now exists within the Vector3 class natively.")]
        public static bool ProjectToSurface(Vector3 start, Vector3 dir, float dist, float radius, float offset, LayerMask layers, out Vector3 endPos, out Quaternion rot)
        {
            if (Physics.SphereCast(new Ray(start, dir), radius, out RaycastHit hit, dist, layers, QueryTriggerInteraction.Ignore))
            {
                endPos = hit.point + hit.normal.normalized * offset;
                rot = Quaternion.LookRotation(hit.normal);
                return true;
            }
            endPos = Vector3.zero;
            rot = Quaternion.identity;
            return false;
        }

        /// <summary>
        /// Spawns a recycled pooled prefab with its forward vector facing a surface that is raycast within a given distance.
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="position"></param>
        /// <param name="direction"></param>
        /// <param name="offset"></param>
        /// <param name="dist"></param>
        /// <param name="layers"></param>
        /// <returns></returns>
        public static Transform SpawnRecycledDecal(GameObject prefab, Vector3 position, Vector3 direction, float offset, float dist, LayerMask layers)
        {
            if (Physics.Raycast(new Ray(position, direction), out RaycastHit hit, dist, layers, QueryTriggerInteraction.Ignore))
            {
                var go = Lazarus.RecycleSummon(prefab, hit.point);
                var trans = go.transform;
                Vector3 point = hit.point + hit.normal.normalized * offset;
                PositionObject(trans, point, Quaternion.LookRotation(-hit.normal), Random.Range(0, 360));
                return trans;
            }
            return null;
        }

        /// <summary>
        /// Spawns a recycled pooled prefab that has it's position set based on a raycast similar to a decal. However,
        /// this version does not orient the prefab.
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="position"></param>
        /// <param name="direction"></param>
        /// <param name="offset"></param>
        /// <param name="dist"></param>
        /// <param name="layers"></param>
        /// <returns></returns>
        public static Transform SpawnRecycledNonOrientedDecal(GameObject prefab, Vector3 position, Vector3 direction, float offset, float dist, LayerMask layers)
        {
            if (Physics.Raycast(new Ray(position, direction), out RaycastHit hit, dist, layers, QueryTriggerInteraction.Ignore))
            {
                var go = Lazarus.RecycleSummon(prefab, hit.point + hit.normal.normalized * offset);
                return go.transform;
            }
            return null;
        }

        /// <summary>
        /// Spawns a pooled prefab with its forward vector facing a surface that is raycast within a given distance.
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="position"></param>
        /// <param name="direction"></param>
        /// <param name="offset"></param>
        /// <param name="dist"></param>
        /// <param name="layers"></param>
        /// <returns></returns>
        public static Transform SpawnPooledDecal(GameObject prefab, Vector3 position, Vector3 direction, float offset, float dist, LayerMask layers)
        {
            Debug.DrawRay(position, direction * dist, Color.cyan);
            Debug.DrawRay(position, Vector3.up * dist, Color.cyan);
            if (Physics.Raycast(new Ray(position, direction), out RaycastHit hit, dist, layers, QueryTriggerInteraction.Ignore))
            {
                var go = Lazarus.Summon(prefab, hit.point);
                var trans = go.transform;
                Vector3 point = hit.point + hit.normal.normalized * offset;
                PositionObject(trans, point, Quaternion.LookRotation(-hit.normal), Random.Range(0, 360));
                return trans;
            }
            return null;
        }

        /// <summary>
        /// Spawns a pooled prefab with its forward vector facing a surface that is raycast within a given distance.
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="position"></param>
        /// <param name="direction"></param>
        /// <param name="offset"></param>
        /// <param name="dist"></param>
        /// <param name="layers"></param>
        /// <returns></returns>
        public static Transform SpawnPooledNonOrientedDecal(GameObject prefab, Vector3 position, Vector3 direction, float offset, float dist, LayerMask layers)
        {
            if (Physics.Raycast(new Ray(position, direction), out RaycastHit hit, dist, layers, QueryTriggerInteraction.Ignore))
            {
                Debug.DrawLine(position, hit.point, Color.red, 3);
                var go = Lazarus.Summon(prefab, hit.point + hit.normal.normalized * offset);
                return go.transform;
            }
            else Debug.DrawRay(position, direction * dist, Color.yellow, 3);
            return null;
        }

        /// <summary>
        /// Spawns a prefab with its forward vector facing a surface that is raycast within a given distance.
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="position"></param>
        /// <param name="direction"></param>
        /// <param name="offset"></param>
        /// <param name="dist"></param>
        /// <param name="layers"></param>
        /// <returns></returns>
        public static Transform SpawnDecal(GameObject prefab, Vector3 position, Vector3 direction, float offset, float dist, LayerMask layers)
        {
            if (Physics.Raycast(new Ray(position, direction), out RaycastHit hit, dist, layers, QueryTriggerInteraction.Ignore))
            {
                var go = UnityEngine.Object.Instantiate(prefab, hit.point, Quaternion.identity);
                var trans = go.transform;
                Vector3 point = hit.point + hit.normal.normalized * offset;
                PositionObject(trans, point, Quaternion.LookRotation(-hit.normal), Random.Range(0, 360));
                return trans;
            }
            return null;
        }

        /// <summary>
        /// Positions, scales, and rotates a GameObject and then applies an additional rotation about the z-axis by 'angle'.
        /// </summary>
        /// <param name="who"></param>
        /// <param name="pos"></param>
        /// <param name="scale"></param>
        /// <param name="rot"></param>
        /// <param name="angle"></param>
        public static void PositionObject(Transform who, Vector3 pos, Vector3 scale, Quaternion rot)
        {
            who.position = pos;
            who.localRotation = rot;
        }

        /// <summary>
        /// Positions, scales, and rotates a GameObject and then applies an additional rotation about the z-axis by 'angle'.
        /// </summary>
        /// <param name="who"></param>
        /// <param name="pos"></param>
        /// <param name="scale"></param>
        /// <param name="rot"></param>
        /// <param name="angle"></param>
        public static void PositionObject(Transform who, Vector3 pos, Vector3 scale, Quaternion rot, float angle)
        {
            who.position = pos;
            who.localRotation = rot;
            Vector3 angles = who.eulerAngles;
            angles.z += angle;
            who.localRotation = Quaternion.Euler(angles);
            who.localScale = new Vector3(scale.x, scale.y, 1);
        }

        /// <summary>
        /// Positions and rotates a GameObject and then applies an additional rotation about the z-axis by 'angle'.
        /// </summary>
        /// <param name="who"></param>
        /// <param name="pos"></param>
        /// <param name="scale"></param>
        /// <param name="rot"></param>
        /// <param name="angle"></param>
        public static void PositionObject(Transform who, Vector3 pos, Quaternion rot, float angle)
        {
            who.position = pos;
            who.localRotation = rot;
            Vector3 angles = who.eulerAngles;
            angles.z += angle;
            who.localRotation = Quaternion.Euler(angles);
        }

        /// <summary>
        /// Casts a ray onto a navmesh surface.
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="distance"></param>
        /// <param name="radius"></param>
        /// <param name="hitPoint"></param>
        /// <returns></returns>
        public static bool NavMeshRaycast(Ray ray, float distance, float radius, out Vector3 hitPoint)
        {
            int samples = Mathf.RoundToInt(distance / (radius * 2));

            for (int i = 0; i < samples; i++)
            {
                Vector3 samplePos = ray.GetPoint(radius + radius * 2 * i);
                bool didHit = UnityEngine.AI.NavMesh.SamplePosition(samplePos, out UnityEngine.AI.NavMeshHit hit, radius, UnityEngine.AI.NavMesh.AllAreas);
                if (didHit)
                {
                    hitPoint = hit.position;
                    return true;
                }
            }

            hitPoint = Vector3.zero;
            return false;

        }
    }
}
