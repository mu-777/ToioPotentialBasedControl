using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using toio;

public class TestCubeController : MonoBehaviour
{
    Cube _cube;

    async void Start()
    {
        var peripheral = await new NearestScanner().Scan();
        _cube = await new CubeConnecter().Connect(peripheral);
    }

    void Update()
    {
        Debug.Log(_cube.GetPosInMatUV());
    }
}
