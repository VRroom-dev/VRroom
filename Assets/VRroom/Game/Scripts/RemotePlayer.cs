using System;
using JetBrains.Annotations;
using Unity.Collections;
using UnityEngine;
using VRroom.Game.Networking;

namespace VRroom.Game {
    [PublicAPI]
    public class RemotePlayer : MonoBehaviour {
        public short networkId;
        public Guid PlayerId;
    
        public RemoteAnimatorManager animatorManager;
        public GameObject avatarObject;
        public Transform hipTransform;
    
        // root interpolation
        private Vector3 _currentRootPosition;
        private Vector3 _previousRootPosition;
        private Quaternion _currentRootRotation;
        private Quaternion _previousRootRotation;
        private float _interpolationPeriod;
        private float _interpolationTime;
    
        public float Distance { get; private set; }

        private void Start() {
        
            NetworkManager.SubscribeToObject(networkId, OnMessageReceived);
        }

        public void FixedUpdate() {
            float t = _interpolationTime / _interpolationPeriod;
            _interpolationTime += Time.deltaTime;
        
            Vector3 hipPosition = hipTransform.position;
            Quaternion hipRotation = hipTransform.rotation;
            transform.position = Vector3.Lerp(_previousRootPosition, _currentRootPosition, t);
            transform.rotation = Quaternion.Slerp(_previousRootRotation, _currentRootRotation, t);
            hipTransform.position = hipPosition;
            hipTransform.rotation = hipRotation;

            Transform localPlayer = GameObject.FindWithTag("LocalPlayer").transform; // replace with local player script when it exists
            Distance = (localPlayer.position - transform.position).magnitude;

            if (!avatarObject) return;
            if (Distance > RemotePlayerManager.HideDistance) {
                animatorManager.Pause();
                avatarObject.SetActive(false);
            } else {
                animatorManager.Play();
                avatarObject.SetActive(true);
            }
        }

        private void OnMessageReceived(MessageType type, NetMessage msg) {
            switch (type) {
                case MessageType.SkeletalData:
                    HandleSkeletalData(msg);
                    break;
                case MessageType.PositionData:
                    HandlePositionData(msg);
                    break;
                case MessageType.VoiceData:
                
                    break;
                default:
                    Debug.LogError($"Remote player received erroneous type from server: {type.ToString()}");
                    break;
            }
        }

        private void HandleSkeletalData(NetMessage msg) {
            NetIKJob job = animatorManager.JobData;
            NativeArray<Quaternion>.Copy(job.TargetRotations, job.PreviousRotations);

            NativeArray<Quaternion> rotations = job.TargetRotations;
            try {
                for (int i = 0; i < NetIKJob.BoneCount; i++) {
                    rotations[i] = new Quaternion(
                                                  msg.ReadShort() / 32767f,
                                                  msg.ReadShort() / 32767f,
                                                  msg.ReadShort() / 32767f,
                                                  msg.ReadShort() / 32767f
                                                 ).normalized;
                }
            } catch (Exception e) {
                Debug.LogException(e);
            }

            animatorManager.JobData = job;
        }

        private void HandlePositionData(NetMessage msg) {
            int updateRate = msg.ReadByte();
            _interpolationPeriod = 1f / updateRate;
            _interpolationTime = 0;
        
            Vector3 hipPosition = new(msg.ReadFloat(), msg.ReadFloat(), msg.ReadFloat());
            Vector3 relativeRootPosition = new(msg.ReadFloat(), msg.ReadFloat(), msg.ReadFloat());
            Quaternion playerRootRotation = new Quaternion(
                                                           msg.ReadShort() / 32767f,
                                                           msg.ReadShort() / 32767f,
                                                           msg.ReadShort() / 32767f,
                                                           msg.ReadShort() / 32767f
                                                          ).normalized;
        
            _previousRootRotation = _currentRootRotation;
            _currentRootRotation = playerRootRotation;
        
            int positionReferenceObjectId = msg.ReadShort();
            int objectSubReferenceId = msg.ReadShort();
        
            Transform referenceTransform = NetworkUtils.ObjectIdToGameObject(positionReferenceObjectId, objectSubReferenceId).transform;
        
            _previousRootPosition = _currentRootPosition;
            _currentRootPosition = referenceTransform.position + relativeRootPosition;

            NetIKJob job = animatorManager.JobData;
            job.PreviousOrigin = job.TargetOrigin;
            job.TargetOrigin = hipPosition;
            job.InterpolationPeriod = _interpolationPeriod;
            job.InterpolationTime = 0;
            animatorManager.JobData = job;
        }

        private void OnDestroy() {
            NetworkManager.UnsubscribeFromObject(networkId, OnMessageReceived);
        }
    }
}