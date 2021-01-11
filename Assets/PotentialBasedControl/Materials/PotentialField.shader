Shader "Custom/PotentialField" {
    Properties
    {
        _MaxHeight("MaxHeight", Float) = 0.2
        _TopColor("TopColor", Color) = (1, 0, 0, 1)
        _BottomColor("BottomColor", Color) = (0, 1, 0, 1)
        _Glossiness("Smoothness", Range(0, 1)) = 0.5
        _Metallic("Metallic", Range(0, 1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
#pragma surface surf Standard fullforwardshadows vertex:vert

        // Use shader model 3.0 target, to get nicer looking lighting
#pragma target 3.0

        struct Input {
            float2 uv_MainTex;
            float2 grid_uv;
            float3 color;
        };

        sampler2D _PotentialFieldTex;
        half _Glossiness;
        half _Metallic;
        fixed4 _TopColor;
        fixed4 _BottomColor;
        float _MaxHeight;
        float _MaxPotentialValue;
        float _MinPotentialValue;
        int _Show3D;
        int _ShowWireframe;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
        // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void vert(inout appdata_full v, out Input o) {

            float4 potential = tex2Dlod(_PotentialFieldTex, float4(v.texcoord.xy, 0.0f, 0.0f));
            v.vertex.y = _Show3D * (potential.r - potential.g) * _MaxHeight;
            o.uv_MainTex = v.texcoord.xy;
            o.grid_uv = v.texcoord1.xy;
            o.color = (potential.r / abs(_MaxPotentialValue)) * _TopColor + (potential.g / abs(_MinPotentialValue))* _BottomColor;
        }

        void surf(Input IN, inout SurfaceOutputStandard o) {
			float2 grid = frac(IN.grid_uv);
            float lineWidth = 0.07;
			if (_ShowWireframe
				* step(lineWidth, grid.x)
				* step(lineWidth, grid.y) 
				* step(grid.x, 1.0 - lineWidth)
				* step(grid.y, 1.0 - lineWidth)) {
                discard;
            }
            o.Albedo = IN.color;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = 1.0;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
