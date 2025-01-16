using UnityEngine.Animations;
using UnityEngine.Playables;

namespace VRroom.Base {
    public class LocalAnimatorManager : AnimatorManager {
        private AnimationMixerPlayable _mixer;
        private AnimatorControllerPlayable _locomotionController;
        private AnimatorControllerPlayable _ikPoseController;
        private AnimatorControllerPlayable _tPoseController;
    
        protected override void Initialize() {
            _mixer = AnimationMixerPlayable.Create(Graph, 3);
        
            _locomotionController = AnimatorControllerPlayable.Create(Graph, Avatar.locomotionController);
            _ikPoseController = AnimatorControllerPlayable.Create(Graph, Avatar.ikPoseController);
            _tPoseController = AnimatorControllerPlayable.Create(Graph, Avatar.tPoseController);
        
            CacheParameterTypes(_locomotionController);
            CacheParameterTypes(_ikPoseController);
            CacheParameterTypes(_tPoseController);
        
            _mixer.ConnectInput(0, BaseController, 0, 1);
            _mixer.ConnectInput(1, _locomotionController, 0, 1);
        
            Output.SetSourcePlayable(_mixer);
        }

        protected override void Uninitialize() {
            if (_mixer.IsValid()) _mixer.Destroy();
            if (_locomotionController.IsValid()) _locomotionController.Destroy();
            if (_ikPoseController.IsValid()) _ikPoseController.Destroy();
            if (_tPoseController.IsValid()) _tPoseController.Destroy();
        }

        public void SetState(AnimatorState state) {
            _mixer.DisconnectInput(2);
            if (state == AnimatorState.TPose) {
                _mixer.ConnectInput(2, _tPoseController, 0, 1);
                _mixer.SetInputWeight(0, 0);
                _mixer.SetInputWeight(1, 0);
            } else if (state == AnimatorState.IKPose) {
                _mixer.ConnectInput(2, _ikPoseController, 0, 1);
                _mixer.SetInputWeight(0, 0);
                _mixer.SetInputWeight(1, 0);
            } else {
                _mixer.SetInputWeight(0, 1);
                _mixer.SetInputWeight(1, 1);
            }
        }
    
        public enum AnimatorState {
            Normal,
            IKPose,
            TPose,
        }
    }
}