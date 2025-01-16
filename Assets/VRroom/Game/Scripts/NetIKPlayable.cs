using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace VRroom.Game {
    public struct NetIKJob : IAnimationJob {
        public Vector3 TargetOrigin;
        public Vector3 PreviousOrigin;
        public NativeArray<Quaternion> TargetRotations;
        public NativeArray<Quaternion> PreviousRotations;
        public NativeArray<TransformStreamHandle> BoneHandles;
        public float InterpolationPeriod;
        public float InterpolationTime;
    
        public const int BoneCount = (int)HumanBodyBones.Jaw;

        public static AnimationScriptPlayable Create(PlayableGraph graph, Animator animator) {
            NetIKJob job = new NetIKJob {
                TargetRotations = new NativeArray<Quaternion>(BoneCount, Allocator.Persistent),
                PreviousRotations = new NativeArray<Quaternion>(BoneCount, Allocator.Persistent),
                BoneHandles = new NativeArray<TransformStreamHandle>(BoneCount, Allocator.Persistent),
            };
        
            for (int i = 0; i < BoneCount; i++) {
                Transform bone = animator.GetBoneTransform((HumanBodyBones)i);
                job.BoneHandles[i] = animator.BindStreamTransform(bone);
            }
        
            return AnimationScriptPlayable.Create(graph, job);
        }

        public void ProcessRootMotion(AnimationStream stream) { }
        public void ProcessAnimation(AnimationStream stream) {
            float t = InterpolationTime / InterpolationPeriod;
        
            Vector3 position = Vector3.Lerp(PreviousOrigin, TargetOrigin, t);
            BoneHandles[0].SetPosition(stream, position);
        
            for (int i = 0; i < BoneHandles.Length; i++) {
                Quaternion interpolated = Quaternion.Slerp(PreviousRotations[i], TargetRotations[i], t);
                BoneHandles[i].SetRotation(stream, interpolated);
            }
        
            InterpolationTime += Time.deltaTime;
        }

        public void Dispose() {
            PreviousRotations.Dispose();
            TargetRotations.Dispose();
            BoneHandles.Dispose();
        }
    }
}