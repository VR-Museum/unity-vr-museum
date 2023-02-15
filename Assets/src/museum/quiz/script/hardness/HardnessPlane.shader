Shader "HardnessPlane"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _BottomColor ("Bottom Color", Color) = (.1,.1,.1,1)
        _NormalMap ("Normal Map", 2D) = "white" {}
        _MainTex ("MainTex", 2D) = "white" {}
        _ColorTex ("Color", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        
        _HeightMap ("Height Map", 2D) = "white" {}
        _HeightAmount ("Height Amount", float) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows vertex:vert addshadow tessellate:tess nolightmap

        #pragma target 5.0
		#pragma only_renderers d3d11 vulkan glcore
		#pragma exclude_renderers gles
		#include "UnityCG.cginc"

        sampler2D _MainTex;
        sampler2D _HeightMap;
        sampler2D _NormalMap;
        sampler2D _ColorTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        fixed4 _BottomColor;    

        float _HeightAmount;

        UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_INSTANCING_BUFFER_END(Props)

        void vert(inout appdata_full v)
        {
            float offset = tex2Dlod(_HeightMap, float4(v.texcoord.xy, 0, 0)).r * _HeightAmount;
            v.vertex.y += offset;
        }

        float tess()
        {
            return 8;
        }
        
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float height = tex2D (_HeightMap, IN.uv_MainTex).r;
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * lerp(_BottomColor, _Color, height);
            o.Albedo = max(c.rgb, tex2D (_ColorTex, IN.uv_MainTex));
            o.Normal = tex2D(_NormalMap, IN.uv_MainTex);
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
