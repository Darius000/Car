using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BezierCurve))]
internal class BezierCurveEditor : Editor
{
    private const float m_DirectionScale = .5f;
    private const int m_LineSteps = 10;
    private const float m_HandleSize = 0.04f;
    private const float m_PickSize = 0.06f;
    private int m_SelectedIndex = -1;

    private BezierCurve m_Curve;
    private Transform m_HandleTransform;
    private Quaternion m_HandleRotation;

    public override void OnInspectorGUI()
    {
        if (m_SelectedIndex >= 0 && m_SelectedIndex < m_Curve.GetControlPointCount)
        {
            DrawSelectedPointInspector(m_SelectedIndex);
        }
    }


    protected virtual void OnSceneGUI()
    {
        m_Curve = (BezierCurve)target;
        m_HandleTransform = m_Curve.transform;
        m_HandleRotation = Tools.pivotRotation == PivotRotation.Local ? m_HandleTransform.rotation : Quaternion.identity;

       
        DrawPoints();
        ShowDirections();
    }

    protected virtual void DrawSelectedPointInspector(int index)
    {
        GUILayout.Label("Selected Point");
        EditorGUI.BeginChangeCheck();
        Vector3 point = EditorGUILayout.Vector3Field("Position", m_Curve.GetControlPoint(index));
        if(EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(m_Curve, "Move Point");
            EditorUtility.SetDirty(m_Curve);
            m_Curve.SetControlPoint(index, point);
        }
    }

    protected virtual void DrawPoints()
    {
        Vector3 p0 = ShowPoint(0);
        Vector3 p1 = ShowPoint(1);
        Vector3 p2 = ShowPoint(2);
        Vector3 p3 = ShowPoint(3);

        Handles.color = Color.grey;
        Handles.DrawLine(p0, p1);
        Handles.DrawLine(p2, p3);
        Handles.DrawBezier(p0, p3, p1, p2, Color.white, null, 2f);
    }

    protected Vector3 ShowPoint(int index)
    {
        Vector3 point = m_HandleTransform.TransformPoint(m_Curve.GetControlPoint(index));

        float size = HandleUtility.GetHandleSize(point);
        if(index == 0)
        {
            size *= 2f;
        }

        Handles.color = GetPointColor(index);
        if(Handles.Button(point,  m_HandleRotation, size * m_HandleSize, size * m_PickSize, Handles.DotHandleCap))
        {
            m_SelectedIndex = index;
            Repaint();
        }

        if(m_SelectedIndex == index)
        {
            EditorGUI.BeginChangeCheck();
            point = Handles.DoPositionHandle(point, m_HandleRotation);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_Curve, "Move Points");
                EditorUtility.SetDirty(m_Curve);
                m_Curve.SetControlPoint(index, m_HandleTransform.InverseTransformPoint(point));
            }
        }

        return point;
    }

    protected virtual Color GetPointColor(int index)
    {
        return Color.white;
    }

    private void ShowDirections()
    {
        Handles.color = Color.green;
        Vector3 point = m_Curve.GetPoint(0f);

        
        Handles.DrawLine(point, point + m_Curve.GetDirection(0f) * m_DirectionScale);

        int steps = GetSteps();
        for (int i = 0; i <= steps; i++)
        {
            point = m_Curve.GetPoint(i / (float)steps);
            Handles.DrawLine(point, point + m_Curve.GetDirection(i / (float)steps) * m_DirectionScale);
        }
    }

    protected virtual int GetSteps()
    {
        return m_LineSteps;
    }
}