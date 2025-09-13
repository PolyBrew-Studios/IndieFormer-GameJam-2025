using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class OurInput : MonoBehaviour
{
    public KeyCode LeftSteering { get; private set; }
    public KeyCode RightSteering { get; private set; }


    [SerializeField] TMP_Text _leftSteerText;
    [SerializeField] TMP_Text _rightSteerText;
    [SerializeField] TMP_Text _accelerationText;
    [SerializeField] VehicleController controlledVehicle;

    float _steerInput = 0;
    void Start()
    {
        float minManhattanDistance = 10;


        int firstKey =  Random.Range(0, UnityKeyboardLayout.KeysByPosition.Count-1);

        var left = UnityKeyboardLayout.KeysByPosition.ElementAt(firstKey);
        LeftSteering = left.Value;
        Vector2 leftSteeringPos = left.Key;

        _leftSteerText.text = $"Left: {LeftSteering}";

        float rightInset = Random.Range(0, 960);
        _leftSteerText.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, rightInset, 300);
        _leftSteerText.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, Random.Range(0, 1080), 300);

        List<KeyCode> adepts = new List<KeyCode>();
        foreach(var key in UnityKeyboardLayout.KeysByPosition)
        {
            if (key.Value == LeftSteering)
                continue;

            float distance = GetManhattanDistance(leftSteeringPos, key.Key);

            if (distance > minManhattanDistance)
                adepts.Add(key.Value);
        }
        int secondKey = Random.Range(0, adepts.Count-1);

        var right = UnityKeyboardLayout.KeysByPosition.ElementAt(secondKey);
        RightSteering = right.Value;
        Vector2 rightSteeringPos = right.Key;

        _rightSteerText.text = $"Right: {RightSteering}";

        float leftInset = Random.Range(0, 960);
        _rightSteerText.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, leftInset, 300);
        _rightSteerText.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, Random.Range(0, 1080), 300);
    }

    float _yMax = 20;
    float _yMin = -20;
    float _currentY = 0;
    float _steerAmount = 0.01f;
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

        float currentSteer = 0;
        if (Input.GetKey(LeftSteering))
            currentSteer -= 1;
        if (Input.GetKey(RightSteering))
            currentSteer += 1;

        if(Mathf.Abs(currentSteer)>0)
        {
            _steerInput = Mathf.Clamp(_steerInput+(currentSteer * _steerAmount),-1,1);
        }
        else
        {
            float v = Mathf.Min(Mathf.Abs(_steerInput), _steerAmount);

            if (_steerInput > 0)
                _steerInput -= v;
            else
                _steerInput += v;
                    
        }

        controlledVehicle.SetForwardInput(accel);
        controlledVehicle.SetTurnInput(_steerInput);

        if (Input.GetKeyDown(KeyCode.Q)) //tmp
            if(Input.GetKeyUp(KeyCode.Q))
                controlledVehicle.GearDownShift();
        if(Input.GetKeyDown(KeyCode.E))
            if(Input.GetKeyUp(KeyCode.E))
                controlledVehicle.GearUpShift();
        
        _accelerationText.text = (accel).ToString();

         Debug.Log(_steerInput);
    }

    private static float GetManhattanDistance(Vector2 k1, Vector2 k2)
    {
        return Mathf.Abs(k1.x - k2.x) + Mathf.Abs(k1.y - k2.y);
    }
}
