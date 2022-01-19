// Dotnet 프레임 4.0 기준이나 3.5 버전으로 컴파일 시 _USING_DOTNET_3_5 를 조건부 컴파일에 선언할 것
// Dotnet 프레임 2.0 기준시 _USING_DOTNET_2_0 를 조건부 컴파일에 선언할 것

/* This is a Open Source for Jig tools
 * 
 * made by Jinwook-On (ojw5014@hanmail.net)
 * supported by Daesung-Choi, Dongjoon-Chang (Advise)
 * supported by Chulhee-yun(Motion Download & Advise)
 * supported by Mohssin (icons)
 * supported by Aram-Lee (all of the keypad & Virtual Keyboard images)
 * supported by Donghyeon-Lee (3 dof parallel Delta robot Kinematics function made)
 * supported by JhoYoung-Choi (Motion Editor advise = beta testing = - tajo27@naver.com) & Debugging & recommend new feature(simulation... and so on)
 * supported by Daniel Park, from DevTree (Motion Editor advise = beta testing = - www.facebook.com/DevTree)
 * supported by Hyungon Kim, from Robotis (Excel command advise)
 * =================================================================================================================> Written by Jinwook, On
 */

#region 나중에 참고
#if false
http://blog.daum.net/toyship/112
마우스로 클릭한 위치의 3차원 계산
#endif
#endregion 나중에 참고

/*
 CLR에서 60초 동안 COM 컨텍스트 0x179930에서 COM 컨텍스트 0x179b58(으)로 전환하지 못했습니다. 
대상 컨텍스트/아파트를 소유하는 스레드가 펌프 대기를 수행하지 않거나, Windows 메시지를 펌프하지 않고 매우 긴 실행 작업을 처리하고 있는 것 같습니다. 
이러한 상황은 대개 성능에 부정적인 영향을 주며 응용 프로그램이 응답하지 않거나 시간이 흐름에 따라 메모리 사용이 증가하는 문제로 이어질 수도 있습니다. 
이 문제를 방지하려면 모든 STA(Single Threaded Apartment) 스레드가 펌프 대기 기본 형식(예: CoWaitForMultipleHandles)을 사용하고 긴 실행 작업 동안 지속적으로 메시지를 펌프해야 합니다.
출처: http://captainyellow.tistory.com/556 [캡틴노랑이]
 
 * => Visual Studio의 Debug 메뉴에서
      Exceptions->Managed Debugging Assistants 트리확장 > ContextSwitchDeadlock항목 Thrown 체크 풀기
 */

using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Threading;

using Tao.FreeGlut;
using Tao.OpenGl;
using Tao.Platform.Windows;
using System.Drawing.Imaging;

#region help - If you want to use my "OpenJigWare.dll"... 
#if false
if you have some errors like this...
'Tao.Platform.Windows.SimpleOpenGlControl' blablablablablabla...
 => 1. You need to copy all DLLs() to the Release folder(freeglut.dll, tao.dll, Tao.FreeGlut.dll, Tao.Ode.dll, Tao.OpenGl.dll, Tao.Platform.Windows.dll, OpenJigWare.dll)
    2. Include almost dll to the Reference(but freeglut.dll) - tao.dll, Tao.FreeGlut.dll, Tao.Ode.dll, Tao.OpenGl.dll, Tao.Platform.Windows.dll, OpenJigWare.dll
#endif
#endregion help - If you want to use my "OpenJigWare.dll"...

// Todo : 
// 1. 디자이너에서 Init, File Open, 마우스 이벤트만 연결하면 되도록 수정할 것. - Done
// 2. 클래스 구분 정의 - Done
// 3. 외부적으로 오브젝트들을 임의 추가가능하도록 기능 추가 - Done
// 4. 모델링 에서 백그라운드 색상 및 빛 계열들도 저장해서 출력시 이게 적용이 되도록 기능 추가.
// 5. 암호화 파일 상태로 저장 및 복원
// 6. 모델링 파일을 숫자가 아닌 문자로 적용되도록... - Done
// 7. 2D 모델의 픽킹 기능 추가
// 8. 2D 모델의 3D 모델링과 유사기능 적용하도록 수정.
// 9. 3D 모델링 데이타 압축 변환기능 추가


// rule 1 : Class => You must set a letter 'C' to the head(But Top class 'Ojw')
// rule 2 : Structure => Set the header letter is 'S', tail letter is '_t'.
// rule 3 : Enum => Set the header letter is 'E', tail letter is '_t'.
// rule 4 : if you make a new class, you should make yours in [User Class,-for example :COjw_19_User.cs]
//          and.. you can change it all names when you made all things completely.(check it from like COjw_00_~)
namespace OpenJigWare
{
    #region 명령 설명
#if false
    [(퀵)명령]
    <<Arg0 은 붙어 있는 명령>>
    Cmd    Arg0    Arg1
    W		안녕?   			captiondp "안녕?" 이라는 말을 넣는다.
    G		1			Group 1 로 셋팅한다.(0은 해제)		
    S		100			100 ms 에 맞춰 모터를 동작 시킨다.								
    D		100			동작 후 100 ms 멈춘다.								
    E		1			프레임 Enable(1 - Enable, 0 - Disable)								
    R	2				2번 수식의 Forward 를 풀어 메모리에 가지고 있는다.								
    N	2	10	20	30		2번 수식 사용 (10, 20, 30) 의 위치로 이동								
    I	2	5	0	-7		2번 수식 사용 x 를 마지막 사용한 값에서 5 증가, z 를 마지막 사용한 값에서 7 감소								
    T	2	10			2번 모터의 위치값을 10 으로 변경한다.								
    P	2	3			2번 모터의 위치값을 현재의 값에 "3" 을 더한다.								
    M	2	0.5			2번 모터의 위치값에 0.5 를 곱한다.								
    C	2	-5			5번 모터의 값을 2번 모터에 붙인다.(-가 붙으면 반대방향으로 붙인다.)								
    X               -1                      전체 모터 미러링
    X               1                       1번모터 미러링


    [명령]
    @HELP
    @SET_ABS,[Num],[X],[Y],[Z]
    @SET_ABS2,[Num],[X],[Y],[Z] // 모든 Motors 의 위치가 0 일 경우의 좌표점을 기준점으로 삼는 절대값
    @SET_INC,[Num],[X],[Y],[Z]
    @SET_INC1,[Num][x_y_z_0_1_2],[길이],[Rot_X_Y_Z_0_1_2],[각도]
    @SET_INC2,[Num][x],[y],[z],[Rot_X],[Rot_Y],[Rot_Z] 
    @SET_INC3,[Num],[반경(길이)],[각도],[z]   // 원통좌표계
    @INIT,-1  // 전체 초기화
    @INIT,[Line]   // Line 초기화
    @SET,[Axis],[value]   // 모터 값 설정
    @SET_PLUS,[Axis],[Value] // 모터 값 증분
    @CLEAR,[Line] // Line 모터 0 설정
    @ENTER,[{LineCount}]    // [LineCount]삭제가능, LineCount 만큼 밑으로 이동
    @SPACE,[{ColumnCount}]    // [ColumnCount]삭제가능, ColumnCount 만큼 옆으로 이동
    @SET_COMMAND,[Value]
    @SET_DATA0,[Value]
    @SET_DATA1,[Value]
    @SET_DATA2,[Value]
    @SET_DATA3,[Value]
    @SET_DATA4,[Value]
    @SET_DATA5,[Value]
    @SET_ENABLE,[Value]
    @SET_SPEED,[Value]
    @SET_DELAY,[Value]
    @SET_MULTI,[Axis],[Value] // 모터 값 곱
    @READ,[Num]    // 해당 수식의 Forward 값을 읽음
    @COPY,[ORG],[TARGET],[DIR:0] 
    @ROT,[X],[Y],[Z]  // Rot, Rotate, Rotation
    @TRANS,[X],[Y],[Z] // Trans, Translate, Translation
    @SCALE,[Value]   // 1.0 기준
#endif
    #endregion 명령 설명
    #region Version
    #region Cautions **********************************************************

    #endregion Cautions **********************************************************
    // OJW5014_20151012
    public struct SVersion_T
    {
        public const string strVersion = "02.01.02";
        public const string strHistory = (String)(
                "[V02.01.03]\r\n" +
                "  COjw_12_3D.cs -> MakeAll() 추가" + "\r\n" +
                "========================================\r\n" + // Release  
                "[V02.01.02]\r\n" +
                "  Raspberrypi Sock 기능 추가" + "\r\n" +
                "========================================\r\n" + // Release  
                "[V02.01.01]\r\n" +
                "  코드 디버깅... MakeStuff 관련" + "\r\n" +
                "========================================\r\n" + // Release  
                "[V02.01.00]\r\n" +
                "  protocol2 추가 외..." + "\r\n" +
                "========================================\r\n" + // Release  
                "[V02.00.46]\r\n" +
                "  CalcF, CalcInv, CalcAuto 등의 추가, 모델링 후 키네마틱스를 자동으로 사용할 수 있도록 환경 추가" + "\r\n" +
                "========================================\r\n" + // Release  
                "[V02.00.45]\r\n" +
                "  그래픽 고성능에서 깨져보이는 현상 수정" + "\r\n" +
                "========================================\r\n" + // Release  
                "[V02.00.44]\r\n" +
                "기준축 값 변경 명령 추가 - SetStandardAxis(bFill_true, fAlpha_1f, fThick_1, fLength_40000f)" + "\r\n" + 
                "  [ 기준축 보이기 - ex)" + "\r\n" +
                "  m_C3d.SetStandardAxis(true);" + "\r\n" +
                "  bool bFill_true = true;" + "\r\n" +
                "  float fAlpha_1f = 1.0f;" + "\r\n" +
                "  float fThick_1 = 5.0f;" + "\r\n" +
                "  float fLength_40000f = 40000.0f;" + "\r\n" +
                "  ]" + "\r\n" +
                "  m_C3d.SetStandardAxis(bFill_true, fAlpha_1f, fThick_1, fLength_40000f);" + "\r\n" +
                "========================================\r\n" +   
                "[V02.00.43]\r\n" +
                "GetData_Forward() 에서 데이타가 저장되지 않는 경우의 버그를 해결\r\n" +
                "========================================\r\n" + // Release   
                "[V02.00.42]\r\n" +
                "구형 버전의 Ojw파일에서 stl 뒤에 '*' 를 붙여 구분하는 것에 대해 소문자 파일이름만 적용되는 버그 해결\r\n" +
                "========================================\r\n" + // Release   
                "[V02.00.41]\r\n" +
                "델타 Forward 수식 디버깅\r\n" +
                "========================================\r\n" + // Release   
                "[V02.00.40]\r\n" +
                "헐크버스터 모델 추가\r\n" +
                "신규 모델링 툴 수정 - 모델링 텍스트 클릭 시 같이 이동하는 부분 적용, Forward/Inverse Kinematics 텍스트 삭제되는 현상 수정\r\n" +
                "========================================\r\n" + // Release   
                "[V02.00.39]\r\n" +
                "Motion Editor bug 해결\r\n" +
                "Motion Editor 에서 ax-12 적용 가능하도록 수정\r\n" +
                "Kinematics.cs 에서 Motor Setting 탭의 MotorName 부분 옆 Set 버튼으로 설정 자동화 부분 제작\r\n" +
                "MakeDHSkeleton 에서 자동으로 추가되는 마지막 파츠 삭제\r\n" +
                "========================================\r\n" + // Release   
                "[V02.00.38]\r\n" +
                "Motion Editor 작성 중\r\n" +
                "========================================\r\n" + // No Release   
                "[V02.00.36]\r\n" +
                "Monster2 Library 개량 및 안정화\r\n" +
                "========================================\r\n" + // No Release   
                "[V02.00.35b]\r\n" +
                "Kinematics compiler 에서 수식 버그 해결 (W변수 선언시...)\r\n" +
                "========================================\r\n" + // No Release    
                "[V02.00.34b]\r\n" +
                "RemoveCaption - 지정한 캡션으로 지우는 기능 추가\r\n" + 
                "MakeDHSkeleton 개선, 불필요한 모델링 만들어지지 않게...\r\n" +
                "Making [new design tool]\r\n" +
                "========================================\r\n" + // No Release              
                "[V02.00.33b]\r\n" +
                "Import 기능 디버깅 - 시리얼 포트가 오픈되지 않아 모터 인식이 되지 않은 경우에 null 에러가 있던 버그 해결\r\n" +
                "========================================\r\n" + // No Release  
                "[V02.00.32b]\r\n" +
                "모션툴에서 파일 오픈시 소슷점 첫째자리 반올림 하도록 수정\r\n" +
                "CM550 다운로더 버그 해결 - 사이즈 계산 미스\r\n" +
                "========================================\r\n" + // No Release  
                "[V02.00.31b]\r\n" +
                "모터명령의 버그패치, 모터초기값 버그 패치(Center 값과 MechMove 값이 바뀌어있는 문제)\r\n" +
                "scanf_answer 초기값 변경\r\n" +
                "========================================\r\n" + // No Release  
                "[V02.00.30b]\r\n" +
                "Excel 을 읽어올 때 모터의 인덱스와 실제 모터 아이디가 다른 경우도 적용, 엑셀의 항목 [덧붙임 명령어] 적용\r\n" +
                "_USING_DOTNET_3_5 적용 시 COjw_03_Message.cs 내 OjwDebugMessage() 함수에서 while 반복문을 사용해 시스템이 느려지는 현상이 있어 이를 tmpLines.CopyTo(txtOjwMessage.Lines, 0) 한줄로 변경\r\n" +
                "FileImport 문제 해결\r\n" +
                "========================================\r\n" + // No Release  
                "기존 구현한 소스와의 호환성을 위해 CMonster 클래스의 EMotor_t 순서를 CDynamixel 클래스에 맞춤(ex:_MODEL_XL_430)\r\n" +
                "========================================\r\n" + // No Release  
                "[V02.00.28b]\r\n" +
                "수식 부에서 Clear 있단 부분(#define _REMOVE_CLR_COMMAND) 원상복귀 - 누적 문제가 발생한다.\r\n" +
                "3D 모델링에서의 m_pSRot 변수를 초기 클리어 하는 부분 전에 삽입했었는데 이게 문제가 되어 초기화 위치를 다른곳으로 변경\r\n" +
                "========================================\r\n" + // Tutorial Release  
                "[V02.00.27b]\r\n" +
                "InitGLContext() 에서 Gl.glClearColor(0.0f, 0.0f, 0.0f, 1.0f) 함수의 AccessViolation 문제가 발생해\r\n " + 
                "기존 public void Init(Control ctrlMain) 함수내의 this.InitializeContexts() 를 C3c() 초기화 함수 위치로 변경, \r\n" +
                "InitGLContext() 함수보다 먼저 발생하게 하였다.\r\n" +
                "========================================\r\n" + // Tutorial Release  
                "[V02.00.26b]\r\n" +
                "CM550 모션 다운로드 기능 리피트 모션 적용\r\n" +
                "========================================\r\n" + // Tutorial Release  
                "[V02.00.25]\r\n" +
                "모션 스프레드시트 명령 기능 강화(x 명령 추가-미러링-)\r\n" +
                "R+Motion 복사기능 강화 - 클립보드 변형이 아닌 그냥 복사되게 : Control^C + R 에서 Control^R 만 해도 되게 변경, 어느 한 프레임만 선택해도 복사되게\r\n" +
                "========================================\r\n" + // Tutorial Release  
                "[V02.00.24]\r\n" +
                "Autoset 기능 강화\r\n" +
                "========================================\r\n" + // Tutorial Release  
                "[V02.00.23]\r\n" +
                "CMonster Library 타임 추정 알고리즘 수정\r\n" +
                "========================================\r\n" + // Tutorial Release  
                "[V02.00.22]\r\n" +
                "Tutorial Release - No Upload\r\n" +
                "Monster Library 의 타임추정 알고리즘 강화\r\n" +
                "========================================\r\n" + // Tutorial Release            
                "[V02.00.21]\r\n" +
                "Tutorial Release - No Upload\r\n" +
                "Joystick bug 해결\r\n" +
                "#18 Lines 기능 강화\r\n" +
                "========================================\r\n" + // Tutorial Release            
                "[V02.00.20]\r\n" +
                "Ojw.CMessage.Write -> Ojw.Log 계열 명령으로 가능하도록 함수 추가\r\n" +
                "Monster Libraty 1차 완성\r\n" +
                "Socket streaming 보완. dotnet 3.5 에서도 동작하게끔 스트리밍 조건 수정\r\n" +
                "========================================\r\n" + // No Released            
                "[V02.00.10]\r\n" +
                "CGrid 클래스 강화, CFile 클래스 일반 파일 로딩 부분 강화\r\n" +
                "CMonster 라이브러리 피드백을 제외한 제어의 완성(1차 테스트 완료, ax, xl-430의 동시제어) -> xl_320 은 아직 제외\r\n" +
                "FileImport/FileExport 개념 추가\r\n" +
                "Monster 라이브러리 제작 중\r\n" +
                "Downgrade : dotnetframework 4.0 -> 3.5 (for unity)\r\n" +
                "========================================\r\n" + // No Released            
                "[V02.00.09]\r\n" +
                "freeglut.dll 의 자동 복사\r\n" +
                "========================================\r\n" + // No Released            
                "[V02.00.08]\r\n" +
                "엑셀 호환기능 대폭강화\r\n" +
                "COjw_12_3D.cs 의 PlayFrame 내 (nLine < 0)인 경우의 버그 해결 - Play_Motion 함수가 동작을 안하는 버그가 발생했었음.\r\n" +
                "========================================\r\n" + // No Released            
                "[V02.00.07]\r\n" +
                "COjw_31_Dynamixel.cs 클래스 추가 및 다이나믹셀 프로토콜 버전 2 기능 추가(with motion tool)\r\n" + 
                "CompareDirectory_List 함수의 버그 수정\r\n" +
                "XYZ 테이블 복사(Control^B) 기능 추가\r\n" +
                "GridMotionEditor_Clear(int nLine) 함수 추가\r\n" +
                "COjw_24_System.cs 의 IsRunningProgram 에서 불필요 메세지 삭제\r\n" +
                "========================================\r\n" + // Released            
                "[V02.00.06]\r\n" +
                "음성인식 네거티브(!) 강화\r\n" +
                "COjw_29_Param.cs 의 multiline 옵션에서 substring(1) 삭제(in Param_Load())\r\n" +
                "========================================\r\n" + // Released
                "[V02.00.05]\r\n" +
                "CTts, CVoice 기능 추가\r\n" + 
                "Ojw.CSystem.ScreenKeyboard, ScreenKeyboard2 추가\r\n" + 
                "CParam 의 멀티라인 저장 기능 추가\r\n" +
                "CParam 의 에러처리 추가\r\n" + 
                "StreamServer 에 SetResolution, GetResolution 추가\r\n" +
                "COjw_16_Camera.cs 의 Init 을 control 이 없는 경우 내부 메모리로 사용하도록 수정, 스트리밍 서버 선언 시 에러 해결\r\n" +
                "COjw_30_Voice.cs 추가\r\n" +
                "CSystem.cs 에 RunProgram 기능 강화(Run시 특정 위치-패널-에 실행프로그램 도킹), KillProgram 기능 강화\r\n" +
                "CSystem.cs 에 ScreenKeyboard, GetPath_Windows, MoveProgram 추가\r\n" +
                "_USING_DOTNET_2_0 추가: Cannot use => Python, Streaming(?)시 버전이 맞지 않아 삭제처리\r\n" + 
                "System 클래스의 디스크 용량 알아내는 함수 추가\r\n" +
                "File 클래스의 파일속성 사이즈 알아내는 함수 추가\r\n" +
                "CSystem.Shutdown 추가(그동안 추가했던 것으로 착각... -_-;)\r\n" +
                "CParam 추가(TextBox, ComboBox, CheckBox, RadioButton)\r\n" +
                "CSystem 의 KillProgram 추가\r\n" + 
                "CSystem 의 IsRunning... & RunProgram..., SendMessage, 기능 강화(Case unsensitive)\r\n" +
                "COjw_16_Camera.cs 의 Grab() 강화\r\n" +
                "========================================\r\n" + // No Released
                "[V02.00.04]\r\n" +
                "SendMessage, Shared Memory 구조 지원\r\n" +
                "CKeyboard 클래스 추가\r\n" +
                "CMouse 에서 Mouse_Get 함수 추가\r\n" + 
                "COjw_27_Herkulex2.cs 의 CalcLimit_Evd 함수의 {nValue &= 0x3fff;} 를 삭제\r\n" + // 이게 0402/0602 제어시 에러유발 (나중에 리눅스 버전도 수정할 것)
                "rob_model2_*.sstl 파일들 추가\r\n" + 
                "Joystick 보완\r\n" +
                "Tools 프로젝트의 기능 추가(서버 스트리밍, 웹캠영상출력, 클라이언트 스트리밍 보완\r\n" +
                "COjw_16_Emgu.cs -> COjw_16_Camera.cs 로 변경, AForge Camera Capture 기능 추가\r\n" +
                "COjw_26_Streaming.cs 에 CStream_Server Class 삽입. MJPEG streaming 전송기능 추가\r\n" +
                "========================================\r\n" + // No Released
                "[V02.00.03]\r\n" +
                "모션툴 휠 제어 버그 수정\r\n" +
                "CHerculex.cs의 Stop 기능 수정 (SetMot_Stop() 추가) 수정\r\n" +
                "========================================\r\n" + // Released
                "[V02.00.02]\r\n" +
                "[OJW5014_20161031]모션툴 Insert,Delete 관련 버그 수정 - CellEnter event 내에 m_CGridMotionEditor.SetChangeCurrentLine() 함수 삽입\r\n" +
                "========================================\r\n" + // No Released
            
                "[V02.00.01]\r\n" +
                "모션툴 이벤트 중복실행 버그 수정\r\n" +
                "========================================\r\n" + // No Released
                "[V02.00.00]\r\n" +
                "개발자대회 응시 최종\r\n" +
                " - 칼만필터 알고리즘 추가(장동준 주임연구원 LG)\r\n" +
                "========================================\r\n" + // Released
                "[V01.02.05]\r\n" +
                "IronPython import 기능 강화\r\n" +
                "RmtFileOpen(), RmtFileSave() 함수 추가 - 로보링크 모션 파일포맷\r\n" +
                "========================================\r\n" + // NoReleased

                "[V01.02.04]\r\n" +
                "CTools_MouseDoubleClick() 내의 더블클릭시 모델링 확대/축소 시 축소가 안되는 문제 해결\\r\n" +
                "IronPython 을 이용한 Inverse Kinematics 수식 기능 추가\r\n" +
                "socket(server,client) 기능 강화 - stringpacket 기능도 추가\r\n" +
                "CHerkulex2 class 추가\r\n" +  
                "CConvert 에 IsValidAlpha() 함수 추가 - 유효한 문자테이블인지 검사\r\n" +
                "GetSerialPort 함수 수정. 시리얼 포트 검색조건 변경\r\n" + 
                "MotionTool Scale 수정 중...\r\n" +
                "조이스틱 데이타 예제 Tool 에 작성\r\n" +
                "========================================\r\n" + // NoReleased
                
                "[V01.02.03]\r\n" +
                "COjw_26_Streaming.cs 추가(AForge.Controls.dll, AForge.Video.DirectShow.dll, AForge.Video.dll 파일 추가)\r\n" + 
                "Joystick 의 GetPos() 함수 추가 - XBox 에 특화된 데이타를 정상화\r\n" +
                "모션툴에서 F2(해당 모터 Servo(1,3,5 등 홀수회) / Driver(2,4,6 등 짝수회) Off, F3 - 해당 모터 Servo/Driver On 기능 추가 - \r\n" + 
                "// 20160420 - 0 번 모터를 선택시 전체 그룹이 선택되는 문제 해결\r\n" + 
                "ReadMot_Angle() 기능 추가\r\n" + 
                "========================================\r\n" + // Released
                "[V01.02.02]\r\n" +
                "CTools_Keyboard 클래스와 ShowKeyboard 함수 기능 추가, 즉, 가상키보드 기능 업그레이드\r\n" +
                "========================================\r\n" + // Released
                "[V01.02.01]\r\n" +
                "SetMot_WIthInverseKinematics() 함수 기능 추가, C3d 함수이기 때문에 모델링에서 바로 움직임 구현 가능\r\n" +
                "MotionTool 에서 다운로드 탭에 Play 버튼이 아무것도 선택 안될시 에러나는 버그 수정\r\n" +
                "C2D 에서 TextC 함수 기능 추가\r\n" + 
                "fAlpha 값이 전체 변경시에만 변경되고 개별 오브젝트가 변경시에 적용이 안되던 버그 해결\r\n" +
                "========================================\r\n" + // NoReleased
                "[V01.02.00]\r\n" +
                "========================================\r\n" + // Released            
                "[V01.01.65]\r\n" +
                "MotionEditor: Append, 탐색기 기능 추가, 디자인 변경(오로카 이미지 추가)\r\n" +
                "Rps,TrackRps 기능 개선 - 실제 RPS 속도에 맞추어 개선\r\n" +
                "Motion File Download 기능 추가 완료(Delete, GetFileList, Download, Run)\r\n" +
                "Security modeling file 인 SSTL 추가, 사용자가 파일을 만들 수는 있으나 복구할 수는 없음.\r\n" + 
                "OjwFileOpen_3D_STL() 함수 디버깅 - 모델링 데이타 줄 그어지는 문제 해결\r\n" + 
                "Dotnet Frame work 3.5 -> 4.0 변경(프레임 3.5 재변경시 변경 후 [프로젝트]-[속성]-[빌드]-조건부 컴파일 기호 //_USING_DOTNET_3_5 를 주석해제하면 된다.\r\n" +
                "========================================\r\n" + // NoReleased            
                "[V01.01.64]\r\n" +
                "InitData() 함수 내에서 strDispObject 의 초기값 #0 을 #7로 변경\r\n" +
                "GetMouseMode(): 4~6, 7~9, 10~12, 13~15 [Offset(Trans,Rot), Position(Trans1,Rot1) 기능 추가, 실시간 변경을 위해 OjwDraw() 함수를 MouseMove 이벤트에 추가\r\n" +
                "Drag and Drop feature 추가\r\n" +
                "Drag and Drop 으로 3D Modeling File(stl, ase, obj) file copy or Load feature 추가\r\n" +
                "========================================\r\n" + // Released            
                "[V01.01.63]\r\n" +
                "BinaryFileOpen() 함수 추가\r\n" +
                "PlayFrame() 에서 LED 등의 기능이상부분 버그 처리(debugging advise - JhoYoung-Choi : tajo27@naver.com\r\n" + 
                "Motion_Play(), Motion_Stop(), Motion_Reset() 함수 추가 - Motion file 을 programming 으로 run 할수 있도록 feature 추가\r\n" +
                "BinaryFileOpen() 함수 추가\r\n" + 
                "m_CGridMotionEditor 이 null 일경우 모델링 파일이 불러지지 않는 버그 패치\r\n" + 
                "MotionEditor 의 Simulation 기능 추가(체크박스 체크 해제시 단일 프레임 시뮬레이션)\r\n" +
                "MotionEditor 의 파일복구 기능 버그 수정\r\n" +
                "MotionEditor 에서 Control^C,E 하고 엑셀 같은곳은 Control^V 시 Evd(Engineering value of degree : raw data) 로 데이타가 전환되어 붙여넣기가 가능\r\n" +
                "CTimer 의 Class Get() 함수 return value 가 double 로 되어 있는 것을 long 으로 수정\r\n" +
                "COjwMotor class 내의 m_nRxIndex 의 초기값을 255로 해 두어야 하는데 0으로 해놓은 오타 덕에 초기 1번의 데이터 Get 이 Error 를 일으키는 문제 해결\r\n" +
                "내부의 CTimer static 메모리를 사용하는 부분을 전부 클래스 변수로 전환\r\n" +
                "========================================\r\n" + // Released            
                "[V01.01.62]" + "\r\n" +
                "selectmotor 함수에 외견상 선택뿐 아니라 실제 모터선택도 같이 되도록 수정" + "\r\n" +
                "Motion Tool Upgrade" + "\r\n" +
                "Motion Tool 에 multimedia 기능 삽입" + "\r\n" +
                "========================================\r\n" + // NoReleased            
                "[V01.01.61]" + "\r\n" +
                "Forward Kinematics 자동 생성 함수에 오타성 버그 발견 및 수정, Inverse Kinematics 내부 메모리 확장" + "\r\n" +
                "IsDigit 기능 강화(부호, 소숫점 판단)" + "\r\n" + 
                "CalcDhParamAll_ToString() 함수에서 자동으로 수식 만들어 주는 부분에 방향이 반대인 경우의 수식 계산과정을 스트링으로 만들어주는 부분 적용" + "\r\n" + 
                "ShowTools_Modeling() 에서 Prop_Set_Main_MouseControlMode(0) 을 삽입, 마우스 모드를 제어 모드가 아닌 이동 모드로 시작하도록 구현" + "\r\n" +
                "========================================\r\n" + // Released            
                "[V01.01.61]" + "\r\n" +
                "SetParam_Axis, SetSpeedType 부분을 CMotor 뿐만 아니라 CHerculex 에도 삽입" + "\r\n" + 
                "CConvert 의 FloatsToDoubles, DoublesToFloats 의 함수 call 위치 변경" + "\r\n" +
                "Kinematics 에 SetValue_V, SetValue_Motor 의 Doubles -> floats 추가" + "\r\n" +
                "IsRunningProgram() 함수 추가" + "\r\n" +
                "CreateProb_VirtualObject, CreateProp_Selected 의 argument 를 Panel -> Control 로 변경" + "\r\n" +
                "CreateProb_VirtualObject -> CreateProp_VirtualObject으로 이름 변경(현재 혼용사용 가능)" + "\r\n" + 
                "DataFileOpen() 함수 구현" + "\r\n" +
                "OjwMouseDown 수정 - 클릭시 선택된 Kinematics 수식 번호가 이전 것으로 되어 있던 버그 수정" + "\r\n" + 
                "Socket Connect 함수를 랙이 생기지 않게 BeginConnect 로 변경" + "\r\n" +
                "C2d 클래스에서 Load 항목 추가 - 기존 색으로 지우던 부분을 로드한 파일로 대치하도록 기능추가." + "\r\n" +
                "GridMotionEditor_Init() 에서 에러 메세지 삭제" + "\r\n" +
                "ShowTools_Modeling() 의 scale 기능 보완 - 현재 0.5 배율까지 확인" + "\r\n" +
                "SelectMotor_Sync_With_Mouse() 함수 추가 - 마우스 클릭시 기존 선택된 selectedmotor 를 해제한다." + "\r\n" +
                "SetMousePickEnable() 함수 추가 - 마우스 클릭시에 색이 변하는 걸 막을 수 있다." + "\r\n" +
                "========================================\r\n" + // NoReleased            
                "[V01.01.59]" + "\r\n" +
                "MakeDHSkeleton() 함수에서 라인피드 문자 추가. 이게 없어 버그 있었음" + "\r\n" +
                "COjw_03_Message.cs 에 SaveImageFile() 기능 추가" + "\r\n" +
                "ShowTools_Modeling 기능의 Scale 기능에서 내부 사이즈를 변동에서 고정값으로 변경(화면잘림현상 해결)" + "\r\n" +
                "========================================\r\n" +
                "[V01.01.58]" + "\r\n" +
                "InitTools_Kinematics() 함수가 여러번 호출되는 부분 수정(DH 파라미터 정의시 2번씩 정의되는 버그 수정) - Key: OJW5014_20150922" + "\r\n" +
                "COjw_03_Message.cs 에 SaveImageFile() 기능 추가" + "\r\n" +
                "ShowTools_Modeling 기능의 Scale 기능에서 내부 사이즈를 변동에서 고정값으로 변경(화면잘림현상 해결)" + "\r\n" +
                "========================================\r\n" +
                "[V01.01.57]" + "\r\n" +
                "InitTools_Kinematics() 함수가 여러번 호출되는 부분 수정(DH 파라미터 정의시 2번씩 정의되는 버그 수정) - Key: OJW5014_20150922" + "\r\n" +
                "COjw_03_Message.cs 에 SaveImageFile() 기능 추가" + "\r\n" +
                "ShowTools_Modeling 기능의 Scale 기능에서 내부 사이즈를 변동에서 고정값으로 변경(화면잘림현상 해결)" + "\r\n" +
                "========================================\r\n" +
                "[V01.01.56]" + "\r\n" +
                "ShowTools_Modeling() 종료 후 다시 call 할 경우 에러나는 문제 해결" + "\r\n" + 
                "COjw_10_Kinematics.cs 에 CalcCmd() 에 Error Exception 추가(IsNan, IsInfinity 대응)" + "\r\n" +
                "화면 이동(View Change)을 직관적으로 변경 with Mouse" + "\r\n" +
                "CalcDhParamAll_ToString 에 Theta 가 아닌 D 값이 모터 변경값일 경우의 수식 적용 되도록 수정" + "\r\n" +
                "(ShowTools_Modeling())3D Modeling Tool 스케일 조정 후 출력 시 깨지는 현상 수정" + "\r\n" +
                "m_txtKinematicsSkeleton 및 Skeleton 기능 추가 및 관련 object들 추가" + "\r\n" +
                "========================================\r\n" +
                "[V01.01.55]" + "\r\n" +
                " - 3D 에서의 CalcLimit 함수를 private 에서 public 으로 전환" + "\r\n" +
                " - Joystick : IsDown_Event, IsUp_Event 기능 추가" + "\r\n" +
                "========================================\r\n" +
                "[V01.01.54]" + "\r\n" +
                " - 가상키보드 기능 추가" + "\r\n" +
                "========================================\r\n" +
                "[V01.01.53]" + "\r\n" +
                " - 궤도(가변 track) 기능 추가" + "\r\n" +
                " - CTools 에서 편집 기능 강화" + "\r\n" +
                " - Ojw.CMessage.Init() 시 version 기록" + "\r\n" +
                " - XBox Joystick 해석 추가" + "\r\n" +
                "========================================\r\n" +
                "[V01.01.52]" + "\r\n" +
                " - 궤도 집어넣는 중" + "\r\n" +
                " - CTools 클래스 추가 : ShowTools_Modeling() 기능 추가(3D 모델링 화면 만들어 줌)" + "\r\n" +
                " - territory 기능 추가" + "\r\n" +
                " - Perspective & Normal 상태의 뷰값 조정" + "\r\n" +
                " - m_txtDraw(TextBox) -> m_rtxtDraw(RichTextBox) 로 수정" + "\r\n" +
                "========================================\r\n" +
                "[V01.01.51]" + "\r\n" +
                " - File Open 메뉴 선택 시 InitTools_Kinematics 가 사전에 실행되지 않은 경우 문제 발생 부분 수정" + "\r\n" +
                "========================================\r\n" +
                "[V01.01.50]" + "\r\n" +
                " - Fixed some picking bug" + "\r\n" +
                " - Forward 및 Inverse 에디터 자동생성 기능 추가" + "\r\n" +
                " - GridMotionEditor_Event_CellEnter() 함수 내부에서 SelectObject 부분 삭제(잘못된 부분)" + "\r\n" +
                " - 3D View 전환 기능 추가" + "\r\n" +
                "   -> IsPerspectiveMode(), SetPerspectiveMode() 함수 추가, Property 에 3D View 추가" + "\r\n" +
                " - cmbDhRefresh() 함수 FileOpen() 에 추가" + "\r\n" +
                " - 수동으로 파일 오픈시에도 txtDraw에 오픈되는게 적용되게..." + "\r\n" +
                "   -> m_txtDraw.Text = GetHeader_strDrawModel(); 구문을 FileOpen() 함수에 추가" + "\r\n" +
                "   -> FileOpened 에만 있던 StringListToGrid() 함수를 FileOpen() 함수에 추가" + "\r\n" +
                " - m_mat_ambient 수치 0.7 -> 0.5 로 다운 Light2에 의한 밝기를 더 어둡게" + "\r\n" +
                " - OjwDebugMessage 함수 에러처리 추가" + "\r\n" +
                " - glBlendFunc 에서 블랜딩 효과 변동" + "\r\n" +
                " - kinematics 수식에 % (나머지) 기능 추가" + "\r\n" +
                "========================================\r\n" +
                "[V01.01.40]" + "\r\n" +
                " - Fixed some picking bug" + "\r\n" +
                " - Removed glViewPoint() fungtion" + "\r\n" +
                "========================================\r\n" +
                "[V01.01.30]" + "\r\n" +
                " - Invoke feature added for threading" + "\r\n" +
                "========================================\r\n" +
                "[V01.01.20]" + "\r\n" +
                " - 12_3D.cs - Init(Panel -> Control), InitProperty...(Label -> Control)" + "\r\n" +
                "========================================\r\n" +
                "[V01.01.10]" + "\r\n" +
                " - Message Param feature added" + "\r\n" +
                "========================================\r\n" +
                "[V01.01.00]" + "\r\n" +
                " - Joystics features added" + "\r\n" +
                " - COjw_xx_Users.cs file added" + "\r\n" +
                "========================================\r\n" +
                "[V01.00.10]" + "\r\n" +
                " - Debugging and some features are added" + "\r\n" + 
                "========================================\r\n" +
                "[V01.00.00]" + "\r\n" +
                " - build" + "\r\n" +
                "========================================\r\n" +
                "[Build History]\r\n" +
                "========================================");
    }
    #endregion Version

    #region Class
    #region MotorInfo (SMotorInfo_t)
    public struct SMotorInfo_t // Motor information
    {
        public int nMotorID;                    // Motor ID
        public int nMotorDir;                   // Direction of Axis (0 - forward, 1 - inverse)
        public float fLimit_Up;                 // Max Angle(+)
        public float fLimit_Down;               // Min Angle(-)
        public int nCenter_Evd;                 // Pulse(Engineering value for 0 degree(Center Position))

        public int nMechMove;                   // Maximum Position ( Maximum Pulse value(Evd) )
        public float fMechAngle;                // It is a Degree when it moves in Maximum Position

        public float fInitAngle;                // Init position which you want it
        public float fInitAngle2;               // Init position which you want it(2'st)


        // Interference Axis(No use)
        public int nInterference_Axis;          // reserve(No use) - 이게 (-)값이면 간섭 축 없음.
        public float fW;                        // reserve(No use) - Side 에 붙은 축의 크기(넓이)
        public float fInterference_W;           // reserve(No use) - 간섭축이 앞뒤로 붙었다고 가정하고 해당 간섭축의 크기(넓이)

        public float fPos_Right;                // reserve(No use) - 축의 오른쪽 위치
        public float fPos_Left;                 // reserve(No use) - 축의 왼쪽 위치

        public float fInterference_Pos_Front;   // reserve(No use) - 해당 간섭축의 앞쪽 위치
        public float fInterference_Pos_Rear;    // reserve(No use) - 해당 간섭축의 뒤쪽 위치

        // NickName
        public String strNickName;              // Nickname(32char)

        public int nGroupNumber;                // Group Number

        public int nAxis_Mirror;                // 0 ~ 253 : Motor ID of Mirroring one
                                                // -1      : there is no mirror motor.
                                                // -2 : there is no mirror motor(but it can has flip-direction(for using center), flip it from '0')

        public int nMotorControlType;           // Motor Control type => 0: Position, 1: Speed type
        
        /////////// 추가
        public float fRpm;//0.229f); // 기본 rpm 단위
        public int nLimitRpm_Raw;//415);
        public int nProtocolVersion;//2); // Version 2(0 해도 동일)
        public int nHwMotor_Index; // 0 : None, 1 : xl-320, 2 : xl_430(Default), 3 - ax-12 => 지금은 이거아님, Monster2 클래스에서 확인(dicMonster)
        public int nHwMotor_Key;//1060); // 0번지에 모델번호 1060, XM430_W210 : 1030, XM430_W350 : 1020
        public int nAddr_Max;//146);
        public int nAddr_Torq;//64);
        public int nAddr_Led;//65);
        public int nAddr_Mode;//10); // 430 -> 10 address    [0 : 전류, 1 : 속도, 3(default) : 관절(위치제어), 4 : 확장위치제어(멀티턴:-256 ~ 256회전), 5 : 전류기반 위치제어, 16 : pwm 제어(voltage control mode)]
        public int nAddr_Speed;//104); // 430 -> 104 4 bytes
        public int nAddr_Speed_Size;//4);
        public int nAddr_Pos_Speed;//112); // 430 -> 112 4 bytes
        public int nAddr_Pos_Speed_Size;//4);
        public int nAddr_Pos;//116); // 430 -> 116 4 bytes
        public int nAddr_Pos_Size;//4);
        public int nSerialType;  // 0 : Default, 1 : Second ... (동시에 2개 이상의 시리얼에 연결된 경우 사용)

        public int nMotorEnable_For_RPTask; // 0: Dontcare, 1: Enable, -1: Disable(이게 -1 이면 로보티즈 모션으로 복사 시 해당 값을 무시한다.)
        public int nMotor_Enable; // 0: Dontcare, 1: Enable, -1: Disable => 이게 Disable 이면 모터 표시를 죽인다.
        public int nMotionEditor_Index; // 0 이면 사용 안함. 1 부터 사용, 0 이상인 경우 여기의 인덱스를 우선적으로 적용, 하나를 세팅했으면 반드시 다른 하나도 세팅할 것
        public int nMotor_HightSpec; // 0 : Default, 1 : HightSpec Motor(Pro)
        public int nReserve_4;
        public int nReserve_5;
        public int nReserve_6;
        public int nReserve_7;
        public int nReserve_8;
        public int nReserve_9;
        public float fGearRatio; // 2차 기어비
        public float fRobotisConvertingVar; // 로보티즈 모델링 매칭을 위한 컨버팅 변수
        public float fReserve_2;
        public float fReserve_3;
        public float fReserve_4;
        public float fReserve_5;
        public float fReserve_6;
        public float fReserve_7;
        public float fReserve_8;
        public float fReserve_9;
        ///////////////////////////////
        public int nGuide_Event;
        public int nGuide_AxisType;
        public int nGuide_RingColorType;
        public float fGuide_RingSize;
        public float fGuide_RingThick;
        public int nGuide_RingDir;
        public float fGuide_3D_Scale;
        public float fGuide_3D_Alpha;

        public float[] afGuide_Pos;// = new float[6]; // x/y/z/p/t/s
        public int[] anGuide_Off_IDs;// = new int[6]; // x/y/z/p/t/s
        public int[] anGuide_Off_Dir;// = new int[6]; // x/y/z/p/t/s
    }
    #endregion MotorInfo (SMotorInfo_t)
    #region GroupInfo (SGroupInfo_t)

    public struct SGroupInfo_t // Group information
    {
        public string strName;
        public int nNumber;
    }
    #endregion GroupInfo (SGroupInfo_t)
    #region DH Parameter (SDhT_t, CDhParam, CDhParamAll)
    public class CDhParam
    {
        public CDhParam() // Constructor
        {
            //nFunction = -1; // 이게 0 이상이면 Forward 수식 대신 Inverse 수식의 지정 함수가 대신 동작한다.
            nInit = 0;
            dA = 0;
            dD = 0;
            dTheta = 0;
            nAxisNum = -1;
            nAxisDir = 0;
            nStartGroup = 0;
            dOffset_X = 0;
            dOffset_Y = 0;
            dOffset_Z = 0;
            strCaption = "";

            nFunctionNumber = -1;
            nFunctionNumber_AfterCalc = -1; // 모든 수식계산이 끝나고 이 수식이 0이 아니면 한번 더 수식 계산을 진행한다.
            nAxisNum2 = -1;

            lstEndpoint.Clear();
        }

        //public int nFunction;
        public int nInit;
        public double dA;
        public double dD;
        public double dTheta;
        public double dAlpha;
        public int nAxisNum; // Motor Number(Similar with Virtual ID) : It means there is no motor when it use minus value(-)
        public int nAxisNum2; // 영향을 끼치는 모터 번호를 지정(-1: 지정안함(default))
        public int nAxisDir; // 0 - Forward, 1 - Inverse
        public int nAxisDir2; // 0 - Forward, 1 - Inverse(nAxisNum2 모터에 더할지(0), 뺄지(1) 를 결정
        public int nStartGroup;
        public double dOffset_X;
        public double dOffset_Y;
        public double dOffset_Z;

        public List<int> lstEndpoint = new List<int>();

        public int nFunctionNumber; // -1 이 기본, 수식번호, 0보다 작거나 255면 사용 안함,
        public int nFunctionNumber_AfterCalc; // -1 이 기본, 수식번호, 0보다 작거나 255면 사용 안함,
        public string strCaption;
        public void InitData()
        {
            // SetData(-1, 0, 0, 0, 0, 0, -1, 0, 0, 0, 0, 0, "");
            SetData(0, 0, 0, 0, 0, -1, 0, 0, 0, 0, 0, "", -1, -1);
        }
        public void SetData(CDhParam OjwDhParam)
        {
            //SetData(OjwDhParam.nFunction, OjwDhParam.nInit, OjwDhParam.dA, OjwDhParam.dD, OjwDhParam.dTheta, OjwDhParam.dAlpha, OjwDhParam.nAxisNum, OjwDhParam.nAxisDir, OjwDhParam.nStartGroup, OjwDhParam.dOffset_X, OjwDhParam.dOffset_Y, OjwDhParam.dOffset_Z, OjwDhParam.strCaption);
            SetData(OjwDhParam.nInit, OjwDhParam.dA, OjwDhParam.dD, OjwDhParam.dTheta, OjwDhParam.dAlpha, OjwDhParam.nAxisNum, OjwDhParam.nAxisDir, OjwDhParam.nStartGroup, OjwDhParam.dOffset_X, OjwDhParam.dOffset_Y, OjwDhParam.dOffset_Z, OjwDhParam.strCaption, OjwDhParam.nFunctionNumber, OjwDhParam.nFunctionNumber_AfterCalc, OjwDhParam.lstEndpoint);
        }
        //public void SetData(int nDh_FunctionNumber, int nDh_Init, double dDh_a, double dDh_d, double dDh_theta, double dDh_alpha, int nDh_AxisNum, int nDh_AxisDir, int nDh_StartGroup, double dDh_Offset_X, double dDh_Offset_Y, double dDh_Offset_Z, string strDh_Caption)
#if true        
        public void SetData(int nDh_Init, double dDh_a, double dDh_d, double dDh_theta, double dDh_alpha, int nDh_AxisNum, int nDh_AxisDir, int nDh_StartGroup, double dDh_Offset_X, double dDh_Offset_Y, double dDh_Offset_Z, string strDh_Caption)
        {
            SetData(nDh_Init, dDh_a, dDh_d, dDh_theta, dDh_alpha, nDh_AxisNum, nDh_AxisDir, nDh_StartGroup, dDh_Offset_X, dDh_Offset_Y, dDh_Offset_Z, strDh_Caption, -1, -1);
        }
#endif
        public void SetData(int nDh_Init, double dDh_a, double dDh_d, double dDh_theta, double dDh_alpha, int nDh_AxisNum, int nDh_AxisDir, int nDh_StartGroup, double dDh_Offset_X, double dDh_Offset_Y, double dDh_Offset_Z, string strDh_Caption, int nDh_FunctionNumber = -1, int nDh_FunctionNumber_AfterCalc = -1, List<int> lstDh_Endpoint = null)
        {
            //nFunction = nDh_FunctionNumber;
            nInit = nDh_Init;
            dA = dDh_a;
            dD = dDh_d;
            dTheta = dDh_theta;
            dAlpha = dDh_alpha;
            nAxisNum = nDh_AxisNum;
            nAxisDir = nDh_AxisDir;
            nStartGroup = nDh_StartGroup;
            dOffset_X = dDh_Offset_X;
            dOffset_Y = dDh_Offset_Y;
            dOffset_Z = dDh_Offset_Z;
            strCaption = strDh_Caption;
            nFunctionNumber = nDh_FunctionNumber;
            nFunctionNumber_AfterCalc = nDh_FunctionNumber_AfterCalc;
            lstEndpoint = lstDh_Endpoint;
        }
    }
    public class CDhParamAll
    {
        public CDhParamAll() // Constructor
        {
            m_pnDir[0] = 0;
            m_pnDir[1] = 0;
            m_pnDir[2] = 0;
            lstIDs.Clear();
            lstFunctions.Clear();
            //lstFunctions_AfterCalc.Clear();
        }
        private CDhParam[] pSDhParam;
        public int GetCount() { return (pSDhParam == null) ? 0 : pSDhParam.Length; }
        private int m_nAxis_X = 0;
        private int m_nAxis_Y = 1;
        private int m_nAxis_Z = 2;

        private List<int> lstIDs = new List<int>();
        private List<int> lstFunctions = new List<int>();
        //private List<int> lstFunctions_AfterCalc = new List<int>();
        public int[] GetMotors() { return lstIDs.ToArray(); }
        public int GetMotors_Count() { return lstIDs.Count; }
        public int[] GetFunctions() { return lstFunctions.ToArray(); }
        public int GetFunctions_Count() { return lstFunctions.Count; }
        //public int[] GetFunctions_AfterCalc() { return lstFunctions_AfterCalc.ToArray(); }
        //public int GetFunctions_Count_AfterCalc() { return lstFunctions_AfterCalc.Count; }

        private int[] m_pnDir = new int[3];
        public void SetAxis_XYZ(int nX, int nX_Dir, int nY, int nY_Dir, int nZ, int nZ_Dir) // Define Motor Axis Number(Default 0, 1, 2)
        {
            if (((nX * nX + nY * nY + nZ * nZ) == 5) && (((nX + 1) * (nY + 1) * (nZ + 1)) == 6))
            {
                if (((nX_Dir >= 0) && (nX_Dir <= 1)) && ((nY_Dir >= 0) && (nY_Dir <= 1)) && ((nZ_Dir >= 0) && (nZ_Dir <= 1)))
                {
                    m_nAxis_X = nX; m_nAxis_Y = nY; m_nAxis_Z = nZ;
                    m_pnDir[m_nAxis_X] = nX_Dir;
                    m_pnDir[m_nAxis_Y] = nY_Dir;
                    m_pnDir[m_nAxis_Z] = nZ_Dir;
                }
            }
        }
        public void GetAxis_XYZ(out int nX, out int nX_Dir, out int nY, out int nY_Dir, out int nZ, out int nZ_Dir)
        {
            nX = m_nAxis_X; nY = m_nAxis_Y; nZ = m_nAxis_Z;
            nX_Dir = m_pnDir[m_nAxis_X];
            nY_Dir = m_pnDir[m_nAxis_Y];
            nZ_Dir = m_pnDir[m_nAxis_Z];
        }
        public CDhParam GetData(int nIndex)
        {
            if ((nIndex >= pSDhParam.Length) || (nIndex < 0)) return null;
            else return pSDhParam[nIndex];
        }
        public bool SetData(int nIndex, CDhParam OjwDhParam)
        {
            if ((nIndex >= pSDhParam.Length) || (nIndex < 0)) return false;
            /////////////////
            int nNumOld = pSDhParam[nIndex].nFunctionNumber;
            if (nNumOld == 255) nNumOld = -1;

            int nNumNew = OjwDhParam.nFunctionNumber;
            if (nNumNew == 255) nNumNew = -1;

            int nNumOld_AfterCalc = pSDhParam[nIndex].nFunctionNumber_AfterCalc;
            int nNumNew_AfterCalc = OjwDhParam.nFunctionNumber_AfterCalc;

            if (nNumOld >= 0)
            {
                if (nNumNew >= 0)
                {
                    if (nNumOld != nNumNew)
                    {
                        if (lstFunctions.IndexOf(nNumNew) < 0)
                        {
                            lstFunctions.Add(nNumNew);
                            //lstFunctions_AfterCalc.Add(nNumNew_AfterCalc);
                        }
                    }
                }
                else
                {
                    lstFunctions.Remove(nNumOld);
                    //lstFunctions_AfterCalc.Remove(nNumOld_AfterCalc);
                }
            }
            else
            {
                if (nNumNew >= 0)
                {
                    if (lstFunctions.IndexOf(nNumNew) < 0)
                    {
                        lstFunctions.Add(nNumNew);
                        //lstFunctions_AfterCalc.Add(nNumNew_AfterCalc);
                    }
                }
            }

            /////////////////
            if (pSDhParam[nIndex].nAxisNum >= 0)
            {
                if (OjwDhParam.nAxisNum >= 0)
                {
                    if (pSDhParam[nIndex].nAxisNum != OjwDhParam.nAxisNum)
                    {
                        if (lstIDs.IndexOf(OjwDhParam.nAxisNum) < 0)
                        {
                            lstIDs.Add(OjwDhParam.nAxisNum);
                        }
                    }
                }
                else
                {
                    lstIDs.Remove(pSDhParam[nIndex].nAxisNum);
                }
            }
            else
            {
                if (OjwDhParam.nAxisNum >= 0)
                {
                    if (lstIDs.IndexOf(OjwDhParam.nAxisNum) < 0)
                    {
                        lstIDs.Add(OjwDhParam.nAxisNum);
                    }
                }
            }
            pSDhParam[nIndex].SetData(OjwDhParam);
            return true;
        }
        public bool AddData(CDhParam OjwDhParam)
        {
            int nCnt = (pSDhParam == null) ? 1 : pSDhParam.Length + 1;
            Array.Resize(ref pSDhParam, nCnt);
            pSDhParam[nCnt - 1] = new CDhParam();
            pSDhParam[nCnt - 1].SetData(OjwDhParam);
            if (pSDhParam[nCnt - 1].nAxisNum >= 0)
            {
                if (lstIDs.IndexOf(pSDhParam[nCnt - 1].nAxisNum) < 0)
                {
                    lstIDs.Add(pSDhParam[nCnt - 1].nAxisNum);
                }
            }

            int nNum = pSDhParam[nCnt - 1].nFunctionNumber;
            int nNum_AfterCalc = pSDhParam[nCnt - 1].nFunctionNumber_AfterCalc;
            if ((nNum >= 0) && (nNum != 255))
            {
                if (lstFunctions.IndexOf(nNum) < 0)
                {
                    lstFunctions.Add(nNum);
                    //lstFunctions_AfterCalc.Add(nNum_AfterCalc);
                }
            }

            return true;
        }

        public void DeleteAll()
        {
            if (pSDhParam != null)
            {
                for (int i = 0; i < pSDhParam.Length; i++)
                    pSDhParam[i] = null;
                Array.Resize(ref pSDhParam, 0);
                lstIDs.Clear();
                lstFunctions.Clear();
                //lstFunctions_AfterCalc.Clear();
            }
        }
    }
    #endregion DH Parameter (SDhT_t, CDhParam, CDhParamAll)
    #endregion Class    
    public partial class Ojw
    {        
        public static void ShowKeyPad_Number(object sender)
        {
            Docking.CKeyPad_t.ShowCalculator((TextBox)sender);
        }
        public static void ShowKeyPad_Alpha(object sender)
        {
            Docking.CKeyPad_t.ShowKeyboard((TextBox)sender);
        }
        #region MotionTool
        #endregion MotionTool
        public class CTools_Designer
        {
            public Docking.frmDesigner m_frmDesigner = new Docking.frmDesigner();
            public void ShowTools(float fScale)
            {
                //m_frmDesigner.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
                //m_frmDesigner.Scale(new SizeF(fScale, fScale));

                m_frmDesigner.Show();
            }
        }
        public class CTools_Motion
        {
            public Ojw.C3d GetC3d() { return m_frmMotion.GetC3d(); }
            public Docking.frmMotionEditor m_frmMotion = new Docking.frmMotionEditor();
            public void Opacity(double dOpacity) { m_frmMotion.Opacity = dOpacity; }
            public void ShowTools() { ShowTools(1.0f); }
            public Button GetUserButton() { return m_frmMotion.btnUserButton; }
            public void ShowTools(float fScale)
            {
                m_frmMotion.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
                m_frmMotion.Scale(new SizeF(fScale, fScale));
                //GetC3d().m_CGridMotionEditor.GetHandle().Scale(new SizeF(fScale, fScale));

                m_frmMotion.Show();
            }
            public void SetUserButton(Docking.frmMotionEditor.UserFunction FUser) { m_frmMotion.SetUserButton(FUser); }
            public void SetIcon(Icon UserIcon) { m_frmMotion.Icon = UserIcon; }
            public void SetTitleImage(Bitmap bmp, Rectangle rc) { m_frmMotion.SetTitleImage(bmp, rc); }
        }
        public class CTools_Speech
        {
            public Docking.frmMsVoice m_frmMsVoice = new Docking.frmMsVoice();
            public void Show()
            {
                m_frmMsVoice.Show();
            }
        }
        public class CTools_Keyboard
        {
            public Docking.frmKeyPad m_frmKeyPad = new Docking.frmKeyPad();
            public void SetOpacity(double dRatio)
            {
                m_frmKeyPad.Opacity = dRatio;
            }
            public void SetCloseEvent(bool bApplicationExit) { m_frmKeyPad.SetCloseEvent(bApplicationExit); }
            public void ShowKeyboard(Image img, int nX, int nY) { ShowKeyboard(img, nX, nY, 1.0f); }
            public void ShowKeyboard(Image img, int nX, int nY, float fScale)
            {
                m_frmKeyPad.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
                m_frmKeyPad.Show();
                m_frmKeyPad.SetImage(img);
                m_frmKeyPad.SetPosition(nX, nY);
                m_frmKeyPad.Scale(new SizeF(fScale, fScale));
            }
            public void ShowKeyboard(int nX, int nY) { ShowKeyboard(nX, nY, 1.0f); }
            public void ShowKeyboard(int nX, int nY, float fScale)
            {
                m_frmKeyPad.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
                m_frmKeyPad.Show();
                m_frmKeyPad.SetPosition(nX, nY);
                m_frmKeyPad.Scale(new SizeF(fScale, fScale));
            }
            public void ShowKeyboard() { ShowKeyboard(1.0f); }
            public void ShowKeyboard(float fScale)
            {
                m_frmKeyPad.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
                m_frmKeyPad.Show();
                m_frmKeyPad.Scale(new SizeF(fScale, fScale));
            }
            public void EnableKorean(bool bEnable)
            {
                m_frmKeyPad.EnableHangul(bEnable);
            }
        }
        #region ModelingTool
        public class CTools
        {
            //private Form frmTool_Designer  = new Form();
            private C3d m_C3d_Designer = new C3d();
            private System.Windows.Forms.Timer m_tmrDrawModel = new System.Windows.Forms.Timer();
            private Panel m_pnProperty = new Panel();
            private Panel m_pnProperty_Selected = new Panel();
            private Panel m_pnKinematics = new Panel();
            private Panel m_pnMotors = new Panel();
            private TextBox m_txtInformation = new TextBox();
            private TextBox m_txtMessage = new TextBox();
            private TabControl m_tbCtrl = new TabControl();
            private Panel m_pnDrawModel = new Panel();
            private TabControl m_tabAngle = new TabControl();
            public void ShowTools_Modeling() { ShowTools_Modeling(1.0f); }
            private const int _SIZE_W = 1650;//1590;
            private const int _SIZE_H = 904;
            private int m_nSize_W = _SIZE_W;
            private int m_nSize_H = _SIZE_H;
            private int m_nSize_DRAW_W = 640;
            private int m_nSize_DRAW_H = 640;
            private int m_nDrawWidth = 0;
            private int m_nDrawHeight = 0;
            private Point m_pntPos_TabParam;
            private Point m_pntPos_TestAngle;
            //private int m_nSize_Disp_W = 0;
            //private int m_nSize_Disp_H = 0;
            //private float m_fScale = 0.0f;
            private Docking.frmModel m_frmTools;
            public void ShowTools_Modeling(float fScale)
            {
                if (fScale == 0)
                {
                    CMessage.Write_Error("Scale error - 0 division");
                    return;
                }

                float fScale2 = fScale;
                fScale = 1.0f;
                // 지정 폴더말고도 일반 폴더도 인식하게...(복사)
                m_C3d_Designer = new C3d();
                #region Main Form
                Form frmTool_Designer = new Form();
                frmTool_Designer.Size = new Size(_SIZE_W, _SIZE_H);
                frmTool_Designer.Text = string.Format("OpenJigWare - Version[{0}]", Ojw.GetVersion());
                frmTool_Designer.FormClosing += new FormClosingEventHandler(frmTool_Designer_FormClosing);
                frmTool_Designer.SizeChanged += new EventHandler(frmTool_Designer_SizeChanged);
                frmTool_Designer.Load += new EventHandler(frmTool_Designer_Load);
                frmTool_Designer.FormClosed += new FormClosedEventHandler(frmTool_Designer_FormClosed);

                frmTool_Designer.DragEnter += new DragEventHandler(frmTool_Designer_DragEnter);
                frmTool_Designer.DragDrop += new DragEventHandler(frmTool_Designer_DragDrop);

                frmTool_Designer.AllowDrop = true;
                #endregion Main Form
                                
                #region Panel(3D)
                m_pnDrawModel = new Panel();
                m_pnDrawModel.Size = new Size(m_nSize_DRAW_W, m_nSize_DRAW_H);
                m_pnDrawModel.Location = new Point(12, 13);
                frmTool_Designer.Controls.Add(m_pnDrawModel);
                #endregion Panel(3D)

                #region Timer
                m_tmrDrawModel = new System.Windows.Forms.Timer();
                m_tmrDrawModel.Tick += new EventHandler(m_tmrDrawModel_Tick);
                #endregion Timer

                #region TestBox - Angle[0 ~ 253]
                m_tabAngle = new TabControl();
                m_tabAngle.Left = m_pnDrawModel.Left;
                m_tabAngle.Top = m_pnDrawModel.Bottom + 10;
                m_tabAngle.Size = new Size(m_pnDrawModel.Width, 173);
                frmTool_Designer.Controls.Add(m_tabAngle);
                ////
                #endregion TestBox - Angle[0 ~ 253]
                ////

                #region TabControl
                int nLeft = 10;
                m_tbCtrl = new TabControl();
                //m_tbCtrl.Size = new Size(frmTool_Designer.Width - 30 - nLeft - m_pnDrawModel.Right, m_tabAngle.Bottom - m_pnDrawModel.Top);
                m_tbCtrl.Size = new Size(1000, m_pnDrawModel.Height + m_tabAngle.Height + 10);//frmTool_Designer.Width - 30 - nLeft - m_pnDrawModel.Right, m_pnDrawModel.Height + m_tabAngle.Height + 10);
                m_tbCtrl.Location = new Point(m_pnDrawModel.Right + nLeft, m_pnDrawModel.Top);
                frmTool_Designer.Controls.Add(m_tbCtrl);

                #region *TabControl - Page0
                TabPage tpPage0 = new TabPage();
                tpPage0.Text = "Draw";
                //tpPage0.Size = new Size(frmTool_Designer.Width - 20 - nLeft - pnDrawModel.Right, 817);
                //tpPage0.Location = new Point(pnDrawModel.Right + nLeft, pnDrawModel.Top);
                m_tbCtrl.TabPages.Add(tpPage0);

                #region **TabControl - Page0 - Property
                int nProperty_Top = 76;
                m_pnProperty = new Panel();
                m_pnProperty.Size = new Size(300, 774 - nProperty_Top);
                m_pnProperty.Location = new Point(6, 9 + nProperty_Top);
                //m_pnProperty.Location = new Point(6, 9);
                m_pnProperty.BorderStyle = BorderStyle.Fixed3D;
                //m_pnProperty.Scale(new SizeF(fScale + 1 - fScale2, fScale + 1 - fScale2));
                tpPage0.Controls.Add(m_pnProperty);
                
                m_pnProperty_Selected = new Panel();
                m_pnProperty_Selected.Size = new Size(m_pnProperty.Width, m_pnProperty.Height);
                m_pnProperty_Selected.Location = new Point(m_pnProperty.Width + 12, m_pnProperty.Top);
                m_pnProperty_Selected.BorderStyle = BorderStyle.Fixed3D;
                tpPage0.Controls.Add(m_pnProperty_Selected);
                #endregion **TabControl - Page0 - Property

                int nToolSize_Width = 318;
                #region **TabControl - Page0 - txtInformation
                int nSize_Gap = 10;
                m_txtInformation = new TextBox();
                m_txtInformation.Multiline = true;
                m_txtInformation.Size = new Size(nToolSize_Width, 200);//m_tbCtrl.Width - m_pnProperty_Selected.Right - 20 - nSize_Gap, 300);
                m_txtInformation.Location = new Point(m_pnProperty_Selected.Right + nSize_Gap, m_pnProperty_Selected.Bottom - m_txtInformation.Height);
                tpPage0.Controls.Add(m_txtInformation);
                #endregion **TabControl - Page0 - txtInformation

                #region **TabControl - Page0 - txtMessage
                m_txtMessage = new TextBox();
                m_txtMessage.Multiline = true;
                m_txtMessage.Size = new Size(nToolSize_Width, 200 - 20);//m_tbCtrl.Width - m_pnProperty_Selected.Right - 20 - nSize_Gap, 100);
                m_txtMessage.Location = new Point(m_pnProperty_Selected.Right + nSize_Gap, m_pnProperty_Selected.Bottom - (m_txtInformation.Height + m_txtMessage.Height + nSize_Gap));
                tpPage0.Controls.Add(m_txtMessage);
                Ojw.CMessage.Init(m_txtMessage);
                #endregion **TabControl - Page0 - txtInformation

                #region **TabControl - Page0 - Width
                #region QuickButton (For Virtual object)
                int nButton_Top = 29; //m_pnProperty_Selected.Top
                int nButton_Width = 18;
                int nButton_Height = 26;
                int nButton_Gap_Down = nButton_Top + 26;// 35;// 40;
                int nButton_Gap_Left = nButton_Width + 1;// 5;
                int nButton_Separation_Left = -(m_txtMessage.Left - m_pnProperty.Left);// -500;
                const int _SEPARATION_VALUE = 4;
                int i = 0;
                #region << Width >>
                Button btnWidth_Up = new Button();
                btnWidth_Up.Size = new Size(nButton_Width, nButton_Height);
                btnWidth_Up.Text = "▲";
                btnWidth_Up.Location = new Point(m_txtMessage.Left + nButton_Separation_Left + nButton_Gap_Left * i, nButton_Top);//m_pnProperty_Selected.Top);
                btnWidth_Up.Click += new EventHandler(btnWidth_Up_Click);
                btnWidth_Up.MouseWheel += new MouseEventHandler(btnWidth_MouseWheel);
                tpPage0.Controls.Add(btnWidth_Up);

                Button btnWidth_Down = new Button();
                btnWidth_Down.Size = new Size(nButton_Width, nButton_Height);
                btnWidth_Down.Text = "▼";
                btnWidth_Down.Location = new Point(m_txtMessage.Left + nButton_Separation_Left + nButton_Gap_Left * i, nButton_Gap_Down);
                btnWidth_Down.Click += new EventHandler(btnWidth_Down_Click);
                btnWidth_Down.MouseWheel += new MouseEventHandler(btnWidth_MouseWheel);
                tpPage0.Controls.Add(btnWidth_Down);
                #endregion << Width >>
                i++;
                #region << Height >>
                Button btnHeight_Up = new Button();
                btnHeight_Up.Size = new Size(nButton_Width, nButton_Height);
                btnHeight_Up.Text = "▲";
                btnHeight_Up.Location = new Point(m_txtMessage.Left + nButton_Separation_Left + nButton_Gap_Left * i, nButton_Top);
                btnHeight_Up.Click += new EventHandler(btnHeight_Up_Click);
                btnHeight_Up.MouseWheel += new MouseEventHandler(btnHeight_MouseWheel);
                tpPage0.Controls.Add(btnHeight_Up);

                Button btnHeight_Down = new Button();
                btnHeight_Down.Size = new Size(nButton_Width, nButton_Height);
                btnHeight_Down.Text = "▼";
                btnHeight_Down.Location = new Point(m_txtMessage.Left + nButton_Separation_Left + nButton_Gap_Left * i, nButton_Gap_Down);
                btnHeight_Down.Click += new EventHandler(btnHeight_Down_Click);
                btnHeight_Down.MouseWheel += new MouseEventHandler(btnHeight_MouseWheel);
                tpPage0.Controls.Add(btnHeight_Down);
                #endregion << Height >>
                i++;
                #region << Depth >>
                Button btnDepth_Up = new Button();
                btnDepth_Up.Size = new Size(nButton_Width, nButton_Height);
                btnDepth_Up.Text = "▲";
                btnDepth_Up.Location = new Point(m_txtMessage.Left + nButton_Separation_Left + nButton_Gap_Left * i, nButton_Top);
                btnDepth_Up.Click += new EventHandler(btnDepth_Up_Click);
                btnDepth_Up.MouseWheel += new MouseEventHandler(btnDepth_MouseWheel);                
                tpPage0.Controls.Add(btnDepth_Up);

                Button btnDepth_Down = new Button();
                btnDepth_Down.Size = new Size(nButton_Width, nButton_Height);
                btnDepth_Down.Text = "▼";
                btnDepth_Down.Location = new Point(m_txtMessage.Left + nButton_Separation_Left + nButton_Gap_Left * i, nButton_Gap_Down);
                btnDepth_Down.Click += new EventHandler(btnDepth_Down_Click);
                btnDepth_Down.MouseWheel += new MouseEventHandler(btnDepth_MouseWheel);   
                tpPage0.Controls.Add(btnDepth_Down);
                #endregion << Depth >>
                i++;

                nButton_Separation_Left += _SEPARATION_VALUE;

                #region << X >>
                Button btn_Offset_X_Up = new Button();
                btn_Offset_X_Up.Size = new Size(nButton_Width, nButton_Height);
                btn_Offset_X_Up.Text = "▲";
                btn_Offset_X_Up.Location = new Point(m_txtMessage.Left + nButton_Separation_Left + nButton_Gap_Left * i, nButton_Top);
                btn_Offset_X_Up.Click += new EventHandler(btn_Offset_X_Up_Click);
                btn_Offset_X_Up.MouseWheel += new MouseEventHandler(btn_Offset_X_MouseWheel);
                tpPage0.Controls.Add(btn_Offset_X_Up);

                Button btn_Offset_X_Down = new Button();
                btn_Offset_X_Down.Size = new Size(nButton_Width, nButton_Height);
                btn_Offset_X_Down.Text = "▼";
                btn_Offset_X_Down.Location = new Point(m_txtMessage.Left + nButton_Separation_Left + nButton_Gap_Left * i, nButton_Gap_Down);
                btn_Offset_X_Down.Click += new EventHandler(btn_Offset_X_Down_Click);
                btn_Offset_X_Down.MouseWheel += new MouseEventHandler(btn_Offset_X_MouseWheel);
                tpPage0.Controls.Add(btn_Offset_X_Down);
                #endregion << X >>
                i++;
                #region << Y >>
                Button btn_Offset_Y_Up = new Button();
                btn_Offset_Y_Up.Size = new Size(nButton_Width, nButton_Height);
                btn_Offset_Y_Up.Text = "▲";
                btn_Offset_Y_Up.Location = new Point(m_txtMessage.Left + nButton_Separation_Left + nButton_Gap_Left * i, nButton_Top);
                btn_Offset_Y_Up.Click += new EventHandler(btn_Offset_Y_Up_Click);
                btn_Offset_Y_Up.MouseWheel += new MouseEventHandler(btn_Offset_Y_MouseWheel);
                tpPage0.Controls.Add(btn_Offset_Y_Up);

                Button btn_Offset_Y_Down = new Button();
                btn_Offset_Y_Down.Size = new Size(nButton_Width, nButton_Height);
                btn_Offset_Y_Down.Text = "▼";
                btn_Offset_Y_Down.Location = new Point(m_txtMessage.Left + nButton_Separation_Left + nButton_Gap_Left * i, nButton_Gap_Down);
                btn_Offset_Y_Down.Click += new EventHandler(btn_Offset_Y_Down_Click);
                btn_Offset_Y_Down.MouseWheel += new MouseEventHandler(btn_Offset_Y_MouseWheel);
                tpPage0.Controls.Add(btn_Offset_Y_Down);
                #endregion << Y >>
                i++;
                #region << Z >>
                Button btn_Offset_Z_Up = new Button();
                btn_Offset_Z_Up.Size = new Size(nButton_Width, nButton_Height);
                btn_Offset_Z_Up.Text = "▲";
                btn_Offset_Z_Up.Location = new Point(m_txtMessage.Left + nButton_Separation_Left + nButton_Gap_Left * i, nButton_Top);
                btn_Offset_Z_Up.Click += new EventHandler(btn_Offset_Z_Up_Click);
                btn_Offset_Z_Up.MouseWheel += new MouseEventHandler(btn_Offset_Z_MouseWheel);
                tpPage0.Controls.Add(btn_Offset_Z_Up);

                Button btn_Offset_Z_Down = new Button();
                btn_Offset_Z_Down.Size = new Size(nButton_Width, nButton_Height);
                btn_Offset_Z_Down.Text = "▼";
                btn_Offset_Z_Down.Location = new Point(m_txtMessage.Left + nButton_Separation_Left + nButton_Gap_Left * i, nButton_Gap_Down);
                btn_Offset_Z_Down.Click += new EventHandler(btn_Offset_Z_Down_Click);
                btn_Offset_Z_Down.MouseWheel += new MouseEventHandler(btn_Offset_Z_MouseWheel);
                tpPage0.Controls.Add(btn_Offset_Z_Down);
                #endregion << Z >>
                i++;

                nButton_Separation_Left += _SEPARATION_VALUE;

                #region << Pan >>
                Button btn_Offset_Pan_Up = new Button();
                btn_Offset_Pan_Up.Size = new Size(nButton_Width, nButton_Height);
                btn_Offset_Pan_Up.Text = "▲";
                btn_Offset_Pan_Up.Location = new Point(m_txtMessage.Left + nButton_Separation_Left + nButton_Gap_Left * i, nButton_Top);
                btn_Offset_Pan_Up.Click += new EventHandler(btn_Offset_Pan_Up_Click);
                btn_Offset_Pan_Up.MouseWheel += new MouseEventHandler(btn_Offset_Pan_MouseWheel);
                tpPage0.Controls.Add(btn_Offset_Pan_Up);

                Button btn_Offset_Pan_Down = new Button();
                btn_Offset_Pan_Down.Size = new Size(nButton_Width, nButton_Height);
                btn_Offset_Pan_Down.Text = "▼";
                btn_Offset_Pan_Down.Location = new Point(m_txtMessage.Left + nButton_Separation_Left + nButton_Gap_Left * i, nButton_Gap_Down);
                btn_Offset_Pan_Down.Click += new EventHandler(btn_Offset_Pan_Down_Click);
                btn_Offset_Pan_Down.MouseWheel += new MouseEventHandler(btn_Offset_Pan_MouseWheel);
                tpPage0.Controls.Add(btn_Offset_Pan_Down);
                #endregion << Pan >>
                i++;
                #region << Tilt >>
                Button btn_Offset_Tilt_Up = new Button();
                btn_Offset_Tilt_Up.Size = new Size(nButton_Width, nButton_Height);
                btn_Offset_Tilt_Up.Text = "▲";
                btn_Offset_Tilt_Up.Location = new Point(m_txtMessage.Left + nButton_Separation_Left + nButton_Gap_Left * i, nButton_Top);
                btn_Offset_Tilt_Up.Click += new EventHandler(btn_Offset_Tilt_Up_Click);
                btn_Offset_Tilt_Up.MouseWheel += new MouseEventHandler(btn_Offset_Tilt_MouseWheel);
                tpPage0.Controls.Add(btn_Offset_Tilt_Up);

                Button btn_Offset_Tilt_Down = new Button();
                btn_Offset_Tilt_Down.Size = new Size(nButton_Width, nButton_Height);
                btn_Offset_Tilt_Down.Text = "▼";
                btn_Offset_Tilt_Down.Location = new Point(m_txtMessage.Left + nButton_Separation_Left + nButton_Gap_Left * i, nButton_Gap_Down);
                btn_Offset_Tilt_Down.Click += new EventHandler(btn_Offset_Tilt_Down_Click);
                btn_Offset_Tilt_Down.MouseWheel += new MouseEventHandler(btn_Offset_Tilt_MouseWheel);
                tpPage0.Controls.Add(btn_Offset_Tilt_Down);
                #endregion << Tilt >>
                i++;
                #region << Swing >>
                Button btn_Offset_Swing_Up = new Button();
                btn_Offset_Swing_Up.Size = new Size(nButton_Width, nButton_Height);
                btn_Offset_Swing_Up.Text = "▲";
                btn_Offset_Swing_Up.Location = new Point(m_txtMessage.Left + nButton_Separation_Left + nButton_Gap_Left * i, nButton_Top);
                btn_Offset_Swing_Up.Click += new EventHandler(btn_Offset_Swing_Up_Click);
                btn_Offset_Swing_Up.MouseWheel += new MouseEventHandler(btn_Offset_Swing_MouseWheel);
                tpPage0.Controls.Add(btn_Offset_Swing_Up);

                Button btn_Offset_Swing_Down = new Button();
                btn_Offset_Swing_Down.Size = new Size(nButton_Width, nButton_Height);
                btn_Offset_Swing_Down.Text = "▼";
                btn_Offset_Swing_Down.Location = new Point(m_txtMessage.Left + nButton_Separation_Left + nButton_Gap_Left * i, nButton_Gap_Down);
                btn_Offset_Swing_Down.Click += new EventHandler(btn_Offset_Swing_Down_Click);
                btn_Offset_Swing_Down.MouseWheel += new MouseEventHandler(btn_Offset_Swing_MouseWheel);
                tpPage0.Controls.Add(btn_Offset_Swing_Down);
                #endregion << Swing >>
                i++;
                //////////

                nButton_Separation_Left += _SEPARATION_VALUE;

                #region << X >>
                Button btn_X_Up = new Button();
                btn_X_Up.Size = new Size(nButton_Width, nButton_Height);
                btn_X_Up.Text = "▲";
                btn_X_Up.Location = new Point(m_txtMessage.Left + nButton_Separation_Left + nButton_Gap_Left * i, nButton_Top);
                btn_X_Up.Click += new EventHandler(btn_X_Up_Click);
                btn_X_Up.MouseWheel += new MouseEventHandler(btn_X_MouseWheel);
                tpPage0.Controls.Add(btn_X_Up);

                Button btn_X_Down = new Button();
                btn_X_Down.Size = new Size(nButton_Width, nButton_Height);
                btn_X_Down.Text = "▼";
                btn_X_Down.Location = new Point(m_txtMessage.Left + nButton_Separation_Left + nButton_Gap_Left * i, nButton_Gap_Down);
                btn_X_Down.Click += new EventHandler(btn_X_Down_Click);
                btn_X_Down.MouseWheel += new MouseEventHandler(btn_X_MouseWheel);
                tpPage0.Controls.Add(btn_X_Down);
                #endregion << X >>
                i++;
                #region << Y >>
                Button btn_Y_Up = new Button();
                btn_Y_Up.Size = new Size(nButton_Width, nButton_Height);
                btn_Y_Up.Text = "▲";
                btn_Y_Up.Location = new Point(m_txtMessage.Left + nButton_Separation_Left + nButton_Gap_Left * i, nButton_Top);
                btn_Y_Up.Click += new EventHandler(btn_Y_Up_Click);
                btn_Y_Up.MouseWheel += new MouseEventHandler(btn_Y_MouseWheel);
                tpPage0.Controls.Add(btn_Y_Up);

                Button btn_Y_Down = new Button();
                btn_Y_Down.Size = new Size(nButton_Width, nButton_Height);
                btn_Y_Down.Text = "▼";
                btn_Y_Down.Location = new Point(m_txtMessage.Left + nButton_Separation_Left + nButton_Gap_Left * i, nButton_Gap_Down);
                btn_Y_Down.Click += new EventHandler(btn_Y_Down_Click);
                btn_Y_Down.MouseWheel += new MouseEventHandler(btn_Y_MouseWheel);
                tpPage0.Controls.Add(btn_Y_Down);
                #endregion << Y >>
                i++;
                #region << Z >>
                Button btn_Z_Up = new Button();
                btn_Z_Up.Size = new Size(nButton_Width, nButton_Height);
                btn_Z_Up.Text = "▲";
                btn_Z_Up.Location = new Point(m_txtMessage.Left + nButton_Separation_Left + nButton_Gap_Left * i, nButton_Top);
                btn_Z_Up.Click += new EventHandler(btn_Z_Up_Click);
                btn_Z_Up.MouseWheel += new MouseEventHandler(btn_Z_MouseWheel);
                tpPage0.Controls.Add(btn_Z_Up);

                Button btn_Z_Down = new Button();
                btn_Z_Down.Size = new Size(nButton_Width, nButton_Height);
                btn_Z_Down.Text = "▼";
                btn_Z_Down.Location = new Point(m_txtMessage.Left + nButton_Separation_Left + nButton_Gap_Left * i, nButton_Gap_Down);
                btn_Z_Down.Click += new EventHandler(btn_Z_Down_Click);
                btn_Z_Down.MouseWheel += new MouseEventHandler(btn_Z_MouseWheel);
                tpPage0.Controls.Add(btn_Z_Down);
                #endregion << Z >>
                i++;

                nButton_Separation_Left += _SEPARATION_VALUE;

                #region << Pan >>
                Button btn_Pan_Up = new Button();
                btn_Pan_Up.Size = new Size(nButton_Width, nButton_Height);
                btn_Pan_Up.Text = "▲";
                btn_Pan_Up.Location = new Point(m_txtMessage.Left + nButton_Separation_Left + nButton_Gap_Left * i, nButton_Top);
                btn_Pan_Up.Click += new EventHandler(btn_Pan_Up_Click);
                btn_Pan_Up.MouseWheel += new MouseEventHandler(btn_Pan_MouseWheel);
                tpPage0.Controls.Add(btn_Pan_Up);

                Button btn_Pan_Down = new Button();
                btn_Pan_Down.Size = new Size(nButton_Width, nButton_Height);
                btn_Pan_Down.Text = "▼";
                btn_Pan_Down.Location = new Point(m_txtMessage.Left + nButton_Separation_Left + nButton_Gap_Left * i, nButton_Gap_Down);
                btn_Pan_Down.Click += new EventHandler(btn_Pan_Down_Click);
                btn_Pan_Down.MouseWheel += new MouseEventHandler(btn_Pan_MouseWheel);
                tpPage0.Controls.Add(btn_Pan_Down);
                #endregion << Pan >>
                i++;
                #region << Tilt >>
                Button btn_Tilt_Up = new Button();
                btn_Tilt_Up.Size = new Size(nButton_Width, nButton_Height);
                btn_Tilt_Up.Text = "▲";
                btn_Tilt_Up.Location = new Point(m_txtMessage.Left + nButton_Separation_Left + nButton_Gap_Left * i, nButton_Top);
                btn_Tilt_Up.Click += new EventHandler(btn_Tilt_Up_Click);
                btn_Tilt_Up.MouseWheel += new MouseEventHandler(btn_Tilt_MouseWheel);
                tpPage0.Controls.Add(btn_Tilt_Up);

                Button btn_Tilt_Down = new Button();
                btn_Tilt_Down.Size = new Size(nButton_Width, nButton_Height);
                btn_Tilt_Down.Text = "▼";
                btn_Tilt_Down.Location = new Point(m_txtMessage.Left + nButton_Separation_Left + nButton_Gap_Left * i, nButton_Gap_Down);
                btn_Tilt_Down.Click += new EventHandler(btn_Tilt_Down_Click);
                btn_Tilt_Down.MouseWheel += new MouseEventHandler(btn_Tilt_MouseWheel);
                tpPage0.Controls.Add(btn_Tilt_Down);
                #endregion << Tilt >>
                i++;
                #region << Swing >>
                Button btn_Swing_Up = new Button();
                btn_Swing_Up.Size = new Size(nButton_Width, nButton_Height);
                btn_Swing_Up.Text = "▲";
                btn_Swing_Up.Location = new Point(m_txtMessage.Left + nButton_Separation_Left + nButton_Gap_Left * i, nButton_Top);
                btn_Swing_Up.Click += new EventHandler(btn_Swing_Up_Click);
                btn_Swing_Up.MouseWheel += new MouseEventHandler(btn_Swing_MouseWheel);
                tpPage0.Controls.Add(btn_Swing_Up);

                Button btn_Swing_Down = new Button();
                btn_Swing_Down.Size = new Size(nButton_Width, nButton_Height);
                btn_Swing_Down.Text = "▼";
                btn_Swing_Down.Location = new Point(m_txtMessage.Left + nButton_Separation_Left + nButton_Gap_Left * i, nButton_Gap_Down);
                btn_Swing_Down.Click += new EventHandler(btn_Swing_Down_Click);
                btn_Swing_Down.MouseWheel += new MouseEventHandler(btn_Swing_MouseWheel);
                tpPage0.Controls.Add(btn_Swing_Down);
                #endregion << Swing >>
                i++;
                #endregion QuickButton (For Virtual object)


                #region QuickButton (For Selected object)
                nButton_Top = 29; //m_pnProperty_Selected.Top
                nButton_Width = 18;
                nButton_Height = 26;
                nButton_Gap_Down = nButton_Top + 26;// 40;
                nButton_Gap_Left = nButton_Width + 1;// 5;
                nButton_Separation_Left = -(m_txtMessage.Left - m_pnProperty_Selected.Left);// -500;
                //const int _SEPARATION_VALUE = 4;
                i = 0;
                Color colorBtnSelect = Color.Orange;//OrangeRed;
                #region **TabControl - Page0 - Width
                #region << Width >>
                Button btnWidth_Up_Selected = new Button();
                btnWidth_Up_Selected.Size = new Size(nButton_Width, nButton_Height);
                btnWidth_Up_Selected.Text = "▲";
                btnWidth_Up_Selected.BackColor = colorBtnSelect;
                btnWidth_Up_Selected.Location = new Point(m_txtMessage.Left + nButton_Separation_Left + nButton_Gap_Left * i, nButton_Top);//m_pnProperty_Selected.Top);
                btnWidth_Up_Selected.Click += new EventHandler(btnWidth_Up_Selected_Click);
                btnWidth_Up_Selected.MouseWheel += new MouseEventHandler(btnWidth_Selected_MouseWheel);
                tpPage0.Controls.Add(btnWidth_Up_Selected);

                Button btnWidth_Down_Selected = new Button();
                btnWidth_Down_Selected.Size = new Size(nButton_Width, nButton_Height);
                btnWidth_Down_Selected.Text = "▼";
                btnWidth_Down_Selected.BackColor = colorBtnSelect;
                btnWidth_Down_Selected.Location = new Point(m_txtMessage.Left + nButton_Separation_Left + nButton_Gap_Left * i, nButton_Gap_Down);
                btnWidth_Down_Selected.Click += new EventHandler(btnWidth_Down_Selected_Click);
                btnWidth_Down_Selected.MouseWheel += new MouseEventHandler(btnWidth_Selected_MouseWheel);
                tpPage0.Controls.Add(btnWidth_Down_Selected);
                #endregion << Width >>
                i++;
                #region << Height >>
                Button btnHeight_Up_Selected = new Button();
                btnHeight_Up_Selected.Size = new Size(nButton_Width, nButton_Height);
                btnHeight_Up_Selected.Text = "▲";
                btnHeight_Up_Selected.BackColor = colorBtnSelect;
                btnHeight_Up_Selected.Location = new Point(m_txtMessage.Left + nButton_Separation_Left + nButton_Gap_Left * i, nButton_Top);
                btnHeight_Up_Selected.Click += new EventHandler(btnHeight_Up_Selected_Click);
                btnHeight_Up_Selected.MouseWheel += new MouseEventHandler(btnHeight_Selected_MouseWheel);
                tpPage0.Controls.Add(btnHeight_Up_Selected);

                Button btnHeight_Down_Selected = new Button();
                btnHeight_Down_Selected.Size = new Size(nButton_Width, nButton_Height);
                btnHeight_Down_Selected.Text = "▼";
                btnHeight_Down_Selected.BackColor = colorBtnSelect;
                btnHeight_Down_Selected.Location = new Point(m_txtMessage.Left + nButton_Separation_Left + nButton_Gap_Left * i, nButton_Gap_Down);
                btnHeight_Down_Selected.Click += new EventHandler(btnHeight_Down_Selected_Click);
                btnHeight_Down_Selected.MouseWheel += new MouseEventHandler(btnHeight_Selected_MouseWheel);
                tpPage0.Controls.Add(btnHeight_Down_Selected);
                #endregion << Height >>
                i++;
                #region << Depth >>
                Button btnDepth_Up_Selected = new Button();
                btnDepth_Up_Selected.Size = new Size(nButton_Width, nButton_Height);
                btnDepth_Up_Selected.Text = "▲";
                btnDepth_Up_Selected.BackColor = colorBtnSelect;
                btnDepth_Up_Selected.Location = new Point(m_txtMessage.Left + nButton_Separation_Left + nButton_Gap_Left * i, nButton_Top);
                btnDepth_Up_Selected.Click += new EventHandler(btnDepth_Up_Selected_Click);
                btnDepth_Up_Selected.MouseWheel += new MouseEventHandler(btnDepth_Selected_MouseWheel);
                tpPage0.Controls.Add(btnDepth_Up_Selected);

                Button btnDepth_Down_Selected = new Button();
                btnDepth_Down_Selected.Size = new Size(nButton_Width, nButton_Height);
                btnDepth_Down_Selected.Text = "▼";
                btnDepth_Down_Selected.BackColor = colorBtnSelect;
                btnDepth_Down_Selected.Location = new Point(m_txtMessage.Left + nButton_Separation_Left + nButton_Gap_Left * i, nButton_Gap_Down);
                btnDepth_Down_Selected.Click += new EventHandler(btnDepth_Down_Selected_Click);
                btnDepth_Down_Selected.MouseWheel += new MouseEventHandler(btnDepth_Selected_MouseWheel);
                tpPage0.Controls.Add(btnDepth_Down_Selected);
                #endregion << Depth >>
                i++;

                nButton_Separation_Left += _SEPARATION_VALUE;

                #region << X >>
                Button btn_Offset_X_Up_Selected = new Button();
                btn_Offset_X_Up_Selected.Size = new Size(nButton_Width, nButton_Height);
                btn_Offset_X_Up_Selected.Text = "▲";
                btn_Offset_X_Up_Selected.BackColor = colorBtnSelect;
                btn_Offset_X_Up_Selected.Location = new Point(m_txtMessage.Left + nButton_Separation_Left + nButton_Gap_Left * i, nButton_Top);
                btn_Offset_X_Up_Selected.Click += new EventHandler(btn_Offset_X_Up_Selected_Click);
                btn_Offset_X_Up_Selected.MouseWheel += new MouseEventHandler(btn_Offset_X_Selected_MouseWheel);
                tpPage0.Controls.Add(btn_Offset_X_Up_Selected);

                Button btn_Offset_X_Down_Selected = new Button();
                btn_Offset_X_Down_Selected.Size = new Size(nButton_Width, nButton_Height);
                btn_Offset_X_Down_Selected.Text = "▼";
                btn_Offset_X_Down_Selected.BackColor = colorBtnSelect;
                btn_Offset_X_Down_Selected.Location = new Point(m_txtMessage.Left + nButton_Separation_Left + nButton_Gap_Left * i, nButton_Gap_Down);
                btn_Offset_X_Down_Selected.Click += new EventHandler(btn_Offset_X_Down_Selected_Click);
                btn_Offset_X_Down_Selected.MouseWheel += new MouseEventHandler(btn_Offset_X_Selected_MouseWheel);
                tpPage0.Controls.Add(btn_Offset_X_Down_Selected);
                #endregion << X >>
                i++;
                #region << Y >>
                Button btn_Offset_Y_Up_Selected = new Button();
                btn_Offset_Y_Up_Selected.Size = new Size(nButton_Width, nButton_Height);
                btn_Offset_Y_Up_Selected.Text = "▲";
                btn_Offset_Y_Up_Selected.BackColor = colorBtnSelect;
                btn_Offset_Y_Up_Selected.Location = new Point(m_txtMessage.Left + nButton_Separation_Left + nButton_Gap_Left * i, nButton_Top);
                btn_Offset_Y_Up_Selected.Click += new EventHandler(btn_Offset_Y_Up_Selected_Click);
                btn_Offset_Y_Up_Selected.MouseWheel += new MouseEventHandler(btn_Offset_Y_Selected_MouseWheel);
                tpPage0.Controls.Add(btn_Offset_Y_Up_Selected);

                Button btn_Offset_Y_Down_Selected = new Button();
                btn_Offset_Y_Down_Selected.Size = new Size(nButton_Width, nButton_Height);
                btn_Offset_Y_Down_Selected.Text = "▼";
                btn_Offset_Y_Down_Selected.BackColor = colorBtnSelect;
                btn_Offset_Y_Down_Selected.Location = new Point(m_txtMessage.Left + nButton_Separation_Left + nButton_Gap_Left * i, nButton_Gap_Down);
                btn_Offset_Y_Down_Selected.Click += new EventHandler(btn_Offset_Y_Down_Selected_Click);
                btn_Offset_Y_Down_Selected.MouseWheel += new MouseEventHandler(btn_Offset_Y_Selected_MouseWheel);
                tpPage0.Controls.Add(btn_Offset_Y_Down_Selected);
                #endregion << Y >>
                i++;
                #region << Z >>
                Button btn_Offset_Z_Up_Selected = new Button();
                btn_Offset_Z_Up_Selected.Size = new Size(nButton_Width, nButton_Height);
                btn_Offset_Z_Up_Selected.Text = "▲";
                btn_Offset_Z_Up_Selected.BackColor = colorBtnSelect;
                btn_Offset_Z_Up_Selected.Location = new Point(m_txtMessage.Left + nButton_Separation_Left + nButton_Gap_Left * i, nButton_Top);
                btn_Offset_Z_Up_Selected.Click += new EventHandler(btn_Offset_Z_Up_Selected_Click);
                btn_Offset_Z_Up_Selected.MouseWheel += new MouseEventHandler(btn_Offset_Z_Selected_MouseWheel);
                tpPage0.Controls.Add(btn_Offset_Z_Up_Selected);

                Button btn_Offset_Z_Down_Selected = new Button();
                btn_Offset_Z_Down_Selected.Size = new Size(nButton_Width, nButton_Height);
                btn_Offset_Z_Down_Selected.Text = "▼";
                btn_Offset_Z_Down_Selected.BackColor = colorBtnSelect;
                btn_Offset_Z_Down_Selected.Location = new Point(m_txtMessage.Left + nButton_Separation_Left + nButton_Gap_Left * i, nButton_Gap_Down);
                btn_Offset_Z_Down_Selected.Click += new EventHandler(btn_Offset_Z_Down_Selected_Click);
                btn_Offset_Z_Down_Selected.MouseWheel += new MouseEventHandler(btn_Offset_Z_Selected_MouseWheel);
                tpPage0.Controls.Add(btn_Offset_Z_Down_Selected);
                #endregion << Z >>
                i++;

                nButton_Separation_Left += _SEPARATION_VALUE;

                #region << Pan >>
                Button btn_Offset_Pan_Up_Selected = new Button();
                btn_Offset_Pan_Up_Selected.Size = new Size(nButton_Width, nButton_Height);
                btn_Offset_Pan_Up_Selected.Text = "▲";
                btn_Offset_Pan_Up_Selected.BackColor = colorBtnSelect;
                btn_Offset_Pan_Up_Selected.Location = new Point(m_txtMessage.Left + nButton_Separation_Left + nButton_Gap_Left * i, nButton_Top);
                btn_Offset_Pan_Up_Selected.Click += new EventHandler(btn_Offset_Pan_Up_Selected_Click);
                btn_Offset_Pan_Up_Selected.MouseWheel += new MouseEventHandler(btn_Offset_Pan_Selected_MouseWheel);
                tpPage0.Controls.Add(btn_Offset_Pan_Up_Selected);

                Button btn_Offset_Pan_Down_Selected = new Button();
                btn_Offset_Pan_Down_Selected.Size = new Size(nButton_Width, nButton_Height);
                btn_Offset_Pan_Down_Selected.Text = "▼";
                btn_Offset_Pan_Down_Selected.BackColor = colorBtnSelect;
                btn_Offset_Pan_Down_Selected.Location = new Point(m_txtMessage.Left + nButton_Separation_Left + nButton_Gap_Left * i, nButton_Gap_Down);
                btn_Offset_Pan_Down_Selected.Click += new EventHandler(btn_Offset_Pan_Down_Selected_Click);
                btn_Offset_Pan_Down_Selected.MouseWheel += new MouseEventHandler(btn_Offset_Pan_Selected_MouseWheel);
                tpPage0.Controls.Add(btn_Offset_Pan_Down_Selected);
                #endregion << Pan >>
                i++;
                #region << Tilt >>
                Button btn_Offset_Tilt_Up_Selected = new Button();
                btn_Offset_Tilt_Up_Selected.Size = new Size(nButton_Width, nButton_Height);
                btn_Offset_Tilt_Up_Selected.Text = "▲";
                btn_Offset_Tilt_Up_Selected.BackColor = colorBtnSelect;
                btn_Offset_Tilt_Up_Selected.Location = new Point(m_txtMessage.Left + nButton_Separation_Left + nButton_Gap_Left * i, nButton_Top);
                btn_Offset_Tilt_Up_Selected.Click += new EventHandler(btn_Offset_Tilt_Up_Selected_Click);
                btn_Offset_Tilt_Up_Selected.MouseWheel += new MouseEventHandler(btn_Offset_Tilt_Selected_MouseWheel);
                tpPage0.Controls.Add(btn_Offset_Tilt_Up_Selected);

                Button btn_Offset_Tilt_Down_Selected = new Button();
                btn_Offset_Tilt_Down_Selected.Size = new Size(nButton_Width, nButton_Height);
                btn_Offset_Tilt_Down_Selected.Text = "▼";
                btn_Offset_Tilt_Down_Selected.BackColor = colorBtnSelect;
                btn_Offset_Tilt_Down_Selected.Location = new Point(m_txtMessage.Left + nButton_Separation_Left + nButton_Gap_Left * i, nButton_Gap_Down);
                btn_Offset_Tilt_Down_Selected.Click += new EventHandler(btn_Offset_Tilt_Down_Selected_Click);
                btn_Offset_Tilt_Down_Selected.MouseWheel += new MouseEventHandler(btn_Offset_Tilt_Selected_MouseWheel);
                tpPage0.Controls.Add(btn_Offset_Tilt_Down_Selected);
                #endregion << Tilt >>
                i++;
                #region << Swing >>
                Button btn_Offset_Swing_Up_Selected = new Button();
                btn_Offset_Swing_Up_Selected.Size = new Size(nButton_Width, nButton_Height);
                btn_Offset_Swing_Up_Selected.Text = "▲";
                btn_Offset_Swing_Up_Selected.BackColor = colorBtnSelect;
                btn_Offset_Swing_Up_Selected.Location = new Point(m_txtMessage.Left + nButton_Separation_Left + nButton_Gap_Left * i, nButton_Top);
                btn_Offset_Swing_Up_Selected.Click += new EventHandler(btn_Offset_Swing_Up_Selected_Click);
                btn_Offset_Swing_Up_Selected.MouseWheel += new MouseEventHandler(btn_Offset_Swing_Selected_MouseWheel);
                tpPage0.Controls.Add(btn_Offset_Swing_Up_Selected);

                Button btn_Offset_Swing_Down_Selected = new Button();
                btn_Offset_Swing_Down_Selected.Size = new Size(nButton_Width, nButton_Height);
                btn_Offset_Swing_Down_Selected.Text = "▼";
                btn_Offset_Swing_Down_Selected.BackColor = colorBtnSelect;
                btn_Offset_Swing_Down_Selected.Location = new Point(m_txtMessage.Left + nButton_Separation_Left + nButton_Gap_Left * i, nButton_Gap_Down);
                btn_Offset_Swing_Down_Selected.Click += new EventHandler(btn_Offset_Swing_Down_Selected_Click);
                btn_Offset_Swing_Down_Selected.MouseWheel += new MouseEventHandler(btn_Offset_Swing_Selected_MouseWheel);
                tpPage0.Controls.Add(btn_Offset_Swing_Down_Selected);
                #endregion << Swing >>
                i++;
                //////////

                nButton_Separation_Left += _SEPARATION_VALUE;

                #region << X >>
                Button btn_X_Up_Selected = new Button();
                btn_X_Up_Selected.Size = new Size(nButton_Width, nButton_Height);
                btn_X_Up_Selected.Text = "▲";
                btn_X_Up_Selected.BackColor = colorBtnSelect;
                btn_X_Up_Selected.Location = new Point(m_txtMessage.Left + nButton_Separation_Left + nButton_Gap_Left * i, nButton_Top);
                btn_X_Up_Selected.Click += new EventHandler(btn_X_Up_Selected_Click);
                btn_X_Up_Selected.MouseWheel += new MouseEventHandler(btn_X_Selected_MouseWheel);
                tpPage0.Controls.Add(btn_X_Up_Selected);

                Button btn_X_Down_Selected = new Button();
                btn_X_Down_Selected.Size = new Size(nButton_Width, nButton_Height);
                btn_X_Down_Selected.Text = "▼";
                btn_X_Down_Selected.BackColor = colorBtnSelect;
                btn_X_Down_Selected.Location = new Point(m_txtMessage.Left + nButton_Separation_Left + nButton_Gap_Left * i, nButton_Gap_Down);
                btn_X_Down_Selected.Click += new EventHandler(btn_X_Down_Selected_Click);
                btn_X_Down_Selected.MouseWheel += new MouseEventHandler(btn_X_Selected_MouseWheel);
                tpPage0.Controls.Add(btn_X_Down_Selected);
                #endregion << X >>
                i++;
                #region << Y >>
                Button btn_Y_Up_Selected = new Button();
                btn_Y_Up_Selected.Size = new Size(nButton_Width, nButton_Height);
                btn_Y_Up_Selected.Text = "▲";
                btn_Y_Up_Selected.BackColor = colorBtnSelect;
                btn_Y_Up_Selected.Location = new Point(m_txtMessage.Left + nButton_Separation_Left + nButton_Gap_Left * i, nButton_Top);
                btn_Y_Up_Selected.Click += new EventHandler(btn_Y_Up_Selected_Click);
                btn_Y_Up_Selected.MouseWheel += new MouseEventHandler(btn_Y_Selected_MouseWheel);
                tpPage0.Controls.Add(btn_Y_Up_Selected);

                Button btn_Y_Down_Selected = new Button();
                btn_Y_Down_Selected.Size = new Size(nButton_Width, nButton_Height);
                btn_Y_Down_Selected.Text = "▼";
                btn_Y_Down_Selected.BackColor = colorBtnSelect;
                btn_Y_Down_Selected.Location = new Point(m_txtMessage.Left + nButton_Separation_Left + nButton_Gap_Left * i, nButton_Gap_Down);
                btn_Y_Down_Selected.Click += new EventHandler(btn_Y_Down_Selected_Click);
                btn_Y_Down_Selected.MouseWheel += new MouseEventHandler(btn_Y_Selected_MouseWheel);
                tpPage0.Controls.Add(btn_Y_Down_Selected);
                #endregion << Y >>
                i++;
                #region << Z >>
                Button btn_Z_Up_Selected = new Button();
                btn_Z_Up_Selected.Size = new Size(nButton_Width, nButton_Height);
                btn_Z_Up_Selected.Text = "▲";
                btn_Z_Up_Selected.BackColor = colorBtnSelect;
                btn_Z_Up_Selected.Location = new Point(m_txtMessage.Left + nButton_Separation_Left + nButton_Gap_Left * i, nButton_Top);
                btn_Z_Up_Selected.Click += new EventHandler(btn_Z_Up_Selected_Click);
                btn_Z_Up_Selected.MouseWheel += new MouseEventHandler(btn_Z_Selected_MouseWheel);
                tpPage0.Controls.Add(btn_Z_Up_Selected);

                Button btn_Z_Down_Selected = new Button();
                btn_Z_Down_Selected.Size = new Size(nButton_Width, nButton_Height);
                btn_Z_Down_Selected.Text = "▼";
                btn_Z_Down_Selected.BackColor = colorBtnSelect;
                btn_Z_Down_Selected.Location = new Point(m_txtMessage.Left + nButton_Separation_Left + nButton_Gap_Left * i, nButton_Gap_Down);
                btn_Z_Down_Selected.Click += new EventHandler(btn_Z_Down_Selected_Click);
                btn_Z_Down_Selected.MouseWheel += new MouseEventHandler(btn_Z_Selected_MouseWheel);
                tpPage0.Controls.Add(btn_Z_Down_Selected);
                #endregion << Z >>
                i++;

                nButton_Separation_Left += _SEPARATION_VALUE;

                #region << Pan >>
                Button btn_Pan_Up_Selected = new Button();
                btn_Pan_Up_Selected.Size = new Size(nButton_Width, nButton_Height);
                btn_Pan_Up_Selected.Text = "▲";
                btn_Pan_Up_Selected.BackColor = colorBtnSelect;
                btn_Pan_Up_Selected.Location = new Point(m_txtMessage.Left + nButton_Separation_Left + nButton_Gap_Left * i, nButton_Top);
                btn_Pan_Up_Selected.Click += new EventHandler(btn_Pan_Up_Selected_Click);
                btn_Pan_Up_Selected.MouseWheel += new MouseEventHandler(btn_Pan_Selected_MouseWheel);
                tpPage0.Controls.Add(btn_Pan_Up_Selected);

                Button btn_Pan_Down_Selected = new Button();
                btn_Pan_Down_Selected.Size = new Size(nButton_Width, nButton_Height);
                btn_Pan_Down_Selected.Text = "▼";
                btn_Pan_Down_Selected.BackColor = colorBtnSelect;
                btn_Pan_Down_Selected.Location = new Point(m_txtMessage.Left + nButton_Separation_Left + nButton_Gap_Left * i, nButton_Gap_Down);
                btn_Pan_Down_Selected.Click += new EventHandler(btn_Pan_Down_Selected_Click);
                btn_Pan_Down_Selected.MouseWheel += new MouseEventHandler(btn_Pan_Selected_MouseWheel);
                tpPage0.Controls.Add(btn_Pan_Down_Selected);
                #endregion << Pan >>
                i++;
                #region << Tilt >>
                Button btn_Tilt_Up_Selected = new Button();
                btn_Tilt_Up_Selected.Size = new Size(nButton_Width, nButton_Height);
                btn_Tilt_Up_Selected.Text = "▲";
                btn_Tilt_Up_Selected.BackColor = colorBtnSelect;
                btn_Tilt_Up_Selected.Location = new Point(m_txtMessage.Left + nButton_Separation_Left + nButton_Gap_Left * i, nButton_Top);
                btn_Tilt_Up_Selected.Click += new EventHandler(btn_Tilt_Up_Selected_Click);
                btn_Tilt_Up_Selected.MouseWheel += new MouseEventHandler(btn_Tilt_Selected_MouseWheel);
                tpPage0.Controls.Add(btn_Tilt_Up_Selected);

                Button btn_Tilt_Down_Selected = new Button();
                btn_Tilt_Down_Selected.Size = new Size(nButton_Width, nButton_Height);
                btn_Tilt_Down_Selected.Text = "▼";
                btn_Tilt_Down_Selected.BackColor = colorBtnSelect;
                btn_Tilt_Down_Selected.Location = new Point(m_txtMessage.Left + nButton_Separation_Left + nButton_Gap_Left * i, nButton_Gap_Down);
                btn_Tilt_Down_Selected.Click += new EventHandler(btn_Tilt_Down_Selected_Click);
                btn_Tilt_Down_Selected.MouseWheel += new MouseEventHandler(btn_Tilt_Selected_MouseWheel);
                tpPage0.Controls.Add(btn_Tilt_Down_Selected);
                #endregion << Tilt >>
                i++;
                #region << Swing >>
                Button btn_Swing_Up_Selected = new Button();
                btn_Swing_Up_Selected.Size = new Size(nButton_Width, nButton_Height);
                btn_Swing_Up_Selected.Text = "▲";
                btn_Swing_Up_Selected.BackColor = colorBtnSelect;
                btn_Swing_Up_Selected.Location = new Point(m_txtMessage.Left + nButton_Separation_Left + nButton_Gap_Left * i, nButton_Top);
                btn_Swing_Up_Selected.Click += new EventHandler(btn_Swing_Up_Selected_Click);
                btn_Swing_Up_Selected.MouseWheel += new MouseEventHandler(btn_Swing_Selected_MouseWheel);
                tpPage0.Controls.Add(btn_Swing_Up_Selected);

                Button btn_Swing_Down_Selected = new Button();
                btn_Swing_Down_Selected.Size = new Size(nButton_Width, nButton_Height);
                btn_Swing_Down_Selected.Text = "▼";
                btn_Swing_Down_Selected.BackColor = colorBtnSelect;
                btn_Swing_Down_Selected.Location = new Point(m_txtMessage.Left + nButton_Separation_Left + nButton_Gap_Left * i, nButton_Gap_Down);
                btn_Swing_Down_Selected.Click += new EventHandler(btn_Swing_Down_Selected_Click);
                btn_Swing_Down_Selected.MouseWheel += new MouseEventHandler(btn_Swing_Selected_MouseWheel);
                tpPage0.Controls.Add(btn_Swing_Down_Selected);
                #endregion << Swing >>
                i++;
                #endregion **TabControl - Page0 - Width
                #endregion QuickButton (For Selected object)

                #region Label
                Label[] albQuick = new Label[10];
                int j;
                for (j = 0; j < 10; j++)
                    albQuick[j] = new Label();
                j = 0;
                albQuick[j++].Location = new Point(btnWidth_Up.Left, 9);
                albQuick[j++].Location = new Point(btn_Offset_X_Up.Left, 9);
                albQuick[j++].Location = new Point(btn_Offset_Pan_Up.Left, 9);
                albQuick[j++].Location = new Point(btn_X_Up.Left, 9);
                albQuick[j++].Location = new Point(btn_Pan_Up.Left, 9);

                Size sizeLabel = new Size(btnDepth_Up.Right - btnWidth_Up.Left + 6, 26);
                j = 0;
                albQuick[j++].Size = sizeLabel;
                albQuick[j++].Size = sizeLabel;
                albQuick[j++].Size = sizeLabel;
                albQuick[j++].Size = sizeLabel;
                albQuick[j++].Size = sizeLabel;
                albQuick[j++].Size = sizeLabel;
                albQuick[j++].Size = sizeLabel;
                albQuick[j++].Size = sizeLabel;
                albQuick[j++].Size = sizeLabel;
                albQuick[j++].Size = sizeLabel;
                j = 0;
                albQuick[j++].Text = "Size";
                albQuick[j++].Text = "(X,Y,Z)";
                albQuick[j++].Text = "(P,T,S)";
                albQuick[j++].Text = "(X,Y,Z)";
                albQuick[j++].Text = "(P,T,S)";

                j = 5;
                albQuick[j++].Location = new Point(btnWidth_Up_Selected.Left, 9);
                albQuick[j++].Location = new Point(btn_Offset_X_Up_Selected.Left, 9);
                albQuick[j++].Location = new Point(btn_Offset_Pan_Up_Selected.Left, 9);
                albQuick[j++].Location = new Point(btn_X_Up_Selected.Left, 9);
                albQuick[j++].Location = new Point(btn_Pan_Up_Selected.Left, 9);
                j = 5;
                albQuick[j++].Text = "Size";
                albQuick[j++].Text = "(X,Y,Z)";
                albQuick[j++].Text = "(P,T,S)";
                albQuick[j++].Text = "(X,Y,Z)";
                albQuick[j++].Text = "(P,T,S)";

                for (j = 0; j < 10; j++)
                    tpPage0.Controls.Add(albQuick[j]);
                #endregion Label

                // Docking Page
                Panel pnModel = new Panel();
                pnModel.Size = new Size(nToolSize_Width, 374);//m_txtMessage.Top - 9 - 5);//m_tbCtrl.Width - m_pnProperty_Selected.Right - 20 - nSize_Gap, m_txtMessage.Top - 9 - 5);
                pnModel.Location = new Point(m_pnProperty_Selected.Right + nSize_Gap, 9);
                pnModel.BorderStyle = BorderStyle.FixedSingle;
                tpPage0.Controls.Add(pnModel);


                bool bTopLevel = false;
                //Docking.frmModel frmTools = new Docking.frmModel();
                m_frmTools = new Docking.frmModel();
                m_frmTools.TopLevel = bTopLevel;
                m_frmTools.Dock = System.Windows.Forms.DockStyle.Fill;
                pnModel.Controls.Add(m_frmTools);
                //m_frmTools.Init(m_C3d_Designer);
                //m_frmTools.Show();

                #endregion **TabControl - Page0 - Buttons

                #endregion *TabControl - Page0

                #region *TabControl - Page1
                TabPage tpPage1 = new TabPage();
                tpPage1.Text = "Kinematics";
                //tpPage1.Size = new Size(frmTool_Designer.Width - 20 - nLeft - pnDrawModel.Right, 817);
                //tpPage1.Location = new Point(pnDrawModel.Right + nLeft, pnDrawModel.Top);
                m_tbCtrl.TabPages.Add(tpPage1);

                #region **TabControl - Page1 - Property
                //m_pnKinematics.Size = new Size(900, 778);
                //m_pnKinematics.Location = new Point(6, 9);
                tpPage1.Controls.Add(m_pnKinematics);
                #endregion **TabControl - Page1 - Property

                #endregion *TabControl - Page2

                #region *TabControl - Page2
                TabPage tpPage2 = new TabPage();
                tpPage2.Text = "Motor";
                m_tbCtrl.TabPages.Add(tpPage2);

                #region **TabControl - Page2 - Motor
                //m_pnMotors.Size = new Size(885, 778);
                m_pnMotors.Size = new Size(940, 778);
                m_pnMotors.Location = new Point(6, 9);
                tpPage2.Controls.Add(m_pnMotors);
                #endregion **TabControl - Page2 - Motor

                #endregion *TabControl - Page2

                #endregion TabControl

                //Init3D(m_pnDrawModel);
                MakeBox(m_tabAngle, 256);
                ///////////////////////////////////




                Init3D(m_pnDrawModel);

                //m_C3d_Designer.InitTools_Motor(m_pnMotors);
                //m_C3d_Designer.InitTools_Status(pnStatus);
                //m_C3d_Designer.InitTools_Kinematics(m_pnKinematics);
                //m_C3d_Designer.InitTools_Background(pnBackground);

                m_frmTools.Init(m_C3d_Designer);




                bool bFull = false;
                if (fScale <= 0.0f)
                {
                    //int nPos_X = (Application.OpenForms[0].Left + Application.OpenForms[0].Right) / 2;
                    //int nPos_Y = (Application.OpenForms[0].Top + Application.OpenForms[0].Bottom) / 2;
                    //if (
                    //    ((nPos_X >= 0) && ((nPos_X <= Screen.PrimaryScreen.Bounds.Width)))
                    //    &&
                    //    ((nPos_Y >= 0) && ((nPos_Y <= Screen.PrimaryScreen.Bounds.Height)))
                    //    )
                    //{

                    //}
                    if ((Screen.PrimaryScreen.Bounds.Width / _SIZE_W) < (Screen.PrimaryScreen.Bounds.Height / _SIZE_H))
                    {
                        fScale = (float)((float)Screen.PrimaryScreen.Bounds.Width / (float)_SIZE_W);
                    }
                    else fScale = (float)((float)Screen.PrimaryScreen.Bounds.Height / (float)_SIZE_H);
                    bFull = true;
                }
                float fRatio_W = fScale;// 0.5f;
                float fRatio_H = fScale;//0.5f;
                m_fFormScale = fScale;
                frmTool_Designer.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi; // (Font 로 되어 있으면 영문윈도우시 깨짐)
                frmTool_Designer.Scale(new SizeF(fRatio_W, fRatio_H));
                foreach (Control control in frmTool_Designer.Controls)
                {
                    control.Font = new Font(control.Font.Name, control.Font.SizeInPoints * fScale2);//fRatio_W * fRatio_H);
                }

                // 수정된 위치 적용
                m_tbCtrl.Location = new Point(m_pnDrawModel.Right + nLeft, m_pnDrawModel.Top);

                m_tabAngle.Left = m_pnDrawModel.Left;
                m_tabAngle.Top = m_pnDrawModel.Bottom + 10;

                ///////////////////////////////////
                m_nSize_W = (int)Math.Round(_SIZE_W * fScale);// frmTool_Designer.Width;
                m_nSize_H = (int)Math.Round(_SIZE_H * fScale);// frmTool_Designer.Height;
                m_nDrawWidth = (int)Math.Round(m_nSize_DRAW_W * fScale2);// m_pnDrawModel.Width;
                m_nDrawHeight = (int)Math.Round(m_nSize_DRAW_H * fScale2);//m_pnDrawModel.Height;

                m_pntPos_TabParam.X = m_tbCtrl.Left;
                m_pntPos_TabParam.Y = m_tbCtrl.Top;
                m_pntPos_TestAngle.X = m_tabAngle.Left;
                m_pntPos_TestAngle.Y = m_tabAngle.Top;
                if (bFull == true) frmTool_Designer.WindowState = FormWindowState.Maximized;
                //#region Myo
                //Ojw.CMyo m_CMyo = new CMyo();
                
                //#endregion Myo


                //m_nSize_Disp_W = m_pnDrawModel.Width;
                //m_nSize_Disp_H = m_pnDrawModel.Height;

                //m_fScale = fScale;



                //m_pnProperty.Scale(new SizeF(fScale + 1 - fScale2, fScale + 1 - fScale2));


                frmTool_Designer.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi; // (Font 로 되어 있으면 영문윈도우시 깨짐)
                frmTool_Designer.Scale(new SizeF(fScale2, fScale2));

                m_frmTools.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi; // (Font 로 되어 있으면 영문윈도우시 깨짐)
                m_frmTools.Scale(new SizeF(1.0f / fScale2, 1.0f / fScale2));


                frmTool_Designer.Size = new Size((int)Math.Round((float)_SIZE_W * fScale2) + 10, (int)Math.Round((float)_SIZE_H * fScale2) + 10);

                m_frmTools.Show();

                m_tmrDrawModel.Enabled = true;
                
                frmTool_Designer.Show();
            }

            private void frmTool_Designer_DragDrop(object sender, DragEventArgs e)
            {
                string[] file_name_array = (string[])e.Data.GetData(DataFormats.FileDrop, false);
                int nCnt_Ojw = 0;
                int nCnt_Virtual = 0;
                foreach (string strItem in file_name_array)
                {
                    #region Design File
                    if (nCnt_Ojw == 0)
                    {
                        if (
                            (strItem.ToLower().IndexOf(".ojw") > 0) ||
                            (strItem.ToLower().IndexOf(".dhf") > 0)
                            )
                        {
                            if (m_C3d_Designer.FileOpen(strItem) == true) // 모델링 파일이 잘 로드 되었다면 
                            {
                                Ojw.CMessage.Write("3d Modeling File Opened");

                                float[] afData = new float[3];
                                m_C3d_Designer.GetPos_Display(out afData[0], out afData[1], out afData[2]);
                                m_C3d_Designer.GetAngle_Display(out afData[0], out afData[1], out afData[2]);

                                m_C3d_Designer.m_strDesignerFilePath = Ojw.CFile.GetPath(strItem);
                                if (m_C3d_Designer.m_strDesignerFilePath == null) m_C3d_Designer.m_strDesignerFilePath = Application.StartupPath;

                                // File Restore
                                //m_C3d.FileRestore();


                                nCnt_Ojw++;
                            }
                        }
                    }
                    #endregion Design File
                    #region 3d file
                    if (nCnt_Virtual == 0)
                    {
                        string strFileName = Ojw.CFile.GetName(strItem).ToLower();
                        if (
                            (strFileName.IndexOf(".stl") > 0) ||
                            (strFileName.IndexOf(".sstl") > 0) ||
                            (strFileName.IndexOf(".ase") > 0) ||
                            (strFileName.IndexOf(".dat") > 0) ||
                            (strFileName.IndexOf(".obj") > 0))
                        {
                            String strFilePath = Application.StartupPath.Trim('\\') + m_C3d_Designer.GetAseFile_Path() + strFileName;
                            bool bFile = Ojw.CFile.IsFile(strFilePath);//bool bLoaded = (m_C3d_Designer.OjwAse_GetIndex(strFileName) >= 0) ? true : false;
                            if (bFile == true)
                            {
                                DialogResult dlgRet = MessageBox.Show("Do you want to overwrite the 3d file?.\r\n\r\nIs it Ok?", "File Overwrite", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                                if (dlgRet != DialogResult.OK)
                                {
                                    //bFile = false;
                                }
                                else
                                {
                                    File.Copy(strItem, strFilePath, bFile);
                                }
                            }
                            else
                            {
                                File.Copy(strItem, strFilePath);
                            }
                            m_C3d_Designer.Prop_Set_DispObject(strFileName);
                            m_C3d_Designer.Prop_Update_VirtualObject();
                            nCnt_Virtual++;
                        }
                    }
                    #endregion Design File
                }
            }
            private void frmTool_Designer_DragEnter(object sender, DragEventArgs e) { if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy; }
                  
            #region Quick Button(For Virtual Object)
            private float m_fValue_Width = 1.0f;
            private float m_fValue_Height = 1.0f;
            private float m_fValue_Depth = 1.0f;
            private float m_fValue_Offset_Trans = 1.0f;
            private float m_fValue_Offset_Rot = 1.0f;
            private float m_fValue_Trans = 1.0f;
            private float m_fValue_Rot = 1.0f;
            private bool m_bWheel = false;
            void btnWidth_MouseWheel(object sender, MouseEventArgs e)
            {
                m_bWheel = true;
                float fValue = (e.Button == MouseButtons.Right) ? 0.1f : 1.0f;
                if (e.Delta < 0) fValue = -fValue;
                m_C3d_Designer.Prop_Set_Width_Or_Radius(m_C3d_Designer.Prop_Get_Width_Or_Radius() + fValue);
                m_C3d_Designer.Prop_Update_VirtualObject();
            }
            void btnWidth_Up_Click(object sender, EventArgs e)
            {
                if (m_bWheel == true) { m_bWheel = false; return; }
                //throw new NotImplementedException();
                m_C3d_Designer.Prop_Set_Width_Or_Radius(m_C3d_Designer.Prop_Get_Width_Or_Radius() + m_fValue_Width);
                m_C3d_Designer.Prop_Update_VirtualObject();
            }
            void btnWidth_Down_Click(object sender, EventArgs e)
            {
                if (m_bWheel == true) { m_bWheel = false; return; }
                //throw new NotImplementedException();
                m_C3d_Designer.Prop_Set_Width_Or_Radius(m_C3d_Designer.Prop_Get_Width_Or_Radius() - m_fValue_Width);
                m_C3d_Designer.Prop_Update_VirtualObject();
            }
            void btnHeight_MouseWheel(object sender, MouseEventArgs e)
            {
                m_bWheel = true;
                float fValue =  (e.Button == MouseButtons.Right) ? 0.1f : 1.0f;
                if (e.Delta < 0) fValue = -fValue;
                m_C3d_Designer.Prop_Set_Height_Or_Depth(m_C3d_Designer.Prop_Get_Height_Or_Depth() + fValue);
                m_C3d_Designer.Prop_Update_VirtualObject();
            }
            void btnHeight_Up_Click(object sender, EventArgs e)
            {
                if (m_bWheel == true) { m_bWheel = false; return; }
                //throw new NotImplementedException();
                m_C3d_Designer.Prop_Set_Height_Or_Depth(m_C3d_Designer.Prop_Get_Height_Or_Depth() + m_fValue_Height);
                m_C3d_Designer.Prop_Update_VirtualObject();
            }
            void btnHeight_Down_Click(object sender, EventArgs e)
            {
                if (m_bWheel == true) { m_bWheel = false; return; }
                //throw new NotImplementedException();
                m_C3d_Designer.Prop_Set_Height_Or_Depth(m_C3d_Designer.Prop_Get_Height_Or_Depth() - m_fValue_Height);
                m_C3d_Designer.Prop_Update_VirtualObject();
            }
            void btnDepth_MouseWheel(object sender, MouseEventArgs e)
            {
                m_bWheel = true;
                float fValue = (e.Button == MouseButtons.Right) ? 0.1f : 1.0f;
                if (e.Delta < 0) fValue = -fValue;
                m_C3d_Designer.Prop_Set_Depth_Or_Cnt(m_C3d_Designer.Prop_Get_Depth_Or_Cnt() + fValue);
                m_C3d_Designer.Prop_Update_VirtualObject();
            }
            void btnDepth_Up_Click(object sender, EventArgs e)
            {
                if (m_bWheel == true) { m_bWheel = false; return; }
                //throw new NotImplementedException();
                m_C3d_Designer.Prop_Set_Depth_Or_Cnt(m_C3d_Designer.Prop_Get_Depth_Or_Cnt() + m_fValue_Depth);
                m_C3d_Designer.Prop_Update_VirtualObject();
            }
            void btnDepth_Down_Click(object sender, EventArgs e)
            {
                if (m_bWheel == true) { m_bWheel = false; return; }
                //throw new NotImplementedException();
                m_C3d_Designer.Prop_Set_Depth_Or_Cnt(m_C3d_Designer.Prop_Get_Depth_Or_Cnt() - m_fValue_Depth);
                m_C3d_Designer.Prop_Update_VirtualObject();
            }
            void btn_Offset_X_MouseWheel(object sender, MouseEventArgs e)
            {
                m_bWheel = true;
                float fValue = (e.Button == MouseButtons.Right) ? 0.1f : 1.0f;
                if (e.Delta < 0) fValue = -fValue;
                SVector3D_t SVec = m_C3d_Designer.Prop_Get_Offset_Trans();
                SVec.x += fValue;
                m_C3d_Designer.Prop_Set_Offset_Trans(SVec);
                m_C3d_Designer.Prop_Update_VirtualObject();
            }
            void btn_Offset_X_Up_Click(object sender, EventArgs e)
            {
                if (m_bWheel == true) { m_bWheel = false; return; }
                SVector3D_t SVec = m_C3d_Designer.Prop_Get_Offset_Trans();
                SVec.x += m_fValue_Offset_Trans;
                m_C3d_Designer.Prop_Set_Offset_Trans(SVec);
                m_C3d_Designer.Prop_Update_VirtualObject();
            }
            void btn_Offset_X_Down_Click(object sender, EventArgs e)
            {
                if (m_bWheel == true) { m_bWheel = false; return; }
                SVector3D_t SVec = m_C3d_Designer.Prop_Get_Offset_Trans();
                SVec.x -= m_fValue_Offset_Trans;
                m_C3d_Designer.Prop_Set_Offset_Trans(SVec);
                m_C3d_Designer.Prop_Update_VirtualObject();
            }
            void btn_Offset_Y_MouseWheel(object sender, MouseEventArgs e)
            {
                m_bWheel = true;
                float fValue = (e.Button == MouseButtons.Right) ? 0.1f : 1.0f;
                if (e.Delta < 0) fValue = -fValue;
                SVector3D_t SVec = m_C3d_Designer.Prop_Get_Offset_Trans();
                SVec.y += fValue;
                m_C3d_Designer.Prop_Set_Offset_Trans(SVec);
                m_C3d_Designer.Prop_Update_VirtualObject();
            }
            void btn_Offset_Y_Up_Click(object sender, EventArgs e)
            {
                if (m_bWheel == true) { m_bWheel = false; return; }
                SVector3D_t SVec = m_C3d_Designer.Prop_Get_Offset_Trans();
                SVec.y += m_fValue_Offset_Trans;
                m_C3d_Designer.Prop_Set_Offset_Trans(SVec);
                m_C3d_Designer.Prop_Update_VirtualObject();
            }
            void btn_Offset_Y_Down_Click(object sender, EventArgs e)
            {
                if (m_bWheel == true) { m_bWheel = false; return; }
                SVector3D_t SVec = m_C3d_Designer.Prop_Get_Offset_Trans();
                SVec.y -= m_fValue_Offset_Trans;
                m_C3d_Designer.Prop_Set_Offset_Trans(SVec);
                m_C3d_Designer.Prop_Update_VirtualObject();
            }
            void btn_Offset_Z_MouseWheel(object sender, MouseEventArgs e)
            {
                m_bWheel = true;
                float fValue = (e.Button == MouseButtons.Right) ? 0.1f : 1.0f;
                if (e.Delta < 0) fValue = -fValue;
                SVector3D_t SVec = m_C3d_Designer.Prop_Get_Offset_Trans();
                SVec.z += fValue;
                m_C3d_Designer.Prop_Set_Offset_Trans(SVec);
                m_C3d_Designer.Prop_Update_VirtualObject();
            }
            void btn_Offset_Z_Up_Click(object sender, EventArgs e)
            {
                if (m_bWheel == true) { m_bWheel = false; return; }
                SVector3D_t SVec = m_C3d_Designer.Prop_Get_Offset_Trans();
                SVec.z += m_fValue_Offset_Trans;
                m_C3d_Designer.Prop_Set_Offset_Trans(SVec);
                m_C3d_Designer.Prop_Update_VirtualObject();
            }
            void btn_Offset_Z_Down_Click(object sender, EventArgs e)
            {
                if (m_bWheel == true) { m_bWheel = false; return; }
                SVector3D_t SVec = m_C3d_Designer.Prop_Get_Offset_Trans();
                SVec.z -= m_fValue_Offset_Trans;
                m_C3d_Designer.Prop_Set_Offset_Trans(SVec);
                m_C3d_Designer.Prop_Update_VirtualObject();
            }
            void btn_Offset_Pan_MouseWheel(object sender, MouseEventArgs e)
            {
                m_bWheel = true;
                float fValue = (e.Button == MouseButtons.Right) ? 0.1f : 1.0f;
                if (e.Delta < 0) fValue = -fValue;
                SAngle3D_t SAngle = m_C3d_Designer.Prop_Get_Offset_Rot();
                SAngle.pan += fValue;
                m_C3d_Designer.Prop_Set_Offset_Rot(SAngle);
                m_C3d_Designer.Prop_Update_VirtualObject();
            }
            void btn_Offset_Pan_Up_Click(object sender, EventArgs e)
            {
                if (m_bWheel == true) { m_bWheel = false; return; }
                SAngle3D_t SAngle = m_C3d_Designer.Prop_Get_Offset_Rot();
                SAngle.pan += m_fValue_Offset_Rot;
                m_C3d_Designer.Prop_Set_Offset_Rot(SAngle);
                m_C3d_Designer.Prop_Update_VirtualObject();
            }
            void btn_Offset_Pan_Down_Click(object sender, EventArgs e)
            {
                if (m_bWheel == true) { m_bWheel = false; return; }
                SAngle3D_t SAngle = m_C3d_Designer.Prop_Get_Offset_Rot();
                SAngle.pan -= m_fValue_Offset_Rot;
                m_C3d_Designer.Prop_Set_Offset_Rot(SAngle);
                m_C3d_Designer.Prop_Update_VirtualObject();
            }
            void btn_Offset_Tilt_MouseWheel(object sender, MouseEventArgs e)
            {
                m_bWheel = true;
                float fValue = (e.Button == MouseButtons.Right) ? 0.1f : 1.0f;
                if (e.Delta < 0) fValue = -fValue;
                SAngle3D_t SAngle = m_C3d_Designer.Prop_Get_Offset_Rot();
                SAngle.tilt += fValue;
                m_C3d_Designer.Prop_Set_Offset_Rot(SAngle);
                m_C3d_Designer.Prop_Update_VirtualObject();
            }
            void btn_Offset_Tilt_Up_Click(object sender, EventArgs e)
            {
                if (m_bWheel == true) { m_bWheel = false; return; }
                SAngle3D_t SAngle = m_C3d_Designer.Prop_Get_Offset_Rot();
                SAngle.tilt += m_fValue_Offset_Rot;
                m_C3d_Designer.Prop_Set_Offset_Rot(SAngle);
                m_C3d_Designer.Prop_Update_VirtualObject();
            }
            void btn_Offset_Tilt_Down_Click(object sender, EventArgs e)
            {
                if (m_bWheel == true) { m_bWheel = false; return; }
                SAngle3D_t SAngle = m_C3d_Designer.Prop_Get_Offset_Rot();
                SAngle.tilt -= m_fValue_Offset_Rot;
                m_C3d_Designer.Prop_Set_Offset_Rot(SAngle);
                m_C3d_Designer.Prop_Update_VirtualObject();
            }
            void btn_Offset_Swing_MouseWheel(object sender, MouseEventArgs e)
            {
                m_bWheel = true;
                float fValue = (e.Button == MouseButtons.Right) ? 0.1f : 1.0f;
                if (e.Delta < 0) fValue = -fValue;
                SAngle3D_t SAngle = m_C3d_Designer.Prop_Get_Offset_Rot();
                SAngle.swing += fValue;
                m_C3d_Designer.Prop_Set_Offset_Rot(SAngle);
                m_C3d_Designer.Prop_Update_VirtualObject();
            }
            void btn_Offset_Swing_Up_Click(object sender, EventArgs e)
            {
                if (m_bWheel == true) { m_bWheel = false; return; }
                SAngle3D_t SAngle = m_C3d_Designer.Prop_Get_Offset_Rot();
                SAngle.swing += m_fValue_Offset_Rot;
                m_C3d_Designer.Prop_Set_Offset_Rot(SAngle);
                m_C3d_Designer.Prop_Update_VirtualObject();
            }
            void btn_Offset_Swing_Down_Click(object sender, EventArgs e)
            {
                if (m_bWheel == true) { m_bWheel = false; return; }
                SAngle3D_t SAngle = m_C3d_Designer.Prop_Get_Offset_Rot();
                SAngle.swing -= m_fValue_Offset_Rot;
                m_C3d_Designer.Prop_Set_Offset_Rot(SAngle);
                m_C3d_Designer.Prop_Update_VirtualObject();
            }
            
            void btn_X_MouseWheel(object sender, MouseEventArgs e)
            {
                m_bWheel = true;
                float fValue = (e.Button == MouseButtons.Right) ? 0.1f : 1.0f;
                if (e.Delta < 0) fValue = -fValue;
                SVector3D_t SVec = m_C3d_Designer.Prop_Get_Trans_1();
                SVec.x += fValue;
                m_C3d_Designer.Prop_Set_Trans_1(SVec);
                m_C3d_Designer.Prop_Update_VirtualObject();
            }
            void btn_X_Up_Click(object sender, EventArgs e)
            {
                if (m_bWheel == true) { m_bWheel = false; return; }
                SVector3D_t SVec = m_C3d_Designer.Prop_Get_Trans_1();
                SVec.x += m_fValue_Trans;
                m_C3d_Designer.Prop_Set_Trans_1(SVec);
                m_C3d_Designer.Prop_Update_VirtualObject();
            }
            void btn_X_Down_Click(object sender, EventArgs e)
            {
                if (m_bWheel == true) { m_bWheel = false; return; }
                SVector3D_t SVec = m_C3d_Designer.Prop_Get_Trans_1();
                SVec.x -= m_fValue_Trans;
                m_C3d_Designer.Prop_Set_Trans_1(SVec);
                m_C3d_Designer.Prop_Update_VirtualObject();
            }
            void btn_Y_MouseWheel(object sender, MouseEventArgs e)
            {
                m_bWheel = true;
                float fValue = (e.Button == MouseButtons.Right) ? 0.1f : 1.0f;
                if (e.Delta < 0) fValue = -fValue;
                SVector3D_t SVec = m_C3d_Designer.Prop_Get_Trans_1();
                SVec.y += fValue;
                m_C3d_Designer.Prop_Set_Trans_1(SVec);
                m_C3d_Designer.Prop_Update_VirtualObject();
            }
            void btn_Y_Up_Click(object sender, EventArgs e)
            {
                if (m_bWheel == true) { m_bWheel = false; return; }
                SVector3D_t SVec = m_C3d_Designer.Prop_Get_Trans_1();
                SVec.y += m_fValue_Trans;
                m_C3d_Designer.Prop_Set_Trans_1(SVec);
                m_C3d_Designer.Prop_Update_VirtualObject();
            }
            void btn_Y_Down_Click(object sender, EventArgs e)
            {
                if (m_bWheel == true) { m_bWheel = false; return; }
                SVector3D_t SVec = m_C3d_Designer.Prop_Get_Trans_1();
                SVec.y -= m_fValue_Trans;
                m_C3d_Designer.Prop_Set_Trans_1(SVec);
                m_C3d_Designer.Prop_Update_VirtualObject();
            }
            void btn_Z_MouseWheel(object sender, MouseEventArgs e)
            {
                m_bWheel = true;
                float fValue = (e.Button == MouseButtons.Right) ? 0.1f : 1.0f;
                if (e.Delta < 0) fValue = -fValue;
                SVector3D_t SVec = m_C3d_Designer.Prop_Get_Trans_1();
                SVec.z += fValue;
                m_C3d_Designer.Prop_Set_Trans_1(SVec);
                m_C3d_Designer.Prop_Update_VirtualObject();
            }
            void btn_Z_Up_Click(object sender, EventArgs e)
            {
                if (m_bWheel == true) { m_bWheel = false; return; }
                SVector3D_t SVec = m_C3d_Designer.Prop_Get_Trans_1();
                SVec.z += m_fValue_Trans;
                m_C3d_Designer.Prop_Set_Trans_1(SVec);
                m_C3d_Designer.Prop_Update_VirtualObject();
            }
            void btn_Z_Down_Click(object sender, EventArgs e)
            {
                if (m_bWheel == true) { m_bWheel = false; return; }
                SVector3D_t SVec = m_C3d_Designer.Prop_Get_Trans_1();
                SVec.z -= m_fValue_Trans;
                m_C3d_Designer.Prop_Set_Trans_1(SVec);
                m_C3d_Designer.Prop_Update_VirtualObject();
            }
            void btn_Pan_MouseWheel(object sender, MouseEventArgs e)
            {
                m_bWheel = true;
                float fValue = (e.Button == MouseButtons.Right) ? 0.1f : 1.0f;
                if (e.Delta < 0) fValue = -fValue;
                SAngle3D_t SAngle = m_C3d_Designer.Prop_Get_Rot_1();
                SAngle.pan += fValue;
                m_C3d_Designer.Prop_Set_Rot_1(SAngle);
                m_C3d_Designer.Prop_Update_VirtualObject();
            }
            void btn_Pan_Up_Click(object sender, EventArgs e)
            {
                if (m_bWheel == true) { m_bWheel = false; return; }
                SAngle3D_t SAngle = m_C3d_Designer.Prop_Get_Rot_1();
                SAngle.pan += m_fValue_Rot;
                m_C3d_Designer.Prop_Set_Rot_1(SAngle);
                m_C3d_Designer.Prop_Update_VirtualObject();
            }
            void btn_Pan_Down_Click(object sender, EventArgs e)
            {
                if (m_bWheel == true) { m_bWheel = false; return; }
                SAngle3D_t SAngle = m_C3d_Designer.Prop_Get_Rot_1();
                SAngle.pan -= m_fValue_Rot;
                m_C3d_Designer.Prop_Set_Rot_1(SAngle);
                m_C3d_Designer.Prop_Update_VirtualObject();
            }
            void btn_Tilt_MouseWheel(object sender, MouseEventArgs e)
            {
                m_bWheel = true;
                float fValue = (e.Button == MouseButtons.Right) ? 0.1f : 1.0f;
                if (e.Delta < 0) fValue = -fValue;
                SAngle3D_t SAngle = m_C3d_Designer.Prop_Get_Rot_1();
                SAngle.tilt += fValue;
                m_C3d_Designer.Prop_Set_Rot_1(SAngle);
                m_C3d_Designer.Prop_Update_VirtualObject();
            }
            void btn_Tilt_Up_Click(object sender, EventArgs e)
            {
                if (m_bWheel == true) { m_bWheel = false; return; }
                SAngle3D_t SAngle = m_C3d_Designer.Prop_Get_Rot_1();
                SAngle.tilt += m_fValue_Rot;
                m_C3d_Designer.Prop_Set_Rot_1(SAngle);
                m_C3d_Designer.Prop_Update_VirtualObject();
            }
            void btn_Tilt_Down_Click(object sender, EventArgs e)
            {
                if (m_bWheel == true) { m_bWheel = false; return; }
                SAngle3D_t SAngle = m_C3d_Designer.Prop_Get_Rot_1();
                SAngle.tilt -= m_fValue_Rot;
                m_C3d_Designer.Prop_Set_Rot_1(SAngle);
                m_C3d_Designer.Prop_Update_VirtualObject();
            }
            void btn_Swing_MouseWheel(object sender, MouseEventArgs e)
            {
                m_bWheel = true;
                float fValue = (e.Button == MouseButtons.Right) ? 0.1f : 1.0f;
                if (e.Delta < 0) fValue = -fValue;
                SAngle3D_t SAngle = m_C3d_Designer.Prop_Get_Rot_1();
                SAngle.swing += fValue;
                m_C3d_Designer.Prop_Set_Rot_1(SAngle);
                m_C3d_Designer.Prop_Update_VirtualObject();
            }
            void btn_Swing_Up_Click(object sender, EventArgs e)
            {
                if (m_bWheel == true) { m_bWheel = false; return; }
                SAngle3D_t SAngle = m_C3d_Designer.Prop_Get_Rot_1();
                SAngle.swing += m_fValue_Rot;
                m_C3d_Designer.Prop_Set_Rot_1(SAngle);
                m_C3d_Designer.Prop_Update_VirtualObject();
            }
            void btn_Swing_Down_Click(object sender, EventArgs e)
            {
                if (m_bWheel == true) { m_bWheel = false; return; }
                SAngle3D_t SAngle = m_C3d_Designer.Prop_Get_Rot_1();
                SAngle.swing -= m_fValue_Rot;
                m_C3d_Designer.Prop_Set_Rot_1(SAngle);
                m_C3d_Designer.Prop_Update_VirtualObject();
            }
            #endregion Quick Button(For Virtual Object)
            
            #region Quick Button(For Selected Object)
            void btnWidth_Selected_MouseWheel(object sender, MouseEventArgs e)
            {
                m_bWheel = true;
                float fValue = (e.Button == MouseButtons.Right) ? 0.1f : 1.0f;
                if (e.Delta < 0) fValue = -fValue;
                m_C3d_Designer.Prop_Set_Width_Or_Radius_Selected(m_C3d_Designer.Prop_Get_Width_Or_Radius_Selected() + fValue);
                m_C3d_Designer.Prop_Update_Selected();
            }
            void btnWidth_Up_Selected_Click(object sender, EventArgs e)
            {
                if (m_bWheel == true) { m_bWheel = false; return; }
                //throw new NotImplementedException();
                m_C3d_Designer.Prop_Set_Width_Or_Radius_Selected(m_C3d_Designer.Prop_Get_Width_Or_Radius_Selected() + m_fValue_Width);
                m_C3d_Designer.Prop_Update_Selected();
            }
            void btnWidth_Down_Selected_Click(object sender, EventArgs e)
            {
                if (m_bWheel == true) { m_bWheel = false; return; }
                //throw new NotImplementedException();
                m_C3d_Designer.Prop_Set_Width_Or_Radius_Selected(m_C3d_Designer.Prop_Get_Width_Or_Radius_Selected() - m_fValue_Width);
                m_C3d_Designer.Prop_Update_Selected();
            }
            void btnHeight_Selected_MouseWheel(object sender, MouseEventArgs e)
            {
                m_bWheel = true;
                float fValue = (e.Button == MouseButtons.Right) ? 0.1f : 1.0f;
                if (e.Delta < 0) fValue = -fValue;
                m_C3d_Designer.Prop_Set_Height_Or_Depth_Selected(m_C3d_Designer.Prop_Get_Height_Or_Depth_Selected() + fValue);
                m_C3d_Designer.Prop_Update_Selected();
            }
            void btnHeight_Up_Selected_Click(object sender, EventArgs e)
            {
                if (m_bWheel == true) { m_bWheel = false; return; }
                //throw new NotImplementedException();
                m_C3d_Designer.Prop_Set_Height_Or_Depth_Selected(m_C3d_Designer.Prop_Get_Height_Or_Depth_Selected() + m_fValue_Height);
                m_C3d_Designer.Prop_Update_Selected();
            }
            void btnHeight_Down_Selected_Click(object sender, EventArgs e)
            {
                if (m_bWheel == true) { m_bWheel = false; return; }
                //throw new NotImplementedException();
                m_C3d_Designer.Prop_Set_Height_Or_Depth_Selected(m_C3d_Designer.Prop_Get_Height_Or_Depth_Selected() - m_fValue_Height);
                m_C3d_Designer.Prop_Update_Selected();
            }
            void btnDepth_Selected_MouseWheel(object sender, MouseEventArgs e)
            {
                m_bWheel = true;
                float fValue = (e.Button == MouseButtons.Right) ? 0.1f : 1.0f;
                if (e.Delta < 0) fValue = -fValue;
                m_C3d_Designer.Prop_Set_Depth_Or_Cnt_Selected(m_C3d_Designer.Prop_Get_Depth_Or_Cnt_Selected() + fValue);
                m_C3d_Designer.Prop_Update_Selected();
            }
            void btnDepth_Up_Selected_Click(object sender, EventArgs e)
            {
                if (m_bWheel == true) { m_bWheel = false; return; }
                //throw new NotImplementedException();
                m_C3d_Designer.Prop_Set_Depth_Or_Cnt_Selected(m_C3d_Designer.Prop_Get_Depth_Or_Cnt_Selected() + m_fValue_Depth);
                m_C3d_Designer.Prop_Update_Selected();
            }
            void btnDepth_Down_Selected_Click(object sender, EventArgs e)
            {
                if (m_bWheel == true) { m_bWheel = false; return; }
                //throw new NotImplementedException();
                m_C3d_Designer.Prop_Set_Depth_Or_Cnt_Selected(m_C3d_Designer.Prop_Get_Depth_Or_Cnt_Selected() - m_fValue_Depth);
                m_C3d_Designer.Prop_Update_Selected();
            }
            void btn_Offset_X_Selected_MouseWheel(object sender, MouseEventArgs e)
            {
                m_bWheel = true;
                float fValue = (e.Button == MouseButtons.Right) ? 0.1f : 1.0f;
                if (e.Delta < 0) fValue = -fValue;
                SVector3D_t SVec = m_C3d_Designer.Prop_Get_Offset_Trans_Selected();
                SVec.x += fValue;
                m_C3d_Designer.Prop_Set_Offset_Trans_Selected(SVec);
                m_C3d_Designer.Prop_Update_Selected();
            }
            void btn_Offset_X_Up_Selected_Click(object sender, EventArgs e)
            {
                if (m_bWheel == true) { m_bWheel = false; return; }
                SVector3D_t SVec = m_C3d_Designer.Prop_Get_Offset_Trans_Selected();
                SVec.x += m_fValue_Offset_Trans;
                m_C3d_Designer.Prop_Set_Offset_Trans_Selected(SVec);
                m_C3d_Designer.Prop_Update_Selected();
            }
            void btn_Offset_X_Down_Selected_Click(object sender, EventArgs e)
            {
                if (m_bWheel == true) { m_bWheel = false; return; }
                SVector3D_t SVec = m_C3d_Designer.Prop_Get_Offset_Trans_Selected();
                SVec.x -= m_fValue_Offset_Trans;
                m_C3d_Designer.Prop_Set_Offset_Trans_Selected(SVec);
                m_C3d_Designer.Prop_Update_Selected();
            }
            void btn_Offset_Y_Selected_MouseWheel(object sender, MouseEventArgs e)
            {
                m_bWheel = true;
                float fValue = (e.Button == MouseButtons.Right) ? 0.1f : 1.0f;
                if (e.Delta < 0) fValue = -fValue;
                SVector3D_t SVec = m_C3d_Designer.Prop_Get_Offset_Trans_Selected();
                SVec.y += fValue;
                m_C3d_Designer.Prop_Set_Offset_Trans_Selected(SVec);
                m_C3d_Designer.Prop_Update_Selected();
            }
            void btn_Offset_Y_Up_Selected_Click(object sender, EventArgs e)
            {
                if (m_bWheel == true) { m_bWheel = false; return; }
                SVector3D_t SVec = m_C3d_Designer.Prop_Get_Offset_Trans_Selected();
                SVec.y += m_fValue_Offset_Trans;
                m_C3d_Designer.Prop_Set_Offset_Trans_Selected(SVec);
                m_C3d_Designer.Prop_Update_Selected();
            }
            void btn_Offset_Y_Down_Selected_Click(object sender, EventArgs e)
            {
                if (m_bWheel == true) { m_bWheel = false; return; }
                SVector3D_t SVec = m_C3d_Designer.Prop_Get_Offset_Trans_Selected();
                SVec.y -= m_fValue_Offset_Trans;
                m_C3d_Designer.Prop_Set_Offset_Trans_Selected(SVec);
                m_C3d_Designer.Prop_Update_Selected();
            }
            void btn_Offset_Z_Selected_MouseWheel(object sender, MouseEventArgs e)
            {
                m_bWheel = true;
                float fValue = (e.Button == MouseButtons.Right) ? 0.1f : 1.0f;
                if (e.Delta < 0) fValue = -fValue;
                SVector3D_t SVec = m_C3d_Designer.Prop_Get_Offset_Trans_Selected();
                SVec.z += fValue;
                m_C3d_Designer.Prop_Set_Offset_Trans_Selected(SVec);
                m_C3d_Designer.Prop_Update_Selected();
            }
            void btn_Offset_Z_Up_Selected_Click(object sender, EventArgs e)
            {
                if (m_bWheel == true) { m_bWheel = false; return; }
                SVector3D_t SVec = m_C3d_Designer.Prop_Get_Offset_Trans_Selected();
                SVec.z += m_fValue_Offset_Trans;
                m_C3d_Designer.Prop_Set_Offset_Trans_Selected(SVec);
                m_C3d_Designer.Prop_Update_Selected();
            }
            void btn_Offset_Z_Down_Selected_Click(object sender, EventArgs e)
            {
                if (m_bWheel == true) { m_bWheel = false; return; }
                SVector3D_t SVec = m_C3d_Designer.Prop_Get_Offset_Trans_Selected();
                SVec.z -= m_fValue_Offset_Trans;
                m_C3d_Designer.Prop_Set_Offset_Trans_Selected(SVec);
                m_C3d_Designer.Prop_Update_Selected();
            }
            void btn_Offset_Pan_Selected_MouseWheel(object sender, MouseEventArgs e)
            {
                m_bWheel = true;
                float fValue = (e.Button == MouseButtons.Right) ? 0.1f : 1.0f;
                if (e.Delta < 0) fValue = -fValue;
                SAngle3D_t SAngle = m_C3d_Designer.Prop_Get_Offset_Rot_Selected();
                SAngle.pan += fValue;
                m_C3d_Designer.Prop_Set_Offset_Rot_Selected(SAngle);
                m_C3d_Designer.Prop_Update_Selected();
            }
            void btn_Offset_Pan_Up_Selected_Click(object sender, EventArgs e)
            {
                if (m_bWheel == true) { m_bWheel = false; return; }
                SAngle3D_t SAngle = m_C3d_Designer.Prop_Get_Offset_Rot_Selected();
                SAngle.pan += m_fValue_Offset_Rot;
                m_C3d_Designer.Prop_Set_Offset_Rot_Selected(SAngle);
                m_C3d_Designer.Prop_Update_Selected();
            }
            void btn_Offset_Pan_Down_Selected_Click(object sender, EventArgs e)
            {
                if (m_bWheel == true) { m_bWheel = false; return; }
                SAngle3D_t SAngle = m_C3d_Designer.Prop_Get_Offset_Rot_Selected();
                SAngle.pan -= m_fValue_Offset_Rot;
                m_C3d_Designer.Prop_Set_Offset_Rot_Selected(SAngle);
                m_C3d_Designer.Prop_Update_Selected();
            }
            void btn_Offset_Tilt_Selected_MouseWheel(object sender, MouseEventArgs e)
            {
                m_bWheel = true;
                float fValue = (e.Button == MouseButtons.Right) ? 0.1f : 1.0f;
                if (e.Delta < 0) fValue = -fValue;
                SAngle3D_t SAngle = m_C3d_Designer.Prop_Get_Offset_Rot_Selected();
                SAngle.tilt += fValue;
                m_C3d_Designer.Prop_Set_Offset_Rot_Selected(SAngle);
                m_C3d_Designer.Prop_Update_Selected();
            }
            void btn_Offset_Tilt_Up_Selected_Click(object sender, EventArgs e)
            {
                if (m_bWheel == true) { m_bWheel = false; return; }
                SAngle3D_t SAngle = m_C3d_Designer.Prop_Get_Offset_Rot_Selected();
                SAngle.tilt += m_fValue_Offset_Rot;
                m_C3d_Designer.Prop_Set_Offset_Rot_Selected(SAngle);
                m_C3d_Designer.Prop_Update_Selected();
            }
            void btn_Offset_Tilt_Down_Selected_Click(object sender, EventArgs e)
            {
                if (m_bWheel == true) { m_bWheel = false; return; }
                SAngle3D_t SAngle = m_C3d_Designer.Prop_Get_Offset_Rot_Selected();
                SAngle.tilt -= m_fValue_Offset_Rot;
                m_C3d_Designer.Prop_Set_Offset_Rot_Selected(SAngle);
                m_C3d_Designer.Prop_Update_Selected();
            }
            void btn_Offset_Swing_Selected_MouseWheel(object sender, MouseEventArgs e)
            {
                m_bWheel = true;
                float fValue = (e.Button == MouseButtons.Right) ? 0.1f : 1.0f;
                if (e.Delta < 0) fValue = -fValue;
                SAngle3D_t SAngle = m_C3d_Designer.Prop_Get_Offset_Rot_Selected();
                SAngle.swing += fValue;
                m_C3d_Designer.Prop_Set_Offset_Rot_Selected(SAngle);
                m_C3d_Designer.Prop_Update_Selected();
            }
            void btn_Offset_Swing_Up_Selected_Click(object sender, EventArgs e)
            {
                if (m_bWheel == true) { m_bWheel = false; return; }
                SAngle3D_t SAngle = m_C3d_Designer.Prop_Get_Offset_Rot_Selected();
                SAngle.swing += m_fValue_Offset_Rot;
                m_C3d_Designer.Prop_Set_Offset_Rot_Selected(SAngle);
                m_C3d_Designer.Prop_Update_Selected();
            }
            void btn_Offset_Swing_Down_Selected_Click(object sender, EventArgs e)
            {
                if (m_bWheel == true) { m_bWheel = false; return; }
                SAngle3D_t SAngle = m_C3d_Designer.Prop_Get_Offset_Rot_Selected();
                SAngle.swing -= m_fValue_Offset_Rot;
                m_C3d_Designer.Prop_Set_Offset_Rot_Selected(SAngle);
                m_C3d_Designer.Prop_Update_Selected();
            }

            void btn_X_Selected_MouseWheel(object sender, MouseEventArgs e)
            {
                m_bWheel = true;
                float fValue = (e.Button == MouseButtons.Right) ? 0.1f : 1.0f;
                if (e.Delta < 0) fValue = -fValue;
                SVector3D_t SVec = m_C3d_Designer.Prop_Get_Trans_1_Selected();
                SVec.x += fValue;
                m_C3d_Designer.Prop_Set_Trans_1_Selected(SVec);
                m_C3d_Designer.Prop_Update_Selected();
            }
            void btn_X_Up_Selected_Click(object sender, EventArgs e)
            {
                if (m_bWheel == true) { m_bWheel = false; return; }
                SVector3D_t SVec = m_C3d_Designer.Prop_Get_Trans_1_Selected();
                SVec.x += m_fValue_Trans;
                m_C3d_Designer.Prop_Set_Trans_1_Selected(SVec);
                m_C3d_Designer.Prop_Update_Selected();
            }
            void btn_X_Down_Selected_Click(object sender, EventArgs e)
            {
                if (m_bWheel == true) { m_bWheel = false; return; }
                SVector3D_t SVec = m_C3d_Designer.Prop_Get_Trans_1_Selected();
                SVec.x -= m_fValue_Trans;
                m_C3d_Designer.Prop_Set_Trans_1_Selected(SVec);
                m_C3d_Designer.Prop_Update_Selected();
            }
            void btn_Y_Selected_MouseWheel(object sender, MouseEventArgs e)
            {
                m_bWheel = true;
                float fValue = (e.Button == MouseButtons.Right) ? 0.1f : 1.0f;
                if (e.Delta < 0) fValue = -fValue;
                SVector3D_t SVec = m_C3d_Designer.Prop_Get_Trans_1_Selected();
                SVec.y += fValue;
                m_C3d_Designer.Prop_Set_Trans_1_Selected(SVec);
                m_C3d_Designer.Prop_Update_Selected();
            }
            void btn_Y_Up_Selected_Click(object sender, EventArgs e)
            {
                if (m_bWheel == true) { m_bWheel = false; return; }
                SVector3D_t SVec = m_C3d_Designer.Prop_Get_Trans_1_Selected();
                SVec.y += m_fValue_Trans;
                m_C3d_Designer.Prop_Set_Trans_1_Selected(SVec);
                m_C3d_Designer.Prop_Update_Selected();
            }
            void btn_Y_Down_Selected_Click(object sender, EventArgs e)
            {
                if (m_bWheel == true) { m_bWheel = false; return; }
                SVector3D_t SVec = m_C3d_Designer.Prop_Get_Trans_1_Selected();
                SVec.y -= m_fValue_Trans;
                m_C3d_Designer.Prop_Set_Trans_1_Selected(SVec);
                m_C3d_Designer.Prop_Update_Selected();
            }
            void btn_Z_Selected_MouseWheel(object sender, MouseEventArgs e)
            {
                m_bWheel = true;
                float fValue = (e.Button == MouseButtons.Right) ? 0.1f : 1.0f;
                if (e.Delta < 0) fValue = -fValue;
                SVector3D_t SVec = m_C3d_Designer.Prop_Get_Trans_1_Selected();
                SVec.z += fValue;
                m_C3d_Designer.Prop_Set_Trans_1_Selected(SVec);
                m_C3d_Designer.Prop_Update_Selected();
            }
            void btn_Z_Up_Selected_Click(object sender, EventArgs e)
            {
                if (m_bWheel == true) { m_bWheel = false; return; }
                SVector3D_t SVec = m_C3d_Designer.Prop_Get_Trans_1_Selected();
                SVec.z += m_fValue_Trans;
                m_C3d_Designer.Prop_Set_Trans_1_Selected(SVec);
                m_C3d_Designer.Prop_Update_Selected();
            }
            void btn_Z_Down_Selected_Click(object sender, EventArgs e)
            {
                if (m_bWheel == true) { m_bWheel = false; return; }
                SVector3D_t SVec = m_C3d_Designer.Prop_Get_Trans_1_Selected();
                SVec.z -= m_fValue_Trans;
                m_C3d_Designer.Prop_Set_Trans_1_Selected(SVec);
                m_C3d_Designer.Prop_Update_Selected();
            }
            void btn_Pan_Selected_MouseWheel(object sender, MouseEventArgs e)
            {
                m_bWheel = true;
                float fValue = (e.Button == MouseButtons.Right) ? 0.1f : 1.0f;
                if (e.Delta < 0) fValue = -fValue;
                SAngle3D_t SAngle = m_C3d_Designer.Prop_Get_Rot_1_Selected();
                SAngle.pan += fValue;
                m_C3d_Designer.Prop_Set_Rot_1_Selected(SAngle);
                m_C3d_Designer.Prop_Update_Selected();
            }
            void btn_Pan_Up_Selected_Click(object sender, EventArgs e)
            {
                if (m_bWheel == true) { m_bWheel = false; return; }
                SAngle3D_t SAngle = m_C3d_Designer.Prop_Get_Rot_1_Selected();
                SAngle.pan += m_fValue_Rot;
                m_C3d_Designer.Prop_Set_Rot_1_Selected(SAngle);
                m_C3d_Designer.Prop_Update_Selected();
            }
            void btn_Pan_Down_Selected_Click(object sender, EventArgs e)
            {
                if (m_bWheel == true) { m_bWheel = false; return; }
                SAngle3D_t SAngle = m_C3d_Designer.Prop_Get_Rot_1_Selected();
                SAngle.pan -= m_fValue_Rot;
                m_C3d_Designer.Prop_Set_Rot_1_Selected(SAngle);
                m_C3d_Designer.Prop_Update_Selected();
            }
            void btn_Tilt_Selected_MouseWheel(object sender, MouseEventArgs e)
            {
                m_bWheel = true;
                float fValue = (e.Button == MouseButtons.Right) ? 0.1f : 1.0f;
                if (e.Delta < 0) fValue = -fValue;
                SAngle3D_t SAngle = m_C3d_Designer.Prop_Get_Rot_1_Selected();
                SAngle.tilt += fValue;
                m_C3d_Designer.Prop_Set_Rot_1_Selected(SAngle);
                m_C3d_Designer.Prop_Update_Selected();
            }
            void btn_Tilt_Up_Selected_Click(object sender, EventArgs e)
            {
                if (m_bWheel == true) { m_bWheel = false; return; }
                SAngle3D_t SAngle = m_C3d_Designer.Prop_Get_Rot_1_Selected();
                SAngle.tilt += m_fValue_Rot;
                m_C3d_Designer.Prop_Set_Rot_1_Selected(SAngle);
                m_C3d_Designer.Prop_Update_Selected();
            }
            void btn_Tilt_Down_Selected_Click(object sender, EventArgs e)
            {
                if (m_bWheel == true) { m_bWheel = false; return; }
                SAngle3D_t SAngle = m_C3d_Designer.Prop_Get_Rot_1_Selected();
                SAngle.tilt -= m_fValue_Rot;
                m_C3d_Designer.Prop_Set_Rot_1_Selected(SAngle);
                m_C3d_Designer.Prop_Update_Selected();
            }
            void btn_Swing_Selected_MouseWheel(object sender, MouseEventArgs e)
            {
                m_bWheel = true;
                float fValue = (e.Button == MouseButtons.Right) ? 0.1f : 1.0f;
                if (e.Delta < 0) fValue = -fValue;
                SAngle3D_t SAngle = m_C3d_Designer.Prop_Get_Rot_1_Selected();
                SAngle.swing += fValue;
                m_C3d_Designer.Prop_Set_Rot_1_Selected(SAngle);
                m_C3d_Designer.Prop_Update_Selected();
            }
            void btn_Swing_Up_Selected_Click(object sender, EventArgs e)
            {
                if (m_bWheel == true) { m_bWheel = false; return; }
                SAngle3D_t SAngle = m_C3d_Designer.Prop_Get_Rot_1_Selected();
                SAngle.swing += m_fValue_Rot;
                m_C3d_Designer.Prop_Set_Rot_1_Selected(SAngle);
                m_C3d_Designer.Prop_Update_Selected();
            }
            void btn_Swing_Down_Selected_Click(object sender, EventArgs e)
            {
                if (m_bWheel == true) { m_bWheel = false; return; }
                SAngle3D_t SAngle = m_C3d_Designer.Prop_Get_Rot_1_Selected();
                SAngle.swing -= m_fValue_Rot;
                m_C3d_Designer.Prop_Set_Rot_1_Selected(SAngle);
                m_C3d_Designer.Prop_Update_Selected();
            }
            #endregion Quick Button(For Selected Object)
            
            //private bool m_bResigin = false;
            private float m_fFormScale = 1.0f;
            void frmTool_Designer_SizeChanged(object sender, EventArgs e)
            {
#if false
                //if (m_bResigin == true)
                //{
                //    m_bResigin = false;
                //    return;
                //}
                //m_bResigin = true;
                Form frmTool = ((Form)sender);
                //if ((frmTool.Width - m_nSize_W) + (frmTool.Height - _SIZE_H) < 5) return;
                float fW = (float)((float)(frmTool.Width) / (float)_SIZE_W);
                float fH = (float)((float)(frmTool.Height) / (float)_SIZE_H);
                float fScale = ((fW < fH) ? fW : fH);
                if ((float)Math.Abs(fScale - m_fFormScale) < 0.1f) return; // 변화 없음
                float fRatio_W = fScale;// 0.5f;
                float fRatio_H = fScale;//0.5f;
                frmTool.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi; // (Font 로 되어 있으면 영문윈도우시 깨짐)
                frmTool.Scale(new SizeF(fRatio_W, fRatio_H));
                foreach (Control control in frmTool.Controls)
                {
                    control.Font = new Font("Verdana", control.Font.SizeInPoints * fRatio_W * fRatio_H);
                }
                ///////////////////////////////////
                m_nSize_W = (int)Math.Round(_SIZE_W * fScale);// frmTool_Designer.Width;
                m_nSize_H = (int)Math.Round(_SIZE_H * fScale);// frmTool_Designer.Height;
                //throw new NotImplementedException();
#else
                return; // OJW5014_20150831
                Form frmMain = (Form)sender;
                //int nW = (frmMain.Width - m_nSize_W);
                //int nH = (frmMain.Height - m_nSize_H);
                //if (nW < 0) nW = 0;
                //if (nH < 0) nH = 0;
                float fScale_W = (float)frmMain.Width / (float)m_nSize_W;
                float fScale_H = (float)frmMain.Height / (float)m_nSize_H;
                float fScale = (fScale_W < fScale_H) ? fScale_W : fScale_H;
                //m_nSize_W = (int)Math.Round(_SIZE_W * fScale);// frmTool_Designer.Width;
                //m_nSize_H = (int)Math.Round(_SIZE_H * fScale);// frmTool_Designer.Height;

                int nW = (int)Math.Round(m_nSize_DRAW_W * m_fFormScale * fScale);
                int nH = (int)Math.Round(m_nSize_DRAW_H * m_fFormScale * fScale);
                m_pnDrawModel.Size = new Size(nW, nH);

                int nLeft = 10;
                m_tabAngle.Left = m_pnDrawModel.Left;
                m_tabAngle.Top = m_pnDrawModel.Bottom + 10;
                m_tbCtrl.Size = new Size(frmMain.Width - 30 - nLeft - m_pnDrawModel.Right, m_tabAngle.Bottom - m_pnDrawModel.Top);
                m_tbCtrl.Location = new Point(m_pnDrawModel.Right + nLeft, m_pnDrawModel.Top);
                

                //m_tbCtrl.Left = m_pntPos_TabParam.X + nW;
                //m_tabAngle.Top = m_pntPos_TestAngle.Y + nH;
                //txtDraw.Width = m_pntSize_DrawBox.X + nW;
                //txtDraw.Top = m_pntPos_DrawBox.Y + nH;
                //m_rtxtDraw.Width = m_pntSize_DrawBox.X + nW;
                //m_rtxtDraw.Top = m_pntPos_DrawBox.Y + nH;

                //btnAdd.Top = txtDraw.Top;
                ////btnAdd.Height = txtDraw.Height;

                //m_pnDrawModel.Width = m_nDrawWidth + nW;
                //m_pnDrawModel.Height = m_nDrawHeight + nH;
#endif
            }
            
            void Init3D(Panel pnDrawModel)
            {
                m_C3d_Designer = new Ojw.C3d();
                m_C3d_Designer.Init(pnDrawModel);
                m_C3d_Designer.GetHandle().MouseDoubleClick += new MouseEventHandler(CTools_MouseDoubleClick);
                m_C3d_Designer.SetAseFile_Path("ase");

                // 기준축 보이기
                m_C3d_Designer.SetStandardAxis(true);//(!chkHideAxis.Checked);
                // 빛 사용
                m_C3d_Designer.Enable_Light(true);//(chkLight.Checked);

                // 클릭한 부분 색 / 투명도 지정
                //m_C3d_Designer.SetAlpha_Display_Enalbe(true);
                m_C3d_Designer.SetPick_ColorMode(true);
                m_C3d_Designer.SetPick_ColorValue(Color.Green);
                m_C3d_Designer.SetPick_AlphaMode(true);
                m_C3d_Designer.SetPick_AlphaValue(0.5f);

                m_C3d_Designer.SetVirtualClass_Enable(true);

                #region PropertyGrid
                
                m_C3d_Designer.CreateProb_VirtualObject(m_pnProperty);
                m_C3d_Designer.CreateProp_Selected(m_pnProperty_Selected, null);
                
                m_C3d_Designer.InitTools_Motor(m_pnMotors);
                //m_C3d_Designer.InitTools_Status(pnStatus);
                //m_pnKinematics.Size = new Size(900, 778);
                //m_pnKinematics.Location = new Point(6, 9);
                m_C3d_Designer.SetTextboxes_ForAngle(m_atxtAngle);
                m_C3d_Designer.InitTools_Kinematics(m_pnKinematics);
                //m_C3d_Designer.InitTools_Background(pnBackground);
                // 
                m_C3d_Designer.Prop_Set_Main_ShowStandardAxis(true);
                m_C3d_Designer.Prop_Set_Main_ShowVirtualAxis(true);
                m_C3d_Designer.Prop_Update_VirtualObject();

                // Draw Text 위치 정하기
                m_C3d_Designer.SetDrawText_ForDisplay(m_rtxtDraw);

                #endregion PropertyGrid
            }
            private void DInit3D()
            {
                m_C3d_Designer.DInit();

            }

            private bool m_bExpand = false;
            void CTools_MouseDoubleClick(object sender, MouseEventArgs e)
            {
                //throw new NotImplementedException();
                if (m_bExpand == true)
                {
                    m_bExpand = false;
                    m_pnDrawModel.Size = new Size(m_nDrawWidth, m_nDrawHeight);
                    //m_pnDrawModel.Size = new Size((int)Math.Round(_SIZE_W * m_fFormScale), (int)Math.Round(_SIZE_H * m_fFormScale));               
                }
                else
                {
                    m_bExpand = true;
                    m_pnDrawModel.Size = new Size((int)Math.Round(_SIZE_W * m_fFormScale), (int)Math.Round(_SIZE_H * m_fFormScale));


                    //frmTool_Designer.Size = new Size((int)Math.Round((float)_SIZE_W * fScale2) + 10, (int)Math.Round((float)_SIZE_H * fScale2) + 10);

                    //m_nSize_W = (int)Math.Round(_SIZE_W * fScale);// frmTool_Designer.Width;
                    //m_nSize_H = (int)Math.Round(_SIZE_H * fScale);// frmTool_Designer.Height;
                    //m_nDrawWidth = m_pnDrawModel.Width;
                    //m_nDrawHeight = m_pnDrawModel.Height;
                }

            }

            void frmTool_Designer_Load(object sender, EventArgs e)
            {
                m_bProgEnd = false;
                //m_C3d_Designer.Init(
                //throw new NotImplementedException();
            }
            void frmTool_Designer_FormClosed(object sender, FormClosedEventArgs e)
            {
                //Application.Exit();
            }
            #region ProgEnd
            private bool m_bProgEnd = false;
            private bool ProgEnd()
            {
                m_tmrDrawModel.Enabled = false;
                //m_bProgEnd_ForChecking = true; // Pause a Thread, and a Timer
                Ojw.CMessage.Write("Command - Program End");
                Ojw.CMessage.Write("===================================");

                DialogResult dlgRet = MessageBox.Show("Do you want to quit this program?.\r\n\r\nIs it Ok?", "Program End", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                if (dlgRet != DialogResult.OK)
                {
                    m_tmrDrawModel.Enabled = true;
                    Ojw.CMessage.Write("Cancel");
                    //m_bProgEnd_ForChecking = false;
                    return false;
                }
                m_bProgEnd = true;
                
                Ojw.CMessage.Write("Program End");

                //tmrDisp.Enabled = false;
                // screen saver status restore...
                Ojw.CRegistry.ScreenSave_En(Ojw.CRegistry.IsRun_ScreenSaver());
                Ojw.CRegistry.PowerSave_En(Ojw.CRegistry.IsRun_ScreenSaver());

                return true;
            }
            private void frmTool_Designer_FormClosing(object sender, FormClosingEventArgs e)
            {
                bool bRet = ProgEnd();
                if (bRet == false)
                {
                    Ojw.CMessage.Write("Canceled ProgEnd()");
                    e.Cancel = true;
                    return;
                }
                
                DInit3D();
                //throw new NotImplementedException();
            }
            #endregion ProgEnd

            private bool m_btmrDrawModel = false;
            private int m_nGroupA, m_nGroupB, m_nGroupC;
            private int m_nKinematicsNumber;
            //private bool m_bPick;
            private bool m_bLimit;
            private void m_tmrDrawModel_Tick(object sender, EventArgs e)
            {
                if (m_bProgEnd == true) return;

                if (m_btmrDrawModel == true) return;
                m_btmrDrawModel = true;

                //m_C3d_Designer.OjwDraw();
                bool bPick;
                m_C3d_Designer.OjwDraw();//out m_nGroupA, out m_nGroupB, out m_nGroupC, out m_nKinematicsNumber, out bPick, out m_bLimit);
                if (m_C3d_Designer.IsMouseDown() == true)
                {
                    if (m_C3d_Designer.GetEvent_Pick() == true)
                    {
                        bPick = true;
                        m_C3d_Designer.GetEvent_Pick_Data(out m_nGroupA, out m_nGroupB, out m_nGroupC, out m_nKinematicsNumber);
                        ShowData(bPick);
                    }
                    //if (bPick == true)
                    //{
                    //    ShowData(bPick);
                    //}
                    //else m_txtInformation.Text = String.Empty;
                }
                if (m_C3d_Designer.IsMouseUp() == true)
                {
                    for (int i = 0; i < 253; i++)
                    {
                        m_atxtAngle[i].Text = Ojw.CConvert.FloatToStr(m_C3d_Designer.GetData(i));
                    }
                    float fX, fY, fZ;
                    float fPan, fTilt, fSwing;
                    m_C3d_Designer.GetPos_Display(out fX, out fY, out fZ);
                    m_C3d_Designer.GetAngle_Display(out fPan, out fTilt, out fSwing);
                    float fScale = m_C3d_Designer.GetScale();
                    m_frmTools.SetPosition(fX, fY, fZ);
                    m_frmTools.SetAngle(fPan, fTilt, fSwing);
                    m_frmTools.SetScale(fScale);
                    //m_txtInformation.Text = String.Empty;
                }

                m_btmrDrawModel = false;
            }
            private void ShowData(bool bPick)
            {
                m_txtInformation.Text = String.Empty;

                if (bPick == false)
                {
                    Ojw.CMessage.Write2(m_txtInformation, "There is no any parts for controlling");
                    return;
                }

                // 클릭했으니 메세지를 한번 보여주자(show messages when it click)	
                Ojw.CMessage.Write2(m_txtInformation, "Current Joint Group = " + Ojw.CConvert.IntToStr(m_nGroupA) + "\r\n");
                Ojw.CMessage.Write2(m_txtInformation, "Current Motor Number = " + Ojw.CConvert.IntToStr(m_nGroupB) + "\r\n");
                Ojw.CMessage.Write2(m_txtInformation, "Current Serve Group Number = " + Ojw.CConvert.IntToStr(m_nGroupC) + "\r\n");
                Ojw.CMessage.Write2(m_txtInformation, "Connected Function number(but, 255 is None)=" + Ojw.CConvert.IntToStr(m_nKinematicsNumber) + "\r\n");
                Ojw.C3d.COjwDesignerHeader CHeader = m_C3d_Designer.GetHeader();

                // 수식부분이 선택된게 아니라면...(if there is no function number...)
                if ((m_nKinematicsNumber == 255) && (m_nGroupB < CHeader.nMotorCnt))
                {
                    if (m_nGroupA > 0) // Is there a data?
                    {
                        Ojw.CMessage.Write2(m_txtInformation, Ojw.CConvert.IntToStr(m_nGroupB) + "번모터(Name : " + Ojw.CConvert.RemoveChar(CHeader.pSMotorInfo[m_nGroupB].strNickName, (char)0) + ")\r\n");
                        Ojw.CMessage.Write2(m_txtInformation, "MotorID =" + Ojw.CConvert.IntToStr(CHeader.pSMotorInfo[m_nGroupB].nMotorID) + "\r\n");
                        Ojw.CMessage.Write2(m_txtInformation, "Direction =" + ((CHeader.pSMotorInfo[m_nGroupB].nMotorDir == 0) ? "Forward" : "Inverse"));
                        Ojw.CMessage.Write2(m_txtInformation, "Limit(Max : but 0 -> there is no Limit)=" + ((CHeader.pSMotorInfo[m_nGroupB].fLimit_Up != 0.0f) ? Ojw.CConvert.FloatToStr(CHeader.pSMotorInfo[m_nGroupB].fLimit_Up) + " 도" : "리미트 없음") + "\r\n");
                        Ojw.CMessage.Write2(m_txtInformation, "Limit(Min : but 0 -> there is no Limit)=" + ((CHeader.pSMotorInfo[m_nGroupB].fLimit_Down != 0.0f) ? Ojw.CConvert.FloatToStr(CHeader.pSMotorInfo[m_nGroupB].fLimit_Down) + " 도" : "리미트 없음") + "\r\n");
                        Ojw.CMessage.Write2(m_txtInformation, "Center(EVD) : 0도에 해당하는 EVD 값 =" + Ojw.CConvert.IntToStr(CHeader.pSMotorInfo[m_nGroupB].nCenter_Evd) + "\r\n");
                        Ojw.CMessage.Write2(m_txtInformation, "Mech Mov=" + Ojw.CConvert.IntToStr(CHeader.pSMotorInfo[m_nGroupB].nMechMove) + "\r\n");
                        Ojw.CMessage.Write2(m_txtInformation, "Angle of Mech M =" + Ojw.CConvert.FloatToStr(CHeader.pSMotorInfo[m_nGroupB].fMechAngle) + "\r\n");
                        Ojw.CMessage.Write2(m_txtInformation, "Initial Position =" + Ojw.CConvert.FloatToStr(CHeader.pSMotorInfo[m_nGroupB].fInitAngle) + "\r\n");
                        Ojw.CMessage.Write2(m_txtInformation, "NickName =" + Ojw.CConvert.RemoveChar(CHeader.pSMotorInfo[m_nGroupB].strNickName, (char)0) + "\r\n");
                        Ojw.CMessage.Write2(m_txtInformation, "Motor\'s Group Number =" + Ojw.CConvert.IntToStr(CHeader.pSMotorInfo[m_nGroupB].nGroupNumber) + "\r\n");
                        //Ojw.CMessage.Write2(m_txtInformation, );
                        //Ojw.CMessage.Write2(m_txtInformation, );

                        // Motor Check(relationship)
                        int nMotID = m_nGroupB;
                        if (CHeader.pSMotorInfo[nMotID].nAxis_Mirror == -1) Ojw.CMessage.Write2(m_txtInformation, "이 모터는 Mirror 시 값의 변형을 주지 않는다.(No Changing when it has command [flip]");
                        else if (CHeader.pSMotorInfo[nMotID].nAxis_Mirror == -2) Ojw.CMessage.Write2(m_txtInformation, "이 모터는 Mirror 시 Motor 의 Center Point 를 중심으로 뒤집도록 한다.(ex: -30 도 -> 30 도)");
                        else Ojw.CMessage.Write2(m_txtInformation, "Current Motor number = " + Ojw.CConvert.IntToStr(nMotID) + ", Mirroring Motor number = " + Ojw.CConvert.IntToStr(CHeader.pSMotorInfo[nMotID].nAxis_Mirror));
                    }
                    else
                    {
                        Ojw.CMessage.Write2(m_txtInformation, "There is a part without controlling");
                    }
                }
                else if (m_nKinematicsNumber != 255) // 수식 번호가 선택된 경우
                {
                    float fX, fY, fZ;
                    Ojw.CKinematics.CForward.CalcKinematics(CHeader.pDhParamAll[m_nKinematicsNumber], m_C3d_Designer.GetData(), out fX, out fY, out fZ);
                    Ojw.CMessage.Write2(m_txtInformation, "연동되는 수식의 번호(Connected Function Number) = " + Ojw.CConvert.IntToStr(m_nKinematicsNumber) + "\r\n");
                    Ojw.CMessage.Write2(m_txtInformation, "Current Position (x,y,z)=" + Ojw.CConvert.FloatToStr((float)Ojw.CMath.Round(fX, 3)) + "," + Ojw.CConvert.FloatToStr((float)Ojw.CMath.Round(fY, 3)) + "," + Ojw.CConvert.FloatToStr((float)Ojw.CMath.Round(fZ, 3)) + "\r\n");
                }
                else Ojw.CMessage.Write2(m_txtInformation, "There is no any parts for controlling");
            }
            private Label[] m_albAngle = null;
            private TextBox[] m_atxtAngle = new TextBox[256];

            private RichTextBox m_rtxtDraw = new RichTextBox();
            private void MakeBox(TabControl tabAngle, int nCnt)
            {
                if (nCnt > 0)
                {
                    //tabAngle.Size = new Size(m_pnDrawModel.Width, 173);// new Size(640, 173);
                    int nColCount = 6;
                    int nRowCount = 5;
                    int nMax = nColCount * nRowCount;

                    m_albAngle = new Label[nCnt];
                    m_atxtAngle = new TextBox[nCnt];

                    int nGapLeft = 40;// 80;// 12;
                    //int nGapRight = 0;// -42;// 5;
                    int nWidth = 60;// 35;
                    int nHeight = 18;

                    int nLableWidth = 40;
                    int nLableOffset = nLableWidth;// 29;
                    
                    int nWidth_Offset = nLableWidth + 10;// (this.Width - (nGapLeft + nGapRight) - (nWidth * nCnt)) / nCnt;
                    int nHeight_Offset = 5;

                    TabPage tp = null;//new TabPage();


                    Color cBackColor = Color.White;
                    #region For RichTextBox
                    m_rtxtDraw = new RichTextBox();
                    tp = new TabPage();
                    tp.Name = "tabPgText";
                    tp.BackColor = cBackColor;
                    tp.Text = "Draw";
                    tabAngle.Controls.Add(tp);
                    m_rtxtDraw.Left = 10;
                    m_rtxtDraw.Top = 10;
                    m_rtxtDraw.Width = tabAngle.Width - m_rtxtDraw.Left * 2 - 10;
                    m_rtxtDraw.Height = tabAngle.Height - m_rtxtDraw.Top * 2 - 30;
                    //m_rtxtDraw.Text = "test";
                    tp.Controls.Add(m_rtxtDraw);
                    #endregion For RichTextBox

                    //int nPage_Back = -1;
                    int nPage = 0;
                    for (int i = 0; i < nCnt; i++)
                    {
                        nPage = i / nMax;
                        try
                        {
                            String strName = "tabPg" + Ojw.CConvert.IntToStr(nPage);
                            if (tabAngle.Controls.Find("tabPg" + Ojw.CConvert.IntToStr(nPage), true).Length > 0)
                            {
                                if ((tp == null) || (tp.Name != strName))
                                {
                                    tp = (TabPage)(tabAngle.Controls.Find("tabPg" + Ojw.CConvert.IntToStr(nPage), true)[0]);
                                    tp.BackColor = cBackColor;
                                    tp.Text = Ojw.CConvert.IntToStr(nMax * nPage) +
                                        " ~ " +
                                        Ojw.CConvert.IntToStr(((nMax * (nPage + 1) - 1) < nCnt) ? (nMax * (nPage + 1) - 1) : nCnt - 1);
                                }
                            }
                            else
                            {
                                if ((tp == null) || (tp.Name != strName))
                                {
                                    tp = new TabPage();
                                    tp.Name = strName;
                                    tp.BackColor = cBackColor;
                                    tp.Text = Ojw.CConvert.IntToStr(nMax * nPage) +
                                    " ~ " +
                                    Ojw.CConvert.IntToStr(((nMax * (nPage + 1) - 1) < nCnt) ? (nMax * (nPage + 1) - 1) : nCnt - 1);
                                    tabAngle.Controls.Add(tp);
                                }
                            }
                        }
                        catch
                        {
                            //   if (tp == null)
                            //   {
                            tp = new TabPage("tabPg" + Ojw.CConvert.IntToStr(nPage));
                            tp.Text = Ojw.CConvert.IntToStr(nMax * nPage) +
                                    " ~ " +
                                    Ojw.CConvert.IntToStr(((nMax * (nPage + 1) - 1) < nCnt) ? (nMax * (nPage + 1) - 1) : nCnt - 1);
                            tabAngle.Controls.Add(tp);
                            //   }
                        }

                        if (tp == null) break;

                        int nPos = i % nColCount;
                        int nLine = (i % (nColCount * nRowCount)) / nColCount;
                        m_atxtAngle[i] = new TextBox();
                        m_atxtAngle[i].Top = (nHeight_Offset) * (nLine + 1) + (nHeight + nHeight_Offset) * nLine;
                        m_atxtAngle[i].Left = nGapLeft + nWidth * nPos + nWidth_Offset * nPos;
                        m_atxtAngle[i].Width = nWidth;
                        m_atxtAngle[i].Height = nHeight;
                        m_atxtAngle[i].Name = "txtAngle" + Ojw.CConvert.IntToStr(nPos);
                        m_atxtAngle[i].Text = "0.0";
                        m_atxtAngle[i].Visible = true;
                        m_atxtAngle[i].TextAlign = HorizontalAlignment.Center;
                        m_atxtAngle[i].TextChanged += new System.EventHandler(m_atxtAngle_TextChanged);
                        tp.Controls.Add(m_atxtAngle[i]);

                        m_albAngle[i] = new Label();
                        m_albAngle[i].Top = 5 + m_atxtAngle[i].Top;
                        m_albAngle[i].Height = nHeight;
                        m_albAngle[i].Width = nLableWidth;
                        m_albAngle[i].Name = "lbAngle" + Ojw.CConvert.IntToStr(nPos);
                        m_albAngle[i].Text = Ojw.CConvert.IntToStr(i, 3);
                        m_albAngle[i].Left = m_atxtAngle[i].Left - nLableOffset;// m_albPos[i].Width - 1;
                        tp.Controls.Add(m_albAngle[i]);
                        //tp = null;
                    }
                }
            }
            private bool m_bAngle_NoUpdate = false;
            private void BlockUpdate(bool bBlock) { m_bAngle_NoUpdate = bBlock; }
            private void m_atxtAngle_TextChanged(object sender, EventArgs e)
            {
                if (m_bAngle_NoUpdate == true) return;
                int nCnt = m_atxtAngle.Length;
                for (int i = 0; i < nCnt; i++)
                {
                    if (m_atxtAngle[i].Focused == true)
                    {
                        m_C3d_Designer.SetData(i, Ojw.CConvert.StrToFloat(m_atxtAngle[i].Text));
                        //MoveToDhPosition(m_fBallSize, 1.0f, Color.Red, m_afAngle);
                        break;
                    }
                }
            }
        }
        #endregion ModelingTool

        public static void ShowPeople()
        {
            string strPeople = String.Empty;
            strPeople = "made by Jinwook-On (ojw5014@hanmail.net)\r\n" +
                        "[Programming]\r\n" +
                        " - Korea Republic of.: Jinwook-On" + "\r\n" +
                        "[Advise]\r\n" +
                        " - Korea Republic of.: Daesung-Choi, Ceolhea-yoon, Dongjoon-Chang" + "\r\n" +
                        "[Icons]\r\n" +
                        " - Saudi Arabia : Mohssin" + "\r\n" +
                        "[Etc]\r\n" +
                        " - Korea Republic of. : Robert Lee(hjlee@robolink.co.kr), Hamsuk heo(hidekeke5@robolink.co.kr)" + "\r\n" +
                        "===========================================";
            Ojw.CMessage.Write(strPeople);
            MessageBox.Show(strPeople);
        }
        public static string GetHistory() { return SVersion_T.strHistory; }
        public const string strVersion = "01.01.51";
        public static string GetVersion() { return SVersion_T.strVersion; }

        #region Define(private)
        private class CDef
        {
            public static int _CHKSUM_NONE = 0;
            public static int _CHKSUM_AND = 1;
            public static int _CHKSUM_OR = 2;
            public static int _CHKSUM_XOR = 3;
            public static int _CHKSUM_SUM = 4;
        }
        #endregion Define    
    }
}
