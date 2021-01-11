using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class PotentialFieldVisualizer : MonoBehaviour
{
    public PotentialControlManager _potentialControlManager;
    public UnityEngine.UI.Button _visualizeButton;
    private UnityEngine.UI.Text _visualizeButtonText;

    [SerializeField]
    private Material _potentialFieldMat;

    private int _fieldSplitNumW = 100;
    private int _fieldSplitNumH = 100;

    private Mesh _mesh;
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;

    private Texture2D _potentialTex;

    void Awake()
    {
        _potentialFieldMat.SetInt("_ShowWireframe", 1);

        _potentialFieldMat.SetInt("_Show3D", 1);
        _visualizeButtonText = _visualizeButton.GetComponentInChildren<UnityEngine.UI.Text>();
        _visualizeButtonText.text = "Show2D";
        _visualizeButton.onClick.AddListener(() =>
        {
            if(_visualizeButtonText.text == "Show2D")
            {
                _visualizeButtonText.text = "Show3D";
                _potentialFieldMat.SetInt("_Show3D", 0);
            }
            else
            {
                _visualizeButtonText.text = "Show2D";
                _potentialFieldMat.SetInt("_Show3D", 1);
            }
        });
    }

    void Start()
    {
        _meshFilter = this.GetComponent<MeshFilter>();
        _meshRenderer = this.GetComponent<MeshRenderer>();
        _mesh = MeshUtils.CreatePlaneMesh(Vector3.zero,
                                          _potentialControlManager.FieldWidth,
                                          _potentialControlManager.FieldHeight,
                                          _fieldSplitNumW, _fieldSplitNumH);
        _meshFilter.sharedMesh = _mesh;
        _meshRenderer.material = _potentialFieldMat;
        _potentialTex = new Texture2D(_fieldSplitNumW, _fieldSplitNumH,
                                      TextureFormat.RGFloat, false);
        _potentialTex.wrapMode = TextureWrapMode.Clamp;
        _potentialFieldMat.SetTexture("_PotentialFieldTex", _potentialTex);
    }

    void Update()
    {
        float maxVal = 0.0f;
        float minVal = 0.0f;
        var cols = new Color[_fieldSplitNumW * _fieldSplitNumH];
        for(int h = 0; h < _fieldSplitNumH; h++)
        {
            for(int w = 0; w < _fieldSplitNumW; w++)
            {
                var u = (float)w / (float)_fieldSplitNumW;
                var v = (float)h / (float)_fieldSplitNumH;
                var potential = _potentialControlManager.GetPotentialFieldValue(u, v);
                var idx = h * _fieldSplitNumW + w;
                cols[idx].r = potential > 0.0f ? Mathf.Abs(potential) : 0.0f;
                cols[idx].g = potential < 0.0f ? Mathf.Abs(potential) : 0.0f;
                cols[idx].b = 0.0f;
                cols[idx].a = 0.0f;

                if(maxVal < potential)
                {
                    maxVal = potential;
                }
                if(minVal > potential)
                {
                    minVal = potential;
                }
            }
        }
        _potentialTex.SetPixels(cols);
        _potentialTex.Apply(false);
        _potentialFieldMat.SetTexture("_PotentialFieldTex", _potentialTex);
        _potentialFieldMat.SetFloat("_MaxPotentialValue", maxVal);
        _potentialFieldMat.SetFloat("_MinPotentialValue", minVal);
    }

}
