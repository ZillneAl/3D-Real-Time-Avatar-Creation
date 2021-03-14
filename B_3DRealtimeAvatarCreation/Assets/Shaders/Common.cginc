// Pcx - Point cloud importer & renderer for Unity
// https://github.com/keijiro/Pcx

#define PCX_MAX_BRIGHTNESS 16


float2 getColorByPos(float x, float y) {
    return float2(x / 2, y);
}

float2 getDepthByPos(float x, float y) {
    return float2(x / 2 + 0.5, y);
}


//translates depthsample to depth representation
float colorToDepth(fixed4 col) {

    int red = (int)(col.r * 256);
    int green = (int)(col.g * 256);
    int blue = (int)(col.b * 256);
    int a = red | green << 8 | blue << 16;
    return ((float)a) / 0xFFFFFF;
}


