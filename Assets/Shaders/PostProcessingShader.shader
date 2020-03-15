Shader "Bitwise/PostProcessing"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NoiseTex ("Texture", 2D) = "black" {}
        _Intensity ("Intensity", Range(0, 1)) = 1
        _LineProgress ("Line Progress", Range(-1, 2)) = -1
        _LineWidth ("Line Width", Range(0, 1)) = 0.01
        _LineDisplacement ("Line Displacement", Range(0, 1)) = 0.5
        _RightSideEasing ("Right Side Easing", Range(0, 1)) = 0.5
        _DipPower ("Dip Power", Range(2, 50)) = 10
        _NoiseRatio ("Noise Ratio", Range(0, 1)) = 0.5
        _NoiseSize ("Noise Size", Range(0, 1)) = 50

        _GraylineNoiseRatio ("Gray Line Noise Ratio", Range(0, 1)) = 0.01
        _GraylineColor ("Gray Line Color", Color) = (0.3, 0.3, 0.3, 1)

        _PrimaryGraylineProgress ("Primary Gray Line Progress", Range(-1, 2)) = -1
        _PrimaryGraylineWidth ("Primary Gray Line Width", Range(0, 1)) = 0.01
        _SecondaryGraylineProgress ("Secondary Gray Line Progress", Range(-1, 2)) = -1
        _SecondaryGraylineWidth ("Secondary Gray Line Width", Range(0, 1)) = 0.01
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            sampler2D _NoiseTex;

            float _Intensity;
            float _LineProgress;
            float _LineWidth;
            float _LineDisplacement;
            float _RightSideEasing;
            float _DipPower;
            float _NoiseRatio;
            float _NoiseSize;
            float _GraylineNoiseRatio;
            float4 _GraylineColor;

            float _PrimaryGraylineProgress;
            float _PrimaryGraylineWidth;
            float _SecondaryGraylineProgress;
            float _SecondaryGraylineWidth;

            fixed4 frag (v2f i) : SV_Target
            {
                float distanceFromLine = abs((1.0 - _LineProgress) - i.uv.y);
                float noiseValue = tex2D(_NoiseTex, i.uv) * _NoiseSize;
                float magnitude = 1.0 - min(distanceFromLine / _LineWidth, 1.0);
                float powerMagnitude = pow(magnitude, _DipPower);
                float displacement = lerp(0.0, _LineDisplacement, powerMagnitude) + lerp(0, noiseValue, _NoiseRatio * magnitude);
                float2 displacedUV = float2(lerp(i.uv.x + (displacement * (1.0 - _RightSideEasing)), i.uv.x + displacement, 1.0 - i.uv.x), i.uv.y);

                float pglDist = abs((1.0 - _PrimaryGraylineProgress) - i.uv.y);
                float pglMag = 1.0 - min(pglDist / _PrimaryGraylineWidth, 1.0);
                float pglFactor = ceil(pglMag - 0.5);
                float pglColor = pglFactor * lerp(_GraylineColor, noiseValue, _GraylineNoiseRatio);

                float sglDist = abs((1.0 - _SecondaryGraylineProgress) - i.uv.y);
                float sglMag = 1.0 - min(sglDist / _SecondaryGraylineWidth, 1.0);
                float sglFactor = ceil(sglMag - 0.5);
                float sglColor = sglFactor * lerp(_GraylineColor, noiseValue, _GraylineNoiseRatio);

                return tex2D(_MainTex, lerp(i.uv, displacedUV, _Intensity)) + pglColor + sglColor;
            }

            ENDCG
        }
    }
}
