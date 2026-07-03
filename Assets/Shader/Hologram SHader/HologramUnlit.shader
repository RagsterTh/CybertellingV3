Shader "Custom/HologramBuiltIn"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        _Color ("Color", Color) = (0,1,1,1)

        _Alpha ("Alpha", Range(0,1)) = 0.4

        _Emission ("Emission", Range(0,10)) = 3

        _FresnelPower ("Fresnel", Range(0.1,8)) = 3

        _ScanSpeed ("Scan Speed", Float) = 2

        _ScanDensity ("Scan Density", Float) = 40

        _ScanIntensity ("Scan Intensity", Range(0,1)) = 0.3

        _NoiseAmount ("Noise", Range(0,0.05)) = 0.01
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;

            float4 _MainTex_ST;

            fixed4 _Color;

            float _Alpha;
            float _Emission;
            float _FresnelPower;

            float _ScanSpeed;
            float _ScanDensity;
            float _ScanIntensity;

            float _NoiseAmount;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;

                float2 uv : TEXCOORD0;

                float3 worldPos : TEXCOORD1;

                float3 worldNormal : TEXCOORD2;

                float3 viewDir : TEXCOORD3;
            };

            float rand(float2 p)
            {
                return frac(sin(dot(p,float2(12.9898,78.233))) * 43758.5453);
            }

            v2f vert(appdata v)
            {
                v2f o;

                o.pos = UnityObjectToClipPos(v.vertex);

                o.uv = TRANSFORM_TEX(v.uv,_MainTex);

                o.worldPos = mul(unity_ObjectToWorld,v.vertex).xyz;

                o.worldNormal = UnityObjectToWorldNormal(v.normal);

                o.viewDir = _WorldSpaceCameraPos - o.worldPos;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 N = normalize(i.worldNormal);

                float3 V = normalize(i.viewDir);

                float fresnel = pow(1 - saturate(dot(N,V)),_FresnelPower);

                float scan = sin(i.worldPos.y * _ScanDensity - _Time.y * _ScanSpeed);

                scan = scan * 0.5 + 0.5;

                float2 uv = i.uv;

                float noise = rand(i.worldPos.xz + _Time.y);

                uv.x += (noise - 0.5) * _NoiseAmount;

                fixed4 tex = tex2D(_MainTex,uv);

                float3 col = tex.rgb;

                col *= _Color.rgb;

                col *= (1 + scan * _ScanIntensity);

                col += fresnel * _Emission * _Color.rgb;

                float alpha = tex.a * _Alpha;

                alpha *= (0.5 + fresnel * 0.5);

                return fixed4(col,alpha);
            }

            ENDCG
        }
    }

    FallBack Off
}