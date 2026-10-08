using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace OpenJigWare
{
    public partial class Ojw
    {
        public sealed partial class COjwWalking_g
        {
            /// <summary>
            /// One repeat cycle of RESOLVED motor positions, after IK/additive commands/mirroring.
            /// No assumed robot, motor units, root trajectory, or physical contacts.
            /// Each row is a target reached from the previous row in moveSeconds, then held.
            /// Time zero starts the transition from the last target to the first target.
            /// </summary>
            public sealed class ReferenceCycle
            {
                private readonly int[] motorIds;
                private readonly string source;
                private readonly List<float[]> positions = new List<float[]>();
                private readonly List<double> move = new List<double>(), hold = new List<double>();
                private double[] boundaries;
                public int MotorCount { get { return motorIds.Length; } }
                public int FrameCount { get { return positions.Count; } }
                public double Duration { get { Seal(); return boundaries[FrameCount]; } }

                public ReferenceCycle(int[] logicalMotorIds, string sourceDescription)
                {
                    if (logicalMotorIds == null || logicalMotorIds.Length == 0) throw new ArgumentException("Motor IDs are required.");
                    HashSet<int> seen = new HashSet<int>();
                    foreach (int id in logicalMotorIds)
                        if (id < 0 || !seen.Add(id)) throw new ArgumentException("Motor IDs must be unique and nonnegative.");
                    motorIds = (int[])logicalMotorIds.Clone(); source = sourceDescription ?? "";
                }

                public void AddFrame(float[] resolvedPositions, double moveSeconds, double holdSeconds)
                {
                    if (boundaries != null) throw new InvalidOperationException("The reference cycle is sealed.");
                    if (resolvedPositions == null || resolvedPositions.Length != MotorCount) throw new ArgumentException("Motor count mismatch.");
                    if (!Finite(moveSeconds) || moveSeconds <= 0 || !Finite(holdSeconds) || holdSeconds < 0)
                        throw new ArgumentException("Move time must be positive; hold time must be nonnegative (seconds).");
                    foreach (float v in resolvedPositions) if (!Finite(v)) throw new ArgumentException("Nonfinite motor position.");
                    positions.Add((float[])resolvedPositions.Clone()); move.Add(moveSeconds); hold.Add(holdSeconds);
                }

                public void Seal()
                {
                    if (boundaries != null) return;
                    if (FrameCount < 2) throw new InvalidOperationException("At least two resolved frames are required.");
                    double[] times = new double[FrameCount + 1];
                    for (int i = 0; i < FrameCount; i++)
                    {
                        times[i + 1] = times[i] + move[i] + hold[i];
                        if (!Finite(times[i + 1]) || times[i + 1] <= times[i]) throw new ArgumentException("Invalid cycle duration.");
                    }
                    boundaries = times;
                }

                /// <summary>Allocation-free after Seal. Velocities are in native position units/second.</summary>
                public void Sample(double timeSeconds, float[] output, float[] velocity)
                {
                    Seal();
                    if (!Finite(timeSeconds)) throw new ArgumentException("Time must be finite.");
                    if (output == null || velocity == null || output.Length != MotorCount || velocity.Length != MotorCount
                        || ReferenceEquals(output, velocity)) throw new ArgumentException("Provide two separate motor-sized buffers.");
                    double t = timeSeconds % boundaries[FrameCount];
                    if (t < 0) t += boundaries[FrameCount];
                    int lo = 0, hi = FrameCount;
                    while (lo + 1 < hi) { int mid = (lo + hi) / 2; if (boundaries[mid] <= t) lo = mid; else hi = mid; }
                    double elapsed = t - boundaries[lo], blend = Math.Min(1, elapsed / move[lo]);
                    float[] a = positions[(lo + FrameCount - 1) % FrameCount], b = positions[lo];
                    for (int j = 0; j < MotorCount; j++)
                    {
                        output[j] = (float)(a[j] + ((double)b[j] - a[j]) * blend);
                        velocity[j] = elapsed < move[lo] ? (float)(((double)b[j] - a[j]) / move[lo]) : 0;
                        if (!Finite(output[j]) || !Finite(velocity[j])) throw new ArithmeticException("Reference sample exceeds float range.");
                    }
                }

                public void Save(string path)
                {
                    Seal();
                    using (StreamWriter writer = new StreamWriter(path, false, new UTF8Encoding(false))) WriteJson(writer);
                }
                public void WriteJson(TextWriter writer)
                {
                    Seal(); if (writer == null) throw new ArgumentNullException("writer");
                    writer.Write("{\n  \"schema\":\"ojw.walking.joint_cycle.v1\",\n  \"source\":"); Quote(writer, source);
                    writer.Write(",\n  \"position_units\":\"ojw_native\",\n  \"time_units\":\"seconds\",\n  \"timing\":\"incoming_move_then_hold\",\n  \"loop\":true,\n  \"motor_ids\":[");
                    for (int j = 0; j < MotorCount; j++) { if (j > 0) writer.Write(','); writer.Write(motorIds[j].ToString(CultureInfo.InvariantCulture)); }
                    writer.Write("],\n  \"frames\":[\n");
                    for (int i = 0; i < FrameCount; i++)
                    {
                        if (i > 0) writer.Write(",\n");
                        writer.Write("    {\"move_time\":" + move[i].ToString("R", CultureInfo.InvariantCulture)
                            + ",\"hold_time\":" + hold[i].ToString("R", CultureInfo.InvariantCulture) + ",\"positions\":[");
                        for (int j = 0; j < MotorCount; j++) { if (j > 0) writer.Write(','); writer.Write(positions[i][j].ToString("R", CultureInfo.InvariantCulture)); }
                        writer.Write("]}");
                    }
                    writer.Write("\n  ]\n}\n");
                }
                private static bool Finite(double v) { return !double.IsNaN(v) && !double.IsInfinity(v); }
                private static void Quote(TextWriter w, string s)
                {
                    w.Write('"');
                    foreach (char c in s)
                    {
                        if (c == '"' || c == '\\') { w.Write('\\'); w.Write(c); }
                        else if (c < 32) w.Write("\\u" + ((int)c).ToString("x4", CultureInfo.InvariantCulture));
                        else w.Write(c);
                    }
                    w.Write('"');
                }
            }
        }
    }
}
