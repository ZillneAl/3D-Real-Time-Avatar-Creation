// Upgrade NOTE: replaced 'UNITY_INSTANCE_ID' with 'UNITY_VERTEX_INPUT_INSTANCE_ID'

Shader "Unlit/VShaderTest"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _HeightTex("Texture", 2D) = "white" {}
        _Farcut("Farcut",Range(0,0.1)) = 0.002
        _Nearcut("Nearcut",Range(0,1)) = 0.2
        _NumSteps("Number of Steps",Range(1,1000)) = 150
    }
        SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        LOD 100
        Cull Front
        ZTest LEqual
        ZWrite On
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"


            struct vert_in
            {
                float4 vertex : POSITION;
                float4 normal : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct frag_in
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 vertexLocal : TEXCOORD1;
                float3 normal : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D _HeightTex;
            float4 _HeightTex_ST;

            float _Farcut;
            float _Nearcut;

            int _NumSteps;



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

            float dilatation(float off, float2 uv) {

                float b = 0;
                b = max(depth(uv + float2(0, off)), b);
                b = max(depth(uv + float2(0, -off)), b);
                b = max(depth(uv + float2(off, 0)), b);
                b = max(depth(uv + float2(-off, 0)), b);
                return b;
            }

            float erosion(float off, float2 uv) {
                float b = 1;
                b = min(depth(uv + float2(0, off)), b);
                b = min(depth(uv + float2(0, -off)), b);
                b = min(depth(uv + float2(off, 0)), b);
                b = min(depth(uv + float2(-off, 0)), b);
                return  b;
            }

            float2 getColorUv(frag_in i) {
                return float2(i.uv.x / 2, i.uv.y);
            }


            float2 getColorByPos(float x, float y) {
                return float2(x / 2 , y);
            }

            float2 getDepthUv(frag_in i) {
                return float2(i.uv.x / 2 + 0.5, i.uv.y);
            }

            float2 getDepthByPos(float x , float y) {
                return float2(x / 2 + 0.5, y);
            }


            frag_in vert(vert_in v)
            {
                frag_in o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.vertexLocal = v.vertex;
                o.normal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            float colorToDepth(fixed4 col) {

                int red = (int)(col.r * 256);
                int green = (int)(col.g * 256);
                int blue = (int)(col.b * 256);
                int a = red | green << 8 | blue << 16;
                return ((float)a) / 0xFFFFFF;
            }

            float getDensity(float3 currPos) {
                return 1;
            }

            fixed4 frag(frag_in i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                int numStep = (int) _NumSteps;
                const float stepSize = 1.7320f/*greatest distance in box*/ / numStep;

                float3 rayStartPos = i.vertexLocal + float3(0.5f, 0.5f, 0.5f);
                float3 rayDir = ObjSpaceViewDir(float4(i.vertexLocal, 0.0f));
                rayDir = normalize(rayDir);

                float4 col = float4(0,0,0,0);
                float3 pos = float3(0,0,0);
                float3 endPos = float3(0, 0, 0);


                bool hit = false;
                float off = 0.003;
                [loop]
                for (uint iStep = 0; iStep < numStep; iStep++)
                {
                    const float t = iStep * stepSize;
                    const float3 currPos = rayStartPos + rayDir * t;
                    // Stop when we are outside the box
                    if (currPos.x < -0.0001f || currPos.x >= 1.0001f || currPos.y < -0.0001f || currPos.y > 1.0001f || currPos.z < -0.0001f || currPos.z > 1.0001f) {
                        endPos = currPos;
                        float d = erosion(off, float2(pos.x, pos.y));
                        if (d > _Nearcut || d < _Farcut) {
                            hit = false;
                        }
                        break;
                    }
                   float heightSample = colorToDepth(tex2D(_MainTex, getDepthByPos(currPos.x, currPos.y)));
                   if (heightSample > currPos.z) {
                       hit = true;
                        pos = currPos;
                   }
                }

                if (hit) {

                    float distSteps = distance(pos, rayStartPos) / numStep;
                    col = tex2D(_MainTex, getColorByPos(pos.x, pos.y));
                    col = col * fixed4(1, 1, 1, numStep * distSteps);
                }
                return col;
            }
            ENDCG
        }
    }
}
