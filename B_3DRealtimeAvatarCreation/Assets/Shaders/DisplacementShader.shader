
Shader "Unlit/Displacement"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Displacement("Displacement", Range(0,100)) = 1
        _Farcut("Farcut",Range(0,0.01)) = 0.002
        _Nearcut("Nearcut",Range(0,1)) = 0.2
        _DepthBlend("Blend", Range(0,1)) = 0.1
        _9x9Distance("9x9Dist", Range(1,10)) = 2
    }
        SubShader
        {
            Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
            LOD 100
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            Pass
            {
                CGPROGRAM

                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile_fog
                
                #include "UnityCG.cginc"
                #include "Common.cginc"

                struct appdata
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                struct v2f
                {
                    float2 uv : TEXCOORD0;
                    UNITY_FOG_COORDS(1)
                    float4 vertex : SV_POSITION;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                    UNITY_VERTEX_OUTPUT_STEREO
                };

                sampler2D _MainTex;
                float4 _MainTex_ST;
                float4 _MainTex_TexelSize;
                float _Displacement;
                float _Farcut;
                float _DepthBlend;
                float _Nearcut;
                float _9x9Distance;

                float2 getColorUv(v2f i) {
                    return float2(i.uv.x / 2, i.uv.y);

                }

                //uv-coords for depth part of rendertexture 
                float2 getDepthUv(v2f i) {
                    return float2(i.uv.x / 2 + 0.5, i.uv.y);

                }

                //sample and colorToDepth
                float depth(float2 iuv) {
                    float2 uv = float2((iuv[0] + 1) / 2, iuv[1] - 0.01);
                    fixed4 col = tex2D(_MainTex, uv);

                    int red = (int)(col.r * 255);
                    int green = (int)(col.g * 255);
                    int blue = (int)(col.b * 255);
                    int a = red | green << 8 | blue << 16;
                    float b = ((float)a) / 0xFFFFFF;
                    return b;
                }

                //performes dilation for plus shaped area
                float dilatation(float off, float2 uv) {

                    float b = 0;
                    b = max(depth(uv + float2(0, off)), b);
                    b = max(depth(uv + float2(0, -off)), b);
                    b = max(depth(uv + float2(off, 0)), b);
                    b = max(depth(uv + float2(-off, 0)), b);
                    return b;
                }


                //performes erosion for plus shaped area
                float erosion(float off, float2 uv) {
                    float b = 1;
                    b = min(depth(uv + float2(0, off)), b);
                    b = min(depth(uv + float2(0, -off)), b);
                    b = min(depth(uv + float2(off, 0)), b);
                    b = min(depth(uv + float2(-off, 0)), b);
                    return  b;
                }

                //vertex shader
                v2f vert(appdata v){
            
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = v.vertex;
                fixed4 depth_vec = tex2Dlod(_MainTex, float4(float2(v.uv.x / 2 + 0.5, v.uv.y), 0, 0));
                float d = colorToDepth(depth_vec);
                o.vertex.xyz -= float3(0, 0, 1) * d * _Displacement;
                o.vertex = UnityObjectToClipPos(o.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }


                //fragment shader
                fixed4 frag(v2f i) : SV_Target
                {
                 UNITY_SETUP_INSTANCE_ID(i);
                float off = 0.003;
                float d = 0;
                float dil = dilatation(off, i.uv);
                if (dil > _Farcut) {
                    float err = erosion(off, i.uv);
                    if (err < _Nearcut) {
                        d = err;
                    }
                }

                fixed4 col;
                if(d < _Nearcut && d > _Farcut){                // performs blending based on distance to center of Nearcut/Farcut
                    col = tex2D(_MainTex, getColorUv(i));
                    float mid = (_Nearcut - _Farcut) / 2;
                    if (_DepthBlend + mid < d) {
                        float perhigh = (_Nearcut - (_DepthBlend + mid)) / (d - mid);
                        col = col * fixed4(1, 1, 1, 1 * perhigh );
                    }
                    else if (d < mid - _DepthBlend) {
                        float perlow = d / (_Farcut + (mid - _DepthBlend));
                        col = col * fixed4(1, 1, 1, 1 * perlow);
                    }
                }
                else
                    col = fixed4(0, 0, 0, 0);

                return col;
        }


        ENDCG
    }

    }
}
