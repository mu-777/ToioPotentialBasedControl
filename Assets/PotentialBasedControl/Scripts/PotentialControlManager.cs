using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using toio;


public class PotentialControlManager : MonoBehaviour
{
    public UnityEngine.UI.Button _startStopButton;
    private UnityEngine.UI.Text _startStopButtonText;
    public bool IsWallPotentialActive = true;

    private List<Cube> _targetCubes;
    private List<Cube> _attractionCubes;
    private List<Cube> _repulsionCubes;

    private Vector3 _fieldOrigin;
    private float _fieldWidth = 0.56f;
    public float FieldWidth { get { return _fieldWidth; } }
    private float _fieldHeight = 0.56f;
    public float FieldHeight { get { return _fieldHeight; } }

    private readonly float m2mm = 1000.0f;
    private readonly float mm2m = 0.001f;

    private PotentialMethods.GaussianParam _attractionParam = new PotentialMethods.GaussianParam(0.3f, -0.4f);
    private PotentialMethods.GaussianParam _repulsionParam = new PotentialMethods.GaussianParam(0.1f, 0.3f);
    private PotentialMethods.RectWallParam _rectWallParam = new PotentialMethods.RectWallParam(new Rect(0.0f, 0.0f, 1.0f, 1.0f),
                                                                                               0.03f, 0.2f);
    private Coroutine _controlLoop = null;
    private float _controlLoopMs = 100.0f;
    private bool _isControlling = false;
    public bool IsControlling { get {return _isControlling; } }

    void Awake()
    {
        _targetCubes = new List<Cube>();
        _attractionCubes = new List<Cube>();
        _repulsionCubes = new List<Cube>();
        _fieldOrigin = this.transform.position;

        _startStopButtonText = _startStopButton.GetComponentInChildren<UnityEngine.UI.Text>();
        _startStopButtonText.text = "Connecting..";
        _startStopButton.interactable = false;
    }

    async void Start()
    {
        var peripherals = await new NearScanner(12).Scan();
        var cubes = await new CubeConnecter().Connect(peripherals);

        for(var i = 0; i < cubes.Length; i++)
        {
            var container = (i == 0) ? _targetCubes : (i == 1) ? _attractionCubes : _repulsionCubes;
            container.Add(cubes[i]);
        }
        ToioUtils.SetLEDColorToCubeList(_targetCubes, Color.blue);
        ToioUtils.SetLEDColorToCubeList(_attractionCubes, Color.green);
        ToioUtils.SetLEDColorToCubeList(_repulsionCubes, Color.red);

        _startStopButtonText.text = "Start";
        _startStopButton.interactable = true;
        _startStopButton.onClick.AddListener(() =>
        {
            if(_isControlling)
            {
                StopControl();
            }
            else
            {
                StartControl();
            }
        });
    }

    void Update()
    {
        if(!_isControlling)
        {
            return;
        }
        foreach(var target in _targetCubes)
        {
            foreach(var goal in _attractionCubes)
            {
                if((target.GetPosInMatUV() - goal.GetPosInMatUV()).magnitude < 0.1f)
                {
                    StopControl();
                    DoGoalAction(target);
                }
            }
        }

    }

    IEnumerator ControlLoop()
    {
        var yielder = new WaitForSeconds(_controlLoopMs * 0.001f);

        while(true)
        {
            foreach(var targetCube in _targetCubes)
            {
                var center2controlPoint = -0.0038f;
                //var center2controlPoint = 0.1f;
                var wheelRadius = 0.00625f;
                var tread = 0.0266f;

                var diff = -GetPotentialDifferential(targetCube.GetPosInMatUV());
                var theta = targetCube.angle * Mathf.Deg2Rad;
                var v = Mathf.Cos(theta) * diff.x + Mathf.Sin(theta) * diff.y;
                var w = (-Mathf.Sin(theta) * diff.x + Mathf.Cos(theta) * diff.y) / center2controlPoint;
                var w_l = (v + 0.5f * tread) / wheelRadius;
                var w_r = (v - 0.5f * tread) / wheelRadius;
                targetCube.MoveWithRadPerSec(w_l, w_r, (int)(_controlLoopMs * 0.2f), Cube.ORDER_TYPE.Strong);
            }
            yield return yielder;
        }
    }

    public bool StartControl()
    {
        if(_controlLoop != null)
        {
            return false;
        }
        ToioUtils.SetLEDColorToCubeList(_targetCubes, Color.blue);
        _controlLoop = StartCoroutine(ControlLoop());
        _startStopButtonText.text = "Stop";
        _isControlling = true;
        return true;
    }

    public bool StopControl()
    {
        if(_controlLoop == null)
        {
            return false;
        }
        ToioUtils.SetLEDColorToCubeList(_targetCubes, Color.gray);
        StopCoroutine(_controlLoop);
        _controlLoop = null;
        _startStopButtonText.text = "Start";
        _isControlling = false;
        return true;
    }

    public Vector2 GetPotentialDifferential(Vector2 uv)
    {
        var deltaScale = 0.001f;
        var deltaX = new Vector2(deltaScale, 0.0f);
        var deltaY = new Vector2(0.0f, deltaScale);
        var diffX = GetPotentialFieldValue(uv + deltaX) - GetPotentialFieldValue(uv - deltaX);
        var diffY = GetPotentialFieldValue(uv + deltaY) - GetPotentialFieldValue(uv - deltaY);
        return new Vector2(diffX, diffY) / (2.0f * deltaScale);
    }

    public float GetPotentialFieldValue(float u, float v)
    {
        return GetPotentialFieldValue(new Vector2(u, v));
    }

    public float GetPotentialFieldValue(Vector2 uv)
    {
        var potential = 0.0f;

        foreach(var attraction in _attractionCubes)
        {
            if(!attraction.isPressed)
            {
                potential += PotentialMethods.GetPotential(uv, attraction.GetPosInMatUV(),
                                                           _attractionParam);
            }
        }
        foreach(var repulsion in _repulsionCubes)
        {
            if(!repulsion.isPressed)
            {
                potential += PotentialMethods.GetPotential(uv, repulsion.GetPosInMatUV(),
                                                           _repulsionParam);
            }
        }
        if (IsWallPotentialActive)
        {
            potential += PotentialMethods.GetPotential(uv, Vector2.zero, _rectWallParam);
        }
        return potential;
    }

    private void DoGoalAction(Cube cube)
    {
        cube.Move(50, -50, 5000, Cube.ORDER_TYPE.Strong);
        cube.TurnOnLightWithScenario(5, new Cube.LightOperation[]
        {
            new Cube.LightOperation(500, 0, 0, 255),
            new Cube.LightOperation(500, 0, 0, 0),
        }, Cube.ORDER_TYPE.Strong);
    }
}
