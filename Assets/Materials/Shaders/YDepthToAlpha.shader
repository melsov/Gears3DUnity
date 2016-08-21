Shader "Unlit/YDepthToAlpha"
{

    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct v2f {
                half3 worldRefl : TEXCOORD0;
				half4 modelSpace : TEXCOORD1;
                float4 pos : SV_POSITION;
            };

            v2f vert (float4 vertex : POSITION, float3 normal : NORMAL)
            {
                v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, vertex); // UnityObjectToClipPos(vertex);
                // compute world space position of the vertex
                float3 worldPos = mul(_Object2World, vertex).xyz;
                // compute world space view direction
                float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
                // world space normal
                float3 worldNormal = UnityObjectToWorldNormal(normal);
                // world space reflection vector
				float3 yy = vertex.xyz * (vertex.y * vertex.w);
				o.worldRefl = normalize(yy); //MMP**** reflect(-worldViewDir, worldNormal);
				o.modelSpace = vertex;
                return o;
            }
        
            fixed4 frag (v2f i) : SV_Target
            {
                // sample the default reflection cubemap, using the reflection vector
                half4 skyData = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, i.worldRefl);
                // decode cubemap data into actual color

				half3 skyColor = skyData.xyz - dot(i.worldRefl, skyData.xzw) * .25; // *DecodeHDR(skyData, unity_SpecCube0_HDR);
                // output it!
                fixed4 c = 0;
				fixed3 ycolor = max(fixed3(.5,.5,.5), fixed3((i.modelSpace.y + .3) * 5 - .45, 0., 0.));
				c.rgb = ycolor; // skyColor;
                return c;
            }
            ENDCG
        }
    }
    
}
