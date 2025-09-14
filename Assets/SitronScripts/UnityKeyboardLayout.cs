using System.Collections.Generic;
using UnityEngine;

public static class UnityKeyboardLayout
{
    public static readonly IReadOnlyDictionary<Vector2, KeyCode> KeysByPosition;

    public static readonly IReadOnlyDictionary<KeyCode, Vector2> PositionByKey;

    static UnityKeyboardLayout()
    {
        var keysByPosition = new Dictionary<Vector2, KeyCode>();
        var positionByKey = new Dictionary<KeyCode, Vector2>();

        void AddKey(KeyCode code, float x, float y)
        {
            var position = new Vector2(x, y);
            if (!keysByPosition.ContainsKey(position))
            {
                keysByPosition[position] = code;
            }
            if (!positionByKey.ContainsKey(code))
            {
                positionByKey[code] = position;
            }
        }


        AddKey(KeyCode.Alpha1, 1f, 1f);
        AddKey(KeyCode.Alpha2, 2f, 1f);
        AddKey(KeyCode.Alpha3, 3f, 1f);
        AddKey(KeyCode.Alpha4, 4f, 1f);
        AddKey(KeyCode.Alpha5, 5f, 1f);
        AddKey(KeyCode.Alpha6, 6f, 1f);
        AddKey(KeyCode.Alpha7, 7f, 1f);
        AddKey(KeyCode.Alpha8, 8f, 1f);
        AddKey(KeyCode.Alpha9, 9f, 1f);
        AddKey(KeyCode.Alpha0, 10f, 1f);
        AddKey(KeyCode.Mouse0, 100f, 1f);
        AddKey(KeyCode.Mouse1, 100f, 1f);


        AddKey(KeyCode.Q, 1.5f, 2f);
        AddKey(KeyCode.W, 2.5f, 2f);
        AddKey(KeyCode.E, 3.5f, 2f);
        // AddKey(KeyCode.R, 4.5f, 2f);
        AddKey(KeyCode.T, 5.5f, 2f);
        AddKey(KeyCode.Y, 6.5f, 2f);
        AddKey(KeyCode.U, 7.5f, 2f);
        AddKey(KeyCode.I, 8.5f, 2f);
        AddKey(KeyCode.O, 9.5f, 2f);
        AddKey(KeyCode.P, 10.5f, 2f);

        AddKey(KeyCode.A, 1.75f, 3f);
        AddKey(KeyCode.S, 2.75f, 3f);
        AddKey(KeyCode.D, 3.75f, 3f);
        AddKey(KeyCode.F, 4.75f, 3f);
        AddKey(KeyCode.G, 5.75f, 3f);
        AddKey(KeyCode.H, 6.75f, 3f);
        AddKey(KeyCode.J, 7.75f, 3f);
        AddKey(KeyCode.K, 8.75f, 3f);
        AddKey(KeyCode.L, 9.75f, 3f);

        AddKey(KeyCode.Z, 2.25f, 4f);
        AddKey(KeyCode.X, 3.25f, 4f);
        AddKey(KeyCode.C, 4.25f, 4f);
        AddKey(KeyCode.V, 5.25f, 4f);
        AddKey(KeyCode.B, 6.25f, 4f);
        AddKey(KeyCode.N, 7.25f, 4f);
        AddKey(KeyCode.M, 8.25f, 4f);

        AddKey(KeyCode.Space, 7.25f, 5f); 



        AddKey(KeyCode.UpArrow, 16f, 4.5f);
        AddKey(KeyCode.LeftArrow, 15f, 5.5f);
        AddKey(KeyCode.DownArrow, 16f, 5.5f);
        AddKey(KeyCode.RightArrow, 17f, 5.5f);



        KeysByPosition = keysByPosition;
        PositionByKey = positionByKey;
    }
}
