Shader "HardnessMapUpdate"
{
    Properties
    {
        _ContactPosition ("Contact Position", Vector) = (-2, -2, 0, 0)
        _innerRadius ("Inner Radius", float) = 0.005
        _outerRadius ("Outer Radius", float) = 0.01
        _smoothMultiplier ("Smooth Multiplier", float) = 1
    }

    SubShader
    {
        Lighting Off
        Blend One Zero
        
        Pass
        {
            CGPROGRAM
            #include "UnityCustomRenderTexture.cginc"
            #pragma vertex CustomRenderTextureVertexShader
            #pragma fragment frag
            #pragma target 3.0
            
            float4 _ContactPosition;
            float _innerRadius;
            float _outerRadius;
            float _smoothMultiplier;
            
            float4 frag(v2f_customrendertexture IN) : COLOR
            {
                float4 color = tex2D(_SelfTexture2D, IN.localTexcoord.xy);
                float4 newColor = smoothstep(_innerRadius, _outerRadius, distance(IN.localTexcoord.xy, _ContactPosition) / _smoothMultiplier);
                
                return min(color, newColor);
            }
            ENDCG
        }
    }
}