using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using VRroom.Base;

namespace VRroom.Game {
    public class RemoteAnimatorManager : AnimatorManager {
        private AnimationMixerPlayable _mixer;
        private AnimationScriptPlayable _netIKPlayable;
        private Transform[] _targetBones;

        public NetIKJob JobData {
            get => _netIKPlayable.GetJobData<NetIKJob>();
            set => _netIKPlayable.SetJobData(value);
        }
    
        protected override void Initialize() {
            _mixer = AnimationMixerPlayable.Create(Graph, 2);
            _netIKPlayable = NetIKJob.Create(Graph, Animator);
        
            _mixer.ConnectInput(0, BaseController, 0, 1);
            _mixer.ConnectInput(1, _netIKPlayable, 0, 1);
        
            Output.SetSourcePlayable(_mixer);
        }

        protected override void Uninitialize() {
            if (_mixer.IsValid()) _mixer.Destroy();
            if (_netIKPlayable.IsValid()) _netIKPlayable.Destroy();
            JobData.Dispose();
        }

        public void ApplyNetIK(bool value) => _mixer.SetInputWeight(1, value ? 1 : 0);
        public void Pause() => Graph.Stop();
        public void Play() => Graph.Play();
    }
}