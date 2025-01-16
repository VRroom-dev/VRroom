Shader "Hidden/FadeTransition" {
    Properties {
        _TransitionAmount ("Transition", Float) = 1
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZTest Always
        ZWrite Off
        
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
	            UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float _TransitionAmount;

            v2f vert (appdata v) {
                v2f o;
	            UNITY_SETUP_INSTANCE_ID(v);
	            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = float4(float2(1, -1) * (v.uv * 2 - 1), 0, 1);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                return float4(0, 0, 0, _TransitionAmount);
            }
            ENDCG
        }
    }
}
