#ifndef LOD_HELPER
#define LOD_HELPER

float ApplyDither(float4 screenPos, float _CrossFade)
{
    float2 screenPercentagePos = screenPos.xy / screenPos.w;
    float2 checkerboard = float2(sin(screenPercentagePos.x * 2 * 3.151592 * _CrossFade * 16),
                                    sin(screenPercentagePos.y * 2 * 3.151592 * _CrossFade * 9));
    float checkboardClip = checkerboard.x > 0 ^ checkerboard.y > 0; 

    float flipLOD = abs(unity_LODFade.x);
    if (unity_LODFade.x > 0)
        flipLOD = 1 - flipLOD;
    flipLOD = 1 - flipLOD;

    //unity_LODFade.x at 1 is off.
    //unity_LODFade.x at 0 is on.

    //unity_LODFade.x at 1 is off.
    //unity_LODFade.x at 0 is on.

    int fadeSign = 1;
    if (unity_LODFade.x < 0)
        fadeSign = -1;

    //clip(-checkboardClip);

    if ((checkboardClip * -1 < 0 && fadeSign == 1) || (checkboardClip * -1 >= 0 && fadeSign == -1))
    {
        //clip(-1);
        float rightLOD = (flipLOD - 0.5) * 2;
        if (rightLOD < 0)
            rightLOD = 0;

        float evenClip = 0;
        if (fadeSign == 1)
        {    
            evenClip = abs(checkerboard.x) > rightLOD && abs(checkerboard.y) > rightLOD;
        }
        else
        {
            evenClip = !(abs(checkerboard.x) > (1 - rightLOD) && abs(checkerboard.y) > (1 - rightLOD));
        }
        clip(evenClip * -1);
    }
    else
    {
        //clip(-1);
        float leftLOD = flipLOD * 2;
        float oddClip = 0;
        if (fadeSign == 1)
        {
            oddClip = abs(checkerboard.x) > leftLOD && abs(checkerboard.y) > leftLOD;
        }
        else
        {
            oddClip = !(abs(checkerboard.x) > (1 - leftLOD) && abs(checkerboard.y) > (1 - leftLOD));
        }
        clip(oddClip * -1);
    }

    return 0;
}
#endif