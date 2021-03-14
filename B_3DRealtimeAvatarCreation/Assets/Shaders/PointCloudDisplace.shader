

Shader "Unlit/PointCloudDisplace"
{
    Properties{
        _MainTex("Texture", 2D) = "white" {}
        _Radius("Sphere Radius", float) = 1.0
        _Farcut("Farcut",Range(0,1)) = 0.2
        _Nearcut("Nearcut",Range(0,1)) = 0.002
        _Displacement("Displacement", Range(0,100)) = 0.7
        _OFF("Offset", Range(0,0.1)) = 0.003
    }
       SubShader{
       Tags { "RenderType" = "Opaque" }
            Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
            LOD 100
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            Pass {
                CGPROGRAM

                #pragma fragment frag
                #pragma geometry geom
                #pragma vertex vert
                #pragma target 4.0                  
                
                #include "UnityCG.cginc"
                #include "Common.cginc"

                struct appdata {
                    float2 uv : TEXCOORD0;
                    float4 vertex : POSITION;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };
                struct v2g {
                    float2 uv : TEXCOORD0;
                    float4 vertex : POSITION;
                    float r : TEXCOORD1;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                    UNITY_VERTEX_OUTPUT_STEREO
                };
                struct g2f {
                    float2 uv : TEXCOORD0;
                    float4 vertex : SV_POSITION;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                    UNITY_VERTEX_OUTPUT_STEREO
                };


                sampler2D _MainTex;
                float _Displacement;
                float4 _MainTex_ST;
                float _Farcut;
                float _Nearcut;
                float _OFF;

                float2 getColorUv(g2f i) {
                    return float2(i.uv.x / 2, i.uv.y);

                }

                //uv-coords for depth part of rendertexture 
                float2 getDepthUv(g2f i) {
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

                float rand(float3 p) {
                    return frac(sin(dot(p.xyz, float3(12.9898, 78.233, 45.5432))) * 43758.5453);
                }
                float2x2 rotate2d(float a) {
                   float s = sin(a);
                   float c = cos(a);
                   return float2x2(c,-s,s,c);
                }
                //Vertex shader: computes normal wrt camera
                v2g vert(appdata v) {
                    UNITY_SETUP_INSTANCE_ID(v);
                    v2g o;
                    UNITY_INITIALIZE_OUTPUT(v2g, o);
                    UNITY_TRANSFER_INSTANCE_ID(v,o);
                    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                    o.vertex = v.vertex;
                    o.uv = v.uv;
                    fixed4 depth_vec = tex2Dlod(_MainTex, float4(float2(v.uv.x / 2 + 0.5, v.uv.y), 0, 0));
                    float d = colorToDepth(depth_vec);
                    o.vertex.xyz -= float3(0, 0, 1) * d * _Displacement;
                    o.vertex = UnityObjectToClipPos(o.vertex);
                    o.r = rand(v.vertex);// calc random value based on object space pos
                    return o;
                }

                float _Radius;
                //Geometry shaders: Creates an equilateral triangle with the original vertex in the orthocenter
                //not capable of Single-Pass Instanced Rendering
                [maxvertexcount(3)]
                void trigeom(point v2g IN[1], inout TriangleStream<g2f> OutputStream)
                {
                   //DEFAULT_UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN[0])
                   float2 dim = float2(_Radius,_Radius);
                   
                   //UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO();
                   float2 p[3];    // equilateral tri
                   p[0] = float2(-dim.x, dim.y * .57735026919);
                   p[1] = float2(0., -dim.y * 1.15470053838);
                   p[2] = float2(dim.x, dim.y * .57735026919);

                   float2x2 r = rotate2d(IN[0].r * 3.14159);
                   for (int i = 0; i < 3; i++) {
                       p[i] = mul(r, p[i]);                         // apply rotation
                       p[i].x *= _ScreenParams.y / _ScreenParams.x; // make square
                   }
                   
                   g2f out1;
                   g2f out2;
                   g2f out3;

                   UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(out1);
                   UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(out2);
                   UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(out3);

                   out1.uv = IN[0].uv;
                   out2.uv = IN[0].uv;
                   out3.uv = IN[0].uv;

                   out1.vertex = IN[0].vertex + float4(p[0], 0, 0) / 2.;
                   out2.vertex = IN[0].vertex + float4(p[1], 0, 0) / 2.;
                   out3.vertex = IN[0].vertex + float4(p[2], 0, 0) / 2.;

                   OutputStream.Append(out1);
                   OutputStream.Append(out2);
                   OutputStream.Append(out3);

                }


                // creates 6 equilateral points from one orthocenter point 
                [maxvertexcount(6)]
                void geom(point v2g input[1], inout PointStream<g2f> outStream)
                {
                    UNITY_SETUP_INSTANCE_ID(input[0]);
                    g2f output;
                    UNITY_INITIALIZE_OUTPUT(g2f, output);
                    UNITY_TRANSFER_INSTANCE_ID(input[0], output);
                    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input[0]);
                    UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(input[0], output);
                    float2 dim = float2(_Radius, _Radius);

                    float2 p[6];    // equilateral hex
                    p[0] = float2(-dim.x, dim.y * .57735026919);
                    p[1] = float2(0., -dim.y * 1.15470053838);
                    p[2] = float2(dim.x, dim.y * .57735026919);
                    p[3] = float2(dim.x * .57735026919, -dim.y);
                    p[4] = float2(dim.x *1.15470053838, dim.y);
                    p[5] = float2(dim.x,- dim.y * .57735026919);



                    float2x2 r = rotate2d(input[0].r * 3.14159);

                    for (int i = 0; i < 6; i++) {
                        p[i] = mul(r, p[i]);                            // apply rotation
                        p[i].x *= _ScreenParams.y / _ScreenParams.x;    // make square
                    }

                    [unroll(6)]
                    for (int j = 0; j < 6; ++j)
                    {
                        output.vertex = input[0].vertex + float4(p[j], 0, 0) / 2.;
                        output.uv = input[0].uv;
                        UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(input[0], output);
                        outStream.Append(output);
                  
                    }
                    outStream.RestartStrip();
                }

                fixed4 frag(g2f i) : SV_Target
                {
                    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                    float depth = dilatation(_OFF, i.uv);
                    if (depth < _Nearcut && depth > _Farcut) {
                        fixed4 col = tex2D(_MainTex, getColorUv(i));
                        return col;
                    }
                    else {
                        return fixed4(0,0,0,0);
                    }

                }
            ENDCG
        }
        }
            FallBack "Diffuse"
}
