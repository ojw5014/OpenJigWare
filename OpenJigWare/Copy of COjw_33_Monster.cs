#define OJW5014_20180212
#if OJW5014_20180212
#else
#define OJW5014_20180207
#endif
using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading;



namespace OpenJigWare
{
    partial class Ojw
    {
#if OJW5014_20180212
        public class CMonster
        {
            #region Define
            // 모터의 수량은 256개 한정으로 정한다.(고정) -> 추후 가변으로 바꿀지는 완성 이후 고민
            private const int _CNT = 256;
            private int m_nMotorCountAll = _CNT; // _CNT 를 바로 사용하지 말고 이 변수를 사용하도록 한다.(추후 확장성)
            private const int _ADDRESS_TORQ_XL_320 = 24;
            private const int _ADDRESS_TORQ_XL_430 = 64;
            private const int _ADDRESS_TORQ_AX = 24;
            public enum EMotor_t
            {
                NONE = 0,
                // Default
                XL_430 = 20, // 360도 4096 : 2048 center : 0.229 RefRpm, 480 Limit Rpm
                XL_320 = 21, // 300도 1024 : 512 center : 0.111 RefRpm, 1023 Limit Rpm

                XM_540 = 30, // LED [65], 360도 4096 : 2048 center : 0.229 RefRpm, 480 Limit Rpm  , 확장위치제어모드시 512 회전 가능(+-256)

                AX_12 = 1, // 9~12v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시)
                AX_18 = 2, //

                DX_113 = 3, //
                DX_116 = 4, //
                DX_117 = 5, //
                RX_10 = 6, // 9~12v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시)
                RX_24F = 7, // 9~12v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시)
                RX_28 = 8, // 12v~18.5v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시)
                RX_64 = 9, // 12v~18.5v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시)
                EX_106 = 10, // 12v~18.5v, 0~251도, c=2048, mx=4095, mn=0, 


                // protocol 2.0
                MX_12 = 11, // 10~14.8v, 0~360, c=2048, mx=4095, mn=0, drpm = 0.916rpm, 다중회전가능(각 7바퀴:-28,672~28,672)
                MX_28 = 12, // 10~14.8v, 0~360, c=2048, mx=4095, mn=0, drpm = 0.114rpm, 다중회전가능(각 7바퀴:-28,672~28,672)  
                MX_64 = 13, // 10~14.8v, 0~360, c=2048, mx=4095, mn=0, drpm = 0.114rpm, 다중회전가능(각 7바퀴:-28,672~28,672)  
                MX_106 = 14, // 10~14.8v, 0~360, c=2048, mx=4095, mn=0, drpm = 0.114rpm, 다중회전가능(각 7바퀴:-28,672~28,672)  
                //MX_ = 15, //

                SG_90 = 100
            }

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            #region define Protocol 1
                private const int _ADDR_1_SIZE = 50;
                private const int _ADDR_1_MotorNumber_0_2                   = 0;    // R 
                private const int _ADDR_1_MotorNumber_SIZE_2                = 2;

                private const int _ADDR_1_FwVersion_2_1                     = 2;    // R       
                private const int _ADDR_1_FwVersion_1                       = 1;
                private const int _ADDR_1_RealID_3_1                        = 3;    // RW 
                private const int _ADDR_1_RealID_SIZE_1                     = 1;
                private const int _ADDR_1_Baudrate_4_1                      = 4;    // RW
                private const int _ADDR_1_Baudrate_SIZE_1                   = 1;
                private const int _ADDR_1_ReturnDelayTime_5_1               = 5;    // RW
                private const int _ADDR_1_ReturnDelayTime_SIZE_1            = 1;

                private const int _ADDR_1_Set_CWAngleLimit_6_2              = 6;    // RW
                private const int _ADDR_1_Set_CWAngleLimit_SIZE_2           = 2;
                private const int _ADDR_1_Set_CCWAngleLimit_8_2             = 8;    // RW
                private const int _ADDR_1_Set_CCWAngleLimit_SIZE_2          = 2;
                private const int _ADDR_1_Limit_Temp_11_1                   = 11;   // RW 
                private const int _ADDR_1_Limit_Temp_Size_1                 = 1;
                private const int _ADDR_1_Limit_Volt_Down_12_1              = 12;   // RW  
                private const int _ADDR_1_Limit_Volt_Down_Size_1            = 1;
                private const int _ADDR_1_Limit_Volt_Up_13_1                = 13;   // RW  
                private const int _ADDR_1_Limit_Volt_Up_Size_1              = 1;
                private const int _ADDR_1_Max_Torq_14_2                     = 14;   // RW  
                private const int _ADDR_1_Max_Torq_Size_2                   = 2;
                private const int _ADDR_1_Return_Level_16_1                 = 16;   // RW  
                private const int _ADDR_1_Return_Level_Size_1               = 1;

                private const int _ADDR_1_Alarm_Led_17_1                    = 17;   // RW
                private const int _ADDR_1_Alarm_Led_Size_1                  = 1;
                private const int _ADDR_1_Alarm_Shutdown_18_1               = 18;   // RW
                private const int _ADDR_1_Alarm_Shutdown_Size_1             = 1;
                /// Ram /////////////////////////////////////////////////////////////////////////////
                private const int _ADDR_1_Set_TorqOnOff_24_1                = 24;   // RW
                private const int _ADDR_1_Set_TorqOnOff_Size_1              = 1;
                private const int _ADDR_1_Set_LED_25_1                      = 25;   // RW
                private const int _ADDR_1_Set_LED_Size_1                    = 1;

                private const int _ADDR_1_CW_Margin_26_1                    = 26;   // RW
                private const int _ADDR_1_CW_Margin_SIZE_1                  = 1;
                private const int _ADDR_1_CCW_Margin_27_1                   = 27;   // RW
                private const int _ADDR_1_CCW_Margin_SIZE_1                 = 1;
                private const int _ADDR_1_CW_Slope_28_1                     = 28;   // RW
                private const int _ADDR_1_CW_Slope_Size_1                   = 1;
                private const int _ADDR_1_CCW_Slope_29_1                    = 29;   // RW
                private const int _ADDR_1_CCW_Slope_Size_1                  = 1;

                private const int _ADDR_1_Set_Goal_Position_30_2            = 30;   // RW
                private const int _ADDR_1_Set_Goal_Position_Size_2          = 2;
                private const int _ADDR_1_Set_Moving_Speed_32_2             = 32;   // RW
                private const int _ADDR_1_Set_Moving_Speed_Size_2           = 2;
                private const int _ADDR_1_Set_Torque_Limit_34_2             = 34;   // RW
                private const int _ADDR_1_Set_Torque_Limit_Size_2           = 2;
                private const int _ADDR_1_Present_Pos_36_2                  = 36;   // R
                private const int _ADDR_1_Present_Pos_Size_2                = 2;
                private const int _ADDR_1_Present_Speed_38_2                = 38;   // R
                private const int _ADDR_1_Present_Speed_Size_2              = 2;
                private const int _ADDR_1_Present_Load_41_2                 = 40;   // R
                private const int _ADDR_1_Present_Load_Size_2               = 2;

                private const int _ADDR_1_Present_Volt_42_1                 = 42;   // R
                private const int _ADDR_1_Present_Volt_Size_1               = 1;
                private const int _ADDR_1_Present_Temp_43_1                 = 43;   // R
                private const int _ADDR_1_Present_Temp_Size_1               = 1;
                private const int _ADDR_1_Reg_Instruction_44_1              = 44;   // R
                private const int _ADDR_1_Reg_Instruction_Size_1            = 1;
                private const int _ADDR_1_IsMoving_46_1                     = 46;   // R
                private const int _ADDR_1_IsMoving_Size_1                   = 1;
                // 47,48,49
                #endregion define Protocol 1
                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                #region define XL-320
                private const int _ADDR_XL320_SIZE = 50;
                private const int _ADDR_XL320_MotorNumber_0_2               = 0;    // R 
                private const int _ADDR_XL320_MotorNumber_SIZE_2            = 2;

                private const int _ADDR_XL320_FwVersion_2_1                 = 2;    // R       
                private const int _ADDR_XL320_FwVersion_1                   = 1;
                private const int _ADDR_XL320_RealID_3_1                    = 3;    // RW 
                private const int _ADDR_XL320_RealID_SIZE_1                 = 1;    
                private const int _ADDR_XL320_Baudrate_4_1                  = 4;    // RW
                private const int _ADDR_XL320_Baudrate_SIZE_1               = 1;
                private const int _ADDR_XL320_ReturnDelayTime_5_1           = 5;    // RW
                private const int _ADDR_XL320_ReturnDelayTime_SIZE_1        = 1;

                private const int _ADDR_XL320_Set_CWAngleLimit_6_2          = 6;    // RW
                private const int _ADDR_XL320_Set_CWAngleLimit_SIZE_2       = 2;
                private const int _ADDR_XL320_Set_CCWAngleLimit_8_2         = 8;    // RW
                private const int _ADDR_XL320_Set_CCWAngleLimit_SIZE_2      = 2;
                private const int _ADDR_XL320_Set_Mode_Control_11_1         = 11;   // RW 
                private const int _ADDR_XL320_Set_Mode_Control_Size_1       = 1;
                private const int _ADDR_XL320_Limit_Temp_12_1               = 12;   // RW 
                private const int _ADDR_XL320_Limit_Temp_Size_1             = 1;
                private const int _ADDR_XL320_Limit_Volt_Down_13_1          = 13;   // RW  
                private const int _ADDR_XL320_Limit_Volt_Down_Size_1        = 1;
                private const int _ADDR_XL320_Limit_Volt_Up_14_1            = 14;   // RW  
                private const int _ADDR_XL320_Limit_Volt_Up_Size_1          = 1;
                private const int _ADDR_XL320_Max_Torq_15_2                 = 15;   // RW  
                private const int _ADDR_XL320_Max_Torq_Size_2               = 2;
                private const int _ADDR_XL320_Return_Level_17_1             = 17;   // RW  
                private const int _ADDR_XL320_Return_Level_Size_1           = 1;
                private const int _ADDR_XL320_Alarm_Shutdown_18_1           = 18;   // RW
                private const int _ADDR_XL320_Alarm_Shutdown_Size_1         = 1;
                /// Ram /////////////////////////////////////////////////////////////////////////////
                private const int _ADDR_XL320_Set_TorqOnOff_24_1            = 24;   // RW
                private const int _ADDR_XL320_Set_TorqOnOff_Size_1          = 1;
                private const int _ADDR_XL320_Set_LED_25_1                  = 25;   // RW
                private const int _ADDR_XL320_Set_LED_Size_1                = 1;

                private const int _ADDR_XL320_GAIN_D_27_1                   = 27;   // RW
                private const int _ADDR_XL320_GAIN_D_Size_1                 = 1;
                private const int _ADDR_XL320_GAIN_I_28_1                   = 28;   // RW
                private const int _ADDR_XL320_GAIN_I_Size_1                 = 1;
                private const int _ADDR_XL320_GAIN_P_29_1                   = 29;   // RW
                private const int _ADDR_XL320_GAIN_P_Size_1                 = 1;

                private const int _ADDR_XL320_Set_Goal_Position_30_2        = 30;   // RW
                private const int _ADDR_XL320_Set_Goal_Position_Size_2      = 2;
                private const int _ADDR_XL320_Set_Moving_Speed_32_2         = 32;   // RW
                private const int _ADDR_XL320_Set_Moving_Speed_Size_2       = 2;
                private const int _ADDR_XL320_Set_Torque_Limit_35_2         = 35;   // RW
                private const int _ADDR_XL320_Set_Torque_Limit_Size_2       = 2;
                private const int _ADDR_XL320_Present_Pos_37_2              = 37;   // R
                private const int _ADDR_XL320_Present_Pos_Size_2            = 2;
                private const int _ADDR_XL320_Present_Speed_39_2            = 39;   // R
                private const int _ADDR_XL320_Present_Speed_Size_2          = 2;
                private const int _ADDR_XL320_Present_Load_41_2             = 41;   // R
                private const int _ADDR_XL320_Present_Load_Size_2           = 2;

                private const int _ADDR_XL320_Present_Volt_45_1             = 45;   // R
                private const int _ADDR_XL320_Present_Volt_Size_1           = 1;
                private const int _ADDR_XL320_Present_Temp_46_1             = 46;   // R
                private const int _ADDR_XL320_Present_Temp_Size_1           = 1;
                private const int _ADDR_XL320_Reg_Instruction_47_1          = 47;   // R
                private const int _ADDR_XL320_Reg_Instruction_Size_1        = 1;
                private const int _ADDR_XL320_IsMoving_49_1                 = 49;   // R
                private const int _ADDR_XL320_IsMoving_Size_1               = 1;

                //private const int _ADDR_XL320_Hardware_Error_50_1          = 50;   // R
                //private const int _ADDR_XL320_Hardware_Error_Size_1        = 1;
                //private const int _ADDR_XL320_Punch_51_2                   = 51;   // RW
                //private const int _ADDR_XL320_Punch_Size_1                 = 1;
                #endregion define XL-320
                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                #region define PROTOCOL 2
                private const int _ADDR_2_MotorNumber_0_2                   = 0;    // R    0 : none
                private const int _ADDR_2_MotorNumber_Size_2                = 2;
                private const int _ADDR_2_FwVersion_6_1                     = 6;    // R      
                private const int _ADDR_2_FwVersion_Size_1                  = 1;
                private const int _ADDR_2_RealID_7_1                        = 7;    // RW   0 ~ 252
                private const int _ADDR_2_RealID_Size_1                     = 1;
                private const int _ADDR_2_Baudrate_8_1                      = 8;    // RW   0-9600, 1-57600, 2-115200, 3-1M, 4-2M,  5-3M, 6-4M, 7-4.5M
                private const int _ADDR_2_Baudrate_Size_1                   = 1;
                // [DriveMode] 
                //    0x01 : 정상회전(0), 역회전(1)
                //    0x02 : 540 전용 Master(0), Slave(1)
                //    0x04 : 속도기준 제어(0), 시간기준 제어(1)
                private const int _ADDR_2_Set_Mode_Drive_10_1               = 10;   // RW 
                private const int _ADDR_2_Set_Mode_Drive_Size_1             = 1;
                private const int _ADDR_2_Set_Mode_Operating_11_1           = 11;   // RW   1=속도제어(무한회전), 3(초기값)=위치제어(RPM), 4=Multi-Turn(-256~256:512회전), 5=전류기반 위치제어모드(-256~256:512회전), 16=PWM제어
                private const int _ADDR_2_Set_Mode_Operating_Size_1         = 1;
                private const int _ADDR_2_ProtocolVersion_13_1              = 13;   // RW   
                private const int _ADDR_2_ProtocolVersion_Size_1            = 1;
                private const int _ADDR_2_Set_Offset_20_4                   = 20;   // RW   1회전 모드인 경우 -1024~1024 까지만 제어
                private const int _ADDR_2_Set_Offset_Size_4                 = 4;
                private const int _ADDR_2_Limit_PWM_36_2                    = 36;   // RW   0~885 (885 = 100%)
                private const int _ADDR_2_Limit_PWM_Size_2                  = 2;
                private const int _ADDR_2_Limit_Curr_38_2                   = 38;   // RW   전류제어모드/전류기반위치제어모드에서 사용, 0~2047
                private const int _ADDR_2_Limit_Curr_Size_2                 = 2;
                // [Shutdown] - Reboot 으로만 해제 가능
                //    0x20 : 과부하
                //    0x10 : 전력이상
                //    0x08 : 엔코더 이상(Following Error)
                //    0x04 : 과열
                //    0x01 : 인가된 전압 이상
                private const int _ADDR_2_Shutdown_63_1                     = 63;   // RW
                private const int _ADDR_2_Shutdown_Size_1                   = 1;
                private const int _ADDR_2_Set_Torq_64_1                     = 64;   // RW   Off(0), On(1)
                private const int _ADDR_2_Set_Torq_Size_1                   = 1;
                private const int _ADDR_2_Set_Led_65_1                      = 65;   // RW   Off(0), On(1)
                private const int _ADDR_2_Set_Led_Size_1                    = 1;
                private const int _ADDR_2_IsReg_Instruction_69_1            = 69;   // R    
                private const int _ADDR_2_IsReg_Instruction_Size_1          = 1;
                private const int _ADDR_2_IsError_70_1                      = 70;   // R    
                private const int _ADDR_2_IsError_Size_1                    = 1;
                private const int _ADDR_2_Gain_Vel_I_76_2                   = 76;   // RW
                private const int _ADDR_2_Gain_Vel_I_Size_2                 = 2;
                private const int _ADDR_2_Gain_Vel_P_78_2                   = 78;   // RW
                private const int _ADDR_2_Gain_Vel_P_Size_2                 = 2;
                private const int _ADDR_2_Gain_Pos_D_80_2                   = 80;   // RW
                private const int _ADDR_2_Gain_Pos_D_Size_2                 = 2;
                private const int _ADDR_2_Gain_Pos_I_82_2                   = 82;   // RW
                private const int _ADDR_2_Gain_Pos_I_Size_2                 = 2;
                private const int _ADDR_2_Gain_Pos_P_84_2                   = 84;   // RW
                private const int _ADDR_2_Gain_Pos_P_Size_2                 = 2;
                private const int _ADDR_2_Gain_Pos_F2_88_2                  = 88;   // RW
                private const int _ADDR_2_Gain_Pos_F2_Size_2                = 2;
                private const int _ADDR_2_Gain_Pos_F1_90_2                  = 90;   // RW
                private const int _ADDR_2_Gain_Pos_F1_Size_2                = 2;

                private const int _ADDR_2_WatchDog_98_1                     = 98;   // RW
                private const int _ADDR_2_WatchDog_Size_1                   = 1;

                private const int _ADDR_2_Set_Goal_PWM_100_2                = 100;  // RW   -PWMLimit ~ +PWMLimit
                private const int _ADDR_2_Set_Goal_PWM_Size_2               = 2;
                private const int _ADDR_2_Set_Goal_Current_102_2            = 102;  // RW   -CurrentLimit ~ +CurrentLimit
                private const int _ADDR_2_Set_Goal_Current_Size_2           = 2;
                private const int _ADDR_2_Set_Goal_Vel_104_4                = 104;  // RW
                private const int _ADDR_2_Set_Goal_Vel_Size_4               = 4;

                private const int _ADDR_2_Profile_Acc_108_4                 = 108;  // RW
                private const int _ADDR_2_Profile_Acc_Size_4                = 4;
                private const int _ADDR_2_Profile_Vel_112_4                 = 112;  // RW
                private const int _ADDR_2_Profile_Vel_Size_4                = 4;

                private const int _ADDR_2_Set_Goal_Pos_116_4                = 116;  // RW
                private const int _ADDR_2_Set_Goal_Pos_Size_4               = 4;

                private const int _ADDR_2_RealTime_Tick_120_1               = 120;  // R    
                private const int _ADDR_2_RealTime_Tick_Size_1              = 1;
                private const int _ADDR_2_IsMoving_122_1                    = 122;  // R    움직임 감지 못함(0), 움직임 감지(1)
                private const int _ADDR_2_IsMoving_Size_1                   = 1;
                // [Moving Status]
                //    0x30  - 0x30 : 사다리꼴 속도 프로파일
                //            0x20 : 삼각 속도 프로파일
                //            0x10 : 사각 속도 프로파일
                //            0x00 : 프로파일 미사용(Step)
                //    0x08 : Following Error
                //    0x02 : Goal Position 명령에 따라 진행 중
                //    0x01 : Inposition
                private const int _ADDR_2_Moving_Status_123_1               = 123;    // R
                private const int _ADDR_2_Moving_Status_Size_1              = 1;

                private const int _ADDR_2_Present_PWM_124_2                 = 124;    // R
                private const int _ADDR_2_Present_PWM_Size_2                = 2;
                private const int _ADDR_2_Present_Curr_126_2                = 126;    // R
                private const int _ADDR_2_Present_Curr_Size_2               = 2;
                private const int _ADDR_2_Present_Vel_128_4                 = 128;    // R
                private const int _ADDR_2_Present_Vel_Size_4                = 4;
                private const int _ADDR_2_Present_Pos_132_4                 = 132;    // R
                private const int _ADDR_2_Present_Pos_Size_4                = 4;
                private const int _ADDR_2_Present_Volt_144_2                = 144;    // R
                private const int _ADDR_2_Present_Volt_Size_2               = 2;
                private const int _ADDR_2_Present_Temp_146_1                = 146;    // R
                private const int _ADDR_2_Present_Temp_Size_1               = 1;
                #endregion define PROTOCOL 2
                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            #endregion Define




            #region Structure
            public struct SAddress_t
            {
                // 잘 쓰는 주소번지 모음
                public int nLed;
                public int nLed_Size;
                
                public int nTorq;
                public int nTorq_Size;
                
                public int nDriveMode;
                public int nDriveMode_Size;
                
                public int nOperatingMode;
                public int nOperatingMode_Size;

                public int nWheel_Speed;
                public int nWheel_Speed_Size;

                public int nPos_Speed;
                public int nPos_Speed_Size;

                public int nPos_Speed;
                public int nPos_Speed_Size;

                //public int nPos_Speed;
                //public int nPos_Speed_Size;
            }
            public struct SMap_t
            {
                public int nSeq;
                public int nSeq_Back;

                public EMotor_t EMot;
                public int nSize;

                public byte[] buffer; // 100 개
            }
            public struct SParam_t
            {
                //public int nID;                     // 모터를 제어하기 위한 가상 아이디. 중복 허용 불가. 보통은 모터 ID 와 일치시킨다.            
                public int nCommIndex;              // 연결 된 통신포트 중 어느것에 연결 된 것이지에 대한 순서.
                public int nRealID;
                public EMotor_t EMot;

                /////////////////////////////////////////////////////////////////
                public int nSeq;
                public int nSeq_Back;

                public int nDriveMode;   // 0x00 - Rpm 모드, 0x40 - Time 제어 모드
                public int nOperatingMode; // Operating Mode (0-전류제어, 1-속도제어, 3-위치제어(default), 4-확장위치제어, 5-전류기반 위치제어(그리퍼), 16-PWM 제어
                /////////////////////////////////////////////////////////////////

                public int nDir;
                //Center
                public float fCenterPos;

                public float fMechMove;
                public float fDegree;
                public float fRefRpm;

                public float fLimitRpm;     // ?

                public float fLimitUp;    // Limit - 0: Ignore
                public float fLimitDn;    // Limit - 0: Ignore

                public int nProtocol;               // 2, 0 - protocol2, 1 - protocol1, 3 - None   
            }
            public struct SStatus_t
            {
                public int nError;
                public int nTorq;
                public int nLed;
            }
            // 0-전류제어, 1-속도제어(바퀴제어), 3-위치제어(default), 4-확장위치제어(X시리즈의 경우 -256 ~ +256회전 지원), 5-전류기반 위치제어(그리퍼)(X시리즈의 경우 -256 ~ +256회전 지원), 16-PWM 제어
            public enum ESetup_t
            {
                _None = -1,
                _Amp = 0,
                _Speed = 1, // 속도제어모드
                _Position = 3,
                _Position_Multi = 4,
                _Position_Amp = 5,
                _Pwm = 16
            }
            //public struct SMap_t
            //{
            //    public bool bShow;
            //    public int nSeq;
            //    public int nSeq_Back;

            //    public int nStep;
            //    public int nPortIndex;
            //    public int nProtocol;

            //    public int nAddress;
            //    public int nAddress_DataLength;

            //    public int nID;
            //    public int nLength;
            //    public int nCmd;
            //    public int nError;
            //    public int nModelNumber;
            //    public int nFw;
            //    public int nCrc0;
            //    public int nCrc1;
            //    public EMotor_t EMotor;

            //    public byte[] abyMap;
            //}

            public struct SMot_t
            {

            }

            //public struct SMot_t
            //{
            //    //public EEnable_t EEnable; // 1 : Pos, 2 : Speed, 3 : Packet
            //    //public int nOperationMode;
            //    //public int nOperationMode_Prev;

            //    public bool bInit_Value;
            //    // position
            //    public int nValue;
            //    public int nValue_Prev;
            //    public float fAngle_Back;
            //    // speed
            //    public int nValue2;
            //    public bool bSetValue2;
            //    public int nValue2_Prev;

            //    public int nStatus_Torq;
            //    public int nStatus_Torq_Prev;
            //    public int nStatus_Error;
            //    public int nStatus_Error_Prev;

            //    public bool bOperationMode;
            //    public EOperationMode_t EOperationMode;          // 2 - 전류제어모드(실제 제어에서는 0을 사용), 1 - 속도제어모드(바퀴제어), 3(default) - 위치제어모드, 4 - 확장위치제어모드(X시리즈의 경우 -256 ~ +256회전 지원), 5  -전류기반 위치제어모드(X시리즈의 경우 -256 ~ +256회전 지원), 16 - PWM 제어모드
            //    public EOperationMode_t EOperationMode_Prev;

            //    public Ojw.CTimer CTmr;
            //    public byte[] abyBuffer;
            //}
            #endregion Structure


            #region Var
            private List<CSerial> m_lstSerial = null;//new List<CSerial>();
            private List<int> m_lstIndex = new List<int>();
            private List<int> m_lstID = new List<int>();
            private Thread m_thReceive = null;
            private List<int> m_lstPort = null;

            private bool m_bProgEnd = false;
            private bool m_bStop = false;
            private bool m_bEms = false;
            private bool m_bMotionEnd = false;
            private bool m_bStart = false;

            //private int[] m_anMot_SerialIndex = new int[_CNT];
            //private int[] m_anMot_RealID = new int[_CNT];
            //private EMotor_t[] m_aEMot_Type = new EMotor_t[_CNT];
            private SAddress_t[] m_aSAddress = new SAddress_t[_CNT];
            private SParam_t[] m_aSParam = new SParam_t[_CNT];

            private int m_nMotorCnt = 0;// { get { return m_lstSCommand.Count; } }
            private int[] m_anEn = new int[256]; // push / pop

            private SMot_t[] m_aSMot = new SMot_t[_CNT];
            //private List<SMap_t> m_lstSMap = new List<SMap_t>();//[_CNT];
            //private SMap_t[] m_aSMap_New = new SMap_t[_CNT];
            private SMap_t[] m_aSMap = new SMap_t[_CNT];
            //private SMap_t[] m_aSMap_Old = new SMap_t[_CNT];
            //private int [] m_anMap_Address = new int[_CNT];




            #endregion Var
            private struct SMotors_t
            {
                public int nRealID;
                public int nCommIndex;
                public int nProtocol;
                public EMotor_t EMotor;
            }
            private List<SMotors_t> m_lstSIds = new List<SMotors_t>();
            private bool m_bAutoset = false;
            private bool m_bAutoset2 = false;
            private Ojw.CTimer m_CTmr_AutoSet = new CTimer();
            public void SetShowReceiving(bool bShow)
            {
                m_bShowReceivedIDs = bShow;
            }
            public void AutoSet() { AutoSet(true, true); }
            public void AutoSet(bool bDisplay, bool bDisplay_Receive)
            {
                m_bShowReceivedIDs = false;
                m_bAutoset = true;
                m_bAutoset2 = false;
                m_lstSIds.Clear();
                Write_Ping(false);
                //m_CTmr_AutoSet.Set();
                //while ((m_CTmr_AutoSet.Get() < 100) && (m_bProgEnd == false)) Thread.Sleep(1);//Ojw.CTimer.Wait(1); ///Thread.Sleep(1);// Ojw.CTimer.Wait();
                if (m_bProgEnd == true) return;
                else
                {
                    m_bAutoset2 = true;
                    // 
                    m_bShowReceivedIDs = bDisplay_Receive;
                    for (int i = 0; i < m_lstSerial.Count; i++)
                    {
                        // 프로토콜 2의 아이디만 수집한다. (싱크리드 명령이 되니까...)
                        List<int> lstIds = new List<int>();
                        lstIds.Clear();
                        foreach (SMotors_t SMot in m_lstSIds)
                        {
                            if (SMot.nCommIndex == i)
                            {
                                //if ((SMot.nProtocol == 2) && (SMot.nCommIndex == i))
                                if (SMot.nProtocol == 2)
                                    lstIds.Add(SMot.nRealID);
                                else
                                {
                                    m_CTmr_AutoSet.Set();
                                    Write2(1, i, SMot.nRealID, 0x02, 0, 50);
                                    Ojw.CTimer.Wait(50);
                                    //m_CTmr_AutoSet.Set();
                                    //while ((m_CTmr_AutoSet.Get() < 200) && (m_bProgEnd == false)) Thread.Sleep(1);//Ojw.CTimer.Wait(1); ///Thread.Sleep(1); //나중에 받으면 넘어가도록 만들 것.
                                }
                            }
                        }
                        if (lstIds.Count > 0)
                        {
                            m_CTmr_AutoSet.Set();
                            Reads(i, 0, 50, lstIds.ToArray()); // 일단 50 까지만 읽는다.(xl-320 때문에...) 나중에 XL-320 이 아니라면 147 까지 읽어온다.
                            Ojw.CTimer.Wait(50);
                            //m_CTmr_AutoSet.Set();
                            //while ((m_CTmr_AutoSet.Get() < 300) && (m_bProgEnd == false)) Thread.Sleep(1);//Ojw.CTimer.Wait(1); ///Thread.Sleep(1);
                        }
                        // 
                    }
                    ///for(m_lstSIds
                    if (bDisplay)
                    {
                        Ojw.CMessage.Write2("Motors=>\r\n");
                        foreach (SMotors_t SMot in m_lstSIds)
                        {
                            int nMotor = FindMotor(SMot.nRealID);
                            Ojw.CMessage.Write2("[{0}]-{1}\r\n", nMotor, m_aSParam[nMotor].EMot);

                        }
                    }
                }
            }
            private EMotor_t GetMotorType(int nHwNum)
            {
                switch (nHwNum)
                {
                    case 113: return EMotor_t.DX_113;
                    case 116: return EMotor_t.DX_116;
                    case 117: return EMotor_t.DX_117;
                    case 18: return EMotor_t.AX_18;
                    case 44:
                    case 12: return EMotor_t.AX_12;
                    //case 300: return EMotor_t.AX_12;
                    case 10: return EMotor_t.RX_10;
                    case 24: return EMotor_t.RX_24F;
                    case 28: return EMotor_t.RX_28;
                    case 64: return EMotor_t.RX_64;
                    case 107: return EMotor_t.EX_106;
                    case 360: return EMotor_t.MX_12;
                    ////////////////////////////////////
                    //여기부터 2.0
                    case 30: return EMotor_t.MX_28;
                    case 311: return EMotor_t.MX_64;
                    case 321: return EMotor_t.MX_106;
                    case 350: return EMotor_t.XL_320;
                    //XM-430((W210)1030, (W350)1020)
                    //XM-540((W150)1130, (W270)1120)
                    //XH-430((W210)1010, (W350)1000, (V210)1050, (V350)1040)
                    case 1030:
                    case 1020:
                    case 1130:
                    case 1120:
                    case 1010:
                    case 1000:
                    case 1050:
                    case 1040:
                    case 1060: return EMotor_t.XL_430;
                }
                return EMotor_t.NONE; ;
            }
            private bool m_bShowReceivedIDs = false;
            // 초기화
            private void Init()
            {
                for (int i = 0; i < m_nMotorCountAll; i++)
                {
                    SetParam(i, i, 0, 0, EMotor_t.XL_430);
                    ////    m_anMot_SerialIndex[i] = 0;
                    ////    m_anMot_RealID[i] = i;
                    ////    m_aEMot_Type[i] = EMotor_t.XL_430;
                    ////    SetAddress(i, EMotor_t.XL_430);
                    //m_aSMot[i].bInit_Value = false;
                    //m_aSMot[i].EEnable = EEnable_t._None;
                    //m_aSMot[i].CTmr = new CTimer();
                    //m_aSMot[i].nStatus_Torq = 0;
                    //m_aSMot[i].nValue = 0;
                    //m_aSMot[i].nValue_Prev = 0;
                    //m_aSMot[i].fAngle_Back = 0.0f; // 초기화 때만 클리어 하고 남겨두는 데이타
                    //m_aSMot[i].bSetValue2 = false;
                    //m_aSMot[i].nValue2 = 0;
                    //m_aSMot[i].nValue2_Prev = 0;
                    //m_aSMot[i].nStatus_Error = 0;
                    //m_aSMot[i].bOperationMode = false;
                    //m_aSMot[i].EOperationMode = EOperationMode_t._Position; // Default

                    //m_aSMap[i] = new SMap_t();
                    //m_aSMap[i].abyMap = new byte[256];
                    ////m_aSMap_Old[i] = new SMap_t();
                    ////m_aSMap_Old[i].abyMap = new byte[200];
                    ////m_anMap_Address[i] = -1;
                }
                //Array.Clear(m_aSMap, 0, m_aSMap.Length);
                InitKinematics();
            }
           /* private void Done(int nMotor)
            {
                m_aSMot[nMotor].EEnable = EEnable_t._None;
                m_aSMot[nMotor].bOperationMode = false;

                //m_aSMot[nMotor].CTmr = new CTimer();
                if (m_bEms == true) m_aSMot[nMotor].bInit_Value = false;
                m_aSMot[nMotor].nValue_Prev = m_aSMot[nMotor].nValue;
                m_aSMot[nMotor].nValue2_Prev = m_aSMot[nMotor].nValue2;
                //m_aSMot[nMotor].nStatus_Error = 0;
                //m_aSMot[nMotor].nOperationMode = 0;
                m_aSMot[nMotor].EOperationMode_Prev = m_aSMot[nMotor].EOperationMode;
                m_aSMot[nMotor].bSetValue2 = false;
                m_aSMot[nMotor].CTmr.Set();
            }
            * */
            public CMonster()
            {
                Init();
            }
            ~CMonster()
            {
                Close();
            }

            public void Stop(int nMotor) // no stop flag setting
            {
                byte[] pbyTmp = Ojw.CConvert.IntToBytes(0);
                //Write(nAxis, 104, pbyTmp);
                if (m_aSParam[nMotor].EMot != EMotor_t.SG_90)
                {
                    //Write2(m_aSParam[nMotor].nProtocol, m_aSParam[nMotor].nCommIndex, ((nMotor < 0) ? 254 : (m_aSParam[nMotor].nRealID)), 0x03, m_aSAddress[nMotor].nGoal_Vel_104_4, 0);

                }
            }
            public void Stop()
            {
                byte[] pbyTmp = Ojw.CConvert.IntToBytes(0);
                //Write(nAxis, 104, pbyTmp);
                for (int i = 0; i < GetSerialCount(); i++)
                {
                    //if (m_aSParam[nMotor].EMot != EMotor_t.SG_90)
                    //{
                    // 속도
                    Write2(1, i, 254, 0x03, 32, 0);
                    Write2(2, i, 254, 0x03, 104, 0);

                    //}
                }
                m_bStop = true;
            }
            public void Ems()
            {
                Stop();
                SetTorq(false);
                m_bEms = true;
            }
            public void Reset()//(int nAxis)
            {
                // Clear Variable
                m_bStop = false;
                m_bEms = false;
            }
            public void Reboot() { Reboot(-1); }//_ID_BROADCASTING); }
            public void Reboot(int nMotor)
            {
                if ((nMotor < 0) || (nMotor == 254))
                {
                    //byte[] pbyTmp = Ojw.CConvert.IntToBytes(0);
                    //Write(nAxis, 104, pbyTmp);
                    for (int i = 0; i < GetSerialCount(); i++)
                    {
                        //if (m_aSParam[nMotor].EMot != EMotor_t.SG_90)
                        //{
                        // 속도
                        Write_Command(1, i, ((nMotor < 0) ? 254 : nMotor), 0x08);
                        Write_Command(2, i, ((nMotor < 0) ? 254 : nMotor), 0x08);

                        //}
                    }
                    //Clear_Flag();
                    m_bStop = false;
                    m_bEms = false;
                }
                else Write_Command(m_aSParam[nMotor].nProtocol, m_aSParam[nMotor].nCommIndex, nMotor, 0x08);
                // Initialize variable

                //m_nMotorCnt_Back = m_nMotorCnt = 0;

            }
            public bool IsStop() { return m_bStop; }
            public bool IsEms() { return m_bEms; }

            #region Open / Close, IsOpen, GetSerial, GetSerialIndex, GetSerialPort, GetSerialCount
            // 해당 컴포트가 열렸는지를 확인
            public bool IsOpen(int nPort)
            {
                if (m_lstPort != null)
                {
                    foreach (int nSerialPort in m_lstPort) if (nSerialPort == nPort) return true;
                }
                return false;
            }
            // 해당 포트의 시리얼 핸들을 리턴
            public CSerial GetSerial(int nPort)
            {
                if (m_lstPort != null)
                {
                    for (int i = 0; i < m_lstPort.Count; i++)
                    {
                        if (m_lstPort[i] == nPort) return m_lstSerial[i];
                    }
                }
                return null;
            }
            // 해당 포트의 인덱스 번호를 리턴
            public int GetSerialIndex(int nPort)
            {
                if (m_lstPort != null)
                {
                    for (int i = 0; i < m_lstPort.Count; i++)
                    {
                        if (m_lstPort[i] == nPort) return i;
                    }
                }
                return -1;
            }
            // 해당 인덱스의 포트 번호를 리턴
            public int GetSerialPort(int nCommIndex)
            {
                if (m_lstPort != null)
                {
                    if (nCommIndex < m_lstPort.Count)
                    {
                        return m_lstPort[nCommIndex];
                    }
                }
                return -1;
            }
            // 전체 포트의 인덱스 수를 리턴
            public int GetSerialCount()
            {
                if (m_lstPort != null)
                {
                    return m_lstPort.Count;
                }
                return -1;
            }

            //// * 중요: 오픈시에 Operation Mode, 모터 종류, 아이디, 위치값, 토크온 상태 를 가져와야 한다.
            // 컴포트를 연다.(중복 되지만 않으면 여러개를 여는 것이 가능)
            public bool Open(int nPort, int nBaudRate)
            {
                Ojw.CSerial COjwSerial = new CSerial();
                if (COjwSerial.Connect(nPort, nBaudRate) == true)
                {
                    if (m_lstSerial == null)
                    {
                        m_lstSerial = new List<CSerial>();
                        m_lstPort = new List<int>();
                        m_lstIndex = new List<int>();
                        m_lstID = new List<int>();
                        //m_lstSMap = new List<SMap_t>();

                        m_thReceive = new Thread(new ThreadStart(Thread_Receive));
                        m_thReceive.Start();
                        //Ojw.CMessage.Write("Init Thread");
                    }
                    m_lstSerial.Add(COjwSerial);
                    m_lstPort.Add(nPort);
                    m_lstIndex.Add(0);
                    m_lstID.Add(0);
                    //SMap_t SMap = new SMap_t();
                    //m_lstSMap.Add(SMap);

                    //Write_Ping(2, m_lstSerial.Count - 1, 254);

                    return true;
                }
                return false;
            }
            // 지정한 포트를 닫는다.
            public void Close(int nPort)
            {
                if (m_lstSerial != null)
                {
                    if (m_lstSerial.Count > 0)
                    {
                        int nIndex = GetSerialIndex(nPort);
                        if (nIndex >= 0)
                        {
                            m_lstSerial[nIndex].DisConnect();
                            m_lstSerial.RemoveAt(nIndex);
                            m_lstPort.RemoveAt(nIndex);
                            m_lstIndex.RemoveAt(nIndex);
                            m_lstID.RemoveAt(nIndex);
                            //m_lstSMap.RemoveAt(nIndex);
                        }
                    }
                    if (m_lstPort.Count == 0)
                    {
                        m_lstSerial.Clear(); m_lstPort.Clear(); m_lstIndex.Clear(); m_lstID.Clear();//m_lstSMap.Clear();
                        m_lstSerial = null; m_lstPort = null; m_lstIndex = null; m_lstID = null;//m_lstSMap = null;
                    }
                }
            }
            // 전체 포트를 닫는다.
            public void Close()
            {
                if (m_lstSerial != null)
                {
                    foreach (CSerial COjwSerial in m_lstSerial)
                    {
                        if (COjwSerial.IsConnect() == true) COjwSerial.DisConnect();
                    }
                    m_lstSerial.Clear(); m_lstPort.Clear(); m_lstIndex.Clear(); m_lstID.Clear();//m_lstSMap.Clear();
                    m_lstSerial = null; m_lstPort = null; m_lstIndex = null; m_lstID = null;//m_lstSMap = null;
                }
            }
            #endregion Open / Close, IsOpen, GetSerial, GetSerialIndex, GetSerialPort, GetSerialCount

            public void Read_Motor(int nMotor, int nAddress, int nLength)
            {
                byte[] pbyLength = Ojw.CConvert.ShortToBytes((short)nLength);
                Write2(m_aSParam[nMotor].nProtocol, m_aSParam[nMotor].nCommIndex, m_aSParam[nMotor].nRealID, 0x02, nAddress, pbyLength);
                pbyLength = null;
            }

            private void Thread_Receive()
            {
                //byte byHead0 = 0;
                //byte byHead1 = 0;
                //byte byHead2 = 0;

                byte[] buf;
                //byte[] abyTmp = new byte[4];
                //Ojw.CMessage.Write("[Thread_Receive] Running Thread");
                int nSeq = 0;
                while ((m_lstSerial != null) && (m_bProgEnd == false))
                {
                    try
                    {
                        if (m_lstSerial == null) break;
                        if (m_lstSerial[nSeq].IsConnect() == false) continue;
                        int nSize = m_lstSerial[nSeq].GetBuffer_Length();
                        if (nSize > 0)
                        {
                            buf = m_lstSerial[nSeq].GetBytes();
                            //Ojw.CMessage.Write("[Receive]");
                            //Ojw.CConvert.ByteToStructure(

                            //if (m_nProtocolVersion == 1)
                            //{
                            //    //continue;
                            //    //Parsor1(buf, nSize);
                            //}
                            //else // (m_aSParam_Axis[nAxis].nProtocol_Version == 2) 
                            //{
                            //    //Parsor(buf, nSize);
                            //}
                            //Ojw.CMessage.Write("");



#if false // test
                            //foreach (byte byData in buf)
                            //{
                            //    Ojw.CMessage.Write2("0x{0}", Ojw.CConvert.IntToHex(byData, 2));                                
                            //}
                            //Ojw.CMessage.Write2("\r\n");
#else
                            int i = 0;
                            foreach (byte byData in buf)
                            {
#if false // for some packet errors
                                if (byData == 0xff)
                                {
                                    if ((m_lstIndex[nSeq] & 0xf0000) != 0)
                                    {
                                        if ((byData == 0xff) && ((m_lstIndex[nSeq] & 0xf0000) == 0x10000))
                                            m_lstIndex[nSeq] = (m_lstIndex[nSeq] & 0xfffff) | 0x20000;
                                        else if ((byData == 0xff) && ((m_lstIndex[nSeq] & 0xf0000) == 0x30000)) // 0xff, 0xff
                                        {
                                            m_lstIndex[nSeq] = 0x1000; // start new ( protocol 1 기준 ) -> 0xff, 0xff 가 들어올 확률은 아주 높다. 해서 이 부분은 일단 블록
                                        }
                                    }
                                    else
                                    {
                                        m_lstIndex[nSeq] = (m_lstIndex[nSeq] & 0xffff) | 0x10000;
                                    }
                                }
#endif
                                if (((m_lstIndex[nSeq] & 0x1000) == 0) && (byData == 0xff)) m_lstIndex[nSeq] = ((m_lstIndex[nSeq] & 0xf0000) | 0x1000);
                                else if (((m_lstIndex[nSeq] & 0x3000) == 0x1000) && (byData == 0xff))
                                {
                                    m_aSMap[m_lstID[nSeq]].bShow = false;

                                    // Protocol 1 Checked
                                    m_lstIndex[nSeq] |= 0x2000;
                                }
                                else if (((m_lstIndex[nSeq] & 0x7000) == 0x3000) && (byData == 0xfd))
                                {
                                    m_lstIndex[nSeq] |= 0x4000;
                                }
                                else if (((m_lstIndex[nSeq] & 0xf000) == 0x7000) && (byData == 0x00))
                                {
                                    // start - Protocol 2
                                    m_lstIndex[nSeq] |= 0x8000;
                                    // 모터는 Seq 가 아니라 새로운 인덱스가 필요하다. 전면 수정 필요. 읽을 때 아이디, 시리얼인덱스, 프로토콜버전의 3가지가 일치하는지 확인하고 꺼내오도록...
                                    //if (m_lstSMap[nSeq].nProtocol != 2) m_lstSMap[nSeq].nProtocol = 2;                                    
                                }
                                else
                                {
                                    // Protocol 2 - Address 122 ~ 136 => (120 ~ 139): 14=>20 바이트(moving(1), MoveStatus(1), PWM(4), Load(4), Velocity(4), Position(4)
                                    if ((m_lstIndex[nSeq] & 0xf000) == 0xf000)
                                    {
                                        // 단, XL-320 은 프로토콜2 여도 주소번지가 다르다. 따로 예외 처리할 것. 나중에...




                                        // [Ping] => Length == 7 이면 Ping
                                        // ID(1), Length(2)=(0x07, 0x00), cmd(1)=0x55, error(1), ModelNumber(2), Fw(1), CRC(2)
                                        // [Read]
                                        // ID(1), Length(2),  Cmd(1) = 0x55, error(1), Datas(N)..., CRC(2) 
                                        switch (m_lstIndex[nSeq] & 0x0fff)
                                        {
                                            case 0:
                                                m_lstID[nSeq] = byData; // ID Setting
                                                if (m_aSMap[m_lstID[nSeq]].nProtocol != 2) m_aSMap[m_lstID[nSeq]].nProtocol = 2;
                                                m_aSMap[m_lstID[nSeq]].nID = byData;
                                                m_lstIndex[nSeq]++;
                                                break;
                                            case 1:
                                                m_aSMap[m_lstID[nSeq]].nLength = byData; // Length == 7 -> Ping
                                                m_lstIndex[nSeq]++;
                                                break;
                                            case 2:
                                                m_aSMap[m_lstID[nSeq]].nLength |= (((int)byData << 8) & 0xff00); // Length == 7 -> Ping
                                                m_lstIndex[nSeq]++;
                                                break;
                                            case 3:
                                                m_aSMap[m_lstID[nSeq]].nCmd = byData;
                                                m_lstIndex[nSeq]++;
                                                break;
                                            case 4:
                                                m_aSMap[m_lstID[nSeq]].nError = byData;
                                                m_aSMap[m_lstID[nSeq]].nStep = 0;
                                                m_lstIndex[nSeq]++;
                                                break;
                                            case 5:
                                                if (m_aSMap[m_lstID[nSeq]].nLength == 7) // Ping
                                                {
                                                    if (m_aSMap[m_lstID[nSeq]].nStep == 0)
                                                    {
                                                        m_aSMap[m_lstID[nSeq]].bShow = true;
                                                        m_aSMap[m_lstID[nSeq]].nModelNumber = byData;
                                                        m_aSMap[m_lstID[nSeq]].abyMap[0] = byData;
                                                    }
                                                    else if (m_aSMap[m_lstID[nSeq]].nStep == 1)
                                                    {
                                                        m_aSMap[m_lstID[nSeq]].nModelNumber |= ((int)byData << 8) & 0xff00;
                                                        m_aSMap[m_lstID[nSeq]].abyMap[1] = byData;
                                                    }
                                                    else m_aSMap[m_lstID[nSeq]].nFw = byData;

                                                    //if (m_aSMap[m_lstID[nSeq]].nStep + 1 >= m_aSMap[m_lstID[nSeq]].nLength - 4)
                                                    //{
                                                    //    //Ojw.CMessage.Write("Model={0}", 
                                                    //    Ojw.CMessage.Write("[Serial[{0}]:[ID:{1}]Received[Protocol={2}]Type=0x{3}", nSeq, m_lstID[nSeq], m_aSMap[m_lstID[nSeq]].nProtocol, Ojw.CConvert.IntToHex(m_aSMap[m_lstID[nSeq]].abyMap[0] | ((m_aSMap[m_lstID[nSeq]].abyMap[1] << 8) & 0xff00), 4));
                                                    //    m_lstIndex[nSeq]++;
                                                    //}
                                                }
                                                else // 
                                                {
                                                    m_aSMap[m_lstID[nSeq]].abyMap[m_aSMap[m_lstID[nSeq]].nStep] = byData;

                                                    //if (m_aSMap[m_lstID[nSeq]].nStep + 1 >= m_aSMap[m_lstID[nSeq]].nLength - 4) m_lstIndex[nSeq]++;
                                                }
                                                if (m_aSMap[m_lstID[nSeq]].nStep + 1 >= m_aSMap[m_lstID[nSeq]].nLength - 4) m_lstIndex[nSeq]++;
                                                m_aSMap[m_lstID[nSeq]].nStep++;
                                                break;
                                            case 6:
                                                m_aSMap[m_lstID[nSeq]].nCrc0 = byData;
                                                m_lstIndex[nSeq]++;
                                                break;
                                            case 7:
                                                m_aSMap[m_lstID[nSeq]].nCrc1 = byData;
                                                m_aSMap[m_lstID[nSeq]].nSeq++;
                                                m_lstIndex[nSeq] = 0;
                                                //if (m_bShowReceivedIDs == true) Ojw.CMessage.Write("[Serial[{0}]:[ID:{1}]Received[Protocol={2}]Type={3}", nSeq, m_lstID[nSeq], m_aSMap[m_lstID[nSeq]].nProtocol, (m_aSMap[m_lstID[nSeq]].abyMap[0] | m_aSMap[m_lstID[nSeq]].abyMap[1] << 8));

                                                if (
                                                    (m_bShowReceivedIDs == true)
                                                    ||
                                                    (m_aSMap[m_lstID[nSeq]].bShow == true)
                                                    )
                                                    Ojw.CMessage.Write("[Serial[{0}]:[ID:{1}]Received[Protocol={2}]Type=0x{3}", nSeq, m_lstID[nSeq], m_aSMap[m_lstID[nSeq]].nProtocol, Ojw.CConvert.IntToHex(m_aSMap[m_lstID[nSeq]].abyMap[0] | ((m_aSMap[m_lstID[nSeq]].abyMap[1] << 8) & 0xff00), 4));
                                                if (m_bAutoset == true)
                                                {
                                                    if (m_CTmr_AutoSet.Get() > 500)
                                                        m_bAutoset = false;
                                                    else
                                                    {
                                                        m_CTmr_AutoSet.Set();
                                                        SMotors_t SMot = new SMotors_t();
                                                        SMot.nCommIndex = nSeq;
                                                        SMot.nRealID = m_lstID[nSeq];
                                                        SMot.nProtocol = m_aSMap[m_lstID[nSeq]].nProtocol;
                                                        if (m_bAutoset2 == false) m_lstSIds.Add(SMot);
                                                    }
                                                }
                                                if (m_bAutoset2 == true)
                                                {
                                                    EMotor_t EMot = GetMotorType(m_aSMap[m_lstID[nSeq]].abyMap[0] | ((m_aSMap[m_lstID[nSeq]].abyMap[1] << 8) & 0xff00));
                                                    if ((EMot != EMotor_t.NONE) && (EMot != EMotor_t.SG_90))
                                                    {
                                                        //SetParam_MotorType(m_lstID[nSeq], EMot);
                                                        SetParam(m_lstID[nSeq], m_lstID[nSeq], nSeq, 0, EMot);
                                                    }
                                                }
                                                break;

#if false
                public int nCmd;
                public int nError;
                public int nModelNumber;
                public EMotor_t EMotor;
#endif
                                        }
                                    }
                                    // Protocol 1 - Address 36 ~ 46 => (30~49) : 10=>20 바이트(Position(2), Speed(2), Load(2), Volt(1), Temp(1), Registered(2), Moving(1)
                                    else if ((m_lstIndex[nSeq] & 0x3000) == 0x3000)
                                    {
#if true
                                        // [Ping] => Length == 2 이면 Ping
                                        // ID(1), Length(1)=(0x02), error(1), Checksum(1)
                                        // [Read]
                                        // ID(1), Length(1),  Cmd(1) = 0x55, error(1), Datas(N)..., CRC(2) 
                                        //switch (m_lstIndex[nSeq] & 0x0fff)
                                        //{
                                        //    case 0: // ID
                                        //        if (m_aSMap_Old[byData].nProtocol != 1) m_aSMap_Old[byData].nProtocol = 1;
                                        //        m_aSMap_Old[byData].nID = byData;
                                        //        m_lstIndex[nSeq]++;
                                        //        break;

                                        //}                                        
                                        switch (m_lstIndex[nSeq] & 0x0fff)
                                        {
                                            case 0:
                                                m_lstID[nSeq] = byData; // ID Setting
                                                if (m_aSMap[m_lstID[nSeq]].nProtocol != 1) m_aSMap[m_lstID[nSeq]].nProtocol = 1;
                                                m_aSMap[m_lstID[nSeq]].nID = byData;
                                                m_lstIndex[nSeq]++;
                                                break;
                                            case 1:
                                                m_aSMap[m_lstID[nSeq]].nLength = byData; // Length == 2 -> Ping
                                                m_lstIndex[nSeq]++;
                                                if (byData == 2) m_lstIndex[nSeq]++; // Ping Data 는 Cmd 가 없다.
                                                break;
                                            //case 2:
                                            //    m_aSMap[m_lstID[nSeq]].nCmd = byData;
                                            //    m_lstIndex[nSeq]++;
                                            //    break;
                                            case 2:
                                                m_aSMap[m_lstID[nSeq]].nError = byData;
                                                m_aSMap[m_lstID[nSeq]].nStep = 0;
                                                m_lstIndex[nSeq]++;
                                                break;
                                            case 3:
                                                if (m_aSMap[m_lstID[nSeq]].nLength == 2) // Ping
                                                {
                                                    //if (m_aSMap[m_lstID[nSeq]].nStep == 0) m_aSMap[m_lstID[nSeq]].nModelNumber = byData;
                                                    //else if (m_aSMap[m_lstID[nSeq]].nStep == 1) m_aSMap[m_lstID[nSeq]].nModelNumber |= ((int)byData << 8) & 0xff00;
                                                    //else m_aSMap[m_lstID[nSeq]].nFw = byData;
                                                }
                                                else // 
                                                {
                                                    m_aSMap[m_lstID[nSeq]].abyMap[m_aSMap[m_lstID[nSeq]].nStep] = byData;
                                                }
                                                if (m_aSMap[m_lstID[nSeq]].nStep + 1 >= m_aSMap[m_lstID[nSeq]].nLength - 3) m_lstIndex[nSeq]++;
                                                m_aSMap[m_lstID[nSeq]].nStep++;
                                                break;
                                            //case 4:
                                            //    m_aSMap[m_lstID[nSeq]].nCrc0 = byData;
                                            //    m_lstIndex[nSeq]++;
                                            //    break;
                                            case 4:
                                                m_aSMap[m_lstID[nSeq]].nCrc0 = byData;
                                                m_aSMap[m_lstID[nSeq]].nCrc1 = byData;
                                                m_aSMap[m_lstID[nSeq]].nSeq++;
                                                m_lstIndex[nSeq] = 0;
                                                //if (m_bShowReceivedIDs == true) Ojw.CMessage.Write("[Serial[{0}]:[ID:{1}]Received[Protocol={2}]", nSeq, m_lstID[nSeq], m_aSMap[m_lstID[nSeq]].nProtocol);
                                                if (m_bShowReceivedIDs == true) Ojw.CMessage.Write("[Serial[{0}]:[ID:{1}]Received[Protocol={2}]Type=0x{3}", nSeq, m_lstID[nSeq], m_aSMap[m_lstID[nSeq]].nProtocol, Ojw.CConvert.IntToHex(m_aSMap[m_lstID[nSeq]].abyMap[0] | ((m_aSMap[m_lstID[nSeq]].abyMap[1] << 8) & 0xff00), 4));
                                                if (m_bAutoset == true)
                                                {
                                                    if (m_CTmr_AutoSet.Get() > 500)
                                                        m_bAutoset = false;
                                                    else
                                                    {
                                                        m_CTmr_AutoSet.Set();
                                                        SMotors_t SMot = new SMotors_t();
                                                        SMot.nCommIndex = nSeq;
                                                        SMot.nRealID = m_lstID[nSeq];
                                                        SMot.nProtocol = m_aSMap[m_lstID[nSeq]].nProtocol;
                                                        if (m_bAutoset2 == false) m_lstSIds.Add(SMot);
                                                    }
                                                }
                                                if (m_bAutoset2 == true)
                                                {
                                                    EMotor_t EMot = GetMotorType(m_aSMap[m_lstID[nSeq]].abyMap[0] | ((m_aSMap[m_lstID[nSeq]].abyMap[1] << 8) & 0xff00));
                                                    if ((EMot != EMotor_t.NONE) && (EMot != EMotor_t.SG_90))
                                                    {
                                                        //SetParam_MotorType(m_lstID[nSeq], EMot);

                                                        SetParam(m_lstID[nSeq], m_lstID[nSeq], nSeq, 0, EMot);
                                                    }
                                                }
                                                break;
                                        }
#endif
                                    }
                                }
                                i++;
                            }
#endif

                        }

                        if ((nSeq + 1) >= m_lstSerial.Count) nSeq = 0;
                        else nSeq++;

                        Thread.Sleep(1);
                    }
                    catch (Exception ex)
                    {
                        Ojw.CMessage.Write_Error(ex.ToString());
                        if (m_lstID != null)
                            if (nSeq < m_lstID.Count)
                                m_aSMap[m_lstID[nSeq]].nStep = 0;
                        if (m_lstIndex != null)
                            if (nSeq < m_lstIndex.Count) m_lstIndex[nSeq] = 0;
                        //break;
                    }
                }

                //Ojw.CMessage.Write("[Thread_Receive] Closed Thread");
            }

            #region Kinematics
            private List<string> m_lststrForward = new List<string>();
            private List<string> m_lststrInverse = new List<string>();
            private List<CDhParamAll> m_lstCDhParamAll = new List<CDhParamAll>();
            private List<Ojw.SOjwCode_t> m_lstSCode = new List<SOjwCode_t>();
            public void InitKinematics()
            {
                m_lststrForward.Clear();
                m_lststrInverse.Clear();
                m_lstCDhParamAll.Clear();
                m_lstSCode.Clear();
            }
            public void DestroyKinematics()
            {
                m_lststrForward.Clear();
                m_lststrInverse.Clear();
                m_lstCDhParamAll.Clear();
                m_lstSCode.Clear();
            }
            public int Kinematics_Forward_GetCount() { return m_lstCDhParamAll.Count; }
            public int Kinematics_Inverse_GetCount() { return m_lstSCode.Count; }
            public int Kinematics_Forward_Add(string strForward)
            {
                if (strForward != null)
                {
                    CDhParamAll CParamAll = new CDhParamAll();
                    if (strForward.Length > 0)
                    {
                        CKinematics.CForward.MakeDhParam(strForward, out CParamAll);

                        m_lststrForward.Add(strForward);
                        m_lstCDhParamAll.Add(CParamAll);
                    }
                }
                return m_lststrForward.Count;
            }
            public int Kinematics_Inverse_Add(bool bPython, string strInverse)
            {
                if (strInverse != null)
                {
                    if (strInverse.Length > 0)
                    {
                        Ojw.SOjwCode_t SCode = new SOjwCode_t();
                        CKinematics.CInverse.Compile(((bPython == true) ? "!" : string.Empty) + strInverse, out SCode);
                        if (SCode.nMotor_Max > 0)
                        {
                            m_lststrInverse.Add(strInverse);
                            m_lstSCode.Add(SCode);
                        }
                    }
                }
                return m_lststrInverse.Count;
            }
            public int Kinematics_Forward_Set(int nFunctionNumber, string strForward)
            {
                if (strForward != null)
                {
                    CDhParamAll CParamAll = new CDhParamAll();
                    if ((strForward.Length > 0) && (nFunctionNumber < m_lststrForward.Count) && (nFunctionNumber < m_lstCDhParamAll.Count))
                    {
                        CKinematics.CForward.MakeDhParam(strForward, out CParamAll);

                        m_lststrForward[nFunctionNumber] = strForward;
                        m_lstCDhParamAll[nFunctionNumber] = CParamAll;
                    }
                }
                return m_lststrForward.Count;
            }
            public int Kinematics_Inverse_Set(int nFunctionNumber, bool bPython, string strInverse)
            {
                if (strInverse != null)
                {
                    if ((strInverse.Length > 0) && (nFunctionNumber < m_lststrInverse.Count) && (nFunctionNumber < m_lstSCode.Count))
                    {
                        Ojw.SOjwCode_t SCode = new SOjwCode_t();
                        CKinematics.CInverse.Compile(((bPython == true) ? "!" : string.Empty) + strInverse, out SCode);
                        if (SCode.nMotor_Max > 0)
                        {
                            m_lststrInverse[nFunctionNumber] = strInverse;
                            m_lstSCode[nFunctionNumber] = SCode;
                        }
                    }
                }
                return m_lststrInverse.Count;
            }

            public bool Get_Xyz(int nFunctionNumber, out double dX, out double dY, out double dZ)
            {
                float[] afMot = new float[m_nMotorCountAll];
                Array.Clear(afMot, 0, afMot.Length);
                for (int i = 0; i < afMot.Length; i++)
                {
                    afMot[i] = m_aSMot[i].fAngle_Back;//(float)CalcEvd2Angle(i, m_aSMot[i].nValue);
                }
                return Get_Xyz(nFunctionNumber, afMot, out dX, out dY, out dZ);
                //return Get_Xyz(nFunctionNumber, m_aSMot, out dX, out dY, out dZ);
            }
            public bool Get_Xyz(int nFunctionNumber, float[] afMot, out double dX, out double dY, out double dZ)
            {
                int i;
                double[] dcolX;
                double[] dcolY;
                double[] dcolZ;

                double[] adMot = new double[afMot.Length];
                Array.Clear(adMot, 0, afMot.Length);
                adMot = Ojw.CConvert.FloatsToDoubles(afMot);
                //for (i = 0; i < afMot.Length; i++) adMot[i] = (double)afMot[i];
                return Ojw.CKinematics.CForward.CalcKinematics(m_lstCDhParamAll[nFunctionNumber], adMot, out dcolX, out dcolY, out dcolZ, out dX, out dY, out dZ);
            }
            //private Ojw.C3d m_C3d = new C3d();
            public void Set_Xyz(int nFunctionNumber, double dX, double dY, double dZ)//, int nTime_Milliseconds)
            {
                int[] anMotorID = new int[256];
                double[] adValue = new double[256];
                int nCnt = GetData_Inverse(nFunctionNumber, dX, dY, dZ, out anMotorID, out adValue);
                for (int i = 0; i < nCnt; i++)
                {
                    Set(anMotorID[i], (float)adValue[i]);
                    //SetData(anMotorID[i], (float)adValue[i]);
                    //CMotor.Set_Angle(anMotorID[i], (float)adValue[i], nTime_Milliseconds);
                }
                //CMotor.Send_Motor();
            }
            private int GetData_Inverse(int nFunctionNumber, double dX, double dY, double dZ, out int[] anMotorID, out double[] adValue)
            {
                // 집어넣기 전에 내부 메모리를 클리어 한다.
                SOjwCode_t SCode = m_lstSCode[nFunctionNumber];
                Ojw.CKinematics.CInverse.SetValue_ClearAll(ref SCode);
                Ojw.CKinematics.CInverse.SetValue_X(dX);
                Ojw.CKinematics.CInverse.SetValue_Y(dY);
                Ojw.CKinematics.CInverse.SetValue_Z(dZ);

                // 현재의 모터각을 전부 집어 넣도록 한다.
                for (int i = 0; i < m_nMotorCountAll; i++)
                {
                    // 모터값을 3D에 넣어주고
                    //SetData(i, Ojw.CConvert.StrToFloat(m_txtAngle[i].Text));
                    // 그 값을 꺼내 수식 계산에 넣어준다.
                    Ojw.CKinematics.CInverse.SetValue_Motor(i, m_aSMot[i].fAngle_Back);
                }

                // 실제 수식계산
                Ojw.CKinematics.CInverse.CalcCode(ref SCode);


                m_lstSCode[nFunctionNumber] = SCode;
                // 나온 결과값을 옮긴다.
                int nMotCnt = SCode.nMotor_Max;
                if (nMotCnt <= 0)
                {
                    anMotorID = null;
                    adValue = null;
                    return 0;
                }
                anMotorID = new int[nMotCnt];
                adValue = new double[nMotCnt];
                for (int i = 0; i < nMotCnt; i++)
                {
                    anMotorID[i] = SCode.pnMotor_Number[i];
                    adValue[i] = Ojw.CKinematics.CInverse.GetValue_Motor(anMotorID[i]);

                    //Set(SCode.pnMotor_Number[i], (float)Ojw.CKinematics.CInverse.GetValue_Motor(SCode.pnMotor_Number[i]));
                }
                return nMotCnt;
            }
            #endregion Kinematics

            private void SetAddress(int nMotor, EMotor_t EMot) { SetAddress(EMot, ref m_aSAddress[nMotor]); }
            private void SetAddress(EMotor_t EMot, ref SAddress_t SAddress)
            {
                switch (EMot)
                {
                    #region AX &...
                    case EMotor_t.MX_12:
                    case EMotor_t.DX_113:
                    case EMotor_t.DX_116: //
                    case EMotor_t.DX_117: //
                    case EMotor_t.RX_10: // 9~12v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시)
                    case EMotor_t.RX_24F: // 9~12v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시)
                    case EMotor_t.RX_28: // 12v~18.5v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시)
                    case EMotor_t.RX_64:
                    case EMotor_t.EX_106:
                    case EMotor_t.AX_12:
                    case EMotor_t.AX_18:
                        SAddress.nMotorNumber_0_2 = 0;        // R    0 : none
                        SAddress.nMotorNumber_Size_2 = 2;
                        SAddress.nFwVersion_6_1 = 6;          // R      
                        SAddress.nFwVersion_Size_1 = 1;

                        SAddress.nRealID_7_1 = 7;             // RW   0 ~ 252
                        SAddress.nRealID_Size_1 = 1;
                        SAddress.nBaudrate_8_1 = 8;           // RW   0-9600, 1-57600, 2-115200, 3-1M, 4-2M,  5-3M, 6-4M, 7-4.5M
                        SAddress.nBaudrate_Size_1 = 1;




                        SAddress.nTorq_64_1 = 24;              // RW   Off(0), On(1)
                        SAddress.nTorq_Size_1 = 1;
                        SAddress.nLed_65_1 = 25;               // RW   Off(0), On(1)
                        SAddress.nLed_Size_1 = 1;

                        SAddress.nMode_Operating_11_1 = 6;    // RW   1=속도제어(무한회전), 3(초기값)=위치제어(RPM), 4=Multi-Turn(-256~256:512회전), 5=전류기반 위치제어모드(-256~256:512회전), 16=PWM제어
                        SAddress.nMode_Operating_Size_1 = 4; // 속도제어시 0, 위치제어시 [6]00, [7]00, [8]ff, [9]03 ( 65283 ) 으로 셋팅해야 함. -> CW Limit 0, CCW Limit 1023

                        SAddress.nGoal_Vel_104_4 = 32;         // RW
                        SAddress.nGoal_Vel_Size_4 = 2;

                        SAddress.nProfile_Vel_112_4 = 32;      // RW
                        SAddress.nProfile_Vel_Size_4 = 2;

                        SAddress.nGoal_Pos_116_4 = 30;         // RW
                        SAddress.nGoal_Pos_Size_4 = 2;



                        // [DriveMode] 
                        //    0x01 : 정상회전(0), 역회전(1)
                        //    0x02 : 540 전용 Master(0), Slave(1)
                        //    0x04 : 속도기준 제어(0), 시간기준 제어(1)
                        //SAddress.nMode_Drive_10_1 = 10;        // RW 
                        //SAddress.nMode_Drive_Size_1 = 1;
                        //SAddress.nMode_Operating_11_1 = 11;    // RW   1=속도제어(무한회전), 3(초기값)=위치제어(RPM), 4=Multi-Turn(-256~256:512회전), 5=전류기반 위치제어모드(-256~256:512회전), 16=PWM제어
                        //SAddress.nMode_Operating_Size_1 = 1;
                        //SAddress.nProtocolVersion_13_1 = 13;   // RW   
                        //SAddress.nProtocolVersion_Size_1 = 1;
                        //SAddress.nOffset_20_4 = 20;            // RW   1회전 모드인 경우 -1024~1024 까지만 제어
                        //SAddress.nOffset_Size_4 = 4;
                        //SAddress.nLimit_PWM_36_2 = 36;         // RW   0~885 (885 = 100%)
                        //SAddress.nLimit_PWM_Size_2 = 2;
                        //SAddress.nLimit_Curr_38_2 = 38;        // RW   전류제어모드/전류기반위치제어모드에서 사용, 0~2047
                        //SAddress.nLimit_Curr_Size_2 = 2;
                        //// [Shutdown] - Reboot 으로만 해제 가능
                        ////    0x20 : 과부하
                        ////    0x10 : 전력이상
                        ////    0x08 : 엔코더 이상(Following Error)
                        ////    0x04 : 과열
                        ////    0x01 : 인가된 전압 이상
                        //SAddress.nShutdown_63_1 = 63;          // RW
                        //SAddress.nShutdown_Size_1 = 1;

                        //SAddress.nError_70_1 = 70;             // R    
                        //SAddress.nError_Size_1 = 1;
                        //SAddress.nGain_Vel_I_76_2 = 76;        // RW
                        //SAddress.nGain_Vel_I_Size_2 = 2;
                        //SAddress.nGain_Vel_P_78_2 = 78;        // RW
                        //SAddress.nGain_Vel_P_Size_2 = 2;
                        //SAddress.nGain_Pos_D_80_2 = 80;        // RW
                        //SAddress.nGain_Pos_D_Size_2 = 2;
                        //SAddress.nGain_Pos_I_82_2 = 82;        // RW
                        //SAddress.nGain_Pos_I_Size_2 = 2;
                        //SAddress.nGain_Pos_P_84_2 = 84;        // RW
                        //SAddress.nGain_Pos_P_Size_2 = 2;
                        //SAddress.nGain_Pos_F2_88_2 = 88;       // RW
                        //SAddress.nGain_Pos_F2_Size_2 = 2;
                        //SAddress.nGain_Pos_F1_90_2 = 90;       // RW
                        //SAddress.nGain_Pos_F1_Size_2 = 2;

                        //SAddress.nWatchDog_98_1 = 98;          // RW
                        //SAddress.nWatchDog_Size_1 = 1;

                        //SAddress.nGoal_PWM_100_2 = 100;         // RW   -PWMLimit ~ +PWMLimit
                        //SAddress.nGoal_PWM_Size_2 = 2;
                        //SAddress.nGoal_Current_102_2 = 102;     // RW   -CurrentLimit ~ +CurrentLimit
                        //SAddress.nGoal_Current_Size_2 = 2;

                        //SAddress.nProfile_Acc_108_4 = 108;      // RW
                        //SAddress.nProfile_Acc_Size_4 = 4;

                        //SAddress.nMoving_122_1 = 122;           // R    움직임 감지 못함(0), 움직임 감지(1)
                        //SAddress.nMoving_Size_1 = 1;
                        //// [Moving Status]
                        ////    0x30  - 0x30 : 사다리꼴 속도 프로파일
                        ////            0x20 : 삼각 속도 프로파일
                        ////            0x10 : 사각 속도 프로파일
                        ////            0x00 : 프로파일 미사용(Step)
                        ////    0x08 : Following Error
                        ////    0x02 : Goal Position 명령에 따라 진행 중
                        ////    0x01 : Inposition
                        //SAddress.nMoving_Status_123_1 = 123;    // R
                        //SAddress.nMoving_Status_Size_1 = 1;

                        //SAddress.nPresent_PWM_124_2 = 124;      // R
                        //SAddress.nPresent_PWM_Size_2 = 2;
                        //SAddress.nPresent_Curr_126_2 = 126;     // R
                        //SAddress.nPresent_Curr_Size_2 = 2;
                        //SAddress.nPresent_Vel_128_4 = 128;      // R
                        //SAddress.nPresent_Vel_Size_4 = 4;
                        //SAddress.nPresent_Pos_132_4 = 132;      // R
                        //SAddress.nPresent_Pos_Size_4 = 4;
                        //SAddress.nPresent_Volt_144_2 = 144;     // R
                        //SAddress.nPresent_Volt_Size_2 = 2;
                        //SAddress.nPresent_Temp_146_1 = 146;     // R
                        //SAddress.nPresent_Temp_Size_1 = 1;


                        /*

                        SetParam_ModelNum(nAxis, 12); // 0번지에 모델번호 12
                        SetParam_Addr_Max(nAxis, 52);
                        SetParam_Addr_Torq(nAxis, 24);
                        SetParam_Addr_Led(nAxis, 25);
                        SetParam_Addr_Mode(nAxis, 11); // 320 -> 11            [1 : 속도, 2(default) : 관절]
                        SetParam_Addr_Speed(nAxis, 32); // 320 -> 32 2 bytes
                        SetParam_Addr_Speed_Size(nAxis, 2);
                        SetParam_Addr_Pos_Speed(nAxis, 32); // 320 -> 32 2 bytes
                        SetParam_Addr_Pos_Speed_Size(nAxis, 2);
                        SetParam_Addr_Pos(nAxis, 30); // 320 -> 30 2 bytes
                        SetParam_Addr_Pos_Size(nAxis, 2);*/
                        break;
                    #endregion AX
                    #region XL_430 & MX Protocol 2
                    case EMotor_t.MX_28:
                    case EMotor_t.MX_64:
                    case EMotor_t.MX_106:
                    case EMotor_t.XM_540:
                    case EMotor_t.XL_430:
                        SAddress.nMotorNumber_0_2 = 0;        // R    0 : none
                        SAddress.nMotorNumber_Size_2 = 2;
                        SAddress.nFwVersion_6_1 = 6;          // R      
                        SAddress.nFwVersion_Size_1 = 1;

                        SAddress.nRealID_7_1 = 7;             // RW   0 ~ 252
                        SAddress.nRealID_Size_1 = 1;
                        SAddress.nBaudrate_8_1 = 8;           // RW   0-9600, 1-57600, 2-115200, 3-1M, 4-2M,  5-3M, 6-4M, 7-4.5M
                        SAddress.nBaudrate_Size_1 = 1;
                        // [DriveMode] 
                        //    0x01 : 정상회전(0), 역회전(1)
                        //    0x02 : 540 전용 Master(0), Slave(1)
                        //    0x04 : 속도기준 제어(0), 시간기준 제어(1)
                        SAddress.nMode_Drive_10_1 = 10;        // RW 
                        SAddress.nMode_Drive_Size_1 = 1;
                        SAddress.nMode_Operating_11_1 = 11;    // RW   1=속도제어(무한회전), 3(초기값)=위치제어(RPM), 4=Multi-Turn(-256~256:512회전), 5=전류기반 위치제어모드(-256~256:512회전), 16=PWM제어
                        SAddress.nMode_Operating_Size_1 = 1;
                        SAddress.nProtocolVersion_13_1 = 13;   // RW   
                        SAddress.nProtocolVersion_Size_1 = 1;
                        SAddress.nOffset_20_4 = 20;            // RW   1회전 모드인 경우 -1024~1024 까지만 제어
                        SAddress.nOffset_Size_4 = 4;
                        SAddress.nLimit_PWM_36_2 = 36;         // RW   0~885 (885 = 100%)
                        SAddress.nLimit_PWM_Size_2 = 2;
                        SAddress.nLimit_Curr_38_2 = 38;        // RW   전류제어모드/전류기반위치제어모드에서 사용, 0~2047
                        SAddress.nLimit_Curr_Size_2 = 2;
                        // [Shutdown] - Reboot 으로만 해제 가능
                        //    0x20 : 과부하
                        //    0x10 : 전력이상
                        //    0x08 : 엔코더 이상(Following Error)
                        //    0x04 : 과열
                        //    0x01 : 인가된 전압 이상
                        SAddress.nShutdown_63_1 = 63;          // RW
                        SAddress.nShutdown_Size_1 = 1;
                        SAddress.nTorq_64_1 = 64;              // RW   Off(0), On(1)
                        SAddress.nTorq_Size_1 = 1;
                        SAddress.nLed_65_1 = 65;               // RW   Off(0), On(1)
                        SAddress.nLed_Size_1 = 1;
                        SAddress.nError_70_1 = 70;             // R    
                        SAddress.nError_Size_1 = 1;
                        SAddress.nGain_Vel_I_76_2 = 76;        // RW
                        SAddress.nGain_Vel_I_Size_2 = 2;
                        SAddress.nGain_Vel_P_78_2 = 78;        // RW
                        SAddress.nGain_Vel_P_Size_2 = 2;
                        SAddress.nGain_Pos_D_80_2 = 80;        // RW
                        SAddress.nGain_Pos_D_Size_2 = 2;
                        SAddress.nGain_Pos_I_82_2 = 82;        // RW
                        SAddress.nGain_Pos_I_Size_2 = 2;
                        SAddress.nGain_Pos_P_84_2 = 84;        // RW
                        SAddress.nGain_Pos_P_Size_2 = 2;
                        SAddress.nGain_Pos_F2_88_2 = 88;       // RW
                        SAddress.nGain_Pos_F2_Size_2 = 2;
                        SAddress.nGain_Pos_F1_90_2 = 90;       // RW
                        SAddress.nGain_Pos_F1_Size_2 = 2;

                        SAddress.nWatchDog_98_1 = 98;          // RW
                        SAddress.nWatchDog_Size_1 = 1;

                        SAddress.nGoal_PWM_100_2 = 100;         // RW   -PWMLimit ~ +PWMLimit
                        SAddress.nGoal_PWM_Size_2 = 2;
                        SAddress.nGoal_Current_102_2 = 102;     // RW   -CurrentLimit ~ +CurrentLimit
                        SAddress.nGoal_Current_Size_2 = 2;
                        SAddress.nGoal_Vel_104_4 = 104;         // RW
                        SAddress.nGoal_Vel_Size_4 = 4;

                        SAddress.nProfile_Acc_108_4 = 108;      // RW
                        SAddress.nProfile_Acc_Size_4 = 4;
                        SAddress.nProfile_Vel_112_4 = 112;      // RW
                        SAddress.nProfile_Vel_Size_4 = 4;

                        SAddress.nGoal_Pos_116_4 = 116;         // RW
                        SAddress.nGoal_Pos_Size_4 = 4;

                        SAddress.nMoving_122_1 = 122;           // R    움직임 감지 못함(0), 움직임 감지(1)
                        SAddress.nMoving_Size_1 = 1;
                        // [Moving Status]
                        //    0x30  - 0x30 : 사다리꼴 속도 프로파일
                        //            0x20 : 삼각 속도 프로파일
                        //            0x10 : 사각 속도 프로파일
                        //            0x00 : 프로파일 미사용(Step)
                        //    0x08 : Following Error
                        //    0x02 : Goal Position 명령에 따라 진행 중
                        //    0x01 : Inposition
                        SAddress.nMoving_Status_123_1 = 123;    // R
                        SAddress.nMoving_Status_Size_1 = 1;

                        SAddress.nPresent_PWM_124_2 = 124;      // R
                        SAddress.nPresent_PWM_Size_2 = 2;
                        SAddress.nPresent_Curr_126_2 = 126;     // R
                        SAddress.nPresent_Curr_Size_2 = 2;
                        SAddress.nPresent_Vel_128_4 = 128;      // R
                        SAddress.nPresent_Vel_Size_4 = 4;
                        SAddress.nPresent_Pos_132_4 = 132;      // R
                        SAddress.nPresent_Pos_Size_4 = 4;
                        SAddress.nPresent_Volt_144_2 = 144;     // R
                        SAddress.nPresent_Volt_Size_2 = 2;
                        SAddress.nPresent_Temp_146_1 = 146;     // R
                        SAddress.nPresent_Temp_Size_1 = 1;
                        break;
                    #endregion XL_430
                    #region XL_320
                    case EMotor_t.XL_320:
                        SAddress.nTorq_64_1 = 24;              // RW   Off(0), On(1)                        
                        SAddress.nLed_65_1 = 25;               // RW   Off(0), On(1)

                        SAddress.nMode_Operating_11_1 = 6;    // RW   1=속도제어(무한회전), 3(초기값)=위치제어(RPM), 4=Multi-Turn(-256~256:512회전), 5=전류기반 위치제어모드(-256~256:512회전), 16=PWM제어
                        SAddress.nMode_Operating_Size_1 = 5; // 속도제어시 0, 위치제어시 [6]00, [7]00, [8]ff, [9]03 ( 65283 ) 으로 셋팅해야 함. -> CW Limit 0, CCW Limit 1023
                        SAddress.nProtocolVersion_13_1 = 13;   // RW  1 바퀴모드, 2 관절모드 (이 부분이 프로토콜 1의 다른 모터와 xl320 이 다른 부분)(아예 위 4바이트에 합쳐서 5바이트 만들어 제어)
                        SAddress.nProtocolVersion_Size_1 = 1;


                        SAddress.nGoal_Vel_104_4 = 32;         // RW
                        SAddress.nGoal_Vel_Size_4 = 2;

                        SAddress.nProfile_Vel_112_4 = 32;      // RW
                        SAddress.nProfile_Vel_Size_4 = 2;

                        SAddress.nGoal_Pos_116_4 = 30;         // RW
                        SAddress.nGoal_Pos_Size_4 = 2;
                        /*
                          SetParam_Addr_Max(nAxis, 52);
                          SetParam_Addr_Torq(nAxis, 24);
                          SetParam_Addr_Led(nAxis, 25);
                          SetParam_Addr_Mode(nAxis, 11); // 320 -> 11            [1 : 속도, 2(default) : 관절]
                        SetParam_Addr_Speed(nAxis, 32); // 320 -> 32 2 bytes
                        SetParam_Addr_Speed_Size(nAxis, 2);
                          SetParam_Addr_Pos_Speed(nAxis, 32); // 320 -> 32 2 bytes
                          SetParam_Addr_Pos_Speed_Size(nAxis, 2);
                          SetParam_Addr_Pos(nAxis, 30); // 320 -> 30 2 bytes
                          SetParam_Addr_Pos_Size(nAxis, 2);
                     */
                        SAddress.nMotorNumber_0_2 = 0;        // R    0 : none
                        SAddress.nMotorNumber_Size_2 = 2;
                        SAddress.nFwVersion_6_1 = 6;          // R      
                        SAddress.nFwVersion_Size_1 = 1;

                        SAddress.nRealID_7_1 = 7;             // RW   0 ~ 252
                        SAddress.nRealID_Size_1 = 1;
                        SAddress.nBaudrate_8_1 = 8;           // RW   0-9600, 1-57600, 2-115200, 3-1M, 4-2M,  5-3M, 6-4M, 7-4.5M
                        SAddress.nBaudrate_Size_1 = 1;
                        // [DriveMode] 
                        //    0x01 : 정상회전(0), 역회전(1)
                        //    0x02 : 540 전용 Master(0), Slave(1)
                        //    0x04 : 속도기준 제어(0), 시간기준 제어(1)
                        SAddress.nMode_Drive_10_1 = 10;        // RW 
                        SAddress.nMode_Drive_Size_1 = 1;
                        //SAddress.nMode_Operating_11_1 = 11;    // RW   1=속도제어(무한회전), 3(초기값)=위치제어(RPM), 4=Multi-Turn(-256~256:512회전), 5=전류기반 위치제어모드(-256~256:512회전), 16=PWM제어
                        //SAddress.nMode_Operating_Size_1 = 1;
                        //SAddress.nProtocolVersion_13_1 = 13;   // RW   
                        //SAddress.nProtocolVersion_Size_1 = 1;
                        SAddress.nOffset_20_4 = 20;            // RW   1회전 모드인 경우 -1024~1024 까지만 제어
                        SAddress.nOffset_Size_4 = 4;
                        SAddress.nLimit_PWM_36_2 = 36;         // RW   0~885 (885 = 100%)
                        SAddress.nLimit_PWM_Size_2 = 2;
                        SAddress.nLimit_Curr_38_2 = 38;        // RW   전류제어모드/전류기반위치제어모드에서 사용, 0~2047
                        SAddress.nLimit_Curr_Size_2 = 2;
                        // [Shutdown] - Reboot 으로만 해제 가능
                        //    0x20 : 과부하
                        //    0x10 : 전력이상
                        //    0x08 : 엔코더 이상(Following Error)
                        //    0x04 : 과열
                        //    0x01 : 인가된 전압 이상
                        SAddress.nShutdown_63_1 = 63;          // RW
                        SAddress.nShutdown_Size_1 = 1;
                        //SAddress.nTorq_64_1 = 64;              // RW   Off(0), On(1)
                        SAddress.nTorq_Size_1 = 1;
                        //SAddress.nLed_65_1 = 65;               // RW   Off(0), On(1)
                        SAddress.nLed_Size_1 = 1;
                        SAddress.nError_70_1 = 70;             // R    
                        SAddress.nError_Size_1 = 1;
                        SAddress.nGain_Vel_I_76_2 = 76;        // RW
                        SAddress.nGain_Vel_I_Size_2 = 2;
                        SAddress.nGain_Vel_P_78_2 = 78;        // RW
                        SAddress.nGain_Vel_P_Size_2 = 2;
                        SAddress.nGain_Pos_D_80_2 = 80;        // RW
                        SAddress.nGain_Pos_D_Size_2 = 2;
                        SAddress.nGain_Pos_I_82_2 = 82;        // RW
                        SAddress.nGain_Pos_I_Size_2 = 2;
                        SAddress.nGain_Pos_P_84_2 = 84;        // RW
                        SAddress.nGain_Pos_P_Size_2 = 2;
                        SAddress.nGain_Pos_F2_88_2 = 88;       // RW
                        SAddress.nGain_Pos_F2_Size_2 = 2;
                        SAddress.nGain_Pos_F1_90_2 = 90;       // RW
                        SAddress.nGain_Pos_F1_Size_2 = 2;

                        SAddress.nWatchDog_98_1 = 98;          // RW
                        SAddress.nWatchDog_Size_1 = 1;

                        SAddress.nGoal_PWM_100_2 = 100;         // RW   -PWMLimit ~ +PWMLimit
                        SAddress.nGoal_PWM_Size_2 = 2;
                        SAddress.nGoal_Current_102_2 = 102;     // RW   -CurrentLimit ~ +CurrentLimit
                        SAddress.nGoal_Current_Size_2 = 2;
                        //SAddress.nGoal_Vel_104_4 = 104;         // RW
                        //SAddress.nGoal_Vel_Size_4 = 4;

                        SAddress.nProfile_Acc_108_4 = 108;      // RW
                        SAddress.nProfile_Acc_Size_4 = 4;
                        //SAddress.nProfile_Vel_112_4 = 112;      // RW
                        //SAddress.nProfile_Vel_Size_4 = 4;

                        //SAddress.nGoal_Pos_116_4 = 116;         // RW
                        //SAddress.nGoal_Pos_Size_4 = 4;

                        SAddress.nMoving_122_1 = 122;           // R    움직임 감지 못함(0), 움직임 감지(1)
                        SAddress.nMoving_Size_1 = 1;
                        // [Moving Status]
                        //    0x30  - 0x30 : 사다리꼴 속도 프로파일
                        //            0x20 : 삼각 속도 프로파일
                        //            0x10 : 사각 속도 프로파일
                        //            0x00 : 프로파일 미사용(Step)
                        //    0x08 : Following Error
                        //    0x02 : Goal Position 명령에 따라 진행 중
                        //    0x01 : Inposition
                        SAddress.nMoving_Status_123_1 = 123;    // R
                        SAddress.nMoving_Status_Size_1 = 1;

                        SAddress.nPresent_PWM_124_2 = 124;      // R
                        SAddress.nPresent_PWM_Size_2 = 2;
                        SAddress.nPresent_Curr_126_2 = 126;     // R
                        SAddress.nPresent_Curr_Size_2 = 2;
                        SAddress.nPresent_Vel_128_4 = 128;      // R
                        SAddress.nPresent_Vel_Size_4 = 4;
                        SAddress.nPresent_Pos_132_4 = 132;      // R
                        SAddress.nPresent_Pos_Size_4 = 4;
                        SAddress.nPresent_Volt_144_2 = 144;     // R
                        SAddress.nPresent_Volt_Size_2 = 2;
                        SAddress.nPresent_Temp_146_1 = 146;     // R
                        SAddress.nPresent_Temp_Size_1 = 1;
                        break;
                    #endregion XL_320
                }
            }




            public int FindMotor(int nMotor_RealID) { int i = 0; foreach (SParam_t SParam in m_aSParam) { if (SParam.nRealID == nMotor_RealID) return i; i++; } return -1; }
            public int Get_RealID(int nMotor) { return m_aSParam[nMotor].nRealID; }

            #region Parameter Function(SetParam...)
            public void SetParam_Dir(int nMotor, int nDir) { m_aSParam[nMotor].nDir = nDir; }
            public void SetParam_RealID(int nMotor, int nMotorRealID) { m_aSParam[nMotor].nRealID = nMotorRealID; }
            //public void SetParam_OperationMode(int nMotor, EOperationMode_t EOperationMode) { m_aSParam[nMotor].EOperationMode = EOperationMode; }

            public void SetParam_CommIndex(int nMotor, int nCommIndex) { m_aSParam[nMotor].nCommIndex = nCommIndex; }               // 연결 이후에 둘 중 하나만 사용 한다.(되도록  CommIndex 를 사용할 것. Commport 로 설정 하려면 통신이 접속이 되어 있어야 한다.)
            public void SetParam_CommPort(int nMotor, int nCommPort) { m_aSParam[nMotor].nCommIndex = GetSerialIndex(nCommPort); }  // CommIndex 설정보다 직관적이나 잘못 설정 될 수 있다. 연결이 안 된 경우 CommIndes 가 잘못 지정될 수가 있다.

            public void SetParam_LimitUp(int nMotor, float fLimitUp) { m_aSParam[nMotor].fLimitUp = fLimitUp; }                       // Limit - 0: Ignore 
            public void SetParam_LimitDown(int nMotor, float fLimitDn) { m_aSParam[nMotor].fLimitDn = fLimitDn; }                       // Limit - 0: Ignore 
            public void SetParam_LimitRpm(int nMotor, float fLimitRpm) { m_aSParam[nMotor].fLimitRpm = fLimitRpm; }                       // Limit - 0: Ignore 
            public void SetParam_MotorType(int nMotor, EMotor_t EMot)
            {
                SetAddress(EMot, ref m_aSAddress[nMotor]);

                m_aSParam[nMotor].EMot = EMot;

                switch (EMot)
                {
                    // 
                    case EMotor_t.SG_90:
                        break;
                    // Protocol1
                    case EMotor_t.DX_113:
                    case EMotor_t.DX_116: //
                    case EMotor_t.DX_117: //
                    case EMotor_t.RX_10: // 9~12v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시)
                    case EMotor_t.RX_24F: // 9~12v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시)
                    case EMotor_t.RX_28: // 12v~18.5v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시)
                    case EMotor_t.RX_64:
                    case EMotor_t.AX_12:
                    case EMotor_t.AX_18:
                        //m_aSParam[nMotor].bEn = true;                       // 활성화

                        //m_aSParam[nMotor].nID = nMotor;                     // 모터를 제어하기 위한 가상 아이디. 중복 허용 불가. 보통은 모터 ID 와 일치시킨다.      
                        //m_aSParam[nMotor].nRealID = nMotorRealID;           // 실제 사용할 모터의 아이디
                        //m_aSParam[nMotor].nCommIndex = nCommIndex;          // 연결 된 통신포트 중 어느것에 연결 된 것이지에 대한 순서.

                        //m_aSParam[nMotor].nDir = nDir;
                        //m_aSParam[nMotor].EMot = EMot;
                        m_aSParam[nMotor].fCenterPos = 512.0f;

                        m_aSParam[nMotor].fMechMove = 1024.0f;
                        m_aSParam[nMotor].fDegree = 300.0f;
                        m_aSParam[nMotor].fRefRpm = 0.111f;                 // 기본 rpm 단위

                        m_aSParam[nMotor].fLimitRpm = 1023f;                // 

                        m_aSParam[nMotor].fLimitUp = 0.0f;    // Limit - 0: Ignore
                        m_aSParam[nMotor].fLimitDn = 0.0f;    // Limit - 0: Ignore

                        m_aSParam[nMotor].nProtocol = 1;               // 2, 0 - protocol2, 1 - protocol1, 3 - None     

                        break;
                    case EMotor_t.EX_106:
                        //m_aSParam[nMotor].EMot = EMot;
                        m_aSParam[nMotor].fCenterPos = 2048.0f;

                        m_aSParam[nMotor].fMechMove = 4095.0f;
                        m_aSParam[nMotor].fDegree = 251.0f;
                        m_aSParam[nMotor].fRefRpm = 0.111f;                 // 기본 rpm 단위

                        m_aSParam[nMotor].fLimitRpm = 1023f;                // 

                        m_aSParam[nMotor].fLimitUp = 0.0f;    // Limit - 0: Ignore
                        m_aSParam[nMotor].fLimitDn = 0.0f;    // Limit - 0: Ignore

                        m_aSParam[nMotor].nProtocol = 1;               // 2, 0 - protocol2, 1 - protocol1, 3 - None     
                        break;
                    #region MX
                    case EMotor_t.MX_12:
                        m_aSParam[nMotor].fCenterPos = 1024.0f;

                        m_aSParam[nMotor].fMechMove = 2048.0f;
                        m_aSParam[nMotor].fDegree = 360.0f;
                        m_aSParam[nMotor].fRefRpm = 0.916f;            // 기본 rpm 단위

                        m_aSParam[nMotor].fLimitRpm = 415f;                // 

                        m_aSParam[nMotor].fLimitUp = 0.0f;    // Limit - 0: Ignore
                        m_aSParam[nMotor].fLimitDn = 0.0f;    // Limit - 0: Ignore

                        m_aSParam[nMotor].nProtocol = 1;               // 2, 0 - protocol2, 1 - protocol1, 3 - None              
                        break;
                    case EMotor_t.MX_28:
                    case EMotor_t.MX_64:
                    case EMotor_t.MX_106:
                        m_aSParam[nMotor].fCenterPos = 2048.0f;

                        m_aSParam[nMotor].fMechMove = 4096.0f;
                        m_aSParam[nMotor].fDegree = 360.0f;
                        m_aSParam[nMotor].fRefRpm = 0.229f; ;//(EMot == EMotor_t.MX_12) ? 0.916f : 0.114f;  //0.229f;                 // 기본 rpm 단위

                        m_aSParam[nMotor].fLimitRpm = 415f;                // 

                        m_aSParam[nMotor].fLimitUp = 0.0f;    // Limit - 0: Ignore
                        m_aSParam[nMotor].fLimitDn = 0.0f;    // Limit - 0: Ignore

                        m_aSParam[nMotor].nProtocol = 2;               // 2, 0 - protocol2, 1 - protocol1, 3 - None              
                        break;
                    #endregion MX
                    // Protocol2
                    case EMotor_t.NONE:
                    case EMotor_t.XM_540:
                    case EMotor_t.XL_430:
                        //m_aSParam[nMotor].bEn = true;                       // 활성화
                        //m_aSParam[nMotor].nModelNum = 1060;
                        //m_aSParam[nMotor].nID = nMotor;                     // 모터를 제어하기 위한 가상 아이디. 중복 허용 불가. 보통은 모터 ID 와 일치시킨다.      
                        //m_aSParam[nMotor].nRealID = nMotorRealID;           // 실제 사용할 모터의 아이디
                        //m_aSParam[nMotor].nCommIndex = nCommIndex;          // 연결 된 통신포트 중 어느것에 연결 된 것이지에 대한 순서.

                        //m_aSParam[nMotor].nDir = nDir;
                        m_aSParam[nMotor].fCenterPos = 2048.0f;

                        m_aSParam[nMotor].fMechMove = 4096.0f;
                        m_aSParam[nMotor].fDegree = 360.0f;
                        m_aSParam[nMotor].fRefRpm = 0.229f;                 // 기본 rpm 단위

                        m_aSParam[nMotor].fLimitRpm = 415f;                // 

                        m_aSParam[nMotor].fLimitUp = 0.0f;    // Limit - 0: Ignore
                        m_aSParam[nMotor].fLimitDn = 0.0f;    // Limit - 0: Ignore

                        m_aSParam[nMotor].nProtocol = 2;               // 2, 0 - protocol2, 1 - protocol1, 3 - None                             
                        break;


                    case EMotor_t.XL_320:
                        //m_aSParam[nMotor].EMot = EMot;
                        m_aSParam[nMotor].fCenterPos = 512.0f;

                        m_aSParam[nMotor].fMechMove = 1024.0f;
                        m_aSParam[nMotor].fDegree = 300.0f;
                        m_aSParam[nMotor].fRefRpm = 0.111f;                 // 기본 rpm 단위

                        m_aSParam[nMotor].fLimitRpm = 1023f;                // 

                        m_aSParam[nMotor].fLimitUp = 0.0f;    // Limit - 0: Ignore
                        m_aSParam[nMotor].fLimitDn = 0.0f;    // Limit - 0: Ignore

                        m_aSParam[nMotor].nProtocol = 2;               // 2, 0 - protocol2, 1 - protocol1, 3 - None     
                        break;
                    //case EMotor_t.XM_540:
                    //    break;
                }
            }
            public void SetParam(int nMotor, int nMotorRealID, int nCommIndex, int nDir, EMotor_t EMot)
            {
                //m_anMot_RealID[nMotor] = nMotorRealID; // ID 변경
                //m_anMot_SerialIndex[nMotor] = nCommIndex; // 통신 포트 변경
                m_aSParam[nMotor].nRealID = nMotorRealID; // ID 변경
                m_aSParam[nMotor].nCommIndex = nCommIndex; // 통신 포트 변경
                m_aSParam[nMotor].nDir = nDir;
                //m_aSParam[nMotor].EOperationMode = EOperationMode_t._Position; // Default
                //if (m_aSParam[nMotor].EOperationMode_Prev == EOperationMode_t._None) m_aSParam[nMotor].EOperationMode_Prev = m_aSParam[nMotor].EOperationMode;
                SetAddress(EMot, ref m_aSAddress[nMotor]);

#if true
                SetParam_MotorType(nMotor, EMot);
#else
                switch (EMot)
                {
                    // 
                    case EMotor_t.SG_90:
                        break;
                    // Protocol1
                    case EMotor_t.AX_12:
                    case EMotor_t.AX_18:
                        //m_aSParam[nMotor].bEn = true;                       // 활성화

                        //m_aSParam[nMotor].nID = nMotor;                     // 모터를 제어하기 위한 가상 아이디. 중복 허용 불가. 보통은 모터 ID 와 일치시킨다.      
                        //m_aSParam[nMotor].nRealID = nMotorRealID;           // 실제 사용할 모터의 아이디
                        //m_aSParam[nMotor].nCommIndex = nCommIndex;          // 연결 된 통신포트 중 어느것에 연결 된 것이지에 대한 순서.

                        m_aSParam[nMotor].nDir = nDir;
                        m_aSParam[nMotor].fCenterPos = 512.0f;

                        m_aSParam[nMotor].fMechMove = 1024.0f;
                        m_aSParam[nMotor].fDegree = 300.0f;
                        m_aSParam[nMotor].fRefRpm = 0.111f;                 // 기본 rpm 단위

                        m_aSParam[nMotor].fLimitRpm = 1023f;                // 

                        m_aSParam[nMotor].fLimitUp = 0.0f;    // Limit - 0: Ignore
                        m_aSParam[nMotor].fLimitDn = 0.0f;    // Limit - 0: Ignore

                        m_aSParam[nMotor].nProtocol = 1;               // 2, 0 - protocol2, 1 - protocol1, 3 - None     

                        break;


                    // Protocol2
                    case EMotor_t.NONE:
                    case EMotor_t.XL_430:
                        //m_aSParam[nMotor].bEn = true;                       // 활성화
                        //m_aSParam[nMotor].nModelNum = 1060;
                        //m_aSParam[nMotor].nID = nMotor;                     // 모터를 제어하기 위한 가상 아이디. 중복 허용 불가. 보통은 모터 ID 와 일치시킨다.      
                        //m_aSParam[nMotor].nRealID = nMotorRealID;           // 실제 사용할 모터의 아이디
                        //m_aSParam[nMotor].nCommIndex = nCommIndex;          // 연결 된 통신포트 중 어느것에 연결 된 것이지에 대한 순서.

                        m_aSParam[nMotor].nDir = nDir;
                        m_aSParam[nMotor].fCenterPos = 2048.0f;

                        m_aSParam[nMotor].fMechMove = 4096.0f;
                        m_aSParam[nMotor].fDegree = 360.0f;
                        m_aSParam[nMotor].fRefRpm = 0.229f;                 // 기본 rpm 단위

                        m_aSParam[nMotor].fLimitRpm = 415f;                // 

                        m_aSParam[nMotor].fLimitUp = 0.0f;    // Limit - 0: Ignore
                        m_aSParam[nMotor].fLimitDn = 0.0f;    // Limit - 0: Ignore

                        m_aSParam[nMotor].nProtocol = 2;               // 2, 0 - protocol2, 1 - protocol1, 3 - None                             
                        break;


                    case EMotor_t.XL_320:
                        break;
                    case EMotor_t.XM_540:
                        break;
                }
#if false
                #region Check
                //if (m_lstSCheckMotorType.Count > 0)
                {
                    bool bOk = false;
                    foreach (SCheckMotorType_t SType in m_lstSCheckMotorType)
                    {
                        if ((SType.EMot == EMot) && (SType.nCommIndex == nCommIndex) && (SType.nProtocol == m_aSMot_Info[nMotor].nProtocol) && (SType.nTorqAddress == m_aSMot_Info[nMotor].SAddress.nTorq_64_1))
                        {
                            bOk = true;
                            break;
                        }
                    }

                    if (bOk == false)
                    {
                        SCheckMotorType_t SType = new SCheckMotorType_t();
                        SType.nCommIndex = nCommIndex;
                        SType.EMot = EMot;
                        SType.nProtocol = m_aSMot_Info[nMotor].nProtocol;
                        SType.nTorqAddress = m_aSMot_Info[nMotor].SAddress.nTorq_64_1;
                        m_lstSCheckMotorType.Add(SType);
                    }
                }
                #endregion Check
                //m_lstSMot_Info.Add(SInfo);
                return m_aSMot_Info[nMotor];
#endif
#endif
            }
            #endregion Parameter Function(SetParam...)

            #region Protocol - basic(updateCRC, MakeStuff, SendPacket)
            private int updateCRC(byte[] data_blk_ptr, int data_blk_size)
            {
                int i, j = 0;
                int[] anCrcTable = new int[256] { 
                    0x0000, 0x8005, 0x800F, 0x000A, 0x801B, 0x001E, 0x0014, 0x8011, 0x8033, 0x0036, 0x003C, 0x8039, 0x0028, 0x802D, 0x8027, 0x0022, 0x8063, 0x0066, 0x006C, 0x8069, 0x0078, 0x807D, 0x8077, 0x0072, 0x0050, 0x8055, 0x805F, 0x005A, 0x804B, 0x004E, 0x0044, 0x8041, 0x80C3, 0x00C6, 0x00CC, 0x80C9, 0x00D8, 0x80DD, 0x80D7, 0x00D2, 
                    0x00F0, 0x80F5, 0x80FF, 0x00FA, 0x80EB, 0x00EE, 0x00E4, 0x80E1, 0x00A0, 0x80A5, 0x80AF, 0x00AA, 0x80BB, 0x00BE, 0x00B4, 0x80B1, 0x8093, 0x0096, 0x009C, 0x8099, 0x0088, 0x808D, 0x8087, 0x0082, 0x8183, 0x0186, 0x018C, 0x8189, 0x0198, 0x819D, 0x8197, 0x0192, 0x01B0, 0x81B5, 0x81BF, 0x01BA, 0x81AB, 0x01AE, 0x01A4, 0x81A1, 
                    0x01E0, 0x81E5, 0x81EF, 0x01EA, 0x81FB, 0x01FE, 0x01F4, 0x81F1, 0x81D3, 0x01D6, 0x01DC, 0x81D9, 0x01C8, 0x81CD, 0x81C7, 0x01C2, 0x0140, 0x8145, 0x814F, 0x014A, 0x815B, 0x015E, 0x0154, 0x8151, 0x8173, 0x0176, 0x017C, 0x8179, 0x0168, 0x816D, 0x8167, 0x0162, 0x8123, 0x0126, 0x012C, 0x8129, 0x0138, 0x813D, 0x8137, 0x0132,
                    0x0110, 0x8115, 0x811F, 0x011A, 0x810B, 0x010E, 0x0104, 0x8101, 0x8303, 0x0306, 0x030C, 0x8309, 0x0318, 0x831D, 0x8317, 0x0312, 0x0330, 0x8335, 0x833F, 0x033A, 0x832B, 0x032E, 0x0324, 0x8321, 0x0360, 0x8365, 0x836F, 0x036A, 0x837B, 0x037E, 0x0374, 0x8371, 0x8353, 0x0356, 0x035C, 0x8359, 0x0348, 0x834D, 0x8347, 0x0342, 
                    0x03C0, 0x83C5, 0x83CF, 0x03CA, 0x83DB, 0x03DE, 0x03D4, 0x83D1, 0x83F3, 0x03F6, 0x03FC, 0x83F9, 0x03E8, 0x83ED, 0x83E7, 0x03E2, 0x83A3, 0x03A6, 0x03AC, 0x83A9, 0x03B8, 0x83BD, 0x83B7, 0x03B2, 0x0390, 0x8395, 0x839F, 0x039A, 0x838B, 0x038E, 0x0384, 0x8381, 0x0280, 0x8285, 0x828F, 0x028A, 0x829B, 0x029E, 0x0294, 0x8291, 
                    0x82B3, 0x02B6, 0x02BC, 0x82B9, 0x02A8, 0x82AD, 0x82A7, 0x02A2, 0x82E3, 0x02E6, 0x02EC, 0x82E9, 0x02F8, 0x82FD, 0x82F7, 0x02F2, 0x02D0, 0x82D5, 0x82DF, 0x02DA, 0x82CB, 0x02CE, 0x02C4, 0x82C1, 0x8243, 0x0246, 0x024C, 0x8249, 0x0258, 0x825D, 0x8257, 0x0252, 0x0270, 0x8275, 0x827F, 0x027A, 0x826B, 0x026E, 0x0264, 0x8261, 
                    0x0220, 0x8225, 0x822F, 0x022A, 0x823B, 0x023E, 0x0234, 0x8231, 0x8213, 0x0216, 0x021C, 0x8219, 0x0208, 0x820D, 0x8207, 0x0202 };
                int nCrc_accum = 0;
                for (j = 0; j < data_blk_size; j++) nCrc_accum = (nCrc_accum << 8) ^ anCrcTable[(((nCrc_accum >> 8) ^ data_blk_ptr[j]) & 0xFF)];
                return nCrc_accum;
            }
            private void MakeStuff(ref byte[] pBuff)
            {
                int nStuff = 0;
                int[] pnIndex = new int[pBuff.Length];
                Array.Clear(pnIndex, 0, pnIndex.Length);
                int nCnt = 0;
                for (int i = 5; i < pBuff.Length; i++)
                {
                    switch (nStuff)
                    {
                        case 0: { if (pBuff[i] == 0xff) nStuff++; } break;
                        case 1: { if (pBuff[i] == 0xff) nStuff++; else nStuff = 0; } break;
                        case 2:
                            {
                                if (pBuff[i] == 0xfd)
                                {
                                    nStuff++;
                                    pnIndex[nCnt++] = i;
                                }
                                else
                                {
                                    nStuff = 0;
                                }
                            }
                            break;
                    }
                }
                if (nCnt > 0)
                {
                    byte[] pBuff2 = new byte[pBuff.Length];
                    Array.Copy(pBuff, pBuff2, pBuff.Length);
                    Array.Resize<byte>(ref pBuff, pBuff2.Length + nCnt);
                    int nIndex = 0;
                    int nPos = 0;
                    foreach (byte byTmp in pBuff)
                    {
                        pBuff[nIndex + nPos] = pBuff2[nIndex];
                        if (nIndex == pnIndex[nPos])
                        {
                            pBuff[nIndex + nPos + 1] = 0xfd;
                            nPos++;
                        }
                        nIndex++;
                    }
                    pBuff2 = null;
                }
                pnIndex = null;
            }
            private void SendPacket(int nPortIndex, byte[] buffer, int nLength) { if (m_lstSerial[nPortIndex].IsConnect() == true) m_lstSerial[nPortIndex].SendPacket(buffer, nLength); }
            #endregion Protocol - basic(updateCRC, MakeStuff, SendPacket)

            #region Packet_Raw

            public string MakePingPacket_String(int nMotorRealID, int nProtocol_Version)
            {
                byte[] pbyteBuffer = MakePingPacket(nMotorRealID, nProtocol_Version);

                string strPing = String.Empty;// "=>";
                foreach (byte byData in pbyteBuffer) strPing += String.Format(",0x{0}", Ojw.CConvert.IntToHex(byData, 2));
                return strPing;
            }
            private byte[] MakePingPacket(int nMotorRealID, int nProtocol_Version)
            {
                int i = 0;
                byte[] pbyteBuffer;
                if (nProtocol_Version == 1)
                {
                    int nLength = 2; // 파라미터의 갯수 + 2
                    int nDefaultSize = 4;
                    pbyteBuffer = new byte[nDefaultSize + nLength];
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = (byte)(nMotorRealID & 0xff);
                    pbyteBuffer[i++] = (byte)(nLength & 0xff);
                    pbyteBuffer[i++] = (byte)(0x01);

                    int nCrc = 0;
                    for (int j = 2; j < pbyteBuffer.Length - 1; j++) nCrc += pbyteBuffer[j];
                    pbyteBuffer[i++] = (byte)(~nCrc & 0xff);

                    //pbyteBuffer[pbyteBuffer.Length - 1] = (byte)(nCrc & 0xff);

                }
                else // m_aSParam_Axis[nAxis].nProtocol_Version == 2
                {
                    int nLength = 3;
                    int nDefaultSize = 7;
                    pbyteBuffer = new byte[nDefaultSize + nLength];
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = 0xff;
                    #region Packet 2.0
                    pbyteBuffer[i++] = 0xfd;
                    pbyteBuffer[i++] = 0x00;
                    #endregion Packet 2.0
                    pbyteBuffer[i++] = (byte)(nMotorRealID & 0xff);
                    pbyteBuffer[i++] = (byte)(nLength & 0xff);
                    pbyteBuffer[i++] = (byte)((nLength >> 8) & 0xff);
                    pbyteBuffer[i++] = (byte)(0x01);

                    MakeStuff(ref pbyteBuffer);
                    int nCrc = updateCRC(pbyteBuffer, pbyteBuffer.Length - 2);
                    pbyteBuffer[pbyteBuffer.Length - 2] = (byte)(nCrc & 0xff);
                    pbyteBuffer[pbyteBuffer.Length - 1] = (byte)((nCrc >> 8) & 0xff);

                    //SendPacket(nSerialIndex, pbyteBuffer, pbyteBuffer.Length);
                }
                return pbyteBuffer;
            }

            public void Write_Ping(int nProtocol_Version, int nSerialIndex, int nMotor)
            {
                int nID = m_aSParam[nMotor].nRealID;
                byte[] pbyteBuffer = MakePingPacket(nID, nProtocol_Version);
                SendPacket(nSerialIndex, pbyteBuffer, pbyteBuffer.Length);
            }
            public void Write_Ping() { Write_Ping(true); }
            public void Write_Ping(bool bShow)
            {
                m_bShowReceivedIDs = bShow;
                for (int nIndex = 0; nIndex < m_lstPort.Count; nIndex++)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        int nID = 254;
                        byte[] pbyteBuffer = MakePingPacket(nID, 2 - i);
                        m_CTmr_AutoSet.Set();
                        SendPacket(nIndex, pbyteBuffer, pbyteBuffer.Length);
                        Ojw.CTimer.Wait(100); // 나중에 handshake 로 바꿀것.
                    }
                }
#if false
                if (m_lstSCheckMotorType.Count > 0)
                {
                    foreach (SCheckMotorType_t SType in m_lstSCheckMotorType)
                    {
                        //int nProtocol = 2;
                        //int nCommIndex = 0;
                        int nID = 254;
                        //int nTorqAddress = 64;

                        //SetTorq(-1, bOn); 
                        byte[] pbyteBuffer = MakePingPacket(nID, SType.nProtocol);
                        SendPacket(SType.nCommIndex, pbyteBuffer, pbyteBuffer.Length);
                    }
                }
#endif
            }
            public void Write_Command(int nMotor, int nCommand) { Write_Command(m_aSParam[nMotor].nProtocol, m_aSParam[nMotor].nCommIndex, nMotor, nCommand); }
            public void Write_Command(int nProtocol_Version, int nSerialIndex, int nMotor, int nCommand)
            {
                int i;
                //int nID = ((nAxis == 254) ? 254 : m_aSMot[nAxis].nID);
                if (m_aSParam[nMotor].nProtocol == 1)
                {
                    int nLength = 1 + 2;
                    int nDefaultSize = 6;
                    byte[] pbyteBuffer = new byte[nDefaultSize + nLength];
                    i = 0;
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = (byte)(m_aSParam[nMotor].nRealID & 0xff);
                    pbyteBuffer[i++] = (byte)(nLength & 0xff);
                    pbyteBuffer[i++] = (byte)(nCommand & 0xff);

                    int nCrc = 0;
                    for (int j = 2; i < pbyteBuffer.Length - 1; j++) nCrc += pbyteBuffer[j];
                    pbyteBuffer[i++] = (byte)(~nCrc & 0xff);

                    pbyteBuffer[pbyteBuffer.Length - 1] = (byte)(nCrc & 0xff);

                    SendPacket(nSerialIndex, pbyteBuffer, pbyteBuffer.Length);
                }
                else // if (m_aSParam_Axis[nAxis].nProtocol_Version == 2)
                {
                    int nLength = 3;
                    int nDefaultSize = 7;
                    byte[] pbyteBuffer = new byte[nDefaultSize + nLength];
                    i = 0;
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = 0xfd;
                    pbyteBuffer[i++] = 0x00;
                    pbyteBuffer[i++] = (byte)(m_aSParam[nMotor].nRealID & 0xff);
                    pbyteBuffer[i++] = (byte)(nLength & 0xff);
                    pbyteBuffer[i++] = (byte)((nLength >> 8) & 0xff);
                    pbyteBuffer[i++] = (byte)(nCommand & 0xff);

                    MakeStuff(ref pbyteBuffer);
                    int nCrc = updateCRC(pbyteBuffer, pbyteBuffer.Length - 2);
                    pbyteBuffer[pbyteBuffer.Length - 2] = (byte)(nCrc & 0xff);
                    pbyteBuffer[pbyteBuffer.Length - 1] = (byte)((nCrc >> 8) & 0xff);

                    SendPacket(nSerialIndex, pbyteBuffer, pbyteBuffer.Length);
                }
            }
            public void Write(int nMotor, int nCommand, int nAddress, params byte[] pbyDatas) { Write2(m_aSParam[nMotor].nProtocol, m_aSParam[nMotor].nCommIndex, nMotor, nCommand, nAddress, pbyDatas); }
            public void Write2(int nProtocol_Version, int nSerialIndex, int nMotorRealID, int nCommand, int nAddress, params byte[] pbyDatas)
            {
                int i;

                //int nID = 0;//((nAxis == 254) ? 254 : m_aSMot[nAxis].nID);

                i = 0;
                if (nProtocol_Version == 1)
                {
                    int nLength = 2 + ((pbyDatas != null) ? pbyDatas.Length + 1 : 0); // 파라미터의 갯수 + 2
                    int nDefaultSize = 4;
                    byte[] pbyteBuffer = new byte[nDefaultSize + nLength];
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = (byte)(nMotorRealID & 0xff);
                    pbyteBuffer[i++] = (byte)(nLength & 0xff);
                    pbyteBuffer[i++] = (byte)(nCommand & 0xff);
                    if (pbyDatas != null)
                    {
                        pbyteBuffer[i++] = (byte)(nAddress & 0xff);
                        foreach (byte byData in pbyDatas) pbyteBuffer[i++] = byData;
                    }
                    int nCrc = 0;
                    for (int j = 2; j < pbyteBuffer.Length - 1; j++) nCrc += pbyteBuffer[j];
                    pbyteBuffer[i++] = (byte)(~nCrc & 0xff);

                    SendPacket(nSerialIndex, pbyteBuffer, pbyteBuffer.Length);
                }
                else // m_aSParam_Axis[nAxis].nProtocol_Version == 2
                {
                    int nLength = 3 + 2 + pbyDatas.Length;
                    int nDefaultSize = 7;
                    byte[] pbyteBuffer = new byte[nDefaultSize + nLength];
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = 0xff;
                    #region Packet 2.0
                    pbyteBuffer[i++] = 0xfd;
                    pbyteBuffer[i++] = 0x00;
                    #endregion Packet 2.0
                    pbyteBuffer[i++] = (byte)(nMotorRealID & 0xff);
                    pbyteBuffer[i++] = (byte)(nLength & 0xff);
                    pbyteBuffer[i++] = (byte)((nLength >> 8) & 0xff);
                    pbyteBuffer[i++] = (byte)(nCommand & 0xff);
                    pbyteBuffer[i++] = (byte)(nAddress & 0xff);
                    pbyteBuffer[i++] = (byte)((nAddress >> 8) & 0xff);
                    if (pbyDatas != null)
                        foreach (byte byData in pbyDatas) pbyteBuffer[i++] = byData;

                    MakeStuff(ref pbyteBuffer);
                    int nCrc = updateCRC(pbyteBuffer, pbyteBuffer.Length - 2);
                    pbyteBuffer[pbyteBuffer.Length - 2] = (byte)(nCrc & 0xff);
                    pbyteBuffer[pbyteBuffer.Length - 1] = (byte)((nCrc >> 8) & 0xff);

                    SendPacket(nSerialIndex, pbyteBuffer, pbyteBuffer.Length);
                }
            }
            public void Writes(int nProtocol_Version, int nSerialIndex, int nAddress, int nDataLength_without_ID, int nMotorCnt, params byte[] pbyDatas)
            {
                int i = 0;
                int nLength = (nDataLength_without_ID + 1) * nMotorCnt;
                if (nProtocol_Version != 1) // xl_320 은 따로 해주도록 한다. 나중에...
                {
                    byte[] pbyteBuffer = new byte[2 + nLength];
                    pbyteBuffer[i++] = (byte)(nDataLength_without_ID & 0xff);
                    pbyteBuffer[i++] = (byte)((nDataLength_without_ID >> 8) & 0xff);
                    Array.Copy(pbyDatas, 0, pbyteBuffer, i, nLength);
                    Write2(nProtocol_Version, nSerialIndex, 0xfe, 0x83, nAddress, pbyteBuffer);
                    pbyteBuffer = null;
                }
                else // (== 1)
                {
                    // Instruction(1) + Address(2) + Data_Length(2) + 
                    byte[] pbyteBuffer = new byte[1 + nLength];
                    pbyteBuffer[i++] = (byte)(nDataLength_without_ID & 0xff);
                    Array.Copy(pbyDatas, 0, pbyteBuffer, i, nLength);
                    Write2(nProtocol_Version, nSerialIndex, 0xfe, 0x83, nAddress, pbyteBuffer);
                    pbyteBuffer = null;
                }
            }
#if true
#else
            public void Writes(int nProtocol_Version, int nSerialIndex, int nAddress, int nDataLength_without_ID, int nMotorCnt, params byte[] pbyDatas)
            {
                int i = 0;
                int nLength = (nDataLength_without_ID + 1) * nMotorCnt;
                if (nProtocol_Version != 1)
                {
                    byte[] pbyteBuffer = new byte[2 + nLength];
                    pbyteBuffer[i++] = (byte)(nDataLength_without_ID & 0xff);
                    pbyteBuffer[i++] = (byte)((nDataLength_without_ID >> 8) & 0xff);
                    Array.Copy(pbyDatas, 0, pbyteBuffer, i, nLength);
                    Write2(nProtocol_Version, nSerialIndex, 0xfe, 0x83, nAddress, pbyteBuffer);
                    pbyteBuffer = null;
                }
                else // (== 1)
                {
                    // Instruction(1) + Address(2) + Data_Length(2) + 
                    byte[] pbyteBuffer = new byte[1 + nLength];
                    pbyteBuffer[i++] = (byte)(nDataLength_without_ID & 0xff);
                    Array.Copy(pbyDatas, 0, pbyteBuffer, i, nLength);
                    Write2(nProtocol_Version, nSerialIndex, 0xfe, 0x83, nAddress, pbyteBuffer);
                    pbyteBuffer = null;
                }
            }
            public void Writes(int nProtocol_Version, int nSerialIndex, int nAddress, int nDataLength_without_ID, int nMotorCnt, params byte[] pbyDatas)
            {
                int i = 0;
                int nLength = (nDataLength_without_ID + 1) * nMotorCnt;
                if (nProtocol_Version != 1)
                {
                    byte[] pbyteBuffer = new byte[2 + nLength];
                    pbyteBuffer[i++] = (byte)(nDataLength_without_ID & 0xff);
                    pbyteBuffer[i++] = (byte)((nDataLength_without_ID >> 8) & 0xff);
                    Array.Copy(pbyDatas, 0, pbyteBuffer, i, nLength);
                    Write(0xfe, 0x83, nAddress, pbyteBuffer);
                    pbyteBuffer = null;
                }
                else
                {
                    byte[] pbyteBuffer = new byte[1 + nLength];
                    pbyteBuffer[i++] = (byte)(nDataLength_without_ID & 0xff);
                    Array.Copy(pbyDatas, 0, pbyteBuffer, i, nLength);
                    Write(0xfe, 0x83, nAddress, pbyteBuffer);
                    pbyteBuffer = null;
                }
            }
            // int nMotor, int nSpeed
            public void Writes_Speed(params int [] anMotor_and_Speed)
            {
                int nCount = anMotor_and_Speed.Length / 2;
                byte[] pbyTmp_Short;
                for (int i = 0; i < nCount; i++)
                {
                    // 속도

                    pbyTmp_Short = Ojw.CConvert.ShortToBytes((short)anMotor_and_Speed[i * 2 + 1]);
                Array.Copy(pbyTmp_Short, 0, pbyteBuffer_Spd, nIndex_Spd, pbyTmp_Short.Length);
            }
#endif
            public void Reads(int nSerialIndex, int nAddress, int nDataLength, params byte[] pbyIDs)
            {
                int nProtocol_Version = 2; // 프로토콜 2 버전부터 싱크리드가 지원
                byte[] pbyteBuffer = new byte[2 + pbyIDs.Length];
                Array.Copy(Ojw.CConvert.ShortToBytes((short)nDataLength), pbyteBuffer, 2);
                Array.Copy(pbyIDs, 0, pbyteBuffer, 2, pbyIDs.Length);
                Write2(nProtocol_Version, nSerialIndex, 0xfe, 0x82, nAddress, pbyteBuffer);
            }
            public void Reads(int nSerialIndex, int nAddress, int nDataLength, params int[] pnIDs)
            {
                int nProtocol_Version = 2; // 프로토콜 2 버전부터 싱크리드가 지원
                byte[] pbyteBuffer = new byte[2 + pnIDs.Length];
                int i = 2;
                Array.Copy(Ojw.CConvert.ShortToBytes((short)nDataLength), pbyteBuffer, i);
                foreach (int nData in pnIDs) pbyteBuffer[i++] = (byte)(nData & 0xff);
                Write2(nProtocol_Version, nSerialIndex, 0xfe, 0x82, nAddress, pbyteBuffer);
            }
            #endregion Packet_Raw

            #region Control
            public void SetTorq(int nMotor, bool bOn)
            {
                if (bOn) { if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return; }
                if (bOn == false) m_aSMot[nMotor].bInit_Value = false;

                m_aSMot[nMotor].nStatus_Torq_Prev = m_aSMot[nMotor].nStatus_Torq;
                m_aSMot[nMotor].nStatus_Torq = Ojw.CConvert.BoolToInt(bOn);
                //if (nMotor < 0) 

                if (m_aSParam[nMotor].EMot != EMotor_t.SG_90)
                {
                    Write2(m_aSParam[nMotor].nProtocol, m_aSParam[nMotor].nCommIndex, ((nMotor < 0) ? 254 : (m_aSParam[nMotor].nRealID)), 0x03, m_aSAddress[nMotor].nTorq_64_1, (byte)((bOn == true) ? 1 : 0));

                }
            }
            public void SetTorq(bool bOn)
            {
                if (bOn) { if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return; }
                for (int i = 0; i < m_aSParam.Length; i++) m_aSMot[i].bInit_Value = false;
                for (int i = 0; i < m_lstPort.Count; i++)
                {
                    // protocol 1(AX)
                    Write2(1, i, 254, 0x03, _ADDRESS_TORQ_AX, (byte)((bOn == true) ? 1 : 0));
                    // protocol 2(XL-430
                    Write2(2, i, 254, 0x03, _ADDRESS_TORQ_XL_430, (byte)((bOn == true) ? 1 : 0));
                }
            }
            #region 같은 프로토콜, 같은 시리얼포트, 같은 종류의 모터로만 구성해야 한다.
            public void SetTorq(bool bOn, params int[] anMotors)
            {
                if (bOn) { if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return; }
                byte[] pbyteBuffer = new byte[anMotors.Length * 2];
                int i = 0;
                byte byVal = (byte)((bOn == true) ? 1 : 0);
                foreach (int nMotor in anMotors)
                {
                    pbyteBuffer[i++] = (byte)(Get_RealID(nMotor) & 0xff);
                    pbyteBuffer[i++] = byVal;
                }
                if (anMotors.Length > 0)
                {
                    int nProtocol = m_aSParam[anMotors[0]].nProtocol;
                    int nSerialIndex = m_aSParam[anMotors[0]].nCommIndex;
                    Writes(nProtocol, nSerialIndex, m_aSAddress[anMotors[0]].nTorq_64_1, m_aSAddress[anMotors[0]].nTorq_Size_1, anMotors.Length, pbyteBuffer);
                }
            }
            //public void SetTorq(bool bOn, params int[] nMotor)
            //{
            //    for (int nCommand = 0; nCommand < 3; nCommand++)
            //    {
            //        for (int nSerialIndex = 0; nSerialIndex < m_lstPort.Count; nSerialIndex++)
            //        {
            //            for (int nProtocol = 1; nProtocol <= 2; nProtocol++)
            //            {
            //                Writes(nProtocol, nSerialIndex, nAddress, nAddress_Size, nMotorPtCount, m_abyBuffer);
            //            }
            //        }
            //    }
            //}
            public void SetTorqOn(params int[] anMotors) { SetTorq(true, anMotors); }
            public void SetTorqOff(params int[] anMotors) { SetTorq(false, anMotors); }
            #endregion 같은 프로토콜, 같은 시리얼포트, 같은 종류의 모터로만 구성해야 한다.

            //  Move  ///////////////////////////
            private bool m_bIgnoredLimit = false;
            public void SetLimitEn(bool bOn) { m_bIgnoredLimit = !bOn; }
            public bool GetLimitEn() { return !m_bIgnoredLimit; }
            public int Clip(int nLimitValue_Up, int nLimitValue_Dn, int nData)
            {
                if (GetLimitEn() == false) return nData;

                int nRet = ((nData > nLimitValue_Up) ? nLimitValue_Up : nData);
                return ((nRet < nLimitValue_Dn) ? nLimitValue_Dn : nRet);
            }
            public float Clip(float fLimitValue_Up, float fLimitValue_Dn, float fData)
            {
                if (GetLimitEn() == false) return fData;
                float fRet = ((fData > fLimitValue_Up) ? fLimitValue_Up : fData);
                return ((fRet < fLimitValue_Dn) ? fLimitValue_Dn : fRet);
            }

            public int CalcLimit_Evd(int nMotor, int nValue)
            {
                //if (Get_Flag_Mode(nAxis) == 0)// || (Get_Flag_Mode(nAxis) == 2))
                //{
                int nPulse = nValue;// &0x4000;
                //if (m_bMultiTurn == false)
                //{
                //nValue &= 0x4000;
                //nValue &= 0x3fff;
                //}

                //nValue &= 0x3fff;
                int nUp = 10000000;
                int nDn = -nUp;
                if (m_aSParam[nMotor].fLimitUp != 0) nUp = CalcAngle2Evd(nMotor, m_aSParam[nMotor].fLimitUp);
                if (m_aSParam[nMotor].fLimitDn != 0) nDn = CalcAngle2Evd(nMotor, m_aSParam[nMotor].fLimitDn);
                if (nUp < nDn) { int nTmp = nUp; nUp = nDn; nDn = nTmp; }
                return (Clip(nUp, nDn, nValue) | nPulse);
                //}
                //return nValue;
            }
            public float CalcLimit_Angle(int nMotor, float fValue)
            {
                //if (Get_Flag_Mode(nAxis) == 0)// || (Get_Flag_Mode(nAxis) == 2))
                //{
                float fUp = 1000000.0f;
                float fDn = -fUp;
                if (m_aSParam[nMotor].fLimitUp != 0) fUp = m_aSParam[nMotor].fLimitUp;
                if (m_aSParam[nMotor].fLimitDn != 0) fDn = m_aSParam[nMotor].fLimitDn;
                return Clip(fUp, fDn, fValue);
                //}
                //return fValue;
            }
            public int CalcAngle2Evd(int nMotor, float fValue)
            {
                fValue *= ((m_aSParam[nMotor].nDir == 0) ? 1.0f : -1.0f);
                int nData = 0;
                //if (Get_Flag_Mode(nAxis) != 0)   // 속도제어
                //{
                //    nData = (int)Math.Round(fValue);
                //    //Ojw.CMessage.Write("Speed Turn");
                //}
                //else
                //{
                // 위치제어
                nData = (int)Math.Round((m_aSParam[nMotor].fMechMove * fValue) / m_aSParam[nMotor].fDegree);
                nData = nData + (int)Math.Round(m_aSParam[nMotor].fCenterPos);
                //}
                return nData;
            }
            public float CalcEvd2Angle(int nMotor, int nValue)
            {
                float fValue = ((m_aSParam[nMotor].nDir == 0) ? 1.0f : -1.0f);
                float fValue2 = 0.0f;
                //if (Get_Flag_Mode(nMotor) != 0)   // 속도제어
                //    fValue2 = (float)nValue * fValue;
                //else                                // 위치제어
                //{
                fValue2 = (float)(((m_aSParam[nMotor].fDegree * ((float)(nValue - (int)Math.Round(m_aSParam[nMotor].fCenterPos)))) / m_aSParam[nMotor].fMechMove) * fValue);
                //}
                return fValue2;
            }
            public void Set_OperationMode(int nMotor, EOperationMode_t EOper)
            {
                m_aSMot[nMotor].EOperationMode = EOper;
                m_aSMot[nMotor].bOperationMode = true;
            }
            //public void Set_Evd(int nMotor, int nEvd, float fRpm)
            //{
            //}
            //public void Set_Evd(int nMotor, int nEvd)
            //{
            //    if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return;
            //    Push(nMotor);
            //    //Read_Motor_Push(nAxis);
            //    m_aSMot[nMotor].EEnable = EEnable_t._Position;
            //    //m_aSMot[nAxis].
            //    //Set_Flag_Mode(nAxis, false);
            //    if (m_aSMot[nMotor].EOperationMode_Prev != m_aSMot[nMotor].EOperationMode) m_aSMot[nMotor].bOperationMode = true;

            //    m_aSMot[nMotor].nValue = (long)CalcLimit_Evd(nMotor, nEvd);
            //    //Set_Flag_NoAction(nAxis, false);
            //    //Push_Id(nAxis);	
            //}
            //public long Get_Evd(int nMotor) { return m_aSMot[nMotor].nValue; }


            // [목표]
            //////////////////////////////////// 전송 단계가 아닌 Set의 단계에서 프로토콜을 적재하도록 수정하자.
            //////////////////////////////////// SyncWrite 가 아닌 BulkWrite 를 사용하자.(아직...)

            public void Set(int nMotor, float fAngle, float fRpm)
            {
                if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return;
                Push(nMotor);
                // Protocol1, XL-320 을 바퀴냐 포지션이냐만 결정하자.

                m_aSMot[nMotor].EEnable = EEnable_t._Position;
                if (m_aSMot[nMotor].EOperationMode_Prev != m_aSMot[nMotor].EOperationMode) m_aSMot[nMotor].bOperationMode = true;
                m_aSMot[nMotor].nValue = CalcLimit_Evd(nMotor, CalcAngle2Evd(nMotor, fAngle));
                m_aSMot[nMotor].bSetValue2 = true; // Rpm 을 따로 설정한 경우만 Set 한다.
                m_aSMot[nMotor].nValue2 = CalcRpm2Raw(nMotor, fRpm);
                m_aSMot[nMotor].fAngle_Back = CalcEvd2Angle(nMotor, m_aSMot[nMotor].nValue);
            }
            public void Set(int nMotor, float fAngle)
            {
                if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return;
                Push(nMotor);
                m_aSMot[nMotor].EEnable = EEnable_t._Position;
                Set_OperationMode(nMotor, EOperationMode_t._Position);
                if (m_aSMot[nMotor].EOperationMode_Prev != m_aSMot[nMotor].EOperationMode) m_aSMot[nMotor].bOperationMode = true;

                m_aSMot[nMotor].nValue = CalcLimit_Evd(nMotor, CalcAngle2Evd(nMotor, fAngle));
                m_aSMot[nMotor].fAngle_Back = CalcEvd2Angle(nMotor, m_aSMot[nMotor].nValue);
            }
            public float Get(int nMotor) { return m_aSMot[nMotor].fAngle_Back; }
            //public float Get(int nMotor) { return CalcEvd2Angle(nMotor, (int)m_aSMot[nMotor].nValue); }

            //private List<byte> m_lstPt = new List<byte>();
            private byte[] m_abyBuffer = new byte[256];
            private byte[] m_abyBuffer_Speed = new byte[256];
            private int m_nBuffer = 0;
            private int m_nBuffer_Speed = 0;
            private bool IsBitMinus(Ojw.CMonster.EMotor_t EMotor)
            {
                bool bRet = false;
                if (EMotor == EMotor_t.AX_12)
                {
                }
                return bRet;
            }
            public void Set_Turn(int nMotor, float fRpm)
            {
                if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return;
                Push(nMotor);
                m_aSMot[nMotor].EEnable = EEnable_t._Speed;
                Set_OperationMode(nMotor, EOperationMode_t._Speed);
                if (m_aSMot[nMotor].EOperationMode_Prev != m_aSMot[nMotor].EOperationMode) m_aSMot[nMotor].bOperationMode = true;
                m_aSMot[nMotor].bSetValue2 = true; // Rpm 을 따로 설정한 경우만 Set 한다.
                m_aSMot[nMotor].nValue2 = CalcRpm2Raw(nMotor, fRpm);
            }
            // float Get(int nMotor) { return CalcEvd2Angle(nMotor, (int)m_aSMot[nMotor].nValue); }
            private List<int> m_lstMotors = new List<int>();
            public void Send_Motor() { Send_Motor(0); }
            public void Send_Motor(int nMilliseconds)
            {
                if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return;
#if false
                if (m_nProtocolVersion == 1)
                    {
                        switch (nMode)
                        {
                            case 0: Writes(nAddr_Spd, nAddr_Size, nCnt2, pbyteBuffer_Spd); Writes(nAddr_Pos, nAddr_Size, nCnt2, pbyteBuffer_Pos); break;
                            case 1: Writes(nAddr_Spd, nAddr_Size, nCnt2, pbyteBuffer_Spd); break;
                            //case 2: Writes(102, 4, nCnt, pbyteBuffer); break;
                            default: break;
                        }
                    }
                    else
                    {
                        
                        switch (nMode)
                        {
                            case 0: Writes(nAddr_Pos_Spd, nAddr_Size, nCnt2, pbyteBuffer_Spd); Writes(nAddr_Pos, nAddr_Size, nCnt2, pbyteBuffer_Pos); break;
                            case 1: Writes(nAddr_Spd, nAddr_Size, nCnt2, pbyteBuffer_Spd); break;
                            //case 2: Writes(102, 4, nCnt, pbyteBuffer); break;
                            default: break;
                        }
                    }
#endif
                //int nAddr = 0;
                int nMotorPtCount = 0;
                int nMotorPtCount_Speed = 0;
                int nMotor = 0;
                int nAddress;
                int nAddress_Size;
                int nAddress_Speed = -1;
                int nAddress_Speed_Size = -1;
                int nMotorCnt = m_nMotorCnt;
                byte[] buffer = new byte[4];
                // Setting - Operation
                for (int nCommand = 0; nCommand < 3; nCommand++)
                {
                    for (int nSerialIndex = 0; nSerialIndex < m_lstPort.Count; nSerialIndex++)
                    {
                        for (int nProtocol = 1; nProtocol <= 2; nProtocol++)
                        {
                            m_lstMotors.Clear();
                            m_nBuffer = 0;
                            nMotorPtCount = 0;
                            nAddress = -1;
                            nAddress_Size = 0;

                            m_nBuffer_Speed = 0;
                            nMotorPtCount_Speed = 0;
                            nAddress_Speed = -1;
                            nAddress_Speed_Size = -1;
                            for (int i = 0; i < nMotorCnt; i++)
                            {
                                //if ((nCommand == 2) && (nProtocol == 2)) nMotor = Pop(); // 별 의미 없긴 하지만... 일단 Pop() 으로 비워주어야 하니...
                                //else nMotor = m_anEn[i];
                                nMotor = m_anEn[i];
                                if ((m_aSParam[nMotor].nProtocol == nProtocol) && (m_aSParam[nMotor].nCommIndex == nSerialIndex))
                                {
                                    //if (m_aSParam[nMotor].EMot == EMotor_t.XL_320) // XL_320 은 아직 논외로 친다.

                                    switch (nCommand)
                                    {
                                        case 0:
                                            if (m_aSMot[nMotor].bOperationMode == true)
                                            {
                                                if (nAddress < 0)
                                                {
                                                    nAddress = m_aSAddress[nMotor].nMode_Operating_11_1;
                                                    nAddress_Size = m_aSAddress[nMotor].nMode_Operating_Size_1;
                                                }

                                                if ((m_aSParam[nMotor].EMot == EMotor_t.AX_12) || (m_aSParam[nMotor].EMot == EMotor_t.AX_18) || (m_aSParam[nMotor].EMot == EMotor_t.XL_320))
                                                {
                                                    m_abyBuffer[m_nBuffer++] = (byte)Get_RealID(nMotor);
                                                    if ((m_aSMot[nMotor].EOperationMode == EOperationMode_t._Position) || (m_aSMot[nMotor].EOperationMode == EOperationMode_t._Position_Multi) || (m_aSMot[nMotor].EOperationMode == EOperationMode_t._Position_Amp))
                                                    {
                                                        m_abyBuffer[m_nBuffer++] = 0x00;
                                                        m_abyBuffer[m_nBuffer++] = 0x00;
                                                        m_abyBuffer[m_nBuffer++] = 0xff;
                                                        m_abyBuffer[m_nBuffer++] = 0x03;
                                                        if (m_aSParam[nMotor].EMot == EMotor_t.XL_320) m_abyBuffer[m_nBuffer++] = 0x02;
                                                    }
                                                    else
                                                    {
                                                        m_abyBuffer[m_nBuffer++] = 0x00;
                                                        m_abyBuffer[m_nBuffer++] = 0x00;
                                                        m_abyBuffer[m_nBuffer++] = 0x00;
                                                        m_abyBuffer[m_nBuffer++] = 0x00;
                                                        if (m_aSParam[nMotor].EMot == EMotor_t.XL_320) m_abyBuffer[m_nBuffer++] = 0x01;
                                                    }
                                                }
                                                else
                                                {
                                                    m_abyBuffer[m_nBuffer++] = (byte)Get_RealID(nMotor);
                                                    m_abyBuffer[m_nBuffer++] = (byte)m_aSMot[nMotor].EOperationMode;
                                                }


                                                m_lstMotors.Add(nMotor);

                                                nMotorPtCount++;
                                            }
                                            break;
                                        case 1:
                                            bool bSet = false;
                                            //bool bSpeed = false;
                                            if (nMilliseconds > 0)
                                            {
                                                if (
                                                     (m_aSMot[nMotor].EOperationMode == EOperationMode_t._Position) ||
                                                     (m_aSMot[nMotor].EOperationMode == EOperationMode_t._Position_Multi) ||
                                                     (m_aSMot[nMotor].EOperationMode == EOperationMode_t._Position_Amp)
                                                    )
                                                {
                                                    float fRpm = (float)Math.Abs(CalcTime2Rpm(CalcEvd2Angle(nMotor, m_aSMot[nMotor].nValue) - CalcEvd2Angle(nMotor, m_aSMot[nMotor].nValue_Prev), (float)nMilliseconds));
                                                    if (fRpm > 1)//!= 0)//> 1)
                                                    {
                                                        bSet = true;
                                                        m_aSMot[nMotor].nValue2 = CalcRpm2Raw(nMotor, fRpm); // short
                                                    }
                                                }
                                            }
                                            if ((m_aSMot[nMotor].bSetValue2 == true) || (bSet == true))
                                            {
                                                if (m_aSMot[nMotor].EEnable == EEnable_t._Position)
                                                {
                                                    if (nAddress < 0)
                                                    {
                                                        nAddress = m_aSAddress[nMotor].nProfile_Vel_112_4;
                                                        nAddress_Size = m_aSAddress[nMotor].nProfile_Vel_Size_4;
                                                    }
                                                    m_abyBuffer[m_nBuffer++] = (byte)Get_RealID(nMotor);

                                                    if (nAddress_Size == 1)
                                                        m_abyBuffer[m_nBuffer++] = (byte)(m_aSMot[nMotor].nValue2 & 0xff);
                                                    else if (nAddress_Size == 2)
                                                    {
                                                        buffer = Ojw.CConvert.ShortToBytes((short)m_aSMot[nMotor].nValue2);
                                                        Array.Copy(buffer, 0, m_abyBuffer, m_nBuffer, nAddress_Size);
                                                        m_nBuffer += nAddress_Size;
                                                    }
                                                    else //(nAddress_Size == 4)
                                                    {
                                                        buffer = Ojw.CConvert.IntToBytes((int)m_aSMot[nMotor].nValue2);
                                                        Array.Copy(buffer, 0, m_abyBuffer, m_nBuffer, nAddress_Size);
                                                        m_nBuffer += nAddress_Size;
                                                    }
                                                    nMotorPtCount++;
                                                }
                                                else if (m_aSMot[nMotor].EEnable == EEnable_t._Speed)
                                                {
                                                    if (nAddress_Speed < 0)
                                                    {
                                                        nAddress_Speed = m_aSAddress[nMotor].nGoal_Vel_104_4; ;
                                                        nAddress_Speed_Size = m_aSAddress[nMotor].nGoal_Vel_Size_4;
                                                    }
                                                    m_abyBuffer_Speed[m_nBuffer_Speed++] = (byte)Get_RealID(nMotor);

                                                    if (nAddress_Speed_Size == 1)
                                                        m_abyBuffer_Speed[m_nBuffer_Speed++] = (byte)(m_aSMot[nMotor].nValue2 & 0xff);
                                                    else if (nAddress_Speed_Size == 2)
                                                    {
                                                        buffer = Ojw.CConvert.ShortToBytes((short)m_aSMot[nMotor].nValue2);
                                                        Array.Copy(buffer, 0, m_abyBuffer_Speed, m_nBuffer_Speed, nAddress_Speed_Size);
                                                        m_nBuffer_Speed += nAddress_Speed_Size;
                                                    }
                                                    else //(nAddress_Speed_Size == 4)
                                                    {
                                                        buffer = Ojw.CConvert.IntToBytes((int)Math.Abs(m_aSMot[nMotor].nValue2) & 0x3ff | (m_aSMot[nMotor].nValue2 < 0 ? 0x400 : 0x00)); // 10 번째 비트가 방향 비트
                                                        Array.Copy(buffer, 0, m_abyBuffer_Speed, m_nBuffer_Speed, nAddress_Speed_Size);
                                                        m_nBuffer_Speed += nAddress_Speed_Size;
                                                    }
                                                    nMotorPtCount_Speed++;
                                                }
                                            }
                                            break;
                                        case 2:
                                            // 여길 속도 모드면 들어오지 않도록 나중에 고칠 것.
                                            // 현재는 bSetValue2 == true 여도 들어오게 되어 있다.
                                            if (
                                                 (m_aSMot[nMotor].EOperationMode == EOperationMode_t._Position) ||
                                                 (m_aSMot[nMotor].EOperationMode == EOperationMode_t._Position_Multi) ||
                                                 (m_aSMot[nMotor].EOperationMode == EOperationMode_t._Position_Amp)
                                                )
                                            {
                                                if (nAddress < 0)
                                                {
                                                    nAddress = m_aSAddress[nMotor].nGoal_Pos_116_4;
                                                    nAddress_Size = m_aSAddress[nMotor].nGoal_Pos_Size_4;
                                                }
                                                m_abyBuffer[m_nBuffer++] = (byte)Get_RealID(nMotor);
                                                if (nAddress_Size == 1)
                                                    m_abyBuffer[m_nBuffer++] = (byte)m_aSMot[nMotor].nValue;//EOperationMode;
                                                else if (nAddress_Size == 2)
                                                {
                                                    buffer = Ojw.CConvert.ShortToBytes((short)m_aSMot[nMotor].nValue);
                                                    Array.Copy(buffer, 0, m_abyBuffer, m_nBuffer, nAddress_Size);
                                                    m_nBuffer += nAddress_Size;
                                                }
                                                else //(nAddress_Size == 4)
                                                {
                                                    buffer = Ojw.CConvert.IntToBytes((int)m_aSMot[nMotor].nValue);
                                                    Array.Copy(buffer, 0, m_abyBuffer, m_nBuffer, nAddress_Size);
                                                    m_nBuffer += nAddress_Size;
                                                }
                                                nMotorPtCount++;
                                            }
#if false // Speed 는 이미 앞전에 셋팅이 되었기 때문에 여기서는 필요 없다.
                                            else if (m_aSMot[nMotor].EOperationMode == EOperationMode_t._Speed)
                                            {
                                                if (nAddress < 0)
                                                {
                                                    nAddress = m_aSAddress[nMotor].nGoal_Vel_104_4;
                                                    nAddress_Size = m_aSAddress[nMotor].nGoal_Vel_Size_4;
                                                }
                                                m_abyBuffer[m_nBuffer++] = (byte)Get_RealID(nMotor);

                                                int nVal = (m_aSMot[nMotor].bSetValue2 == true) ? m_aSMot[nMotor].nValue2 : m_aSMot[nMotor].nValue;
                                                if (nAddress_Size == 1)
                                                    m_abyBuffer[m_nBuffer++] = (byte)nVal;
                                                else if (nAddress_Size == 2)
                                                {
                                                    // 이때는 10 비트가 부호비트. -> 나중에 고쳐 둘것.
                                                    buffer = Ojw.CConvert.ShortToBytes((short)nVal);
                                                    Array.Copy(buffer, 0, m_abyBuffer, m_nBuffer, nAddress_Size);
                                                    m_nBuffer += nAddress_Size;
                                                }
                                                else //(nAddress_Size == 4)
                                                {
                                                    buffer = Ojw.CConvert.IntToBytes((int)nVal);
                                                    Array.Copy(buffer, 0, m_abyBuffer, m_nBuffer, nAddress_Size);
                                                    m_nBuffer += nAddress_Size;
                                                }

                                                nMotorPtCount++;
                                            }
#endif
                                            break;
                                    }
                                    //nMotorPtCount++;
                                }
                                //else // SG-90
                                //{
                                //}
                            }
                            if (nMotorPtCount > 0)
                            {
                                Ojw.CTimer CTmr = new CTimer();
                                if (nCommand == 0)
                                {
                                    //           CTmr.Set(); while (CTmr.Get() < 50) Thread.Sleep(1);
                                    SetTorqOff(m_lstMotors.ToArray());
                                }
                                //        CTmr.Set(); while (CTmr.Get() < 50) Thread.Sleep(1);//Ojw.CTimer.Wait(1);
                                Writes(nProtocol, nSerialIndex, nAddress, nAddress_Size, nMotorPtCount, m_abyBuffer);
                                //        CTmr.Set(); while (CTmr.Get() < 10) Thread.Sleep(1); //Ojw.CTimer.Wait(1);
                                //if (nCommand == 2)
                                //{
                                //    m_aSMot
                                //}

                                if (nCommand == 0)
                                {
                                    SetTorqOn(m_lstMotors.ToArray()); // 나중에는 토크가 기존에 살아 있던 것만 골라 살도록 한다.
                                }
                            }
                            if (nMotorPtCount_Speed > 0)
                            {
                                Writes(nProtocol, nSerialIndex, nAddress_Speed, nAddress_Speed_Size, nMotorPtCount_Speed, m_abyBuffer_Speed);
                            }
                        }
                    }
                }

                for (int i = 0; i < nMotorCnt; i++)
                {
                    nMotor = m_anEn[i];
                    Done(nMotor);
                    //Done(Pop());
                }
                //Pop()
                m_nMotorCnt = 0;
                m_nWait = nMilliseconds;
            }

            #endregion Control
            int m_nWait = 0;
            public void Delay(int nMilliseconds)
            {
                CTimer CTmr = new CTimer();
                CTmr.Set(); while (CTmr.Get() < nMilliseconds) Ojw.CTimer.Wait(1);
            }
            public void Wait()
            {
                if (m_nWait <= 0) return;
                Wait(m_nWait);
            }
            public void Wait_Per(float fPercent_0_1)
            {
                Wait((int)Math.Round((float)m_nWait * fPercent_0_1));
            }
            public void Wait(int nMilliseconds)
            {
                CTimer CTmr = new CTimer();
                CTmr.Set(); while (CTmr.Get() < nMilliseconds) Ojw.CTimer.Wait(1);
            }


            #region MotionFile
            // Set3D 하면 모델링에서 가져온 상태로 강제 재셋팅, -> PlayFile(파일이름) 하면 된다.
            private Ojw.C3d m_C3d = null;
            private Ojw.C3d.COjwDesignerHeader m_CHeader = null;//new C3d.COjwDesignerHeader();
            public void SetHeader(Ojw.C3d.COjwDesignerHeader CHeader)
            {
                m_CHeader = CHeader;
                for (int i = 0; i < m_CHeader.nMotorCnt; i++)
                {
                    // 0 : None, 1 : xl-320, 2 : xl_430(Default), 3 - ax-12
                    if (m_CHeader.pSMotorInfo[i].nHwMotorName == 2)
                    {
                        SetParam_MotorType(i, EMotor_t.XL_430);
                        SetParam_Dir(i, m_CHeader.pSMotorInfo[i].nMotorDir);
                    }
                    else if (m_CHeader.pSMotorInfo[i].nHwMotorName == 3)
                    {
                        SetParam_MotorType(i, EMotor_t.AX_12);
                        //SetParam_RealID
                        SetParam_Dir(i, m_CHeader.pSMotorInfo[i].nMotorDir);
                    }

                    //m_CHeader.pSMotorInfo[i].fLimit_Up,
                    //m_CHeader.pSMotorInfo[i].fLimit_Down,
                    //(float)m_CHeader.pSMotorInfo[i].nCenter_Evd,
                    //0,
                    //(float)m_CHeader.pSMotorInfo[i].nMechMove,
                    //m_CHeader.pSMotorInfo[i].fMechAngle);
                }
            }
            public void Set3D(Ojw.C3d C3dModel) { m_C3d = C3dModel; SetHeader(m_C3d.GetHeader()); }
            private const int _SIZE_MOTOR_MAX = 999;
            public void PlayFrame(int nLine, SMotion_t SMotion)
            {
                if (SMotion.nFrameSize <= 0) return;
                if ((nLine < 0) || (nLine >= SMotion.nFrameSize)) return;

                if ((m_bStop == false) && (m_bEms == false) && (m_bMotionEnd == false))
                {
                    //m_bStop = false; 
                    //for (int i = 0; i < _SIZE_MOTOR_MAX; i++) Set_Flag_Stop(i, false);
                    SetTorq(true);

                    for (int nAxis = 0; nAxis < m_nMotorCnt; nAxis++)
                    {
                        if (m_CHeader.pSMotorInfo[nAxis].nMotorControlType != 0) // 위치제어가 아니라면
                        {
                            //SetParam_Dir(nAxis, m_CHeader.pSMotorInfo[nAxis].nMotorDir);
                            Set_Turn(nAxis, SMotion.STable[nLine].anMot[nAxis]);
                        }
                        else
                        {
                            SetParam_Dir(nAxis, m_CHeader.pSMotorInfo[nAxis].nMotorDir);

                            Set(nAxis, SMotion.STable[nLine].anMot[nAxis]);
                        }
                    }
                    Send_Motor(SMotion.STable[nLine].nTime);
                }
            }
            public void PlayFrame(SMotionTable_t STable)
            {
                if ((m_bStop == false) && (m_bEms == false) && (m_bMotionEnd == false))
                {
                    //m_bStop = false; 
                    //for (int i = 0; i < _SIZE_MOTOR_MAX; i++) Set_Flag_Stop(i, false);
                    SetTorq(true);
                    for (int nAxis = 0; nAxis < m_CHeader.nMotorCnt; nAxis++)
                    {
                        if (m_CHeader.pSMotorInfo[nAxis].nMotorControlType != 0) // 위치제어가 아니라면
                        {
                            //SetParam_Dir(nAxis, m_CHeader.pSMotorInfo[nAxis].nMotorDir);
                            //Set_Flag_Led(nAxis,
                            //    Get_Flag_Led_Green(STable.anLed[nAxis]),
                            //    Get_Flag_Led_Blue(STable.anLed[nAxis]),
                            //    Get_Flag_Led_Red(STable.anLed[nAxis])
                            //    );

                            Set_Turn(nAxis, STable.anMot[nAxis]);
                        }
                        else
                        {
                            //SetParam_Dir(nAxis, m_CHeader.pSMotorInfo[nAxis].nMotorDir);
                            //Set_Flag_Led(nAxis,
                            //    Get_Flag_Led_Green(STable.anLed[nAxis]),
                            //    Get_Flag_Led_Blue(STable.anLed[nAxis]),
                            //    Get_Flag_Led_Red(STable.anLed[nAxis])
                            //    );

                            Set(nAxis, STable.anMot[nAxis]);
                        }
                    }
                    Send_Motor(STable.nTime);
                }
            }
            public void PlayFile(string strFileName)
            {
                try
                {
                    if (m_C3d == null) return;
                    SMotion_t SMotion = new SMotion_t();
                    if (m_C3d.BinaryFileOpen(strFileName, out SMotion) == true)
                    {
                        if (SMotion.nFrameSize > 0)
                        {
                            m_bStart = true;

                            m_C3d.WaitAction_SetTimer();

                            foreach (SMotionTable_t STable in SMotion.STable)
                            {
                                if (STable.bEn == true)
                                {
                                    PlayFrame(STable);

                                    int nDelay = STable.nTime + STable.nDelay;
                                    if (nDelay > 0) m_C3d.WaitAction_ByTimer(nDelay);
                                }
                            }
                            m_bStart = false;
                            m_bMotionEnd = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Ojw.CMessage.Write_Error("Error -> PlayMotion(), " + ex.ToString());
                }
            }
            #endregion MotionFile

            #region Background
            private void Push(int nMotor) { if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return; if (IsCmd(nMotor) == false) m_anEn[m_nMotorCnt++] = nMotor; }
            private int Pop() { if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return -1; if (m_nMotorCnt > 0) return m_anEn[--m_nMotorCnt]; return -1; }
            public bool IsCmd(int nMotor) { for (int i = 0; i < m_nMotorCnt; i++) if (m_anEn[i] == nMotor) return true; return false; }

            private float CalcRaw2Rpm(int nMotor, int nValue) { return (float)nValue * m_aSParam[nMotor].fRefRpm; }
            private int CalcRpm2Raw(int nMotor, float fRpm) { return (int)Math.Round(Clip(m_aSParam[nMotor].fLimitRpm, 0, fRpm / m_aSParam[nMotor].fRefRpm)); }

            private float CalcTime2Rpm(float fDeltaAngle, float fTime)
            {
                //if (fDeltaAngle == 0) fDeltaAngle = (float)CMath._ZERO;
                #region Kor
                // 1도 이동시간 => fTime / fDeltaAngle

                // 60 초 동안 _MAX_RPM 을 회전하는 것이 RPM, 1도 움직이는 것을 체크하려면 여기에 360 도 회전값을 고려해 주어야 한다.
                // _MAX_RPM 은 1분(60초) 동안 _MAX_RPM 바퀴 (즉, 360 * _MAX_RPM) 를 회전한 값
                // _MAX_RPM * 360 : 60 seconds => 480(_MAX_EV_RPM) 일때 
                // => 1초간 6 * _MAX_RPM 도 회전, 1ms => _MAX_RPM * 360도 / 60000ms = 0.65952 도 이동
                // => 1도 움직이는데 60000 / (_MAX_RPM * 360) = 1.516254246 ms 가 필요

                // 1도 이동시간 => 60000 / (Rpm * 360)
                // 이동각도 당 이동시간 계산 => 1도 이동시간 * fDeltaAngle
                #endregion Kor
                //return 60000 / (fTime / fDeltaAngle * 360.0f);
                return (60.0f * fDeltaAngle * 1000.0f) / (360.0f * fTime);
            }
            #endregion Background
        }
#else
#if OJW5014_20180207
        public class CMonster
        {
            #region Define
            // 모터의 수량은 256개 한정으로 정한다.(고정) -> 추후 가변으로 바꿀지는 완성 이후 고민
            private const int _CNT = 256;
            private int m_nMotorCountAll = _CNT; // _CNT 를 바로 사용하지 말고 이 변수를 사용하도록 한다.(추후 확장성)
            private const int _ADDRESS_TORQ_XL_320 = 24;
            private const int _ADDRESS_TORQ_XL_430 = 64;
            private const int _ADDRESS_TORQ_AX = 24;
            public enum EMotor_t
            {
                NONE = 0,
                // Default
                XL_430 = 20, // 360도 4096 : 2048 center : 0.229 RefRpm, 480 Limit Rpm
                XL_320 = 21, // 300도 1024 : 512 center : 0.111 RefRpm, 1023 Limit Rpm

                XM_540 = 30, // LED [65], 360도 4096 : 2048 center : 0.229 RefRpm, 480 Limit Rpm  , 확장위치제어모드시 512 회전 가능(+-256)

                AX_12 = 1, // 9~12v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시)
                AX_18 = 2, //
                
                DX_113 = 3, //
                DX_116 = 4, //
                DX_117 = 5, //
                RX_10 = 6, // 9~12v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시)
                RX_24F = 7, // 9~12v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시)
                RX_28 = 8, // 12v~18.5v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시)
                RX_64 = 9, // 12v~18.5v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시)
                EX_106 = 10, // 12v~18.5v, 0~251도, c=2048, mx=4095, mn=0, 


                // protocol 2.0
                MX_12 = 11, // 10~14.8v, 0~360, c=2048, mx=4095, mn=0, drpm = 0.916rpm, 다중회전가능(각 7바퀴:-28,672~28,672)
                MX_28 = 12, // 10~14.8v, 0~360, c=2048, mx=4095, mn=0, drpm = 0.114rpm, 다중회전가능(각 7바퀴:-28,672~28,672)  
                MX_64 = 13, // 10~14.8v, 0~360, c=2048, mx=4095, mn=0, drpm = 0.114rpm, 다중회전가능(각 7바퀴:-28,672~28,672)  
                MX_106 = 14, // 10~14.8v, 0~360, c=2048, mx=4095, mn=0, drpm = 0.114rpm, 다중회전가능(각 7바퀴:-28,672~28,672)  
                //MX_ = 15, //

                SG_90 = 100
            }

            #endregion Define

            #region Structure
            public struct SAddress_t
            {
                #region 확장기능 // Table Address - 여기부터는 값을 가지는게 아닌 주소번지만 가지도록...
                public int nMotorNumber_0_2;        // R    0 : none
                public int nMotorNumber_Size_2;
                public int nFwVersion_6_1;          // R      
                public int nFwVersion_Size_1;
                public int nRealID_7_1;             // RW   0 ~ 252
                public int nRealID_Size_1;
                public int nBaudrate_8_1;           // RW   0-9600, 1-57600, 2-115200, 3-1M, 4-2M,  5-3M, 6-4M, 7-4.5M
                public int nBaudrate_Size_1;
                // [DriveMode] 
                //    0x01 : 정상회전(0), 역회전(1)
                //    0x02 : 540 전용 Master(0), Slave(1)
                //    0x04 : 속도기준 제어(0), 시간기준 제어(1)
                public int nMode_Drive_10_1;        // RW 
                public int nMode_Drive_Size_1;
                public int nMode_Operating_11_1;    // RW   1=속도제어(무한회전), 3(초기값)=위치제어(RPM), 4=Multi-Turn(-256~256:512회전), 5=전류기반 위치제어모드(-256~256:512회전), 16=PWM제어
                public int nMode_Operating_Size_1;
                public int nProtocolVersion_13_1;   // RW   
                public int nProtocolVersion_Size_1;
                public int nOffset_20_4;            // RW   1회전 모드인 경우 -1024~1024 까지만 제어
                public int nOffset_Size_4;
                public int nLimit_PWM_36_2;         // RW   0~885 (885 = 100%)
                public int nLimit_PWM_Size_2;
                public int nLimit_Curr_38_2;        // RW   전류제어모드/전류기반위치제어모드에서 사용, 0~2047
                public int nLimit_Curr_Size_2;
                // [Shutdown] - Reboot 으로만 해제 가능
                //    0x20 : 과부하
                //    0x10 : 전력이상
                //    0x08 : 엔코더 이상(Following Error)
                //    0x04 : 과열
                //    0x01 : 인가된 전압 이상
                public int nShutdown_63_1;          // RW
                public int nShutdown_Size_1;
                public int nTorq_64_1;              // RW   Off(0), On(1)
                public int nTorq_Size_1;
                public int nLed_65_1;               // RW   Off(0), On(1)
                public int nLed_Size_1;
                public int nError_70_1;             // R    
                public int nError_Size_1;
                public int nGain_Vel_I_76_2;        // RW
                public int nGain_Vel_I_Size_2;
                public int nGain_Vel_P_78_2;        // RW
                public int nGain_Vel_P_Size_2;
                public int nGain_Pos_D_80_2;        // RW
                public int nGain_Pos_D_Size_2;
                public int nGain_Pos_I_82_2;        // RW
                public int nGain_Pos_I_Size_2;
                public int nGain_Pos_P_84_2;        // RW
                public int nGain_Pos_P_Size_2;
                public int nGain_Pos_F2_88_2;       // RW
                public int nGain_Pos_F2_Size_2;
                public int nGain_Pos_F1_90_2;       // RW
                public int nGain_Pos_F1_Size_2;

                public int nWatchDog_98_1;          // RW
                public int nWatchDog_Size_1;

                public int nGoal_PWM_100_2;         // RW   -PWMLimit ~ +PWMLimit
                public int nGoal_PWM_Size_2;
                public int nGoal_Current_102_2;     // RW   -CurrentLimit ~ +CurrentLimit
                public int nGoal_Current_Size_2;
                public int nGoal_Vel_104_4;         // RW
                public int nGoal_Vel_Size_4;

                public int nProfile_Acc_108_4;      // RW
                public int nProfile_Acc_Size_4;
                public int nProfile_Vel_112_4;      // RW
                public int nProfile_Vel_Size_4;

                public int nGoal_Pos_116_4;         // RW
                public int nGoal_Pos_Size_4;

                public int nMoving_122_1;           // R    움직임 감지 못함(0), 움직임 감지(1)
                public int nMoving_Size_1;
                // [Moving Status]
                //    0x30  - 0x30 : 사다리꼴 속도 프로파일
                //            0x20 : 삼각 속도 프로파일
                //            0x10 : 사각 속도 프로파일
                //            0x00 : 프로파일 미사용(Step)
                //    0x08 : Following Error
                //    0x02 : Goal Position 명령에 따라 진행 중
                //    0x01 : Inposition
                public int nMoving_Status_123_1;    // R
                public int nMoving_Status_Size_1;

                public int nPresent_PWM_124_2;      // R
                public int nPresent_PWM_Size_2;
                public int nPresent_Curr_126_2;     // R
                public int nPresent_Curr_Size_2;
                public int nPresent_Vel_128_4;      // R
                public int nPresent_Vel_Size_4;
                public int nPresent_Pos_132_4;      // R
                public int nPresent_Pos_Size_4;
                public int nPresent_Volt_144_2;     // R
                public int nPresent_Volt_Size_2;
                public int nPresent_Temp_146_1;     // R
                public int nPresent_Temp_Size_1;
                #endregion 확장기능
            }
            public struct SParam_t
            {
                //public int nID;                     // 모터를 제어하기 위한 가상 아이디. 중복 허용 불가. 보통은 모터 ID 와 일치시킨다.            
                public int nCommIndex;              // 연결 된 통신포트 중 어느것에 연결 된 것이지에 대한 순서.
                public int nRealID;
                public EMotor_t EMot;

                public int nDir;
                //Center
                public float fCenterPos;

                public float fMechMove;
                public float fDegree;
                public float fRefRpm;

                public float fLimitRpm;     // ?

                public float fLimitUp;    // Limit - 0: Ignore
                public float fLimitDn;    // Limit - 0: Ignore

                //public EOperationMode_t EOperationMode;          // 2 - 전류제어모드(실제 제어에서는 0을 사용), 1 - 속도제어모드(바퀴제어), 3(default) - 위치제어모드, 4 - 확장위치제어모드(X시리즈의 경우 -256 ~ +256회전 지원), 5  -전류기반 위치제어모드(X시리즈의 경우 -256 ~ +256회전 지원), 16 - PWM 제어모드
                //public EOperationMode_t EOperationMode_Prev;
                public int nProtocol;               // 2, 0 - protocol2, 1 - protocol1, 3 - None   
            }
            // 0 - 전류제어모드(이거 2로 바꾼다), 1 - 속도제어모드(바퀴제어), 3(default) - 위치제어모드, 4 - 확장위치제어모드(X시리즈의 경우 -256 ~ +256회전 지원), 5  -전류기반 위치제어모드(X시리즈의 경우 -256 ~ +256회전 지원), 16 - PWM 제어모드
            public enum EOperationMode_t
            {
                _None = 0,
                _Amp = 2,
                _Speed = 1,
                _Position = 3,
                _Position_Multi = 4,
                _Position_Amp = 5,
                _Pwm = 16
            }
            public enum EEnable_t
            {
                _None = 0,
                //_Disable = 0,
                _Amp = 2,
                _Speed = 1,
                _Position = 3,
                _Pwm = 16
            }
            public struct SMap_t
            {
                public bool bShow;
                public int nSeq;
                public int nSeq_Back;

                public int nStep;
                public int nPortIndex;
                public int nProtocol;

                public int nAddress;
                public int nAddress_DataLength;

                public int nID;
                public int nLength;
                public int nCmd;
                public int nError;
                public int nModelNumber;
                public int nFw;
                public int nCrc0;
                public int nCrc1;
                public EMotor_t EMotor;

                public byte[] abyMap;
            }

            public struct SMot_t
            {
                public EEnable_t EEnable; // 1 : Pos, 2 : Speed, 3 : Packet
                //public int nOperationMode;
                //public int nOperationMode_Prev;

                public bool bInit_Value;
                // position
                public int nValue;
                public int nValue_Prev;
                public float fAngle_Back;
                // speed
                public int nValue2;
                public bool bSetValue2;
                public int nValue2_Prev;

                public int nStatus_Torq;
                public int nStatus_Torq_Prev;
                public int nStatus_Error;
                public int nStatus_Error_Prev;

                public bool bOperationMode;
                public EOperationMode_t EOperationMode;          // 2 - 전류제어모드(실제 제어에서는 0을 사용), 1 - 속도제어모드(바퀴제어), 3(default) - 위치제어모드, 4 - 확장위치제어모드(X시리즈의 경우 -256 ~ +256회전 지원), 5  -전류기반 위치제어모드(X시리즈의 경우 -256 ~ +256회전 지원), 16 - PWM 제어모드
                public EOperationMode_t EOperationMode_Prev;

                public Ojw.CTimer CTmr;
                public byte[] abyBuffer;
            }
            #endregion Structure


        #region Var
            private List<CSerial> m_lstSerial = null;//new List<CSerial>();
            private List<int> m_lstIndex = new List<int>();
            private List<int> m_lstID = new List<int>();
            private Thread m_thReceive = null;
            private List<int> m_lstPort = null;

            private bool m_bProgEnd = false;
            private bool m_bStop = false;
            private bool m_bEms = false;
            private bool m_bMotionEnd = false;
            private bool m_bStart = false;

            //private int[] m_anMot_SerialIndex = new int[_CNT];
            //private int[] m_anMot_RealID = new int[_CNT];
            //private EMotor_t[] m_aEMot_Type = new EMotor_t[_CNT];
            private SAddress_t[] m_aSAddress = new SAddress_t[_CNT];
            private SParam_t[] m_aSParam = new SParam_t[_CNT];

            private int m_nMotorCnt = 0;// { get { return m_lstSCommand.Count; } }
            private int[] m_anEn = new int[256]; // push / pop

            private SMot_t[] m_aSMot = new SMot_t[_CNT];
            //private List<SMap_t> m_lstSMap = new List<SMap_t>();//[_CNT];
            //private SMap_t[] m_aSMap_New = new SMap_t[_CNT];
            private SMap_t[] m_aSMap = new SMap_t[_CNT];
            //private SMap_t[] m_aSMap_Old = new SMap_t[_CNT];
            //private int [] m_anMap_Address = new int[_CNT];




            #endregion Var
            private struct SMotors_t
            {
                public int nRealID;
                public int nCommIndex;
                public int nProtocol;
                public EMotor_t EMotor;
            }
            private List<SMotors_t> m_lstSIds = new List<SMotors_t>();
            private bool m_bAutoset = false;
            private bool m_bAutoset2 = false;
            private Ojw.CTimer m_CTmr_AutoSet = new CTimer();
            public void SetShowReceiving(bool bShow)
            {
                m_bShowReceivedIDs = bShow;
            }
            public void AutoSet() { AutoSet(true, true); }
            public void AutoSet(bool bDisplay, bool bDisplay_Receive)
            {
                m_bShowReceivedIDs = false;
                m_bAutoset = true;
                m_bAutoset2 = false;
                m_lstSIds.Clear();
                Write_Ping(false);
                //m_CTmr_AutoSet.Set();
                //while ((m_CTmr_AutoSet.Get() < 100) && (m_bProgEnd == false)) Thread.Sleep(1);//Ojw.CTimer.Wait(1); ///Thread.Sleep(1);// Ojw.CTimer.Wait();
                if (m_bProgEnd == true) return;
                else
                {
                    m_bAutoset2 = true;
                    // 
                    m_bShowReceivedIDs = bDisplay_Receive;
                    for (int i = 0; i < m_lstSerial.Count; i++)
                    {
                        // 프로토콜 2의 아이디만 수집한다. (싱크리드 명령이 되니까...)
                        List<int> lstIds = new List<int>();
                        lstIds.Clear();
                        foreach (SMotors_t SMot in m_lstSIds)
                        {
                            if (SMot.nCommIndex == i)
                            {
                                //if ((SMot.nProtocol == 2) && (SMot.nCommIndex == i))
                                if (SMot.nProtocol == 2)
                                    lstIds.Add(SMot.nRealID);
                                else
                                {
                                    m_CTmr_AutoSet.Set();
                                    Write2(1, i, SMot.nRealID, 0x02, 0, 50);
                                    Ojw.CTimer.Wait(50);
                                    //m_CTmr_AutoSet.Set();
                                    //while ((m_CTmr_AutoSet.Get() < 200) && (m_bProgEnd == false)) Thread.Sleep(1);//Ojw.CTimer.Wait(1); ///Thread.Sleep(1); //나중에 받으면 넘어가도록 만들 것.
                                }
                            }
                        }
                        if (lstIds.Count > 0)
                        {
                            m_CTmr_AutoSet.Set();
                            Reads(i, 0, 50, lstIds.ToArray()); // 일단 50 까지만 읽는다.(xl-320 때문에...) 나중에 XL-320 이 아니라면 147 까지 읽어온다.
                            Ojw.CTimer.Wait(50);
                            //m_CTmr_AutoSet.Set();
                            //while ((m_CTmr_AutoSet.Get() < 300) && (m_bProgEnd == false)) Thread.Sleep(1);//Ojw.CTimer.Wait(1); ///Thread.Sleep(1);
                        }
                        // 
                    }
                    ///for(m_lstSIds
                    if (bDisplay)
                    {
                        Ojw.CMessage.Write2("Motors=>\r\n");
                        foreach (SMotors_t SMot in m_lstSIds)
                        {
                            int nMotor = FindMotor(SMot.nRealID);
                            Ojw.CMessage.Write2("[{0}]-{1}\r\n", nMotor, m_aSParam[nMotor].EMot);

                        }
                    }
                }
            }
            private EMotor_t GetMotorType(int nHwNum)
            {
                switch (nHwNum)
                {
                    case 113: return EMotor_t.DX_113;
                    case 116: return EMotor_t.DX_116;
                    case 117: return EMotor_t.DX_117;
                    case 18: return EMotor_t.AX_18;
                    case 44:
                    case 12: return EMotor_t.AX_12;
                    //case 300: return EMotor_t.AX_12;
                    case 10: return EMotor_t.RX_10;
                    case 24: return EMotor_t.RX_24F;
                    case 28: return EMotor_t.RX_28;
                    case 64: return EMotor_t.RX_64;
                    case 107: return EMotor_t.EX_106;
                    case 360: return EMotor_t.MX_12;
                    ////////////////////////////////////
                    //여기부터 2.0
                    case 30: return EMotor_t.MX_28;
                    case 311: return EMotor_t.MX_64;
                    case 321: return EMotor_t.MX_106;
                    case 350: return EMotor_t.XL_320;
                    case 1060: return EMotor_t.XL_430;
                }
                return EMotor_t.NONE; ;
            }
            private bool m_bShowReceivedIDs = false;
            // 초기화
            private void Init()
            {
                for (int i = 0; i < m_nMotorCountAll; i++)
                {
                    SetParam(i, i, 0, 0, EMotor_t.XL_430);
                    //    m_anMot_SerialIndex[i] = 0;
                    //    m_anMot_RealID[i] = i;
                    //    m_aEMot_Type[i] = EMotor_t.XL_430;
                    //    SetAddress(i, EMotor_t.XL_430);
                    m_aSMot[i].bInit_Value = false;
                    m_aSMot[i].EEnable = EEnable_t._None;
                    m_aSMot[i].CTmr = new CTimer();
                    m_aSMot[i].nStatus_Torq = 0;
                    m_aSMot[i].nValue = 0;
                    m_aSMot[i].nValue_Prev = 0;
                    m_aSMot[i].fAngle_Back = 0.0f; // 초기화 때만 클리어 하고 남겨두는 데이타
                    m_aSMot[i].bSetValue2 = false;
                    m_aSMot[i].nValue2 = 0;
                    m_aSMot[i].nValue2_Prev = 0;
                    m_aSMot[i].nStatus_Error = 0;
                    m_aSMot[i].bOperationMode = false;
                    m_aSMot[i].EOperationMode = EOperationMode_t._Position; // Default

                    m_aSMap[i] = new SMap_t();
                    m_aSMap[i].abyMap = new byte[256];
                    //m_aSMap_Old[i] = new SMap_t();
                    //m_aSMap_Old[i].abyMap = new byte[200];
                    //m_anMap_Address[i] = -1;
                }
                //Array.Clear(m_aSMap, 0, m_aSMap.Length);
                InitKinematics();
            }
            private void Done(int nMotor)
            {
                m_aSMot[nMotor].EEnable = EEnable_t._None;
                m_aSMot[nMotor].bOperationMode = false;

                //m_aSMot[nMotor].CTmr = new CTimer();
                if (m_bEms == true) m_aSMot[nMotor].bInit_Value = false;
                m_aSMot[nMotor].nValue_Prev = m_aSMot[nMotor].nValue;
                m_aSMot[nMotor].nValue2_Prev = m_aSMot[nMotor].nValue2;
                //m_aSMot[nMotor].nStatus_Error = 0;
                //m_aSMot[nMotor].nOperationMode = 0;
                m_aSMot[nMotor].EOperationMode_Prev = m_aSMot[nMotor].EOperationMode;
                m_aSMot[nMotor].bSetValue2 = false;
                m_aSMot[nMotor].CTmr.Set();
            }
            public CMonster()
            {
                Init();
            }
            ~CMonster()
            {
                Close();
            }

            public void Stop(int nMotor) // no stop flag setting
            {
                byte[] pbyTmp = Ojw.CConvert.IntToBytes(0);
                //Write(nAxis, 104, pbyTmp);
                if (m_aSParam[nMotor].EMot != EMotor_t.SG_90)
                {
                    Write2(m_aSParam[nMotor].nProtocol, m_aSParam[nMotor].nCommIndex, ((nMotor < 0) ? 254 : (m_aSParam[nMotor].nRealID)), 0x03, m_aSAddress[nMotor].nGoal_Vel_104_4, 0);

                }
            }
            public void Stop()
            {
                byte[] pbyTmp = Ojw.CConvert.IntToBytes(0);
                //Write(nAxis, 104, pbyTmp);
                for (int i = 0; i < GetSerialCount(); i++)
                {
                    //if (m_aSParam[nMotor].EMot != EMotor_t.SG_90)
                    //{
                    // 속도
                    Write2(1, i, 254, 0x03, 32, 0);
                    Write2(2, i, 254, 0x03, 104, 0);

                    //}
                }
                m_bStop = true;
            }
            public void Ems()
            {
                Stop();
                SetTorq(false);
                m_bEms = true;
            }
            public void Reset()//(int nAxis)
            {
                // Clear Variable
                m_bStop = false;
                m_bEms = false;
            }
            public void Reboot() { Reboot(-1); }//_ID_BROADCASTING); }
            public void Reboot(int nMotor)
            {
                if ((nMotor < 0) || (nMotor == 254))
                {
                    //byte[] pbyTmp = Ojw.CConvert.IntToBytes(0);
                    //Write(nAxis, 104, pbyTmp);
                    for (int i = 0; i < GetSerialCount(); i++)
                    {
                        //if (m_aSParam[nMotor].EMot != EMotor_t.SG_90)
                        //{
                        // 속도
                        Write_Command(1, i, ((nMotor < 0) ? 254 : nMotor), 0x08);
                        Write_Command(2, i, ((nMotor < 0) ? 254 : nMotor), 0x08);

                        //}
                    }
                    //Clear_Flag();
                    m_bStop = false;
                    m_bEms = false;
                }
                else Write_Command(m_aSParam[nMotor].nProtocol, m_aSParam[nMotor].nCommIndex, nMotor, 0x08);
                // Initialize variable

                //m_nMotorCnt_Back = m_nMotorCnt = 0;

            }
            public bool IsStop() { return m_bStop; }
            public bool IsEms() { return m_bEms; }

        #region Open / Close, IsOpen, GetSerial, GetSerialIndex, GetSerialPort, GetSerialCount
            // 해당 컴포트가 열렸는지를 확인
            public bool IsOpen(int nPort)
            {
                if (m_lstPort != null)
                {
                    foreach (int nSerialPort in m_lstPort) if (nSerialPort == nPort) return true;
                }
                return false;
            }
            // 해당 포트의 시리얼 핸들을 리턴
            public CSerial GetSerial(int nPort)
            {
                if (m_lstPort != null)
                {
                    for (int i = 0; i < m_lstPort.Count; i++)
                    {
                        if (m_lstPort[i] == nPort) return m_lstSerial[i];
                    }
                }
                return null;
            }
            // 해당 포트의 인덱스 번호를 리턴
            public int GetSerialIndex(int nPort)
            {
                if (m_lstPort != null)
                {
                    for (int i = 0; i < m_lstPort.Count; i++)
                    {
                        if (m_lstPort[i] == nPort) return i;
                    }
                }
                return -1;
            }
            // 해당 인덱스의 포트 번호를 리턴
            public int GetSerialPort(int nCommIndex)
            {
                if (m_lstPort != null)
                {
                    if (nCommIndex < m_lstPort.Count)
                    {
                        return m_lstPort[nCommIndex];
                    }
                }
                return -1;
            }
            // 전체 포트의 인덱스 수를 리턴
            public int GetSerialCount()
            {
                if (m_lstPort != null)
                {
                    return m_lstPort.Count;
                }
                return -1;
            }

            //// * 중요: 오픈시에 Operation Mode, 모터 종류, 아이디, 위치값, 토크온 상태 를 가져와야 한다.
            // 컴포트를 연다.(중복 되지만 않으면 여러개를 여는 것이 가능)
            public bool Open(int nPort, int nBaudRate)
            {
                Ojw.CSerial COjwSerial = new CSerial();
                if (COjwSerial.Connect(nPort, nBaudRate) == true)
                {
                    if (m_lstSerial == null)
                    {
                        m_lstSerial = new List<CSerial>();
                        m_lstPort = new List<int>();
                        m_lstIndex = new List<int>();
                        m_lstID = new List<int>();
                        //m_lstSMap = new List<SMap_t>();

                        m_thReceive = new Thread(new ThreadStart(Thread_Receive));
                        m_thReceive.Start();
                        //Ojw.CMessage.Write("Init Thread");
                    }
                    m_lstSerial.Add(COjwSerial);
                    m_lstPort.Add(nPort);
                    m_lstIndex.Add(0);
                    m_lstID.Add(0);
                    //SMap_t SMap = new SMap_t();
                    //m_lstSMap.Add(SMap);

                    //Write_Ping(2, m_lstSerial.Count - 1, 254);

                    return true;
                }
                return false;
            }
            // 지정한 포트를 닫는다.
            public void Close(int nPort)
            {
                if (m_lstSerial != null)
                {
                    if (m_lstSerial.Count > 0)
                    {
                        int nIndex = GetSerialIndex(nPort);
                        if (nIndex >= 0)
                        {
                            m_lstSerial[nIndex].DisConnect();
                            m_lstSerial.RemoveAt(nIndex);
                            m_lstPort.RemoveAt(nIndex);
                            m_lstIndex.RemoveAt(nIndex);
                            m_lstID.RemoveAt(nIndex);
                            //m_lstSMap.RemoveAt(nIndex);
                        }
                    }
                    if (m_lstPort.Count == 0)
                    {
                        m_lstSerial.Clear(); m_lstPort.Clear(); m_lstIndex.Clear(); m_lstID.Clear();//m_lstSMap.Clear();
                        m_lstSerial = null; m_lstPort = null; m_lstIndex = null; m_lstID = null;//m_lstSMap = null;
                    }
                }
            }
            // 전체 포트를 닫는다.
            public void Close()
            {
                if (m_lstSerial != null)
                {
                    foreach (CSerial COjwSerial in m_lstSerial)
                    {
                        if (COjwSerial.IsConnect() == true) COjwSerial.DisConnect();
                    }
                    m_lstSerial.Clear(); m_lstPort.Clear(); m_lstIndex.Clear(); m_lstID.Clear();//m_lstSMap.Clear();
                    m_lstSerial = null; m_lstPort = null; m_lstIndex = null; m_lstID = null;//m_lstSMap = null;
                }
            }
        #endregion Open / Close, IsOpen, GetSerial, GetSerialIndex, GetSerialPort, GetSerialCount

            public void Read_Motor(int nMotor, int nAddress, int nLength)
            {
                byte[] pbyLength = Ojw.CConvert.ShortToBytes((short)nLength);
                Write2(m_aSParam[nMotor].nProtocol, m_aSParam[nMotor].nCommIndex, m_aSParam[nMotor].nRealID, 0x02, nAddress, pbyLength);
                pbyLength = null;
            }

            private void Thread_Receive()
            {
                //byte byHead0 = 0;
                //byte byHead1 = 0;
                //byte byHead2 = 0;

                byte[] buf;
                //byte[] abyTmp = new byte[4];
                //Ojw.CMessage.Write("[Thread_Receive] Running Thread");
                int nSeq = 0;
                while ((m_lstSerial != null) && (m_bProgEnd == false))
                {
                    try
                    {
                        if (m_lstSerial == null) break;
                        if (m_lstSerial[nSeq].IsConnect() == false) continue;
                        int nSize = m_lstSerial[nSeq].GetBuffer_Length();
                        if (nSize > 0)
                        {
                            buf = m_lstSerial[nSeq].GetBytes();
                            //Ojw.CMessage.Write("[Receive]");
                            //Ojw.CConvert.ByteToStructure(

                            //if (m_nProtocolVersion == 1)
                            //{
                            //    //continue;
                            //    //Parsor1(buf, nSize);
                            //}
                            //else // (m_aSParam_Axis[nAxis].nProtocol_Version == 2) 
                            //{
                            //    //Parsor(buf, nSize);
                            //}
                            //Ojw.CMessage.Write("");



#if false // test
                            //foreach (byte byData in buf)
                            //{
                            //    Ojw.CMessage.Write2("0x{0}", Ojw.CConvert.IntToHex(byData, 2));                                
                            //}
                            //Ojw.CMessage.Write2("\r\n");
#else
                            int i = 0;
                            foreach (byte byData in buf)
                            {
#if false // for some packet errors
                                if (byData == 0xff)
                                {
                                    if ((m_lstIndex[nSeq] & 0xf0000) != 0)
                                    {
                                        if ((byData == 0xff) && ((m_lstIndex[nSeq] & 0xf0000) == 0x10000))
                                            m_lstIndex[nSeq] = (m_lstIndex[nSeq] & 0xfffff) | 0x20000;
                                        else if ((byData == 0xff) && ((m_lstIndex[nSeq] & 0xf0000) == 0x30000)) // 0xff, 0xff
                                        {
                                            m_lstIndex[nSeq] = 0x1000; // start new ( protocol 1 기준 ) -> 0xff, 0xff 가 들어올 확률은 아주 높다. 해서 이 부분은 일단 블록
                                        }
                                    }
                                    else
                                    {
                                        m_lstIndex[nSeq] = (m_lstIndex[nSeq] & 0xffff) | 0x10000;
                                    }
                                }
#endif
                                if (((m_lstIndex[nSeq] & 0x1000) == 0) && (byData == 0xff)) m_lstIndex[nSeq] = ((m_lstIndex[nSeq] & 0xf0000) | 0x1000);
                                else if (((m_lstIndex[nSeq] & 0x3000) == 0x1000) && (byData == 0xff))
                                {
                                    m_aSMap[m_lstID[nSeq]].bShow = false;

                                    // Protocol 1 Checked
                                    m_lstIndex[nSeq] |= 0x2000;
                                }
                                else if (((m_lstIndex[nSeq] & 0x7000) == 0x3000) && (byData == 0xfd))
                                {
                                    m_lstIndex[nSeq] |= 0x4000;
                                }
                                else if (((m_lstIndex[nSeq] & 0xf000) == 0x7000) && (byData == 0x00))
                                {
                                    // start - Protocol 2
                                    m_lstIndex[nSeq] |= 0x8000;
                                    // 모터는 Seq 가 아니라 새로운 인덱스가 필요하다. 전면 수정 필요. 읽을 때 아이디, 시리얼인덱스, 프로토콜버전의 3가지가 일치하는지 확인하고 꺼내오도록...
                                    //if (m_lstSMap[nSeq].nProtocol != 2) m_lstSMap[nSeq].nProtocol = 2;                                    
                                }
                                else
                                {
                                    // Protocol 2 - Address 122 ~ 136 => (120 ~ 139): 14=>20 바이트(moving(1), MoveStatus(1), PWM(4), Load(4), Velocity(4), Position(4)
                                    if ((m_lstIndex[nSeq] & 0xf000) == 0xf000)
                                    {
                                        // 단, XL-320 은 프로토콜2 여도 주소번지가 다르다. 따로 예외 처리할 것. 나중에...




                                        // [Ping] => Length == 7 이면 Ping
                                        // ID(1), Length(2)=(0x07, 0x00), cmd(1)=0x55, error(1), ModelNumber(2), Fw(1), CRC(2)
                                        // [Read]
                                        // ID(1), Length(2),  Cmd(1) = 0x55, error(1), Datas(N)..., CRC(2) 
                                        switch (m_lstIndex[nSeq] & 0x0fff)
                                        {
                                            case 0:
                                                m_lstID[nSeq] = byData; // ID Setting
                                                if (m_aSMap[m_lstID[nSeq]].nProtocol != 2) m_aSMap[m_lstID[nSeq]].nProtocol = 2;
                                                m_aSMap[m_lstID[nSeq]].nID = byData;
                                                m_lstIndex[nSeq]++;
                                                break;
                                            case 1:
                                                m_aSMap[m_lstID[nSeq]].nLength = byData; // Length == 7 -> Ping
                                                m_lstIndex[nSeq]++;
                                                break;
                                            case 2:
                                                m_aSMap[m_lstID[nSeq]].nLength |= (((int)byData << 8) & 0xff00); // Length == 7 -> Ping
                                                m_lstIndex[nSeq]++;
                                                break;
                                            case 3:
                                                m_aSMap[m_lstID[nSeq]].nCmd = byData;
                                                m_lstIndex[nSeq]++;
                                                break;
                                            case 4:
                                                m_aSMap[m_lstID[nSeq]].nError = byData;
                                                m_aSMap[m_lstID[nSeq]].nStep = 0;
                                                m_lstIndex[nSeq]++;
                                                break;
                                            case 5:
                                                if (m_aSMap[m_lstID[nSeq]].nLength == 7) // Ping
                                                {
                                                    if (m_aSMap[m_lstID[nSeq]].nStep == 0)
                                                    {
                                                        m_aSMap[m_lstID[nSeq]].bShow = true;
                                                        m_aSMap[m_lstID[nSeq]].nModelNumber = byData;
                                                        m_aSMap[m_lstID[nSeq]].abyMap[0] = byData;
                                                    }
                                                    else if (m_aSMap[m_lstID[nSeq]].nStep == 1)
                                                    {
                                                        m_aSMap[m_lstID[nSeq]].nModelNumber |= ((int)byData << 8) & 0xff00;
                                                        m_aSMap[m_lstID[nSeq]].abyMap[1] = byData;
                                                    }
                                                    else m_aSMap[m_lstID[nSeq]].nFw = byData;

                                                    //if (m_aSMap[m_lstID[nSeq]].nStep + 1 >= m_aSMap[m_lstID[nSeq]].nLength - 4)
                                                    //{
                                                    //    //Ojw.CMessage.Write("Model={0}", 
                                                    //    Ojw.CMessage.Write("[Serial[{0}]:[ID:{1}]Received[Protocol={2}]Type=0x{3}", nSeq, m_lstID[nSeq], m_aSMap[m_lstID[nSeq]].nProtocol, Ojw.CConvert.IntToHex(m_aSMap[m_lstID[nSeq]].abyMap[0] | ((m_aSMap[m_lstID[nSeq]].abyMap[1] << 8) & 0xff00), 4));
                                                    //    m_lstIndex[nSeq]++;
                                                    //}
                                                }
                                                else // 
                                                {
                                                    m_aSMap[m_lstID[nSeq]].abyMap[m_aSMap[m_lstID[nSeq]].nStep] = byData;

                                                    //if (m_aSMap[m_lstID[nSeq]].nStep + 1 >= m_aSMap[m_lstID[nSeq]].nLength - 4) m_lstIndex[nSeq]++;
                                                }
                                                if (m_aSMap[m_lstID[nSeq]].nStep + 1 >= m_aSMap[m_lstID[nSeq]].nLength - 4) m_lstIndex[nSeq]++;
                                                m_aSMap[m_lstID[nSeq]].nStep++;
                                                break;
                                            case 6:
                                                m_aSMap[m_lstID[nSeq]].nCrc0 = byData;
                                                m_lstIndex[nSeq]++;
                                                break;
                                            case 7:
                                                m_aSMap[m_lstID[nSeq]].nCrc1 = byData;
                                                m_aSMap[m_lstID[nSeq]].nSeq++;
                                                m_lstIndex[nSeq] = 0;
                                                //if (m_bShowReceivedIDs == true) Ojw.CMessage.Write("[Serial[{0}]:[ID:{1}]Received[Protocol={2}]Type={3}", nSeq, m_lstID[nSeq], m_aSMap[m_lstID[nSeq]].nProtocol, (m_aSMap[m_lstID[nSeq]].abyMap[0] | m_aSMap[m_lstID[nSeq]].abyMap[1] << 8));

                                                if (
                                                    (m_bShowReceivedIDs == true) 
                                                    ||
                                                    (m_aSMap[m_lstID[nSeq]].bShow == true)
                                                    )
                                                    Ojw.CMessage.Write("[Serial[{0}]:[ID:{1}]Received[Protocol={2}]Type=0x{3}", nSeq, m_lstID[nSeq], m_aSMap[m_lstID[nSeq]].nProtocol, Ojw.CConvert.IntToHex(m_aSMap[m_lstID[nSeq]].abyMap[0] | ((m_aSMap[m_lstID[nSeq]].abyMap[1] << 8) & 0xff00), 4));
                                                if (m_bAutoset == true)
                                                {
                                                    if (m_CTmr_AutoSet.Get() > 500)
                                                        m_bAutoset = false;
                                                    else
                                                    {
                                                        m_CTmr_AutoSet.Set();
                                                        SMotors_t SMot = new SMotors_t();
                                                        SMot.nCommIndex = nSeq;
                                                        SMot.nRealID = m_lstID[nSeq];
                                                        SMot.nProtocol = m_aSMap[m_lstID[nSeq]].nProtocol;
                                                        if (m_bAutoset2 == false) m_lstSIds.Add(SMot);
                                                    }
                                                }
                                                if (m_bAutoset2 == true)
                                                {
                                                    EMotor_t EMot = GetMotorType(m_aSMap[m_lstID[nSeq]].abyMap[0] | ((m_aSMap[m_lstID[nSeq]].abyMap[1] << 8) & 0xff00));
                                                    if ((EMot != EMotor_t.NONE) && (EMot != EMotor_t.SG_90))
                                                    {
                                                        //SetParam_MotorType(m_lstID[nSeq], EMot);
                                                        SetParam(m_lstID[nSeq], m_lstID[nSeq], nSeq, 0, EMot);
                                                    }
                                                }
                                                break;

#if false
                public int nCmd;
                public int nError;
                public int nModelNumber;
                public EMotor_t EMotor;
#endif
                                        }
                                    }
                                    // Protocol 1 - Address 36 ~ 46 => (30~49) : 10=>20 바이트(Position(2), Speed(2), Load(2), Volt(1), Temp(1), Registered(2), Moving(1)
                                    else if ((m_lstIndex[nSeq] & 0x3000) == 0x3000)
                                    {
#if true
                                        // [Ping] => Length == 2 이면 Ping
                                        // ID(1), Length(1)=(0x02), error(1), Checksum(1)
                                        // [Read]
                                        // ID(1), Length(1),  Cmd(1) = 0x55, error(1), Datas(N)..., CRC(2) 
                                        //switch (m_lstIndex[nSeq] & 0x0fff)
                                        //{
                                        //    case 0: // ID
                                        //        if (m_aSMap_Old[byData].nProtocol != 1) m_aSMap_Old[byData].nProtocol = 1;
                                        //        m_aSMap_Old[byData].nID = byData;
                                        //        m_lstIndex[nSeq]++;
                                        //        break;

                                        //}                                        
                                        switch (m_lstIndex[nSeq] & 0x0fff)
                                        {
                                            case 0:
                                                m_lstID[nSeq] = byData; // ID Setting
                                                if (m_aSMap[m_lstID[nSeq]].nProtocol != 1) m_aSMap[m_lstID[nSeq]].nProtocol = 1;
                                                m_aSMap[m_lstID[nSeq]].nID = byData;
                                                m_lstIndex[nSeq]++;
                                                break;
                                            case 1:
                                                m_aSMap[m_lstID[nSeq]].nLength = byData; // Length == 2 -> Ping
                                                m_lstIndex[nSeq]++;
                                                if (byData == 2) m_lstIndex[nSeq]++; // Ping Data 는 Cmd 가 없다.
                                                break;
                                            //case 2:
                                            //    m_aSMap[m_lstID[nSeq]].nCmd = byData;
                                            //    m_lstIndex[nSeq]++;
                                            //    break;
                                            case 2:
                                                m_aSMap[m_lstID[nSeq]].nError = byData;
                                                m_aSMap[m_lstID[nSeq]].nStep = 0;
                                                m_lstIndex[nSeq]++;
                                                break;
                                            case 3:
                                                if (m_aSMap[m_lstID[nSeq]].nLength == 2) // Ping
                                                {
                                                    //if (m_aSMap[m_lstID[nSeq]].nStep == 0) m_aSMap[m_lstID[nSeq]].nModelNumber = byData;
                                                    //else if (m_aSMap[m_lstID[nSeq]].nStep == 1) m_aSMap[m_lstID[nSeq]].nModelNumber |= ((int)byData << 8) & 0xff00;
                                                    //else m_aSMap[m_lstID[nSeq]].nFw = byData;
                                                }
                                                else // 
                                                {
                                                    m_aSMap[m_lstID[nSeq]].abyMap[m_aSMap[m_lstID[nSeq]].nStep] = byData;
                                                }
                                                if (m_aSMap[m_lstID[nSeq]].nStep + 1 >= m_aSMap[m_lstID[nSeq]].nLength - 3) m_lstIndex[nSeq]++;
                                                m_aSMap[m_lstID[nSeq]].nStep++;
                                                break;
                                            //case 4:
                                            //    m_aSMap[m_lstID[nSeq]].nCrc0 = byData;
                                            //    m_lstIndex[nSeq]++;
                                            //    break;
                                            case 4:
                                                m_aSMap[m_lstID[nSeq]].nCrc0 = byData;
                                                m_aSMap[m_lstID[nSeq]].nCrc1 = byData;
                                                m_aSMap[m_lstID[nSeq]].nSeq++;
                                                m_lstIndex[nSeq] = 0;
                                                //if (m_bShowReceivedIDs == true) Ojw.CMessage.Write("[Serial[{0}]:[ID:{1}]Received[Protocol={2}]", nSeq, m_lstID[nSeq], m_aSMap[m_lstID[nSeq]].nProtocol);
                                                if (m_bShowReceivedIDs == true) Ojw.CMessage.Write("[Serial[{0}]:[ID:{1}]Received[Protocol={2}]Type=0x{3}", nSeq, m_lstID[nSeq], m_aSMap[m_lstID[nSeq]].nProtocol, Ojw.CConvert.IntToHex(m_aSMap[m_lstID[nSeq]].abyMap[0] | ((m_aSMap[m_lstID[nSeq]].abyMap[1] << 8) & 0xff00), 4));
                                                if (m_bAutoset == true)
                                                {
                                                    if (m_CTmr_AutoSet.Get() > 500)
                                                        m_bAutoset = false;
                                                    else
                                                    {
                                                        m_CTmr_AutoSet.Set();
                                                        SMotors_t SMot = new SMotors_t();
                                                        SMot.nCommIndex = nSeq;
                                                        SMot.nRealID = m_lstID[nSeq];
                                                        SMot.nProtocol = m_aSMap[m_lstID[nSeq]].nProtocol;
                                                        if (m_bAutoset2 == false) m_lstSIds.Add(SMot);
                                                    }
                                                }
                                                if (m_bAutoset2 == true)
                                                {
                                                    EMotor_t EMot = GetMotorType(m_aSMap[m_lstID[nSeq]].abyMap[0] | ((m_aSMap[m_lstID[nSeq]].abyMap[1] << 8) & 0xff00));
                                                    if ((EMot != EMotor_t.NONE) && (EMot != EMotor_t.SG_90))
                                                    {
                                                        //SetParam_MotorType(m_lstID[nSeq], EMot);

                                                        SetParam(m_lstID[nSeq], m_lstID[nSeq], nSeq, 0, EMot);
                                                    }
                                                }
                                                break;
                                        }
#endif
                                    }
                                }
                                i++;
                            }
#endif

                        }

                        if ((nSeq + 1) >= m_lstSerial.Count) nSeq = 0;
                        else nSeq++;

                        Thread.Sleep(1);
                    }
                    catch (Exception ex)
                    {
                        Ojw.CMessage.Write_Error(ex.ToString());
                        if (m_lstID != null)
                            if (nSeq < m_lstID.Count)
                                m_aSMap[m_lstID[nSeq]].nStep = 0;
                        if (m_lstIndex != null)
                            if (nSeq < m_lstIndex.Count) m_lstIndex[nSeq] = 0;
                        //break;
                    }
                }

                //Ojw.CMessage.Write("[Thread_Receive] Closed Thread");
            }

        #region Kinematics
            private List<string> m_lststrForward = new List<string>();
            private List<string> m_lststrInverse = new List<string>();
            private List<CDhParamAll> m_lstCDhParamAll = new List<CDhParamAll>();
            private List<Ojw.SOjwCode_t> m_lstSCode = new List<SOjwCode_t>();
            public void InitKinematics()
            {
                m_lststrForward.Clear();
                m_lststrInverse.Clear();
                m_lstCDhParamAll.Clear();
                m_lstSCode.Clear();
            }
            public void DestroyKinematics()
            {
                m_lststrForward.Clear();
                m_lststrInverse.Clear();
                m_lstCDhParamAll.Clear();
                m_lstSCode.Clear();
            }
            public int Kinematics_Forward_GetCount() { return m_lstCDhParamAll.Count; }
            public int Kinematics_Inverse_GetCount() { return m_lstSCode.Count; }
            public int Kinematics_Forward_Add(string strForward)
            {
                if (strForward != null)
                {
                    CDhParamAll CParamAll = new CDhParamAll();
                    if (strForward.Length > 0)
                    {
                        CKinematics.CForward.MakeDhParam(strForward, out CParamAll);

                        m_lststrForward.Add(strForward);
                        m_lstCDhParamAll.Add(CParamAll);
                    }
                }
                return m_lststrForward.Count;
            }
            public int Kinematics_Inverse_Add(bool bPython, string strInverse)
            {
                if (strInverse != null)
                {
                    if (strInverse.Length > 0)
                    {
                        Ojw.SOjwCode_t SCode = new SOjwCode_t();
                        CKinematics.CInverse.Compile(((bPython == true) ? "!" : string.Empty) + strInverse, out SCode);
                        if (SCode.nMotor_Max > 0)
                        {
                            m_lststrInverse.Add(strInverse);
                            m_lstSCode.Add(SCode);
                        }
                    }
                }
                return m_lststrInverse.Count;
            }
            public int Kinematics_Forward_Set(int nFunctionNumber, string strForward)
            {
                if (strForward != null)
                {
                    CDhParamAll CParamAll = new CDhParamAll();
                    if ((strForward.Length > 0) && (nFunctionNumber < m_lststrForward.Count) && (nFunctionNumber < m_lstCDhParamAll.Count))
                    {
                        CKinematics.CForward.MakeDhParam(strForward, out CParamAll);

                        m_lststrForward[nFunctionNumber] = strForward;
                        m_lstCDhParamAll[nFunctionNumber] = CParamAll;
                    }
                }
                return m_lststrForward.Count;
            }
            public int Kinematics_Inverse_Set(int nFunctionNumber, bool bPython, string strInverse)
            {
                if (strInverse != null)
                {
                    if ((strInverse.Length > 0) && (nFunctionNumber < m_lststrInverse.Count) && (nFunctionNumber < m_lstSCode.Count))
                    {
                        Ojw.SOjwCode_t SCode = new SOjwCode_t();
                        CKinematics.CInverse.Compile(((bPython == true) ? "!" : string.Empty) + strInverse, out SCode);
                        if (SCode.nMotor_Max > 0)
                        {
                            m_lststrInverse[nFunctionNumber] = strInverse;
                            m_lstSCode[nFunctionNumber] = SCode;
                        }
                    }
                }
                return m_lststrInverse.Count;
            }

            public bool Get_Xyz(int nFunctionNumber, out double dX, out double dY, out double dZ)
            {
                float[] afMot = new float[m_nMotorCountAll];
                Array.Clear(afMot, 0, afMot.Length);
                for (int i = 0; i < afMot.Length; i++)
                {
                    afMot[i] = m_aSMot[i].fAngle_Back;//(float)CalcEvd2Angle(i, m_aSMot[i].nValue);
                }
                return Get_Xyz(nFunctionNumber, afMot, out dX, out dY, out dZ);
                //return Get_Xyz(nFunctionNumber, m_aSMot, out dX, out dY, out dZ);
            }
            public bool Get_Xyz(int nFunctionNumber, float[] afMot, out double dX, out double dY, out double dZ)
            {
                int i;
                double[] dcolX;
                double[] dcolY;
                double[] dcolZ;

                double[] adMot = new double[afMot.Length];
                Array.Clear(adMot, 0, afMot.Length);
                adMot = Ojw.CConvert.FloatsToDoubles(afMot);
                //for (i = 0; i < afMot.Length; i++) adMot[i] = (double)afMot[i];
                return Ojw.CKinematics.CForward.CalcKinematics(m_lstCDhParamAll[nFunctionNumber], adMot, out dcolX, out dcolY, out dcolZ, out dX, out dY, out dZ);
            }
            //private Ojw.C3d m_C3d = new C3d();
            public void Set_Xyz(int nFunctionNumber, double dX, double dY, double dZ)//, int nTime_Milliseconds)
            {
                int[] anMotorID = new int[256];
                double[] adValue = new double[256];
                int nCnt = GetData_Inverse(nFunctionNumber, dX, dY, dZ, out anMotorID, out adValue);
                for (int i = 0; i < nCnt; i++)
                {
                    Set(anMotorID[i], (float)adValue[i]);
                    //SetData(anMotorID[i], (float)adValue[i]);
                    //CMotor.Set_Angle(anMotorID[i], (float)adValue[i], nTime_Milliseconds);
                }
                //CMotor.Send_Motor();
            }
            private int GetData_Inverse(int nFunctionNumber, double dX, double dY, double dZ, out int[] anMotorID, out double[] adValue)
            {
                // 집어넣기 전에 내부 메모리를 클리어 한다.
                SOjwCode_t SCode = m_lstSCode[nFunctionNumber];
                Ojw.CKinematics.CInverse.SetValue_ClearAll(ref SCode);
                Ojw.CKinematics.CInverse.SetValue_X(dX);
                Ojw.CKinematics.CInverse.SetValue_Y(dY);
                Ojw.CKinematics.CInverse.SetValue_Z(dZ);

                // 현재의 모터각을 전부 집어 넣도록 한다.
                for (int i = 0; i < m_nMotorCountAll; i++)
                {
                    // 모터값을 3D에 넣어주고
                    //SetData(i, Ojw.CConvert.StrToFloat(m_txtAngle[i].Text));
                    // 그 값을 꺼내 수식 계산에 넣어준다.
                    Ojw.CKinematics.CInverse.SetValue_Motor(i, m_aSMot[i].fAngle_Back);
                }

                // 실제 수식계산
                Ojw.CKinematics.CInverse.CalcCode(ref SCode);


                m_lstSCode[nFunctionNumber] = SCode;
                // 나온 결과값을 옮긴다.
                int nMotCnt = SCode.nMotor_Max;
                if (nMotCnt <= 0)
                {
                    anMotorID = null;
                    adValue = null;
                    return 0;
                }
                anMotorID = new int[nMotCnt];
                adValue = new double[nMotCnt];
                for (int i = 0; i < nMotCnt; i++)
                {
                    anMotorID[i] = SCode.pnMotor_Number[i];
                    adValue[i] = Ojw.CKinematics.CInverse.GetValue_Motor(anMotorID[i]);

                    //Set(SCode.pnMotor_Number[i], (float)Ojw.CKinematics.CInverse.GetValue_Motor(SCode.pnMotor_Number[i]));
                }
                return nMotCnt;
            }
        #endregion Kinematics

            private void SetAddress(int nMotor, EMotor_t EMot) { SetAddress(EMot, ref m_aSAddress[nMotor]); }
            private void SetAddress(EMotor_t EMot, ref SAddress_t SAddress)
            {
                switch (EMot)
                {
        #region AX &...
                    case EMotor_t.MX_12:
                    case EMotor_t.DX_113:
                    case EMotor_t.DX_116: //
                    case EMotor_t.DX_117: //
                    case EMotor_t.RX_10: // 9~12v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시)
                    case EMotor_t.RX_24F: // 9~12v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시)
                    case EMotor_t.RX_28: // 12v~18.5v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시)
                    case EMotor_t.RX_64:
                    case EMotor_t.EX_106:
                    case EMotor_t.AX_12:
                    case EMotor_t.AX_18:
                        SAddress.nMotorNumber_0_2 = 0;        // R    0 : none
                        SAddress.nMotorNumber_Size_2 = 2;
                        SAddress.nFwVersion_6_1 = 6;          // R      
                        SAddress.nFwVersion_Size_1 = 1;

                        SAddress.nRealID_7_1 = 7;             // RW   0 ~ 252
                        SAddress.nRealID_Size_1 = 1;
                        SAddress.nBaudrate_8_1 = 8;           // RW   0-9600, 1-57600, 2-115200, 3-1M, 4-2M,  5-3M, 6-4M, 7-4.5M
                        SAddress.nBaudrate_Size_1 = 1;




                        SAddress.nTorq_64_1 = 24;              // RW   Off(0), On(1)
                        SAddress.nTorq_Size_1 = 1;
                        SAddress.nLed_65_1 = 25;               // RW   Off(0), On(1)
                        SAddress.nLed_Size_1 = 1;

                        SAddress.nMode_Operating_11_1 = 6;    // RW   1=속도제어(무한회전), 3(초기값)=위치제어(RPM), 4=Multi-Turn(-256~256:512회전), 5=전류기반 위치제어모드(-256~256:512회전), 16=PWM제어
                        SAddress.nMode_Operating_Size_1 = 4; // 속도제어시 0, 위치제어시 [6]00, [7]00, [8]ff, [9]03 ( 65283 ) 으로 셋팅해야 함. -> CW Limit 0, CCW Limit 1023

                        SAddress.nGoal_Vel_104_4 = 32;         // RW
                        SAddress.nGoal_Vel_Size_4 = 2;

                        SAddress.nProfile_Vel_112_4 = 32;      // RW
                        SAddress.nProfile_Vel_Size_4 = 2;

                        SAddress.nGoal_Pos_116_4 = 30;         // RW
                        SAddress.nGoal_Pos_Size_4 = 2;



                        // [DriveMode] 
                        //    0x01 : 정상회전(0), 역회전(1)
                        //    0x02 : 540 전용 Master(0), Slave(1)
                        //    0x04 : 속도기준 제어(0), 시간기준 제어(1)
                        //SAddress.nMode_Drive_10_1 = 10;        // RW 
                        //SAddress.nMode_Drive_Size_1 = 1;
                        //SAddress.nMode_Operating_11_1 = 11;    // RW   1=속도제어(무한회전), 3(초기값)=위치제어(RPM), 4=Multi-Turn(-256~256:512회전), 5=전류기반 위치제어모드(-256~256:512회전), 16=PWM제어
                        //SAddress.nMode_Operating_Size_1 = 1;
                        //SAddress.nProtocolVersion_13_1 = 13;   // RW   
                        //SAddress.nProtocolVersion_Size_1 = 1;
                        //SAddress.nOffset_20_4 = 20;            // RW   1회전 모드인 경우 -1024~1024 까지만 제어
                        //SAddress.nOffset_Size_4 = 4;
                        //SAddress.nLimit_PWM_36_2 = 36;         // RW   0~885 (885 = 100%)
                        //SAddress.nLimit_PWM_Size_2 = 2;
                        //SAddress.nLimit_Curr_38_2 = 38;        // RW   전류제어모드/전류기반위치제어모드에서 사용, 0~2047
                        //SAddress.nLimit_Curr_Size_2 = 2;
                        //// [Shutdown] - Reboot 으로만 해제 가능
                        ////    0x20 : 과부하
                        ////    0x10 : 전력이상
                        ////    0x08 : 엔코더 이상(Following Error)
                        ////    0x04 : 과열
                        ////    0x01 : 인가된 전압 이상
                        //SAddress.nShutdown_63_1 = 63;          // RW
                        //SAddress.nShutdown_Size_1 = 1;

                        //SAddress.nError_70_1 = 70;             // R    
                        //SAddress.nError_Size_1 = 1;
                        //SAddress.nGain_Vel_I_76_2 = 76;        // RW
                        //SAddress.nGain_Vel_I_Size_2 = 2;
                        //SAddress.nGain_Vel_P_78_2 = 78;        // RW
                        //SAddress.nGain_Vel_P_Size_2 = 2;
                        //SAddress.nGain_Pos_D_80_2 = 80;        // RW
                        //SAddress.nGain_Pos_D_Size_2 = 2;
                        //SAddress.nGain_Pos_I_82_2 = 82;        // RW
                        //SAddress.nGain_Pos_I_Size_2 = 2;
                        //SAddress.nGain_Pos_P_84_2 = 84;        // RW
                        //SAddress.nGain_Pos_P_Size_2 = 2;
                        //SAddress.nGain_Pos_F2_88_2 = 88;       // RW
                        //SAddress.nGain_Pos_F2_Size_2 = 2;
                        //SAddress.nGain_Pos_F1_90_2 = 90;       // RW
                        //SAddress.nGain_Pos_F1_Size_2 = 2;

                        //SAddress.nWatchDog_98_1 = 98;          // RW
                        //SAddress.nWatchDog_Size_1 = 1;

                        //SAddress.nGoal_PWM_100_2 = 100;         // RW   -PWMLimit ~ +PWMLimit
                        //SAddress.nGoal_PWM_Size_2 = 2;
                        //SAddress.nGoal_Current_102_2 = 102;     // RW   -CurrentLimit ~ +CurrentLimit
                        //SAddress.nGoal_Current_Size_2 = 2;

                        //SAddress.nProfile_Acc_108_4 = 108;      // RW
                        //SAddress.nProfile_Acc_Size_4 = 4;

                        //SAddress.nMoving_122_1 = 122;           // R    움직임 감지 못함(0), 움직임 감지(1)
                        //SAddress.nMoving_Size_1 = 1;
                        //// [Moving Status]
                        ////    0x30  - 0x30 : 사다리꼴 속도 프로파일
                        ////            0x20 : 삼각 속도 프로파일
                        ////            0x10 : 사각 속도 프로파일
                        ////            0x00 : 프로파일 미사용(Step)
                        ////    0x08 : Following Error
                        ////    0x02 : Goal Position 명령에 따라 진행 중
                        ////    0x01 : Inposition
                        //SAddress.nMoving_Status_123_1 = 123;    // R
                        //SAddress.nMoving_Status_Size_1 = 1;

                        //SAddress.nPresent_PWM_124_2 = 124;      // R
                        //SAddress.nPresent_PWM_Size_2 = 2;
                        //SAddress.nPresent_Curr_126_2 = 126;     // R
                        //SAddress.nPresent_Curr_Size_2 = 2;
                        //SAddress.nPresent_Vel_128_4 = 128;      // R
                        //SAddress.nPresent_Vel_Size_4 = 4;
                        //SAddress.nPresent_Pos_132_4 = 132;      // R
                        //SAddress.nPresent_Pos_Size_4 = 4;
                        //SAddress.nPresent_Volt_144_2 = 144;     // R
                        //SAddress.nPresent_Volt_Size_2 = 2;
                        //SAddress.nPresent_Temp_146_1 = 146;     // R
                        //SAddress.nPresent_Temp_Size_1 = 1;


                        /*

                        SetParam_ModelNum(nAxis, 12); // 0번지에 모델번호 12
                        SetParam_Addr_Max(nAxis, 52);
                        SetParam_Addr_Torq(nAxis, 24);
                        SetParam_Addr_Led(nAxis, 25);
                        SetParam_Addr_Mode(nAxis, 11); // 320 -> 11            [1 : 속도, 2(default) : 관절]
                        SetParam_Addr_Speed(nAxis, 32); // 320 -> 32 2 bytes
                        SetParam_Addr_Speed_Size(nAxis, 2);
                        SetParam_Addr_Pos_Speed(nAxis, 32); // 320 -> 32 2 bytes
                        SetParam_Addr_Pos_Speed_Size(nAxis, 2);
                        SetParam_Addr_Pos(nAxis, 30); // 320 -> 30 2 bytes
                        SetParam_Addr_Pos_Size(nAxis, 2);*/
                        break;
        #endregion AX
        #region XL_430 & MX Protocol 2
                    case EMotor_t.MX_28:
                    case EMotor_t.MX_64:
                    case EMotor_t.MX_106:
                    case EMotor_t.XM_540:
                    case EMotor_t.XL_430:
                        SAddress.nMotorNumber_0_2 = 0;        // R    0 : none
                        SAddress.nMotorNumber_Size_2 = 2;
                        SAddress.nFwVersion_6_1 = 6;          // R      
                        SAddress.nFwVersion_Size_1 = 1;

                        SAddress.nRealID_7_1 = 7;             // RW   0 ~ 252
                        SAddress.nRealID_Size_1 = 1;
                        SAddress.nBaudrate_8_1 = 8;           // RW   0-9600, 1-57600, 2-115200, 3-1M, 4-2M,  5-3M, 6-4M, 7-4.5M
                        SAddress.nBaudrate_Size_1 = 1;
                        // [DriveMode] 
                        //    0x01 : 정상회전(0), 역회전(1)
                        //    0x02 : 540 전용 Master(0), Slave(1)
                        //    0x04 : 속도기준 제어(0), 시간기준 제어(1)
                        SAddress.nMode_Drive_10_1 = 10;        // RW 
                        SAddress.nMode_Drive_Size_1 = 1;
                        SAddress.nMode_Operating_11_1 = 11;    // RW   1=속도제어(무한회전), 3(초기값)=위치제어(RPM), 4=Multi-Turn(-256~256:512회전), 5=전류기반 위치제어모드(-256~256:512회전), 16=PWM제어
                        SAddress.nMode_Operating_Size_1 = 1;
                        SAddress.nProtocolVersion_13_1 = 13;   // RW   
                        SAddress.nProtocolVersion_Size_1 = 1;
                        SAddress.nOffset_20_4 = 20;            // RW   1회전 모드인 경우 -1024~1024 까지만 제어
                        SAddress.nOffset_Size_4 = 4;
                        SAddress.nLimit_PWM_36_2 = 36;         // RW   0~885 (885 = 100%)
                        SAddress.nLimit_PWM_Size_2 = 2;
                        SAddress.nLimit_Curr_38_2 = 38;        // RW   전류제어모드/전류기반위치제어모드에서 사용, 0~2047
                        SAddress.nLimit_Curr_Size_2 = 2;
                        // [Shutdown] - Reboot 으로만 해제 가능
                        //    0x20 : 과부하
                        //    0x10 : 전력이상
                        //    0x08 : 엔코더 이상(Following Error)
                        //    0x04 : 과열
                        //    0x01 : 인가된 전압 이상
                        SAddress.nShutdown_63_1 = 63;          // RW
                        SAddress.nShutdown_Size_1 = 1;
                        SAddress.nTorq_64_1 = 64;              // RW   Off(0), On(1)
                        SAddress.nTorq_Size_1 = 1;
                        SAddress.nLed_65_1 = 65;               // RW   Off(0), On(1)
                        SAddress.nLed_Size_1 = 1;
                        SAddress.nError_70_1 = 70;             // R    
                        SAddress.nError_Size_1 = 1;
                        SAddress.nGain_Vel_I_76_2 = 76;        // RW
                        SAddress.nGain_Vel_I_Size_2 = 2;
                        SAddress.nGain_Vel_P_78_2 = 78;        // RW
                        SAddress.nGain_Vel_P_Size_2 = 2;
                        SAddress.nGain_Pos_D_80_2 = 80;        // RW
                        SAddress.nGain_Pos_D_Size_2 = 2;
                        SAddress.nGain_Pos_I_82_2 = 82;        // RW
                        SAddress.nGain_Pos_I_Size_2 = 2;
                        SAddress.nGain_Pos_P_84_2 = 84;        // RW
                        SAddress.nGain_Pos_P_Size_2 = 2;
                        SAddress.nGain_Pos_F2_88_2 = 88;       // RW
                        SAddress.nGain_Pos_F2_Size_2 = 2;
                        SAddress.nGain_Pos_F1_90_2 = 90;       // RW
                        SAddress.nGain_Pos_F1_Size_2 = 2;

                        SAddress.nWatchDog_98_1 = 98;          // RW
                        SAddress.nWatchDog_Size_1 = 1;

                        SAddress.nGoal_PWM_100_2 = 100;         // RW   -PWMLimit ~ +PWMLimit
                        SAddress.nGoal_PWM_Size_2 = 2;
                        SAddress.nGoal_Current_102_2 = 102;     // RW   -CurrentLimit ~ +CurrentLimit
                        SAddress.nGoal_Current_Size_2 = 2;
                        SAddress.nGoal_Vel_104_4 = 104;         // RW
                        SAddress.nGoal_Vel_Size_4 = 4;

                        SAddress.nProfile_Acc_108_4 = 108;      // RW
                        SAddress.nProfile_Acc_Size_4 = 4;
                        SAddress.nProfile_Vel_112_4 = 112;      // RW
                        SAddress.nProfile_Vel_Size_4 = 4;

                        SAddress.nGoal_Pos_116_4 = 116;         // RW
                        SAddress.nGoal_Pos_Size_4 = 4;

                        SAddress.nMoving_122_1 = 122;           // R    움직임 감지 못함(0), 움직임 감지(1)
                        SAddress.nMoving_Size_1 = 1;
                        // [Moving Status]
                        //    0x30  - 0x30 : 사다리꼴 속도 프로파일
                        //            0x20 : 삼각 속도 프로파일
                        //            0x10 : 사각 속도 프로파일
                        //            0x00 : 프로파일 미사용(Step)
                        //    0x08 : Following Error
                        //    0x02 : Goal Position 명령에 따라 진행 중
                        //    0x01 : Inposition
                        SAddress.nMoving_Status_123_1 = 123;    // R
                        SAddress.nMoving_Status_Size_1 = 1;

                        SAddress.nPresent_PWM_124_2 = 124;      // R
                        SAddress.nPresent_PWM_Size_2 = 2;
                        SAddress.nPresent_Curr_126_2 = 126;     // R
                        SAddress.nPresent_Curr_Size_2 = 2;
                        SAddress.nPresent_Vel_128_4 = 128;      // R
                        SAddress.nPresent_Vel_Size_4 = 4;
                        SAddress.nPresent_Pos_132_4 = 132;      // R
                        SAddress.nPresent_Pos_Size_4 = 4;
                        SAddress.nPresent_Volt_144_2 = 144;     // R
                        SAddress.nPresent_Volt_Size_2 = 2;
                        SAddress.nPresent_Temp_146_1 = 146;     // R
                        SAddress.nPresent_Temp_Size_1 = 1;
                        break;
        #endregion XL_430
        #region XL_320
                    case EMotor_t.XL_320:
                        SAddress.nTorq_64_1 = 24;              // RW   Off(0), On(1)                        
                        SAddress.nLed_65_1 = 25;               // RW   Off(0), On(1)

                        SAddress.nMode_Operating_11_1 = 6;    // RW   1=속도제어(무한회전), 3(초기값)=위치제어(RPM), 4=Multi-Turn(-256~256:512회전), 5=전류기반 위치제어모드(-256~256:512회전), 16=PWM제어
                        SAddress.nMode_Operating_Size_1 = 5; // 속도제어시 0, 위치제어시 [6]00, [7]00, [8]ff, [9]03 ( 65283 ) 으로 셋팅해야 함. -> CW Limit 0, CCW Limit 1023
                        SAddress.nProtocolVersion_13_1 = 13;   // RW  1 바퀴모드, 2 관절모드 (이 부분이 프로토콜 1의 다른 모터와 xl320 이 다른 부분)(아예 위 4바이트에 합쳐서 5바이트 만들어 제어)
                        SAddress.nProtocolVersion_Size_1 = 1;


                        SAddress.nGoal_Vel_104_4 = 32;         // RW
                        SAddress.nGoal_Vel_Size_4 = 2;

                        SAddress.nProfile_Vel_112_4 = 32;      // RW
                        SAddress.nProfile_Vel_Size_4 = 2;

                        SAddress.nGoal_Pos_116_4 = 30;         // RW
                        SAddress.nGoal_Pos_Size_4 = 2;
                        /*
                          SetParam_Addr_Max(nAxis, 52);
                          SetParam_Addr_Torq(nAxis, 24);
                          SetParam_Addr_Led(nAxis, 25);
                          SetParam_Addr_Mode(nAxis, 11); // 320 -> 11            [1 : 속도, 2(default) : 관절]
                        SetParam_Addr_Speed(nAxis, 32); // 320 -> 32 2 bytes
                        SetParam_Addr_Speed_Size(nAxis, 2);
                          SetParam_Addr_Pos_Speed(nAxis, 32); // 320 -> 32 2 bytes
                          SetParam_Addr_Pos_Speed_Size(nAxis, 2);
                          SetParam_Addr_Pos(nAxis, 30); // 320 -> 30 2 bytes
                          SetParam_Addr_Pos_Size(nAxis, 2);
                     */
                        SAddress.nMotorNumber_0_2 = 0;        // R    0 : none
                        SAddress.nMotorNumber_Size_2 = 2;
                        SAddress.nFwVersion_6_1 = 6;          // R      
                        SAddress.nFwVersion_Size_1 = 1;

                        SAddress.nRealID_7_1 = 7;             // RW   0 ~ 252
                        SAddress.nRealID_Size_1 = 1;
                        SAddress.nBaudrate_8_1 = 8;           // RW   0-9600, 1-57600, 2-115200, 3-1M, 4-2M,  5-3M, 6-4M, 7-4.5M
                        SAddress.nBaudrate_Size_1 = 1;
                        // [DriveMode] 
                        //    0x01 : 정상회전(0), 역회전(1)
                        //    0x02 : 540 전용 Master(0), Slave(1)
                        //    0x04 : 속도기준 제어(0), 시간기준 제어(1)
                        SAddress.nMode_Drive_10_1 = 10;        // RW 
                        SAddress.nMode_Drive_Size_1 = 1;
                        //SAddress.nMode_Operating_11_1 = 11;    // RW   1=속도제어(무한회전), 3(초기값)=위치제어(RPM), 4=Multi-Turn(-256~256:512회전), 5=전류기반 위치제어모드(-256~256:512회전), 16=PWM제어
                        //SAddress.nMode_Operating_Size_1 = 1;
                        //SAddress.nProtocolVersion_13_1 = 13;   // RW   
                        //SAddress.nProtocolVersion_Size_1 = 1;
                        SAddress.nOffset_20_4 = 20;            // RW   1회전 모드인 경우 -1024~1024 까지만 제어
                        SAddress.nOffset_Size_4 = 4;
                        SAddress.nLimit_PWM_36_2 = 36;         // RW   0~885 (885 = 100%)
                        SAddress.nLimit_PWM_Size_2 = 2;
                        SAddress.nLimit_Curr_38_2 = 38;        // RW   전류제어모드/전류기반위치제어모드에서 사용, 0~2047
                        SAddress.nLimit_Curr_Size_2 = 2;
                        // [Shutdown] - Reboot 으로만 해제 가능
                        //    0x20 : 과부하
                        //    0x10 : 전력이상
                        //    0x08 : 엔코더 이상(Following Error)
                        //    0x04 : 과열
                        //    0x01 : 인가된 전압 이상
                        SAddress.nShutdown_63_1 = 63;          // RW
                        SAddress.nShutdown_Size_1 = 1;
                        //SAddress.nTorq_64_1 = 64;              // RW   Off(0), On(1)
                        SAddress.nTorq_Size_1 = 1;
                        //SAddress.nLed_65_1 = 65;               // RW   Off(0), On(1)
                        SAddress.nLed_Size_1 = 1;
                        SAddress.nError_70_1 = 70;             // R    
                        SAddress.nError_Size_1 = 1;
                        SAddress.nGain_Vel_I_76_2 = 76;        // RW
                        SAddress.nGain_Vel_I_Size_2 = 2;
                        SAddress.nGain_Vel_P_78_2 = 78;        // RW
                        SAddress.nGain_Vel_P_Size_2 = 2;
                        SAddress.nGain_Pos_D_80_2 = 80;        // RW
                        SAddress.nGain_Pos_D_Size_2 = 2;
                        SAddress.nGain_Pos_I_82_2 = 82;        // RW
                        SAddress.nGain_Pos_I_Size_2 = 2;
                        SAddress.nGain_Pos_P_84_2 = 84;        // RW
                        SAddress.nGain_Pos_P_Size_2 = 2;
                        SAddress.nGain_Pos_F2_88_2 = 88;       // RW
                        SAddress.nGain_Pos_F2_Size_2 = 2;
                        SAddress.nGain_Pos_F1_90_2 = 90;       // RW
                        SAddress.nGain_Pos_F1_Size_2 = 2;

                        SAddress.nWatchDog_98_1 = 98;          // RW
                        SAddress.nWatchDog_Size_1 = 1;

                        SAddress.nGoal_PWM_100_2 = 100;         // RW   -PWMLimit ~ +PWMLimit
                        SAddress.nGoal_PWM_Size_2 = 2;
                        SAddress.nGoal_Current_102_2 = 102;     // RW   -CurrentLimit ~ +CurrentLimit
                        SAddress.nGoal_Current_Size_2 = 2;
                        //SAddress.nGoal_Vel_104_4 = 104;         // RW
                        //SAddress.nGoal_Vel_Size_4 = 4;

                        SAddress.nProfile_Acc_108_4 = 108;      // RW
                        SAddress.nProfile_Acc_Size_4 = 4;
                        //SAddress.nProfile_Vel_112_4 = 112;      // RW
                        //SAddress.nProfile_Vel_Size_4 = 4;

                        //SAddress.nGoal_Pos_116_4 = 116;         // RW
                        //SAddress.nGoal_Pos_Size_4 = 4;

                        SAddress.nMoving_122_1 = 122;           // R    움직임 감지 못함(0), 움직임 감지(1)
                        SAddress.nMoving_Size_1 = 1;
                        // [Moving Status]
                        //    0x30  - 0x30 : 사다리꼴 속도 프로파일
                        //            0x20 : 삼각 속도 프로파일
                        //            0x10 : 사각 속도 프로파일
                        //            0x00 : 프로파일 미사용(Step)
                        //    0x08 : Following Error
                        //    0x02 : Goal Position 명령에 따라 진행 중
                        //    0x01 : Inposition
                        SAddress.nMoving_Status_123_1 = 123;    // R
                        SAddress.nMoving_Status_Size_1 = 1;

                        SAddress.nPresent_PWM_124_2 = 124;      // R
                        SAddress.nPresent_PWM_Size_2 = 2;
                        SAddress.nPresent_Curr_126_2 = 126;     // R
                        SAddress.nPresent_Curr_Size_2 = 2;
                        SAddress.nPresent_Vel_128_4 = 128;      // R
                        SAddress.nPresent_Vel_Size_4 = 4;
                        SAddress.nPresent_Pos_132_4 = 132;      // R
                        SAddress.nPresent_Pos_Size_4 = 4;
                        SAddress.nPresent_Volt_144_2 = 144;     // R
                        SAddress.nPresent_Volt_Size_2 = 2;
                        SAddress.nPresent_Temp_146_1 = 146;     // R
                        SAddress.nPresent_Temp_Size_1 = 1;
                        break;
        #endregion XL_320
                }
            }




            public int FindMotor(int nMotor_RealID) { int i = 0; foreach (SParam_t SParam in m_aSParam) { if (SParam.nRealID == nMotor_RealID) return i; i++; } return -1; }
            public int Get_RealID(int nMotor) { return m_aSParam[nMotor].nRealID; }

        #region Parameter Function(SetParam...)
            public void SetParam_Dir(int nMotor, int nDir) { m_aSParam[nMotor].nDir = nDir; }
            public void SetParam_RealID(int nMotor, int nMotorRealID) { m_aSParam[nMotor].nRealID = nMotorRealID; }
            //public void SetParam_OperationMode(int nMotor, EOperationMode_t EOperationMode) { m_aSParam[nMotor].EOperationMode = EOperationMode; }

            public void SetParam_CommIndex(int nMotor, int nCommIndex) { m_aSParam[nMotor].nCommIndex = nCommIndex; }               // 연결 이후에 둘 중 하나만 사용 한다.(되도록  CommIndex 를 사용할 것. Commport 로 설정 하려면 통신이 접속이 되어 있어야 한다.)
            public void SetParam_CommPort(int nMotor, int nCommPort) { m_aSParam[nMotor].nCommIndex = GetSerialIndex(nCommPort); }  // CommIndex 설정보다 직관적이나 잘못 설정 될 수 있다. 연결이 안 된 경우 CommIndes 가 잘못 지정될 수가 있다.

            public void SetParam_LimitUp(int nMotor, float fLimitUp) { m_aSParam[nMotor].fLimitUp = fLimitUp; }                       // Limit - 0: Ignore 
            public void SetParam_LimitDown(int nMotor, float fLimitDn) { m_aSParam[nMotor].fLimitDn = fLimitDn; }                       // Limit - 0: Ignore 
            public void SetParam_LimitRpm(int nMotor, float fLimitRpm) { m_aSParam[nMotor].fLimitRpm = fLimitRpm; }                       // Limit - 0: Ignore 
            public void SetParam_MotorType(int nMotor, EMotor_t EMot)
            {
                SetAddress(EMot, ref m_aSAddress[nMotor]);

                m_aSParam[nMotor].EMot = EMot;

                switch (EMot)
                {
                    // 
                    case EMotor_t.SG_90:
                        break;
                    // Protocol1
                    case EMotor_t.DX_113:
                    case EMotor_t.DX_116: //
                    case EMotor_t.DX_117: //
                    case EMotor_t.RX_10: // 9~12v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시)
                    case EMotor_t.RX_24F: // 9~12v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시)
                    case EMotor_t.RX_28: // 12v~18.5v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시)
                    case EMotor_t.RX_64:
                    case EMotor_t.AX_12:
                    case EMotor_t.AX_18:
                        //m_aSParam[nMotor].bEn = true;                       // 활성화

                        //m_aSParam[nMotor].nID = nMotor;                     // 모터를 제어하기 위한 가상 아이디. 중복 허용 불가. 보통은 모터 ID 와 일치시킨다.      
                        //m_aSParam[nMotor].nRealID = nMotorRealID;           // 실제 사용할 모터의 아이디
                        //m_aSParam[nMotor].nCommIndex = nCommIndex;          // 연결 된 통신포트 중 어느것에 연결 된 것이지에 대한 순서.

                        //m_aSParam[nMotor].nDir = nDir;
                        //m_aSParam[nMotor].EMot = EMot;
                        m_aSParam[nMotor].fCenterPos = 512.0f;

                        m_aSParam[nMotor].fMechMove = 1024.0f;
                        m_aSParam[nMotor].fDegree = 300.0f;
                        m_aSParam[nMotor].fRefRpm = 0.111f;                 // 기본 rpm 단위

                        m_aSParam[nMotor].fLimitRpm = 1023f;                // 

                        m_aSParam[nMotor].fLimitUp = 0.0f;    // Limit - 0: Ignore
                        m_aSParam[nMotor].fLimitDn = 0.0f;    // Limit - 0: Ignore

                        m_aSParam[nMotor].nProtocol = 1;               // 2, 0 - protocol2, 1 - protocol1, 3 - None     

                        break;
                    case EMotor_t.EX_106:
                        //m_aSParam[nMotor].EMot = EMot;
                        m_aSParam[nMotor].fCenterPos = 2048.0f;

                        m_aSParam[nMotor].fMechMove = 4095.0f;
                        m_aSParam[nMotor].fDegree = 251.0f;
                        m_aSParam[nMotor].fRefRpm = 0.111f;                 // 기본 rpm 단위

                        m_aSParam[nMotor].fLimitRpm = 1023f;                // 

                        m_aSParam[nMotor].fLimitUp = 0.0f;    // Limit - 0: Ignore
                        m_aSParam[nMotor].fLimitDn = 0.0f;    // Limit - 0: Ignore

                        m_aSParam[nMotor].nProtocol = 1;               // 2, 0 - protocol2, 1 - protocol1, 3 - None     
                        break;
        #region MX
                    case EMotor_t.MX_12:
                        m_aSParam[nMotor].fCenterPos = 1024.0f;

                        m_aSParam[nMotor].fMechMove = 2048.0f;
                        m_aSParam[nMotor].fDegree = 360.0f;
                        m_aSParam[nMotor].fRefRpm = 0.916f;            // 기본 rpm 단위

                        m_aSParam[nMotor].fLimitRpm = 415f;                // 

                        m_aSParam[nMotor].fLimitUp = 0.0f;    // Limit - 0: Ignore
                        m_aSParam[nMotor].fLimitDn = 0.0f;    // Limit - 0: Ignore

                        m_aSParam[nMotor].nProtocol = 1;               // 2, 0 - protocol2, 1 - protocol1, 3 - None              
                        break;
                    case EMotor_t.MX_28:
                    case EMotor_t.MX_64:
                    case EMotor_t.MX_106:
                        m_aSParam[nMotor].fCenterPos = 2048.0f;

                        m_aSParam[nMotor].fMechMove = 4096.0f;
                        m_aSParam[nMotor].fDegree = 360.0f;
                        m_aSParam[nMotor].fRefRpm = 0.229f; ;//(EMot == EMotor_t.MX_12) ? 0.916f : 0.114f;  //0.229f;                 // 기본 rpm 단위

                        m_aSParam[nMotor].fLimitRpm = 415f;                // 

                        m_aSParam[nMotor].fLimitUp = 0.0f;    // Limit - 0: Ignore
                        m_aSParam[nMotor].fLimitDn = 0.0f;    // Limit - 0: Ignore

                        m_aSParam[nMotor].nProtocol = 2;               // 2, 0 - protocol2, 1 - protocol1, 3 - None              
                        break;
        #endregion MX
                    // Protocol2
                    case EMotor_t.NONE:
                    case EMotor_t.XM_540:
                    case EMotor_t.XL_430:
                        //m_aSParam[nMotor].bEn = true;                       // 활성화
                        //m_aSParam[nMotor].nModelNum = 1060;
                        //m_aSParam[nMotor].nID = nMotor;                     // 모터를 제어하기 위한 가상 아이디. 중복 허용 불가. 보통은 모터 ID 와 일치시킨다.      
                        //m_aSParam[nMotor].nRealID = nMotorRealID;           // 실제 사용할 모터의 아이디
                        //m_aSParam[nMotor].nCommIndex = nCommIndex;          // 연결 된 통신포트 중 어느것에 연결 된 것이지에 대한 순서.

                        //m_aSParam[nMotor].nDir = nDir;
                        m_aSParam[nMotor].fCenterPos = 2048.0f;

                        m_aSParam[nMotor].fMechMove = 4096.0f;
                        m_aSParam[nMotor].fDegree = 360.0f;
                        m_aSParam[nMotor].fRefRpm = 0.229f;                 // 기본 rpm 단위

                        m_aSParam[nMotor].fLimitRpm = 415f;                // 

                        m_aSParam[nMotor].fLimitUp = 0.0f;    // Limit - 0: Ignore
                        m_aSParam[nMotor].fLimitDn = 0.0f;    // Limit - 0: Ignore

                        m_aSParam[nMotor].nProtocol = 2;               // 2, 0 - protocol2, 1 - protocol1, 3 - None                             
                        break;


                    case EMotor_t.XL_320:
                        //m_aSParam[nMotor].EMot = EMot;
                        m_aSParam[nMotor].fCenterPos = 512.0f;

                        m_aSParam[nMotor].fMechMove = 1024.0f;
                        m_aSParam[nMotor].fDegree = 300.0f;
                        m_aSParam[nMotor].fRefRpm = 0.111f;                 // 기본 rpm 단위

                        m_aSParam[nMotor].fLimitRpm = 1023f;                // 

                        m_aSParam[nMotor].fLimitUp = 0.0f;    // Limit - 0: Ignore
                        m_aSParam[nMotor].fLimitDn = 0.0f;    // Limit - 0: Ignore

                        m_aSParam[nMotor].nProtocol = 2;               // 2, 0 - protocol2, 1 - protocol1, 3 - None     
                        break;
                    //case EMotor_t.XM_540:
                    //    break;
                }
            }
            public void SetParam(int nMotor, int nMotorRealID, int nCommIndex, int nDir, EMotor_t EMot)
            {
                //m_anMot_RealID[nMotor] = nMotorRealID; // ID 변경
                //m_anMot_SerialIndex[nMotor] = nCommIndex; // 통신 포트 변경
                m_aSParam[nMotor].nRealID = nMotorRealID; // ID 변경
                m_aSParam[nMotor].nCommIndex = nCommIndex; // 통신 포트 변경
                m_aSParam[nMotor].nDir = nDir;
                //m_aSParam[nMotor].EOperationMode = EOperationMode_t._Position; // Default
                //if (m_aSParam[nMotor].EOperationMode_Prev == EOperationMode_t._None) m_aSParam[nMotor].EOperationMode_Prev = m_aSParam[nMotor].EOperationMode;
                SetAddress(EMot, ref m_aSAddress[nMotor]);

#if true
                SetParam_MotorType(nMotor, EMot);
#else
                switch (EMot)
                {
                    // 
                    case EMotor_t.SG_90:
                        break;
                    // Protocol1
                    case EMotor_t.AX_12:
                    case EMotor_t.AX_18:
                        //m_aSParam[nMotor].bEn = true;                       // 활성화

                        //m_aSParam[nMotor].nID = nMotor;                     // 모터를 제어하기 위한 가상 아이디. 중복 허용 불가. 보통은 모터 ID 와 일치시킨다.      
                        //m_aSParam[nMotor].nRealID = nMotorRealID;           // 실제 사용할 모터의 아이디
                        //m_aSParam[nMotor].nCommIndex = nCommIndex;          // 연결 된 통신포트 중 어느것에 연결 된 것이지에 대한 순서.

                        m_aSParam[nMotor].nDir = nDir;
                        m_aSParam[nMotor].fCenterPos = 512.0f;

                        m_aSParam[nMotor].fMechMove = 1024.0f;
                        m_aSParam[nMotor].fDegree = 300.0f;
                        m_aSParam[nMotor].fRefRpm = 0.111f;                 // 기본 rpm 단위

                        m_aSParam[nMotor].fLimitRpm = 1023f;                // 

                        m_aSParam[nMotor].fLimitUp = 0.0f;    // Limit - 0: Ignore
                        m_aSParam[nMotor].fLimitDn = 0.0f;    // Limit - 0: Ignore

                        m_aSParam[nMotor].nProtocol = 1;               // 2, 0 - protocol2, 1 - protocol1, 3 - None     

                        break;


                    // Protocol2
                    case EMotor_t.NONE:
                    case EMotor_t.XL_430:
                        //m_aSParam[nMotor].bEn = true;                       // 활성화
                        //m_aSParam[nMotor].nModelNum = 1060;
                        //m_aSParam[nMotor].nID = nMotor;                     // 모터를 제어하기 위한 가상 아이디. 중복 허용 불가. 보통은 모터 ID 와 일치시킨다.      
                        //m_aSParam[nMotor].nRealID = nMotorRealID;           // 실제 사용할 모터의 아이디
                        //m_aSParam[nMotor].nCommIndex = nCommIndex;          // 연결 된 통신포트 중 어느것에 연결 된 것이지에 대한 순서.

                        m_aSParam[nMotor].nDir = nDir;
                        m_aSParam[nMotor].fCenterPos = 2048.0f;

                        m_aSParam[nMotor].fMechMove = 4096.0f;
                        m_aSParam[nMotor].fDegree = 360.0f;
                        m_aSParam[nMotor].fRefRpm = 0.229f;                 // 기본 rpm 단위

                        m_aSParam[nMotor].fLimitRpm = 415f;                // 

                        m_aSParam[nMotor].fLimitUp = 0.0f;    // Limit - 0: Ignore
                        m_aSParam[nMotor].fLimitDn = 0.0f;    // Limit - 0: Ignore

                        m_aSParam[nMotor].nProtocol = 2;               // 2, 0 - protocol2, 1 - protocol1, 3 - None                             
                        break;


                    case EMotor_t.XL_320:
                        break;
                    case EMotor_t.XM_540:
                        break;
                }
#if false
        #region Check
                //if (m_lstSCheckMotorType.Count > 0)
                {
                    bool bOk = false;
                    foreach (SCheckMotorType_t SType in m_lstSCheckMotorType)
                    {
                        if ((SType.EMot == EMot) && (SType.nCommIndex == nCommIndex) && (SType.nProtocol == m_aSMot_Info[nMotor].nProtocol) && (SType.nTorqAddress == m_aSMot_Info[nMotor].SAddress.nTorq_64_1))
                        {
                            bOk = true;
                            break;
                        }
                    }

                    if (bOk == false)
                    {
                        SCheckMotorType_t SType = new SCheckMotorType_t();
                        SType.nCommIndex = nCommIndex;
                        SType.EMot = EMot;
                        SType.nProtocol = m_aSMot_Info[nMotor].nProtocol;
                        SType.nTorqAddress = m_aSMot_Info[nMotor].SAddress.nTorq_64_1;
                        m_lstSCheckMotorType.Add(SType);
                    }
                }
        #endregion Check
                //m_lstSMot_Info.Add(SInfo);
                return m_aSMot_Info[nMotor];
#endif
#endif
            }
        #endregion Parameter Function(SetParam...)

        #region Protocol - basic(updateCRC, MakeStuff, SendPacket)
            private int updateCRC(byte[] data_blk_ptr, int data_blk_size)
            {
                int i, j = 0;
                int[] anCrcTable = new int[256] { 
                    0x0000, 0x8005, 0x800F, 0x000A, 0x801B, 0x001E, 0x0014, 0x8011, 0x8033, 0x0036, 0x003C, 0x8039, 0x0028, 0x802D, 0x8027, 0x0022, 0x8063, 0x0066, 0x006C, 0x8069, 0x0078, 0x807D, 0x8077, 0x0072, 0x0050, 0x8055, 0x805F, 0x005A, 0x804B, 0x004E, 0x0044, 0x8041, 0x80C3, 0x00C6, 0x00CC, 0x80C9, 0x00D8, 0x80DD, 0x80D7, 0x00D2, 
                    0x00F0, 0x80F5, 0x80FF, 0x00FA, 0x80EB, 0x00EE, 0x00E4, 0x80E1, 0x00A0, 0x80A5, 0x80AF, 0x00AA, 0x80BB, 0x00BE, 0x00B4, 0x80B1, 0x8093, 0x0096, 0x009C, 0x8099, 0x0088, 0x808D, 0x8087, 0x0082, 0x8183, 0x0186, 0x018C, 0x8189, 0x0198, 0x819D, 0x8197, 0x0192, 0x01B0, 0x81B5, 0x81BF, 0x01BA, 0x81AB, 0x01AE, 0x01A4, 0x81A1, 
                    0x01E0, 0x81E5, 0x81EF, 0x01EA, 0x81FB, 0x01FE, 0x01F4, 0x81F1, 0x81D3, 0x01D6, 0x01DC, 0x81D9, 0x01C8, 0x81CD, 0x81C7, 0x01C2, 0x0140, 0x8145, 0x814F, 0x014A, 0x815B, 0x015E, 0x0154, 0x8151, 0x8173, 0x0176, 0x017C, 0x8179, 0x0168, 0x816D, 0x8167, 0x0162, 0x8123, 0x0126, 0x012C, 0x8129, 0x0138, 0x813D, 0x8137, 0x0132,
                    0x0110, 0x8115, 0x811F, 0x011A, 0x810B, 0x010E, 0x0104, 0x8101, 0x8303, 0x0306, 0x030C, 0x8309, 0x0318, 0x831D, 0x8317, 0x0312, 0x0330, 0x8335, 0x833F, 0x033A, 0x832B, 0x032E, 0x0324, 0x8321, 0x0360, 0x8365, 0x836F, 0x036A, 0x837B, 0x037E, 0x0374, 0x8371, 0x8353, 0x0356, 0x035C, 0x8359, 0x0348, 0x834D, 0x8347, 0x0342, 
                    0x03C0, 0x83C5, 0x83CF, 0x03CA, 0x83DB, 0x03DE, 0x03D4, 0x83D1, 0x83F3, 0x03F6, 0x03FC, 0x83F9, 0x03E8, 0x83ED, 0x83E7, 0x03E2, 0x83A3, 0x03A6, 0x03AC, 0x83A9, 0x03B8, 0x83BD, 0x83B7, 0x03B2, 0x0390, 0x8395, 0x839F, 0x039A, 0x838B, 0x038E, 0x0384, 0x8381, 0x0280, 0x8285, 0x828F, 0x028A, 0x829B, 0x029E, 0x0294, 0x8291, 
                    0x82B3, 0x02B6, 0x02BC, 0x82B9, 0x02A8, 0x82AD, 0x82A7, 0x02A2, 0x82E3, 0x02E6, 0x02EC, 0x82E9, 0x02F8, 0x82FD, 0x82F7, 0x02F2, 0x02D0, 0x82D5, 0x82DF, 0x02DA, 0x82CB, 0x02CE, 0x02C4, 0x82C1, 0x8243, 0x0246, 0x024C, 0x8249, 0x0258, 0x825D, 0x8257, 0x0252, 0x0270, 0x8275, 0x827F, 0x027A, 0x826B, 0x026E, 0x0264, 0x8261, 
                    0x0220, 0x8225, 0x822F, 0x022A, 0x823B, 0x023E, 0x0234, 0x8231, 0x8213, 0x0216, 0x021C, 0x8219, 0x0208, 0x820D, 0x8207, 0x0202 };
                int nCrc_accum = 0;
                for (j = 0; j < data_blk_size; j++) nCrc_accum = (nCrc_accum << 8) ^ anCrcTable[(((nCrc_accum >> 8) ^ data_blk_ptr[j]) & 0xFF)];
                return nCrc_accum;
            }
            private void MakeStuff(ref byte[] pBuff)
            {
                int nStuff = 0;
                int[] pnIndex = new int[pBuff.Length];
                Array.Clear(pnIndex, 0, pnIndex.Length);
                int nCnt = 0;
                for (int i = 5; i < pBuff.Length; i++)
                {
                    switch (nStuff)
                    {
                        case 0: { if (pBuff[i] == 0xff) nStuff++; } break;
                        case 1: { if (pBuff[i] == 0xff) nStuff++; else nStuff = 0; } break;
                        case 2:
                            {
                                if (pBuff[i] == 0xfd)
                                {
                                    nStuff++;
                                    pnIndex[nCnt++] = i;
                                }
                                else
                                {
                                    nStuff = 0;
                                }
                            }
                            break;
                    }
                }
                if (nCnt > 0)
                {
                    byte[] pBuff2 = new byte[pBuff.Length];
                    Array.Copy(pBuff, pBuff2, pBuff.Length);
                    Array.Resize<byte>(ref pBuff, pBuff2.Length + nCnt);
                    int nIndex = 0;
                    int nPos = 0;
                    foreach (byte byTmp in pBuff)
                    {
                        pBuff[nIndex + nPos] = pBuff2[nIndex];
                        if (nIndex == pnIndex[nPos])
                        {
                            pBuff[nIndex + nPos + 1] = 0xfd;
                            nPos++;
                        }
                        nIndex++;
                    }
                    pBuff2 = null;
                }
                pnIndex = null;
            }
            private void SendPacket(int nPortIndex, byte[] buffer, int nLength) { if (m_lstSerial[nPortIndex].IsConnect() == true) m_lstSerial[nPortIndex].SendPacket(buffer, nLength); }
        #endregion Protocol - basic(updateCRC, MakeStuff, SendPacket)

        #region Packet_Raw

            public string MakePingPacket_String(int nMotorRealID, int nProtocol_Version)
            {
                byte[] pbyteBuffer = MakePingPacket(nMotorRealID, nProtocol_Version);

                string strPing = String.Empty;// "=>";
                foreach (byte byData in pbyteBuffer) strPing += String.Format(",0x{0}", Ojw.CConvert.IntToHex(byData, 2));
                return strPing;
            }
            private byte[] MakePingPacket(int nMotorRealID, int nProtocol_Version)
            {
                int i = 0;
                byte[] pbyteBuffer;
                if (nProtocol_Version == 1)
                {
                    int nLength = 2; // 파라미터의 갯수 + 2
                    int nDefaultSize = 4;
                    pbyteBuffer = new byte[nDefaultSize + nLength];
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = (byte)(nMotorRealID & 0xff);
                    pbyteBuffer[i++] = (byte)(nLength & 0xff);
                    pbyteBuffer[i++] = (byte)(0x01);

                    int nCrc = 0;
                    for (int j = 2; j < pbyteBuffer.Length - 1; j++) nCrc += pbyteBuffer[j];
                    pbyteBuffer[i++] = (byte)(~nCrc & 0xff);

                    //pbyteBuffer[pbyteBuffer.Length - 1] = (byte)(nCrc & 0xff);

                }
                else // m_aSParam_Axis[nAxis].nProtocol_Version == 2
                {
                    int nLength = 3;
                    int nDefaultSize = 7;
                    pbyteBuffer = new byte[nDefaultSize + nLength];
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = 0xff;
        #region Packet 2.0
                    pbyteBuffer[i++] = 0xfd;
                    pbyteBuffer[i++] = 0x00;
        #endregion Packet 2.0
                    pbyteBuffer[i++] = (byte)(nMotorRealID & 0xff);
                    pbyteBuffer[i++] = (byte)(nLength & 0xff);
                    pbyteBuffer[i++] = (byte)((nLength >> 8) & 0xff);
                    pbyteBuffer[i++] = (byte)(0x01);

                    MakeStuff(ref pbyteBuffer);
                    int nCrc = updateCRC(pbyteBuffer, pbyteBuffer.Length - 2);
                    pbyteBuffer[pbyteBuffer.Length - 2] = (byte)(nCrc & 0xff);
                    pbyteBuffer[pbyteBuffer.Length - 1] = (byte)((nCrc >> 8) & 0xff);

                    //SendPacket(nSerialIndex, pbyteBuffer, pbyteBuffer.Length);
                }
                return pbyteBuffer;
            }

            public void Write_Ping(int nProtocol_Version, int nSerialIndex, int nMotor)
            {
                int nID = m_aSParam[nMotor].nRealID;
                byte[] pbyteBuffer = MakePingPacket(nID, nProtocol_Version);
                SendPacket(nSerialIndex, pbyteBuffer, pbyteBuffer.Length);
            }
            public void Write_Ping() { Write_Ping(true); }
            public void Write_Ping(bool bShow)
            {
                m_bShowReceivedIDs = bShow;
                for (int nIndex = 0; nIndex < m_lstPort.Count; nIndex++)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        int nID = 254;
                        byte[] pbyteBuffer = MakePingPacket(nID, 2 - i);
                        m_CTmr_AutoSet.Set();
                        SendPacket(nIndex, pbyteBuffer, pbyteBuffer.Length);
                        Ojw.CTimer.Wait(100); // 나중에 handshake 로 바꿀것.
                    }
                }
#if false
                if (m_lstSCheckMotorType.Count > 0)
                {
                    foreach (SCheckMotorType_t SType in m_lstSCheckMotorType)
                    {
                        //int nProtocol = 2;
                        //int nCommIndex = 0;
                        int nID = 254;
                        //int nTorqAddress = 64;

                        //SetTorq(-1, bOn); 
                        byte[] pbyteBuffer = MakePingPacket(nID, SType.nProtocol);
                        SendPacket(SType.nCommIndex, pbyteBuffer, pbyteBuffer.Length);
                    }
                }
#endif
            }
            public void Write_Command(int nMotor, int nCommand) { Write_Command(m_aSParam[nMotor].nProtocol, m_aSParam[nMotor].nCommIndex, nMotor, nCommand); }
            public void Write_Command(int nProtocol_Version, int nSerialIndex, int nMotor, int nCommand)
            {
                int i;
                //int nID = ((nAxis == 254) ? 254 : m_aSMot[nAxis].nID);
                if (m_aSParam[nMotor].nProtocol == 1)
                {
                    int nLength = 1 + 2;
                    int nDefaultSize = 6;
                    byte[] pbyteBuffer = new byte[nDefaultSize + nLength];
                    i = 0;
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = (byte)(m_aSParam[nMotor].nRealID & 0xff);
                    pbyteBuffer[i++] = (byte)(nLength & 0xff);
                    pbyteBuffer[i++] = (byte)(nCommand & 0xff);

                    int nCrc = 0;
                    for (int j = 2; i < pbyteBuffer.Length - 1; j++) nCrc += pbyteBuffer[j];
                    pbyteBuffer[i++] = (byte)(~nCrc & 0xff);

                    pbyteBuffer[pbyteBuffer.Length - 1] = (byte)(nCrc & 0xff);

                    SendPacket(nSerialIndex, pbyteBuffer, pbyteBuffer.Length);
                }
                else // if (m_aSParam_Axis[nAxis].nProtocol_Version == 2)
                {
                    int nLength = 3;
                    int nDefaultSize = 7;
                    byte[] pbyteBuffer = new byte[nDefaultSize + nLength];
                    i = 0;
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = 0xfd;
                    pbyteBuffer[i++] = 0x00;
                    pbyteBuffer[i++] = (byte)(m_aSParam[nMotor].nRealID & 0xff);
                    pbyteBuffer[i++] = (byte)(nLength & 0xff);
                    pbyteBuffer[i++] = (byte)((nLength >> 8) & 0xff);
                    pbyteBuffer[i++] = (byte)(nCommand & 0xff);

                    MakeStuff(ref pbyteBuffer);
                    int nCrc = updateCRC(pbyteBuffer, pbyteBuffer.Length - 2);
                    pbyteBuffer[pbyteBuffer.Length - 2] = (byte)(nCrc & 0xff);
                    pbyteBuffer[pbyteBuffer.Length - 1] = (byte)((nCrc >> 8) & 0xff);

                    SendPacket(nSerialIndex, pbyteBuffer, pbyteBuffer.Length);
                }
            }
            public void Write(int nMotor, int nCommand, int nAddress, params byte[] pbyDatas) { Write2(m_aSParam[nMotor].nProtocol, m_aSParam[nMotor].nCommIndex, nMotor, nCommand, nAddress, pbyDatas); }
            public void Write2(int nProtocol_Version, int nSerialIndex, int nMotorRealID, int nCommand, int nAddress, params byte[] pbyDatas)
            {
                int i;

                //int nID = 0;//((nAxis == 254) ? 254 : m_aSMot[nAxis].nID);

                i = 0;
                if (nProtocol_Version == 1)
                {
                    int nLength = 2 + ((pbyDatas != null) ? pbyDatas.Length + 1 : 0); // 파라미터의 갯수 + 2
                    int nDefaultSize = 4;
                    byte[] pbyteBuffer = new byte[nDefaultSize + nLength];
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = (byte)(nMotorRealID & 0xff);
                    pbyteBuffer[i++] = (byte)(nLength & 0xff);
                    pbyteBuffer[i++] = (byte)(nCommand & 0xff);
                    if (pbyDatas != null)
                    {
                        pbyteBuffer[i++] = (byte)(nAddress & 0xff);
                        foreach (byte byData in pbyDatas) pbyteBuffer[i++] = byData;
                    }
                    int nCrc = 0;
                    for (int j = 2; j < pbyteBuffer.Length - 1; j++) nCrc += pbyteBuffer[j];
                    pbyteBuffer[i++] = (byte)(~nCrc & 0xff);

                    SendPacket(nSerialIndex, pbyteBuffer, pbyteBuffer.Length);
                }
                else // m_aSParam_Axis[nAxis].nProtocol_Version == 2
                {
                    int nLength = 3 + 2 + pbyDatas.Length;
                    int nDefaultSize = 7;
                    byte[] pbyteBuffer = new byte[nDefaultSize + nLength];
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = 0xff;
        #region Packet 2.0
                    pbyteBuffer[i++] = 0xfd;
                    pbyteBuffer[i++] = 0x00;
        #endregion Packet 2.0
                    pbyteBuffer[i++] = (byte)(nMotorRealID & 0xff);
                    pbyteBuffer[i++] = (byte)(nLength & 0xff);
                    pbyteBuffer[i++] = (byte)((nLength >> 8) & 0xff);
                    pbyteBuffer[i++] = (byte)(nCommand & 0xff);
                    pbyteBuffer[i++] = (byte)(nAddress & 0xff);
                    pbyteBuffer[i++] = (byte)((nAddress >> 8) & 0xff);
                    if (pbyDatas != null)
                        foreach (byte byData in pbyDatas) pbyteBuffer[i++] = byData;

                    MakeStuff(ref pbyteBuffer);
                    int nCrc = updateCRC(pbyteBuffer, pbyteBuffer.Length - 2);
                    pbyteBuffer[pbyteBuffer.Length - 2] = (byte)(nCrc & 0xff);
                    pbyteBuffer[pbyteBuffer.Length - 1] = (byte)((nCrc >> 8) & 0xff);

                    SendPacket(nSerialIndex, pbyteBuffer, pbyteBuffer.Length);
                }
            }
            public void Writes(int nProtocol_Version, int nSerialIndex, int nAddress, int nDataLength_without_ID, int nMotorCnt, params byte[] pbyDatas)
            {
                int i = 0;
                int nLength = (nDataLength_without_ID + 1) * nMotorCnt;
                if (nProtocol_Version != 1) // xl_320 은 따로 해주도록 한다. 나중에...
                {
                    byte[] pbyteBuffer = new byte[2 + nLength];
                    pbyteBuffer[i++] = (byte)(nDataLength_without_ID & 0xff);
                    pbyteBuffer[i++] = (byte)((nDataLength_without_ID >> 8) & 0xff);
                    Array.Copy(pbyDatas, 0, pbyteBuffer, i, nLength);
                    Write2(nProtocol_Version, nSerialIndex, 0xfe, 0x83, nAddress, pbyteBuffer);
                    pbyteBuffer = null;
                }
                else // (== 1)
                {
                    // Instruction(1) + Address(2) + Data_Length(2) + 
                    byte[] pbyteBuffer = new byte[1 + nLength];
                    pbyteBuffer[i++] = (byte)(nDataLength_without_ID & 0xff);
                    Array.Copy(pbyDatas, 0, pbyteBuffer, i, nLength);
                    Write2(nProtocol_Version, nSerialIndex, 0xfe, 0x83, nAddress, pbyteBuffer);
                    pbyteBuffer = null;
                }
            }
#if true
#else
            public void Writes(int nProtocol_Version, int nSerialIndex, int nAddress, int nDataLength_without_ID, int nMotorCnt, params byte[] pbyDatas)
            {
                int i = 0;
                int nLength = (nDataLength_without_ID + 1) * nMotorCnt;
                if (nProtocol_Version != 1)
                {
                    byte[] pbyteBuffer = new byte[2 + nLength];
                    pbyteBuffer[i++] = (byte)(nDataLength_without_ID & 0xff);
                    pbyteBuffer[i++] = (byte)((nDataLength_without_ID >> 8) & 0xff);
                    Array.Copy(pbyDatas, 0, pbyteBuffer, i, nLength);
                    Write2(nProtocol_Version, nSerialIndex, 0xfe, 0x83, nAddress, pbyteBuffer);
                    pbyteBuffer = null;
                }
                else // (== 1)
                {
                    // Instruction(1) + Address(2) + Data_Length(2) + 
                    byte[] pbyteBuffer = new byte[1 + nLength];
                    pbyteBuffer[i++] = (byte)(nDataLength_without_ID & 0xff);
                    Array.Copy(pbyDatas, 0, pbyteBuffer, i, nLength);
                    Write2(nProtocol_Version, nSerialIndex, 0xfe, 0x83, nAddress, pbyteBuffer);
                    pbyteBuffer = null;
                }
            }
            public void Writes(int nProtocol_Version, int nSerialIndex, int nAddress, int nDataLength_without_ID, int nMotorCnt, params byte[] pbyDatas)
            {
                int i = 0;
                int nLength = (nDataLength_without_ID + 1) * nMotorCnt;
                if (nProtocol_Version != 1)
                {
                    byte[] pbyteBuffer = new byte[2 + nLength];
                    pbyteBuffer[i++] = (byte)(nDataLength_without_ID & 0xff);
                    pbyteBuffer[i++] = (byte)((nDataLength_without_ID >> 8) & 0xff);
                    Array.Copy(pbyDatas, 0, pbyteBuffer, i, nLength);
                    Write(0xfe, 0x83, nAddress, pbyteBuffer);
                    pbyteBuffer = null;
                }
                else
                {
                    byte[] pbyteBuffer = new byte[1 + nLength];
                    pbyteBuffer[i++] = (byte)(nDataLength_without_ID & 0xff);
                    Array.Copy(pbyDatas, 0, pbyteBuffer, i, nLength);
                    Write(0xfe, 0x83, nAddress, pbyteBuffer);
                    pbyteBuffer = null;
                }
            }
            // int nMotor, int nSpeed
            public void Writes_Speed(params int [] anMotor_and_Speed)
            {
                int nCount = anMotor_and_Speed.Length / 2;
                byte[] pbyTmp_Short;
                for (int i = 0; i < nCount; i++)
                {
                    // 속도

                    pbyTmp_Short = Ojw.CConvert.ShortToBytes((short)anMotor_and_Speed[i * 2 + 1]);
                Array.Copy(pbyTmp_Short, 0, pbyteBuffer_Spd, nIndex_Spd, pbyTmp_Short.Length);
            }
#endif
            public void Reads(int nSerialIndex, int nAddress, int nDataLength, params byte[] pbyIDs)
            {
                int nProtocol_Version = 2; // 프로토콜 2 버전부터 싱크리드가 지원
                byte[] pbyteBuffer = new byte[2 + pbyIDs.Length];
                Array.Copy(Ojw.CConvert.ShortToBytes((short)nDataLength), pbyteBuffer, 2);
                Array.Copy(pbyIDs, 0, pbyteBuffer, 2, pbyIDs.Length);
                Write2(nProtocol_Version, nSerialIndex, 0xfe, 0x82, nAddress, pbyteBuffer);
            }
            public void Reads(int nSerialIndex, int nAddress, int nDataLength, params int[] pnIDs)
            {
                int nProtocol_Version = 2; // 프로토콜 2 버전부터 싱크리드가 지원
                byte[] pbyteBuffer = new byte[2 + pnIDs.Length];
                int i = 2;
                Array.Copy(Ojw.CConvert.ShortToBytes((short)nDataLength), pbyteBuffer, i);
                foreach (int nData in pnIDs) pbyteBuffer[i++] = (byte)(nData & 0xff);
                Write2(nProtocol_Version, nSerialIndex, 0xfe, 0x82, nAddress, pbyteBuffer);
            }
        #endregion Packet_Raw

        #region Control
            public void SetTorq(int nMotor, bool bOn)
            {
                if (bOn) { if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return; }
                if (bOn == false) m_aSMot[nMotor].bInit_Value = false;

                m_aSMot[nMotor].nStatus_Torq_Prev = m_aSMot[nMotor].nStatus_Torq;
                m_aSMot[nMotor].nStatus_Torq = Ojw.CConvert.BoolToInt(bOn);
                //if (nMotor < 0) 

                if (m_aSParam[nMotor].EMot != EMotor_t.SG_90)
                {
                    Write2(m_aSParam[nMotor].nProtocol, m_aSParam[nMotor].nCommIndex, ((nMotor < 0) ? 254 : (m_aSParam[nMotor].nRealID)), 0x03, m_aSAddress[nMotor].nTorq_64_1, (byte)((bOn == true) ? 1 : 0));

                }
            }
            public void SetTorq(bool bOn)
            {
                if (bOn) { if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return; }
                for (int i = 0; i < m_aSParam.Length; i++) m_aSMot[i].bInit_Value = false;
                for (int i = 0; i < m_lstPort.Count; i++)
                {
                    // protocol 1(AX)
                    Write2(1, i, 254, 0x03, _ADDRESS_TORQ_AX, (byte)((bOn == true) ? 1 : 0));
                    // protocol 2(XL-430
                    Write2(2, i, 254, 0x03, _ADDRESS_TORQ_XL_430, (byte)((bOn == true) ? 1 : 0));
                }
            }
        #region 같은 프로토콜, 같은 시리얼포트, 같은 종류의 모터로만 구성해야 한다.
            public void SetTorq(bool bOn, params int[] anMotors)
            {
                if (bOn) { if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return; }
                byte[] pbyteBuffer = new byte[anMotors.Length * 2];
                int i = 0;
                byte byVal = (byte)((bOn == true) ? 1 : 0);
                foreach (int nMotor in anMotors)
                {
                    pbyteBuffer[i++] = (byte)(Get_RealID(nMotor) & 0xff);
                    pbyteBuffer[i++] = byVal;
                }
                if (anMotors.Length > 0)
                {
                    int nProtocol = m_aSParam[anMotors[0]].nProtocol;
                    int nSerialIndex = m_aSParam[anMotors[0]].nCommIndex;
                    Writes(nProtocol, nSerialIndex, m_aSAddress[anMotors[0]].nTorq_64_1, m_aSAddress[anMotors[0]].nTorq_Size_1, anMotors.Length, pbyteBuffer);
                }
            }
            //public void SetTorq(bool bOn, params int[] nMotor)
            //{
            //    for (int nCommand = 0; nCommand < 3; nCommand++)
            //    {
            //        for (int nSerialIndex = 0; nSerialIndex < m_lstPort.Count; nSerialIndex++)
            //        {
            //            for (int nProtocol = 1; nProtocol <= 2; nProtocol++)
            //            {
            //                Writes(nProtocol, nSerialIndex, nAddress, nAddress_Size, nMotorPtCount, m_abyBuffer);
            //            }
            //        }
            //    }
            //}
            public void SetTorqOn(params int[] anMotors) { SetTorq(true, anMotors); }
            public void SetTorqOff(params int[] anMotors) { SetTorq(false, anMotors); }
        #endregion 같은 프로토콜, 같은 시리얼포트, 같은 종류의 모터로만 구성해야 한다.

            //  Move  ///////////////////////////
            private bool m_bIgnoredLimit = false;
            public void SetLimitEn(bool bOn) { m_bIgnoredLimit = !bOn; }
            public bool GetLimitEn() { return !m_bIgnoredLimit; }
            public int Clip(int nLimitValue_Up, int nLimitValue_Dn, int nData)
            {
                if (GetLimitEn() == false) return nData;

                int nRet = ((nData > nLimitValue_Up) ? nLimitValue_Up : nData);
                return ((nRet < nLimitValue_Dn) ? nLimitValue_Dn : nRet);
            }
            public float Clip(float fLimitValue_Up, float fLimitValue_Dn, float fData)
            {
                if (GetLimitEn() == false) return fData;
                float fRet = ((fData > fLimitValue_Up) ? fLimitValue_Up : fData);
                return ((fRet < fLimitValue_Dn) ? fLimitValue_Dn : fRet);
            }

            public int CalcLimit_Evd(int nMotor, int nValue)
            {
                //if (Get_Flag_Mode(nAxis) == 0)// || (Get_Flag_Mode(nAxis) == 2))
                //{
                int nPulse = nValue;// &0x4000;
                //if (m_bMultiTurn == false)
                //{
                //nValue &= 0x4000;
                //nValue &= 0x3fff;
                //}

                //nValue &= 0x3fff;
                int nUp = 10000000;
                int nDn = -nUp;
                if (m_aSParam[nMotor].fLimitUp != 0) nUp = CalcAngle2Evd(nMotor, m_aSParam[nMotor].fLimitUp);
                if (m_aSParam[nMotor].fLimitDn != 0) nDn = CalcAngle2Evd(nMotor, m_aSParam[nMotor].fLimitDn);
                if (nUp < nDn) { int nTmp = nUp; nUp = nDn; nDn = nTmp; }
                return (Clip(nUp, nDn, nValue) | nPulse);
                //}
                //return nValue;
            }
            public float CalcLimit_Angle(int nMotor, float fValue)
            {
                //if (Get_Flag_Mode(nAxis) == 0)// || (Get_Flag_Mode(nAxis) == 2))
                //{
                float fUp = 1000000.0f;
                float fDn = -fUp;
                if (m_aSParam[nMotor].fLimitUp != 0) fUp = m_aSParam[nMotor].fLimitUp;
                if (m_aSParam[nMotor].fLimitDn != 0) fDn = m_aSParam[nMotor].fLimitDn;
                return Clip(fUp, fDn, fValue);
                //}
                //return fValue;
            }
            public int CalcAngle2Evd(int nMotor, float fValue)
            {
                fValue *= ((m_aSParam[nMotor].nDir == 0) ? 1.0f : -1.0f);
                int nData = 0;
                //if (Get_Flag_Mode(nAxis) != 0)   // 속도제어
                //{
                //    nData = (int)Math.Round(fValue);
                //    //Ojw.CMessage.Write("Speed Turn");
                //}
                //else
                //{
                // 위치제어
                nData = (int)Math.Round((m_aSParam[nMotor].fMechMove * fValue) / m_aSParam[nMotor].fDegree);
                nData = nData + (int)Math.Round(m_aSParam[nMotor].fCenterPos);
                //}
                return nData;
            }
            public float CalcEvd2Angle(int nMotor, int nValue)
            {
                float fValue = ((m_aSParam[nMotor].nDir == 0) ? 1.0f : -1.0f);
                float fValue2 = 0.0f;
                //if (Get_Flag_Mode(nMotor) != 0)   // 속도제어
                //    fValue2 = (float)nValue * fValue;
                //else                                // 위치제어
                //{
                fValue2 = (float)(((m_aSParam[nMotor].fDegree * ((float)(nValue - (int)Math.Round(m_aSParam[nMotor].fCenterPos)))) / m_aSParam[nMotor].fMechMove) * fValue);
                //}
                return fValue2;
            }
            public void Set_OperationMode(int nMotor, EOperationMode_t EOper)
            {
                m_aSMot[nMotor].EOperationMode = EOper;
                m_aSMot[nMotor].bOperationMode = true;
            }
            //public void Set_Evd(int nMotor, int nEvd, float fRpm)
            //{
            //}
            //public void Set_Evd(int nMotor, int nEvd)
            //{
            //    if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return;
            //    Push(nMotor);
            //    //Read_Motor_Push(nAxis);
            //    m_aSMot[nMotor].EEnable = EEnable_t._Position;
            //    //m_aSMot[nAxis].
            //    //Set_Flag_Mode(nAxis, false);
            //    if (m_aSMot[nMotor].EOperationMode_Prev != m_aSMot[nMotor].EOperationMode) m_aSMot[nMotor].bOperationMode = true;

            //    m_aSMot[nMotor].nValue = (long)CalcLimit_Evd(nMotor, nEvd);
            //    //Set_Flag_NoAction(nAxis, false);
            //    //Push_Id(nAxis);	
            //}
            //public long Get_Evd(int nMotor) { return m_aSMot[nMotor].nValue; }


            // [목표]
            //////////////////////////////////// 전송 단계가 아닌 Set의 단계에서 프로토콜을 적재하도록 수정하자.
            //////////////////////////////////// SyncWrite 가 아닌 BulkWrite 를 사용하자.(아직...)

            public void Set(int nMotor, float fAngle, float fRpm)
            {
                if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return;
                Push(nMotor);
                // Protocol1, XL-320 을 바퀴냐 포지션이냐만 결정하자.

                m_aSMot[nMotor].EEnable = EEnable_t._Position;
                if (m_aSMot[nMotor].EOperationMode_Prev != m_aSMot[nMotor].EOperationMode) m_aSMot[nMotor].bOperationMode = true;
                m_aSMot[nMotor].nValue = CalcLimit_Evd(nMotor, CalcAngle2Evd(nMotor, fAngle));
                m_aSMot[nMotor].bSetValue2 = true; // Rpm 을 따로 설정한 경우만 Set 한다.
                m_aSMot[nMotor].nValue2 = CalcRpm2Raw(nMotor, fRpm);
                m_aSMot[nMotor].fAngle_Back = CalcEvd2Angle(nMotor, m_aSMot[nMotor].nValue);
            }
            public void Set(int nMotor, float fAngle)
            {
                if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return;
                Push(nMotor);
                m_aSMot[nMotor].EEnable = EEnable_t._Position;
                Set_OperationMode(nMotor, EOperationMode_t._Position);
                if (m_aSMot[nMotor].EOperationMode_Prev != m_aSMot[nMotor].EOperationMode) m_aSMot[nMotor].bOperationMode = true;

                m_aSMot[nMotor].nValue = CalcLimit_Evd(nMotor, CalcAngle2Evd(nMotor, fAngle));
                m_aSMot[nMotor].fAngle_Back = CalcEvd2Angle(nMotor, m_aSMot[nMotor].nValue);
            }
            public float Get(int nMotor) { return m_aSMot[nMotor].fAngle_Back; }
            //public float Get(int nMotor) { return CalcEvd2Angle(nMotor, (int)m_aSMot[nMotor].nValue); }

            //private List<byte> m_lstPt = new List<byte>();
            private byte[] m_abyBuffer = new byte[256];
            private byte[] m_abyBuffer_Speed = new byte[256];
            private int m_nBuffer = 0;
            private int m_nBuffer_Speed = 0;
            private bool IsBitMinus(Ojw.CMonster.EMotor_t EMotor)
            {
                bool bRet = false;
                if (EMotor == EMotor_t.AX_12)
                {
                }
                return bRet;
            }
            public void Set_Turn(int nMotor, float fRpm)
            {
                if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return;
                Push(nMotor);
                m_aSMot[nMotor].EEnable = EEnable_t._Speed;
                Set_OperationMode(nMotor, EOperationMode_t._Speed);
                if (m_aSMot[nMotor].EOperationMode_Prev != m_aSMot[nMotor].EOperationMode) m_aSMot[nMotor].bOperationMode = true;
                m_aSMot[nMotor].bSetValue2 = true; // Rpm 을 따로 설정한 경우만 Set 한다.
                m_aSMot[nMotor].nValue2 = CalcRpm2Raw(nMotor, fRpm);
            }
            // float Get(int nMotor) { return CalcEvd2Angle(nMotor, (int)m_aSMot[nMotor].nValue); }
            private List<int> m_lstMotors = new List<int>();
            public void Send_Motor() { Send_Motor(0); }
            public void Send_Motor(int nMilliseconds)
            {
                if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return;
#if false
                if (m_nProtocolVersion == 1)
                    {
                        switch (nMode)
                        {
                            case 0: Writes(nAddr_Spd, nAddr_Size, nCnt2, pbyteBuffer_Spd); Writes(nAddr_Pos, nAddr_Size, nCnt2, pbyteBuffer_Pos); break;
                            case 1: Writes(nAddr_Spd, nAddr_Size, nCnt2, pbyteBuffer_Spd); break;
                            //case 2: Writes(102, 4, nCnt, pbyteBuffer); break;
                            default: break;
                        }
                    }
                    else
                    {
                        
                        switch (nMode)
                        {
                            case 0: Writes(nAddr_Pos_Spd, nAddr_Size, nCnt2, pbyteBuffer_Spd); Writes(nAddr_Pos, nAddr_Size, nCnt2, pbyteBuffer_Pos); break;
                            case 1: Writes(nAddr_Spd, nAddr_Size, nCnt2, pbyteBuffer_Spd); break;
                            //case 2: Writes(102, 4, nCnt, pbyteBuffer); break;
                            default: break;
                        }
                    }
#endif
                //int nAddr = 0;
                int nMotorPtCount = 0;
                int nMotorPtCount_Speed = 0;
                int nMotor = 0;
                int nAddress;
                int nAddress_Size;
                int nAddress_Speed = -1;
                int nAddress_Speed_Size = -1;
                int nMotorCnt = m_nMotorCnt;
                byte[] buffer = new byte[4];
                // Setting - Operation
                for (int nCommand = 0; nCommand < 3; nCommand++)
                {
                    for (int nSerialIndex = 0; nSerialIndex < m_lstPort.Count; nSerialIndex++)
                    {
                        for (int nProtocol = 1; nProtocol <= 2; nProtocol++)
                        {
                            m_lstMotors.Clear();
                            m_nBuffer = 0;
                            nMotorPtCount = 0;
                            nAddress = -1;
                            nAddress_Size = 0;

                            m_nBuffer_Speed = 0;
                            nMotorPtCount_Speed = 0;
                            nAddress_Speed = -1;
                            nAddress_Speed_Size = -1;
                            for (int i = 0; i < nMotorCnt; i++)
                            {
                                //if ((nCommand == 2) && (nProtocol == 2)) nMotor = Pop(); // 별 의미 없긴 하지만... 일단 Pop() 으로 비워주어야 하니...
                                //else nMotor = m_anEn[i];
                                nMotor = m_anEn[i];
                                if ((m_aSParam[nMotor].nProtocol == nProtocol) && (m_aSParam[nMotor].nCommIndex == nSerialIndex))
                                {
                                    //if (m_aSParam[nMotor].EMot == EMotor_t.XL_320) // XL_320 은 아직 논외로 친다.

                                    switch (nCommand)
                                    {
                                        case 0:
                                            if (m_aSMot[nMotor].bOperationMode == true)
                                            {
                                                if (nAddress < 0)
                                                {
                                                    nAddress = m_aSAddress[nMotor].nMode_Operating_11_1;
                                                    nAddress_Size = m_aSAddress[nMotor].nMode_Operating_Size_1;
                                                }

                                                if ((m_aSParam[nMotor].EMot == EMotor_t.AX_12) || (m_aSParam[nMotor].EMot == EMotor_t.AX_18) || (m_aSParam[nMotor].EMot == EMotor_t.XL_320))
                                                {
                                                    m_abyBuffer[m_nBuffer++] = (byte)Get_RealID(nMotor);
                                                    if ((m_aSMot[nMotor].EOperationMode == EOperationMode_t._Position) || (m_aSMot[nMotor].EOperationMode == EOperationMode_t._Position_Multi) || (m_aSMot[nMotor].EOperationMode == EOperationMode_t._Position_Amp))
                                                    {
                                                        m_abyBuffer[m_nBuffer++] = 0x00;
                                                        m_abyBuffer[m_nBuffer++] = 0x00;
                                                        m_abyBuffer[m_nBuffer++] = 0xff;
                                                        m_abyBuffer[m_nBuffer++] = 0x03;
                                                        if (m_aSParam[nMotor].EMot == EMotor_t.XL_320) m_abyBuffer[m_nBuffer++] = 0x02;
                                                    }
                                                    else
                                                    {
                                                        m_abyBuffer[m_nBuffer++] = 0x00;
                                                        m_abyBuffer[m_nBuffer++] = 0x00;
                                                        m_abyBuffer[m_nBuffer++] = 0x00;
                                                        m_abyBuffer[m_nBuffer++] = 0x00;
                                                        if (m_aSParam[nMotor].EMot == EMotor_t.XL_320) m_abyBuffer[m_nBuffer++] = 0x01;
                                                    }
                                                }
                                                else
                                                {
                                                    m_abyBuffer[m_nBuffer++] = (byte)Get_RealID(nMotor);
                                                    m_abyBuffer[m_nBuffer++] = (byte)m_aSMot[nMotor].EOperationMode;
                                                }


                                                m_lstMotors.Add(nMotor);

                                                nMotorPtCount++;
                                            }
                                            break;
                                        case 1:
                                            bool bSet = false;
                                            //bool bSpeed = false;
                                            if (nMilliseconds > 0)
                                            {
                                                if (
                                                     (m_aSMot[nMotor].EOperationMode == EOperationMode_t._Position) ||
                                                     (m_aSMot[nMotor].EOperationMode == EOperationMode_t._Position_Multi) ||
                                                     (m_aSMot[nMotor].EOperationMode == EOperationMode_t._Position_Amp)
                                                    )
                                                {
                                                    float fRpm = (float)Math.Abs(CalcTime2Rpm(CalcEvd2Angle(nMotor, m_aSMot[nMotor].nValue) - CalcEvd2Angle(nMotor, m_aSMot[nMotor].nValue_Prev), (float)nMilliseconds));
                                                    if (fRpm > 1)//!= 0)//> 1)
                                                    {
                                                        bSet = true;
                                                        m_aSMot[nMotor].nValue2 = CalcRpm2Raw(nMotor, fRpm); // short
                                                    }
                                                }
                                            }
                                            if ((m_aSMot[nMotor].bSetValue2 == true) || (bSet == true))
                                            {
                                                if (m_aSMot[nMotor].EEnable == EEnable_t._Position)
                                                {
                                                    if (nAddress < 0)
                                                    {
                                                        nAddress = m_aSAddress[nMotor].nProfile_Vel_112_4;
                                                        nAddress_Size = m_aSAddress[nMotor].nProfile_Vel_Size_4;
                                                    }
                                                    m_abyBuffer[m_nBuffer++] = (byte)Get_RealID(nMotor);

                                                    if (nAddress_Size == 1)
                                                        m_abyBuffer[m_nBuffer++] = (byte)(m_aSMot[nMotor].nValue2 & 0xff);
                                                    else if (nAddress_Size == 2)
                                                    {
                                                        buffer = Ojw.CConvert.ShortToBytes((short)m_aSMot[nMotor].nValue2);
                                                        Array.Copy(buffer, 0, m_abyBuffer, m_nBuffer, nAddress_Size);
                                                        m_nBuffer += nAddress_Size;
                                                    }
                                                    else //(nAddress_Size == 4)
                                                    {
                                                        buffer = Ojw.CConvert.IntToBytes((int)m_aSMot[nMotor].nValue2);
                                                        Array.Copy(buffer, 0, m_abyBuffer, m_nBuffer, nAddress_Size);
                                                        m_nBuffer += nAddress_Size;
                                                    }
                                                    nMotorPtCount++;
                                                }
                                                else if (m_aSMot[nMotor].EEnable == EEnable_t._Speed)
                                                {
                                                    if (nAddress_Speed < 0)
                                                    {
                                                        nAddress_Speed = m_aSAddress[nMotor].nGoal_Vel_104_4; ;
                                                        nAddress_Speed_Size = m_aSAddress[nMotor].nGoal_Vel_Size_4;
                                                    }
                                                    m_abyBuffer_Speed[m_nBuffer_Speed++] = (byte)Get_RealID(nMotor);

                                                    if (nAddress_Speed_Size == 1)
                                                        m_abyBuffer_Speed[m_nBuffer_Speed++] = (byte)(m_aSMot[nMotor].nValue2 & 0xff);
                                                    else if (nAddress_Speed_Size == 2)
                                                    {
                                                        buffer = Ojw.CConvert.ShortToBytes((short)m_aSMot[nMotor].nValue2);
                                                        Array.Copy(buffer, 0, m_abyBuffer_Speed, m_nBuffer_Speed, nAddress_Speed_Size);
                                                        m_nBuffer_Speed += nAddress_Speed_Size;
                                                    }
                                                    else //(nAddress_Speed_Size == 4)
                                                    {
                                                        buffer = Ojw.CConvert.IntToBytes((int)Math.Abs(m_aSMot[nMotor].nValue2) & 0x3ff | (m_aSMot[nMotor].nValue2 < 0 ? 0x400 : 0x00)); // 10 번째 비트가 방향 비트
                                                        Array.Copy(buffer, 0, m_abyBuffer_Speed, m_nBuffer_Speed, nAddress_Speed_Size);
                                                        m_nBuffer_Speed += nAddress_Speed_Size;
                                                    }
                                                    nMotorPtCount_Speed++;
                                                }
                                            }
                                            break;
                                        case 2:
                                            // 여길 속도 모드면 들어오지 않도록 나중에 고칠 것.
                                            // 현재는 bSetValue2 == true 여도 들어오게 되어 있다.
                                            if (
                                                 (m_aSMot[nMotor].EOperationMode == EOperationMode_t._Position) ||
                                                 (m_aSMot[nMotor].EOperationMode == EOperationMode_t._Position_Multi) ||
                                                 (m_aSMot[nMotor].EOperationMode == EOperationMode_t._Position_Amp)
                                                )
                                            {
                                                if (nAddress < 0)
                                                {
                                                    nAddress = m_aSAddress[nMotor].nGoal_Pos_116_4;
                                                    nAddress_Size = m_aSAddress[nMotor].nGoal_Pos_Size_4;
                                                }
                                                m_abyBuffer[m_nBuffer++] = (byte)Get_RealID(nMotor);
                                                if (nAddress_Size == 1)
                                                    m_abyBuffer[m_nBuffer++] = (byte)m_aSMot[nMotor].nValue;//EOperationMode;
                                                else if (nAddress_Size == 2)
                                                {
                                                    buffer = Ojw.CConvert.ShortToBytes((short)m_aSMot[nMotor].nValue);
                                                    Array.Copy(buffer, 0, m_abyBuffer, m_nBuffer, nAddress_Size);
                                                    m_nBuffer += nAddress_Size;
                                                }
                                                else //(nAddress_Size == 4)
                                                {
                                                    buffer = Ojw.CConvert.IntToBytes((int)m_aSMot[nMotor].nValue);
                                                    Array.Copy(buffer, 0, m_abyBuffer, m_nBuffer, nAddress_Size);
                                                    m_nBuffer += nAddress_Size;
                                                }
                                                nMotorPtCount++;
                                            }
#if false // Speed 는 이미 앞전에 셋팅이 되었기 때문에 여기서는 필요 없다.
                                            else if (m_aSMot[nMotor].EOperationMode == EOperationMode_t._Speed)
                                            {
                                                if (nAddress < 0)
                                                {
                                                    nAddress = m_aSAddress[nMotor].nGoal_Vel_104_4;
                                                    nAddress_Size = m_aSAddress[nMotor].nGoal_Vel_Size_4;
                                                }
                                                m_abyBuffer[m_nBuffer++] = (byte)Get_RealID(nMotor);

                                                int nVal = (m_aSMot[nMotor].bSetValue2 == true) ? m_aSMot[nMotor].nValue2 : m_aSMot[nMotor].nValue;
                                                if (nAddress_Size == 1)
                                                    m_abyBuffer[m_nBuffer++] = (byte)nVal;
                                                else if (nAddress_Size == 2)
                                                {
                                                    // 이때는 10 비트가 부호비트. -> 나중에 고쳐 둘것.
                                                    buffer = Ojw.CConvert.ShortToBytes((short)nVal);
                                                    Array.Copy(buffer, 0, m_abyBuffer, m_nBuffer, nAddress_Size);
                                                    m_nBuffer += nAddress_Size;
                                                }
                                                else //(nAddress_Size == 4)
                                                {
                                                    buffer = Ojw.CConvert.IntToBytes((int)nVal);
                                                    Array.Copy(buffer, 0, m_abyBuffer, m_nBuffer, nAddress_Size);
                                                    m_nBuffer += nAddress_Size;
                                                }

                                                nMotorPtCount++;
                                            }
#endif
                                            break;
                                    }
                                    //nMotorPtCount++;
                                }
                                //else // SG-90
                                //{
                                //}
                            }
                            if (nMotorPtCount > 0)
                            {
                                Ojw.CTimer CTmr = new CTimer();
                                if (nCommand == 0)
                                {
                                    //           CTmr.Set(); while (CTmr.Get() < 50) Thread.Sleep(1);
                                    SetTorqOff(m_lstMotors.ToArray());
                                }
                                //        CTmr.Set(); while (CTmr.Get() < 50) Thread.Sleep(1);//Ojw.CTimer.Wait(1);
                                Writes(nProtocol, nSerialIndex, nAddress, nAddress_Size, nMotorPtCount, m_abyBuffer);
                                //        CTmr.Set(); while (CTmr.Get() < 10) Thread.Sleep(1); //Ojw.CTimer.Wait(1);
                                //if (nCommand == 2)
                                //{
                                //    m_aSMot
                                //}

                                if (nCommand == 0)
                                {
                                    SetTorqOn(m_lstMotors.ToArray()); // 나중에는 토크가 기존에 살아 있던 것만 골라 살도록 한다.
                                }
                            }
                            if (nMotorPtCount_Speed > 0)
                            {
                                Writes(nProtocol, nSerialIndex, nAddress_Speed, nAddress_Speed_Size, nMotorPtCount_Speed, m_abyBuffer_Speed);
                            }
                        }
                    }
                }

                for (int i = 0; i < nMotorCnt; i++)
                {
                    nMotor = m_anEn[i];
                    Done(nMotor);
                    //Done(Pop());
                }
                //Pop()
                m_nMotorCnt = 0;
                m_nWait = nMilliseconds;
            }

        #endregion Control
            int m_nWait = 0;
            public void Delay(int nMilliseconds)
            {
                CTimer CTmr = new CTimer();
                CTmr.Set(); while (CTmr.Get() < nMilliseconds) Ojw.CTimer.Wait(1);
            }
            public void Wait()
            {
                if (m_nWait <= 0) return;
                Wait(m_nWait);
            }
            public void Wait_Per(float fPercent_0_1)
            {
                Wait((int)Math.Round((float)m_nWait * fPercent_0_1));
            }
            public void Wait(int nMilliseconds)
            {
                CTimer CTmr = new CTimer();
                CTmr.Set(); while (CTmr.Get() < nMilliseconds) Ojw.CTimer.Wait(1);
            }


        #region MotionFile
            // Set3D 하면 모델링에서 가져온 상태로 강제 재셋팅, -> PlayFile(파일이름) 하면 된다.
            private Ojw.C3d m_C3d = null;
            private Ojw.C3d.COjwDesignerHeader m_CHeader = null;//new C3d.COjwDesignerHeader();
            public void SetHeader(Ojw.C3d.COjwDesignerHeader CHeader)
            {
                m_CHeader = CHeader;
                for (int i = 0; i < m_CHeader.nMotorCnt; i++)
                {
                    // 0 : None, 1 : xl-320, 2 : xl_430(Default), 3 - ax-12
                    if (m_CHeader.pSMotorInfo[i].nHwMotorName == 2)
                    {
                        SetParam_MotorType(i, EMotor_t.XL_430);
                        SetParam_Dir(i, m_CHeader.pSMotorInfo[i].nMotorDir);
                    }
                    else if (m_CHeader.pSMotorInfo[i].nHwMotorName == 3)
                    {
                        SetParam_MotorType(i, EMotor_t.AX_12);
                        //SetParam_RealID
                        SetParam_Dir(i, m_CHeader.pSMotorInfo[i].nMotorDir);
                    }

                    //m_CHeader.pSMotorInfo[i].fLimit_Up,
                    //m_CHeader.pSMotorInfo[i].fLimit_Down,
                    //(float)m_CHeader.pSMotorInfo[i].nCenter_Evd,
                    //0,
                    //(float)m_CHeader.pSMotorInfo[i].nMechMove,
                    //m_CHeader.pSMotorInfo[i].fMechAngle);
                }
            }
            public void Set3D(Ojw.C3d C3dModel) { m_C3d = C3dModel; SetHeader(m_C3d.GetHeader()); }
            private const int _SIZE_MOTOR_MAX = 999;
            public void PlayFrame(int nLine, SMotion_t SMotion)
            {
                if (SMotion.nFrameSize <= 0) return;
                if ((nLine < 0) || (nLine >= SMotion.nFrameSize)) return;

                if ((m_bStop == false) && (m_bEms == false) && (m_bMotionEnd == false))
                {
                    //m_bStop = false; 
                    //for (int i = 0; i < _SIZE_MOTOR_MAX; i++) Set_Flag_Stop(i, false);
                    SetTorq(true);

                    for (int nAxis = 0; nAxis < m_nMotorCnt; nAxis++)
                    {
                        if (m_CHeader.pSMotorInfo[nAxis].nMotorControlType != 0) // 위치제어가 아니라면
                        {
                            //SetParam_Dir(nAxis, m_CHeader.pSMotorInfo[nAxis].nMotorDir);
                            Set_Turn(nAxis, SMotion.STable[nLine].anMot[nAxis]);
                        }
                        else
                        {
                            SetParam_Dir(nAxis, m_CHeader.pSMotorInfo[nAxis].nMotorDir);

                            Set(nAxis, SMotion.STable[nLine].anMot[nAxis]);
                        }
                    }
                    Send_Motor(SMotion.STable[nLine].nTime);
                }
            }
            public void PlayFrame(SMotionTable_t STable)
            {
                if ((m_bStop == false) && (m_bEms == false) && (m_bMotionEnd == false))
                {
                    //m_bStop = false; 
                    //for (int i = 0; i < _SIZE_MOTOR_MAX; i++) Set_Flag_Stop(i, false);
                    SetTorq(true);
                    for (int nAxis = 0; nAxis < m_CHeader.nMotorCnt; nAxis++)
                    {
                        if (m_CHeader.pSMotorInfo[nAxis].nMotorControlType != 0) // 위치제어가 아니라면
                        {
                            //SetParam_Dir(nAxis, m_CHeader.pSMotorInfo[nAxis].nMotorDir);
                            //Set_Flag_Led(nAxis,
                            //    Get_Flag_Led_Green(STable.anLed[nAxis]),
                            //    Get_Flag_Led_Blue(STable.anLed[nAxis]),
                            //    Get_Flag_Led_Red(STable.anLed[nAxis])
                            //    );

                            Set_Turn(nAxis, STable.anMot[nAxis]);
                        }
                        else
                        {
                            //SetParam_Dir(nAxis, m_CHeader.pSMotorInfo[nAxis].nMotorDir);
                            //Set_Flag_Led(nAxis,
                            //    Get_Flag_Led_Green(STable.anLed[nAxis]),
                            //    Get_Flag_Led_Blue(STable.anLed[nAxis]),
                            //    Get_Flag_Led_Red(STable.anLed[nAxis])
                            //    );

                            Set(nAxis, STable.anMot[nAxis]);
                        }
                    }
                    Send_Motor(STable.nTime);
                }
            }
            public void PlayFile(string strFileName)
            {
                try
                {
                    if (m_C3d == null) return;
                    SMotion_t SMotion = new SMotion_t();
                    if (m_C3d.BinaryFileOpen(strFileName, out SMotion) == true)
                    {
                        if (SMotion.nFrameSize > 0)
                        {
                            m_bStart = true;

                            m_C3d.WaitAction_SetTimer();

                            foreach (SMotionTable_t STable in SMotion.STable)
                            {
                                if (STable.bEn == true)
                                {
                                    PlayFrame(STable);

                                    int nDelay = STable.nTime + STable.nDelay;
                                    if (nDelay > 0) m_C3d.WaitAction_ByTimer(nDelay);
                                }
                            }
                            m_bStart = false;
                            m_bMotionEnd = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Ojw.CMessage.Write_Error("Error -> PlayMotion(), " + ex.ToString());
                }
            }
        #endregion MotionFile

        #region Background
            private void Push(int nMotor) { if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return; if (IsCmd(nMotor) == false) m_anEn[m_nMotorCnt++] = nMotor; }
            private int Pop() { if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return -1; if (m_nMotorCnt > 0) return m_anEn[--m_nMotorCnt]; return -1; }
            public bool IsCmd(int nMotor) { for (int i = 0; i < m_nMotorCnt; i++) if (m_anEn[i] == nMotor) return true; return false; }

            private float CalcRaw2Rpm(int nMotor, int nValue) { return (float)nValue * m_aSParam[nMotor].fRefRpm; }
            private int CalcRpm2Raw(int nMotor, float fRpm) { return (int)Math.Round(Clip(m_aSParam[nMotor].fLimitRpm, 0, fRpm / m_aSParam[nMotor].fRefRpm)); }

            private float CalcTime2Rpm(float fDeltaAngle, float fTime)
            {
                //if (fDeltaAngle == 0) fDeltaAngle = (float)CMath._ZERO;
        #region Kor
                // 1도 이동시간 => fTime / fDeltaAngle

                // 60 초 동안 _MAX_RPM 을 회전하는 것이 RPM, 1도 움직이는 것을 체크하려면 여기에 360 도 회전값을 고려해 주어야 한다.
                // _MAX_RPM 은 1분(60초) 동안 _MAX_RPM 바퀴 (즉, 360 * _MAX_RPM) 를 회전한 값
                // _MAX_RPM * 360 : 60 seconds => 480(_MAX_EV_RPM) 일때 
                // => 1초간 6 * _MAX_RPM 도 회전, 1ms => _MAX_RPM * 360도 / 60000ms = 0.65952 도 이동
                // => 1도 움직이는데 60000 / (_MAX_RPM * 360) = 1.516254246 ms 가 필요

                // 1도 이동시간 => 60000 / (Rpm * 360)
                // 이동각도 당 이동시간 계산 => 1도 이동시간 * fDeltaAngle
        #endregion Kor
                //return 60000 / (fTime / fDeltaAngle * 360.0f);
                return (60.0f * fDeltaAngle * 1000.0f) / (360.0f * fTime);
            }
        #endregion Background
        }
#else
        public class CMonster
        {
        #region Define
            // 모터의 수량은 256개 한정으로 정한다.(고정) -> 추후 가변으로 바꿀지는 완성 이후 고민
            private const int _CNT = 256;
            private int m_nMotorCountAll = _CNT; // _CNT 를 바로 사용하지 말고 이 변수를 사용하도록 한다.(추후 확장성)
            private const int _ADDRESS_TORQ_XL_320 = 24;
            private const int _ADDRESS_TORQ_XL_430 = 64;
            private const int _ADDRESS_TORQ_AX = 24;
            public enum EMotor_t
            {
                NONE = 0,
                // Default
                XL_430 = 20, // 360도 4096 : 2048 center : 0.229 RefRpm, 480 Limit Rpm
                XL_320 = 21, // 300도 1024 : 512 center : 0.111 RefRpm, 1023 Limit Rpm

                XM_540 = 30, // LED [65], 360도 4096 : 2048 center : 0.229 RefRpm, 480 Limit Rpm  , 확장위치제어모드시 512 회전 가능(+-256)

                AX_12 = 1, // 9~12v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시)
                AX_18 = 2, //


                DX_113 = 3, //
                DX_116 = 4, //
                DX_117 = 5, //
                RX_10 = 6, // 9~12v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시)
                RX_24F = 7, // 9~12v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시)
                RX_28 = 8, // 12v~18.5v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시)
                RX_64 = 9, // 12v~18.5v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시)
                EX_106 = 10, // 12v~18.5v, 0~251도, c=2048, mx=4095, mn=0, 


                // protocol 2.0
                MX_12 = 11, // 10~14.8v, 0~360, c=2048, mx=4095, mn=0, drpm = 0.916rpm, 다중회전가능(각 7바퀴:-28,672~28,672)
                MX_28 = 12, // 10~14.8v, 0~360, c=2048, mx=4095, mn=0, drpm = 0.114rpm, 다중회전가능(각 7바퀴:-28,672~28,672)  
                MX_64 = 13, // 10~14.8v, 0~360, c=2048, mx=4095, mn=0, drpm = 0.114rpm, 다중회전가능(각 7바퀴:-28,672~28,672)  
                MX_106 = 14, // 10~14.8v, 0~360, c=2048, mx=4095, mn=0, drpm = 0.114rpm, 다중회전가능(각 7바퀴:-28,672~28,672)  
                //MX_ = 15, //

                SG_90 = 100
            }

        #endregion Define

        #region Structure
            public struct SAddress_t
            {
        #region 확장기능 // Table Address - 여기부터는 값을 가지는게 아닌 주소번지만 가지도록...
                public int nMotorNumber_0_2;        // R    0 : none
                public int nMotorNumber_Size_2;
                public int nFwVersion_6_1;          // R      
                public int nFwVersion_Size_1;
                public int nRealID_7_1;             // RW   0 ~ 252
                public int nRealID_Size_1;
                public int nBaudrate_8_1;           // RW   0-9600, 1-57600, 2-115200, 3-1M, 4-2M,  5-3M, 6-4M, 7-4.5M
                public int nBaudrate_Size_1;
                // [DriveMode] 
                //    0x01 : 정상회전(0), 역회전(1)
                //    0x02 : 540 전용 Master(0), Slave(1)
                //    0x04 : 속도기준 제어(0), 시간기준 제어(1)
                public int nMode_Drive_10_1;        // RW 
                public int nMode_Drive_Size_1;
                public int nMode_Operating_11_1;    // RW   1=속도제어(무한회전), 3(초기값)=위치제어(RPM), 4=Multi-Turn(-256~256:512회전), 5=전류기반 위치제어모드(-256~256:512회전), 16=PWM제어
                public int nMode_Operating_Size_1;
                public int nProtocolVersion_13_1;   // RW   
                public int nProtocolVersion_Size_1;
                public int nOffset_20_4;            // RW   1회전 모드인 경우 -1024~1024 까지만 제어
                public int nOffset_Size_4;
                public int nLimit_PWM_36_2;         // RW   0~885 (885 = 100%)
                public int nLimit_PWM_Size_2;
                public int nLimit_Curr_38_2;        // RW   전류제어모드/전류기반위치제어모드에서 사용, 0~2047
                public int nLimit_Curr_Size_2;
                // [Shutdown] - Reboot 으로만 해제 가능
                //    0x20 : 과부하
                //    0x10 : 전력이상
                //    0x08 : 엔코더 이상(Following Error)
                //    0x04 : 과열
                //    0x01 : 인가된 전압 이상
                public int nShutdown_63_1;          // RW
                public int nShutdown_Size_1;
                public int nTorq_64_1;              // RW   Off(0), On(1)
                public int nTorq_Size_1;
                public int nLed_65_1;               // RW   Off(0), On(1)
                public int nLed_Size_1;
                public int nError_70_1;             // R    
                public int nError_Size_1;
                public int nGain_Vel_I_76_2;        // RW
                public int nGain_Vel_I_Size_2;
                public int nGain_Vel_P_78_2;        // RW
                public int nGain_Vel_P_Size_2;
                public int nGain_Pos_D_80_2;        // RW
                public int nGain_Pos_D_Size_2;
                public int nGain_Pos_I_82_2;        // RW
                public int nGain_Pos_I_Size_2;
                public int nGain_Pos_P_84_2;        // RW
                public int nGain_Pos_P_Size_2;
                public int nGain_Pos_F2_88_2;       // RW
                public int nGain_Pos_F2_Size_2;
                public int nGain_Pos_F1_90_2;       // RW
                public int nGain_Pos_F1_Size_2;

                public int nWatchDog_98_1;          // RW
                public int nWatchDog_Size_1;

                public int nGoal_PWM_100_2;         // RW   -PWMLimit ~ +PWMLimit
                public int nGoal_PWM_Size_2;
                public int nGoal_Current_102_2;     // RW   -CurrentLimit ~ +CurrentLimit
                public int nGoal_Current_Size_2;
                public int nGoal_Vel_104_4;         // RW
                public int nGoal_Vel_Size_4;

                public int nProfile_Acc_108_4;      // RW
                public int nProfile_Acc_Size_4;
                public int nProfile_Vel_112_4;      // RW
                public int nProfile_Vel_Size_4;

                public int nGoal_Pos_116_4;         // RW
                public int nGoal_Pos_Size_4;

                public int nMoving_122_1;           // R    움직임 감지 못함(0), 움직임 감지(1)
                public int nMoving_Size_1;
                // [Moving Status]
                //    0x30  - 0x30 : 사다리꼴 속도 프로파일
                //            0x20 : 삼각 속도 프로파일
                //            0x10 : 사각 속도 프로파일
                //            0x00 : 프로파일 미사용(Step)
                //    0x08 : Following Error
                //    0x02 : Goal Position 명령에 따라 진행 중
                //    0x01 : Inposition
                public int nMoving_Status_123_1;    // R
                public int nMoving_Status_Size_1;

                public int nPresent_PWM_124_2;      // R
                public int nPresent_PWM_Size_2;
                public int nPresent_Curr_126_2;     // R
                public int nPresent_Curr_Size_2;
                public int nPresent_Vel_128_4;      // R
                public int nPresent_Vel_Size_4;
                public int nPresent_Pos_132_4;      // R
                public int nPresent_Pos_Size_4;
                public int nPresent_Volt_144_2;     // R
                public int nPresent_Volt_Size_2;
                public int nPresent_Temp_146_1;     // R
                public int nPresent_Temp_Size_1;
        #endregion 확장기능
            }
            public struct SParam_t
            {
                //public int nID;                     // 모터를 제어하기 위한 가상 아이디. 중복 허용 불가. 보통은 모터 ID 와 일치시킨다.            
                public int nCommIndex;              // 연결 된 통신포트 중 어느것에 연결 된 것이지에 대한 순서.
                public int nRealID;
                public EMotor_t EMot;

                public int nDir;
                //Center
                public float fCenterPos;
                
                public float fMechMove;
                public float fDegree;
                public float fRefRpm;

                public float fLimitRpm;     // ?

                public float fLimitUp;    // Limit - 0: Ignore
                public float fLimitDn;    // Limit - 0: Ignore
                
                //public EOperationMode_t EOperationMode;          // 2 - 전류제어모드(실제 제어에서는 0을 사용), 1 - 속도제어모드(바퀴제어), 3(default) - 위치제어모드, 4 - 확장위치제어모드(X시리즈의 경우 -256 ~ +256회전 지원), 5  -전류기반 위치제어모드(X시리즈의 경우 -256 ~ +256회전 지원), 16 - PWM 제어모드
                //public EOperationMode_t EOperationMode_Prev;
                public int nProtocol;               // 2, 0 - protocol2, 1 - protocol1, 3 - None   
            }
            // 0 - 전류제어모드(이거 2로 바꾼다), 1 - 속도제어모드(바퀴제어), 3(default) - 위치제어모드, 4 - 확장위치제어모드(X시리즈의 경우 -256 ~ +256회전 지원), 5  -전류기반 위치제어모드(X시리즈의 경우 -256 ~ +256회전 지원), 16 - PWM 제어모드
            public enum EOperationMode_t
            {
                _None = 0,
                _Amp = 2,
                _Speed = 1,
                _Position = 3,
                _Position_Multi = 4,
                _Position_Amp = 5,
                _Pwm = 16
            }
            public enum EEnable_t
            {
                _None = 0,
                //_Disable = 0,
                _Amp = 2,
                _Speed = 1,
                _Position = 3,
                _Pwm = 16
            }
            public struct SMap_t
            {
                public int nSeq;
                public int nSeq_Back;

                public int nStep;
                public int nPortIndex;
                public int nProtocol;

                public int nAddress;
                public int nAddress_DataLength;

                public int nID;
                public int nLength;
                public int nCmd;
                public int nError;
                public int nModelNumber;
                public int nFw;
                public int nCrc0;
                public int nCrc1;
                public EMotor_t EMotor;
                                
                public byte[] abyMap;
            }

            public struct SMot_t
            {
                public EEnable_t EEnable; // 1 : Pos, 2 : Speed, 3 : Packet
                //public int nOperationMode;
                //public int nOperationMode_Prev;

                public bool bInit_Value;
                // position
                public int nValue;
                public int nValue_Prev;
                public float fAngle_Back;
                // speed
                public int nValue2;
                public bool bSetValue2;
                public int nValue2_Prev;

                public int nStatus_Torq;
                public int nStatus_Torq_Prev;
                public int nStatus_Error;
                public int nStatus_Error_Prev;

                public bool bOperationMode;
                public EOperationMode_t EOperationMode;          // 2 - 전류제어모드(실제 제어에서는 0을 사용), 1 - 속도제어모드(바퀴제어), 3(default) - 위치제어모드, 4 - 확장위치제어모드(X시리즈의 경우 -256 ~ +256회전 지원), 5  -전류기반 위치제어모드(X시리즈의 경우 -256 ~ +256회전 지원), 16 - PWM 제어모드
                public EOperationMode_t EOperationMode_Prev;

                public Ojw.CTimer CTmr;
                public byte[] abyBuffer;
            }
        #endregion Structure


        #region Var
            private List<CSerial> m_lstSerial = null;//new List<CSerial>();
            private List<int> m_lstIndex = new List<int>();
            private List<int> m_lstID = new List<int>();
            private Thread m_thReceive = null;
            private List<int> m_lstPort = null;

            private bool m_bProgEnd = false;
            private bool m_bStop = false;
            private bool m_bEms = false;
            private bool m_bMotionEnd = false;
            private bool m_bStart = false;

            //private int[] m_anMot_SerialIndex = new int[_CNT];
            //private int[] m_anMot_RealID = new int[_CNT];
            //private EMotor_t[] m_aEMot_Type = new EMotor_t[_CNT];
            private SAddress_t[] m_aSAddress = new SAddress_t[_CNT];
            private SParam_t[] m_aSParam = new SParam_t[_CNT];
            
            private int m_nMotorCnt = 0;// { get { return m_lstSCommand.Count; } }
            private int[] m_anEn = new int[256]; // push / pop

            private SMot_t[] m_aSMot = new SMot_t[_CNT];
            //private List<SMap_t> m_lstSMap = new List<SMap_t>();//[_CNT];
            //private SMap_t[] m_aSMap_New = new SMap_t[_CNT];
            private SMap_t[] m_aSMap = new SMap_t[_CNT];
            //private SMap_t[] m_aSMap_Old = new SMap_t[_CNT];
            //private int [] m_anMap_Address = new int[_CNT];


            

        #endregion Var
            private struct SMotors_t
            {
                public int nRealID;
                public int nCommIndex;
                public int nProtocol;
                public EMotor_t EMotor;
            }
            private List<SMotors_t> m_lstSIds = new List<SMotors_t>();
            private bool m_bAutoset = false;
            private bool m_bAutoset2 = false;
            private Ojw.CTimer m_CTmr_AutoSet = new CTimer();
            public void AutoSet() { AutoSet(true, true); }
            public void AutoSet(bool bDisplay, bool bDisplay_Receive)
            {
                m_bShowReceivedIDs = false;
                m_bAutoset = true;
                m_bAutoset2 = false;
                m_lstSIds.Clear();
                Write_Ping();
                //m_CTmr_AutoSet.Set();
                //while ((m_CTmr_AutoSet.Get() < 100) && (m_bProgEnd == false)) Thread.Sleep(1);//Ojw.CTimer.Wait(1); ///Thread.Sleep(1);// Ojw.CTimer.Wait();
                if (m_bProgEnd == true) return;
                else
                {
                    m_bAutoset2 = true;
                    // 
                    m_bShowReceivedIDs = bDisplay_Receive;
                    for (int i = 0; i < m_lstSerial.Count; i++)
                    { 
                        // 프로토콜 2의 아이디만 수집한다. (싱크리드 명령이 되니까...)
                        List<int> lstIds = new List<int>();
                        lstIds.Clear();
                        foreach (SMotors_t SMot in m_lstSIds)
                        {
                            if (SMot.nCommIndex == i)
                            {
                                //if ((SMot.nProtocol == 2) && (SMot.nCommIndex == i))
                                if (SMot.nProtocol == 2)
                                    lstIds.Add(SMot.nRealID);
                                else
                                {
                                    m_CTmr_AutoSet.Set();
                                    Write2(1, i, SMot.nRealID, 0x02, 0, 50);
                                    Ojw.CTimer.Wait(50);
                                    //m_CTmr_AutoSet.Set();
                                    //while ((m_CTmr_AutoSet.Get() < 200) && (m_bProgEnd == false)) Thread.Sleep(1);//Ojw.CTimer.Wait(1); ///Thread.Sleep(1); //나중에 받으면 넘어가도록 만들 것.
                                }
                            }
                        }
                        if (lstIds.Count > 0)
                        {
                            m_CTmr_AutoSet.Set();
                            Reads(i, 0, 50, lstIds.ToArray()); // 일단 50 까지만 읽는다.(xl-320 때문에...) 나중에 XL-320 이 아니라면 147 까지 읽어온다.
                            Ojw.CTimer.Wait(50);
                            //m_CTmr_AutoSet.Set();
                            //while ((m_CTmr_AutoSet.Get() < 300) && (m_bProgEnd == false)) Thread.Sleep(1);//Ojw.CTimer.Wait(1); ///Thread.Sleep(1);
                        }                        
                        // 
                    }
                    ///for(m_lstSIds
                    if (bDisplay)
                    {
                        Ojw.CMessage.Write2("Motors=>\r\n");
                        foreach (SMotors_t SMot in m_lstSIds)
                        {
                            int nMotor = FindMotor(SMot.nRealID);
                            Ojw.CMessage.Write2("[{0}]-{1}\r\n", nMotor, m_aSParam[nMotor].EMot);

                        }
                    }
                }
            }
            private EMotor_t GetMotorType(int nHwNum)
            {
                switch (nHwNum)
                {
                    case 113: return EMotor_t.DX_113;
                    case 116: return EMotor_t.DX_116;
                    case 117: return EMotor_t.DX_117;
                    case 18: return EMotor_t.AX_18;
                    case 44:
                    case 12: return EMotor_t.AX_12;
                    //case 300: return EMotor_t.AX_12;
                    case 10: return EMotor_t.RX_10;
                    case 24: return EMotor_t.RX_24F;
                    case 28: return EMotor_t.RX_28;
                    case 64: return EMotor_t.RX_64;
                    case 107: return EMotor_t.EX_106;
                    case 360: return EMotor_t.MX_12;
                        ////////////////////////////////////
                        //여기부터 2.0
                    case 30: return EMotor_t.MX_28;
                    case 311: return EMotor_t.MX_64;
                    case 321: return EMotor_t.MX_106;
                    case 350: return EMotor_t.XL_320;
                    case 1060: return EMotor_t.XL_430;
                }
                return EMotor_t.NONE; ;
            }
            private bool m_bShowReceivedIDs = false;
            // 초기화
            private void Init()
            {
                for (int i = 0; i < m_nMotorCountAll; i++)
                {
                    SetParam(i, i, 0, 0, EMotor_t.XL_430);
                    //    m_anMot_SerialIndex[i] = 0;
                    //    m_anMot_RealID[i] = i;
                    //    m_aEMot_Type[i] = EMotor_t.XL_430;
                    //    SetAddress(i, EMotor_t.XL_430);
                    m_aSMot[i].bInit_Value = false;
                    m_aSMot[i].EEnable = EEnable_t._None;
                    m_aSMot[i].CTmr = new CTimer();
                    m_aSMot[i].nStatus_Torq = 0;
                    m_aSMot[i].nValue = 0;
                    m_aSMot[i].nValue_Prev = 0;
                    m_aSMot[i].fAngle_Back = 0.0f; // 초기화 때만 클리어 하고 남겨두는 데이타
                    m_aSMot[i].bSetValue2 = false;
                    m_aSMot[i].nValue2 = 0;
                    m_aSMot[i].nValue2_Prev = 0;
                    m_aSMot[i].nStatus_Error = 0;
                    m_aSMot[i].bOperationMode = false;
                    m_aSMot[i].EOperationMode = EOperationMode_t._Position; // Default

                    m_aSMap[i] = new SMap_t();
                    m_aSMap[i].abyMap = new byte[256];
                    //m_aSMap_Old[i] = new SMap_t();
                    //m_aSMap_Old[i].abyMap = new byte[200];
                    //m_anMap_Address[i] = -1;
                }
                //Array.Clear(m_aSMap, 0, m_aSMap.Length);
                InitKinematics();
            }
            private void Done(int nMotor)
            {
                m_aSMot[nMotor].EEnable = EEnable_t._None;
                m_aSMot[nMotor].bOperationMode = false;

                //m_aSMot[nMotor].CTmr = new CTimer();
                if (m_bEms == true) m_aSMot[nMotor].bInit_Value = false;
                m_aSMot[nMotor].nValue_Prev = m_aSMot[nMotor].nValue;
                m_aSMot[nMotor].nValue2_Prev = m_aSMot[nMotor].nValue2;
                //m_aSMot[nMotor].nStatus_Error = 0;
                //m_aSMot[nMotor].nOperationMode = 0;
                m_aSMot[nMotor].EOperationMode_Prev = m_aSMot[nMotor].EOperationMode;
                m_aSMot[nMotor].bSetValue2 = false;
                m_aSMot[nMotor].CTmr.Set();
            }
            public CMonster()
            {
                Init();                
            }
            ~CMonster() 
            {
                Close();
            }

            public void Stop(int nMotor) // no stop flag setting
            {
                byte[] pbyTmp = Ojw.CConvert.IntToBytes(0);
                //Write(nAxis, 104, pbyTmp);
                if (m_aSParam[nMotor].EMot != EMotor_t.SG_90)
                {
                    Write2(m_aSParam[nMotor].nProtocol, m_aSParam[nMotor].nCommIndex, ((nMotor < 0) ? 254 : (m_aSParam[nMotor].nRealID)), 0x03, m_aSAddress[nMotor].nGoal_Vel_104_4, 0);

                }                
            }
            public void Stop()
            {
                byte[] pbyTmp = Ojw.CConvert.IntToBytes(0);
                //Write(nAxis, 104, pbyTmp);
                for (int i = 0; i < GetSerialCount(); i++)
                {
                    //if (m_aSParam[nMotor].EMot != EMotor_t.SG_90)
                    //{
                    // 속도
                    Write2(1, i, 254, 0x03, 32, 0);
                    Write2(2, i, 254, 0x03, 104, 0);

                    //}
                }
                m_bStop = true;
            }
            public void Ems()
            {
                Stop();
                SetTorq(false);
                m_bEms = true;
            }
            public void Reset()//(int nAxis)
            {
                // Clear Variable
                m_bStop = false;
                m_bEms = false;
            }
            public void Reboot() { Reboot(-1); }//_ID_BROADCASTING); }
            public void Reboot(int nMotor)
            {
                if ((nMotor < 0) || (nMotor == 254))
                {
                    //byte[] pbyTmp = Ojw.CConvert.IntToBytes(0);
                    //Write(nAxis, 104, pbyTmp);
                    for (int i = 0; i < GetSerialCount(); i++)
                    {
                        //if (m_aSParam[nMotor].EMot != EMotor_t.SG_90)
                        //{
                        // 속도
                        Write_Command(1, i, ((nMotor < 0) ? 254 : nMotor), 0x08);
                        Write_Command(2, i, ((nMotor < 0) ? 254 : nMotor), 0x08);

                        //}
                    }
                    //Clear_Flag();
                    m_bStop = false;
                    m_bEms = false;
                }
                else Write_Command(m_aSParam[nMotor].nProtocol, m_aSParam[nMotor].nCommIndex, nMotor, 0x08);
                // Initialize variable
               
                //m_nMotorCnt_Back = m_nMotorCnt = 0;

            }
            public bool IsStop() { return m_bStop; }
            public bool IsEms() { return m_bEms; }

        #region Open / Close, IsOpen, GetSerial, GetSerialIndex, GetSerialPort, GetSerialCount
            // 해당 컴포트가 열렸는지를 확인
            public bool IsOpen(int nPort)
            {
                if (m_lstPort != null)
                {
                    foreach (int nSerialPort in m_lstPort) if (nSerialPort == nPort) return true;
                }
                return false;
            }
            // 해당 포트의 시리얼 핸들을 리턴
            public CSerial GetSerial(int nPort)
            {
                if (m_lstPort != null)
                {
                    for (int i = 0; i < m_lstPort.Count; i++)
                    {
                        if (m_lstPort[i] == nPort) return m_lstSerial[i];
                    }
                }
                return null;
            }
            // 해당 포트의 인덱스 번호를 리턴
            public int GetSerialIndex(int nPort)
            {
                if (m_lstPort != null)
                {
                    for (int i = 0; i < m_lstPort.Count; i++)
                    {
                        if (m_lstPort[i] == nPort) return i;
                    }
                }
                return -1;
            }
            // 해당 인덱스의 포트 번호를 리턴
            public int GetSerialPort(int nCommIndex)
            {
                if (m_lstPort != null)
                {
                    if (nCommIndex < m_lstPort.Count)
                    {
                        return m_lstPort[nCommIndex];
                    }
                }
                return -1;
            }
            // 전체 포트의 인덱스 수를 리턴
            public int GetSerialCount()
            {
                if (m_lstPort != null)
                {
                    return m_lstPort.Count;
                }
                return -1;
            }

            //// * 중요: 오픈시에 Operation Mode, 모터 종류, 아이디, 위치값, 토크온 상태 를 가져와야 한다.
            // 컴포트를 연다.(중복 되지만 않으면 여러개를 여는 것이 가능)
            public bool Open(int nPort, int nBaudRate)
            {
                Ojw.CSerial COjwSerial = new CSerial();
                if (COjwSerial.Connect(nPort, nBaudRate) == true)
                {
                    if (m_lstSerial == null)
                    {
                        m_lstSerial = new List<CSerial>();
                        m_lstPort = new List<int>();
                        m_lstIndex = new List<int>();
                        m_lstID = new List<int>();
                        //m_lstSMap = new List<SMap_t>();

                        m_thReceive = new Thread(new ThreadStart(Thread_Receive));
                        m_thReceive.Start();
                        //Ojw.CMessage.Write("Init Thread");
                    }
                    m_lstSerial.Add(COjwSerial);
                    m_lstPort.Add(nPort);
                    m_lstIndex.Add(0);
                    m_lstID.Add(0);
                    //SMap_t SMap = new SMap_t();
                    //m_lstSMap.Add(SMap);

                    //Write_Ping(2, m_lstSerial.Count - 1, 254);

                    return true;
                }
                return false;
            }
            // 지정한 포트를 닫는다.
            public void Close(int nPort)
            {
                if (m_lstSerial != null)
                {
                    if (m_lstSerial.Count > 0)
                    {
                        int nIndex = GetSerialIndex(nPort);
                        if (nIndex >= 0)
                        {
                            m_lstSerial[nIndex].DisConnect();
                            m_lstSerial.RemoveAt(nIndex);
                            m_lstPort.RemoveAt(nIndex);
                            m_lstIndex.RemoveAt(nIndex);
                            m_lstID.RemoveAt(nIndex);
                            //m_lstSMap.RemoveAt(nIndex);
                        }
                    }
                    if (m_lstPort.Count == 0)
                    {
                        m_lstSerial.Clear(); m_lstPort.Clear(); m_lstIndex.Clear(); m_lstID.Clear();//m_lstSMap.Clear();
                        m_lstSerial = null; m_lstPort = null; m_lstIndex = null; m_lstID = null;//m_lstSMap = null;
                    }
                }
            }
            // 전체 포트를 닫는다.
            public void Close()
            {
                if (m_lstSerial != null)
                {
                    foreach (CSerial COjwSerial in m_lstSerial)
                    {
                        if (COjwSerial.IsConnect() == true) COjwSerial.DisConnect();
                    }
                    m_lstSerial.Clear(); m_lstPort.Clear(); m_lstIndex.Clear(); m_lstID.Clear();//m_lstSMap.Clear();
                    m_lstSerial = null; m_lstPort = null; m_lstIndex = null; m_lstID = null;//m_lstSMap = null;
                }
            }
        #endregion Open / Close, IsOpen, GetSerial, GetSerialIndex, GetSerialPort, GetSerialCount

            public void Read_Motor(int nMotor, int nAddress, int nLength)
            {
                byte[] pbyLength = Ojw.CConvert.ShortToBytes((short)nLength);
                Write2(m_aSParam[nMotor].nProtocol, m_aSParam[nMotor].nCommIndex, m_aSParam[nMotor].nRealID, 0x02, nAddress, pbyLength);
                pbyLength = null;
            }

            private void Thread_Receive()
            {
                //byte byHead0 = 0;
                //byte byHead1 = 0;
                //byte byHead2 = 0;

                byte[] buf;
                //byte[] abyTmp = new byte[4];
                //Ojw.CMessage.Write("[Thread_Receive] Running Thread");
                int nSeq = 0;
                while ((m_lstSerial != null) && (m_bProgEnd == false))
                {
                    try
                    {
                        if (m_lstSerial == null) break;
                        if (m_lstSerial[nSeq].IsConnect() == false) continue;
                        int nSize = m_lstSerial[nSeq].GetBuffer_Length();
                        if (nSize > 0)
                        {
                            buf = m_lstSerial[nSeq].GetBytes();
                            //Ojw.CMessage.Write("[Receive]");
                            //Ojw.CConvert.ByteToStructure(

                            //if (m_nProtocolVersion == 1)
                            //{
                            //    //continue;
                            //    //Parsor1(buf, nSize);
                            //}
                            //else // (m_aSParam_Axis[nAxis].nProtocol_Version == 2) 
                            //{
                            //    //Parsor(buf, nSize);
                            //}
                            //Ojw.CMessage.Write("");


                            
#if false // test
                            //foreach (byte byData in buf)
                            //{
                            //    Ojw.CMessage.Write2("0x{0}", Ojw.CConvert.IntToHex(byData, 2));                                
                            //}
                            //Ojw.CMessage.Write2("\r\n");
#else
                            int i = 0;
                            foreach (byte byData in buf)
                            {
#if false // for some packet errors
                                if (byData == 0xff)
                                {
                                    if ((m_lstIndex[nSeq] & 0xf0000) != 0)
                                    {
                                        if ((byData == 0xff) && ((m_lstIndex[nSeq] & 0xf0000) == 0x10000))
                                            m_lstIndex[nSeq] = (m_lstIndex[nSeq] & 0xfffff) | 0x20000;
                                        else if ((byData == 0xff) && ((m_lstIndex[nSeq] & 0xf0000) == 0x30000)) // 0xff, 0xff
                                        {
                                            m_lstIndex[nSeq] = 0x1000; // start new ( protocol 1 기준 ) -> 0xff, 0xff 가 들어올 확률은 아주 높다. 해서 이 부분은 일단 블록
                                        }
                                    }
                                    else
                                    {
                                        m_lstIndex[nSeq] = (m_lstIndex[nSeq] & 0xffff) | 0x10000;
                                    }
                                }
#endif
                                if (((m_lstIndex[nSeq] & 0x1000) == 0) && (byData == 0xff)) m_lstIndex[nSeq] = ((m_lstIndex[nSeq] & 0xf0000) | 0x1000);
                                else if (((m_lstIndex[nSeq] & 0x3000) == 0x1000) && (byData == 0xff))
                                {
                                    // Protocol 1 Checked
                                    m_lstIndex[nSeq] |= 0x2000;
                                }
                                else if (((m_lstIndex[nSeq] & 0x7000) == 0x3000) && (byData == 0xfd))
                                {
                                    m_lstIndex[nSeq] |= 0x4000;
                                }
                                else if (((m_lstIndex[nSeq] & 0xf000) == 0x7000) && (byData == 0x00))
                                {
                                    // start - Protocol 2
                                    m_lstIndex[nSeq] |= 0x8000;
                                    // 모터는 Seq 가 아니라 새로운 인덱스가 필요하다. 전면 수정 필요. 읽을 때 아이디, 시리얼인덱스, 프로토콜버전의 3가지가 일치하는지 확인하고 꺼내오도록...
                                    //if (m_lstSMap[nSeq].nProtocol != 2) m_lstSMap[nSeq].nProtocol = 2;                                    
                                }
                                else
                                {
                                    // Protocol 2 - Address 122 ~ 136 => (120 ~ 139): 14=>20 바이트(moving(1), MoveStatus(1), PWM(4), Load(4), Velocity(4), Position(4)
                                    if ((m_lstIndex[nSeq] & 0xf000) == 0xf000)
                                    {
                                        // 단, XL-320 은 프로토콜2 여도 주소번지가 다르다. 따로 예외 처리할 것. 나중에...




                                        // [Ping] => Length == 7 이면 Ping
                                        // ID(1), Length(2)=(0x07, 0x00), cmd(1)=0x55, error(1), ModelNumber(2), Fw(1), CRC(2)
                                        // [Read]
                                        // ID(1), Length(2),  Cmd(1) = 0x55, error(1), Datas(N)..., CRC(2) 
                                        switch (m_lstIndex[nSeq] & 0x0fff)
                                        {
                                            case 0 :
                                                m_lstID[nSeq] = byData; // ID Setting
                                                if (m_aSMap[m_lstID[nSeq]].nProtocol != 2) m_aSMap[m_lstID[nSeq]].nProtocol = 2;
                                                m_aSMap[m_lstID[nSeq]].nID = byData;
                                                m_lstIndex[nSeq]++;
                                                break;
                                            case 1:
                                                m_aSMap[m_lstID[nSeq]].nLength = byData; // Length == 7 -> Ping
                                                m_lstIndex[nSeq]++;
                                                break;
                                            case 2:
                                                m_aSMap[m_lstID[nSeq]].nLength |= (((int)byData << 8) & 0xff00); // Length == 7 -> Ping
                                                m_lstIndex[nSeq]++;
                                                break;
                                            case 3:
                                                m_aSMap[m_lstID[nSeq]].nCmd = byData;
                                                m_lstIndex[nSeq]++;
                                                break;
                                            case 4:
                                                m_aSMap[m_lstID[nSeq]].nError = byData;
                                                m_aSMap[m_lstID[nSeq]].nStep = 0;
                                                m_lstIndex[nSeq]++;
                                                break;
                                            case 5:
                                                if (m_aSMap[m_lstID[nSeq]].nLength == 7) // Ping
                                                {
                                                    if (m_aSMap[m_lstID[nSeq]].nStep == 0) m_aSMap[m_lstID[nSeq]].nModelNumber = byData;
                                                    else if (m_aSMap[m_lstID[nSeq]].nStep == 1) m_aSMap[m_lstID[nSeq]].nModelNumber |= ((int)byData << 8) & 0xff00;
                                                    else m_aSMap[m_lstID[nSeq]].nFw = byData;
                                                }
                                                else // 
                                                {
                                                    m_aSMap[m_lstID[nSeq]].abyMap[m_aSMap[m_lstID[nSeq]].nStep] = byData;
                                                }
                                                if (m_aSMap[m_lstID[nSeq]].nStep + 1 >= m_aSMap[m_lstID[nSeq]].nLength - 4) m_lstIndex[nSeq]++;
                                                m_aSMap[m_lstID[nSeq]].nStep++;
                                                break;
                                            case 6:
                                                m_aSMap[m_lstID[nSeq]].nCrc0 = byData;
                                                m_lstIndex[nSeq]++;
                                                break;
                                            case 7:
                                                m_aSMap[m_lstID[nSeq]].nCrc1 = byData;
                                                m_aSMap[m_lstID[nSeq]].nSeq++;
                                                m_lstIndex[nSeq] = 0;
                                                //if (m_bShowReceivedIDs == true) Ojw.CMessage.Write("[Serial[{0}]:[ID:{1}]Received[Protocol={2}]Type={3}", nSeq, m_lstID[nSeq], m_aSMap[m_lstID[nSeq]].nProtocol, (m_aSMap[m_lstID[nSeq]].abyMap[0] | m_aSMap[m_lstID[nSeq]].abyMap[1] << 8));
                                                
                                               if (m_bShowReceivedIDs == true) Ojw.CMessage.Write("[Serial[{0}]:[ID:{1}]Received[Protocol={2}]Type=0x{3}", nSeq, m_lstID[nSeq], m_aSMap[m_lstID[nSeq]].nProtocol, Ojw.CConvert.IntToHex(m_aSMap[m_lstID[nSeq]].abyMap[0] | ((m_aSMap[m_lstID[nSeq]].abyMap[1] << 8) & 0xff00), 4));
                                               if (m_bAutoset == true)
                                                {
                                                    if (m_CTmr_AutoSet.Get() > 500)
                                                        m_bAutoset = false;
                                                    else
                                                    {
                                                        m_CTmr_AutoSet.Set();
                                                        SMotors_t SMot = new SMotors_t();
                                                        SMot.nCommIndex = nSeq;
                                                        SMot.nRealID = m_lstID[nSeq];
                                                        SMot.nProtocol = m_aSMap[m_lstID[nSeq]].nProtocol;
                                                        if (m_bAutoset2 == false) m_lstSIds.Add(SMot);
                                                    }
                                                }
                                               if (m_bAutoset2 == true)
                                               {
                                                   EMotor_t EMot = GetMotorType(m_aSMap[m_lstID[nSeq]].abyMap[0] | ((m_aSMap[m_lstID[nSeq]].abyMap[1] << 8) & 0xff00));
                                                   if ((EMot != EMotor_t.NONE) && (EMot != EMotor_t.SG_90))
                                                   {
                                                       //SetParam_MotorType(m_lstID[nSeq], EMot);
                                                       SetParam(m_lstID[nSeq], m_lstID[nSeq], nSeq, 0, EMot);
                                                   }
                                               }
                                                break;

#if false
                public int nCmd;
                public int nError;
                public int nModelNumber;
                public EMotor_t EMotor;
#endif
                                        }
                                    }
                                    // Protocol 1 - Address 36 ~ 46 => (30~49) : 10=>20 바이트(Position(2), Speed(2), Load(2), Volt(1), Temp(1), Registered(2), Moving(1)
                                    else if ((m_lstIndex[nSeq] & 0x3000) == 0x3000)
                                    {
#if true
                                        // [Ping] => Length == 2 이면 Ping
                                        // ID(1), Length(1)=(0x02), error(1), Checksum(1)
                                        // [Read]
                                        // ID(1), Length(1),  Cmd(1) = 0x55, error(1), Datas(N)..., CRC(2) 
                                        //switch (m_lstIndex[nSeq] & 0x0fff)
                                        //{
                                        //    case 0: // ID
                                        //        if (m_aSMap_Old[byData].nProtocol != 1) m_aSMap_Old[byData].nProtocol = 1;
                                        //        m_aSMap_Old[byData].nID = byData;
                                        //        m_lstIndex[nSeq]++;
                                        //        break;
                                           
                                        //}                                        
                                        switch (m_lstIndex[nSeq] & 0x0fff)
                                        {
                                            case 0:
                                                m_lstID[nSeq] = byData; // ID Setting
                                                if (m_aSMap[m_lstID[nSeq]].nProtocol != 1) m_aSMap[m_lstID[nSeq]].nProtocol = 1;
                                                m_aSMap[m_lstID[nSeq]].nID = byData;
                                                m_lstIndex[nSeq]++;
                                                break;
                                            case 1:
                                                m_aSMap[m_lstID[nSeq]].nLength = byData; // Length == 2 -> Ping
                                                m_lstIndex[nSeq]++;
                                                if (byData == 2) m_lstIndex[nSeq]++; // Ping Data 는 Cmd 가 없다.
                                                break;
                                            //case 2:
                                            //    m_aSMap[m_lstID[nSeq]].nCmd = byData;
                                            //    m_lstIndex[nSeq]++;
                                            //    break;
                                            case 2:
                                                m_aSMap[m_lstID[nSeq]].nError = byData;
                                                m_aSMap[m_lstID[nSeq]].nStep = 0;
                                                m_lstIndex[nSeq]++;
                                                break;
                                            case 3:
                                                if (m_aSMap[m_lstID[nSeq]].nLength == 2) // Ping
                                                {
                                                    //if (m_aSMap[m_lstID[nSeq]].nStep == 0) m_aSMap[m_lstID[nSeq]].nModelNumber = byData;
                                                    //else if (m_aSMap[m_lstID[nSeq]].nStep == 1) m_aSMap[m_lstID[nSeq]].nModelNumber |= ((int)byData << 8) & 0xff00;
                                                    //else m_aSMap[m_lstID[nSeq]].nFw = byData;
                                                }
                                                else // 
                                                {
                                                    m_aSMap[m_lstID[nSeq]].abyMap[m_aSMap[m_lstID[nSeq]].nStep] = byData;
                                                }
                                                if (m_aSMap[m_lstID[nSeq]].nStep + 1 >= m_aSMap[m_lstID[nSeq]].nLength - 3) m_lstIndex[nSeq]++;
                                                m_aSMap[m_lstID[nSeq]].nStep++;
                                                break;
                                            //case 4:
                                            //    m_aSMap[m_lstID[nSeq]].nCrc0 = byData;
                                            //    m_lstIndex[nSeq]++;
                                            //    break;
                                            case 4:
                                                m_aSMap[m_lstID[nSeq]].nCrc0 = byData;
                                                m_aSMap[m_lstID[nSeq]].nCrc1 = byData;
                                                m_aSMap[m_lstID[nSeq]].nSeq++;
                                                m_lstIndex[nSeq] = 0;
                                                //if (m_bShowReceivedIDs == true) Ojw.CMessage.Write("[Serial[{0}]:[ID:{1}]Received[Protocol={2}]", nSeq, m_lstID[nSeq], m_aSMap[m_lstID[nSeq]].nProtocol);
                                                if (m_bShowReceivedIDs == true) Ojw.CMessage.Write("[Serial[{0}]:[ID:{1}]Received[Protocol={2}]Type=0x{3}", nSeq, m_lstID[nSeq], m_aSMap[m_lstID[nSeq]].nProtocol, Ojw.CConvert.IntToHex(m_aSMap[m_lstID[nSeq]].abyMap[0] | ((m_aSMap[m_lstID[nSeq]].abyMap[1] << 8) & 0xff00),4));
                                                if (m_bAutoset == true)
                                                {
                                                    if (m_CTmr_AutoSet.Get() > 500)
                                                        m_bAutoset = false;
                                                    else
                                                    {
                                                        m_CTmr_AutoSet.Set();
                                                        SMotors_t SMot = new SMotors_t();
                                                        SMot.nCommIndex = nSeq;
                                                        SMot.nRealID = m_lstID[nSeq];
                                                        SMot.nProtocol = m_aSMap[m_lstID[nSeq]].nProtocol;
                                                        if (m_bAutoset2 == false) m_lstSIds.Add(SMot);
                                                    }
                                                }
                                                if (m_bAutoset2 == true)
                                                {
                                                    EMotor_t EMot = GetMotorType(m_aSMap[m_lstID[nSeq]].abyMap[0] | ((m_aSMap[m_lstID[nSeq]].abyMap[1] << 8) & 0xff00));
                                                    if ((EMot != EMotor_t.NONE) && (EMot != EMotor_t.SG_90))
                                                    {
                                                        //SetParam_MotorType(m_lstID[nSeq], EMot);

                                                        SetParam(m_lstID[nSeq], m_lstID[nSeq], nSeq, 0, EMot);
                                                    }
                                                }
                                                break;
                                        }
#endif
                                    }
                                }
                                i++;
                            }
#endif

                        }

                        if ((nSeq + 1) >= m_lstSerial.Count) nSeq = 0;
                        else nSeq++;

                        Thread.Sleep(1);
                    }
                    catch(Exception ex)
                    {
                        Ojw.CMessage.Write_Error(ex.ToString());
                        if (m_lstID != null)
                            if (nSeq < m_lstID.Count)
                                m_aSMap[m_lstID[nSeq]].nStep = 0;
                        if (m_lstIndex != null)
                            if (nSeq < m_lstIndex.Count) m_lstIndex[nSeq] = 0;
                        //break;
                    }
                }

                //Ojw.CMessage.Write("[Thread_Receive] Closed Thread");
            }

        #region Kinematics
            private List<string> m_lststrForward = new List<string>();
            private List<string> m_lststrInverse = new List<string>();
            private List<CDhParamAll> m_lstCDhParamAll = new List<CDhParamAll>();
            private List<Ojw.SOjwCode_t> m_lstSCode = new List<SOjwCode_t>();
            public void InitKinematics()
            {
                m_lststrForward.Clear();
                m_lststrInverse.Clear();
                m_lstCDhParamAll.Clear();
                m_lstSCode.Clear();
            }
            public void DestroyKinematics()
            {
                m_lststrForward.Clear();
                m_lststrInverse.Clear();
                m_lstCDhParamAll.Clear();
                m_lstSCode.Clear();
            }
            public int Kinematics_Forward_GetCount() { return m_lstCDhParamAll.Count; }
            public int Kinematics_Inverse_GetCount() { return m_lstSCode.Count; }
            public int Kinematics_Forward_Add(string strForward)
            {
                if (strForward != null)
                {
                    CDhParamAll CParamAll = new CDhParamAll();
                    if (strForward.Length > 0)
                    {
                        CKinematics.CForward.MakeDhParam(strForward, out CParamAll);

                        m_lststrForward.Add(strForward);
                        m_lstCDhParamAll.Add(CParamAll);
                    }
                }
                return m_lststrForward.Count;
            }
            public int Kinematics_Inverse_Add(bool bPython, string strInverse)
            {
                if (strInverse != null)
                {
                    if (strInverse.Length > 0)
                    {
                        Ojw.SOjwCode_t SCode = new SOjwCode_t();
                        CKinematics.CInverse.Compile(((bPython == true) ? "!" : string.Empty) + strInverse, out SCode);
                        if (SCode.nMotor_Max > 0)
                        {
                            m_lststrInverse.Add(strInverse);
                            m_lstSCode.Add(SCode);
                        }
                    }
                }
                return m_lststrInverse.Count;
            }
            public int Kinematics_Forward_Set(int nFunctionNumber, string strForward)
            {
                if (strForward != null)
                {
                    CDhParamAll CParamAll = new CDhParamAll();
                    if ((strForward.Length > 0) && (nFunctionNumber < m_lststrForward.Count) && (nFunctionNumber < m_lstCDhParamAll.Count))
                    {
                        CKinematics.CForward.MakeDhParam(strForward, out CParamAll);

                        m_lststrForward[nFunctionNumber] = strForward;
                        m_lstCDhParamAll[nFunctionNumber] = CParamAll;
                    }
                }
                return m_lststrForward.Count;
            }
            public int Kinematics_Inverse_Set(int nFunctionNumber, bool bPython, string strInverse)
            {
                if (strInverse != null)
                {
                    if ((strInverse.Length > 0) && (nFunctionNumber < m_lststrInverse.Count) && (nFunctionNumber < m_lstSCode.Count))
                    {
                        Ojw.SOjwCode_t SCode = new SOjwCode_t();
                        CKinematics.CInverse.Compile(((bPython == true) ? "!" : string.Empty) + strInverse, out SCode);
                        if (SCode.nMotor_Max > 0)
                        {
                            m_lststrInverse[nFunctionNumber] = strInverse;
                            m_lstSCode[nFunctionNumber] = SCode;
                        }
                    }
                }
                return m_lststrInverse.Count;
            }

            public bool Get_Xyz(int nFunctionNumber, out double dX, out double dY, out double dZ)
            {
                float [] afMot = new float[m_nMotorCountAll];
                Array.Clear(afMot, 0, afMot.Length);
                for (int i = 0; i < afMot.Length; i++)
                {
                    afMot[i] = m_aSMot[i].fAngle_Back;//(float)CalcEvd2Angle(i, m_aSMot[i].nValue);
                }
                return Get_Xyz(nFunctionNumber, afMot, out dX, out dY, out dZ);
                //return Get_Xyz(nFunctionNumber, m_aSMot, out dX, out dY, out dZ);
            }
            public bool Get_Xyz(int nFunctionNumber, float [] afMot, out double dX, out double dY, out double dZ)
            {
                int i;
                double[] dcolX;
                double[] dcolY;
                double[] dcolZ;

                double[] adMot = new double[afMot.Length];
                Array.Clear(adMot, 0, afMot.Length);
                adMot = Ojw.CConvert.FloatsToDoubles(afMot);
                //for (i = 0; i < afMot.Length; i++) adMot[i] = (double)afMot[i];
                return Ojw.CKinematics.CForward.CalcKinematics(m_lstCDhParamAll[nFunctionNumber], adMot, out dcolX, out dcolY, out dcolZ, out dX, out dY, out dZ);
            }
            //private Ojw.C3d m_C3d = new C3d();
            public void Set_Xyz(int nFunctionNumber, double dX, double dY, double dZ)//, int nTime_Milliseconds)
            {
                int[] anMotorID = new int[256];
                double[] adValue = new double[256];
                int nCnt = GetData_Inverse(nFunctionNumber, dX, dY, dZ, out anMotorID, out adValue);
                for (int i = 0; i < nCnt; i++)
                {
                    Set(anMotorID[i], (float)adValue[i]);
                    //SetData(anMotorID[i], (float)adValue[i]);
                    //CMotor.Set_Angle(anMotorID[i], (float)adValue[i], nTime_Milliseconds);
                }
                //CMotor.Send_Motor();
            }
            private int GetData_Inverse(int nFunctionNumber, double dX, double dY, double dZ, out int[] anMotorID, out double[] adValue)
            {
                // 집어넣기 전에 내부 메모리를 클리어 한다.
                SOjwCode_t SCode = m_lstSCode[nFunctionNumber];
                Ojw.CKinematics.CInverse.SetValue_ClearAll(ref SCode);
                Ojw.CKinematics.CInverse.SetValue_X(dX);
                Ojw.CKinematics.CInverse.SetValue_Y(dY);
                Ojw.CKinematics.CInverse.SetValue_Z(dZ);

                // 현재의 모터각을 전부 집어 넣도록 한다.
                for (int i = 0; i < m_nMotorCountAll; i++)
                {
                    // 모터값을 3D에 넣어주고
                    //SetData(i, Ojw.CConvert.StrToFloat(m_txtAngle[i].Text));
                    // 그 값을 꺼내 수식 계산에 넣어준다.
                    Ojw.CKinematics.CInverse.SetValue_Motor(i, m_aSMot[i].fAngle_Back);
                }

                // 실제 수식계산
                Ojw.CKinematics.CInverse.CalcCode(ref SCode);


                m_lstSCode[nFunctionNumber] = SCode;
                // 나온 결과값을 옮긴다.
                int nMotCnt = SCode.nMotor_Max;
                if (nMotCnt <= 0)
                {
                    anMotorID = null;
                    adValue = null;
                    return 0;
                }
                anMotorID = new int[nMotCnt];
                adValue = new double[nMotCnt];
                for (int i = 0; i < nMotCnt; i++)
                {
                    anMotorID[i] = SCode.pnMotor_Number[i];
                    adValue[i] = Ojw.CKinematics.CInverse.GetValue_Motor(anMotorID[i]);

                    //Set(SCode.pnMotor_Number[i], (float)Ojw.CKinematics.CInverse.GetValue_Motor(SCode.pnMotor_Number[i]));
                }
                return nMotCnt;
            }
        #endregion Kinematics

            private void SetAddress(int nMotor, EMotor_t EMot) { SetAddress(EMot, ref m_aSAddress[nMotor]); }
            private void SetAddress(EMotor_t EMot, ref SAddress_t SAddress)
            {
                switch (EMot)
                {
        #region AX &...
                    case EMotor_t.MX_12:
                    case EMotor_t.DX_113:
                    case EMotor_t.DX_116: //
                    case EMotor_t.DX_117: //
                    case EMotor_t.RX_10: // 9~12v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시)
                    case EMotor_t.RX_24F: // 9~12v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시)
                    case EMotor_t.RX_28: // 12v~18.5v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시)
                    case EMotor_t.RX_64:
                    case EMotor_t.EX_106:
                    case EMotor_t.AX_12:
                    case EMotor_t.AX_18:
                        SAddress.nMotorNumber_0_2 = 0;        // R    0 : none
                        SAddress.nMotorNumber_Size_2 = 2;
                        SAddress.nFwVersion_6_1 = 6;          // R      
                        SAddress.nFwVersion_Size_1 = 1;

                        SAddress.nRealID_7_1 = 7;             // RW   0 ~ 252
                        SAddress.nRealID_Size_1 = 1;
                        SAddress.nBaudrate_8_1 = 8;           // RW   0-9600, 1-57600, 2-115200, 3-1M, 4-2M,  5-3M, 6-4M, 7-4.5M
                        SAddress.nBaudrate_Size_1 = 1;




                        SAddress.nTorq_64_1 = 24;              // RW   Off(0), On(1)
                        SAddress.nTorq_Size_1 = 1;
                        SAddress.nLed_65_1 = 25;               // RW   Off(0), On(1)
                        SAddress.nLed_Size_1 = 1;

                        SAddress.nMode_Operating_11_1 = 6;    // RW   1=속도제어(무한회전), 3(초기값)=위치제어(RPM), 4=Multi-Turn(-256~256:512회전), 5=전류기반 위치제어모드(-256~256:512회전), 16=PWM제어
                        SAddress.nMode_Operating_Size_1 = 4; // 속도제어시 0, 위치제어시 [6]00, [7]00, [8]ff, [9]03 ( 65283 ) 으로 셋팅해야 함. -> CW Limit 0, CCW Limit 1023

                        SAddress.nGoal_Vel_104_4 = 32;         // RW
                        SAddress.nGoal_Vel_Size_4 = 2;

                        SAddress.nProfile_Vel_112_4 = 32;      // RW
                        SAddress.nProfile_Vel_Size_4 = 2;

                        SAddress.nGoal_Pos_116_4 = 30;         // RW
                        SAddress.nGoal_Pos_Size_4 = 2;



                        // [DriveMode] 
                        //    0x01 : 정상회전(0), 역회전(1)
                        //    0x02 : 540 전용 Master(0), Slave(1)
                        //    0x04 : 속도기준 제어(0), 시간기준 제어(1)
                        //SAddress.nMode_Drive_10_1 = 10;        // RW 
                        //SAddress.nMode_Drive_Size_1 = 1;
                        //SAddress.nMode_Operating_11_1 = 11;    // RW   1=속도제어(무한회전), 3(초기값)=위치제어(RPM), 4=Multi-Turn(-256~256:512회전), 5=전류기반 위치제어모드(-256~256:512회전), 16=PWM제어
                        //SAddress.nMode_Operating_Size_1 = 1;
                        //SAddress.nProtocolVersion_13_1 = 13;   // RW   
                        //SAddress.nProtocolVersion_Size_1 = 1;
                        //SAddress.nOffset_20_4 = 20;            // RW   1회전 모드인 경우 -1024~1024 까지만 제어
                        //SAddress.nOffset_Size_4 = 4;
                        //SAddress.nLimit_PWM_36_2 = 36;         // RW   0~885 (885 = 100%)
                        //SAddress.nLimit_PWM_Size_2 = 2;
                        //SAddress.nLimit_Curr_38_2 = 38;        // RW   전류제어모드/전류기반위치제어모드에서 사용, 0~2047
                        //SAddress.nLimit_Curr_Size_2 = 2;
                        //// [Shutdown] - Reboot 으로만 해제 가능
                        ////    0x20 : 과부하
                        ////    0x10 : 전력이상
                        ////    0x08 : 엔코더 이상(Following Error)
                        ////    0x04 : 과열
                        ////    0x01 : 인가된 전압 이상
                        //SAddress.nShutdown_63_1 = 63;          // RW
                        //SAddress.nShutdown_Size_1 = 1;

                        //SAddress.nError_70_1 = 70;             // R    
                        //SAddress.nError_Size_1 = 1;
                        //SAddress.nGain_Vel_I_76_2 = 76;        // RW
                        //SAddress.nGain_Vel_I_Size_2 = 2;
                        //SAddress.nGain_Vel_P_78_2 = 78;        // RW
                        //SAddress.nGain_Vel_P_Size_2 = 2;
                        //SAddress.nGain_Pos_D_80_2 = 80;        // RW
                        //SAddress.nGain_Pos_D_Size_2 = 2;
                        //SAddress.nGain_Pos_I_82_2 = 82;        // RW
                        //SAddress.nGain_Pos_I_Size_2 = 2;
                        //SAddress.nGain_Pos_P_84_2 = 84;        // RW
                        //SAddress.nGain_Pos_P_Size_2 = 2;
                        //SAddress.nGain_Pos_F2_88_2 = 88;       // RW
                        //SAddress.nGain_Pos_F2_Size_2 = 2;
                        //SAddress.nGain_Pos_F1_90_2 = 90;       // RW
                        //SAddress.nGain_Pos_F1_Size_2 = 2;

                        //SAddress.nWatchDog_98_1 = 98;          // RW
                        //SAddress.nWatchDog_Size_1 = 1;

                        //SAddress.nGoal_PWM_100_2 = 100;         // RW   -PWMLimit ~ +PWMLimit
                        //SAddress.nGoal_PWM_Size_2 = 2;
                        //SAddress.nGoal_Current_102_2 = 102;     // RW   -CurrentLimit ~ +CurrentLimit
                        //SAddress.nGoal_Current_Size_2 = 2;

                        //SAddress.nProfile_Acc_108_4 = 108;      // RW
                        //SAddress.nProfile_Acc_Size_4 = 4;

                        //SAddress.nMoving_122_1 = 122;           // R    움직임 감지 못함(0), 움직임 감지(1)
                        //SAddress.nMoving_Size_1 = 1;
                        //// [Moving Status]
                        ////    0x30  - 0x30 : 사다리꼴 속도 프로파일
                        ////            0x20 : 삼각 속도 프로파일
                        ////            0x10 : 사각 속도 프로파일
                        ////            0x00 : 프로파일 미사용(Step)
                        ////    0x08 : Following Error
                        ////    0x02 : Goal Position 명령에 따라 진행 중
                        ////    0x01 : Inposition
                        //SAddress.nMoving_Status_123_1 = 123;    // R
                        //SAddress.nMoving_Status_Size_1 = 1;

                        //SAddress.nPresent_PWM_124_2 = 124;      // R
                        //SAddress.nPresent_PWM_Size_2 = 2;
                        //SAddress.nPresent_Curr_126_2 = 126;     // R
                        //SAddress.nPresent_Curr_Size_2 = 2;
                        //SAddress.nPresent_Vel_128_4 = 128;      // R
                        //SAddress.nPresent_Vel_Size_4 = 4;
                        //SAddress.nPresent_Pos_132_4 = 132;      // R
                        //SAddress.nPresent_Pos_Size_4 = 4;
                        //SAddress.nPresent_Volt_144_2 = 144;     // R
                        //SAddress.nPresent_Volt_Size_2 = 2;
                        //SAddress.nPresent_Temp_146_1 = 146;     // R
                        //SAddress.nPresent_Temp_Size_1 = 1;


                        /*

                        SetParam_ModelNum(nAxis, 12); // 0번지에 모델번호 12
                        SetParam_Addr_Max(nAxis, 52);
                        SetParam_Addr_Torq(nAxis, 24);
                        SetParam_Addr_Led(nAxis, 25);
                        SetParam_Addr_Mode(nAxis, 11); // 320 -> 11            [1 : 속도, 2(default) : 관절]
                        SetParam_Addr_Speed(nAxis, 32); // 320 -> 32 2 bytes
                        SetParam_Addr_Speed_Size(nAxis, 2);
                        SetParam_Addr_Pos_Speed(nAxis, 32); // 320 -> 32 2 bytes
                        SetParam_Addr_Pos_Speed_Size(nAxis, 2);
                        SetParam_Addr_Pos(nAxis, 30); // 320 -> 30 2 bytes
                        SetParam_Addr_Pos_Size(nAxis, 2);*/
                        break;
        #endregion AX
        #region XL_430 & MX Protocol 2
                    case EMotor_t.MX_28:
                    case EMotor_t.MX_64:
                    case EMotor_t.MX_106:
                    case EMotor_t.XM_540:
                    case EMotor_t.XL_430:
                        SAddress.nMotorNumber_0_2 = 0;        // R    0 : none
                        SAddress.nMotorNumber_Size_2 = 2;
                        SAddress.nFwVersion_6_1 = 6;          // R      
                        SAddress.nFwVersion_Size_1 = 1;

                        SAddress.nRealID_7_1 = 7;             // RW   0 ~ 252
                        SAddress.nRealID_Size_1 = 1;
                        SAddress.nBaudrate_8_1 = 8;           // RW   0-9600, 1-57600, 2-115200, 3-1M, 4-2M,  5-3M, 6-4M, 7-4.5M
                        SAddress.nBaudrate_Size_1 = 1;
                        // [DriveMode] 
                        //    0x01 : 정상회전(0), 역회전(1)
                        //    0x02 : 540 전용 Master(0), Slave(1)
                        //    0x04 : 속도기준 제어(0), 시간기준 제어(1)
                        SAddress.nMode_Drive_10_1 = 10;        // RW 
                        SAddress.nMode_Drive_Size_1 = 1;
                        SAddress.nMode_Operating_11_1 = 11;    // RW   1=속도제어(무한회전), 3(초기값)=위치제어(RPM), 4=Multi-Turn(-256~256:512회전), 5=전류기반 위치제어모드(-256~256:512회전), 16=PWM제어
                        SAddress.nMode_Operating_Size_1 = 1;
                        SAddress.nProtocolVersion_13_1 = 13;   // RW   
                        SAddress.nProtocolVersion_Size_1 = 1;
                        SAddress.nOffset_20_4 = 20;            // RW   1회전 모드인 경우 -1024~1024 까지만 제어
                        SAddress.nOffset_Size_4 = 4;
                        SAddress.nLimit_PWM_36_2 = 36;         // RW   0~885 (885 = 100%)
                        SAddress.nLimit_PWM_Size_2 = 2;
                        SAddress.nLimit_Curr_38_2 = 38;        // RW   전류제어모드/전류기반위치제어모드에서 사용, 0~2047
                        SAddress.nLimit_Curr_Size_2 = 2;
                        // [Shutdown] - Reboot 으로만 해제 가능
                        //    0x20 : 과부하
                        //    0x10 : 전력이상
                        //    0x08 : 엔코더 이상(Following Error)
                        //    0x04 : 과열
                        //    0x01 : 인가된 전압 이상
                        SAddress.nShutdown_63_1 = 63;          // RW
                        SAddress.nShutdown_Size_1 = 1;
                        SAddress.nTorq_64_1 = 64;              // RW   Off(0), On(1)
                        SAddress.nTorq_Size_1 = 1;
                        SAddress.nLed_65_1 = 65;               // RW   Off(0), On(1)
                        SAddress.nLed_Size_1 = 1;
                        SAddress.nError_70_1 = 70;             // R    
                        SAddress.nError_Size_1 = 1;
                        SAddress.nGain_Vel_I_76_2 = 76;        // RW
                        SAddress.nGain_Vel_I_Size_2 = 2;
                        SAddress.nGain_Vel_P_78_2 = 78;        // RW
                        SAddress.nGain_Vel_P_Size_2 = 2;
                        SAddress.nGain_Pos_D_80_2 = 80;        // RW
                        SAddress.nGain_Pos_D_Size_2 = 2;
                        SAddress.nGain_Pos_I_82_2 = 82;        // RW
                        SAddress.nGain_Pos_I_Size_2 = 2;
                        SAddress.nGain_Pos_P_84_2 = 84;        // RW
                        SAddress.nGain_Pos_P_Size_2 = 2;
                        SAddress.nGain_Pos_F2_88_2 = 88;       // RW
                        SAddress.nGain_Pos_F2_Size_2 = 2;
                        SAddress.nGain_Pos_F1_90_2 = 90;       // RW
                        SAddress.nGain_Pos_F1_Size_2 = 2;

                        SAddress.nWatchDog_98_1 = 98;          // RW
                        SAddress.nWatchDog_Size_1 = 1;

                        SAddress.nGoal_PWM_100_2 = 100;         // RW   -PWMLimit ~ +PWMLimit
                        SAddress.nGoal_PWM_Size_2 = 2;
                        SAddress.nGoal_Current_102_2 = 102;     // RW   -CurrentLimit ~ +CurrentLimit
                        SAddress.nGoal_Current_Size_2 = 2;
                        SAddress.nGoal_Vel_104_4 = 104;         // RW
                        SAddress.nGoal_Vel_Size_4 = 4;

                        SAddress.nProfile_Acc_108_4 = 108;      // RW
                        SAddress.nProfile_Acc_Size_4 = 4;
                        SAddress.nProfile_Vel_112_4 = 112;      // RW
                        SAddress.nProfile_Vel_Size_4 = 4;

                        SAddress.nGoal_Pos_116_4 = 116;         // RW
                        SAddress.nGoal_Pos_Size_4 = 4;

                        SAddress.nMoving_122_1 = 122;           // R    움직임 감지 못함(0), 움직임 감지(1)
                        SAddress.nMoving_Size_1 = 1;
                        // [Moving Status]
                        //    0x30  - 0x30 : 사다리꼴 속도 프로파일
                        //            0x20 : 삼각 속도 프로파일
                        //            0x10 : 사각 속도 프로파일
                        //            0x00 : 프로파일 미사용(Step)
                        //    0x08 : Following Error
                        //    0x02 : Goal Position 명령에 따라 진행 중
                        //    0x01 : Inposition
                        SAddress.nMoving_Status_123_1 = 123;    // R
                        SAddress.nMoving_Status_Size_1 = 1;

                        SAddress.nPresent_PWM_124_2 = 124;      // R
                        SAddress.nPresent_PWM_Size_2 = 2;
                        SAddress.nPresent_Curr_126_2 = 126;     // R
                        SAddress.nPresent_Curr_Size_2 = 2;
                        SAddress.nPresent_Vel_128_4 = 128;      // R
                        SAddress.nPresent_Vel_Size_4 = 4;
                        SAddress.nPresent_Pos_132_4 = 132;      // R
                        SAddress.nPresent_Pos_Size_4 = 4;
                        SAddress.nPresent_Volt_144_2 = 144;     // R
                        SAddress.nPresent_Volt_Size_2 = 2;
                        SAddress.nPresent_Temp_146_1 = 146;     // R
                        SAddress.nPresent_Temp_Size_1 = 1;
                        break;
        #endregion XL_430
        #region XL_320
                    case EMotor_t.XL_320:
                        SAddress.nTorq_64_1 = 24;              // RW   Off(0), On(1)                        
                        SAddress.nLed_65_1 = 25;               // RW   Off(0), On(1)

                        SAddress.nMode_Operating_11_1 = 6;    // RW   1=속도제어(무한회전), 3(초기값)=위치제어(RPM), 4=Multi-Turn(-256~256:512회전), 5=전류기반 위치제어모드(-256~256:512회전), 16=PWM제어
                        SAddress.nMode_Operating_Size_1 = 5; // 속도제어시 0, 위치제어시 [6]00, [7]00, [8]ff, [9]03 ( 65283 ) 으로 셋팅해야 함. -> CW Limit 0, CCW Limit 1023
                        SAddress.nProtocolVersion_13_1 = 13;   // RW  1 바퀴모드, 2 관절모드 (이 부분이 프로토콜 1의 다른 모터와 xl320 이 다른 부분)(아예 위 4바이트에 합쳐서 5바이트 만들어 제어)
                        SAddress.nProtocolVersion_Size_1 = 1;

                        
                        SAddress.nGoal_Vel_104_4 = 32;         // RW
                        SAddress.nGoal_Vel_Size_4 = 2;

                        SAddress.nProfile_Vel_112_4 = 32;      // RW
                        SAddress.nProfile_Vel_Size_4 = 2;

                        SAddress.nGoal_Pos_116_4 = 30;         // RW
                        SAddress.nGoal_Pos_Size_4 = 2;
                        /*
                          SetParam_Addr_Max(nAxis, 52);
                          SetParam_Addr_Torq(nAxis, 24);
                          SetParam_Addr_Led(nAxis, 25);
                          SetParam_Addr_Mode(nAxis, 11); // 320 -> 11            [1 : 속도, 2(default) : 관절]
                        SetParam_Addr_Speed(nAxis, 32); // 320 -> 32 2 bytes
                        SetParam_Addr_Speed_Size(nAxis, 2);
                          SetParam_Addr_Pos_Speed(nAxis, 32); // 320 -> 32 2 bytes
                          SetParam_Addr_Pos_Speed_Size(nAxis, 2);
                          SetParam_Addr_Pos(nAxis, 30); // 320 -> 30 2 bytes
                          SetParam_Addr_Pos_Size(nAxis, 2);
                     */
                        SAddress.nMotorNumber_0_2 = 0;        // R    0 : none
                        SAddress.nMotorNumber_Size_2 = 2;
                        SAddress.nFwVersion_6_1 = 6;          // R      
                        SAddress.nFwVersion_Size_1 = 1;

                        SAddress.nRealID_7_1 = 7;             // RW   0 ~ 252
                        SAddress.nRealID_Size_1 = 1;
                        SAddress.nBaudrate_8_1 = 8;           // RW   0-9600, 1-57600, 2-115200, 3-1M, 4-2M,  5-3M, 6-4M, 7-4.5M
                        SAddress.nBaudrate_Size_1 = 1;
                        // [DriveMode] 
                        //    0x01 : 정상회전(0), 역회전(1)
                        //    0x02 : 540 전용 Master(0), Slave(1)
                        //    0x04 : 속도기준 제어(0), 시간기준 제어(1)
                        SAddress.nMode_Drive_10_1 = 10;        // RW 
                        SAddress.nMode_Drive_Size_1 = 1;
                        //SAddress.nMode_Operating_11_1 = 11;    // RW   1=속도제어(무한회전), 3(초기값)=위치제어(RPM), 4=Multi-Turn(-256~256:512회전), 5=전류기반 위치제어모드(-256~256:512회전), 16=PWM제어
                        //SAddress.nMode_Operating_Size_1 = 1;
                        //SAddress.nProtocolVersion_13_1 = 13;   // RW   
                        //SAddress.nProtocolVersion_Size_1 = 1;
                        SAddress.nOffset_20_4 = 20;            // RW   1회전 모드인 경우 -1024~1024 까지만 제어
                        SAddress.nOffset_Size_4 = 4;
                        SAddress.nLimit_PWM_36_2 = 36;         // RW   0~885 (885 = 100%)
                        SAddress.nLimit_PWM_Size_2 = 2;
                        SAddress.nLimit_Curr_38_2 = 38;        // RW   전류제어모드/전류기반위치제어모드에서 사용, 0~2047
                        SAddress.nLimit_Curr_Size_2 = 2;
                        // [Shutdown] - Reboot 으로만 해제 가능
                        //    0x20 : 과부하
                        //    0x10 : 전력이상
                        //    0x08 : 엔코더 이상(Following Error)
                        //    0x04 : 과열
                        //    0x01 : 인가된 전압 이상
                        SAddress.nShutdown_63_1 = 63;          // RW
                        SAddress.nShutdown_Size_1 = 1;
                            //SAddress.nTorq_64_1 = 64;              // RW   Off(0), On(1)
                        SAddress.nTorq_Size_1 = 1;
                            //SAddress.nLed_65_1 = 65;               // RW   Off(0), On(1)
                        SAddress.nLed_Size_1 = 1;
                        SAddress.nError_70_1 = 70;             // R    
                        SAddress.nError_Size_1 = 1;
                        SAddress.nGain_Vel_I_76_2 = 76;        // RW
                        SAddress.nGain_Vel_I_Size_2 = 2;
                        SAddress.nGain_Vel_P_78_2 = 78;        // RW
                        SAddress.nGain_Vel_P_Size_2 = 2;
                        SAddress.nGain_Pos_D_80_2 = 80;        // RW
                        SAddress.nGain_Pos_D_Size_2 = 2;
                        SAddress.nGain_Pos_I_82_2 = 82;        // RW
                        SAddress.nGain_Pos_I_Size_2 = 2;
                        SAddress.nGain_Pos_P_84_2 = 84;        // RW
                        SAddress.nGain_Pos_P_Size_2 = 2;
                        SAddress.nGain_Pos_F2_88_2 = 88;       // RW
                        SAddress.nGain_Pos_F2_Size_2 = 2;
                        SAddress.nGain_Pos_F1_90_2 = 90;       // RW
                        SAddress.nGain_Pos_F1_Size_2 = 2;

                        SAddress.nWatchDog_98_1 = 98;          // RW
                        SAddress.nWatchDog_Size_1 = 1;

                        SAddress.nGoal_PWM_100_2 = 100;         // RW   -PWMLimit ~ +PWMLimit
                        SAddress.nGoal_PWM_Size_2 = 2;
                        SAddress.nGoal_Current_102_2 = 102;     // RW   -CurrentLimit ~ +CurrentLimit
                        SAddress.nGoal_Current_Size_2 = 2;
                        //SAddress.nGoal_Vel_104_4 = 104;         // RW
                        //SAddress.nGoal_Vel_Size_4 = 4;

                        SAddress.nProfile_Acc_108_4 = 108;      // RW
                        SAddress.nProfile_Acc_Size_4 = 4;
                            //SAddress.nProfile_Vel_112_4 = 112;      // RW
                            //SAddress.nProfile_Vel_Size_4 = 4;

                            //SAddress.nGoal_Pos_116_4 = 116;         // RW
                            //SAddress.nGoal_Pos_Size_4 = 4;

                        SAddress.nMoving_122_1 = 122;           // R    움직임 감지 못함(0), 움직임 감지(1)
                        SAddress.nMoving_Size_1 = 1;
                        // [Moving Status]
                        //    0x30  - 0x30 : 사다리꼴 속도 프로파일
                        //            0x20 : 삼각 속도 프로파일
                        //            0x10 : 사각 속도 프로파일
                        //            0x00 : 프로파일 미사용(Step)
                        //    0x08 : Following Error
                        //    0x02 : Goal Position 명령에 따라 진행 중
                        //    0x01 : Inposition
                        SAddress.nMoving_Status_123_1 = 123;    // R
                        SAddress.nMoving_Status_Size_1 = 1;

                        SAddress.nPresent_PWM_124_2 = 124;      // R
                        SAddress.nPresent_PWM_Size_2 = 2;
                        SAddress.nPresent_Curr_126_2 = 126;     // R
                        SAddress.nPresent_Curr_Size_2 = 2;
                        SAddress.nPresent_Vel_128_4 = 128;      // R
                        SAddress.nPresent_Vel_Size_4 = 4;
                        SAddress.nPresent_Pos_132_4 = 132;      // R
                        SAddress.nPresent_Pos_Size_4 = 4;
                        SAddress.nPresent_Volt_144_2 = 144;     // R
                        SAddress.nPresent_Volt_Size_2 = 2;
                        SAddress.nPresent_Temp_146_1 = 146;     // R
                        SAddress.nPresent_Temp_Size_1 = 1;
                        break;
        #endregion XL_320
                }
            }




            public int FindMotor(int nMotor_RealID) { int i = 0; foreach (SParam_t SParam in m_aSParam) { if (SParam.nRealID == nMotor_RealID) return i; i++; } return -1; }
            public int Get_RealID(int nMotor) { return m_aSParam[nMotor].nRealID; }

        #region Parameter Function(SetParam...)
            public void SetParam_Dir(int nMotor, int nDir) { m_aSParam[nMotor].nDir = nDir; }
            public void SetParam_RealID(int nMotor, int nMotorRealID) { m_aSParam[nMotor].nRealID = nMotorRealID; }
            //public void SetParam_OperationMode(int nMotor, EOperationMode_t EOperationMode) { m_aSParam[nMotor].EOperationMode = EOperationMode; }
            
            public void SetParam_CommIndex(int nMotor, int nCommIndex) { m_aSParam[nMotor].nCommIndex = nCommIndex; }               // 연결 이후에 둘 중 하나만 사용 한다.(되도록  CommIndex 를 사용할 것. Commport 로 설정 하려면 통신이 접속이 되어 있어야 한다.)
            public void SetParam_CommPort(int nMotor, int nCommPort) { m_aSParam[nMotor].nCommIndex = GetSerialIndex(nCommPort); }  // CommIndex 설정보다 직관적이나 잘못 설정 될 수 있다. 연결이 안 된 경우 CommIndes 가 잘못 지정될 수가 있다.
            
            public void SetParam_LimitUp(int nMotor, float fLimitUp) { m_aSParam[nMotor].fLimitUp = fLimitUp; }                       // Limit - 0: Ignore 
            public void SetParam_LimitDown(int nMotor, float fLimitDn) { m_aSParam[nMotor].fLimitDn = fLimitDn; }                       // Limit - 0: Ignore 
            public void SetParam_LimitRpm(int nMotor, float fLimitRpm) { m_aSParam[nMotor].fLimitRpm = fLimitRpm; }                       // Limit - 0: Ignore 
            public void SetParam_MotorType(int nMotor, EMotor_t EMot)
            {
                SetAddress(EMot, ref m_aSAddress[nMotor]);

                m_aSParam[nMotor].EMot = EMot;

                switch (EMot)
                {
                    // 
                    case EMotor_t.SG_90:
                        break;
                    // Protocol1
                    case EMotor_t.DX_113:
                    case EMotor_t.DX_116: //
                    case EMotor_t.DX_117: //
                    case EMotor_t.RX_10: // 9~12v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시)
                    case EMotor_t.RX_24F: // 9~12v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시)
                    case EMotor_t.RX_28: // 12v~18.5v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시)
                    case EMotor_t.RX_64:
                    case EMotor_t.AX_12:
                    case EMotor_t.AX_18:
                        //m_aSParam[nMotor].bEn = true;                       // 활성화

                        //m_aSParam[nMotor].nID = nMotor;                     // 모터를 제어하기 위한 가상 아이디. 중복 허용 불가. 보통은 모터 ID 와 일치시킨다.      
                        //m_aSParam[nMotor].nRealID = nMotorRealID;           // 실제 사용할 모터의 아이디
                        //m_aSParam[nMotor].nCommIndex = nCommIndex;          // 연결 된 통신포트 중 어느것에 연결 된 것이지에 대한 순서.

                        //m_aSParam[nMotor].nDir = nDir;
                        //m_aSParam[nMotor].EMot = EMot;
                        m_aSParam[nMotor].fCenterPos = 512.0f;

                        m_aSParam[nMotor].fMechMove = 1024.0f;
                        m_aSParam[nMotor].fDegree = 300.0f;
                        m_aSParam[nMotor].fRefRpm = 0.111f;                 // 기본 rpm 단위

                        m_aSParam[nMotor].fLimitRpm = 1023f;                // 

                        m_aSParam[nMotor].fLimitUp = 0.0f;    // Limit - 0: Ignore
                        m_aSParam[nMotor].fLimitDn = 0.0f;    // Limit - 0: Ignore

                        m_aSParam[nMotor].nProtocol = 1;               // 2, 0 - protocol2, 1 - protocol1, 3 - None     

                        break;
                    case EMotor_t.EX_106:
                        //m_aSParam[nMotor].EMot = EMot;
                        m_aSParam[nMotor].fCenterPos = 2048.0f;

                        m_aSParam[nMotor].fMechMove = 4095.0f;
                        m_aSParam[nMotor].fDegree = 251.0f;
                        m_aSParam[nMotor].fRefRpm = 0.111f;                 // 기본 rpm 단위

                        m_aSParam[nMotor].fLimitRpm = 1023f;                // 

                        m_aSParam[nMotor].fLimitUp = 0.0f;    // Limit - 0: Ignore
                        m_aSParam[nMotor].fLimitDn = 0.0f;    // Limit - 0: Ignore

                        m_aSParam[nMotor].nProtocol = 1;               // 2, 0 - protocol2, 1 - protocol1, 3 - None     
                        break;
        #region MX
                    case EMotor_t.MX_12:
                        m_aSParam[nMotor].fCenterPos = 1024.0f;

                        m_aSParam[nMotor].fMechMove = 2048.0f;
                        m_aSParam[nMotor].fDegree = 360.0f;
                        m_aSParam[nMotor].fRefRpm = 0.916f;            // 기본 rpm 단위

                        m_aSParam[nMotor].fLimitRpm = 415f;                // 

                        m_aSParam[nMotor].fLimitUp = 0.0f;    // Limit - 0: Ignore
                        m_aSParam[nMotor].fLimitDn = 0.0f;    // Limit - 0: Ignore

                        m_aSParam[nMotor].nProtocol = 1;               // 2, 0 - protocol2, 1 - protocol1, 3 - None              
                        break;
                    case EMotor_t.MX_28:
                    case EMotor_t.MX_64:
                    case EMotor_t.MX_106:
                        m_aSParam[nMotor].fCenterPos = 2048.0f;

                        m_aSParam[nMotor].fMechMove = 4096.0f;
                        m_aSParam[nMotor].fDegree = 360.0f;
                        m_aSParam[nMotor].fRefRpm = 0.229f;;//(EMot == EMotor_t.MX_12) ? 0.916f : 0.114f;  //0.229f;                 // 기본 rpm 단위

                        m_aSParam[nMotor].fLimitRpm = 415f;                // 

                        m_aSParam[nMotor].fLimitUp = 0.0f;    // Limit - 0: Ignore
                        m_aSParam[nMotor].fLimitDn = 0.0f;    // Limit - 0: Ignore

                        m_aSParam[nMotor].nProtocol = 2;               // 2, 0 - protocol2, 1 - protocol1, 3 - None              
                        break;
        #endregion MX
                    // Protocol2
                    case EMotor_t.NONE:
                    case EMotor_t.XM_540:
                    case EMotor_t.XL_430:
                        //m_aSParam[nMotor].bEn = true;                       // 활성화
                        //m_aSParam[nMotor].nModelNum = 1060;
                        //m_aSParam[nMotor].nID = nMotor;                     // 모터를 제어하기 위한 가상 아이디. 중복 허용 불가. 보통은 모터 ID 와 일치시킨다.      
                        //m_aSParam[nMotor].nRealID = nMotorRealID;           // 실제 사용할 모터의 아이디
                        //m_aSParam[nMotor].nCommIndex = nCommIndex;          // 연결 된 통신포트 중 어느것에 연결 된 것이지에 대한 순서.

                        //m_aSParam[nMotor].nDir = nDir;
                        m_aSParam[nMotor].fCenterPos = 2048.0f;

                        m_aSParam[nMotor].fMechMove = 4096.0f;
                        m_aSParam[nMotor].fDegree = 360.0f;
                        m_aSParam[nMotor].fRefRpm = 0.229f;                 // 기본 rpm 단위

                        m_aSParam[nMotor].fLimitRpm = 415f;                // 

                        m_aSParam[nMotor].fLimitUp = 0.0f;    // Limit - 0: Ignore
                        m_aSParam[nMotor].fLimitDn = 0.0f;    // Limit - 0: Ignore

                        m_aSParam[nMotor].nProtocol = 2;               // 2, 0 - protocol2, 1 - protocol1, 3 - None                             
                        break;


                    case EMotor_t.XL_320:
                        //m_aSParam[nMotor].EMot = EMot;
                        m_aSParam[nMotor].fCenterPos = 512.0f;

                        m_aSParam[nMotor].fMechMove = 1024.0f;
                        m_aSParam[nMotor].fDegree = 300.0f;
                        m_aSParam[nMotor].fRefRpm = 0.111f;                 // 기본 rpm 단위

                        m_aSParam[nMotor].fLimitRpm = 1023f;                // 

                        m_aSParam[nMotor].fLimitUp = 0.0f;    // Limit - 0: Ignore
                        m_aSParam[nMotor].fLimitDn = 0.0f;    // Limit - 0: Ignore

                        m_aSParam[nMotor].nProtocol = 2;               // 2, 0 - protocol2, 1 - protocol1, 3 - None     
                        break;
                    //case EMotor_t.XM_540:
                    //    break;
                }
            }
            public void SetParam(int nMotor, int nMotorRealID, int nCommIndex, int nDir, EMotor_t EMot)
            {
                //m_anMot_RealID[nMotor] = nMotorRealID; // ID 변경
                //m_anMot_SerialIndex[nMotor] = nCommIndex; // 통신 포트 변경
                m_aSParam[nMotor].nRealID = nMotorRealID; // ID 변경
                m_aSParam[nMotor].nCommIndex = nCommIndex; // 통신 포트 변경
                m_aSParam[nMotor].nDir = nDir;
                //m_aSParam[nMotor].EOperationMode = EOperationMode_t._Position; // Default
                //if (m_aSParam[nMotor].EOperationMode_Prev == EOperationMode_t._None) m_aSParam[nMotor].EOperationMode_Prev = m_aSParam[nMotor].EOperationMode;
                SetAddress(EMot, ref m_aSAddress[nMotor]);

#if true
                SetParam_MotorType(nMotor, EMot);
#else
                switch (EMot)
                {
                    // 
                    case EMotor_t.SG_90:
                        break;
                    // Protocol1
                    case EMotor_t.AX_12:
                    case EMotor_t.AX_18:
                        //m_aSParam[nMotor].bEn = true;                       // 활성화

                        //m_aSParam[nMotor].nID = nMotor;                     // 모터를 제어하기 위한 가상 아이디. 중복 허용 불가. 보통은 모터 ID 와 일치시킨다.      
                        //m_aSParam[nMotor].nRealID = nMotorRealID;           // 실제 사용할 모터의 아이디
                        //m_aSParam[nMotor].nCommIndex = nCommIndex;          // 연결 된 통신포트 중 어느것에 연결 된 것이지에 대한 순서.

                        m_aSParam[nMotor].nDir = nDir;
                        m_aSParam[nMotor].fCenterPos = 512.0f;

                        m_aSParam[nMotor].fMechMove = 1024.0f;
                        m_aSParam[nMotor].fDegree = 300.0f;
                        m_aSParam[nMotor].fRefRpm = 0.111f;                 // 기본 rpm 단위

                        m_aSParam[nMotor].fLimitRpm = 1023f;                // 

                        m_aSParam[nMotor].fLimitUp = 0.0f;    // Limit - 0: Ignore
                        m_aSParam[nMotor].fLimitDn = 0.0f;    // Limit - 0: Ignore

                        m_aSParam[nMotor].nProtocol = 1;               // 2, 0 - protocol2, 1 - protocol1, 3 - None     

                        break;


                    // Protocol2
                    case EMotor_t.NONE:
                    case EMotor_t.XL_430:
                        //m_aSParam[nMotor].bEn = true;                       // 활성화
                        //m_aSParam[nMotor].nModelNum = 1060;
                        //m_aSParam[nMotor].nID = nMotor;                     // 모터를 제어하기 위한 가상 아이디. 중복 허용 불가. 보통은 모터 ID 와 일치시킨다.      
                        //m_aSParam[nMotor].nRealID = nMotorRealID;           // 실제 사용할 모터의 아이디
                        //m_aSParam[nMotor].nCommIndex = nCommIndex;          // 연결 된 통신포트 중 어느것에 연결 된 것이지에 대한 순서.

                        m_aSParam[nMotor].nDir = nDir;
                        m_aSParam[nMotor].fCenterPos = 2048.0f;

                        m_aSParam[nMotor].fMechMove = 4096.0f;
                        m_aSParam[nMotor].fDegree = 360.0f;
                        m_aSParam[nMotor].fRefRpm = 0.229f;                 // 기본 rpm 단위

                        m_aSParam[nMotor].fLimitRpm = 415f;                // 

                        m_aSParam[nMotor].fLimitUp = 0.0f;    // Limit - 0: Ignore
                        m_aSParam[nMotor].fLimitDn = 0.0f;    // Limit - 0: Ignore

                        m_aSParam[nMotor].nProtocol = 2;               // 2, 0 - protocol2, 1 - protocol1, 3 - None                             
                        break;


                    case EMotor_t.XL_320:
                        break;
                    case EMotor_t.XM_540:
                        break;
                }
#if false
        #region Check
                //if (m_lstSCheckMotorType.Count > 0)
                {
                    bool bOk = false;
                    foreach (SCheckMotorType_t SType in m_lstSCheckMotorType)
                    {
                        if ((SType.EMot == EMot) && (SType.nCommIndex == nCommIndex) && (SType.nProtocol == m_aSMot_Info[nMotor].nProtocol) && (SType.nTorqAddress == m_aSMot_Info[nMotor].SAddress.nTorq_64_1))
                        {
                            bOk = true;
                            break;
                        }
                    }

                    if (bOk == false)
                    {
                        SCheckMotorType_t SType = new SCheckMotorType_t();
                        SType.nCommIndex = nCommIndex;
                        SType.EMot = EMot;
                        SType.nProtocol = m_aSMot_Info[nMotor].nProtocol;
                        SType.nTorqAddress = m_aSMot_Info[nMotor].SAddress.nTorq_64_1;
                        m_lstSCheckMotorType.Add(SType);
                    }
                }
        #endregion Check
                //m_lstSMot_Info.Add(SInfo);
                return m_aSMot_Info[nMotor];
#endif
#endif
            }
        #endregion Parameter Function(SetParam...)

        #region Protocol - basic(updateCRC, MakeStuff, SendPacket)
            private int updateCRC(byte[] data_blk_ptr, int data_blk_size)
            {
                int i, j = 0;
                int[] anCrcTable = new int[256] { 
                    0x0000, 0x8005, 0x800F, 0x000A, 0x801B, 0x001E, 0x0014, 0x8011, 0x8033, 0x0036, 0x003C, 0x8039, 0x0028, 0x802D, 0x8027, 0x0022, 0x8063, 0x0066, 0x006C, 0x8069, 0x0078, 0x807D, 0x8077, 0x0072, 0x0050, 0x8055, 0x805F, 0x005A, 0x804B, 0x004E, 0x0044, 0x8041, 0x80C3, 0x00C6, 0x00CC, 0x80C9, 0x00D8, 0x80DD, 0x80D7, 0x00D2, 
                    0x00F0, 0x80F5, 0x80FF, 0x00FA, 0x80EB, 0x00EE, 0x00E4, 0x80E1, 0x00A0, 0x80A5, 0x80AF, 0x00AA, 0x80BB, 0x00BE, 0x00B4, 0x80B1, 0x8093, 0x0096, 0x009C, 0x8099, 0x0088, 0x808D, 0x8087, 0x0082, 0x8183, 0x0186, 0x018C, 0x8189, 0x0198, 0x819D, 0x8197, 0x0192, 0x01B0, 0x81B5, 0x81BF, 0x01BA, 0x81AB, 0x01AE, 0x01A4, 0x81A1, 
                    0x01E0, 0x81E5, 0x81EF, 0x01EA, 0x81FB, 0x01FE, 0x01F4, 0x81F1, 0x81D3, 0x01D6, 0x01DC, 0x81D9, 0x01C8, 0x81CD, 0x81C7, 0x01C2, 0x0140, 0x8145, 0x814F, 0x014A, 0x815B, 0x015E, 0x0154, 0x8151, 0x8173, 0x0176, 0x017C, 0x8179, 0x0168, 0x816D, 0x8167, 0x0162, 0x8123, 0x0126, 0x012C, 0x8129, 0x0138, 0x813D, 0x8137, 0x0132,
                    0x0110, 0x8115, 0x811F, 0x011A, 0x810B, 0x010E, 0x0104, 0x8101, 0x8303, 0x0306, 0x030C, 0x8309, 0x0318, 0x831D, 0x8317, 0x0312, 0x0330, 0x8335, 0x833F, 0x033A, 0x832B, 0x032E, 0x0324, 0x8321, 0x0360, 0x8365, 0x836F, 0x036A, 0x837B, 0x037E, 0x0374, 0x8371, 0x8353, 0x0356, 0x035C, 0x8359, 0x0348, 0x834D, 0x8347, 0x0342, 
                    0x03C0, 0x83C5, 0x83CF, 0x03CA, 0x83DB, 0x03DE, 0x03D4, 0x83D1, 0x83F3, 0x03F6, 0x03FC, 0x83F9, 0x03E8, 0x83ED, 0x83E7, 0x03E2, 0x83A3, 0x03A6, 0x03AC, 0x83A9, 0x03B8, 0x83BD, 0x83B7, 0x03B2, 0x0390, 0x8395, 0x839F, 0x039A, 0x838B, 0x038E, 0x0384, 0x8381, 0x0280, 0x8285, 0x828F, 0x028A, 0x829B, 0x029E, 0x0294, 0x8291, 
                    0x82B3, 0x02B6, 0x02BC, 0x82B9, 0x02A8, 0x82AD, 0x82A7, 0x02A2, 0x82E3, 0x02E6, 0x02EC, 0x82E9, 0x02F8, 0x82FD, 0x82F7, 0x02F2, 0x02D0, 0x82D5, 0x82DF, 0x02DA, 0x82CB, 0x02CE, 0x02C4, 0x82C1, 0x8243, 0x0246, 0x024C, 0x8249, 0x0258, 0x825D, 0x8257, 0x0252, 0x0270, 0x8275, 0x827F, 0x027A, 0x826B, 0x026E, 0x0264, 0x8261, 
                    0x0220, 0x8225, 0x822F, 0x022A, 0x823B, 0x023E, 0x0234, 0x8231, 0x8213, 0x0216, 0x021C, 0x8219, 0x0208, 0x820D, 0x8207, 0x0202 };
                int nCrc_accum = 0;
                for (j = 0; j < data_blk_size; j++) nCrc_accum = (nCrc_accum << 8) ^ anCrcTable[(((nCrc_accum >> 8) ^ data_blk_ptr[j]) & 0xFF)];
                return nCrc_accum;
            }
            private void MakeStuff(ref byte[] pBuff)
            {
                int nStuff = 0;
                int[] pnIndex = new int[pBuff.Length];
                Array.Clear(pnIndex, 0, pnIndex.Length);
                int nCnt = 0;
                for (int i = 5; i < pBuff.Length; i++)
                {
                    switch (nStuff)
                    {
                        case 0: { if (pBuff[i] == 0xff) nStuff++; } break;
                        case 1: { if (pBuff[i] == 0xff) nStuff++; else nStuff = 0; } break;
                        case 2:
                            {
                                if (pBuff[i] == 0xfd)
                                {
                                    nStuff++;
                                    pnIndex[nCnt++] = i;
                                }
                                else
                                {
                                    nStuff = 0;
                                }
                            }
                            break;
                    }
                }
                if (nCnt > 0)
                {
                    byte[] pBuff2 = new byte[pBuff.Length];
                    Array.Copy(pBuff, pBuff2, pBuff.Length);
                    Array.Resize<byte>(ref pBuff, pBuff2.Length + nCnt);
                    int nIndex = 0;
                    int nPos = 0;
                    foreach (byte byTmp in pBuff)
                    {
                        pBuff[nIndex + nPos] = pBuff2[nIndex];
                        if (nIndex == pnIndex[nPos])
                        {
                            pBuff[nIndex + nPos + 1] = 0xfd;
                            nPos++;
                        }
                        nIndex++;
                    }
                    pBuff2 = null;
                }
                pnIndex = null;
            }
            private void SendPacket(int nPortIndex, byte[] buffer, int nLength) { if (m_lstSerial[nPortIndex].IsConnect() == true) m_lstSerial[nPortIndex].SendPacket(buffer, nLength); }
        #endregion Protocol - basic(updateCRC, MakeStuff, SendPacket)
            
        #region Packet_Raw

            public string MakePingPacket_String(int nMotorRealID, int nProtocol_Version)
            {
                byte[] pbyteBuffer = MakePingPacket(nMotorRealID, nProtocol_Version);

                string strPing = String.Empty;// "=>";
                foreach (byte byData in pbyteBuffer) strPing += String.Format(",0x{0}", Ojw.CConvert.IntToHex(byData, 2));
                return strPing;
            }
            private byte[] MakePingPacket(int nMotorRealID, int nProtocol_Version)
            {
                int i = 0;
                byte[] pbyteBuffer;
                if (nProtocol_Version == 1)
                {
                    int nLength = 2; // 파라미터의 갯수 + 2
                    int nDefaultSize = 4;
                    pbyteBuffer = new byte[nDefaultSize + nLength];
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = (byte)(nMotorRealID & 0xff);
                    pbyteBuffer[i++] = (byte)(nLength & 0xff);
                    pbyteBuffer[i++] = (byte)(0x01);

                    int nCrc = 0;
                    for (int j = 2; j < pbyteBuffer.Length - 1; j++) nCrc += pbyteBuffer[j];
                    pbyteBuffer[i++] = (byte)(~nCrc & 0xff);

                    //pbyteBuffer[pbyteBuffer.Length - 1] = (byte)(nCrc & 0xff);

                }
                else // m_aSParam_Axis[nAxis].nProtocol_Version == 2
                {
                    int nLength = 3;
                    int nDefaultSize = 7;
                    pbyteBuffer = new byte[nDefaultSize + nLength];
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = 0xff;
        #region Packet 2.0
                    pbyteBuffer[i++] = 0xfd;
                    pbyteBuffer[i++] = 0x00;
        #endregion Packet 2.0
                    pbyteBuffer[i++] = (byte)(nMotorRealID & 0xff);
                    pbyteBuffer[i++] = (byte)(nLength & 0xff);
                    pbyteBuffer[i++] = (byte)((nLength >> 8) & 0xff);
                    pbyteBuffer[i++] = (byte)(0x01);

                    MakeStuff(ref pbyteBuffer);
                    int nCrc = updateCRC(pbyteBuffer, pbyteBuffer.Length - 2);
                    pbyteBuffer[pbyteBuffer.Length - 2] = (byte)(nCrc & 0xff);
                    pbyteBuffer[pbyteBuffer.Length - 1] = (byte)((nCrc >> 8) & 0xff);

                    //SendPacket(nSerialIndex, pbyteBuffer, pbyteBuffer.Length);
                }
                return pbyteBuffer;
            }

            public void Write_Ping(int nProtocol_Version, int nSerialIndex, int nMotor)
            {
                int nID = m_aSParam[nMotor].nRealID;
                byte [] pbyteBuffer = MakePingPacket(nID, nProtocol_Version);
                SendPacket(nSerialIndex, pbyteBuffer, pbyteBuffer.Length);
            }
            public void Write_Ping()
            {
                for (int nIndex = 0; nIndex < m_lstPort.Count; nIndex++)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        int nID = 254;
                        byte[] pbyteBuffer = MakePingPacket(nID, 2 - i);
                        m_CTmr_AutoSet.Set();
                        SendPacket(nIndex, pbyteBuffer, pbyteBuffer.Length);
                        Ojw.CTimer.Wait(100); // 나중에 handshake 로 바꿀것.
                    }
                }
#if false
                if (m_lstSCheckMotorType.Count > 0)
                {
                    foreach (SCheckMotorType_t SType in m_lstSCheckMotorType)
                    {
                        //int nProtocol = 2;
                        //int nCommIndex = 0;
                        int nID = 254;
                        //int nTorqAddress = 64;

                        //SetTorq(-1, bOn); 
                        byte[] pbyteBuffer = MakePingPacket(nID, SType.nProtocol);
                        SendPacket(SType.nCommIndex, pbyteBuffer, pbyteBuffer.Length);
                    }
                }
#endif
            }
            public void Write_Command(int nMotor, int nCommand) { Write_Command(m_aSParam[nMotor].nProtocol, m_aSParam[nMotor].nCommIndex, nMotor, nCommand); }            
            public void Write_Command(int nProtocol_Version, int nSerialIndex, int nMotor, int nCommand)
            {
                int i;
                //int nID = ((nAxis == 254) ? 254 : m_aSMot[nAxis].nID);
                if (m_aSParam[nMotor].nProtocol == 1)
                {
                    int nLength = 1 + 2;
                    int nDefaultSize = 6;
                    byte[] pbyteBuffer = new byte[nDefaultSize + nLength];
                    i = 0;
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = (byte)(m_aSParam[nMotor].nRealID & 0xff);
                    pbyteBuffer[i++] = (byte)(nLength & 0xff);
                    pbyteBuffer[i++] = (byte)(nCommand & 0xff);

                    int nCrc = 0;
                    for (int j = 2; i < pbyteBuffer.Length - 1; j++) nCrc += pbyteBuffer[j];
                    pbyteBuffer[i++] = (byte)(~nCrc & 0xff);

                    pbyteBuffer[pbyteBuffer.Length - 1] = (byte)(nCrc & 0xff);

                    SendPacket(nSerialIndex, pbyteBuffer, pbyteBuffer.Length);
                }
                else // if (m_aSParam_Axis[nAxis].nProtocol_Version == 2)
                {
                    int nLength = 3;
                    int nDefaultSize = 7;
                    byte[] pbyteBuffer = new byte[nDefaultSize + nLength];
                    i = 0;
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = 0xfd;
                    pbyteBuffer[i++] = 0x00;
                    pbyteBuffer[i++] = (byte)(m_aSParam[nMotor].nRealID & 0xff);
                    pbyteBuffer[i++] = (byte)(nLength & 0xff);
                    pbyteBuffer[i++] = (byte)((nLength >> 8) & 0xff);
                    pbyteBuffer[i++] = (byte)(nCommand & 0xff);

                    MakeStuff(ref pbyteBuffer);
                    int nCrc = updateCRC(pbyteBuffer, pbyteBuffer.Length - 2);
                    pbyteBuffer[pbyteBuffer.Length - 2] = (byte)(nCrc & 0xff);
                    pbyteBuffer[pbyteBuffer.Length - 1] = (byte)((nCrc >> 8) & 0xff);

                    SendPacket(nSerialIndex, pbyteBuffer, pbyteBuffer.Length);
                }
            }
            public void Write(int nMotor, int nCommand, int nAddress, params byte[] pbyDatas) { Write2(m_aSParam[nMotor].nProtocol, m_aSParam[nMotor].nCommIndex, nMotor, nCommand, nAddress, pbyDatas); }
            public void Write2(int nProtocol_Version, int nSerialIndex, int nMotorRealID, int nCommand, int nAddress, params byte[] pbyDatas)
            {
                int i;

                //int nID = 0;//((nAxis == 254) ? 254 : m_aSMot[nAxis].nID);

                i = 0;
                if (nProtocol_Version == 1)
                {
                    int nLength = 2 + ((pbyDatas != null) ? pbyDatas.Length + 1 : 0); // 파라미터의 갯수 + 2
                    int nDefaultSize = 4;
                    byte[] pbyteBuffer = new byte[nDefaultSize + nLength];
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = (byte)(nMotorRealID & 0xff);
                    pbyteBuffer[i++] = (byte)(nLength & 0xff);
                    pbyteBuffer[i++] = (byte)(nCommand & 0xff);
                    if (pbyDatas != null)
                    {
                        pbyteBuffer[i++] = (byte)(nAddress & 0xff);
                        foreach (byte byData in pbyDatas) pbyteBuffer[i++] = byData;
                    }
                    int nCrc = 0;
                    for (int j = 2; j < pbyteBuffer.Length - 1; j++) nCrc += pbyteBuffer[j];
                    pbyteBuffer[i++] = (byte)(~nCrc & 0xff);
                    
                    SendPacket(nSerialIndex, pbyteBuffer, pbyteBuffer.Length);
                }
                else // m_aSParam_Axis[nAxis].nProtocol_Version == 2
                {
                    int nLength = 3 + 2 + pbyDatas.Length;
                    int nDefaultSize = 7;
                    byte[] pbyteBuffer = new byte[nDefaultSize + nLength];
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = 0xff;
        #region Packet 2.0
                    pbyteBuffer[i++] = 0xfd;
                    pbyteBuffer[i++] = 0x00;
        #endregion Packet 2.0
                    pbyteBuffer[i++] = (byte)(nMotorRealID & 0xff);
                    pbyteBuffer[i++] = (byte)(nLength & 0xff);
                    pbyteBuffer[i++] = (byte)((nLength >> 8) & 0xff);
                    pbyteBuffer[i++] = (byte)(nCommand & 0xff);
                    pbyteBuffer[i++] = (byte)(nAddress & 0xff);
                    pbyteBuffer[i++] = (byte)((nAddress >> 8) & 0xff);
                    if (pbyDatas != null)
                        foreach (byte byData in pbyDatas) pbyteBuffer[i++] = byData;

                    MakeStuff(ref pbyteBuffer);
                    int nCrc = updateCRC(pbyteBuffer, pbyteBuffer.Length - 2);
                    pbyteBuffer[pbyteBuffer.Length - 2] = (byte)(nCrc & 0xff);
                    pbyteBuffer[pbyteBuffer.Length - 1] = (byte)((nCrc >> 8) & 0xff);

                    SendPacket(nSerialIndex, pbyteBuffer, pbyteBuffer.Length);
                }
            }
            public void Writes(int nProtocol_Version, int nSerialIndex, int nAddress, int nDataLength_without_ID, int nMotorCnt, params byte[] pbyDatas)
            {
                int i = 0;
                int nLength = (nDataLength_without_ID + 1) * nMotorCnt;
                if (nProtocol_Version != 1) // xl_320 은 따로 해주도록 한다. 나중에...
                {
                    byte[] pbyteBuffer = new byte[2 + nLength];
                    pbyteBuffer[i++] = (byte)(nDataLength_without_ID & 0xff);
                    pbyteBuffer[i++] = (byte)((nDataLength_without_ID >> 8) & 0xff);
                    Array.Copy(pbyDatas, 0, pbyteBuffer, i, nLength);
                    Write2(nProtocol_Version, nSerialIndex, 0xfe, 0x83, nAddress, pbyteBuffer);
                    pbyteBuffer = null;
                }
                else // (== 1)
                {
                    // Instruction(1) + Address(2) + Data_Length(2) + 
                    byte[] pbyteBuffer = new byte[1 + nLength];
                    pbyteBuffer[i++] = (byte)(nDataLength_without_ID & 0xff);
                    Array.Copy(pbyDatas, 0, pbyteBuffer, i, nLength);
                    Write2(nProtocol_Version, nSerialIndex, 0xfe, 0x83, nAddress, pbyteBuffer);
                    pbyteBuffer = null;
                }
            }
#if true
#else
            public void Writes(int nProtocol_Version, int nSerialIndex, int nAddress, int nDataLength_without_ID, int nMotorCnt, params byte[] pbyDatas)
            {
                int i = 0;
                int nLength = (nDataLength_without_ID + 1) * nMotorCnt;
                if (nProtocol_Version != 1)
                {
                    byte[] pbyteBuffer = new byte[2 + nLength];
                    pbyteBuffer[i++] = (byte)(nDataLength_without_ID & 0xff);
                    pbyteBuffer[i++] = (byte)((nDataLength_without_ID >> 8) & 0xff);
                    Array.Copy(pbyDatas, 0, pbyteBuffer, i, nLength);
                    Write2(nProtocol_Version, nSerialIndex, 0xfe, 0x83, nAddress, pbyteBuffer);
                    pbyteBuffer = null;
                }
                else // (== 1)
                {
                    // Instruction(1) + Address(2) + Data_Length(2) + 
                    byte[] pbyteBuffer = new byte[1 + nLength];
                    pbyteBuffer[i++] = (byte)(nDataLength_without_ID & 0xff);
                    Array.Copy(pbyDatas, 0, pbyteBuffer, i, nLength);
                    Write2(nProtocol_Version, nSerialIndex, 0xfe, 0x83, nAddress, pbyteBuffer);
                    pbyteBuffer = null;
                }
            }
            public void Writes(int nProtocol_Version, int nSerialIndex, int nAddress, int nDataLength_without_ID, int nMotorCnt, params byte[] pbyDatas)
            {
                int i = 0;
                int nLength = (nDataLength_without_ID + 1) * nMotorCnt;
                if (nProtocol_Version != 1)
                {
                    byte[] pbyteBuffer = new byte[2 + nLength];
                    pbyteBuffer[i++] = (byte)(nDataLength_without_ID & 0xff);
                    pbyteBuffer[i++] = (byte)((nDataLength_without_ID >> 8) & 0xff);
                    Array.Copy(pbyDatas, 0, pbyteBuffer, i, nLength);
                    Write(0xfe, 0x83, nAddress, pbyteBuffer);
                    pbyteBuffer = null;
                }
                else
                {
                    byte[] pbyteBuffer = new byte[1 + nLength];
                    pbyteBuffer[i++] = (byte)(nDataLength_without_ID & 0xff);
                    Array.Copy(pbyDatas, 0, pbyteBuffer, i, nLength);
                    Write(0xfe, 0x83, nAddress, pbyteBuffer);
                    pbyteBuffer = null;
                }
            }
            // int nMotor, int nSpeed
            public void Writes_Speed(params int [] anMotor_and_Speed)
            {
                int nCount = anMotor_and_Speed.Length / 2;
                byte[] pbyTmp_Short;
                for (int i = 0; i < nCount; i++)
                {
                    // 속도

                    pbyTmp_Short = Ojw.CConvert.ShortToBytes((short)anMotor_and_Speed[i * 2 + 1]);
                Array.Copy(pbyTmp_Short, 0, pbyteBuffer_Spd, nIndex_Spd, pbyTmp_Short.Length);
            }
#endif
            public void Reads(int nSerialIndex, int nAddress, int nDataLength, params byte[] pbyIDs)
            {
                int nProtocol_Version = 2; // 프로토콜 2 버전부터 싱크리드가 지원
                byte[] pbyteBuffer = new byte[2 + pbyIDs.Length];
                Array.Copy(Ojw.CConvert.ShortToBytes((short)nDataLength), pbyteBuffer, 2);
                Array.Copy(pbyIDs, 0, pbyteBuffer, 2, pbyIDs.Length);
                Write2(nProtocol_Version, nSerialIndex, 0xfe, 0x82, nAddress, pbyteBuffer);
            }
            public void Reads(int nSerialIndex, int nAddress, int nDataLength, params int[] pnIDs)
            {
                int nProtocol_Version = 2; // 프로토콜 2 버전부터 싱크리드가 지원
                byte[] pbyteBuffer = new byte[2 + pnIDs.Length];
                int i = 2;
                Array.Copy(Ojw.CConvert.ShortToBytes((short)nDataLength), pbyteBuffer, i);
                foreach (int nData in pnIDs) pbyteBuffer[i++] = (byte)(nData & 0xff);
                Write2(nProtocol_Version, nSerialIndex, 0xfe, 0x82, nAddress, pbyteBuffer);
            }
        #endregion Packet_Raw

        #region Control
            public void SetTorq(int nMotor, bool bOn)
            {
                if (bOn) { if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return; }
                if (bOn == false) m_aSMot[nMotor].bInit_Value = false;

                m_aSMot[nMotor].nStatus_Torq_Prev = m_aSMot[nMotor].nStatus_Torq;
                m_aSMot[nMotor].nStatus_Torq = Ojw.CConvert.BoolToInt(bOn);
                //if (nMotor < 0) 

                if (m_aSParam[nMotor].EMot != EMotor_t.SG_90)
                {
                    Write2(m_aSParam[nMotor].nProtocol, m_aSParam[nMotor].nCommIndex, ((nMotor < 0) ? 254 : (m_aSParam[nMotor].nRealID)), 0x03, m_aSAddress[nMotor].nTorq_64_1, (byte)((bOn == true) ? 1 : 0));
                    
                }
            }
            public void SetTorq(bool bOn)
            {
                if (bOn) { if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return; }
                for (int i = 0; i < m_aSParam.Length; i++) m_aSMot[i].bInit_Value = false;
                for (int i = 0; i < m_lstPort.Count; i++)
                {
                    // protocol 1(AX)
                    Write2(1, i, 254, 0x03, _ADDRESS_TORQ_AX, (byte)((bOn == true) ? 1 : 0));
                    // protocol 2(XL-430
                    Write2(2, i, 254, 0x03, _ADDRESS_TORQ_XL_430, (byte)((bOn == true) ? 1 : 0));
                }
            }
        #region 같은 프로토콜, 같은 시리얼포트, 같은 종류의 모터로만 구성해야 한다.
            public void SetTorq(bool bOn, params int[] anMotors)
            {
                if (bOn) { if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return; }
                byte[] pbyteBuffer = new byte[anMotors.Length * 2];
                int i = 0;
                byte byVal = (byte)((bOn == true) ? 1 : 0);
                foreach (int nMotor in anMotors)
                {
                    pbyteBuffer[i++] = (byte)(Get_RealID(nMotor) & 0xff);
                    pbyteBuffer[i++] = byVal;
                }
                if (anMotors.Length > 0)
                {
                    int nProtocol = m_aSParam[anMotors[0]].nProtocol;
                    int nSerialIndex = m_aSParam[anMotors[0]].nCommIndex;
                    Writes(nProtocol, nSerialIndex, m_aSAddress[anMotors[0]].nTorq_64_1, m_aSAddress[anMotors[0]].nTorq_Size_1, anMotors.Length, pbyteBuffer);
                }
            }
            //public void SetTorq(bool bOn, params int[] nMotor)
            //{
            //    for (int nCommand = 0; nCommand < 3; nCommand++)
            //    {
            //        for (int nSerialIndex = 0; nSerialIndex < m_lstPort.Count; nSerialIndex++)
            //        {
            //            for (int nProtocol = 1; nProtocol <= 2; nProtocol++)
            //            {
            //                Writes(nProtocol, nSerialIndex, nAddress, nAddress_Size, nMotorPtCount, m_abyBuffer);
            //            }
            //        }
            //    }
            //}
            public void SetTorqOn(params int[] anMotors) { SetTorq(true, anMotors); }
            public void SetTorqOff(params int[] anMotors) { SetTorq(false, anMotors); }
        #endregion 같은 프로토콜, 같은 시리얼포트, 같은 종류의 모터로만 구성해야 한다.

            //  Move  ///////////////////////////
            private bool m_bIgnoredLimit = false;
            public void SetLimitEn(bool bOn) { m_bIgnoredLimit = !bOn; }
            public bool GetLimitEn() { return !m_bIgnoredLimit; }
            public int Clip(int nLimitValue_Up, int nLimitValue_Dn, int nData)
            {
                if (GetLimitEn() == false) return nData;

                int nRet = ((nData > nLimitValue_Up) ? nLimitValue_Up : nData);
                return ((nRet < nLimitValue_Dn) ? nLimitValue_Dn : nRet);
            }
            public float Clip(float fLimitValue_Up, float fLimitValue_Dn, float fData)
            {
                if (GetLimitEn() == false) return fData;
                float fRet = ((fData > fLimitValue_Up) ? fLimitValue_Up : fData);
                return ((fRet < fLimitValue_Dn) ? fLimitValue_Dn : fRet);
            }

            public int CalcLimit_Evd(int nMotor, int nValue)
            {
                //if (Get_Flag_Mode(nAxis) == 0)// || (Get_Flag_Mode(nAxis) == 2))
                //{
                int nPulse = nValue;// &0x4000;
                //if (m_bMultiTurn == false)
                //{
                //nValue &= 0x4000;
                //nValue &= 0x3fff;
                //}

                //nValue &= 0x3fff;
                int nUp = 10000000;
                int nDn = -nUp;
                if (m_aSParam[nMotor].fLimitUp != 0) nUp = CalcAngle2Evd(nMotor, m_aSParam[nMotor].fLimitUp);
                if (m_aSParam[nMotor].fLimitDn != 0) nDn = CalcAngle2Evd(nMotor, m_aSParam[nMotor].fLimitDn);
                if (nUp < nDn) { int nTmp = nUp; nUp = nDn; nDn = nTmp; }
                return (Clip(nUp, nDn, nValue) | nPulse);
                //}
                //return nValue;
            }
            public float CalcLimit_Angle(int nMotor, float fValue)
            {
                //if (Get_Flag_Mode(nAxis) == 0)// || (Get_Flag_Mode(nAxis) == 2))
                //{
                float fUp = 1000000.0f;
                float fDn = -fUp;
                if (m_aSParam[nMotor].fLimitUp != 0) fUp = m_aSParam[nMotor].fLimitUp;
                if (m_aSParam[nMotor].fLimitDn != 0) fDn = m_aSParam[nMotor].fLimitDn;
                return Clip(fUp, fDn, fValue);
                //}
                //return fValue;
            }
            public int CalcAngle2Evd(int nMotor, float fValue)
            {
                fValue *= ((m_aSParam[nMotor].nDir == 0) ? 1.0f : -1.0f);
                int nData = 0;
                //if (Get_Flag_Mode(nAxis) != 0)   // 속도제어
                //{
                //    nData = (int)Math.Round(fValue);
                //    //Ojw.CMessage.Write("Speed Turn");
                //}
                //else
                //{
                    // 위치제어
                    nData = (int)Math.Round((m_aSParam[nMotor].fMechMove * fValue) / m_aSParam[nMotor].fDegree);
                    nData = nData + (int)Math.Round(m_aSParam[nMotor].fCenterPos);
                //}
                return nData;
            }
            public float CalcEvd2Angle(int nMotor, int nValue)
            {
                float fValue = ((m_aSParam[nMotor].nDir == 0) ? 1.0f : -1.0f);
                float fValue2 = 0.0f;
                //if (Get_Flag_Mode(nMotor) != 0)   // 속도제어
                //    fValue2 = (float)nValue * fValue;
                //else                                // 위치제어
                //{
                    fValue2 = (float)(((m_aSParam[nMotor].fDegree * ((float)(nValue - (int)Math.Round(m_aSParam[nMotor].fCenterPos)))) / m_aSParam[nMotor].fMechMove) * fValue);
                //}
                return fValue2;
            }
            public void Set_OperationMode(int nMotor, EOperationMode_t EOper)
            {
                m_aSMot[nMotor].EOperationMode = EOper;
                m_aSMot[nMotor].bOperationMode = true;
            }
            //public void Set_Evd(int nMotor, int nEvd, float fRpm)
            //{
            //}
            //public void Set_Evd(int nMotor, int nEvd)
            //{
            //    if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return;
            //    Push(nMotor);
            //    //Read_Motor_Push(nAxis);
            //    m_aSMot[nMotor].EEnable = EEnable_t._Position;
            //    //m_aSMot[nAxis].
            //    //Set_Flag_Mode(nAxis, false);
            //    if (m_aSMot[nMotor].EOperationMode_Prev != m_aSMot[nMotor].EOperationMode) m_aSMot[nMotor].bOperationMode = true;

            //    m_aSMot[nMotor].nValue = (long)CalcLimit_Evd(nMotor, nEvd);
            //    //Set_Flag_NoAction(nAxis, false);
            //    //Push_Id(nAxis);	
            //}
            //public long Get_Evd(int nMotor) { return m_aSMot[nMotor].nValue; }


            // [목표]
            //////////////////////////////////// 전송 단계가 아닌 Set의 단계에서 프로토콜을 적재하도록 수정하자.
            //////////////////////////////////// SyncWrite 가 아닌 BulkWrite 를 사용하자.(아직...)

            public void Set(int nMotor, float fAngle, float fRpm)
            {
                if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return;
                Push(nMotor);
                // Protocol1, XL-320 을 바퀴냐 포지션이냐만 결정하자.

                m_aSMot[nMotor].EEnable = EEnable_t._Position; 
                if (m_aSMot[nMotor].EOperationMode_Prev != m_aSMot[nMotor].EOperationMode) m_aSMot[nMotor].bOperationMode = true;
                m_aSMot[nMotor].nValue = CalcLimit_Evd(nMotor, CalcAngle2Evd(nMotor, fAngle));
                m_aSMot[nMotor].bSetValue2 = true; // Rpm 을 따로 설정한 경우만 Set 한다.
                m_aSMot[nMotor].nValue2 = CalcRpm2Raw(nMotor, fRpm);
                m_aSMot[nMotor].fAngle_Back = CalcEvd2Angle(nMotor, m_aSMot[nMotor].nValue);
            }
            public void Set(int nMotor, float fAngle)
            {
                if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return;
                Push(nMotor);
                m_aSMot[nMotor].EEnable = EEnable_t._Position;
                Set_OperationMode(nMotor, EOperationMode_t._Position);
                if (m_aSMot[nMotor].EOperationMode_Prev != m_aSMot[nMotor].EOperationMode) m_aSMot[nMotor].bOperationMode = true;
                
                m_aSMot[nMotor].nValue = CalcLimit_Evd(nMotor, CalcAngle2Evd(nMotor, fAngle));
                m_aSMot[nMotor].fAngle_Back = CalcEvd2Angle(nMotor, m_aSMot[nMotor].nValue);
            }
            public float Get(int nMotor) { return m_aSMot[nMotor].fAngle_Back; }
            //public float Get(int nMotor) { return CalcEvd2Angle(nMotor, (int)m_aSMot[nMotor].nValue); }

            //private List<byte> m_lstPt = new List<byte>();
            private byte[] m_abyBuffer = new byte[256];
            private byte[] m_abyBuffer_Speed = new byte[256];
            private int m_nBuffer = 0;
            private int m_nBuffer_Speed = 0;
            private bool IsBitMinus(Ojw.CMonster.EMotor_t EMotor)
            {
                bool bRet = false;
                if (EMotor == EMotor_t.AX_12)
                {
                }
                return bRet;
            }
            public void Set_Turn(int nMotor, float fRpm)
            {
                if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return;
                Push(nMotor);
                m_aSMot[nMotor].EEnable = EEnable_t._Speed;
                Set_OperationMode(nMotor, EOperationMode_t._Speed);
                if (m_aSMot[nMotor].EOperationMode_Prev != m_aSMot[nMotor].EOperationMode) m_aSMot[nMotor].bOperationMode = true;
                m_aSMot[nMotor].bSetValue2 = true; // Rpm 을 따로 설정한 경우만 Set 한다.
                m_aSMot[nMotor].nValue2 = CalcRpm2Raw(nMotor, fRpm);
            }
            // float Get(int nMotor) { return CalcEvd2Angle(nMotor, (int)m_aSMot[nMotor].nValue); }
            private List<int> m_lstMotors = new List<int>();
            public void Send_Motor() { Send_Motor(0); }
            public void Send_Motor(int nMilliseconds)
            {
                if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return;
#if false
                if (m_nProtocolVersion == 1)
                    {
                        switch (nMode)
                        {
                            case 0: Writes(nAddr_Spd, nAddr_Size, nCnt2, pbyteBuffer_Spd); Writes(nAddr_Pos, nAddr_Size, nCnt2, pbyteBuffer_Pos); break;
                            case 1: Writes(nAddr_Spd, nAddr_Size, nCnt2, pbyteBuffer_Spd); break;
                            //case 2: Writes(102, 4, nCnt, pbyteBuffer); break;
                            default: break;
                        }
                    }
                    else
                    {
                        
                        switch (nMode)
                        {
                            case 0: Writes(nAddr_Pos_Spd, nAddr_Size, nCnt2, pbyteBuffer_Spd); Writes(nAddr_Pos, nAddr_Size, nCnt2, pbyteBuffer_Pos); break;
                            case 1: Writes(nAddr_Spd, nAddr_Size, nCnt2, pbyteBuffer_Spd); break;
                            //case 2: Writes(102, 4, nCnt, pbyteBuffer); break;
                            default: break;
                        }
                    }
#endif
                //int nAddr = 0;
                int nMotorPtCount = 0;
                int nMotorPtCount_Speed = 0;
                int nMotor = 0;
                int nAddress;
                int nAddress_Size;
                int nAddress_Speed = -1;
                int nAddress_Speed_Size = -1;
                int nMotorCnt = m_nMotorCnt;
                byte[] buffer = new byte[4];
                // Setting - Operation
                for (int nCommand = 0; nCommand < 3; nCommand++)
                {
                    for (int nSerialIndex = 0; nSerialIndex < m_lstPort.Count; nSerialIndex++)
                    {
                        for (int nProtocol = 1; nProtocol <= 2; nProtocol++)
                        {
                            m_lstMotors.Clear();
                            m_nBuffer = 0;
                            nMotorPtCount = 0;
                            nAddress = -1;
                            nAddress_Size = 0;

                            m_nBuffer_Speed = 0;
                            nMotorPtCount_Speed = 0;
                            nAddress_Speed = -1;
                            nAddress_Speed_Size = -1;
                            for (int i = 0; i < nMotorCnt; i++)
                            {
                                //if ((nCommand == 2) && (nProtocol == 2)) nMotor = Pop(); // 별 의미 없긴 하지만... 일단 Pop() 으로 비워주어야 하니...
                                //else nMotor = m_anEn[i];
                                nMotor = m_anEn[i];
                                if ((m_aSParam[nMotor].nProtocol == nProtocol) && (m_aSParam[nMotor].nCommIndex == nSerialIndex))
                                {
                                    //if (m_aSParam[nMotor].EMot == EMotor_t.XL_320) // XL_320 은 아직 논외로 친다.
                                
                                    switch (nCommand)
                                    {
                                        case 0 :
                                            if (m_aSMot[nMotor].bOperationMode == true)
                                            {
                                                if (nAddress < 0)
                                                {
                                                    nAddress = m_aSAddress[nMotor].nMode_Operating_11_1;
                                                    nAddress_Size = m_aSAddress[nMotor].nMode_Operating_Size_1;
                                                }

                                                if ((m_aSParam[nMotor].EMot == EMotor_t.AX_12) || (m_aSParam[nMotor].EMot == EMotor_t.AX_18) || (m_aSParam[nMotor].EMot == EMotor_t.XL_320))
                                                {
                                                    m_abyBuffer[m_nBuffer++] = (byte)Get_RealID(nMotor);
                                                    if ((m_aSMot[nMotor].EOperationMode == EOperationMode_t._Position) || (m_aSMot[nMotor].EOperationMode == EOperationMode_t._Position_Multi) || (m_aSMot[nMotor].EOperationMode == EOperationMode_t._Position_Amp))
                                                    {
                                                        m_abyBuffer[m_nBuffer++] = 0x00;
                                                        m_abyBuffer[m_nBuffer++] = 0x00;
                                                        m_abyBuffer[m_nBuffer++] = 0xff;
                                                        m_abyBuffer[m_nBuffer++] = 0x03;
                                                        if (m_aSParam[nMotor].EMot == EMotor_t.XL_320) m_abyBuffer[m_nBuffer++] = 0x02;
                                                    }
                                                    else
                                                    {
                                                        m_abyBuffer[m_nBuffer++] = 0x00;
                                                        m_abyBuffer[m_nBuffer++] = 0x00;
                                                        m_abyBuffer[m_nBuffer++] = 0x00;
                                                        m_abyBuffer[m_nBuffer++] = 0x00;
                                                        if (m_aSParam[nMotor].EMot == EMotor_t.XL_320) m_abyBuffer[m_nBuffer++] = 0x01;
                                                    }
                                                }
                                                else
                                                {
                                                    m_abyBuffer[m_nBuffer++] = (byte)Get_RealID(nMotor);
                                                    m_abyBuffer[m_nBuffer++] = (byte)m_aSMot[nMotor].EOperationMode;
                                                }


                                                m_lstMotors.Add(nMotor);

                                                nMotorPtCount++;
                                            }
                                            break;
                                        case 1:
                                            bool bSet = false;
                                            //bool bSpeed = false;
                                            if (nMilliseconds > 0)
                                            {
                                                if (
                                                     (m_aSMot[nMotor].EOperationMode == EOperationMode_t._Position) ||
                                                     (m_aSMot[nMotor].EOperationMode == EOperationMode_t._Position_Multi) ||
                                                     (m_aSMot[nMotor].EOperationMode == EOperationMode_t._Position_Amp)
                                                    )
                                                {
                                                    float fRpm = (float)Math.Abs(CalcTime2Rpm(CalcEvd2Angle(nMotor, m_aSMot[nMotor].nValue) - CalcEvd2Angle(nMotor, m_aSMot[nMotor].nValue_Prev), (float)nMilliseconds));
                                                    if (fRpm > 1)//!= 0)//> 1)
                                                    {
                                                        bSet = true;
                                                        m_aSMot[nMotor].nValue2 = CalcRpm2Raw(nMotor, fRpm); // short
                                                    }
                                                }
                                            }
                                            if ((m_aSMot[nMotor].bSetValue2 == true) || (bSet == true))
                                            {
                                                if (m_aSMot[nMotor].EEnable == EEnable_t._Position)
                                                {
                                                    if (nAddress < 0)
                                                    {
                                                        nAddress = m_aSAddress[nMotor].nProfile_Vel_112_4;
                                                        nAddress_Size = m_aSAddress[nMotor].nProfile_Vel_Size_4;
                                                    }
                                                    m_abyBuffer[m_nBuffer++] = (byte)Get_RealID(nMotor);

                                                    if (nAddress_Size == 1)
                                                        m_abyBuffer[m_nBuffer++] = (byte)(m_aSMot[nMotor].nValue2 & 0xff);
                                                    else if (nAddress_Size == 2)
                                                    {
                                                        buffer = Ojw.CConvert.ShortToBytes((short)m_aSMot[nMotor].nValue2);
                                                        Array.Copy(buffer, 0, m_abyBuffer, m_nBuffer, nAddress_Size);
                                                        m_nBuffer += nAddress_Size;
                                                    }
                                                    else //(nAddress_Size == 4)
                                                    {
                                                        buffer = Ojw.CConvert.IntToBytes((int)m_aSMot[nMotor].nValue2);
                                                        Array.Copy(buffer, 0, m_abyBuffer, m_nBuffer, nAddress_Size);
                                                        m_nBuffer += nAddress_Size;
                                                    }
                                                    nMotorPtCount++;
                                                }
                                                else if (m_aSMot[nMotor].EEnable == EEnable_t._Speed)
                                                {
                                                    if (nAddress_Speed < 0)
                                                    {
                                                        nAddress_Speed = m_aSAddress[nMotor].nGoal_Vel_104_4; ;
                                                        nAddress_Speed_Size = m_aSAddress[nMotor].nGoal_Vel_Size_4;
                                                    }
                                                    m_abyBuffer_Speed[m_nBuffer_Speed++] = (byte)Get_RealID(nMotor);

                                                    if (nAddress_Speed_Size == 1)
                                                        m_abyBuffer_Speed[m_nBuffer_Speed++] = (byte)(m_aSMot[nMotor].nValue2 & 0xff);
                                                    else if (nAddress_Speed_Size == 2)
                                                    {
                                                        buffer = Ojw.CConvert.ShortToBytes((short)m_aSMot[nMotor].nValue2);
                                                        Array.Copy(buffer, 0, m_abyBuffer_Speed, m_nBuffer_Speed, nAddress_Speed_Size);
                                                        m_nBuffer_Speed += nAddress_Speed_Size;
                                                    }
                                                    else //(nAddress_Speed_Size == 4)
                                                    {
                                                        buffer = Ojw.CConvert.IntToBytes((int)Math.Abs(m_aSMot[nMotor].nValue2) & 0x3ff | (m_aSMot[nMotor].nValue2 < 0 ? 0x400 : 0x00)); // 10 번째 비트가 방향 비트
                                                        Array.Copy(buffer, 0, m_abyBuffer_Speed, m_nBuffer_Speed, nAddress_Speed_Size);
                                                        m_nBuffer_Speed += nAddress_Speed_Size;
                                                    }
                                                    nMotorPtCount_Speed++;
                                                }
                                            }
                                            break;
                                        case 2:
                                            // 여길 속도 모드면 들어오지 않도록 나중에 고칠 것.
                                            // 현재는 bSetValue2 == true 여도 들어오게 되어 있다.
                                            if (
                                                 (m_aSMot[nMotor].EOperationMode == EOperationMode_t._Position) ||
                                                 (m_aSMot[nMotor].EOperationMode == EOperationMode_t._Position_Multi) ||
                                                 (m_aSMot[nMotor].EOperationMode == EOperationMode_t._Position_Amp)
                                                )
                                            {
                                                if (nAddress < 0)
                                                {
                                                    nAddress = m_aSAddress[nMotor].nGoal_Pos_116_4;
                                                    nAddress_Size = m_aSAddress[nMotor].nGoal_Pos_Size_4;
                                                }
                                                m_abyBuffer[m_nBuffer++] = (byte)Get_RealID(nMotor);
                                                if (nAddress_Size == 1)
                                                    m_abyBuffer[m_nBuffer++] = (byte)m_aSMot[nMotor].nValue;//EOperationMode;
                                                else if (nAddress_Size == 2)
                                                {
                                                    buffer = Ojw.CConvert.ShortToBytes((short)m_aSMot[nMotor].nValue);
                                                    Array.Copy(buffer, 0, m_abyBuffer, m_nBuffer, nAddress_Size);
                                                    m_nBuffer += nAddress_Size;
                                                }
                                                else //(nAddress_Size == 4)
                                                {
                                                    buffer = Ojw.CConvert.IntToBytes((int)m_aSMot[nMotor].nValue);
                                                    Array.Copy(buffer, 0, m_abyBuffer, m_nBuffer, nAddress_Size);
                                                    m_nBuffer += nAddress_Size;
                                                }
                                                nMotorPtCount++;
                                            }
#if false // Speed 는 이미 앞전에 셋팅이 되었기 때문에 여기서는 필요 없다.
                                            else if (m_aSMot[nMotor].EOperationMode == EOperationMode_t._Speed)
                                            {
                                                if (nAddress < 0)
                                                {
                                                    nAddress = m_aSAddress[nMotor].nGoal_Vel_104_4;
                                                    nAddress_Size = m_aSAddress[nMotor].nGoal_Vel_Size_4;
                                                }
                                                m_abyBuffer[m_nBuffer++] = (byte)Get_RealID(nMotor);

                                                int nVal = (m_aSMot[nMotor].bSetValue2 == true) ? m_aSMot[nMotor].nValue2 : m_aSMot[nMotor].nValue;
                                                if (nAddress_Size == 1)
                                                    m_abyBuffer[m_nBuffer++] = (byte)nVal;
                                                else if (nAddress_Size == 2)
                                                {
                                                    // 이때는 10 비트가 부호비트. -> 나중에 고쳐 둘것.
                                                    buffer = Ojw.CConvert.ShortToBytes((short)nVal);
                                                    Array.Copy(buffer, 0, m_abyBuffer, m_nBuffer, nAddress_Size);
                                                    m_nBuffer += nAddress_Size;
                                                }
                                                else //(nAddress_Size == 4)
                                                {
                                                    buffer = Ojw.CConvert.IntToBytes((int)nVal);
                                                    Array.Copy(buffer, 0, m_abyBuffer, m_nBuffer, nAddress_Size);
                                                    m_nBuffer += nAddress_Size;
                                                }

                                                nMotorPtCount++;
                                            }
#endif
                                            break;
                                    }
                                    //nMotorPtCount++;
                                }
                                //else // SG-90
                                //{
                                //}
                            }
                            if (nMotorPtCount > 0)
                            {
                                Ojw.CTimer CTmr = new CTimer();
                                if (nCommand == 0)
                                {
                         //           CTmr.Set(); while (CTmr.Get() < 50) Thread.Sleep(1);
                                    SetTorqOff(m_lstMotors.ToArray());
                                }
                        //        CTmr.Set(); while (CTmr.Get() < 50) Thread.Sleep(1);//Ojw.CTimer.Wait(1);
                                Writes(nProtocol, nSerialIndex, nAddress, nAddress_Size, nMotorPtCount, m_abyBuffer);
                        //        CTmr.Set(); while (CTmr.Get() < 10) Thread.Sleep(1); //Ojw.CTimer.Wait(1);
                                //if (nCommand == 2)
                                //{
                                //    m_aSMot
                                //}

                                if (nCommand == 0)
                                {
                                    SetTorqOn(m_lstMotors.ToArray()); // 나중에는 토크가 기존에 살아 있던 것만 골라 살도록 한다.
                                }
                            }
                            if (nMotorPtCount_Speed > 0)
                            {
                                Writes(nProtocol, nSerialIndex, nAddress_Speed, nAddress_Speed_Size, nMotorPtCount_Speed, m_abyBuffer_Speed);
                            }
                        }
                    }
                }

                for (int i = 0; i < nMotorCnt; i++)
                {
                    nMotor = m_anEn[i];
                    Done(nMotor);
                    //Done(Pop());
                }
                //Pop()
                m_nMotorCnt = 0;
                m_nWait = nMilliseconds;
            }

        #endregion Control
            int m_nWait = 0;
            public void Delay(int nMilliseconds)
            {
                CTimer CTmr = new CTimer();
                CTmr.Set(); while (CTmr.Get() < nMilliseconds) Ojw.CTimer.Wait(1);
            }
            public void Wait()
            {
                if (m_nWait <= 0) return;
                Wait(m_nWait);
            }
            public void Wait_Per(float fPercent_0_1)
            {
                Wait((int)Math.Round((float)m_nWait * fPercent_0_1));
            }
            public void Wait(int nMilliseconds)
            {
                CTimer CTmr = new CTimer();
                CTmr.Set(); while (CTmr.Get() < nMilliseconds) Ojw.CTimer.Wait(1);
            }


        #region MotionFile
            // Set3D 하면 모델링에서 가져온 상태로 강제 재셋팅, -> PlayFile(파일이름) 하면 된다.
            private Ojw.C3d m_C3d = null;
            private Ojw.C3d.COjwDesignerHeader m_CHeader = null;//new C3d.COjwDesignerHeader();
            public void SetHeader(Ojw.C3d.COjwDesignerHeader CHeader)
            {
                m_CHeader = CHeader;
                for (int i = 0; i < m_CHeader.nMotorCnt; i++)
                {
                    // 0 : None, 1 : xl-320, 2 : xl_430(Default), 3 - ax-12
                    if (m_CHeader.pSMotorInfo[i].nHwMotorName == 2)
                    {
                        SetParam_MotorType(i, EMotor_t.XL_430);
                        SetParam_Dir(i, m_CHeader.pSMotorInfo[i].nMotorDir);
                    }
                    else if (m_CHeader.pSMotorInfo[i].nHwMotorName == 3)
                    {
                        SetParam_MotorType(i, EMotor_t.AX_12);
                        //SetParam_RealID
                        SetParam_Dir(i, m_CHeader.pSMotorInfo[i].nMotorDir);
                    }   
                        
                        //m_CHeader.pSMotorInfo[i].fLimit_Up,
                        //m_CHeader.pSMotorInfo[i].fLimit_Down,
                        //(float)m_CHeader.pSMotorInfo[i].nCenter_Evd,
                        //0,
                        //(float)m_CHeader.pSMotorInfo[i].nMechMove,
                        //m_CHeader.pSMotorInfo[i].fMechAngle);
                }
            }
            public void Set3D(Ojw.C3d C3dModel) { m_C3d = C3dModel; SetHeader(m_C3d.GetHeader()); }
            private const int _SIZE_MOTOR_MAX = 999;
            public void PlayFrame(int nLine, SMotion_t SMotion)
            {
                if (SMotion.nFrameSize <= 0) return;
                if ((nLine < 0) || (nLine >= SMotion.nFrameSize)) return;

                if ((m_bStop == false) && (m_bEms == false) && (m_bMotionEnd == false))
                {
                    //m_bStop = false; 
                    //for (int i = 0; i < _SIZE_MOTOR_MAX; i++) Set_Flag_Stop(i, false);
                    SetTorq(true);

                    for (int nAxis = 0; nAxis < m_nMotorCnt; nAxis++)
                    {
                        if (m_CHeader.pSMotorInfo[nAxis].nMotorControlType != 0) // 위치제어가 아니라면
                        {
                            //SetParam_Dir(nAxis, m_CHeader.pSMotorInfo[nAxis].nMotorDir);
                            Set_Turn(nAxis, SMotion.STable[nLine].anMot[nAxis]);
                        }
                        else
                        {
                            SetParam_Dir(nAxis, m_CHeader.pSMotorInfo[nAxis].nMotorDir);
                            
                            Set(nAxis, SMotion.STable[nLine].anMot[nAxis]);
                        }
                    }
                    Send_Motor(SMotion.STable[nLine].nTime);
                }
            }
            public void PlayFrame(SMotionTable_t STable)
            {
                if ((m_bStop == false) && (m_bEms == false) && (m_bMotionEnd == false))
                {
                    //m_bStop = false; 
                    //for (int i = 0; i < _SIZE_MOTOR_MAX; i++) Set_Flag_Stop(i, false);
                    SetTorq(true);
                    for (int nAxis = 0; nAxis < m_CHeader.nMotorCnt; nAxis++)
                    {
                        if (m_CHeader.pSMotorInfo[nAxis].nMotorControlType != 0) // 위치제어가 아니라면
                        {
                            //SetParam_Dir(nAxis, m_CHeader.pSMotorInfo[nAxis].nMotorDir);
                            //Set_Flag_Led(nAxis,
                            //    Get_Flag_Led_Green(STable.anLed[nAxis]),
                            //    Get_Flag_Led_Blue(STable.anLed[nAxis]),
                            //    Get_Flag_Led_Red(STable.anLed[nAxis])
                            //    );

                            Set_Turn(nAxis, STable.anMot[nAxis]);
                        }
                        else
                        {
                            //SetParam_Dir(nAxis, m_CHeader.pSMotorInfo[nAxis].nMotorDir);
                            //Set_Flag_Led(nAxis,
                            //    Get_Flag_Led_Green(STable.anLed[nAxis]),
                            //    Get_Flag_Led_Blue(STable.anLed[nAxis]),
                            //    Get_Flag_Led_Red(STable.anLed[nAxis])
                            //    );

                            Set(nAxis, STable.anMot[nAxis]);
                        }
                    }
                    Send_Motor(STable.nTime);
                }
            }
            public void PlayFile(string strFileName)
            {
                try
                {
                    if (m_C3d == null) return;
                    SMotion_t SMotion = new SMotion_t();
                    if (m_C3d.BinaryFileOpen(strFileName, out SMotion) == true)
                    {
                        if (SMotion.nFrameSize > 0)
                        {
                            m_bStart = true;

                            m_C3d.WaitAction_SetTimer();

                            foreach (SMotionTable_t STable in SMotion.STable)
                            {
                                if (STable.bEn == true)
                                {
                                    PlayFrame(STable);

                                    int nDelay = STable.nTime + STable.nDelay;
                                    if (nDelay > 0) m_C3d.WaitAction_ByTimer(nDelay);
                                }
                            }
                            m_bStart = false;
                            m_bMotionEnd = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Ojw.CMessage.Write_Error("Error -> PlayMotion(), " + ex.ToString());
                }
            }
        #endregion MotionFile

        #region Background
            private void Push(int nMotor) { if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return; if (IsCmd(nMotor) == false) m_anEn[m_nMotorCnt++] = nMotor; }
            private int Pop() { if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return -1; if (m_nMotorCnt > 0) return m_anEn[--m_nMotorCnt]; return -1; }
            public bool IsCmd(int nMotor) { for (int i = 0; i < m_nMotorCnt; i++) if (m_anEn[i] == nMotor) return true; return false; }

            private float CalcRaw2Rpm(int nMotor, int nValue) { return (float)nValue * m_aSParam[nMotor].fRefRpm; }
            private int CalcRpm2Raw(int nMotor, float fRpm) { return (int)Math.Round(Clip(m_aSParam[nMotor].fLimitRpm, 0, fRpm / m_aSParam[nMotor].fRefRpm)); }

            private float CalcTime2Rpm(float fDeltaAngle, float fTime)
            {
                //if (fDeltaAngle == 0) fDeltaAngle = (float)CMath._ZERO;
        #region Kor
                // 1도 이동시간 => fTime / fDeltaAngle

                // 60 초 동안 _MAX_RPM 을 회전하는 것이 RPM, 1도 움직이는 것을 체크하려면 여기에 360 도 회전값을 고려해 주어야 한다.
                // _MAX_RPM 은 1분(60초) 동안 _MAX_RPM 바퀴 (즉, 360 * _MAX_RPM) 를 회전한 값
                // _MAX_RPM * 360 : 60 seconds => 480(_MAX_EV_RPM) 일때 
                // => 1초간 6 * _MAX_RPM 도 회전, 1ms => _MAX_RPM * 360도 / 60000ms = 0.65952 도 이동
                // => 1도 움직이는데 60000 / (_MAX_RPM * 360) = 1.516254246 ms 가 필요

                // 1도 이동시간 => 60000 / (Rpm * 360)
                // 이동각도 당 이동시간 계산 => 1도 이동시간 * fDeltaAngle
        #endregion Kor
                //return 60000 / (fTime / fDeltaAngle * 360.0f);
                return (60.0f * fDeltaAngle * 1000.0f) / (360.0f * fTime);
            }
        #endregion Background
        }
#endif
#if false
        // if you make your class, just write in here
        public class CMonster_
        {
            //private int m_nSerial = 0;
            private List<CSerial> m_lstSerial = null;//new List<CSerial>();
            private Thread m_thReceive = null;
            private List<int> m_lstPort = null;

            private bool m_bProgEnd = false;
            private bool m_bStop = false;
            private bool m_bEms = false;

            private const int _CNT = 256;
            private int m_nMotorCnt = 0;// { get { return m_lstSCommand.Count; } }
            private int[] m_anEn = new int[256]; // push / pop
            private SCommand_t[] m_aSMot = new SCommand_t[_CNT];
            private SCommand_t[] m_aSMot_Prev = new SCommand_t[_CNT];
            private SMotorInfo_t[] m_aSMot_Info = new SMotorInfo_t[_CNT];
            private SMotorStatus_t[] m_aSMot_Status = new SMotorStatus_t[_CNT];
            //private List<SMotorInfo_t> m_lstSMot_Info = new List<SMotorInfo_t>();
            public List<SCommand_t> m_lstSCommand = new List<SCommand_t>();

        #region Structure
            /* dir, bLimitUp, limitUp, bLimitDown, limitDown, CenterEvd, MaxEvd, MaxDegree, 
        SetParam_Dir(nAxis, 0);
        SetParam_LimitUp(nAxis, 0.0f);
        SetParam_LimitDown(nAxis, 0.0f);
        SetParam_CenterEvdValue(nAxis, 2048.0f);
        SetParam_Display(nAxis, 0.0f);
        SetParam_MechMove(nAxis, 4096.0f);
        SetParam_Degree(nAxis, 360.0f);
        SetParam_Rpm(nAxis, 0.229f); // 기본 rpm 단위
        SetParam_LimitRpm_Raw(nAxis, 415);//480);
        SetParam_ProtocolVersion(nAxis, 2); // Version 2(0 해도 동일)
        SetParam_ModelNum(nAxis, 1060); // 0번지에 모델번호 1060, XM430_W210 : 1030, XM430_W350 : 1020
        SetParam_Addr_Max(nAxis, 146);
        SetParam_Addr_Torq(nAxis, 64);
        SetParam_Addr_Led(nAxis, 65);
        SetParam_Addr_Mode(nAxis, 10); // 430 -> 10 address    [0 : 전류, 1 : 속도, 3(default) : 관절(위치제어), 4 : 확장위치제어(멀티턴:-256 ~ 256회전), 5 : 전류기반 위치제어, 16 : pwm 제어(voltage control mode)]
        SetParam_Addr_Speed(nAxis, 104); // 430 -> 104 4 bytes
        SetParam_Addr_Speed_Size(nAxis, 4);
        SetParam_Addr_Pos_Speed(nAxis, 112); // 430 -> 112 4 bytes
        SetParam_Addr_Pos_Speed_Size(nAxis, 4);
        SetParam_Addr_Pos(nAxis, 116); // 430 -> 116 4 bytes
        SetParam_Addr_Pos_Size(nAxis, 4);
         */
            // type 이 바뀌는 경우(멀티턴, 회전, 위치 rpm/time 제어)
            //private struct SQue_t
            //{
            //    byte[] pbyteData;
            //}
            public struct SCommand_t
            {
                public bool bEn;
                public bool bEn_Led;
                public bool bEn_Bytes; // data 통신

                public int nOperationMode;          // 0 - 전류제어모드, 1 - 속도제어모드(바퀴제어), 3(default) - 위치제어모드, 4 - 확장위치제어모드(X시리즈의 경우 -256 ~ +256회전 지원), 5  -전류기반 위치제어모드(X시리즈의 경우 -256 ~ +256회전 지원), 16 - PWM 제어모드
                public int nOperationMode_Prev;

                /////////////////////////////
                public double dAngle;
                public double dAngle_Prev;
                public long lEvd; // Evd

                /////////////////////////////
                public int nLed;

                /////////////////////////////
                public float fRpm;
                public float fMilli;

                /////////////////////////////
                public byte[] pbyBuffer;
                //public SMotorInfo_t SParam;

                /////////////////////////////
                //public int nStatus;
                //public byte [] pbyMap;
            }
            public struct SMotorStatus_t
            {
                bool bTorq;
                bool bError;
                int nStatus;
                int nOperationMode;
            }
            public struct SMotorInfo_t
            {
                public bool bEn;                    // 활성화
                public int nID;                     // 모터를 제어하기 위한 가상 아이디. 중복 허용 불가. 보통은 모터 ID 와 일치시킨다.            
                public int nCommIndex;              // 연결 된 통신포트 중 어느것에 연결 된 것이지에 대한 순서.

                public int nRealID;
                public int nDir;
                //Center
                public float fCenterPos;

                public float fMechMove;
                public float fDegree;
                public float fRefRpm;

                public float fLimitRpm;     // ?

                public float fLimitUp;    // Limit - 0: Ignore
                public float fLimitDn;    // Limit - 0: Ignore
                
                //public int nOperationMode;          // 0 - 전류제어모드, 1 - 속도제어모드(바퀴제어), 3(default) - 위치제어모드, 4 - 확장위치제어모드(X시리즈의 경우 -256 ~ +256회전 지원), 5  -전류기반 위치제어모드(X시리즈의 경우 -256 ~ +256회전 지원), 16 - PWM 제어모드

                public int nProtocol;               // 2, 0 - protocol2, 1 - protocol1, 3 - None         
#if false
                // 430 -> 146 ( 0번지에 모델번호 
                // 320 -> 52 (0번지에 모델번호 350)
                public int nAddr_Max; // indirect address 를 제외한 읽을 주소의 끝 번지

                // 430 -> 64 address
                // 320 -> 24
                public int nAddr_Torq; // torq 1 byte, led 1 byte
                // 430 -> 104 4 bytes
                // 320 -> 32 2 bytes
                public int nAddr_Led; // 
                // 430 -> 10 address    [0 : 전류, 1 : 속도, 3(default) : 관절(위치제어), 4 : 확장위치제어(멀티턴:-256 ~ 256회전), 5 : 전류기반 위치제어, 16 : pwm 제어(voltage control mode)]
                // 320 -> 11            [1 : 속도, 2(default) : 관절]
                public int nAddr_Mode;
                // 430 -> 104 4 bytes
                // 320 -> 32 2 bytes
                public int nAddr_Speed;
                public int nAddr_Speed_Size;
                // 430 -> 108 4 bytes
                // 320 -> 32 2 bytes
                public int nAddr_Pos_Speed;
                public int nAddr_Pos_Speed_Size;
                // 430 -> 112 4 bytes
                // 320 -> 30 2 bytes
                public int nAddr_Pos;
                public int nAddr_Pos_Size;

#endif

                //public float fPos;
                //public float fRpm_Raw;

                //public int nFlag; // 76[543210] NoAction(5), Red(4), Blue(3), Green(2), Mode(    
                //public int nLed;
                //public bool bTorq;
                //public int nControlMode; // 0 - None, 1 - Speed, 3 - Pos, 4 - Multi Turn
                //public int nDriveMode;   // 0 - Rpm Based, 1 - Time Based
                public SAddress_t SAddress;
            }
            public struct SAddress_t
            {
        #region 확장기능 // Table Address - 여기부터는 값을 가지는게 아닌 주소번지만 가지도록...
                public int nMotorNumber_0_2;        // R    0 : none
                public int nMotorNumber_Size_2;
                public int nFwVersion_6_1;          // R      
                public int nFwVersion_Size_1;
                public int nRealID_7_1;             // RW   0 ~ 252
                public int nRealID_Size_1;
                public int nBaudrate_8_1;           // RW   0-9600, 1-57600, 2-115200, 3-1M, 4-2M,  5-3M, 6-4M, 7-4.5M
                public int nBaudrate_Size_1;
                // [DriveMode] 
                //    0x01 : 정상회전(0), 역회전(1)
                //    0x02 : 540 전용 Master(0), Slave(1)
                //    0x04 : 속도기준 제어(0), 시간기준 제어(1)
                public int nMode_Drive_10_1;        // RW 
                public int nMode_Drive_Size_1;
                public int nMode_Operating_11_1;    // RW   1=속도제어(무한회전), 3(초기값)=위치제어(RPM), 4=Multi-Turn(-256~256:512회전), 5=전류기반 위치제어모드(-256~256:512회전), 16=PWM제어
                public int nMode_Operating_Size_1;
                public int nProtocolVersion_13_1;   // RW   
                public int nProtocolVersion_Size_1;
                public int nOffset_20_4;            // RW   1회전 모드인 경우 -1024~1024 까지만 제어
                public int nOffset_Size_4;
                public int nLimit_PWM_36_2;         // RW   0~885 (885 = 100%)
                public int nLimit_PWM_Size_2;
                public int nLimit_Curr_38_2;        // RW   전류제어모드/전류기반위치제어모드에서 사용, 0~2047
                public int nLimit_Curr_Size_2;
                // [Shutdown] - Reboot 으로만 해제 가능
                //    0x20 : 과부하
                //    0x10 : 전력이상
                //    0x08 : 엔코더 이상(Following Error)
                //    0x04 : 과열
                //    0x01 : 인가된 전압 이상
                public int nShutdown_63_1;          // RW
                public int nShutdown_Size_1;
                public int nTorq_64_1;              // RW   Off(0), On(1)
                public int nTorq_Size_1;
                public int nLed_65_1;               // RW   Off(0), On(1)
                public int nLed_Size_1;
                public int nError_70_1;             // R    
                public int nError_Size_1;
                public int nGain_Vel_I_76_2;        // RW
                public int nGain_Vel_I_Size_2;        
                public int nGain_Vel_P_78_2;        // RW
                public int nGain_Vel_P_Size_2;
                public int nGain_Pos_D_80_2;        // RW
                public int nGain_Pos_D_Size_2;        
                public int nGain_Pos_I_82_2;        // RW
                public int nGain_Pos_I_Size_2;
                public int nGain_Pos_P_84_2;        // RW
                public int nGain_Pos_P_Size_2;        
                public int nGain_Pos_F2_88_2;       // RW
                public int nGain_Pos_F2_Size_2;
                public int nGain_Pos_F1_90_2;       // RW
                public int nGain_Pos_F1_Size_2;       

                public int nWatchDog_98_1;          // RW
                public int nWatchDog_Size_1;

                public int nGoal_PWM_100_2;         // RW   -PWMLimit ~ +PWMLimit
                public int nGoal_PWM_Size_2;  
                public int nGoal_Current_102_2;     // RW   -CurrentLimit ~ +CurrentLimit
                public int nGoal_Current_Size_2;
                public int nGoal_Vel_104_4;         // RW
                public int nGoal_Vel_Size_4;

                public int nProfile_Acc_108_4;      // RW
                public int nProfile_Acc_Size_4;
                public int nProfile_Vel_112_4;      // RW
                public int nProfile_Vel_Size_4;

                public int nGoal_Pos_116_4;         // RW
                public int nGoal_Pos_Size_4;

                public int nMoving_122_1;           // R    움직임 감지 못함(0), 움직임 감지(1)
                public int nMoving_Size_1;
                // [Moving Status]
                //    0x30  - 0x30 : 사다리꼴 속도 프로파일
                //            0x20 : 삼각 속도 프로파일
                //            0x10 : 사각 속도 프로파일
                //            0x00 : 프로파일 미사용(Step)
                //    0x08 : Following Error
                //    0x02 : Goal Position 명령에 따라 진행 중
                //    0x01 : Inposition
                public int nMoving_Status_123_1;    // R
                public int nMoving_Status_Size_1;

                public int nPresent_PWM_124_2;      // R
                public int nPresent_PWM_Size_2;      
                public int nPresent_Curr_126_2;     // R
                public int nPresent_Curr_Size_2;     
                public int nPresent_Vel_128_4;      // R
                public int nPresent_Vel_Size_4;      
                public int nPresent_Pos_132_4;      // R
                public int nPresent_Pos_Size_4;      
                public int nPresent_Volt_144_2;     // R
                public int nPresent_Volt_Size_2;     
                public int nPresent_Temp_146_1;     // R
                public int nPresent_Temp_Size_1;     
        #endregion 확장기능
            }
        #endregion Structure
            private void SetAddress(EMotor_t EMot, ref SAddress_t SAddress)
            {
                
                switch (EMot)
                {
                    /*
                        SetParam_Addr_Max(nAxis, 52);
                        SetParam_Addr_Torq(nAxis, 24);
                        SetParam_Addr_Led(nAxis, 25);
                        SetParam_Addr_Mode(nAxis, 11); // 320 -> 11            [1 : 속도, 2(default) : 관절]
                        SetParam_Addr_Speed(nAxis, 32); // 320 -> 32 2 bytes
                        SetParam_Addr_Speed_Size(nAxis, 2);
                        SetParam_Addr_Pos_Speed(nAxis, 32); // 320 -> 32 2 bytes
                        SetParam_Addr_Pos_Speed_Size(nAxis, 2);
                        SetParam_Addr_Pos(nAxis, 30); // 320 -> 30 2 bytes
                        SetParam_Addr_Pos_Size(nAxis, 2);
                     */
                    case EMotor_t.AX_12:
                    case EMotor_t.AX_18:
                        SAddress.nMotorNumber_0_2 = 0;        // R    0 : none
                        SAddress.nMotorNumber_Size_2 = 2;     
                        SAddress.nFwVersion_6_1 = 6;          // R      
                        SAddress.nFwVersion_Size_1 = 1;

                        SAddress.nRealID_7_1 = 7;             // RW   0 ~ 252
                        SAddress.nRealID_Size_1 = 1;
                        SAddress.nBaudrate_8_1 = 8;           // RW   0-9600, 1-57600, 2-115200, 3-1M, 4-2M,  5-3M, 6-4M, 7-4.5M
                        SAddress.nBaudrate_Size_1 = 1;




                        SAddress.nTorq_64_1 = 24;              // RW   Off(0), On(1)
                        SAddress.nTorq_Size_1 = 1;
                        SAddress.nLed_65_1 = 25;               // RW   Off(0), On(1)
                        SAddress.nLed_Size_1 = 1;

                        SAddress.nMode_Operating_11_1 = 11;    // RW   1=속도제어(무한회전), 3(초기값)=위치제어(RPM), 4=Multi-Turn(-256~256:512회전), 5=전류기반 위치제어모드(-256~256:512회전), 16=PWM제어
                        SAddress.nMode_Operating_Size_1 = 1;


                        SAddress.nGoal_Vel_104_4 = 32;         // RW
                        SAddress.nGoal_Vel_Size_4 = 2;

                        SAddress.nProfile_Vel_112_4 = 32;      // RW
                        SAddress.nProfile_Vel_Size_4 = 2;

                        SAddress.nGoal_Pos_116_4 = 30;         // RW
                        SAddress.nGoal_Pos_Size_4 = 2;



                        // [DriveMode] 
                        //    0x01 : 정상회전(0), 역회전(1)
                        //    0x02 : 540 전용 Master(0), Slave(1)
                        //    0x04 : 속도기준 제어(0), 시간기준 제어(1)
                        //SAddress.nMode_Drive_10_1 = 10;        // RW 
                        //SAddress.nMode_Drive_Size_1 = 1;
                        //SAddress.nMode_Operating_11_1 = 11;    // RW   1=속도제어(무한회전), 3(초기값)=위치제어(RPM), 4=Multi-Turn(-256~256:512회전), 5=전류기반 위치제어모드(-256~256:512회전), 16=PWM제어
                        //SAddress.nMode_Operating_Size_1 = 1;
                        //SAddress.nProtocolVersion_13_1 = 13;   // RW   
                        //SAddress.nProtocolVersion_Size_1 = 1;
                        //SAddress.nOffset_20_4 = 20;            // RW   1회전 모드인 경우 -1024~1024 까지만 제어
                        //SAddress.nOffset_Size_4 = 4;
                        //SAddress.nLimit_PWM_36_2 = 36;         // RW   0~885 (885 = 100%)
                        //SAddress.nLimit_PWM_Size_2 = 2;
                        //SAddress.nLimit_Curr_38_2 = 38;        // RW   전류제어모드/전류기반위치제어모드에서 사용, 0~2047
                        //SAddress.nLimit_Curr_Size_2 = 2;
                        //// [Shutdown] - Reboot 으로만 해제 가능
                        ////    0x20 : 과부하
                        ////    0x10 : 전력이상
                        ////    0x08 : 엔코더 이상(Following Error)
                        ////    0x04 : 과열
                        ////    0x01 : 인가된 전압 이상
                        //SAddress.nShutdown_63_1 = 63;          // RW
                        //SAddress.nShutdown_Size_1 = 1;
                        
                        //SAddress.nError_70_1 = 70;             // R    
                        //SAddress.nError_Size_1 = 1;
                        //SAddress.nGain_Vel_I_76_2 = 76;        // RW
                        //SAddress.nGain_Vel_I_Size_2 = 2;
                        //SAddress.nGain_Vel_P_78_2 = 78;        // RW
                        //SAddress.nGain_Vel_P_Size_2 = 2;
                        //SAddress.nGain_Pos_D_80_2 = 80;        // RW
                        //SAddress.nGain_Pos_D_Size_2 = 2;
                        //SAddress.nGain_Pos_I_82_2 = 82;        // RW
                        //SAddress.nGain_Pos_I_Size_2 = 2;
                        //SAddress.nGain_Pos_P_84_2 = 84;        // RW
                        //SAddress.nGain_Pos_P_Size_2 = 2;
                        //SAddress.nGain_Pos_F2_88_2 = 88;       // RW
                        //SAddress.nGain_Pos_F2_Size_2 = 2;
                        //SAddress.nGain_Pos_F1_90_2 = 90;       // RW
                        //SAddress.nGain_Pos_F1_Size_2 = 2;

                        //SAddress.nWatchDog_98_1 = 98;          // RW
                        //SAddress.nWatchDog_Size_1 = 1;

                        //SAddress.nGoal_PWM_100_2 = 100;         // RW   -PWMLimit ~ +PWMLimit
                        //SAddress.nGoal_PWM_Size_2 = 2;
                        //SAddress.nGoal_Current_102_2 = 102;     // RW   -CurrentLimit ~ +CurrentLimit
                        //SAddress.nGoal_Current_Size_2 = 2;

                        //SAddress.nProfile_Acc_108_4 = 108;      // RW
                        //SAddress.nProfile_Acc_Size_4 = 4;

                        //SAddress.nMoving_122_1 = 122;           // R    움직임 감지 못함(0), 움직임 감지(1)
                        //SAddress.nMoving_Size_1 = 1;
                        //// [Moving Status]
                        ////    0x30  - 0x30 : 사다리꼴 속도 프로파일
                        ////            0x20 : 삼각 속도 프로파일
                        ////            0x10 : 사각 속도 프로파일
                        ////            0x00 : 프로파일 미사용(Step)
                        ////    0x08 : Following Error
                        ////    0x02 : Goal Position 명령에 따라 진행 중
                        ////    0x01 : Inposition
                        //SAddress.nMoving_Status_123_1 = 123;    // R
                        //SAddress.nMoving_Status_Size_1 = 1;

                        //SAddress.nPresent_PWM_124_2 = 124;      // R
                        //SAddress.nPresent_PWM_Size_2 = 2;
                        //SAddress.nPresent_Curr_126_2 = 126;     // R
                        //SAddress.nPresent_Curr_Size_2 = 2;
                        //SAddress.nPresent_Vel_128_4 = 128;      // R
                        //SAddress.nPresent_Vel_Size_4 = 4;
                        //SAddress.nPresent_Pos_132_4 = 132;      // R
                        //SAddress.nPresent_Pos_Size_4 = 4;
                        //SAddress.nPresent_Volt_144_2 = 144;     // R
                        //SAddress.nPresent_Volt_Size_2 = 2;
                        //SAddress.nPresent_Temp_146_1 = 146;     // R
                        //SAddress.nPresent_Temp_Size_1 = 1;


                        /*

                        SetParam_ModelNum(nAxis, 12); // 0번지에 모델번호 12
                        SetParam_Addr_Max(nAxis, 52);
                        SetParam_Addr_Torq(nAxis, 24);
                        SetParam_Addr_Led(nAxis, 25);
                        SetParam_Addr_Mode(nAxis, 11); // 320 -> 11            [1 : 속도, 2(default) : 관절]
                        SetParam_Addr_Speed(nAxis, 32); // 320 -> 32 2 bytes
                        SetParam_Addr_Speed_Size(nAxis, 2);
                        SetParam_Addr_Pos_Speed(nAxis, 32); // 320 -> 32 2 bytes
                        SetParam_Addr_Pos_Speed_Size(nAxis, 2);
                        SetParam_Addr_Pos(nAxis, 30); // 320 -> 30 2 bytes
                        SetParam_Addr_Pos_Size(nAxis, 2);*/
                        break;
                        
                    case EMotor_t.XL_430:
                        SAddress.nMotorNumber_0_2 = 0;        // R    0 : none
                        SAddress.nMotorNumber_Size_2 = 2;     
                        SAddress.nFwVersion_6_1 = 6;          // R      
                        SAddress.nFwVersion_Size_1 = 1;

                        SAddress.nRealID_7_1 = 7;             // RW   0 ~ 252
                        SAddress.nRealID_Size_1 = 1;
                        SAddress.nBaudrate_8_1 = 8;           // RW   0-9600, 1-57600, 2-115200, 3-1M, 4-2M,  5-3M, 6-4M, 7-4.5M
                        SAddress.nBaudrate_Size_1 = 1;
                        // [DriveMode] 
                        //    0x01 : 정상회전(0), 역회전(1)
                        //    0x02 : 540 전용 Master(0), Slave(1)
                        //    0x04 : 속도기준 제어(0), 시간기준 제어(1)
                        SAddress.nMode_Drive_10_1 = 10;        // RW 
                        SAddress.nMode_Drive_Size_1 = 1;
                        SAddress.nMode_Operating_11_1 = 11;    // RW   1=속도제어(무한회전), 3(초기값)=위치제어(RPM), 4=Multi-Turn(-256~256:512회전), 5=전류기반 위치제어모드(-256~256:512회전), 16=PWM제어
                        SAddress.nMode_Operating_Size_1 = 1;
                        SAddress.nProtocolVersion_13_1 = 13;   // RW   
                        SAddress.nProtocolVersion_Size_1 = 1;
                        SAddress.nOffset_20_4 = 20;            // RW   1회전 모드인 경우 -1024~1024 까지만 제어
                        SAddress.nOffset_Size_4 = 4;
                        SAddress.nLimit_PWM_36_2 = 36;         // RW   0~885 (885 = 100%)
                        SAddress.nLimit_PWM_Size_2 = 2;
                        SAddress.nLimit_Curr_38_2 = 38;        // RW   전류제어모드/전류기반위치제어모드에서 사용, 0~2047
                        SAddress.nLimit_Curr_Size_2 = 2;
                        // [Shutdown] - Reboot 으로만 해제 가능
                        //    0x20 : 과부하
                        //    0x10 : 전력이상
                        //    0x08 : 엔코더 이상(Following Error)
                        //    0x04 : 과열
                        //    0x01 : 인가된 전압 이상
                        SAddress.nShutdown_63_1 = 63;          // RW
                        SAddress.nShutdown_Size_1 = 1;
                        SAddress.nTorq_64_1 = 64;              // RW   Off(0), On(1)
                        SAddress.nTorq_Size_1 = 1;
                        SAddress.nLed_65_1 = 65;               // RW   Off(0), On(1)
                        SAddress.nLed_Size_1 = 1;
                        SAddress.nError_70_1 = 70;             // R    
                        SAddress.nError_Size_1 = 1;
                        SAddress.nGain_Vel_I_76_2 = 76;        // RW
                        SAddress.nGain_Vel_I_Size_2 = 2;
                        SAddress.nGain_Vel_P_78_2 = 78;        // RW
                        SAddress.nGain_Vel_P_Size_2 = 2;
                        SAddress.nGain_Pos_D_80_2 = 80;        // RW
                        SAddress.nGain_Pos_D_Size_2 = 2;
                        SAddress.nGain_Pos_I_82_2 = 82;        // RW
                        SAddress.nGain_Pos_I_Size_2 = 2;
                        SAddress.nGain_Pos_P_84_2 = 84;        // RW
                        SAddress.nGain_Pos_P_Size_2 = 2;
                        SAddress.nGain_Pos_F2_88_2 = 88;       // RW
                        SAddress.nGain_Pos_F2_Size_2 = 2;
                        SAddress.nGain_Pos_F1_90_2 = 90;       // RW
                        SAddress.nGain_Pos_F1_Size_2 = 2;

                        SAddress.nWatchDog_98_1 = 98;          // RW
                        SAddress.nWatchDog_Size_1 = 1;

                        SAddress.nGoal_PWM_100_2 = 100;         // RW   -PWMLimit ~ +PWMLimit
                        SAddress.nGoal_PWM_Size_2 = 2;
                        SAddress.nGoal_Current_102_2 = 102;     // RW   -CurrentLimit ~ +CurrentLimit
                        SAddress.nGoal_Current_Size_2 = 2;
                        SAddress.nGoal_Vel_104_4 = 104;         // RW
                        SAddress.nGoal_Vel_Size_4 = 4;

                        SAddress.nProfile_Acc_108_4 = 108;      // RW
                        SAddress.nProfile_Acc_Size_4 = 4;
                        SAddress.nProfile_Vel_112_4 = 112;      // RW
                        SAddress.nProfile_Vel_Size_4 = 4;

                        SAddress.nGoal_Pos_116_4 = 116;         // RW
                        SAddress.nGoal_Pos_Size_4 = 4;

                        SAddress.nMoving_122_1 = 122;           // R    움직임 감지 못함(0), 움직임 감지(1)
                        SAddress.nMoving_Size_1 = 1;
                        // [Moving Status]
                        //    0x30  - 0x30 : 사다리꼴 속도 프로파일
                        //            0x20 : 삼각 속도 프로파일
                        //            0x10 : 사각 속도 프로파일
                        //            0x00 : 프로파일 미사용(Step)
                        //    0x08 : Following Error
                        //    0x02 : Goal Position 명령에 따라 진행 중
                        //    0x01 : Inposition
                        SAddress.nMoving_Status_123_1 = 123;    // R
                        SAddress.nMoving_Status_Size_1 = 1;

                        SAddress.nPresent_PWM_124_2 = 124;      // R
                        SAddress.nPresent_PWM_Size_2 = 2;
                        SAddress.nPresent_Curr_126_2 = 126;     // R
                        SAddress.nPresent_Curr_Size_2 = 2;
                        SAddress.nPresent_Vel_128_4 = 128;      // R
                        SAddress.nPresent_Vel_Size_4 = 4;
                        SAddress.nPresent_Pos_132_4 = 132;      // R
                        SAddress.nPresent_Pos_Size_4 = 4;
                        SAddress.nPresent_Volt_144_2 = 144;     // R
                        SAddress.nPresent_Volt_Size_2 = 2;
                        SAddress.nPresent_Temp_146_1 = 146;     // R
                        SAddress.nPresent_Temp_Size_1 = 1;
                        break;
                    case EMotor_t.XL_320:
                        break;
                }
            }
            private void Init(int nMotorCount)
            {
                Array.Resize<SMotorStatus_t>(ref m_aSMot_Status, nMotorCount);
                Array.Clear(m_aSMot_Status, 0, m_aSMot_Status.Length);

                Array.Resize<SMotorInfo_t>(ref m_aSMot_Info, nMotorCount);
                Array.Clear(m_aSMot_Info, 0, m_aSMot_Info.Length);
                //m_lstSMot_Info.Clear();

                Array.Resize<int>(ref m_anEn, nMotorCount);
                Array.Clear(m_anEn, 0, m_anEn.Length);

                Array.Resize<SCommand_t>(ref m_aSMot, nMotorCount);
                Array.Clear(m_aSMot, 0, m_aSMot.Length);
                
                m_lstSCommand.Clear();
            }
            public CMonster(int nMotorCount)
            {
                Init(nMotorCount);
            }
            public CMonster()
            {
                Init(_CNT);
                //Array.Clear(m_anEn, 0, m_anEn.Length);
                //Array.Clear(m_aSMot, 0, m_aSMot.Length);
                //m_lstSCommand.Clear();
            }
            ~CMonster()
            {
                Close();
            }
#if true
            public enum EMotor_t
            {
                NONE = 0,
                // Default
                XL_430 = 20, // 360도 4096 : 2048 center : 0.229 RefRpm, 480 Limit Rpm
                XL_320 = 21, // 300도 1024 : 512 center : 0.111 RefRpm, 1023 Limit Rpm

                XM_540 = 30, // LED [65], 360도 4096 : 2048 center : 0.229 RefRpm, 480 Limit Rpm  , 확장위치제어모드시 512 회전 가능(+-256)

                AX_12 = 1, //
                AX_18 = 2, //
            

                DX_113 = 3, //
                DX_116 = 4, //
                DX_117 = 5, //
                RX_10 = 6, //
                RX_24F = 7, //
                RX_28 = 8, //
                RX_64 = 9, //
                EX_106 = 10, //
                MX_12 = 11, //

                // protocol 2.0
                MX_28 = 12, //
                MX_64 = 13, //
                MX_106 = 14, //
                MX_ = 15, //

                SG_90 = 100
            }
#else
            // Model
            NONE = 0; 
            // Default
            public const int _MOTOR_XL_430 = 20; // 360도 4096 : 2048 center : 0.229 RefRpm, 480 Limit Rpm
            public const int _MOTOR_XL_320 = 21; // 300도 1024 : 512 center : 0.111 RefRpm, 1023 Limit Rpm
            /*
            public const int _MOTOR_XM_540 = 30; // LED [65], 360도 4096 : 2048 center : 0.229 RefRpm, 480 Limit Rpm  , 확장위치제어모드시 512 회전 가능(+-256)
            */
            public const int _MOTOR_AX_12 = 1; //
            public const int _MOTOR_AX_18 = 2; //
            /*
            public const int _MOTOR_DX_113 = 3; //
            public const int _MOTOR_DX_116 = 4; //
            public const int _MOTOR_DX_117 = 5; //
            public const int _MOTOR_RX_10 = 6; //
            public const int _MOTOR_RX_24F = 7; //
            public const int _MOTOR_RX_28 = 8; //
            public const int _MOTOR_RX_64 = 9; //
            public const int _MOTOR_EX_106 = 10; //
            public const int _MOTOR_MX_12 = 11; //

            // protocol 2.0
            public const int _MOTOR_MX_28 = 12; //
            public const int _MOTOR_MX_64 = 13; //
            public const int _MOTOR_MX_106 = 14; //
            public const int _MOTOR_MX_ = 15; //
            */
            /*
             Open(3,57600);
             Open(5,1000000);
             
             * 
             * 
             * 
             * 
             * 
             Close()
             
             
              
             * */
#endif
            private struct SCheckMotorType_t
            {
                public int nCommIndex;
                public int nTorqAddress;
                public int nProtocol;
                public EMotor_t EMot;
            }
            //private List<int> m_anMotorComm = new List<int>();
            //private List<EMotor_t> m_anMotorType = new List<EMotor_t>();
            private List<SCheckMotorType_t> m_lstSCheckMotorType = new List<SCheckMotorType_t>();
            private List<int> m_anID = new List<int>();
            public void ClearMotor() { m_anID.Clear(); m_lstSCheckMotorType.Clear(); }//m_anMotorType.Clear(); m_anMotorComm.Clear(); }
            public Ojw.CMonster.SMotorInfo_t SetMotor(int nMotor, EMotor_t EMot) { return SetMotor(nMotor, nMotor, 0, 0, EMot); }
            public Ojw.CMonster.SMotorInfo_t SetMotor(int nMotor, int nCommIndex, EMotor_t EMot) { return SetMotor(nMotor, nMotor, nCommIndex, 0, EMot); }
            public Ojw.CMonster.SMotorInfo_t SetMotor(int nMotor, int nMotorRealID, int nCommIndex, int nDir, EMotor_t EMot)
            {                
                //SMotorInfo_t SInfo = new SMotorInfo_t();
                SetAddress(EMot, ref m_aSMot_Info[nMotor].SAddress);
                switch (EMot)
                {
                    // 
                    case EMotor_t.SG_90:
                        break;
                    // Protocol1
                    case EMotor_t.AX_12:
                    case EMotor_t.AX_18:
                        m_aSMot_Info[nMotor].bEn = true;                       // 활성화
                        
                        m_aSMot_Info[nMotor].nID = nMotor;                     // 모터를 제어하기 위한 가상 아이디. 중복 허용 불가. 보통은 모터 ID 와 일치시킨다.      
                        m_aSMot_Info[nMotor].nRealID = nMotorRealID;           // 실제 사용할 모터의 아이디
                        m_aSMot_Info[nMotor].nCommIndex = nCommIndex;          // 연결 된 통신포트 중 어느것에 연결 된 것이지에 대한 순서.

                        m_aSMot_Info[nMotor].nDir = nDir;
                        m_aSMot_Info[nMotor].fCenterPos = 512.0f;

                        m_aSMot_Info[nMotor].fMechMove = 1024.0f;
                        m_aSMot_Info[nMotor].fDegree = 300.0f;
                        m_aSMot_Info[nMotor].fRefRpm = 0.111f;                 // 기본 rpm 단위

                        m_aSMot_Info[nMotor].fLimitRpm = 1023f;                // 

                        m_aSMot_Info[nMotor].fLimitUp = 0.0f;    // Limit - 0: Ignore
                        m_aSMot_Info[nMotor].fLimitDn = 0.0f;    // Limit - 0: Ignore
                
                        m_aSMot_Info[nMotor].nProtocol = 1;               // 2, 0 - protocol2, 1 - protocol1, 3 - None     
                                                
                        break;


                    // Protocol2
                    case EMotor_t.NONE:
                    case EMotor_t.XL_430:
                        m_aSMot_Info[nMotor].bEn = true;                       // 활성화
                        //m_aSMot_Info[nMotor].nModelNum = 1060;
                        m_aSMot_Info[nMotor].nID = nMotor;                     // 모터를 제어하기 위한 가상 아이디. 중복 허용 불가. 보통은 모터 ID 와 일치시킨다.      
                        m_aSMot_Info[nMotor].nRealID = nMotorRealID;           // 실제 사용할 모터의 아이디
                        m_aSMot_Info[nMotor].nCommIndex = nCommIndex;          // 연결 된 통신포트 중 어느것에 연결 된 것이지에 대한 순서.

                        m_aSMot_Info[nMotor].nDir = nDir;
                        m_aSMot_Info[nMotor].fCenterPos = 2048.0f;

                        m_aSMot_Info[nMotor].fMechMove = 4096.0f;
                        m_aSMot_Info[nMotor].fDegree = 360.0f;
                        m_aSMot_Info[nMotor].fRefRpm = 0.229f;                 // 기본 rpm 단위

                        m_aSMot_Info[nMotor].fLimitRpm = 415f;                // 

                        m_aSMot_Info[nMotor].fLimitUp = 0.0f;    // Limit - 0: Ignore
                        m_aSMot_Info[nMotor].fLimitDn = 0.0f;    // Limit - 0: Ignore
                
                        m_aSMot_Info[nMotor].nProtocol = 2;               // 2, 0 - protocol2, 1 - protocol1, 3 - None                             
                        break;


                    case EMotor_t.XL_320:
                        break;
                    case EMotor_t.XM_540:
                        break;
                }
        #region Check
                //if (m_lstSCheckMotorType.Count > 0)
                {
                    bool bOk = false;
                    foreach (SCheckMotorType_t SType in m_lstSCheckMotorType)
                    {
                        if ((SType.EMot == EMot) && (SType.nCommIndex == nCommIndex) && (SType.nProtocol == m_aSMot_Info[nMotor].nProtocol) && (SType.nTorqAddress == m_aSMot_Info[nMotor].SAddress.nTorq_64_1))
                        {
                            bOk = true;
                            break;
                        }
                    }

                    if (bOk == false)
                    {
                        SCheckMotorType_t SType = new SCheckMotorType_t();
                        SType.nCommIndex = nCommIndex;
                        SType.EMot = EMot;
                        SType.nProtocol = m_aSMot_Info[nMotor].nProtocol;
                        SType.nTorqAddress = m_aSMot_Info[nMotor].SAddress.nTorq_64_1;
                        m_lstSCheckMotorType.Add(SType);
                    }
                }
        #endregion Check
                //m_lstSMot_Info.Add(SInfo);
                return m_aSMot_Info[nMotor];
            }
            public int GetMotor_Count() 
            {
                if (m_lstSCheckMotorType != null)
                {
                    return m_lstSCheckMotorType.Count;
                }
                return 0;
            }
            public Ojw.CMonster.SMotorInfo_t GetMotorInfo(int nMotor)
            {
                //SMotorInfo_t SInfo = new SMotorInfo_t();

                return m_aSMot_Info[nMotor];

                //return SInfo;
            }
            public int GetMotor_RealID(int nMotor) { return m_aSMot_Info[nMotor].nRealID; }
            public int GetMotor_ID(int nRealID) { for (int i = 0; i < m_aSMot_Info.Length; i++) if (m_aSMot_Info[i].nRealID == nRealID) return i; return -1; }
            public int GetMotor_ID(int nCommIndex, int nRealID) { for (int i = 0; i < m_aSMot_Info.Length; i++) if ((m_aSMot_Info[i].nRealID == nRealID) && (m_aSMot_Info[i].nCommIndex == nCommIndex)) return i; return -1; }
            //public void SetParam_RealID(int nMotor, int nRealID) { m_lstSMot_Info[nMotor].nRealID = nRealID; }
            //public void SetParam_Dir(int nMotor, int nDir) { m_aSParam_Axis[nMotor].nDir = m_aSMot[nMotor].nDir = nDir; }
            //public void SetParam_LimitUp(int nMotor, float fLimitUp) { m_aSParam_Axis[nMotor].fLimitUp = m_aSMot[nMotor].fLimitUp = fLimitUp; }
            //public void SetParam_LimitDown(int nMotor, float fLimitDn) { m_aSParam_Axis[nMotor].fLimitDn = m_aSMot[nMotor].fLimitDn = fLimitDn; }
            //public void SetParam_CenterEvdValue(int nMotor, float fCenterPos) { m_aSParam_Axis[nMotor].fCenterPos = m_aSMot[nMotor].fCenterPos = fCenterPos; }
            //public void SetParam_Display(int nMotor, float fOffsetAngle_Display) { m_aSParam_Axis[nMotor].fOffsetAngle_Display = fOffsetAngle_Display; }
            //public void SetParam_MechMove(int nMotor, float fMechMove) { m_aSParam_Axis[nMotor].fMechMove = m_aSMot[nMotor].fMechMove = fMechMove; }
            //public void SetParam_Degree(int nMotor, float fDegree) { m_aSParam_Axis[nMotor].fDegree = m_aSMot[nMotor].fDegree = fDegree; }
            //public void SetParam_Rpm(int nMotor, float fRpm) { m_aSParam_Axis[nMotor].fRefRpm = m_aSMot[nMotor].fRefRpm = fRpm; }
            //public void SetParam_LimitRpm_Raw(int nMotor, float fLimitRpm) { m_aSParam_Axis[nMotor].fLimitRpm = m_aSMot[nMotor].fLimitRpm = fLimitRpm; }

            // 0 : none(== version 2), 1 : version 1, 2 : version 2
            //public void SetParam_ProtocolVersion(int nAxis, int nProtocol_Version)
            //{
            //    m_nProtocolVersion = nProtocol_Version; // 일단은 어떤 모터든 변경하면 그게 적용되도록 한다. 나중에는 모터 개별적으로 대응되도록 할 것
            //    m_aSParam_Axis[nAxis].nProtocol_Version = m_aSMot[nAxis].nProtocol_Version = nProtocol_Version;
            //}
        #region Open / Close
            // 해당 컴포트가 열렸는지를 확인
            public bool IsOpen(int nPort)
            {
                if (m_lstPort != null)
                {
                    foreach (int nSerialPort in m_lstPort) if (nSerialPort == nPort) return true;
                }
                return false;
            }
            // 해당 포트의 시리얼 핸들을 리턴
            public CSerial GetSerial(int nPort)
            {
                if (m_lstPort != null)
                {
                    for (int i = 0; i < m_lstPort.Count; i++)
                    {
                        if (m_lstPort[i] == nPort) return m_lstSerial[i];
                    }
                }
                return null;
            }
            // 해당 포트의 인덱스 번호를 리턴
            public int GetSerialIndex(int nPort)
            {
                if (m_lstPort != null)
                {
                    for (int i = 0; i < m_lstPort.Count; i++)
                    {
                        if (m_lstPort[i] == nPort) return i;
                    }
                }
                return -1;
            }
            // 해당 인덱스의 포트 번호를 리턴
            public int GetSerialPort(int nCommIndex)
            {
                if (m_lstPort != null)
                {
                    if (nCommIndex < m_lstPort.Count)
                    {
                        return m_lstPort[nCommIndex];
                    }
                }
                return -1;
            }
            // 전체 포트의 인덱스 수를 리턴
            public int GetSerialCount()
            {
                if (m_lstPort != null)
                {
                    return m_lstPort.Count;
                }
                return -1;
            }
            // 컴포트를 연다.(중복 되지만 않으면 여러개를 여는 것이 가능)
            public bool Open(int nPort, int nBaudRate)
            {
                Ojw.CSerial COjwSerial = new CSerial();
                if (COjwSerial.Connect(nPort, nBaudRate) == true)
                {
                    if (m_lstSerial == null)
                    {
                        m_lstSerial = new List<CSerial>();
                        m_lstPort = new List<int>();

                        m_thReceive = new Thread(new ThreadStart(Thread_Receive));
                        m_thReceive.Start();
                        Ojw.CMessage.Write("Init Thread");
                    }
                     m_lstSerial.Add(COjwSerial);
                     m_lstPort.Add(nPort);
                                         
                    return true;
                }
                return false;
            }
            // 지정한 포트를 닫는다.
            public void Close(int nPort)
            {
                if (m_lstSerial != null)
                {
                    if (m_lstSerial.Count > 0)
                    {
                        int nIndex = GetSerialIndex(nPort);
                        if (nIndex >= 0) 
                        {
                            m_lstSerial[nIndex].DisConnect();
                            m_lstSerial.RemoveAt(nIndex);
                            m_lstPort.RemoveAt(nIndex);
                        }
                    }
                    if (m_lstPort.Count == 0)
                    {
                        m_lstSerial.Clear(); m_lstPort.Clear();
                        m_lstSerial = null; m_lstPort = null;
                    }
                }
            }
            // 전체 포트를 닫는다.
            public void Close()
            {
                if (m_lstSerial != null)
                {
                    foreach (CSerial COjwSerial in m_lstSerial)
                    {
                        if (COjwSerial.IsConnect() == true) COjwSerial.DisConnect();
                    }
                    m_lstSerial.Clear(); m_lstPort.Clear();
                    m_lstSerial = null; m_lstPort = null;
                }
            }
        #endregion Open / Close

            private void Thread_Receive()
            {
                byte byHead0 = 0;
                byte byHead1 = 0;
                byte byHead2 = 0;

                byte[] buf;
                Ojw.CMessage.Write("[Thread_Receive] Running Thread");
                int nSeq = 0;
                while (m_lstSerial != null)
                {
                    int nSize = m_lstSerial[nSeq].GetBuffer_Length();
                    if (nSize > 0)
                    {
                        buf = m_lstSerial[nSeq].GetBytes();
                        //Ojw.CMessage.Write("[Receive]");
                        //Ojw.CConvert.ByteToStructure(

                        //if (m_nProtocolVersion == 1)
                        //{
                        //    //continue;
                        //    //Parsor1(buf, nSize);
                        //}
                        //else // (m_aSParam_Axis[nAxis].nProtocol_Version == 2) 
                        //{
                        //    //Parsor(buf, nSize);
                        //}
                        //Ojw.CMessage.Write("");


                        foreach(byte byData in buf)
                        {
                            Ojw.CMessage.Write2("0x{0}", Ojw.CConvert.IntToHex(byData, 2));
                        }
                        Ojw.CMessage.Write2("\r\n");

                    }

                    if ((nSeq + 1) >= m_lstSerial.Count) nSeq = 0;
                    else nSeq++;

                    Thread.Sleep(1);
                }

                Ojw.CMessage.Write("[Thread_Receive] Closed Thread");
            }

        #region Control
            public void Stop()
            {
            }
            public void Stop(int nMotor)
            {
            }
            public void Ems() // Emergency Signal/Switch
            {
            }
            public bool IsStop() { return m_bStop; }
            public bool IsEms() { return m_bEms; }

            public void Reset()
            {
                m_bStop = false;
                m_bEms = false;
            }
            public void Reboot(int nMotor)
            {
            }
            public void Reboot()
            {
            }
            public void SetTorq(int nMotor, bool bOn)
            {
                Write2(m_aSMot_Info[nMotor].nProtocol, m_aSMot_Info[nMotor].nCommIndex, ((nMotor < 0) ? 254 : (m_aSMot_Info[nMotor].nRealID)), 0x03, m_aSMot_Info[nMotor].SAddress.nTorq_64_1, (byte)((bOn == true) ? 1 : 0));                
            }
            public void SetTorq(bool bOn) 
            {
#if false
                int nProtocol = 2;
                int nCommIndex = 0;
                int nID = 254;
                int nTorqAddress = 64;
                
                //SetTorq(-1, bOn); 
                Write2(nProtocol, nCommIndex, nID, 0x03, nTorqAddress, (byte)((bOn == true) ? 1 : 0));     
#else
                if (m_lstSCheckMotorType.Count > 0) { foreach (SCheckMotorType_t SType in m_lstSCheckMotorType) Write2(SType.nProtocol, SType.nCommIndex, 254, 0x03, SType.nTorqAddress, (byte)((bOn == true) ? 1 : 0)); }
                else { for (int i = 0; i < m_lstPort.Count; i++) Write2(2, i, 254, 0x03, 64, (byte)((bOn == true) ? 1 : 0)); }
#endif
            }
#if true
            private void Push_RealID(int nRealID) { Push_RealID(0, nRealID); }
            private void Push_RealID(int nCommIndex, int nRealID) 
            {
                int nMot = GetMotor_ID(nCommIndex, nRealID); 
                if (nMot >= 0) Push(nMot);
            }
            private void Push(int nMotor) { if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return; if (IsCmd(nMotor) == false) m_anEn[m_nMotorCnt++] = nMotor; }
            private int Pop() { if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return -1; if (m_nMotorCnt > 0) return m_anEn[--m_nMotorCnt]; return -1; }
            public bool IsCmd(int nMotor) { for (int i = 0; i < m_nMotorCnt; i++) if (m_anEn[i] == nMotor) return true; return false; }   
#else
            private void Push(int nMotor) { if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return; if (IsCmd(nMotor) == false) m_SMot[m_nMotorCnt++] = nMotor; }
            private int Pop() { if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return -1; if (m_nMotorCnt > 0) return m_SMot[--m_nMotorCnt]; return -1; }
            public bool IsCmd(int nMotor)
            {
                for (int i = 0; i < m_nMotorCnt; i++) if (m_SMot[i] == nMotor) return true;
                return false;
            }         
#endif
            // 경우에 따라 리미트를 무시해야 할 경우 사용
            private bool m_bIgnoredLimit = false;
            public void SetLimitEn(bool bOn) { m_bIgnoredLimit = !bOn; }
            public bool GetLimitEn() { return !m_bIgnoredLimit; }
            public int Clip(int nLimitValue_Up, int nLimitValue_Dn, int nData)
            {
                if (GetLimitEn() == false) return nData;

                int nRet = ((nData > nLimitValue_Up) ? nLimitValue_Up : nData);
                return ((nRet < nLimitValue_Dn) ? nLimitValue_Dn : nRet);
            }
            public float Clip(float fLimitValue_Up, float fLimitValue_Dn, float fData)
            {
                if (GetLimitEn() == false) return fData;
                float fRet = ((fData > fLimitValue_Up) ? fLimitValue_Up : fData);
                return ((fRet < fLimitValue_Dn) ? fLimitValue_Dn : fRet);
            }

            public int CalcLimit_Evd(int nAxis, int nValue)
            {
                //if (Get_Flag_Mode(nAxis) == 0)// || (Get_Flag_Mode(nAxis) == 2))
                //{
                    int nPulse = nValue;// &0x4000;
                    //if (m_bMultiTurn == false)
                    //{
                        //nValue &= 0x4000;
                        //nValue &= 0x3fff;
                    //}

                    //nValue &= 0x3fff;
                    int nUp = 10000000;
                    int nDn = -nUp;
                    if (m_aSMot_Info[nAxis].fLimitUp != 0) nUp = CalcAngle2Evd(nAxis, m_aSMot_Info[nAxis].fLimitUp);
                    if (m_aSMot_Info[nAxis].fLimitDn != 0) nDn = CalcAngle2Evd(nAxis, m_aSMot_Info[nAxis].fLimitDn);
                    if (nUp < nDn) { int nTmp = nUp; nUp = nDn; nDn = nTmp; }
                    return (Clip(nUp, nDn, nValue) | nPulse);
                //}
                return nValue;
            }
            public float CalcLimit_Angle(int nAxis, float fValue)
            {
                //if (Get_Flag_Mode(nAxis) == 0)// || (Get_Flag_Mode(nAxis) == 2))
                //{
                    float fUp = 1000000.0f;
                    float fDn = -fUp;
                    if (m_aSMot_Info[nAxis].fLimitUp != 0) fUp = m_aSMot_Info[nAxis].fLimitUp;
                    if (m_aSMot_Info[nAxis].fLimitDn != 0) fDn = m_aSMot_Info[nAxis].fLimitDn;
                    return Clip(fUp, fDn, fValue);
                //}
                return fValue;
            }
            public int CalcAngle2Evd(int nAxis, float fValue)
            {
                fValue *= ((m_aSMot_Info[nAxis].nDir == 0) ? 1.0f : -1.0f);
                int nData = 0;
                //if (Get_Flag_Mode(nAxis) != 0)   // 속도제어
                //{
                //    nData = (int)Math.Round(fValue);
                //    //Ojw.CMessage.Write("Speed Turn");
                //}
                //else
                {
                    // 위치제어
                    nData = (int)Math.Round((m_aSMot_Info[nAxis].fMechMove * fValue) / m_aSMot_Info[nAxis].fDegree);
                    nData = nData + (int)Math.Round(m_aSMot_Info[nAxis].fCenterPos);
                }

                return nData;
            }
            public float CalcEvd2Angle(int nAxis, int nValue)
            {
                float fValue = ((m_aSMot_Info[nAxis].nDir == 0) ? 1.0f : -1.0f);
                float fValue2 = 0.0f;
                //if (Get_Flag_Mode(nAxis) != 0)   // 속도제어
                //    fValue2 = (float)nValue * fValue;
                //else                                // 위치제어
                {
                    fValue2 = (float)(((m_aSMot_Info[nAxis].fDegree * ((float)(nValue - (int)Math.Round(m_aSMot_Info[nAxis].fCenterPos)))) / m_aSMot_Info[nAxis].fMechMove) * fValue);
                }
                return fValue2;
            }

            public void Set_Evd(int nAxis, int nEvd)
            {
                if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return;
                Push(nAxis);
                //Read_Motor_Push(nAxis);
                m_aSMot[nAxis].bEn = true;
                //m_aSMot[nAxis].
                    //Set_Flag_Mode(nAxis, false);
                m_aSMot[nAxis].nOperationMode = 3;
                m_aSMot[nAxis].lEvd = (long)CalcLimit_Evd(nAxis, nEvd);
                //Set_Flag_NoAction(nAxis, false);
                //Push_Id(nAxis);	
            }
            public long Get_Evd(int nAxis) { return m_aSMot[nAxis].lEvd; }

            public void Set(int nAxis, float fAngle)
            {
                if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return;
                Push(nAxis);
                //Read_Motor_Push(nAxis);
                m_aSMot[nAxis].bEn = true;
                m_aSMot[nAxis].nOperationMode = 3;
                //Set_Flag_Mode(nAxis, false);
                m_aSMot[nAxis].lEvd = (long)CalcLimit_Evd(nAxis, CalcAngle2Evd(nAxis, fAngle));
                //Set_Flag_NoAction(nAxis, false);
                //Push_Id(nAxis);	
            }
            public float Get(int nAxis) { return CalcEvd2Angle(nAxis, (int)m_aSMot[nAxis].lEvd); }

            // ---- Speed Control ----
            //public void Set_Turn(int nAxis, int nEvd)
            //{
            //    if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return;
            //    Push_Id(nAxis);
            //    Read_Motor_Push(nAxis);
            //    m_aSMot[nAxis].bEn = true;
            //    Set_Flag_Mode(nAxis, true);
            //    m_aSMot[nAxis].fPos = CalcLimit_Evd(nAxis, nEvd);
            //    Set_Flag_NoAction(nAxis, false);
            //    //Push_Id(nAxis);	
            //}
        #endregion Control

            //public void Set(int nAxis, float fAngle)
            //{
            //}
            //public void Set(int nAxis, float fAngle, float fRpm)
            //{
            //}
            //public float Get(int nAxis)
            //{
            //    return 0.0f;
            //}
            //public void Send(float fMilliSeconds)
            //{

            //}


        #region Protocol - basic
            private int updateCRC(byte[] data_blk_ptr, int data_blk_size)
            {
                int i, j = 0;
                int[] anCrcTable = new int[256] { 
                    0x0000, 0x8005, 0x800F, 0x000A, 0x801B, 0x001E, 0x0014, 0x8011, 0x8033, 0x0036, 0x003C, 0x8039, 0x0028, 0x802D, 0x8027, 0x0022, 0x8063, 0x0066, 0x006C, 0x8069, 0x0078, 0x807D, 0x8077, 0x0072, 0x0050, 0x8055, 0x805F, 0x005A, 0x804B, 0x004E, 0x0044, 0x8041, 0x80C3, 0x00C6, 0x00CC, 0x80C9, 0x00D8, 0x80DD, 0x80D7, 0x00D2, 
                    0x00F0, 0x80F5, 0x80FF, 0x00FA, 0x80EB, 0x00EE, 0x00E4, 0x80E1, 0x00A0, 0x80A5, 0x80AF, 0x00AA, 0x80BB, 0x00BE, 0x00B4, 0x80B1, 0x8093, 0x0096, 0x009C, 0x8099, 0x0088, 0x808D, 0x8087, 0x0082, 0x8183, 0x0186, 0x018C, 0x8189, 0x0198, 0x819D, 0x8197, 0x0192, 0x01B0, 0x81B5, 0x81BF, 0x01BA, 0x81AB, 0x01AE, 0x01A4, 0x81A1, 
                    0x01E0, 0x81E5, 0x81EF, 0x01EA, 0x81FB, 0x01FE, 0x01F4, 0x81F1, 0x81D3, 0x01D6, 0x01DC, 0x81D9, 0x01C8, 0x81CD, 0x81C7, 0x01C2, 0x0140, 0x8145, 0x814F, 0x014A, 0x815B, 0x015E, 0x0154, 0x8151, 0x8173, 0x0176, 0x017C, 0x8179, 0x0168, 0x816D, 0x8167, 0x0162, 0x8123, 0x0126, 0x012C, 0x8129, 0x0138, 0x813D, 0x8137, 0x0132,
                    0x0110, 0x8115, 0x811F, 0x011A, 0x810B, 0x010E, 0x0104, 0x8101, 0x8303, 0x0306, 0x030C, 0x8309, 0x0318, 0x831D, 0x8317, 0x0312, 0x0330, 0x8335, 0x833F, 0x033A, 0x832B, 0x032E, 0x0324, 0x8321, 0x0360, 0x8365, 0x836F, 0x036A, 0x837B, 0x037E, 0x0374, 0x8371, 0x8353, 0x0356, 0x035C, 0x8359, 0x0348, 0x834D, 0x8347, 0x0342, 
                    0x03C0, 0x83C5, 0x83CF, 0x03CA, 0x83DB, 0x03DE, 0x03D4, 0x83D1, 0x83F3, 0x03F6, 0x03FC, 0x83F9, 0x03E8, 0x83ED, 0x83E7, 0x03E2, 0x83A3, 0x03A6, 0x03AC, 0x83A9, 0x03B8, 0x83BD, 0x83B7, 0x03B2, 0x0390, 0x8395, 0x839F, 0x039A, 0x838B, 0x038E, 0x0384, 0x8381, 0x0280, 0x8285, 0x828F, 0x028A, 0x829B, 0x029E, 0x0294, 0x8291, 
                    0x82B3, 0x02B6, 0x02BC, 0x82B9, 0x02A8, 0x82AD, 0x82A7, 0x02A2, 0x82E3, 0x02E6, 0x02EC, 0x82E9, 0x02F8, 0x82FD, 0x82F7, 0x02F2, 0x02D0, 0x82D5, 0x82DF, 0x02DA, 0x82CB, 0x02CE, 0x02C4, 0x82C1, 0x8243, 0x0246, 0x024C, 0x8249, 0x0258, 0x825D, 0x8257, 0x0252, 0x0270, 0x8275, 0x827F, 0x027A, 0x826B, 0x026E, 0x0264, 0x8261, 
                    0x0220, 0x8225, 0x822F, 0x022A, 0x823B, 0x023E, 0x0234, 0x8231, 0x8213, 0x0216, 0x021C, 0x8219, 0x0208, 0x820D, 0x8207, 0x0202 };
                int nCrc_accum = 0;
                for (j = 0; j < data_blk_size; j++) nCrc_accum = (nCrc_accum << 8) ^ anCrcTable[(((nCrc_accum >> 8) ^ data_blk_ptr[j]) & 0xFF)];
                return nCrc_accum;
            }
            private void MakeStuff(ref byte[] pBuff)
            {
                int nStuff = 0;
                int[] pnIndex = new int[pBuff.Length];
                Array.Clear(pnIndex, 0, pnIndex.Length);
                int nCnt = 0;
                for (int i = 5; i < pBuff.Length; i++)
                {
                    switch (nStuff)
                    {
                        case 0: { if (pBuff[i] == 0xff) nStuff++; } break;
                        case 1: { if (pBuff[i] == 0xff) nStuff++; else nStuff = 0; } break;
                        case 2:
                            {
                                if (pBuff[i] == 0xfd)
                                {
                                    nStuff++;
                                    pnIndex[nCnt++] = i;
                                }
                                else
                                {
                                    nStuff = 0;
                                }
                            }
                            break;
                    }
                }
                if (nCnt > 0)
                {
                    byte[] pBuff2 = new byte[pBuff.Length];
                    Array.Copy(pBuff, pBuff2, pBuff.Length);
                    Array.Resize<byte>(ref pBuff, pBuff2.Length + nCnt);
                    int nIndex = 0;
                    int nPos = 0;
                    foreach (byte byTmp in pBuff)
                    {
                        pBuff[nIndex + nPos] = pBuff2[nIndex];
                        if (nIndex == pnIndex[nPos])
                        {
                            pBuff[nIndex + nPos + 1] = 0xfd;
                            nPos++;
                        }
                        nIndex++;
                    }
                    pBuff2 = null;
                }
                pnIndex = null;
            }
            private void SendPacket(int nPortIndex, byte[] buffer, int nLength) { if (m_lstSerial[nPortIndex].IsConnect() == true) m_lstSerial[nPortIndex].SendPacket(buffer, nLength); }
        #endregion Protocol - basic
        #region Protocol - Command
            private int m_nProtocol_Version = 2;
            private int m_nSerialIndex = 0;
            public void SetProtocol(int nProtocol_Version) { m_nProtocol_Version = nProtocol_Version; }
            public void SetPortIndex(int nIndex)
            {
                m_nSerialIndex = nIndex;
                if (m_lstSerial == null) m_nSerialIndex = -1;
                else if (m_nSerialIndex >= m_lstSerial.Count) m_nSerialIndex = m_lstSerial.Count - 1;
            }
            public string MakePingPacket_String(int nMotorID, int nProtocol_Version)
            {
                byte[] pbyteBuffer = MakePingPacket(nMotorID, nProtocol_Version);

                string strPing = String.Empty;// "=>";
                foreach (byte byData in pbyteBuffer) strPing += String.Format(",0x{0}", Ojw.CConvert.IntToHex(byData, 2));
                return strPing;
            }
            public byte[] MakePingPacket(int nMotorID, int nProtocol_Version)
            {
                int i = 0;
                byte[] pbyteBuffer;
                if (nProtocol_Version == 1)
                {
                    int nLength = 2; // 파라미터의 갯수 + 2
                    int nDefaultSize = 4;
                    pbyteBuffer = new byte[nDefaultSize + nLength];
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = (byte)(nMotorID & 0xff);
                    pbyteBuffer[i++] = (byte)(nLength & 0xff);
                    pbyteBuffer[i++] = (byte)(0x01);

                    int nCrc = 0;
                    for (int j = 2; j < pbyteBuffer.Length - 1; j++) nCrc += pbyteBuffer[j];
                    pbyteBuffer[i++] = (byte)(~nCrc & 0xff);

                    //pbyteBuffer[pbyteBuffer.Length - 1] = (byte)(nCrc & 0xff);

                }
                else // m_aSParam_Axis[nAxis].nProtocol_Version == 2
                {
                    int nLength = 3;
                    int nDefaultSize = 7;
                    pbyteBuffer = new byte[nDefaultSize + nLength];
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = 0xff;
        #region Packet 2.0
                    pbyteBuffer[i++] = 0xfd;
                    pbyteBuffer[i++] = 0x00;
        #endregion Packet 2.0
                    pbyteBuffer[i++] = (byte)(nMotorID & 0xff);
                    pbyteBuffer[i++] = (byte)(nLength & 0xff);
                    pbyteBuffer[i++] = (byte)((nLength >> 8) & 0xff);
                    pbyteBuffer[i++] = (byte)(0x01);

                    MakeStuff(ref pbyteBuffer);
                    int nCrc = updateCRC(pbyteBuffer, pbyteBuffer.Length - 2);
                    pbyteBuffer[pbyteBuffer.Length - 2] = (byte)(nCrc & 0xff);
                    pbyteBuffer[pbyteBuffer.Length - 1] = (byte)((nCrc >> 8) & 0xff);

                    //SendPacket(nSerialIndex, pbyteBuffer, pbyteBuffer.Length);
                }
                return pbyteBuffer;
            }
            public void Write_Ping(int nProtocol_Version, int nSerialIndex, int nMotorID)
            {
                byte [] pbyteBuffer = MakePingPacket(nMotorID, nProtocol_Version);
                SendPacket(nSerialIndex, pbyteBuffer, pbyteBuffer.Length);
            }
            public void Write_Ping()
            {
                for (int nIndex = 0; nIndex < m_lstPort.Count; nIndex++)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        int nID = 254;
                        byte[] pbyteBuffer = MakePingPacket(nID, 2 - i);
                        SendPacket(nIndex, pbyteBuffer, pbyteBuffer.Length);
                        Ojw.CTimer.Wait(50); // 나중에 handshake 로 바꿀것.
                    }
                }
#if false
                if (m_lstSCheckMotorType.Count > 0)
                {
                    foreach (SCheckMotorType_t SType in m_lstSCheckMotorType)
                    {
                        //int nProtocol = 2;
                        //int nCommIndex = 0;
                        int nID = 254;
                        //int nTorqAddress = 64;

                        //SetTorq(-1, bOn); 
                        byte[] pbyteBuffer = MakePingPacket(nID, SType.nProtocol);
                        SendPacket(SType.nCommIndex, pbyteBuffer, pbyteBuffer.Length);
                    }
                }
#endif
            }
            public void Write_Command(int nMotorID, int nCommand) { Write_Command(m_nProtocol_Version, m_nSerialIndex, nMotorID, nCommand); }            
            public void Write_Command(int nProtocol_Version, int nSerialIndex, int nMotorID, int nCommand)
            {
                int i;
                //int nID = ((nAxis == 254) ? 254 : m_aSMot[nAxis].nID);
                if (nProtocol_Version == 1)
                {
                    int nLength = 1 + 2;
                    int nDefaultSize = 6;
                    byte[] pbyteBuffer = new byte[nDefaultSize + nLength];
                    i = 0;
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = (byte)(nMotorID & 0xff);
                    pbyteBuffer[i++] = (byte)(nLength & 0xff);
                    pbyteBuffer[i++] = (byte)(nCommand & 0xff);

                    int nCrc = 0;
                    for (int j = 2; i < pbyteBuffer.Length - 1; j++) nCrc += pbyteBuffer[j];
                    pbyteBuffer[i++] = (byte)(~nCrc & 0xff);

                    pbyteBuffer[pbyteBuffer.Length - 1] = (byte)(nCrc & 0xff);

                    SendPacket(nSerialIndex, pbyteBuffer, pbyteBuffer.Length);
                }
                else // if (m_aSParam_Axis[nAxis].nProtocol_Version == 2)
                {
                    int nLength = 3;
                    int nDefaultSize = 7;
                    byte[] pbyteBuffer = new byte[nDefaultSize + nLength];
                    i = 0;
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = 0xfd;
                    pbyteBuffer[i++] = 0x00;
                    pbyteBuffer[i++] = (byte)(nMotorID & 0xff);
                    pbyteBuffer[i++] = (byte)(nLength & 0xff);
                    pbyteBuffer[i++] = (byte)((nLength >> 8) & 0xff);
                    pbyteBuffer[i++] = (byte)(nCommand & 0xff);

                    MakeStuff(ref pbyteBuffer);
                    int nCrc = updateCRC(pbyteBuffer, pbyteBuffer.Length - 2);
                    pbyteBuffer[pbyteBuffer.Length - 2] = (byte)(nCrc & 0xff);
                    pbyteBuffer[pbyteBuffer.Length - 1] = (byte)((nCrc >> 8) & 0xff);

                    SendPacket(nSerialIndex, pbyteBuffer, pbyteBuffer.Length);
                }
            }
            public void Write(int nMotorID, int nCommand, int nAddress, params byte[] pbyDatas) { Write2(m_nProtocol_Version, m_nSerialIndex, nMotorID, nCommand, nAddress, pbyDatas); }
            public void Write2(int nProtocol_Version, int nSerialIndex, int nMotorID, int nCommand, int nAddress, params byte[] pbyDatas)
            {
                int i;

                //int nID = 0;//((nAxis == 254) ? 254 : m_aSMot[nAxis].nID);

                i = 0;
                if (nProtocol_Version == 1)
                {
                    int nLength = 2 + ((pbyDatas != null) ? pbyDatas.Length + 1 : 0); // 파라미터의 갯수 + 2
                    int nDefaultSize = 4;
                    byte[] pbyteBuffer = new byte[nDefaultSize + nLength];
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = (byte)(nMotorID & 0xff);
                    pbyteBuffer[i++] = (byte)(nLength & 0xff);
                    pbyteBuffer[i++] = (byte)(nCommand & 0xff);
                    if (pbyDatas != null)
                    {
                        pbyteBuffer[i++] = (byte)(nAddress & 0xff);
                        foreach (byte byData in pbyDatas) pbyteBuffer[i++] = byData;
                    }
                    int nCrc = 0;
                    for (int j = 2; j < pbyteBuffer.Length - 1; j++) nCrc += pbyteBuffer[j];
                    pbyteBuffer[i++] = (byte)(~nCrc & 0xff);
                    
                    SendPacket(nSerialIndex, pbyteBuffer, pbyteBuffer.Length);
                }
                else // m_aSParam_Axis[nAxis].nProtocol_Version == 2
                {
                    int nLength = 3 + 2 + pbyDatas.Length;
                    int nDefaultSize = 7;
                    byte[] pbyteBuffer = new byte[nDefaultSize + nLength];
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = 0xff;
        #region Packet 2.0
                    pbyteBuffer[i++] = 0xfd;
                    pbyteBuffer[i++] = 0x00;
        #endregion Packet 2.0
                    pbyteBuffer[i++] = (byte)(nMotorID & 0xff);
                    pbyteBuffer[i++] = (byte)(nLength & 0xff);
                    pbyteBuffer[i++] = (byte)((nLength >> 8) & 0xff);
                    pbyteBuffer[i++] = (byte)(nCommand & 0xff);
                    pbyteBuffer[i++] = (byte)(nAddress & 0xff);
                    pbyteBuffer[i++] = (byte)((nAddress >> 8) & 0xff);
                    if (pbyDatas != null)
                        foreach (byte byData in pbyDatas) pbyteBuffer[i++] = byData;

                    MakeStuff(ref pbyteBuffer);
                    int nCrc = updateCRC(pbyteBuffer, pbyteBuffer.Length - 2);
                    pbyteBuffer[pbyteBuffer.Length - 2] = (byte)(nCrc & 0xff);
                    pbyteBuffer[pbyteBuffer.Length - 1] = (byte)((nCrc >> 8) & 0xff);

                    SendPacket(nSerialIndex, pbyteBuffer, pbyteBuffer.Length);
                }
            }
            public void Writes(int nProtocol_Version, int nSerialIndex, int nAddress, int nDataLength_without_ID, int nMotorCnt, params byte[] pbyDatas)
            {
                int i = 0;
                int nLength = (nDataLength_without_ID + 1) * nMotorCnt;
                if (nProtocol_Version != 1)
                {
                    byte[] pbyteBuffer = new byte[2 + nLength];
                    pbyteBuffer[i++] = (byte)(nDataLength_without_ID & 0xff);
                    pbyteBuffer[i++] = (byte)((nDataLength_without_ID >> 8) & 0xff);
                    Array.Copy(pbyDatas, 0, pbyteBuffer, i, nLength);
                    Write2(nProtocol_Version, nSerialIndex, 0xfe, 0x83, nAddress, pbyteBuffer);
                    pbyteBuffer = null;
                }
                else // (== 1)
                {
                    // Instruction(1) + Address(2) + Data_Length(2) + 
                    byte[] pbyteBuffer = new byte[1 + nLength];
                    pbyteBuffer[i++] = (byte)(nDataLength_without_ID & 0xff);
                    Array.Copy(pbyDatas, 0, pbyteBuffer, i, nLength);
                    Write2(nProtocol_Version, nSerialIndex, 0xfe, 0x83, nAddress, pbyteBuffer);
                    pbyteBuffer = null;
                }
            }
#if false
            public void Writes_Data(int nProtocol_Version, int nSerialIndex, int nAddress, int nAddress_Size, params int [] pnIds_nDatas)
            {
                int nCount = pnIds_nDatas.Length / 2;
                byte [] pbyTmp;
                byte[] pbyBuffer = new byte[nCount * (nAddress_Size + 1)];
                if (nProtocol_Version == 1)
                {
                }
                else
                {
                    for (int i = 0; i < nCount; i++)
                    {
                        pbyBuffer[i*(nAddress_Size + 1)] = (byte)(pnIds_nDatas[i * 2]);
                        pbyTmp = pnIds_nDatas[i * 2 + 1]);
                        Array.Copy(pbyTmp, 0, 
                    }
                    pbyTmp = Ojw.CConvert.IntToBytes((int)Math.Round(Get_Speed(nAxis)));
                    Array.Copy(pbyTmp, 0, pbyteBuffer_Spd, nIndex_Spd, pbyTmp.Length);
                }
                

                if (nProtocol_Version == 1)
                {
                    //switch (nMode)
                    //{
                        //case 0: 
                    Writes(nAddr_Spd, nAddress_Size, nCnt2, pbyteBuffer_Spd); Writes(nAddr_Pos, nAddress_Size, nCnt2, pbyteBuffer_Pos); //break;
                        //case 1: Writes(nAddr_Spd, nAddr_Size, nCnt2, pbyteBuffer_Spd); break;
                        ////case 2: Writes(102, 4, nCnt, pbyteBuffer); break;
                        //default: break;
                    //}
                }
                else
                {

                    //switch (nMode)
                    //{
                        //case 0: 
                    Writes(nAddr_Pos_Spd, nAddress_Size, nCnt2, pbyteBuffer_Spd); Writes(nAddr_Pos, nAddress_Size, nCnt2, pbyteBuffer_Pos); //break;
                        //case 1: Writes(nAddr_Spd, nAddr_Size, nCnt2, pbyteBuffer_Spd); break;
                        ////case 2: Writes(102, 4, nCnt, pbyteBuffer); break;
                        //default: break;
                    //}
                }
            }
#else
            public void Writes(int nProtocol_Version, int nSerialIndex, int nAddress, int nDataLength_without_ID, int nMotorCnt, params byte[] pbyDatas)
            {
                int i = 0;
                int nLength = (nDataLength_without_ID + 1) * nMotorCnt;
                if (nProtocol_Version != 1)
                {
                    byte[] pbyteBuffer = new byte[2 + nLength];
                    pbyteBuffer[i++] = (byte)(nDataLength_without_ID & 0xff);
                    pbyteBuffer[i++] = (byte)((nDataLength_without_ID >> 8) & 0xff);
                    Array.Copy(pbyDatas, 0, pbyteBuffer, i, nLength);
                    Write(0xfe, 0x83, nAddress, pbyteBuffer);
                    pbyteBuffer = null;
                }
                else
                {
                    byte[] pbyteBuffer = new byte[1 + nLength];
                    pbyteBuffer[i++] = (byte)(nDataLength_without_ID & 0xff);
                    Array.Copy(pbyDatas, 0, pbyteBuffer, i, nLength);
                    Write(0xfe, 0x83, nAddress, pbyteBuffer);
                    pbyteBuffer = null;
                }
            }
            // int nMotor, int nSpeed
            public void Writes_Speed(params int [] anMotor_and_Speed)
            {
                int nCount = anMotor_and_Speed.Length / 2;
                byte[] pbyTmp_Short;
                for (int i = 0; i < nCount; i++)
                {
                    // 속도

                    pbyTmp_Short = Ojw.CConvert.ShortToBytes((short)anMotor_and_Speed[i * 2 + 1]);
                Array.Copy(pbyTmp_Short, 0, pbyteBuffer_Spd, nIndex_Spd, pbyTmp_Short.Length);
            }
#endif
            //public void Write(int nProtocol_Version, int nSerialIndex, int nAxis, int nAddress, params byte[] pbyDatas) { Write2(nProtocol_Version, nSerialIndex, nAxis, 0x03, nAddress, pbyDatas); }
        #endregion Protocol - Command
        }
#endif
#endif
    }
}
