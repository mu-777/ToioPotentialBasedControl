using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// https://cpplover.blogspot.com/2014/11/blog-post_8.html
// https://stackoverflow.com/questions/600978/how-to-do-template-specialization-in-c-sharp

public class PotentialMethods
{
    public static float GetPotential<T>(Vector2 targetPos, Vector2 potentialOrigin, T methodParam)
    {
        return (PotentialFunctionMap[typeof(T)] as PotentialMethod<T>).GetPotential(targetPos, potentialOrigin, methodParam);
    }

    private static Dictionary<Type, object> PotentialFunctionMap = new Dictionary<Type, object>
    {
        {typeof(GaussianParam),  new Gaussian()},
        {typeof(RectWallParam),  new RectWall()},
        {typeof(TestParam),  new Test()},
    };

    private interface PotentialMethod<T>
    {
        float GetPotential(Vector2 targetPos, Vector2 potentialOrigin, T param);
    }

    public struct GaussianParam
    {
        public float sigma;
        public float scale;
        public GaussianParam(float inSigma, float inScale)
        {
            sigma = inSigma;
            scale = inScale;
        }
    }

    private class Gaussian: PotentialMethod<GaussianParam>
    {
        public float GetPotential(Vector2 targetPos, Vector2 potentialOrigin, GaussianParam param)
        {
            var sigma2 = param.sigma * param.sigma;
            return param.scale * Mathf.Exp(-(targetPos - potentialOrigin).sqrMagnitude / (2.0f * sigma2));
        }
    }

    public struct RectWallParam
    {
        public Rect rect;
        public float threshDist;
        public float wallHeight;
        public RectWallParam(Rect inRect, float inThreshDist, float inWallHeight)
        {
            rect = inRect;
            threshDist = inThreshDist;
            wallHeight = inWallHeight;
        }
    }

    private class RectWall : PotentialMethod<RectWallParam>
    {
        public float GetPotential(Vector2 targetPos, Vector2 potentialOrigin, RectWallParam param)
        {
            var minDist = Mathf.Min(Mathf.Abs(param.rect.xMin - targetPos.x),
                                 Mathf.Abs(param.rect.yMin - targetPos.y),
                                 Mathf.Abs(param.rect.xMax - targetPos.x),
                                 Mathf.Abs(param.rect.yMax - targetPos.y));
            return (minDist < param.threshDist) ? param.wallHeight : 0.0f; 
        }
    }

    public struct TestParam
    {
        public float val;
        public TestParam(float inVal)
        {
            val = inVal;
        }
    }

    private class Test : PotentialMethod<TestParam>
    {
        public float GetPotential(Vector2 targetPos, Vector2 potentialOrigin, TestParam param)
        {
            return (targetPos - potentialOrigin).magnitude * param.val;
        }
    }
}
