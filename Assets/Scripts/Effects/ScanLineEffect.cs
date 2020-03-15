using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScanLineEffect : MonoBehaviour
{
    private static readonly int LineProgressIndex = Shader.PropertyToID("_LineProgress");
    private static readonly int PGLProgressIndex = Shader.PropertyToID("_PrimaryGraylineProgress");
    private static readonly int SGLProgressIndex = Shader.PropertyToID("_SecondaryGraylineProgress");

    private class LineInfo
    {
        private readonly Material material;

        private readonly float minScanSpeed;
        private readonly float maxScanSpeed;

        private readonly float maxTimeUntilScan;
        private readonly float minTimeUntilScan;

        private readonly int shaderIndex;

        private float scanProgress;
        private bool scanning;

        private float scanSpeed;
        private float timeUntilScan;

        public LineInfo(Material mat, float minSS, float maxSS, float minTime, float maxTime, int index)
        {
            material = mat;
            minScanSpeed = minSS;
            maxScanSpeed = maxSS;
            minTimeUntilScan = minTime;
            maxTimeUntilScan = maxTime;
            shaderIndex = index;

            scanProgress = 0f;
            scanning = false;
        }

        public void Update(float deltaTime)
        {
            timeUntilScan -= deltaTime;
            if (!scanning && timeUntilScan <= 0f)
            {
                material.SetFloat(shaderIndex, 0f);
                scanSpeed = (float) Utils.RNG.NextDouble() * (maxScanSpeed - minScanSpeed) + minScanSpeed;
                scanning = true;
                scanProgress = 0f;
            }
            if (scanning)
            {
                scanProgress += Time.deltaTime * scanSpeed;
                material.SetFloat(shaderIndex, scanProgress);

                if (scanProgress >= 1f)
                {
                    material.SetFloat(shaderIndex, -1f);
                    scanning = false;
                    timeUntilScan = (float)Utils.RNG.NextDouble() * (maxTimeUntilScan - minTimeUntilScan) + minTimeUntilScan;
                }
            }
        }
    }

    public Material BlitMaterial;

    private LineInfo mainLine;
    private LineInfo pgl;
    private LineInfo sgl;

    protected void Start()
    {
        mainLine = new LineInfo(BlitMaterial, 0.25f, 2f, 5f, 30f, LineProgressIndex);
        pgl = new LineInfo(BlitMaterial, 0.5f, 1f, 2.5f, 10f, PGLProgressIndex);
        sgl = new LineInfo(BlitMaterial, 1f, 3f, 10f, 15f, SGLProgressIndex);
    }

    protected void Update()
    {
        mainLine.Update(Time.deltaTime);
        pgl.Update(Time.deltaTime);
        sgl.Update(Time.deltaTime);
    }
}
