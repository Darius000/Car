using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BezierSpline))]
internal class BezierSplineEditor : BezierCurveEditor
{
    private BezierSpline m_Spline;
    private const int m_StepsPerCurve = 10;

    private static Color[] m_ModeColors =
    {
        Color.white,
        Color.yellow,
        Color.cyan
    };


    public override void OnInspectorGUI()
    {
        m_Spline = (BezierSpline)target;

        EditorGUI.BeginChangeCheck();
        bool loop = EditorGUILayout.Toggle("Loop", m_Spline.Loop);
        if(EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(m_Spline, "Toggle Loop");
            EditorUtility.SetDirty(m_Spline);
            m_Spline.Loop = loop;
        }

        base.OnInspectorGUI();
        
        if(GUILayout.Button("Add Curve"))
        {
            Undo.RecordObject(m_Spline, "Add Curve");
            m_Spline.AddCurve();
            EditorUtility.SetDirty(m_Spline);
        }
    }

    protected override void OnSceneGUI()
    {
        m_Spline = (BezierSpline)target;

        
        base.OnSceneGUI();
    }

    protected override void DrawPoints()
    {
        Vector3 p0 = ShowPoint(0);

        for(int i = 1; i < m_Spline.GetControlPointCount; i+= 3)
        {
            Vector3 p1 = ShowPoint(i);
            Vector3 p2 = ShowPoint(i+1);
            Vector3 p3 = ShowPoint(i+2);

            Handles.color = Color.gray;
            Handles.DrawLine(p0, p1);
            Handles.DrawLine(p2, p3);

            Handles.DrawBezier(p0, p3, p1, p2, Color.white, null, 2f);
            p0 = p3;
        }
    }

    protected override void DrawSelectedPointInspector(int index)
    {
        base.DrawSelectedPointInspector(index);

        EditorGUI.BeginChangeCheck();
        BezierControlPointMode mode = (BezierControlPointMode)EditorGUILayout.EnumPopup("Mode", m_Spline.GetControlPointMode(index));
        if(EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(m_Spline, "Change Point Mode");
            m_Spline.SetControlPointMode(index, mode);
            EditorUtility.SetDirty(m_Spline);
        }
    }

    protected override int GetSteps()
    {
        return m_StepsPerCurve * m_Spline.CurveCount;
    }

    protected override Color GetPointColor(int index)
    {
        return m_ModeColors[(int)m_Spline.GetControlPointMode(index)];
    }
}