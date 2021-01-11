using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using toio;

public static class ToioUtils
{
    public static void SetLEDColorToCubeList(List<Cube> cubes, Color color)
    {
        var duration = 0;
        foreach(var c in cubes)
        {
            c.TurnLedOn(color, duration);
        }
    }

    public static void TurnLedOn(this Cube self, Color color, int durationMs)
    {
        self.TurnLedOn((int)(color.r * 255), (int)(color.g * 255), (int)(color.b * 255), durationMs);
    }

    // https://toio.github.io/toio-spec/docs/hardware_position_id
    public static Vector2 GetPosInMatUV(this Cube self)
    {
        var a = 1.0f / 410f;
        var b = -45f / 410f;
        return new Vector2(self.x * a + b, self.y * a + b);
    }

    // https://toio.github.io/toio-spec/docs/ble_motor#%E3%83%A2%E3%83%BC%E3%82%BF%E3%83%BC%E5%88%B6%E5%BE%A1
    public static void MoveWithRadPerSec(this Cube self, float left_radPerSec, float right_radPerSec,
                                         int durationMs, Cube.ORDER_TYPE order = Cube.ORDER_TYPE.Weak)
    {
        Func<float, float> radPerSec2rpm = (float radPerSec) => { return Mathf.Sign(radPerSec) * Mathf.Clamp(Mathf.Abs(radPerSec * 30.0f / Mathf.PI), 34.0f, 494.0f); };
        Func<float, int> rpm2inputval = (float rpm) => { return (int)(rpm * (107.0f / 460.0f) + (42.0f / 460.0f)); };

        self.Move(rpm2inputval(radPerSec2rpm(left_radPerSec)),
                  rpm2inputval(radPerSec2rpm(right_radPerSec)), 
                  durationMs, order);
    }

}
