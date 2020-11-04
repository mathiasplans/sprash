using UnityEngine;
using System;
public readonly struct Vector6 {
    private readonly Vector3Int v1;
    private readonly Vector3Int v2;

    public readonly int a, b, c, d, e, f;

    private Vector6(Vector3Int first, Vector3Int second) {
        this.v1 = first;
        this.v2 = second;

        this.a = v1.x;
        this.b = v1.y;
        this.c = v1.z;
        this.d = v2.x;
        this.e = v2.y;
        this.f = v2.z;
    }

    public Vector6(int a, int b, int c, int d, int e, int f) {
        this.v1 = new Vector3Int(a, b, c);
        this.v2 = new Vector3Int(d, e, f);

        this.a = v1.x;
        this.b = v1.y;
        this.c = v1.z;
        this.d = v2.x;
        this.e = v2.y;
        this.f = v2.z;
    }

    public Vector6(int a) {
        this.v1 = new Vector3Int(a, a, a);
        this.v2 = new Vector3Int(a, a, a);

        this.a = v1.x;
        this.b = v1.y;
        this.c = v1.z;
        this.d = v2.x;
        this.e = v2.y;
        this.f = v2.z;
    }

    public int Amplitude() {
        return Math.Abs(a) + Math.Abs(b) + Math.Abs(c) + Math.Abs(d) + Math.Abs(e) + Math.Abs(f);
    }

    public int Distance(Vector6 other) {
        return Math.Abs(a - other.a) + Math.Abs(b - other.a) + Math.Abs(c - other.c) + Math.Abs(d - other.d) + Math.Abs(f - other.f);
    }

    public override int GetHashCode() {
        int h1 = v1.GetHashCode();
        int h2 = v2.GetHashCode();
        return h1 ^ (h1 << 7) ^ (h1 >> 11) ^ (h2 << 13) ^ (h2 >> 17) ^ (h2 << 19);
    }

    public static Vector6 operator +(Vector6 v) {
        return v;
    }

    public static Vector6 operator -(Vector6 v) {
        return new Vector6(-v.v1, -v.v2);
    }

    public static Vector6 operator +(Vector6 a, Vector6 b) {
        return new Vector6(a.v1 + b.v1, a.v2 + b.v2);
    }

    public static Vector6 operator -(Vector6 a, Vector6 b) {
        return new Vector6(a.v1 - b.v1, a.v2 - b.v2);
    }

    public static Vector6 operator *(int a, Vector6 b) {
        return new Vector6(a * b.v1, a * b.v2);
    }

    public static Vector6 operator *(Vector6 a, int b) {
        return b * a;
    }

    public static bool operator ==(Vector6 a, Vector6 b) {
        if ((object) a == null)
            return (object) b == null;

        return a.Equals(b);
    }

    public static bool operator !=(Vector6 a, Vector6 b) {
        if ((object) a != null)
            return a.Equals(b);

        return (object) b != null;
    }

    public override bool Equals(object obj) {
        return base.Equals(obj);
    }
}