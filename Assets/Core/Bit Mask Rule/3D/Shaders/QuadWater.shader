Shader "Custom/QuadWater"
{
    Properties
    {
        [Header(Depth Gradient)]
        _DepthShallowColor("Shallow Color", Color) = (0.325, 0.807, 0.971, 0.725)
        _DepthDeepColor("Deep Color", Color) = (0.086, 0.407, 1, 0.749)
        _DepthMaxDistance("Maximum Distance", Float) = 10
        _EyeMaxDistance("Eye Max Distance", float) = 100

        [Header(Foam)]
        _FoamTexture("Texture", 2D) = "white" {}
        _FoamColor("Color", Color) = (1, 1, 1, 1)
        _FoamMaxDistance("Maximum Distance", float) = 2
        _FoamWaveCount("Wave Count", float) = 8
        _FoamWaveSpeed("Wave Speed", float) = 0.5

        [Header(Normal maps)]
        _BumpMap1("Bump Map 1", 2D) = "bump" {}
        _BumpMap2("Bump Map 2", 2D) = "bump" {}
        _BumpScale("Bump Scale", float) = 1.0
        [Gamma]_Glossiness("Glossiness", Range(0, 1)) = 0.5

        [Header(Shadow Mapping)]
        _MaxShadowDistance("Maximum Sample Distance", float) = 50.0
        _ShadowColor("Color", Color) = (0.5, 0.5, 0.5, 1.0)

        [Header(Fresnel)]
        _FresnelPower("Fresnel power", float) = 1
        _FresnelDensity("Fresnel Density", float) = 2
        _ReflectPower("Reflect Power", float) = 2
        _ReflectDensity("Reflect Density", float) = 2

        [Header(Rain)]
        _RippleUVTS("TilingAndOffset", Vector) = (1, 1, 0, 0)
        _RippleFrequency("Frequency", float) = 1
        _RippleRange("Range", Vector) = (0.3, 0.7, 0.5, 0.5)
        _RippleRadius("Radius", Range(0, 1)) = 0.1
        _RippleWidth("Width", Range(0, 1)) = 0.05

        [NoScaleOffset]_ReflectionTex("Reflection Tex", 2D) = "black" {}
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            TEXTURE2D(_ReflectionTex);
            SAMPLER(sampler_ReflectionTex);
            TEXTURE2D(_FoamTexture);
            SAMPLER(sampler_FoamTexture);
            TEXTURE2D(_BumpMap1);
            SAMPLER(sampler_BumpMap1);
            TEXTURE2D(_BumpMap2);
            SAMPLER(sampler_BumpMap2);

            CBUFFER_START(UnityPerMaterial)
                float4 _DepthShallowColor;
                float4 _DepthDeepColor;
                float _DepthMaxDistance;
                float _EyeMaxDistance;

                float4 _FoamTexture_ST;
                float4 _FoamColor;
                float _FoamMaxDistance;
                float _FoamWaveCount;
                float _FoamWaveSpeed;

                float4 _BumpMap1_ST;
                float4 _BumpMap2_ST;
                float _BumpScale;
                float _Glossiness;

                float _MaxShadowDistance;
                float4 _ShadowColor;

                float _FresnelPower;
                float _FresnelDensity;
                float _ReflectPower;
                float _ReflectDensity;

                float4 _RippleUVTS;
                float _RippleFrequency;
                float4 _RippleRange;
                float _RippleRadius;
                float _RippleWidth;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 tangentWS : TEXCOORD2;
                float3 bitangentWS : TEXCOORD3;
                float2 uv : TEXCOORD4;
                float4 screenPos : TEXCOORD5;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                float3 tangentWS = TransformObjectToWorldDir(input.tangentOS.xyz);
                float3 bitangentWS = cross(normalWS, tangentWS) * input.tangentOS.w;

                output.positionWS = positionWS;
                output.normalWS = normalize(normalWS);
                output.tangentWS = normalize(tangentWS);
                output.bitangentWS = normalize(bitangentWS);
                output.positionCS = TransformWorldToHClip(positionWS);
                output.screenPos = ComputeScreenPos(output.positionCS);
                output.uv = input.uv;
                return output;
            }

            float3 UnpackNormalThere(TEXTURE2D_PARAM(bumpMap, sampler_bumpMap), float2 uv, float4 st, float scale, float3x3 tToW)
            {
                float2 uvTS = uv * st.xy + _Time.y * st.zw;
                float3 normalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(bumpMap, sampler_bumpMap, uvTS), scale);
                return normalize(mul(tToW, normalTS));
            }

            float Hash21(float2 p)
            {
                float3 p3 = frac(float3(p.x, p.y, p.x) * 0.1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }

            float rand2dTo1d(float2 uv)
            {
                return Hash21(uv);
            }

            float Rain(float2 texcoord)
            {
                float2 uv = _RippleUVTS.xy * texcoord + _RippleUVTS.zw;

                float2 floorUV = floor(uv);
                float random_X = lerp(_RippleRange.x, _RippleRange.y, rand2dTo1d(floorUV));
                float random_Y = lerp(_RippleRange.x, _RippleRange.y, rand2dTo1d(floorUV + 1));
                float2 center = float2(random_X, random_Y);

                float2 fracUV = frac(uv);
                float dis = distance(fracUV, center);
                float disChange = dis + _RippleRange.z - frac(rand2dTo1d(floorUV + 2) + _Time.y * _RippleFrequency);
                float ripple = smoothstep(_RippleRadius + _RippleWidth, _RippleRadius, disChange) - smoothstep(_RippleRadius, _RippleRadius - _RippleWidth, disChange);

                return ripple * pow(1 - dis, _RippleRange.w);
            }

            float4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 screenUV = input.screenPos.xy / input.screenPos.w;
                screenUV = UnityStereoTransformScreenSpaceTex(screenUV);

                float rawDepth = SampleSceneDepth(screenUV);
                float sceneDepth = LinearEyeDepth(rawDepth, _ZBufferParams);
                float surfaceDepth = LinearEyeDepth(input.positionCS.z / input.positionCS.w, _ZBufferParams);

                float depthOffset = sceneDepth - surfaceDepth;
                float depthOffset01 = saturate(depthOffset / _DepthMaxDistance);
                float3 waterColor = _DepthDeepColor.rgb;
                float waterAlpha = lerp(_DepthShallowColor.a, _DepthDeepColor.a, depthOffset01);

                float foamOffset01 = saturate(depthOffset / _FoamMaxDistance);
                float2 foamUV = input.uv * _FoamTexture_ST.xy + _Time.y * _FoamTexture_ST.zw;
                float foamTex = SAMPLE_TEXTURE2D(_FoamTexture, sampler_FoamTexture, foamUV).r;
                float foamStep = foamOffset01 - (saturate(sin((foamOffset01 - _Time.y * _FoamWaveSpeed) * _FoamWaveCount * PI)) * (1.0 - foamOffset01));
                float foam = smoothstep(foamStep - 0.01, foamStep + 0.01, foamTex);
                float3 foamColor = _FoamColor.rgb * foam;

                float rain = Rain(input.uv);

                float3x3 tToW = float3x3(input.tangentWS, input.bitangentWS, input.normalWS);
                float3 worldPos = input.positionWS;
                float3 viewDir = normalize(_WorldSpaceCameraPos - worldPos);

                float3 normal1 = UnpackNormalThere(TEXTURE2D_ARGS(_BumpMap1, sampler_BumpMap1), input.uv, _BumpMap1_ST, _BumpScale, tToW);
                float3 normal2 = UnpackNormalThere(TEXTURE2D_ARGS(_BumpMap2, sampler_BumpMap2), input.uv, _BumpMap2_ST, _BumpScale, tToW);
                float3 normalDir = normalize(normal1 + normal2);

                float4 shadowCoord = TransformWorldToShadowCoord(worldPos);
                Light mainLight = GetMainLight(shadowCoord);
                float3 lightDir = normalize(mainLight.direction);
                float3 halfDir = normalize(lightDir + viewDir);

                float lDotH = saturate(dot(lightDir, halfDir));
                float nDotL = saturate(dot(normalDir, lightDir));
                float nDotH = saturate(dot(normalDir, halfDir));
                float nDotV = saturate(dot(normalDir, viewDir));
                float roughness = max((1 - _Glossiness) * (1 - _Glossiness), 0.00001);

                float3 ambient = SampleSH(normalDir);

                float fd90 = 0.5 + 2 * lDotH * lDotH * roughness;
                float lightScatter = (1 + (fd90 - 1) * pow(1 - nDotL, 5));
                float viewScatter = (1 + (fd90 - 1) * pow(1 - nDotV, 5));
                float diffuseTerm = saturate(lightScatter * viewScatter * nDotL);
                float3 diffuse = mainLight.color.rgb * diffuseTerm;

                float D = pow(roughness, 2) / (PI * pow((pow(nDotH, 2) * (pow(roughness, 2) - 1)) + 1, 2));
                float F = _Glossiness + (1 - _Glossiness) * pow(1 - lDotH, 5);
                float k = pow(roughness, 2);
                float lambda_V = (nDotV * (1 - k) + k);
                float lambda_L = (nDotL * (1 - k) + k);
                float G = 1 / (lambda_V * lambda_L);
                float specularTerm = saturate(D * G * F);
                float3 specular = mainLight.color.rgb * specularTerm;
                float3 lightColor = ambient + diffuse + specular;

                float shadowFade = saturate(1.0 - (distance(worldPos, _WorldSpaceCameraPos) / max(_MaxShadowDistance, 0.0001)));
                float shadowAtten = lerp(1.0, mainLight.shadowAttenuation, shadowFade);
                float3 lightAndShadowColor = lerp(_ShadowColor.rgb, lightColor, shadowAtten);

                float fresnel = pow(1 - saturate(dot(normalDir, viewDir)), _FresnelPower) * _FresnelDensity;
                float3 reflectColor = SAMPLE_TEXTURE2D(_ReflectionTex, sampler_ReflectionTex, screenUV).rgb;
                reflectColor = pow(reflectColor, _ReflectPower) * _ReflectDensity;
                float3 fresnelColor = reflectColor * fresnel;

                float3 finalColor = waterColor * lightAndShadowColor + fresnelColor + rain + foamColor;
                float alpha = saturate(max(foam, waterAlpha) + fresnel);
                return float4(finalColor, alpha);
            }
            ENDHLSL
        }
    }
}
