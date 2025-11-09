Shader "Custom/LaserBeam_Pro_Static"
{
     Properties
    {
        _MainColor ("Main Laser Color", Color) = (0, 1, 1, 1)
        _EdgeColor ("Edge Glow Color", Color) = (0.3, 0.8, 1, 1)
        _NoiseTex ("Noise Texture", 2D) = "white" {}

        _GlowIntensity ("Glow Intensity", Range(0, 10)) = 4
        _DistortionStrength ("Distortion Strength", Range(0, 0.1)) = 0.05
        _Speed ("Animation Speed", Range(0, 10)) = 2
        _Direction ("Flow Direction", Range(-1, 1)) = 1
        _Width ("Laser Core Width", Range(0.01, 1)) = 0.2
        _EdgeFade ("Edge Softness", Range(0.1, 10)) = 4
        _EnergyPulse ("Energy Pulse Strength", Range(0, 5)) = 1
        _NoisePower ("Noise Power", Range(0.5, 10)) = 4
        _Brightness ("Overall Brightness", Range(0, 5)) = 1
        _NoiseScale ("Noise Scale", Range(0.1, 5)) = 1.2

        _NoiseOffset ("Noise Random Offset", Range(0, 1000)) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha One
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _NoiseTex;

            float4 _MainColor;
            float4 _EdgeColor;
            float _GlowIntensity;
            float _DistortionStrength;
            float _Speed;
            float _Direction;
            float _Width;
            float _EdgeFade;
            float _EnergyPulse;
            float _NoisePower;
            float _Brightness;
            float _NoiseScale;
            float _NoiseOffset;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                // Movimiento base del ruido con offset aleatorio
                float time = _Time.y * _Speed;
                float2 noiseUV = i.uv * _NoiseScale;
                noiseUV.x += (time * 0.15 * _Direction) + _NoiseOffset;
                noiseUV.y += _NoiseOffset * 0.37;

                float noise = tex2D(_NoiseTex, noiseUV).r;

                // Distorsión del haz
                float2 distortedUV = i.uv + (noise - 0.5) * _DistortionStrength;

                // Núcleo del láser
                float core = exp(-_EdgeFade * abs(distortedUV.y - 0.5) / _Width);

                // Chispas energéticas
                float sparks = pow(noise, _NoisePower) * 2.0;

                // Pulso energético suave
                float pulse = (sin(_Time.y * 3.0) * 0.5 + 0.5) * _EnergyPulse;

                // Intensidad general
                float intensity = (core * _GlowIntensity + sparks + pulse) * _Brightness;

                // Color final
                float4 color = lerp(_EdgeColor, _MainColor, core) * intensity;
                color.a = saturate(core + sparks * 0.5);

                return color;
            }
            ENDHLSL
        }
    }

    FallBack Off
}