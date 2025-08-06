Shader "Custom/VisionShader"
{
    Properties
    {
        _MainColor ("Main Color", Color) = (0,0,0,0.7)
        _EdgeColor ("Edge Color", Color) = (0.2,0.2,0.8,0.3)
        _EdgeWidth ("Edge Width", Range(0,0.5)) = 0.1
    }
    SubShader
    {
        Tags { 
            "Queue" = "Transparent+100" 
            "RenderType" = "Transparent" 
        }
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
            };
            
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float distToCenter : TEXCOORD1;
            };
            
            fixed4 _MainColor;
            fixed4 _EdgeColor;
            float _EdgeWidth;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                
                // Calcular distancia al centro (jugador)
                o.distToCenter = distance(v.vertex.xy, float2(0,0));
                
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Calcular borde suavizado
                float edgeFactor = smoothstep(
                    1.0 - _EdgeWidth, 
                    1.0, 
                    i.distToCenter / _EdgeWidth
                );
                
                // Mezclar colores
                fixed4 col = lerp(_EdgeColor, _MainColor, edgeFactor);
                
                // Hacer más transparente cerca del centro
                col.a *= smoothstep(0, 0.5, i.distToCenter);
                
                return col;
            }
            ENDCG
        }
    }
}