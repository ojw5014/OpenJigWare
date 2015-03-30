using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace OpenJigWare
{
    partial class Ojw
    {
        public class CTimer
        {
            private static bool m_bProgEnd = false;
            private static bool m_bStop = false;
            public CTimer()
            {
            }
            ~CTimer()
            {
                m_bProgEnd = true;
                m_bStop = true;
            }
            public static bool IsStop() { return m_bStop; }
            //public bool IsStop() { return m_bStop; }
            public static void Stop() { m_bStop = true; }
            //public void Stop() { m_bStop = true; }
            public static void Reset() { m_bStop = false; }

            // Check Timer Alive
            public static bool IsTimer(int nHandle) { if ((nHandle >= 0) && (nHandle < m_abTimer.Length)) return m_abTimer[nHandle]; return false; }
            public bool IsTimer() { return m_bTimer; }

            private const int _SIZE_STATIC_TIMER = 10;
            private static bool[] m_abTimer = new bool[_SIZE_STATIC_TIMER];
            private static long[] m_alTimer = new long[_SIZE_STATIC_TIMER];
            private static long[] m_alTimer_Tick = new long[_SIZE_STATIC_TIMER];
            private bool m_bTimer = false;
            private long m_lTimer = 0;
            private long m_lTimer_Tick = 0;

            #region static 
            // if you use this _SIZE_STATIC_TIMER will change.
            //(KOR: 안하면 기존 _SIZE_STATIC_TIMER 개의 메모리로 고정된다.)
            public static void Init(int nCnt) 
            {
                Array.Resize<bool>(ref m_abTimer, nCnt);
                Array.Resize<long>(ref m_alTimer, nCnt);
                Array.Resize<long>(ref m_alTimer_Tick, nCnt);
                Array.Clear(m_abTimer, 0, nCnt);
                Array.Clear(m_alTimer, 0, nCnt);
                Array.Clear(m_alTimer_Tick, 0, nCnt);
            }
            public static void DInit()
            {
                int nCnt = _SIZE_STATIC_TIMER;
                Array.Resize<bool>(ref m_abTimer, nCnt);
                Array.Resize<long>(ref m_alTimer, nCnt);
                Array.Resize<long>(ref m_alTimer_Tick, nCnt);                
            }
            // wait millisecond(1/1000 s) (Kor: 시간 t( 1/1000 s ) 만큼의 딜레이 발생)
            public static void Wait(long t)
            {
                long temp_t = t, temp;
                long temp_1 = 0;

                DateTime tmrTemp = DateTime.Now;
                long temp_2 = (long)tmrTemp.Ticks * 100 / 1000000;

                do
                {
                    if ((m_bProgEnd == true) || (m_bProgEnd == true) || (m_bStop == true) || (m_bKillWait == true)) break;
                    // Delay
                    tmrTemp = DateTime.Now;
                    temp = (long)tmrTemp.Ticks * 100 / 1000000;

                    Application.DoEvents();

                    temp_1 = temp - temp_2;
                } while (temp_1 <= temp_t);
                m_bKillWait = false; // Only 1'st use(Kor: KillWait 은 한번만 수행한다.)
            }
            private static bool m_bKillWait = false;
            public static bool IsKillWait() { return m_bKillWait; }
            public static void KillWait() { m_bKillWait = true; }

            // Handle = 0~99
            // Set Timer
            public static void Set(int nHandle) { Set_Tick(nHandle, DateTime.Now.Ticks); }
            private static void Set_Tick(int nHandle, long lTick) { long temp = (long)lTick * 100 / 1000000; m_abTimer[nHandle] = true; m_alTimer[nHandle] = temp; m_alTimer_Tick[nHandle] = lTick; }
            // Kill Timer ( if you have set you should kill it for sure ) (Kor: 생성을 했으면 반드시 Kill를 하도록 한다. )
            public static void Kill(int nHandle) { m_abTimer[nHandle] = false; m_alTimer[nHandle] = 0; m_alTimer_Tick[nHandle] = 0; }

            // Handle = 0~99
            // return value from set to current time(Kor: Timer 생성 후 현재까지의 시간 값을 return)
            public static long Get(int nHandle)
            {
                if (m_abTimer[nHandle] == true)
                {
                    DateTime tmrTemp = DateTime.Now;
                    long temp = (long)tmrTemp.Ticks * 100 / 1000000;

                    long temp_Gap = temp - m_alTimer[nHandle];
                    return temp_Gap;
                }
                else return 0;
            }
            public static long GetCurrentTime() { return (long)DateTime.Now.Ticks * 100 / 1000000; }

            //public static long GetTick() { return DateTime.Now.Ticks; }
            public static long GetTick(int nHandle)
            {
                if (m_abTimer[nHandle] == true)
                {
                    long temp_Gap = DateTime.Now.Ticks - m_alTimer_Tick[nHandle];
                    return temp_Gap;
                }
                else return (long)(0x7fffffffffffffff);
            }

            // Handle = 0~99
            // After Timer generates does not exceed a specified time(t), or if the Timer is not generating.. It will return value a FALSE(Kor: Timer 생성 후 지정한 시간(t)를 넘지 않거나 Timer가 생성되지 않으면 FALSE(0)를 return)
            // Returns true if 10 seconds exceeded(Kor: Timer 생성 후 지정한 시간(t)를 넘었다면 TRUE(1) 를 return)
            public static bool Check(int nHandle, long t)
            {
                if (m_abTimer[nHandle])
                {
                    DateTime tmrTemp = DateTime.Now;
                    long temp = (long)tmrTemp.Ticks * 100 / 1000000;

                    long temp_Gap = temp - m_alTimer[nHandle];
                    if (temp_Gap < t) return false;
                    else return true;
                }
                else return false;
            }
            #endregion static
            #region Var
            // Handle
            // Set Timer
            public void Set() { Set_Tick(DateTime.Now.Ticks); }
            private void Set_Tick(long lTick) { long temp = (long)lTick * 100 / 1000000; m_bTimer = true; m_lTimer = temp; m_lTimer_Tick = lTick; }
            // Kill Timer ( if you have set you should kill it for sure ) (Kor: 생성을 했으면 반드시 Kill를 하도록 한다. )
            public void Kill() { m_bTimer = false; m_lTimer = 0; m_lTimer_Tick = 0; }

            // Handle = 0~(_SIZE_STATIC_TIMER - 1)
            // Returns to the current time(Kor: Timer 생성 후 현재까지의 시간 값을 return)
            public long Get()
            {
                if (m_bTimer == true)
                {
                    DateTime tmrTemp = DateTime.Now;
                    long temp = (long)tmrTemp.Ticks * 100 / 1000000;

                    long temp_Gap = temp - m_lTimer;
                    return temp_Gap;
                }
                else return (long)(0x7fffffffffffffff);
            }
            public long GetTick()
            {
                if (m_bTimer == true)
                {
                    long temp_Gap = DateTime.Now.Ticks - m_lTimer_Tick;
                    return temp_Gap;
                }
                else return (long)(0x7fffffffffffffff);
            }

            // Handle = 0~(_SIZE_STATIC_TIMER - 1)
            // After Timer generates does not exceed a specified time(t), or if the Timer is not generating.. It will return value a FALSE(Kor: Timer 생성 후 지정한 시간(t)를 넘지 않거나 Timer가 생성되지 않으면 FALSE(0)를 return)
            // Returns true if 10 seconds exceeded(Kor: Timer 생성 후 지정한 시간(t)를 넘었다면 TRUE(1) 를 return)
            public bool Check(long t)
            {
                if (m_bTimer)
                {
                    DateTime tmrTemp = DateTime.Now;
                    long temp = (long)tmrTemp.Ticks * 100 / 1000000;

                    long temp_Gap = temp - m_lTimer;
                    if (temp_Gap < t) return false;
                    else return true;
                }
                else return false;
            }
            #endregion Var

            public static int GetYear() { return DateTime.Now.Year; }
            public static int GetMonth() { return DateTime.Now.Month; }
            public static int GetDay() { return DateTime.Now.Day; }
            public static int GetHour() { return DateTime.Now.Hour; }
            public static int GetMinute() { return DateTime.Now.Minute; }
            public static int GetSecond() { return DateTime.Now.Second; }
            // 0 - 일(Sun), 1 - 월(Mon), 2 - 화(Tue), 3 - 수(Wed), 4 - 목(Thu), 5 - 금(Fri), 6 - 토(Sat), => -1 - 에러(Error)
            public static int GetWeek() { try { return int.Parse(DateTime.Now.DayOfWeek.ToString("d")); } catch { return -1; } }
        }
    }
}
