Shader "Unlit/CutoutShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        //_DepthCutoff ("Cutoff", Range (0,1)) = 0.02
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        LOD 100
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha 

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            float2 getColorUv(v2f i) {
                return float2(i.uv.x /2, i.uv.y);
                
            }
            
            float2 getDepthUv(v2f i) {
                return float2(i.uv.x /2 + 0.5, i.uv.y);
                
            } 
            
            float getDepth(v2f i) {
                fixed4 depth = tex2D(_MainTex, getDepthUv(i));
                /*if (depth.r == 0 && depth.b == 0 && depth.g == 0) {
                    return -1;
                }*/
                return (depth.r+depth.b+depth.g)/3;
            } 

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, getDepthUv(i));
                //col = fixed4(getDepth(i), getDepth(i), getDepth(i), 1);
                //col = col * fixed4(50, 50, 50, 1);
                
                return col;
            }
            
 
            ENDCG
        }
       
    }
}
