using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class OurInput : MonoBehaviour
{
    KeyCode _leftSteering;
    KeyCode _rightSteering;

    [SerializeField] TMP_Text _leftSteerText;
    [SerializeField] TMP_Text _rightSteerText;
    [SerializeField] TMP_Text _accelerationText;
    [SerializeField] VehicleController controlledVehicle;


    void Start()
    {
        float minManhattanDistance = 10;


        int firstKey = Random.Range(0, UnityKeyboardLayout.KeysByPosition.Count-1);

        var left = UnityKeyboardLayout.KeysByPosition.ElementAt(firstKey);
        _leftSteering = left.Value;
        Vector2 leftSteeringPos = left.Key;

        _leftSteerText.text = $"Left: {_leftSteering}";

        float rightInset = Random.Range(0, 960);
        _leftSteerText.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, rightInset, 300);
        _leftSteerText.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, Random.Range(0, 1080), 300);

        List<KeyCode> adepts = new List<KeyCode>();
        foreach(var key in UnityKeyboardLayout.KeysByPosition)
        {
            float distance = GetManhattanDistance(leftSteeringPos, key.Key);

            if (distance > minManhattanDistance)
                adepts.Add(key.Value);
        }
        int secondKey = Random.Range(0, adepts.Count-1);

        var right = UnityKeyboardLayout.KeysByPosition.ElementAt(secondKey);
        _rightSteering = right.Value;
        Vector2 rightSteeringPos = right.Key;

        _rightSteerText.text = $"Right: {_rightSteering}";

        float leftInset = Random.Range(0, 960);
        _rightSteerText.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, leftInset, 300);
        _rightSteerText.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, Random.Range(0, 1080), 300);
    }

    float _yMax = 20;
    float _yMin = -20;
    float _currentY = 0;

    private void Update()
    {
        float yDelta = Input.mousePositionDelta.y;
        float accel = 0;
        if (yDelta > 0)
        {
            accel = Mathf.Abs(Mathf.Clamp(_currentY + yDelta, _yMin, _yMax) - _currentY);
            _currentY = Mathf.Clamp(_currentY + yDelta, _yMin, _yMax);
        }
        else if (yDelta < 0)
        {
            accel = Mathf.Abs(_currentY - Mathf.Clamp(_currentY + yDelta, _yMin, _yMax));
            _currentY = Mathf.Clamp( _currentY + yDelta,_yMin,_yMax);

        }

        float steerInput = 0;
        if (Input.GetKey(_leftSteering))
            steerInput -= 1;
        if (Input.GetKey(_rightSteering))
            steerInput += 1;

        controlledVehicle.SetForwardInput(accel);
        controlledVehicle.SetTurnInput(steerInput);

        _accelerationText.text = (accel).ToString();

        Debug.Log(_currentY);
    }

    private static float GetManhattanDistance(Vector2 k1, Vector2 k2)
    {
        return Mathf.Abs(k1.x - k2.x) + Mathf.Abs(k1.y - k2.y);
    }
}
