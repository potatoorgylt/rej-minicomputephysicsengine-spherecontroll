Shader "Instanced/InstancedSurfaceShader" {
    Properties {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model
        #pragma surface surf Standard addshadow fullforwardshadows
        #pragma multi_compile_instancing
        #pragma instancing_options procedural:setup

        sampler2D _MainTex;

        struct Input {
            float2 uv_MainTex;
        };

        struct Entity
        {
            float3 position;
            float3 velocity;
            float radius;
            float massInverse;
        };

    #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
        StructuredBuffer<Entity> EntityBuffer;
        StructuredBuffer<float4> EntityPropsBuffer;
    #endif

        void setup()
        {
        #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
            Entity entity = EntityBuffer[unity_InstanceID];
            float3 xyz = entity.position;
            float s = entity.radius * 2.0;

            unity_ObjectToWorld._11_21_31_41 = float4(s, 0, 0, 0);
            unity_ObjectToWorld._12_22_32_42 = float4(0, s, 0, 0);
            unity_ObjectToWorld._13_23_33_43 = float4(0, 0, s, 0);
            unity_ObjectToWorld._14_24_34_44 = float4(xyz    , 1);
            unity_WorldToObject = unity_ObjectToWorld;
            unity_WorldToObject._14_24_34 *= -1;
            unity_WorldToObject._11_22_33 = 1.0f / unity_WorldToObject._11_22_33;
        #endif
        }

        half _Glossiness;
        half _Metallic;

        void surf (Input IN, inout SurfaceOutputStandard o) {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
            o.Albedo *= EntityPropsBuffer[unity_InstanceID].xyz;
            o.Smoothness *= EntityPropsBuffer[unity_InstanceID].w;
        #endif
        }
        ENDCG
    }
    FallBack "Diffuse"
}