Shader "Custom/ColorTextureUpdate"
{
    Properties
    {
        _DrawingPoint ("Drawing Point", Vector) = (-2, -2, 0, 0)
        _DrawingColor ("Drawing Color", Color) = (1, 0, 0, 1)
        _innerRadius ("Inner Radius", float) = 0.005
        _outerRadius ("Outer Radius", float) = 0.01
        _smoothMultiplier ("Smooth Multiplier", float) = 2
        _TestPoint1 ("T1", Vector) = (0.5, 0.5, 0, 0)
        _TestPoint2 ("T2", Vector) = (0.6, 0.5, 0, 0)
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
            
            float4 _DrawingPoint;
            float4 _DrawingColor;
            float _innerRadius;
            float _outerRadius;
            float _smoothMultiplier;
            float4 _TestPoint1;
            float4 _TestPoint2;

            float4 frag(v2f_customrendertexture IN) : COLOR
            {
                float A = 1 / (_TestPoint2.x - _TestPoint1.x);
                float B = -1 / (_TestPoint2.y - _TestPoint1.y);
                float C = _TestPoint1.y / (_TestPoint2.y - _TestPoint1.y) - _TestPoint1.x / (_TestPoint2.x - _TestPoint1.x);
                float Z = C + A * IN.localTexcoord.y - B * IN.localTexcoord.x;
                float y = (-B * C + A * (Z - C)) / (A * A + B * B);
                float x = (A * y - Z + C) / B;
                float proectionX = ((x >= _TestPoint1.x && x <= _TestPoint2.x) || (x <= _TestPoint1.x && x >= _TestPoint2.x) || x == _TestPoint1.x || x == _TestPoint2.x) ? x : ((distance(x, _TestPoint1.x) < distance(x, _TestPoint2.x)) ? _TestPoint1.x : _TestPoint2.x);
                float proectionY = ((y >= _TestPoint1.y && y <= _TestPoint2.y) || (y <= _TestPoint1.y && y >= _TestPoint2.y) || y == _TestPoint1.y || y == _TestPoint2.y) ? y : ((distance(y, _TestPoint1.y) < distance(y, _TestPoint2.y)) ? _TestPoint1.y : _TestPoint2.y);
                float4 proection = float4(proectionX, proectionY, 0, 0);
                float4 color = tex2D(_SelfTexture2D, IN.localTexcoord.xy);
                float4 newColor = (1 - smoothstep(_innerRadius, _outerRadius, distance(IN.localTexcoord.xy, _DrawingPoint) / _smoothMultiplier))
                 * _DrawingColor;
                float4 testColor = 
                 (1 - smoothstep(_innerRadius, _outerRadius, distance(IN.localTexcoord.xy, proection) / _smoothMultiplier))
                 * _DrawingColor / _smoothMultiplier;

                return max(testColor, max(color, newColor));
            }
            ENDCG
        }
    }
}
