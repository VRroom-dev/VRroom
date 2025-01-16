using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace VRroom.Base {
    [PublicAPI]
    public class AnimatorManager : MonoBehaviour {
        protected Avatar Avatar;
        protected Animator Animator;
        protected PlayableGraph Graph;
        protected AnimationPlayableOutput Output;
        protected AnimatorControllerPlayable BaseController;
        private readonly Dictionary<string, AnimatorControllerParameterType> _parameterTypes = new();

        private void Start() {
            Avatar = GetComponent<Avatar>();

            if (!TryGetComponent(out Animator))
                Animator = gameObject.AddComponent<Animator>();

            BaseController = AnimatorControllerPlayable.Create(Graph, Avatar.baseController);
            CacheParameterTypes(BaseController);

            Graph = PlayableGraph.Create("AnimatorManager");
            Output = AnimationPlayableOutput.Create(Graph, "Output", Animator);

            Initialize();
        }

        private void OnDestroy() {
            if (Graph.IsValid()) Graph.Destroy();
            if (BaseController.IsValid()) BaseController.Destroy();
            Uninitialize();
        }

        public int GetInt(AnimatorControllerPlayable controller, string paramName) {
            if (!_parameterTypes.TryGetValue(paramName, out var originalType)) return 0;
            return originalType switch {
                AnimatorControllerParameterType.Int => controller.GetInteger(paramName),
                AnimatorControllerParameterType.Bool => controller.GetBool(paramName) ? 1 : 0,
                AnimatorControllerParameterType.Float => Mathf.RoundToInt(controller.GetFloat(paramName)),
                _ => 0
            };
        }

        public float GetFloat(AnimatorControllerPlayable controller, string paramName) {
            if (!_parameterTypes.TryGetValue(paramName, out var originalType)) return 0f;
            return originalType switch {
                AnimatorControllerParameterType.Float => controller.GetFloat(paramName),
                AnimatorControllerParameterType.Int => controller.GetInteger(paramName),
                AnimatorControllerParameterType.Bool => controller.GetBool(paramName) ? 1f : 0f,
                _ => 0f
            };
        }

        public bool GetBool(AnimatorControllerPlayable controller, string paramName) {
            if (!_parameterTypes.TryGetValue(paramName, out var originalType)) return false;
            return originalType switch {
                AnimatorControllerParameterType.Bool => controller.GetBool(paramName),
                AnimatorControllerParameterType.Int => controller.GetInteger(paramName) != 0,
                AnimatorControllerParameterType.Float => controller.GetFloat(paramName) >= 0.5f,
                _ => false
            };
        }

        public void SetInt(AnimatorControllerPlayable controller, string parameter, int value) {
            if (!_parameterTypes.TryGetValue(parameter, out var originalType)) return;
            switch (originalType) {
                case AnimatorControllerParameterType.Bool:
                    controller.SetBool(parameter, value != 0);
                    break;
                case AnimatorControllerParameterType.Int:
                    controller.SetInteger(parameter, value);
                    break;
                case AnimatorControllerParameterType.Float:
                    controller.SetFloat(parameter, value);
                    break;
            }
        }

        public void SetFloat(AnimatorControllerPlayable controller, string parameter, float value) {
            if (!_parameterTypes.TryGetValue(parameter, out var originalType)) return;
            switch (originalType) {
                case AnimatorControllerParameterType.Bool:
                    controller.SetBool(parameter, value >= 0.5f);
                    break;
                case AnimatorControllerParameterType.Int:
                    controller.SetInteger(parameter, (int)value);
                    break;
                case AnimatorControllerParameterType.Float:
                    controller.SetFloat(parameter, value);
                    break;
            }
        }

        public void SetBool(AnimatorControllerPlayable controller, string parameter, bool value) {
            if (!_parameterTypes.TryGetValue(parameter, out var originalType)) return;
            switch (originalType) {
                case AnimatorControllerParameterType.Bool:
                    controller.SetBool(parameter, value);
                    break;
                case AnimatorControllerParameterType.Int:
                    controller.SetInteger(parameter, value ? 1 : 0);
                    break;
                case AnimatorControllerParameterType.Float:
                    controller.SetFloat(parameter, value ? 1f : 0f);
                    break;
            }
        }

        protected void CacheParameterTypes(AnimatorControllerPlayable controller) {
            for (int i = 0; i < controller.GetParameterCount(); i++) {
                AnimatorControllerParameter param = controller.GetParameter(i);
                _parameterTypes.TryAdd(param.name, param.type);
            }
        }

        protected virtual void Initialize() { }
        protected virtual void Uninitialize() { }
    }
}