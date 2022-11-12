Shader"ScarletNexus/CharacterOutline"
{
    Properties
    {
        _SSize("_SSize", Vector) = (0,0,0,0)
        props_v_0_1("props_v_0_1", Vector) = (0,0,0,0)
        _perPSize("_perPSize", Vector) = (0,0,0,0)
        props_f_0_0("props_f_0_0", Vector) = (0,0,0,0)
        props_f_0_1("props_f_0_1", Vector) = (0,0,0,0)
        props_f_0_2("props_f_0_2", Vector) = (0,0,0,0)
        props_f_0_3("props_f_0_3", Vector) = (0,0,0,0)
//        props_f_0_4("props_f_0_4", Vector) = (0,0,0,0)
//        props_f_0_5("props_f_0_5", Vector) = (0,0,0,0)
        props_f_0_6("props_f_0_6", Vector) = (0,0,0,0)
        props_f_0_7("props_f_0_7", Vector) = (0,0,0,0)
        props_f_0_8("props_f_0_8", Vector) = (0,0,0,0)
        props_f_0_9("props_f_0_9", Vector) = (0,0,0,0)
        props_f_0_10("props_f_0_10", Vector) = (0,0,0,0)
        props_f_0_11("props_f_0_11", Vector) = (0,0,0,0)
        props_f_0_12("props_f_0_12", Vector) = (0,0,0,0)
        props_f_0_13("props_f_0_13", Vector) = (0,0,0,0)
        props_f_0_14("props_f_0_14", Vector) = (0,0,0,0)
        props_f_0_15("props_f_0_15", Vector) = (0,0,0,0)
        props_f_0_16("props_f_0_16", Vector) = (0,0,0,0)
        props_f_0_17("props_f_0_17", Vector) = (0,0,0,0)
        props_f_0_18("props_f_0_18", Vector) = (0,0,0,0)
        props_f_0_19("props_f_0_19", Vector) = (0,0,0,0)
        props_f_0_20("props_f_0_20", Vector) = (0,0,0,0)
        props_f_0_21("props_f_0_21", Vector) = (0,0,0,0)
        props_f_0_22("props_f_0_22", Vector) = (0,0,0,0)
        props_f_0_23("props_f_0_23", Vector) = (0,0,0,0)
        props_f_0_24("props_f_0_24", Vector) = (0,0,0,0)
        props_f_0_25("props_f_0_25", Vector) = (0,0,0,0)
        props_f_0_26("props_f_0_26", Vector) = (0,0,0,0)
        props_f_0_27("props_f_0_27", Vector) = (0,0,0,0)
        props_f_0_28("props_f_0_28", Vector) = (0,0,0,0)
        props_f_0_29("props_f_0_29", Vector) = (0,0,0,0)
        props_f_0_30("props_f_0_30", Vector) = (0,0,0,0)
        props_f_0_31("props_f_0_31", Vector) = (0,0,0,0)
        props_f_0_32("props_f_0_32", Vector) = (0,0,0,0)
        props_f_0_33("props_f_0_33", Vector) = (0,0,0,0)
        props_f_0_34("props_f_0_34", Vector) = (0,0,0,0)
        props_f_0_35("props_f_0_35", Vector) = (0,0,0,0)
        props_f_0_36("props_f_0_36", Vector) = (0,0,0,0)
//        props_f_0_37("props_f_0_37", Vector) = (0,0,0,0)
        _perPixelSize("_perPixelSize", Vector) = (0,0,0,0)
        props_f_1_0("props_f_1_0", Vector) = (0,0,0,0)
        props_f_1_1("props_f_1_1", Vector) = (0,0,0,0)
        props_f_1_2("props_f_1_2", Vector) = (0,0,0,0)
        props_f_1_3("props_f_1_3", Vector) = (0,0,0,0)
        props_f_1_4("props_f_1_4", Vector) = (0,0,0,0)
        props_f_1_5("props_f_1_5", Vector) = (0,0,0,0)
        props_f_1_6("props_f_1_6", Vector) = (0,0,0,0)
        props_f_1_7("props_f_1_7", Vector) = (0,0,0,0)
        props_f_1_8("props_f_1_8", Vector) = (0,0,0,0)
        props_f_1_9("props_f_1_9", Vector) = (0,0,0,0)
        props_f_1_10("props_f_1_10", Vector) = (0,0,0,0)
        props_f_1_11("props_f_1_11", Vector) = (0,0,0,0)
        props_f_1_12("props_f_1_12", Vector) = (0,0,0,0)
        props_f_1_13("props_f_1_13", Vector) = (0,0,0,0)
        props_f_1_14("props_f_1_14", Vector) = (0,0,0,0)
        props_f_1_15("props_f_1_15", Vector) = (0,0,0,0)
        props_f_1_16("props_f_1_16", Vector) = (0,0,0,0)
        props_f_1_17("props_f_1_17", Vector) = (0,0,0,0)
        props_f_1_18("props_f_1_18", Vector) = (0,0,0,0)
        props_f_1_19("props_f_1_19", Vector) = (0,0,0,0)
        props_f_1_20("props_f_1_20", Vector) = (0,0,0,0)
        props_f_1_21("props_f_1_21", Vector) = (0,0,0,0)
        props_f_1_22("props_f_1_22", Vector) = (0,0,0,0)
        props_f_1_23("props_f_1_23", Vector) = (0,0,0,0)
        props_f_1_24("props_f_1_24", Vector) = (0,0,0,0)
        props_f_1_25("props_f_1_25", Vector) = (0,0,0,0)
        props_f_1_26("props_f_1_26", Vector) = (0,0,0,0)
        props_f_1_27("props_f_1_27", Vector) = (0,0,0,0)
        props_f_1_28("props_f_1_28", Vector) = (0,0,0,0)
        props_f_1_29("props_f_1_29", Vector) = (0,0,0,0)
        props_f_1_30("props_f_1_30", Vector) = (0,0,0,0)
        props_f_1_31("props_f_1_31", Vector) = (0,0,0,0)
        props_f_1_32("props_f_1_32", Vector) = (0,0,0,0)
        props_f_1_33("props_f_1_33", Vector) = (0,0,0,0)
        props_f_1_34("props_f_1_34", Vector) = (0,0,0,0)
        props_f_1_35("props_f_1_35", Vector) = (0,0,0,0)
        props_f_1_36("props_f_1_36", Vector) = (0,0,0,0)
        props_f_1_37("props_f_1_37", Vector) = (0,0,0,0)
        props_f_1_38("props_f_1_38", Vector) = (0,0,0,0)
        props_f_1_39("props_f_1_39", Vector) = (0,0,0,0)
        props_f_1_40("props_f_1_40", Vector) = (0,0,0,0)
        props_f_1_41("props_f_1_41", Vector) = (0,0,0,0)
        props_f_1_42("props_f_1_42", Vector) = (0,0,0,0)
        props_f_1_43("props_f_1_43", Vector) = (0,0,0,0)
        props_f_1_44("props_f_1_44", Vector) = (0,0,0,0)
        props_f_1_45("props_f_1_45", Vector) = (0,0,0,0)
        props_f_1_46("props_f_1_46", Vector) = (0,0,0,0)
        props_f_1_47("props_f_1_47", Vector) = (0,0,0,0)
        props_f_1_48("props_f_1_48", Vector) = (0,0,0,0)
        props_f_1_49("props_f_1_49", Vector) = (0,0,0,0)
        props_f_1_50("props_f_1_50", Vector) = (0,0,0,0)
        props_f_1_51("props_f_1_51", Vector) = (0,0,0,0)
        props_f_1_52("props_f_1_52", Vector) = (0,0,0,0)
        props_f_1_53("props_f_1_53", Vector) = (0,0,0,0)
        props_f_1_54("props_f_1_54", Vector) = (0,0,0,0)
        props_f_1_55("props_f_1_55", Vector) = (0,0,0,0)
        props_f_1_56("props_f_1_56", Vector) = (0,0,0,0)
        props_f_1_57("props_f_1_57", Vector) = (0,0,0,0)
        props_f_1_58("props_f_1_58", Vector) = (0,0,0,0)
        props_f_1_59("props_f_1_59", Vector) = (0,0,0,0)
        props_f_1_60("props_f_1_60", Vector) = (0,0,0,0)
        props_f_1_61("props_f_1_61", Vector) = (0,0,0,0)
        props_f_1_62("props_f_1_62", Vector) = (0,0,0,0)
        props_f_1_63("props_f_1_63", Vector) = (0,0,0,0)
        props_f_1_64("props_f_1_64", Vector) = (0,0,0,0)
        props_f_1_65("props_f_1_65", Vector) = (0,0,0,0)
        props_f_1_66("props_f_1_66", Vector) = (0,0,0,0)
        props_f_1_67("props_f_1_67", Vector) = (0,0,0,0)
        props_f_1_68("props_f_1_68", Vector) = (0,0,0,0)
        props_f_1_69("props_f_1_69", Vector) = (0,0,0,0)
        props_f_1_70("props_f_1_70", Vector) = (0,0,0,0)
        props_f_1_71("props_f_1_71", Vector) = (0,0,0,0)
        props_f_1_72("props_f_1_72", Vector) = (0,0,0,0)
        props_f_1_73("props_f_1_73", Vector) = (0,0,0,0)
        props_f_1_74("props_f_1_74", Vector) = (0,0,0,0)
        props_f_1_75("props_f_1_75", Vector) = (0,0,0,0)
        props_f_1_76("props_f_1_76", Vector) = (0,0,0,0)
        props_f_1_77("props_f_1_77", Vector) = (0,0,0,0)
        props_f_1_78("props_f_1_78", Vector) = (0,0,0,0)
        props_f_1_79("props_f_1_79", Vector) = (0,0,0,0)
        props_f_1_80("props_f_1_80", Vector) = (0,0,0,0)
        props_f_1_81("props_f_1_81", Vector) = (0,0,0,0)
        props_f_1_82("props_f_1_82", Vector) = (0,0,0,0)
        props_f_1_83("props_f_1_83", Vector) = (0,0,0,0)
        props_f_1_84("props_f_1_84", Vector) = (0,0,0,0)
        props_f_1_85("props_f_1_85", Vector) = (0,0,0,0)
        props_f_1_86("props_f_1_86", Vector) = (0,0,0,0)
        props_f_1_87("props_f_1_87", Vector) = (0,0,0,0)
        props_f_1_88("props_f_1_88", Vector) = (0,0,0,0)
        props_f_1_89("props_f_1_89", Vector) = (0,0,0,0)
        props_f_1_90("props_f_1_90", Vector) = (0,0,0,0)
        props_f_1_91("props_f_1_91", Vector) = (0,0,0,0)
        props_f_1_92("props_f_1_92", Vector) = (0,0,0,0)
        props_f_1_93("props_f_1_93", Vector) = (0,0,0,0)
        props_f_1_94("props_f_1_94", Vector) = (0,0,0,0)
        props_f_1_95("props_f_1_95", Vector) = (0,0,0,0)
        props_f_1_96("props_f_1_96", Vector) = (0,0,0,0)
        props_f_1_97("props_f_1_97", Vector) = (0,0,0,0)
        props_f_1_98("props_f_1_98", Vector) = (0,0,0,0)
        props_f_1_99("props_f_1_99", Vector) = (0,0,0,0)
        props_f_1_100("props_f_1_100", Vector) = (0,0,0,0)
        props_f_1_101("props_f_1_101", Vector) = (0,0,0,0)
        props_f_1_102("props_f_1_102", Vector) = (0,0,0,0)
        props_f_1_103("props_f_1_103", Vector) = (0,0,0,0)
        props_f_1_104("props_f_1_104", Vector) = (0,0,0,0)
        props_f_1_105("props_f_1_105", Vector) = (0,0,0,0)
        props_f_1_106("props_f_1_106", Vector) = (0,0,0,0)
        props_f_1_107("props_f_1_107", Vector) = (0,0,0,0)
        props_f_1_108("props_f_1_108", Vector) = (0,0,0,0)
        props_f_1_109("props_f_1_109", Vector) = (0,0,0,0)
        props_f_1_110("props_f_1_110", Vector) = (0,0,0,0)
        props_f_1_111("props_f_1_111", Vector) = (0,0,0,0)
        props_f_1_112("props_f_1_112", Vector) = (0,0,0,0)
        props_f_1_113("props_f_1_113", Vector) = (0,0,0,0)
        props_f_1_114("props_f_1_114", Vector) = (0,0,0,0)
        props_f_1_115("props_f_1_115", Vector) = (0,0,0,0)
        props_f_1_116("props_f_1_116", Vector) = (0,0,0,0)
        props_f_1_117("props_f_1_117", Vector) = (0,0,0,0)
        props_f_1_118("props_f_1_118", Vector) = (0,0,0,0)
        props_f_1_119("props_f_1_119", Vector) = (0,0,0,0)
        props_f_1_120("props_f_1_120", Vector) = (0,0,0,0)
        props_f_1_121("props_f_1_121", Vector) = (0,0,0,0)
        props_f_1_122("props_f_1_122", Vector) = (0,0,0,0)
        props_f_1_123("props_f_1_123", Vector) = (0,0,0,0)
        props_f_1_124("props_f_1_124", Vector) = (0,0,0,0)
        props_f_1_125("props_f_1_125", Vector) = (0,0,0,0)
        props_f_1_126("props_f_1_126", Vector) = (0,0,0,0)
        props_f_1_127("props_f_1_127", Vector) = (0,0,0,0)
        props_f_1_128("props_f_1_128", Vector) = (0,0,0,0)
        props_f_1_129("props_f_1_129", Vector) = (0,0,0,0)
        props_f_1_130("props_f_1_130", Vector) = (0,0,0,0)
        props_f_1_131("props_f_1_131", Vector) = (0,0,0,0)
        props_f_1_132("props_f_1_132", Vector) = (0,0,0,0)
        props_f_1_133("props_f_1_133", Vector) = (0,0,0,0)
        props_f_1_134("props_f_1_134", Vector) = (0,0,0,0)
        props_f_1_135("props_f_1_135", Vector) = (0,0,0,0)
        props_f_2_0("props_f_2_0", Vector) = (0,0,0,0)
        props_f_2_1("props_f_2_1", Vector) = (0,0,0,0)
        props_f_2_2("props_f_2_2", Vector) = (0,0,0,0)
        props_f_2_3("props_f_2_3", Vector) = (0,0,0,0)
        props_f_2_4("props_f_2_4", Vector) = (0,0,0,0)
        props_f_3_0("props_f_3_0", Vector) = (0,0,0,0)
        props_f_4_0("props_f_4_0", Vector) = (0,0,0,0)
        props_f_4_1("props_f_4_1", Vector) = (0,0,0,0)
        props_f_4_2("props_f_4_2", Vector) = (0,0,0,0)
        props_f_4_3("props_f_4_3", Vector) = (0,0,0,0)
      _CameraDepthTexture("_CameraDepthTexture", 2D) = "white"
      t1("t1", 2D) = "white"
      _CharaBitMask("_CharaBitMask", 2D) = "white"
      t3("t3", 2D) = "white"
      _CameraColorTexture("_CameraColorTexture", 2D) = "white"
    }
    SubShader
    {
        Tags{"RenderType" = "Opaque"}

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/core.hlsl"

            struct appdata
            {
                float4 vertex : TEXCOORD0;
                float4 uv : TEXCOORD1;
            };
            struct v2f
            {
                float4 v2f_0_sv_position : SV_POSITION;
                float4 v2f_1 : TEXCOORD1;
                float4 replaceUV : TEXCOORD2;
            };
            struct gbuffer
            {
                float4 emissive : TEXCOORD0;

            };
            float4 _SSize;
            float4 props_v_0_1;
            float4 _perPSize;

            float4 props_f_0_0;
            float4 props_f_0_1;
            float4 props_f_0_2;
            float4 props_f_0_3;
            // float4 props_f_0_4;
            // float4 props_f_0_5;
            float4 props_f_0_6;
            float4 props_f_0_7;
            float4 props_f_0_8;
            float4 props_f_0_9;
            float4 props_f_0_10;
            float4 props_f_0_11;
            float4 props_f_0_12;
            float4 props_f_0_13;
            float4 props_f_0_14;
            float4 props_f_0_15;
            float4 props_f_0_16;
            float4 props_f_0_17;
            float4 props_f_0_18;
            float4 props_f_0_19;
            float4 props_f_0_20;
            float4 props_f_0_21;
            float4 props_f_0_22;
            float4 props_f_0_23;
            float4 props_f_0_24;
            float4 props_f_0_25;
            float4 props_f_0_26;
            float4 props_f_0_27;
            float4 props_f_0_28;
            float4 props_f_0_29;
            float4 props_f_0_30;
            float4 props_f_0_31;
            float4 props_f_0_32;
            float4 props_f_0_33;
            float4 props_f_0_34;
            float4 props_f_0_35;
            float4 props_f_0_36;
            // float4 props_f_0_37;
            float4 _perPixelSize;
            float4 props_f_1_0;
            float4 props_f_1_1;
            float4 props_f_1_2;
            float4 props_f_1_3;
            float4 props_f_1_4;
            float4 props_f_1_5;
            float4 props_f_1_6;
            float4 props_f_1_7;
            float4 props_f_1_8;
            float4 props_f_1_9;
            float4 props_f_1_10;
            float4 props_f_1_11;
            float4 props_f_1_12;
            float4 props_f_1_13;
            float4 props_f_1_14;
            float4 props_f_1_15;
            float4 props_f_1_16;
            float4 props_f_1_17;
            float4 props_f_1_18;
            float4 props_f_1_19;
            float4 props_f_1_20;
            float4 props_f_1_21;
            float4 props_f_1_22;
            float4 props_f_1_23;
            float4 props_f_1_24;
            float4 props_f_1_25;
            float4 props_f_1_26;
            float4 props_f_1_27;
            float4 props_f_1_28;
            float4 props_f_1_29;
            float4 props_f_1_30;
            float4 props_f_1_31;
            float4 props_f_1_32;
            float4 props_f_1_33;
            float4 props_f_1_34;
            float4 props_f_1_35;
            float4 props_f_1_36;
            float4 props_f_1_37;
            float4 props_f_1_38;
            float4 props_f_1_39;
            float4 props_f_1_40;
            float4 props_f_1_41;
            float4 props_f_1_42;
            float4 props_f_1_43;
            float4 props_f_1_44;
            float4 props_f_1_45;
            float4 props_f_1_46;
            float4 props_f_1_47;
            float4 props_f_1_48;
            float4 props_f_1_49;
            float4 props_f_1_50;
            float4 props_f_1_51;
            float4 props_f_1_52;
            float4 props_f_1_53;
            float4 props_f_1_54;
            float4 props_f_1_55;
            float4 props_f_1_56;
            float4 props_f_1_57;
            float4 props_f_1_58;
            float4 props_f_1_59;
            float4 props_f_1_60;
            float4 props_f_1_61;
            float4 props_f_1_62;
            float4 props_f_1_63;
            float4 props_f_1_64;
            float4 props_f_1_65;
            float4 props_f_1_66;
            float4 props_f_1_67;
            float4 props_f_1_68;
            float4 props_f_1_69;
            float4 props_f_1_70;
            float4 props_f_1_71;
            float4 props_f_1_72;
            float4 props_f_1_73;
            float4 props_f_1_74;
            float4 props_f_1_75;
            float4 props_f_1_76;
            float4 props_f_1_77;
            float4 props_f_1_78;
            float4 props_f_1_79;
            float4 props_f_1_80;
            float4 props_f_1_81;
            float4 props_f_1_82;
            float4 props_f_1_83;
            float4 props_f_1_84;
            float4 props_f_1_85;
            float4 props_f_1_86;
            float4 props_f_1_87;
            float4 props_f_1_88;
            float4 props_f_1_89;
            float4 props_f_1_90;
            float4 props_f_1_91;
            float4 props_f_1_92;
            float4 props_f_1_93;
            float4 props_f_1_94;
            float4 props_f_1_95;
            float4 props_f_1_96;
            float4 props_f_1_97;
            float4 props_f_1_98;
            float4 props_f_1_99;
            float4 props_f_1_100;
            float4 props_f_1_101;
            float4 props_f_1_102;
            float4 props_f_1_103;
            float4 props_f_1_104;
            float4 props_f_1_105;
            float4 props_f_1_106;
            float4 props_f_1_107;
            float4 props_f_1_108;
            float4 props_f_1_109;
            float4 props_f_1_110;
            float4 props_f_1_111;
            float4 props_f_1_112;
            float4 props_f_1_113;
            float4 props_f_1_114;
            float4 props_f_1_115;
            float4 props_f_1_116;
            float4 props_f_1_117;
            float4 props_f_1_118;
            float4 props_f_1_119;
            float4 props_f_1_120;
            float4 props_f_1_121;
            float4 props_f_1_122;
            float4 props_f_1_123;
            float4 props_f_1_124;
            float4 props_f_1_125;
            float4 props_f_1_126;
            float4 props_f_1_127;
            float4 props_f_1_128;
            float4 props_f_1_129;
            float4 props_f_1_130;
            float4 props_f_1_131;
            float4 props_f_1_132;
            float4 props_f_1_133;
            float4 props_f_1_134;
            float4 props_f_1_135;
            float4 props_f_2_0;
            float4 props_f_2_1;
            float4 props_f_2_2;
            float4 props_f_2_3;
            float4 props_f_2_4;
            float4 props_f_3_0;
            float4 props_f_4_0;
            float4 props_f_4_1;
            float4 props_f_4_2;
            float4 props_f_4_3;

            Texture2D _CameraDepthTexture;
            SamplerState sampler_CameraDepthTexture;

            Texture2D t1;
            SamplerState samplert1;

            Texture2D _CharaBitMask;
            SamplerState sampler_CharaBitMask;

            Texture2D t3;
            SamplerState samplert3;

            Texture2D _CameraColorTexture;
            SamplerState sampler_CameraColorTexture;


            v2f vert(appdata v)
            {
                v2f o;
                o.v2f_0_sv_position.xy = (((v.vertex.xy * _SSize.xy + _SSize.zw) * _perPSize.xy) * 2 + -1) * 1;
                o.v2f_0_sv_position.zw = v.vertex.zw;
                o.v2f_1.xy = v.vertex.xy;
                o.v2f_1.zw = 0;
                o.replaceUV = v.uv;
                return o;
            }

            gbuffer frag(v2f i)
            {
                gbuffer o;
                // r0.zw = (-((float)props_f_0_37.xy) + i.v2f_0_sv_position.xy) * _perPixelSize.zw;
                // r1.xy = r0.zw * props_f_0_5.xy + props_f_0_4.xy;
                // r1.xyz = SAMPLE_TEXTURE2D(_CameraColorTexture, sampler_CameraColorTexture, r1.xy);
                float3 color = SAMPLE_TEXTURE2D(_CameraColorTexture, sampler_CameraColorTexture, i.replaceUV/*r1.xy*/);
                // r2.xy = (r0.zw * props_f_1_131.xy + props_f_1_130.xy) * props_f_1_132.zw;
                // r2.zw = SAMPLE_TEXTURE2D(t1, samplert1, r2.xy).zw;
                float2 mask/*r2.zw*/ = SAMPLE_TEXTURE2D(t1, samplert1, i.replaceUV /*r2.xy*/).zw;
                uint flag0 = 0x00001111;//15
                uint flag1 = 0x00001100;//12
                uint flag2 = 0x00001101;//13
                
                // r1.w = ((int)(round((mask.y/*r2.w*/ * 255)))) & 15;
                uint litMask = (int)round(mask.y * 255);
                litMask = litMask & flag0;
                
                // r3.xy = r1.w == 12;
                int mask1 = litMask == flag1;
                int mask2 = litMask == flag2;
                
                // r2.w = r3.y | r3.x;
                int sketchMask = mask1 | mask2;
                
                // r3.yz = (round(((mask.x/*r2.z*/) * 255))) & uint2(128,127);
                uint litMask2 = (int)round(mask.x * 255);
                uint flag3 = 0x1000000;//128
                uint flag4 = 0x0111111;//127
                int mask3 = litMask2 & flag3;
                int mask4 = litMask2 & flag4;
                
                // r3.y = r2.w ? ((0 < r3.y) & 1) : 1;
                int open0 = sketchMask ? mask3 : true;
                
                // r1.w = r1.w != 12;
                int nFlag1 = litMask != flag1;
                
                // r3.w = SAMPLE_TEXTURE2D(_CharaBitMask, sampler_CharaBitMask, r2.xy).w;
                uint charaBitMask = SAMPLE_TEXTURE2D(_CharaBitMask, sampler_CharaBitMask, i.replaceUV).w;
                
                // r3.z = ((float)r3.z) * 0.0079;
                float threshold = ((float)mask4) / 127;
                
                // r2.z = r2.w ? r3.z : r2.z;
                float strength = sketchMask ? threshold : mask.x/*r2.z*/;
                
                // r2.w = r3.w < 0.99;
                int charaMask = charaBitMask < 0.99;
                
                // r2.z = ((r3.x && ((r2.z >= 0.01) & r2.w)) & 1) * r3.y;
                int strength1 = ((mask1 & ((strength >= 0.01) & charaMask)) & 1) * open0;
                
                // r1.xyz = r1.xyz * props_f_1_135.z;
                color = color * props_f_1_135.z;
                
                // r2.z = 0.5 < r2.z;
                int open1 = 0.5 < strength1;
                
                // r1.w = r1.w ? 0 : r2.z;
                int open2 = nFlag1 ? 0 : open1;
                
                // if(r1.w != 0){
                if(open2){
                  // r1.w = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, r2.xy);
                    float originalDepth = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, i.replaceUV);
                    
                  // r2.z = max(r1.w, 0);
                    r2.z = max(originalDepth, 0);

                    r3.xyzw = (r2.z * props_f_1_47.xyzw + (i.v2f_0_sv_position.x * props_f_1_45.xyzw + (i.v2f_0_sv_position.y * props_f_1_46.xyzw))) + props_f_1_48.xyzw;

                    // r3.xyz = ((r3.xyz / r3.w)) - props_f_1_71.xyz;
                    r3.xyz = ((r3.xyz / charaBitMask)) - props_f_1_71.xyz;
                    
                    r2.zw = props_f_1_131.zw * props_f_4_2.x;
                    r3.w = max((props_f_1_131.x * 0.0005), 1);
                    r2.zw = r2.zw * r3.w;
                    r4.xy = r0.xy * _perPixelSize.zw + -r2.zw;
                    r5.xyzw = r2.zw * 0.0000,-1.0000,1.0000,-1 + r0.zw;
                    r6.xyzw = r2.zw * -1.0000,0.0000,1.0000,0 + r0.zw;
                    r7.xyzw = r2.zw * -1.0000,1.0000,0.0000,1 + r0.zw;
                    r0.xy = r0.xy * _perPixelSize.zw + r2.zw;
                    r8.xyz = props_f_1_68.xyz - props_f_2_4.xyz;
                    r9.xyz = r3.xyz - props_f_2_4.xyz;
                    r0.w = 0 < (dot(r9.xyz, r8.xyz));
                    r2.w = r0.z >= (dot(r8.xyz, r8.xyz));
                    r0.z = (r0.z / r2.z);
                    r3.zw = r8.xy * r0.z + props_f_2_4.xy;
                    r2.zw = r2.w ? props_f_1_68.xy : r3.zw;
                    r0.zw = -(r0.w ? r2.zw : props_f_2_4.xy) + r3.xy;
                    r0.z = (props_f_2_0.x * props_f_4_2.y) * -((((sqrt((dot(r0.zw, r0.zw)))) - props_f_2_0.z) / (-props_f_2_0.z + props_f_2_0.y))) + 1;
                    r2.zw = clamp(((r4.xy * props_f_1_131.xy + props_f_1_130.xy) * props_f_1_132.zw), props_f_1_133.zw, props_f_1_133.xy);
                    r3.xyzw = clamp(((r5.xyzw * props_f_1_131.xy + props_f_1_130.xy) * props_f_1_132.zw), props_f_1_133.zw, props_f_1_133.xy);
                    r4.xyzw = clamp(((r6.xyzw * props_f_1_131.xy + props_f_1_130.xy) * props_f_1_132.zw), props_f_1_133.zw, props_f_1_133.xy);
                    r5.xyzw = clamp(((r7.xyzw * props_f_1_131.xy + props_f_1_130.xy) * props_f_1_132.zw), props_f_1_133.zw, props_f_1_133.xy);
                    r0.xy = clamp(((r0.xy * props_f_1_131.xy + props_f_1_130.xy) * props_f_1_132.zw), props_f_1_133.zw, props_f_1_133.xy);
                    r2.xy =  (r2.xy * props_f_1_132.xy);
                    r6.xy = round(r2.xy);
                    r6.zw = 0;
                    r0.w = t3[r6.xyzw];
                    
                    // r2.x = r1.w * props_f_1_66.x + props_f_1_66.y;
                    r2.x = originalDepth * props_f_1_66.x + props_f_1_66.y;
                    r1.w = ((1 / (r1.w * props_f_1_66.z + -props_f_1_66.w))) + r2.x;
                    r2.x = ((1 / ((SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, r2.zw)) * props_f_1_66.z + -props_f_1_66.w))) + ((SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, r2.zw)) * props_f_1_66.x + props_f_1_66.y);
                    r2.y = ((1 / ((SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, r3.xy)) * props_f_1_66.z + -props_f_1_66.w))) + ((SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, r3.xy)) * props_f_1_66.x + props_f_1_66.y);
                    r2.z = ((1 / ((SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, r3.zw)) * props_f_1_66.z + -props_f_1_66.w))) + ((SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, r3.zw)) * props_f_1_66.x + props_f_1_66.y);
                    r2.w = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, r4.xy);
                    r3.x = r2.w * props_f_1_66.x + props_f_1_66.y;
                    r2.w = ((1 / (r2.w * props_f_1_66.z + -props_f_1_66.w))) + r3.x;
                    r3.x = ((1 / ((SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, r4.zw)) * props_f_1_66.z + -props_f_1_66.w))) + ((SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, r4.zw)) * props_f_1_66.x + props_f_1_66.y);
                    r3.y = ((1 / ((SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, r5.xy)) * props_f_1_66.z + -props_f_1_66.w))) + ((SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, r5.xy)) * props_f_1_66.x + props_f_1_66.y);
                    r3.z = ((1 / ((SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, r5.zw)) * props_f_1_66.z + -props_f_1_66.w))) + ((SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, r5.zw)) * props_f_1_66.x + props_f_1_66.y);
                    r0.x = ((1 / ((SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, r0.xy)) * props_f_1_66.z + -props_f_1_66.w))) + ((SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, r0.xy)) * props_f_1_66.x + props_f_1_66.y);
                    r0.y = -props_f_3_0.x * ((r0.w == 1) && 1) + 1;
                    r0.w = -r1.w + r2.x;
                    r2.x = -r1.w + r2.y;
                    r2.y = -r1.w + r2.z;
                    r2.z = -r1.w + r2.w;
                    r2.w = -r1.w + r3.x;
                    r3.x = -r1.w + r3.y;
                    r3.y = -r1.w + r3.z;
                    r0.x = max((max((max((max((max((max((max((max((max((-r1.w + r0.x), r3.y)), r3.x)), r2.w)), r2.z)), r2.y)), r2.x)), r2.x)), r0.w)), 0);
                    r0.w = r1.w * 0.005;
                    r2.x = r0.w * props_f_4_2.z;
                    r0.w = r0.w * props_f_4_2.w + -r2.x;
                    r0.x = r0.x * 2 + -r2.x;
                    r0.w = (1 / r0.w);
                    r0.x = r0.z * (smoothstep((r0.w * r0.x)));
                    r0.w = -(props_f_2_0.w - props_f_2_1.x) + props_f_2_0.w;
                    r0.z = -r0.z + r1.w;
                    r0.w = (1 / r0.w);
                    r0.z = (smoothstep((r0.w * r0.z))) * r0.x;
                    r2.xyz = color * r0.x;
                    r0.x = r0.y * r0.z;
                    r0.yzw = r2.xyz * props_f_4_3.x + -color;
                    color = r0.x * r0.yzw + color;
                }
                o.emissive.xyz = (max((lerp(-color, props_f_4_1.xyz, props_f_4_3.y)), 0)) * props_f_1_135.y;
                o.emissive.w = 1;
                return o;
            }
            ENDHLSL
        }
    }
}