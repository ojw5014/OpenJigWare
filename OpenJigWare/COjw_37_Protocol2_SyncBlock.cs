using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace OpenJigWare
{
    partial class Ojw
    {
        public partial class CProtocol2
        {
            // 일괄 SyncRead — 한 요청으로 여러 모터를 읽어 이번 응답만 한 번에 돌려준다 (2026-11-06)
            //   기존 SyncRead_With_Address 와 달리: ① m_aCReceive(10개 제한)를 거치지 않아 15개를 한 요청으로 검증할 수 있다
            //   ② 응답을 다 받으면 바로 돌아오고, 기다리는 방법을 고를 수 있다 (SyncRead_SetPollSleep) ③ 구간별 시각을 결과에 담는다.
            //   토크·레지스터를 쓰지 않는다. 실패해도 재시도·토크 변경·이전 값 반환을 하지 않는다.
            //   기다리는 동안 화면 메시지를 처리하지 않는다(Application.DoEvents 없음) — 화면 스레드에서 부르면 그동안 화면이 멈춘다.
            public const int SYNCREAD_OK = 0;           // 모든 ID 의 응답이 검증을 통과
            public const int SYNCREAD_TIMEOUT = 1;      // 시간 안에 못 받은 ID 가 있다 (abReceived 확인)
            public const int SYNCREAD_PROTOCOL = 2;     // 모두 받았으나 CRC·길이·중복·예상 밖 ID 등 이상 패킷이 섞였다
            public const int SYNCREAD_MOTOR_ERROR = 3;  // 모두 받았으나 상태 바이트 오류가 있다 (abyError 확인)
            public const int SYNCREAD_BAD_ARGS = 4;     // 인자 오류 (요청을 보내지 않음)
            public const int SYNCREAD_NOT_OPEN = 5;     // 시리얼이 열려 있지 않음 (요청을 보내지 않음)
            public const int SYNCREAD_IO_ERROR = 6;     // 시리얼 예외

            public class CSyncReadResult_t
            {
                public bool bOk = false;                // nStatus == SYNCREAD_OK. false 면 abyData 를 측정값으로 쓰지 말 것
                public int nStatus = SYNCREAD_BAD_ARGS;
                public int nAddress = 0;
                public int nSize = 0;
                public int[] anIDs = new int[0];        // 요청 순서
                public byte[] abyData = new byte[0];    // anIDs 순서로 nSize 바이트씩. 이번 호출에서 받은 것만 (못 받은 칸은 0)
                public byte[] abyError = new byte[0];   // ID 별 상태 바이트
                public bool[] abReceived = new bool[0]; // ID 별 이번 호출에서 CRC·길이가 맞는 응답을 받았는가
                public int nCrcErrors = 0, nBadLength = 0, nBadCmd = 0, nDuplicates = 0, nUnexpectedIDs = 0, nGarbageBytes = 0;
                public int nStaleBytes = 0;             // 요청 전에 버린 이전 바이트 (오래된 응답이 이번 값으로 섞이지 않게)
                public int nPolls = 0, nReads = 0, nBytes = 0;
                // 호출 시작부터의 시각 (ms). 음수 = 그 단계에 이르지 못함
                public double dSendStart_ms = -1, dSendEnd_ms = -1, dFirstByte_ms = -1, dLastPacket_ms = -1, dReturn_ms = -1;
                public double dParse_ms = 0;            // 패킷 해석에 쓴 시간 합
                /// <summary>ID 칸 nIndex 의 nOffset 위치 부호 있는 32비트 (리틀 엔디안)</summary>
                public int GetInt32(int nIndex, int nOffset) { return BitConverter.ToInt32(abyData, nIndex * nSize + nOffset); }
            }

            /// <summary>응답 바이트열 검증기 — 시리얼과 무관 (단위 시험용 공개). 조각으로 들어와도 된다</summary>
            public class CSyncReadParser
            {
                private readonly bool m_bFast;   // Fast Sync Read: 한 상태 패킷(ID 254) 안에 [오류, ID, 자료, CRC] 반복
                private readonly CProtocol2 m_Owner;
                private readonly CSyncReadResult_t m_R;
                private readonly Dictionary<int, int> m_mapIndex = new Dictionary<int, int>();
                private byte[] m_aby = new byte[2048];
                private int m_nLen = 0;
                private int m_nMissing;
                private readonly Action<int, byte[], int> m_FValid;

                public CSyncReadParser(CProtocol2 Owner, CSyncReadResult_t R, Action<int, byte[], int> FValid = null, bool bFast = false)
                {
                    m_Owner = Owner; m_R = R; m_FValid = FValid; m_bFast = bFast; m_nMissing = R.anIDs.Length;
                    for (int i = 0; i < R.anIDs.Length; i++) m_mapIndex[R.anIDs[i]] = i;
                }
                public bool IsComplete { get { return m_nMissing == 0; } }
                public void Feed(byte[] aby, int nCount)
                {
                    if (m_nLen + nCount > m_aby.Length) Array.Resize(ref m_aby, Math.Max(m_aby.Length * 2, m_nLen + nCount));
                    Buffer.BlockCopy(aby, 0, m_aby, m_nLen, nCount);
                    m_nLen += nCount;
                    int nPos = 0;
                    while (true)
                    {
                        int nHead = -1;
                        for (int i = nPos; i + 3 < m_nLen; i++)
                            if ((m_aby[i] == 0xFF) && (m_aby[i + 1] == 0xFF) && (m_aby[i + 2] == 0xFD) && (m_aby[i + 3] == 0x00)) { nHead = i; break; }
                        if (nHead < 0)
                        {
                            // 머리 일부가 끝에 걸쳤을 수 있으니 3바이트는 남긴다
                            int nKeep = Math.Max(nPos, m_nLen - 3);
                            m_R.nGarbageBytes += nKeep - nPos;
                            nPos = nKeep;
                            break;
                        }
                        m_R.nGarbageBytes += nHead - nPos;
                        nPos = nHead;
                        if (nPos + 7 > m_nLen) break;                       // 길이 칸을 아직 못 받음
                        int nLength = m_aby[nPos + 5] | (m_aby[nPos + 6] << 8);   // 명령 + 오류 + 자료 + CRC
                        if ((nLength < 4) || (nLength > (m_bFast ? (m_R.nSize + 4) * m_R.anIDs.Length * 2 + 16 : m_R.nSize * 2 + 16)))
                        {
                            m_R.nBadLength++; nPos += 4; continue;          // 깨진 길이를 믿고 기다리지 않는다 — 다음 머리에서 다시 맞춘다
                        }
                        if (nPos + 7 + nLength > m_nLen) break;             // 패킷 끝을 아직 못 받음
                        Packet(nPos, nLength);
                        nPos += 7 + nLength;
                    }
                    if (nPos > 0)
                    {
                        Buffer.BlockCopy(m_aby, nPos, m_aby, 0, m_nLen - nPos);
                        m_nLen -= nPos;
                    }
                }
                // Fast Sync Read 상태 패킷: 전체 CRC 는 Packet 에서 통과. 디스터핑한 뒤 [오류 1, ID 1, 자료 nSize, CRC 2] 를 모터 수만큼 나눈다
                private void FastPacket(int nPos, int nEnd)
                {
                    if ((m_aby[nPos + 4] != 0xFE) || (m_aby[nPos + 7] != 0x55)) { m_R.nBadCmd++; return; }
                    int nUnit = m_R.nSize + 4;
                    byte[] abyAll = new byte[nEnd - nPos];
                    int nOut = 0, nFF = 0;
                    for (int i = nPos + 8; i < nEnd; i++)
                    {
                        byte by = m_aby[i];
                        if (nFF == 3)
                        {
                            nFF = 0;
                            if (by == 0xFD) continue;
                        }
                        if (by == 0xFF) nFF = Math.Min(nFF + 1, 2);
                        else if ((nFF == 2) && (by == 0xFD)) nFF = 3;
                        else nFF = 0;
                        abyAll[nOut++] = by;
                    }
                    // 전체 길이 끝의 마지막 모터 CRC 2바이트는 패킷 CRC 와 같은 자리다 (nUnit 에서 2 뺀 길이까지 허용)
                    if ((nOut != nUnit * m_R.anIDs.Length) && (nOut != nUnit * m_R.anIDs.Length - 2)) { m_R.nBadLength++; return; }
                    for (int k = 0; k < m_R.anIDs.Length; k++)
                    {
                        int nOff = k * nUnit, nID = abyAll[nOff + 1], nIndex;
                        if (!m_mapIndex.TryGetValue(nID, out nIndex)) { m_R.nUnexpectedIDs++; continue; }
                        if (m_R.abReceived[nIndex]) { m_R.nDuplicates++; continue; }
                        byte[] abyPayload = new byte[m_R.nSize];
                        Buffer.BlockCopy(abyAll, nOff + 2, abyPayload, 0, m_R.nSize);
                        Buffer.BlockCopy(abyPayload, 0, m_R.abyData, nIndex * m_R.nSize, m_R.nSize);
                        m_R.abyError[nIndex] = abyAll[nOff];
                        m_R.abReceived[nIndex] = true;
                        m_nMissing--;
                        if ((abyAll[nOff] == 0) && (m_FValid != null)) m_FValid(nID, abyPayload, m_R.nSize);
                    }
                }
                private void Packet(int nPos, int nLength)
                {
                    int nCrc = 0;
                    int nEnd = nPos + 7 + nLength - 2;
                    for (int i = nPos; i < nEnd; i++) nCrc = m_Owner.updateCRC_Byte(nCrc, m_aby[i]);
                    int nRecv = m_aby[nEnd] | (m_aby[nEnd + 1] << 8);
                    if ((nCrc & 0xFFFF) != nRecv) { m_R.nCrcErrors++; return; }
                    if (m_bFast) { FastPacket(nPos, nEnd); return; }
                    int nID = m_aby[nPos + 4], nCmd = m_aby[nPos + 7], nErr = m_aby[nPos + 8], nIndex;
                    if (nCmd != 0x55) { m_R.nBadCmd++; return; }
                    if (!m_mapIndex.TryGetValue(nID, out nIndex)) { m_R.nUnexpectedIDs++; return; }
                    if (m_R.abReceived[nIndex]) { m_R.nDuplicates++; return; }
                    // 바이트 디스터핑 (FF FF FD FD → FF FF FD)
                    byte[] abyPayload = new byte[m_R.nSize];
                    int nOut = 0, nFF = 0;
                    for (int i = nPos + 9; i < nEnd; i++)
                    {
                        byte by = m_aby[i];
                        if (nFF == 3)
                        {
                            nFF = 0;
                            if (by == 0xFD) continue;
                        }
                        if (by == 0xFF) nFF = Math.Min(nFF + 1, 2);
                        else if ((nFF == 2) && (by == 0xFD)) nFF = 3;
                        else nFF = 0;
                        if (nOut >= m_R.nSize) { nOut++; break; }
                        abyPayload[nOut++] = by;
                    }
                    if (nOut != m_R.nSize) { m_R.nBadLength++; return; }
                    Buffer.BlockCopy(abyPayload, 0, m_R.abyData, nIndex * m_R.nSize, m_R.nSize);
                    m_R.abyError[nIndex] = (byte)nErr;
                    m_R.abReceived[nIndex] = true;
                    m_nMissing--;
                    if ((nErr == 0) && (m_FValid != null)) m_FValid(nID, abyPayload, m_R.nSize);
                }
            }

            private bool m_bSyncRead_Dirty = false;   // 직전 호출이 실패 — 늦은 응답이 올 수 있어 다음 요청 전에 조용해질 때까지 비운다
            private int m_nSyncRead_PollSleep_ms = 0;
            private const int _SYNCREAD_QUIET_MS = 5, _SYNCREAD_QUIET_LIMIT_MS = 100;
            /// <summary>기다리는 동안 쉬는 시간(ms). 0(기본) = Thread.Yield 로 바로 다시 본다 (지연 최소, CPU 코어 하나를 쓴다), 1 이상 = 그만큼 쉰다.
            /// 윈도우의 Thread.Sleep(1) 은 약 15 ms 걸리므로(타이머 해상도), 고해상도 대기 타이머를 쓸 수 있으면 그것으로 쉰다</summary>
            public void SyncRead_SetPollSleep(int nMillisec) { m_nSyncRead_PollSleep_ms = Math.Max(0, nMillisec); }

            #region 정밀 대기 (윈도우 고해상도 대기 타이머)
            // 윈도우의 Thread.Sleep(1) 은 기본 타이머 해상도(15.6 ms) 때문에 약 15 ms 걸린다(실측 15.1 ms).
            // 윈도우 10(1803) 이상의 고해상도 대기 타이머는 프로세스 전체 타이머 해상도를 바꾸지 않고 약 1 ms 단위로 깬다.
            // 만들 수 없거나(옛 윈도우) 윈도우가 아니면 Thread.Sleep 으로 쉰다.
            [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            private static extern SafeWaitHandle CreateWaitableTimerExW(IntPtr lpTimerAttributes, string lpTimerName, uint dwFlags, uint dwDesiredAccess);
            [DllImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool SetWaitableTimer(SafeWaitHandle hTimer, ref long lpDueTime, int lPeriod, IntPtr pfnCompletionRoutine, IntPtr lpArgToCompletionRoutine, [MarshalAs(UnmanagedType.Bool)] bool fResume);
            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern uint WaitForSingleObject(SafeWaitHandle hHandle, uint dwMilliseconds);
            private const uint _CREATE_WAITABLE_TIMER_HIGH_RESOLUTION = 0x00000002, _TIMER_ALL_ACCESS = 0x001F0003;
            private SafeWaitHandle m_hSyncRead_Timer = null;
            private int m_nSyncRead_TimerState = 0;   // 0 = 아직 시도 안 함, 1 = 고해상도 타이머 사용, -1 = 사용 불가 (Thread.Sleep)
            private void SyncRead_Sleep(int nMillisec)
            {
                if (m_nSyncRead_TimerState == 0)
                {
                    m_nSyncRead_TimerState = -1;
                    try
                    {
                        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                        {
                            SafeWaitHandle h = CreateWaitableTimerExW(IntPtr.Zero, null, _CREATE_WAITABLE_TIMER_HIGH_RESOLUTION, _TIMER_ALL_ACCESS);
                            if ((h != null) && !h.IsInvalid) { m_hSyncRead_Timer = h; m_nSyncRead_TimerState = 1; }
                        }
                    }
                    catch { }
                }
                if (m_nSyncRead_TimerState == 1)
                {
                    try
                    {
                        long lDue = -10000L * nMillisec;   // 100 ns 단위, 음수 = 지금부터
                        if (SetWaitableTimer(m_hSyncRead_Timer, ref lDue, 0, IntPtr.Zero, IntPtr.Zero, false))
                        {
                            WaitForSingleObject(m_hSyncRead_Timer, (uint)(nMillisec + 50));
                            return;
                        }
                    }
                    catch { }
                    m_nSyncRead_TimerState = -1;
                }
                Thread.Sleep(nMillisec);
            }
            #endregion 정밀 대기 (윈도우 고해상도 대기 타이머)

            /// <summary>한 번의 Sync Read 요청으로 anIDs 를 읽어 이번 응답만 돌려준다. 다 받으면 바로 돌아온다.
            /// bOk 가 false 면 abyData 를 측정값으로 쓰지 말 것. 재시도·쓰기를 하지 않는다. 시리얼 연결 전용.
            /// nTimeout_ms = 요청을 다 보낸 때부터 기다리는 최대 시간 (응답이 가장 늦게 도착할 수 있는 시각보다 길게 — USB 변환기의 지연 포함)</summary>
            public CSyncReadResult_t SyncReadBlock(int nAddress, int nSize, int nTimeout_ms, params int[] anIDs)
            {
                return SyncRead_Core(false, nAddress, nSize, nTimeout_ms, anIDs);
            }
            /// <summary>SyncReadBlock 과 같되 Fast Sync Read(명령 0x8A) — 모터들이 한 상태 패킷으로 이어 응답한다.
            /// 지원하는 펌웨어에서만 응답한다 (없으면 SYNCREAD_TIMEOUT). 읽기 명령이라 레지스터를 바꾸지 않는다</summary>
            public CSyncReadResult_t SyncReadFastBlock(int nAddress, int nSize, int nTimeout_ms, params int[] anIDs)
            {
                return SyncRead_Core(true, nAddress, nSize, nTimeout_ms, anIDs);
            }
            // 성공(bOk)한 결과를 캐시에 반영: m_abyMap 에 블록 복사 + 블록 안에 통째로 든 현재 위치로 각도 캐시 갱신 (GetAngle·Move_NoWait 기준).
            //   실패한 읽기는 어떤 ID 도 갱신하지 않는다. 요청 ID 순서(anIDs)대로 abyData 칸을 읽는다. 추가 할당 없음
            //   ID 한 칸(_SIZE_MAP)을 넘는 주소는 맵에 넣지 않는다 (다음 ID 의 칸을 덮지 않게). 결과(abyData)에는 그대로 있다
            private void SyncRead_ApplyCache(CSyncReadResult_t R)
            {
                int nAddress = R.nAddress, nSize = R.nSize;
                bool bMap = (nAddress >= 0) && (nAddress + nSize <= _SIZE_MAP);
                for (int i = 0; i < R.anIDs.Length; i++)
                {
                    int nID = R.anIDs[i], nOff = i * nSize;
                    int nMap = nID * _SIZE_MAP + nAddress;
                    if (bMap && (nMap + nSize <= m_abyMap.Length)) Buffer.BlockCopy(R.abyData, nOff, m_abyMap, nMap, nSize);
                    ApplyReadBlock_ToCache(nID, R.abyData, nOff, nAddress, nSize);
                }
            }
            private CSyncReadResult_t SyncRead_Core(bool bFast, int nAddress, int nSize, int nTimeout_ms, int[] anIDs)
            {
                CSyncReadResult_t R = new CSyncReadResult_t();
                R.nAddress = nAddress; R.nSize = nSize;
                if ((anIDs == null) || (anIDs.Length == 0) || (nSize <= 0) || (nAddress < 0) || (nTimeout_ms <= 0)) return R;
                var seen = new HashSet<int>();
                foreach (int nID in anIDs) if ((nID < 0) || (nID > 252) || !seen.Add(nID)) return R;
                R.anIDs = (int[])anIDs.Clone();
                R.abyData = new byte[anIDs.Length * nSize];
                R.abyError = new byte[anIDs.Length];
                R.abReceived = new bool[anIDs.Length];
                if (!m_CSerial.IsConnect()) { R.nStatus = SYNCREAD_NOT_OPEN; return R; }

                long lStart = Stopwatch.GetTimestamp();
                Func<double> Now = () => (Stopwatch.GetTimestamp() - lStart) * 1000.0 / Stopwatch.Frequency;
                byte[] abyBuf = new byte[1024];
                try
                {
                    // 요청 전 이전 바이트를 버린다. 직전이 실패였으면 늦은 응답이 멎을 때까지
                    double dQuiet = Now();
                    while (true)
                    {
                        int nOld = m_CSerial.ReadAvailable(abyBuf, 0, abyBuf.Length);
                        if (nOld > 0) { R.nStaleBytes += nOld; dQuiet = Now(); }
                        if (Now() >= _SYNCREAD_QUIET_LIMIT_MS) break;          // 바이트가 끊이지 않고 들어와도 여기서 멈춘다
                        if (!m_bSyncRead_Dirty) { if (nOld == 0) break; continue; }
                        if (Now() - dQuiet >= _SYNCREAD_QUIET_MS) break;
                        SyncRead_Sleep(1);
                    }
                    m_nReceive_Header = 0; m_nReceive_Index = 0;   // 옛 파서도 깨끗한 상태로

                    // 캐시(m_abyMap·각도)는 전체 응답 검증이 끝난 뒤 bOk 일 때만 한꺼번에 반영한다 (아래 SyncRead_ApplyCache) — 일부 ID 만 새 값이 되지 않도록
                    CSyncReadParser Parser = new CSyncReadParser(this, R, null, bFast);

                    R.dSendStart_ms = Now();
                    Request_Clear();
                    for (int i = 0; i < anIDs.Length; i++) Request_Push(anIDs[i]);
                    if (bFast)
                    {
                        byte[] abyParam = new byte[4 + anIDs.Length];
                        abyParam[0] = (byte)(nAddress & 0xff); abyParam[1] = (byte)((nAddress >> 8) & 0xff);
                        abyParam[2] = (byte)(nSize & 0xff); abyParam[3] = (byte)((nSize >> 8) & 0xff);
                        for (int i = 0; i < anIDs.Length; i++) abyParam[4 + i] = (byte)anIDs[i];
                        Request(254, 0x8A, abyParam);
                        Request_Clear();
                    }
                    else Request_Flush(nAddress, nSize);
                    m_nRequestMotors = 0;
                    R.dSendEnd_ms = Now();

                    double dLimit = R.dSendEnd_ms + nTimeout_ms;
                    while (!Parser.IsComplete)
                    {
                        R.nPolls++;
                        int nRead = m_CSerial.ReadAvailable(abyBuf, 0, abyBuf.Length);
                        if (nRead > 0)
                        {
                            double dGot = Now();
                            if (R.dFirstByte_ms < 0) R.dFirstByte_ms = dGot;
                            R.nReads++; R.nBytes += nRead;
                            Parser.Feed(abyBuf, nRead);
                            R.dParse_ms += Now() - dGot;
                            if (Parser.IsComplete) { R.dLastPacket_ms = dGot; break; }
                            if (Now() >= dLimit) break;                         // 바이트가 끊이지 않고 들어와도 제한 시간은 지킨다
                            continue;
                        }
                        if (Now() >= dLimit) break;
                        if (m_nSyncRead_PollSleep_ms > 0) SyncRead_Sleep(m_nSyncRead_PollSleep_ms); else Thread.Yield();
                    }

                    bool bAll = Parser.IsComplete, bMotorErr = false;
                    for (int i = 0; i < R.abyError.Length; i++) if (R.abyError[i] != 0) bMotorErr = true;
                    if (!bAll) { R.nStatus = SYNCREAD_TIMEOUT; m_nTimeout_Count++; }
                    else if ((R.nCrcErrors + R.nBadLength + R.nBadCmd + R.nDuplicates + R.nUnexpectedIDs + R.nGarbageBytes) > 0) R.nStatus = SYNCREAD_PROTOCOL;
                    else if (bMotorErr) R.nStatus = SYNCREAD_MOTOR_ERROR;
                    else R.nStatus = SYNCREAD_OK;
                    m_nCRC_Error_Count += R.nCrcErrors;
                }
                catch (Exception ex)
                {
                    R.nStatus = SYNCREAD_IO_ERROR;
                    Ojw.LogErr("SyncReadBlock - " + ex.Message);
                }
                R.bOk = (R.nStatus == SYNCREAD_OK);
                if (R.bOk)
                {
                    try { SyncRead_ApplyCache(R); }
                    catch (Exception ex) { R.nStatus = SYNCREAD_IO_ERROR; R.bOk = false; Ojw.LogErr("SyncReadBlock cache - " + ex.Message); }
                }
                m_bSyncRead_Dirty = !R.bOk;
                R.dReturn_ms = Now();
                return R;
            }
        }
    }
}
