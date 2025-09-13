Shader "Custom/NewSurfaceShader"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}

        // Water colors
        _ShallowColor ("Shallow Color", Color) = (0.2, 0.7, 1.0, 1)
        _DeepColor    ("Deep Color",    Color) = (0.0, 0.25, 0.6, 1)
        _FoamColor    ("Foam Color",    Color) = (1,1,1,1)

        // Wave settings
        _WaveAmplitude ("Wave Amplitude", Range(0,1)) = 0.1
        _WaveFrequency ("Wave Frequency", Range(0,10)) = 2.0
        _WaveSpeed     ("Wave Speed",     Range(0,5)) = 1.0

        // Stylization
        _FresnelPower ("Fresnel Power", Range(0.5, 8)) = 3.0
        _FoamThreshold ("Foam Threshold (normal.y)", Range(0,1)) = 0.65
        _FoamBlend     ("Foam Blend Width", Range(0.001, 0.5)) = 0.08

        // Transparency & depth-based blending
        _DepthMaxDistance ("Depth Max Distance", Range(0.01, 10)) = 2.0
        _OpacityShallow   ("Opacity Shallow", Range(0,1)) = 0.4
        _OpacityDeep      ("Opacity Deep", Range(0,1)) = 0.8

        // Kept for backward compatibility; not used in Unlit path but kept so materials don't break
        _Glossiness ("Smoothness", Range(0,1)) = 0.8
        _Metallic   ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        LOD 300

        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _ShallowColor;
                float4 _DeepColor;
                float4 _FoamColor;
                float _WaveAmplitude;
                float _WaveFrequency;
                float _WaveSpeed;
                float _FresnelPower;
                float _FoamThreshold;
                float _FoamBlend;
                float _DepthMaxDistance;
                float _OpacityShallow;
                float _OpacityDeep;
            CBUFFER_END

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 worldPos    : TEXCOORD0;
                float3 normalWS    : TEXCOORD1;
                float2 uv          : TEXCOORD2;
                float3 viewDirWS   : TEXCOORD3;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                // Object -> World
                float3 worldPos = TransformObjectToWorld(IN.positionOS.xyz);

                // Wave displacement in world space (y axis)
                float t = _TimeParameters.y * _WaveSpeed;
                float w1 = sin(worldPos.x * _WaveFrequency + t);
                float w2 = sin((worldPos.z + worldPos.x * 0.7) * (_WaveFrequency * 0.75) + t * 1.2);
                float wave = (w1 + w2) * 0.5;
                worldPos.y += wave * _WaveAmplitude;

                OUT.worldPos  = worldPos;
                OUT.positionHCS = TransformWorldToHClip(worldPos);
                OUT.normalWS  = TransformObjectToWorldNormal(IN.normalOS);
                OUT.uv        = IN.uv;
                OUT.viewDirWS = GetWorldSpaceViewDir(worldPos);
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                // Normalize vectors
                float3 n = normalize(IN.normalWS);
                float3 v = normalize(IN.viewDirWS);

                // Sample base texture
                float4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);

                // Screen-space depth based blending: estimate water depth
                float2 uvSS = GetNormalizedScreenSpaceUV(IN.positionHCS);
                #if defined(UNITY_REVERSED_Z)
                    float sceneRawDepth = SampleSceneDepth(uvSS);
                #else
                    float sceneRawDepth = SampleSceneDepth(uvSS);
                #endif
                float sceneEyeDepth = LinearEyeDepth(sceneRawDepth, _ZBufferParams);
                float3 viewPos = TransformWorldToView(IN.worldPos);
                float waterEyeDepth = -viewPos.z; // positive towards camera
                float depthDelta = max(0.0, sceneEyeDepth - waterEyeDepth);
                float depthFactor = saturate(depthDelta / max(0.001, _DepthMaxDistance)); // 0 = shallow edge, 1 = deep

                // Base color from depth: mix shallow near intersections to deep away from them
                float3 depthCol = lerp(_ShallowColor.rgb, _DeepColor.rgb, depthFactor);

                // Fresnel factor for rim and subtle top color influence
                float fres = pow(saturate(1.0 - dot(v, n)), _FresnelPower);
                float3 waterCol = lerp(depthCol, _ShallowColor.rgb, fres * 0.5);

                // Foam highlight using upward-facing normals (stylized)
                float foamMask = saturate((n.y - _FoamThreshold) / max(_FoamBlend, 1e-4));
                float3 finalCol = lerp(waterCol, _FoamColor.rgb, foamMask);

                // Modulate by texture
                finalCol *= tex.rgb;

                // Alpha: more transparent in shallow, more opaque in deep; add slight fresnel boost
                float alpha = lerp(_OpacityShallow, _OpacityDeep, depthFactor);
                alpha = saturate(alpha + fres * 0.2);
                alpha *= tex.a;

                return float4(finalCol, alpha);
            }
            ENDHLSL
        }
    }
    Fallback Off
}
