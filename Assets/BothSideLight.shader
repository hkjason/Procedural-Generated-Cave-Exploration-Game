Shader "Custom/BothSideLight"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BackfaceLightColor ("Backface Light Color", Color) = (0.2, 0.2, 0.2, 1)
        _BackfaceLightIntensity ("Backface Light Intensity", Range(0,1)) = 0.3
        _AmbientRange ("Ambient Range", Float) = 10.0
        _PointLightColor ("Point Light Color", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Lambert fullforwardshadows

        sampler2D _MainTex;
        half4 _BackfaceLightColor;
        half _BackfaceLightIntensity;
        half _AmbientRange;
        half4 _PointLightColor;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
            float3 viewDir;
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            half4 c = tex2D(_MainTex, IN.uv_MainTex);
            o.Albedo = c.rgb;

            half3 normal = normalize(o.Normal);
            half3 lightDir = normalize(_WorldSpaceLightPos0.xyz - IN.worldPos);
            half NdotL = max(0, dot(normal, lightDir));

            // Calculate backface lighting
            half3 backfaceLight = _BackfaceLightColor.rgb * max(0, dot(-normal, lightDir)) * _BackfaceLightIntensity;

            // Calculate front face lighting
            half3 frontfaceLight = _LightColor0.rgb * NdotL;

            // Mix backface and frontface lighting
            half3 combinedLighting = frontfaceLight + backfaceLight;

            // Add ambient effect based on distance
            float distance = length(_WorldSpaceLightPos0.xyz - IN.worldPos);
            float ambientFactor = saturate(1.0 - distance / _AmbientRange);
            combinedLighting += _PointLightColor.rgb * ambientFactor;

            o.Albedo *= combinedLighting;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
