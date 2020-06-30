//All credit goes to HSL and HSV wiki page on color spaces. Page under Creative Commons Attribution-ShareAlike License.
//Not in default included shader files.
//https://en.wikipedia.org/wiki/HSL_and_HSV#HSV_to_RGB
float3 HSLToRGB(float h, float s, float l)
{
    float chroma = (1 - abs(2 * l - 1)) * s;
    float h2 = h / 60;
    float remainder = fmod(h2, 2);
    float x = chroma * (1 - abs(remainder - 1));
    float3 hslColor = float3(0, 0, 0);
    float h2Floor = floor(h2);
    if (h2Floor == 0)
    {
        hslColor = float3(chroma, x, 0);
    }
    else if (h2Floor == 1)
    {
        hslColor = float3(x, chroma, 0);
    }
    else if (h2Floor == 2)
    {
        hslColor = float3(0, chroma, x);
    }
    else if (h2Floor == 3)
    {
        hslColor = float3(0, x, chroma);
    }
    else if (h2Floor == 4)
    {
        hslColor = float3(x, 0, chroma);
    }
    else if (h2Floor == 5)
    {
        hslColor = float3(chroma, 0, x);
    }

    float m = l - (chroma / 2);
    return float3(hslColor.r + m, hslColor.g + m, hslColor.b + m);
}

float RGBHue(float r, float g, float b)
{
    float maxTerm = max(max(r, g), b);
    float minTerm = min(min(r, g), b);
    float h = 0;
    if (maxTerm == minTerm)
    {
        h = 0;
    }
    else if (maxTerm == r)
    {
        h = 60 * (0 + (g - b) / (maxTerm - minTerm));
    }
    else if (maxTerm == g)
    {
        h = 60 * (2 + (b - r) / (maxTerm - minTerm));
    }
    else if (maxTerm == b)
    {
        h = 60 * (4 + (r - g) / (maxTerm - minTerm));
    }
    
    if (h < 0)
        h = h + 360;
    return h;
}

float RGBHue(float3 rgb) { return RGBHue(rgb.r, rgb.g, rgb.b); }

float RGBSat(float r, float g, float b)
{
    float maxTerm = max(max(r, g), b);
    float minTerm = min(min(r, g), b);
    float s = 0;
    if (maxTerm == 0 || minTerm == 1)
    {
        s = 0;
    }
    else
    {
        s = (maxTerm - minTerm) / (1 - abs(maxTerm + minTerm - 1));
    }
    return s;
}

float RGBSat(float3 rgb) { return RGBSat(rgb.r, rgb.g, rgb.b); }

float RGBLightness(float3 r, float g, float b)
{
    float maxTerm = max(max(r, g), b);
    float minTerm = min(min(r, g), b);
    return (maxTerm + minTerm) / 2;
}

float RGBLightness(float3 rgb) { return RGBLightness(rgb.r, rgb.g, rgb.b); }

float HueDirection(float current, float target)
{
    if (current > target)
    {
        float deltaLeft = abs(current - target);
        if (deltaLeft > 180)
        {
            return -1;
        }
        else
        {
            return 1;
        }
    } 
    else if (current < target) 
    {
        float deltaRight = abs(target - current);
        if (deltaRight > 180)
        {
            return 1;
        }
        else
        {
            return -1;
        }
    }
    else
    {
        return 0;
    }
}

float HueTowards(float current, float target, float percentage, float direction)
{
    if (direction == 1)
    {
        if (target < current)
        {
            float f = current * (1 - percentage) + (target + 360) * percentage;
            if (f > 360)
                f -= 360;
            return f;
        }
        else
        {
            float f = current * (1 - percentage) + (target) * percentage;
            return f;
        }
    } 
    else if (direction == -1)
    {
        if (target > current)
        {
            float f = (current + 360) * (1 - percentage) + target * percentage;
            if (f > 360)
                f -= 360;
            return f;
        }
        else
        {
            float f = (current) * (1 - percentage) + target * percentage;
            return f;
        }
    }
    else
    {
        return current;
    }
}

float HueLerp(float current, float target, float percentage)
{
   return HueTowards(current, target, percentage, HueDirection(current, target));
}