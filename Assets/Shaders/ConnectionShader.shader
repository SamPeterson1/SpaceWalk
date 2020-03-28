Shader "Custom/ConnectionShader"
{
    Properties
    {
        _Radius("Radius", Float) = 10
    }
    SubShader
    {
        Tags {
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
        }
        CGPROGRAM
        float _Radius;
        ENDCG

    }

}
