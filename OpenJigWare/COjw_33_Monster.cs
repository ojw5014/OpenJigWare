#define _NO_CHANGE_OPERATION
#define _DUPLICATE // 아이디 중복 제거
//#define _THREAD_
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
using System.IO;

#region 참고자료
#region 다이나믹셀 프로토콜1 과 프로토콜 2#
#if false
15, 1M - 0, 1, 2(ax-12a)
    57600 - 1(xl-430)
====================================
<<protocol1>>
[Write]
0xff, 0xff, id, length, instruction, param........., checksum (length = param... + 2)
[Read]
0xff, 0xff, id, length, Error, param........., checksum (length = param... + 2)

 => ping return     0xff 0xff ID, Length(=2), Error, Checksum
[명령]
0x01 PING  	수행 내용 없음. 제어기가 Status Packet을 받고자할 경우 사용 			0
0x02 READ_DATA 	다이나믹셀의 데이터를 읽는 명령 							2
0x03 WRITE_DATA 다이나믹셀의 데이터를 쓰는 명령							2 이상
0x04 REG WRITE  WRTE_DATA와 내용은 유사하나, 대기상태로 있다가 ACTION 명령이 도착해야 수행됨 	2 이상
0x05 ACTION 	REG WRITE 로 등록된 동작을 시작하라는 명령					0
0x06 RESET 	다이나믹셀의 상태를 공장 출하 상태로 복귀시키는 명령 				0
0x83 SYNC WRITE 한번에 여러 개의 다이나믹셀을 동시에 제어하고자 할때 사용되는 명령			4 이상
0x92 BULK READ  한번의 명령으로 여러 개의 다이나믹셀의 데이터를 순차적으로 읽음. 			4 이상
		(단, 이명령은 MX시리즈에서만 사용할 수 있습니다.)

[Checksum]
Check Sum = ~ ( ID + Length + Instruction + Parameter1 + … Parameter N )
[Error]
Bit 7 0 			-
Bit 6 Instruction Error	정의되지 않은 Instruction이 전송된 경우. 또는 reg_write명령없이 action명령이 전달된 경우 1로 설정됨
Bit 5 Overload Error	설정된 Torque로 현재의 하중을 제어할 수 없을 때 1로 설정됨
Bit 4 Checksum Error 	전송된 Instruction Packet의 Checksum이 맞지 않을 때 1로 설정됨
Bit 3 Range Error	사용범위를 벗어난 명령일 경우 1로 설정됨
Bit 2 Overheating Error	Dynamixel 내부 온도가 Control Table에 설정된 동작 온도 범위를 벗어났을 때 1로 설정됨
Bit 1 Angle Limit Error	Goal Position이 CW Angle Limit ~ CCW Angle Limit 범위 밖의 값으로 Writing 되었을때 1로 설정됨
Bit 0 Input Volt Error 	Error인가된 전압이 Control Table에 설정된 동작 전압 범위를 벗어났을 경우 1로 설정됨
======================================
[protocol 2]
0xff, 0xff, 0xfd, 0x00, id, length(2bytes), instruction, param........., CRC(2bytes) (length = param... + 3)

[명령]
0x01 Ping  		Packet ID와 동일한 ID를 갖은 장치에 Packet이 도달했는지 여부 확인을 위한 Instruction
0x02 Read  		장치로부터 데이터를 읽어오기 위한 Instruction
0x03 Write 		장치에 데이터를 쓰기 위한 Instruction
0x04 Reg Write		Instruction Packet을 대기 상태로 등록하는 Instruction, Action 명령에 의해 실행됨
0x05 Action		Reg Write 로 미리 등록한 Packet을 실행하는 Instruction
0x06 Factory Reset	컨트롤테이블을 공장 출하 상태의 초기값으로 되돌리는 Instruction
0x08 Reboot		장치를 재부팅 시키는 Instruction
0x55 Status(Return)	Instruction Packet 에 대한 Return Instruction
0x82 Sync Read		다수의 장치에 대해서, 동일한 Address에서 동일한 길이의 데이터를 한 번에 읽기 위한 Instruction
0x83 Sync Write 	다수의 장치에 대해서, 동일한 Address에 동일한 길이의 데이터를 한 번에 쓰기 위한 Instruction
0x92 Bulk Read		다수의 장치에 대해서, 서로 다른 Address에서 서로 다른 길이의 데이터를 한 번에 읽기 위한 Instruction

#endif
#endregion 다이나믹셀 프로토콜1 과 프로토콜 2
#region 모터사양-다이나믹셀
#if false
vdx113(0x71:113)   - 9~12v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm, sttrq=1.0, 54rpm (바퀴모드 지원 안함)
dx116(0x74:116)   - 12~16v, 0~300, c=512, mx=1023, mn=0, drpm=0.111rpm (바퀴모드 지원 안함)
dx117(0x75:117)   - 12~18.5v, 0~300, c=512, mx=1023, mn=0, drpm=0.111rpm (바퀴모드 지원 안함)

ax-12(0x2c:44, 0x012c:300)  - 9~12v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시, 1024 기준 100% 의 출력 비율로 속도 조절)
ax-18(0x12:18)

Rx-10  (10) - 9~12v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시, 1024 기준 100% 의 출력 비율로 속도 조절)
Rx-24F (24) - 9~12v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시, 1024 기준 100% 의 출력 비율로 속도 조절)
Rx-28  (28) - 12v~18.5v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시, 1024 기준 100% 의 출력 비율로 속도 조절)
Rx-64  (64) - 12v~18.5v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시, 1024 기준 100% 의 출력 비율로 속도 조절)

=> 여기까지 메모리 0~49 의 50개 레지스터(0~23 Rom)

Ex-106+ (107)	- 12v~18.5v, 0~251도, c=2048, mx=4095, mn=0, (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시, 1024 기준 100% 의 출력 비율로 속도 조절)
=> 0~57의 58개 레지스터(0~23 Rom)
////////////////////////////////////////////////////////////////////
[ CW/CCW
 바퀴 모드      : 둘 다 0인 값
 관절 모드      : 둘 다 0이 아닌 값
 다중 회전 모드 : 둘 다 4095 인 값
] -> 이건 1.0 프로토콜의 경우, 2.0 프로토콜 사용시 레지스터 2.0과 동일
mx-12w(0x0168:360)  - 10~14.8v, 0~360, c=2048, mx=4095, mn=0, drpm = 0.916rpm, 다중회전가능(각 7바퀴:-28,672~28,672)
mx-28(0x1d:29, 2.0:30)   - 10~14.8v, 0~360, c=2048, mx=4095, mn=0, drpm = 0.229rpm, 다중회전가능(각 7바퀴:-28,672~28,672)  
mx-64(0x0136:310, 2.0:311) drpm = 0.229
mx-106(2.0: 321)  - 10~14.8v, 0~360, c=2048, mx=4095, mn=0, drpm = 0.229rpm, 다중회전가능(각 7바퀴:-28,672~28,672)  

xl-320(0x015E:350)  - 6~8.4v, 0~300, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시)
=> 0~52 까지 53 개 레지스터(0~23 Rom)
xl-430(0x0424:1060)  - 6.5~12v, 0~360, c=2048, mx=4095, mn=0, drpm = 0.229rpm, 다중회전가능(각 256바퀴:-1,048,575~1,048,575)  
=> 0~146 까지 본 데이타(나머지는 Indirect address ~ 661 까지)(0~63 까지 Rom)


XM-430((W210)1030, (W350)1020)
XM-540((W150)1130, (W270)1120)
XH-430((W210)1010, (W350)1000, (V210)1050, (V350)1040)

pin
        [XL-430]
      -------------
     |             |
     |             |
     |             |
     |             |
    GND           Data
    VDD           VDD
    Data          GND
     |             |
     |             |
     |             |
     |             |
     |             |
      -------------

        [ 4 Pin ]
      -------------
     |             |
     |             |
     |             |
     |             |
    GND           D-
    VDD           D+
    D+            VDD
    D-            GND
     |             |
     |             |
     |             |
     |             |
     |             |
      -------------


version 42 Time 제어
(
  [셋팅]
   Drive Mode(10) -> 0x04
   Acceleration Limit[40(Size[4],Default[32767])] : 32767 -> 변경x,  		profile acceleration[108] 연관
   Velocity Limit[44(Size[4],Default[415])] : 415 -> 1023, 	profile velocity[112] 연관

  [동작]
      operation mode[11(Size[1])] : 3(default)   [ 4:extened position ]  [ 5: current base position ]

      profile velocity[112(size[4])] :  ... ms
      Goal position[116(size[4])] : ...
    
  1. 사전에 Drive mode(10) 의 0x04 가 1로 셋
  2. 
#endif
#endregion 모터사양-다이나믹셀
#endregion 참고자료

namespace OpenJigWare
{
    partial class Ojw
    {
        public class CMonster
        {
            #region Define

            public enum EMotor_t
            {
#if true
                NONE = 0,
                // Default
                XL_320 = 350, // 300도 1024 : 512 center : 0.111 RefRpm, 1023 Limit Rpm
                XL_430 = 1060, // 360도 4096 : 2048 center : 0.229 RefRpm, 480 Limit Rpm
                AX_12 = 12, // 9~12v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시)
                AX_18 = 18, //

                //XM_430 = 29,
                //XM_540 = 30, // LED [65], 360도 4096 : 2048 center : 0.229 RefRpm, 480 Limit Rpm  , 확장위치제어모드시 512 회전 가능(+-256)

                XL_2XL = 1090,

                XM430_W210 = 1030,
                XM430_W350 = 1020,

                XM540_W270 = 1120,
                XM540_W150 = 1130,

                XH430_W210 = 1010,
                XH430_W350 = 1000,
                XH430_V210 = 1050,
                XH430_V350 = 1040,

                DX_113 = 113, //
                DX_116 = 116, //
                DX_117 = 117, //
                RX_10 = 10, // 9~12v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시)
                RX_24F = 24, // 9~12v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시)
                RX_28 = 28, // 12v~18.5v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시)
                RX_64 = 64, // 12v~18.5v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시)
                EX_106 = 106, // 12v~18.5v, 0~251도, c=2048, mx=4095, mn=0, 
                EX_106P = 107, // 12v~18.5v, 0~251도, c=2048, mx=4095, mn=0, 


                // protocol 2.0
                MX_12 = 360, // 10~14.8v, 0~360, c=2048, mx=4095, mn=0, drpm = 0.916rpm, 다중회전가능(각 7바퀴:-28,672~28,672)
                MX_28 = 30, // 10~14.8v, 0~360, c=2048, mx=4095, mn=0, drpm = 0.114rpm, 다중회전가능(각 7바퀴:-28,672~28,672)  
                MX_64 = 311, // 10~14.8v, 0~360, c=2048, mx=4095, mn=0, drpm = 0.114rpm, 다중회전가능(각 7바퀴:-28,672~28,672)  
                MX_106 = 321, // 10~14.8v, 0~360, c=2048, mx=4095, mn=0, drpm = 0.114rpm, 다중회전가능(각 7바퀴:-28,672~28,672)  
                //MX_ = 17, //

                SG_90 = 100
#else
                NONE = 0,
                // Default
                XL_320 = 1, // 300도 1024 : 512 center : 0.111 RefRpm, 1023 Limit Rpm
                XL_430 = 2, // 360도 4096 : 2048 center : 0.229 RefRpm, 480 Limit Rpm
                AX_12 = 3, // 9~12v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시)
                AX_18 = 4, //

                //XM_430 = 29,
                //XM_540 = 30, // LED [65], 360도 4096 : 2048 center : 0.229 RefRpm, 480 Limit Rpm  , 확장위치제어모드시 512 회전 가능(+-256)
                
                XL_2XL = 50,
                
                XM430_W210 = 28,
                XM430_W350 = 29,

                XM540_W270 = 30,
                XM540_W150 = 31,

                XH430_W210 = 32,
                XH430_W350 = 33,
                XH430_V210 = 34,
                XH430_V350 = 35,

                DX_113 = 5, //
                DX_116 = 6, //
                DX_117 = 7, //
                RX_10 = 8, // 9~12v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시)
                RX_24F = 9, // 9~12v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시)
                RX_28 = 10, // 12v~18.5v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시)
                RX_64 = 11, // 12v~18.5v, 0~300도, c=512, mx=1023, mn=0, drpm=0.111rpm (0x400, 즉 10 번째 비트로 방향 제어 - 속도 모드시)
                EX_106 = 12, // 12v~18.5v, 0~251도, c=2048, mx=4095, mn=0, 


                // protocol 2.0
                MX_12 = 13, // 10~14.8v, 0~360, c=2048, mx=4095, mn=0, drpm = 0.916rpm, 다중회전가능(각 7바퀴:-28,672~28,672)
                MX_28 = 14, // 10~14.8v, 0~360, c=2048, mx=4095, mn=0, drpm = 0.114rpm, 다중회전가능(각 7바퀴:-28,672~28,672)  
                MX_64 = 15, // 10~14.8v, 0~360, c=2048, mx=4095, mn=0, drpm = 0.114rpm, 다중회전가능(각 7바퀴:-28,672~28,672)  
                MX_106 = 16, // 10~14.8v, 0~360, c=2048, mx=4095, mn=0, drpm = 0.114rpm, 다중회전가능(각 7바퀴:-28,672~28,672)  
                //MX_ = 17, //

                SG_90 = 100
#endif
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
                    case 300: return EMotor_t.AX_12; // AX_12_W
                    case 10: return EMotor_t.RX_10;
                    case 24: return EMotor_t.RX_24F;
                    case 28: return EMotor_t.RX_28;
                    case 64: return EMotor_t.RX_64;
                    case 106: return EMotor_t.EX_106;// 이건 확실치 않으나 이런 경우가 있었음.
                    case 107: return EMotor_t.EX_106P;
                    case 360: return EMotor_t.MX_12;
                    ////////////////////////////////////
                    //여기부터 2.0
                    case 30: return EMotor_t.MX_28;
                    case 311: return EMotor_t.MX_64;
                    case 321: return EMotor_t.MX_106;
                    case 350: return EMotor_t.XL_320;

                    case 1000: return EMotor_t.XH430_W350;
                    case 1010: return EMotor_t.XH430_W210;
                    case 1020: return EMotor_t.XM430_W350;
                    case 1030: return EMotor_t.XM430_W210;
                    case 1040: return EMotor_t.XH430_V350;
                    case 1050: return EMotor_t.XH430_V210;
                    case 1060: return EMotor_t.XL_430;
                    case 1090: return EMotor_t.XL_2XL;

                    case 1120: return EMotor_t.XM540_W270;
                    case 1130: return EMotor_t.XM540_W150;
                                        
                    
                }
                return EMotor_t.NONE; ;
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
                private const int _ADDRESS_LENGTH = 20;//16;
            public class CAddress_t
            {
                public int nAddressStart = 100; // protocol2 - pwm
                public int nAddressLength = _ADDRESS_LENGTH;
                public int nProtocol;

                public bool IsMinusBit = false;

                // 잘 쓰는 주소번지 모음
                public int nLed = 65; // 430: 65(1)
                public int nLed_Size = 1;
                
                public int nTorq = 64; // 430: 64(1)
                public int nTorq_Size = 1;
                
                public int nDriveMode = 10; // 430: 10(1)
                public int nDriveMode_Size = 1;
                
                public int nOperatingMode = 11; // 430: 11(1)
                public int nOperatingMode_Size = 1;
                
                public int nWheel_Speed = 104; // 430: 104(4)
                public int nWheel_Speed_Size = 4;
                
                public int nAcceleration = 108; //430: 108(4)
                public int nAcceleration_Size = 4;

                public int nPos_Speed = 112; // 430: 112(4)
                public int nPos_Speed_Size = 4;

                public int nPwm = 100; // 430: 100(4)
                public int nPwm_Size = 4;

                public int nPos = 116; // 430: 116(4)
                public int nPos_Size = 4;

                //public int nGetPwm = 124;
                //public int nGetPwm_Size = 2;
                public int nGetTemp = 146; // 320: 46(1), ax: 43(1)
                public int nGetTemp_Size = 1;
                public int nGetVolt = 144; // 320: 45(1), ax: 42(1)
                public int nGetVolt_Size = 2;
                public int nGetLoad = 126; // 320 : 41(2), ax: 40(2)
                public int nGetLoad_Size = 2;
                public int nGetSpeed = 128; // 320 : 39(2), ax: 38(2)
                public int nGetSpeed_Size = 4;
                public int nGetPos = 132; // 320: 37(2), ax: 36(2)
                public int nGetPos_Size = 4;

                public int nGetSimple = 124; // 320: 37(11), ax: 36(8)
                public int nGetSimple_Size = 23;
                //public int nPos_Speed;
                //public int nPos_Speed_Size;
            }
            public class CMap_t
            {
                public int nSeq;
                public int nSeq_Back;

                public EMotor_t EMot;
                public int nSize;
                public int nRunningAddress = 0;
                public byte GetByte(int nAddress) { return buffer[nAddress]; }
                public short GetShort(int nAddress) { return Ojw.CConvert.BytesToShort(buffer, nAddress); }
                public ushort GetUShort(int nAddress) { return Ojw.CConvert.BytesToUShort(buffer, nAddress); }
                public int GetInt(int nAddress) { return Ojw.CConvert.BytesToInt(buffer, nAddress); }
                public uint GetUInt(int nAddress) { return Ojw.CConvert.BytesToUInt(buffer, nAddress); }
                public long GetLong(int nAddress) { return Ojw.CConvert.BytesToLong(buffer, nAddress); }
                public ulong GetULong(int nAddress) { return Ojw.CConvert.BytesToULong(buffer, nAddress); }
                public float GetFloat(int nAddress) { return Ojw.CConvert.BytesToFloat(buffer, nAddress); }
                public double GetDouble(int nAddress) { return Ojw.CConvert.BytesToDouble(buffer, nAddress); }
                public byte[] buffer = new byte[200]; // 100 개
            }
            public CMap_t[] m_aCMap = new CMap_t[255];
            public class CParam_t
            {
                public bool bEn = false;
                public EMotor_t EMot = EMotor_t.NONE;
                //public int nID;                     // 모터를 제어하기 위한 가상 아이디. 중복 허용 불가. 보통은 모터 ID 와 일치시킨다.            
                public int nCommIndex = 0;              // 연결 된 통신포트 중 어느것에 연결 된 것이지에 대한 순서.
                public int nRealID = 0;

                /////////////////////////////////////////////////////////////////
                public int nSeq = 0;
                public int nSeq_Back = 0;

                public int nDriveMode = 0;   // 0x00 - Rpm 모드, 0x04 - Time 제어 모드
                //public int nOperatingMode = 3; // Operating Mode (0-전류제어, 1-속도제어, 3-위치제어(default), 4-확장위치제어, 5-전류기반 위치제어(그리퍼), 16-PWM 제어
                public bool bTimeControl = true;
                public EOperation_t EOperation;
                public bool IsTorq = false;
                /////////////////////////////////////////////////////////////////

                public int nDir = 0; // forward = 0, inverse = 1;
                
                //Center
                public float fCenterPos = 2048.0f;

                public float fMechMove = 4096.0f;
                public float fDegree = 360.0f;
                public float fRefRpm = 0.229f;

                public float fLimitRpm = 415.0f;     // 0 ~ 1023

                public bool bLimitUp = false;
                public float fLimitUp = 360.0f;    // Limit - bLimitUp == false: Ignore
                public bool bLimitDown = false;
                public float fLimitDn = 0.0f;    // Limit - bLimitUp == false: Ignore

                public int nProtocol = 2;               // 2, 0 - protocol2, 1 - protocol1, 3 - None   

                public CAddress_t CAddress = new CAddress_t();
            }
            // private void SetAddress(int nMotor, EMotor_t EMot) { SetAddress(EMot, ref m_aCAddress[nMotor]); }
            private void SetAddress(EMotor_t EMot, ref CAddress_t CAddress)
            {
                CAddress.nProtocol = 2;
                CAddress.IsMinusBit = false;
                switch (EMot)
                {
                    #region AX &...
                    case EMotor_t.NONE: // 특별 추가
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
                        CAddress.IsMinusBit = true;
                        CAddress.nProtocol = 1;
                        CAddress.nLed = 25; // 430: 65(1)
                        CAddress.nLed_Size = 1;
                
                        CAddress.nTorq = 24; // 430: 64(1)
                        CAddress.nTorq_Size = 1;
                
                        CAddress.nDriveMode = 10; // 430: 10(1)
                        CAddress.nDriveMode_Size = 0;
                
                        CAddress.nOperatingMode = 6; // 430: 11(1)
                        //CAddress.nOperatingMode_Size = 5; // 속도제어시 0x0000000001, 위치제어시 [6]00, [7]00, [8]ff, [9]03 ( 65283 ) [10]02(XL320) 으로 셋팅해야 함. -> CW Limit 0, CCW Limit 1023, control mode 2 => 0x0000ff0302
                        CAddress.nOperatingMode_Size = 4; // 속도제어시 0x0000000001, 위치제어시 [6]00, [7]00, [8]ff, [9]03 ( 65283 ) 으로 셋팅해야 함. -> CW Limit 0, CCW Limit 1023, control mode 2 => 0x0000ff03
                
                        CAddress.nWheel_Speed = 32; // 430: 104(4)
                        CAddress.nWheel_Speed_Size = 2;
                
                        CAddress.nAcceleration = 108; //430: 108(4)
                        CAddress.nAcceleration_Size = 0;

                        CAddress.nPos_Speed = 32; // 430: 112(4)
                        CAddress.nPos_Speed_Size = 2;

                        CAddress.nPos = 30; // 430: 116(4)
                        CAddress.nPos_Size = 2;

                        CAddress.nAddressStart = 30;
                        CAddress.nAddressLength = 4;
                        CAddress.nGetTemp = 43; // 320: 46(1), ax: 43(1)
                        CAddress.nGetTemp_Size = 1;
                        CAddress.nGetVolt = 42; // 320: 45(1), ax: 42(1)
                        CAddress.nGetVolt_Size = 2;
                        CAddress.nGetLoad = 40; // 320 : 41(2), ax: 40(2)
                        CAddress.nGetLoad_Size = 2;
                        CAddress.nGetSpeed = 38; // 320 : 39(2), ax: 38(2)
                        CAddress.nGetSpeed_Size = 2;
                        CAddress.nGetPos = 36; // 320: 37(2), ax: 36(2)
                        CAddress.nGetPos_Size = 2;

                        
                        CAddress.nGetSimple = 36; // 320: 37(11), ax: 36(8)
                        CAddress.nGetSimple_Size = 8;
#if false
                        _ADDR_1_MotorNumber_0_2 = 0;        // R    0 : none
                        _ADDR_1_MotorNumber_Size_2 = 2;
                        _ADDR_1_FwVersion_6_1 = 6;          // R      
                        _ADDR_1_FwVersion_Size_1 = 1;

                        CAddress.nRealID_7_1 = 7;             // RW   0 ~ 252
                        CAddress.nRealID_Size_1 = 1;
                        CAddress.nBaudrate_8_1 = 8;           // RW   0-9600, 1-57600, 2-115200, 3-1M, 4-2M,  5-3M, 6-4M, 7-4.5M
                        CAddress.nBaudrate_Size_1 = 1;




                        CAddress.nTorq_64_1 = 24;              // RW   Off(0), On(1)
                        CAddress.nTorq_Size_1 = 1;
                        CAddress.nLed_65_1 = 25;               // RW   Off(0), On(1)
                        CAddress.nLed_Size_1 = 1;

                        CAddress.nMode_Operating_11_1 = 6;    // RW   1=속도제어(무한회전), 3(초기값)=위치제어(RPM), 4=Multi-Turn(-256~256:512회전), 5=전류기반 위치제어모드(-256~256:512회전), 16=PWM제어
                        CAddress.nMode_Operating_Size_1 = 4; // 속도제어시 0, 위치제어시 [6]00, [7]00, [8]ff, [9]03 ( 65283 ) 으로 셋팅해야 함. -> CW Limit 0, CCW Limit 1023

                        CAddress.nGoal_Vel_104_4 = 32;         // RW
                        CAddress.nGoal_Vel_Size_4 = 2;

                        CAddress.nProfile_Vel_112_4 = 32;      // RW
                        CAddress.nProfile_Vel_Size_4 = 2;

                        CAddress.nGoal_Pos_116_4 = 30;         // RW
                        CAddress.nGoal_Pos_Size_4 = 2;
#endif

                        break;
                    #endregion AX
                    #region XL_430 & MX Protocol 2
                    case EMotor_t.MX_28:
                    case EMotor_t.MX_64:
                    case EMotor_t.MX_106:
                    case EMotor_t.XM430_W210:
                    case EMotor_t.XM430_W350:

                    case EMotor_t.XM540_W270:
                    case EMotor_t.XM540_W150:

                    case EMotor_t.XH430_W210:
                    case EMotor_t.XH430_W350:
                    case EMotor_t.XH430_V210:
                    case EMotor_t.XH430_V350:

                    case EMotor_t.XL_430:
                    case EMotor_t.XL_2XL:
                        CAddress.nLed = 65; // 430: 65(1)
                        CAddress.nLed_Size = 1;
                
                        CAddress.nTorq = 64; // 430: 64(1)
                        CAddress.nTorq_Size = 1;
                
                        CAddress.nDriveMode = 10; // 430: 10(1)
                        CAddress.nDriveMode_Size = 1;
                
                        CAddress.nOperatingMode = 11; // 430: 11(1)
                        CAddress.nOperatingMode_Size = 1;
                
                        CAddress.nWheel_Speed = 104; // 430: 104(4)
                        CAddress.nWheel_Speed_Size = 4;
                
                        CAddress.nAcceleration = 108; //430: 108(4)
                        CAddress.nAcceleration_Size = 4;

                        CAddress.nPos_Speed = 112; // 430: 112(4)
                        CAddress.nPos_Speed_Size = 4;

                        CAddress.nPwm = 112; // 430: 112(4)
                        CAddress.nPwm_Size = 4;

                        CAddress.nPos = 116; // 430: 116(4)
                        CAddress.nPos_Size = 4;
                        
                        CAddress.nAddressStart = 100;
                        CAddress.nAddressLength = _ADDRESS_LENGTH;

                        CAddress.nGetTemp = 146; // 320: 46(1), ax: 43(1)
                        CAddress.nGetTemp_Size = 1;
                        CAddress.nGetVolt = 144; // 320: 45(1), ax: 42(1)
                        CAddress.nGetVolt_Size = 2;
                        CAddress.nGetLoad = 126; // 320 : 41(2), ax: 40(2)
                        CAddress.nGetLoad_Size = 2;
                        CAddress.nGetSpeed = 128; // 320 : 39(2), ax: 38(2)
                        CAddress.nGetSpeed_Size = 4;
                        CAddress.nGetPos = 132; // 320: 37(2), ax: 36(2)
                        CAddress.nGetPos_Size = 4;

                        CAddress.nGetSimple = 124; // 320: 37(11), ax: 36(8)
                        CAddress.nGetSimple_Size = 23;
                        break;
                    #endregion XL_430
                    #region XL_320
                    case EMotor_t.XL_320:
                        CAddress.IsMinusBit = true;
                        CAddress.nLed = 25; // 430: 65(1)
                        CAddress.nLed_Size = 1;
                
                        CAddress.nTorq = 24; // 430: 64(1)
                        CAddress.nTorq_Size = 1;
                
                        CAddress.nDriveMode = 10; // 430: 10(1)
                        CAddress.nDriveMode_Size = 0;
                
                        CAddress.nOperatingMode = 11; // 430: 11(1)
                        CAddress.nOperatingMode_Size = 1; // [11]2 로 셋팅해야 함
                
                        CAddress.nWheel_Speed = 32; // 430: 104(4)
                        CAddress.nWheel_Speed_Size = 2;
                
                        CAddress.nAcceleration = 108; //430: 108(4)
                        CAddress.nAcceleration_Size = 0;

                        CAddress.nPos_Speed = 32; // 430: 112(4)
                        CAddress.nPos_Speed_Size = 2;

                        CAddress.nPos = 30; // 430: 116(4)
                        CAddress.nPos_Size = 2;
                        
                        CAddress.nAddressStart = 30;
                        CAddress.nAddressLength = 4;

                        CAddress.nGetTemp = 46; // 320: 46(1), ax: 43(1)
                        CAddress.nGetTemp_Size = 1;
                        CAddress.nGetVolt = 45; // 320: 45(1), ax: 42(1)
                        CAddress.nGetVolt_Size = 1;
                        CAddress.nGetLoad = 41; // 320 : 41(2), ax: 40(2)
                        CAddress.nGetLoad_Size = 2;
                        CAddress.nGetSpeed = 39; // 320 : 39(2), ax: 38(2)
                        CAddress.nGetSpeed_Size = 2;
                        CAddress.nGetPos = 37; // 320: 37(2), ax: 36(2)
                        CAddress.nGetPos_Size = 2;

                        CAddress.nGetSimple = 37; // 320: 37(11), ax: 36(8)
                        CAddress.nGetSimple_Size = 11;
                        //   CAddress.nTorq_64_1 = 24;              // RW   Off(0), On(1)                        
                        //   CAddress.nLed_65_1 = 25;               // RW   Off(0), On(1)

                        //   CAddress.nMode_Operating_11_1 = 6;    // RW   1=속도제어(무한회전), 3(초기값)=위치제어(RPM), 4=Multi-Turn(-256~256:512회전), 5=전류기반 위치제어모드(-256~256:512회전), 16=PWM제어
                        //   CAddress.nMode_Operating_Size_1 = 5; // 속도제어시 0, 위치제어시 [6]00, [7]00, [8]ff, [9]03 ( 65283 ) 으로 셋팅해야 함. -> CW Limit 0, CCW Limit 1023
                        //   CAddress.nProtocolVersion_13_1 = 13;   // RW  1 바퀴모드, 2 관절모드 (이 부분이 프로토콜 1의 다른 모터와 xl320 이 다른 부분)(아예 위 4바이트에 합쳐서 5바이트 만들어 제어)
                        //   CAddress.nProtocolVersion_Size_1 = 1;


                        //   CAddress.nGoal_Vel_104_4 = 32;         // RW
                        //   CAddress.nGoal_Vel_Size_4 = 2;

                        //   CAddress.nProfile_Vel_112_4 = 32;      // RW
                        //   CAddress.nProfile_Vel_Size_4 = 2;

                        //   CAddress.nGoal_Pos_116_4 = 30;         // RW
                        //   CAddress.nGoal_Pos_Size_4 = 2;
                     //   /*
                     //     SetParam_Addr_Max(nAxis, 52);
                     //     SetParam_Addr_Torq(nAxis, 24);
                     //     SetParam_Addr_Led(nAxis, 25);
                     //     SetParam_Addr_Mode(nAxis, 11); // 320 -> 11            [1 : 속도, 2(default) : 관절]
                     //   SetParam_Addr_Speed(nAxis, 32); // 320 -> 32 2 bytes
                     //   SetParam_Addr_Speed_Size(nAxis, 2);
                     //     SetParam_Addr_Pos_Speed(nAxis, 32); // 320 -> 32 2 bytes
                     //     SetParam_Addr_Pos_Speed_Size(nAxis, 2);
                     //     SetParam_Addr_Pos(nAxis, 30); // 320 -> 30 2 bytes
                     //     SetParam_Addr_Pos_Size(nAxis, 2);
                     //*/
                        //   CAddress.nMotorNumber_0_2 = 0;        // R    0 : none
                        //   CAddress.nMotorNumber_Size_2 = 2;
                        //   CAddress.nFwVersion_6_1 = 6;          // R      
                        //   CAddress.nFwVersion_Size_1 = 1;

                        //   CAddress.nRealID_7_1 = 7;             // RW   0 ~ 252
                        //   CAddress.nRealID_Size_1 = 1;
                        //   CAddress.nBaudrate_8_1 = 8;           // RW   0-9600, 1-57600, 2-115200, 3-1M, 4-2M,  5-3M, 6-4M, 7-4.5M
                        //   CAddress.nBaudrate_Size_1 = 1;
                     //   // [DriveMode] 
                     //   //    0x01 : 정상회전(0), 역회전(1)
                     //   //    0x02 : 540 전용 Master(0), Slave(1)
                     //   //    0x04 : 속도기준 제어(0), 시간기준 제어(1)
                        //   CAddress.nMode_Drive_10_1 = 10;        // RW 
                        //   CAddress.nMode_Drive_Size_1 = 1;
                        //   //CAddress.nMode_Operating_11_1 = 11;    // RW   1=속도제어(무한회전), 3(초기값)=위치제어(RPM), 4=Multi-Turn(-256~256:512회전), 5=전류기반 위치제어모드(-256~256:512회전), 16=PWM제어
                        //   //CAddress.nMode_Operating_Size_1 = 1;
                        //   //CAddress.nProtocolVersion_13_1 = 13;   // RW   
                        //   //CAddress.nProtocolVersion_Size_1 = 1;
                        //   CAddress.nOffset_20_4 = 20;            // RW   1회전 모드인 경우 -1024~1024 까지만 제어
                        //   CAddress.nOffset_Size_4 = 4;
                        //   CAddress.nLimit_PWM_36_2 = 36;         // RW   0~885 (885 = 100%)
                        //   CAddress.nLimit_PWM_Size_2 = 2;
                        //   CAddress.nLimit_Curr_38_2 = 38;        // RW   전류제어모드/전류기반위치제어모드에서 사용, 0~2047
                        //   CAddress.nLimit_Curr_Size_2 = 2;
                     //   // [Shutdown] - Reboot 으로만 해제 가능
                     //   //    0x20 : 과부하
                     //   //    0x10 : 전력이상
                     //   //    0x08 : 엔코더 이상(Following Error)
                     //   //    0x04 : 과열
                     //   //    0x01 : 인가된 전압 이상
                        //   CAddress.nShutdown_63_1 = 63;          // RW
                        //   CAddress.nShutdown_Size_1 = 1;
                        //   //CAddress.nTorq_64_1 = 64;              // RW   Off(0), On(1)
                        //   CAddress.nTorq_Size_1 = 1;
                        //   //CAddress.nLed_65_1 = 65;               // RW   Off(0), On(1)
                        //   CAddress.nLed_Size_1 = 1;
                        //   CAddress.nError_70_1 = 70;             // R    
                        //   CAddress.nError_Size_1 = 1;
                        //   CAddress.nGain_Vel_I_76_2 = 76;        // RW
                        //   CAddress.nGain_Vel_I_Size_2 = 2;
                        //   CAddress.nGain_Vel_P_78_2 = 78;        // RW
                        //   CAddress.nGain_Vel_P_Size_2 = 2;
                        //   CAddress.nGain_Pos_D_80_2 = 80;        // RW
                        //   CAddress.nGain_Pos_D_Size_2 = 2;
                        //   CAddress.nGain_Pos_I_82_2 = 82;        // RW
                        //   CAddress.nGain_Pos_I_Size_2 = 2;
                        //   CAddress.nGain_Pos_P_84_2 = 84;        // RW
                        //   CAddress.nGain_Pos_P_Size_2 = 2;
                        //   CAddress.nGain_Pos_F2_88_2 = 88;       // RW
                        //   CAddress.nGain_Pos_F2_Size_2 = 2;
                        //   CAddress.nGain_Pos_F1_90_2 = 90;       // RW
                        //   CAddress.nGain_Pos_F1_Size_2 = 2;

                        //   CAddress.nWatchDog_98_1 = 98;          // RW
                        //   CAddress.nWatchDog_Size_1 = 1;

                        //   CAddress.nGoal_PWM_100_2 = 100;         // RW   -PWMLimit ~ +PWMLimit
                        //   CAddress.nGoal_PWM_Size_2 = 2;
                        //   CAddress.nGoal_Current_102_2 = 102;     // RW   -CurrentLimit ~ +CurrentLimit
                        //   CAddress.nGoal_Current_Size_2 = 2;
                        //   //CAddress.nGoal_Vel_104_4 = 104;         // RW
                        //   //CAddress.nGoal_Vel_Size_4 = 4;

                        //   CAddress.nProfile_Acc_108_4 = 108;      // RW
                        //   CAddress.nProfile_Acc_Size_4 = 4;
                        //   //CAddress.nProfile_Vel_112_4 = 112;      // RW
                        //   //CAddress.nProfile_Vel_Size_4 = 4;

                        //   //CAddress.nGoal_Pos_116_4 = 116;         // RW
                        //   //CAddress.nGoal_Pos_Size_4 = 4;

                        //   CAddress.nMoving_122_1 = 122;           // R    움직임 감지 못함(0), 움직임 감지(1)
                        //   CAddress.nMoving_Size_1 = 1;
                     //   // [Moving Status]
                     //   //    0x30  - 0x30 : 사다리꼴 속도 프로파일
                     //   //            0x20 : 삼각 속도 프로파일
                     //   //            0x10 : 사각 속도 프로파일
                     //   //            0x00 : 프로파일 미사용(Step)
                     //   //    0x08 : Following Error
                     //   //    0x02 : Goal Position 명령에 따라 진행 중
                     //   //    0x01 : Inposition
                        //   CAddress.nMoving_Status_123_1 = 123;    // R
                        //   CAddress.nMoving_Status_Size_1 = 1;

                        //   CAddress.nPresent_PWM_124_2 = 124;      // R
                        //   CAddress.nPresent_PWM_Size_2 = 2;
                        //   CAddress.nPresent_Curr_126_2 = 126;     // R
                        //   CAddress.nPresent_Curr_Size_2 = 2;
                        //   CAddress.nPresent_Vel_128_4 = 128;      // R
                        //   CAddress.nPresent_Vel_Size_4 = 4;
                        //   CAddress.nPresent_Pos_132_4 = 132;      // R
                        //   CAddress.nPresent_Pos_Size_4 = 4;
                        //   CAddress.nPresent_Volt_144_2 = 144;     // R
                        //   CAddress.nPresent_Volt_Size_2 = 2;
                        //   CAddress.nPresent_Temp_146_1 = 146;     // R
                        //   CAddress.nPresent_Temp_Size_1 = 1;
                        break;
                    #endregion XL_320
                }
            }
            #region Parameter Function(SetParam...)
            public void SetParam_Dir(int nMotor, int nDir) { m_aCParam[nMotor].nDir = nDir; }
            //public void SetParam_TimeControl(int nMotor, bool bSetTimeControl) { m_aCParam[nMotor].nDriveMode |= ((bSetTimeControl == true) ? 0x04 : 0x00); }
            public void SetParam_RealID(int nMotor, int nMotorRealID) { m_aCParam[nMotor].nRealID = nMotorRealID; }
            //public void SetParam_OperationMode(int nMotor, EOperationMode_t EOperationMode) { m_aCParam[nMotor].EOperationMode = EOperationMode; }

            public void SetParam_CommIndex(int nMotor, int nCommIndex) { m_aCParam[nMotor].nCommIndex = nCommIndex; }               // 연결 이후에 둘 중 하나만 사용 한다.(되도록  CommIndex 를 사용할 것. Commport 로 설정 하려면 통신이 접속이 되어 있어야 한다.)
            //public void SetParam_CommPort(int nMotor, int nCommPort) { m_aCParam[nMotor].nCommIndex = GetSerialIndex(nCommPort); }  // CommIndex 설정보다 직관적이나 잘못 설정 될 수 있다. 연결이 안 된 경우 CommIndes 가 잘못 지정될 수가 있다.

            public void SetParam_LimitUp(int nMotor, float fLimitUp) { m_aCParam[nMotor].fLimitUp = fLimitUp; }                       // Limit - 0: Ignore 
            public void SetParam_LimitDown(int nMotor, float fLimitDn) { m_aCParam[nMotor].fLimitDn = fLimitDn; }                       // Limit - 0: Ignore 
            public void SetParam_LimitRpm(int nMotor, float fLimitRpm) { m_aCParam[nMotor].fLimitRpm = fLimitRpm; }                       // Limit - 0: Ignore 
            public void SetParam(int nMotor, EMotor_t EMot) { int nConnect = m_lstConnect.Count - 1; if (nConnect < 0) nConnect = 0; SetParam(((m_lstConnect.Count - 1) < 0 ? 0 : (m_lstConnect.Count - 1)), nMotor, EMot); }
            public void SetParam(int nCommIndex, int nMotor, EMotor_t EMot) { SetParam_MotorType(nCommIndex, nMotor, EMot); }
            //public void SetParam(int nMotor, EMotor_t EMot) { SetParam_MotorType(m_aCParam[nMotor].nCommIndex, nMotor, EMot); }
            public void SetParam_MotorType(int nCommIndex, int nMotor, EMotor_t EMot)
            {
                SetAddress(EMot, ref m_aCParam[nMotor].CAddress);
                if (m_lstConnect.Count > m_aCParam[nMotor].nCommIndex)
                {
                    if (m_aCParam[nMotor].EMot == EMotor_t.XL_320) m_lstConnect[m_aCParam[nMotor].nCommIndex].bXL320 = true;
                    else if (m_aCParam[nMotor].CAddress.nProtocol == 1) m_lstConnect[m_aCParam[nMotor].nCommIndex].bProtocol1 = true;
                    else m_lstConnect[m_aCParam[nMotor].nCommIndex].bProtocol2 = true;
                }
                m_aCParam[nMotor].EMot = EMot;
                m_aCParam[nMotor].bEn = true;
                //m_aCParam[nMotor].EMot = EMotor_t.NONE;
                //public int nID;                     // 모터를 제어하기 위한 가상 아이디. 중복 허용 불가. 보통은 모터 ID 와 일치시킨다.            
                m_aCParam[nMotor].nCommIndex = nCommIndex;// 0;              // 연결 된 통신포트 중 어느것에 연결 된 것이지에 대한 순서.
                m_aCParam[nMotor].nRealID = nMotor;

                /////////////////////////////////////////////////////////////////
                m_aCParam[nMotor].nSeq = 0;
                m_aCParam[nMotor].nSeq_Back = 0;

                m_aCParam[nMotor].nDriveMode = 0;   // 0x00 - Rpm 모드, 0x40 - Time 제어 모드
                m_aCParam[nMotor].EOperation = EOperation_t._Position; // Operating Mode (0-전류제어, 1-속도제어, 3-위치제어(default), 4-확장위치제어, 5-전류기반 위치제어(그리퍼), 16-PWM 제어
                /////////////////////////////////////////////////////////////////

                m_aCParam[nMotor].nDir = 0; // forward = 0, inverse = 1;
                //Center
                m_aCParam[nMotor].fCenterPos = 2048.0f;

                m_aCParam[nMotor].fMechMove = 4096.0f;
                m_aCParam[nMotor].fDegree = 360.0f;
                m_aCParam[nMotor].fRefRpm = 0.229f;

                m_aCParam[nMotor].fLimitRpm = 415.0f;     // 0 ~ 1023

                m_aCParam[nMotor].bLimitUp = false;
                m_aCParam[nMotor].fLimitUp = 360.0f;    // Limit - bLimitUp == false: Ignore
                m_aCParam[nMotor].bLimitDown = false;
                m_aCParam[nMotor].fLimitDn = 0.0f;    // Limit - bLimitUp == false: Ignore

                m_aCParam[nMotor].nProtocol = 2;               // 2, 0 - protocol2, 1 - protocol1, 3 - None   



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
                        //m_aCParam[nMotor].bEn = true;                       // 활성화

                        //m_aCParam[nMotor].nID = nMotor;                     // 모터를 제어하기 위한 가상 아이디. 중복 허용 불가. 보통은 모터 ID 와 일치시킨다.      
                        //m_aCParam[nMotor].nRealID = nMotorRealID;           // 실제 사용할 모터의 아이디
                        //m_aCParam[nMotor].nCommIndex = nCommIndex;          // 연결 된 통신포트 중 어느것에 연결 된 것이지에 대한 순서.

                        //m_aCParam[nMotor].nDir = nDir;
                        //m_aCParam[nMotor].EMot = EMot;
                        m_aCParam[nMotor].fCenterPos = 512.0f;

                        m_aCParam[nMotor].fMechMove = 1024.0f;
                        m_aCParam[nMotor].fDegree = 300.0f;
                        m_aCParam[nMotor].fRefRpm = 0.111f;                 // 기본 rpm 단위

                        m_aCParam[nMotor].fLimitRpm = 1023f;                // 

                        m_aCParam[nMotor].fLimitUp = 0.0f;    // Limit - 0: Ignore
                        m_aCParam[nMotor].fLimitDn = 0.0f;    // Limit - 0: Ignore

                        m_aCParam[nMotor].nProtocol = 1;               // 2, 0 - protocol2, 1 - protocol1, 3 - None     

                        break;
                    case EMotor_t.EX_106:
                        //m_aCParam[nMotor].EMot = EMot;
                        m_aCParam[nMotor].fCenterPos = 2048.0f;

                        m_aCParam[nMotor].fMechMove = 4096.0f;
                        m_aCParam[nMotor].fDegree = 251.0f;
                        m_aCParam[nMotor].fRefRpm = 0.111f;                 // 기본 rpm 단위

                        m_aCParam[nMotor].fLimitRpm = 1023f;                // 

                        m_aCParam[nMotor].fLimitUp = 0.0f;    // Limit - 0: Ignore
                        m_aCParam[nMotor].fLimitDn = 0.0f;    // Limit - 0: Ignore

                        m_aCParam[nMotor].nProtocol = 1;               // 2, 0 - protocol2, 1 - protocol1, 3 - None     
                        break;
                    #region MX
                    case EMotor_t.MX_12:
                        m_aCParam[nMotor].fCenterPos = 1024.0f;

                        m_aCParam[nMotor].fMechMove = 2048.0f;
                        m_aCParam[nMotor].fDegree = 360.0f;
                        m_aCParam[nMotor].fRefRpm = 0.916f;            // 기본 rpm 단위

                        m_aCParam[nMotor].fLimitRpm = 415f;                // 

                        m_aCParam[nMotor].fLimitUp = 0.0f;    // Limit - 0: Ignore
                        m_aCParam[nMotor].fLimitDn = 0.0f;    // Limit - 0: Ignore

                        m_aCParam[nMotor].nProtocol = 1;               // 2, 0 - protocol2, 1 - protocol1, 3 - None              
                        break;
                    case EMotor_t.MX_28:
                    case EMotor_t.MX_64:
                    case EMotor_t.MX_106:
                        m_aCParam[nMotor].fCenterPos = 2048.0f;

                        m_aCParam[nMotor].fMechMove = 4096.0f;
                        m_aCParam[nMotor].fDegree = 360.0f;
                        m_aCParam[nMotor].fRefRpm = 0.229f; ;//(EMot == EMotor_t.MX_12) ? 0.916f : 0.114f;  //0.229f;                 // 기본 rpm 단위

                        m_aCParam[nMotor].fLimitRpm = 415f;                // 

                        m_aCParam[nMotor].fLimitUp = 0.0f;    // Limit - 0: Ignore
                        m_aCParam[nMotor].fLimitDn = 0.0f;    // Limit - 0: Ignore

                        m_aCParam[nMotor].nProtocol = 2;               // 2, 0 - protocol2, 1 - protocol1, 3 - None              
                        break;
                    #endregion MX
                    // Protocol2
                    case EMotor_t.NONE:

                    case EMotor_t.XM430_W210:
                    case EMotor_t.XM430_W350:

                    case EMotor_t.XM540_W270:
                    case EMotor_t.XM540_W150:

                    case EMotor_t.XH430_W210:
                    case EMotor_t.XH430_W350:
                    case EMotor_t.XH430_V210:
                    case EMotor_t.XH430_V350:
                        
                    case EMotor_t.XL_430:
                    case EMotor_t.XL_2XL:
                        //m_aCParam[nMotor].bEn = true;                       // 활성화
                        //m_aCParam[nMotor].nModelNum = 1060;
                        //m_aCParam[nMotor].nID = nMotor;                     // 모터를 제어하기 위한 가상 아이디. 중복 허용 불가. 보통은 모터 ID 와 일치시킨다.      
                        //m_aCParam[nMotor].nRealID = nMotorRealID;           // 실제 사용할 모터의 아이디
                        //m_aCParam[nMotor].nCommIndex = nCommIndex;          // 연결 된 통신포트 중 어느것에 연결 된 것이지에 대한 순서.

                        //m_aCParam[nMotor].nDir = nDir;
                        m_aCParam[nMotor].fCenterPos = 2048.0f;

                        m_aCParam[nMotor].fMechMove = 4096.0f;
                        m_aCParam[nMotor].fDegree = 360.0f;
                        m_aCParam[nMotor].fRefRpm = 0.229f;                 // 기본 rpm 단위

                        m_aCParam[nMotor].fLimitRpm = 415f;                // 

                        m_aCParam[nMotor].fLimitUp = 0.0f;    // Limit - 0: Ignore
                        m_aCParam[nMotor].fLimitDn = 0.0f;    // Limit - 0: Ignore

                        m_aCParam[nMotor].nProtocol = 2;               // 2, 0 - protocol2, 1 - protocol1, 3 - None                             
                        break;


                    case EMotor_t.XL_320:
                        //m_aCParam[nMotor].EMot = EMot;
                        m_aCParam[nMotor].fCenterPos = 512.0f;

                        m_aCParam[nMotor].fMechMove = 1024.0f;
                        m_aCParam[nMotor].fDegree = 300.0f;
                        m_aCParam[nMotor].fRefRpm = 0.111f;                 // 기본 rpm 단위

                        m_aCParam[nMotor].fLimitRpm = 1023f;                // 

                        m_aCParam[nMotor].fLimitUp = 0.0f;    // Limit - 0: Ignore
                        m_aCParam[nMotor].fLimitDn = 0.0f;    // Limit - 0: Ignore

                        m_aCParam[nMotor].nProtocol = 2;               // 2, 0 - protocol2, 1 - protocol1, 3 - None     
                        break;
                    //case EMotor_t.XM_540:
                    //    break;
                }
                //if (nCommIndex < 0) nCommIndex = 0;
                //if (m_aCParam[nMotor].CAddress.nProtocol == 1) m_lstConnect[nCommIndex].bProtocol1 = true;
                //else if (m_aCParam[nMotor].CAddress.nProtocol == 2) m_lstConnect[nCommIndex].bProtocol2 = true;
                //if (EMot == EMotor_t.XL_320)
                //{
                //    m_lstConnect[nCommIndex].bXL320 = true;
                //}
            }
            public void SetParam(int nMotor, int nMotorRealID, int nCommIndex, int nDir, EMotor_t EMot)
            {
                //m_anMot_RealID[nMotor] = nMotorRealID; // ID 변경
                //m_anMot_SerialIndex[nMotor] = nCommIndex; // 통신 포트 변경
                m_aCParam[nMotor].nRealID = nMotorRealID; // ID 변경
                m_aCParam[nMotor].nCommIndex = nCommIndex; // 통신 포트 변경
                m_aCParam[nMotor].nDir = nDir;
                //m_aCParam[nMotor].EOperationMode = EOperationMode_t._Position; // Default
                //if (m_aCParam[nMotor].EOperationMode_Prev == EOperationMode_t._None) m_aCParam[nMotor].EOperationMode_Prev = m_aCParam[nMotor].EOperationMode;
                SetAddress(EMot, ref m_aCParam[nMotor].CAddress);

                SetParam_MotorType(nCommIndex, nMotor, EMot);
                if (m_aCParam[nMotor].CAddress.nProtocol == 1) m_lstConnect[nCommIndex].bProtocol1 = true;
                else if (m_aCParam[nMotor].CAddress.nProtocol == 2) m_lstConnect[nCommIndex].bProtocol2 = true;
                if (EMot == EMotor_t.XL_320)
                {
                    m_lstConnect[nCommIndex].bXL320 = true;
                }
            }

            #endregion Parameter Function(SetParam...)
            public struct SStatus_t
            {
                public int nError;
                public int nTorq;
                public int nLed;
            }
            // 0-전류제어, 1-속도제어(바퀴제어), 3-위치제어(default), 4-확장위치제어(X시리즈의 경우 -256 ~ +256회전 지원), 5-전류기반 위치제어(그리퍼)(X시리즈의 경우 -256 ~ +256회전 지원), 16-PWM 제어
            public enum EOperation_t
            {
                _None = -1,
                _Amp = 0,
                _Speed = 1, // 속도제어모드
                _Speed_Raw = 100,
                _Position_Raw = 300, // 내가 임의로 만든 것.
                _Position = 3,
                _Position_Multi_Raw = 400, // 내가 임의로 만든 것.
                _Position_Multi = 4,
                //_Position_Time = 5,
                //_Position_Multi_Time = 6,
                //_Position_Amp = 5,
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

            public class CPacket_t // 받은 패킷의 분석용
            {
                public int nProtocol; 

                public int nAddress;
                public int nAddress_DataLength;

                public int nID;
                public int nLength;
                public int nCmd;
                public int nError;
                public int nModelNumber;
                public int nFw;
                public int nCrc;
                public int nCrc0;
                public int nCrc1;
                public int nStep;
                public int nSeq;
                public byte[] abyMap = new byte[200];
            }

            

            #endregion Structure
            
            public class CConnection_t
            {
                public CConnection_t()
                {
                }
                ~CConnection_t()
                {
                }
                public bool bXL320 = false;
                public bool bProtocol1 = false;
                public bool bProtocol2 = false;
                public int nType = 0; // 0 - None, 1 - serial, 2 - tcp/ip
                public Ojw.CSerial CSerial = new CSerial();
                public Ojw.CSocket CSock = new CSocket();
                public int nRequestData = 0;
            }

            public class CMot_t
            {
                public CMot_t()
                {
                }
                ~CMot_t()
                {
                }
                public bool bTorq = false;
                public int nOperation = 3;

            }

            #region Var
            private List<int> m_lstScanedMotors_ID = new List<int>();
            private List<int> m_lstScanedMotors_Model = new List<int>();
            private List<int> m_lstScanedMotors_Protocol = new List<int>();
            private List<int> m_lstScanedMotors_CommIndex = new List<int>();
            //private List<CPacket_t> m_lstPacket = new List<CPacket_t>();
            private List<CConnection_t> m_lstConnect = new List<CConnection_t>();
            private List<int> m_lstIndex = new List<int>();
            private List<int> m_lstID = new List<int>();

            private const int _MAX_MOTOR = 254;
            private const int _ID_BROADCASTING = 254;
            private CParam_t[] m_aCParam = new CParam_t[_MAX_MOTOR + 1];
#if _THREAD_
            private Thread m_thReceive = null;
#endif
            private bool m_bProgEnd = false;
            private bool m_bStop = false;
            private bool m_bEms = false;
            private bool m_bMotionEnd = false;
            private bool m_bStart = false;

            #endregion Var
            
            // 초기화
            private void Init()
            {
                m_lstScanedMotors_ID.Clear();
                m_lstScanedMotors_Model.Clear();
                m_lstScanedMotors_Protocol.Clear();
                m_lstScanedMotors_CommIndex.Clear();
                
                for (int i = 0; i < m_aCParam.Length; i++)
                {
                    m_aCParam[i] = new CParam_t();
                    SetParam(0, i, EMotor_t.XL_430);
                    //m_aCParam[i].nRealID = i;
                }
                for (int i = 0; i < m_aCMap.Length; i++)
                {
                    m_aCMap[i] = new CMap_t();
                    m_aCPush_Prev[i] = new SPush_t();
                    m_aCPush_Prev[i].nMotor = i;
                    m_aCPush_Prev[i].nConnection = 0;// -1;
                    m_aCPush_Prev[i].EOperation = EOperation_t._None;
                    
                    m_aCTmr[i] = new CTimer();
                    m_anWait[i] = 0;
                    m_afDiff[i] = 0;
                }
            }
            private void DInit()
            {
            }
            public CMonster()
            {
                Init();
            }
            ~CMonster()
            {
                Close();
                DInit();
            }      

            #region Open / Close, IsOpen, GetSerial, GetSerialIndex, GetSerialPort, GetSerialCount
            // 해당 컴포트가 열렸는지를 확인
            //public bool IsOpen(int nIndex)//(int nPort)
            //{
            //    if ((nIndex < 0) | (nIndex >= m_lstConnect.Count)) return false;
            //    //return (m_lstConnect[nIndex].nType == 0) ? false : true;
            //    if      (m_lstConnect[nIndex].nType == 1) return m_lstConnect[nIndex].CSerial.IsConnect();
            //    else if (m_lstConnect[nIndex].nType == 2) return m_lstConnect[nIndex].CSock.IsConnect();
            //    return false;
            //}
            public bool IsOpen()
            {
                return (GetCount_Connection() > 0) ? true : false;
            }
            public bool IsOpen(int nPort)
            {
                foreach (CConnection_t CConnect in m_lstConnect) { if (CConnect.CSerial.GetPortNumber() == nPort) { return true; } }
                return false;
            }
            public int FindSerialIndex_by_Port(int nPort)
            {
                for (int i = 0; i < m_lstConnect.Count; i++)
                {
                    if (m_lstConnect[i].CSerial.GetPortNumber() == nPort)
                    {
                        return i; 
                    }
                }
                return -1;
            }
            
#region GetData
            /*
             * 
                        CAddress.nGetTemp = 146; // 320: 46(1), ax: 43(1)
                        CAddress.nGetTemp_Size = 1;
                        CAddress.nGetVolt = 144; // 320: 45(1), ax: 42(1)
                        CAddress.nGetVolt_Size = 2;
                        CAddress.nGetLoad = 126; // 320 : 41(2), ax: 40(2)
                        CAddress.nGetLoad_Size = 2;
                        CAddress.nGetSpeed = 128; // 320 : 39(2), ax: 38(2)
                        CAddress.nGetSpeed_Size = 4;
                        CAddress.nGetPos = 132; // 320: 37(2), ax: 36(2)
                        CAddress.nGetPos_Size = 4;

                        CAddress.nGetSimple = 124; // 320: 37(11), ax: 36(8)
                        CAddress.nGetSimple_Size = 23;
             * */
            public float GetData_Angle(int nMotor)
            {
                if (m_aCParam[nMotor].CAddress.nGetPos_Size == 4) return CalcEvd2Angle(nMotor, m_aCMap[nMotor].GetInt(m_aCParam[nMotor].CAddress.nGetPos));
                return CalcEvd2Angle(nMotor, m_aCMap[nMotor].GetShort(m_aCParam[nMotor].CAddress.nGetPos));
            }
            public int GetData_Speed(int nMotor)
            {
                if (m_aCParam[nMotor].CAddress.nGetSpeed_Size == 4) return m_aCMap[nMotor].GetInt(m_aCParam[nMotor].CAddress.nGetSpeed);
                return (int)m_aCMap[nMotor].GetShort(m_aCParam[nMotor].CAddress.nGetSpeed);
            }
            public int GetData_Load(int nMotor) { return (int)m_aCMap[nMotor].GetShort(m_aCParam[nMotor].CAddress.nGetLoad); }
            public int GetData_Volt(int nMotor) { return (int)m_aCMap[nMotor].GetByte(m_aCParam[nMotor].CAddress.nGetVolt); }
            public int GetData_Temp(int nMotor) { return (int)m_aCMap[nMotor].GetByte(m_aCParam[nMotor].CAddress.nGetTemp); }
#endregion GetData
            
            public float Read_Pos(int nMotor)
            {
                Read_Motor(m_aCParam[nMotor].nProtocol, nMotor, m_aCParam[nMotor].CAddress.nGetPos, m_aCParam[nMotor].CAddress.nGetPos_Size);
                int nValue = ((m_aCParam[nMotor].CAddress.nPos_Size == 4) ? m_aCMap[nMotor].GetInt(m_aCParam[nMotor].CAddress.nGetPos) : m_aCMap[nMotor].GetShort(m_aCParam[nMotor].CAddress.nGetPos));
                m_aCPush_Prev[nMotor].fValue = CalcEvd2Angle(nMotor, nValue);
                return m_aCPush_Prev[nMotor].fValue;// CalcEvd2Angle(nMotor, nValue);
            }
            public void Read_Motor(int nMotor, int nAddress, int nLength)
            {
                Read_Motor(m_aCParam[nMotor].nProtocol, nMotor, nAddress, nLength);
            }        
            public void Read_Motor(int nProtocol, int nMotor, int nAddress, int nLength)
            {
                //foreach (byte byID in pbyIDs) m_aCMap[byID].nRunningAddress = nAddress;
                m_aCMap[nMotor].nRunningAddress = nAddress;
                //WaitBusy(m_aCParam[nMotor].nCommIndex);
                //SetBusy(m_aCParam[nMotor].nCommIndex);
                if (nProtocol == 1)
                {
                    Write2(nProtocol, m_aCParam[nMotor].nCommIndex, m_aCParam[nMotor].nRealID, 0x02, nAddress, (byte)(nLength & 0xff));
                }
                else
                {
                    byte[] pbyLength = Ojw.CConvert.ShortToBytes((short)nLength);
                    Write2(nProtocol, m_aCParam[nMotor].nCommIndex, m_aCParam[nMotor].nRealID, 0x02, nAddress, pbyLength);
                    pbyLength = null;
                }
                Received(m_aCParam[nMotor].nCommIndex, 200, 1);
            }
            //// 해당 포트의 시리얼 핸들을 리턴
            //public CSerial GetSerial(int nPort)
            //{
            //    if (m_lstPort != null)
            //    {
            //        for (int i = 0; i < m_lstPort.Count; i++)
            //        {
            //            if (m_lstPort[i] == nPort) return m_lstSerial[i];
            //        }
            //    }
            //    return null;
            //}
            //// 해당 포트의 인덱스 번호를 리턴
            //public int GetSerialIndex(int nPort)
            //{
            //    if (m_lstPort != null)
            //    {
            //        for (int i = 0; i < m_lstPort.Count; i++)
            //        {
            //            if (m_lstPort[i] == nPort) return i;
            //        }
            //    }
            //    return -1;
            //}
            //// 해당 인덱스의 포트 번호를 리턴
            //public int GetSerialPort(int nCommIndex)
            //{
            //    if (m_lstPort != null)
            //    {
            //        if (nCommIndex < m_lstPort.Count)
            //        {
            //            return m_lstPort[nCommIndex];
            //        }
            //    }
            //    return -1;
            //}
            // 전체 포트의 인덱스 수를 리턴
            public int GetCount_Connection() { return m_lstConnect.Count; }

            //// * 중요: 오픈시에 Operation Mode, 모터 종류, 아이디, 위치값, 토크온 상태 를 가져와야 한다.
            // 컴포트를 연다.(중복 되지만 않으면 여러개를 여는 것이 가능)
            public bool Open(int nPort, int nBaudRate) { return Open_Serial(nPort, nBaudRate); }
            public bool Open(int nPort, string strIpAddress) { return Open_Socket(nPort, strIpAddress); }
            public bool Open_Socket(int nPort, string strIpAddress)
            {
                bool bConnected = false;
                // 시리얼 끼리 비교해서 중복 처리
                foreach (CConnection_t CConnect in m_lstConnect) { if (CConnect.nType == 2) { if (CConnect.CSock.m_nPort == nPort) { bConnected = CConnect.CSock.IsConnect(); break; } } }

                if (bConnected == false)
                {
                    CConnection_t CConnect = new CConnection_t();
                    if (CConnect.CSock.Connect(strIpAddress, nPort, false) == true)
                    {
                        CConnect.nType = 2;
                        m_lstConnect.Add(CConnect);

                        m_lstIndex.Add(0);
                        m_lstID.Add(-1);
                        //m_lstBuffers.Add(new byte[1] { 0 });
                        m_lstBuffers.Add(null);
                        //m_lstBuffers[m_lstBuffers.Count - 1] = null;
                        //m_lstPacket.Add(new CPacket_t());
                        
                        if (m_lstConnect.Count == 1)
                        {
                            for (int i = 0; i < m_aCParam.Length; i++)
                            {
                                SetParam(0, i, EMotor_t.XL_430);
                            }
                        }

#if _THREAD_
                        if (m_bThreadRun == false)
                        {
                            m_thReceive = new Thread(new ThreadStart(Thread_Receive));
                            m_thReceive.Start();
                        }
#endif
                        // 여기서 잠깐 Wait 후에 세팅 상태를 가져오도록 한다.

                        return true;
                    }
                    else return false;
                }
                return false;
            }
            private bool m_bThreadRun = false;
            public bool Open_Serial(int nPort, int nBaudRate)
            {
                bool bConnected = false;
                // 시리얼 끼리 비교해서 중복 처리
                foreach (CConnection_t CConnect in m_lstConnect) { if (CConnect.nType == 1) { if (CConnect.CSerial.GetPortNumber() == nPort) { bConnected = CConnect.CSerial.IsConnect(); break; } } }

                if (bConnected == false)
                {
                    CConnection_t CConnect = new CConnection_t();
                    if (CConnect.CSerial.Connect(nPort, nBaudRate) == true)
                    {
                        CConnect.nType = 1;
                        m_lstConnect.Add(CConnect);

                        m_lstIndex.Add(0);
                        m_lstID.Add(-1);
                        m_lstBuffers.Add(null);
                        //m_lstBuffers.Add(new byte[1] { 0 });
                        //m_lstPacket.Add(new CPacket_t());

                        //if (m_lstConnect.Count == 1)
                        //{
                        //    for (int i = 0; i < m_aCParam.Length; i++)
                        //    {
                        //        SetParam(0, i, EMotor_t.XL_430);
                        //    }
                        //}
#if _THREAD_
                        if (m_bThreadRun == false)
                        {
                            m_thReceive = new Thread(new ThreadStart(Thread_Receive));
                            m_thReceive.Start();
                        }
#endif
                        // 여기서 잠깐 Wait 후에 세팅 상태를 가져오도록 한다.


                        if (m_CHeader != null)
                        {
                            if (m_CHeader.anMotorIDs.Length > 0)
                            {
                                for (int i = 0; i < m_CHeader.anMotorIDs.Length; i++)
                                {
                                    int nID = m_CHeader.anMotorIDs[i];
                                    if (m_aCParam[nID].CAddress.nProtocol == 1) m_lstConnect[m_lstConnect.Count - 1].bProtocol1 = true;
                                    else if (m_aCParam[nID].CAddress.nProtocol == 2) m_lstConnect[m_lstConnect.Count - 1].bProtocol2 = true;
                                    if (m_aCParam[nID].EMot == EMotor_t.XL_320)
                                    {
                                        m_lstConnect[m_lstConnect.Count - 1].bXL320 = true;
                                    }
                                }
                            }
                        }

                        return true;
                    }
                    else return false;
                }
                return false;    
            }
            // 지정한 포트를 닫는다.
            public void Close(int nPort)
            {
                int i = FindSerialIndex_by_Port(nPort);
                if (i < 0) return;
                switch (m_lstConnect[i].nType)
                {
                    case 1:
                        {
                            if (m_lstConnect[i].CSerial.IsConnect()) m_lstConnect[i].CSerial.DisConnect();
                        }
                        break;
                    case 2:
                        {
                            if (m_lstConnect[i].CSock.IsConnect()) m_lstConnect[i].CSock.DisConnect();
                        }
                        break;
                }
                m_lstConnect[i].nType = 0;
                m_lstIndex[i] = 0;
                m_lstConnect.RemoveAt(i);
                m_lstIndex.RemoveAt(i);
                m_lstID.RemoveAt(i);
                m_lstBuffers.RemoveAt(i);
            }
            // 전체 포트를 닫는다.
            public void Close()
            {
                int nCount = m_lstConnect.Count;
                for (int i = 0; i < nCount; i++)
                {
                    switch (m_lstConnect[i].nType)
                    {
                        case 1:
                            {
                                if (m_lstConnect[i].CSerial.IsConnect()) m_lstConnect[i].CSerial.DisConnect();
                            }
                            break;
                        case 2:
                            {
                                if (m_lstConnect[i].CSock.IsConnect()) m_lstConnect[i].CSock.DisConnect();
                            }
                            break;
                    }
                    m_lstConnect[i].nType = 0;
                    ////
                    m_lstIndex[i] = 0;
                    m_lstBuffers[i] = null;
                }
                m_lstConnect.Clear();
                ////
                m_lstIndex.Clear();
                m_lstID.Clear();
                m_lstBuffers.Clear();
            }
            #endregion Open / Close, IsOpen, GetSerial, GetSerialIndex, GetSerialPort, GetSerialCount

            //public void Read_Motor(int nMotor, int nAddress, int nLength)
            //{
            //    byte[] pbyLength = Ojw.CConvert.ShortToBytes((short)nLength);
            //    Write2(m_aCParam[nMotor].nProtocol, m_aCParam[nMotor].nCommIndex, m_aCParam[nMotor].nRealID, 0x02, nAddress, pbyLength);
            //    pbyLength = null;
            //}
            private List<byte[]> m_lstBuffers = new List<byte[]>();
            //private byte[] m_pbyBuffer = null;//new byte[1024];
            public bool Received(int nCommIndex, int nWaitTime, int nCount_AllMotors)
            {
                //int nRet = Receiving(nCommIndex, 500, nCount_AllMotors, null);
                int nRet = Receiving(nCommIndex, nWaitTime, nCount_AllMotors, null);
                if (nRet > 0) // 데이타가 남아 있다면
                {
                    Ojw.CTimer CTmr = new CTimer();
                    CTmr.Set();
                    //while ((nRet = Receiving(nCommIndex, 500, nCount_AllMotors, m_pbyBuffer)) > 0)
                    while ((nRet = Receiving(nCommIndex, nWaitTime, nCount_AllMotors, m_lstBuffers[nCommIndex])) > 0)
                    {
                        if (CTmr.Get() > 5000) break;
                        //if (CTmr.Get() > nWaitTime) break;
                        Thread.Sleep(1);
                        //Ojw.CTimer.Wait(1);
                    }
                    Array.Clear(m_lstBuffers[nCommIndex], 0, m_lstBuffers[nCommIndex].Length);
                    if (nRet == 0) return true;
                    //Ojw.CMessage.Write("It still has datas..."); ;
                }
                else return true;
                //return nRet;
                return false;
            }
            private int Receiving(int nCommIndex, int nWaitTime, int nCount_AllMotors, byte [] pBuff)// int nProtocol, int nWaitTime)
            {
                //int nRet = 0;
                int i = 0;
                Ojw.CTimer CTmr = new CTimer();
                byte[] buf = null;
                //int nSeq = 0;
                int nSize = 0;
                bool bDone = false;
                int nCount_Motors = 0;
                CPacket_t[] aCPacket = new CPacket_t[m_lstConnect.Count];//CPacket_t[10]; // 10 개 이상은 일단 열지 말자...(추후 가변 작업)
                for (i = 0; i < aCPacket.Length; i++) aCPacket[i] = new CPacket_t();
                i = 0;
                CTmr.Set();
                bool bReceived = false;
                while ((m_lstConnect.Count > 0) && (m_bProgEnd == false) && (CTmr.Get() < nWaitTime))
                {
                    try
                    {                        
                        if (m_lstConnect[nCommIndex].nType == 0) break;//continue;
                        else if (m_lstConnect[nCommIndex].nType == 1)
                        {
                            if (m_lstConnect[nCommIndex].CSerial.IsConnect() == false) break;// continue;
                        }
                        else if (m_lstConnect[nCommIndex].nType == 2)
                        {
                            if (m_lstConnect[nCommIndex].CSock.IsConnect() == false) break;//continue;
                        }
                        // nType 이 1, 2 인경우만 감안
                        nSize = ((m_lstConnect[nCommIndex].nType == 1) ? m_lstConnect[nCommIndex].CSerial.GetBuffer_Length() : ((m_lstConnect[nCommIndex].nType == 2) ? m_lstConnect[nCommIndex].CSock.GetBuffer_Length() : 0));
                        int nBuffSize = (pBuff == null) ? 0 : pBuff.Length;
                        if ((nSize + nBuffSize) > 0)// || (m_lstConnect[nCommIndex].CSerial.GetBuffer_Length() > 0))
                        {
                            bReceived = true;
                            //m_lstConnect[nCommIndex].nRequestData = 0;
                            //m_lstlTest.Add(m_CTmrTest.Get()); m_CTmrTest.Set();

                            if (nBuffSize > 0)
                            {
                                buf = new byte[nSize + nBuffSize];
                                Array.Copy(pBuff, 0, buf, 0, nBuffSize);
                                Array.Copy(((m_lstConnect[nCommIndex].nType == 1) ? m_lstConnect[nCommIndex].CSerial.GetBytes() : m_lstConnect[nCommIndex].CSock.GetBytes()), 0, buf, nBuffSize, nSize);
                            }
                            else buf = ((m_lstConnect[nCommIndex].nType == 1) ? m_lstConnect[nCommIndex].CSerial.GetBytes() : m_lstConnect[nCommIndex].CSock.GetBytes());

#if false // test
                            foreach (byte byData in buf)
                            {
                                Ojw.CMessage.Write2("0x{0}", Ojw.CConvert.IntToHex(byData, 2));
                            }
                            Ojw.CMessage.Write2("\r\n");
#else
                            //byte [] pbyHeader = new byte[4];
                            i = 0;
                            foreach (byte byData in buf)
                            {
#if false // for some packet errors
                            if (byData == 0xff)
                            {
                                if ((m_lstIndex[nCommIndex] & 0xf0000) != 0)
                                {
                                    if ((byData == 0xff) && ((m_lstIndex[nCommIndex] & 0xf0000) == 0x10000))
                                        m_lstIndex[nCommIndex] = (m_lstIndex[nCommIndex] & 0xfffff) | 0x20000;
                                    else if ((byData == 0xff) && ((m_lstIndex[nCommIndex] & 0xf0000) == 0x30000)) // 0xff, 0xff
                                    {
                                        m_lstIndex[nCommIndex] = 0x1000; // start new ( protocol 1 기준 ) -> 0xff, 0xff 가 들어올 확률은 아주 높다. 해서 이 부분은 일단 블록
                                    }
                                }
                                else
                                {
                                    m_lstIndex[nCommIndex] = (m_lstIndex[nCommIndex] & 0xffff) | 0x10000;
                                }
                            }
#endif
                                if (((m_lstIndex[nCommIndex] & 0x1000) == 0) && (byData == 0xff)) m_lstIndex[nCommIndex] = ((m_lstIndex[nCommIndex] & 0xf0000) | 0x1000);
                                else if (((m_lstIndex[nCommIndex] & 0x3000) == 0x1000) && (byData == 0xff))
                                {
                                    //aCPacket[nCommIndex].bShow = false;

                                    // Protocol 1 Checked
                                    m_lstIndex[nCommIndex] |= 0x2000;
                                }
                                else if (((m_lstIndex[nCommIndex] & 0x7000) == 0x3000) && (byData == 0xfd))
                                {
                                    m_lstIndex[nCommIndex] |= 0x4000;
                                }
                                else if (((m_lstIndex[nCommIndex] & 0xf000) == 0x7000) && (byData == 0x00))
                                {
                                    // start - Protocol 2
                                    m_lstIndex[nCommIndex] |= 0x8000;
                                    // 모터는 Seq 가 아니라 새로운 인덱스가 필요하다. 전면 수정 필요. 읽을 때 아이디, 시리얼인덱스, 프로토콜버전의 3가지가 일치하는지 확인하고 꺼내오도록...
                                    //if (m_lstSMap[nCommIndex].nProtocol != 2) m_lstSMap[nCommIndex].nProtocol = 2;                                    
                                }
                                else
                                {
                                    //if () m_lstIndex[nCommIndex] = 0;

                                    


                                    // Protocol 2 - Address 122 ~ 136 => (120 ~ 139): 14=>20 바이트(moving(1), MoveStatus(1), PWM(4), Load(4), Velocity(4), Position(4)
                                    if ((m_lstIndex[nCommIndex] & 0xf000) == 0xf000)
                                    {
                                        // 단, XL-320 은 프로토콜2 여도 주소번지가 다르다. 따로 예외 처리할 것. 나중에...

                                        // [Ping] => Length == 7 이면 Ping
                                        // ID(1), Length(2)=(0x07, 0x00), cmd(1)=0x55, error(1), ModelNumber(2), Fw(1), CRC(2)
                                        // [Read]
                                        // ID(1), Length(2),  Cmd(1) = 0x55, error(1), Datas(N)..., CRC(2) 
                                        
                                        switch (m_lstIndex[nCommIndex] & 0x0fff)
                                        {
                                            case 0:
                                                if (byData > 253)
                                                {
                                                    m_lstIndex[nCommIndex] = 0;
                                                    break;
                                                }
                                                m_lstID[nCommIndex] = byData; // ID Setting
                                                if (aCPacket[nCommIndex].nProtocol != 2) aCPacket[nCommIndex].nProtocol = 2;
                                                //m_aSMap[m_lstID[nCommIndex]].nID = byData;
                                                aCPacket[nCommIndex].nID = m_lstID[nCommIndex];
                                                m_lstIndex[nCommIndex]++;
                                                break;
                                            case 1:
                                                aCPacket[nCommIndex].nLength = byData; // Length == 7 -> Ping
                                                m_lstIndex[nCommIndex]++;
                                                break;
                                            case 2:
                                                aCPacket[nCommIndex].nLength |= (((int)byData << 8) & 0xff00); // Length == 7 -> Ping
                                                m_lstIndex[nCommIndex]++;
                                                break;
                                            case 3:
                                                aCPacket[nCommIndex].nCmd = byData;
                                                m_lstIndex[nCommIndex]++;
                                                break;
                                            case 4:
                                                aCPacket[nCommIndex].nError = byData;
                                                aCPacket[nCommIndex].nStep = 0;
                                                if (aCPacket[nCommIndex].nLength == 7) // Ping
                                                {
                                                    aCPacket[nCommIndex].abyMap[0] = 0;
                                                    aCPacket[nCommIndex].abyMap[1] = 0;
                                                }
                                                m_lstIndex[nCommIndex]++;
                                                break;
                                            case 5:
                                                if (aCPacket[nCommIndex].nLength == 7) // Ping
                                                {
                                                    if (aCPacket[nCommIndex].nStep == 0)
                                                    {
                                                        //aCPacket[nCommIndex].bShow = true;
                                                        aCPacket[nCommIndex].nModelNumber = byData;
                                                        aCPacket[nCommIndex].abyMap[0] = byData;
                                                    }
                                                    else if (aCPacket[nCommIndex].nStep == 1)
                                                    {
                                                        aCPacket[nCommIndex].nModelNumber |= ((int)byData << 8) & 0xff00;
                                                        aCPacket[nCommIndex].abyMap[1] = byData;
                                                    }
                                                    else aCPacket[nCommIndex].nFw = byData;

                                                    //if (aCPacket[nCommIndex].nStep + 1 >= aCPacket[nCommIndex].nLength - 4)
                                                    //{
                                                    //    //Ojw.CMessage.Write("Model={0}", 
                                                    //    Ojw.CMessage.Write("[Serial[{0}]:[ID:{1}]Received[Protocol={2}]Type=0x{3}", nCommIndex, m_lstID[nCommIndex], aCPacket[nCommIndex].nProtocol, Ojw.CConvert.IntToHex(aCPacket[nCommIndex].abyMap[0] | ((aCPacket[nCommIndex].abyMap[1] << 8) & 0xff00), 4));
                                                    //    m_lstIndex[nCommIndex]++;
                                                    //}
                                                }
                                                else // 
                                                {
                                                    aCPacket[nCommIndex].abyMap[aCPacket[nCommIndex].nStep] = byData;

                                                    //if (aCPacket[nCommIndex].nStep + 1 >= aCPacket[nCommIndex].nLength - 4) m_lstIndex[nCommIndex]++;
                                                }
                                                if (aCPacket[nCommIndex].nStep + 1 >= aCPacket[nCommIndex].nLength - 4) m_lstIndex[nCommIndex]++;
                                                aCPacket[nCommIndex].nStep++;
                                                break;
                                            case 6:
                                                aCPacket[nCommIndex].nCrc0 = byData;
                                                m_lstIndex[nCommIndex]++;
                                                break;
                                            case 7:
                                                aCPacket[nCommIndex].nCrc1 = byData;
                                                m_lstIndex[nCommIndex] = 0;

                                                
                                                //{

                                                    if (m_bPing == true)
                                                    {
                                                        //EMotor_t EMot = GetMotorType(aCPacket[nCommIndex].abyMap[0] | ((aCPacket[nCommIndex].abyMap[1] << 8) & 0xff00));
                                                        Ojw.CMessage.Write2("Serial[{0}]:[ID:{1}]Received[Protocol={2}]Type=0x{3}\r\n",//**{4}\r\n", 
                                                            nCommIndex,
                                                            m_lstID[nCommIndex],
                                                            aCPacket[nCommIndex].nProtocol,
                                                            Ojw.CConvert.IntToHex(aCPacket[nCommIndex].abyMap[0] | ((aCPacket[nCommIndex].abyMap[1] << 8) & 0xff00), 4)//, 
                                                            //EMot
                                                        );
                                                        //m_bPing = false;
                                                    }
                                                    if (m_bAutoset == true)
                                                    {
    #if _DUPLICATE
                                                        bool bDup = false;
                                                        if (m_lstID[nCommIndex] >= 0)
                                                        {
                                                            foreach (int nDuplicated in m_lstScanedMotors_ID)
                                                            {
                                                                if (nDuplicated == m_lstID[nCommIndex])
                                                                {
                                                                    Ojw.CMessage.Write2("[warning]-----ID{0} duplication[protocol version 2 -----\r\n", nDuplicated);
                                                                    bDup = true;
                                                                    break;
                                                                }
                                                            }
                                                            if (bDup == false)
                                                            //int nFind = m_lstScanedMotors_ID.Find(x => x == m_lstID[nCommIndex]);
                                                            //if (nFind == 0)
        #endif
                                                            {
                                                                m_lstScanedMotors_ID.Add(m_lstID[nCommIndex]);
                                                                m_lstScanedMotors_Model.Add(aCPacket[nCommIndex].abyMap[0] | ((aCPacket[nCommIndex].abyMap[1] << 8) & 0xff00));
                                                                m_lstScanedMotors_Protocol.Add(aCPacket[nCommIndex].nProtocol);
                                                                m_lstScanedMotors_CommIndex.Add(nCommIndex);
                                                                //m_bAutoset = false;
                                                            }
                                                        }
                                                    }
                                                    int nMap_Address = ((m_bAutoset == true) ? 0 : m_aCMap[aCPacket[nCommIndex].nID].nRunningAddress);
                                                    int nMap_Size = aCPacket[nCommIndex].nStep;
                                                    Array.Copy(aCPacket[nCommIndex].abyMap, 0, m_aCMap[aCPacket[nCommIndex].nID].buffer, nMap_Address, nMap_Size);//m_aCMap[aCPacket[nCommIndex].nID].buffer.Length);

                                                    // Done
                                                    m_lstConnect[nCommIndex].nRequestData = 0;
                                                    bDone = true;
                                                //}
                                                m_lstID[nCommIndex] = -1;
                                                break;
                                        }

                                        //i++;
                                    }
                                    // Protocol 1 - Address 36 ~ 46 => (30~49) : 10=>20 바이트(Position(2), Speed(2), Load(2), Volt(1), Temp(1), Registered(2), Moving(1)
                                    else if ((m_lstIndex[nCommIndex] & 0x3000) == 0x3000)
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
                                        switch (m_lstIndex[nCommIndex] & 0x0fff)
                                        {
                                            case 0:
                                                m_lstID[nCommIndex] = byData; // ID Setting
                                                if (aCPacket[nCommIndex].nProtocol != 1) aCPacket[nCommIndex].nProtocol = 1;
                                                aCPacket[nCommIndex].nID = byData;
                                                m_lstIndex[nCommIndex]++;
                                                break;
                                            case 1:
                                                aCPacket[nCommIndex].nLength = byData; // Length == 2 -> Ping
                                                m_lstIndex[nCommIndex]++;
                                                if (byData == 2) m_lstIndex[nCommIndex]++; // Ping Data 는 Cmd 가 없다.
                                                break;
                                            //case 2:
                                            //    aCPacket[nCommIndex].nCmd = byData;
                                            //    m_lstIndex[nCommIndex]++;
                                            //    break;
                                            case 2:
                                                aCPacket[nCommIndex].nError = byData;
                                                aCPacket[nCommIndex].nStep = 0;
                                                m_lstIndex[nCommIndex]++;
                                                break;
                                            case 3:
                                                if (aCPacket[nCommIndex].nLength == 2) // Ping
                                                {
                                                    aCPacket[nCommIndex].abyMap[0] = 0;
                                                    aCPacket[nCommIndex].abyMap[1] = 0;

                                                    //if (aCPacket[nCommIndex].nStep == 0) aCPacket[nCommIndex].nModelNumber = byData;
                                                    //else if (aCPacket[nCommIndex].nStep == 1) aCPacket[nCommIndex].nModelNumber |= ((int)byData << 8) & 0xff00;
                                                    //else aCPacket[nCommIndex].nFw = byData;
                                                }
                                                else // 
                                                {
                                                    aCPacket[nCommIndex].abyMap[aCPacket[nCommIndex].nStep] = byData;
                                                }
                                                if (aCPacket[nCommIndex].nStep + 1 >= aCPacket[nCommIndex].nLength - 3) m_lstIndex[nCommIndex]++;
                                                aCPacket[nCommIndex].nStep++;
                                                break;
                                            //case 4:
                                            //    aCPacket[nCommIndex].nCrc0 = byData;
                                            //    m_lstIndex[nCommIndex]++;
                                            //    break;
                                            case 4:
                                                aCPacket[nCommIndex].nCrc0 = byData;
                                                aCPacket[nCommIndex].nCrc1 = byData;
                                                aCPacket[nCommIndex].nSeq++;
                                                m_lstIndex[nCommIndex] = 0;
                                                if (m_bPing == true)
                                                {
                                                    EMotor_t EMot = GetMotorType(aCPacket[nCommIndex].abyMap[0] | ((aCPacket[nCommIndex].abyMap[1] << 8) & 0xff00));
                                                    Ojw.CMessage.Write2("Serial[{0}]:[ID:{1}]Received[Protocol={2}]Type=0x{3}\r\n",//**{4}\r\n",
                                                        nCommIndex,
                                                        m_lstID[nCommIndex],
                                                        aCPacket[nCommIndex].nProtocol,
                                                        Ojw.CConvert.IntToHex(aCPacket[nCommIndex].abyMap[0] | ((aCPacket[nCommIndex].abyMap[1] << 8) & 0xff00), 4)//,
                                                        //EMot
                                                    );
                                                    //m_bPing = false;
                                                }
                                                if (m_bAutoset == true)
                                                {
                                                    //m_lstScanedMotors_ID.Add(m_lstID[nCommIndex]);
                                                    //m_lstScanedMotors_Model.Add(aCPacket[nCommIndex].abyMap[0] | ((aCPacket[nCommIndex].abyMap[1] << 8) & 0xff00));
                                                    //m_lstScanedMotors_Protocol.Add(aCPacket[nCommIndex].nProtocol);
                                                    //m_lstScanedMotors_CommIndex.Add(nCommIndex);
                                                    ////m_bAutoset = false;
#if _DUPLICATE
                                                    bool bDup = false;
                                                    foreach (int nDuplicated in m_lstScanedMotors_ID)
                                                    {
                                                        if (nDuplicated == m_lstID[nCommIndex])
                                                        {
                                                            Ojw.CMessage.Write2("[warning]-----ID{0} duplication[protocol version 1 -----\r\n", nDuplicated);
                                                            bDup = true;
                                                            break;
                                                        }
                                                    }
                                                    //if (m_lstScanedMotors_ID.Find(x => x == m_lstID[nCommIndex]) == 0)
                                                    if (bDup == false)
#endif
                                                    {
                                                        m_lstScanedMotors_ID.Add(m_lstID[nCommIndex]);
                                                        m_lstScanedMotors_Model.Add(aCPacket[nCommIndex].abyMap[0] | ((aCPacket[nCommIndex].abyMap[1] << 8) & 0xff00));
                                                        m_lstScanedMotors_Protocol.Add(aCPacket[nCommIndex].nProtocol);
                                                        m_lstScanedMotors_CommIndex.Add(nCommIndex);
                                                        //m_bAutoset = false;
                                                    }
                                                }

                                                int nMap_Address = ((m_bAutoset == true) ? 0 : m_aCMap[aCPacket[nCommIndex].nID].nRunningAddress);
                                                int nMap_Size = aCPacket[nCommIndex].nStep;
                                                Array.Copy(aCPacket[nCommIndex].abyMap, 0, m_aCMap[aCPacket[nCommIndex].nID].buffer, nMap_Address, nMap_Size);//m_aCMap[aCPacket[nCommIndex].nID].buffer.Length);
                                                //Array.Copy(aCPacket[nCommIndex].abyMap, 0, m_aCMap[aCPacket[nCommIndex].nID].buffer, 0, m_aCMap[aCPacket[nCommIndex].nID].buffer.Length);
                                                //Array.Copy(aCPacket[nCommIndex].abyMap, 0, m_aCMap[aCPacket[nCommIndex].nID].buffer, ((m_bAutoset == true) ? 0 : m_aCMap[aCPacket[nCommIndex].nID].nRunningAddress), m_aCMap[aCPacket[nCommIndex].nID].buffer.Length);

                                                // Done
                                                m_lstConnect[nCommIndex].nRequestData = 0;
                                                bDone = true;

                                                m_lstID[nCommIndex] = -1;
#if false
                                            //if (m_bShowReceivedIDs == true) Ojw.CMessage.Write("[Serial[{0}]:[ID:{1}]Received[Protocol={2}]", nCommIndex, m_lstID[nCommIndex], aCPacket[nCommIndex].nProtocol);
                                            if (m_bShowReceivedIDs == true) Ojw.CMessage.Write("[Serial[{0}]:[ID:{1}]Received[Protocol={2}]Type=0x{3}", nCommIndex, m_lstID[nCommIndex], aCPacket[nCommIndex].nProtocol, Ojw.CConvert.IntToHex(aCPacket[nCommIndex].abyMap[0] | ((aCPacket[nCommIndex].abyMap[1] << 8) & 0xff00), 4));
                                            if (m_bAutoset == true)
                                            {
                                                if (m_CTmr_AutoSet.Get() > 500)
                                                    m_bAutoset = false;
                                                else
                                                {
                                                    m_CTmr_AutoSet.Set();
                                                    SMotors_t SMot = new SMotors_t();
                                                    SMot.nCommIndex = nCommIndex;
                                                    SMot.nRealID = m_lstID[nCommIndex];
                                                    SMot.nProtocol = aCPacket[nCommIndex].nProtocol;
                                                    if (m_bAutoset2 == false) m_lstSIds.Add(SMot);
                                                }
                                            }
                                            if (m_bAutoset2 == true)
                                            {
                                                EMotor_t EMot = GetMotorType(aCPacket[nCommIndex].abyMap[0] | ((aCPacket[nCommIndex].abyMap[1] << 8) & 0xff00));
                                                if ((EMot != EMotor_t.NONE) && (EMot != EMotor_t.SG_90))
                                                {
                                                    //SetParam_MotorType(m_lstID[nCommIndex], EMot);

                                                    SetParam(m_lstID[nCommIndex], m_lstID[nCommIndex], nCommIndex, 0, EMot);
                                                }
                                            }
#endif
                                                break;
                                        }
#endif

                                        //i++;
                                    }
                                    else 
                                    {
                                        //Ojw.Log("=========> 통신에러,i={0}? [{1}],이전데이타={2}==========", i, Ojw.CConvert.IntToHex(byData, 2), (pBuff != null) ? pBuff.Length : 0);

                                        //foreach (byte byData2 in buf)
                                        //{
                                        //    Ojw.Log2("0x{0},", Ojw.CConvert.IntToHex(byData2, 2));
                                        //}
                                        m_lstIndex[nCommIndex] = 0;
                                        //i = 0;
                                        //i++;
                                    }

                                    if ((m_lstIndex[nCommIndex] & 0x0fff) > 0) // 0 은 ID
                                    {
                                        if ((m_lstID[nCommIndex] < 0) || (m_lstID[nCommIndex] > 253))
                                        {
                                            m_lstIndex[nCommIndex] = 0;
                                        }
                                    }
                                }
                                i++;

                                if (bDone == true)
                                {
                                    bDone = false;
                                    nCount_Motors++;
                                    //if (nCount_Motors >= nCount_AllMotors)
                                    //{
                                    //    return nSize2;
                                    //}
                                    if (nCount_Motors >= nCount_AllMotors)
                                    {
                                        break;
                                    }
                                }

                                //                                if (i >= buf.Length)
                                //                                {
                                //#if true // test
                                //                                    Ojw.CMessage.Write2("Test[Done={0}, garbage={1}=============\r\n", bDone, nSize - i);
                                //                                    foreach (byte byItem in buf)
                                //                                    {
                                //                                        Ojw.CMessage.Write2("0x{0},", Ojw.CConvert.IntToHex(byItem, 2));
                                //                                    }
                                //                                    Ojw.CMessage.Write2("\r\n****************\r\n");
                                //#endif

                                //                                    //if (bDone == true)
                                //                                    //{
                                //                                    int nSize2 = nSize - i;
                                //                                    if ((nSize2) > 0)
                                //                                    {
                                //                                        m_lstBuffers[nCommIndex] = new byte[nSize2];
                                //                                        Array.Copy(buf, i, m_lstBuffers[nCommIndex], 0, nSize2);
                                //                                    }

                                //                                    return nSize2;
                                //                                    //}
                                //                                }
                            } // foreach
#endif

#if false // test
                            Ojw.CMessage.Write2("Test2[Done={0}, garbage={1}=============\r\n", bDone, nSize - i);
                            foreach (byte byItem in buf)
                            {
                                Ojw.CMessage.Write2("0x{0},", Ojw.CConvert.IntToHex(byItem, 2));
                            }
                            Ojw.CMessage.Write2("\r\n****************\r\n");
#endif
                            //m_lstConnect[nCommIndex].nRequestData = 0;
                        } // if => nSize > 0
                        //if ((nCommIndex + 1) >= m_lstConnect.Count) nCommIndex = 0;
                        //else nCommIndex++;
                        else // 들어온 데이타가 없으면 즉시 종료
                        {
                            //if (bReceived == false) { if (CTmr.Get() >= 50) break; }
                            if (bReceived == false) { if (CTmr.Get() >= 50) break; }
                            else break;
                            //break;
                        }
                        //Ojw.CTimer.Wait(1);                        
                        Thread.Sleep(1);
                    }
                    catch (Exception ex)
                    {
                        Ojw.CMessage.Write_Error(ex.ToString());
                        //if (m_lstID != null)
                        //if (nSeq < m_lstID.Count)
                        //aCPacket[nSeq].nStep = 0;
                        if (m_lstIndex != null)
                            if (nCommIndex < m_lstIndex.Count) m_lstIndex[nCommIndex] = 0;
                        //break;
                    }
                } // while
                
#if false // test
                Ojw.CMessage.Write2("Test[Done={0}, garbage={1}=============\r\n", bDone, nSize - i);
                if (buf != null)
                {
                    foreach (byte byItem in buf)
                    {
                        Ojw.CMessage.Write2("0x{0},", Ojw.CConvert.IntToHex(byItem, 2));
                    }
                }
                Ojw.CMessage.Write2("\r\n****************\r\n");
#endif

                //if (bDone == true)
                //{
                int nSize2 = nSize - i;
                if ((nSize2) > 0)
                {
                    m_lstBuffers[nCommIndex] = new byte[nSize2];
                    Array.Copy(buf, i, m_lstBuffers[nCommIndex], 0, nSize2);
                }
                else nSize2 = 0;

                return nSize2;
                //return nRet;
                //Ojw.CMessage.Write("[Thread_Receive] Closed Thread");
            }
            private void Thread_Receive()
            {
                byte[] buf;
                int nSeq = 0;
                int nSize = 0;
                CPacket_t [] aCPacket = new CPacket_t[10]; // 10 개 이상은 일단 열지 말자...(추후 가변 작업)
                for (int i = 0; i < aCPacket.Length; i++) aCPacket[i] = new CPacket_t();
                while ((m_lstConnect.Count > 0) && (m_bProgEnd == false))
                {
                    try
                    {
                        if (m_lstConnect[nSeq].nType == 0) continue;
                        else if (m_lstConnect[nSeq].nType == 1)
                        {
                            if (m_lstConnect[nSeq].CSerial.IsConnect() == false) continue;                            
                        }
                        else if (m_lstConnect[nSeq].nType == 2)
                        {
                            if (m_lstConnect[nSeq].CSock.IsConnect() == false) continue;
                        }
                        // nType 이 1, 2 인경우만 감안
                        nSize = ((m_lstConnect[nSeq].nType == 1) ? m_lstConnect[nSeq].CSerial.GetBuffer_Length() : ((m_lstConnect[nSeq].nType == 2) ? m_lstConnect[nSeq].CSock.GetBuffer_Length() : 0));
                        
                        if (nSize > 0)// || (m_lstConnect[nSeq].CSerial.GetBuffer_Length() > 0))
                        {
                            lock (this)
                            {
                                //m_lstConnect[nSeq].nRequestData = 0;
                                
                                buf = ((m_lstConnect[nSeq].nType == 1) ? m_lstConnect[nSeq].CSerial.GetBytes() : m_lstConnect[nSeq].CSock.GetBytes());

#if false // test
                            foreach (byte byData in buf)
                            {
                                Ojw.CMessage.Write2("0x{0}", Ojw.CConvert.IntToHex(byData, 2));
                            }
                            Ojw.CMessage.Write2("\r\n");
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
                                        //aCPacket[nSeq].bShow = false;

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
                                                    if (aCPacket[nSeq].nProtocol != 2) aCPacket[nSeq].nProtocol = 2;
                                                    //m_aSMap[m_lstID[nSeq]].nID = byData;
                                                    aCPacket[nSeq].nID = m_lstID[nSeq];
                                                    m_lstIndex[nSeq]++;
                                                    break;
                                                case 1:
                                                    aCPacket[nSeq].nLength = byData; // Length == 7 -> Ping
                                                    m_lstIndex[nSeq]++;
                                                    break;
                                                case 2:
                                                    aCPacket[nSeq].nLength |= (((int)byData << 8) & 0xff00); // Length == 7 -> Ping
                                                    m_lstIndex[nSeq]++;
                                                    break;
                                                case 3:
                                                    aCPacket[nSeq].nCmd = byData;
                                                    m_lstIndex[nSeq]++;
                                                    break;
                                                case 4:
                                                    aCPacket[nSeq].nError = byData;
                                                    aCPacket[nSeq].nStep = 0;
                                                    if (aCPacket[nSeq].nLength == 7) // Ping
                                                    {
                                                        aCPacket[nSeq].abyMap[0] = 0;
                                                        aCPacket[nSeq].abyMap[1] = 0;
                                                    }
                                                    m_lstIndex[nSeq]++;
                                                    break;
                                                case 5:
                                                    if (aCPacket[nSeq].nLength == 7) // Ping
                                                    {
                                                        if (aCPacket[nSeq].nStep == 0)
                                                        {
                                                            //aCPacket[nSeq].bShow = true;
                                                            aCPacket[nSeq].nModelNumber = byData;
                                                            aCPacket[nSeq].abyMap[0] = byData;
                                                        }
                                                        else if (aCPacket[nSeq].nStep == 1)
                                                        {
                                                            aCPacket[nSeq].nModelNumber |= ((int)byData << 8) & 0xff00;
                                                            aCPacket[nSeq].abyMap[1] = byData;
                                                        }
                                                        else aCPacket[nSeq].nFw = byData;

                                                        //if (aCPacket[nSeq].nStep + 1 >= aCPacket[nSeq].nLength - 4)
                                                        //{
                                                        //    //Ojw.CMessage.Write("Model={0}", 
                                                        //    Ojw.CMessage.Write("[Serial[{0}]:[ID:{1}]Received[Protocol={2}]Type=0x{3}", nSeq, m_lstID[nSeq], aCPacket[nSeq].nProtocol, Ojw.CConvert.IntToHex(aCPacket[nSeq].abyMap[0] | ((aCPacket[nSeq].abyMap[1] << 8) & 0xff00), 4));
                                                        //    m_lstIndex[nSeq]++;
                                                        //}
                                                    }
                                                    else // 
                                                    {
                                                        aCPacket[nSeq].abyMap[aCPacket[nSeq].nStep] = byData;

                                                        //if (aCPacket[nSeq].nStep + 1 >= aCPacket[nSeq].nLength - 4) m_lstIndex[nSeq]++;
                                                    }
                                                    if (aCPacket[nSeq].nStep + 1 >= aCPacket[nSeq].nLength - 4) m_lstIndex[nSeq]++;
                                                    aCPacket[nSeq].nStep++;
                                                    break;
                                                case 6:
                                                    aCPacket[nSeq].nCrc0 = byData;
                                                    m_lstIndex[nSeq]++;
                                                    break;
                                                case 7:
                                                    aCPacket[nSeq].nCrc1 = byData;
                                                    m_lstIndex[nSeq] = 0;
                                                    if (m_bPing == true)
                                                    {
                                                        //EMotor_t EMot = GetMotorType(aCPacket[nSeq].abyMap[0] | ((aCPacket[nSeq].abyMap[1] << 8) & 0xff00));
                                                        Ojw.CMessage.Write2("Serial[{0}]:[ID:{1}]Received[Protocol={2}]Type=0x{3}\r\n",//**{4}\r\n", 
                                                            nSeq,
                                                            m_lstID[nSeq],
                                                            aCPacket[nSeq].nProtocol,
                                                            Ojw.CConvert.IntToHex(aCPacket[nSeq].abyMap[0] | ((aCPacket[nSeq].abyMap[1] << 8) & 0xff00), 4)//, 
                                                            //EMot
                                                        );
                                                        //m_bPing = false;
                                                    }
                                                    if (m_bAutoset == true)
                                                    {
                                                        bool bDup = false;
                                                        foreach (int nDuplicated in m_lstScanedMotors_ID)
                                                        {
                                                            if (nDuplicated == m_lstID[nSeq])
                                                            {
                                                                bDup = true;
                                                                break;
                                                            }
                                                        }
                                                        if (bDup == false)
                                                        //int nFind = m_lstScanedMotors_ID.Find(x => x == m_lstID[nSeq]);
                                                        //if (nFind == 0)
                                                        {
                                                            m_lstScanedMotors_ID.Add(m_lstID[nSeq]);
                                                            m_lstScanedMotors_Model.Add(aCPacket[nSeq].abyMap[0] | ((aCPacket[nSeq].abyMap[1] << 8) & 0xff00));
                                                            m_lstScanedMotors_Protocol.Add(aCPacket[nSeq].nProtocol);
                                                            m_lstScanedMotors_CommIndex.Add(nSeq);
                                                            //m_bAutoset = false;
                                                        }
                                                    }

                                                    Array.Copy(aCPacket[nSeq].abyMap, 0, m_aCMap[aCPacket[nSeq].nID].buffer, 0, m_aCMap[aCPacket[nSeq].nID].buffer.Length);

                                                    // Done
                                                    m_lstConnect[nSeq].nRequestData = 0;
#if false
                                                aCPacket[nSeq].nSeq++;
                                                //if (m_bShowReceivedIDs == true) Ojw.CMessage.Write("[Serial[{0}]:[ID:{1}]Received[Protocol={2}]Type={3}", nSeq, m_lstID[nSeq], aCPacket[nSeq].nProtocol, (aCPacket[nSeq].abyMap[0] | aCPacket[nSeq].abyMap[1] << 8));

                                                if (
                                                    (m_bShowReceivedIDs == true)
                                                    ||
                                                    (aCPacket[nSeq].bShow == true)
                                                    )
                                                    Ojw.CMessage.Write("[Serial[{0}]:[ID:{1}]Received[Protocol={2}]Type=0x{3}", nSeq, m_lstID[nSeq], aCPacket[nSeq].nProtocol, Ojw.CConvert.IntToHex(aCPacket[nSeq].abyMap[0] | ((aCPacket[nSeq].abyMap[1] << 8) & 0xff00), 4));
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
                                                        SMot.nProtocol = aCPacket[nSeq].nProtocol;
                                                        if (m_bAutoset2 == false) m_lstSIds.Add(SMot);
                                                    }
                                                }
                                                if (m_bAutoset2 == true)
                                                {
                                                    EMotor_t EMot = GetMotorType(aCPacket[nSeq].abyMap[0] | ((aCPacket[nSeq].abyMap[1] << 8) & 0xff00));
                                                    if ((EMot != EMotor_t.NONE) && (EMot != EMotor_t.SG_90))
                                                    {
                                                        //SetParam_MotorType(m_lstID[nSeq], EMot);
                                                        SetParam(m_lstID[nSeq], m_lstID[nSeq], nSeq, 0, EMot);
                                                    }
                                                }
#endif
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
                                                    if (aCPacket[nSeq].nProtocol != 1) aCPacket[nSeq].nProtocol = 1;
                                                    aCPacket[nSeq].nID = byData;
                                                    m_lstIndex[nSeq]++;
                                                    break;
                                                case 1:
                                                    aCPacket[nSeq].nLength = byData; // Length == 2 -> Ping
                                                    m_lstIndex[nSeq]++;
                                                    if (byData == 2) m_lstIndex[nSeq]++; // Ping Data 는 Cmd 가 없다.
                                                    break;
                                                //case 2:
                                                //    aCPacket[nSeq].nCmd = byData;
                                                //    m_lstIndex[nSeq]++;
                                                //    break;
                                                case 2:
                                                    aCPacket[nSeq].nError = byData;
                                                    aCPacket[nSeq].nStep = 0;
                                                    m_lstIndex[nSeq]++;
                                                    break;
                                                case 3:
                                                    if (aCPacket[nSeq].nLength == 2) // Ping
                                                    {
                                                        aCPacket[nSeq].abyMap[0] = 0;
                                                        aCPacket[nSeq].abyMap[1] = 0;

                                                        //if (aCPacket[nSeq].nStep == 0) aCPacket[nSeq].nModelNumber = byData;
                                                        //else if (aCPacket[nSeq].nStep == 1) aCPacket[nSeq].nModelNumber |= ((int)byData << 8) & 0xff00;
                                                        //else aCPacket[nSeq].nFw = byData;
                                                    }
                                                    else // 
                                                    {
                                                        aCPacket[nSeq].abyMap[aCPacket[nSeq].nStep] = byData;
                                                    }
                                                    if (aCPacket[nSeq].nStep + 1 >= aCPacket[nSeq].nLength - 3) m_lstIndex[nSeq]++;
                                                    aCPacket[nSeq].nStep++;
                                                    break;
                                                //case 4:
                                                //    aCPacket[nSeq].nCrc0 = byData;
                                                //    m_lstIndex[nSeq]++;
                                                //    break;
                                                case 4:
                                                    aCPacket[nSeq].nCrc0 = byData;
                                                    aCPacket[nSeq].nCrc1 = byData;
                                                    aCPacket[nSeq].nSeq++;
                                                    m_lstIndex[nSeq] = 0;
                                                    if (m_bPing == true)
                                                    {
                                                        EMotor_t EMot = GetMotorType(aCPacket[nSeq].abyMap[0] | ((aCPacket[nSeq].abyMap[1] << 8) & 0xff00));
                                                        Ojw.CMessage.Write2("Serial[{0}]:[ID:{1}]Received[Protocol={2}]Type=0x{3}\r\n",//**{4}\r\n",
                                                            nSeq,
                                                            m_lstID[nSeq],
                                                            aCPacket[nSeq].nProtocol,
                                                            Ojw.CConvert.IntToHex(aCPacket[nSeq].abyMap[0] | ((aCPacket[nSeq].abyMap[1] << 8) & 0xff00), 4)//,
                                                            //EMot
                                                        );
                                                        //m_bPing = false;
                                                    }
                                                    if (m_bAutoset == true)
                                                    {
                                                        //m_lstScanedMotors_ID.Add(m_lstID[nSeq]);
                                                        //m_lstScanedMotors_Model.Add(aCPacket[nSeq].abyMap[0] | ((aCPacket[nSeq].abyMap[1] << 8) & 0xff00));
                                                        //m_lstScanedMotors_Protocol.Add(aCPacket[nSeq].nProtocol);
                                                        //m_lstScanedMotors_CommIndex.Add(nSeq);
                                                        ////m_bAutoset = false;
                                                        bool bDup = false;
                                                        foreach (int nDuplicated in m_lstScanedMotors_ID)
                                                        {
                                                            if (nDuplicated == m_lstID[nSeq])
                                                            {
                                                                bDup = true;
                                                                break;
                                                            }
                                                        }
                                                        //if (m_lstScanedMotors_ID.Find(x => x == m_lstID[nSeq]) == 0)
                                                        if (bDup == false)
                                                        {
                                                            m_lstScanedMotors_ID.Add(m_lstID[nSeq]);
                                                            m_lstScanedMotors_Model.Add(aCPacket[nSeq].abyMap[0] | ((aCPacket[nSeq].abyMap[1] << 8) & 0xff00));
                                                            m_lstScanedMotors_Protocol.Add(aCPacket[nSeq].nProtocol);
                                                            m_lstScanedMotors_CommIndex.Add(nSeq);
                                                            //m_bAutoset = false;
                                                        }
                                                    }

                                                    Array.Copy(aCPacket[nSeq].abyMap, 0, m_aCMap[aCPacket[nSeq].nID].buffer, 0, m_aCMap[aCPacket[nSeq].nID].buffer.Length);
                                                    // Done
                                                    m_lstConnect[nSeq].nRequestData = 0;
#if false
                                                //if (m_bShowReceivedIDs == true) Ojw.CMessage.Write("[Serial[{0}]:[ID:{1}]Received[Protocol={2}]", nSeq, m_lstID[nSeq], aCPacket[nSeq].nProtocol);
                                                if (m_bShowReceivedIDs == true) Ojw.CMessage.Write("[Serial[{0}]:[ID:{1}]Received[Protocol={2}]Type=0x{3}", nSeq, m_lstID[nSeq], aCPacket[nSeq].nProtocol, Ojw.CConvert.IntToHex(aCPacket[nSeq].abyMap[0] | ((aCPacket[nSeq].abyMap[1] << 8) & 0xff00), 4));
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
                                                        SMot.nProtocol = aCPacket[nSeq].nProtocol;
                                                        if (m_bAutoset2 == false) m_lstSIds.Add(SMot);
                                                    }
                                                }
                                                if (m_bAutoset2 == true)
                                                {
                                                    EMotor_t EMot = GetMotorType(aCPacket[nSeq].abyMap[0] | ((aCPacket[nSeq].abyMap[1] << 8) & 0xff00));
                                                    if ((EMot != EMotor_t.NONE) && (EMot != EMotor_t.SG_90))
                                                    {
                                                        //SetParam_MotorType(m_lstID[nSeq], EMot);

                                                        SetParam(m_lstID[nSeq], m_lstID[nSeq], nSeq, 0, EMot);
                                                    }
                                                }
#endif
                                                    break;
                                            }
#endif
                                        }
                                    }
                                    i++;
                                }
#endif
                                //m_lstConnect[nSeq].nRequestData = 0;
                            } // lock(this)

                        }

                        if ((nSeq + 1) >= m_lstConnect.Count) nSeq = 0;
                        else nSeq++;

                        Thread.Sleep(1);
                    }
                    catch (Exception ex)
                    {
                        Ojw.CMessage.Write_Error(ex.ToString());
                        //if (m_lstID != null)
                            //if (nSeq < m_lstID.Count)
                        //aCPacket[nSeq].nStep = 0;
                        if (m_lstIndex != null)
                            if (nSeq < m_lstIndex.Count) m_lstIndex[nSeq] = 0;
                        //break;
                    }
                }

                Ojw.CMessage.Write("[Thread_Receive] Closed Thread");
            }
#if false
            private void SetAddress(int nMotor, EMotor_t EMot) { SetAddress(EMot, ref m_aCAddress[nMotor]); }
            private void SetAddress(EMotor_t EMot, ref CAddress_t CAddress)
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
#if false
                        _ADDR_1_MotorNumber_0_2 = 0;        // R    0 : none
                        _ADDR_1_MotorNumber_Size_2 = 2;
                        _ADDR_1_FwVersion_6_1 = 6;          // R      
                        _ADDR_1_FwVersion_Size_1 = 1;

                        CAddress.nRealID_7_1 = 7;             // RW   0 ~ 252
                        CAddress.nRealID_Size_1 = 1;
                        CAddress.nBaudrate_8_1 = 8;           // RW   0-9600, 1-57600, 2-115200, 3-1M, 4-2M,  5-3M, 6-4M, 7-4.5M
                        CAddress.nBaudrate_Size_1 = 1;




                        CAddress.nTorq_64_1 = 24;              // RW   Off(0), On(1)
                        CAddress.nTorq_Size_1 = 1;
                        CAddress.nLed_65_1 = 25;               // RW   Off(0), On(1)
                        CAddress.nLed_Size_1 = 1;

                        CAddress.nMode_Operating_11_1 = 6;    // RW   1=속도제어(무한회전), 3(초기값)=위치제어(RPM), 4=Multi-Turn(-256~256:512회전), 5=전류기반 위치제어모드(-256~256:512회전), 16=PWM제어
                        CAddress.nMode_Operating_Size_1 = 4; // 속도제어시 0, 위치제어시 [6]00, [7]00, [8]ff, [9]03 ( 65283 ) 으로 셋팅해야 함. -> CW Limit 0, CCW Limit 1023

                        CAddress.nGoal_Vel_104_4 = 32;         // RW
                        CAddress.nGoal_Vel_Size_4 = 2;

                        CAddress.nProfile_Vel_112_4 = 32;      // RW
                        CAddress.nProfile_Vel_Size_4 = 2;

                        CAddress.nGoal_Pos_116_4 = 30;         // RW
                        CAddress.nGoal_Pos_Size_4 = 2;
#endif

                        break;
            #endregion AX
            #region XL_430 & MX Protocol 2
                    case EMotor_t.MX_28:
                    case EMotor_t.MX_64:
                    case EMotor_t.MX_106:
                    case EMotor_t.XM_540:
                    case EMotor_t.XL_430:
                        CAddress.nMotorNumber_0_2 = 0;        // R    0 : none
                        CAddress.nMotorNumber_Size_2 = 2;
                        CAddress.nFwVersion_6_1 = 6;          // R      
                        CAddress.nFwVersion_Size_1 = 1;

                        CAddress.nRealID_7_1 = 7;             // RW   0 ~ 252
                        CAddress.nRealID_Size_1 = 1;
                        CAddress.nBaudrate_8_1 = 8;           // RW   0-9600, 1-57600, 2-115200, 3-1M, 4-2M,  5-3M, 6-4M, 7-4.5M
                        CAddress.nBaudrate_Size_1 = 1;
                        // [DriveMode] 
                        //    0x01 : 정상회전(0), 역회전(1)
                        //    0x02 : 540 전용 Master(0), Slave(1)
                        //    0x04 : 속도기준 제어(0), 시간기준 제어(1)
                        CAddress.nMode_Drive_10_1 = 10;        // RW 
                        CAddress.nMode_Drive_Size_1 = 1;
                        CAddress.nMode_Operating_11_1 = 11;    // RW   1=속도제어(무한회전), 3(초기값)=위치제어(RPM), 4=Multi-Turn(-256~256:512회전), 5=전류기반 위치제어모드(-256~256:512회전), 16=PWM제어
                        CAddress.nMode_Operating_Size_1 = 1;
                        CAddress.nProtocolVersion_13_1 = 13;   // RW   
                        CAddress.nProtocolVersion_Size_1 = 1;
                        CAddress.nOffset_20_4 = 20;            // RW   1회전 모드인 경우 -1024~1024 까지만 제어
                        CAddress.nOffset_Size_4 = 4;
                        CAddress.nLimit_PWM_36_2 = 36;         // RW   0~885 (885 = 100%)
                        CAddress.nLimit_PWM_Size_2 = 2;
                        CAddress.nLimit_Curr_38_2 = 38;        // RW   전류제어모드/전류기반위치제어모드에서 사용, 0~2047
                        CAddress.nLimit_Curr_Size_2 = 2;
                        // [Shutdown] - Reboot 으로만 해제 가능
                        //    0x20 : 과부하
                        //    0x10 : 전력이상
                        //    0x08 : 엔코더 이상(Following Error)
                        //    0x04 : 과열
                        //    0x01 : 인가된 전압 이상
                        CAddress.nShutdown_63_1 = 63;          // RW
                        CAddress.nShutdown_Size_1 = 1;
                        CAddress.nTorq_64_1 = 64;              // RW   Off(0), On(1)
                        CAddress.nTorq_Size_1 = 1;
                        CAddress.nLed_65_1 = 65;               // RW   Off(0), On(1)
                        CAddress.nLed_Size_1 = 1;
                        CAddress.nError_70_1 = 70;             // R    
                        CAddress.nError_Size_1 = 1;
                        CAddress.nGain_Vel_I_76_2 = 76;        // RW
                        CAddress.nGain_Vel_I_Size_2 = 2;
                        CAddress.nGain_Vel_P_78_2 = 78;        // RW
                        CAddress.nGain_Vel_P_Size_2 = 2;
                        CAddress.nGain_Pos_D_80_2 = 80;        // RW
                        CAddress.nGain_Pos_D_Size_2 = 2;
                        CAddress.nGain_Pos_I_82_2 = 82;        // RW
                        CAddress.nGain_Pos_I_Size_2 = 2;
                        CAddress.nGain_Pos_P_84_2 = 84;        // RW
                        CAddress.nGain_Pos_P_Size_2 = 2;
                        CAddress.nGain_Pos_F2_88_2 = 88;       // RW
                        CAddress.nGain_Pos_F2_Size_2 = 2;
                        CAddress.nGain_Pos_F1_90_2 = 90;       // RW
                        CAddress.nGain_Pos_F1_Size_2 = 2;

                        CAddress.nWatchDog_98_1 = 98;          // RW
                        CAddress.nWatchDog_Size_1 = 1;

                        CAddress.nGoal_PWM_100_2 = 100;         // RW   -PWMLimit ~ +PWMLimit
                        CAddress.nGoal_PWM_Size_2 = 2;
                        CAddress.nGoal_Current_102_2 = 102;     // RW   -CurrentLimit ~ +CurrentLimit
                        CAddress.nGoal_Current_Size_2 = 2;
                        CAddress.nGoal_Vel_104_4 = 104;         // RW
                        CAddress.nGoal_Vel_Size_4 = 4;

                        CAddress.nProfile_Acc_108_4 = 108;      // RW
                        CAddress.nProfile_Acc_Size_4 = 4;
                        CAddress.nProfile_Vel_112_4 = 112;      // RW
                        CAddress.nProfile_Vel_Size_4 = 4;

                        CAddress.nGoal_Pos_116_4 = 116;         // RW
                        CAddress.nGoal_Pos_Size_4 = 4;

                        CAddress.nMoving_122_1 = 122;           // R    움직임 감지 못함(0), 움직임 감지(1)
                        CAddress.nMoving_Size_1 = 1;
                        // [Moving Status]
                        //    0x30  - 0x30 : 사다리꼴 속도 프로파일
                        //            0x20 : 삼각 속도 프로파일
                        //            0x10 : 사각 속도 프로파일
                        //            0x00 : 프로파일 미사용(Step)
                        //    0x08 : Following Error
                        //    0x02 : Goal Position 명령에 따라 진행 중
                        //    0x01 : Inposition
                        CAddress.nMoving_Status_123_1 = 123;    // R
                        CAddress.nMoving_Status_Size_1 = 1;

                        CAddress.nPresent_PWM_124_2 = 124;      // R
                        CAddress.nPresent_PWM_Size_2 = 2;
                        CAddress.nPresent_Curr_126_2 = 126;     // R
                        CAddress.nPresent_Curr_Size_2 = 2;
                        CAddress.nPresent_Vel_128_4 = 128;      // R
                        CAddress.nPresent_Vel_Size_4 = 4;
                        CAddress.nPresent_Pos_132_4 = 132;      // R
                        CAddress.nPresent_Pos_Size_4 = 4;
                        CAddress.nPresent_Volt_144_2 = 144;     // R
                        CAddress.nPresent_Volt_Size_2 = 2;
                        CAddress.nPresent_Temp_146_1 = 146;     // R
                        CAddress.nPresent_Temp_Size_1 = 1;
                        break;
            #endregion XL_430
            #region XL_320
                    case EMotor_t.XL_320:
                        CAddress.nTorq_64_1 = 24;              // RW   Off(0), On(1)                        
                        CAddress.nLed_65_1 = 25;               // RW   Off(0), On(1)

                        CAddress.nMode_Operating_11_1 = 6;    // RW   1=속도제어(무한회전), 3(초기값)=위치제어(RPM), 4=Multi-Turn(-256~256:512회전), 5=전류기반 위치제어모드(-256~256:512회전), 16=PWM제어
                        CAddress.nMode_Operating_Size_1 = 5; // 속도제어시 0, 위치제어시 [6]00, [7]00, [8]ff, [9]03 ( 65283 ) 으로 셋팅해야 함. -> CW Limit 0, CCW Limit 1023
                        CAddress.nProtocolVersion_13_1 = 13;   // RW  1 바퀴모드, 2 관절모드 (이 부분이 프로토콜 1의 다른 모터와 xl320 이 다른 부분)(아예 위 4바이트에 합쳐서 5바이트 만들어 제어)
                        CAddress.nProtocolVersion_Size_1 = 1;


                        CAddress.nGoal_Vel_104_4 = 32;         // RW
                        CAddress.nGoal_Vel_Size_4 = 2;

                        CAddress.nProfile_Vel_112_4 = 32;      // RW
                        CAddress.nProfile_Vel_Size_4 = 2;

                        CAddress.nGoal_Pos_116_4 = 30;         // RW
                        CAddress.nGoal_Pos_Size_4 = 2;
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
                        CAddress.nMotorNumber_0_2 = 0;        // R    0 : none
                        CAddress.nMotorNumber_Size_2 = 2;
                        CAddress.nFwVersion_6_1 = 6;          // R      
                        CAddress.nFwVersion_Size_1 = 1;

                        CAddress.nRealID_7_1 = 7;             // RW   0 ~ 252
                        CAddress.nRealID_Size_1 = 1;
                        CAddress.nBaudrate_8_1 = 8;           // RW   0-9600, 1-57600, 2-115200, 3-1M, 4-2M,  5-3M, 6-4M, 7-4.5M
                        CAddress.nBaudrate_Size_1 = 1;
                        // [DriveMode] 
                        //    0x01 : 정상회전(0), 역회전(1)
                        //    0x02 : 540 전용 Master(0), Slave(1)
                        //    0x04 : 속도기준 제어(0), 시간기준 제어(1)
                        CAddress.nMode_Drive_10_1 = 10;        // RW 
                        CAddress.nMode_Drive_Size_1 = 1;
                        //CAddress.nMode_Operating_11_1 = 11;    // RW   1=속도제어(무한회전), 3(초기값)=위치제어(RPM), 4=Multi-Turn(-256~256:512회전), 5=전류기반 위치제어모드(-256~256:512회전), 16=PWM제어
                        //CAddress.nMode_Operating_Size_1 = 1;
                        //CAddress.nProtocolVersion_13_1 = 13;   // RW   
                        //CAddress.nProtocolVersion_Size_1 = 1;
                        CAddress.nOffset_20_4 = 20;            // RW   1회전 모드인 경우 -1024~1024 까지만 제어
                        CAddress.nOffset_Size_4 = 4;
                        CAddress.nLimit_PWM_36_2 = 36;         // RW   0~885 (885 = 100%)
                        CAddress.nLimit_PWM_Size_2 = 2;
                        CAddress.nLimit_Curr_38_2 = 38;        // RW   전류제어모드/전류기반위치제어모드에서 사용, 0~2047
                        CAddress.nLimit_Curr_Size_2 = 2;
                        // [Shutdown] - Reboot 으로만 해제 가능
                        //    0x20 : 과부하
                        //    0x10 : 전력이상
                        //    0x08 : 엔코더 이상(Following Error)
                        //    0x04 : 과열
                        //    0x01 : 인가된 전압 이상
                        CAddress.nShutdown_63_1 = 63;          // RW
                        CAddress.nShutdown_Size_1 = 1;
                        //CAddress.nTorq_64_1 = 64;              // RW   Off(0), On(1)
                        CAddress.nTorq_Size_1 = 1;
                        //CAddress.nLed_65_1 = 65;               // RW   Off(0), On(1)
                        CAddress.nLed_Size_1 = 1;
                        CAddress.nError_70_1 = 70;             // R    
                        CAddress.nError_Size_1 = 1;
                        CAddress.nGain_Vel_I_76_2 = 76;        // RW
                        CAddress.nGain_Vel_I_Size_2 = 2;
                        CAddress.nGain_Vel_P_78_2 = 78;        // RW
                        CAddress.nGain_Vel_P_Size_2 = 2;
                        CAddress.nGain_Pos_D_80_2 = 80;        // RW
                        CAddress.nGain_Pos_D_Size_2 = 2;
                        CAddress.nGain_Pos_I_82_2 = 82;        // RW
                        CAddress.nGain_Pos_I_Size_2 = 2;
                        CAddress.nGain_Pos_P_84_2 = 84;        // RW
                        CAddress.nGain_Pos_P_Size_2 = 2;
                        CAddress.nGain_Pos_F2_88_2 = 88;       // RW
                        CAddress.nGain_Pos_F2_Size_2 = 2;
                        CAddress.nGain_Pos_F1_90_2 = 90;       // RW
                        CAddress.nGain_Pos_F1_Size_2 = 2;

                        CAddress.nWatchDog_98_1 = 98;          // RW
                        CAddress.nWatchDog_Size_1 = 1;

                        CAddress.nGoal_PWM_100_2 = 100;         // RW   -PWMLimit ~ +PWMLimit
                        CAddress.nGoal_PWM_Size_2 = 2;
                        CAddress.nGoal_Current_102_2 = 102;     // RW   -CurrentLimit ~ +CurrentLimit
                        CAddress.nGoal_Current_Size_2 = 2;
                        //CAddress.nGoal_Vel_104_4 = 104;         // RW
                        //CAddress.nGoal_Vel_Size_4 = 4;

                        CAddress.nProfile_Acc_108_4 = 108;      // RW
                        CAddress.nProfile_Acc_Size_4 = 4;
                        //CAddress.nProfile_Vel_112_4 = 112;      // RW
                        //CAddress.nProfile_Vel_Size_4 = 4;

                        //CAddress.nGoal_Pos_116_4 = 116;         // RW
                        //CAddress.nGoal_Pos_Size_4 = 4;

                        CAddress.nMoving_122_1 = 122;           // R    움직임 감지 못함(0), 움직임 감지(1)
                        CAddress.nMoving_Size_1 = 1;
                        // [Moving Status]
                        //    0x30  - 0x30 : 사다리꼴 속도 프로파일
                        //            0x20 : 삼각 속도 프로파일
                        //            0x10 : 사각 속도 프로파일
                        //            0x00 : 프로파일 미사용(Step)
                        //    0x08 : Following Error
                        //    0x02 : Goal Position 명령에 따라 진행 중
                        //    0x01 : Inposition
                        CAddress.nMoving_Status_123_1 = 123;    // R
                        CAddress.nMoving_Status_Size_1 = 1;

                        CAddress.nPresent_PWM_124_2 = 124;      // R
                        CAddress.nPresent_PWM_Size_2 = 2;
                        CAddress.nPresent_Curr_126_2 = 126;     // R
                        CAddress.nPresent_Curr_Size_2 = 2;
                        CAddress.nPresent_Vel_128_4 = 128;      // R
                        CAddress.nPresent_Vel_Size_4 = 4;
                        CAddress.nPresent_Pos_132_4 = 132;      // R
                        CAddress.nPresent_Pos_Size_4 = 4;
                        CAddress.nPresent_Volt_144_2 = 144;     // R
                        CAddress.nPresent_Volt_Size_2 = 2;
                        CAddress.nPresent_Temp_146_1 = 146;     // R
                        CAddress.nPresent_Temp_Size_1 = 1;
                        break;
            #endregion XL_320
                }
            }
#endif
#if false
            public int FindMotor(int nMotor_RealID) { int i = 0; foreach (SParam_t SParam in m_aCParam) { if (SParam.nRealID == nMotor_RealID) return i; i++; } return -1; }
            public int Get_RealID(int nMotor) { return m_aCParam[nMotor].nRealID; }

            #region Parameter Function(SetParam...)
            public void SetParam_Dir(int nMotor, int nDir) { m_aCParam[nMotor].nDir = nDir; }
            public void SetParam_RealID(int nMotor, int nMotorRealID) { m_aCParam[nMotor].nRealID = nMotorRealID; }
            //public void SetParam_OperationMode(int nMotor, EOperationMode_t EOperationMode) { m_aCParam[nMotor].EOperationMode = EOperationMode; }

            public void SetParam_CommIndex(int nMotor, int nCommIndex) { m_aCParam[nMotor].nCommIndex = nCommIndex; }               // 연결 이후에 둘 중 하나만 사용 한다.(되도록  CommIndex 를 사용할 것. Commport 로 설정 하려면 통신이 접속이 되어 있어야 한다.)
            public void SetParam_CommPort(int nMotor, int nCommPort) { m_aCParam[nMotor].nCommIndex = GetSerialIndex(nCommPort); }  // CommIndex 설정보다 직관적이나 잘못 설정 될 수 있다. 연결이 안 된 경우 CommIndes 가 잘못 지정될 수가 있다.

            public void SetParam_LimitUp(int nMotor, float fLimitUp) { m_aCParam[nMotor].fLimitUp = fLimitUp; }                       // Limit - 0: Ignore 
            public void SetParam_LimitDown(int nMotor, float fLimitDn) { m_aCParam[nMotor].fLimitDn = fLimitDn; }                       // Limit - 0: Ignore 
            public void SetParam_LimitRpm(int nMotor, float fLimitRpm) { m_aCParam[nMotor].fLimitRpm = fLimitRpm; }                       // Limit - 0: Ignore 
            public void SetParam_MotorType(int nMotor, EMotor_t EMot)
            {
                SetAddress(EMot, ref m_aCAddress[nMotor]);

                m_aCParam[nMotor].EMot = EMot;

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
                        //m_aCParam[nMotor].bEn = true;                       // 활성화

                        //m_aCParam[nMotor].nID = nMotor;                     // 모터를 제어하기 위한 가상 아이디. 중복 허용 불가. 보통은 모터 ID 와 일치시킨다.      
                        //m_aCParam[nMotor].nRealID = nMotorRealID;           // 실제 사용할 모터의 아이디
                        //m_aCParam[nMotor].nCommIndex = nCommIndex;          // 연결 된 통신포트 중 어느것에 연결 된 것이지에 대한 순서.

                        //m_aCParam[nMotor].nDir = nDir;
                        //m_aCParam[nMotor].EMot = EMot;
                        m_aCParam[nMotor].fCenterPos = 512.0f;

                        m_aCParam[nMotor].fMechMove = 1024.0f;
                        m_aCParam[nMotor].fDegree = 300.0f;
                        m_aCParam[nMotor].fRefRpm = 0.111f;                 // 기본 rpm 단위

                        m_aCParam[nMotor].fLimitRpm = 1023f;                // 

                        m_aCParam[nMotor].fLimitUp = 0.0f;    // Limit - 0: Ignore
                        m_aCParam[nMotor].fLimitDn = 0.0f;    // Limit - 0: Ignore

                        m_aCParam[nMotor].nProtocol = 1;               // 2, 0 - protocol2, 1 - protocol1, 3 - None     

                        break;
                    case EMotor_t.EX_106:
                        //m_aCParam[nMotor].EMot = EMot;
                        m_aCParam[nMotor].fCenterPos = 2048.0f;

                        m_aCParam[nMotor].fMechMove = 4096.0f;
                        m_aCParam[nMotor].fDegree = 251.0f;
                        m_aCParam[nMotor].fRefRpm = 0.111f;                 // 기본 rpm 단위

                        m_aCParam[nMotor].fLimitRpm = 1023f;                // 

                        m_aCParam[nMotor].fLimitUp = 0.0f;    // Limit - 0: Ignore
                        m_aCParam[nMotor].fLimitDn = 0.0f;    // Limit - 0: Ignore

                        m_aCParam[nMotor].nProtocol = 1;               // 2, 0 - protocol2, 1 - protocol1, 3 - None     
                        break;
            #region MX
                    case EMotor_t.MX_12:
                        m_aCParam[nMotor].fCenterPos = 1024.0f;

                        m_aCParam[nMotor].fMechMove = 2048.0f;
                        m_aCParam[nMotor].fDegree = 360.0f;
                        m_aCParam[nMotor].fRefRpm = 0.916f;            // 기본 rpm 단위

                        m_aCParam[nMotor].fLimitRpm = 415f;                // 

                        m_aCParam[nMotor].fLimitUp = 0.0f;    // Limit - 0: Ignore
                        m_aCParam[nMotor].fLimitDn = 0.0f;    // Limit - 0: Ignore

                        m_aCParam[nMotor].nProtocol = 1;               // 2, 0 - protocol2, 1 - protocol1, 3 - None              
                        break;
                    case EMotor_t.MX_28:
                    case EMotor_t.MX_64:
                    case EMotor_t.MX_106:
                        m_aCParam[nMotor].fCenterPos = 2048.0f;

                        m_aCParam[nMotor].fMechMove = 4096.0f;
                        m_aCParam[nMotor].fDegree = 360.0f;
                        m_aCParam[nMotor].fRefRpm = 0.229f; ;//(EMot == EMotor_t.MX_12) ? 0.916f : 0.114f;  //0.229f;                 // 기본 rpm 단위

                        m_aCParam[nMotor].fLimitRpm = 415f;                // 

                        m_aCParam[nMotor].fLimitUp = 0.0f;    // Limit - 0: Ignore
                        m_aCParam[nMotor].fLimitDn = 0.0f;    // Limit - 0: Ignore

                        m_aCParam[nMotor].nProtocol = 2;               // 2, 0 - protocol2, 1 - protocol1, 3 - None              
                        break;
            #endregion MX
                    // Protocol2
                    case EMotor_t.NONE:
                    case EMotor_t.XM_540:
                    case EMotor_t.XL_430:
                        //m_aCParam[nMotor].bEn = true;                       // 활성화
                        //m_aCParam[nMotor].nModelNum = 1060;
                        //m_aCParam[nMotor].nID = nMotor;                     // 모터를 제어하기 위한 가상 아이디. 중복 허용 불가. 보통은 모터 ID 와 일치시킨다.      
                        //m_aCParam[nMotor].nRealID = nMotorRealID;           // 실제 사용할 모터의 아이디
                        //m_aCParam[nMotor].nCommIndex = nCommIndex;          // 연결 된 통신포트 중 어느것에 연결 된 것이지에 대한 순서.

                        //m_aCParam[nMotor].nDir = nDir;
                        m_aCParam[nMotor].fCenterPos = 2048.0f;

                        m_aCParam[nMotor].fMechMove = 4096.0f;
                        m_aCParam[nMotor].fDegree = 360.0f;
                        m_aCParam[nMotor].fRefRpm = 0.229f;                 // 기본 rpm 단위

                        m_aCParam[nMotor].fLimitRpm = 415f;                // 

                        m_aCParam[nMotor].fLimitUp = 0.0f;    // Limit - 0: Ignore
                        m_aCParam[nMotor].fLimitDn = 0.0f;    // Limit - 0: Ignore

                        m_aCParam[nMotor].nProtocol = 2;               // 2, 0 - protocol2, 1 - protocol1, 3 - None                             
                        break;


                    case EMotor_t.XL_320:
                        //m_aCParam[nMotor].EMot = EMot;
                        m_aCParam[nMotor].fCenterPos = 512.0f;

                        m_aCParam[nMotor].fMechMove = 1024.0f;
                        m_aCParam[nMotor].fDegree = 300.0f;
                        m_aCParam[nMotor].fRefRpm = 0.111f;                 // 기본 rpm 단위

                        m_aCParam[nMotor].fLimitRpm = 1023f;                // 

                        m_aCParam[nMotor].fLimitUp = 0.0f;    // Limit - 0: Ignore
                        m_aCParam[nMotor].fLimitDn = 0.0f;    // Limit - 0: Ignore

                        m_aCParam[nMotor].nProtocol = 2;               // 2, 0 - protocol2, 1 - protocol1, 3 - None     
                        break;
                    //case EMotor_t.XM_540:
                    //    break;
                }
            }
            public void SetParam(int nMotor, int nMotorRealID, int nCommIndex, int nDir, EMotor_t EMot)
            {
                //m_anMot_RealID[nMotor] = nMotorRealID; // ID 변경
                //m_anMot_SerialIndex[nMotor] = nCommIndex; // 통신 포트 변경
                m_aCParam[nMotor].nRealID = nMotorRealID; // ID 변경
                m_aCParam[nMotor].nCommIndex = nCommIndex; // 통신 포트 변경
                m_aCParam[nMotor].nDir = nDir;
                //m_aCParam[nMotor].EOperationMode = EOperationMode_t._Position; // Default
                //if (m_aCParam[nMotor].EOperationMode_Prev == EOperationMode_t._None) m_aCParam[nMotor].EOperationMode_Prev = m_aCParam[nMotor].EOperationMode;
                SetAddress(EMot, ref m_aCAddress[nMotor]);

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
                        //m_aCParam[nMotor].bEn = true;                       // 활성화

                        //m_aCParam[nMotor].nID = nMotor;                     // 모터를 제어하기 위한 가상 아이디. 중복 허용 불가. 보통은 모터 ID 와 일치시킨다.      
                        //m_aCParam[nMotor].nRealID = nMotorRealID;           // 실제 사용할 모터의 아이디
                        //m_aCParam[nMotor].nCommIndex = nCommIndex;          // 연결 된 통신포트 중 어느것에 연결 된 것이지에 대한 순서.

                        m_aCParam[nMotor].nDir = nDir;
                        m_aCParam[nMotor].fCenterPos = 512.0f;

                        m_aCParam[nMotor].fMechMove = 1024.0f;
                        m_aCParam[nMotor].fDegree = 300.0f;
                        m_aCParam[nMotor].fRefRpm = 0.111f;                 // 기본 rpm 단위

                        m_aCParam[nMotor].fLimitRpm = 1023f;                // 

                        m_aCParam[nMotor].fLimitUp = 0.0f;    // Limit - 0: Ignore
                        m_aCParam[nMotor].fLimitDn = 0.0f;    // Limit - 0: Ignore

                        m_aCParam[nMotor].nProtocol = 1;               // 2, 0 - protocol2, 1 - protocol1, 3 - None     

                        break;


                    // Protocol2
                    case EMotor_t.NONE:
                    case EMotor_t.XL_430:
                        //m_aCParam[nMotor].bEn = true;                       // 활성화
                        //m_aCParam[nMotor].nModelNum = 1060;
                        //m_aCParam[nMotor].nID = nMotor;                     // 모터를 제어하기 위한 가상 아이디. 중복 허용 불가. 보통은 모터 ID 와 일치시킨다.      
                        //m_aCParam[nMotor].nRealID = nMotorRealID;           // 실제 사용할 모터의 아이디
                        //m_aCParam[nMotor].nCommIndex = nCommIndex;          // 연결 된 통신포트 중 어느것에 연결 된 것이지에 대한 순서.

                        m_aCParam[nMotor].nDir = nDir;
                        m_aCParam[nMotor].fCenterPos = 2048.0f;

                        m_aCParam[nMotor].fMechMove = 4096.0f;
                        m_aCParam[nMotor].fDegree = 360.0f;
                        m_aCParam[nMotor].fRefRpm = 0.229f;                 // 기본 rpm 단위

                        m_aCParam[nMotor].fLimitRpm = 415f;                // 

                        m_aCParam[nMotor].fLimitUp = 0.0f;    // Limit - 0: Ignore
                        m_aCParam[nMotor].fLimitDn = 0.0f;    // Limit - 0: Ignore

                        m_aCParam[nMotor].nProtocol = 2;               // 2, 0 - protocol2, 1 - protocol1, 3 - None                             
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
                        if ((SType.EMot == EMot) && (SType.nCommIndex == nCommIndex) && (SType.nProtocol == m_aSMot_Info[nMotor].nProtocol) && (SType.nTorqAddress == m_aSMot_Info[nMotor].CAddress.nTorq_64_1))
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
                        SType.nTorqAddress = m_aSMot_Info[nMotor].CAddress.nTorq_64_1;
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
#endif
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
                // (0)0xff, (1)0xff, (2)0xfd, (3)0x00,     
                // (4)ID != 0xff 이니 검사할 필요 없다.
                for (int i = 5; i < pBuff.Length; i++)
                {
                    switch (nStuff)
                    {
                        case 0: { if (pBuff[i] == 0xff) nStuff++; } break;
                        case 1: { if (pBuff[i] == 0xff) nStuff++; else nStuff = 0; } break;
                        case 2: { nStuff = 0; if (pBuff[i] == 0xfd) { pnIndex[nCnt++] = i; } } break;
                    }
                }
                if (nCnt > 0)
                {
                    byte[] pBuff2 = (byte [])pBuff.Clone();
                    Array.Resize<byte>(ref pBuff, pBuff2.Length + nCnt);
                    int nIndex = 0;
                    int nPos = 0;
                    int i = 5;
                    // 내부의 패킷길이값 재 설정 
                    pBuff[i++] = (byte)((pBuff.Length - 7) & 0xff);
                    pBuff[i++] = (byte)(((pBuff.Length - 7) >> 8) & 0xff);
                    for (i = 7; i < pBuff.Length; i++)
                    {
                        pBuff[i + nPos] = pBuff2[i];
                        if (i == pnIndex[nPos])
                        {
                            pBuff[nIndex + nPos + 1] = 0xfd;
                            nPos++;
                        }
                    }
                    pBuff2 = null;
                }
                pnIndex = null;
            }
            public void SendPacket(int nIndex_Connection, byte[] buffer, int nLength) 
            {
                // Serial
                if (m_lstConnect[nIndex_Connection].nType == 1)
                {
                    if (m_lstConnect[nIndex_Connection].CSerial.IsConnect() == true) m_lstConnect[nIndex_Connection].CSerial.SendPacket(buffer, nLength);
                }
                // Socket
                else if (m_lstConnect[nIndex_Connection].nType == 2)
                {
                    if (m_lstConnect[nIndex_Connection].CSock.IsConnect() == true) m_lstConnect[nIndex_Connection].CSock.SendPacket(buffer, nLength);
                }
                else                
                {
                }
            }
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
                else // m_aCParam_Axis[nAxis].nProtocol_Version == 2
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
            //private void SetBusy(int nSerialIndex)
            //{
            //    // if(m_lstConnect[nSerialIndex].nRequestData != 0) while();
            //    m_lstConnect[nSerialIndex].nRequestData = 1;
            //}
            //private void WaitBusy() { for (int i = 0; i < m_lstConnect.Count; i++) WaitBusy(i); }
            //private void WaitBusy(int nSerialIndex)
            //{
            //    if (m_lstConnect[nSerialIndex].nRequestData != 0)
            //    {
            //        Ojw.CTimer CTmr = new CTimer();
            //        CTmr.Set();
            //        while (true)
            //        {
            //            if (m_lstConnect[nSerialIndex].nRequestData == 0) 
            //                break;
            //            if (CTmr.Get() >= 1000) 
            //                break;
            //            Ojw.CTimer.Wait(1); //System.Windows.Forms.Application.DoEvents();// Thread.Sleep(1); //System.Windows.Forms.Application.DoEvents();// Thread.Sleep(1);
            //        }
            //        //m_lstConnect[nSerialIndex].nRequestData = 0;
            //    }
            //}
            private int m_nWait = 0;
            public void Delay(int nMilliseconds)
            {
                CTimer CTmr = new CTimer();
                CTmr.Set(); while (CTmr.Get() < nMilliseconds)
                {
                    if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) break;
                    //Ojw.CTimer.Wait(0);
                    DoEvent();
                }
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
                CTmr.Set(); while (CTmr.Get() < nMilliseconds)
                {
                    if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) break;
                    //Ojw.CTimer.Wait(0);

                    DoEvent();
                }
            }
            public void DoEvent()
            {
                System.Windows.Forms.Application.DoEvents();
            }
            private bool m_bAutoset = false;
            private bool m_bPing = false;
            public void Write_Ping(int nProtocol_Version, int nSerialIndex, int nMotor)
            {
                int nID = m_aCParam[nMotor].nRealID;
                byte[] pbyteBuffer = MakePingPacket(nID, nProtocol_Version);
                //WaitBusy(nSerialIndex);
                //SetBusy(nSerialIndex);
                m_bPing = true;
                SendPacket(nSerialIndex, pbyteBuffer, pbyteBuffer.Length);
                //WaitBusy(nSerialIndex);
                m_bPing = false;
            }
            private struct SScanMotors_t
            {
                public int nID;
                public int nCommIndex;

            }
            
            private List<SScanMotors_t> m_lstMotors = new List<SScanMotors_t>();
            
            //private Ojw.CTimer m_CTmrTest = new CTimer();
            private List<long> m_lstlTest = new List<long>();
            public void AutoSet()
            {
                for (int i = 0; i < m_aCMap.Length; i++) { m_aCMap[i].EMot = EMotor_t.NONE; Array.Clear(m_aCMap[i].buffer, 0, m_aCMap[i].buffer.Length); }
                //Array.Clear(m_aCMap, 0, m_aCMap.Length);

                m_bAutoset = true;

                for (int i = 0; i < m_lstConnect.Count; i++) m_lstID[i] = -1;
                //m_lstScanedMotors_ID.Clear();
                //m_lstScanedMotors_Model.Clear();
                //m_lstScanedMotors_CommIndex.Clear();

                Init();
                
                m_lstlTest.Clear();
                //m_CTmrTest.Set();
                for (int i = 0; i < 1; i++)
                {
                    Write_Ping(false);
                    //Reads(0, 0, 120, 
                }
                m_bAutoset = false;
                foreach (long lData in m_lstlTest)
                    Ojw.CMessage.Write("[Tmr] {0}", lData);
                if (m_lstScanedMotors_ID.Count > 0)
                {
                    for (int i = 0; i < m_lstScanedMotors_ID.Count; i++)
                    {
                        int nID = m_lstScanedMotors_ID[i];// m_aCMap[m_lstScanedMotors_ID[i]].GetUShort(0);
                        if (nID < 0)
                        {
                            Ojw.Log("ID Error");
                            continue;
                        }

                        SetParam_CommIndex(nID, m_lstScanedMotors_CommIndex[i]);
                        int nModel = (m_lstScanedMotors_Model[i] == 0) ? m_aCMap[m_lstScanedMotors_ID[i]].GetUShort(0) : m_lstScanedMotors_Model[i];
                        EMotor_t EMot = GetMotorType(nModel);
                        if ((EMot == EMotor_t.XL_430) || (EMot >= EMotor_t.MX_12))
                        {
                            Read_Motor(nID, 0, 120);
                        }
                        else if (EMot == EMotor_t.NONE)
                        {
                            Read_Motor(1, nID, 0, 50);
                        }
                        else Read_Motor(nID, 0, 50);
                        if ((nID < 0) || (nID > 253))
                            continue;
                        EMotor_t EMot2 = GetMotorType((EMot == EMotor_t.NONE) ? (m_aCMap[nID].GetUShort(0)) : nModel);
                        int nModel2 = (EMot == EMotor_t.NONE) ? (m_aCMap[nID].GetUShort(0)) : nModel;
                        SetParam(m_lstScanedMotors_CommIndex[i], nID, EMot2);

                        //SetTorq(false);
                        //SetOperation(EOperation_t._Position, nID); // 포지션 제어로 강제 셋팅
                        if (m_aCParam[nID].nProtocol == 2)
                        {
                            if (EMot2 != EMotor_t.XL_320)
                            {
                                if ((m_aCMap[nID].GetByte(10) & 0x04) != 0)
                                {   
                                    m_aCParam[nID].bTimeControl = true;
                                }
                                else m_aCParam[nID].bTimeControl = false;
                            }
                            else 
                            {
                                m_aCParam[nID].bTimeControl = false;
                                if (m_aCMap[nID].GetByte(11) == 1) m_aCParam[nID].EOperation = EOperation_t._Speed;
                                else if ((m_aCMap[nID].GetByte(11) == 3) || (m_aCMap[nID].GetByte(11) == 2)) m_aCParam[nID].EOperation = EOperation_t._Position; // 2 는 Raw 데이타를 의미(내가 정의한 것)
                                else if (m_aCMap[nID].GetByte(11) == 4) m_aCParam[nID].EOperation = EOperation_t._Position_Multi;
                                else if (m_aCMap[nID].GetByte(11) == 16) m_aCParam[nID].EOperation = EOperation_t._Pwm;
                            }
                        }
                        else m_aCParam[nID].bTimeControl = false;
                        
                        Ojw.CMessage.Write2("Port[{0}] : ID[{1}] ===> {2}[{3}]{4}\r\n",
                            m_lstScanedMotors_CommIndex[i],
                            m_lstScanedMotors_ID[i],
                            EMot2,//GetMotorType((EMot == EMotor_t.NONE) ? (m_aCMap[nID].GetUShort(0)) : nModel), // GetMotorType(nModel),//
                            nModel2,//nModel//GetMotorType(m_lstScanedMotors_Model[i])
                            ((m_aCParam[nID].bTimeControl == true) ? "<Time Control>" : "")
                            );
                    }
                }
                //return;
                ////WaitBusy();

                ////m_bAutoset = false;
#if false
                m_lstMotors.Clear();
                List<SScanMotors_t> lstMotors = new List<SScanMotors_t>();
                lstMotors.Clear();
                if (m_lstScanedMotors_ID.Count > 0)
                {
                    for (int i = 0; i < m_lstScanedMotors_ID.Count; i++)
                    {
                        m_aCParam[m_lstScanedMotors_ID[i]].nCommIndex = m_lstScanedMotors_CommIndex[i];
                        m_aCParam[m_lstScanedMotors_ID[i]].nProtocol = m_lstScanedMotors_Protocol[i];
                        m_aCParam[m_lstScanedMotors_ID[i]].nRealID = m_lstScanedMotors_ID[i];
                        m_aCParam[m_lstScanedMotors_ID[i]].nCommIndex = m_lstScanedMotors_CommIndex[i];
                        if (GetMotorType(m_lstScanedMotors_Model[i]) == EMotor_t.NONE) Read_Motor(m_lstScanedMotors_ID[i], 0, 50);
                        else if (GetMotorType(m_lstScanedMotors_Model[i]) == EMotor_t.XL_320) Read_Motor(m_lstScanedMotors_ID[i], 0, 50);
                        else Read_Motor(m_lstScanedMotors_ID[i], 0, 120);
                        //m_lstScanedMotors_CommIndex[i] == 
                    }
                }
#endif
                ////for (int i = 0; i < m_lstConnect.Count; i++)
                ////{
                ////    for (int j = 0; j < 2; j++)
                ////    {
                ////        if (
                ////        m_lstMotors.Add(
                ////    }
                ////}

                ////if (m_lstScanedMotors_ID.Count > 0)
                ////{
                ////    for (int i = 0; i < m_lstScanedMotors_ID.Count; i++)
                ////    {
                ////        //Reads(
                ////    }
                ////}
                //WaitBusy();
                //if (m_lstScanedMotors_ID.Count > 0)
                //{
                //    for (int i = 0; i < m_lstScanedMotors_ID.Count; i++)
                //    {
                //        int nID = m_aCMap[m_lstScanedMotors_ID[i]].GetUShort(0);
                //        if ((nID < 0) || (nID > 253))
                //            continue;
                //        Ojw.CMessage.Write2("Port[{0}] : ID[{1}] ===> {2}\r\n",
                //            m_lstScanedMotors_CommIndex[i],
                //            m_lstScanedMotors_ID[i],
                //            GetMotorType(m_lstScanedMotors_Model[i])//GetMotorType(m_lstScanedMotors_Model[i])
                //            );
                //    }
                //}

            }
            //private bool m_bScanning = false;
            //public void ScanMotors()
            //{
            //    m_bScanning = true;
            //    m_lstScanedMotors.Clear();

            //    for (int i = 0; i < m_lstConnect.Count; i++)
            //    {
            //        Write_Ping(2, i, );
            //    }

            //    m_bScanning = false;
            //}

            public int[] GetScanIDs() { return m_lstScanedMotors_ID.ToArray(); }

            public void Write_Ping() { Write_Ping(true); }
            public void Write_Ping(bool bShow)
            {
                int nCount_AllMotors = 254;
#if false
                //m_bShowReceivedIDs = bShow;
                m_bPing = bShow;
                for (int nIndex = 0; nIndex < m_lstConnect.Count; nIndex++)
                {
                    WaitBusy(nIndex);
                    for (int i = 0; i < 2; i++)
                    {
                        int nID = 254;
                        byte[] pbyteBuffer = MakePingPacket(nID, 2 - i);
                        //m_CTmr_AutoSet.Set();
                        SetBusy(nIndex);
                        SendPacket(nIndex, pbyteBuffer, pbyteBuffer.Length);
                        WaitBusy(nIndex);
                        //Ojw.CTimer.Wait(100); // 나중에 handshake 로 바꿀것.
                    }
                }
                m_bPing = false;
#else

                m_bPing = bShow;
                int nID = 254;
                for (int nIndex = 0; nIndex < m_lstConnect.Count; nIndex++)
                {
                    for (int i = 0; i < 2; i++) // Protocol 1, 2
                    {
                        //int i = 2;
                        byte[] pbyteBuffer = MakePingPacket(nID, 2 - i);
                        SendPacket(nIndex, pbyteBuffer, pbyteBuffer.Length);
#if false
                        if (Receiving(nIndex, 200, nCount_AllMotors, null) > 0) // 데이타가 남아 있다면
                        {
                            Ojw.CTimer CTmr = new CTimer();
                            CTmr.Set();
                            while (Receiving(nIndex, 200, nCount_AllMotors, m_lstBuffers[nCommIndex]) > 0)
                            {
                                if (CTmr.Get() > 5000) break;
                                Thread.Sleep(1);
                            }
                            //Ojw.CMessage.Write("It still has datas..."); ;
                        }
#else
                        Received(nIndex, 500, nCount_AllMotors);
#endif
                    }
                }
                m_bPing = false;
#endif                
            }
            public void Write_Command(int nMotor, int nCommand) { Write_Command(m_aCParam[nMotor].nProtocol, m_aCParam[nMotor].nCommIndex, nMotor, nCommand); }
            public void Write_Command(int nProtocol_Version, int nSerialIndex, int nMotor, int nCommand)
            {
                int i;
                //int nID = ((nAxis == 254) ? 254 : m_aSMot[nAxis].nID);
                if (nProtocol_Version == 1)//if (m_aCParam[nMotor].nProtocol == 1)
                {
                    int nLength = 2;// 1 + 2;
                    int nDefaultSize = 6;
                    byte[] pbyteBuffer = new byte[nDefaultSize + nLength];
                    i = 0;
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = (byte)(m_aCParam[nMotor].nRealID & 0xff);
                    pbyteBuffer[i++] = (byte)(nLength & 0xff);
                    pbyteBuffer[i++] = (byte)(nCommand & 0xff);

                    int nCrc = 0;
                    for (int j = 2; j < pbyteBuffer.Length - 1; j++) nCrc += pbyteBuffer[j];
                    pbyteBuffer[i++] = (byte)(~nCrc & 0xff);

                    pbyteBuffer[pbyteBuffer.Length - 1] = (byte)(nCrc & 0xff);

                    SendPacket(nSerialIndex, pbyteBuffer, pbyteBuffer.Length);
                }
                else // if (m_aCParam_Axis[nAxis].nProtocol_Version == 2)
                {
                    int nLength = 3;
                    int nDefaultSize = 7;
                    byte[] pbyteBuffer = new byte[nDefaultSize + nLength];
                    i = 0;
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = 0xfd;
                    pbyteBuffer[i++] = 0x00;
                    pbyteBuffer[i++] = (byte)(m_aCParam[nMotor].nRealID & 0xff);
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
            public void Write_One(int nMotor, int nCommand, int nData) { Write_One(m_aCParam[nMotor].nProtocol, m_aCParam[nMotor].nCommIndex, nMotor, nCommand, nData); }
            public void Write_One(int nProtocol_Version, int nSerialIndex, int nMotor, int nCommand, int nData)
            {
                int i;
                //int nID = ((nAxis == 254) ? 254 : m_aSMot[nAxis].nID);
                if (nProtocol_Version == 1)//if (m_aCParam[nMotor].nProtocol == 1)
                {
                    int nLength = 1 + 2;// +1; // 
                    int nDefaultSize = 6;
                    byte[] pbyteBuffer = new byte[nDefaultSize + nLength];
                    i = 0;
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = (byte)(m_aCParam[nMotor].nRealID & 0xff);
                    pbyteBuffer[i++] = (byte)(nLength & 0xff);
                    pbyteBuffer[i++] = (byte)(nCommand & 0xff);
                    pbyteBuffer[i++] = (byte)(nData & 0xff);

                    int nCrc = 0;
                    for (int j = 2; j < pbyteBuffer.Length - 1; j++) nCrc += pbyteBuffer[j];
                    pbyteBuffer[i++] = (byte)(~nCrc & 0xff);

                    pbyteBuffer[pbyteBuffer.Length - 1] = (byte)(nCrc & 0xff);

                    SendPacket(nSerialIndex, pbyteBuffer, pbyteBuffer.Length);
                }
                else // if (m_aCParam_Axis[nAxis].nProtocol_Version == 2)
                {
                    int nLength = 4;
                    int nDefaultSize = 7;
                    byte[] pbyteBuffer = new byte[nDefaultSize + nLength];
                    i = 0;
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = 0xff;
                    pbyteBuffer[i++] = 0xfd;
                    pbyteBuffer[i++] = 0x00;
                    pbyteBuffer[i++] = (byte)(m_aCParam[nMotor].nRealID & 0xff);
                    pbyteBuffer[i++] = (byte)(nLength & 0xff);
                    pbyteBuffer[i++] = (byte)((nLength >> 8) & 0xff);
                    pbyteBuffer[i++] = (byte)(nCommand & 0xff);
                    pbyteBuffer[i++] = (byte)(nData & 0xff);

                    MakeStuff(ref pbyteBuffer);
                    int nCrc = updateCRC(pbyteBuffer, pbyteBuffer.Length - 2);
                    pbyteBuffer[pbyteBuffer.Length - 2] = (byte)(nCrc & 0xff);
                    pbyteBuffer[pbyteBuffer.Length - 1] = (byte)((nCrc >> 8) & 0xff);

                    SendPacket(nSerialIndex, pbyteBuffer, pbyteBuffer.Length);
                }
            }
            public void Write(int nMotor, int nCommand, int nAddress, params byte[] pbyDatas) { Write2(m_aCParam[nMotor].nProtocol, m_aCParam[nMotor].nCommIndex, nMotor, nCommand, nAddress, pbyDatas); }
            public void Write2(int nProtocol_Version, int nSerialIndex, int nMotorRealID, int nCommand, int nAddress, params byte[] pbyDatas)
            {
                int i;

                //int nID = 0;//((nAxis == 254) ? 254 : m_aSMot[nAxis].nID);

                i = 0;
                if (nProtocol_Version == 1)
                {
                    //int nLength = 2 + ((pbyDatas != null) ? pbyDatas.Length : 0);
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
                //else if (nProtocol_Version == 0) // XL-320
                //{
                //    int nLength = 3 + 2 + pbyDatas.Length;
                //    int nDefaultSize = 7;
                //    byte[] pbyteBuffer = new byte[nDefaultSize + nLength];
                //    pbyteBuffer[i++] = 0xff;
                //    pbyteBuffer[i++] = 0xff;
                //    #region Packet 2.0
                //    pbyteBuffer[i++] = 0xfd;
                //    pbyteBuffer[i++] = 0x00;
                //    #endregion Packet 2.0
                //    pbyteBuffer[i++] = (byte)(nMotorRealID & 0xff);
                //    pbyteBuffer[i++] = (byte)(nLength & 0xff);
                //    pbyteBuffer[i++] = (byte)((nLength >> 8) & 0xff);
                //    pbyteBuffer[i++] = (byte)(nCommand & 0xff);
                //    pbyteBuffer[i++] = (byte)(nAddress & 0xff);
                //    pbyteBuffer[i++] = (byte)((nAddress >> 8) & 0xff);
                //    if (pbyDatas != null)
                //        foreach (byte byData in pbyDatas) pbyteBuffer[i++] = byData;

                //    MakeStuff(ref pbyteBuffer);
                //    int nCrc = updateCRC(pbyteBuffer, pbyteBuffer.Length - 2);
                //    pbyteBuffer[pbyteBuffer.Length - 2] = (byte)(nCrc & 0xff);
                //    pbyteBuffer[pbyteBuffer.Length - 1] = (byte)((nCrc >> 8) & 0xff);

                //    SendPacket(nSerialIndex, pbyteBuffer, pbyteBuffer.Length);
                //}
                else // m_aCParam_Axis[nAxis].nProtocol_Version == 2
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
            public class CBulkWriteAll_t
            {
                private List<CBulkWrite_t> lstCBulk = null;
                public int Add(int nID, int nAddress, int nLength, byte [] pbyteData)
                {
                    CBulkWrite_t CBulk = new CBulkWrite_t();
                    CBulk.nID = nID;
                    CBulk.nAddress = nAddress;
                    CBulk.nLength = nLength;
                    CBulk.buffer = (byte [])pbyteData.Clone();
                    return lstCBulk.Count;
                }
                public byte [] Pop()
                {
                    int nLength = 0;
                    foreach(CBulkWrite_t CBulk in lstCBulk) nLength += CBulk.nLength + 5;
                    byte [] pbyteReturn = new byte[nLength];
                    int i = 0;
                    byte [] pbyTmp = new byte[2];
                    foreach(CBulkWrite_t CBulk in lstCBulk) 
                    {
                        pbyteReturn[i++] = (byte)CBulk.nID;
                        pbyTmp = Ojw.CConvert.ShortToBytes((short)CBulk.nAddress);
                        Array.Copy(pbyTmp, 0, pbyteReturn, i, sizeof(short)); i += sizeof(short);
                        pbyTmp = Ojw.CConvert.ShortToBytes((short)CBulk.nLength);
                        Array.Copy(pbyTmp, 0, pbyteReturn, i, sizeof(short)); i += sizeof(short);
                        Array.Copy(CBulk.buffer, 0, pbyteReturn, i, CBulk.nLength); i += CBulk.nLength;
                    }
                    lstCBulk.Clear();
                    return pbyteReturn;
                }
            }
            private CBulkWriteAll_t m_CBulk = new CBulkWriteAll_t();
            private class CBulkWrite_t
            {
                public int nID;
                public int nAddress;
                public int nLength;
                public byte [] buffer;
                public byte [] GetPackets()
                {
                    byte[] pbyteBuffer = new byte[5 + nLength];
                    byte[] pbyTmp;
                    int i = 0;
                    pbyteBuffer[i++] = (byte)(nID & 0xff); // ID

                    pbyTmp = Ojw.CConvert.ShortToBytes((short)nAddress);
                    pbyteBuffer[i++] = pbyTmp[0];
                    pbyteBuffer[i++] = pbyTmp[1];
                    
                    pbyTmp = Ojw.CConvert.ShortToBytes((short)nLength);
                    pbyteBuffer[i++] = pbyTmp[0];
                    pbyteBuffer[i++] = pbyTmp[1];

                    Array.Copy(buffer, 0, pbyteBuffer, i, nLength);
                    
                    return pbyteBuffer;
                }
            }
            public void Push_Bulk(int nID, int nAddress, params byte [] abyData)
            {
                m_CBulk.Add(nID, nAddress, abyData.Length, abyData);
            }
            public void Writes_Bulk(int nSerialIndex, CBulkWriteAll_t CBulkAll) // 이건 protocol2 만 된다.
            {
                byte[] pbyteDatas = CBulkAll.Pop();
                int nLength = pbyteDatas.Length + 3;
                
                int i = 0;
                //int nLength = 3 + 2 + pbyDatas.Length;
                int nDefaultSize = 7;
                int nCommand = 0x93;
                byte[] pbyteBuffer = new byte[nDefaultSize + nLength];
                pbyteBuffer[i++] = 0xff;
                pbyteBuffer[i++] = 0xff;
                #region Packet 2.0
                pbyteBuffer[i++] = 0xfd;
                pbyteBuffer[i++] = 0x00;
                #endregion Packet 2.0
                pbyteBuffer[i++] = (byte)(0xfe);
                pbyteBuffer[i++] = (byte)(nLength & 0xff);
                pbyteBuffer[i++] = (byte)((nLength >> 8) & 0xff);
                pbyteBuffer[i++] = (byte)(nCommand & 0xff);
                if (pbyteDatas != null)
                    foreach (byte byData in pbyteDatas) pbyteBuffer[i++] = byData;

                MakeStuff(ref pbyteBuffer);
                int nCrc = updateCRC(pbyteBuffer, pbyteBuffer.Length - 2);
                pbyteBuffer[pbyteBuffer.Length - 2] = (byte)(nCrc & 0xff);
                pbyteBuffer[pbyteBuffer.Length - 1] = (byte)((nCrc >> 8) & 0xff);

                SendPacket(nSerialIndex, pbyteBuffer, pbyteBuffer.Length);
                pbyteBuffer = null;
                pbyteDatas = null;
            }
            public void Reads(params int[] pnIDs)
            {
                //m_lstConnect[0].bProtocol1
                List<int> lstID_1 = new List<int>();
                List<int> lstID_2 = new List<int>();
                List<int> lstID_0 = new List<int>(); // xl320
                lstID_0.Clear();
                lstID_1.Clear();
                lstID_2.Clear();
                //int [] anAddress = new int[3];
                //Array.Clear(anAddress, 0, anAddress.Length);
                for (int nComm = 0; nComm < m_lstConnect.Count; nComm++)
                {
                    foreach (int nID in pnIDs)
                    {
                        if (m_aCParam[nID].nCommIndex == nComm)
                        {
                            if (m_aCParam[nID].EMot == EMotor_t.XL_320) lstID_0.Add(nID); //{ lstID_0.Add(nID); anAddress[0] = m_aCParam[nID].CAddress.nPos_Speed
                            else if (m_aCParam[nID].nProtocol == 1)     lstID_1.Add(nID);
                            else if (m_aCParam[nID].nProtocol == 2)     lstID_2.Add(nID);
                        }
                    }
                    if (lstID_0.Count > 0) Reads_Packet(nComm, m_aCParam[lstID_0[0]].CAddress.nGetSimple, m_aCParam[lstID_0[0]].CAddress.nGetSimple_Size, lstID_0.ToArray());
                    if (lstID_1.Count > 0) Reads_Packet(nComm, m_aCParam[lstID_1[0]].CAddress.nGetSimple, m_aCParam[lstID_1[0]].CAddress.nGetSimple_Size, lstID_1.ToArray());
                    if (lstID_2.Count > 0) Reads_Packet(nComm, m_aCParam[lstID_2[0]].CAddress.nGetSimple, m_aCParam[lstID_2[0]].CAddress.nGetSimple_Size, lstID_2.ToArray());
                    //if (
                }
                //Reads(nSerialIndex, nAddress, nDataLength, pnIDs);
            }
            public void Reads_Packet(int nSerialIndex, int nAddress, int nDataLength, params byte[] pbyIDs)
            {
                //WaitBusy(nSerialIndex);
                //SetBusy(nSerialIndex);
                int nProtocol_Version = 2; // 프로토콜 2 버전부터 싱크리드가 지원
                byte[] pbyteBuffer = new byte[2 + pbyIDs.Length];
                foreach (byte byID in pbyIDs) m_aCMap[byID].nRunningAddress = nAddress;
                Array.Copy(Ojw.CConvert.ShortToBytes((short)nDataLength), pbyteBuffer, 2);
                Array.Copy(pbyIDs, 0, pbyteBuffer, 2, pbyIDs.Length);
                Write2(nProtocol_Version, nSerialIndex, 0xfe, 0x82, nAddress, pbyteBuffer);
                Received(nSerialIndex, 200, pbyIDs.Length);
                //WaitBusy(nSerialIndex);
            }
            public void Reads_Packet(int nSerialIndex, int nAddress, int nDataLength, params int[] pnIDs)
            {
                //WaitBusy(nSerialIndex);
                //SetBusy(nSerialIndex);
                int nProtocol_Version = 2; // 프로토콜 2 버전부터 싱크리드가 지원
                byte[] pbyteBuffer = new byte[2 + pnIDs.Length];
                int i = 2;
                foreach (int nID in pnIDs) m_aCMap[nID].nRunningAddress = nAddress;
                Array.Copy(Ojw.CConvert.ShortToBytes((short)nDataLength), pbyteBuffer, i);
                foreach (int nData in pnIDs) pbyteBuffer[i++] = (byte)(nData & 0xff);
                Write2(nProtocol_Version, nSerialIndex, 0xfe, 0x82, nAddress, pbyteBuffer);
                //WaitBusy(nSerialIndex);
                Received(nSerialIndex, 200, pnIDs.Length);
            }
            #endregion Packet_Raw

            #region Control
            //public void SetParam(int nMotor, EMotor_t EMot)
            //{
            //    m_aCParam[nMotor].EMot = EMot;
            //}
                #region Torq
                
                //public void SetTorq(int nMotor, bool bOn)
                //{
                //    //m_a
                //    if (bOn) { if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return; }
                //    if (nMotor < 
                //    //if (bOn == false) m_aSMot[nMotor].bInit_Value = false;

                //    //m_aSMot[nMotor].nStatus_Torq_Prev = m_aSMot[nMotor].nStatus_Torq;
                //    //m_aSMot[nMotor].nStatus_Torq = Ojw.CConvert.BoolToInt(bOn);

                //    if (m_aCParam[nMotor].EMot != EMotor_t.SG_90)
                //    {
            //        Write2(m_aCParam[nMotor].nProtocol, m_aCParam[nMotor].nCommIndex, ((nMotor < 0) ? 254 : (m_aCParam[nMotor].nRealID)), 0x03, m_aCParam[nMotor].CAddress.nTorq, (byte)((bOn == true) ? 1 : 0));

                //    }
                //}

                private const int _ADDRESS_TORQ_XL_320 = 24;
                private const int _ADDRESS_TORQ_XL_430 = 64;
                private const int _ADDRESS_TORQ_AX = 24;
            
                private class CStatus_t
                {
                    public CStatus_t()
                    {
                    }
                    ~CStatus_t()
                    {
                    }
                    public bool bTimeControl = false;
                    public bool bTorq = false;
                    public EOperation_t EOperation = EOperation_t._None;
                    public float fEvd_Dest = 0.0f;
                    //public float fEvd_

                }
                public void SetTimeControl(bool bTimeControl)
                {
                    //for (int i = 0; i < m_aCParam.Length; i++) m_aCMot[i].bInit_Value = false;

                    for (int nIndex = 0; nIndex < m_lstConnect.Count; nIndex++)
                    {
                        if (m_lstConnect[nIndex].bProtocol2 == true)
                        {
                            Write2(2, nIndex, 254, 0x03, _ADDR_2_Set_Mode_Drive_10_1, (byte)((bTimeControl == true) ? 4 : 0));
                        }
                    }
                    for (int i = 0; i < 254; i++) m_aCParam[i].bTimeControl = bTimeControl;
                }
                public void SetTimeControl(bool bTimeControl, params int[] anMotors)
                {
                    if (anMotors != null)
                    {
                        List<byte>[] alstBuffer = new List<byte>[m_lstConnect.Count * 2];
                        for (int nComm = 0; nComm < m_lstConnect.Count; nComm++)
                        {
                            int nProt1 = 0;// nComm * 2;
                            int nProt2 = 1;// nComm * 2 + 1;
                            alstBuffer[nProt1] = new List<byte>();
                            alstBuffer[nProt2] = new List<byte>();
                            byte byVal = (byte)(bTimeControl == true ? 4 : 0);

                            //int nAddress1 = 0;
                            int nAddress2 = 0;
                            //int nAddress1_Size = 0;
                            int nAddress2_Size = 0;
                            //int nCount_Motor1 = 0;
                            int nCount_Motor2 = 0;

                            foreach (int nMotor in anMotors)
                            {
                                if (m_aCParam[nMotor].nCommIndex == nComm)
                                {
                                    if (m_aCParam[nMotor].nProtocol == 2)
                                    {
                                        nAddress2 = m_aCParam[nMotor].CAddress.nOperatingMode - 1;
                                        nAddress2_Size = m_aCParam[nMotor].CAddress.nOperatingMode_Size;

                                        alstBuffer[nProt2].Add((byte)(m_aCParam[nMotor].nRealID & 0xff));
                                        alstBuffer[nProt2].Add(byVal);

                                        m_aCParam[nMotor].bTimeControl = bTimeControl;
                                        //m_aCPush_Prev[nMotor].bTimeControl = bTimeControl;

                                        nCount_Motor2++;
                                    }
                                    else if (m_aCParam[nMotor].nProtocol == 1)
                                    {
                                        

                                        //nCount_Motor1++;
                                    }
                                }
                            }
                            //if (alstBuffer[nProt1].Count > 0)
                            //{
                                //Writes(1, nComm, nAddress1, nAddress1_Size, nCount_Motor1, alstBuffer[nProt1].ToArray());
                            //}
                            //else 
                            if (alstBuffer[nProt2].Count > 0)
                            {
                                Writes(2, nComm, nAddress2, nAddress2_Size, nCount_Motor2, alstBuffer[nProt2].ToArray());
                            }
                        }
                    }
                }
                public void SetOperation(EOperation_t EOperation, params int[] anMotors)
                {
                    if (anMotors != null)
                    {
                        List<byte> [] alstBuffer = new List<byte>[m_lstConnect.Count * 2];
                        for (int nComm = 0; nComm < m_lstConnect.Count; nComm++)
                        {
                            int nProt1 = 0;// nComm * 2;
                            int nProt2 = 1;// nComm * 2 + 1;
                            alstBuffer[nProt1] = new List<byte>();
                            alstBuffer[nProt2] = new List<byte>();
                            byte byVal = (byte)((int)(EOperation) & 0xff);

                            int nAddress1 = 0;
                            int nAddress2 = 0;
                            int nAddress1_Size = 0;
                            int nAddress2_Size = 0;
                            int nCount_Motor1 = 0;
                            int nCount_Motor2 = 0;

                            foreach (int nMotor in anMotors)
                            {
                                if (m_aCParam[nMotor].nCommIndex == nComm)
                                {
                                    if (m_aCParam[nMotor].nProtocol == 2)
                                    {
                                        nAddress2 = m_aCParam[nMotor].CAddress.nOperatingMode;
                                        nAddress2_Size = m_aCParam[nMotor].CAddress.nOperatingMode_Size;

                                        alstBuffer[nProt2].Add((byte)(m_aCParam[nMotor].nRealID & 0xff));
                                        alstBuffer[nProt2].Add(byVal);

                                        m_aCParam[nMotor].EOperation = EOperation;
                                        m_aCPush_Prev[nMotor].EOperation = EOperation;

                                        nCount_Motor2++;
                                    }
                                    else if (m_aCParam[nMotor].nProtocol == 1)
                                    {
                                        nAddress1 = m_aCParam[nMotor].CAddress.nOperatingMode;
                                        nAddress1_Size = m_aCParam[nMotor].CAddress.nOperatingMode_Size;
                                        //nAddress1 = 6; // 6,7,8,9  ([0, 0, 0, 0 : wheel], [0,0,255,3:Pos])
                                        //nAddress1_Size = 4;

                                        alstBuffer[nProt1].Add((byte)(m_aCParam[nMotor].nRealID & 0xff));
                                        if (EOperation == EOperation_t._Speed)
                                        {
                                            alstBuffer[nProt1].Add(0);
                                            alstBuffer[nProt1].Add(0);
                                            alstBuffer[nProt1].Add(0);
                                            alstBuffer[nProt1].Add(0);
                                        }
                                        else
                                        {
                                            alstBuffer[nProt1].Add(0);
                                            alstBuffer[nProt1].Add(0);
                                            alstBuffer[nProt1].Add(255);
                                            alstBuffer[nProt1].Add(3);
                                        }

                                        m_aCParam[nMotor].EOperation = EOperation;
                                        m_aCPush_Prev[nMotor].EOperation = EOperation;

                                        nCount_Motor1++;
                                    }
                                }
                            }
                            if (alstBuffer[nProt1].Count > 0)
                            {
                                Writes(1, nComm, nAddress1, nAddress1_Size, nCount_Motor1, alstBuffer[nProt1].ToArray());
                            }
                            else if (alstBuffer[nProt2].Count > 0)
                            {
                                Writes(2, nComm, nAddress2, nAddress2_Size, nCount_Motor2, alstBuffer[nProt2].ToArray());
                            }
                        }

                        //////    // protocol 1
                        //////    // protocol 2
                        //////    //byte[] pbyteBuffer = new byte[anMotors.Length * 2];
                        //////    //int i = 0;
                        //////    //byte byVal = (byte)((int)(EOperation) & 0xff);
                        //////    //foreach (int nMotor in anMotors)
                        //////    //{
                        //////    //    pbyteBuffer[i++] = (byte)(m_aCParam[nMotor].nRealID & 0xff);
                        //////    //    pbyteBuffer[i++] = byVal;
                        //////    //    m_aCParam[nMotor].EOperation = EOperation;

                        //////    //    m_aCPush_Prev[nMotor].EOperation = EOperation;
                        //////    //}
                        //////    //Writes(m_aCParam[anMotors[0]].nProtocol, m_aCParam[anMotors[0]].nCommIndex, m_aCParam[anMotors[0]].CAddress.nOperatingMode, m_aCParam[anMotors[0]].CAddress.nOperatingMode_Size, 1, pbyteBuffer);

                        //////    for (int nProtocol = 1; nProtocol <= 2; nProtocol++)
                        //////    {
                        //////        //if ((m_lstConnect[nComm].bProtocol1 == false) && (nProtocol == 1)) continue;
                        //////        //if ((m_lstConnect[nComm].bProtocol2 == false) && (nProtocol == 2)) continue;

                        //////        if ((nProtocol == 1) && (EOperation == EOperation_t._Speed))
                        //////        {

                        //////        }
                        //////        else
                        //////        {
                        //////            byte[] pbyteBuffer = new byte[anMotors.Length * 2];
                        //////            int i = 0;
                        //////            byte byVal = (byte)((int)(EOperation) & 0xff);
                        //////            foreach (int nMotor in anMotors)
                        //////            {
                        //////                pbyteBuffer[i++] = (byte)(m_aCParam[nMotor].nRealID & 0xff);
                        //////                pbyteBuffer[i++] = byVal;
                        //////                m_aCParam[nMotor].EOperation = EOperation;

                        //////                m_aCPush_Prev[nMotor].EOperation = EOperation;
                        //////            }
                        //////        }
                        //////        Writes(nProtocol, nComm, m_aCParam[nMotor].CAddress.nOperatingMode, m_aCParam[nMotor].CAddress.nOperatingMode_Size, 1, pbyteBuffer);
                        //////    }
                        //////}

                        //byte[] pbyteBuffer = new byte[anMotors.Length * 2];
                        //int i = 0;
                        //byte byVal = (byte)((int)(EOperation) & 0xff);
                        //foreach (int nMotor in anMotors)
                        //{
                        //    pbyteBuffer[i++] = (byte)(m_aCParam[nMotor].nRealID & 0xff);
                        //    pbyteBuffer[i++] = byVal;
                        //    m_aCParam[nMotor].EOperation = EOperation;

                        //    m_aCPush_Prev[nMotor].EOperation = EOperation;
                        //}
                        //Writes(m_aCParam[anMotors[0]].nProtocol, m_aCParam[anMotors[0]].nCommIndex, m_aCParam[anMotors[0]].CAddress.nOperatingMode, m_aCParam[anMotors[0]].CAddress.nOperatingMode_Size, 1, pbyteBuffer);                        
                    }
                }
                //public void SetSpeed(EOperation_t EOperation, params int[] anMotors)
                //{
                //    if (anMotors != null)
                //    {
                //        byte[] pbyteBuffer = new byte[anMotors.Length * 2];
                //        int i = 0;
                //        byte byVal = (byte)((int)(EOperation) & 0xff);
                //        foreach (int nMotor in anMotors)
                //        {
                //            pbyteBuffer[i++] = (byte)(m_aCParam[nMotor].nRealID & 0xff);
                //            pbyteBuffer[i++] = byVal;
                //        }
                //        Writes(m_aCParam[anMotors[0]].nProtocol, m_aCParam[anMotors[0]].nCommIndex, m_aCParam[anMotors[0]].CAddress.nOperatingMode, m_aCParam[anMotors[0]].CAddress.nOperatingMode_Size, 1, pbyteBuffer);
                //    }
                //}
                private void Clear_Multirutn_Packet(int nSerialIndex, int nMotor)
                {
                    int nID = (nMotor == 254) ? 254 : m_aCParam[nMotor].nRealID;
                    //int nSerialIndex = 
                    int nProtocol2 = ((nMotor == 254) ? ((m_lstConnect[nSerialIndex].bProtocol2 == true) ? 2 : 1) : m_aCParam[nMotor].nProtocol);
                    if (nProtocol2 == 2)
                    {
                        int nMotorRealID = nID;
                        //byte [] buffer = new byte[8];
                        //SendPacket(i, 
                        int nLength = 8;
                        int nDefaultSize = 7;
                        byte[] pbyteBuffer = new byte[nDefaultSize + nLength];
                        int i = 0;
                        pbyteBuffer[i++] = 0xff;
                        pbyteBuffer[i++] = 0xff;
                        #region Packet 2.0
                        pbyteBuffer[i++] = 0xfd;
                        pbyteBuffer[i++] = 0x00;
                        #endregion Packet 2.0
                        pbyteBuffer[i++] = (byte)(nMotorRealID & 0xff);
                        pbyteBuffer[i++] = (byte)(nLength & 0xff);
                        pbyteBuffer[i++] = (byte)((nLength >> 8) & 0xff);
                        pbyteBuffer[i++] = (byte)(0x10 & 0xff); // Command
                        // Magic Code
                        pbyteBuffer[i++] = (byte)(0x01 & 0xff);
                        pbyteBuffer[i++] = (byte)(0x44 & 0xff);
                        pbyteBuffer[i++] = (byte)(0x58 & 0xff);
                        pbyteBuffer[i++] = (byte)(0x4c & 0xff);
                        pbyteBuffer[i++] = (byte)(0x22 & 0xff);

                        MakeStuff(ref pbyteBuffer);
                        int nCrc = updateCRC(pbyteBuffer, pbyteBuffer.Length - 2);
                        pbyteBuffer[pbyteBuffer.Length - 2] = (byte)(nCrc & 0xff);
                        pbyteBuffer[pbyteBuffer.Length - 1] = (byte)((nCrc >> 8) & 0xff);

                        SendPacket(nSerialIndex, pbyteBuffer, pbyteBuffer.Length);
                    }
                }
                public void Clear_Multiturn()
                {
                    for (int i = 0; i < m_lstConnect.Count; i++)
                    {                        
                        Clear_Multirutn_Packet(i, 0xfe);
                    }
                }

                //public void SetOperation
                public void SetTorq(bool bOn)
                {
                    SetTorq(254, bOn);
                }
                public void SetTorq(int nID, bool bOn)
                {
                    if (bOn) { if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return; }
                    //for (int i = 0; i < m_aCParam.Length; i++) m_aCMot[i].bInit_Value = false;

                    for (int nIndex = 0; nIndex < m_lstConnect.Count; nIndex++)
                    {
#if false
                        if (m_lstConnect[nIndex].bProtocol1 == true)
                        {
                            // protocol 1(AX)
                            Write2(1, nIndex, nID, 0x03, _ADDRESS_TORQ_AX, (byte)((bOn == true) ? 1 : 0));
                        }
                        else
                        {
                            if (m_lstConnect[nIndex].bXL320 == true)
                            {
                                // protocol 2(XL-430
                                Write2(2, nIndex, nID, 0x03, _ADDRESS_TORQ_XL_320, (byte)((bOn == true) ? 1 : 0));
                            }
                            else
                            {
                                // protocol 2(XL-430
                                Write2(2, nIndex, nID, 0x03, _ADDRESS_TORQ_XL_430, (byte)((bOn == true) ? 1 : 0));
                            }
                        }
#else
                        if (m_lstConnect[nIndex].bProtocol2 == true)
                        {
                            // protocol 1(AX)
                            Write2(2, nIndex, nID, 0x03, _ADDRESS_TORQ_XL_430, (byte)((bOn == true) ? 1 : 0));
                        }
                        if (m_lstConnect[nIndex].bXL320 == true) 
                        {
                            // protocol 1(AX)
                            Write2(2, nIndex, nID, 0x03, _ADDRESS_TORQ_XL_320, (byte)((bOn == true) ? 1 : 0));
                        }
                        if (m_lstConnect[nIndex].bProtocol1 == true)
                        {
                            // protocol 1(AX)
                            Write2(1, nIndex, nID, 0x03, _ADDRESS_TORQ_AX, (byte)((bOn == true) ? 1 : 0));
                        }                       
#endif
                    }
                    if ((nID <= 253) && (nID >= 0))
                    {
                        int[] anMotors = new int[1]; // => 나중 확장을 위해 이렇게 해 두었다. 현재는 불필요 자원 낭비.
                        anMotors[0] = nID;
                        
                        List<byte>[] alstBuffer = new List<byte>[m_lstConnect.Count * 2];
                        for (int nComm = 0; nComm < m_lstConnect.Count; nComm++)
                        {
                            int nProt1 = 0;// nComm * 2;
                            int nProt2 = 1;// nComm * 2 + 1;
                            alstBuffer[nProt1] = new List<byte>();
                            alstBuffer[nProt2] = new List<byte>();
                            byte byVal = (byte)((bOn == false) ? 0 : 1);

                            int nAddress1 = 0;
                            int nAddress2 = 0;
                            int nAddress1_Size = 0;
                            int nAddress2_Size = 0;
                            int nCount_Motor1 = 0;
                            int nCount_Motor2 = 0;

                            foreach (int nMotor in anMotors)
                            {
                                if (m_aCParam[nMotor].nCommIndex == nComm)
                                {
                                    if (m_aCParam[nMotor].nProtocol == 2)
                                    {
                                        nAddress2 = m_aCParam[nMotor].CAddress.nTorq;
                                        nAddress2_Size = m_aCParam[nMotor].CAddress.nTorq_Size;

                                        alstBuffer[nProt2].Add((byte)(m_aCParam[nMotor].nRealID & 0xff));
                                        alstBuffer[nProt2].Add(byVal);

                                        nCount_Motor2++;
                                        m_aCParam[nMotor].IsTorq = bOn;
                                    }
                                    else if (m_aCParam[nMotor].nProtocol == 1)
                                    {
                                        nAddress1 = m_aCParam[nMotor].CAddress.nTorq;
                                        nAddress1_Size = m_aCParam[nMotor].CAddress.nTorq_Size;

                                        alstBuffer[nProt1].Add((byte)(m_aCParam[nMotor].nRealID & 0xff));
                                        alstBuffer[nProt1].Add(byVal);

                                        m_aCParam[nID].IsTorq = bOn;

                                        nCount_Motor1++;
                                        m_aCParam[nMotor].IsTorq = bOn;
                                    }
                                }
                            }
                            if (alstBuffer[nProt1].Count > 0)
                            {
                                Writes(1, nComm, nAddress1, nAddress1_Size, nCount_Motor1, alstBuffer[nProt1].ToArray());
                            }
                            else if (alstBuffer[nProt2].Count > 0)
                            {
                                Writes(2, nComm, nAddress2, nAddress2_Size, nCount_Motor2, alstBuffer[nProt2].ToArray());
                            }
                        }
                    }
                    else for (int i = 0; i < 254; i++) m_aCParam[i].IsTorq = bOn;
                }
                #endregion Torq
#region Reboot / Reset
                public void Reboot() { Reboot(-1); }//_ID_BROADCASTING); }
                public void Reboot(int nMotor)
                {
                    if ((nMotor < 0) || (nMotor == 254))
                    {
                        //byte[] pbyTmp = Ojw.CConvert.IntToBytes(0);
                        //Write(nAxis, 104, pbyTmp);
                        for (int i = 0; i < GetCount_Connection(); i++)
                        {
                            //if (m_aCParam[nMotor].EMot != EMotor_t.SG_90)
                            //{
                            //Write_Command(1, i, ((nMotor < 0) ? 254 : nMotor), 0x06);
                            
                            // reboot 은 프로토콜 1 에는 없다.
                            Write_Command(2, i, ((nMotor < 0) ? 254 : nMotor), 0x08);

                            //}
                        }
                        //Clear_Flag();
                        m_bStop = false;
                        m_bEms = false;
                    }
                    else Write_Command(m_aCParam[nMotor].nProtocol, m_aCParam[nMotor].nCommIndex, nMotor, 0x08);
                    // Initialize variable

                    //m_nMotorCnt_Back = m_nMotorCnt = 0;

                }
                public void Reset() { Reset(-1); }//_ID_BROADCASTING); }
                public void Reset(int nMotor)
                {
                    //if ((nMotor < 0) || (nMotor == 254))
                    //{
                    //    //byte[] pbyTmp = Ojw.CConvert.IntToBytes(0);
                    //    //Write(nAxis, 104, pbyTmp);
                    //    for (int i = 0; i < GetCount_Connection(); i++)
                    //    {
                    //        //if (m_aCParam[nMotor].EMot != EMotor_t.SG_90)
                    //        //{
                    //        // 속도
                    //        Write_Command(1, i, ((nMotor < 0) ? 254 : nMotor), 0x06);
                    //        Write_One(2, i, ((nMotor < 0) ? 254 : nMotor), 0x06, 2); // 1 : ID 제외 초기화,             2 : id, braudcast 제외 초기화

                    //        //}
                    //    }
                    //    //Clear_Flag();
                    //    m_bStop = false;
                    //    m_bEms = false;
                    //}
                    //else Write_Command(m_aCParam[nMotor].nProtocol, m_aCParam[nMotor].nCommIndex, nMotor, 0x06);
                    //// Initialize variable

                    ////m_nMotorCnt_Back = m_nMotorCnt = 0;
                    m_bStop = false;
                    m_bEms = false;

                }
#endregion Reboot / Reset






            //public CParam_t GetParam(
            SMot_t[] m_aSMot = new SMot_t[256];
            //SMot_t[] m_aSMot_Prev = new SMot_t[256];
            public void Set(int nMotor, float fAngle)
            {
                if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return;
                Push(nMotor, m_aCParam[nMotor].nCommIndex, EOperation_t._Position, fAngle);
                //Push(nMotor, m_aCParam[nMotor].nCommIndex, ((m_aCParam[nMotor].EOperation == EOperation_t._Position_Multi) ? EOperation_t._Position_Multi : EOperation_t._Position), fAngle);
            }
            public void Set_Raw(int nMotor, float fRaw)
            {
                if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return;
                Push(nMotor, m_aCParam[nMotor].nCommIndex, EOperation_t._Position_Raw, fRaw);
                //Push(nMotor, m_aCParam[nMotor].nCommIndex, ((m_aCParam[nMotor].EOperation == EOperation_t._Position_Multi) ? EOperation_t._Position_Multi : EOperation_t._Position), fAngle);
            }
            public void Set_Multi(int nMotor, float fAngle)
            {
                if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return;
                Push(nMotor, m_aCParam[nMotor].nCommIndex, EOperation_t._Position_Multi, fAngle);
            }
            public void Set_Turn(int nMotor, float fRpm)
            {
                if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return;
                Push(nMotor, m_aCParam[nMotor].nCommIndex, EOperation_t._Speed, fRpm);
            }
            public void Set_Turn_Raw(int nMotor, float fRaw)
            {
                if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return;
                Push(nMotor, m_aCParam[nMotor].nCommIndex, EOperation_t._Speed_Raw, fRaw);
            }
            public void Set_Pwm(int nMotor, float fValue)
            {
                if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return;
                Push(nMotor, m_aCParam[nMotor].nCommIndex, EOperation_t._Pwm, fValue);
            }                
            //    m_aSMot[nMotor].fPos = CalcLimit_Evd(nMotor, CalcAngle2Evd(nMotor, fAngle));
            //    //m_aSMot[nMotor].nValue = CalcLimit_Evd(nMotor, CalcAngle2Evd(nMotor, fAngle));
            //    //m_aSMot[nMotor].fAngle_Back = CalcEvd2Angle(nMotor, m_aSMot[nMotor].nValue);
            //}
            //public void Send_Motor(int nMilliSecond)
            //{
            //    Writes_Bulk(0, m_CBulk);
            //}
            //    m_aSMot[nMotor].nValue = CalcLimit_Evd(nMotor, CalcAngle2Evd(nMotor, fAngle));
            //    m_aSMot[nMotor].bSetValue2 = true; // Rpm 을 따로 설정한 경우만 Set 한다.
            //    m_aSMot[nMotor].nValue2 = CalcRpm2Raw(nMotor, fRpm);
            //    m_aSMot[nMotor].fAngle_Back = CalcEvd2Angle(nMotor, m_aSMot[nMotor].nValue);

            //    m_aSMot[nMotor].nValue = CalcLimit_Evd(nMotor, CalcAngle2Evd(nMotor, fAngle));
            //    m_aSMot[nMotor].fAngle_Back = CalcEvd2Angle(nMotor, m_aSMot[nMotor].nValue);\
            //public float Get(int nMotor) { return m_aSMot[nMotor].fAngle_Back; }
            public void Send_Motor() { Send_Motor(0); }
#if true
            public void Move(params float[] afMotors)
            {
                if (afMotors.Length < 3)
                {
                    Ojw.CMessage.Write_Error("Check Motor Parameters(Length < 3:ID, Time, Delay)");
                    return;
                }
                // ID, Angle, ID, Angle, Time, Delay
                int nDelaySpace = (((afMotors.Length % 2) == 0) ? 1 : 0);
                int nPos_Time = afMotors.Length - 1 - nDelaySpace;
                int nCount = (nPos_Time / 2);
                int nPos_Delay = ((nDelaySpace > 0) ? afMotors.Length - 1 : -1);
                int nTime = (int)afMotors[nPos_Time];
                int nDelay = ((nDelaySpace > 0) ? (int)afMotors[nPos_Delay] : nTime);
                //if (nDelaySpace > 0) nDelay = (int)afMotors[nPos_Delay];
                //else nDelay = nTime;
                for (int i = 0; i < nCount; i++)
                {
#if false
                    if (
                        (m_aCParam[(int)afMotors[i * 2]].EOperation != EOperation_t._Position_Multi) &&
                        ((afMotors[i * 2 + 1] < 180) && (afMotors[i * 2 + 1] > -180))
                        )
                        Set((int)afMotors[i * 2], afMotors[i * 2 + 1]);
                    else Set_Multi((int)afMotors[i * 2], afMotors[i * 2 + 1]);
#else
                    Set((int)afMotors[i * 2], afMotors[i * 2 + 1]);
#endif
                }
                Send_Motor(nTime);
                if (nDelay > 0) Wait(nTime);
            }

            //private void MLin(double dX0, double dY0, double dZ0, double dX1, double dY1, double dZ1, double dStep, bool bContinue)
            //{
            //    //dX0 += m_fOffset_X;
            //    //dX1 += m_fOffset_X;
            //    //dZ0 += m_fOffset_Y;
            //    //dZ1 += m_fOffset_Y;
            //    double dLength = Math.Sqrt((dX1 - dX0) * (dX1 - dX0) + (dY1 - dY0) * (dY1 - dY0) + (dZ1 - dZ0) * (dZ1 - dZ0));
            //    double dX, dY, dZ;
            //    double[] adAngle = new double[3];
            //    double dTmp = dLength / dStep;
            //    int nCnt = (int)Math.Round(dTmp);
            //    int nStart = ((bContinue == true) ? 1 : 0);
            //    dTmp = dTmp / 2.0;
            //    for (int i = nStart; i <= nCnt; i++)
            //    {
            //        if (m_bStop == true) return;

            //        #region 'S'Curve
            //        #endregion

            //        if (i <= nCnt / 2)
            //        {
            //            //dAngle = (double)(nStart) * 90.0f / (dLength / 2.0);

            //            dX = dX0 + ((dX1 - dX0) / 2.0) - ((dX1 - dX0) / 2.0) * (float)CMath.Cos((double)i / dTmp * 90.0);
            //            dY = dY0 + ((dY1 - dY0) / 2.0) - ((dY1 - dY0) / 2.0) * (float)CMath.Cos((double)i / dTmp * 90.0);
            //            dZ = dZ0 + ((dZ1 - dZ0) / 2.0) - ((dZ1 - dZ0) / 2.0) * (float)CMath.Cos((double)i / dTmp * 90.0);
            //        }
            //        else
            //        {
            //            dX = dX0 + (dX1 - dX0) / 2.0 - ((dX1 - dX0) / 2.0) * (float)CMath.Cos((double)i / dTmp * 90.0);
            //            dY = dY0 + (dY1 - dY0) / 2.0 - ((dY1 - dY0) / 2.0) * (float)CMath.Cos((double)i / dTmp * 90.0);
            //            dZ = dZ0 + (dZ1 - dZ0) / 2.0 - ((dZ1 - dZ0) / 2.0) * (float)CMath.Cos((double)i / dTmp * 90.0);
            //            //    //dAngle = (float)((double)(nStart) - dLength / 2.0) * 90.0 / ((float)(dLength) / 2.0);

            //            //    dX = dX0 + (dX1 - dX0) / dLength * dStep * i;
            //            //    dY = dY0 + (dY1 - dY0) / dLength * dStep * i;
            //            //    dZ = dZ0 + (dZ1 - dZ0) / dLength * dStep * i;
            //        }
                    
            //        if (InverseKinematics(dZ, -dX, -dY, out adAngle[0], out adAngle[1], out adAngle[2]) == false) m_bStop = true;
            //        Move((float)adAngle[0], (float)adAngle[1], (float)adAngle[2], (float)m_nSpeed);//_SPD);                
            //    }
            //}

            public void Move_Turn(params float[] afMotors)
            {
                if (afMotors.Length < 3)
                {
                    Ojw.CMessage.Write_Error("Check Motor Parameters(Length < 3:ID, Time, Delay)");
                    return;
                }
                // ID, Angle, ID, Angle, Time, Delay
                int nDelaySpace = (((afMotors.Length % 2) == 0) ? 1 : 0);
                int nPos_Time = afMotors.Length - 1 - nDelaySpace;
                int nCount = (nPos_Time / 2);
                int nPos_Delay = ((nDelaySpace > 0) ? afMotors.Length - 1 : -1);
                int nTime = (int)afMotors[nPos_Time];
                int nDelay = ((nDelaySpace > 0) ? (int)afMotors[nPos_Delay] : nTime);
                //if (nDelaySpace > 0) nDelay = (int)afMotors[nPos_Delay];
                //else nDelay = nTime;
                for (int i = 0; i < nCount; i++) Set_Turn((int)afMotors[i * 2], afMotors[i * 2 + 1]);
                Send_Motor(nTime);
                if (nDelay > 0) Wait(nTime);
            }
            public void Send_Motor(int nMilliseconds)
            {
                if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true) || (m_lstConnect.Count <= 0) || (m_lstPush.Count <= 0)) return;

                int nPwm = 885;
                byte[,] buffer = new byte[m_lstConnect.Count * 2, 1024];//4096]; // 0 - preset
                int[] anIndex = new int[m_lstConnect.Count * 2];
                int[] anMotorCount = new int[m_lstConnect.Count * 2];
                int[] anAddress = new int[m_lstConnect.Count * 2];
                int[] anAddress_Size = new int[m_lstConnect.Count * 2];
                Array.Clear(anMotorCount, 0, anMotorCount.Length);
                Array.Clear(anAddress, 0, anAddress.Length);
                Array.Clear(anAddress_Size, 0, anAddress_Size.Length);
                Array.Clear(anIndex, 0, anIndex.Length);

                byte[] byInt = new byte[4];
                byte[] byShort = new byte[2];

                foreach (SPush_t SPush in m_lstPush)
                {
                    int nPos = (((SPush.nConnection < 0) ? 0 : SPush.nConnection) * 2 + m_aCParam[SPush.nMotor].nProtocol - 1);

#if false
                    if (SPush.EOperation != m_aCParam[SPush.nMotor].EOperation)
                    {
                        if (m_aCParam[SPush.nMotor].IsTorq == true)
                        {
                            SetTorq(SPush.nMotor, false);
                            //SetOperation(m_aCParam[SPush.nMotor].EOperation, SPush.nMotor);
                            SetOperation(SPush.EOperation, SPush.nMotor);
                            SetTorq(SPush.nMotor, true);
                        }
                        else SetOperation(m_aCParam[SPush.nMotor].EOperation, SPush.nMotor);
                    }
#endif
                    //int[] anValue = new int[5]; // pwm(2) + none(2), vel(4), acc(4), prof vel(4), pos(4)   // pos(2), vel(2)
                    //int[] anSettingIndex = new int[5]; 
                    //int[] anSettingIndex_Size = new int[5];
                    //int nAddress = ;

                    int nSize = 0;
                    // start address, length, 
                    // pwm, vel, acc,                    
                    switch (SPush.EOperation)
                    {
                        case EOperation_t._Speed:
                        case EOperation_t._Speed_Raw:
                            {
                                
                                // pwm(2) + none(2), vel(4), acc(4), prof vel(4), pos(4)   
                                //pwm
                                buffer[nPos, anIndex[nPos]++] = (byte)(m_aCParam[SPush.nMotor].nRealID & 0xff);
                                if (m_aCParam[SPush.nMotor].CAddress.nAddressStart == 100) // pwm address
                                {
                                    nSize = m_aCParam[SPush.nMotor].CAddress.nAddressLength;// -4;
                                    // pwm, none
                                    byShort = Ojw.CConvert.ShortToBytes((short)nPwm);
                                    for (int i = 0; i < byShort.Length; i++)
                                        buffer[nPos, anIndex[nPos]++] = byShort[i];
                                    for (int i = 0; i < 2; i++) buffer[nPos, anIndex[nPos]++] = 0;
                                    // vel
                                    byInt = Ojw.CConvert.IntToBytes((SPush.EOperation == EOperation_t._Speed_Raw) ? (int)SPush.fValue : CalcRpm2Raw(SPush.nMotor, SPush.fValue * ((m_aCParam[SPush.nMotor].nDir == 0) ? 1.0f : -1.0f)));
                                    for (int i = 0; i < byInt.Length; i++)
                                        buffer[nPos, anIndex[nPos]++] = byInt[i];
                                    // acc
                                    for (int i = 0; i < 4; i++) buffer[nPos, anIndex[nPos]++] = 0;
                                    // prof vel
                                    for (int i = 0; i < 4; i++) buffer[nPos, anIndex[nPos]++] = 0;
                                    // pos
                                    for (int i = 0; i < 4; i++) buffer[nPos, anIndex[nPos]++] = 0;                                    
                                }
                                // pos(2), vel(2)
                                else // XL-320, ax...
                                {
                                    nSize = m_aCParam[SPush.nMotor].CAddress.nAddressLength;
                                    // pos
                                    for (int i = 0; i < 2; i++) buffer[nPos, anIndex[nPos]++] = 0;
                                    // vel
                                    short sOr = 0;
                                    float fVal = SPush.fValue * ((m_aCParam[SPush.nMotor].nDir == 0) ? 1.0f : -1.0f);
                                    if (fVal < 0)
                                    {
                                        fVal = -fVal;
                                        sOr = 0x400;
                                    }
                                    short sData = (short)CalcRpm2Raw(SPush.nMotor, fVal);
                                    sData |= sOr;
                                    byShort = Ojw.CConvert.ShortToBytes(sData);
                                    for (int i = 0; i < byShort.Length; i++)
                                        buffer[nPos, anIndex[nPos]++] = byShort[i];
                                }
                                anMotorCount[nPos]++;
                            }
                            break;
                        case EOperation_t._Pwm:
                            {
                                // pwm(2) + none(2), vel(4), acc(4), prof vel(4), pos(4)   
                                //pwm
                                if (m_aCParam[SPush.nMotor].CAddress.nAddressStart == 100) // pwm address
                                {
                                    buffer[nPos, anIndex[nPos]++] = (byte)(m_aCParam[SPush.nMotor].nRealID & 0xff);
                                    nSize = m_aCParam[SPush.nMotor].CAddress.nAddressLength;
                                    // pwm, none
                                    byShort = Ojw.CConvert.ShortToBytes((short)CalcRpm2Raw(SPush.nMotor, SPush.fValue));
                                    for (int i = 0; i < byShort.Length; i++)
                                        buffer[nPos, anIndex[nPos]++] = byShort[i];
                                    // none
                                    for (int i = 0; i < 2; i++) buffer[nPos, anIndex[nPos]++] = 0;
                                    // vel
                                    for (int i = 0; i < 4; i++) buffer[nPos, anIndex[nPos]++] = 0;
                                    // acc
                                    for (int i = 0; i < 4; i++) buffer[nPos, anIndex[nPos]++] = 0;
                                    // prof vel
                                    for (int i = 0; i < 4; i++) buffer[nPos, anIndex[nPos]++] = 0;
                                    // pos
                                    for (int i = 0; i < 4; i++) buffer[nPos, anIndex[nPos]++] = 0;

                                    anMotorCount[nPos]++;
                                }
                                // pos(2), vel(2)
                                else // XL-320, ax...
                                {
                                 
                                }
                            }
                            break; 
                        case EOperation_t._Position_Raw:
                        case EOperation_t._Position:
                            {
                                buffer[nPos, anIndex[nPos]++] = (byte)(m_aCParam[SPush.nMotor].nRealID & 0xff);
                                nSize = m_aCParam[SPush.nMotor].CAddress.nAddressLength;
                                // speed
                                int nSpeed = 0;
                                if (m_aCParam[SPush.nMotor].bTimeControl == false)
                                {
                                    float fDiff = SPush.fValue - m_aCPush_Prev[SPush.nMotor].fValue;
                                    if (m_anWait[SPush.nMotor] > 0)
                                    {
                                        int nSpendTime = (int)m_aCTmr[SPush.nMotor].Get();
                                        if (nSpendTime < m_anWait[SPush.nMotor])
                                        {
                                            fDiff = SPush.fValue - (m_aCPush_Prev[SPush.nMotor].fValue - (fDiff * (float)(m_anWait[SPush.nMotor] - nSpendTime) / (float)m_anWait[SPush.nMotor]));
                                            //fDiff = fDiff * (float)(m_anWait[SPush.nMotor] - nSpendTime) / (float)m_anWait[SPush.nMotor];
                                        }
                                    }                                    
                                    m_afDiff[SPush.nMotor] = fDiff;

                                    //Raw 데이타면 상대 각도차를 실제 각도차로 다시 환산한다.
                                    if (SPush.EOperation == EOperation_t._Position_Raw) fDiff = CalcEvd2Angle(SPush.nMotor, (int)fDiff);

                                    float fRpm = (float)Math.Abs(CalcTime2Rpm(fDiff, (float)nMilliseconds));
                                    if (fRpm > 0)//!= 0)//> 1)
                                    {
                                        //bSet = true;
                                        nSpeed = CalcRpm2Raw(SPush.nMotor, fRpm); // short
                                    }
                                }
                                else nSpeed = nMilliseconds;
                                m_aCPush_Prev[SPush.nMotor].fValue = SPush.fValue;
                                    
                                // Position
                                int nValue = CalcLimit_Evd(SPush.nMotor, 
                                    (SPush.EOperation == EOperation_t._Position_Raw) ? (int)SPush.fValue : CalcAngle2Evd(SPush.nMotor, SPush.fValue)
                                    );

                                // pwm(2) + none(2), vel(4), acc(4), prof vel(4), pos(4)   
                                //pwm
                                if (m_aCParam[SPush.nMotor].CAddress.nAddressStart == 100) // pwm address
                                {                                                                       
                                    // pwm, none
                                    //for (int i = 0; i < 4; i++) buffer[nPos, anIndex[nPos]++] = 0;
                                    byShort = Ojw.CConvert.ShortToBytes((short)nPwm);
                                    for (int i = 0; i < byShort.Length; i++)
                                        buffer[nPos, anIndex[nPos]++] = byShort[i];
                                    for (int i = 0; i < 2; i++) buffer[nPos, anIndex[nPos]++] = 0;
                                    // vel
                                    for (int i = 0; i < 4; i++) buffer[nPos, anIndex[nPos]++] = 0;
                                    // acc
                                    for (int i = 0; i < 4; i++) buffer[nPos, anIndex[nPos]++] = 0;
                                    // prof vel
                                    byInt = Ojw.CConvert.IntToBytes(nSpeed);
                                    for (int i = 0; i < byInt.Length; i++)
                                        buffer[nPos, anIndex[nPos]++] = byInt[i]; 
                                    // pos
                                    byInt = Ojw.CConvert.IntToBytes(nValue);
                                    for (int i = 0; i < byInt.Length; i++)
                                        buffer[nPos, anIndex[nPos]++] = byInt[i];
                                }
                                // pos(2), vel(2)
                                else // XL-320, ax...
                                {
                                    // pos
                                    byShort = Ojw.CConvert.ShortToBytes((short)nValue);
                                    for (int i = 0; i < byShort.Length; i++)
                                        buffer[nPos, anIndex[nPos]++] = byShort[i];
                                    // vel
                                    byShort = Ojw.CConvert.ShortToBytes((short)nSpeed);
                                    for (int i = 0; i < byShort.Length; i++)
                                        buffer[nPos, anIndex[nPos]++] = byShort[i];
                                }
                                anMotorCount[nPos]++;
                            }
                            break;
                        case EOperation_t._Position_Multi_Raw:
                        case EOperation_t._Position_Multi:
                            {
                                buffer[nPos, anIndex[nPos]++] = (byte)(m_aCParam[SPush.nMotor].nRealID & 0xff);
                                nSize = m_aCParam[SPush.nMotor].CAddress.nAddressLength;
                                // speed
                                int nSpeed = 0;
                                if (m_aCParam[SPush.nMotor].bTimeControl == false)
                                {
                                    float fDiff = SPush.fValue - m_aCPush_Prev[SPush.nMotor].fValue;
                                    if (m_anWait[SPush.nMotor] > 0)
                                    {
                                        int nSpendTime = (int)m_aCTmr[SPush.nMotor].Get();
                                        if (nSpendTime < m_anWait[SPush.nMotor])
                                        {
                                            
                                            fDiff = SPush.fValue - (m_aCPush_Prev[SPush.nMotor].fValue - (fDiff * (float)(m_anWait[SPush.nMotor] - nSpendTime) / (float)m_anWait[SPush.nMotor]));
                                            //fDiff = fDiff * (float)(m_anWait[SPush.nMotor] - nSpendTime) / (float)m_anWait[SPush.nMotor];
                                        }
                                    }          
                                    m_afDiff[SPush.nMotor] = fDiff;

                                    //Raw 데이타면 상대 각도차를 실제 각도차로 다시 환산한다.
                                    if (SPush.EOperation == EOperation_t._Position_Multi_Raw) fDiff = CalcEvd2Angle(SPush.nMotor, (int)fDiff);

                                    float fRpm = (float)Math.Abs(CalcTime2Rpm(fDiff, (float)nMilliseconds));
                                    if (fRpm > 0)//!= 0)//> 1)
                                    {
                                        //bSet = true;
                                        nSpeed = CalcRpm2Raw(SPush.nMotor, fRpm); // short
                                    }
                                }
                                else nSpeed = nMilliseconds;
                                m_aCPush_Prev[SPush.nMotor].fValue = SPush.fValue;

                                // Position
                                int nValue = (SPush.EOperation == EOperation_t._Position_Raw) ? (int)SPush.fValue : CalcAngle2Evd(SPush.nMotor, SPush.fValue);// CalcLimit_Evd(SPush.nMotor, CalcAngle2Evd(SPush.nMotor, SPush.fValue));

                                // pwm(2) + none(2), vel(4), acc(4), prof vel(4), pos(4)   
                                //pwm
                                if (m_aCParam[SPush.nMotor].CAddress.nAddressStart == 100) // pwm address
                                {
                                    // pwm, none
                                    //for (int i = 0; i < 4; i++) buffer[nPos, anIndex[nPos]++] = 0;
                                    byShort = Ojw.CConvert.ShortToBytes((short)nPwm);
                                    for (int i = 0; i < byShort.Length; i++)
                                        buffer[nPos, anIndex[nPos]++] = byShort[i];
                                    for (int i = 0; i < 2; i++) buffer[nPos, anIndex[nPos]++] = 0;
                                    // vel
                                    for (int i = 0; i < 4; i++) buffer[nPos, anIndex[nPos]++] = 0;
                                    //byInt = Ojw.CConvert.IntToBytes(nSpeed);
                                    //for (int i = 0; i < byInt.Length; i++)
                                        //buffer[nPos, anIndex[nPos]++] = byInt[i];
                                    // acc
                                    for (int i = 0; i < 4; i++) buffer[nPos, anIndex[nPos]++] = 0;
                                    // prof vel
                                    byInt = Ojw.CConvert.IntToBytes(nSpeed);
                                    for (int i = 0; i < byInt.Length; i++)
                                        buffer[nPos, anIndex[nPos]++] = byInt[i];
                                    //for (int i = 0; i < 4; i++) buffer[nPos, anIndex[nPos]++] = 0;
                                    // pos
                                    byInt = Ojw.CConvert.IntToBytes(nValue);
                                    for (int i = 0; i < byInt.Length; i++)
                                        buffer[nPos, anIndex[nPos]++] = byInt[i];
                                }
                                // pos(2), vel(2)
                                else // XL-320, ax...
                                {
                                    // pos
                                    byShort = Ojw.CConvert.ShortToBytes((short)nValue);
                                    for (int i = 0; i < byShort.Length; i++)
                                        buffer[nPos, anIndex[nPos]++] = byShort[i];
                                    // vel
                                    byShort = Ojw.CConvert.ShortToBytes((short)nSpeed);
                                    for (int i = 0; i < byShort.Length; i++)
                                        buffer[nPos, anIndex[nPos]++] = byShort[i];
                                }
                                anMotorCount[nPos]++;
                            }
                            break;
                    }
                    if (nSize > 0)
                    {
                        //Ojw.CMessage.Write("Test: [{0}]IsTimer = {1}", m_aCTmr[SPush.nMotor].IsTimer(), m_aCTmr[SPush.nMotor].Get());

                        //m_aCTmr[SPush.nMotor].Set();
                        

                        anAddress_Size[nPos] = nSize;
                        anAddress[nPos] = m_aCParam[SPush.nMotor].CAddress.nAddressStart;                        
#if false
                        foreach (int nMotor in anMotors)
                        {
                            pbyteBuffer[i++] = (byte)(m_aCParam[nMotor].nRealID & 0xff);
                            pbyteBuffer[i++] = byVal;
                            m_aCParam[nMotor].EOperation = EOperation;
                        }
                        Writes(m_aCParam[anMotors[0]].nProtocol, m_aCParam[anMotors[0]].nCommIndex, m_aCParam[anMotors[0]].CAddress.nOperatingMode, m_aCParam[anMotors[0]].CAddress.nOperatingMode_Size, 1, pbyteBuffer);
#endif
                    }


                }

                foreach (SPush_t SPush in m_lstPush)
                {
                    if ((SPush.EOperation == EOperation_t._Position) || (SPush.EOperation == EOperation_t._Position_Multi))
                    {
                        m_anWait[SPush.nMotor] = nMilliseconds;
                        m_aCTmr[SPush.nMotor].Set(); // 해당 타이머 초기화
                    }
                    else m_anWait[SPush.nMotor] = 0;
                }

                for (int nConnection = 0; nConnection < m_lstConnect.Count; nConnection++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        int nPos = nConnection * 2 + j;                        
                        if (anIndex[nPos] > 0)
                        {
                            byte[] pbyBuffer = new byte[anIndex[nPos]];
                            for (int i = 0; i < anIndex[nPos]; i++)
                                pbyBuffer[i] = buffer[nPos, i];
                            Writes(j + 1, nConnection, anAddress[nPos], anAddress_Size[nPos], anMotorCount[nPos], pbyBuffer);//buffer[j, nPos]);
                        }
                    }
                }
                m_lstPush.Clear();
                m_nWait = nMilliseconds;
            }
#else
            public void Send_Motor(int nMilliseconds)
            {
                if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true) || (m_lstConnect.Count <= 0) || (m_lstPush.Count <= 0)) return;

                byte[,] buffer_Speed = new byte[m_lstConnect.Count * 2, 1024];//4096]; // 0 - preset
                int[] anIndex_Setting = new int[m_lstConnect.Count * 2];
                int[] anAddress_Setting = new int[m_lstConnect.Count * 2];
                int[] anAddress_Setting_Size = new int[m_lstConnect.Count * 2];
                int[] anMotorCount_Setting = new int[m_lstConnect.Count * 2];
                Array.Clear(anIndex_Setting, 0, anIndex_Setting.Length);    
                Array.Clear(anAddress_Setting, 0, anAddress_Setting.Length);
                Array.Clear(anAddress_Setting_Size, 0, anAddress_Setting_Size.Length);
                Array.Clear(anMotorCount_Setting, 0, anMotorCount_Setting.Length);        

                byte[,] buffer = new byte[m_lstConnect.Count * 2, 1024];//4096]; // 0 - preset
                int[] anIndex = new int[m_lstConnect.Count * 2];
                int[] anMotorCount = new int[m_lstConnect.Count * 2];
                int[] anAddress = new int[m_lstConnect.Count * 2];
                int[] anAddress_Size = new int[m_lstConnect.Count * 2];
                Array.Clear(anMotorCount, 0, anMotorCount.Length);                
                Array.Clear(anAddress, 0, anAddress.Length);
                Array.Clear(anAddress_Size, 0, anAddress_Size.Length);
                Array.Clear(anIndex, 0, anIndex.Length);            
                foreach (SPush_t SPush in m_lstPush)
                {
                    if (SPush.EOperation != m_aCParam[SPush.nMotor].EOperation)
                    {
                        if (m_aCParam[SPush.nMotor].IsTorq == true)
                        {
                            SetTorq(SPush.nMotor, false);
                            //SetOperation(m_aCParam[SPush.nMotor].EOperation, SPush.nMotor);
                            SetOperation(SPush.EOperation, SPush.nMotor);
                            SetTorq(SPush.nMotor, true);
                        }
                        else SetOperation(m_aCParam[SPush.nMotor].EOperation, SPush.nMotor);
                    }
                    int nValue = 0;
                    int nAddress = 0;
                    int nAddress_Setting = -1;
                    bool bAddress_Setting = false;
                    int nSize = 0;
                    int nSize_Setting = 0;
                    //EOperation_t EOper = SPush.EOperation;
                    switch (SPush.EOperation)
                    {
                        case EOperation_t._Speed:
                            {
                                nValue = CalcRpm2Raw(SPush.nMotor, SPush.fValue);
                                nAddress_Setting = m_aCParam[SPush.nMotor].CAddress.nWheel_Speed;

                                //nAddress_Setting = m_aCParam[SPush.nMotor].CAddress.nPos_Speed;

                                nSize_Setting = m_aCParam[SPush.nMotor].CAddress.nWheel_Speed_Size;
                                bAddress_Setting = true;
                            }
                            break;
                        case EOperation_t._Pwm:
                            {
                                nValue = (int)Math.Round(SPush.fValue);
                                nAddress = m_aCParam[SPush.nMotor].CAddress.nPwm;
                                nSize = m_aCParam[SPush.nMotor].CAddress.nPwm_Size;
                            }
                            break;
                        case EOperation_t._Position:
                            {
                                nValue = CalcLimit_Evd(SPush.nMotor, CalcAngle2Evd(SPush.nMotor, SPush.fValue));
                                nAddress = m_aCParam[SPush.nMotor].CAddress.nPos;
                                nAddress_Setting = m_aCParam[SPush.nMotor].CAddress.nPos_Speed;
                                nSize = m_aCParam[SPush.nMotor].CAddress.nPos_Size;
                                nSize_Setting = m_aCParam[SPush.nMotor].CAddress.nPos_Speed_Size;
                                bAddress_Setting = true;
                            }
                            break;
                        case EOperation_t._Position_Multi:
                            {
                                nValue = CalcAngle2Evd(SPush.nMotor, SPush.fValue);
                                nAddress = m_aCParam[SPush.nMotor].CAddress.nPos;
                                nAddress_Setting = m_aCParam[SPush.nMotor].CAddress.nPos_Speed;
                                nSize = m_aCParam[SPush.nMotor].CAddress.nPos_Size;
                                nSize_Setting = m_aCParam[SPush.nMotor].CAddress.nPos_Speed_Size;
                                bAddress_Setting = true;
                            }
                            break;
                    }
                    if (nSize > 0)
                    {
                        int nPos = (SPush.nConnection * 2 + m_aCParam[SPush.nMotor].nProtocol - 1);
                        byte[] byTmp = new byte[nSize];
                        //if (m_aCTmr[SPush.nMotor].IsTimer() == true) 

                        Ojw.CMessage.Write("{0}IsTimer = {1}", m_aCTmr[SPush.nMotor].IsTimer(), m_aCTmr[SPush.nMotor].Get());

                        m_aCTmr[SPush.nMotor].Set();
                        #region Speed
                        if (bAddress_Setting == true)
                        {
                            buffer_Speed[nPos, anIndex_Setting[nPos]++] = (byte)(m_aCParam[SPush.nMotor].nRealID & 0xff);
                            int nValue2 = 0;
                            float fRpm = (float)Math.Abs(CalcTime2Rpm(SPush.fValue - m_aCPush_Prev[SPush.nMotor].fValue, (float)nMilliseconds));
                            if (fRpm > 1)//!= 0)//> 1)
                            {
                                //bSet = true;
                                nValue2 = CalcRpm2Raw(SPush.nMotor, fRpm); // short
                            }
                            m_aCPush_Prev[SPush.nMotor].fValue = SPush.fValue;

                            byTmp = ((nSize == 2) ? Ojw.CConvert.ShortToBytes((short)nValue2) : Ojw.CConvert.IntToBytes(nValue2));

                            for (int i = 0; i < byTmp.Length; i++)
                                buffer_Speed[nPos, anIndex_Setting[nPos]++] = byTmp[i];

                            anMotorCount_Setting[nPos]++;
                        }
                        #endregion Speed

                        #region Position
                        if ((SPush.EOperation == EOperation_t._Position) || (SPush.EOperation == EOperation_t._Position_Multi))
                        {
                            buffer[nPos, anIndex[nPos]++] = (byte)(m_aCParam[SPush.nMotor].nRealID & 0xff);
                            byTmp = ((nSize == 2) ? Ojw.CConvert.ShortToBytes((short)nValue) : Ojw.CConvert.IntToBytes(nValue));
                            for (int i = 0; i < byTmp.Length; i++)
                                buffer[nPos, anIndex[nPos]++] = byTmp[i];
                            anMotorCount[nPos]++;
                        }
                        #endregion Position

                        anAddress_Size[nPos] = nSize;
                        anAddress[nPos] = nAddress;

                        anAddress_Setting[nPos] = nAddress_Setting;
                        anAddress_Setting_Size[nPos] = nSize_Setting;

#if false
                        foreach (int nMotor in anMotors)
                        {
                            pbyteBuffer[i++] = (byte)(m_aCParam[nMotor].nRealID & 0xff);
                            pbyteBuffer[i++] = byVal;
                            m_aCParam[nMotor].EOperation = EOperation;
                        }
                        Writes(m_aCParam[anMotors[0]].nProtocol, m_aCParam[anMotors[0]].nCommIndex, m_aCParam[anMotors[0]].CAddress.nOperatingMode, m_aCParam[anMotors[0]].CAddress.nOperatingMode_Size, 1, pbyteBuffer);
#endif
                    }


                }
                for (int nConnection = 0; nConnection < m_lstConnect.Count; nConnection++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        int nPos = nConnection * 2 + j;
                        if (anIndex_Setting[nPos] > 0)
                        {
                            byte[] pbyBuffer = new byte[anIndex_Setting[nPos]];
                            for (int i = 0; i < anIndex_Setting[nPos]; i++)
                                pbyBuffer[i] = buffer_Speed[nPos, i];
                            Writes(j + 1, nConnection, anAddress_Setting[nPos], anAddress_Setting_Size[nPos], anMotorCount_Setting[nPos], pbyBuffer);
                        }
                        if (anIndex[nPos] > 0)
                        {

                            //foreach (int nMotor in anMotors)
                            //{
                            //    pbyteBuffer[i++] = (byte)(m_aCParam[nMotor].nRealID & 0xff);
                            //    pbyteBuffer[i++] = byVal;
                            //    m_aCParam[nMotor].EOperation = EOperation;
                            //}
                            //Writes(m_aCParam[anMotors[0]].nProtocol, m_aCParam[anMotors[0]].nCommIndex, m_aCParam[anMotors[0]].CAddress.nOperatingMode, m_aCParam[anMotors[0]].CAddress.nOperatingMode_Size, 1, pbyteBuffer);


                            byte [] pbyBuffer = new byte[anIndex[nPos]];                            
                            for (int i = 0; i < anIndex[nPos]; i++)
                                pbyBuffer[i] = buffer[nPos, i];
                            //    Array.Copy(buffer, pbyBuffer, anIndex[nPos]);
                            Writes(j + 1, nConnection, anAddress[nPos], anAddress_Size[nPos], anMotorCount[nPos], pbyBuffer);//buffer[j, nPos]);
                        }
                    }
                }
                m_lstPush.Clear();
                m_nWait = nMilliseconds;
            }
#endif
            #endregion Control

            #region Background
            //private int m_nMotorCnt = 0;
            //private int [] m_anEn = new int[256];
            private struct SPush_t
            {
                public int nMotor;
                public int nConnection;
                public EOperation_t EOperation;
                public float fValue;
                public float fDelta;
                public SPush_t(int motor, int connection, EOperation_t eoper, float value, float delta)
                {
                    this.nMotor = motor;
                    this.nConnection = connection;
                    this.EOperation = eoper;
                    this.fValue = value;
                    this.fDelta = delta;
                }
            }
            private List<SPush_t> m_lstPush = new List<SPush_t>();
            private Ojw.CTimer[] m_aCTmr = new CTimer[256];
            private int[] m_anWait = new int[256];
            private float[] m_afDiff = new float[256];
            private SPush_t [] m_aCPush_Prev = new SPush_t[256];
            //private List<int> m_lstEn = new List<int>();
            private void Push(int nMotor, int nConnection, EOperation_t EOperation, float fValue) { if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return; if (IsCmd(nMotor) == false) m_lstPush.Add(new SPush_t(nMotor, nConnection, EOperation, fValue, 0.0f)); }//m_lstEn.Add(nMotor); }
            private SPush_t Pop()
            {
                SPush_t SRet = new SPush_t(-1, -1, EOperation_t._None, 0.0f, 0.0f);//SPush_t(-1, -1, EOperation_t._None, 0.0f, 0,0f);
                if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true))
                    return SRet;
                if (m_lstPush.Count > 0) 
                {
                    SRet = m_lstPush[m_lstPush.Count - 1];
                    m_lstPush.RemoveAt(m_lstPush.Count - 1);
                    return SRet;
                }
                return SRet; 
            }
            public bool IsCmd(int nMotor) { for (int i = 0; i < m_lstPush.Count; i++) { if (m_lstPush[i].nMotor == nMotor) { return true; } } return false; }



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
                if (m_aCParam[nMotor].fLimitUp != 0) nUp = CalcAngle2Evd(nMotor, m_aCParam[nMotor].fLimitUp);
                if (m_aCParam[nMotor].fLimitDn != 0) nDn = CalcAngle2Evd(nMotor, m_aCParam[nMotor].fLimitDn);
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
                if (m_aCParam[nMotor].fLimitUp != 0) fUp = m_aCParam[nMotor].fLimitUp;
                if (m_aCParam[nMotor].fLimitDn != 0) fDn = m_aCParam[nMotor].fLimitDn;
                return Clip(fUp, fDn, fValue);
                //}
                //return fValue;
            }
            public int CalcAngle2Evd(int nMotor, float fValue)
            {
                fValue *= ((m_aCParam[nMotor].nDir == 0) ? 1.0f : -1.0f);
                int nData = 0;
                //if (Get_Flag_Mode(nAxis) != 0)   // 속도제어
                //{
                //    nData = (int)Math.Round(fValue);
                //    //Ojw.CMessage.Write("Speed Turn");
                //}
                //else
                //{
                // 위치제어
                nData = (int)Math.Round((m_aCParam[nMotor].fMechMove * fValue) / m_aCParam[nMotor].fDegree);
                nData = nData + (int)Math.Round(m_aCParam[nMotor].fCenterPos);
                //}
                return nData;
            }
            public float CalcEvd2Angle(int nMotor, int nValue)
            {
                float fValue = ((m_aCParam[nMotor].nDir == 0) ? 1.0f : -1.0f);
                float fValue2 = 0.0f;
                //if (Get_Flag_Mode(nMotor) != 0)   // 속도제어
                //    fValue2 = (float)nValue * fValue;
                //else                                // 위치제어
                //{
                fValue2 = (float)(((m_aCParam[nMotor].fDegree * ((float)(nValue - (int)Math.Round(m_aCParam[nMotor].fCenterPos)))) / m_aCParam[nMotor].fMechMove) * fValue);
                //}
                return fValue2;
            }
            private float CalcRaw2Rpm(int nMotor, int nValue) { return (float)nValue * m_aCParam[nMotor].fRefRpm; }
            private int CalcRpm2Raw(int nMotor, float fRpm) { return (int)Math.Round(Clip(m_aCParam[nMotor].fLimitRpm, -m_aCParam[nMotor].fLimitRpm, fRpm / m_aCParam[nMotor].fRefRpm)); }

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

            #region Motion Play
            Ojw.C3d.COjwDesignerHeader m_CHeader = null;
            public void Set3DHeader(Ojw.C3d.COjwDesignerHeader CHeader) 
            {
                m_CHeader = CHeader; 
                // SetParam
                //m_CHeader.nMotorCnt;
                //m_CHeader.pSMotorInfo[];
                // 여기서 모터 파라미터들을 셋팅하도록 한다.
                //int nMax = 0;
                List<int> anMotorIDs = new List<int>();
                anMotorIDs.Clear();
                if (m_CHeader.anMotorIDs != null)
                {
                    anMotorIDs.AddRange(m_CHeader.anMotorIDs);
                    anMotorIDs.Sort();
#if true
                for (int i = 0; i < m_CHeader.anMotorIDs.Length; i++)
                {
                    //int nMotor = m_CHeader.anMotorIDs[i];
                    ////SetParam(nMotor, (EMotor_t)m_CHeader.pSMotorInfo[nMotor].nMotorEnable_For_RPTask
                    //SetParam(nMotor, (EMotor_t)m_CHeader.pSMotorInfo[nMotor].nHwMotor_Index);
                    SetParam(i, (EMotor_t)m_CHeader.pSMotorInfo[i].nHwMotor_Index);

                    //bool bFind = anMotorIDs.Exists(x => x == i);

                    //if (bFind == true) SetParam_RealID(i, m_CHeader.pSMotorInfo[i].nMotorID);
                    //else SetParam_RealID(i, -1);

                    SetParam_LimitUp(i, m_CHeader.pSMotorInfo[i].fLimit_Up);
                    SetParam_LimitDown(i, m_CHeader.pSMotorInfo[i].fLimit_Down);
                    m_aCParam[i].fMechMove = (float)m_CHeader.pSMotorInfo[i].nMechMove;
                    m_aCParam[i].fCenterPos = m_CHeader.pSMotorInfo[i].nCenter_Evd;
                    m_aCParam[i].fDegree = m_CHeader.pSMotorInfo[i].fMechAngle;
                    m_aCParam[i].nDir = m_CHeader.pSMotorInfo[i].nMotorDir;
                    m_aCParam[i].nRealID = m_CHeader.pSMotorInfo[i].nMotorID;
                }
#else
                //for (int i = 0; i < 255; i++) // 나중에 이 불합리한 구조 좀 바꾸자... ㅠ.ㅠ
                //{
                //    if (m_CHeader.pSMotorInfo[i].nMotorID > nMax) nMax = m_CHeader.pSMotorInfo[i].nMotorID;
                //}
                for (int i = 0; i < 255; i++)
                {
                    // 여기서도 모터아이디 0 은 그냥 버리자. -_-;;;(나중에는 그냥 모터 순서의 의미로만으로도 사용할 수 있게... -> 다른 것들처럼 리스트로 가변 변수 처리 하도록...
                    //if (m_CHeader.pSMotorInfo[i].nHwMotor_Index > 0)
                    //{
                        //SetParam(i, (EMotor_t)m_CHeader.pSMotorInfo[i].nHwMotor_Index);
                    //}

                    bool bFind = anMotorIDs.Exists(x=> x == i);
                    //SetParam_CommIndex(i, 0); // 컴포트 인덱서는 일단 0으로 하자.
                    if (bFind == true) SetParam_RealID(i, m_CHeader.pSMotorInfo[i].nMotorID);
                    else SetParam_RealID(i, -1); 
                    SetParam_LimitUp(i, m_CHeader.pSMotorInfo[i].fLimit_Up);
                    SetParam_LimitDown(i, m_CHeader.pSMotorInfo[i].fLimit_Down);

                    //m_aCParam[i].fCenterPos = (float)m_CHeader.pSMotorInfo[i].nCenter_Evd;

                    //m_aCParam[i].nDriveMode = 0;   // 0x00 - Rpm 모드, 0x40 - Time 제어 모드
                    //m_aCParam[i].EOperation = EOperation_t._Position; // Operating Mode (0-전류제어, 1-속도제어, 3-위치제어(default), 4-확장위치제어, 5-전류기반 위치제어(그리퍼), 16-PWM 제어
                
                }
#endif
                }

            }
            public bool Motion_Play(string strFileName)
            {
                bool bRet = true;

                //SMotion_t SMot
                //if (DataFileOpen(strFileName, null, ref SMot) == true)
                //{
                //}
                SMotion_t SMot = new SMotion_t();
                if (DataFileOpen(strFileName, null, ref SMot) == true)
                {
                    for (int i = 0; i < SMot.nFrameSize; i++)
                    {
                        //if (m_bStop == true) 
                        if (SMot.STable[i].bEn == true)
                        {
                            for (int j = 0; j < SMot.nMotorCnt; j++)
                            {
                                // 이거 나중엔 Raw 데이터를 활용할 수 있도록 하자. 현재는 Angle                                
                                //Set(j, CalcEvd2Angle(j, SMot.STable[i].anMot[j]));
                                Set_Raw(j, SMot.STable[i].anMot[j]);
                            }
                            int nTime = SMot.STable[i].nTime;
                            int nDelay = (SMot.STable[i].nTime + SMot.STable[i].nDelay);
                            Send_Motor(nTime);
                            Delay(nDelay);
                        }
                    }
                    // Send
                    // Delay
                }
                return bRet;
            }
            public Ojw.C3d.COjwDesignerHeader GetHeader() { return m_CHeader; }
            public bool IsHeader() { return (m_CHeader == null) ? false : true; }
            public bool DataFileOpen(String strFileName, byte[] byteArrayData, ref SMotion_t SMot)//, bool bMessage)//, bool bTableOut)
            {                                
                bool bFile = false;

                if (byteArrayData == null) bFile = true;
                else bFile = false;

                bool bFileOpened = false;
                String _STR_EXT = "dmt";
                //String _STR_VER_V_12 = "1.2";
                //String _STR_VER_V_11 = "1.1";
                String _STR_VER_V_10 = "1.0";

                FileInfo f = null;
                FileStream fs = null;

                try
                {
                    int i, j;
                    byte[] byteData;
                    //string strFileName2 = "";
                    if (bFile == true)
                    {
                        f = new FileInfo(strFileName);
                        fs = f.OpenRead();
                        byteData = new byte[fs.Length];
                        fs.Read(byteData, 0, byteData.Length);
                        //strFileName2 = f.Name;

                        fs.Close();
                        f = null;
                    }
                    else
                    {
                        if (byteArrayData.Length <= 6)
                        {
                            //OjwMessage("FileSize Error(Size[" + byteArrayData.Length.ToString() + "] <= 6)");
                            return false;
                        }
                        byteData = new byte[byteArrayData.Length];
                        byteData = byteArrayData;
                        //strFileName2 = strFileName;
                    }

                    // 데이타 형식 구분
                    String strTmp = "";
                    strTmp += (char)byteData[0];
                    strTmp += (char)byteData[1];
                    strTmp += (char)byteData[2];
                    strTmp += (char)byteData[3];
                    strTmp += (char)byteData[4];
                    strTmp += (char)byteData[5];

                    //m_strMotionFile_FileName = strFileName;
                    //m_strMotionFile_TableName = "";

                    if (strTmp.ToUpper() == _STR_EXT.ToUpper() + _STR_VER_V_10)
                    {
                        #region FileOpen V1.0
                        int nPos = 6;   // 앞의 6개는 'DMT1.0' 에 할당

                        #region Header

                        #region 타이틀(21)
                        byte[] byteGetData = new byte[21];
                        for (i = 0; i < 21; i++) byteGetData[i] = 0;
                        for (i = 0; i < 21; i++)
                        {
                            if (byteData[i + nPos] == 0) break;
                            byteGetData[i] = byteData[i + nPos];
                        }
                        SMot.strTableName = System.Text.Encoding.Default.GetString(byteGetData);
                        SMot.strFileName = strFileName;
                        nPos += 21;
                        byteGetData = null;
                        #endregion 타이틀(21)

                        #region Start Position(1)
                        int nStartPosition = (int)(byteData[nPos++]);
                        SMot.nStartPosition = (nStartPosition >= 0) ? nStartPosition : 0;
                        //m_nMotionFile_StartPosition = ((nStartPosition > 0) ? nStartPosition : 0);
                        #endregion Start Position(1)

                        #region Size - MotionFrame(2), Comment(2), Caption(2), PlayTime(4), RobotModelNumber(2), MotorCnt(1)
                        // Size
                        //int nFrameSize, nCnt_LineComment, nPlayTime, nCommentSize, nRobotModelNum, nMotorCnt;
                        SMot.nFrameSize = (int)(byteData[nPos] + byteData[nPos + 1] * 256); nPos += 2;
                        SMot.nCommentSize = (int)(byteData[nPos] + byteData[nPos + 1] * 256); nPos += 2;
                        SMot.nCnt_LineComment = (int)(byteData[nPos] + byteData[nPos + 1] * 256); nPos += 2;
                        SMot.nPlayTime = (int)(byteData[nPos] + byteData[nPos + 1] * 256 + byteData[nPos + 2] * 256 * 256 + byteData[nPos + 3] * 256 * 256 * 256); nPos += 4;
                        SMot.nRobotModelNum = (int)(byteData[nPos] + byteData[nPos + 1] * 256); nPos += 2;
                        SMot.nMotorCnt = (int)(byteData[nPos++]);
                        #endregion Size - MotionFrame, Comment, Caption, PlayTime

                        #endregion Header

#if false
                        // nRobotModelNum 를 읽고 해당 파일을 읽어들인다.
                        #region Header 검증
                        if (nMotorCnt != m_CHeader.nMotorCnt)
                        {
                            //MessageBox.Show("디자이너 파일의 모터 수량과 맞지 않습니다.(요구모터수량=" + Ojw.CConvert.IntToStr(m_CHeader.nMotorCnt) + ", 모션파일에 정의된 모터수량=" + Ojw.CConvert.IntToStr(nMotorCnt) + ")\n");// 해당 모델에 맞는 모션을 로드하십시오.");
                            //DialogResult dlgRet = MessageBox.Show("무시하고 계속 열겠습니까?", "파일열기", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                            MessageBox.Show("Motor quantity error.(Motors in 3D Modeling =" + Ojw.CConvert.IntToStr(m_CHeader.nMotorCnt) + ", Motors in File =" + Ojw.CConvert.IntToStr(nMotorCnt) + ")\n");// 해당 모델에 맞는 모션을 로드하십시오.");
                            DialogResult dlgRet = MessageBox.Show("Do you want to continue?", "File Open", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                            if (dlgRet == DialogResult.OK)
                            {
                            }
                            else return false;
                        }
                        #endregion Header 검증
#endif
                        SMot.STable = new SMotionTable_t[SMot.nFrameSize];
                        #region 실제 모션
                        int nH = SMot.nFrameSize;
                        int nData;
                        short sData;
                        float fValue;
                        for (j = 0; j < nH; j++)
                        {
                            //En
                            #region Enable
                            int nEn = byteData[nPos++];
                            bool bEn = ((nEn & 0x01) != 0) ? true : false;
                            //m_CGridMotionEditor.SetEnable(j, bEn);
                            SMot.STable[j].bEn = bEn;
                            #endregion Enable
                            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            #region Motor
                            int nMotorCntMax = SMot.nMotorCnt;//int)Math.Max(SMot.nMotorCnt, m_CHeader.nMotorCnt);
                            SMot.STable[j].anMot = new int[nMotorCntMax];
                            // 0-Index, 1-En, 2 ~ 24, 25 - speed, 26 - delay, 27,28,29,30 - Data0-3, 31 - time, 32 - caption
                            for (int nAxis = 0; nAxis < nMotorCntMax; nAxis++)
                            {
                                //if (nAxis >= m_CHeader.nMotorCnt) nPos += 2;
                                //else if (nAxis >= nMotorCnt) m_CGridMotionEditor.SetData(j, nAxis, 0.0f);// 실 모터수량과 맞지 않다면 그 부분을 0 으로 채울 것
                                //else
                                //{                                    
                                    SMot.STable[j].anMot[nAxis] = (int)(BitConverter.ToInt16(byteData, nPos)); nPos += 2;
                                    
                                    /* - Save
                                    fValue = GridMotionEditor_GetMotor(i, j);
                                    sData = (short)(OjwMotor.CalcAngle2Evd(j, fValue) & 0x03ff);
                                    //sData |= 0x0400; // 속도모드인때 정(0-0x0000), 역(1-0x0400)
                                    //sData |= LED;  // 00 - 0ff, 0x0800 - Red(01), 0x1000 - Blue(10), 0x1800 - Green(11)
                                    //sData |= 제어타입 // 0 - 위치, 0x2000 - 속도
                                    sData |= 0x4000; //Enable // 개별 Enable (0 - Disable, 0x4000 - Enable)
                                     */
                                //}
                            }
                            #endregion Motor
                            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                            #region Speed(2), Delay(2), Group(1), Command(1), Data0(2), Data1(2)
                            // Speed  
                            SMot.STable[j].nTime = (int)(byteData[nPos] + byteData[nPos + 1] * 256); nPos += 2;
                            //m_CGridMotionEditor.SetSpeed(j, nData);
                            //GridMotionEditor_SetTime(j, nData);

                            // Delay  
                            SMot.STable[j].nDelay = BitConverter.ToInt16(byteData, nPos); nPos += 2;
                            //m_CGridMotionEditor.SetDelay(j, nData);
                            //GridMotionEditor_SetDelay(j, nData);

                            // Group  
                            SMot.STable[j].nGroup = (int)(byteData[nPos++]);
                            //m_CGridMotionEditor.SetGroup(j, nData);
                            //GridMotionEditor_SetGroup(j, nData);

                            // Command  
                            SMot.STable[j].nCmd = (int)(byteData[nPos++]);
                            //GridMotionEditor_SetCommand(j, nData);// m_CGridMotionEditor.SetData2(j, nData);//SetCommand(j, nData);


                            // Data0  
                            SMot.STable[j].nData0 = (int)(byteData[nPos] + byteData[nPos + 1] * 256); nPos += 2;
                            //m_CGridMotionEditor.SetData0(j, nData);
                            //GridMotionEditor_SetData3(j, nData);
                            //GridMotionEditor_SetData0(j, nData);
                            // Data1  
                            SMot.STable[j].nData1 = (int)(byteData[nPos] + byteData[nPos + 1] * 256); nPos += 2;
                            //Grid_SetData1(j, nData);
                            //GridMotionEditor_SetData4(j, nData);
                            //GridMotionEditor_SetData1(j, nData);
                            #endregion Speed(2), Delay(2), Group(1), Command(1), Data0(2), Data1(2)

                            //SMot.STable[j].
                            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                            #region 추가한 Frame 위치 및 자세
                            nPos += 4;//SetFrame_X(j, (float)BitConverter.ToSingle(byteData, nPos)); nPos += 4;
                            nPos += 4;//SetFrame_Y(j, (float)BitConverter.ToSingle(byteData, nPos)); nPos += 4;
                            nPos += 4;//SetFrame_Z(j, (float)BitConverter.ToSingle(byteData, nPos)); nPos += 4;

                            nPos += 4;//SetFrame_Pan(j, (float)BitConverter.ToSingle(byteData, nPos)); nPos += 4;
                            nPos += 4;//SetFrame_Tilt(j, (float)BitConverter.ToSingle(byteData, nPos)); nPos += 4;
                            nPos += 4;//SetFrame_Swing(j, (float)BitConverter.ToSingle(byteData, nPos)); nPos += 4;
                            #endregion 추가한 Frame 위치 및 자세
                        }
                        #endregion 실제 모션

//#if !_COLOR_GRID_IN_PAINT
//                        m_CGridMotionEditor.SetColorGrid(0, nFrameSize);
//                        //Grid_SetColorGrid(0, nFrameSize);
//#endif
//                        string strData_ME = "";
//                        string strData_FE = "";

//                        // 'M' 'E'
//                        strData_ME += (char)(byteData[nPos++]);
//                        strData_ME += (char)(byteData[nPos++]);

//                        #region Comment Data
//                        // Comment
//                        byte[] pstrComment = new byte[nCommentSize];
//                        for (j = 0; j < nCommentSize; j++)
//                            pstrComment[j] = (byte)(byteData[nPos++]);
//                        m_strMotionFile_Comment = System.Text.Encoding.Default.GetString(pstrComment);
//                        pstrComment = null;
//                        #endregion Comment Data

//                        #region Caption
//                        int nLineNum = 0;
//                        string strLineComment;
//                        byte[] byLine = new byte[46];
//                        for (j = 0; j < nCnt_LineComment; j++)
//                        {
//                            nLineNum = (short)(byteData[nPos] + byteData[nPos + 1] * 256); nPos += 2;
//                            for (int k = 0; k < 46; k++)
//                                byLine[k] = (byte)(byteData[nPos++]);
//                            strLineComment = System.Text.Encoding.Default.GetString(byLine);
//                            strLineComment = strLineComment.Trim((char)0);
//                            m_CGridMotionEditor.SetCaption(nLineNum, strLineComment);
//                        }
//                        byLine = null;
//                        #endregion Caption

//                        // 'T' 'E'
//                        strData_FE += (char)(byteData[nPos++]);
//                        strData_FE += (char)(byteData[nPos++]);

                        bFileOpened = true;
                        #endregion FileOpen V1.0
                    }
#if false
                    else if (strTmp.ToUpper() == _STR_EXT.ToUpper() + _STR_VER_V_11)
                    {
                        //chkFileVersionForSave_1_0.Checked = false;
                        //chkFileVersionForSave.Checked = true;
                        #region FileOpen V1.1
                        //if (bMessage == true) OjwMessage("[" + _STR_EXT.ToUpper() + _STR_VER.ToUpper() + " Binary File Data(" + strTmp + ")]");
                        int nPos = 6;   // 앞의 6개는 'DMT1.0' 에 할당

                        #region Header

                        #region 타이틀(21)
                        byte[] byteGetData = new byte[21];
                        for (i = 0; i < 21; i++) byteGetData[i] = 0;
                        for (i = 0; i < 21; i++)
                        {
                            if (byteData[i + nPos] == 0) break;
                            byteGetData[i] = byteData[i + nPos];
                        }
                        m_strMotionFile_TableName = System.Text.Encoding.Default.GetString(byteGetData);
                        nPos += 21;
                        byteGetData = null;
                        #endregion 타이틀(21)

                        #region Start Position(1)
                        int nStartPosition = (int)(byteData[nPos++]);
                        nStartPosition = (nStartPosition >= 0) ? nStartPosition : 0;
                        m_nMotionFile_StartPosition = ((nStartPosition > 0) ? nStartPosition : 0);
                        #endregion Start Position(1)

                        #region Size - MotionFrame(2), Comment(2), Caption(2), PlayTime(4), RobotModelNumber(2), MotorCnt(1)
                        // Size
                        int nFrameSize, nCnt_LineComment, nPlayTime, nCommentSize, nRobotModelNum, nMotorCnt;
                        nFrameSize = (int)(byteData[nPos] + byteData[nPos + 1] * 256); nPos += 2;
                        nCommentSize = (int)(byteData[nPos] + byteData[nPos + 1] * 256); nPos += 2;
                        nCnt_LineComment = (int)(byteData[nPos] + byteData[nPos + 1] * 256); nPos += 2;
                        nPlayTime = (int)(byteData[nPos] + byteData[nPos + 1] * 256 + byteData[nPos + 2] * 256 * 256 + byteData[nPos + 3] * 256 * 256 * 256); nPos += 4;
                        nRobotModelNum = (int)(byteData[nPos] + byteData[nPos + 1] * 256); nPos += 2;
                        nMotorCnt = (int)(byteData[nPos++]);
                        #endregion Size - MotionFrame, Comment, Caption, PlayTime

                        #endregion Header

                        // nRobotModelNum 를 읽고 해당 파일을 읽어들인다.
                        #region Header 검증
                        if (nMotorCnt != m_CHeader.nMotorCnt)
                        {
                            //if (bFile == true)
                            //{
                            //    fs.Close();
                            //    f = null;
                            //}
                            this.Cursor = System.Windows.Forms.Cursors.Default;
                            MessageBox.Show("디자이너 파일의 모터 수량과 맞지 않습니다.(요구모터수량=" + Ojw.CConvert.IntToStr(m_CHeader.nMotorCnt) + ", 모션파일에 정의된 모터수량=" + Ojw.CConvert.IntToStr(nMotorCnt) + ")\n");// 해당 모델에 맞는 모션을 로드하십시오.");
                            DialogResult dlgRet = MessageBox.Show("무시하고 계속 열겠습니까?", "파일열기", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                            if (dlgRet == DialogResult.OK)
                            {
                                //MessageBox.Show("Yes");
                                //return;
                            }
                            else return false;
                        }
                        #endregion Header 검증

                        //Grid_ChangePos(dgAngle, 0, 0);
                        //Grid_ChangePos(dgKinematics, 0, 0);
                        //GridInit(nMotorCnt, nFrameSize, false);// + 50);
                        //GridInit(nMotorCnt, _SIZE_FRAME, false);

                        for (i = nFrameSize; i < m_CGridMotionEditor.GetLineCount() - nFrameSize; i++) m_CGridMotionEditor.Clear(i);

                        #region 실제 모션
                        int nH = nFrameSize;
                        int nData, nData2;
                        //short sData;
                        float fValue;
                        for (j = 0; j < nH; j++)
                        {
                            //En
                            #region Enable
                            int nEn = byteData[nPos++];
                            bool bEn = ((nEn & 0x01) != 0) ? true : false;
                            m_CGridMotionEditor.SetEnable(j, bEn);
                            #endregion Enable
                            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            #region Motor
                            int nMotorCntMax = (int)Math.Max(nMotorCnt, m_CHeader.nMotorCnt);
                            // 0-Index, 1-En, 2 ~ 24, 25 - speed, 26 - delay, 27,28,29,30 - Data0-3, 31 - time, 32 - caption
                            for (int nAxis = 0; nAxis < nMotorCntMax; nAxis++)
                            {
                                if (nAxis >= m_CHeader.nMotorCnt) nPos += 3;
                                else if (nAxis >= nMotorCnt) m_CGridMotionEditor.SetData(j, nAxis, 0.0f);// 실 모터수량과 맞지 않다면 그 부분을 0 으로 채울 것
                                else
                                {
                                    nData = byteData[nPos++];
                                    nData += byteData[nPos++] * 256;
                                    nData += byteData[nPos++] * 256 * 256;
                                    //nData = (int)(BitConverter.ToInt(byteData, nPos)); nPos += 3;
                                    //sData = (short)(nData & 0x0fff);
                                    nData2 = nData & 0x3fff;
                                    if ((nData & 0x4000) != 0) nData2 *= -1; // 부호비트 검사

                                    // 엔코더 타입정의
                                    // 일단 넘어간다.

                                    // Stop Bit
                                    // 넘어간다.

                                    // Mode
#if false
                                        //Grid_SetFlag_Type(j, nAxis, (((nData & 0x20000) != 0) ? true : false));

                                        //Grid_SetFlag_Led(j, nAxis, ((nData >> 18) & 0x07));
                                        //Grid_SetFlag_En(j, nAxis, ((nData == 0x200000) ? false : true));

                                        if (m_CGridMotionEditor.GetEnable(j, nAxis) == false)
                                        {
                                            m_CGridMotionEditor.SetData(j, nAxis, 0);
                                        }
                                        else
#endif
                                    {
                                        fValue = CalcEvd2Angle(nAxis, (int)nData2);
                                        m_CGridMotionEditor.SetData(j, nAxis, fValue);
                                    }



                                    /* - Save
                                    fValue = GridMotionEditor_GetMotor(i, j);
                                    sData = (short)(OjwMotor.CalcAngle2Evd(j, fValue) & 0x03ff);
                                    //sData |= 0x0400; // 속도모드인때 정(0-0x0000), 역(1-0x0400)
                                    //sData |= LED;  // 00 - 0ff, 0x0800 - Red(01), 0x1000 - Blue(10), 0x1800 - Green(11)
                                    //sData |= 제어타입 // 0 - 위치, 0x2000 - 속도
                                    sData |= 0x4000; //Enable // 개별 Enable (0 - Disable, 0x4000 - Enable)
                                     */


                                    //fValue = GridMotionEditor_GetMotor(i, j);
                                    //nData = (int)(((Grid_GetFlag_En(i, j) == true) ? CalcAngle2Evd(j, fValue) : 0x07ff) & 0x0fff);

                                    //nData |= (int)(((j >= 6) && (j <= 8)) ? 0x8000 : 0x0000);
                                    //nData |= (int)((Grid_GetFlag_Type(i, j) == true) ? 0x20000 : 0x0000); // 제어타입 // 0 - 위치, 0x20000 - 속도

                                    //nData |= (int)((Grid_GetFlag_Led(i, j) & 0x07) << 18);
                                    //nData |= (int)((Grid_GetFlag_Type(i, j) == true) ? 0x8000 : 0x0000);
                                    //nData |= (int)((Grid_GetFlag_En(i, j) == false) ? 0x200000 : 0x00000);

                                    ////byteData = BitConverter.GetBytes((Int32)nData);
                                    ////fs.Write(byteData, 0, 3);
                                }
                            }
                            #endregion Motor
                            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                            #region Speed(2), Delay(2), Group(1), Command(1), Data0(2), Data1(2)
                            // Speed  
                            nData = (int)(byteData[nPos] + byteData[nPos + 1] * 256); nPos += 2;
                            GridMotionEditor_SetTime(j, nData);

                            // Delay  
                            nData = BitConverter.ToInt16(byteData, nPos); nPos += 2;
                            GridMotionEditor_SetDelay(j, nData);

                            // Group  
                            nData = (int)(byteData[nPos++]);
                            GridMotionEditor_SetGroup(j, nData);

                            // Command  
                            nData = (int)(byteData[nPos++]);
                            GridMotionEditor_SetCommand(j, nData);

                            // Data0  
                            nData = (int)(byteData[nPos] + byteData[nPos + 1] * 256); nPos += 2;
                            GridMotionEditor_SetData0(j, nData);
                            // Data1  
                            nData = (int)(byteData[nPos] + byteData[nPos + 1] * 256); nPos += 2;
                            GridMotionEditor_SetData1(j, nData);
                            #endregion Speed(2), Delay(2), Group(1), Command(1), Data0(2), Data1(2)
                            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                            #region 추가한 Frame 위치 및 자세
                            nPos += 4;//SetFrame_X(j, (float)BitConverter.ToSingle(byteData, nPos)); nPos += 4;
                            nPos += 4;//SetFrame_Y(j, (float)BitConverter.ToSingle(byteData, nPos)); nPos += 4;
                            nPos += 4;//SetFrame_Z(j, (float)BitConverter.ToSingle(byteData, nPos)); nPos += 4;

                            nPos += 4;//SetFrame_Pan(j, (float)BitConverter.ToSingle(byteData, nPos)); nPos += 4;
                            nPos += 4;//SetFrame_Tilt(j, (float)BitConverter.ToSingle(byteData, nPos)); nPos += 4;
                            nPos += 4;//SetFrame_Swing(j, (float)BitConverter.ToSingle(byteData, nPos)); nPos += 4;
                            #endregion 추가한 Frame 위치 및 자세
                        }
                        #endregion 실제 모션

//#if !_COLOR_GRID_IN_PAINT
//                        m_CGridMotionEditor.SetColorGrid(0, nFrameSize);
//#endif

//                        string strData_ME = "";
//                        string strData_FE = "";

//                        // 'M' 'E'
//                        strData_ME += (char)(byteData[nPos++]);
//                        strData_ME += (char)(byteData[nPos++]);

//                        #region Comment Data
//                        // Comment
//                        byte[] pstrComment = new byte[nCommentSize];
//                        for (j = 0; j < nCommentSize; j++)
//                            pstrComment[j] = (byte)(byteData[nPos++]);
//                        m_strMotionFile_Comment = System.Text.Encoding.Default.GetString(pstrComment);
//                        pstrComment = null;
//                        #endregion Comment Data

//                        #region Caption
//                        int nLineNum = 0;
//                        string strLineComment;
//                        byte[] byLine = new byte[46];
//                        for (j = 0; j < nCnt_LineComment; j++)
//                        {
//                            nLineNum = (short)(byteData[nPos] + byteData[nPos + 1] * 256); nPos += 2;
//                            for (int k = 0; k < 46; k++)
//                                byLine[k] = (byte)(byteData[nPos++]);
//                            strLineComment = System.Text.Encoding.Default.GetString(byLine);
//                            strLineComment = strLineComment.Trim((char)0);
//                            m_CGridMotionEditor.SetCaption(nLineNum, strLineComment);
//                        }
//                        byLine = null;
//                        #endregion Caption

//                        // 'T' 'E'
//                        strData_FE += (char)(byteData[nPos++]);
//                        strData_FE += (char)(byteData[nPos++]);

//                        //                     if (bMessage == true)
//                        //                     {
//                        //                         if (strData_ME != "ME") OjwMessage("Motion Table Error\r\n");
//                        //                         else OjwMessage("Table Loaded");
//                        //                         if (strData_FE != "TE") OjwMessage("File Error\r\n");
//                        //                         else OjwMessage("Table Loaded");
//                        //                     }

                        bFileOpened = true;
                        #endregion FileOpen V1.1
                    }
                    else if (strTmp.ToUpper() == _STR_EXT.ToUpper() + _STR_VER_V_12)
                    {
                        //chkFileVersionForSave_1_0.Checked = false;
                        //chkFileVersionForSave.Checked = false;
                        #region FileOpen V1.2
                        int nPos = 6;   // 앞의 6개는 'DMT1.2' 에 할당

                        #region Header

                        #region 타이틀(21)
                        byte[] byteGetData = new byte[21];
                        for (i = 0; i < 21; i++) byteGetData[i] = 0;
                        for (i = 0; i < 21; i++)
                        {
                            if (byteData[i + nPos] == 0) break;
                            byteGetData[i] = byteData[i + nPos];
                        }
                        m_strMotionFile_TableName = System.Text.Encoding.Default.GetString(byteGetData);
                        nPos += 21;
                        byteGetData = null;
                        #endregion 타이틀(21)

                        #region Start Position(1)
                        int nStartPosition = (int)(byteData[nPos++]);
                        nStartPosition = (nStartPosition >= 0) ? nStartPosition : 0;
                        m_nMotionFile_StartPosition = ((nStartPosition > 0) ? nStartPosition : 0);
                        #endregion Start Position(1)

                        #region Size - MotionFrame(2), Comment(2), Caption(2), PlayTime(4), RobotModelNumber(2), MotorCnt(1), Motor Index(MC), Mirror Index(MC)
                        // Size
                        int nFrameSize, nCnt_LineComment, nPlayTime, nCommentSize, nRobotModelNum, nMotorCnt;
                        nFrameSize = (int)(byteData[nPos] + byteData[nPos + 1] * 256); nPos += 2;
                        nCommentSize = (int)(byteData[nPos] + byteData[nPos + 1] * 256); nPos += 2;
                        nCnt_LineComment = (int)(byteData[nPos] + byteData[nPos + 1] * 256); nPos += 2;
                        nPlayTime = (int)(byteData[nPos] + byteData[nPos + 1] * 256 + byteData[nPos + 2] * 256 * 256 + byteData[nPos + 3] * 256 * 256 * 256); nPos += 4;
                        nRobotModelNum = (int)(byteData[nPos] + byteData[nPos + 1] * 256); nPos += 2;
                        nMotorCnt = (int)(byteData[nPos++]);

                        // 모터의 인덱스
                        byte[] pbyteMotorIndex = new byte[nMotorCnt];
                        for (int nIndex = 0; nIndex < nMotorCnt; nIndex++) pbyteMotorIndex[nIndex] = byteData[nPos++];

                        // 모터의 Mirror 인덱스
                        byte[] pbyteMirrorIndex = new byte[nMotorCnt];
                        for (int nIndex = 0; nIndex < nMotorCnt; nIndex++) pbyteMirrorIndex[nIndex] = byteData[nPos++];

                        #endregion Size - MotionFrame(2), Comment(2), Caption(2), PlayTime(4), RobotModelNumber(2), MotorCnt(1), Motor Index(MC), Mirror Index(MC)

                        #endregion Header

                        // nRobotModelNum 를 읽고 해당 파일을 읽어들인다.
                        #region Header 검증
                        if (nMotorCnt != m_CHeader.nMotorCnt)
                        {
                            //if (bFile == true)
                            //{
                            //    fs.Close();
                            //    f = null;
                            //}
                            this.Cursor = System.Windows.Forms.Cursors.Default;
                            MessageBox.Show("디자이너 파일의 모터 수량과 맞지 않습니다.(요구모터수량=" + Ojw.CConvert.IntToStr(m_CHeader.nMotorCnt) + ", 모션파일에 정의된 모터수량=" + Ojw.CConvert.IntToStr(nMotorCnt) + ")\n");// 해당 모델에 맞는 모션을 로드하십시오.");
                            DialogResult dlgRet = MessageBox.Show("무시하고 계속 열겠습니까?", "파일열기", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                            if (dlgRet == DialogResult.OK)
                            {
                                //MessageBox.Show("Yes");
                                //return;
                            }
                            else return false;
                        }
                        #endregion Header 검증

                        //Grid_ChangePos(dgAngle, 0, 0);
                        //Grid_ChangePos(dgKinematics, 0, 0);
                        //GridInit(nMotorCnt, nFrameSize, false);// + 50);
                        //GridInit(nMotorCnt, _SIZE_FRAME, false);

                        for (i = nFrameSize; i < m_CGridMotionEditor.GetLineCount() - nFrameSize; i++) m_CGridMotionEditor.Clear(i);

                        #region 실제 모션
                        int nH = nFrameSize;
                        int nData;
                        short sData;
                        float fValue;
                        for (j = 0; j < nH; j++)
                        {
                            //En
                            #region Enable
                            int nEn = byteData[nPos++];
                            bool bEn = ((nEn & 0x01) != 0) ? true : false;
                            m_CGridMotionEditor.SetEnable(j, bEn);
                            #endregion Enable
                            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            #region Motor
                            int nMotorCntMax = (int)Math.Max(nMotorCnt, m_CHeader.nMotorCnt);
                            // 0-Index, 1-En, 2 ~ 24, 25 - speed, 26 - delay, 27,28,29,30 - Data0-3, 31 - time, 32 - caption
                            for (int nAxis = 0; nAxis < nMotorCntMax; nAxis++)
                            {
                                if (nAxis >= m_CHeader.nMotorCnt) nPos += 3;
                                else if (nAxis >= nMotorCnt) m_CGridMotionEditor.SetData(j, nAxis, 0.0f);// 실 모터수량과 맞지 않다면 그 부분을 0 으로 채울 것
                                else
                                {
                                    nData = (int)(BitConverter.ToInt16(byteData, nPos)); nPos += 2;
                                    sData = (short)(nData & 0x3fff);
                                    if ((nData & 0x4000) != 0) sData -= 0x1000;
                                    // 엔코더 타입((0x8000) != 0)


                                    ///////////////////////////
                                    // Reserve(2), Noaction(1), LED(3-Red Blue Green), Mode(1), Stop Bit(1)
                                    int byteTmp = byteData[nPos++];


                                    ///////////////////////////












                                    //Grid_SetFlag_Led(j, nAxis, ((nData >> 12) & 0x07));
                                    //Grid_SetFlag_Type(j, nAxis, (((nData & 0x8000) != 0) ? true : false));
                                    //Grid_SetFlag_En(j, nAxis, ((sData == 0x7ff) ? false : true));

                                    if (sData == 0x7ff)
                                    {
                                        m_CGridMotionEditor.SetData(j, nAxis, 0);
                                    }
                                    else
                                    {
                                        fValue = CalcEvd2Angle(nAxis, (int)sData);
                                        m_CGridMotionEditor.SetData(j, nAxis, fValue);
                                    }



                                    /* - Save
                                    fValue = GridMotionEditor_GetMotor(i, j);
                                    sData = (short)(OjwMotor.CalcAngle2Evd(j, fValue) & 0x03ff);
                                    //sData |= 0x0400; // 속도모드인때 정(0-0x0000), 역(1-0x0400)
                                    //sData |= LED;  // 00 - 0ff, 0x0800 - Red(01), 0x1000 - Blue(10), 0x1800 - Green(11)
                                    //sData |= 제어타입 // 0 - 위치, 0x2000 - 속도
                                    sData |= 0x4000; //Enable // 개별 Enable (0 - Disable, 0x4000 - Enable)
                                     */
                                }
                            }
                            #endregion Motor
                            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                            #region Speed(2), Delay(2), Group(1), Command(1), Data0(2), Data1(2)
                            // Speed  
                            nData = (int)(byteData[nPos] + byteData[nPos + 1] * 256); nPos += 2;
                            GridMotionEditor_SetTime(j, nData);

                            // Delay  
                            nData = BitConverter.ToInt16(byteData, nPos); nPos += 2;
                            GridMotionEditor_SetDelay(j, nData);

                            // Group  
                            nData = (int)(byteData[nPos++]);
                            GridMotionEditor_SetGroup(j, nData);

                            // Command  
                            nData = (int)(byteData[nPos++]);
                            GridMotionEditor_SetCommand(j, nData);

                            // Data0  
                            nData = (int)(byteData[nPos] + byteData[nPos + 1] * 256); nPos += 2;
                            GridMotionEditor_SetData0(j, nData);
                            // Data1  
                            nData = (int)(byteData[nPos] + byteData[nPos + 1] * 256); nPos += 2;
                            GridMotionEditor_SetData1(j, nData);
                            #endregion Speed(2), Delay(2), Group(1), Command(1), Data0(2), Data1(2)
                            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                            #region 추가한 Frame 위치 및 자세
                            nPos += 4;//SetFrame_X(j, (float)BitConverter.ToSingle(byteData, nPos)); nPos += 4;
                            nPos += 4;//SetFrame_Y(j, (float)BitConverter.ToSingle(byteData, nPos)); nPos += 4;
                            nPos += 4;//SetFrame_Z(j, (float)BitConverter.ToSingle(byteData, nPos)); nPos += 4;

                            nPos += 4;//SetFrame_Pan(j, (float)BitConverter.ToSingle(byteData, nPos)); nPos += 4;
                            nPos += 4;//SetFrame_Tilt(j, (float)BitConverter.ToSingle(byteData, nPos)); nPos += 4;
                            nPos += 4;//SetFrame_Swing(j, (float)BitConverter.ToSingle(byteData, nPos)); nPos += 4;
                            #endregion 추가한 Frame 위치 및 자세
                        }
                        #endregion 실제 모션

//#if !_COLOR_GRID_IN_PAINT
//                        m_CGridMotionEditor.SetColorGrid(0, nFrameSize);
//#endif
//                        string strData_ME = "";
//                        string strData_FE = "";

//                        // 'M' 'E'
//                        strData_ME += (char)(byteData[nPos++]);
//                        strData_ME += (char)(byteData[nPos++]);

                        //#region Comment Data
                        //// Comment
                        //byte[] pstrComment = new byte[nCommentSize];
                        //for (j = 0; j < nCommentSize; j++)
                        //    pstrComment[j] = (byte)(byteData[nPos++]);
                        //m_strMotionFile_Comment = System.Text.Encoding.Default.GetString(pstrComment);
                        //pstrComment = null;
                        //#endregion Comment Data

                        //#region Caption
                        //int nLineNum = 0;
                        //string strLineComment;
                        //byte[] byLine = new byte[46];
                        //for (j = 0; j < nCnt_LineComment; j++)
                        //{
                        //    nLineNum = (short)(byteData[nPos] + byteData[nPos + 1] * 256); nPos += 2;
                        //    for (int k = 0; k < 46; k++)
                        //        byLine[k] = (byte)(byteData[nPos++]);
                        //    strLineComment = System.Text.Encoding.Default.GetString(byLine);
                        //    strLineComment = strLineComment.Trim((char)0);
                        //    m_CGridMotionEditor.SetCaption(nLineNum, strLineComment);
                        //}
                        //byLine = null;
                        //#endregion Caption
                                                                    
                        pbyteMotorIndex = null;
                        pbyteMirrorIndex = null;

                        bFileOpened = true;
                        #endregion FileOpen V1.0
                    }
#endif
                    ////////////////////////////////////////////////////////////////////////////

                    if (bFileOpened == true) return true;
                    return false;
                }
                catch
                {
                    return false;
                }
            }


            #endregion Motion Play


            #endregion Background
#if false
            #region Control
            public void SetTorq(int nMotor, bool bOn)
            {
                if (bOn) { if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return; }
                if (bOn == false) m_aSMot[nMotor].bInit_Value = false;

                m_aSMot[nMotor].nStatus_Torq_Prev = m_aSMot[nMotor].nStatus_Torq;
                m_aSMot[nMotor].nStatus_Torq = Ojw.CConvert.BoolToInt(bOn);
                //if (nMotor < 0) 

                if (m_aCParam[nMotor].EMot != EMotor_t.SG_90)
                {
                    Write2(m_aCParam[nMotor].nProtocol, m_aCParam[nMotor].nCommIndex, ((nMotor < 0) ? 254 : (m_aCParam[nMotor].nRealID)), 0x03, m_aCAddress[nMotor].nTorq_64_1, (byte)((bOn == true) ? 1 : 0));

                }
            }
            public void SetTorq(bool bOn)
            {
                if (bOn) { if ((m_bStop == true) || (m_bEms == true) || (m_bProgEnd == true)) return; }
                for (int i = 0; i < m_aCParam.Length; i++) m_aSMot[i].bInit_Value = false;
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
                    int nProtocol = m_aCParam[anMotors[0]].nProtocol;
                    int nSerialIndex = m_aCParam[anMotors[0]].nCommIndex;
                    Writes(nProtocol, nSerialIndex, m_aCAddress[anMotors[0]].nTorq_64_1, m_aCAddress[anMotors[0]].nTorq_Size_1, anMotors.Length, pbyteBuffer);
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
                if (m_aCParam[nMotor].fLimitUp != 0) nUp = CalcAngle2Evd(nMotor, m_aCParam[nMotor].fLimitUp);
                if (m_aCParam[nMotor].fLimitDn != 0) nDn = CalcAngle2Evd(nMotor, m_aCParam[nMotor].fLimitDn);
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
                if (m_aCParam[nMotor].fLimitUp != 0) fUp = m_aCParam[nMotor].fLimitUp;
                if (m_aCParam[nMotor].fLimitDn != 0) fDn = m_aCParam[nMotor].fLimitDn;
                return Clip(fUp, fDn, fValue);
                //}
                //return fValue;
            }
            public int CalcAngle2Evd(int nMotor, float fValue)
            {
                fValue *= ((m_aCParam[nMotor].nDir == 0) ? 1.0f : -1.0f);
                int nData = 0;
                //if (Get_Flag_Mode(nAxis) != 0)   // 속도제어
                //{
                //    nData = (int)Math.Round(fValue);
                //    //Ojw.CMessage.Write("Speed Turn");
                //}
                //else
                //{
                // 위치제어
                nData = (int)Math.Round((m_aCParam[nMotor].fMechMove * fValue) / m_aCParam[nMotor].fDegree);
                nData = nData + (int)Math.Round(m_aCParam[nMotor].fCenterPos);
                //}
                return nData;
            }
            public float CalcEvd2Angle(int nMotor, int nValue)
            {
                float fValue = ((m_aCParam[nMotor].nDir == 0) ? 1.0f : -1.0f);
                float fValue2 = 0.0f;
                //if (Get_Flag_Mode(nMotor) != 0)   // 속도제어
                //    fValue2 = (float)nValue * fValue;
                //else                                // 위치제어
                //{
                fValue2 = (float)(((m_aCParam[nMotor].fDegree * ((float)(nValue - (int)Math.Round(m_aCParam[nMotor].fCenterPos)))) / m_aCParam[nMotor].fMechMove) * fValue);
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
                                if ((m_aCParam[nMotor].nProtocol == nProtocol) && (m_aCParam[nMotor].nCommIndex == nSerialIndex))
                                {
                                    //if (m_aCParam[nMotor].EMot == EMotor_t.XL_320) // XL_320 은 아직 논외로 친다.

                                    switch (nCommand)
                                    {
                                        case 0:
                                            if (m_aSMot[nMotor].bOperationMode == true)
                                            {
                                                if (nAddress < 0)
                                                {
                                                    nAddress = m_aCAddress[nMotor].nMode_Operating_11_1;
                                                    nAddress_Size = m_aCAddress[nMotor].nMode_Operating_Size_1;
                                                }

                                                if ((m_aCParam[nMotor].EMot == EMotor_t.AX_12) || (m_aCParam[nMotor].EMot == EMotor_t.AX_18) || (m_aCParam[nMotor].EMot == EMotor_t.XL_320))
                                                {
                                                    m_abyBuffer[m_nBuffer++] = (byte)Get_RealID(nMotor);
                                                    if ((m_aSMot[nMotor].EOperationMode == EOperationMode_t._Position) || (m_aSMot[nMotor].EOperationMode == EOperationMode_t._Position_Multi) || (m_aSMot[nMotor].EOperationMode == EOperationMode_t._Position_Amp))
                                                    {
                                                        m_abyBuffer[m_nBuffer++] = 0x00;
                                                        m_abyBuffer[m_nBuffer++] = 0x00;
                                                        m_abyBuffer[m_nBuffer++] = 0xff;
                                                        m_abyBuffer[m_nBuffer++] = 0x03;
                                                        if (m_aCParam[nMotor].EMot == EMotor_t.XL_320) m_abyBuffer[m_nBuffer++] = 0x02;
                                                    }
                                                    else
                                                    {
                                                        m_abyBuffer[m_nBuffer++] = 0x00;
                                                        m_abyBuffer[m_nBuffer++] = 0x00;
                                                        m_abyBuffer[m_nBuffer++] = 0x00;
                                                        m_abyBuffer[m_nBuffer++] = 0x00;
                                                        if (m_aCParam[nMotor].EMot == EMotor_t.XL_320) m_abyBuffer[m_nBuffer++] = 0x01;
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
                                                        nAddress = m_aCAddress[nMotor].nProfile_Vel_112_4;
                                                        nAddress_Size = m_aCAddress[nMotor].nProfile_Vel_Size_4;
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
                                                        nAddress_Speed = m_aCAddress[nMotor].nGoal_Vel_104_4; ;
                                                        nAddress_Speed_Size = m_aCAddress[nMotor].nGoal_Vel_Size_4;
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
                                                    nAddress = m_aCAddress[nMotor].nGoal_Pos_116_4;
                                                    nAddress_Size = m_aCAddress[nMotor].nGoal_Pos_Size_4;
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
                                                    nAddress = m_aCAddress[nMotor].nGoal_Vel_104_4;
                                                    nAddress_Size = m_aCAddress[nMotor].nGoal_Vel_Size_4;
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
#endif
        }

    }
}
