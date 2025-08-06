Shader "Custom/GlobalVision"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _VisionRadius ("Vision Radius", Range(0,50)) = 10
        _SoftEdge ("Soft Edge", Range(0.1,5)) = 1.0
        _PlayerPos ("Player Position", Vector) = (0,0,0,0)
    }
    SubShader
    {
        Tags { 
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "ForceNoShadowCasting" = "True"
        }
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest Always
        Cull Off
        LOD 100

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
                float3 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _VisionRadius;
            float _SoftEdge;
            float3 _PlayerPos;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                
                // Convertir a posición mundial
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float dist = distance(i.worldPos.xy, _PlayerPos.xy);
                float vision = 1.0 - smoothstep(
                    _VisionRadius - _SoftEdge, 
                    _VisionRadius, 
                    dist
                );
                
                // Aplicar como máscara de transparencia
                return fixed4(0, 0, 0, 1 - vision); // Negro con transparencia variable
            }
            ENDCG
        }
    }
}